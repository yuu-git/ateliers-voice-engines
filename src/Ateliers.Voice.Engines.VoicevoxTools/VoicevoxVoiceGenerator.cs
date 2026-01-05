using System.Diagnostics;

namespace Ateliers.Voice.Engines.VoicevoxTools;

public sealed class VoicevoxVoiceGenerator : IVoiceGenerator
{
    private readonly VoicevoxOptions _options;

    public VoicevoxVoiceGenerator(VoicevoxOptions options)
    {
        _options = options;
    }

    public async Task<VoiceGenerateResult> GenerateAsync(
        VoiceGenerateRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // 1. テキストを一時ファイルに書き出す
        var textFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(textFile, request.Text, cancellationToken);

        try
        {
            // 2. VOICEVOX CLI 実行
            var psi = new ProcessStartInfo
            {
                FileName = _options.VoicevoxExecutablePath,
                Arguments =
                    $"-s {request.SpeakerId} " +
                    $"-o \"{request.OutputWavePath}\" " +
                    $"\"{textFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("VOICEVOX process could not be started.");

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new InvalidOperationException(
                    $"VOICEVOX failed. ExitCode={process.ExitCode}, Error={error}");
            }

            return new VoiceGenerateResult
            {
                OutputWavePath = request.OutputWavePath,
                Elapsed = stopwatch.Elapsed
            };
        }
        finally
        {
            File.Delete(textFile);
        }
    }
}
