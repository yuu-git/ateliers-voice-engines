
namespace Ateliers.Voice.Engines.VoicePeakTools.Narrators;

public class UnknownNarrator : VoicePeakNarratorBase
{
    public UnknownNarrator(string name) : base(name, name, name) { }

    public override string Description => "名称から識別できない不明なナレーターです。使用できる感情パラメーターも不明です。";

    public override IEnumerable<VoicePeakEmotion> GetEmotions() => Enumerable.Empty<VoicePeakEmotion>();

    public override string GetEmotionString() => string.Empty;
}
