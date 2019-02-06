using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.Ocr.Binary
{
    public class BinaryOcrDb
    {
        public string FileName { get; }
        public List<BinaryOcrBitmap> CompareImages = new List<BinaryOcrBitmap>();
        public List<BinaryOcrBitmap> CompareImagesExpanded = new List<BinaryOcrBitmap>();

        public BinaryOcrDb(string fileName)
        {
            FileName = fileName;
        }

        public BinaryOcrDb(string fileName, bool loadCompareImages)
        {
            FileName = fileName;
            if (loadCompareImages)
            {
                LoadCompareImages();
            }
        }

        public void Save()
        {
            using (Stream gz = new GZipStream(File.OpenWrite(FileName), CompressionMode.Compress))
            {
                foreach (var bob in CompareImages)
                {
                    if (bob.ExpandCount > 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Ups, expand image in CompareImages!");
                    }

                    bob.Save(gz);
                }
                foreach (var bob in CompareImagesExpanded)
                {
                    if (bob.ExpandCount == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Ups, not expanded image in CompareImagesExpanded!");
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
        }

        public void LoadCompareImages()
        {
            var list = new List<BinaryOcrBitmap>();
            var expandList = new List<BinaryOcrBitmap>();

            if (!File.Exists(FileName))
            {
                CompareImages = list;
                return;
            }

            using (Stream gz = new GZipStream(File.OpenRead(FileName), CompressionMode.Decompress))
            {
                bool done = false;
                while (!done)
                {
                    var bob = new BinaryOcrBitmap(gz);
                    if (bob.LoadedOk)
                    {
                        if (bob.ExpandCount > 0)
                        {
                            expandList.Add(bob);
                            bob.ExpandedList = new List<BinaryOcrBitmap>();
                            for (int i = 1; i < bob.ExpandCount; i++)
                            {
                                var expandedBob = new BinaryOcrBitmap(gz);
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
            }
            CompareImages = list;
            CompareImagesExpanded = expandList;
        }

        private static int MaxCommaQuoteTopDiff = 15;

        public static bool AllowEqual(BinaryOcrBitmap match, BinaryOcrBitmap newBob)
        {
            if (match.Text != null && (match.Text == "," || match.Text == "'") &&
                Math.Abs(match.Y - newBob.Y) > MaxCommaQuoteTopDiff)
            {
                return false;
            }
            return true;
        }

        public int FindExactMatch(BinaryOcrBitmap bob)
        {
            for (int i = 0; i < CompareImages.Count; i++)
            {
                var b = CompareImages[i];
                if (bob.Hash == b.Hash && bob.Width == b.Width && bob.Height == b.Height && bob.NumberOfColoredPixels == b.NumberOfColoredPixels)
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
            for (int i = 0; i < CompareImagesExpanded.Count; i++)
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
                    bool allAlike = true;
                    for (int i = 0; i < bob.ExpandCount - 1; i++)
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
            var list = new List<string>();
            foreach (string dir in Directory.GetFiles(Configuration.OcrDirectory.TrimEnd(Path.DirectorySeparatorChar), "*.db"))
            {
                string s = Path.GetFileNameWithoutExtension(dir);
                list.Add(s);
            }
            return list.OrderBy(p=>p).ToList();
        }

    }
}
