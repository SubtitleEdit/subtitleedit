using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 17 performance hunt. Every "Old" benchmark is a self-contained copy of the
/// pre-round-17 shape and every "New" benchmark is the shape that shipped (or the shipping
/// method itself where it is public); <see cref="Setup"/> asserts the two agree.
///
/// The themes are (a) "Substring(i) inside a per-character loop", which allocated and copied
/// the whole rest of the line on every character - Sami, EBU STL, the four D-Cinema writers,
/// PAC and "remove text for hearing impaired"; (b) per-item linear/XPath scans that could be
/// indexed once - the IMSC 1.1 style and region lookups, PAC's character table, EBU's special
/// ASCII table and Blu-ray's palette matching; and (c) per-pixel byte work that fits in one
/// 32-bit load/store (NikseBitmap.ConvertToFourColors).
///
/// Default job, Apple M4, .NET 10, StdDev under 1% on every row:
///
///   Sami tag scan              15,938 us -> 35.4 us   450x   707 MB -> 66 KB allocated
///   IMSC 1.1 region lookup      4,330 us -> 22.7 us   191x   871 KB -> 5 KB
///   IMSC 1.1 style lookup       9,672 us -> 66.2 us   146x   2.1 MB -> 50 KB
///   PAC character table         1,940 us -> 30.6 us    63x   4.1 MB -> 111 KB
///   D-Cinema tag scan             568 us -> 43.4 us    13x   10.8 MB -> 0
///   EBU STL text encode         2,055 us -> 219 us    9.4x   5.8 MB -> 66 KB
///   RemoveHiTagsInsideLine        116 us -> 26.8 us   4.3x   1.5 MB -> 48 KB
///   Blu-ray SUP RLE encode      1,554 us -> 450 us    3.5x
///   Blu-ray SUP palette           878 us -> 460 us    1.9x
///   ASSA event line fields       76.7 us -> 55.7 us  1.38x   441 KB -> 166 KB
///   NikseBitmap four colours     85.8 us -> 71.4 us  1.20x
/// </summary>
[MemoryDiagnoser]
public class PerfHuntRound17Benchmarks
{
    // ------------------------------------------------------------------ shared corpora
    private List<string> _assaEventLines = new();
    private string _samiText = string.Empty;
    private string[] _hiLines = Array.Empty<string>();
    private string[] _writerLines = Array.Empty<string>();
    private XmlDocument _imscDocument = new();
    private List<XmlNode> _imscSpans = new();
    private List<string> _imscRegionIds = new();
    private Dictionary<int, string> _pacCodes = new();
    private string _pacText = string.Empty;
    private Encoding _pacEncoding = Encoding.Latin1;
    private SKBitmap _caption = null!;
    private byte[] _fourColorSource = Array.Empty<byte>();
    private List<SKColor> _supPalette = new();

    private const string TtmlNs = "http://www.w3.org/ns/ttml";

    [GlobalSetup]
    public void Setup()
    {
        BuildAssaCorpus();
        BuildSamiCorpus();
        BuildHiCorpus();
        BuildWriterCorpus();
        BuildImscDocument();
        BuildPacCorpus();
        BuildBitmaps();
        AssertEquivalence();
    }

    private static string Sentence(int i) =>
        $"The quick brown fox number {i} jumps over the lazy dog, twice, and then rests.";

    private void BuildAssaCorpus()
    {
        var lines = new List<string>();
        for (var i = 0; i < 400; i++)
        {
            lines.Add($"Dialogue: 0,0:0{i % 9}:0{i % 6}.00,0:0{i % 9}:0{(i + 2) % 6}.00,Default,,0000,0000,0000,,{Sentence(i)}");
        }

        _assaEventLines = lines;
    }

