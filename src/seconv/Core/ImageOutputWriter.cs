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
            // WebVTT thumbnail bundle: a directory of numbered PNG sprites plus an
            // index.vtt that references each by start/end time. The handler treats
            // the output path as a folder (creates it, writes PNGs + index.vtt
            // inside), so e.g. `seconv movie.srt webvttthumbnail` produces
            // `movie.vtt/0001.png`, `0002.png`, ..., `index.vtt`.
            var x when x.Equals("WebVTT Thumbnail", StringComparison.OrdinalIgnoreCase) || x.Equals("WebVttThumbnail", StringComparison.OrdinalIgnoreCase)
                => new ExportHandlerWebVttThumbnail(),
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

    /// <summary>
    /// Image-to-image variant: write a sequence of pre-rendered bitmaps (Blu-Ray .sup,
    /// VobSub, MKV PGS, DVB-sub) into <paramref name="handler"/> without going through
    /// <see cref="ImageRenderer"/>. Source dimensions baked into each
    /// <see cref="BitmapSubtitleLoader.BitmapSubtitleItem"/> override
    /// <see cref="ConversionOptions.Resolution"/> when present, so e.g. a 1920x1080
    /// Blu-Ray sup re-exports as 1920x1080 BDN-XML even if the CLI's default
    /// <c>--resolution</c> is something else.
    /// </summary>
    public static void WritePreservedBitmaps(
        IReadOnlyList<BitmapSubtitleLoader.BitmapSubtitleItem> items,
        string filePath,
        IExportHandler handler,
        ConversionOptions options)
    {
        if (items.Count == 0)
        {
            throw new InvalidOperationException("No bitmaps to write.");
        }

        var (defaultWidth, defaultHeight) = options.Resolution ?? (1920, 1080);

        var first = items[0];
        var firstParam = BuildPreservedParameter(first, 0, defaultWidth, defaultHeight, options);
        handler.WriteHeader(filePath, firstParam);

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var ip = BuildPreservedParameter(item, i, defaultWidth, defaultHeight, options);
            handler.CreateParagraph(ip);
            handler.WriteParagraph(ip);
            // We do NOT dispose item.Bitmap here — ownership stays with BitmapSubtitleItem;
            // the caller disposes the whole list when done. Disposing twice would crash.
        }
        handler.WriteFooter();
    }

    private static ImageParameter BuildPreservedParameter(
        BitmapSubtitleLoader.BitmapSubtitleItem item,
        int index,
        int defaultWidth,
        int defaultHeight,
        ConversionOptions options)
    {
        var screenWidth = item.ScreenWidth ?? defaultWidth;
        var screenHeight = item.ScreenHeight ?? defaultHeight;

        return new ImageParameter
        {
            Index = index,
            Text = string.Empty,
            Bitmap = item.Bitmap,
            StartTime = item.StartTime.TimeSpan,
            EndTime = item.EndTime.TimeSpan,
            Alignment = ExportAlignment.BottomCenter,
            ContentAlignment = ExportContentAlignment.Center,
            // Font/colour fields are still required by the ImageParameter contract even
            // though no rendering happens — handlers read them for metadata in some
            // formats (e.g. FCP XML). Mirror BuildParameter's defaults.
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
