using Ateliers.Logging;
using Ateliers.Voice.Engines;
using Ateliers.Voice.Engines.VoicePeakTools.Narrators;

namespace Ateliers.Voice.Engines.VoicePeakTools.IntegrationTests;

public class VoicePeakVoiceGeneratorTests
{
    // VoicePeak のインストールパス
    private const string VoicePeakExecutablePath = "C:\\Program Files\\VOICEPEAK\\voicepeak.exe";
    private const string TestOutputDirectory = "./";

    private ConsoleLogger CreateTestLogger()
    {
        var loggerOption = new LoggerOptions
        {
            MinimumLevel = LogLevel.Debug,
            EnableConsole = true,
        };
        return new ConsoleLogger(loggerOption);
    }

    [Fact(DisplayName = @"音声ファイル生成が成功すること")]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_ShouldGenerateVoiceFile()
    {
        // Arrange
        var options = new VoicePeakOptions
        {
            VoicePeakExecutablePath = VoicePeakExecutablePath
        };
        var generator = new VoicePeakVoiceGenerator(CreateTestLogger(), options);
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName("Frimomen");

        var request = new VoicePeakGenerateRequest
        {
            Text = "音声生成テストです",
            Narrator = narrator,
            // 実際の出力パスに置き換え
            OutputPath = Path.Combine(TestOutputDirectory, nameof(GenerateVoiceFileAsync_ShouldGenerateVoiceFile), "Output.wav")
        };
        // Act
        var result = await generator.GenerateVoiceFileAsync(request);
        // Assert
        Assert.Equal(request.OutputPath, result.OutputWavPath);
        File.Exists(result.OutputWavPath);
    }

    [Fact(DisplayName = @"感情パラメーターを設定して音声ファイル生成が成功すること")]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WithEmotionParameters_ShouldGenerateVoiceFile()
    {
        // Arrange
        var options = new VoicePeakOptions
        {
            // 実際のパスに置き換え
            VoicePeakExecutablePath = VoicePeakExecutablePath
        };
        var generator = new VoicePeakVoiceGenerator(options);
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName<NatukiKarin>("夏色花梨");

        narrator.HighTension = 20;
        narrator.Buchigire = 20;

        var request = new VoicePeakGenerateRequest
        {
            Text = "音声生成テストです",
            Narrator = narrator,
            // 実際の出力パスに置き換え
            OutputPath = Path.Combine(TestOutputDirectory, nameof(GenerateVoiceFileAsync_WithEmotionParameters_ShouldGenerateVoiceFile), "Output.wav")
        };
        // Act
        var result = await generator.GenerateVoiceFileAsync(request);
        // Assert
        Assert.Equal(request.OutputPath, result.OutputWavPath);
        File.Exists(result.OutputWavPath);
    }

    [Fact(DisplayName = @"テキストファイルのみ保存するモードで音声ファイル生成が成功すること")]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WithTextOnlyMode_ShouldGenerateVoiceFileAndTextFile()
    {
        // Arrange
        var options = new VoicePeakOptions
        {
            VoicePeakExecutablePath = VoicePeakExecutablePath
        };
        var generator = new VoicePeakVoiceGenerator(options);
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName("Frimomen");

        var request = new VoicePeakGenerateRequest
        {
            Text = "テキストファイル保存テストです",
            Narrator = narrator,
            OutputPath = Path.Combine(TestOutputDirectory, nameof(GenerateVoiceFileAsync_WithTextOnlyMode_ShouldGenerateVoiceFileAndTextFile), "Output.wav"),
            Options = new VoicePeakGenerateOptions
            {
                TextFileSaveMode = TextFileSaveMode.TextOnly
            }
        };

        // Act
        var result = await generator.GenerateVoiceFileAsync(request);

        // Assert
        Assert.Equal(request.OutputPath, result.OutputWavPath);
        Assert.True(File.Exists(result.OutputWavPath), "WAVファイルが存在すること");

        var textFilePath = Path.ChangeExtension(result.OutputWavPath, ".txt");
        Assert.True(File.Exists(textFilePath), "テキストファイルが存在すること");

        var savedText = await File.ReadAllTextAsync(textFilePath);
        Assert.Equal(request.Text, savedText);
    }

