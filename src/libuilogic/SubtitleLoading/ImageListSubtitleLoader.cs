using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.SubtitleLoading;

/// <summary>
/// Recognizes the image-list subtitle files SE 4 opened for OCR: each cue names an image file
/// next to the subtitle (DOST, SpuImage, SE image HTML index, SON, SatBox png, Rhozet Harmonic
/// image, DVD Studio Pro space graphic, Sonic Scenarist bitmaps, EDL with png clips, SubRip with
/// image file names). The returned subtitle has the image file names as text, the shape the
/// BDN OCR import reads.
/// </summary>
public static class ImageListSubtitleLoader
{
    private static readonly Regex AlignmentTag = new Regex(@"\{\\an\d\}", RegexOptions.Compiled);

    private static readonly string[] ImageExtensions = [".bmp", ".png", ".jpg", ".tif"];

    /// <param name="fileName">The file being opened.</param>
    /// <param name="encoding">Encoding to read the file with.</param>
    /// <param name="parsedSubtitle">What Subtitle.Parse made of the file, if anything - text formats
    /// like SubRip, EDL, Scenarist or Adobe Encore (line/tabs) parse image-list files too, with the
    /// image file names as text.</param>
    public static Subtitle? TryLoad(string fileName, Encoding encoding, Subtitle? parsedSubtitle)
    {
        if (parsedSubtitle != null && !MostlyImageFileNames(parsedSubtitle))
        {
            return null;
        }

        List<string> lines;
        try
        {
            lines = FileUtil.ReadAllLinesShared(fileName, encoding);
        }
        catch
        {
            return null;
        }

        foreach (var format in GetCandidateFormats(fileName))
        {
            try
            {
                if (!format.IsMine(lines, fileName))
                {
                    continue;
                }

                var subtitle = new Subtitle();
                format.LoadSubtitle(subtitle, lines, fileName);
                if (subtitle.Paragraphs.Count == 0)
                {
                    continue;
                }

                if (format is DvdStudioProSpaceGraphic)
                {
                    // A non-default alignment is put in front of the graphic's file name.
                    foreach (var p in subtitle.Paragraphs)
                    {
                        p.Text = AlignmentTag.Replace(p.Text, string.Empty).Trim();
                        if (p.Text.StartsWith("<<Graphic>>", StringComparison.Ordinal))
                        {
                            p.Text = p.Text.Remove(0, "<<Graphic>>".Length).Trim();
                        }
                    }
                }

                subtitle.OriginalFormat = format;
                return subtitle;
            }
            catch
            {
                // not this format
            }
        }

        // SubRip or EDL whose cues are image file names - the parsed subtitle already has the
        // shape the OCR import reads. Same thresholds as SE 4.
        if (parsedSubtitle?.OriginalFormat is SubRip)
        {
            var imageCount = parsedSubtitle.Paragraphs.Count(p => EndsWithImageExtension(p.Text));
            return imageCount > 2 && imageCount >= parsedSubtitle.Paragraphs.Count - 2 ? parsedSubtitle : null;
        }

        if (parsedSubtitle?.OriginalFormat is Edl &&
            string.Equals(Path.GetExtension(fileName), ".edl", StringComparison.OrdinalIgnoreCase))
        {
            var pngCount = parsedSubtitle.Paragraphs.Count(p => p.Text.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
            return pngCount > 1 && pngCount > parsedSubtitle.Paragraphs.Count - 2 ? parsedSubtitle : null;
        }

        return null;
    }

    /// <summary>
    /// Loads an image-list xml project (BDN xml, or a Final Cut Pro image xmeml where each
    /// clipitem references a png) whose cues carry image file names for the OCR importer.
    /// Returns null when the file is neither.
    /// </summary>
    public static Subtitle? TryLoadImageListXml(string fileName)
    {
        try
        {
            var lines = FileUtil.ReadAllLinesShared(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName));

            var bdnXml = new BdnXml();
            if (bdnXml.IsMine(lines, fileName))
            {
                var subtitle = new Subtitle();
                bdnXml.LoadSubtitle(subtitle, lines, fileName);
                if (subtitle.Paragraphs.Count > 0)
                {
                    subtitle.OriginalFormat = bdnXml;
                    return subtitle;
                }
            }

            var timedImages = new TimedImagesXml();
            if (timedImages.IsMine(lines, fileName))
            {
                var subtitle = new Subtitle();
                timedImages.LoadSubtitle(subtitle, lines, fileName);
                if (subtitle.Paragraphs.Count > 0)
                {
                    subtitle.OriginalFormat = timedImages;
                    return subtitle;
                }
            }

            // Cheap content gate first - FinalCutProImage has no fast IsMine of its own.
            if (lines.Any(l => l.Contains("<xmeml", StringComparison.Ordinal)) &&
                lines.Any(l => l.Contains("<pathurl>", StringComparison.Ordinal)))
            {
                var fcpImage = new FinalCutProImage();
                var subtitle = new Subtitle();
                fcpImage.LoadSubtitle(subtitle, lines, fileName);
                if (subtitle.Paragraphs.Count > 0)
                {
                    subtitle.OriginalFormat = fcpImage;
                    return subtitle;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<SubtitleFormat> GetCandidateFormats(string fileName)
    {
        yield return new RhozetHarmonicImage();
        yield return new DvdStudioProSpaceGraphic();
        yield return new SpuImage();
        if (string.Equals(Path.GetExtension(fileName), ".dost", StringComparison.OrdinalIgnoreCase))
        {
            yield return new Dost();
        }

        yield return new SeImageHtmlIndex();
        yield return new Son();
        yield return new SatBoxPng();
        yield return new SonicScenaristBitmaps();
    }

    /// <summary>
    /// Cheap gate for files a text format already parsed: most cues must name an image, so an
    /// ordinary subtitle never pays for reading the file again.
    /// </summary>
    private static bool MostlyImageFileNames(Subtitle subtitle)
    {
        var count = subtitle.Paragraphs.Count;
        return count > 0 && subtitle.Paragraphs.Count(p => ContainsImageExtension(p.Text)) * 2 > count;
    }

    private static bool ContainsImageExtension(string text)
    {
        return ImageExtensions.Any(ext => text.Contains(ext, StringComparison.OrdinalIgnoreCase));
    }

    private static bool EndsWithImageExtension(string text)
    {
        return ImageExtensions.Any(ext => text.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }
}
