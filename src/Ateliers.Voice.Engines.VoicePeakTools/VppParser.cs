using System.Text.Json;

namespace Ateliers.Voice.Engins.VoicePeakTools;

public class VppParser : IVppParser
{
    public VppFile ParseVppFile(string filePath)
    {
        string json = File.ReadAllText(filePath).TrimEnd('\0');

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<VppFile>(json, options);
    }

    public bool ValidateVppFile(string filePath)
    {
        try
        {
            ParseVppFile(filePath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}