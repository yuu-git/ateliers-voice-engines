namespace Ateliers.Voice.Engines;

public sealed class VoiceGenerateRequest
{
    public required string Text { get; init; }

    /// <summary>
    /// 出力 wav ファイルのフルパス
    /// </summary>
    public required string OutputWavePath { get; init; }

    /// <summary>
    /// VOICEVOX の speaker ID（例: 1 = 四国めたん）
    /// </summary>
    public int SpeakerId { get; init; } = 1;
}
