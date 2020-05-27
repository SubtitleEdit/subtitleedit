using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Forms.Ocr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class NOcrDb
    {
        public string FileName { get; set; }
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

        public NOcrChar GetMatch(NikseBitmap bitmap, int topMargin, bool deepSeek, bool italic, double italicAngle)
        {
            // only very very accurate matches
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

            // only very accurate matches
            double widthPercent = bitmap.Height * 100.0 / bitmap.Width;
            foreach (var oc in OcrCharacters)
            {
                if (Math.Abs(widthPercent - oc.WidthPercent) < 15 && Math.Abs(bitmap.Width - oc.Width) < 5 && Math.Abs(bitmap.Height - oc.Height) < 5 && Math.Abs(oc.MarginTop - topMargin) < 5)
                {
                    if (IsMatch(bitmap, oc, 0))
                    {
                        return oc;
                    }
                }
            }

            foreach (var oc in OcrCharacters)
            {
                if (Math.Abs(bitmap.Width - oc.Width) < 8 && Math.Abs(bitmap.Height - oc.Height) < 8 && Math.Abs(oc.MarginTop - topMargin) < 8)
                {
                    if (IsMatch(bitmap, oc, 1))
                    {
                        return oc;
                    }
                }
            }

            foreach (var oc in OcrCharacters)
            {
                if (Math.Abs(widthPercent - oc.WidthPercent) < 20 && Math.Abs(oc.MarginTop - topMargin) < 15)
                {
                    if (IsMatch(bitmap, oc, 2))
                    {
                        return oc;
                    }
                }
            }

            foreach (var oc in OcrCharacters)
            {
                if (!oc.IsSensitive && Math.Abs(widthPercent - oc.WidthPercent) < 20 && Math.Abs(oc.MarginTop - topMargin) < 15 && oc.LinesForeground.Count + oc.LinesBackground.Count > 40)
                {
                    if (IsMatch(bitmap, oc, 20))
                    {
                        return oc;
                    }
                }
            }

            foreach (var oc in OcrCharacters)
            {
                if (oc.IsSensitive && Math.Abs(widthPercent - oc.WidthPercent) < 30 && Math.Abs(oc.MarginTop - topMargin) < 15 && oc.LinesForeground.Count + oc.LinesBackground.Count > 40)
                {
                    if (IsMatch(bitmap, oc, 10))
                    {
                        return oc;
                    }
                }
            }

            if (italic)
            {
                foreach (var oc in OcrCharacters)
                {
                    if (Math.Abs(widthPercent - oc.WidthPercent) < 60 && Math.Abs(oc.MarginTop - topMargin) < 17 && oc.LinesForeground.Count + oc.LinesBackground.Count > 50)
                    {
                        var italicOc = MakeItalicNOcrChar(oc, 0, italicAngle);
                        if (IsMatch(bitmap, italicOc, 25))
                        {
                            return italicOc;
                        }
                    }
                }
            }

            if (deepSeek)
            {
                foreach (var oc in OcrCharacters)
                {
                    if (Math.Abs(widthPercent - oc.WidthPercent) < 60 && Math.Abs(oc.MarginTop - topMargin) < 17 && oc.LinesForeground.Count + oc.LinesBackground.Count > 50)
                    {
                        if (IsMatch(bitmap, oc, 25))
                        {
                            return oc;
                        }
                    }
                }
            }

            return null;
        }

        public static NOcrChar MakeItalicNOcrChar(NOcrChar oldChar, int movePixelsLeft, double unItalicFactor)
        {
            var c = new NOcrChar();
            foreach (NOcrPoint op in oldChar.LinesForeground)
            {
                c.LinesForeground.Add(new NOcrPoint(MakePointItalic(op.Start, oldChar.Height, movePixelsLeft, unItalicFactor), MakePointItalic(op.End, oldChar.Height, movePixelsLeft, unItalicFactor)));
            }

            foreach (NOcrPoint op in oldChar.LinesBackground)
            {
                c.LinesBackground.Add(new NOcrPoint(MakePointItalic(op.Start, oldChar.Height, movePixelsLeft, unItalicFactor), MakePointItalic(op.End, oldChar.Height, movePixelsLeft, unItalicFactor)));
            }

            c.Text = oldChar.Text;
            c.Width = oldChar.Width;
            c.Height = oldChar.Height;
            c.MarginTop = oldChar.MarginTop;
            c.Italic = true;
            return c;
        }

        private static Point MakePointItalic(Point p, int height, int moveLeftPixels, double italicAngle)
        {
            return new Point((int)Math.Round(p.X + (height - p.Y) * italicAngle - moveLeftPixels), p.Y);
        }

        public static bool IsMatch(NikseBitmap bitmap, NOcrChar oc, int errorsAllowed)
        {
            var index = 0;
            var errors = 0;
            while (index < oc.LinesForeground.Count)
            {
                var op = oc.LinesForeground[index];
                foreach (var point in op.ScaledGetPoints(oc, bitmap.Width, bitmap.Height))
                {
                    if (point.X >= 0 && point.Y >= 0 && point.X < bitmap.Width && point.Y < bitmap.Height)
                    {
                        var c = bitmap.GetPixel(point.X, point.Y);
                        if (c.A <= 150 || c.R + c.G + c.B <= VobSubOcr.NocrMinColor)
                        {
                            errors++;
                            if (errors > errorsAllowed)
                            {
                                return false;
                            }
                        }
                    }
                }

                index++;
            }

            index = 0;
            while (index < oc.LinesBackground.Count)
            {
                var op = oc.LinesBackground[index];
                foreach (var point in op.ScaledGetPoints(oc, bitmap.Width, bitmap.Height))
                {
                    if (point.X >= 0 && point.Y >= 0 && point.X < bitmap.Width && point.Y < bitmap.Height)
                    {
                        var c = bitmap.GetPixel(point.X, point.Y);
                        if (c.A > 150 && c.R + c.G + c.B > VobSubOcr.NocrMinColor)
                        {
                            errors++;
                            if (errors > errorsAllowed)
                            {
                                return false;
                            }
                        }
                    }
                }
                index++;
            }

            return true;
        }
    }
}
