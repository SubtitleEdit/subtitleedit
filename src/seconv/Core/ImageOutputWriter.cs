using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace SeConv.Core;

/// <summary>
/// Renders text subtitles into image-based output formats (Blu-Ray sup, VobSub, BDN-XML,
/// DOST, FCP, D-Cinema, images-with-time-code, WebVTT thumbnails). Each paragraph is
/// rendered to an SKBitmap via <see cref="ImageRenderer.GenerateBitmap"/> and fed through
/// the format-specific <see cref="IExportHandler"/>.
/// </summary>
internal static class ImageOutputWriter
{
    public sealed record FormatTarget(string Name, IExportHandler Handler);

    public static IExportHandler? TryCreateHandler(string normalizedFormat)
    {
        var n = normalizedFormat.Trim();
        return n switch
        {
            var x when x.Equals("Blu-ray sup", StringComparison.OrdinalIgnoreCase) || x.Equals("BluRaySup", StringComparison.OrdinalIgnoreCase)
                => new ExportHandlerBluRaySup(),
            var x when x.Equals("VobSub", StringComparison.OrdinalIgnoreCase)
                => new ExportHandlerVobSub(),
            var x when x.Equals("BDN-XML", StringComparison.OrdinalIgnoreCase) || x.Equals("BdnXml", StringComparison.OrdinalIgnoreCase)
                => new ExportHandlerBdnXml(),
            var x when x.Equals("DOST/image", StringComparison.OrdinalIgnoreCase) || x.Equals("Dost", StringComparison.OrdinalIgnoreCase)
                => new ExportHandlerDost(),
            var x when x.Equals("FCP/image", StringComparison.OrdinalIgnoreCase) || x.Equals("FcpImage", StringComparison.OrdinalIgnoreCase)
                => new ExportHandlerFcp(),
            var x when x.Equals("D-Cinema interop/png", StringComparison.OrdinalIgnoreCase) || x.Equals("DCinemaInterop", StringComparison.OrdinalIgnoreCase)
                => new ExportHandlerDCinemaInteropPng(),
            var x when x.Equals("D-Cinema SMPTE 2014/png", StringComparison.OrdinalIgnoreCase) || x.Equals("DCinemaSmpte2014", StringComparison.OrdinalIgnoreCase)
                => new ExportHandlerDCinemaSmpte2014Png(),
            var x when x.Equals("Images with time codes in file name", StringComparison.OrdinalIgnoreCase) || x.Equals("ImagesWithTimeCodes", StringComparison.OrdinalIgnoreCase)
                => new ExportHandlerImagesWithTimeCode(),
            _ => null,
        };
    }

    public static void Write(Subtitle subtitle, string filePath, IExportHandler handler, ConversionOptions options)
    {
        if (subtitle.Paragraphs.Count == 0)
        {
            throw new InvalidOperationException("No paragraphs to render.");
        }

        var (screenWidth, screenHeight) = options.Resolution ?? (1920, 1080);

        // Pre-render header with the first paragraph as a representative
        var firstParam = BuildParameter(subtitle.Paragraphs[0], 0, screenWidth, screenHeight, options);
        firstParam.Bitmap = ImageRenderer.GenerateBitmap(firstParam);
        handler.WriteHeader(filePath, firstParam);

        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var p = subtitle.Paragraphs[i];
            var ip = BuildParameter(p, i, screenWidth, screenHeight, options);
            ip.Bitmap = ImageRenderer.GenerateBitmap(ip);
            handler.CreateParagraph(ip);
            handler.WriteParagraph(ip);
            ip.Bitmap?.Dispose();
        }
        handler.WriteFooter();
    }

    private static ImageParameter BuildParameter(Paragraph p, int index, int screenWidth, int screenHeight, ConversionOptions options)
    {
        return new ImageParameter
        {
            Index = index,
            Text = p.Text ?? string.Empty,
            StartTime = p.StartTime.TimeSpan,
            EndTime = p.EndTime.TimeSpan,
            Alignment = ExportAlignment.BottomCenter,
            ContentAlignment = ExportContentAlignment.Center,
            FontName = "Arial",
            FontSize = 50,
            FontColor = SKColors.White,
            IsBold = false,
            OutlineColor = SKColors.Black,
            OutlineWidth = 2.5,
            ShadowColor = SKColors.Black,
            ShadowWidth = 0,
            BackgroundColor = SKColors.Transparent,
            BackgroundCornerRadius = 0,
            LineSpacingPercent = 100,
            ScreenWidth = screenWidth,
            ScreenHeight = screenHeight,
            BottomTopMargin = (int)(screenHeight * 0.05),
            LeftRightMargin = (int)(screenWidth * 0.05),
            PaddingLeftRight = 0,
            PaddingTopBottom = 0,
            FramesPerSecond = options.TargetFps ?? options.Fps ?? 25.0,
            IsRightToLeft = false,
            IsForced = false,
            IsFullFrame = false,
            Error = string.Empty,
        };
    }
}
