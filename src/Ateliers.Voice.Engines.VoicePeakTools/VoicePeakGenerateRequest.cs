using Ateliers.Voice.Engines.VoicePeakTools.Narrators;

namespace Ateliers.Voice.Engines.VoicePeakTools;

/// <summary>
/// VoicePeak 音声生成リクエストクラス
/// </summary>
public class VoicePeakGenerateRequestWithParameterString : VoicePeakGenerateRequestBase
{
    public string NarratorName { get; set; } = string.Empty;

    public string EmotionParametersString { get; set; } = string.Empty;

    public override IEnumerable<string> Validate()
    {
        foreach (var msg in base.Validate())
        {
            yield return msg;
        }
        if (string.IsNullOrWhiteSpace(NarratorName))
        {
            yield return "ナレーター名が指定されていません。";
        }
    }
}

/// <summary>
/// VoicePeak 音声生成リクエストクラス (ナレーター指定版)
/// </summary>
public class VoicePeakGenerateRequest : VoicePeakGenerateRequestBase
{
    public IVoicePeakNarrator Narrator { get; set; } = new Frimomen();

    /// <summary>
    /// 音声生成オプション（メタデータ保存設定など）
    /// </summary>
    public VoicePeakGenerateOptions? Options { get; set; }

    public override IEnumerable<string> Validate()
    {
        foreach (var msg in base.Validate())
        {
            yield return msg;
        }
        if (Narrator == null)
        {
            yield return "ナレーターが指定されていません。デフォルトのフリモメンナレーターが使用されます。";
        }
    }
}

/// <summary>
/// VoicePeak 音声生成リクエスト基底クラス
/// </summary>
public abstract class VoicePeakGenerateRequestBase
{
    /// <summary>
    /// 読み上げテキスト
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 出力ファイルパス
    /// </summary>
    public string OutputPath { get; set; } = ".\\output.wav";

    /// <summary>
    /// 速度調整パラメーター
    /// </summary>
    /// <remarks>
    /// 最小値: 50 (0.5倍速) ～ 最大値: 200 (2.0倍速)
    /// </remarks>
    public int Speed
    {
        get { return ClampSpeedValue(_speed); }
        set { _speed = value; }
    }

    /// <summary>
    /// 音高調整パラメーター
    /// </summary>
    /// <remarks>
    /// 最小値: -300 ～ 最大値: 300
    /// </remarks>
    public int Pitch
    {
        get { return ClampPitchValue(_pitch); }
        set { _pitch = value; }
    }

    private int _speed = 100;
    private const int minSpeed = 50;
    private const int maxSpeed = 200;

    private int _pitch = 0;
    private const int minPitch = -300;
    private const int maxPitch = 300;

    protected int ClampSpeedValue(int value)
    {
        if (value < minSpeed)
        {
            return minSpeed;
        }
        else if (value > maxSpeed)
        {
            return maxSpeed;
        }
        else
        {
            return value;
        }
    }

    protected int ClampPitchValue(int value)
    {
        if (value < minPitch)
        {
            return minPitch;
        }
        else if (value > maxPitch)
        {
            return maxPitch;
        }
        else
        {
            return value;
        }
    }

    public virtual IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            yield return "読み上げテキストが指定されていません。";
        }
        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            yield return "出力ファイルパスが指定されていません。";
        }
    }

    public virtual IEnumerable<string> CheckParameterLimits()
    {
        if (Speed < minSpeed)
        {
            yield return $"速度調整パラメーターが {minSpeed} 未満です。現在の値: {Speed}。 最小値 {minSpeed} を使用します。";
        }
        else if (Speed > maxSpeed)
        {
            yield return $"速度調整パラメーターが {maxSpeed} を超えています。現在の値: {Speed}。 最大値 {maxSpeed} を使用します。";
        }

        if (Pitch < minPitch)
        {
            yield return $"音高調整パラメーターが {minPitch} 未満です。現在の値: {Pitch}。 最小値 {minPitch} を使用します。";
        }
        else if (Pitch > maxPitch)
        {
            yield return $"音高調整パラメーターが {maxPitch} を超えています。現在の値: {Pitch}。 最大値 {maxPitch} を使用します。";
        }
    }
}
