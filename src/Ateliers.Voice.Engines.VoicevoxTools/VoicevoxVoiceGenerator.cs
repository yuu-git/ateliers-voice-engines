using Microsoft.Extensions.FileSystemGlobbing;
using System.Diagnostics;
using System.Text.Json;
using VoicevoxCoreSharp.Core;
using VoicevoxCoreSharp.Core.Enum;
using VoicevoxCoreSharp.Core.Struct;

namespace Ateliers.Voice.Engines.VoicevoxTools;

/// <summary>
/// VOICEVOX エンジンを使用した音声生成サービス
/// </summary>
public sealed class VoicevoxVoiceGenerator : IVoicevoxVoiceGenerator
{
    private readonly VoicevoxOptions _options;
    private readonly ILogger _logger;
    private readonly IExecutionContext _context;
    private readonly Synthesizer _synthesizer;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private const string LogPrefix = nameof(VoicevoxVoiceGenerator);

    public VoicevoxVoiceGenerator(
        VoicevoxOptions options,
        ILogger logger,
        IExecutionContext context)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));

        using var initScope = _context.BeginScope($"{LogPrefix}:Initialize");
        
        _logger.Info($"{LogPrefix}: 初期化を開始");

        // OpenJTalk辞書パスを解決
        _logger.Debug($"{LogPrefix}: OpenJTalk 辞書パスを解決中...");
        var openJTalkDictPath = ResolveOpenJTalkDictPath(options.ResourcePath);
        _logger.Debug($"{LogPrefix}: OpenJTalk 辞書パス: {openJTalkDictPath}");

        // OpenJTalk初期化
        _logger.Debug($"{LogPrefix}: OpenJTalk を初期化中...");
        var result = OpenJtalk.New(openJTalkDictPath, out var openJtalk);
        EnsureOk(result, "OpenJTalkの初期化に失敗");

        // ONNX Runtime初期化
        _logger.Debug($"{LogPrefix}: ONNX Runtime を初期化中...");
        result = Onnxruntime.LoadOnce(
            LoadOnnxruntimeOptions.Default(),
            out var onnxruntime);
        EnsureOk(result, "ONNX Runtimeの初期化に失敗");

        // Synthesizer初期化
        _logger.Debug($"{LogPrefix}: Synthesizer を初期化中...");
        result = Synthesizer.New(
            onnxruntime,
            openJtalk,
            InitializeOptions.Default(),
            out _synthesizer);
        EnsureOk(result, "Synthesizerの初期化に失敗");

        // Voice modelsの読み込み
        var modelDir = Path.Combine(options.ResourcePath, "model");
        _logger.Debug($"{LogPrefix}: 音声モデルディレクトリ: {modelDir}");

        var matcher = new Matcher();
        matcher.AddIncludePatterns(new[] { "*.vvm" });

        var allModelPaths = matcher
            .GetResultsInFullPath(modelDir)
            .ToList();

        _logger.Debug($"{LogPrefix}: 検出された音声モデル数: {allModelPaths.Count}個");

        IEnumerable<string> modelPathsToLoad;

        if (options.VoiceModelNames is null || options.VoiceModelNames.Count == 0)
        {
            modelPathsToLoad = allModelPaths;
            _logger.Info($"{LogPrefix}: すべての音声モデルを読み込みます: {allModelPaths.Count}個");
        }
        else
        {
            var normalizedNames = options.VoiceModelNames
                .Select(n => Path.GetFileNameWithoutExtension(n))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            modelPathsToLoad = allModelPaths
                .Where(path =>
                    normalizedNames.Contains(
                        Path.GetFileNameWithoutExtension(path)));

            _logger.Info($"{LogPrefix}: 指定された音声モデルを読み込みます: {string.Join(", ", options.VoiceModelNames)}");
        }

        if (!modelPathsToLoad.Any())
        {
            var ex = new InvalidOperationException(
                "No matching voice models (*.vvm) were found. " +
                "Please check VoiceModelNames in VoicevoxOptions.");
            _logger.Critical($"{LogPrefix}: 初期化失敗: 音声モデルが見つかりません", ex);
            throw ex;
        }

        _logger.Info($"{LogPrefix}: 音声モデルを読み込み中: {modelPathsToLoad.Count()}個");
        var loadedCount = 0;
        foreach (var path in modelPathsToLoad)
        {
            _logger.Debug($"{LogPrefix}: 音声モデル読み込み中: {Path.GetFileName(path)}");
            result = VoiceModelFile.Open(path, out var voiceModel);
            EnsureOk(result, $"音声モデル読み込み失敗: {Path.GetFileName(path)}");

            result = _synthesizer.LoadVoiceModel(voiceModel);
            EnsureOk(result, $"音声モデルロード失敗: {Path.GetFileName(path)}");

            voiceModel.Dispose();
            loadedCount++;
        }

        _logger.Info($"{LogPrefix}: 音声モデル読み込み完了: {loadedCount}個");

        openJtalk.Dispose();

        _logger.Info($"{LogPrefix}: 初期化完了");
    }

    /// <summary>
    /// 音声ファイルを生成します
    /// </summary>
    public async Task<VoicevoxGenerateResult> GenerateVoiceFileAsync(
        VoicevoxGenerateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var scope = _context.BeginScope($"{LogPrefix}:GenerateVoiceFile");
        
        _logger.Info($"{LogPrefix}: 音声生成開始: text={request.Text.Length}文字, outputWavFileName={request.OutputWavFileName}");

        var stopwatch = Stopwatch.StartNew();

        var outputDir = CreateWorkDirectory();
        _logger.Debug($"{LogPrefix}: 出力ディレクトリ: {outputDir}");

        var voicevoxOptions = request.Options;
        var styleId = voicevoxOptions?.StyleId;

        var outputWavPath = await SynthesizeToFileAsync(
            request,
            outputDir,
            voicevoxOptions,
            styleId,
            cancellationToken);

        stopwatch.Stop();

        _logger.Info($"{LogPrefix}: 音声生成完了: outputWavPath={outputWavPath}, elapsed={stopwatch.Elapsed}");
        
        return new VoicevoxGenerateResult
        {
            OutputWavPath = outputWavPath,
            Elapsed = stopwatch.Elapsed
        };
    }

    /// <summary>
    /// 複数の音声ファイルを生成します
    /// </summary>
    public async Task<IReadOnlyList<VoicevoxGenerateResult>> GenerateVoiceFilesAsync(
        IEnumerable<VoicevoxGenerateRequest> requests,
        CancellationToken cancellationToken = default)
    {
        using var scope = _context.BeginScope($"{LogPrefix}:GenerateVoiceFiles");
        
        var requestList = requests.ToList();
        _logger.Info($"{LogPrefix}: 複数音声生成開始: リクエスト数={requestList.Count}個");

        var outputDir = CreateWorkDirectory();
        _logger.Debug($"{LogPrefix}: 出力ディレクトリ: {outputDir}");

        var results = new List<VoicevoxGenerateResult>();
        var index = 0;

        foreach (var request in requestList)
        {
            index++;
            _logger.Debug($"{LogPrefix}: 音声生成中 ({index}/{requestList.Count}): {request.OutputWavFileName}");

            var stopwatch = Stopwatch.StartNew();
            var voicevoxOptions = request.Options;
            var styleId = voicevoxOptions?.StyleId;

            var outputWavPath = await SynthesizeToFileAsync(
                request,
                outputDir,
                voicevoxOptions,
                styleId,
                cancellationToken);

            stopwatch.Stop();

            results.Add(new VoicevoxGenerateResult
            {
                OutputWavPath = outputWavPath,
                Elapsed = stopwatch.Elapsed
            });
        }

        _logger.Info($"{LogPrefix}: 複数音声生成完了: {results.Count}個の音声ファイルを生成");
        return results;
    }

    private async Task<byte[]> SynthesizeAsync(
        string text,
        uint? styleId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.Debug($"{LogPrefix}: 音声合成開始: text={text.Length}文字, styleId={styleId}");

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var sid = styleId ?? _options.DefaultStyleId;
            _logger.Debug($"{LogPrefix}: 使用するスタイルID={sid}");

            var result = _synthesizer.Tts(
                text,
                sid,
                TtsOptions.Default(),
                out _,
                out var wav);

            EnsureOk(result, "音声合成に失敗");

            _logger.Debug($"{LogPrefix}: 音声合成完了: サイズ={wav!.Length}バイト");
            return wav!;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<string> SynthesizeToFileAsync(
        VoicevoxGenerateRequest request,
        string outputDir,
        VoicevoxGenerateOptions? voicevoxOptions,
        uint? styleId = null,
        CancellationToken cancellationToken = default)
    {
        var text = request.Text;
        var outputWavFileName = request.OutputWavFileName;

        _logger.Debug($"{LogPrefix}: ファイル生成開始: text={text.Length}文字, outputWavFileName={outputWavFileName}");

        if (string.IsNullOrWhiteSpace(outputWavFileName))
        {
            var ex = new ArgumentException(
                "OutputWavFileName must be specified",
                nameof(outputWavFileName));
            _logger.Critical($"{LogPrefix}: 出力ファイル名が指定されていません", ex);
            throw ex;
        }

        var wav = await SynthesizeAsync(text, styleId, cancellationToken);

        var outputWavPath = Path.Combine(outputDir, outputWavFileName);
        _logger.Debug($"{LogPrefix}: ファイルを書き込み中: {outputWavPath}");

        await File.WriteAllBytesAsync(outputWavPath, wav, cancellationToken);

        // テキスト/メタデータファイルの保存
        var textFileSaveMode = voicevoxOptions?.TextFileSaveMode ?? TextFileSaveMode.TextOnly;
        await SaveTextFileAsync(
            text,
            outputDir,
            outputWavFileName,
            textFileSaveMode,
            voicevoxOptions,
            cancellationToken);

        _logger.Debug($"{LogPrefix}: ファイル生成完了: {outputWavPath}");
        return outputWavPath;
    }

    private void EnsureOk(ResultCode result, string errorContext = "操作失敗")
    {
        if (result != ResultCode.RESULT_OK)
        {
            var message = result.ToMessage();
            var ex = new InvalidOperationException($"{errorContext}: {message}");
            _logger.Critical($"{LogPrefix}: {errorContext}: resultCode={result}, message={message}", ex);
            throw ex;
        }
    }

    private string ResolveOpenJTalkDictPath(string resourcePath)
    {
        _logger.Debug($"{LogPrefix}: OpenJTalk辞書パス解決: resourcePath={resourcePath}");

        var baseDir = Path.Combine(
            resourcePath,
            "engine_internal",
            "pyopenjtalk"
        );

        _logger.Debug($"{LogPrefix}: ベースディレクトリ: {baseDir}");

        if (!Directory.Exists(baseDir))
        {
            var ex = new DirectoryNotFoundException(
                $"pyopenjtalk directory not found: {baseDir}");
            _logger.Critical($"{LogPrefix}: pyopenjtalk ディレクトリが見つかりません: {baseDir}", ex);
            throw ex;
        }

        var dictDirs = Directory.EnumerateDirectories(
            baseDir,
            "open_jtalk_dic_utf_8-*",
            SearchOption.TopDirectoryOnly
        ).ToList();

        _logger.Debug($"{LogPrefix}: 検出された辞書ディレクトリ数={dictDirs.Count}");

        if (dictDirs.Count == 0)
        {
            var ex = new DirectoryNotFoundException(
                $"open_jtalk dictionary not found under: {baseDir}");
            _logger.Critical($"{LogPrefix}: open_jtalk 辞書が見つかりません: {baseDir}", ex);
            throw ex;
        }

        if (dictDirs.Count > 1)
        {
            _logger.Warn($"{LogPrefix}: 複数の辞書ディレクトリが見つかりました。最初のものを使用します。");
        }

        var selectedPath = dictDirs[0];
        _logger.Debug($"{LogPrefix}: 選択された辞書パス={selectedPath}");
        return selectedPath;
    }

    private string CreateWorkDirectory()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
        var outputDir = Path.Combine(_options.OutputBaseDirectory, timestamp);
        Directory.CreateDirectory(outputDir);
        return outputDir;
    }

    private async Task SaveTextFileAsync(
        string text,
        string outputDir,
        string outputWavFileName,
        TextFileSaveMode saveMode,
        VoicevoxGenerateOptions? options,
        CancellationToken cancellationToken)
    {
        if (saveMode == TextFileSaveMode.None)
        {
            _logger.Debug($"{LogPrefix}: テキストファイル保存なし (Mode: None)");
            return;
        }

        var baseFileName = Path.GetFileNameWithoutExtension(outputWavFileName);

        if (saveMode == TextFileSaveMode.TextOnly)
        {
            var textFilePath = Path.Combine(outputDir, $"{baseFileName}.txt");
            _logger.Debug($"{LogPrefix}: テキストファイル保存中: {textFilePath}");

            await File.WriteAllTextAsync(textFilePath, text, System.Text.Encoding.UTF8, cancellationToken);

            _logger.Debug($"{LogPrefix}: テキストファイル保存完了");
        }
        else if (saveMode == TextFileSaveMode.WithMetadata)
        {
            var jsonFilePath = Path.Combine(outputDir, $"{baseFileName}.json");
            _logger.Debug($"{LogPrefix}: メタデータファイル保存中: {jsonFilePath}");

            var metadata = new VoicevoxMetadata
            {
                Text = text,
                GeneratedAt = DateTime.UtcNow.ToString("O"),
                OutputFileName = outputWavFileName,
                Options = options
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(metadata, jsonOptions);
            await File.WriteAllTextAsync(jsonFilePath, json, System.Text.Encoding.UTF8, cancellationToken);

            _logger.Debug($"{LogPrefix}: メタデータファイル保存完了");
        }
    }

    public void Dispose()
    {
        _logger.Debug($"{LogPrefix}: リソース破棄開始");
        _synthesizer.Dispose();
        _gate.Dispose();
        _logger.Debug($"{LogPrefix}: リソース破棄完了");
    }
}
