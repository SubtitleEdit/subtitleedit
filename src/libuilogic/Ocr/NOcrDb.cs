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
        private readonly int _hashCode;

        public MatchCacheKey(NikseBitmap2 bitmap, int topMargin, bool deepSeek, int maxWrongPixels, bool lastDitch)
        {
            _pixels = bitmap.GetPixelData().ToArray();
            _width = bitmap.Width;
            _topMargin = topMargin;
            _maxWrongPixels = maxWrongPixels;
            _deepSeek = deepSeek;
            _lastDitch = lastDitch;

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
                IsExpandedLineMatch(oc, targetItem, nikseBitmap))
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
                    IsExpandedLineMatchScaled(oc, targetItem, nikseBitmap, size.X, size.Y))
                {
                    return oc;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Scaled expanded match against the actual bounding box of the target group. The old
    /// "scaled" pass walked the character's lines at the character's own size, so an expanded
    /// glyph (e.g. a two-part quote) rendered at a different size than it was trained at could
    /// essentially never match; the single-part fallback then turned every cross-size " into '.
    /// A small area-scaled error budget absorbs antialiasing differences.
    /// </summary>
    private static bool IsExpandedLineMatchScaled(NOcrChar oc, ImageSplitterItem2 targetItem, NikseBitmap2 nikseBitmap, int targetWidth, int targetHeight)
    {
        var errorsAllowed = Math.Max(4, targetWidth * targetHeight / 16);
        var errors = 0;
        var scaledMarginTop = (int)Math.Round(oc.MarginTop * targetHeight / (double)Math.Max(1, oc.Height), MidpointRounding.AwayFromZero);
        var originY = targetItem.Y - scaledMarginTop;

        foreach (var op in oc.LinesForeground)
        {
            foreach (var point in op.ScaledWalkPoints(oc, targetWidth, targetHeight))
            {
                var p = new OcrPoint(point.X + targetItem.X, point.Y + originY);
                // Out-of-bounds foreground points can't be on text - count them as errors.
                if (p.X < 0 || p.Y < 0 || p.X >= nikseBitmap.Width || p.Y >= nikseBitmap.Height ||
                    nikseBitmap.GetAlpha(p.X, p.Y) <= 150)
                {
                    if (++errors > errorsAllowed)
                    {
                        return false;
                    }
                }
            }
        }

        foreach (var op in oc.LinesBackground)
        {
            foreach (var point in op.ScaledWalkPoints(oc, targetWidth, targetHeight))
            {
                var p = new OcrPoint(point.X + targetItem.X, point.Y + originY);
                // Out-of-bounds background points are definitionally "not on text" - fine.
                if (p.X < 0 || p.Y < 0 || p.X >= nikseBitmap.Width || p.Y >= nikseBitmap.Height)
                {
                    continue;
                }

                if (nikseBitmap.GetAlpha(p.X, p.Y) > 150)
                {
                    if (++errors > errorsAllowed)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static bool IsExpandedLineMatch(NOcrChar oc, ImageSplitterItem2 targetItem, NikseBitmap2 nikseBitmap)
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

                if (nikseBitmap.GetAlpha(p.X, p.Y) <= 150)
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

    public NOcrChar? GetMatch(NikseBitmap2 parentBitmap, List<ImageSplitterItem2> list, ImageSplitterItem2 item, int topMargin, bool deepSeek, int maxWrongPixels, bool lastDitch = false)
    {
        if (item.NikseBitmap == null)
        {
            return null;
        }

        var key = new MatchCacheKey(item.NikseBitmap, topMargin, deepSeek, maxWrongPixels, lastDitch);
        MatchCacheEntry? cached;
        lock (_lock)
        {
            _matchCache.TryGetValue(key, out cached);
        }

        if (cached != null)
        {
            if (cached.ExactMatch != null)
            {
                return cached.ExactMatch;
            }

            // The expanded scan depends on the neighboring splitter items and the parent
            // bitmap, so it can't be cached per glyph bitmap - always evaluate it live.
            return GetMatchExpanded(parentBitmap, item, list.IndexOf(item), list) ?? cached.SingleMatch;
        }

        // A perfect single match (exact size, 0 errors) means the user has explicitly added an
        // entry for this bitmap, so prefer it over a possibly-greedy expanded match.
        var exactSingle = GetExactMatchSingle(item.NikseBitmap, topMargin);
        if (exactSingle != null)
        {
            CacheMatch(key, new MatchCacheEntry { ExactMatch = exactSingle });
            return exactSingle;
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
        var single = GetMatchSingle(item.NikseBitmap, topMargin, deepSeek, maxWrongPixels, lastDitch, skipExactCheck: true);
        CacheMatch(key, new MatchCacheEntry { SingleMatch = single });
        return single;
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

    private NOcrChar? GetExactMatchSingle(NikseBitmap2 bitmap, int topMargin)
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
            if (Math.Abs(oc.MarginTop - topMargin) < 5 && IsMatch(bitmap, oc, 0))
            {
                return oc;
            }
        }

        return null;
    }

    public NOcrChar? GetMatchSingle(NikseBitmap2 bitmap, int topMargin, bool deepSeek, int maxWrongPixels, bool lastDitch = false, bool skipExactCheck = false)
    {
        if (!skipExactCheck)
        {
            var exact = GetExactMatchSingle(bitmap, topMargin);
            if (exact != null)
            {
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
            // later one. Now the candidate with the fewest wrong pixels wins; ties keep list
            // order (newest first), so a user's just-added character still takes precedence over
            // an equally-good older one. The counting loop shrinks its budget to "current best
            // minus one", so each candidate aborts as soon as it can no longer win.
            NOcrChar? best = null;
            var bestErrors = int.MaxValue;
            foreach (var oc in OcrCharacters)
            {
                if (!PassFilter(bitmap, heightToWidthPercent, oc, topMargin, pass))
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

                var budget = Math.Min(Math.Min(candidateAllowed, areaErrorCap), bestErrors - 1);
                if (TryCountErrors(bitmap, oc, budget, out var errors))
                {
                    best = oc;
                    bestErrors = errors;
                    if (errors == 0)
                    {
                        break;
                    }
                }
            }

            if (best != null)
            {
                return best;
            }
        }

        return null;
    }

    // Below this pixel area a glyph is a "small glyph" (dot, comma, dash, apostrophe, ...):
    // pixel evidence barely discriminates solid blobs, so shape gating must come from the
    // aspect ratio instead.
    private const int SmallGlyphAreaLimit = 150;
    private const double SmallGlyphMaxAspectRatio = 2.0;

    private static bool PassFilter(NikseBitmap2 bitmap, double heightToWidthPercent, NOcrChar oc, int topMargin, in MatchPass pass)
    {
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
            if (aspectMaxDelta != int.MaxValue &&
                Math.Abs(heightToWidthPercent - oc.HeightToWidthPercent) >= aspectMaxDelta)
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

    /// <summary>
    /// Like <see cref="IsMatch"/> but reports how many pixels disagreed, so callers can rank
    /// candidates that all fit the budget. Returns false (and an unspecified count) as soon as
    /// the budget is exceeded.
    /// </summary>
    private static bool TryCountErrors(NikseBitmap2 bitmap, NOcrChar oc, int errorsAllowed, out int errors)
    {
        errors = 0;
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