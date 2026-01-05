namespace Ateliers.Voice.Engines.VoicePeakTools.UnitTests;

public class VoicePeakVoiceGeneratorTests
{
    [Fact]
    public async Task GenerateVoiceFileAsync_ShouldGenerateVoiceFile()
    {
        // Arrange
        var options = new VoicePeakOptions
        {
            // 実際のパスに置き換え
            VoicePeakExecutablePath = "C:\\Program Files\\VOICEPEAK\\voicepeak.exe"
        };
        var generator = new VoicePeakVoiceGenerator(options);
        var request = new VoicePeakGenerateRequest
        {
            Text = "音声生成テストです",
            // 実際のナレーター名に置き換え
            Narrator = "夏色花梨", 
            // 実際の出力パスに置き換え
            OutputPath = "C:\\temp\\Output.wav", 
        };
        // Act
        var result = await generator.GenerateVoiceFileAsync(request);
        // Assert
        Assert.Equal(request.OutputPath, result);
        File.Exists(request.OutputPath);
    }
}
