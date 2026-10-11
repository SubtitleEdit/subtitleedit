using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Logic.Platform.Progress;

internal sealed class MacDockProgress : IPlatformProgress
{
    private const int NSImageScaleProportionallyUpOrDown = 3;
    private const int NSViewWidthSizable = 2;
    private const int NSViewHeightSizable = 16;

    private IntPtr _dockTile;
    private IntPtr _originalContentView;
    private IntPtr _imageView;
    private IntPtr _progressView;
    private int? _percentage;
    private bool _unavailable;

    public void Update(double? percentage, bool indeterminate)
    {
        if (_unavailable) return;
        var value = !indeterminate && percentage is { } progress ? (int?)Math.Clamp(progress, 0, 100) : null;
        if (_percentage == value) return;
        try
        {
            if (value is null)
            {
                RestoreIcon();
                return;
            }

            EnsureDockTile();
            var png = Render(value.Value);
            var allocatedData = Send(Class("NSData"), "alloc");
            var data = SendBytes(allocatedData, Selector("initWithBytes:length:"), png, (nuint)png.Length);
            var image = IntPtr.Zero;
            try
            {
                var allocatedImage = Send(Class("NSImage"), "alloc");
                image = Send(allocatedImage, "initWithData:", data);
                if (image == IntPtr.Zero) throw new InvalidOperationException("Cannot create Dock progress image.");

                Send(_progressView, "setImage:", image);
                Send(_dockTile, "setContentView:", _imageView);
                Send(_dockTile, "display");
                _percentage = value;
            }
            finally
            {
                Send(image, "release");
                Send(data, "release");
            }
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException
            or InvalidOperationException or NotSupportedException)
        {
            Debug.WriteLine($"Dock progress unavailable: {exception.Message}");
            Dispose();
        }
    }

    private void EnsureDockTile()
    {
        if (_imageView != IntPtr.Zero) return;

        var application = Send(Class("NSApplication"), "sharedApplication");
        _dockTile = Send(application, "dockTile");
        if (_dockTile == IntPtr.Zero) throw new InvalidOperationException("No application Dock tile.");

        var size = SendSize(_dockTile, Selector("size"));
        if (size.Width <= 0 || size.Height <= 0)
            throw new InvalidOperationException("Invalid Dock tile size.");

        var contentView = Send(_dockTile, "contentView");
        // Keep the original view alive while the Dock uses our replacement.
        _originalContentView = Send(contentView, "retain");

        var icon = Send(application, "applicationIconImage");
        if (icon == IntPtr.Zero) throw new InvalidOperationException("No application Dock icon.");

        _imageView = CreateImageView(size);
        Send(_imageView, "setImage:", icon);
        _progressView = CreateImageView(size);
        Send(_imageView, "addSubview:", _progressView);
    }

    private static IntPtr CreateImageView(NativeSize size)
    {
        var frame = new NativeRect { Width = size.Width, Height = size.Height };
        var allocatedView = Send(Class("NSImageView"), "alloc");
        var view = SendRect(allocatedView, Selector("initWithFrame:"), frame);
        if (view == IntPtr.Zero) throw new InvalidOperationException("Cannot create Dock content view.");
        Send(view, "setImageScaling:", new IntPtr(NSImageScaleProportionallyUpOrDown));
        Send(view, "setAutoresizingMask:", new IntPtr(NSViewWidthSizable | NSViewHeightSizable));
        return view;
    }

    internal static byte[] Render(int percentage)
    {
        using var bitmap = new SKBitmap(256, 256);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        using var paint = new SKPaint { IsAntialias = true, Color = new SKColor(0, 0, 0, 220) };
        canvas.DrawRoundRect(new SKRect(44, 186, 212, 210), 12, 12, paint);
        paint.Color = SKColors.White;
        var width = 160 * Math.Clamp(percentage, 0, 100) / 100f;
        if (width > 0)
            canvas.DrawRoundRect(new SKRect(48, 190, 48 + width, 206), 8, 8, paint);
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return encoded.ToArray();
    }

    private void RestoreIcon()
    {
        if (_percentage is null) return;
        Send(_dockTile, "setContentView:", _originalContentView);
        Send(_dockTile, "display");
        _percentage = null;
    }

    public void Dispose()
    {
        RestoreIcon();
        Send(_progressView, "release");
        Send(_imageView, "release");
        Send(_originalContentView, "release");
        _progressView = _imageView = _originalContentView = _dockTile = IntPtr.Zero;
        _unavailable = true;
    }

    private static IntPtr Send(IntPtr receiver, string selector) =>
        receiver == IntPtr.Zero ? IntPtr.Zero : SendMessage(receiver, Selector(selector));
    private static IntPtr Send(IntPtr receiver, string selector, IntPtr argument) =>
        SendPointer(receiver, Selector(selector), argument);

    private const string ObjC = "/usr/lib/libobjc.A.dylib";
    [DllImport(ObjC, EntryPoint = "objc_getClass")]
    private static extern IntPtr Class([MarshalAs(UnmanagedType.LPUTF8Str)] string name);
    [DllImport(ObjC, EntryPoint = "sel_registerName")]
    private static extern IntPtr Selector([MarshalAs(UnmanagedType.LPUTF8Str)] string name);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPointer(IntPtr receiver, IntPtr selector, IntPtr argument);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendBytes(IntPtr receiver, IntPtr selector, byte[] bytes, nuint length);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendRect(IntPtr receiver, IntPtr selector, NativeRect frame);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")]
    private static extern NativeSize SendSize(IntPtr receiver, IntPtr selector);
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize { public double Width, Height; }
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public double X, Y, Width, Height; }
}
