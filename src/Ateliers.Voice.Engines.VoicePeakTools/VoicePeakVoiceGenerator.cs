using System.Diagnostics;

namespace Ateliers.Voice.Engines.VoicePeakTools;

public class VoicePeakVoiceGenerator
{
    private readonly VoicePeakOptions _options;

    public VoicePeakVoiceGenerator(VoicePeakOptions options)
    {
        _options = options;
    }

    public async Task<string> GenerateVoiceFileAsync(
        VoicePeakGenerateRequest request,
        CancellationToken cancellationToken = default)
    {
        // CLI コマンドの構築
        string arguments = $"-s \"{request.Text}\" -n \"{request.Narrator}\" -o \"{request.OutputPath}\" --speed {request.Speed}";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = _options.VoicePeakExecutablePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            string errorOutput = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new Exception($"VoicePeak command failed with exit code {process.ExitCode}: {errorOutput}");
        }

        return request.OutputPath;
    }
}
