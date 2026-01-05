namespace Ateliers.Voice.Engines.VoicePeakTools;

public class VoicePeakGenerateRequest
{
    public string Text { get; set; } = string.Empty;

    public string Narrator { get; set; } = "フリモメン";

    public string OutputPath { get; set; } = ".\\output.wav";

    public int Speed { get; set; } = 100;
}
