using System.Reflection;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch convert to an image format (VobSub, Blu-ray sup, ...) renders from the export-images
/// profile the target-format settings dialog saves, so every switch of that dialog has to be
/// saved in the profile and read back by the batch converter.
/// </summary>
public class BatchConverterImageProfileTests
{
    private const string PersianSrt = "1\n00:00:01,000 --> 00:00:03,000\nسال 2024 بود\n";

    private static ImageParameter RenderFirstLine(SeExportImagesProfile profile, string srt)
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
            new SubRip().LoadSubtitle(subtitle, srt.SplitToLines(), "movie.srt");
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

    /// <summary>#16831: "Right to left" was ignored, so numbers in Persian lines came out reversed.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RightToLeft_FollowsProfile(bool isRightToLeft)
    {
        var ip = RenderFirstLine(new SeExportImagesProfile { IsRightToLeft = isRightToLeft }, PersianSrt);

        Assert.Equal(isRightToLeft, ip.IsRightToLeft);
    }
}
