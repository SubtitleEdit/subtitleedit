using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// OCR through Apple's Vision framework (VNRecognizeTextRequest) - the same recognizer Preview
/// and Live Text use.
///
/// It is the only local OCR engine on macOS that needs no download at all: no runtime, no model,
/// no Homebrew, no Python. That matters most on Intel Macs, where CrispEmbed has no build and
/// the remaining local options are Tesseract (brew) and PaddleOCR (pip) - see
/// <see cref="CrispEmbedEngine.CanBeDownloaded"/>.
///
/// Driven through objc_msgSend rather than a binding library, the same way
/// <see cref="Main.Layout.MacWindowsMenuInterop"/> drives AppKit. Vision is not linked into the
/// process, so the framework is dlopen'd on first use; every entry point below is defensive and
/// reports "unavailable" rather than throwing, so a macOS that ever lacks the framework simply
/// does not offer the engine.
/// </summary>
public static class AppleVisionOcr
{
    public const string StaticName = "Apple Vision";

    private const string LibObjC = "/usr/lib/libobjc.A.dylib";
    private const string LibSystem = "/usr/lib/libSystem.dylib";
    private const string VisionFramework = "/System/Library/Frameworks/Vision.framework/Vision";

    // VNRequestTextRecognitionLevel
    private const long RecognitionLevelAccurate = 0;
    private const long RecognitionLevelFast = 1;

    [DllImport(LibSystem)]
    private static extern IntPtr dlopen(string path, int mode);

    [DllImport(LibObjC)]
    private static extern IntPtr objc_getClass(string name);

    [DllImport(LibObjC)]
    private static extern IntPtr sel_registerName(string name);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPtr(IntPtr receiver, IntPtr sel);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPtrPtr(IntPtr receiver, IntPtr sel, IntPtr a);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPtrPtrPtr(IntPtr receiver, IntPtr sel, IntPtr a, IntPtr b);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPtrBytesLong(IntPtr receiver, IntPtr sel, byte[] bytes, long length);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPtrUtf8(IntPtr receiver, IntPtr sel, string a);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPtrLong(IntPtr receiver, IntPtr sel, long a);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern long SendLong(IntPtr receiver, IntPtr sel);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern void SendVoid(IntPtr receiver, IntPtr sel);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern void SendVoidLong(IntPtr receiver, IntPtr sel, long a);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern void SendVoidBool(IntPtr receiver, IntPtr sel, byte a);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern void SendVoidPtr(IntPtr receiver, IntPtr sel, IntPtr a);

