using System.IO.Compression;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

public class BinaryOcrDb
{
    public const string Extension = ".db";
    public string FileName { get; }
    public List<BinaryOcrBitmap> CompareImages = new List<BinaryOcrBitmap>();
    public List<BinaryOcrBitmap> CompareImagesExpanded = new List<BinaryOcrBitmap>();

    public List<BinaryOcrBitmap> AllCompareImages => CompareImages.Concat(CompareImagesExpanded).ToList();

    public BinaryOcrDb(string fileName, bool loadCompareImages = true)
    {
        FileName = fileName;
        if (loadCompareImages)
        {
            LoadCompareImages();
        }
    }

    // Shallow copy: shares BinaryOcrBitmap references with `source` but gives the new instance
    // its own list objects. Intended for read-only matching across threads, where each thread
    // needs an independent CompareImages list (the matcher reorders it on hot hits).
    // Do NOT use the copy for Add/Save — that would mutate state shared with `source`.
    public BinaryOcrDb(BinaryOcrDb source)
    {
        FileName = source.FileName;
        CompareImages = new List<BinaryOcrBitmap>(source.CompareImages);
        CompareImagesExpanded = new List<BinaryOcrBitmap>(source.CompareImagesExpanded);
    }

    public void Save()
    {
        if (File.Exists(FileName))
        {
            File.Delete(FileName);
        }

        var compareImages = new List<BinaryOcrBitmap>(CompareImages);
        var compareImagesExpanded = new List<BinaryOcrBitmap>(CompareImagesExpanded);

        using Stream gz = new GZipStream(File.OpenWrite(FileName), CompressionMode.Compress);
        foreach (var bob in compareImages)
        {
            if (bob.ExpandCount > 0)
            {
                throw new Exception("Oops, expand image in CompareImages!");
            }

            bob.Save(gz);
        }
        foreach (var bob in compareImagesExpanded)
        {
            if (bob.ExpandCount == 0)
            {
                throw new Exception("Oops, not expanded image in CompareImagesExpanded!");
            }

            bob.Save(gz);
            if (bob.ExpandedList.Count != bob.ExpandCount - 1)
            {
                throw new Exception("BinaryOcrDb.Save: Expanded image should have " + (bob.ExpandCount - 1) + " sub images");
            }
            foreach (var expandedBob in bob.ExpandedList)
            {
                if (expandedBob.Text != null)
                {
                    throw new Exception("BinaryOcrDb.Save: sub image should have null text");
                }

                expandedBob.Save(gz);
            }
        }
    }

