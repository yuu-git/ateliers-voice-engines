using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Ateliers.Voice.Engines.VoicePeakTools;

/// <summary>
/// VoicePeak 音声生成クラス
/// </summary>
public class VoicePeakVoiceGenerator : IVoicePeakVoiceGenerator
{
    private readonly ILogger? _logger;
    private readonly VoicePeakOptions _options;
    private const string LogPrefix = nameof(VoicePeakVoiceGenerator);

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="options"> VoicePeak オプションを指定します。</param>
    public VoicePeakVoiceGenerator(VoicePeakOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _options = options;
    }

    /// <summary>
    /// コンストラクタ (DI 用)
    /// </summary>
    /// <param name="logger"> ロガーを指定します。</param>
    /// <param name="options"> VoicePeak オプションを指定します。</param>
    public VoicePeakVoiceGenerator(ILogger logger, VoicePeakOptions options)
    {
        _logger = logger;

        if (options is null)
        {
            var ex = new ArgumentNullException(nameof(options));
            _logger?.Critical("VoicePeakVoiceGenerator の初期化に失敗しました。", ex);
            throw ex;
        }

        _options = options;
    }

    /// <summary>
    /// VoicePeak を使用して複数の音声ファイルを生成します。
    /// </summary>
    /// <param name="requests"> 音声生成リクエストの列挙を指定します。</param>
    /// <param name="cancellationToken"> 非同期のキャンセルトークンを指定します(省略可能)。</param>
    /// <returns> 生成された音声ファイルの結果の列挙。</returns>
    /// <remarks> ※ VoicePeak CLI を使用するため、処理に時間がかかる場合があります。 </remarks>
    public async Task<IReadOnlyList<VoicePeakGenerateResult>> GenerateVoicesFileAsync(
        IEnumerable<VoicePeakGenerateRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var results = new List<VoicePeakGenerateResult>();
        foreach (var request in requests)
        {
            var result = await GenerateVoiceFileAsync(request, cancellationToken);
            results.Add(result);
        }
        return results;
    }

    /// <summary>
    /// VoicePeak を使用して音声ファイルを生成します。
    /// </summary>
    /// <param name="request"> 音声生成リクエストを指定します。</param>
    /// <param name="cancellationToken"> 非同期のキャンセルトークンを指定します(省略可能)。</param>
    /// <returns> 生成された音声ファイルの結果。</returns>
    /// <remarks> ※ VoicePeak CLI を使用するため、処理に時間がかかる場合があります。 </remarks>
    public async Task<VoicePeakGenerateResult> GenerateVoiceFileAsync(
        VoicePeakGenerateRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger?.Info($"{LogPrefix}: 音声生成開始: text={request.Text.Length}文字, outputPath={request.OutputPath}");

        var stopwatch = Stopwatch.StartNew();

        string arguments = string.Empty;
        arguments += $" -s \"{request.Text}\"";
        arguments += $" -t \"{request.Text}.txt\"";
        arguments += $" -o \"{request.OutputPath}\"";
        arguments += $" -n \"{request.Narrator.VoicePeakSystemName}\"";
        arguments += $" -e \"{request.Narrator.GetEmotionString()}\"";
        arguments += $" --speed {request.Speed}";
        arguments += $" --pitch {request.Pitch}";

        var outputWavPath = await ExecuteVoicePeakAsync(request.OutputPath, arguments, cancellationToken);

        // テキスト/メタデータファイルの保存
        var textFileSaveMode = request.Options?.TextFileSaveMode ?? TextFileSaveMode.TextOnly;
        await SaveTextFileAsync(
            request.Text,
            outputWavPath,
            textFileSaveMode,
            request,
            cancellationToken);

        stopwatch.Stop();

        _logger?.Info($"{LogPrefix}: 音声生成完了: outputWavPath={outputWavPath}, elapsed={stopwatch.Elapsed}");

        return new VoicePeakGenerateResult
        {
            OutputWavPath = outputWavPath,
            Elapsed = stopwatch.Elapsed
        };
    }

