using System.IO.Compression;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

public class NOcrDb
{
    public string FileName { get; set; }
    public List<NOcrChar> OcrCharacters = new();
    public List<NOcrChar> OcrCharactersExpanded = new();

    public int TotalCharacterCount => OcrCharacters.Count + OcrCharactersExpanded.Count;

    private const string Version = "V2";

    // An expanded or single match with too few lines matches almost anything of similar
    // dimensions, so skip it. (Reject the trivial 0-lines case that would let any entry with
    // the right dimensions win against a properly-trained match.)
    private const int MinLinesForExpandedMatch = 1;
    private const int MinLinesForSingleMatch = 1;

    public NOcrDb(string fileName)
    {
        FileName = fileName;
        LoadOcrCharacters();
    }

    public NOcrDb(NOcrDb db, string fileName)
    {
        FileName = fileName;

        OcrCharacters = new List<NOcrChar>(db.OcrCharacters);
        OcrCharactersExpanded = new List<NOcrChar>(db.OcrCharactersExpanded);
    }

    public IEnumerable<NOcrChar> OcrCharactersCombined => OcrCharacters.Concat(OcrCharactersExpanded);

    private readonly Lock _lock = new();

    public void Save()
    {
        lock (_lock)
        {
            var tempFileName = FileName + ".tmp";

            using (var gz = new GZipStream(File.Create(tempFileName), CompressionMode.Compress))
            {
                var versionBuffer = Encoding.ASCII.GetBytes(Version);
                gz.Write(versionBuffer, 0, versionBuffer.Length);

                foreach (var ocrChar in OcrCharacters)
                {
                    ocrChar.Save(gz);
                }

                foreach (var ocrChar in OcrCharactersExpanded)
                {
                    ocrChar.Save(gz);
                }
            }

            File.Move(tempFileName, FileName, overwrite: true);
        }
    }

    public void LoadOcrCharacters()
    {
        lock (_lock)
        {
            var list = new List<NOcrChar>();
            var listExpanded = new List<NOcrChar>();

            if (!File.Exists(FileName))
            {
                OcrCharacters = list;
                OcrCharactersExpanded = listExpanded;
                return;
            }

            byte[] buffer;
            using (var stream = new MemoryStream())
            {
                using (var gz = new GZipStream(File.OpenRead(FileName), CompressionMode.Decompress))
                {
                    gz.CopyTo(stream);
                }

                buffer = stream.ToArray();
            }

            var versionBuffer = Encoding.ASCII.GetBytes(Version);
            var isVersion2 = buffer.Length >= versionBuffer.Length &&
                             buffer.AsSpan(0, versionBuffer.Length).SequenceEqual(versionBuffer);
            var position = isVersion2 ? versionBuffer.Length : 0;
            var done = false;
            while (!done)
            {
                var ocrChar = new NOcrChar(ref position, buffer, isVersion2);
                if (ocrChar.LoadedOk)
                {
                    if (ocrChar.ExpandCount > 0)
                    {
                        listExpanded.Add(ocrChar);
                    }
                    else
                    {
                        list.Add(ocrChar);
                    }
                }
                else
                {
                    done = true;
                }
            }

            OcrCharacters = list;
            OcrCharactersExpanded = listExpanded;
        }
    }

    public void Add(NOcrChar ocrChar)
    {
        lock (_lock)
        {
            if (ocrChar.ExpandCount > 0)
            {
                OcrCharactersExpanded.Insert(0, ocrChar);
            }
            else
            {
                OcrCharacters.Insert(0, ocrChar);
            }
        }
    }

    public void Remove(NOcrChar ocrChar)
    {
        lock (_lock)
        {
            if (ocrChar.ExpandCount > 0)
            {
                OcrCharactersExpanded.Remove(ocrChar);
            }
            else
            {
                OcrCharacters.Remove(ocrChar);
            }
        }
    }

