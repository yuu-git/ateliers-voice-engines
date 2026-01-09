using Ateliers.Voice.Engines.VoicePeakTools.Narrators;

namespace Ateliers.Voice.Engines.VoicePeakTools;

public interface IVoicePeakNarrator
{
    string VoicePeakSystemName { get; }

    string JpName { get; }

    string EnName { get; }

    public string Description { get; }

    IEnumerable<VoicePeakEmotion> GetEmotions();

    string GetEmotionString();

    string GetInfomationMarkdown();

    IList<string> GetInfomationMarkdownLines();

    void SetEmotionParameter(string emotionParameterString);
}
