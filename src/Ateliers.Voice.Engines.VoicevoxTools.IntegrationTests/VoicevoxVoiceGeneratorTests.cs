using Ateliers.DependencyInjection;
using Ateliers.Logging.DependencyInjection;
using Ateliers.Voice.Engines;
using Ateliers.Voice.Engines.VoicevoxTools;
using Microsoft.Extensions.DependencyInjection;

namespace Ateliers.Voice.Engines.VoicevoxTools.IntegrationTests;

public sealed class VoicevoxVoiceGeneratorTests
{
    static VoicevoxVoiceGeneratorTests()
    {
        var path = @"C:\Program Files\VOICEVOX\vv-engine";
        if (Directory.Exists(path))
        {
            NativeLibraryPath.Use(path);
        }
    }

    // ★ 環境に合わせて書き換えてください
    private const string ResourcePath = @"C:\Program Files\VOICEVOX\vv-engine";

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WavFileIsGenerated()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAteliersExecutionContext();
        services.AddAteliersLogging(logging =>
        {
            logging
                .SetCategory("VoicevoxTest")
                .SetMinimumLevel(Logging.LogLevel.Debug)
                .AddConsole();
        });

        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<Ateliers.ILogger>();
        var context = provider.GetRequiredService<Ateliers.IExecutionContext>();

        var options = new VoicevoxOptions
        {
            ResourcePath = ResourcePath,
            VoiceModelNames = new[] { "0.vvm" },
            OutputBaseDirectory = "./test_output"
        };

        using var generator = new VoicevoxVoiceGenerator(options, logger, context);

        var request = new VoicevoxGenerateRequest
        {
            Text = "これはテスト音声です。",
            OutputWavFileName = "test_output.wav",
            Options = new VoicevoxGenerateOptions
            {
                StyleId = 1
            }
        };

        // Act
        var result = await generator.GenerateVoiceFileAsync(request);

        // Assert
        Assert.True(File.Exists(result.OutputWavPath));

        var fileInfo = new FileInfo(result.OutputWavPath);
        Assert.True(fileInfo.Length > 0);