    private void LoadCompareImages()
    {
        var list = new List<BinaryOcrBitmap>();
        var expandList = new List<BinaryOcrBitmap>();

        if (!File.Exists(FileName))
        {
            CompareImages = list;
            return;
        }

        using var stream = new MemoryStream();
        using (var gz = new GZipStream(File.OpenRead(FileName), CompressionMode.Decompress))
        {
            gz.CopyTo(stream);
        }

        stream.Position = 0;
        bool done = false;
        while (!done)
        {
            var bob = new BinaryOcrBitmap(stream);
            if (bob.LoadedOk)
            {
                if (bob.ExpandCount > 0)
                {
                    expandList.Add(bob);
                    bob.ExpandedList = new List<BinaryOcrBitmap>();
                    for (int i = 1; i < bob.ExpandCount; i++)
                    {
                        var expandedBob = new BinaryOcrBitmap(stream);
                        if (expandedBob.LoadedOk)
                        {
                            if (expandedBob.Text != null)
                            {
                                throw new Exception("BinaryOcrDb.LoadCompareImages: sub image should have null text");
                            }

                            bob.ExpandedList.Add(expandedBob);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    list.Add(bob);
                }
            }
            else
            {
                done = true;
            }
        }

        CompareImages = list;
        CompareImagesExpanded = expandList;
    }

    private const int MaxCommaQuoteTopDiff = 15;

    public static bool AllowEqual(BinaryOcrBitmap match, BinaryOcrBitmap newBob)
    {
        return match.Text == null || (match.Text != "," && match.Text != "'") ||
               Math.Abs(match.Y - newBob.Y) <= MaxCommaQuoteTopDiff;
    }

    // The matcher asks for "every compare image of exactly this size" up to 44 times per glyph
    // (11 size variants x 4 nudged targets). CompareImages is a public list that the UI adds to
    // and removes from, so the index is checked against the list on every use and rebuilt when
    // the list no longer looks like the one it was built from. Buckets keep list order - the
    // matcher's "first best match wins" depends on it - and are never modified once published,
    // so a thread that is still walking one is not disturbed by a rebuild (batch convert shares
    // the nOCR fallback database between its workers).
    private sealed class SizeIndex
    {
        public readonly Dictionary<long, List<BinaryOcrBitmap>> BySize = new Dictionary<long, List<BinaryOcrBitmap>>();
        public readonly List<BinaryOcrBitmap> Source;
        public readonly int Count;
        public readonly BinaryOcrBitmap? First;
        public readonly BinaryOcrBitmap? Last;

        public SizeIndex(List<BinaryOcrBitmap> list)
        {
            Source = list;
            Count = list.Count;
            First = list.Count > 0 ? list[0] : null;
            Last = list.Count > 0 ? list[list.Count - 1] : null;
            foreach (var b in list)
            {
                var key = SizeKey(b.Width, b.Height);
                if (!BySize.TryGetValue(key, out var bucket))
                {
                    bucket = new List<BinaryOcrBitmap>();
                    BySize.Add(key, bucket);
                }

                bucket.Add(b);
            }
        }

        public bool IsBuiltFrom(List<BinaryOcrBitmap> list)
        {
            return ReferenceEquals(Source, list) &&
                   Count == list.Count &&
                   (Count == 0 || (ReferenceEquals(First, list[0]) && ReferenceEquals(Last, list[list.Count - 1])));
        }
    }

    private SizeIndex? _sizeIndex;
    private readonly object _sizeIndexLock = new object();
    private static readonly List<BinaryOcrBitmap> EmptyBucket = new List<BinaryOcrBitmap>();

    private static long SizeKey(int width, int height) => ((long)width << 32) | (uint)height;

    /// <summary>
    /// The compare images with exactly this size, in <see cref="CompareImages"/> order.
    /// The returned list belongs to the index - do not modify it.
    /// </summary>
    public List<BinaryOcrBitmap> GetCompareImagesBySize(int width, int height)
    {
        SizeIndex index;
        lock (_sizeIndexLock)
        {
            var list = CompareImages;
            if (_sizeIndex == null || !_sizeIndex.IsBuiltFrom(list))
            {
                _sizeIndex = new SizeIndex(list);
            }

            index = _sizeIndex;
        }

        return index.BySize.TryGetValue(SizeKey(width, height), out var result) ? result : EmptyBucket;
    }

    /// <summary>
    /// Same pick as <see cref="FindExactMatch"/> (first match in list order), without the scan of
    /// the whole list - for callers that want the image, not its position.
    /// </summary>
    public BinaryOcrBitmap? FindExactMatchItem(BinaryOcrBitmap bob)
    {
        var bobHash = bob.Hash;
        foreach (var b in GetCompareImagesBySize(bob.Width, bob.Height))
        {
            if (bobHash == b.Hash && bob.NumberOfColoredPixels == b.NumberOfColoredPixels && AllowEqual(b, bob))
            {
                return b;
            }
        }

        return null;
    }

    /// <summary>Moves a hot compare image to the start of <see cref="CompareImages"/>.</summary>
    public void MoveToFront(BinaryOcrBitmap item)
    {
        lock (_sizeIndexLock)
        {
            if (CompareImages.Remove(item))
            {
                CompareImages.Insert(0, item);
                _sizeIndex = null; // the order inside the item's bucket changed
            }
        }
    }

    public int FindExactMatch(BinaryOcrBitmap bob)
    {
        var bobHash = bob.Hash;
        for (var i = 0; i < CompareImages.Count; i++)
        {
            var b = CompareImages[i];
            if (bobHash == b.Hash && bob.Width == b.Width && bob.Height == b.Height && bob.NumberOfColoredPixels == b.NumberOfColoredPixels)
            {
                if (AllowEqual(b, bob))
                {
                    return i;
                }
            }
        }
        
        return -1;
    }

    public int FindExactMatchExpanded(BinaryOcrBitmap bob)
    {
        for (var i = 0; i < CompareImagesExpanded.Count; i++)
        {
            var b = CompareImagesExpanded[i];
            if (bob.Hash == b.Hash &&
                bob.Width == b.Width &&
                bob.Height == b.Height &&
                bob.NumberOfColoredPixels == b.NumberOfColoredPixels &&
                bob.ExpandCount == b.ExpandCount &&
                bob.AreColorsEqual(b))
            {
                bool ok = true;
                for (int k = 0; k < b.ExpandedList.Count; k++)
                {
                    if (bob.ExpandedList[k].Hash != b.ExpandedList[k].Hash ||
                        !bob.ExpandedList[k].AreColorsEqual(b.ExpandedList[k])) // expanded images
                    {
                        ok = false;
                    }
                }
                if (ok)
                {
                    return i;
                }
            }

        }
        
        return -1;
    }

    public int Add(BinaryOcrBitmap bob)
    {
        int index;
        if (bob.ExpandCount > 0)
        {
            if (bob.ExpandedList == null || bob.ExpandCount - 1 != bob.ExpandedList.Count)
            {
                throw new Exception("BinaryOcrDb.Add: There should be " + (bob.ExpandCount - 1) + " sub image(s)");
            }

            if (bob.ExpandedList[0].Text != null)
            {
                throw new Exception("BinaryOcrDb.Add: sub image should have null text");
            }

            index = FindExactMatchExpanded(bob);
            if (index == -1 || CompareImagesExpanded[index].ExpandCount != bob.ExpandCount)
            {
                CompareImagesExpanded.Add(bob);
            }
            else
            {
                var allAlike = true;
                for (var i = 0; i < bob.ExpandCount - 1; i++)
                {
                    if (bob.ExpandedList[i].Hash != CompareImagesExpanded[index].ExpandedList[i].Hash)
                    {
                        allAlike = false;
                    }

                    if (bob.ExpandedList[i].Text != null)
                    {
                        throw new Exception("BinaryOcrDb.Add: sub image should have null text");
                    }
                }
                
                if (!allAlike)
                {
                    CompareImagesExpanded.Add(bob);
                }
                else
                {
                    throw new Exception("BinaryOcrDb.Add: Expanded image already in db!");
                }
            }
        }
        else
        {
            index = FindExactMatch(bob);
            if (index == -1)
            {
                CompareImages.Add(bob);
            }
            else
            {
                throw new Exception("BinaryOcrDb.Add: Image already in db!");
            }
        }
        
        return index;
    }

    public static List<string> GetDatabases(string ocrFolder)
    {
        if (string.IsNullOrEmpty(ocrFolder) || !Directory.Exists(ocrFolder))
        {
            return [];
        }
        var files = Directory.GetFiles(ocrFolder.TrimEnd(Path.DirectorySeparatorChar), "*" + Extension);
        return files
            .Select(p => Path.GetFileNameWithoutExtension(p) ?? string.Empty)
            .Where(p => !string.IsNullOrEmpty(p))
            .OrderBy(p => p)
            .ToList();
    }
}

