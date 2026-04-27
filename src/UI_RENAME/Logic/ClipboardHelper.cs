using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic;

public static class ClipboardHelper
{
    // Win32 API declarations
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint RegisterClipboardFormatW([MarshalAs(UnmanagedType.LPWStr)] string lpszFormat);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr hMem);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO pbmi, uint usage, out IntPtr ppvBits, IntPtr hSection, uint offset);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr ho);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public RGBQUAD[] bmiColors;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RGBQUAD
    {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbReserved;
    }

    private const uint CF_DIB = 8;
    private const uint GMEM_MOVEABLE = 0x0002;
    private const int BI_RGB = 0;

    public static async Task SetTextAsync(Window window, string text)
    {
        var clipboard = TopLevel.GetTopLevel(window)?.Clipboard;
        if (clipboard != null)
        {
            try
            {
                await clipboard.SetTextAsync(text);
            }
            catch
            {
                await Task.Delay(50);

                try
                {
                    await clipboard.SetTextAsync(text);
                }
                catch
                {
                    // Ignore exceptions when clipboard is not available (e.g., remote desktop)
                }
            }
        }
    }

    // GetTextAsync 
    public static async Task<string?> GetTextAsync(Window window)
    {
        var clipboard = TopLevel.GetTopLevel(window)?.Clipboard;
        if (clipboard != null)
        {
            try
            {
                return await ClipboardExtensions.TryGetTextAsync(clipboard);
            }
            catch
            {
                await Task.Delay(50);
                try
                {
                    return await ClipboardExtensions.TryGetTextAsync(clipboard);
                }
                catch
                {
                    // Ignore exceptions when clipboard is not available (e.g., remote desktop)
                }
            }
        }

        return string.Empty;
    }

    public static async Task CopyImageToClipboard(Bitmap bitmap)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await CopyImageToClipboardWindows(bitmap);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await CopyImageToClipboardLinux(bitmap);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            await CopyImageToClipboardMacOS(bitmap);
        }
        else
        {
            throw new PlatformNotSupportedException("Clipboard image copy not supported on this platform");
        }
    }

    private static async Task CopyImageToClipboardWindows(Bitmap bitmap)
    {
        await Task.Run(() =>
        {
            IntPtr hGlobal = IntPtr.Zero;

            try
            {
                var width = bitmap.PixelSize.Width;
                var height = bitmap.PixelSize.Height;

                // Save bitmap to memory stream and read pixel data
                using var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream);
                memoryStream.Position = 0;

                // Use SkiaSharp to decode the image and get pixel data
                using var skBitmap = SkiaSharp.SKBitmap.Decode(memoryStream);
                if (skBitmap == null)
                {
                    throw new InvalidOperationException("Failed to decode bitmap");
                }

                // Ensure we have BGRA format
                using var bgraBitmap = new SkiaSharp.SKBitmap(width, height, SkiaSharp.SKColorType.Bgra8888, SkiaSharp.SKAlphaType.Premul);
                using var canvas = new SkiaSharp.SKCanvas(bgraBitmap);
                canvas.DrawBitmap(skBitmap, 0, 0);
                canvas.Flush();

                var pixels = bgraBitmap.Bytes;

                // Calculate sizes
                var stride = width * 4; // 4 bytes per pixel (BGRA)
                var imageSize = stride * height;
                var headerSize = Marshal.SizeOf<BITMAPINFOHEADER>();
                var totalSize = headerSize + imageSize;

                // Allocate global memory
                hGlobal = GlobalAlloc(GMEM_MOVEABLE, new UIntPtr((uint)totalSize));
                if (hGlobal == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Failed to allocate global memory");
                }

                IntPtr ptr = GlobalLock(hGlobal);
                if (ptr == IntPtr.Zero)
                {
                    GlobalFree(hGlobal);
                    throw new InvalidOperationException("Failed to lock global memory");
                }

                try
                {
                    // Write BITMAPINFOHEADER
                    var header = new BITMAPINFOHEADER
                    {
                        biSize = (uint)headerSize,
                        biWidth = width,
                        biHeight = height, // positive for bottom-up DIB
                        biPlanes = 1,
                        biBitCount = 32,
                        biCompression = BI_RGB,
                        biSizeImage = (uint)imageSize,
                        biXPelsPerMeter = 0,
                        biYPelsPerMeter = 0,
                        biClrUsed = 0,
                        biClrImportant = 0
                    };

                    Marshal.StructureToPtr(header, ptr, false);

                    // Write pixel data (flip vertically for bottom-up DIB)
                    IntPtr pixelPtr = IntPtr.Add(ptr, headerSize);
                    for (int y = 0; y < height; y++)
                    {
                        int sourceOffset = (height - 1 - y) * stride;
                        IntPtr destPtr = IntPtr.Add(pixelPtr, y * stride);
                        Marshal.Copy(pixels, sourceOffset, destPtr, stride);
                    }
                }
                finally
                {
                    GlobalUnlock(hGlobal);
                }

                // Open clipboard and set data
                if (!OpenClipboard(IntPtr.Zero))
                {
                    GlobalFree(hGlobal);
                    throw new InvalidOperationException("Failed to open clipboard");
                }

                try
                {
                    if (!EmptyClipboard())
                    {
                        GlobalFree(hGlobal);
                        throw new InvalidOperationException("Failed to empty clipboard");
                    }

                    if (SetClipboardData(CF_DIB, hGlobal) == IntPtr.Zero)
                    {
                        GlobalFree(hGlobal);
                        throw new InvalidOperationException("Failed to set clipboard data");
                    }

                    // Clipboard now owns the memory
                    hGlobal = IntPtr.Zero;
                }
                finally
                {
                    CloseClipboard();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to copy image to clipboard on Windows: {ex.Message}");
                throw;
            }
            finally
            {
                if (hGlobal != IntPtr.Zero)
                {
                    GlobalFree(hGlobal);
                }
            }
        });
    }

    private static async Task CopyImageToClipboardLinux(Bitmap bitmap)
    {
        try
        {
            // Save to temporary file
            var tempPath = Path.Combine(Path.GetTempPath(), $"clipboard_{Guid.NewGuid()}.png");
            bitmap.Save(tempPath);

            // Try xclip first
            var psi = new ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = $"-selection clipboard -t image/png -i '{tempPath}'",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using var process = Process.Start(psi);
                if (process != null)
                {
                    await process.WaitForExitAsync();

                    // Clean up temp file after a delay
                    await Task.Delay(500);
                    try { File.Delete(tempPath); } catch { }
                    return;
                }
            }
            catch
            {
                // If xclip fails, try wl-copy (Wayland)
                psi.FileName = "wl-copy";
                psi.Arguments = $"--type image/png < '{tempPath}'";

                using var process = Process.Start(psi);
                if (process != null)
                {
                    await process.WaitForExitAsync();

                    await Task.Delay(500);
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to copy image to clipboard on Linux: {ex.Message}");
            throw;
        }
    }

    private static async Task CopyImageToClipboardMacOS(Bitmap bitmap)
    {
        try
        {
            // Save to temporary file
            var tempPath = Path.Combine(Path.GetTempPath(), $"clipboard_{Guid.NewGuid()}.png");
            bitmap.Save(tempPath);

            // Use osascript to copy image to clipboard
            var psi = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'set the clipboard to (read (POSIX file \"{tempPath}\") as «class PNGf»)'",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync();

                // Clean up temp file after a delay
                await Task.Delay(500);
                try { File.Delete(tempPath); } catch { }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to copy image to clipboard on macOS: {ex.Message}");
            throw;
        }
    }
}
