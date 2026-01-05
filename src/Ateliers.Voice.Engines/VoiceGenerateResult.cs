namespace Ateliers.Voice.Engines;

public sealed class VoiceGenerateResult
{
    public required string OutputWavePath { get; init; }
    public TimeSpan Elapsed { get; init; }
}
