using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
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

    /// <summary>
    /// Inpaints one frame by streaming it through crispembed --inpaint-stream (the same engine
    /// path as the full run, no temp files); returns the RGB result or null.
    /// </summary>
    public static async Task<byte[]?> InpaintAsync(BurnedInRemovalOptions o, byte[] rgb, byte[] mask, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(o.CrispEmbedExecutable,
            CrispEmbedEngine.GetInpaintArguments(o.ModelFileName) + $" --inpaint-stream {o.VideoWidth}x{o.VideoHeight}")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(o.CrispEmbedExecutable) ?? string.Empty,
        };
        CrispEmbedEngine.SetInpaintEnvironment(startInfo);
        using var process = Process.Start(startInfo);
        if (process == null)
        {
            return null;
        }

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
        var result = new byte[rgb.Length];
        try
        {
            await process.StandardInput.BaseStream.WriteAsync(rgb, CancellationToken.None);
            await process.StandardInput.BaseStream.WriteAsync(mask, CancellationToken.None);
            process.StandardInput.Close();
            await process.StandardOutput.BaseStream.ReadExactlyAsync(result, CancellationToken.None);
        }
        catch (Exception exception) when (exception is IOException or EndOfStreamException)
        {
            result = null;
        }

        await process.WaitForExitAsync(CancellationToken.None);
        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        if (result == null || process.ExitCode != 0)
        {
            Se.LogError("Remove burned-in subtitles: crispembed --inpaint-stream failed: " + await stderr);
            return null;
        }

        return result;
    }

    /// <summary>Paints a black rectangle into an RGB24 frame (the black-box method's result).</summary>
    public static void FillBlack(byte[] rgb, int width, int x, int y, int w, int h)
    {
        var height = rgb.Length / 3 / width;
        for (var yy = Math.Max(0, y); yy < Math.Min(height, y + h); yy++)
        {
            var from = (yy * width + Math.Max(0, x)) * 3;
            var to = (yy * width + Math.Min(width, x + w)) * 3;
            if (to > from)
            {
                Array.Clear(rgb, from, to - from);
            }
        }
    }

    /// <summary>Box blurs a rectangle of an RGB24 frame (two passes, like ffmpeg's boxblur=r:2).</summary>
    public static void BlurRect(byte[] rgb, int width, int x, int y, int w, int h, int radius)
    {
        var height = rgb.Length / 3 / width;
        var x0 = Math.Max(0, x);
        var y0 = Math.Max(0, y);
        var x1 = Math.Min(width, x + w);
        var y1 = Math.Min(height, y + h);
        if (x1 <= x0 || y1 <= y0 || radius < 1)
        {
            return;
        }

        var rw = x1 - x0;
        var rh = y1 - y0;
        var line = new int[Math.Max(rw, rh) * 3];
        for (var pass = 0; pass < 2; pass++)
        {
            for (var yy = y0; yy < y1; yy++)
            {
                BoxLine(rgb, (yy * width + x0) * 3, 3, rw, radius, line);
            }

            for (var xx = x0; xx < x1; xx++)
            {
                BoxLine(rgb, (y0 * width + xx) * 3, width * 3, rh, radius, line);
            }
        }
    }

    // One clamped running-mean pass over n pixels starting at offset, step bytes apart.
    private static void BoxLine(byte[] rgb, int offset, int step, int n, int radius, int[] line)
    {
        for (var i = 0; i < n; i++)
        {
            for (var c = 0; c < 3; c++)
            {
                line[i * 3 + c] = rgb[offset + i * step + c];
            }
        }

        for (var c = 0; c < 3; c++)
        {
            var sum = 0;
            for (var i = -radius; i <= radius; i++)
            {
                sum += line[Math.Clamp(i, 0, n - 1) * 3 + c];
            }

            for (var i = 0; i < n; i++)
            {
                rgb[offset + i * step + c] = (byte)(sum / (2 * radius + 1));
                sum += line[Math.Min(n - 1, i + radius + 1) * 3 + c] - line[Math.Max(0, i - radius) * 3 + c];
            }
        }
    }

    /// <summary>
    /// Fills a rectangle of an RGB24 frame from the pixels around it, like ffmpeg's delogo: a
    /// blend of the horizontal and vertical interpolations between the edges, the shorter
    /// (more reliable) span weighted higher.
    /// </summary>
    public static void DelogoRect(byte[] rgb, int width, int x, int y, int w, int h)
    {
        var height = rgb.Length / 3 / width;
        var x0 = Math.Clamp(x, 1, width - 2);
        var y0 = Math.Clamp(y, 1, height - 2);
        var x1 = Math.Clamp(x + w, x0 + 1, width - 1);
        var y1 = Math.Clamp(y + h, y0 + 1, height - 1);
        var rw = x1 - x0 + 1;
        var rh = y1 - y0 + 1;
        var source = (byte[])rgb.Clone();
        for (var yy = y0; yy < y1; yy++)
        {
            var fy = (double)(yy - y0 + 1) / rh;
            for (var xx = x0; xx < x1; xx++)
            {
                var fx = (double)(xx - x0 + 1) / rw;
                for (var c = 0; c < 3; c++)
                {
                    var left = source[(yy * width + x0 - 1) * 3 + c];
                    var right = source[(yy * width + x1) * 3 + c];
                    var top = source[((y0 - 1) * width + xx) * 3 + c];
                    var bottom = source[(y1 * width + xx) * 3 + c];
                    var horizontal = left * (1 - fx) + right * fx;
                    var vertical = top * (1 - fy) + bottom * fy;
                    rgb[(yy * width + xx) * 3 + c] = (byte)Math.Round((horizontal * rh + vertical * rw) / (rw + rh));
                }
            }
        }
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