    [Fact(DisplayName = @"メタデータ付きで音声ファイル生成が成功すること")]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WithMetadataMode_ShouldGenerateVoiceFileAndJsonFile()
    {
        // Arrange
        var options = new VoicePeakOptions
        {
            VoicePeakExecutablePath = VoicePeakExecutablePath
        };
        var generator = new VoicePeakVoiceGenerator(options);
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName<NatukiKarin>("夏色花梨");
        narrator.HighTension = 50;

        var request = new VoicePeakGenerateRequest
        {
            Text = "メタデータ保存テストです",
            Narrator = narrator,
            OutputPath = Path.Combine(TestOutputDirectory, nameof(GenerateVoiceFileAsync_WithMetadataMode_ShouldGenerateVoiceFileAndJsonFile), "Output.wav"),
            Speed = 120,
            Pitch = 50,
            Options = new VoicePeakGenerateOptions
            {
                TextFileSaveMode = TextFileSaveMode.WithMetadata
            }
        };

        // Act
        var result = await generator.GenerateVoiceFileAsync(request);

        // Assert
        Assert.Equal(request.OutputPath, result.OutputWavPath);
        Assert.True(File.Exists(result.OutputWavPath), "WAVファイルが存在すること");

        var jsonFilePath = Path.ChangeExtension(result.OutputWavPath, ".json");
        Assert.True(File.Exists(jsonFilePath), "JSONファイルが存在すること");

        var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
        Assert.Contains("\"text\"", jsonContent);
        Assert.Contains("メタデータ保存テストです", jsonContent);
        Assert.Contains("\"service\": \"VoicePeak\"", jsonContent);
        Assert.Contains("\"narratorName\"", jsonContent);
        Assert.Contains("\"speed\": 120", jsonContent);
        Assert.Contains("\"pitch\": 50", jsonContent);
    }

    [Fact(DisplayName = @"テキストファイル保存なしモードで音声ファイルのみ生成されること")]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WithNoneMode_ShouldGenerateOnlyVoiceFile()
    {
        // Arrange
        var options = new VoicePeakOptions
        {
            VoicePeakExecutablePath = VoicePeakExecutablePath
        };
        var generator = new VoicePeakVoiceGenerator(options);
        var narrator = VoicePeakNarraterFactory.CreateNarratorByName("Frimomen");

        var request = new VoicePeakGenerateRequest
        {
            Text = "音声ファイルのみ生成テストです",
            Narrator = narrator,
            OutputPath = Path.Combine(TestOutputDirectory, nameof(GenerateVoiceFileAsync_WithNoneMode_ShouldGenerateOnlyVoiceFile), "Output.wav"),
            Options = new VoicePeakGenerateOptions
            {
                TextFileSaveMode = TextFileSaveMode.None
            }
        };

        // Act
        var result = await generator.GenerateVoiceFileAsync(request);

        // Assert
        Assert.Equal(request.OutputPath, result.OutputWavPath);
        Assert.True(File.Exists(result.OutputWavPath), "WAVファイルが存在すること");

        var textFilePath = Path.ChangeExtension(result.OutputWavPath, ".txt");
        var jsonFilePath = Path.ChangeExtension(result.OutputWavPath, ".json");
        Assert.False(File.Exists(textFilePath), "テキストファイルが存在しないこと");
        Assert.False(File.Exists(jsonFilePath), "JSONファイルが存在しないこと");
    }

    [Fact(DisplayName = @"VoicePeak にインストールされているナレーター一覧の取得が成功すること")]
    [Trait("Category", "Integration")]
    public async Task GetInstallNarratorsAsync_ShouldReturnNarratorList()
    {
        // Arrange
        var options = new VoicePeakOptions
        {
            // 実際のパスに置き換え
            VoicePeakExecutablePath = VoicePeakExecutablePath
        };
        var generator = new VoicePeakVoiceGenerator(CreateTestLogger(), options);
        // Act
        var narrators = await generator.GetInstallNarratorsAsync();
        // Assert
        Assert.NotEmpty(narrators);
        Assert.Contains("Frimomen", narrators); // 実際のナレーター名に置き換え
    }
}
