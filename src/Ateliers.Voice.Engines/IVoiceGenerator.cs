namespace Ateliers.Voice.Engines;

public interface IVoiceGenerator
{
    Task<VoiceGenerateResult> GenerateAsync(
        VoiceGenerateRequest request,
        CancellationToken cancellationToken = default);
}
