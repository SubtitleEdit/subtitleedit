using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Nikse.SubtitleEdit.Logic.Ocr.Binary
{
    public class BinaryOcrDb
    {
        public string FileName { get; private set; }
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
                LoadCompareImages();
        }

        public void Save()
        {
            using (Stream gz = new GZipStream(File.OpenWrite(FileName), CompressionMode.Compress))
            {
                foreach (var bob in CompareImages)
                    bob.Save(gz);
                foreach (var bob in CompareImagesExpanded)
                {
                    bob.Save(gz);
                    foreach (var expandedBob in bob.ExpandedList)
                        expandedBob.Save(gz);
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
                                    bob.ExpandedList.Add(expandedBob);
                                else
                                    break;
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

        public int FindExactMatch(BinaryOcrBitmap bob)
        {
            for (int i = 0; i < CompareImages.Count; i++)
            {
                var b = CompareImages[i];
                if (bob.Hash == b.Hash && bob.Width == b.Width && bob.Height == b.Height && bob.NumberOfColoredPixels == b.NumberOfColoredPixels)
                    return i;
            }
            return -1;
        }

        public int FindExactMatchExpanded(BinaryOcrBitmap bob)
        {
            for (int i = 0; i < CompareImagesExpanded.Count; i++)
            {
                var b = CompareImagesExpanded[i];
                if (bob.Hash == b.Hash && bob.Width == b.Width && bob.Height == b.Height && bob.NumberOfColoredPixels == b.NumberOfColoredPixels)
                    return i;
            }
            return -1;
        }

        public int Add(BinaryOcrBitmap bob)
        {
            int index;
            if (bob.ExpandCount > 0)
            {
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
                            allAlike = false;
                    }
                    if (!allAlike)
                        CompareImages.Add(bob);
                    else
                        System.Windows.Forms.MessageBox.Show("Expanded image already in db!");
                }
            }
            else
            {
                index = FindExactMatch(bob);
                if (index == -1)
                    CompareImages.Add(bob);
                else
                    System.Windows.Forms.MessageBox.Show("Image already in db!");
            }
            return index;
        }

    }
}