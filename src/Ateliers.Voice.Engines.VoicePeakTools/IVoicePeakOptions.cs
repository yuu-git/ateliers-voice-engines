namespace Ateliers.Voice.Engines.VoicePeakTools;

public interface IVoicePeakOptions
{
    /// <summary>
    /// VoicePeak.exe のフルパス
    /// </summary>
    string VoicePeakExecutablePath { get; }

    /// <summary>
    /// デフォルトナレーター名
    /// </summary>
    string DefaultNarratorName { get; }
}
