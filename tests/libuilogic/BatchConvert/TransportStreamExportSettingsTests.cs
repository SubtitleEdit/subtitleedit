using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.BatchConvert;

public class TransportStreamExportSettingsTests
{
    [Fact]
    public void FileNameEnding_EmptyTemplate_GivesNothing()
    {
        Assert.Equal(string.Empty, TransportStreamFileNameEnding.Format(string.Empty, "eng", 1234));
        Assert.Equal(string.Empty, TransportStreamFileNameEnding.Format(null, "eng", 1234));
    }

    [Fact]
    public void FileNameEnding_ThreeLetterLanguage_FillsAllPlaceholders()
    {
        var template = ".{two-letter-country-code}-{two-letter-country-code-uppercase}-{three-letter-country-code}-{three-letter-country-code-uppercase}";
        Assert.Equal(".en-EN-eng-ENG", TransportStreamFileNameEnding.Format(template, "eng", 1234));
    }

    [Fact]
    public void FileNameEnding_TwoLetterLanguage_ExpandsToThreeLetter()
    {
        Assert.Equal(".dan", TransportStreamFileNameEnding.Format(".{three-letter-country-code}", "da", 1234));
    }

    [Fact]
    public void FileNameEnding_NoLanguage_FallsBackToTrackId()
    {
        // No language in the PMT: SE4 used the PID/page so two anonymous tracks never collide.
        Assert.Equal(".888", TransportStreamFileNameEnding.Format(".{two-letter-country-code}", string.Empty, 888));
        Assert.Equal(".888", TransportStreamFileNameEnding.Format(".{three-letter-country-code-uppercase}", null, 888));
    }

    [Fact]
    public void FileNameEnding_LiteralTextIsKept()
    {
        Assert.Equal("_sub_en", TransportStreamFileNameEnding.Format("_sub_{two-letter-country-code}", "eng", 1));
    }

    [Fact]
    public void Sample_UsesEnglish()
    {
        Assert.Equal("MyVideoFile.en.sup", TransportStreamFileNameEnding.MakeSample(".{two-letter-country-code}"));
    }

    [Fact]
    public void Override_NothingEnabled_KeepsOriginalPosition()
    {
        var param = MakeParam(720, 576, 100, 40);
        TransportStreamExportOverride.Apply(param, new SKPointI(50, 500), new TransportStreamExportSettings());

        Assert.Equal(720, param.ScreenWidth);
        Assert.Equal(576, param.ScreenHeight);
        Assert.Equal(new SKPointI(50, 500), param.OverridePosition);
        Assert.Equal(100, param.Bitmap.Width);
    }

    [Fact]
    public void Override_UnknownPosition_LeavesOverrideNull()
    {
        var param = MakeParam(720, 576, 100, 40);
        TransportStreamExportOverride.Apply(param, new SKPointI(-1, -1), new TransportStreamExportSettings());
        Assert.Null(param.OverridePosition);
    }

    [Fact]
    public void Override_ScreenSize_ScalesBitmapAndPosition()
    {
        var param = MakeParam(720, 576, 100, 40);
        var settings = new TransportStreamExportSettings { OverrideScreenSize = true, ScreenWidth = 1440, ScreenHeight = 1152 };
        TransportStreamExportOverride.Apply(param, new SKPointI(50, 500), settings);

        Assert.Equal(1440, param.ScreenWidth);
        Assert.Equal(1152, param.ScreenHeight);
        Assert.Equal(200, param.Bitmap.Width);
        Assert.Equal(80, param.Bitmap.Height);
        Assert.Equal(new SKPointI(100, 1000), param.OverridePosition);
    }

    [Fact]
    public void Override_XCenter_YBottomMargin()
    {
        var param = MakeParam(1920, 1080, 200, 60);
        var settings = new TransportStreamExportSettings
        {
            OverrideXPosition = true,
            HAlign = TransportStreamExportSettings.HAlignCenter,
            OverrideYPosition = true,
            BottomMarginPercent = 10,
        };
        TransportStreamExportOverride.Apply(param, new SKPointI(5, 5), settings);

        // centered: (1920 - 200) / 2; bottom: 1080 - 108 - 60
        Assert.Equal(new SKPointI(860, 912), param.OverridePosition);
    }

    [Fact]
    public void Override_XLeftAndRight_UseMarginPercent()
    {
        var settings = new TransportStreamExportSettings
        {
            OverrideXPosition = true,
            HAlign = TransportStreamExportSettings.HAlignLeft,
            HMarginPercent = 5,
        };

        var left = MakeParam(1920, 1080, 200, 60);
        TransportStreamExportOverride.Apply(left, new SKPointI(700, 900), settings);
        Assert.Equal(new SKPointI(96, 900), left.OverridePosition); // X replaced, Y kept

        settings.HAlign = TransportStreamExportSettings.HAlignRight;
        var right = MakeParam(1920, 1080, 200, 60);
        TransportStreamExportOverride.Apply(right, new SKPointI(700, 900), settings);
        Assert.Equal(new SKPointI(1920 - 96 - 200, 900), right.OverridePosition);
    }

    [Fact]
    public void Override_OnlyY_WithUnknownX_LeavesOverrideNull()
    {
        var param = MakeParam(1920, 1080, 200, 60);
        var settings = new TransportStreamExportSettings { OverrideYPosition = true };
        TransportStreamExportOverride.Apply(param, new SKPointI(-1, -1), settings);
        Assert.Null(param.OverridePosition);
    }

    [Fact]
    public void Override_ScreenSizeThenPosition_UsesNewScreen()
    {
        var param = MakeParam(720, 576, 100, 40);
        var settings = new TransportStreamExportSettings
        {
            OverrideScreenSize = true,
            ScreenWidth = 1920,
            ScreenHeight = 1080,
            OverrideXPosition = true,
            OverrideYPosition = true,
            BottomMarginPercent = 5,
        };
        TransportStreamExportOverride.Apply(param, new SKPointI(50, 500), settings);

        var bitmapWidth = param.Bitmap.Width; // 100 * 1920/720 = 267
        var bitmapHeight = param.Bitmap.Height; // 40 * 1080/576 = 75
        Assert.Equal(267, bitmapWidth);
        Assert.Equal(75, bitmapHeight);
        Assert.Equal(new SKPointI((int)Math.Round(1920 / 2.0 - bitmapWidth / 2.0), 1080 - 54 - bitmapHeight), param.OverridePosition);
    }

    private static ImageParameter MakeParam(int screenWidth, int screenHeight, int bitmapWidth, int bitmapHeight)
    {
        return new ImageParameter
        {
            ScreenWidth = screenWidth,
            ScreenHeight = screenHeight,
            Bitmap = new SKBitmap(bitmapWidth, bitmapHeight),
        };
    }
}
