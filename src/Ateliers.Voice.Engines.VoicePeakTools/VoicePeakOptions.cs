namespace Ateliers.Voice.Engines.VoicePeakTools;

public sealed class VoicePeakOptions : IVoicePeakOptions
{
    /// <summary>
    /// VoicePeak.exe のフルパス
    /// </summary>
    public string VoicePeakExecutablePath { get; set; } = "C:\\Program Files\\VOICEPEAK\\voicepeak.exe";

    /// <summary>
    /// デフォルトナレーター名
    /// </summary>
    public string DefaultNarratorName { get; set; } = "Frimomen";
}