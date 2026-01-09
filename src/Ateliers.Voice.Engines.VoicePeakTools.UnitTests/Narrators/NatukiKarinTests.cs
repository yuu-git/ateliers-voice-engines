using Ateliers.Voice.Engines.VoicePeakTools.Narrators;

namespace Ateliers.Voice.Engines.VoicePeakTools.UnitTests.Narrators;

public class NatukiKarinTests
{

    [Fact(DisplayName = @"夏色花梨の感情パラメーターのデフォルト値を確認")]
    public void NatukiKarin_EmotionParameters_ShouldHaveCorrectDefaults()
    {
        // Arrange
        var narrator = new NatukiKarin();
        // Act & Assert
        Assert.Equal(0, narrator.HighTension);
        Assert.Equal(0, narrator.Buchigire);
        Assert.Equal(0, narrator.Nageki);
        Assert.Equal(0, narrator.Sagesumi);
        Assert.Equal(0, narrator.Sasayaki);
    }

    [Fact(DisplayName = @"夏色花梨の感情パラメーターを文字列で設定できること")]
    public void NatukiKarin_SetEmotionParameter_ShouldSetValuesCorrectly()
    {
        // Arrange
        var narrator = new NatukiKarin();
        var emotionString = "hightension=50,buchigire=30,nageki=20,sagesumi=10,sasayaki=5";
        // Act
        narrator.SetEmotionParameter(emotionString);
        // Assert
        Assert.Equal(50, narrator.HighTension);
        Assert.Equal(30, narrator.Buchigire);
        Assert.Equal(20, narrator.Nageki);
        Assert.Equal(10, narrator.Sagesumi);
        Assert.Equal(5, narrator.Sasayaki);
    }
}