    /// <summary>
    /// CGPoint. Read the corners rather than <c>boundingBox</c> on purpose: a CGRect is four
    /// doubles, and on x86_64 a struct that size comes back through the hidden-pointer
    /// objc_msgSend_stret path while arm64 returns it in registers - two different calls to get
    /// right, one of which cannot be exercised on an Apple Silicon dev machine. A CGPoint is two
    /// doubles, which both architectures return in registers, so the corners need one code path.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;
        public double Y;
    }

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern CGPoint SendPoint(IntPtr receiver, IntPtr sel);

    private static readonly object AvailabilityLock = new();
    private static bool _availabilityChecked;
    private static bool _isAvailable;
    private static List<OcrLanguage2>? _languages;

    /// <summary>
    /// True when this process can actually run a Vision text request. Checked once and cached:
    /// the answer cannot change while SE runs, and <see cref="OcrEngineItem.GetOcrEngines"/>
    /// asks on every OCR window.
    /// </summary>
    public static bool IsAvailable()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return false;
        }

        lock (AvailabilityLock)
        {
            if (_availabilityChecked)
            {
                return _isAvailable;
            }

            _availabilityChecked = true;
            try
            {
                // RTLD_LAZY. Vision is not linked into the app, so without this the runtime
                // classes below are simply not registered and objc_getClass returns nil.
                _isAvailable = dlopen(VisionFramework, 1) != IntPtr.Zero &&
                               objc_getClass("VNRecognizeTextRequest") != IntPtr.Zero &&
                               objc_getClass("VNImageRequestHandler") != IntPtr.Zero;
            }
            catch
            {
                _isAvailable = false;
            }

            return _isAvailable;
        }
    }

    /// <summary>
    /// The languages this machine's Vision can recognize, straight from the framework rather
    /// than a hard-coded list: the set grows with macOS releases, and the recognizer rejects a
    /// language it does not know.
    /// </summary>
    public static List<OcrLanguage2> GetLanguages()
    {
        lock (AvailabilityLock)
        {
            if (_languages != null)
            {
                return _languages;
            }

            _languages = ReadSupportedLanguages();
            return _languages;
        }
    }

    private static List<OcrLanguage2> ReadSupportedLanguages()
    {
        var list = new List<OcrLanguage2>();
        if (!IsAvailable())
        {
            return list;
        }

        var pool = NewAutoreleasePool();
        var request = IntPtr.Zero;
        try
        {
            request = NewRequest();
            if (request == IntPtr.Zero)
            {
                return list;
            }

            var supported = SendPtrPtr(request, Sel("supportedRecognitionLanguagesAndReturnError:"), IntPtr.Zero);
            if (supported == IntPtr.Zero)
            {
                return list;
            }

            var count = SendLong(supported, Sel("count"));
            for (long i = 0; i < count; i++)
            {
                var code = NsToString(SendPtrLong(supported, Sel("objectAtIndex:"), i));
                if (!string.IsNullOrEmpty(code))
                {
                    list.Add(new OcrLanguage2(code!, DisplayName(code!)));
                }
            }
        }
        catch
        {
            // A machine that cannot list languages cannot recognize text either; the empty
            // list leaves the engine selectable but with nothing to choose, which the caller
            // reports better than an exception out of a property getter would.
        }
        finally
        {
            Release(request);
            DrainAutoreleasePool(pool);
        }

        return list;
    }

    /// <summary>
    /// Turns Vision's BCP-47 tag ("pt-BR", "zh-Hans") into something readable. .NET's own
    /// culture data already knows these, and falls back to the tag itself for the ones it does
    /// not - which is still better than showing nothing.
    /// </summary>
    private static string DisplayName(string code)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(code);
            if (!string.IsNullOrWhiteSpace(culture.EnglishName) && !culture.EnglishName.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase))
            {
                return $"{culture.EnglishName} ({code})";
            }
        }
        catch (CultureNotFoundException)
        {
            // fall through to the raw tag
        }

        return code;
    }

    /// <summary>
    /// Recognizes the text in one subtitle image.
    /// </summary>
    /// <param name="bitmap">The subtitle image. A transparent background is fine - Vision reads
    /// these as well as it reads the same text on black, so SE's images go in untouched.</param>
    /// <param name="languageCode">A tag from <see cref="GetLanguages"/>. Empty means "let Vision
    /// pick", which is its own default.</param>
    /// <param name="fast">Vision's fast recognition level instead of the accurate one. Roughly an
    /// order of magnitude quicker and noticeably worse on the small, soft text subtitles are made
    /// of, so the accurate level is the default.</param>
    /// <param name="cancellationToken">Checked before the request starts; a Vision request on one
    /// subtitle image is short enough that it is not worth interrupting once running.</param>
    /// <returns>The recognized text, or an empty string when Vision found none.</returns>
    public static string Ocr(SKBitmap? bitmap, string? languageCode, bool fast, CancellationToken cancellationToken)
    {
        // Cancellation first: a caller that has already cancelled wants to hear about it whatever
        // the state of the engine, and checking it after the availability gate made the method
        // behave differently by platform - off macOS it returned empty for a cancelled token
        // instead of throwing.
        cancellationToken.ThrowIfCancellationRequested();

        if (bitmap == null || bitmap.Width < 1 || bitmap.Height < 1 || !IsAvailable())
        {
            return string.Empty;
        }

        var png = EncodePng(bitmap);
        if (png == null || png.Length == 0)
        {
            return string.Empty;
        }

        // Every NSString / NSArray / NSData below is autoreleased. Nothing drains the thread's
        // pool for us on a .NET worker thread, so without an explicit pool a batch of a few
        // thousand subtitle images would hold every intermediate object until the process ends.
        var pool = NewAutoreleasePool();
        var request = IntPtr.Zero;
        var handler = IntPtr.Zero;
        try
        {
            request = NewRequest();
            if (request == IntPtr.Zero)
            {
                return string.Empty;
            }

            SendVoidLong(request, Sel("setRecognitionLevel:"), fast ? RecognitionLevelFast : RecognitionLevelAccurate);
            SendVoidBool(request, Sel("setUsesLanguageCorrection:"), 1);

            if (!string.IsNullOrEmpty(languageCode))
            {
                var languages = SendPtrPtr(Cls("NSArray"), Sel("arrayWithObject:"), NsString(languageCode!));
                if (languages != IntPtr.Zero)
                {
                    SendVoidPtr(request, Sel("setRecognitionLanguages:"), languages);
                }
            }

            var data = SendPtrBytesLong(Cls("NSData"), Sel("dataWithBytes:length:"), png, png.Length);
            if (data == IntPtr.Zero)
            {
                return string.Empty;
            }

            handler = SendPtrPtrPtr(SendPtr(Cls("VNImageRequestHandler"), Sel("alloc")),
                Sel("initWithData:options:"), data, IntPtr.Zero);
            if (handler == IntPtr.Zero)
            {
                return string.Empty;
            }

            var requests = SendPtrPtr(Cls("NSArray"), Sel("arrayWithObject:"), request);
            SendPtrPtrPtr(handler, Sel("performRequests:error:"), requests, IntPtr.Zero);

            return AppleVisionTextLayout.Compose(ReadObservations(request));
        }
        catch
        {
            // One unreadable image must not stop a run of thousands: an empty result reads as
            // "nothing recognized here", which is what the caller already handles.
            return string.Empty;
        }
        finally
        {
            Release(handler);
            Release(request);
            DrainAutoreleasePool(pool);
        }
    }

    private static IEnumerable<AppleVisionObservation> ReadObservations(IntPtr request)
    {
        var results = SendPtr(request, Sel("results"));
        if (results == IntPtr.Zero)
        {
            yield break;
        }

        var count = SendLong(results, Sel("count"));
        for (long i = 0; i < count; i++)
        {
            var observation = SendPtrLong(results, Sel("objectAtIndex:"), i);
            if (observation == IntPtr.Zero)
            {
                continue;
            }

            // topCandidates: returns the recognizer's ranked readings; one is enough, and asking
            // for more makes Vision do extra work per observation.
            var candidates = SendPtrLong(observation, Sel("topCandidates:"), 1);
            if (candidates == IntPtr.Zero || SendLong(candidates, Sel("count")) < 1)
            {
                continue;
            }

            var top = SendPtrLong(candidates, Sel("objectAtIndex:"), 0);
            var text = NsToString(SendPtr(top, Sel("string")));
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            var topLeft = SendPoint(observation, Sel("topLeft"));
            var bottomRight = SendPoint(observation, Sel("bottomRight"));

            yield return new AppleVisionObservation(text!, topLeft.X, bottomRight.X, topLeft.Y, bottomRight.Y);
        }
    }

    private static byte[]? EncodePng(SKBitmap bitmap)
    {
        try
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image?.Encode(SKEncodedImageFormat.Png, 100);
            return data?.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private static IntPtr NewRequest()
    {
        var requestClass = Cls("VNRecognizeTextRequest");
        return requestClass == IntPtr.Zero
            ? IntPtr.Zero
            : SendPtr(SendPtr(requestClass, Sel("alloc")), Sel("init"));
    }

    private static IntPtr NewAutoreleasePool()
    {
        var poolClass = Cls("NSAutoreleasePool");
        return poolClass == IntPtr.Zero
            ? IntPtr.Zero
            : SendPtr(SendPtr(poolClass, Sel("alloc")), Sel("init"));
    }

    private static void DrainAutoreleasePool(IntPtr pool)
    {
        if (pool != IntPtr.Zero)
        {
            SendVoid(pool, Sel("drain"));
        }
    }

    private static void Release(IntPtr obj)
    {
        if (obj != IntPtr.Zero)
        {
            SendVoid(obj, Sel("release"));
        }
    }

    private static IntPtr Cls(string name) => objc_getClass(name);

    private static IntPtr Sel(string name) => sel_registerName(name);

    private static IntPtr NsString(string value) =>
        SendPtrUtf8(Cls("NSString"), Sel("stringWithUTF8String:"), value);

    private static string? NsToString(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero)
        {
            return null;
        }

        var utf8 = SendPtr(nsString, Sel("UTF8String"));
        return utf8 == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(utf8);
    }
}
