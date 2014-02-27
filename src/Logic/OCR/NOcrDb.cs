using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Nikse.SubtitleEdit.Logic.OCR
{
    public class NOcrDb
    {

        public string FileName { get; private set; }
        public List<NOcrChar> OcrCharacters = new List<NOcrChar>();

        public NOcrDb(string fileName)
        {
            FileName = fileName;
            LoadOcrCharacters();
        }

        public void Save()
        {
            using (Stream fs = File.OpenWrite(FileName))
            {
                using (Stream gz = new GZipStream(fs, CompressionMode.Compress))
                {
                    foreach (var ocrChar in OcrCharacters)
                        ocrChar.Save(gz);
                }
            }
        }

        public void LoadOcrCharacters()
        {
            var list = new List<NOcrChar>();

            if (!File.Exists(FileName))
            {
                OcrCharacters = list;
                return;
            }

            using (Stream fs = File.OpenRead(FileName))
            {
                using (Stream gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    bool done = false;
                    while (!done)
                    {
                        var ocrChar = new NOcrChar(gz);
                        if (ocrChar.LoadedOk)
                        {
                            list.Add(ocrChar);
                        }
                        else
                        {
                            done = true;
                        }
                    }
                }
            }
            OcrCharacters = list;
        }

        public int FindExactMatch(NOcrChar ocrChar)
        {
            return -1;
        }

        public void Add(NOcrChar ocrChar)
        {
            OcrCharacters.Add(ocrChar);
        }


    }
}
