namespace Ateliers.Voice.Engines.VoicePeakTools;

/// <summary>
/// VOICEPEAK 音声生成のインターフェース
/// </summary>
public interface IVoicePeakVoiceGenerator
{
    /// <summary>
    /// 音声ファイルを生成します
    /// </summary>
    Task<VoicePeakGenerateResult> GenerateVoiceFileAsync(
        VoicePeakGenerateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数の音声ファイルを生成します
    /// </summary>
    Task<IReadOnlyList<VoicePeakGenerateResult>> GenerateVoicesFileAsync(
        IEnumerable<VoicePeakGenerateRequest> requests,
        CancellationToken cancellationToken = default);
}
