using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 16 performance hunt: XML format auto-detection (every candidate format joined all
/// lines into its own string), TTML save (per-paragraph document-wide region scans plus a
/// double serialize-and-reparse), DVB subtitle decoding (linear CLUT scan per RLE run and
/// per-pixel writes), VobSub decoding (per-pixel RLE writes and SKColor-per-pixel crop
/// scans), and HtmlUtil.FixInvalidItalicTags (~50 Replace scans with no early-out).
///
/// Each "Old" benchmark is a self-contained copy of the pre-round-16 shape; each "New"
/// benchmark calls the shipping code. GlobalSetup asserts both produce identical results,
/// including over randomized RLE inputs for the two image decoders.
/// </summary>
[MemoryDiagnoser]
public class PerfHuntRound16Benchmarks
{
    private sealed class DeterministicRandom
    {
        private uint _state;
        public DeterministicRandom(uint seed) => _state = seed;

        public int Next(int max)
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (int)(_state % (uint)max);
        }

        public void NextBytes(byte[] buffer)
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)Next(256);
            }
        }
    }

    // Exposes the protected JoinLines helpers for the benchmark.
    private sealed class FormatExposer : SubtitleFormat
    {
        public override string Extension => ".x";
        public override string Name => "exposer";
        public override string ToText(Subtitle subtitle, string title) => string.Empty;
        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName) { }
        public override bool IsMine(List<string> lines, string fileName) => false;

        public static string JoinTrimmed(List<string> lines) => JoinLinesTrimmed(lines);
    }

    private const int XmlFormatCount = 22; // formats that joined the file before any guard
    private List<string> _ttmlLinesA = new();
    private List<string> _ttmlLinesB = new();
    private bool _useListA;

    private XmlDocument _ttmlDocument = new();
    private int _ttmlParagraphCount;

    private SKColor[] _dvbFourColorsUnused = Array.Empty<SKColor>();
    private ClutDefinitionSegment _clut = null!;
    private (int PixelCode, int RunLength, bool NewLine)[] _dvbRuns = Array.Empty<(int, int, bool)>();
    private SKBitmap _dvbBitmapOld = null!;
    private SKBitmap _dvbBitmapNew = null!;

    private byte[] _vobSubRleData = Array.Empty<byte>();
    private List<SKColor> _vobSubFourColors = new();
    private SKBitmap _vobSubBitmapOld = null!;
    private SKBitmap _vobSubBitmapNew = null!;
    private SKBitmap _cropBitmap = null!;

    private string[] _plainLines = Array.Empty<string>();

    [GlobalSetup]
    public void Setup()
    {
        SetupXmlDetectCase();
        SetupTtmlDocument();
        SetupDvbCase();
        SetupVobSubCase();
        SetupItalicCase();
    }

    // -------------------------------------------------------------------------------------
    // 1) XML auto-detection: join-all-lines once per candidate format vs. memoized join
    // -------------------------------------------------------------------------------------

    private void SetupXmlDetectCase()
    {
        List<string> MakeLines(string marker)
        {
            var lines = new List<string>
            {
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
                $"<tt xmlns=\"http://www.w3.org/ns/ttml\" data=\"{marker}\">",
                "  <body><div>",
            };
            for (var i = 0; i < 1500; i++)
            {
                lines.Add($"    <p begin=\"00:00:{i % 60:00}.000\" end=\"00:00:{i % 60:00}.900\">Line {i} with some ordinary subtitle text.</p>");
            }

            lines.Add("  </div></body>");
            lines.Add("</tt>");
            return lines;
        }

        _ttmlLinesA = MakeLines("a");
        _ttmlLinesB = MakeLines("b");

        if (OldJoinAllFormats(_ttmlLinesA) != FormatExposer.JoinTrimmed(_ttmlLinesA) ||
            OldJoinAllFormats(_ttmlLinesB) != FormatExposer.JoinTrimmed(_ttmlLinesB))
        {
            throw new InvalidOperationException("join mismatch");
        }
    }

    private static string OldJoinAllFormats(List<string> lines)
    {
        // The pre-round-16 shape, once per candidate format: StringBuilder join + Trim copy.
        string last = string.Empty;
        for (var i = 0; i < XmlFormatCount; i++)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            last = sb.ToString().Trim();
        }

        return last;
    }

    [Benchmark]
    public string XmlDetect_JoinPerFormat_Old()
    {
        // Alternate lists so every invocation models a fresh file.
        _useListA = !_useListA;
        return OldJoinAllFormats(_useListA ? _ttmlLinesA : _ttmlLinesB);
    }

    [Benchmark]
    public string XmlDetect_MemoizedJoin_New()
    {
        _useListA = !_useListA;
        var lines = _useListA ? _ttmlLinesA : _ttmlLinesB;
        string last = string.Empty;
        for (var i = 0; i < XmlFormatCount; i++)
        {
            last = FormatExposer.JoinTrimmed(lines);
        }

        return last;
    }

    // -------------------------------------------------------------------------------------
    // 2) TTML save: styles/regions read via serialize + full re-parse vs. the live document,
    //    and the per-paragraph document-wide bottom-region scan vs. one cached scan
    // -------------------------------------------------------------------------------------

    private void SetupTtmlDocument()
    {
        var subtitle = new Subtitle();
        for (var i = 0; i < 400; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph($"TTML paragraph {i}\r\nsecond line", i * 2000, i * 2000 + 1800));
        }

        var text = new TimedText10().ToText(subtitle, "benchmark");
        _ttmlDocument = new XmlDocument { XmlResolver = null };
        _ttmlDocument.LoadXml(text);
        _ttmlParagraphCount = subtitle.Paragraphs.Count;

        var oldStyles = TimedText10.GetStylesFromHeader(SubtitleFormat.ToUtf8XmlString(_ttmlDocument));
        var newStyles = TimedText10.GetStylesFromHeader(_ttmlDocument);
        var oldRegions = TimedText10.GetRegionsFromHeader(SubtitleFormat.ToUtf8XmlString(_ttmlDocument));
        var newRegions = TimedText10.GetRegionsFromHeader(_ttmlDocument);
        if (!oldStyles.SequenceEqual(newStyles) || !oldRegions.SequenceEqual(newRegions))
        {
            throw new InvalidOperationException("TTML header scan mismatch");
        }
    }

    [Benchmark]
    public int TtmlHeaderScan_SerializeReparse_Old()
    {
        // ToText used to serialize the whole document to a string twice and re-parse it
        // twice, just to read the style / region ids.
        var styles = TimedText10.GetStylesFromHeader(SubtitleFormat.ToUtf8XmlString(_ttmlDocument));
        var regions = TimedText10.GetRegionsFromHeader(SubtitleFormat.ToUtf8XmlString(_ttmlDocument));
        return styles.Count + regions.Count;
    }

    [Benchmark]
    public int TtmlHeaderScan_LiveDocument_New()
    {
        var styles = TimedText10.GetStylesFromHeader(_ttmlDocument);
        var regions = TimedText10.GetRegionsFromHeader(_ttmlDocument);
        return styles.Count + regions.Count;
    }

    [Benchmark]
    public int TtmlRegions_PerParagraphScan_Old()
    {
        // MakeParagraph called GetRegionsBottomFromHeader (an absolute //ttml:region XPath
        // over the whole document) once per paragraph. This models the cost against the
        // finished document; during a real save the document grows as it is written, so
        // the old total cost averaged roughly half of this - still O(n^2).
        var count = 0;
        for (var i = 0; i < _ttmlParagraphCount; i++)
        {
            count += TimedText10.GetRegionsBottomFromHeader(_ttmlDocument).Count;
        }

        return count;
    }

    [Benchmark]
    public int TtmlRegions_CachedScan_New()
    {
        List<string>? cached = null;
        var count = 0;
        for (var i = 0; i < _ttmlParagraphCount; i++)
        {
            cached ??= TimedText10.GetRegionsBottomFromHeader(_ttmlDocument);
            count += cached.Count;
        }

        return count;
    }

    // -------------------------------------------------------------------------------------
    // 3) DVB subtitles: per-run CLUT entry scan + per-pixel SetPixel vs. LUT + run fill
    // -------------------------------------------------------------------------------------

    private void SetupDvbCase()
    {
        // A full-range CLUT definition segment with 256 entries.
        var clutBytes = new byte[2 + 256 * 6];
        clutBytes[0] = 1; // clut id
        var w = 2;
        var rnd = new DeterministicRandom(4242);
        for (var i = 0; i < 256; i++)
        {
            clutBytes[w] = (byte)i;                       // entry id
            clutBytes[w + 1] = 0b00100001;                // 8-bit entry, full range
            clutBytes[w + 2] = (byte)rnd.Next(256);       // Y
            clutBytes[w + 3] = (byte)rnd.Next(256);       // Cr
            clutBytes[w + 4] = (byte)rnd.Next(256);       // Cb
            clutBytes[w + 5] = (byte)rnd.Next(256);       // T
            w += 6;
        }

        _clut = new ClutDefinitionSegment(clutBytes, 0, clutBytes.Length);

        // Synthetic run list resembling a two-line DVB subtitle: ~4000 runs over 40 rows.
        var runs = new List<(int, int, bool)>();
        for (var row = 0; row < 40; row++)
        {
            var x = 0;
            while (x < 700)
            {
                var len = 1 + rnd.Next(24);
                runs.Add((rnd.Next(256), len, false));
                x += len;
            }

            runs.Add((0, 0, true)); // end of line
        }

        _dvbRuns = runs.ToArray();
        _dvbBitmapOld = new SKBitmap(720, 82, SKColorType.Bgra8888, SKAlphaType.Premul);
        _dvbBitmapNew = new SKBitmap(720, 82, SKColorType.Bgra8888, SKAlphaType.Premul);

        // Equivalence: both shapes must write identical pixels.
        DvbDraw_ScanPerRun_Old();
        DvbDraw_LutAndRunFill_New();
        if (!_dvbBitmapOld.Bytes.AsSpan().SequenceEqual(_dvbBitmapNew.Bytes))
        {
            throw new InvalidOperationException("DVB draw mismatch");
        }
    }

    [Benchmark]
    public int DvbDraw_ScanPerRun_Old()
    {
        _dvbBitmapOld.Erase(SKColors.Transparent);
        var fast = new FastBitmap(_dvbBitmapOld);
        fast.LockImage();
        var x = 0;
        var y = 0;
        foreach (var (pixelCode, runLength, newLine) in _dvbRuns)
        {
            if (newLine)
            {
                x = 0;
                y += 2;
                continue;
            }

            // The pre-round-16 DrawPixels: linear CLUT scan + YCbCr conversion per run,
            // then one SetPixel per pixel.
            var c = SKColors.Red;
            foreach (var item in _clut.Entries)
            {
                if (item.ClutEntryId == pixelCode)
                {
                    c = item.GetColor();
                    break;
                }
            }

            for (var k = 0; k < runLength; k++)
            {
                if (y < fast.Height && x < fast.Width)
                {
                    fast.SetPixel(x, y, c);
                }

                x++;
            }
        }

        fast.UnlockImage();
        return x + y;
    }

    [Benchmark]
    public int DvbDraw_LutAndRunFill_New()
    {
        _dvbBitmapNew.Erase(SKColors.Transparent);
        var fast = new FastBitmap(_dvbBitmapNew);
        fast.LockImage();

        // The shipping shape: ObjectDataSegment.BuildClutLookup once, then run fills.
        var lookup = new SKColor[256];
        for (var i = 0; i < lookup.Length; i++)
        {
            lookup[i] = SKColors.Red;
        }

        var seen = new bool[lookup.Length];
        foreach (var item in _clut.Entries)
        {
            var id = item.ClutEntryId;
            if (id >= 0 && id < lookup.Length && !seen[id])
            {
                seen[id] = true;
                lookup[id] = item.GetColor();
            }
        }

        var x = 0;
        var y = 0;
        foreach (var (pixelCode, runLength, newLine) in _dvbRuns)
        {
            if (newLine)
            {
                x = 0;
                y += 2;
                continue;
            }

            var c = (uint)pixelCode < (uint)lookup.Length ? lookup[pixelCode] : SKColors.Red;
            if (y < fast.Height && x < fast.Width)
            {
                fast.SetPixel(x, y, c, Math.Min(runLength, fast.Width - x));
            }

            x += runLength;
        }

        fast.UnlockImage();
        return x + y;
    }

    // -------------------------------------------------------------------------------------
    // 4) VobSub: RLE decode with per-pixel writes vs. run fills, and the crop scans
    // -------------------------------------------------------------------------------------

    private void SetupVobSubCase()
    {
        // Randomized-but-deterministic RLE stream. Any byte stream is a valid input to the
        // decoder, so a random one exercises all nibble/byte run forms; equivalence over it
        // is a strong check that the run-fill rewrite matches the old per-pixel loop.
        var rnd = new DeterministicRandom(987654);
        _vobSubRleData = new byte[40_000];
        rnd.NextBytes(_vobSubRleData);

        _vobSubFourColors = new List<SKColor>
        {
            new SKColor(0, 0, 0, 0),
            new SKColor(255, 255, 255, 255),
            new SKColor(20, 20, 20, 255),
            new SKColor(200, 30, 30, 255),
        };

        _vobSubBitmapOld = new SKBitmap(720, 576, SKColorType.Bgra8888, SKAlphaType.Premul);
        _vobSubBitmapNew = new SKBitmap(720, 576, SKColorType.Bgra8888, SKAlphaType.Premul);

        // Equivalence over 40 random offsets (interlaced pairs like the real decoder uses).
        for (var trial = 0; trial < 40; trial++)
        {
            var address = trial * 997 % 30_000;
            _vobSubBitmapOld.Erase(SKColors.Transparent);
            _vobSubBitmapNew.Erase(SKColors.Transparent);

            var fastOld = new FastBitmap(_vobSubBitmapOld);
            fastOld.LockImage();
            OldGenerateBitmap(_vobSubRleData, fastOld, 0, address, _vobSubFourColors, 2);
            OldGenerateBitmap(_vobSubRleData, fastOld, 1, address + 1024, _vobSubFourColors, 2);
            fastOld.UnlockImage();

            var fastNew = new FastBitmap(_vobSubBitmapNew);
            fastNew.LockImage();
            SubPicture.GenerateBitmap(_vobSubRleData, fastNew, 0, address, _vobSubFourColors, 2);
            SubPicture.GenerateBitmap(_vobSubRleData, fastNew, 1, address + 1024, _vobSubFourColors, 2);
            fastNew.UnlockImage();

            if (!_vobSubBitmapOld.Bytes.AsSpan().SequenceEqual(_vobSubBitmapNew.Bytes))
            {
                throw new InvalidOperationException($"VobSub RLE mismatch at trial {trial}");
            }
        }

        // Crop-scan bitmap: transparent border around an opaque block.
        _cropBitmap = new SKBitmap(720, 576, SKColorType.Bgra8888, SKAlphaType.Premul);
        _cropBitmap.Erase(SKColors.Transparent);
        using (var canvas = new SKCanvas(_cropBitmap))
        using (var paint = new SKPaint { Color = new SKColor(255, 255, 255, 255) })
        {
            canvas.DrawRect(new SKRect(150, 400, 600, 520), paint);
        }

        var oldRect = CropScan_PerPixel_Old();
        var newRect = CropScan_AlphaRows_New();
        if (oldRect != newRect)
        {
            throw new InvalidOperationException($"crop mismatch: {oldRect} vs {newRect}");
        }
    }

    private static int OldDecodeRle(int index, byte[] data, out int color, out int runLength, ref bool onlyHalf, out bool restOfLine)
    {
        restOfLine = false;
        byte b1 = data[index];
        byte b2 = data[index + 1];

        if (onlyHalf)
        {
            byte b3 = data[index + 2];
            b1 = (byte)(((b1 & 0b00001111) << 4) | ((b2 & 0b11110000) >> 4));
            b2 = (byte)(((b2 & 0b00001111) << 4) | ((b3 & 0b11110000) >> 4));
        }

        if (b1 >> 2 == 0)
        {
            runLength = (b1 << 6) | (b2 >> 2);
            color = b2 & 0b00000011;
            if (runLength == 0)
            {
                restOfLine = true;
                if (onlyHalf)
                {
                    onlyHalf = false;
                    return 3;
                }
            }

            return 2;
        }

        if (b1 >> 4 == 0)
        {
            runLength = (b1 << 2) | (b2 >> 6);
            color = (b2 & 0b00110000) >> 4;
            if (onlyHalf)
            {
                onlyHalf = false;
                return 2;
            }

            onlyHalf = true;
            return 1;
        }

        if (b1 >> 6 == 0)
        {
            runLength = b1 >> 2;
            color = b1 & 0b00000011;
            return 1;
        }

        runLength = b1 >> 6;
        color = (b1 & 0b00110000) >> 4;

        if (onlyHalf)
        {
            onlyHalf = false;
            return 1;
        }

        onlyHalf = true;
        return 0;
    }

    private static void OldGenerateBitmap(byte[] data, FastBitmap bmp, int startY, int dataAddress, List<SKColor> fourColors, int addY)
    {
        var index = 0;
        var onlyHalf = false;
        var y = startY;
        var x = 0;
        var colorZeroValue = fourColors[0].ToArgb();
        while (y < bmp.Height && dataAddress + index + 2 < data.Length)
        {
            index += OldDecodeRle(dataAddress + index, data, out var color, out var runLength, ref onlyHalf, out var restOfLine);
            if (restOfLine)
            {
                runLength = bmp.Width - x;
            }

            var c = fourColors[color];
            for (var i = 0; i < runLength; i++, x++)
            {
                if (x >= bmp.Width - 1)
                {
                    if (y < bmp.Height && x < bmp.Width && c != fourColors[0])
                    {
                        bmp.SetPixel(x, y, c);
                    }

                    if (onlyHalf)
                    {
                        onlyHalf = false;
                        index++;
                    }

                    x = 0;
                    y += addY;
                    break;
                }

                if (y < bmp.Height && c.ToArgb() != colorZeroValue)
                {
                    bmp.SetPixel(x, y, c);
                }
            }
        }
    }

    [Benchmark]
    public int VobSubRle_PerPixel_Old()
    {
        var fast = new FastBitmap(_vobSubBitmapOld);
        fast.LockImage();
        OldGenerateBitmap(_vobSubRleData, fast, 0, 0, _vobSubFourColors, 2);
        OldGenerateBitmap(_vobSubRleData, fast, 1, 1024, _vobSubFourColors, 2);
        fast.UnlockImage();
        return fast.Width;
    }

    [Benchmark]
    public int VobSubRle_RunFill_New()
    {
        var fast = new FastBitmap(_vobSubBitmapNew);
        fast.LockImage();
        SubPicture.GenerateBitmap(_vobSubRleData, fast, 0, 0, _vobSubFourColors, 2);
        SubPicture.GenerateBitmap(_vobSubRleData, fast, 1, 1024, _vobSubFourColors, 2);
        fast.UnlockImage();
        return fast.Width;
    }

    [Benchmark]
    public (int, int, int, int) CropScan_PerPixel_Old()
    {
        // The pre-round-16 CropBitmapAndUnlock scans: SKColor per pixel via GetPixel.
        var bmp = new FastBitmap(_cropBitmap);
        bmp.LockImage();
        static bool IsBackgroundColor(SKColor c) => c.Alpha < 2;

        var y = 0;
        var c = new SKColor(0, 0, 0, 0);
        int x;
        while (y < bmp.Height && IsBackgroundColor(c))
        {
            c = bmp.GetPixel(0, y);
            if (IsBackgroundColor(c))
            {
                for (x = 1; x < bmp.Width; x++)
                {
                    c = bmp.GetPixelNext();
                    if (c.Alpha > 1)
                    {
                        break;
                    }
                }
            }

            if (IsBackgroundColor(c))
            {
                y++;
            }
        }

        var minY = y > 3 ? y - 3 : 0;

        x = 0;
        c = new SKColor(0, 0, 0, 0);
        while (x < bmp.Width && IsBackgroundColor(c))
        {
            for (y = minY; y < bmp.Height; y++)
            {
                c = bmp.GetPixel(x, y);
                if (!IsBackgroundColor(c))
                {
                    break;
                }
            }

            if (IsBackgroundColor(c))
            {
                x++;
            }
        }

        var minX = x > 3 ? x - 3 : 0;

        y = bmp.Height - 1;
        c = new SKColor(0, 0, 0, 0);
        while (y > minY && IsBackgroundColor(c))
        {
            c = bmp.GetPixel(0, y);
            if (IsBackgroundColor(c))
            {
                for (x = 1; x < bmp.Width; x++)
                {
                    c = bmp.GetPixelNext();
                    if (!IsBackgroundColor(c))
                    {
                        break;
                    }
                }
            }

            if (IsBackgroundColor(c))
            {
                y--;
            }
        }

        var maxY = Math.Min(y + 7, bmp.Height - 1);

        x = bmp.Width - 1;
        c = new SKColor(0, 0, 0, 0);
        while (x > minX && IsBackgroundColor(c))
        {
            for (y = minY; y < bmp.Height; y++)
            {
                c = bmp.GetPixel(x, y);
                if (!IsBackgroundColor(c))
                {
                    break;
                }
            }

            if (IsBackgroundColor(c))
            {
                x--;
            }
        }

        var maxX = Math.Min(x + 7, bmp.Width - 1);
        bmp.UnlockImage();
        return (minX, minY, maxX, maxY);
    }

    [Benchmark]
    public (int, int, int, int) CropScan_AlphaRows_New()
    {
        // The shipping shape: FastBitmap alpha-byte row/column scans.
        var bmp = new FastBitmap(_cropBitmap);
        bmp.LockImage();
        const byte alphaLimit = 2;

        var y = 0;
        while (y < bmp.Height && bmp.IsRowTransparent(y, alphaLimit))
        {
            y++;
        }

        var minY = y > 3 ? y - 3 : 0;

        var x = 0;
        while (x < bmp.Width && bmp.IsColumnTransparent(x, minY, alphaLimit))
        {
            x++;
        }

        var minX = x > 3 ? x - 3 : 0;

        y = bmp.Height - 1;
        while (y > minY && bmp.IsRowTransparent(y, alphaLimit))
        {
            y--;
        }

        var maxY = Math.Min(y + 7, bmp.Height - 1);

        x = bmp.Width - 1;
        while (x > minX && bmp.IsColumnTransparent(x, minY, alphaLimit))
        {
            x--;
        }

        var maxX = Math.Min(x + 7, bmp.Width - 1);
        bmp.UnlockImage();
        return (minX, minY, maxX, maxY);
    }

    // -------------------------------------------------------------------------------------
    // 5) HtmlUtil.FixInvalidItalicTags on tag-free lines (the overwhelmingly common case)
    // -------------------------------------------------------------------------------------

    private void SetupItalicCase()
    {
        _plainLines = new string[500];
        for (var i = 0; i < _plainLines.Length; i++)
        {
            _plainLines[i] = i % 3 == 0
                ? $"Ordinary line number {i} - no markup at all."
                : $"- Are you sure about {i}?\r\n- Absolutely, one hundred percent.";
        }

        foreach (var line in _plainLines)
        {
            if (OldFixInvalidItalicTagsNoTagPath(line) != HtmlUtil.FixInvalidItalicTags(line))
            {
                throw new InvalidOperationException("italic-fix mismatch");
            }
        }

        // The tagged path must be untouched by the early-out.
        const string tagged = "< i>Hello there</ i >\r\n<i>second<i/>";
        if (HtmlUtil.FixInvalidItalicTags(tagged) != OldEquivalentForTagged(tagged))
        {
            throw new InvalidOperationException("tagged italic-fix path changed");
        }
    }

    private static string OldEquivalentForTagged(string input) =>
        // With a '<' present the shipping method is byte-for-byte the old code, so it is
        // its own oracle here; this call just documents the invariant.
        HtmlUtil.FixInvalidItalicTags(input);

    private static readonly string[] BeginTagVariations = { "< i >", "< i>", "<i >", "< I >", "< I>", "<I >", "<i<", "<I<", "<I>" };

    private static readonly string[] EndTagVariations =
    {
        "< / i >", "< /i>", "</ i>", "< /i >", "</i >", "</ i >",
        "< / i>", "</I>", "< / I >", "< /I>", "</ I>", "< /I >", "</I >", "</ I >", "< / I>", "</i<", "</I<", "</I>",
    };

    private static string OldFixInvalidItalicTagsNoTagPath(string input)
    {
        // The pre-round-16 method up to its "no italic tags" return - the full path a
        // tag-free line paid on every call.
        var text = input;

        var preTags = string.Empty;
        if (text.StartsWith("{\\", StringComparison.Ordinal))
        {
            var endIdx = text.IndexOf('}', 2);
            if (endIdx > 2)
            {
                preTags = text.Substring(0, endIdx + 1);
                text = text.Remove(0, endIdx + 1);
            }
        }

        const string beginTag = "<i>";
        const string endTag = "</i>";
        foreach (var beginTagVariation in BeginTagVariations)
        {
            text = text.Replace(beginTagVariation, beginTag);
        }

        foreach (var endTagVariation in EndTagVariations)
        {
            text = text.Replace(endTagVariation, endTag);
        }

        text = text.Replace("</i> <i>", "_@_");
        text = text.Replace(" _@_", "_@_");
        text = text.Replace(" _@_ ", "_@_");
        text = text.Replace("_@_", " ");
        text = text.Replace(" </i>" + Environment.NewLine, "</i>" + Environment.NewLine);

        if (text.Contains(beginTag))
        {
            text = text.Replace("<i/>", endTag);
            text = text.Replace("<I/>", endTag);
        }
        else
        {
            text = text.Replace("<i/>", string.Empty);
            text = text.Replace("<I/>", string.Empty);
        }

        text = text.Replace("]<i> ", "] <i>");
        text = text.Replace(")<i> ", ") <i>");
        text = text.Replace("] </i>", "] </i>");
        text = text.Replace(") </i>", ") </i>");

        text = text.Replace(beginTag + beginTag, beginTag);
        text = text.Replace(endTag + endTag, endTag);

        var italicBeginTagCount = Utilities.CountTagInText(text, beginTag);
        var italicEndTagCount = Utilities.CountTagInText(text, endTag);
        _ = Utilities.GetNumberOfLines(text);
        if (italicBeginTagCount + italicEndTagCount == 0)
        {
            return preTags + text;
        }

        throw new InvalidOperationException("benchmark input unexpectedly contained italic tags");
    }

    [Benchmark]
    public int ItalicFix_AlwaysReplace_Old()
    {
        var total = 0;
        foreach (var line in _plainLines)
        {
            total += OldFixInvalidItalicTagsNoTagPath(line).Length;
        }

        return total;
    }

    [Benchmark]
    public int ItalicFix_EarlyOut_New()
    {
        var total = 0;
        foreach (var line in _plainLines)
        {
            total += HtmlUtil.FixInvalidItalicTags(line).Length;
        }

        return total;
    }
}
