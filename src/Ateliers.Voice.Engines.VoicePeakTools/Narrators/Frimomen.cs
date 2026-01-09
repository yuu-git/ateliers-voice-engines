namespace Ateliers.Voice.Engines.VoicePeakTools.Narrators;

/// <summary>
/// ナレータークラス: フリモメン
/// </summary>
public class Frimomen : VoicePeakNarratorBase
{
    public Frimomen() : base("Frimomen", "フリモメン", "furimomen") { }

    /// <summary>
    /// 感情パラメーター: 幸せ
    /// </summary>
    public int Happy
    {
        get { return _happy.Value; }
        set { _happy.Value = value; }
    }

    /// <summary>
    /// 感情パラメーター: 怒り
    /// </summary>
    public int Angry
    {
        get { return _angry.Value; }
        set { _angry.Value = value; }
    }

    /// <summary>
    /// 感情パラメーター: 悲しみ
    /// </summary>
    public int Sad 
    {
        get { return _sad.Value; }
        set { _sad.Value = value; }
    }


    /// <summary>
    /// /// 感情パラメーター: お調子者
    /// </summary>
    public int Ochoushimono 
    {
        get { return _ochoushimono.Value; }
        set { _ochoushimono.Value = value; }
    }


    private VoicePeakEmotion _happy = new VoicePeakEmotion("happy", 0, "幸せ");
    private VoicePeakEmotion _angry = new VoicePeakEmotion("angry", 0, "怒り");
    private VoicePeakEmotion _sad = new VoicePeakEmotion("sad", 0, "悲しみ");
    private VoicePeakEmotion _ochoushimono = new VoicePeakEmotion("ochoushimono", 0, "お調子者");

    public override string Description =>
        "『フリモメン』は、声優・古賀明の声を元に製作した、深みのある低音ボイスが特徴です。" +
        "通常読み上げの他、「幸せ」「怒り」「悲しみ」「お調子者」という4種の感情表現にも対応しています。";

    public override IEnumerable<VoicePeakEmotion> GetEmotions()
    {
        yield return _happy;
        yield return _angry;
        yield return _sad;
        yield return _ochoushimono;
    }
}
