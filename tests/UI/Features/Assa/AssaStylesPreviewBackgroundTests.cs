using Nikse.SubtitleEdit.Features.Assa;
using SkiaSharp;

namespace UITests.Features.Assa;

/// <summary>
/// Styles dialog preview backgrounds: the transparent libass render on dark, checker or light.
/// </summary>
public class AssaStylesPreviewBackgroundTests
{
    [Theory]
    [InlineData(0, 40)]
    [InlineData(2, 232)]
    public void Background_FillsAroundTheText(int backgroundIndex, byte expectedGray)
    {
        using var render = new SKBitmap(100, 60);
        render.Erase(SKColors.Transparent);
        using var composed = AssaStylesViewModel.ComposeOnBackground(render, backgroundIndex);

        var pixel = composed.GetPixel(50, 30);
        Assert.Equal(expectedGray, pixel.Red);
    }

    [Fact]
    public void Checker_AlternatesCells()
    {
        using var render = new SKBitmap(100, 60);
        render.Erase(SKColors.Transparent);
        using var composed = AssaStylesViewModel.ComposeOnBackground(render, 1);

        Assert.NotEqual(composed.GetPixel(5, 5), composed.GetPixel(30, 5));
        Assert.Equal(composed.GetPixel(5, 5), composed.GetPixel(30, 30));
    }

    [Avalonia.Headless.XUnit.AvaloniaFact]
    public void PreviewText_IsSavedInSettings()
    {
        var old = Nikse.SubtitleEdit.Logic.Config.Se.Settings.Assa.StylesPreviewText;
        try
        {
            var vm = new AssaStylesViewModel(new Nikse.SubtitleEdit.Logic.Media.FileHelper(), new StubWindowService());
            vm.PreviewText = "Hello\\Nworld";
            Assert.Equal("Hello\\Nworld", Nikse.SubtitleEdit.Logic.Config.Se.Settings.Assa.StylesPreviewText);
            Assert.Equal("Hello\\Nworld", new AssaStylesViewModel(new Nikse.SubtitleEdit.Logic.Media.FileHelper(), new StubWindowService()).PreviewText);
        }
        finally
        {
            Nikse.SubtitleEdit.Logic.Config.Se.Settings.Assa.StylesPreviewText = old;
        }
    }
}
