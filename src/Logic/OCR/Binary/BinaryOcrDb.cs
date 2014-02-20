using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Nikse.SubtitleEdit.Logic.OCR.Binary
{
    public class BinaryOcrDb
    {
        public string FileName { get; private set; }
        public List<BinaryOcrBitmap> CompareImages = new List<BinaryOcrBitmap>();

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
            using (Stream fs = File.OpenWrite(FileName))
            {
                using (Stream gz = new GZipStream(fs, CompressionMode.Compress))
                {
                    foreach (var bob in CompareImages)
                        bob.Save(gz);
                    gz.Flush();
                    gz.Close();
                }
            }
        }

        public void LoadCompareImages()
        {
            var list = new List<BinaryOcrBitmap>();

            if (!File.Exists(FileName))
            {
                CompareImages = list;
                return;
            }

            using (Stream fs = File.OpenRead(FileName))
            {
                using (Stream gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    bool done = false;
                    while (!done)
                    {
                        var bob = new BinaryOcrBitmap(gz);
                        if (bob.LoadedOK)
                            list.Add(bob);
                        else
                            done = true;
                    }
                }
            }
            CompareImages = list;
        }

        public int FindExactMatch(BinaryOcrBitmap bob)
        {
            for (int i=0; i<CompareImages.Count; i++)
            {
                var b = CompareImages[i];
                if (bob.Hash == b.Hash && bob.Width == b.Width && bob.Height == b.Height && bob.NumberOfColoredPixels == b.NumberOfColoredPixels)
                    return i;
            }
            return -1;
        }

        public int Add(BinaryOcrBitmap bob)
        {
            int index = FindExactMatch(bob);
            if (index == -1)
                CompareImages.Add(bob);
            return index;
        }


    }
}