        // wav ヘッダ確認
        using var fs = File.OpenRead(result.OutputWavPath);
        var header = new byte[4];
        await fs.ReadAsync(header);

        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(header));

        // テキストファイルが生成されていることを確認（デフォルト: TextOnly）
        var textFilePath = Path.Combine(Path.GetDirectoryName(result.OutputWavPath)!, "test_output.txt");
        Assert.True(File.Exists(textFilePath));

        var savedText = await File.ReadAllTextAsync(textFilePath);
        Assert.Equal("これはテスト音声です。", savedText);

        // 処理時間が記録されていることを確認
        Assert.True(result.Elapsed > TimeSpan.Zero);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WithMetadata_JsonFileIsGenerated()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAteliersExecutionContext();
        services.AddAteliersLogging(logging =>
        {
            logging
                .SetCategory("VoicevoxTest")
                .SetMinimumLevel(Logging.LogLevel.Information)
                .AddConsole();
        });

        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<Ateliers.ILogger>();
        var context = provider.GetRequiredService<Ateliers.IExecutionContext>();

        var options = new VoicevoxOptions
        {
            ResourcePath = ResourcePath,
            VoiceModelNames = new[] { "0.vvm" },
            OutputBaseDirectory = "./test_output"
        };

        using var generator = new VoicevoxVoiceGenerator(options, logger, context);

        var request = new VoicevoxGenerateRequest
        {
            Text = "メタデータ付きテスト音声です。",
            OutputWavFileName = "test_metadata.wav",
            Options = new VoicevoxGenerateOptions
            {
                StyleId = 2,
                SpeedScale = 1.2f,
                TextFileSaveMode = TextFileSaveMode.WithMetadata
            }
        };

        // Act
        var result = await generator.GenerateVoiceFileAsync(request);

        // Assert
        Assert.True(File.Exists(result.OutputWavPath));

        // JSON ファイルが生成されていることを確認
        var jsonFilePath = Path.Combine(Path.GetDirectoryName(result.OutputWavPath)!, "test_metadata.json");
        Assert.True(File.Exists(jsonFilePath));

        var json = await File.ReadAllTextAsync(jsonFilePath);
        Assert.Contains("メタデータ付きテスト音声です。", json);
        Assert.Contains("\"service\": \"Voicevox\"", json);
        Assert.Contains("\"styleId\": 2", json);
        Assert.Contains("\"speedScale\": 1.2", json);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFileAsync_WithNone_NoTextFileIsGenerated()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAteliersExecutionContext();
        services.AddAteliersLogging(logging =>
        {
            logging
                .SetCategory("VoicevoxTest")
                .SetMinimumLevel(Logging.LogLevel.Information)
                .AddConsole();
        });

        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<Ateliers.ILogger>();
        var context = provider.GetRequiredService<Ateliers.IExecutionContext>();

        var options = new VoicevoxOptions
        {
            ResourcePath = ResourcePath,
            VoiceModelNames = new[] { "0.vvm" },
            OutputBaseDirectory = "./test_output"
        };

        using var generator = new VoicevoxVoiceGenerator(options, logger, context);

        var request = new VoicevoxGenerateRequest
        {
            Text = "テキストファイルなしのテスト音声です。",
            OutputWavFileName = "test_no_text.wav",
            Options = new VoicevoxGenerateOptions
            {
                StyleId = 1,
                TextFileSaveMode = TextFileSaveMode.None
            }
        };

        // Act
        var result = await generator.GenerateVoiceFileAsync(request);

        // Assert
        Assert.True(File.Exists(result.OutputWavPath));

        // テキストファイルが生成されていないことを確認
        var textFilePath = Path.Combine(Path.GetDirectoryName(result.OutputWavPath)!, "test_no_text.txt");
        Assert.False(File.Exists(textFilePath));

        var jsonFilePath = Path.Combine(Path.GetDirectoryName(result.OutputWavPath)!, "test_no_text.json");
        Assert.False(File.Exists(jsonFilePath));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFilesAsync_MultipleWavFilesAreGenerated()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAteliersExecutionContext();
        services.AddAteliersLogging(logging =>
        {
            logging
                .SetCategory("VoicevoxTest")
                .SetMinimumLevel(Logging.LogLevel.Information)
                .AddConsole();
        });

        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<Ateliers.ILogger>();
        var context = provider.GetRequiredService<Ateliers.IExecutionContext>();

        var options = new VoicevoxOptions
        {
            ResourcePath = ResourcePath,
            VoiceModelNames = new[] { "0.vvm" },
            OutputBaseDirectory = "./test_output"
        };

        using var generator = new VoicevoxVoiceGenerator(options, logger, context);

        var requests = new[]
        {
            new VoicevoxGenerateRequest
            {
                Text = "最初の音声です。",
                OutputWavFileName = "multi_01.wav",
                Options = new VoicevoxGenerateOptions { StyleId = 1 }
            },
            new VoicevoxGenerateRequest
            {
                Text = "2番目の音声です。",
                OutputWavFileName = "multi_02.wav",
                Options = new VoicevoxGenerateOptions { StyleId = 1 }
            },
            new VoicevoxGenerateRequest
            {
                Text = "3番目の音声です。",
                OutputWavFileName = "multi_03.wav",
                Options = new VoicevoxGenerateOptions { StyleId = 1 }
            }
        };

        // Act
        var results = await generator.GenerateVoiceFilesAsync(requests);

        // Assert
        Assert.Equal(3, results.Count);

        // すべての WAV ファイルが生成されていることを確認
        foreach (var result in results)
        {
            Assert.True(File.Exists(result.OutputWavPath));
            
            var fileInfo = new FileInfo(result.OutputWavPath);
            Assert.True(fileInfo.Length > 0);
            Assert.True(result.Elapsed > TimeSpan.Zero);
        }

        // すべてのテキストファイルが生成されていることを確認（デフォルト: TextOnly）
        var expectedTexts = new[]
        {
            ("multi_01.txt", "最初の音声です。"),
            ("multi_02.txt", "2番目の音声です。"),
            ("multi_03.txt", "3番目の音声です。")
        };

        var outputDir = Path.GetDirectoryName(results[0].OutputWavPath)!;
        foreach (var (fileName, expectedText) in expectedTexts)
        {
            var textFilePath = Path.Combine(outputDir, fileName);
            Assert.True(File.Exists(textFilePath));

            var savedText = await File.ReadAllTextAsync(textFilePath);
            Assert.Equal(expectedText, savedText);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateVoiceFilesAsync_WithDifferentModes_FilesAreGeneratedCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAteliersExecutionContext();
        services.AddAteliersLogging(logging =>
        {
            logging
                .SetCategory("VoicevoxTest")
                .SetMinimumLevel(Logging.LogLevel.Information)
                .AddConsole();
        });

        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<Ateliers.ILogger>();
        var context = provider.GetRequiredService<Ateliers.IExecutionContext>();

        var options = new VoicevoxOptions
        {
            ResourcePath = ResourcePath,
            VoiceModelNames = new[] { "0.vvm" },
            OutputBaseDirectory = "./test_output"
        };

        using var generator = new VoicevoxVoiceGenerator(options, logger, context);

        var requests = new[]
        {
            new VoicevoxGenerateRequest
            {
                Text = "テキストのみ保存。",
                OutputWavFileName = "mode_text.wav",
                Options = new VoicevoxGenerateOptions
                {
                    StyleId = 1,
                    TextFileSaveMode = TextFileSaveMode.TextOnly
                }
            },
            new VoicevoxGenerateRequest
            {
                Text = "メタデータ付き保存。",
                OutputWavFileName = "mode_metadata.wav",
                Options = new VoicevoxGenerateOptions
                {
                    StyleId = 2,
                    SpeedScale = 1.1f,
                    TextFileSaveMode = TextFileSaveMode.WithMetadata
                }
            },
            new VoicevoxGenerateRequest
            {
                Text = "保存なし。",
                OutputWavFileName = "mode_none.wav",
                Options = new VoicevoxGenerateOptions
                {
                    StyleId = 1,
                    TextFileSaveMode = TextFileSaveMode.None
                }
            }
        };

        // Act
        var results = await generator.GenerateVoiceFilesAsync(requests);

        // Assert
        Assert.Equal(3, results.Count);

        var outputDir = Path.GetDirectoryName(results[0].OutputWavPath)!;

        // TextOnly: .txt ファイルのみ存在
        var textOnlyTxtPath = Path.Combine(outputDir, "mode_text.txt");
        Assert.True(File.Exists(textOnlyTxtPath));
        var textOnlyJsonPath = Path.Combine(outputDir, "mode_text.json");
        Assert.False(File.Exists(textOnlyJsonPath));

        // WithMetadata: .json ファイルのみ存在
        var metadataJsonPath = Path.Combine(outputDir, "mode_metadata.json");
        Assert.True(File.Exists(metadataJsonPath));
        var metadataTxtPath = Path.Combine(outputDir, "mode_metadata.txt");
        Assert.False(File.Exists(metadataTxtPath));

        var json = await File.ReadAllTextAsync(metadataJsonPath);
        Assert.Contains("メタデータ付き保存。", json);
        Assert.Contains("\"styleId\": 2", json);
        Assert.Contains("\"speedScale\": 1.1", json);

        // None: テキストファイルなし
        var noneTxtPath = Path.Combine(outputDir, "mode_none.txt");
        Assert.False(File.Exists(noneTxtPath));
        var noneJsonPath = Path.Combine(outputDir, "mode_none.json");
        Assert.False(File.Exists(noneJsonPath));
    }
}
