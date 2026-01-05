using Xunit;

namespace Ateliers.Voice.Engines.VoicevoxTools.UnitTests;

public class VoicevoxVoiceGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_ShouldCreateWaveFile()
    {
        var options = new VoicevoxOptions
        {
            VoicevoxExecutablePath = @"C:\Program Files\VOICEVOX\VOICEVOX.exe"
        };

        var generator = new VoicevoxVoiceGenerator(options);

        var output = Path.Combine(
            Path.GetTempPath(),
            $"voicevox_test_{Guid.NewGuid()}.wav");

        var request = new VoiceGenerateRequest
        {
            Text = "テストです。VOICEVOXの音声生成確認。",
            OutputWavePath = output,
            SpeakerId = 1
        };

        var result = await generator.GenerateAsync(request);

        Assert.True(File.Exists(result.OutputWavePath));

        //File.Delete(result.OutputWavePath);
    }
}
