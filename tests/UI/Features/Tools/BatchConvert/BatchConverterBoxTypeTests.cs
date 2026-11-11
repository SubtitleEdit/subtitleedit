using System.Reflection;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// The export-images dialog saves a box type and box padding in the profile, but batch convert
/// to an image format never read them, so a boxed profile rendered plain outlined text.
/// </summary>
public class BatchConverterBoxTypeTests
{
    private const string InputSrt = "1\n00:00:01,000 --> 00:00:03,000\nBoxed text.\n";

    private static ImageParameter RenderFirstLine(SeExportImagesProfile profile)
    {
        var exportImages = Se.Settings.File.ExportImages;
        var oldProfiles = exportImages.Profiles;
        var oldLastProfileName = exportImages.LastProfileName;
        try
        {
            profile.ProfileName = "Test";
            exportImages.Profiles = [profile];
            exportImages.LastProfileName = "Test";

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(new BatchConvertConfig { TargetFormatName = BatchConverter.FormatVobSub });

            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, InputSrt.SplitToLines(), "movie.srt");
            var item = new BatchConvertItem("movie.srt", 1, new SubRip().Name, subtitle);

            var images = (OcrSubtitleImageParameter)typeof(BatchConverter)
                .GetMethod("CreateImageSubtitles", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(converter, [item])!;
            return images.GetImageParameter(0)!;
        }
        finally
        {
            exportImages.Profiles = oldProfiles;
            exportImages.LastProfileName = oldLastProfileName;
        }
    }

    [Theory]
    [InlineData(FontBoxType.None, ExportBoxType.None)]
    [InlineData(FontBoxType.OneBox, ExportBoxType.OneBox)]
    [InlineData(FontBoxType.BoxPerLine, ExportBoxType.BoxPerLine)]
    public void BoxType_FollowsProfile(FontBoxType profileBoxType, ExportBoxType expected)
    {
        var ip = RenderFirstLine(new SeExportImagesProfile { BoxType = profileBoxType });

        Assert.Equal(expected, ip.BoxType);
    }

    [Fact]
    public void BoxPadding_FollowsProfile()
    {
        var ip = RenderFirstLine(new SeExportImagesProfile
        {
            BoxType = FontBoxType.OneBox,
            BoxPaddingLeft = 11,
            BoxPaddingRight = 12,
            BoxPaddingTop = 13,
            BoxPaddingBottom = 14,
        });

        Assert.Equal(11, ip.BoxPaddingLeft);
        Assert.Equal(12, ip.BoxPaddingRight);
        Assert.Equal(13, ip.BoxPaddingTop);
        Assert.Equal(14, ip.BoxPaddingBottom);
    }

    [Fact]
    public void OneBox_DrawsBackgroundAroundText()
    {
        var plain = RenderFirstLine(new SeExportImagesProfile { BoxType = FontBoxType.None, OutlineWidth = 0, ShadowWidth = 0 });
        var boxed = RenderFirstLine(new SeExportImagesProfile
        {
            BoxType = FontBoxType.OneBox,
            BackgroundColor = "#ff0000ff",
            BoxPaddingLeft = 20,
            BoxPaddingRight = 20,
            BoxPaddingTop = 20,
            BoxPaddingBottom = 20,
            OutlineWidth = 0,
            ShadowWidth = 0,
        });

        Assert.True(boxed.Bitmap!.Width > plain.Bitmap!.Width, $"boxed {boxed.Bitmap.Width} vs plain {plain.Bitmap.Width}");
    }
}
