using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ateliers.Voice.Engines.VoicePeakTools.Narrators;

/// <summary>
/// ナレータークラス: 夏色花梨
/// </summary>
public class NatukiKarin : VoicePeakNarratorBase
{
    /// <summary>
    /// ナレータークラスコンストラクター
    /// </summary>
    public NatukiKarin() : base("夏色花梨", "夏色花梨", "natukikarin") { }

    /// <summary>
    /// 感情パラメーター: ハイテンション
    /// </summary>
    [JsonPropertyName("hightension")]
    public int HighTension
    {
        get { return _highTension.Value; }
        set { _highTension.Value = value; }
    }

    /// <summary>
    /// 感情パラメーター: ブチギレ
    /// </summary>
    [JsonPropertyName("buchigire")]
    public int Buchigire
    {
        get { return _buchigire.Value; }
        set { _buchigire.Value = value; }
    }

    /// <summary>
    /// 感情パラメーター: 嘆き
    /// </summary>
    [JsonPropertyName("nageki")]
    public int Nageki
    {
        get { return _nageki.Value; }
        set { _nageki.Value = value; }
    }

    /// <summary>
    /// 感情パラメーター: 蔑み
    /// </summary>
    [JsonPropertyName("sagesumi")]
    public int Sagesumi
    {
        get { return _sagesumi.Value; }
        set { _sagesumi.Value = value; }
    }

    /// <summary>
    /// 感情パラメーター: ささやき
    /// </summary>
    [JsonPropertyName("sasayaki")]
    public int Sasayaki
    {
        get { return _sasayaki.Value; }
        set { _sasayaki.Value = value; }
    }

    private VoicePeakEmotion _highTension = new VoicePeakEmotion("hightension", 0, "ハイテンション");
    private VoicePeakEmotion _buchigire = new VoicePeakEmotion("buchigire", 0, "ブチギレ");
    private VoicePeakEmotion _nageki = new VoicePeakEmotion("nageki", 0, "嘆き");
    private VoicePeakEmotion _sagesumi = new VoicePeakEmotion("sagesumi", 0, "蔑み");
    private VoicePeakEmotion _sasayaki = new VoicePeakEmotion("sasayaki", 0, "ささやき");

    public override string Description =>
            "『VOICEPEAK 夏色花梨』は、声優「高木美佑」の声を元に制作した、芯のある可愛い声が特徴の入力文字読み上げソフトです。" +
            "通常読み上げの他、「ハイテンション」「ブチギレ」「嘆き」「蔑み」「ささやき」という5種の感情表現にも対応しています。";

    public override IEnumerable<VoicePeakEmotion> GetEmotions()
    {
        yield return _highTension;
        yield return _buchigire;
        yield return _nageki;
        yield return _sagesumi;
        yield return _sasayaki;
    }

    public async Task<FileInfo> SaveToJson(string outputPath)
    {
        var jsonString = ToJsonString();
        await File.WriteAllTextAsync(outputPath, jsonString);
        return new FileInfo(outputPath);
    }

    public string ToJsonString()
    {
        return JsonSerializer.Serialize(this);
    }
}