using System.Text.Json.Serialization;

namespace Ateliers.Voice.Engines.VoicevoxTools;

/// <summary>
/// VOICEVOX 音声生成のメタデータ
/// </summary>
public sealed class VoicevoxMetadata
{
    /// <summary>
    /// 音声生成に使用したテキスト
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// 生成日時（ISO 8601形式）
    /// </summary>
    [JsonPropertyName("generatedAt")]
    public string GeneratedAt { get; init; } = string.Empty;

    /// <summary>
    /// 使用したサービス名
    /// </summary>
    [JsonPropertyName("service")]
    public string Service { get; init; } = "Voicevox";

    /// <summary>
    /// 出力ファイル名
    /// </summary>
    [JsonPropertyName("outputFileName")]
    public string OutputFileName { get; init; } = string.Empty;

    /// <summary>
    /// 使用したオプション設定
    /// </summary>
    [JsonPropertyName("options")]
    public VoicevoxGenerateOptions? Options { get; init; }
}