    public NOcrChar? GetMatchExpanded(NikseBitmap2 nikseBitmap, ImageSplitterItem2 targetItem, int listIndex, List<ImageSplitterItem2> list)
    {
        if (targetItem.NikseBitmap == null)
        {
            return null;
        }

        var w = targetItem.NikseBitmap.Width;
        for (var i = 0; i < OcrCharactersExpanded.Count; i++)
        {
            var oc = OcrCharactersExpanded[i];
            if (oc.ExpandCount > 1 && oc.Width > w && targetItem.X + oc.Width < nikseBitmap.Width &&
                oc.LinesForeground.Count + oc.LinesBackground.Count >= MinLinesForExpandedMatch &&
                IsExpandedLineMatch(oc, targetItem, nikseBitmap, scaled: false))
            {
                var size = GetTotalSize(listIndex, list, oc.ExpandCount);
                if (Math.Abs(size.X - oc.Width) < 3 && Math.Abs(size.Y - oc.Height) < 3)
                {
                    return oc;
                }
            }
        }

        for (var i = 0; i < OcrCharactersExpanded.Count; i++)
        {
            var oc = OcrCharactersExpanded[i];
            if (oc.ExpandCount > 1 && oc.Width > w && targetItem.X + oc.Width < nikseBitmap.Width &&
                oc.LinesForeground.Count + oc.LinesBackground.Count >= MinLinesForExpandedMatch &&
                IsExpandedLineMatch(oc, targetItem, nikseBitmap, scaled: true))
            {
                var size = GetTotalSize(listIndex, list, oc.ExpandCount);
                var heightToWidthPercent = size.Y * 100.0 / size.X;
                if (Math.Abs(heightToWidthPercent - oc.HeightToWidthPercent) < 15 &&
                    Math.Abs(size.X - oc.Width) < 25 && Math.Abs(size.Y - oc.Height) < 20)
                {
                    return oc;
                }
            }
        }

        return null;
    }