    /// <summary>
    /// VoicePeak にインストールされている利用可能なナレーターの一覧を取得します。
    /// </summary>
    /// <param name="cancellationToken"> 非同期のキャンセルトークンを指定します(省略可能)。</param>
    /// <returns> ナレーター名の列挙。</returns>
    /// <exception cref="Exception"> VoicePeak コマンドの実行に失敗した場合。</exception>
    /// <remarks> ※ VoicePeak CLI を使用するため、処理に時間がかかる場合があります。 </remarks>
    public async Task<IEnumerable<string>> GetInstallNarratorsAsync(CancellationToken cancellationToken = default)
    {
        _logger?.Info($"{LogPrefix}: インストールされているナレーター一覧の取得開始");
        string arguments = "--list-narrator";
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = _options.VoicePeakExecutablePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        using var process = Process.Start(psi)!;
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            string errorOutput = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new Exception($"VoicePeak command failed with exit code {process.ExitCode}: {errorOutput}");
        }
        string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var narrators = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        _logger?.Info($"{LogPrefix}: インストールされているナレーター一覧の取得完了: {narrators.Length}人 ({string.Join(", ", narrators)})");

        return narrators;
    }

    /// <summary>
    /// デフォルトのナレーターを取得します。
    /// </summary>
    /// <returns> デフォルトナレーター。</returns>
    public IVoicePeakNarrator GetDefaultNarrator()
    {
        return VoicePeakNarraterFactory.CreateNarratorByName(_options.DefaultNarratorName);
    }

    private async Task<string> ExecuteVoicePeakAsync(string outputPath, string arguments, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(Path.GetDirectoryName(outputPath)!))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            _logger?.Info($"{LogPrefix}: 出力ディレクトリを作成しました: {Path.GetDirectoryName(outputPath)}");
        }

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = _options.VoicePeakExecutablePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        using var process = Process.Start(psi)!;
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            string errorOutput = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new Exception($"VoicePeak command failed with exit code {process.ExitCode}: {errorOutput}");
        }
        return outputPath;
    }

    private async Task SaveTextFileAsync(
        string text,
        string outputWavPath,
        TextFileSaveMode saveMode,
        VoicePeakGenerateRequest request,
        CancellationToken cancellationToken)
    {
        if (saveMode == TextFileSaveMode.None)
        {
            _logger?.Debug($"{LogPrefix}: テキストファイル保存なし (Mode: None)");
            return;
        }

        var outputDir = Path.GetDirectoryName(outputWavPath) ?? string.Empty;
        var baseFileName = Path.GetFileNameWithoutExtension(outputWavPath);

        if (saveMode == TextFileSaveMode.TextOnly)
        {
            var textFilePath = Path.Combine(outputDir, $"{baseFileName}.txt");
            _logger?.Debug($"{LogPrefix}: テキストファイル保存中: {textFilePath}");

            await File.WriteAllTextAsync(textFilePath, text, Encoding.UTF8, cancellationToken);

            _logger?.Debug($"{LogPrefix}: テキストファイル保存完了");
        }
        else if (saveMode == TextFileSaveMode.WithMetadata)
        {
            var jsonFilePath = Path.Combine(outputDir, $"{baseFileName}.json");
            _logger?.Debug($"{LogPrefix}: メタデータファイル保存中: {jsonFilePath}");

            var metadata = new VoicePeakMetadata
            {
                Text = text,
                GeneratedAt = DateTime.UtcNow.ToString("O"),
                OutputFileName = Path.GetFileName(outputWavPath),
                Options = CreateMetadataOptions(request)
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(metadata, jsonOptions);
            await File.WriteAllTextAsync(jsonFilePath, json, Encoding.UTF8, cancellationToken);

            _logger?.Debug($"{LogPrefix}: メタデータファイル保存完了");
        }
    }

    private VoicePeakGenerateOptions CreateMetadataOptions(VoicePeakGenerateRequest request)
    {
        return new VoicePeakGenerateOptions
        {
            NarratorName = request.Narrator.VoicePeakSystemName,
            EmotionParameters = request.Narrator.GetEmotionString(),
            Speed = request.Speed,
            Pitch = request.Pitch,
            TextFileSaveMode = request.Options?.TextFileSaveMode ?? TextFileSaveMode.TextOnly
        };
    }
}
