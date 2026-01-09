using System.Text.Json.Serialization;

namespace Ateliers.Voice.Engines.VoicePeakTools
{
    /// <summary>
    /// VPPファイル全体構造
    /// </summary>
    public class VppFile
    {
        [JsonPropertyName("version")]
        public string Version { get; init; }

        [JsonPropertyName("project")]
        public VppProject Project { get; init; }

        [JsonPropertyName("voices")]
        public Dictionary<string, Voice> Voices { get; init; }
    }

    /// <summary>
    /// プロジェクト情報
    /// </summary>
    public class VppProject
    {
        [JsonPropertyName("params")]
        public ProjectParams Params { get; init; }

        [JsonPropertyName("emotions")]
        public Dictionary<string, double> Emotions { get; init; }

        [JsonPropertyName("global-emotions")]
        public List<GlobalEmotion> GlobalEmotions { get; init; }

        [JsonPropertyName("global-settings")]
        public List<GlobalSetting> GlobalSettings { get; init; }

        [JsonPropertyName("export")]
        public ExportSettings Export { get; init; }

        [JsonPropertyName("blocks")]
        public List<Block> Blocks { get; init; }
    }

    /// <summary>
    /// プロジェクトパラメータ
    /// </summary>
    public class ProjectParams
    {
        [JsonPropertyName("speed")]
        public double Speed { get; init; }
        [JsonPropertyName("pitch")]
        public double Pitch { get; init; }
        [JsonPropertyName("pause")]
        public double Pause { get; init; }
        [JsonPropertyName("volume")]
        public double Volume { get; init; }
    }

    /// <summary>
    /// グローバルエモーション
    /// </summary>
    public class GlobalEmotion
    {
        [JsonPropertyName("nar")]
        public string Narrator { get; init; }
        [JsonPropertyName("em")]
        public Dictionary<string, double> Emotions { get; init; }
    }

    /// <summary>
    /// グローバル設定
    /// </summary>
    public class GlobalSetting
    {
        [JsonPropertyName("nar")]
        public string Narrator { get; init; }

        [JsonPropertyName("pm")]
        public ProjectParams Params { get; init; }
    }

    /// <summary>
    /// エクスポート設定
    /// </summary>
    public class ExportSettings
    {
        [JsonPropertyName("mode")]
        public int Mode { get; init; }

        [JsonPropertyName("audio-format")]
        public int AudioFormat { get; init; }
        
        [JsonPropertyName("sample-rate")]
        public int SampleRate { get;    init; }

        [JsonPropertyName("write-scripts")]
        public bool WriteScripts { get; init; }

        [JsonPropertyName("write-text")]
        public bool WriteText { get; init; }

        [JsonPropertyName("write-srt")]
        public bool WriteSrt { get; init; }

        [JsonPropertyName("write-lab")]
        public bool WriteLab { get; init; }

        [JsonPropertyName("name-rule")]
        public bool NameRule { get; init; }

        [JsonPropertyName("char-code")]
        public int CharCode { get; init; }

        [JsonPropertyName("name-formats")]
        public List<int> NameFormats { get; init; }
    }

    /// <summary>
    /// ナレーションブロック
    /// </summary>
    public class Block
    {
        [JsonPropertyName("narrator")]
        public NarratorInfo Narrator { get; init; }

        [JsonPropertyName("time-offset-mode")]
        public double TimeOffsetMode { get; init; }

        [JsonPropertyName("time-offset")]
        public double TimeOffset { get; init; }

        [JsonPropertyName("params")]
        public ProjectParams Params { get; init; }

        [JsonPropertyName("emotions")]
        public Dictionary<string, double> Emotions { get; init; }
        
        [JsonPropertyName("sentence-list")]
        public List<Sentence> SentenceList { get; init; }

        [JsonPropertyName("sentence-ranges")]
        public List<List<int>> SentenceRanges { get; init; }
    }

    /// <summary>
    /// ナレーター情報
    /// </summary>
    public class NarratorInfo
    {
        [JsonPropertyName("key")]
        public string Key { get; init; }
        [JsonPropertyName("language")]
        public string Language { get; init; }
        [JsonPropertyName("narrator-version")]
        public int NarratorVersion { get; init; }
    }

    /// <summary>
    /// 文情報
    /// </summary>
    public class Sentence
    {
        [JsonPropertyName("text")]
        public string Text { get; init; }

        [JsonPropertyName("has-eos")]
        public bool HasEos { get; init; }

        [JsonPropertyName("tokens")]
        public List<Token> Tokens { get; init; }
    }

    /// <summary>
    /// トーク情報
    /// </summary>
    public class Token
    {
        [JsonPropertyName("s")]
        public string S { get; init; }

        [JsonPropertyName("pos")]
        public int Pos { get; init; }

        [JsonPropertyName("lang")]
        public int Lang { get; init; }

        [JsonPropertyName("pe")]
        public bool Pe { get; init; }

        [JsonPropertyName("syl")]
        public List<Syllable> Syl { get; init; }

        [JsonPropertyName("r8")]
        public List<int> R8 { get; init; }

        [JsonPropertyName("r32")]
        public List<int> R32 { get; init; }
    }

    /// <summary>
    /// 音節情報
    /// </summary>
    public class Syllable
    {
        [JsonPropertyName("s")]
        public string S { get; init; }

        [JsonPropertyName("ig")]
        public bool Ig { get; init; }

        [JsonPropertyName("a")]
        public int A { get; init; }

        [JsonPropertyName("i")]
        public double I { get; init; }

        [JsonPropertyName("u")]
        public bool U { get; init; }

        [JsonPropertyName("p")]
        public List<Phoneme> P { get; init; }
    }

    /// <summary>
    /// 音素情報
    /// </summary>
    public class Phoneme
    {
        [JsonPropertyName("s")]
        public string S { get; init; }

        [JsonPropertyName("d")]
        public double D { get; init; }

        [JsonPropertyName("n")]
        public bool N { get; init; }

        [JsonPropertyName("dt")]
        public int Dt { get; init; }
    }

    /// <summary>
    /// 音声情報
    /// </summary>
    public class Voice
    {
        [JsonPropertyName("latest")]
        public int Latest { get; init; }

        [JsonPropertyName("nid")]
        public string Nid { get; init; }
    }
}
