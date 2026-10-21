using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

/// <summary>
/// Frame decoding, single-frame inpainting and the preview bitmap for the review step. Frames
/// are decoded with the same RGB conversion as the removal run, so the preview shows exactly
/// what the run will see.
/// </summary>
internal static class BurnedInPreviewRenderer
{
    public static byte[]? DecodeFrame(BurnedInRemovalOptions o, double seconds)
    {
        var matrix = BurnedInSubtitleRemover.ColorMatrix(o.VideoHeight);
        var args = $"-nostdin -v error -ss {seconds.ToString("0.###", CultureInfo.InvariantCulture)} -i \"{o.VideoFileName}\" -map 0:v:0 -frames:v 1 " +
                   $"-vf \"scale=in_color_matrix={matrix}:in_range=tv,format=rgb24\" -f rawvideo -pix_fmt rgb24 -";
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(FfmpegHelper.GetFfmpegLocation(), args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };
        process.Start();
        process.BeginErrorReadLine();
        using var memory = new MemoryStream();
        process.StandardOutput.BaseStream.CopyTo(memory);
        process.WaitForExit();
        var bytes = memory.ToArray();
        return bytes.Length >= o.VideoWidth * o.VideoHeight * 3 ? bytes : null;
    }

    /// <summary>Inpaints one frame with crispembed --inpaint; returns the RGB result or null.</summary>
    public static async Task<byte[]?> InpaintAsync(BurnedInRemovalOptions o, byte[] rgb, byte[] mask, CancellationToken cancellationToken)
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-remove-burned-in-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var framePng = Path.Combine(folder, "frame.png");
            var maskPng = Path.Combine(folder, "mask.png");
            var outPng = Path.Combine(folder, "out.png");
            SavePng(framePng, rgb, o.VideoWidth, o.VideoHeight, 3);
            SavePng(maskPng, mask, o.VideoWidth, o.VideoHeight, 1);

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(o.CrispEmbedExecutable,
                    $"--inpaint-model \"{o.ModelFileName}\" --inpaint \"{framePng}\" --mask \"{maskPng}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(o.CrispEmbedExecutable) ?? string.Empty,
                },
            };
            process.Start();
            using var registration = cancellationToken.Register(() =>
            {
                try
                {
                    process.Kill(true);
                }
                catch
                {
                    // ignore
                }
            });
            var stderr = process.StandardError.ReadToEndAsync(CancellationToken.None);
            await using (var file = File.Create(outPng))
            {
                await process.StandardOutput.BaseStream.CopyToAsync(file, CancellationToken.None);
            }

            await process.WaitForExitAsync(CancellationToken.None);
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            if (process.ExitCode != 0 || new FileInfo(outPng).Length == 0)
            {
                Se.LogError("Remove burned-in subtitles: crispembed --inpaint failed: " + await stderr);
                return null;
            }

            using var result = SKBitmap.Decode(outPng);
            var resultRgb = new byte[rgb.Length];
            for (var y = 0; y < o.VideoHeight; y++)
            {
                for (var x = 0; x < o.VideoWidth; x++)
                {
                    var c = result.GetPixel(x, y);
                    var i = (y * o.VideoWidth + x) * 3;
                    resultRgb[i] = c.Red;
                    resultRgb[i + 1] = c.Green;
                    resultRgb[i + 2] = c.Blue;
                }
            }

            return resultRgb;
        }
        finally
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch
            {
                // ignore
            }
        }
    }

    private static void SavePng(string fileName, byte[] pixels, int width, int height, int channels)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
        var span = bitmap.GetPixelSpan();
        for (int i = 0, o = 0; i < width * height; i++, o += 4)
        {
            var p = i * channels;
            span[o] = pixels[p];
            span[o + 1] = channels == 3 ? pixels[p + 1] : pixels[p];
            span[o + 2] = channels == 3 ? pixels[p + 2] : pixels[p];
            span[o + 3] = 255;
        }

        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(fileName);
        data.SaveTo(stream);
    }

    /// <summary>
    /// The full frame as an Avalonia bitmap: the mask (when given) tinted in the accent red with
    /// a crisp edge, and the scan area outlined.
    /// </summary>
    public static Bitmap Compose(byte[] rgb, byte[]? mask, BurnedInRemovalOptions o, bool drawScanArea)
    {
        int w = o.VideoWidth, h = o.VideoHeight;
        using var bitmap = new SKBitmap(w, h, SKColorType.Rgba8888, SKAlphaType.Opaque);
        var span = bitmap.GetPixelSpan();
        for (int i = 0, d = 0; i < w * h; i++, d += 4)
        {
            int r = rgb[i * 3], g = rgb[i * 3 + 1], b = rgb[i * 3 + 2];
            if (mask != null && mask[i] != 0)
            {
                var x = i % w;
                var y = i / w;
                var edge = (x > 0 && mask[i - 1] == 0) || (x + 1 < w && mask[i + 1] == 0) ||
                           (y > 0 && mask[i - w] == 0) || (y + 1 < h && mask[i + w] == 0);
                if (edge)
                {
                    r = 255;
                    g = 70;
                    b = 70;
                }
                else
                {
                    r = (r * 45 + 255 * 55) / 100;
                    g = g * 45 / 100;
                    b = (b * 45 + 40 * 55) / 100;
                }
            }

            span[d] = (byte)r;
            span[d + 1] = (byte)g;
            span[d + 2] = (byte)b;
            span[d + 3] = 255;
        }

        using var canvas = new SKCanvas(bitmap);
        if (drawScanArea)
        {
            using var paint = new SKPaint
            {
                Color = new SKColor(255, 200, 60, 230),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = Math.Max(1.5f, h / 360f),
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new[] { h / 70f, h / 140f }, 0),
            };
            canvas.DrawRect(o.RegionX + 0.5f, o.RegionY + 0.5f, o.RegionWidth - 1, o.RegionHeight - 1, paint);
        }

        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 90);
        using var stream = new MemoryStream(data.ToArray());
        return new Bitmap(stream);
    }
}
