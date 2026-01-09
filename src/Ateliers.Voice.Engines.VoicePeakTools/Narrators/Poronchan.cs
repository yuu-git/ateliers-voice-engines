namespace Ateliers.Voice.Engines.VoicePeakTools.Narrators;

public class Poronchan : VoicePeakNarratorBase
{
    public Poronchan() : base("ポロンちゃん", "ポロンちゃん", "poronchan") { }

    /// <summary>
    /// 感情パラメーター: ロボ
    /// </summary>
    public int Robot
    {
        get { return _robot.Value; }
        set { _robot.Value = value; }
    }

    /// <summary>
    /// 感情パラメーター: ほんわか
    /// </summary>
    public int Mellow
    {
        get { return _mellow.Value; }
        set { _mellow.Value = value; }
    }

    /// <summary>
    /// 感情パラメーター: ぷんぷん
    /// </summary>
    public int PunPun
    {
        get { return _punpun.Value; }
        set { _punpun.Value = value; }
    }

    /// <summary>
    /// 感情パラメーター: 天才
    /// </summary>
    public int Genius 
    {
        get { return _genius.Value; }
        set { _genius.Value = value; }
    }

    /// <summary>
    /// 感情パラメーター: 泣き
    /// </summary>
    public int Teary
    {
        get { return _teary.Value; }
        set { _teary.Value = value; }
    }

    private VoicePeakEmotion _robot = new VoicePeakEmotion("robot", 0, "ロボ");
    private VoicePeakEmotion _mellow = new VoicePeakEmotion("mellow", 0, "ほんわか");
    private VoicePeakEmotion _punpun = new VoicePeakEmotion("punpun", 0, "ぷんぷん");
    private VoicePeakEmotion _genius = new VoicePeakEmotion("genius", 0, "天才");
    private VoicePeakEmotion _teary = new VoicePeakEmotion("teary", 0, "泣き");

    public override string Description =>
            "『ポロンちゃん』は、声優・木村朱里の声を元に製作した、少年ぽさもあるかわいらしい声が特徴です。" +
            "通常読み上げの他、「ロボ」「天才」「ほんわか」「ぷんぷん」「泣き」という5種の感情表現にも対応しています。";

    public override IEnumerable<VoicePeakEmotion> GetEmotions()
    {
        yield return _robot;
        yield return _mellow;
        yield return _punpun;
        yield return _genius;
        yield return _teary;
    }
}
