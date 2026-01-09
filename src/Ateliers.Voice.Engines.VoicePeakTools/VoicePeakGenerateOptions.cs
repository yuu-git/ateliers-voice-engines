using System.Text.Json.Serialization;

namespace Ateliers.Voice.Engines.VoicePeakTools;

/// <summary>
/// VoicePeak 音声生成オプション
/// </summary>
public sealed class VoicePeakGenerateOptions
{
    /// <summary>
    /// ナレーター名
    /// </summary>
    [JsonPropertyName("narratorName")]
    public string? NarratorName { get; init; }

    /// <summary>
    /// 感情パラメータ文字列
    /// </summary>
    [JsonPropertyName("emotionParameters")]
    public string? EmotionParameters { get; init; }

    /// <summary>
    /// 速度調整パラメーター（50-200）
    /// </summary>
    [JsonPropertyName("speed")]
    public int? Speed { get; init; }

    /// <summary>
    /// 音高調整パラメーター（-300-300）
    /// </summary>
    [JsonPropertyName("pitch")]
    public int? Pitch { get; init; }

    /// <summary>
    /// テキストファイルの保存モード（デフォルト: TextOnly）
    /// </summary>
    [JsonPropertyName("textFileSaveMode")]
    public TextFileSaveMode TextFileSaveMode { get; init; } = TextFileSaveMode.TextOnly;
}
