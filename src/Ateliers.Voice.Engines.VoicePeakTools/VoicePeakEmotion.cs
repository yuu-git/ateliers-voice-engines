namespace Ateliers.Voice.Engines.VoicePeakTools;


public class VoicePeakEmotion
{
    public VoicePeakEmotion(string parameterName, int value = 0, string parameterDescription = "")
    {
        ParameterName = parameterName.ToLower();
        Value = value;
        ParameterDescription = parameterDescription;
    }

    public string ParameterName { get; init; } = string.Empty;

    public string ParameterDescription { get; init; } = string.Empty;

    public int Value
    {
        get => _value;
        set => _value = ClampEmotionValue(value);
    }

    public int MinEmotionValue => 0;

    public int MaxEmotionValue => 100;

    private int _value = 0;

    protected int ClampEmotionValue(int value)
    {
        if (value < MinEmotionValue)
        {
            return MinEmotionValue;
        }
        else if (value > MaxEmotionValue)
        {
            return MaxEmotionValue;
        }
        else
        {
            return value;
        }
    }

    public string ToParameterString()
    {
        return $"{ParameterName.ToLower()}={Value}";
    }
}
