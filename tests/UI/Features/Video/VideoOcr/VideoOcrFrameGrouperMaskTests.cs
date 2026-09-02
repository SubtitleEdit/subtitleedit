using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using SkiaSharp;

namespace UITests.Features.Video.VideoOcr;

/// <summary>
/// Regression tests for the brightness mask: it must be thresholded at a resolution where
/// thin glyph strokes survive. Thresholding a small (96px) thumbnail directly averaged the
/// anti-aliased white strokes with their dark outlines to below the brightness minimum, so
/// frames with perfectly legible subtitles were classified blank and never OCR'ed.
/// </summary>
public class VideoOcrFrameGrouperMaskTests
{
    /// <summary>
    /// Writes a video-frame-crop-like JPEG: dark background with (optionally) thin white
    /// glyph-stroke-like lines bordered by black outlines, mimicking rendered subtitle text.
    /// </summary>
    private static string WriteFrame(string folder, string name, bool withText)
    {
        using var bitmap = new SKBitmap(720, 120);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(new SKColor(60, 70, 60)); // dark-ish video background

            if (withText)
            {
                // Rows of 2px-tall white strokes with 1px black outlines, spread like two
                // text lines - thin enough that a 7.5x downscale blends them to gray.
                using var outline = new SKPaint { Color = SKColors.Black };
                using var stroke = new SKPaint { Color = SKColors.White };
                foreach (var y in new[] { 40, 70 })
                {
                    for (var x = 150; x < 570; x += 14)
                    {
                        canvas.DrawRect(x - 1, y - 1, 12, 4, outline);
                        canvas.DrawRect(x, y, 10, 2, stroke);
                    }
                }
            }
        }

        var fileName = Path.Combine(folder, name);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        File.WriteAllBytes(fileName, data.ToArray());
        return fileName;
    }

    [Fact]
    public void Group_ThinBrightStrokes_AreNotBlank()
    {
        var folder = Path.Combine(Path.GetTempPath(), "vocr_mask_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var frames = new List<string>
            {
                WriteFrame(folder, "img000000.jpg", withText: true),
                WriteFrame(folder, "img000001.jpg", withText: true),
                WriteFrame(folder, "img000002.jpg", withText: true),
            };

            var groups = VideoOcrFrameGrouper.Group(frames, 190, 92, null, CancellationToken.None);

            Assert.True(groups.Count >= 1);
            Assert.All(groups, g => Assert.False(g.IsBlank));

            // Identical frames must also land in a single group (one OCR call).
            Assert.Single(groups);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void Group_NoText_IsBlank()
    {
        var folder = Path.Combine(Path.GetTempPath(), "vocr_mask_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var frames = new List<string>
            {
                WriteFrame(folder, "img000000.jpg", withText: false),
                WriteFrame(folder, "img000001.jpg", withText: false),
            };

            var groups = VideoOcrFrameGrouper.Group(frames, 190, 92, null, CancellationToken.None);

            Assert.Single(groups);
            Assert.True(groups[0].IsBlank);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void Group_TextAppears_SplitsIntoBlankAndTextGroups()
    {
        var folder = Path.Combine(Path.GetTempPath(), "vocr_mask_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var frames = new List<string>
            {
                WriteFrame(folder, "img000000.jpg", withText: false),
                WriteFrame(folder, "img000001.jpg", withText: false),
                WriteFrame(folder, "img000002.jpg", withText: true),
                WriteFrame(folder, "img000003.jpg", withText: true),
            };

            var groups = VideoOcrFrameGrouper.Group(frames, 190, 92, null, CancellationToken.None);

            Assert.Equal(2, groups.Count);
            Assert.True(groups[0].IsBlank);
            Assert.False(groups[1].IsBlank);
            Assert.Equal(2, groups[1].StartFrame);
            Assert.Equal(3, groups[1].EndFrame);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
