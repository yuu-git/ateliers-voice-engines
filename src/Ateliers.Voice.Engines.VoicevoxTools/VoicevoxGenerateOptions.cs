using System.Text.Json.Serialization;

namespace Ateliers.Voice.Engines.VoicevoxTools;

/// <summary>
/// VOICEVOX 音声生成オプション
/// </summary>
public sealed class VoicevoxGenerateOptions
{
    /// <summary>
    /// スタイルID（話者・感情の組み合わせ）
    /// </summary>
    [JsonPropertyName("styleId")]
    public uint? StyleId { get; init; }

    /// <summary>
    /// 話速スケール（デフォルト: 1.0）
    /// </summary>
    [JsonPropertyName("speedScale")]
    public float? SpeedScale { get; init; }

    /// <summary>
    /// ピッチスケール（デフォルト: 0.0）
    /// </summary>
    [JsonPropertyName("pitchScale")]
    public float? PitchScale { get; init; }

    /// <summary>
    /// イントネーションスケール（デフォルト: 1.0）
    /// </summary>
    [JsonPropertyName("intonationScale")]
    public float? IntonationScale { get; init; }

    /// <summary>
    /// 音量スケール（デフォルト: 1.0）
    /// </summary>
    [JsonPropertyName("volumeScale")]
    public float? VolumeScale { get; init; }

    /// <summary>
    /// 開始無音時間（秒）
    /// </summary>
    [JsonPropertyName("prePhonemeLength")]
    public float? PrePhonemeLength { get; init; }

    /// <summary>
    /// 終了無音時間（秒）
    /// </summary>
    [JsonPropertyName("postPhonemeLength")]
    public float? PostPhonemeLength { get; init; }

    /// <summary>
    /// テキストファイルの保存モード（デフォルト: TextOnly）
    /// </summary>
    [JsonPropertyName("textFileSaveMode")]
    public TextFileSaveMode TextFileSaveMode { get; init; } = TextFileSaveMode.TextOnly;
}