    private static bool IsExpandedLineMatch(NOcrChar oc, ImageSplitterItem2 targetItem, NikseBitmap2 nikseBitmap, bool scaled)
    {
        foreach (var op in oc.LinesForeground)
        {
            var points = scaled ? op.ScaledGetPoints(oc, oc.Width, oc.Height - 1) : op.GetPoints();
            foreach (var point in points)
            {
                var p = new OcrPoint(point.X + targetItem.X, point.Y + targetItem.Y - oc.MarginTop);
                // A foreground line point that falls outside the parent bitmap can't possibly be
                // on text — reject the match. (Previously negative Y was silently skipped, which
                // let large-MarginTop expanded chars match by leaving the upper part of every
                // line unchecked.)
                if (p.X < 0 || p.Y < 0 || p.X >= nikseBitmap.Width || p.Y >= nikseBitmap.Height)
                {
                    return false;
                }

                if (nikseBitmap.GetAlpha(p.X, p.Y) <= 150)
                {
                    return false;
                }
            }
        }

        foreach (var op in oc.LinesBackground)
        {
            var points = scaled ? op.ScaledGetPoints(oc, oc.Width, oc.Height - 1) : op.GetPoints();
            foreach (var point in points)
            {
                var p = new OcrPoint(point.X + targetItem.X, point.Y + targetItem.Y - oc.MarginTop);
                // For background lines, points that fall outside the bitmap are definitionally
                // "not on text", so they pass; only in-bounds points that turn out to be on text
                // (alpha > 150) cause a failure.
                if (p.X < 0 || p.Y < 0 || p.X >= nikseBitmap.Width || p.Y >= nikseBitmap.Height)
                {
                    continue;
                }

                if (nikseBitmap.GetAlpha(p.X, p.Y) > 150)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static OcrPoint GetTotalSize(int listIndex, List<ImageSplitterItem2> items, int count)
    {
        if (listIndex + count > items.Count)
        {
            return new OcrPoint(-100, -100);
        }

        var minimumX = int.MaxValue;
        var maximumX = int.MinValue;
        var minimumY = int.MaxValue;
        var maximumY = int.MinValue;

        for (var idx = listIndex; idx < listIndex + count; idx++)
        {
            var item = items[idx];
            if (item.NikseBitmap == null)
            {
                return new OcrPoint(-100, -100);
            }

            if (item.Y < minimumY)
            {
                minimumY = item.Y;
            }

            if (item.Y + item.NikseBitmap.Height > maximumY)
            {
                maximumY = item.Y + item.NikseBitmap.Height;
            }

            if (item.X < minimumX)
            {
                minimumX = item.X;
            }

            if (item.X + item.NikseBitmap.Width > maximumX)
            {
                maximumX = item.X + item.NikseBitmap.Width;
            }
        }

        return new OcrPoint(maximumX - minimumX, maximumY - minimumY);
    }

    public NOcrChar? GetMatch(NikseBitmap2 parentBitmap, List<ImageSplitterItem2> list, ImageSplitterItem2 item, int topMargin, bool deepSeek, int maxWrongPixels)
    {
        if (item.NikseBitmap == null)
        {
            return null;
        }

        // A perfect single match (exact size, 0 errors) means the user has explicitly added an
        // entry for this bitmap, so prefer it over a possibly-greedy expanded match.
        var exactSingle = GetExactMatchSingle(item.NikseBitmap, topMargin);
        if (exactSingle != null)
        {
            return exactSingle;
        }

        var expandedResult = GetMatchExpanded(parentBitmap, item, list.IndexOf(item), list);
        if (expandedResult != null)
        {
            return expandedResult;
        }

        return GetMatchSingle(item.NikseBitmap, topMargin, deepSeek, maxWrongPixels);
    }

    private NOcrChar? GetExactMatchSingle(NikseBitmap2 bitmap, int topMargin)
    {
        foreach (var oc in OcrCharacters)
        {
            if (bitmap.Width == oc.Width && bitmap.Height == oc.Height && Math.Abs(oc.MarginTop - topMargin) < 5)
            {
                if (IsMatch(bitmap, oc, 0))
                {
                    return oc;
                }
            }
        }

        return null;
    }

    public NOcrChar? GetMatchSingle(NikseBitmap2 bitmap, int topMargin, bool deepSeek, int maxWrongPixels)
    {
        var exact = GetExactMatchSingle(bitmap, topMargin);
        if (exact != null)
        {
            return exact;
        }

        var heightToWidthPercent = bitmap.Height * 100.0 / bitmap.Width;

        foreach (var pass in MatchPasses)
        {
            if (pass.RequireDeepSeek && !deepSeek)
            {
                continue;
            }

            if (maxWrongPixels < pass.MinAllowance)
            {
                continue;
            }

            var errorsAllowed = pass.ErrorsAllowed(maxWrongPixels);

            foreach (var oc in OcrCharacters)
            {
                if (!PassFilter(bitmap, heightToWidthPercent, oc, topMargin, pass))
                {
                    continue;
                }

                if (IsMatch(bitmap, oc, errorsAllowed))
                {
                    return oc;
                }
            }
        }

        return null;
    }

    private static bool PassFilter(NikseBitmap2 bitmap, double heightToWidthPercent, NOcrChar oc, int topMargin, in MatchPass pass)
    {
        if (pass.AspectMaxDelta != int.MaxValue &&
            Math.Abs(heightToWidthPercent - oc.HeightToWidthPercent) >= pass.AspectMaxDelta)
        {
            return false;
        }

        if (pass.SizeMaxDelta != int.MaxValue &&
            (Math.Abs(bitmap.Width - oc.Width) >= pass.SizeMaxDelta ||
             Math.Abs(bitmap.Height - oc.Height) >= pass.SizeMaxDelta))
        {
            return false;
        }

        if (Math.Abs(oc.MarginTop - topMargin) >= pass.MarginTopMaxDelta)
        {
            return false;
        }

        if (pass.MinLineCount > 0 &&
            oc.LinesForeground.Count + oc.LinesBackground.Count < pass.MinLineCount)
        {
            return false;
        }

        return pass.Sensitivity switch
        {
            SensitivityFilter.OnlySensitive => oc.IsSensitive,
            SensitivityFilter.NotSensitive => !oc.IsSensitive,
            _ => true,
        };
    }

    private enum SensitivityFilter { Either, NotSensitive, OnlySensitive }

    /// <summary>
    /// One row in the match cascade run by <see cref="GetMatchSingle"/>. Earlier rows are stricter
    /// and run first; the function returns the first <see cref="NOcrChar"/> any row accepts.
    /// </summary>
    /// <remarks>
    /// Conditions are "fail if &gt;= delta" (equivalent to the original "&lt; delta" pass condition).
    /// Use <see cref="int.MaxValue"/> to disable a delta check entirely.
    /// </remarks>
    private readonly struct MatchPass
    {
        public int MinAllowance { get; init; }       // run only if maxWrongPixels >= this (0 = always)
        public bool RequireDeepSeek { get; init; }
        public int AspectMaxDelta { get; init; }     // int.MaxValue = no aspect-ratio check
        public int SizeMaxDelta { get; init; }       // int.MaxValue = no width/height check; otherwise applied to both
        public int MarginTopMaxDelta { get; init; }
        public int MinLineCount { get; init; }       // 0 = no line-count check
        public SensitivityFilter Sensitivity { get; init; }
        public Func<int, int> ErrorsAllowed { get; init; }
    }

    private static readonly Func<int, int> ErrorsZero = _ => 0;
    private static readonly Func<int, int> ErrorsOne = _ => 1;
    private static readonly Func<int, int> ErrorsTen = _ => 10;
    private static readonly Func<int, int> ErrorsCappedAtThree = max => Math.Min(3, max);
    private static readonly Func<int, int> ErrorsCappedAtTwenty = max => Math.Min(20, max);
    private static readonly Func<int, int> ErrorsAsRequested = max => max;

    private static readonly MatchPass[] MatchPasses =
    {
        // very accurate (size + aspect, 0 errors)
        new MatchPass
        {
            AspectMaxDelta = 15, SizeMaxDelta = 5, MarginTopMaxDelta = 5,
            ErrorsAllowed = ErrorsZero,
        },
        // 4-px size tolerance, 1 error
        new MatchPass
        {
            MinAllowance = 1,
            AspectMaxDelta = int.MaxValue, SizeMaxDelta = 4, MarginTopMaxDelta = 8,
            ErrorsAllowed = ErrorsOne,
        },
        // 8-px size tolerance, 1 error
        new MatchPass
        {
            MinAllowance = 1,
            AspectMaxDelta = int.MaxValue, SizeMaxDelta = 8, MarginTopMaxDelta = 8,
            ErrorsAllowed = ErrorsOne,
        },
        // aspect-only screen, errors capped at 3
        new MatchPass
        {
            MinAllowance = 2,
            AspectMaxDelta = 20, SizeMaxDelta = int.MaxValue, MarginTopMaxDelta = 15,
            ErrorsAllowed = ErrorsCappedAtThree,
        },
        // wide tolerance for not-sensitive chars, requires many lines, errors capped at 20
        new MatchPass
        {
            MinAllowance = 10,
            AspectMaxDelta = 20, SizeMaxDelta = int.MaxValue, MarginTopMaxDelta = 15,
            MinLineCount = 41, Sensitivity = SensitivityFilter.NotSensitive,
            ErrorsAllowed = ErrorsCappedAtTwenty,
        },
        // looser aspect for sensitive chars (O, o, 0, ...), 10 errors
        new MatchPass
        {
            MinAllowance = 10,
            AspectMaxDelta = 30, SizeMaxDelta = int.MaxValue, MarginTopMaxDelta = 15,
            MinLineCount = 41, Sensitivity = SensitivityFilter.OnlySensitive,
            ErrorsAllowed = ErrorsTen,
        },
        // deepSeek: very wide aspect, requires lots of lines, errors as requested
        new MatchPass
        {
            RequireDeepSeek = true,
            AspectMaxDelta = 60, SizeMaxDelta = int.MaxValue, MarginTopMaxDelta = 17,
            MinLineCount = 51,
            ErrorsAllowed = ErrorsAsRequested,
        },
    };

    public static NOcrChar MakeItalicNOcrChar(NOcrChar oldChar, int movePixelsLeft, double unItalicFactor)
    {
        var c = new NOcrChar();
        foreach (var op in oldChar.LinesForeground)
        {
            c.LinesForeground.Add(new NOcrLine(MakePointItalic(op.Start, oldChar.Height, movePixelsLeft, unItalicFactor), MakePointItalic(op.End, oldChar.Height, movePixelsLeft, unItalicFactor)));
        }

        foreach (var op in oldChar.LinesBackground)
        {
            c.LinesBackground.Add(new NOcrLine(MakePointItalic(op.Start, oldChar.Height, movePixelsLeft, unItalicFactor), MakePointItalic(op.End, oldChar.Height, movePixelsLeft, unItalicFactor)));
        }

        c.Text = oldChar.Text;
        c.Width = oldChar.Width;
        c.Height = oldChar.Height;
        c.MarginTop = oldChar.MarginTop;
        c.Italic = true;

        return c;
    }

    private static OcrPoint MakePointItalic(OcrPoint p, int height, int moveLeftPixels, double italicAngle)
    {
        return new OcrPoint((int)Math.Round(p.X + (height - p.Y) * italicAngle - moveLeftPixels), p.Y);
    }

    public static bool IsMatch(NikseBitmap2 bitmap, NOcrChar oc, int errorsAllowed)
    {
        // A zero-line entry would skip both foreach loops below and return true, matching any
        // bitmap with the right dimensions. Reject that here.
        if (oc.LinesForeground.Count + oc.LinesBackground.Count < MinLinesForSingleMatch)
        {
            return false;
        }

        var errors = 0;
        var width = bitmap.Width;
        var height = bitmap.Height;
        var pixelData = bitmap.GetPixelData();
        var widthX4 = width * 4;

        foreach (var op in oc.LinesForeground)
        {
            foreach (var point in op.ScaledGetPoints(oc, width, height))
            {
                if ((uint)point.X < (uint)width && (uint)point.Y < (uint)height)
                {
                    var a = pixelData[point.X * 4 + point.Y * widthX4 + 3];
                    if (a <= 150)
                    {
                        if (++errors > errorsAllowed)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        foreach (var op in oc.LinesBackground)
        {
            foreach (var point in op.ScaledGetPoints(oc, width, height))
            {
                if ((uint)point.X < (uint)width && (uint)point.Y < (uint)height)
                {
                    var a = pixelData[point.X * 4 + point.Y * widthX4 + 3];
                    if (a > 150)
                    {
                        if (++errors > errorsAllowed)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    public static List<string> GetDatabases(string ocrFolder)
    {
        if (string.IsNullOrEmpty(ocrFolder) || !Directory.Exists(ocrFolder))
        {
            return [];
        }

        return Directory
            .GetFiles(ocrFolder.TrimEnd(Path.DirectorySeparatorChar), "*.nocr")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(p => p)
            .ToList()!;
    }
}