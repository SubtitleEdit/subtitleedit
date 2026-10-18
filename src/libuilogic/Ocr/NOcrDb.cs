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

    // Subtitle lines repeat the same glyph bitmaps over and over, so cache match results per
    // exact pixel content. Unmatched glyphs benefit the most: a "no match" previously re-ran the
    // whole pass cascade against the full database for every occurrence. The cache is cleared on
    // any mutation (Add/Remove/Load) and on Save, because the edit dialogs mutate the character
    // lists/lines directly and always call Save afterwards.
    private readonly Dictionary<MatchCacheKey, MatchCacheEntry> _matchCache = new();
    private const int MatchCacheMaxEntries = 5000;

    // Exact-match candidates bucketed by (width, height) so the exact pass doesn't scan the
    // whole single-character list per glyph. Rebuilt lazily after mutations; insertion order is
    // preserved within a bucket so the first-match-wins semantics are unchanged.
    private Dictionary<long, List<NOcrChar>>? _exactSizeIndex;

    private sealed class MatchCacheEntry
    {
        public NOcrChar? ExactMatch;
        public NOcrChar? SingleMatch;
    }

    private readonly struct MatchCacheKey : IEquatable<MatchCacheKey>
    {
        private readonly byte[] _pixels;
        private readonly int _width;
        private readonly int _topMargin;
        private readonly int _maxWrongPixels;
        private readonly bool _deepSeek;
        private readonly bool _lastDitch;
        private readonly double _italicFactor;
        private readonly BaselinePlacement _placement;
        private readonly int _hashCode;

        public MatchCacheKey(NikseBitmap2 bitmap, int topMargin, bool deepSeek, int maxWrongPixels, bool lastDitch, double italicFactor, BaselinePlacement placement)
        {
            _pixels = bitmap.GetPixelData().ToArray();
            _width = bitmap.Width;
            _topMargin = topMargin;
            _maxWrongPixels = maxWrongPixels;
            _deepSeek = deepSeek;
            _lastDitch = lastDitch;
            _italicFactor = italicFactor;
            _placement = placement;

            // FNV-1a over the pixel bytes plus the match parameters.
            var hash = unchecked((int)2166136261);
            foreach (var b in _pixels)
            {
                hash = unchecked((hash ^ b) * 16777619);
            }

            hash = unchecked((hash ^ _width) * 16777619);
            hash = unchecked((hash ^ _topMargin) * 16777619);
            hash = unchecked((hash ^ _maxWrongPixels) * 16777619);
            hash = unchecked((hash ^ (_deepSeek ? 1 : 0) ^ (_lastDitch ? 2 : 0)) * 16777619);
            hash = unchecked((hash ^ _italicFactor.GetHashCode()) * 16777619);
            hash = unchecked((hash ^ (int)_placement) * 16777619);
            _hashCode = hash;
        }

        public bool Equals(MatchCacheKey other)
        {
            return _hashCode == other._hashCode &&
                   _width == other._width &&
                   _topMargin == other._topMargin &&
                   _maxWrongPixels == other._maxWrongPixels &&
                   _deepSeek == other._deepSeek &&
                   _lastDitch == other._lastDitch &&
                   _italicFactor.Equals(other._italicFactor) &&
                   _placement == other._placement &&
                   _pixels.AsSpan().SequenceEqual(other._pixels);
        }

        public override bool Equals(object? obj) => obj is MatchCacheKey other && Equals(other);

        public override int GetHashCode() => _hashCode;
    }

    private void InvalidateCaches()
    {
        _matchCache.Clear();
        _exactSizeIndex = null;
    }

    public void Save()
    {
        lock (_lock)
        {
            var tempFileName = FileName + ".tmp";

            // Characters are written a byte at a time - buffered, so the deflater gets 64 KB
            // writes instead of ~2.5 million single-byte ones.
            using (var gz = new GZipStream(File.Create(tempFileName), CompressionMode.Compress))
            using (var buffered = new BufferedStream(gz, 1 << 16))
            {
                var versionBuffer = Encoding.ASCII.GetBytes(Version);
                buffered.Write(versionBuffer, 0, versionBuffer.Length);

                foreach (var ocrChar in OcrCharacters)
                {
                    ocrChar.Save(buffered);
                }

                foreach (var ocrChar in OcrCharactersExpanded)
                {
                    ocrChar.Save(buffered);
                }
            }

            File.Move(tempFileName, FileName, overwrite: true);

            // The edit dialogs mutate OcrCharacters/character lines directly and then call
            // Save, so treat Save as a mutation barrier for the caches.
            InvalidateCaches();
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
                InvalidateCaches();
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
            InvalidateCaches();
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

            InvalidateCaches();
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

            InvalidateCaches();
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
                IsExpandedLineMatch(oc, targetItem, nikseBitmap, list, listIndex))
            {
                var size = GetTotalSize(listIndex, list, oc.ExpandCount);
                if (Math.Abs(size.X - oc.Width) < 3 && Math.Abs(size.Y - oc.Height) < 3 &&
                    IsPlausibleGroup(oc, list, listIndex))
                {
                    return oc;
                }
            }
        }

        for (var i = 0; i < OcrCharactersExpanded.Count; i++)
        {
            var oc = OcrCharactersExpanded[i];
            if (oc.ExpandCount > 1 && oc.Width > w && targetItem.X + oc.Width < nikseBitmap.Width &&
                oc.LinesForeground.Count + oc.LinesBackground.Count >= MinLinesForExpandedMatch)
            {
                var size = GetTotalSize(listIndex, list, oc.ExpandCount);
                if (size.X <= 0 || size.Y <= 0)
                {
                    continue;
                }

                var heightToWidthPercent = size.Y * 100.0 / size.X;
                if (Math.Abs(heightToWidthPercent - oc.HeightToWidthPercent) < 15 &&
                    Math.Abs(size.X - oc.Width) < 25 && Math.Abs(size.Y - oc.Height) < 20 &&
                    IsScaleRatioSane(size.X, oc.Width) && IsScaleRatioSane(size.Y, oc.Height) &&
                    IsExpandedLineMatchScaled(oc, targetItem, nikseBitmap, size.X, size.Y, list, listIndex) &&
                    IsPlausibleGroup(oc, list, listIndex))
                {
                    return oc;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// An accented letter split into parts ("û" as "u" plus a separate "^") has every part but
    /// the letter itself above the letter's top. Two bars side by side ("ll") or a bar with an
    /// apostrophe next to its top ("I'") fit a slanted "û"/"ù" entry scaled onto them, but no
    /// part of those sits above the other.
    /// </summary>
    private static bool IsPlausibleGroup(NOcrChar oc, List<ImageSplitterItem2> list, int listIndex)
    {
        if (oc.Text.Length != 1 || !char.IsLetter(oc.Text[0]) ||
            oc.Text.Normalize(System.Text.NormalizationForm.FormD).Length < 2 ||
            listIndex < 0 || listIndex + oc.ExpandCount > list.Count)
        {
            return true;
        }

        var parts = list.GetRange(listIndex, oc.ExpandCount);
        if (parts.Any(p => p.NikseBitmap == null))
        {
            return true;
        }

        var letter = parts.MaxBy(p => p.NikseBitmap!.Height)!;
        var tolerance = Math.Max(2, letter.NikseBitmap!.Height / 6);
        return parts.All(p => p == letter || p.Top + p.NikseBitmap!.Height <= letter.Top + tolerance);
    }

    /// <summary>
    /// The scaled expanded pass may only bridge a moderate size difference between the trained
    /// character and the target group. Without this gate the absolute size deltas (±25/±20 px)
    /// let a tiny two-part glyph (a 10x12 double quote) be "scaled" onto a letter-pair group
    /// three to four times its size.
    /// </summary>
    private static bool IsScaleRatioSane(int targetSize, int charSize)
    {
        if (targetSize <= 0 || charSize <= 0)
        {
            return false;
        }

        var big = Math.Max(targetSize, charSize);
        var small = Math.Min(targetSize, charSize);
        return big * 2 <= small * 5; // ratio <= 2.5
    }

    /// <summary>
    /// Scaled expanded match against the actual bounding box of the target group. The old
    /// "scaled" pass walked the character's lines at the character's own size, so an expanded
    /// glyph (e.g. a two-part quote) rendered at a different size than it was trained at could
    /// essentially never match; the single-part fallback then turned every cross-size " into '.
    /// The error budget scales with the number of line points actually walked (~5%), NOT with
    /// the target area: measured legit scaled matches stay under ~2-3% wrong points, while a
    /// wrong expanded character claiming a group of ordinary neighboring letters (e.g. a
    /// trained "fi" pair swallowing "t"+"i") runs at 9%+. An area-based budget grows with the
    /// group being claimed, so it was loosest exactly for those letter-pair steals.
    /// </summary>
    /// <summary>
    /// The scaled pass stretches an entry over a group up to 25 px wider than it was drawn
    /// with, and tolerates a few misplaced points. An entry drawn with a handful of lines then
    /// fits almost any group of the right size: a shipped "%" with five lines claimed a dialog
    /// dash plus an opening quote, its lines all landing on the dash and one quote stroke. A
    /// match that claims a piece none of its foreground lines touches says nothing about that
    /// piece, so every claimed piece must be hit. (The exact pass keeps its own, stricter walk.)
    /// </summary>
    private static bool IsExpandedLineMatchScaled(NOcrChar oc, ImageSplitterItem2 targetItem, NikseBitmap2 nikseBitmap, int targetWidth, int targetHeight, List<ImageSplitterItem2> list, int listIndex)
    {
        var errors = 0;
        var points = 0;
        var scaledMarginTop = (int)Math.Round(oc.MarginTop * targetHeight / (double)Math.Max(1, oc.Height), MidpointRounding.AwayFromZero);
        var originY = targetItem.Y - scaledMarginTop;
        var pieceCount = Math.Min(oc.ExpandCount, list.Count - listIndex);
        Span<bool> touched = pieceCount <= 16 ? stackalloc bool[pieceCount] : new bool[pieceCount];

        foreach (var op in oc.LinesForeground)
        {
            foreach (var point in op.ScaledWalkPoints(oc, targetWidth, targetHeight))
            {
                points++;
                var p = new OcrPoint(point.X + targetItem.X, point.Y + originY);
                // Out-of-bounds foreground points can't be on text - count them as errors.
                var piece = p.X < 0 || p.Y < 0 || p.X >= nikseBitmap.Width || p.Y >= nikseBitmap.Height
                    ? -1
                    : GroupForegroundPiece(list, listIndex, oc.ExpandCount, p.X, p.Y);
                if (piece < 0)
                {
                    errors++;
                }
                else
                {
                    touched[piece] = true;
                }
            }
        }

        for (var i = 0; i < pieceCount; i++)
        {
            if (!touched[i] && list[listIndex + i].NikseBitmap != null)
            {
                return false;
            }
        }

        foreach (var op in oc.LinesBackground)
        {
            foreach (var point in op.ScaledWalkPoints(oc, targetWidth, targetHeight))
            {
                points++;
                var p = new OcrPoint(point.X + targetItem.X, point.Y + originY);
                // Out-of-bounds background points are definitionally "not on text" - fine.
                if (p.X < 0 || p.Y < 0 || p.X >= nikseBitmap.Width || p.Y >= nikseBitmap.Height)
                {
                    continue;
                }

                if (nikseBitmap.GetAlpha(p.X, p.Y) > 150)
                {
                    errors++;
                }
            }
        }

        return errors <= Math.Max(4, points / 20);
    }

    private static bool IsExpandedLineMatch(NOcrChar oc, ImageSplitterItem2 targetItem, NikseBitmap2 nikseBitmap, List<ImageSplitterItem2> list, int listIndex)
    {
        foreach (var op in oc.LinesForeground)
        {
            foreach (var point in op.WalkPoints())
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

                if (!IsGroupForeground(list, listIndex, oc.ExpandCount, p.X, p.Y))
                {
                    return false;
                }
            }
        }

        foreach (var op in oc.LinesBackground)
        {
            foreach (var point in op.WalkPoints())
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

    /// <summary>
    /// Foreground test for an expanded candidate: the parent-bitmap pixel at (x, y) counts only
    /// when it belongs to one of the splitter items the candidate would claim. Sampling the
    /// parent bitmap directly let ink of a neighbouring, already-matched glyph satisfy the
    /// candidate's lines - in italic "I'll" the apostrophe sits inside the x-range of the first
    /// "l", so a trained "'ll" still matched when the scan started at that "l", and the
    /// apostrophe came out twice (#14768). Every group item is a disjoint cut of the parent
    /// bitmap, so its own pixels are the group's ink.
    /// </summary>
    private static bool IsGroupForeground(List<ImageSplitterItem2> list, int listIndex, int count, int x, int y)
    {
        return GroupForegroundPiece(list, listIndex, count, x, y) >= 0;
    }

    /// <summary>Index (from the group start) of the claimed piece with ink at (x, y), or -1.</summary>
    private static int GroupForegroundPiece(List<ImageSplitterItem2> list, int listIndex, int count, int x, int y)
    {
        var end = Math.Min(list.Count, listIndex + count);
        for (var i = listIndex; i < end; i++)
        {
            var item = list[i];
            var bmp = item.NikseBitmap;
            if (bmp == null)
            {
                continue;
            }

            var localX = x - item.X;
            var localY = y - item.Y;
            if (localX >= 0 && localY >= 0 && localX < bmp.Width && localY < bmp.Height && bmp.GetAlpha(localX, localY) > 150)
            {
                return i - listIndex;
            }
        }

        return -1;
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

    /// <param name="italicFactor">
    /// When greater than zero, a glyph that matches nothing upright is de-slanted by this factor
    /// and matched again; a hit is returned as italic. Italic subtitles otherwise fail to match
    /// against upright characters. Pass 0 to disable.
    /// </param>
    public NOcrChar? GetMatch(NikseBitmap2 parentBitmap, List<ImageSplitterItem2> list, ImageSplitterItem2 item, int topMargin, bool deepSeek, int maxWrongPixels, bool lastDitch = false, double italicFactor = 0)
    {
        if (item.NikseBitmap == null)
        {
            return null;
        }

        var placement = GetBaselinePlacement(list, item);
        var match = GetMatch(parentBitmap, list, item, topMargin, deepSeek, maxWrongPixels, lastDitch, italicFactor, placement);
        if (placement == BaselinePlacement.OnBaseline && match is { Text: "„", ExpandCount: 2 })
        {
            // Both halves of a low quote are commas. A comma and a period side by side (",..")
            // fit a two-part "„" entry too - keep the first glyph on its own then.
            var index = list.IndexOf(item);
            if (index + 1 < list.Count && !HangsBelowBaseline(list[index + 1]))
            {
                match = GetMatchSingle(item.NikseBitmap, topMargin, deepSeek, maxWrongPixels, lastDitch, placement: placement);
            }
        }

        match ??= MatchUnplacedMark(item, placement);
        return PlaceMark(match, placement, item);
    }

    // Share of wrong points a comma/apostrophe shape may have when its position is ignored.
    private const double UnplacedMarkMaxRate = 0.15;

    // An upright match with a higher share of wrong points than this is challenged by the
    // straightened glyph (sheared by the italic factor and by 1.5 x it), which wins when its
    // share is at most two thirds of the upright one. Measured on 38 Blu-ray discs: a steep bold
    // italic went from "/f* accogd/ng fo plan" to "/t* accord/ng to plan" while upright discs
    // were byte-identical.
    private const double ItalicChallengeMinRate = 0.02;
    private const double ItalicChallengeRatio = 1.5;
    private const double ItalicChallengeSteeperShear = 1.5;

    /// <summary>
    /// A small mark that matched nothing, tried against the comma and apostrophe shapes without
    /// their trained position: a curly apostrophe is a raised comma, and comma entries sit at the
    /// baseline, so the top-margin gate kept them off it ("You*ll", "didn*t"). Where the mark
    /// sits then decides what it is (<see cref="PlaceMark"/>).
    /// </summary>
    private NOcrChar? MatchUnplacedMark(ImageSplitterItem2 item, BaselinePlacement placement)
    {
        if (placement == BaselinePlacement.Unknown || item.NikseBitmap is not { } bitmap ||
            item.Baseline is not { } baseline || bitmap.Height * 2 > baseline)
        {
            return null;
        }

        var (comma, commaRate) = FindBestByText(bitmap, ",", UnplacedMarkMaxRate);
        var (apostrophe, apostropheRate) = FindBestByText(bitmap, "'", UnplacedMarkMaxRate);
        return comma != null && (apostrophe == null || commaRate <= apostropheRate) ? comma : apostrophe;
    }

    /// <summary>
    /// The glyph reaches more than a twentieth of the line height below the baseline, like a
    /// comma's tail (a period sits on it).
    /// </summary>
    private static bool HangsBelowBaseline(ImageSplitterItem2 item) =>
        item.Baseline is { } baseline && item.NikseBitmap != null &&
        item.Top + item.NikseBitmap.Height - baseline > Math.Max(1, baseline / 20);

    private NOcrChar? GetMatch(NikseBitmap2 parentBitmap, List<ImageSplitterItem2> list, ImageSplitterItem2 item, int topMargin, bool deepSeek, int maxWrongPixels, bool lastDitch, double italicFactor, BaselinePlacement placement)
    {
        // The public GetMatch returns early for an item without a bitmap.
        var bitmap = item.NikseBitmap!;
        var key = new MatchCacheKey(bitmap, topMargin, deepSeek, maxWrongPixels, lastDitch, italicFactor, placement);
        MatchCacheEntry? cached;
        lock (_lock)
        {
            _matchCache.TryGetValue(key, out cached);
        }

        if (cached != null)
        {
            if (cached.ExactMatch != null)
            {
                return ComposeYery(cached.ExactMatch, list, item, deepSeek, maxWrongPixels, lastDitch);
            }

            // The expanded scan depends on the neighboring splitter items and the parent
            // bitmap, so it can't be cached per glyph bitmap - always evaluate it live.
            return GetMatchExpanded(parentBitmap, item, list.IndexOf(item), list) ?? ComposeYery(cached.SingleMatch, list, item, deepSeek, maxWrongPixels, lastDitch);
        }

        // A perfect single match (exact size, 0 errors) means the user has explicitly added an
        // entry for this bitmap, so prefer it over a possibly-greedy expanded match.
        var exactSingle = GetExactMatchSingle(bitmap, topMargin, placement);
        if (exactSingle != null)
        {
            CacheMatch(key, new MatchCacheEntry { ExactMatch = exactSingle });
            return ComposeYery(exactSingle, list, item, deepSeek, maxWrongPixels, lastDitch);
        }

        var expandedResult = GetMatchExpanded(parentBitmap, item, list.IndexOf(item), list);
        if (expandedResult != null)
        {
            // Nothing cached: the single-match result was never computed, and the expanded
            // result must not be cached (see above).
            return expandedResult;
        }

        // skipExactCheck: the exact scan already ran (and missed) above - without the flag,
        // GetMatchSingle re-scanned the whole single-character list a second time for every
        // glyph that wasn't an exact match.
        var single = GetMatchSingle(bitmap, topMargin, deepSeek, maxWrongPixels, lastDitch: false, skipExactCheck: true, placement, out var singleRate);

        // A slanted glyph often still fits some entry loosely - an italic "t" read as an italic
        // "f", "r" as "g", "y" as "x" - and a loose hit used to end the search before the
        // straightened glyph was ever tried. When the upright match is not a close one, match the
        // straightened glyph too and keep whichever fits clearly better.
        if (single != null && italicFactor > 0 && singleRate > ItalicChallengeMinRate &&
            bitmap.Width * bitmap.Height >= SmallGlyphAreaLimit)
        {
            // Slants differ between fonts: a bold italic sheared by the configured factor is
            // still half slanted, and a half-straightened "h" fits "b". Try a steeper shear too
            // and keep the better fit.
            NOcrChar? challenger = null;
            var challengerRate = double.MaxValue;
            foreach (var factor in new[] { italicFactor, italicFactor * ItalicChallengeSteeperShear })
            {
                var straightened = bitmap.UnItalic(factor);
                if (straightened.Width <= 0 || straightened.Height <= 0)
                {
                    continue;
                }

                var candidate = GetMatchSingle(straightened, topMargin, deepSeek, maxWrongPixels, lastDitch: false, skipExactCheck: false, placement, out var candidateRate);
                if (candidate != null && candidateRate < challengerRate)
                {
                    challenger = candidate;
                    challengerRate = candidateRate;
                }
            }

            // A mark may not replace a mark: a straightened comma is a period, which says
            // nothing about slant.
            if (challenger != null && challengerRate * ItalicChallengeRatio <= singleRate &&
                (single.Text.All(char.IsLetterOrDigit) || challenger.Text.All(char.IsLetterOrDigit)))
            {
                single = new NOcrChar(challenger) { Italic = true };
            }
        }

        // Two letters drawn touching ("av", "wt", "As") are one glyph that fits nothing. Read it
        // as two unless the last-ditch pass finds a single character that fits better than the
        // worse of the two halves: a touching "ay" beat a loose "U" ("alwUs"), while a "D" the
        // last-ditch pass read right must not turn into "77".
        if (single == null)
        {
            var pair = MatchTouchingLetters(bitmap, topMargin, maxWrongPixels, depth: 0, out var pairRate);
            NOcrChar? guess = null;
            var guessRate = double.MaxValue;
            if (lastDitch)
            {
                guess = GetMatchSingle(bitmap, topMargin, deepSeek, maxWrongPixels, true, true, placement, out guessRate);
            }

            single = pair != null && (guess == null || pairRate < guessRate) ? pair : guess;
        }

        if (single == null && italicFactor > 0)
        {
            // Nothing matched upright. Straighten the glyph and try once more - this is how an
            // italic 'l' stops being read as an 'i'. A hit is italic by construction, so flag a
            // copy; the database entry itself must stay upright. Two touching italic letters
            // ("fa", "is", "ts", "rs" in a bold italic) are cut along a vertical column too, which
            // on the slanted pair leaves ragged halves that fit nothing - so the straightened
            // glyph gets the touching-pair split as well.
            var unItalic = bitmap.UnItalic(italicFactor);
            if (unItalic.Width > 0 && unItalic.Height > 0)
            {
                var italicMatch = GetMatchSingle(unItalic, topMargin, deepSeek, maxWrongPixels, lastDitch, placement: placement)
                                  ?? MatchTouchingLetters(unItalic, topMargin, maxWrongPixels, depth: 0, out _, returnStrokeEnds: true);
                if (italicMatch != null)
                {
                    single = new NOcrChar(italicMatch) { Italic = true };
                }
            }
        }

        CacheMatch(key, new MatchCacheEntry { SingleMatch = single });
        return ComposeYery(single, list, item, deepSeek, maxWrongPixels, lastDitch);
    }

    /// <summary>
    /// Reads a glyph that matched nothing as two touching letters: cut it at one of the
    /// thinnest columns of its middle half, trim each side to its ink and match both strictly
    /// (no deep-seek or last-ditch guessing). One side may itself be a touching pair ("ryw").
    /// Only letters and digits count, so a stray mark can't make a split look good.
    /// </summary>
    private NOcrChar? MatchTouchingLetters(NikseBitmap2 bitmap, int topMargin, int maxWrongPixels, int depth, out double rate, bool returnStrokeEnds = false)
    {
        rate = double.MaxValue;
        var width = bitmap.Width;
        var height = bitmap.Height;
        if (width < 10 || width * 10 < height * 7)
        {
            return null;
        }

        var ink = new int[width];
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (bitmap.GetAlpha(x, y) > 150)
                {
                    ink[x]++;
                }
            }
        }

        var cuts = Enumerable.Range(width / 4, width / 2 + 1)
            .Where(x => x > 0 && x < width - 1)
            .OrderBy(x => ink[x])
            .ThenBy(x => Math.Abs(x - width / 2))
            .ToList();
        // Up to three cuts; for a straightened italic glyph (returnStrokeEnds) each is tried with
        // and without a stroke end given back to its letter (see ReturnStrokeEnds; a cut through
        // the dot of an "i" moves the dot's remainder too). The best fit over all of them wins
        // rather than the first that fits: the thinnest column of "fw" is inside the "w".
        var tried = new List<int>();
        NOcrChar? bestPair = null;
        var bestPairRate = double.MaxValue;
        foreach (var cut in cuts)
        {
            if (tried.Count == 3)
            {
                break;
            }

            if (tried.Any(t => Math.Abs(t - cut) <= 2))
            {
                continue;
            }

            tried.Add(cut);
            var plainLeft = bitmap.CopyRectangle(new NikseRectangle(0, 0, cut, height));
            var plainRight = bitmap.CopyRectangle(new NikseRectangle(cut + 1, 0, width - cut - 1, height));
            var returnedRight = returnStrokeEnds ? new NikseBitmap2(plainRight) : plainRight;
            var returnedLeft = returnStrokeEnds ? ReturnStrokeEnds(bitmap, cut, plainLeft, returnedRight) : plainLeft;
            var attempts = ReferenceEquals(returnedLeft, plainLeft)
                ? new[] { (plainLeft, plainRight) }
                : new[] { (returnedLeft, returnedRight), (plainLeft, plainRight) };
            foreach (var (leftHalf, rightHalf) in attempts)
            {
                var left = TrimToInk(leftHalf, out var leftTop);
                var right = TrimToInk(rightHalf, out var rightTop);
                if (left == null || right == null)
                {
                    continue;
                }

                var leftMatch = MatchLetterOrPair(left, topMargin + leftTop, maxWrongPixels, depth, out var leftRate, returnStrokeEnds);
                if (leftMatch == null || leftRate >= bestPairRate)
                {
                    continue;
                }

                var rightMatch = MatchLetterOrPair(right, topMargin + rightTop, maxWrongPixels, depth, out var rightRate, returnStrokeEnds);
                if (rightMatch == null ||
                    leftMatch.Text.All(char.IsDigit) != rightMatch.Text.All(char.IsDigit))
                {
                    // Touching letters or touching digits, never one of each: an "M" cut in two
                    // read as "b1" or "A4".
                    continue;
                }

                var attemptRate = Math.Max(leftRate, rightRate);
                if (attemptRate >= bestPairRate)
                {
                    continue;
                }

                // A "w" cut down its middle is two "v"s - it was one letter all along.
                var text = HarmonizeCase(leftMatch.Text, rightMatch.Text, left, right, leftTop, rightTop) switch
                {
                    "vv" => "w",
                    "VV" => "W",
                    var pair => pair,
                };

                bestPairRate = attemptRate;
                bestPair = new NOcrChar(text)
                {
                    Width = width,
                    Height = height,
                    MarginTop = topMargin,
                    Italic = leftMatch.Italic && rightMatch.Italic,
                };
            }
        }

        rate = bestPairRate;
        return bestPair;
    }

    // Letters whose capital is the same shape as the lowercase, only taller.
    private const string SameShapeCapitals = "CKOPSUVWXZ";

    /// <summary>
    /// Each half is matched on its own, so a lowercase "s" the height of its neighbour "r" can
    /// come back as a scaled "S" ("rS", "Vv"). Two halves of the same height and top are the
    /// same case: a capital is taller than an x-height letter next to it.
    /// </summary>
    internal static string HarmonizeCase(string leftText, string rightText, NikseBitmap2 left, NikseBitmap2 right, int leftTop, int rightTop)
    {
        if (leftText.Length != 1 || rightText.Length != 1)
        {
            return leftText + rightText;
        }

        var tolerance = Math.Max(2, Math.Max(left.Height, right.Height) / 8);
        var sameSize = Math.Abs(left.Height - right.Height) <= tolerance && Math.Abs(leftTop - rightTop) <= tolerance;
        if (!sameSize)
        {
            return leftText + rightText;
        }

        var l = leftText[0];
        var r = rightText[0];
        if (char.IsLower(l) && char.IsUpper(r) && SameShapeCapitals.Contains(r))
        {
            r = char.ToLowerInvariant(r);
        }
        else if (char.IsUpper(l) && char.IsLower(r) && SameShapeCapitals.Contains(l))
        {
            l = char.ToLowerInvariant(l);
        }

        return string.Concat(l, r);
    }

    /// <summary>
    /// The cut runs down a column, so the arm of an "f" or the crossbar of a "t" reaching over
    /// the next letter leaves its end on that letter's side ("fa": a stub above the "a" that
    /// reads as an accent, "Ià", while the "f" without its arm reads as "I"). A piece of ink in
    /// the right half that is not that half's main body, touches the cut, and continues through
    /// the cut column in the whole glyph is such a stroke end: it goes back to the left half.
    /// The dot of an "i" or an accent next to the cut does not continue through it and stays.
    /// </summary>
    internal static NikseBitmap2 ReturnStrokeEnds(NikseBitmap2 whole, int cut, NikseBitmap2 left, NikseBitmap2 right)
    {
        var width = right.Width;
        var height = right.Height;
        if (width <= 0 || height <= 0)
        {
            return left;
        }

        var label = new int[width * height];
        var sizes = new List<int> { 0 };
        var continues = new List<bool> { false };
        var maxX = new List<int> { 0 };
        var minY = new List<int> { 0 };
        var maxY = new List<int> { 0 };
        var stack = new Stack<int>();
        for (var start = 0; start < label.Length; start++)
        {
            if (label[start] != 0 || right.GetAlpha(start % width, start / width) <= 150)
            {
                continue;
            }

            var id = sizes.Count;
            sizes.Add(0);
            continues.Add(false);
            maxX.Add(0);
            minY.Add(height);
            maxY.Add(0);
            label[start] = id;
            stack.Push(start);
            while (stack.Count > 0)
            {
                var index = stack.Pop();
                var x = index % width;
                var y = index / width;
                sizes[id]++;
                maxX[id] = Math.Max(maxX[id], x);
                minY[id] = Math.Min(minY[id], y);
                maxY[id] = Math.Max(maxY[id], y);
                if (x == 0 && whole.GetAlpha(cut, y) > 150)
                {
                    continues[id] = true;
                }

                for (var dy = -1; dy <= 1; dy++)
                {
                    for (var dx = -1; dx <= 1; dx++)
                    {
                        var nx = x + dx;
                        var ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        {
                            continue;
                        }

                        var n = ny * width + nx;
                        if (label[n] == 0 && right.GetAlpha(nx, ny) > 150)
                        {
                            label[n] = id;
                            stack.Push(n);
                        }
                    }
                }
            }
        }

        if (sizes.Count <= 2)
        {
            return left; // one piece: nothing to give back
        }

        var main = 1;
        for (var id = 2; id < sizes.Count; id++)
        {
            if (sizes[id] > sizes[main])
            {
                main = id;
            }
        }

        // A stroke end is a sliver: no taller than a stroke is thick and far smaller than the
        // letter. The top curl of an "s" cut through its spine, or the middle of a "w", is
        // neither and stays where it is.
        var extent = 0;
        for (var id = 1; id < sizes.Count; id++)
        {
            if (id != main && continues[id] && (maxY[id] - minY[id] + 1) * 4 <= height && sizes[id] * 4 <= sizes[main])
            {
                extent = Math.Max(extent, maxX[id] + 1);
            }
            else
            {
                continues[id] = false;
            }
        }

        if (extent == 0)
        {
            return left;
        }

        // Widen the left half over the cut column and the stroke ends, and move them across.
        var widened = new NikseBitmap2(cut + 1 + extent, height);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < cut; x++)
            {
                var alpha = left.GetAlpha(x, y);
                if (alpha > 0)
                {
                    widened.SetPixel(x, y, left.GetPixel(x, y));
                }
            }
        }

        for (var index = 0; index < label.Length; index++)
        {
            var id = label[index];
            if (id == 0 || id == main || !continues[id])
            {
                continue;
            }

            var x = index % width;
            var y = index / width;
            widened.SetPixel(cut + 1 + x, y, right.GetPixel(x, y));
            right.SetAlpha(x, y, 0);
            if (x == 0 && whole.GetAlpha(cut, y) > 150)
            {
                widened.SetPixel(cut, y, whole.GetPixel(cut, y));
            }
        }

        return widened;
    }

    private NOcrChar? MatchLetterOrPair(NikseBitmap2 part, int topMargin, int maxWrongPixels, int depth, out double rate, bool returnStrokeEnds)
    {
        var match = GetMatchSingle(part, topMargin, false, maxWrongPixels, false, false, BaselinePlacement.Unknown, out rate);
        if (match != null)
        {
            return match.Text.All(char.IsLetterOrDigit) ? match : null;
        }

        rate = double.MaxValue;
        return depth == 0 ? MatchTouchingLetters(part, topMargin, maxWrongPixels, depth + 1, out rate, returnStrokeEnds) : null;
    }

    /// <summary>The part of <paramref name="bitmap"/> that has ink, and how many rows were cut off its top.</summary>
    private static NikseBitmap2? TrimToInk(NikseBitmap2 bitmap, out int top)
    {
        top = 0;
        int minX = bitmap.Width, minY = bitmap.Height, maxX = -1, maxY = -1;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetAlpha(x, y) > 150)
                {
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        // Too small to be a letter: a sliver of the neighbour's stroke.
        if (maxX < 0 || maxX - minX + 1 < 3 || maxY - minY + 1 < bitmap.Height / 3)
        {
            return null;
        }

        top = minY;
        return bitmap.CopyRectangle(new NikseRectangle(minX, minY, maxX - minX + 1, maxY - minY + 1));
    }

    /// <summary>
    /// "ы"/"Ы" is a soft sign plus a separate bar. The trained two-part entries only fit fonts
    /// with a similar gap and weight - in a heavy outlined font the pair is far wider than any
    /// trained one and came out as "ьІ". A soft sign directly followed (no space) by a dotless bar
    /// of the same height and top is that letter in any font: a capital "І" after a lowercase
    /// "ь" is taller, and a dotted "і"/"ї" is taller than its neighbours too. In heavy italic the
    /// soft sign half can read as a hard sign and the bar as "Ї" or "/" ("тъЇ", "мъ/"); neither
    /// "ъ" nor "ь" is ever followed by such a bar, so those count as well.
    /// </summary>
    private NOcrChar? ComposeYery(NOcrChar? softSign, List<ImageSplitterItem2> list, ImageSplitterItem2 item, bool deepSeek, int maxWrongPixels, bool lastDitch)
    {
        if (softSign?.Text is not ("ь" or "Ь" or "ъ" or "Ъ") || item.NikseBitmap == null)
        {
            return softSign;
        }

        var index = list.IndexOf(item);
        var next = index >= 0 && index + 1 < list.Count ? list[index + 1] : null;
        if (next?.NikseBitmap == null)
        {
            return softSign;
        }

        var height = item.NikseBitmap.Height;
        var tolerance = Math.Max(2, height / 8);
        if (Math.Abs(next.NikseBitmap.Height - height) > tolerance ||
            Math.Abs(next.Top - item.Top) > tolerance ||
            next.NikseBitmap.Width >= item.NikseBitmap.Width)
        {
            return softSign;
        }

        var bar = GetMatchSingle(next.NikseBitmap, next.Top, deepSeek, maxWrongPixels, lastDitch);
        if (bar?.Text is not ("І" or "і" or "Ї" or "ї" or "l" or "I" or "|" or "/" or "1"))
        {
            return softSign;
        }

        return new NOcrChar(softSign)
        {
            Text = softSign.Text is "ь" or "ъ" ? "ы" : "Ы",
            ExpandCount = 2,
        };
    }

    private void CacheMatch(MatchCacheKey key, MatchCacheEntry entry)
    {
        lock (_lock)
        {
            if (_matchCache.Count >= MatchCacheMaxEntries)
            {
                _matchCache.Clear();
            }

            _matchCache[key] = entry;
        }
    }

    private NOcrChar? GetExactMatchSingle(NikseBitmap2 bitmap, int topMargin, BaselinePlacement placement = BaselinePlacement.Unknown)
    {
        var index = _exactSizeIndex;
        if (index == null)
        {
            lock (_lock)
            {
                index = _exactSizeIndex;
                if (index == null)
                {
                    index = new Dictionary<long, List<NOcrChar>>();
                    foreach (var oc in OcrCharacters)
                    {
                        var k = ((long)oc.Width << 32) | (uint)oc.Height;
                        if (!index.TryGetValue(k, out var bucket))
                        {
                            bucket = new List<NOcrChar>();
                            index[k] = bucket;
                        }

                        bucket.Add(oc);
                    }

                    _exactSizeIndex = index;
                }
            }
        }

        if (!index.TryGetValue(((long)bitmap.Width << 32) | (uint)bitmap.Height, out var candidates))
        {
            return null;
        }

        foreach (var oc in candidates)
        {
            if (Math.Abs(oc.MarginTop - topMargin) < 5 && FitsBaselinePlacement(oc, placement) && IsMatch(bitmap, oc, 0))
            {
                return oc;
            }
        }

        return null;
    }

    public NOcrChar? GetMatchSingle(NikseBitmap2 bitmap, int topMargin, bool deepSeek, int maxWrongPixels, bool lastDitch = false, bool skipExactCheck = false, BaselinePlacement placement = BaselinePlacement.Unknown)
    {
        return GetMatchSingle(bitmap, topMargin, deepSeek, maxWrongPixels, lastDitch, skipExactCheck, placement, out _);
    }

    /// <param name="matchRate">The winner's share of wrong points (0 for an exact match, <see cref="double.MaxValue"/> for none).</param>
    private NOcrChar? GetMatchSingle(NikseBitmap2 bitmap, int topMargin, bool deepSeek, int maxWrongPixels, bool lastDitch, bool skipExactCheck, BaselinePlacement placement, out double matchRate)
    {
        matchRate = double.MaxValue;
        if (!skipExactCheck)
        {
            var exact = GetExactMatchSingle(bitmap, topMargin, placement);
            if (exact != null)
            {
                matchRate = 0;
                return exact;
            }
        }

        var heightToWidthPercent = bitmap.Height * 100.0 / bitmap.Width;

        // Scale the loose passes' error budgets with the glyph's pixel area: 20 wrong pixels is
        // nothing on a 25x35 letter but lets a ~50-pixel apostrophe, dash or dot match almost
        // anything (e.g. "-" was claimed by "." in the last-ditch pass). One error per 8 pixels
        // of area leaves normal letters unaffected and only tightens tiny glyphs; the floor of 8
        // keeps cross-size matching of small glyphs possible.
        var areaErrorCap = Math.Max(8, bitmap.Width * bitmap.Height / 8);
        var hasRowGap = HasGapOrNeck(bitmap);

        foreach (var pass in MatchPasses)
        {
            if (pass.RequireDeepSeek && !deepSeek)
            {
                continue;
            }

            if (pass.RequireLastDitch && !lastDitch)
            {
                continue;
            }

            if (maxWrongPixels < pass.MinAllowance)
            {
                continue;
            }

            var errorsAllowed = pass.ErrorsAllowed(maxWrongPixels);

            // Best-match within the pass: the first candidate that fits the error budget used to
            // win outright, so with generous budgets a mediocre early entry beat a near-perfect
            // later one. Now the candidate with the lowest share of wrong points wins; ties keep
            // list order (newest first), so a user's just-added character still takes precedence
            // over an equally-good older one. Ranking by share instead of by count matters once
            // entries differ in line count: a 100-line entry (from Train nOCR) used to beat a
            // 300-line one on a count that was low only because fewer points were checked, which
            // read "o" as a sparsely trained "ø" and "i" as "ì".
            NOcrChar? best = null;
            var bestRate = double.MaxValue;
            NOcrChar? bestUpper = null;
            var bestUpperRate = double.MaxValue;
            NOcrChar? bestLower = null;
            var bestLowerRate = double.MaxValue;
            foreach (var oc in OcrCharacters)
            {
                if (!PassFilter(bitmap, heightToWidthPercent, oc, topMargin, pass) || !FitsBaselinePlacement(oc, placement) ||
                    !hasRowGap && IsDottedOrAccentedI(oc.Text))
                {
                    continue;
                }

                // Sensitive characters (O/o/0, quotes, dashes, ...) get a tighter budget within
                // the same pass instead of being deferred to a later pass: excluding them
                // entirely let non-sensitive lookalikes (Q, C, .) claim their glyphs at up to
                // 20 errors before the sensitive candidate was ever considered.
                var candidateAllowed = oc.IsSensitive && pass.ErrorsAllowedSensitive != null
                    ? pass.ErrorsAllowedSensitive(maxWrongPixels)
                    : errorsAllowed;

                // The number of points walked grows with the glyph's size, so a character trained
                // at 20 px and matched at 40 px collects twice the errors for the same fit.
                var scale = bitmap.Height / (double)oc.Height;
                if (scale > 1)
                {
                    candidateAllowed = (int)Math.Round(candidateAllowed * scale);
                }

                var budget = Math.Min(candidateAllowed, areaErrorCap);
                if (best != null)
                {
                    // A candidate can only win with a lower share than the best so far, and its
                    // share is at least errors / (all its points), so stop counting once that
                    // bound reaches the best share.
                    var maxPoints = CountPointsUpperBound(oc, bitmap.Width, bitmap.Height);
                    budget = Math.Min(budget, (int)Math.Ceiling((bestRate + CaseTwinRateMargin) * maxPoints) - 1);
                }

                if (TryCountErrors(bitmap, oc, budget, out var errors, out var points) && points > 0)
                {
                    var rate = errors / (double)points;
                    if (IsCaseTwinLetter(oc.Text))
                    {
                        if (char.IsUpper(oc.Text[0]) && rate < bestUpperRate)
                        {
                            bestUpper = oc;
                            bestUpperRate = rate;
                        }
                        else if (char.IsLower(oc.Text[0]) && rate < bestLowerRate)
                        {
                            bestLower = oc;
                            bestLowerRate = rate;
                        }
                    }

                    if (rate < bestRate)
                    {
                        best = oc;
                        bestRate = rate;
                        if (errors == 0 && !IsCaseTwinLetter(oc.Text))
                        {
                            break;
                        }
                    }
                }
            }

            if (best != null && IsCaseTwinLetter(best.Text) && (bestUpper == null || bestLower == null))
            {
                // The strict passes gate on size, so the other case (smaller or bigger by
                // definition) is often not even a candidate in the pass that found this one.
                var twinText = char.IsUpper(best.Text[0]) ? best.Text.ToLowerInvariant() : best.Text.ToUpperInvariant();
                var (twin, twinRate) = FindBestByText(bitmap, twinText, bestRate + CaseTwinRateMargin);
                if (char.IsUpper(twinText[0]))
                {
                    (bestUpper, bestUpperRate) = (twin, twinRate);
                }
                else
                {
                    (bestLower, bestLowerRate) = (twin, twinRate);
                }
            }

            best = ResolveCaseTwin(best, bestRate, bestUpper, bestUpperRate, bestLower, bestLowerRate, bitmap.Height, topMargin);
            best = PreferUnaccented(bitmap, best, bestRate);

            if (best != null)
            {
                matchRate = bestRate;
                return best;
            }
        }

        return null;
    }

    /// <summary>
    /// Where a glyph's bottom is relative to its line's baseline: dots, commas and low quotes
    /// sit on it, dashes and raised quotes float above it. Pixels alone can't tell a heavy dot
    /// from a short dash (both a solid blob), and the loose passes' top-margin tolerance let a
    /// dash entry claim the period at the end of a line.
    /// </summary>
    public enum BaselinePlacement
    {
        Unknown,
        OnBaseline,
        AboveBaseline,
    }

    /// <summary>
    /// For a small mark in a line of text: <see cref="BaselinePlacement.OnBaseline"/> when its
    /// bottom is within a tenth of the line height of the baseline (or below it),
    /// <see cref="BaselinePlacement.AboveBaseline"/> when it is at least a fifth of the line height
    /// above it, otherwise unknown. Also unknown unless at least three glyphs in the line are half
    /// as tall again as this one - in a line of nothing but marks ("■ ■ ■") the median bottom is
    /// no baseline, and letters don't need the check.
    /// </summary>
    public static BaselinePlacement GetBaselinePlacement(List<ImageSplitterItem2> list, ImageSplitterItem2 item)
    {
        if (item.Baseline is not { } baseline || item.NikseBitmap == null || baseline <= 0)
        {
            return BaselinePlacement.Unknown;
        }

        var index = list.IndexOf(item);
        var start = index;
        while (start > 0 && list[start - 1].SpecialCharacter != Environment.NewLine)
        {
            start--;
        }

        var tallerGlyphs = 0;
        for (var i = start; i < list.Count && list[i].SpecialCharacter != Environment.NewLine; i++)
        {
            if (list[i].NikseBitmap is { } bitmap && bitmap.Height * 2 >= item.NikseBitmap.Height * 3)
            {
                tallerGlyphs++;
            }
        }

        if (index < 0 || tallerGlyphs < 3)
        {
            return BaselinePlacement.Unknown;
        }

        var gap = baseline - (item.Top + item.NikseBitmap.Height);
        if (gap <= Math.Max(2, baseline / 10))
        {
            return BaselinePlacement.OnBaseline;
        }

        if (gap >= Math.Max(3, baseline / 5))
        {
            return BaselinePlacement.AboveBaseline;
        }

        return BaselinePlacement.Unknown;
    }

    /// <summary>
    /// Marks whose shape has no lookalike at the other height: a dash or a degree sign never sits
    /// on the baseline, a period, ellipsis or underscore never floats above it.
    /// </summary>
    private static bool FitsBaselinePlacement(NOcrChar oc, BaselinePlacement placement) => placement switch
    {
        BaselinePlacement.OnBaseline => oc.Text is not ("-" or "‐" or "–" or "—" or "°" or "^"),
        BaselinePlacement.AboveBaseline => oc.Text is not ("." or "…" or "_"),
        _ => true,
    };

    /// <summary>
    /// Commas and quotes are the same shapes at different heights: a comma above the baseline
    /// is an apostrophe ("we,ve"), a raised quote on the baseline is a comma or a low quote. A
    /// "comma" that doesn't hang below the baseline and is about as wide as tall is a period -
    /// in heavy fonts a solid dot fits a comma entry scaled down.
    /// </summary>
    private static NOcrChar? PlaceMark(NOcrChar? match, BaselinePlacement placement, ImageSplitterItem2 item)
    {
        var text = (placement, match?.Text) switch
        {
            (BaselinePlacement.OnBaseline, ",") when !HangsBelowBaseline(item) &&
                                                     item.NikseBitmap!.Height * 2 <= item.NikseBitmap.Width * 3 => ".",
            (BaselinePlacement.AboveBaseline, ",") => "'",
            (BaselinePlacement.AboveBaseline, "„") => "\"",
            (BaselinePlacement.OnBaseline, "'" or "’" or "‘" or "`" or "´") => ",",
            (BaselinePlacement.OnBaseline, "\"" or "“" or "”") => "„",
            _ => null,
        };

        // The copy constructor doesn't carry ExpandCount - a two-part quote must still consume
        // both parts.
        return text == null ? match : new NOcrChar(match!) { Text = text, ExpandCount = match!.ExpandCount };
    }

    // Below this pixel area a glyph is a "small glyph" (dot, comma, dash, apostrophe, ...):
    // pixel evidence barely discriminates solid blobs, so shape gating must come from the
    // aspect ratio instead.
    private const int SmallGlyphAreaLimit = 150;
    private const double SmallGlyphMaxAspectRatio = 2.0;
    private const int SmallGlyphMaxAreaRatio = 5;

    private static bool PassFilter(NikseBitmap2 bitmap, double heightToWidthPercent, NOcrChar oc, int topMargin, in MatchPass pass)
    {
        // A small entry (dot, comma, apostrophe) carries only a few lines of evidence, and since
        // every gate below scales with the glyph, it fits a much bigger glyph once stretched
        // onto it: an apostrophe trained at 3x9 read the stem of a 8x22 "t" with zero errors.
        // Allow about twice the size per axis, as between a DVD and a Blu-ray rendering.
        var area = bitmap.Width * bitmap.Height;
        var ocArea = oc.Width * oc.Height;
        var smallArea = Math.Min(area, ocArea);
        if (smallArea < SmallGlyphAreaLimit && Math.Max(area, ocArea) > smallArea * SmallGlyphMaxAreaRatio)
        {
            return false;
        }

        if (bitmap.Width * bitmap.Height < SmallGlyphAreaLimit)
        {
            // Small glyphs: absolute h/w% deltas are useless here - a dot (aspect ~100) vs a
            // dash (~40) differ by 60 points while two apostrophes can differ by 100+ points,
            // and the wide passes have no aspect gate at all (which let "." claim "-" through
            // the 8px size-tolerance pass on 0 pixel errors, since a solid blob's lines all hit
            // a solid dash). Compare aspect as a ratio instead, in every pass.
            var big = Math.Max(heightToWidthPercent, oc.HeightToWidthPercent);
            var small = Math.Min(heightToWidthPercent, oc.HeightToWidthPercent);
            if (small <= 0 || big / small > SmallGlyphMaxAspectRatio)
            {
                return false;
            }
        }
        else
        {
            // Sensitive characters may use their own (typically looser) aspect gate - O/o/0
            // vary more in aspect between fonts than their pixel shapes suggest.
            var aspectMaxDelta = oc.IsSensitive && pass.AspectMaxDeltaSensitive != 0
                ? pass.AspectMaxDeltaSensitive
                : pass.AspectMaxDelta;
            // Relative difference, in percent of the narrower aspect: an absolute h/w-% delta
            // treated tall glyphs unfairly - "i" at 4x21 vs 6x43 is 525 vs 717, so a delta of 190
            // shut it out of every pass although the shapes are the same.
            var aspectDelta = Math.Abs(heightToWidthPercent - oc.HeightToWidthPercent) * 100.0 /
                              Math.Min(heightToWidthPercent, oc.HeightToWidthPercent);
            if (aspectMaxDelta != int.MaxValue && aspectDelta >= aspectMaxDelta)
            {
                return false;
            }
        }

        if (pass.SizeMaxDelta != int.MaxValue &&
            (Math.Abs(bitmap.Width - oc.Width) >= pass.SizeMaxDelta ||
             Math.Abs(bitmap.Height - oc.Height) >= pass.SizeMaxDelta))
        {
            return false;
        }

        // Top margin and its tolerance are pixels at the size the character was trained at; scale
        // both to the glyph being matched, or a "." trained at 30 px can never match one at 60 px
        // (expected top 18, actual 36), while an "8" passes for an "e" at a smaller size.
        var scale = bitmap.Height / (double)oc.Height;
        var expectedTop = oc.MarginTop * scale;
        var topTolerance = pass.MarginTopMaxDelta * Math.Max(1.0, scale);
        if (Math.Abs(expectedTop - topMargin) >= topTolerance)
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
        public bool RequireLastDitch { get; init; }  // run only when the caller opted in (batch convert)
        public int AspectMaxDelta { get; init; }     // int.MaxValue = no aspect-ratio check
        public int SizeMaxDelta { get; init; }       // int.MaxValue = no width/height check; otherwise applied to both
        public int MarginTopMaxDelta { get; init; }
        public int MinLineCount { get; init; }       // 0 = no line-count check
        public SensitivityFilter Sensitivity { get; init; }
        public Func<int, int> ErrorsAllowed { get; init; }
        public Func<int, int>? ErrorsAllowedSensitive { get; init; } // budget for IsSensitive candidates (null = same as ErrorsAllowed)
        public int AspectMaxDeltaSensitive { get; init; }            // aspect gate for IsSensitive candidates (0 = same as AspectMaxDelta)
    }

    private static readonly Func<int, int> ErrorsZero = _ => 0;
    private static readonly Func<int, int> ErrorsOne = _ => 1;
    private static readonly Func<int, int> ErrorsTen = _ => 10;
    private static readonly Func<int, int> ErrorsCappedAtThree = max => Math.Min(3, max);
    private static readonly Func<int, int> ErrorsCappedAtTwenty = max => Math.Min(20, max);
    private static readonly Func<int, int> ErrorsAsRequested = max => max;
    private static readonly Func<int, int> ErrorsAsRequestedDoubled = max => max * 2;

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
        // wide tolerance, requires many lines, errors capped at 20 - sensitive chars (O, o, 0,
        // quotes, dashes, ...) compete in the SAME ranking with a tighter budget (10) and a
        // looser aspect gate (30). They used to run in a separate later pass, which let
        // non-sensitive lookalikes (Q, C, .) claim their glyphs unopposed at up to 20 errors.
        new MatchPass
        {
            MinAllowance = 10,
            AspectMaxDelta = 20, SizeMaxDelta = int.MaxValue, MarginTopMaxDelta = 15,
            MinLineCount = 41,
            ErrorsAllowed = ErrorsCappedAtTwenty,
            ErrorsAllowedSensitive = ErrorsTen,
            AspectMaxDeltaSensitive = 30,
        },
        // deepSeek: very wide aspect, requires lots of lines, errors as requested
        new MatchPass
        {
            RequireDeepSeek = true,
            AspectMaxDelta = 60, SizeMaxDelta = int.MaxValue, MarginTopMaxDelta = 17,
            MinLineCount = 51,
            ErrorsAllowed = ErrorsAsRequested,
        },
        // last-ditch: opt-in from batch contexts where the user can't fix asterisks inline.
        // Drop the MinLineCount gate that blocks sparsely-trained DB characters and double the
        // error budget so we return a best-guess character instead of "*". Wrong guesses can
        // still be cleaned up by the "fix common OCR errors" and replace-list passes; an
        // asterisk blocks them entirely. (#10766)
        new MatchPass
        {
            RequireLastDitch = true,
            AspectMaxDelta = 80, SizeMaxDelta = int.MaxValue, MarginTopMaxDelta = 25,
            MinLineCount = 15,
            ErrorsAllowed = ErrorsAsRequestedDoubled,
        },
    };

    // Letters whose upper and lower case differ only in size ("o"/"O", "s"/"S", ...): scaled to
    // the glyph they fit equally well, so pixels alone can't pick the case. Cyrillic has many
    // more of them than Latin ("о"/"О", "т"/"Т", "и"/"И", ...).
    private const string CaseTwinLetters = "cCoOsSuUvVwWxXzZøØ" + NOcrCaseFixer.CyrillicCaseTwins;
    private const double CaseTwinRateMargin = 0.01;

    /// <summary>
    /// "i", "j" and their accented forms are a stem with a separate dot or accent above it, so the
    /// glyph has an empty row between them. A slanted italic "l" is one solid stroke and fitted
    /// an italic "ì"/"Ì" entry, whose grave continues the slant of the stem ("shouìders").
    /// </summary>
    private static bool IsDottedOrAccentedI(string text) =>
        text.Length == 1 && "iìíîïjĵÌÍÎÏĴ".Contains(text[0]);

    /// <summary>
    /// The glyph has a row between a top part and the rest that is empty, or a neck: at most 40%
    /// of the ink of both the widest row above it and the row below it. In rounded bold fonts the
    /// corners of the stem reach into the gap under the dot of "i", so the gap row isn't quite
    /// empty. A solid stroke ("l") keeps its width, and a "t" is wider at the crossbar than below.
    /// </summary>
    private static bool HasGapOrNeck(NikseBitmap2 bitmap)
    {
        var counts = new int[bitmap.Height];
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetAlpha(x, y) > 150)
                {
                    counts[y]++;
                }
            }
        }

        var maxAbove = counts.Length > 0 ? counts[0] : 0;
        for (var y = 1; y < counts.Length - 1; y++)
        {
            if (counts[y] == 0 ||
                counts[y] * 10 <= Math.Min(maxAbove, counts[y + 1]) * 4)
            {
                return true;
            }

            maxAbove = Math.Max(maxAbove, counts[y]);
        }

        return false;
    }

    // How much worse (share of wrong points) "i"/"I" may fit and still beat "ì"/"Ì".
    private const double UnaccentedRateMargin = 0.02;

    /// <summary>
    /// "ì"/"Ì" must clearly beat "i"/"I". Scaled onto the glyph's box they differ only in the
    /// shape of the mark on top, so the big round dot of a font the database wasn't trained on
    /// read as a grave ("thìs", "ìs") all through English text. Only the grave "i": it is rare
    /// (Italian, word-final), while "í", "Á", "Å", ... are everyday letters whose plain twin
    /// can fit about as well and must not win.
    /// </summary>
    private NOcrChar? PreferUnaccented(NikseBitmap2 bitmap, NOcrChar? best, double bestRate)
    {
        var plainText = best?.Text switch
        {
            "ì" => "i",
            "Ì" => "I",
            _ => null,
        };

        if (plainText == null)
        {
            return best;
        }

        var (plain, _) = FindBestByText(bitmap, plainText, bestRate + UnaccentedRateMargin);
        return plain ?? best;
    }

    private static bool IsCaseTwinLetter(string text) => text.Length == 1 && CaseTwinLetters.Contains(text[0]);

    /// <summary>
    /// When the winner is a case-twin letter and its other case scored within
    /// <see cref="CaseTwinRateMargin"/>, pick the case whose trained top margin, scaled to the
    /// glyph, is closer to where the glyph sits in its line. The case fixer does this from the
    /// heights seen so far, but has nothing to go on for the first lines of a file.
    /// </summary>
    private static NOcrChar? ResolveCaseTwin(NOcrChar? best, double bestRate, NOcrChar? upper, double upperRate, NOcrChar? lower, double lowerRate, int height, int topMargin)
    {
        if (best == null || !IsCaseTwinLetter(best.Text) || upper == null || lower == null ||
            !string.Equals(upper.Text, lower.Text, StringComparison.OrdinalIgnoreCase) ||
            Math.Max(upperRate, lowerRate) > bestRate + CaseTwinRateMargin)
        {
            return best;
        }

        double TopDistance(NOcrChar oc) => Math.Abs(oc.MarginTop * height / (double)oc.Height - topMargin);
        return TopDistance(upper) <= TopDistance(lower) ? upper : lower;
    }

    /// <summary>
    /// The entry with text <paramref name="text"/> and a similar aspect that fits
    /// <paramref name="bitmap"/> best, if its share of wrong points is below <paramref name="maxRate"/>.
    /// </summary>
    private (NOcrChar? Match, double Rate) FindBestByText(NikseBitmap2 bitmap, string text, double maxRate)
    {
        NOcrChar? best = null;
        var bestRate = maxRate;
        var heightToWidthPercent = bitmap.Height * 100.0 / bitmap.Width;
        foreach (var oc in OcrCharacters)
        {
            if (oc.Text != text ||
                Math.Abs(heightToWidthPercent - oc.HeightToWidthPercent) * 100.0 / Math.Min(heightToWidthPercent, oc.HeightToWidthPercent) >= 30)
            {
                continue;
            }

            var maxPoints = CountPointsUpperBound(oc, bitmap.Width, bitmap.Height);
            if (TryCountErrors(bitmap, oc, (int)Math.Ceiling(bestRate * maxPoints), out var errors, out var points) &&
                points > 0 && errors / (double)points <= bestRate)
            {
                best = oc;
                bestRate = errors / (double)points;
            }
        }

        return (best, bestRate);
    }

    /// <summary>
    /// Number of points <see cref="TryCountErrors"/> walks for <paramref name="oc"/> at the given
    /// size if none of them falls outside the bitmap.
    /// </summary>
    private static int CountPointsUpperBound(NOcrChar oc, int width, int height)
    {
        var points = 0;
        foreach (var op in oc.LinesForeground)
        {
            var start = op.GetScaledStart(oc, width, height);
            var end = op.GetScaledEnd(oc, width, height);
            points += Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y)) + 1;
        }

        foreach (var op in oc.LinesBackground)
        {
            var start = op.GetScaledStart(oc, width, height);
            var end = op.GetScaledEnd(oc, width, height);
            points += Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y)) + 1;
        }

        return points;
    }

    /// <summary>
    /// Like <see cref="IsMatch"/> but reports how many pixels disagreed, so callers can rank
    /// candidates that all fit the budget, and how many points were checked. Returns false (and
    /// unspecified counts) as soon as the budget is exceeded.
    /// </summary>
    private static bool TryCountErrors(NikseBitmap2 bitmap, NOcrChar oc, int errorsAllowed, out int errors, out int points)
    {
        errors = 0;
        points = 0;
        if (errorsAllowed < 0)
        {
            return false;
        }

        // Same zero-line guard as IsMatch: an entry with no lines would "match" anything.
        if (oc.LinesForeground.Count + oc.LinesBackground.Count < MinLinesForSingleMatch)
        {
            return false;
        }

        var width = bitmap.Width;
        var height = bitmap.Height;
        var pixelData = bitmap.GetPixelData();
        var widthX4 = width * 4;

        foreach (var op in oc.LinesForeground)
        {
            foreach (var point in op.ScaledWalkPoints(oc, width, height))
            {
                if ((uint)point.X < (uint)width && (uint)point.Y < (uint)height)
                {
                    points++;
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
            foreach (var point in op.ScaledWalkPoints(oc, width, height))
            {
                if ((uint)point.X < (uint)width && (uint)point.Y < (uint)height)
                {
                    points++;
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
            foreach (var point in op.ScaledWalkPoints(oc, width, height))
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
            foreach (var point in op.ScaledWalkPoints(oc, width, height))
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