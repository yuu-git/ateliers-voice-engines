namespace Ateliers.Voice.Engines.VoicevoxTools;

/// <summary>
/// VOICEVOX 音声生成リクエスト
/// </summary>
public sealed class VoicevoxGenerateRequest
{
    /// <summary>
    /// 音声合成するテキスト
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// 出力ファイル名（拡張子含む）
    /// </summary>
    public required string OutputWavFileName { get; init; }

    /// <summary>
    /// 音声生成オプション
    /// </summary>
    public VoicevoxGenerateOptions? Options { get; init; }
}
