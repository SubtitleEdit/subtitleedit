using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Files.Compare;
using System.Reflection;

namespace UITests.Features.Files.Compare;

public class CompareColorsTests
{
    private static T GetPrivateStatic<T>(string name)
        => (T)typeof(CompareColors).GetField(name, BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

    [Theory]
    [InlineData("OnlyInOneFileRowLight", 255, 235, 233)]
    [InlineData("OnlyInOneFileRowDark", 255, 235, 233)]
    [InlineData("TextOrTimeDifferenceRowLight", 230, 255, 237)]
    [InlineData("TextOrTimeDifferenceRowDark", 230, 255, 237)]
    [InlineData("NumberDifferenceRowLight", 255, 248, 220)]
    [InlineData("NumberDifferenceRowDark", 255, 248, 220)]
    public void GetExportColor_MapsEveryThemeBrushToTheLightPastel(string brushField, byte r, byte g, byte b)
    {
        // The exported HTML page is white with black text, so a dark-theme row must still
        // export as its light twin.
        var color = CompareColors.GetExportColor(GetPrivateStatic<IBrush>(brushField));

        Assert.NotNull(color);
        Assert.Equal(Color.FromRgb(r, g, b), color!.Value);
    }

    [Fact]
    public void GetExportColor_ForAnUnhighlightedCell_IsNull()
    {
        Assert.Null(CompareColors.GetExportColor(new SolidColorBrush(Colors.Transparent)));
        Assert.Null(CompareColors.GetExportColor(null));
    }

    [Theory]
    [InlineData("OnlyInOneFileDark")]
    [InlineData("TextOrTimeDifferenceDark")]
    [InlineData("NumberDifferenceDark")]
    public void DarkHighlights_AreDarkEnoughToReadLightTextOn(string colorField)
    {
        // The light pastels were used in both themes, which left near-white rows under the
        // dark theme's near-white text (#13435).
        var color = GetPrivateStatic<Color>(colorField);
        var luminance = (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255.0;

        Assert.True(luminance < 0.3, $"{colorField} luminance {luminance:0.00} is too light for the dark theme");
    }

    [Theory]
    [InlineData("OnlyInOneFileLight")]
    [InlineData("TextOrTimeDifferenceLight")]
    [InlineData("NumberDifferenceLight")]
    public void LightHighlights_AreLightEnoughToReadDarkTextOn(string colorField)
    {
        var color = GetPrivateStatic<Color>(colorField);
        var luminance = (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255.0;

        Assert.True(luminance > 0.7, $"{colorField} luminance {luminance:0.00} is too dark for the light theme");
    }
}
