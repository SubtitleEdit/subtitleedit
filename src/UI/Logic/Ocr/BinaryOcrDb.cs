using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Ocr;

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

    public static List<string> GetDatabases()
    {
        var files = Directory.GetFiles(Se.OcrFolder.TrimEnd(Path.DirectorySeparatorChar), "*" + Extension);
        return files
            .Select(p => Path.GetFileNameWithoutExtension(p) ?? string.Empty)
            .Where(p => !string.IsNullOrEmpty(p))
            .OrderBy(p => p)
            .ToList();
    }
}

