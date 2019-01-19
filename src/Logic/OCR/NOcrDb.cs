using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class NOcrDb
    {
        public string FileName { get; }
        public List<NOcrChar> OcrCharacters = new List<NOcrChar>();
        public List<NOcrChar> OcrCharactersExpanded = new List<NOcrChar>();

        public NOcrDb(string fileName)
        {
            FileName = fileName;
            LoadOcrCharacters();
        }

        public void Save()
        {
            using (Stream gz = new GZipStream(File.OpenWrite(FileName), CompressionMode.Compress))
            {
                foreach (var ocrChar in OcrCharacters)
                {
                    ocrChar.Save(gz);
                }

                foreach (var ocrChar in OcrCharactersExpanded)
                {
                    ocrChar.Save(gz);
                }
            }
        }

        public void LoadOcrCharacters()
        {
            var list = new List<NOcrChar>();
            var listExpanded = new List<NOcrChar>();

            if (!File.Exists(FileName))
            {
                OcrCharacters = list;
                OcrCharactersExpanded = listExpanded;
                return;
            }

            using (Stream gz = new GZipStream(File.OpenRead(FileName), CompressionMode.Decompress))
            {
                bool done = false;
                while (!done)
                {
                    var ocrChar = new NOcrChar(gz);
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
            }
            OcrCharacters = list;
            OcrCharactersExpanded = listExpanded;
        }

        public static int FindExactMatch()
        {
            return -1;
        }

        public void Add(NOcrChar ocrChar)
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

        public NOcrChar GetMatch(NikseBitmap nbmp)
        {
            const int nocrMinColor = 300;
            const int topMargin = 1;
            double widthPercent = nbmp.Height * 100.0 / nbmp.Width;

            foreach (NOcrChar oc in OcrCharacters)
            {
                if (Math.Abs(widthPercent - oc.WidthPercent) < 20 && Math.Abs(oc.MarginTop - topMargin) < 5)
                { // only very accurate matches

                    bool ok = true;
                    var index = 0;
                    while (index < oc.LinesForeground.Count && ok)
                    {
                        NOcrPoint op = oc.LinesForeground[index];
                        foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                        {
                            if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                            {
                                Color c = nbmp.GetPixel(point.X, point.Y);
                                if (c.A <= 150 || c.R + c.G + c.B <= nocrMinColor)
                                {
                                    Point p = new Point(point.X - 1, point.Y);
                                    if (p.X < 0)
                                    {
                                        p.X = 1;
                                    }

                                    c = nbmp.GetPixel(p.X, p.Y);
                                    if (nbmp.Width > 20 && c.A > 150 && c.R + c.G + c.B > nocrMinColor)
                                    {
                                    }
                                    else
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }
                        index++;
                    }
                    index = 0;
                    while (index < oc.LinesBackground.Count && ok)
                    {
                        NOcrPoint op = oc.LinesBackground[index];
                        foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                        {
                            if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                            {
                                Color c = nbmp.GetPixel(point.X, point.Y);
                                if (c.A > 150 && c.R + c.G + c.B > nocrMinColor)
                                {
                                    Point p = new Point(point.X, point.Y);
                                    if (oc.Width > 19 && point.X > 0)
                                    {
                                        p.X = p.X - 1;
                                    }

                                    c = nbmp.GetPixel(p.X, p.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > nocrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }
                        index++;
                    }
                    if (ok)
                    {
                        return oc;
                    }
                }
            }
            return null;
        }

    }
}
