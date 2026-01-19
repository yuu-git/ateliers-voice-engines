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
    private readonly ILogger? _logger;
    private readonly IExecutionContext? _context;
    private readonly Synthesizer _synthesizer;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private const string LogPrefix = nameof(VoicevoxVoiceGenerator);

    public VoicevoxVoiceGenerator(
        VoicevoxOptions options,
        ILogger? logger = null,
        IExecutionContext? context = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _context = context;

        using var initScope = _context?.BeginScope($"{LogPrefix}:Initialize");
        
        _logger?.Info($"{LogPrefix}: 初期化を開始");

        // ネイティブライブラリパスを解決して登録
        ResolveAndRegisterNativeLibraries(options.ResourcePath);

        // OpenJTalk辞書パスを解決
        var openJTalkDictPath = ResolveOpenJTalkDictPath(options.ResourcePath);

        // OpenJTalk初期化
        _logger?.Debug($"{LogPrefix}: OpenJTalk を初期化中...");
        var result = OpenJtalk.New(openJTalkDictPath, out var openJtalk);
        EnsureOk(result, "OpenJTalkの初期化に失敗");

        // ONNX Runtime初期化
        _logger?.Debug($"{LogPrefix}: ONNX Runtime を初期化中...");
        result = Onnxruntime.LoadOnce(
            LoadOnnxruntimeOptions.Default(),
            out var onnxruntime);
        EnsureOk(result, "ONNX Runtimeの初期化に失敗");

        // Synthesizer初期化
        _logger?.Debug($"{LogPrefix}: Synthesizer を初期化中...");
        result = Synthesizer.New(
            onnxruntime,
            openJtalk,
            InitializeOptions.Default(),
            out _synthesizer);
        EnsureOk(result, "Synthesizerの初期化に失敗");

        // Voice modelsの読み込み
        var modelDir = ResolveModelDirectory(options.ResourcePath);
        _logger?.Debug($"{LogPrefix}: 音声モデルディレクトリ: {modelDir}");

        var matcher = new Matcher();
        matcher.AddIncludePatterns(new[] { "*.vvm" });

        var allModelPaths = matcher
            .GetResultsInFullPath(modelDir)
            .ToList();

        _logger?.Debug($"{LogPrefix}: 検出された音声モデル数: {allModelPaths.Count}個");

        IEnumerable<string> modelPathsToLoad;

        if (options.VoiceModelNames is null || options.VoiceModelNames.Count == 0)
        {
            modelPathsToLoad = allModelPaths;
            _logger?.Info($"{LogPrefix}: すべての音声モデルを読み込みます: {allModelPaths.Count}個");
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

            _logger?.Info($"{LogPrefix}: 指定された音声モデルを読み込みます: {string.Join(", ", options.VoiceModelNames)}");
        }

        if (!modelPathsToLoad.Any())
        {
            var ex = new InvalidOperationException(
                "No matching voice models (*.vvm) were found. " +
                "Please check VoiceModelNames in VoicevoxOptions.");
            _logger?.Critical($"{LogPrefix}: 初期化失敗: 音声モデルが見つかりません", ex);
            throw ex;
        }

        _logger?.Info($"{LogPrefix}: 音声モデルを読み込み中: {modelPathsToLoad.Count()}個");
        var loadedCount = 0;
        foreach (var path in modelPathsToLoad)
        {
            _logger?.Debug($"{LogPrefix}: 音声モデル読み込み中: {Path.GetFileName(path)}");
            result = VoiceModelFile.Open(path, out var voiceModel);
            EnsureOk(result, $"音声モデル読み込み失敗: {Path.GetFileName(path)}");

            result = _synthesizer.LoadVoiceModel(voiceModel);
            EnsureOk(result, $"音声モデルロード失敗: {Path.GetFileName(path)}");

            voiceModel.Dispose();
            loadedCount++;
        }

        _logger?.Info($"{LogPrefix}: 音声モデル読み込み完了: {loadedCount}個");

        openJtalk.Dispose();

        _logger?.Info($"{LogPrefix}: 初期化完了");
    }

    /// <summary>
    /// 音声ファイルを生成します
    /// </summary>
    public async Task<VoicevoxGenerateResult> GenerateVoiceFileAsync(
        VoicevoxGenerateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var scope = _context?.BeginScope($"{LogPrefix}:GenerateVoiceFile");
        
        _logger?.Info($"{LogPrefix}: 音声生成開始: text={request.Text.Length}文字, outputWavFileName={request.OutputWavFileName}");

        var stopwatch = Stopwatch.StartNew();

        var outputDir = CreateWorkDirectory();
        _logger?.Debug($"{LogPrefix}: 出力ディレクトリ: {outputDir}");

        var voicevoxOptions = request.Options;
        var styleId = voicevoxOptions?.StyleId;

        var outputWavPath = await SynthesizeToFileAsync(
            request,
            outputDir,
            voicevoxOptions,
            styleId,
            cancellationToken);

        stopwatch.Stop();

        _logger?.Info($"{LogPrefix}: 音声生成完了: outputWavPath={outputWavPath}, elapsed={stopwatch.Elapsed}");
        
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
        using var scope = _context?.BeginScope($"{LogPrefix}:GenerateVoiceFiles");
        
        var requestList = requests.ToList();
        _logger?.Info($"{LogPrefix}: 複数音声生成開始: リクエスト数={requestList.Count}個");

        var outputDir = CreateWorkDirectory();
        _logger?.Debug($"{LogPrefix}: 出力ディレクトリ: {outputDir}");

        var results = new List<VoicevoxGenerateResult>();
        var index = 0;

        foreach (var request in requestList)
        {
            index++;
            _logger?.Debug($"{LogPrefix}: 音声生成中 ({index}/{requestList.Count}): {request.OutputWavFileName}");

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

        _logger?.Info($"{LogPrefix}: 複数音声生成完了: {results.Count}個の音声ファイルを生成");
        return results;
    }

    private async Task<byte[]> SynthesizeAsync(
        string text,
        uint? styleId = null,
        CancellationToken cancellationToken = default)
    {
        _logger?.Debug($"{LogPrefix}: 音声合成開始: text={text.Length}文字, styleId={styleId}");

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var sid = styleId ?? _options.DefaultStyleId;
            _logger?.Debug($"{LogPrefix}: 使用するスタイルID={sid}");

            var result = _synthesizer.Tts(
                text,
                sid,
                TtsOptions.Default(),
                out _,
                out var wav);

            EnsureOk(result, "音声合成に失敗");

            _logger?.Debug($"{LogPrefix}: 音声合成完了: サイズ={wav!.Length}バイト");
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

        _logger?.Debug($"{LogPrefix}: ファイル生成開始: text={text.Length}文字, outputWavFileName={outputWavFileName}");

        if (string.IsNullOrWhiteSpace(outputWavFileName))
        {
            var ex = new ArgumentException(
                "OutputWavFileName must be specified",
                nameof(outputWavFileName));
            _logger?.Critical($"{LogPrefix}: 出力ファイル名が指定されていません", ex);
            throw ex;
        }

        var wav = await SynthesizeAsync(text, styleId, cancellationToken);

        var outputWavPath = Path.Combine(outputDir, outputWavFileName);
        _logger?.Debug($"{LogPrefix}: ファイルを書き込み中: {outputWavPath}");

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

        _logger?.Debug($"{LogPrefix}: ファイル生成完了: {outputWavPath}");
        return outputWavPath;
    }

    private void ResolveAndRegisterNativeLibraries(string resourcePath)
    {
        _logger?.Debug($"{LogPrefix}: ネイティブライブラリパス解決開始: resourcePath={resourcePath}");

        var registeredPaths = new List<string>();

        // パターン1: VOICEVOX Core の構造 (c_api/lib/*.dll, onnxruntime/lib/*.dll)
        var coreLibDirs = new[]
        {
            Path.Combine(resourcePath, "c_api", "lib"),
            Path.Combine(resourcePath, "onnxruntime", "lib")
        };

        foreach (var dir in coreLibDirs)
        {
            if (Directory.Exists(dir))
            {
                var dlls = Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly);
                if (dlls.Length > 0)
                {
                    _logger?.Info($"{LogPrefix}: ネイティブライブラリパスを登録 (VOICEVOX Core): {dir} ({dlls.Length}個の.dllファイル)");
                    foreach (var dll in dlls)
                    {
                        _logger?.Debug($"{LogPrefix}: - {Path.GetFileName(dll)}");
                    }
                    NativeLibraryPath.Use(dir);
                    registeredPaths.Add(dir);
                }
                else
                {
                    _logger?.Debug($"{LogPrefix}: {dir} は存在しますが、.dllファイルが見つかりません");
                }
            }
            else
            {
                _logger?.Debug($"{LogPrefix}: ディレクトリが存在しません: {dir}");
            }
        }

        // パターン2: VOICEVOX vv-engine の構造（ルートに .dll がある可能性）
        if (Directory.Exists(resourcePath))
        {
            var rootDlls = Directory.GetFiles(resourcePath, "*.dll", SearchOption.TopDirectoryOnly);
            if (rootDlls.Length > 0)
            {
                _logger?.Info($"{LogPrefix}: ネイティブライブラリパス（ルート）を登録 (VOICEVOX vv-engine): {resourcePath} ({rootDlls.Length}個の.dllファイル)");
                foreach (var dll in rootDlls)
                {
                    _logger?.Debug($"{LogPrefix}: - {Path.GetFileName(dll)}");
                }
                NativeLibraryPath.Use(resourcePath);
                registeredPaths.Add(resourcePath);
            }
            else
            {
            _logger?.Debug($"{LogPrefix}: ルートディレクトリに .dll ファイルが見つかりません: {resourcePath}");
            }
        }

        if (registeredPaths.Count > 0)
        {
            _logger?.Info($"{LogPrefix}: ネイティブライブラリパス登録完了: {registeredPaths.Count}個のディレクトリ");
        }
        else
        {
            _logger?.Warn($"{LogPrefix}: ネイティブライブラリが見つかりませんでした。動作に問題がある場合は、手動で NativeLibraryPath.Use() を呼び出してください。");
        }
    }

    private void EnsureOk(ResultCode result, string errorContext = "操作失敗")
    {
        if (result != ResultCode.RESULT_OK)
        {
            var message = result.ToMessage();
            var ex = new InvalidOperationException($"{errorContext}: {message}");
            _logger?.Critical($"{LogPrefix}: {errorContext}: resultCode={result}, message={message}", ex);
            throw ex;
        }
    }

    private string ResolveModelDirectory(string resourcePath)
    {
        _logger?.Debug($"{LogPrefix}: 音声モデルディレクトリ解決: resourcePath={resourcePath}");

        // パターン1: VOICEVOX Core のディレクトリ構造 (models/vvms/*.vvm)
        var coreModelsDir = Path.Combine(resourcePath, "models", "vvms");
        if (Directory.Exists(coreModelsDir))
        {
            var coreVvmFiles = Directory.GetFiles(coreModelsDir, "*.vvm");
            if (coreVvmFiles.Length > 0)
            {
                _logger?.Info($"{LogPrefix}: VOICEVOX Core パターンで音声モデルディレクトリを検出: {coreModelsDir} ({coreVvmFiles.Length}個の.vvmファイル)");
                return coreModelsDir;
            }
            _logger?.Debug($"{LogPrefix}: {coreModelsDir} は存在しますが、.vvmファイルが見つかりません");
        }

        // パターン2: VOICEVOX vv-engine のディレクトリ構造 (model/*.vvm)
        var vvEngineModelDir = Path.Combine(resourcePath, "model");
        if (Directory.Exists(vvEngineModelDir))
        {
            var vvEngineVvmFiles = Directory.GetFiles(vvEngineModelDir, "*.vvm");
            if (vvEngineVvmFiles.Length > 0)
            {
                _logger?.Info($"{LogPrefix}: VOICEVOX vv-engine パターンで音声モデルディレクトリを検出: {vvEngineModelDir} ({vvEngineVvmFiles.Length}個の.vvmファイル)");
                return vvEngineModelDir;
            }
            _logger?.Debug($"{LogPrefix}: {vvEngineModelDir} は存在しますが、.vvmファイルが見つかりません");
        }

        // どちらも見つからない場合
        var ex = new DirectoryNotFoundException(
            $"Voice model directory with *.vvm files not found. Searched patterns:\n" +
            $"  1. VOICEVOX Core: {coreModelsDir}\n" +
            $"  2. VOICEVOX vv-engine: {vvEngineModelDir}");
        _logger?.Critical($"{LogPrefix}: 音声モデルディレクトリが見つかりません", ex);
        throw ex;
    }

    private string ResolveOpenJTalkDictPath(string resourcePath)
    {
        _logger?.Debug($"{LogPrefix}: OpenJTalk辞書パス解決: resourcePath={resourcePath}");

        // パターン1: VOICEVOX Core (新バージョン) のディレクトリ構造
        // {resourcePath}/dict/open_jtalk_dic_utf_8-*
        _logger?.Debug($"{LogPrefix}: VOICEVOX Core (dict) パターンで検索中...");
        var coreDictBaseDir = Path.Combine(resourcePath, "dict");
        
        List<string> coreDictPatternDirs;
        if (Directory.Exists(coreDictBaseDir))
        {
            _logger?.Debug($"{LogPrefix}: dict ディレクトリが存在します: {coreDictBaseDir}");

            try
            {
                coreDictPatternDirs = Directory.EnumerateDirectories(
                    coreDictBaseDir,
                    "open_jtalk_dic_utf_8-*",
                    SearchOption.TopDirectoryOnly
                ).ToList();

                _logger?.Debug($"{LogPrefix}: 検索結果: {coreDictPatternDirs.Count}個のディレクトリが見つかりました");
                foreach (var dir in coreDictPatternDirs)
                {
                    _logger?.Debug($"{LogPrefix}: - {dir}");
                }
            }
            catch (Exception ex)
            {
                _logger?.Debug($"{LogPrefix}: VOICEVOX Core (dict) パターンの検索中にエラー: {ex.Message}");
                coreDictPatternDirs = new List<string>();
            }

            if (coreDictPatternDirs.Count > 0)
            {
                _logger?.Info($"{LogPrefix}: VOICEVOX Core (dict) パターンで辞書を検出: {coreDictPatternDirs.Count}個");

                if (coreDictPatternDirs.Count > 1)
                {
                    _logger?.Warn($"{LogPrefix}: 複数の辞書ディレクトリが見つかりました。最初のものを使用します。");
                }

                var selectedPath = coreDictPatternDirs[0];
                _logger?.Debug($"{LogPrefix}: 選択された辞書パス (VOICEVOX Core dict): {selectedPath}");
                return selectedPath;
            }
        }
        else
        {
            _logger?.Debug($"{LogPrefix}: dict ディレクトリが存在しません: {coreDictBaseDir}");
        }

        // パターン2: VOICEVOX Core (旧バージョン) のディレクトリ構造
        // {resourcePath}/open_jtalk_dic_utf_8-*
        _logger?.Debug($"{LogPrefix}: VOICEVOX Core (root) パターンで検索中...");
        
        List<string> corePatternDictDirs;
        try
        {
            corePatternDictDirs = Directory.EnumerateDirectories(
                resourcePath,
                "open_jtalk_dic_utf_8-*",
                SearchOption.TopDirectoryOnly
            ).ToList();

            _logger?.Debug($"{LogPrefix}: 検索結果: {corePatternDictDirs.Count}個のディレクトリが見つかりました");
            foreach (var dir in corePatternDictDirs)
            {
                _logger?.Debug($"{LogPrefix}: - {dir}");
            }
        }
        catch (Exception ex)
        {
            _logger?.Debug($"{LogPrefix}: VOICEVOX Core (root) パターンの検索中にエラー: {ex.Message}");
            corePatternDictDirs = new List<string>();
        }

        if (corePatternDictDirs.Count > 0)
        {
            _logger?.Info($"{LogPrefix}: VOICEVOX Core (root) パターンで辞書を検出: {corePatternDictDirs.Count}個");

            if (corePatternDictDirs.Count > 1)
            {
                _logger?.Warn($"{LogPrefix}: 複数の辞書ディレクトリが見つかりました。最初のものを使用します。");
            }

            var selectedPath = corePatternDictDirs[0];
            _logger?.Debug($"{LogPrefix}: 選択された辞書パス (VOICEVOX Core root): {selectedPath}");
            return selectedPath;
        }

        // パターン3: VOICEVOX vv-engine のディレクトリ構造
        // {resourcePath}/engine_internal/pyopenjtalk/open_jtalk_dic_utf_8-*
        _logger?.Debug($"{LogPrefix}: VOICEVOX vv-engine パターンで検索中...");
        var fullInstallBaseDir = Path.Combine(
            resourcePath,
            "engine_internal",
            "pyopenjtalk"
        );

        List<string> fullInstallDictDirs;
        if (Directory.Exists(fullInstallBaseDir))
        {
            _logger?.Debug($"{LogPrefix}: pyopenjtalk ディレクトリが存在します: {fullInstallBaseDir}");

            try
            {
                fullInstallDictDirs = Directory.EnumerateDirectories(
                    fullInstallBaseDir,
                    "open_jtalk_dic_utf_8-*",
                    SearchOption.TopDirectoryOnly
                ).ToList();

                _logger?.Debug($"{LogPrefix}: 検索結果: {fullInstallDictDirs.Count}個のディレクトリが見つかりました");
                foreach (var dir in fullInstallDictDirs)
                {
                    _logger?.Debug($"{LogPrefix}: - {dir}");
                }
            }
            catch (Exception ex)
            {
                _logger?.Debug($"{LogPrefix}: VOICEVOX vv-engine パターンの検索中にエラー: {ex.Message}");
                fullInstallDictDirs = new List<string>();
            }

            if (fullInstallDictDirs.Count > 0)
            {
                _logger?.Info($"{LogPrefix}: VOICEVOX vv-engine パターンで辞書を検出: {fullInstallDictDirs.Count}個");

                if (fullInstallDictDirs.Count > 1)
                {
                    _logger?.Warn($"{LogPrefix}: 複数の辞書ディレクトリが見つかりました。最初のものを使用します。");
                }

                var selectedPath = fullInstallDictDirs[0];
                _logger?.Debug($"{LogPrefix}: 選択された辞書パス (VOICEVOX vv-engine): {selectedPath}");
                return selectedPath;
            }
        }
        else
        {
            _logger?.Debug($"{LogPrefix}: pyopenjtalk ディレクトリが存在しません: {fullInstallBaseDir}");
        }

        // どのパターンでも見つからない場合
        // 実際に存在するディレクトリを列挙してエラーメッセージに含める
        var actualDirs = Directory.Exists(resourcePath)
            ? Directory.GetDirectories(resourcePath).Select(path => Path.GetFileName(path)).ToList()
            : [];

        var actualDirsMessage = actualDirs.Any()
            ? $"\n実際に存在するディレクトリ: {string.Join(", ", actualDirs)}"
            : "\n指定されたリソースパスが存在しません";

        var ex2 = new DirectoryNotFoundException(
            $"OpenJTalk dictionary not found. Searched patterns:\n" +
            $"  1. VOICEVOX Core (dict): {coreDictBaseDir}/open_jtalk_dic_utf_8-*\n" +
            $"  2. VOICEVOX Core (root): {resourcePath}/open_jtalk_dic_utf_8-*\n" +
            $"  3. VOICEVOX vv-engine: {fullInstallBaseDir}/open_jtalk_dic_utf_8-*" +
            actualDirsMessage);
        _logger?.Critical($"{LogPrefix}: OpenJTalk 辞書が見つかりません", ex2);
        throw ex2;
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
            _logger?.Debug($"{LogPrefix}: テキストファイル保存なし (Mode: None)");
            return;
        }

        var baseFileName = Path.GetFileNameWithoutExtension(outputWavFileName);

        if (saveMode == TextFileSaveMode.TextOnly)
        {
            var textFilePath = Path.Combine(outputDir, $"{baseFileName}.txt");
            _logger?.Debug($"{LogPrefix}: テキストファイル保存中: {textFilePath}");

            await File.WriteAllTextAsync(textFilePath, text, System.Text.Encoding.UTF8, cancellationToken);

            _logger?.Debug($"{LogPrefix}: テキストファイル保存完了");
        }
        else if (saveMode == TextFileSaveMode.WithMetadata)
        {
            var jsonFilePath = Path.Combine(outputDir, $"{baseFileName}.json");
            _logger?.Debug($"{LogPrefix}: メタデータファイル保存中: {jsonFilePath}");

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

            _logger?.Debug($"{LogPrefix}: メタデータファイル保存完了");
        }
    }

    public void Dispose()
    {
        _logger?.Debug($"{LogPrefix}: リソース破棄開始");
        _synthesizer.Dispose();
        _gate.Dispose();
        _logger?.Debug($"{LogPrefix}: リソース破棄完了");
    }
}
