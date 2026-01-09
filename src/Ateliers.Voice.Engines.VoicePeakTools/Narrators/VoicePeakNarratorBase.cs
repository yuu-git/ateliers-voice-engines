namespace Ateliers.Voice.Engines.VoicePeakTools.Narrators;

public abstract class VoicePeakNarratorBase : IVoicePeakNarrator
{
    public string VoicePeakSystemName { get; }

    public string JpName { get; }

    public string EnName { get; }

    public abstract string Description { get; }


    public VoicePeakNarratorBase(string voicePeakSystemName, string jpNmae, string enName)
    {
        if (string.IsNullOrWhiteSpace(voicePeakSystemName))
        {
            throw new ArgumentException("VoicePeakSystemName cannot be null or whitespace.", nameof(voicePeakSystemName));
        }

        if (string.IsNullOrWhiteSpace(jpNmae))
        {
            throw new ArgumentException("JpName cannot be null or whitespace.", nameof(jpNmae));
        }

        if (string.IsNullOrWhiteSpace(enName))
        {
            throw new ArgumentException("EnName cannot be null or whitespace.", nameof(enName));
        }

        VoicePeakSystemName = voicePeakSystemName;
        JpName = jpNmae;
        EnName = enName;
    }

    public abstract IEnumerable<VoicePeakEmotion> GetEmotions();

    public virtual string GetEmotionString()
    {
        return GetEmotions().Select(e => e.ToParameterString()).Aggregate((a, b) => $"{a},{b}");
    }


    public virtual string GetInfomationMarkdown()
    {
        var lines = GetInfomationMarkdownLines();
        return string.Join(Environment.NewLine, lines);
    }

    public virtual IList<string> GetInfomationMarkdownLines()
    {
        var lines = new List<string>
        {
            $"# ナレーター情報",
            "",
            $"**ナレーター名**: {JpName} ({EnName})",
            $"**VoicePeak システム名**: `{VoicePeakSystemName}`",
            "",
            $"## 説明",
            "",
            Description,
            "",
            "## 感情パラメーター",
            ""
        };

        var emotions = GetEmotions();

        if (!emotions.Any())
        {
            lines.Add("このナレーターには感情パラメーターがありません。");
        }
        else
        {
            lines.Add("| パラメーター名 | 説明 | 値の範囲 |");
            lines.Add("| --- | --- | --- | --- |");
            foreach (var emotion in emotions)
            {
                var paramDescription = string.IsNullOrWhiteSpace(emotion.ParameterDescription) ? "説明なし" : emotion.ParameterDescription;
                lines.Add($"| `{emotion.ParameterName}` | {paramDescription} | 0 - 100 |");
            }

            lines.Add("");
            lines.Add("### 感情パラメーター文字列（現在の設定）");
            lines.Add("");
            lines.Add("```");
            lines.Add(GetEmotionString());
            lines.Add("```");
        }

        return lines;
    }

    public virtual void SetEmotionParameter(string emotionParameterString)
    {
        var emotions = emotionParameterString.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (emotions.Length == 0)
        {
            return;
        }

        var emotionParameters = GetEmotions();

        foreach (var emotion in emotions)
        {
            var parts = emotion.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                continue;
            }
            var paramName = parts[0].Trim().ToLower();
            if (!int.TryParse(parts[1].Trim(), out int paramValue))
            {
                continue;
            }

            var targetEmotion = emotionParameters.FirstOrDefault(e => e.ParameterName == paramName);
            if (targetEmotion is null)
            {
                continue;
            }

            targetEmotion.Value = paramValue;
        }
    }
}
