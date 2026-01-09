namespace Ateliers.Voice.Engines.VoicevoxTools;

/// <summary>
/// VOICEVOX エンジンの設定オプション
/// </summary>
public sealed class VoicevoxOptions
{
    /// <summary>
    /// VOICEVOX リソースディレクトリのパス
    /// </summary>
    public required string ResourcePath { get; init; }

    /// <summary>
    /// デフォルトのスタイルID
    /// </summary>
    public uint DefaultStyleId { get; init; } = 1;

    /// <summary>
    /// 読み込む voice model (*.vvm) のファイル名。
    /// null または空の場合は全モデルを読み込む。
    /// </summary>
    public IReadOnlyCollection<string>? VoiceModelNames { get; init; }

    /// <summary>
    /// 出力ディレクトリのベースパス
    /// </summary>
    public string OutputBaseDirectory { get; init; } = "./output";
}
