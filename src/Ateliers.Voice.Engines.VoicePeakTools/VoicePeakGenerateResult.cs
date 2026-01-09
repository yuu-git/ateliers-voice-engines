namespace Ateliers.Voice.Engines.VoicePeakTools;

/// <summary>
/// VoicePeak 音声生成結果
/// </summary>
public sealed class VoicePeakGenerateResult
{
    /// <summary>
    /// 生成された WAV ファイルのフルパス
    /// </summary>
    public required string OutputWavPath { get; init; }

    /// <summary>
    /// 処理時間
    /// </summary>
    public TimeSpan Elapsed { get; init; }
}