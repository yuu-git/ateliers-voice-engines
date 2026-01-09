namespace Ateliers.Voice.Engines.VoicevoxTools;

/// <summary>
/// VOICEVOX 音声生成のインターフェース
/// </summary>
public interface IVoicevoxVoiceGenerator : IDisposable
{
    /// <summary>
    /// 音声ファイルを生成します
    /// </summary>
    Task<VoicevoxGenerateResult> GenerateVoiceFileAsync(
        VoicevoxGenerateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数の音声ファイルを生成します
    /// </summary>
    Task<IReadOnlyList<VoicevoxGenerateResult>> GenerateVoiceFilesAsync(
        IEnumerable<VoicevoxGenerateRequest> requests,
        CancellationToken cancellationToken = default);
}