    private void BuildSamiCorpus()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 200; i++)
        {
            sb.Append("<i>Italic ").Append(i).Append("</i> and <font color=\"#ff0000\">red</font> ")
              .Append(Sentence(i)).Append('\n');
        }

        _samiText = sb.ToString();
    }

    private void BuildHiCorpus()
    {
        var lines = new List<string>(200);
        for (var i = 0; i < 200; i++)
        {
            // No removable hearing-impaired tag: that is the case almost every line of a real
            // subtitle takes, and it is the one the per-character Substring(i) scan dominated.
            lines.Add($"Sentence number {i} ends here. Another one follows! And a third? Yes it does.");
        }

        _hiLines = lines.ToArray();
    }

    private void BuildWriterCorpus()
    {
        var lines = new List<string>(200);
        for (var i = 0; i < 200; i++)
        {
            lines.Add($"<i>Italic {i}</i> plain <b>bold</b> and <font color=\"#00ff00\">green</font> {Sentence(i)}");
        }

        _writerLines = lines.ToArray();
    }

    private void BuildImscDocument()
    {
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>")
          .Append("<tt xmlns=\"").Append(TtmlNs).Append("\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xml:lang=\"en\"><head><styling>");
        for (var i = 0; i < 40; i++)
        {
            sb.Append("<style xml:id=\"s").Append(i).Append("\" tts:fontStyle=\"italic\" tts:color=\"#ff00").Append(i % 10).Append("0\"/>");
        }

        sb.Append("</styling><layout>");
        for (var i = 0; i < 20; i++)
        {
            sb.Append("<region xml:id=\"r").Append(i)
              .Append("\" tts:origin=\"10% ").Append(i * 4)
              .Append("%\" tts:extent=\"80% 20%\" tts:displayAlign=\"after\" tts:textAlign=\"center\"/>");
        }

        sb.Append("</layout></head><body><div>");
        for (var i = 0; i < 300; i++)
        {
            sb.Append("<p begin=\"00:00:0").Append(i % 9).Append(".000\" end=\"00:00:0").Append((i % 9) + 1)
              .Append(".000\" region=\"r").Append(i % 20).Append("\"><span style=\"s").Append(i % 40)
              .Append(" s").Append((i + 7) % 40).Append("\">styled ").Append(i).Append("</span> tail</p>");
        }

        sb.Append("</div></body></tt>");

        _imscDocument = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
        _imscDocument.LoadXml(sb.ToString());

        var nsmgr = new XmlNamespaceManager(_imscDocument.NameTable);
        nsmgr.AddNamespace("ttml", TtmlNs);
        _imscSpans = new List<XmlNode>();
        foreach (XmlNode span in _imscDocument.SelectNodes("//ttml:span", nsmgr)!)
        {
            _imscSpans.Add(span);
        }

        _imscRegionIds = new List<string>();
        foreach (XmlNode p in _imscDocument.SelectNodes("//ttml:p", nsmgr)!)
        {
            _imscRegionIds.Add(p.Attributes!["region"]!.Value);
        }
    }

    private void BuildPacCorpus()
    {
        // Same shape and size as Pac.LatinCodes: a code-to-character table scanned the other way
        // round for every character written.
        _pacCodes = new Dictionary<int, string>();
        const string letters = "ÃÑÕãñõÄËÏÖÜäëïöüÀÈÌÒÙàèìòùÁÉÍÓÚáéíóúÂÊÎÔÛâêîôûÅåÇçØøÆæßÐðÞþŁłŃńŚśŹźŻż";
        var code = 0xe041;
        foreach (var ch in letters)
        {
            _pacCodes[code++] = ch.ToString();
        }

        // Pad to the real table's size so the linear scan costs what it costs in the product.
        for (var i = 0; _pacCodes.Count < 350; i++)
        {
            _pacCodes[code++] = "Ā" + (char)i;
        }

        var sb = new StringBuilder();
        for (var i = 0; i < 20; i++)
        {
            sb.Append(Sentence(i)).Append(" Àccénts: Ø æ ß").Append(Environment.NewLine);
        }

        _pacText = sb.ToString();
        _pacEncoding = Encoding.Latin1;
    }

    private void BuildBitmaps()
    {
        _caption = MakeCaption(360, 72);
        _fourColorSource = new NikseBitmap(_caption).GetPixelData().ToArray();
        _supPalette = OldGetBitmapPalette(_caption, SKColors.White);
    }

    /// <summary>
    /// A stand-in for a rendered caption: white glyph strokes with a black outline over a
    /// transparent background, anti-aliased. That is what the Blu-ray palette and RLE passes
    /// actually see - a few thousand distinct colours from the coverage ramp, each of them
    /// repeated on row after row - rather than uniform noise.
    /// </summary>
    private static SKBitmap MakeCaption(int width, int height)
    {
        var bmp = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                // Distance to the nearest "stroke" of a repeating glyph-like pattern.
                var sx = (x % 24) - 12;
                var sy = (y % 18) - 9;
                var d = Math.Sqrt(sx * sx * 0.6 + sy * sy * 0.25) + ((x * 7 + y * 13) % 5) * 0.12;

                if (d > 5.2)
                {
                    bmp.SetPixel(x, y, SKColors.Transparent);
                    continue;
                }

                var coverage = Math.Clamp(1.0 - (d - 3.0), 0.0, 1.0);   // fill vs outline
                var alpha = (byte)Math.Clamp((5.2 - d) * 90.0, 1.0, 255.0);
                var level = (byte)(coverage * 255.0);
                bmp.SetPixel(x, y, new SKColor(level, level, (byte)Math.Min(255, level + (x % 3) * 4), alpha));
            }
        }

        return bmp;
    }

    private void AssertEquivalence()
    {
        void Check(bool ok, string what)
        {
            if (!ok)
            {
                throw new InvalidOperationException("old/new mismatch: " + what);
            }
        }

        Check(OldAssaEventFields(_assaEventLines) == NewAssaEventFields(_assaEventLines), "assa event fields");
        Check(OldImscStyleScan(_imscSpans, _imscDocument) == NewImscStyleScan(_imscSpans, _imscDocument), "imsc style scan");
        Check(OldImscRegionScan(_imscRegionIds, _imscDocument) == NewImscRegionScan(_imscRegionIds, _imscDocument), "imsc region scan");
        Check(OldRemoveHiTags(_hiLines) == NewRemoveHiTags(_hiLines), "remove hi tags");
        Check(OldSamiTagScan(_samiText) == NewSamiTagScan(_samiText), "sami tag scan");
        Check(OldDCinemaTagScan(_writerLines) == NewDCinemaTagScan(_writerLines), "d-cinema tag scan");

        var ebuOld = OldEbuEncode(_writerLines, _pacEncoding);
        var ebuNew = NewEbuEncode(_writerLines, _pacEncoding);
        Check(ebuOld.SequenceEqual(ebuNew), "ebu encode");

        var pacOld = OldPacLatinBytes(_pacEncoding, _pacText, 2, _pacCodes);
        var pacNew = NewPacLatinBytes(_pacEncoding, _pacText, 2, _pacCodes);
        Check(pacOld.SequenceEqual(pacNew), "pac latin bytes");

        var palOld = OldGetBitmapPalette(_caption, SKColors.White);
        var palNew = NewGetBitmapPalette(_caption, SKColors.White);
        Check(palOld.Count == palNew.Count && palOld.SequenceEqual(palNew), "blu-ray palette");

        var rleOld = OldEncodeImage(_caption, _supPalette);
        var rleNew = NewEncodeImage(_caption, _supPalette);
        Check(rleOld.SequenceEqual(rleNew), "blu-ray rle");

        // useInnerAntialize=false only: the true branch ends in a second full pass
        // (VobSubAntialize) that this round did not touch and the copy below does not model.
        var fourOld = OldConvertToFourColors(_fourColorSource, SKColors.Transparent, SKColors.White, SKColors.Black, false);
        var fourNew = new NikseBitmap(_caption.Width, _caption.Height, (byte[])_fourColorSource.Clone());
        fourNew.ConvertToFourColors(SKColors.Transparent, SKColors.White, SKColors.Black, false);
        Check(fourOld.AsSpan().SequenceEqual(fourNew.GetPixelData()), "convert to four colors");
    }

    // ================================================================== ASSA event lines
    // Old: Split(',') allocated a string[] plus a string per field, Trim() a second one for each
    // field kept, and the text field was split apart and re-joined comma by comma.

    [Benchmark]
    public int AssaEventFields_Old() => OldAssaEventFields(_assaEventLines);

    [Benchmark]
    public int AssaEventFields_New() => NewAssaEventFields(_assaEventLines);

    private static int OldAssaEventFields(List<string> lines)
    {
        const int indexLayer = 0, indexStart = 1, indexEnd = 2, indexStyle = 3, indexActor = -1;
        const int indexName = 4, indexMarginL = 5, indexMarginR = 6, indexMarginV = 7, indexEffect = 8, indexText = 9;
        var textBuilder = new StringBuilder();
        var hash = 17;
        foreach (var line in lines)
        {
            textBuilder.Clear();
            string start = string.Empty, end = string.Empty, style = string.Empty, actor = string.Empty;
            string marginL = string.Empty, marginR = string.Empty, marginV = string.Empty, effect = string.Empty;
            var layer = 0;

            var splitLine = line.Remove(0, 9).Split(',');
            for (var i = 0; i < splitLine.Length; i++)
            {
                if (i == indexStart)
                {
                    start = splitLine[i].Trim();
                }
                else if (i == indexEnd)
                {
                    end = splitLine[i].Trim();
                }
                else if (i == indexStyle)
                {
                    style = splitLine[i].Trim();
                }
                else if (i == indexActor && indexName == -1)
                {
                    actor = splitLine[i].Trim();
                }
                else if (i == indexName)
                {
                    actor = splitLine[i].Trim();
                }
                else if (i == indexMarginL)
                {
                    marginL = splitLine[i].Trim();
                }
                else if (i == indexMarginR)
                {
                    marginR = splitLine[i].Trim();
                }
                else if (i == indexMarginV)
                {
                    marginV = splitLine[i].Trim();
                }
                else if (i == indexEffect)
                {
                    effect = splitLine[i].Trim();
                }
                else if (i == indexLayer)
                {
                    int.TryParse(splitLine[i].Replace("Comment:", string.Empty).Trim(), out layer);
                }
                else if (i == indexText)
                {
                    textBuilder.Append(splitLine[i]);
                }
                else if (i > indexText)
                {
                    textBuilder.Append(',').Append(splitLine[i]);
                }
            }

            hash = Mix(hash, start, end, style, actor, marginL, marginR, marginV, effect, layer, textBuilder.ToString());
        }

        return hash;
    }

    private static int NewAssaEventFields(List<string> lines)
    {
        const int indexLayer = 0, indexStart = 1, indexEnd = 2, indexStyle = 3, indexActor = -1;
        const int indexName = 4, indexMarginL = 5, indexMarginR = 6, indexMarginV = 7, indexEffect = 8, indexText = 9;
        const bool textIsTrailing = true;
        var textBuilder = new StringBuilder();
        var hash = 17;
        foreach (var line in lines)
        {
            textBuilder.Clear();
            string start = string.Empty, end = string.Empty, style = string.Empty, actor = string.Empty;
            string marginL = string.Empty, marginR = string.Empty, marginV = string.Empty, effect = string.Empty;
            var layer = 0;

            var fields = line.AsSpan(9);
            string? text = null;
            var fieldStart = 0;
            for (var i = 0; ; i++)
            {
                var comma = fields.Slice(fieldStart).IndexOf(',');
                var fieldEnd = comma < 0 ? fields.Length : fieldStart + comma;

                if (textIsTrailing && i == indexText)
                {
                    text = fields.Slice(fieldStart).ToString();
                    break;
                }

                var field = fields.Slice(fieldStart, fieldEnd - fieldStart);
                if (i == indexStart)
                {
                    start = field.Trim().ToString();
                }
                else if (i == indexEnd)
                {
                    end = field.Trim().ToString();
                }
                else if (i == indexStyle)
                {
                    style = field.Trim().ToString();
                }
                else if (i == indexActor && indexName == -1)
                {
                    actor = field.Trim().ToString();
                }
                else if (i == indexName)
                {
                    actor = field.Trim().ToString();
                }
                else if (i == indexMarginL)
                {
                    marginL = field.Trim().ToString();
                }
                else if (i == indexMarginR)
                {
                    marginR = field.Trim().ToString();
                }
                else if (i == indexMarginV)
                {
                    marginV = field.Trim().ToString();
                }
                else if (i == indexEffect)
                {
                    effect = field.Trim().ToString();
                }
                else if (i == indexLayer)
                {
                    if (field.IndexOf("Comment:".AsSpan(), StringComparison.Ordinal) >= 0)
                    {
                        int.TryParse(field.ToString().Replace("Comment:", string.Empty).Trim(), out layer);
                    }
                    else
                    {
                        int.TryParse(field.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture, out layer);
                    }
                }
                else if (i == indexText)
                {
                    textBuilder.Append(field);
                }
                else if (i > indexText)
                {
                    textBuilder.Append(',').Append(field);
                }

                if (comma < 0)
                {
                    break;
                }

                fieldStart = fieldEnd + 1;
            }

            text ??= textBuilder.ToString();
            hash = Mix(hash, start, end, style, actor, marginL, marginR, marginV, effect, layer, text);
        }

        return hash;
    }

    private static int Mix(int hash, string a, string b, string c, string d, string e, string f, string g, string h, int layer, string text)
    {
        unchecked
        {
            hash = hash * 31 + a.GetHashCode(StringComparison.Ordinal);
            hash = hash * 31 + b.GetHashCode(StringComparison.Ordinal);
            hash = hash * 31 + c.GetHashCode(StringComparison.Ordinal);
            hash = hash * 31 + d.GetHashCode(StringComparison.Ordinal);
            hash = hash * 31 + e.GetHashCode(StringComparison.Ordinal);
            hash = hash * 31 + f.GetHashCode(StringComparison.Ordinal);
            hash = hash * 31 + g.GetHashCode(StringComparison.Ordinal);
            hash = hash * 31 + h.GetHashCode(StringComparison.Ordinal);
            hash = hash * 31 + layer;
            hash = hash * 31 + text.GetHashCode(StringComparison.Ordinal);
            return hash;
        }
    }

    // ================================================================== IMSC 1.1 style lookup
    // Old: a fresh XmlNamespaceManager plus a document-wide "//ttml:style" XPath for every style
    // name of every span. New: the head's styles grouped by id once per load.

    [Benchmark]
    public int ImscStyleScan_Old() => OldImscStyleScan(_imscSpans, _imscDocument);

    [Benchmark]
    public int ImscStyleScan_New() => NewImscStyleScan(_imscSpans, _imscDocument);

    private static int OldImscStyleScan(List<XmlNode> spans, XmlDocument xml)
    {
        var hits = 0;
        foreach (var child in spans)
        {
            var styleNames = child.Attributes!["style"]!.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var styleName in styleNames)
            {
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ttml", TtmlNs);
                var head = xml.DocumentElement!.SelectSingleNode("ttml:head", nsmgr);
                foreach (XmlNode styleNode in head!.SelectNodes("//ttml:style", nsmgr)!)
                {
                    string? currentStyle = null;
                    if (styleNode.Attributes!["xml:id"] != null)
                    {
                        currentStyle = styleNode.Attributes["xml:id"]!.Value;
                    }
                    else if (styleNode.Attributes["id"] != null)
                    {
                        currentStyle = styleNode.Attributes["id"]!.Value;
                    }

                    if (currentStyle == styleName)
                    {
                        if (styleNode.Attributes["tts:fontStyle"] != null && styleNode.Attributes["tts:fontStyle"]!.Value == "italic")
                        {
                            hits++;
                        }

                        if (styleNode.Attributes["tts:color"] != null)
                        {
                            hits += styleNode.Attributes["tts:color"]!.Value.Length;
                        }
                    }
                }
            }
        }

        return hits;
    }

    private static int NewImscStyleScan(List<XmlNode> spans, XmlDocument xml)
    {
        var index = BuildIndex(xml, "//ttml:style");
        var hits = 0;
        foreach (var child in spans)
        {
            var styleNames = child.Attributes!["style"]!.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var styleName in styleNames)
            {
                if (!index.TryGetValue(styleName, out var nodes))
                {
                    continue;
                }

                foreach (var styleNode in nodes)
                {
                    if (styleNode.Attributes!["tts:fontStyle"] != null && styleNode.Attributes["tts:fontStyle"]!.Value == "italic")
                    {
                        hits++;
                    }

                    if (styleNode.Attributes["tts:color"] != null)
                    {
                        hits += styleNode.Attributes["tts:color"]!.Value.Length;
                    }
                }
            }
        }

        return hits;
    }

    private static Dictionary<string, List<XmlNode>> BuildIndex(XmlDocument xml, string xpath)
    {
        var result = new Dictionary<string, List<XmlNode>>();
        var nsmgr = new XmlNamespaceManager(xml.NameTable);
        nsmgr.AddNamespace("ttml", TtmlNs);
        foreach (XmlNode node in xml.SelectNodes(xpath, nsmgr)!)
        {
            var id = node.Attributes?["xml:id"]?.Value ?? node.Attributes?["id"]?.Value;
            if (id == null)
            {
                continue;
            }

            if (!result.TryGetValue(id, out var list))
            {
                list = new List<XmlNode>(1);
                result[id] = list;
            }

            list.Add(node);
        }

        return result;
    }

    // ================================================================== IMSC 1.1 region lookup

    [Benchmark]
    public int ImscRegionScan_Old() => OldImscRegionScan(_imscRegionIds, _imscDocument);

    [Benchmark]
    public int ImscRegionScan_New() => NewImscRegionScan(_imscRegionIds, _imscDocument);

    private static int OldImscRegionScan(List<string> regionIds, XmlDocument xml)
    {
        var hits = 0;
        foreach (var regionId in regionIds)
        {
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ttml", TtmlNs);
            var head = xml.DocumentElement!.SelectSingleNode("ttml:head", nsmgr);
            if (head == null)
            {
                continue;
            }

            foreach (XmlNode regionNode in head.SelectNodes("//ttml:region", nsmgr)!)
            {
                var id = regionNode.Attributes!["xml:id"]?.Value ?? regionNode.Attributes["id"]?.Value;
                if (id != regionId)
                {
                    continue;
                }

                hits += (regionNode.Attributes["tts:origin"]?.Value ?? string.Empty).Length;
            }
        }

        return hits;
    }

    private static int NewImscRegionScan(List<string> regionIds, XmlDocument xml)
    {
        var index = BuildIndex(xml, "//ttml:region");
        var hits = 0;
        foreach (var regionId in regionIds)
        {
            if (!index.TryGetValue(regionId, out var nodes))
            {
                continue;
            }

            foreach (var regionNode in nodes)
            {
                hits += (regionNode.Attributes!["tts:origin"]?.Value ?? string.Empty).Length;
            }
        }

        return hits;
    }

    // ================================================================== remove text for HI
    // New calls the shipping method; Old is the pre-round-17 per-character Substring(i) scan.
    // The corpus deliberately holds no removable tag, so neither shape reaches the removal
    // branch - that is the case nearly every line of a real subtitle takes.

    private RemoveTextForHI _removeTextForHi = null!;

    [Benchmark]
    public int RemoveHiTagsInsideLine_Old() => OldRemoveHiTags(_hiLines);

    [Benchmark]
    public int RemoveHiTagsInsideLine_New() => NewRemoveHiTags(_hiLines);

    private RemoveTextForHI GetHi()
    {
        return _removeTextForHi ??= new RemoveTextForHI(new RemoveTextForHISettings(new Subtitle())
        {
            OnlyIfInSeparateLine = false,
            RemoveTextBetweenSquares = true,
            RemoveTextBetweenBrackets = true,
            RemoveTextBetweenParentheses = true,
            RemoveTextBetweenQuestionMarks = true,
            RemoveTextBetweenCustomTags = false,
        });
    }

    private int OldRemoveHiTags(string[] lines)
    {
        var hi = GetHi();
        var total = 0;
        foreach (var input in lines)
        {
            var newText = input;
            const string endChars = ".?!";
            for (var i = 6; i < newText.Length; i++)
            {
                var s = newText.Substring(i);
                if (s.Length > 2 && endChars.Contains(s[0]))
                {
                    var pre = string.Empty;

                    s = s.Remove(0, 1);
                    if (s.StartsWith(' '))
                    {
                        pre = s.StartsWith(" <i>", StringComparison.Ordinal) ? " <i>" : " ";
                    }
                    else if (s.StartsWith("<i>", StringComparison.Ordinal))
                    {
                        pre = "<i>";
                    }
                    else if (s.StartsWith("</i>", StringComparison.Ordinal))
                    {
                        pre = "</i>";
                    }

                    if (pre.Length > 0)
                    {
                        s = s.Remove(0, pre.Length);
                        if (s.Length > 1 && s[0] == ' ')
                        {
                            pre += " ";
                            s = s.Remove(0, 1);
                        }

                        if (hi.HasHearImpairedTagsAtStartOrEnd(s))
                        {
                            throw new InvalidOperationException("benchmark corpus must not hit the removal branch");
                        }
                    }
                }
            }

            total += newText.Length;
        }

        return total;
    }

    private int NewRemoveHiTags(string[] lines)
    {
        var hi = GetHi();
        var total = 0;
        foreach (var input in lines)
        {
            total += hi.RemoveHearingImpairedTagsInsideLine(input).Length;
        }

        return total;
    }

    // ================================================================== Sami tag scan

    [Benchmark]
    public int SamiTagScan_Old() => OldSamiTagScan(_samiText);

    [Benchmark]
    public int SamiTagScan_New() => NewSamiTagScan(_samiText);

    private static int OldSamiTagScan(string text)
    {
        var totalLine = new StringBuilder();
        var partialLine = new StringBuilder();
        var tagOn = false;
        for (var i = 0; i < text.Length; i++)
        {
            var t = text.Substring(i);
            if (t.StartsWith('<') &&
                (t.StartsWith("<font", StringComparison.Ordinal) ||
                 t.StartsWith("<div", StringComparison.Ordinal) ||
                 t.StartsWith("<i", StringComparison.Ordinal) ||
                 t.StartsWith("<b", StringComparison.Ordinal) ||
                 t.StartsWith("<s", StringComparison.Ordinal) ||
                 t.StartsWith("</", StringComparison.Ordinal)))
            {
                totalLine.Append(partialLine);
                partialLine.Clear();
                tagOn = true;
                totalLine.Append('<');
            }
            else if (t.StartsWith('>') && tagOn)
            {
                tagOn = false;
                totalLine.Append('>');
            }
            else if (!tagOn)
            {
                partialLine.Append(text[i]);
            }
            else
            {
                totalLine.Append(text[i]);
            }
        }

        totalLine.Append(partialLine);
        return totalLine.Length;
    }

    private static int NewSamiTagScan(string text)
    {
        var totalLine = new StringBuilder();
        var partialLine = new StringBuilder();
        var tagOn = false;
        for (var i = 0; i < text.Length; i++)
        {
            var t = text.AsSpan(i);
            if (t[0] == '<' &&
                (t.StartsWith("<font".AsSpan(), StringComparison.Ordinal) ||
                 t.StartsWith("<div".AsSpan(), StringComparison.Ordinal) ||
                 t.StartsWith("<i".AsSpan(), StringComparison.Ordinal) ||
                 t.StartsWith("<b".AsSpan(), StringComparison.Ordinal) ||
                 t.StartsWith("<s".AsSpan(), StringComparison.Ordinal) ||
                 t.StartsWith("</".AsSpan(), StringComparison.Ordinal)))
            {
                totalLine.Append(partialLine);
                partialLine.Clear();
                tagOn = true;
                totalLine.Append('<');
            }
            else if (t[0] == '>' && tagOn)
            {
                tagOn = false;
                totalLine.Append('>');
            }
            else if (!tagOn)
            {
                partialLine.Append(text[i]);
            }
            else
            {
                totalLine.Append(text[i]);
            }
        }

        totalLine.Append(partialLine);
        return totalLine.Length;
    }

    // ================================================================== D-Cinema tag scan

    [Benchmark]
    public int DCinemaTagScan_Old() => OldDCinemaTagScan(_writerLines);

    [Benchmark]
    public int DCinemaTagScan_New() => NewDCinemaTagScan(_writerLines);

    private static int OldDCinemaTagScan(string[] lines)
    {
        var count = 0;
        foreach (var line in lines)
        {
            var isItalic = false;
            var isBold = false;
            var fontNo = 0;
            var i = 0;
            while (i < line.Length)
            {
                if (!isItalic && line.Substring(i).StartsWith("<i>", StringComparison.Ordinal))
                {
                    isItalic = true;
                    i += 2;
                }
                else if (!isBold && line.Substring(i).StartsWith("<b>", StringComparison.Ordinal))
                {
                    isBold = true;
                    i += 2;
                }
                else if (isItalic && line.Substring(i).StartsWith("</i>", StringComparison.Ordinal))
                {
                    isItalic = false;
                    i += 3;
                }
                else if (isBold && line.Substring(i).StartsWith("</b>", StringComparison.Ordinal))
                {
                    isBold = false;
                    i += 3;
                }
                else if (line.Substring(i).StartsWith("<font color=", StringComparison.Ordinal) && line.Substring(i + 3).Contains('>'))
                {
                    fontNo++;
                    i = line.IndexOf('>', i);
                }
                else if (fontNo > 0 && line.Substring(i).StartsWith("</font>", StringComparison.Ordinal))
                {
                    fontNo--;
                    i += 6;
                }
                else
                {
                    count++;
                }

                i++;
            }
        }

        return count;
    }

    private static int NewDCinemaTagScan(string[] lines)
    {
        var count = 0;
        foreach (var line in lines)
        {
            var isItalic = false;
            var isBold = false;
            var fontNo = 0;
            var i = 0;
            while (i < line.Length)
            {
                if (!isItalic && line.AsSpan(i).StartsWith("<i>".AsSpan(), StringComparison.Ordinal))
                {
                    isItalic = true;
                    i += 2;
                }
                else if (!isBold && line.AsSpan(i).StartsWith("<b>".AsSpan(), StringComparison.Ordinal))
                {
                    isBold = true;
                    i += 2;
                }
                else if (isItalic && line.AsSpan(i).StartsWith("</i>".AsSpan(), StringComparison.Ordinal))
                {
                    isItalic = false;
                    i += 3;
                }
                else if (isBold && line.AsSpan(i).StartsWith("</b>".AsSpan(), StringComparison.Ordinal))
                {
                    isBold = false;
                    i += 3;
                }
                else if (line.AsSpan(i).StartsWith("<font color=".AsSpan(), StringComparison.Ordinal) && line.AsSpan(i + 3).IndexOf('>') >= 0)
                {
                    fontNo++;
                    i = line.IndexOf('>', i);
                }
                else if (fontNo > 0 && line.AsSpan(i).StartsWith("</font>".AsSpan(), StringComparison.Ordinal))
                {
                    fontNo--;
                    i += 6;
                }
                else
                {
                    count++;
                }

                i++;
            }
        }

        return count;
    }

    // ================================================================== EBU STL text encoding

    private static readonly Dictionary<int, string> EbuSpecialAsciiCodes = new Dictionary<int, string>
    {
        { 0xd3, "©" }, { 0xd4, "™" }, { 0xd5, "♪" },
        { 0xe0, "Ω" }, { 0xe1, "Æ" }, { 0xe2, "Ð" }, { 0xe3, "ª" }, { 0xe4, "Ħ" },
        { 0xe6, "Ĳ" }, { 0xe7, "Ŀ" }, { 0xe8, "Ł" }, { 0xe9, "Ø" }, { 0xea, "Œ" },
        { 0xeb, "º" }, { 0xec, "Þ" }, { 0xed, "Ŧ" }, { 0xee, "Ŋ" }, { 0xef, "ŉ" },
        { 0xf0, "ĸ" }, { 0xf1, "æ" }, { 0xf2, "đ" }, { 0xf3, "ð" }, { 0xf4, "ħ" },
        { 0xf5, "ı" }, { 0xf6, "ĳ" }, { 0xf7, "ŀ" }, { 0xf8, "ł" }, { 0xf9, "ø" },
        { 0xfa, "œ" }, { 0xfb, "ß" }, { 0xfc, "þ" }, { 0xfd, "ŧ" }, { 0xfe, "ŋ" },
    };

    private static readonly Dictionary<char, int> EbuSpecialAsciiCodesByChar = BuildEbuByChar();

    private static Dictionary<char, int> BuildEbuByChar()
    {
        var result = new Dictionary<char, int>();
        foreach (var kvp in EbuSpecialAsciiCodes)
        {
            if (kvp.Value.Length == 1 && !result.ContainsKey(kvp.Value[0]))
            {
                result.Add(kvp.Value[0], kvp.Key);
            }
        }

        return result;
    }

    [Benchmark]
    public int EbuTextEncode_Old() => OldEbuEncode(_writerLines, _pacEncoding).Count;

    [Benchmark]
    public int EbuTextEncode_New() => NewEbuEncode(_writerLines, _pacEncoding).Count;

    private static List<byte> OldEbuEncode(string[] lines, Encoding encoding)
    {
        var textBytes = new List<byte>();
        foreach (var line in lines)
        {
            var i = 0;
            while (i < line.Length)
            {
                var newStart = line.Substring(i);
                if (newStart.StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                {
                    var end = line.IndexOf('>', i);
                    i = end > 0 ? end + 1 : i + 1;
                }
                else if (newStart == "</font>")
                {
                    i += "</font>".Length;
                }
                else if (newStart.StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                {
                    i += "</font>".Length;
                }
                else if (newStart.StartsWith("<i>", StringComparison.Ordinal))
                {
                    i += "<i>".Length;
                    textBytes.Add(0x80);
                }
                else if (newStart.StartsWith("</i>", StringComparison.Ordinal))
                {
                    i += "</i>".Length;
                    textBytes.Add(0x81);
                }
                else if (newStart.StartsWith("<u>", StringComparison.Ordinal))
                {
                    i += "<u>".Length;
                    textBytes.Add(0x82);
                }
                else if (newStart.StartsWith("</u>", StringComparison.Ordinal))
                {
                    i += "</u>".Length;
                    textBytes.Add(0x83);
                }
                else if (newStart.StartsWith("<box>", StringComparison.Ordinal))
                {
                    i += "<box>".Length;
                    textBytes.Add(0x84);
                }
                else if (newStart.StartsWith("</box>", StringComparison.Ordinal))
                {
                    i += "</box>".Length;
                    textBytes.Add(0x85);
                }
                else
                {
                    var nextCh = line.Substring(i, 1);
                    if (nextCh == "#")
                    {
                        textBytes.Add(0x23);
                    }
                    else if (EbuSpecialAsciiCodes.ContainsValue(nextCh))
                    {
                        textBytes.Add((byte)EbuSpecialAsciiCodes.First(p => p.Value == nextCh).Key);
                    }
                    else
                    {
                        textBytes.AddRange(encoding.GetBytes(nextCh));
                    }

                    i++;
                }
            }
        }

        return textBytes;
    }

    private static List<byte> NewEbuEncode(string[] lines, Encoding encoding)
    {
        var textBytes = new List<byte>();

        // Allocated once outside the loops - a stackalloc per character would grow the frame
        // for every character in the file (CA2014).
        Span<char> chars = stackalloc char[1];
        Span<byte> bytes = stackalloc byte[8];
        foreach (var line in lines)
        {
            var i = 0;
            while (i < line.Length)
            {
                var newStart = line.AsSpan(i);
                if (newStart.StartsWith("<font ".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    var end = line.IndexOf('>', i);
                    i = end > 0 ? end + 1 : i + 1;
                }
                else if (newStart.SequenceEqual("</font>".AsSpan()))
                {
                    i += "</font>".Length;
                }
                else if (newStart.StartsWith("</font>".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    i += "</font>".Length;
                }
                else if (newStart.StartsWith("<i>".AsSpan(), StringComparison.Ordinal))
                {
                    i += "<i>".Length;
                    textBytes.Add(0x80);
                }
                else if (newStart.StartsWith("</i>".AsSpan(), StringComparison.Ordinal))
                {
                    i += "</i>".Length;
                    textBytes.Add(0x81);
                }
                else if (newStart.StartsWith("<u>".AsSpan(), StringComparison.Ordinal))
                {
                    i += "<u>".Length;
                    textBytes.Add(0x82);
                }
                else if (newStart.StartsWith("</u>".AsSpan(), StringComparison.Ordinal))
                {
                    i += "</u>".Length;
                    textBytes.Add(0x83);
                }
                else if (newStart.StartsWith("<box>".AsSpan(), StringComparison.Ordinal))
                {
                    i += "<box>".Length;
                    textBytes.Add(0x84);
                }
                else if (newStart.StartsWith("</box>".AsSpan(), StringComparison.Ordinal))
                {
                    i += "</box>".Length;
                    textBytes.Add(0x85);
                }
                else
                {
                    var ch = line[i];
                    if (ch == '#')
                    {
                        textBytes.Add(0x23);
                    }
                    else if (EbuSpecialAsciiCodesByChar.TryGetValue(ch, out var special))
                    {
                        textBytes.Add((byte)special);
                    }
                    else
                    {
                        chars[0] = ch;
                        var count = encoding.GetBytes(chars, bytes);
                        for (var k = 0; k < count; k++)
                        {
                            textBytes.Add(bytes[k]);
                        }
                    }

                    i++;
                }
            }
        }

        return textBytes;
    }

    // ================================================================== PAC latin bytes

    [Benchmark]
    public int PacLatinBytes_Old() => OldPacLatinBytes(_pacEncoding, _pacText, 2, _pacCodes).Length;

    [Benchmark]
    public int PacLatinBytes_New() => NewPacLatinBytes(_pacEncoding, _pacText, 2, _pacCodes).Length;

    private static KeyValuePair<int, string>? OldFind(Dictionary<int, string> list, string letter)
    {
        return list?.Where(c => c.Value == letter).Cast<KeyValuePair<int, string>?>().FirstOrDefault();
    }

    private static readonly Dictionary<Dictionary<int, string>, Dictionary<string, KeyValuePair<int, string>>> PacReverse = new();

    private static KeyValuePair<int, string>? NewFind(Dictionary<int, string> list, string letter)
    {
        if (list == null || letter == null)
        {
            return null;
        }

        if (!PacReverse.TryGetValue(list, out var lookup))
        {
            lookup = new Dictionary<string, KeyValuePair<int, string>>(list.Count, StringComparer.Ordinal);
            foreach (var kvp in list)
            {
                if (!lookup.ContainsKey(kvp.Value))
                {
                    lookup.Add(kvp.Value, kvp);
                }
            }

            PacReverse[list] = lookup;
        }

        return lookup.TryGetValue(letter, out var found) ? found : (KeyValuePair<int, string>?)null;
    }

    private static byte[] OldPacLatinBytes(Encoding encoding, string text, byte alignment, Dictionary<int, string> codes)
    {
        var i = 0;
        var buffer = new byte[text.Replace(Environment.NewLine, "12").Length * 4];
        var extra = 0;
        while (i < text.Length)
        {
            if (text.Substring(i).StartsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                buffer[i + extra] = 0xfe;
                i++;
                buffer[i + extra] = alignment;
                extra++;
                buffer[i + extra] = 3;
            }
            else
            {
                var letter = text.Substring(i, 1);
                var code = OldFind(codes, letter);
                if (code != null)
                {
                    var byteValue = code.Value.Key;
                    if (byteValue < 256)
                    {
                        buffer[i + extra] = (byte)byteValue;
                    }
                    else
                    {
                        buffer[i + extra] = (byte)(byteValue / 256);
                        extra++;
                        buffer[i + extra] = (byte)(byteValue % 256);
                    }
                }
                else
                {
                    var values = encoding.GetBytes(letter);
                    for (var k = 0; k < values.Length; k++)
                    {
                        if (k > 0)
                        {
                            extra++;
                        }

                        buffer[i + extra] = values[k];
                    }
                }
            }

            i++;
        }

        var result = new byte[i + extra];
        Array.Copy(buffer, result, result.Length);
        return result;
    }

    private static byte[] NewPacLatinBytes(Encoding encoding, string text, byte alignment, Dictionary<int, string> codes)
    {
        var i = 0;
        var buffer = new byte[text.Replace(Environment.NewLine, "12").Length * 4];
        var extra = 0;
        while (i < text.Length)
        {
            if (text.AsSpan(i).StartsWith(Environment.NewLine.AsSpan(), StringComparison.Ordinal))
            {
                buffer[i + extra] = 0xfe;
                i++;
                buffer[i + extra] = alignment;
                extra++;
                buffer[i + extra] = 3;
            }
            else
            {
                var letter = text.Substring(i, 1);
                var code = NewFind(codes, letter);
                if (code != null)
                {
                    var byteValue = code.Value.Key;
                    if (byteValue < 256)
                    {
                        buffer[i + extra] = (byte)byteValue;
                    }
                    else
                    {
                        buffer[i + extra] = (byte)(byteValue / 256);
                        extra++;
                        buffer[i + extra] = (byte)(byteValue % 256);
                    }
                }
                else
                {
                    var values = encoding.GetBytes(letter);
                    for (var k = 0; k < values.Length; k++)
                    {
                        if (k > 0)
                        {
                            extra++;
                        }

                        buffer[i + extra] = values[k];
                    }
                }
            }

            i++;
        }

        var result = new byte[i + extra];
        Array.Copy(buffer, result, result.Length);
        return result;
    }

    // ================================================================== Blu-ray SUP palette

    [Benchmark]
    public int BluRayPalette_Old() => OldGetBitmapPalette(_caption, SKColors.White).Count;

    [Benchmark]
    public int BluRayPalette_New() => NewGetBitmapPalette(_caption, SKColors.White).Count;

    private static SKColor[] ReadRow(SKBitmap bitmap, int y, SKColor[] row)
    {
        for (var x = 0; x < bitmap.Width; x++)
        {
            row[x] = bitmap.GetPixel(x, y);
        }

        return row;
    }

    private static bool HasCloseColor(SKColor color, List<SKColor> palette, int maxDifference)
    {
        var max = palette.Count;
        for (var i = 0; i < max; i++)
        {
            var c = palette[i];
            var difference = Math.Abs(c.Alpha - color.Alpha) + Math.Abs(c.Red - color.Red) + Math.Abs(c.Green - color.Green) + Math.Abs(c.Blue - color.Blue);
            if (difference < maxDifference)
            {
                return true;
            }
        }

        return false;
    }

    private static List<SKColor> OldGetBitmapPalette(SKBitmap bitmap, SKColor fontColor)
    {
        var pal = new List<SKColor>(255);
        var lookup = new HashSet<SKColor>(255);
        var row = new SKColor[bitmap.Width];
        pal.Add(fontColor);
        lookup.Add(fontColor);

        for (var y = 0; y < bitmap.Height; y++)
        {
            ReadRow(bitmap, y, row);
            for (var x = 0; x < bitmap.Width; x++)
            {
                var c = row[x];
                if (c.Alpha > 0)
                {
                    if (!lookup.Contains(c))
                    {
                        pal.Add(c);
                        lookup.Add(c);
                    }

                    if (pal.Count >= 254)
                    {
                        break;
                    }
                }
            }

            if (pal.Count >= 254)
            {
                break;
            }
        }

        if (pal.Count < 254)
        {
            pal.Add(SKColors.Transparent);
            return pal;
        }

        pal = new List<SKColor>();
        lookup = new HashSet<SKColor>();
        pal.Add(fontColor);
        lookup.Add(fontColor);
        for (var y = 0; y < bitmap.Height; y++)
        {
            ReadRow(bitmap, y, row);
            for (var x = 0; x < bitmap.Width; x++)
            {
                var c = row[x];
                if (c.Alpha > 0)
                {
                    if (lookup.Contains(c))
                    {
                    }
                    else if (pal.Count < 100)
                    {
                        if (!HasCloseColor(c, pal, 1))
                        {
                            pal.Add(c);
                            lookup.Add(c);
                        }
                    }
                    else if (pal.Count < 240)
                    {
                        if (!HasCloseColor(c, pal, 5))
                        {
                            pal.Add(c);
                            lookup.Add(c);
                        }
                    }
                    else if (pal.Count < 254 && !HasCloseColor(c, pal, 25))
                    {
                        pal.Add(c);
                        lookup.Add(c);
                    }
                }
            }
        }

        pal.Add(SKColors.Transparent);
        return pal;
    }

    private static List<SKColor> NewGetBitmapPalette(SKBitmap bitmap, SKColor fontColor)
    {
        var pal = new List<SKColor>(255);
        var lookup = new HashSet<SKColor>(255);
        var row = new SKColor[bitmap.Width];
        pal.Add(fontColor);
        lookup.Add(fontColor);

        for (var y = 0; y < bitmap.Height; y++)
        {
            ReadRow(bitmap, y, row);
            for (var x = 0; x < bitmap.Width; x++)
            {
                var c = row[x];
                if (c.Alpha > 0)
                {
                    if (!lookup.Contains(c))
                    {
                        pal.Add(c);
                        lookup.Add(c);
                    }

                    if (pal.Count >= 254)
                    {
                        break;
                    }
                }
            }

            if (pal.Count >= 254)
            {
                break;
            }
        }

        if (pal.Count < 254)
        {
            pal.Add(SKColors.Transparent);
            return pal;
        }

        pal = new List<SKColor>();
        lookup = new HashSet<SKColor>();
        pal.Add(fontColor);
        lookup.Add(fontColor);
        var rejected = new HashSet<SKColor>();
        for (var y = 0; y < bitmap.Height; y++)
        {
            ReadRow(bitmap, y, row);
            for (var x = 0; x < bitmap.Width; x++)
            {
                var c = row[x];
                if (c.Alpha > 0)
                {
                    if (lookup.Contains(c))
                    {
                    }
                    else if (rejected.Contains(c))
                    {
                    }
                    else if (pal.Count < 100)
                    {
                        if (!HasCloseColor(c, pal, 1))
                        {
                            pal.Add(c);
                            lookup.Add(c);
                        }
                        else
                        {
                            rejected.Add(c);
                        }
                    }
                    else if (pal.Count < 240)
                    {
                        if (!HasCloseColor(c, pal, 5))
                        {
                            pal.Add(c);
                            lookup.Add(c);
                        }
                        else
                        {
                            rejected.Add(c);
                        }
                    }
                    else if (pal.Count < 254)
                    {
                        if (!HasCloseColor(c, pal, 25))
                        {
                            pal.Add(c);
                            lookup.Add(c);
                        }
                        else
                        {
                            rejected.Add(c);
                        }
                    }
                }
            }

            if (pal.Count >= 254)
            {
                break;
            }
        }

        pal.Add(SKColors.Transparent);
        return pal;
    }

    // ================================================================== Blu-ray SUP RLE encode

    [Benchmark]
    public int BluRayEncodeImage_Old() => OldEncodeImage(_caption, _supPalette).Length;

    [Benchmark]
    public int BluRayEncodeImage_New() => NewEncodeImage(_caption, _supPalette).Length;

    private static byte FindBestMatch(SKColor color, List<SKColor> palette)
    {
        var smallestDiff = 1000;
        var smallestDiffIndex = -1;
        var max = palette.Count;
        for (var i = 0; i < max; i++)
        {
            var c = palette[i];
            var diff = Math.Abs(c.Alpha - color.Alpha) + Math.Abs(c.Red - color.Red) + Math.Abs(c.Green - color.Green) + Math.Abs(c.Blue - color.Blue);
            if (diff < smallestDiff)
            {
                smallestDiff = diff;
                smallestDiffIndex = i;
            }
        }

        return (byte)smallestDiffIndex;
    }

    private static byte[] EncodeImageCore(SKBitmap bm, List<SKColor> palette, bool memoize)
    {
        var lookup = new Dictionary<SKColor, int>();
        for (var i = 0; i < palette.Count; i++)
        {
            var color = palette[i];
            if (!lookup.ContainsKey(color))
            {
                lookup.Add(color, i);
            }
        }

        var transparentColor = (byte)palette[palette.Count - 1];
        var bytes = new List<byte>(bm.Width * 2);
        var row = new SKColor[bm.Width];
        for (var y = 0; y < bm.Height; y++)
        {
            ReadRow(bm, y, row);
            int x;
            int len;
            for (x = 0; x < bm.Width; x += len)
            {
                var c = row[x];

                byte color;
                if (c.Alpha == 0)
                {
                    color = transparentColor;
                }
                else if (lookup.TryGetValue(c, out var intC))
                {
                    color = (byte)intC;
                }
                else
                {
                    color = FindBestMatch(c, palette);
                    if (memoize && lookup.Count < 1 << 16)
                    {
                        lookup[c] = color;
                    }
                }

                for (len = 1; x + len < bm.Width; len++)
                {
                    if (row[x + len] != c)
                    {
                        break;
                    }
                }

                if (len <= 2 && color != 0)
                {
                    bytes.Add(color);
                    if (len == 2)
                    {
                        bytes.Add(color);
                    }
                }
                else
                {
                    if (len > 0x3fff)
                    {
                        len = 0x3fff;
                    }

                    bytes.Add(0);
                    if (color == 0 && len < 0x40)
                    {
                        bytes.Add((byte)len);
                    }
                    else if (color == 0)
                    {
                        bytes.Add((byte)(0x40 | (len >> 8)));
                        bytes.Add((byte)len);
                    }
                    else if (len < 0x40)
                    {
                        bytes.Add((byte)(0x80 | len));
                        bytes.Add(color);
                    }
                    else
                    {
                        bytes.Add((byte)(0xc0 | (len >> 8)));
                        bytes.Add((byte)len);
                        bytes.Add(color);
                    }
                }
            }

            if (x == bm.Width)
            {
                bytes.Add(0);
                bytes.Add(0);
            }
        }

        return bytes.ToArray();
    }

    private static byte[] OldEncodeImage(SKBitmap bm, List<SKColor> palette) => EncodeImageCore(bm, palette, memoize: false);

    private static byte[] NewEncodeImage(SKBitmap bm, List<SKColor> palette) => EncodeImageCore(bm, palette, memoize: true);

    // ================================================================== NikseBitmap four colours
    // New calls the shipping method; Old is the pre-round-17 byte-indexed loop with a
    // Buffer.BlockCopy per pixel.

    [Benchmark]
    public int ConvertToFourColors_Old() => OldConvertToFourColors(_fourColorSource, SKColors.Transparent, SKColors.White, SKColors.Black, false).Length;

    [Benchmark]
    public int ConvertToFourColors_New()
    {
        // Same one clone of the source pixels as the old copy does, so only the loop differs.
        var nb = new NikseBitmap(_caption.Width, _caption.Height, (byte[])_fourColorSource.Clone());
        nb.ConvertToFourColors(SKColors.Transparent, SKColors.White, SKColors.Black, false);
        return nb.GetPixelData().Length;
    }

    private static SKColor GetOutlineColor(SKColor borderColor)
    {
        if (borderColor.Red + borderColor.Green + borderColor.Blue < 30)
        {
            return new SKColor(75, 75, 75, 200);
        }

        return new SKColor(borderColor.Red, borderColor.Green, borderColor.Blue, 150);
    }

    private static byte[] OldConvertToFourColors(byte[] source, SKColor background, SKColor pattern, SKColor emphasis1, bool useInnerAntialize)
    {
        var bitmapData = (byte[])source.Clone();

        var backgroundBuffer = new byte[4];
        backgroundBuffer[0] = background.Blue;
        backgroundBuffer[1] = background.Green;
        backgroundBuffer[2] = background.Red;
        backgroundBuffer[3] = background.Alpha;

        var patternBuffer = new byte[4];
        patternBuffer[0] = pattern.Blue;
        patternBuffer[1] = pattern.Green;
        patternBuffer[2] = pattern.Red;
        patternBuffer[3] = pattern.Alpha;

        var emphasis1Buffer = new byte[4];
        emphasis1Buffer[0] = emphasis1.Blue;
        emphasis1Buffer[1] = emphasis1.Green;
        emphasis1Buffer[2] = emphasis1.Red;
        emphasis1Buffer[3] = emphasis1.Alpha;

        var emphasis2Buffer = new byte[4];
        var emphasis2 = GetOutlineColor(emphasis1);
        if (!useInnerAntialize)
        {
            emphasis2Buffer[0] = emphasis2.Blue;
            emphasis2Buffer[1] = emphasis2.Green;
            emphasis2Buffer[2] = emphasis2.Red;
            emphasis2Buffer[3] = emphasis2.Alpha;
        }

        for (var i = 0; i < bitmapData.Length; i += 4)
        {
            var smallestDiff = 10000;
            var buffer = backgroundBuffer;
            if (backgroundBuffer[3] == 0 && bitmapData[i + 3] < 10)
            {
            }
            else
            {
                var patternDiff = Math.Abs(patternBuffer[0] - bitmapData[i]) + Math.Abs(patternBuffer[1] - bitmapData[i + 1]) + Math.Abs(patternBuffer[2] - bitmapData[i + 2]) + Math.Abs(patternBuffer[3] - bitmapData[i + 3]);
                if (patternDiff < smallestDiff)
                {
                    smallestDiff = patternDiff;
                    buffer = patternBuffer;
                }

                var emphasis1Diff = Math.Abs(emphasis1Buffer[0] - bitmapData[i]) + Math.Abs(emphasis1Buffer[1] - bitmapData[i + 1]) + Math.Abs(emphasis1Buffer[2] - bitmapData[i + 2]) + Math.Abs(emphasis1Buffer[3] - bitmapData[i + 3]);
                if (useInnerAntialize)
                {
                    if (emphasis1Diff - 20 < smallestDiff)
                    {
                        buffer = emphasis1Buffer;
                    }
                }
                else
                {
                    if (emphasis1Diff < smallestDiff)
                    {
                        smallestDiff = emphasis1Diff;
                        buffer = emphasis1Buffer;
                    }

                    var emphasis2Diff = Math.Abs(emphasis2Buffer[0] - bitmapData[i]) + Math.Abs(emphasis2Buffer[1] - bitmapData[i + 1]) + Math.Abs(emphasis2Buffer[2] - bitmapData[i + 2]) + Math.Abs(emphasis2Buffer[3] - bitmapData[i + 3]);
                    if (emphasis2Diff < smallestDiff)
                    {
                        buffer = emphasis2Buffer;
                    }
                    else if (bitmapData[i + 3] >= 10 && bitmapData[i + 3] < 90)
                    {
                        buffer = emphasis2Buffer;
                    }
                }
            }

            Buffer.BlockCopy(buffer, 0, bitmapData, i, 4);
        }

        return bitmapData;
    }
}
