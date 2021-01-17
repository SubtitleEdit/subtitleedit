using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class NOcrDb
    {
        public string FileName { get; set; }
        public List<NOcrChar> OcrCharacters = new List<NOcrChar>();
        public List<NOcrChar> OcrCharactersExpanded = new List<NOcrChar>();

        private const string Version = "V2";

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

        public void Save()
        {
            using (Stream gz = new GZipStream(File.OpenWrite(FileName), CompressionMode.Compress))
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

            bool isVersion2;
            using (Stream gz = new GZipStream(File.OpenRead(FileName), CompressionMode.Decompress))
            {
                var versionBuffer = new byte[Version.Length];
                gz.Read(versionBuffer, 0, versionBuffer.Length);
                isVersion2 = Encoding.ASCII.GetString(versionBuffer) == Version;
            }

            using (Stream gz = new GZipStream(File.OpenRead(FileName), CompressionMode.Decompress))
            {
                bool done = false;
                if (isVersion2)
                {
                    var versionBuffer = new byte[Version.Length];
                    gz.Read(versionBuffer, 0, versionBuffer.Length);
                }
                while (!done)
                {
                    var ocrChar = new NOcrChar(gz, isVersion2);
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

        public void Remove(NOcrChar ocrChar)
        {
            if (ocrChar.ExpandCount > 0)
            {
                OcrCharactersExpanded.Remove(ocrChar);
            }
            else
            {
                OcrCharacters.Remove(ocrChar);
            }
        }

        public NOcrChar GetMatchExpanded(NikseBitmap nikseBitmap, ImageSplitterItem targetItem, int listIndex, List<ImageSplitterItem> list)
        {
            int w = targetItem.NikseBitmap.Width;
            for (var i = 0; i < OcrCharactersExpanded.Count; i++)
            {
                var oc = OcrCharactersExpanded[i];
                if (oc.ExpandCount > 1 && oc.Width > w && targetItem.X + oc.Width < nikseBitmap.Width)
                {
                    bool ok = true;
                    var index = 0;
                    while (index < oc.LinesForeground.Count && ok)
                    {
                        var op = oc.LinesForeground[index];
                        foreach (var point in op.GetPoints())
                        {
                            var p = new Point(point.X + targetItem.X, point.Y + targetItem.Y - oc.MarginTop);
                            if (p.X >= 0 && p.Y >= 0 && p.X < nikseBitmap.Width && p.Y < nikseBitmap.Height)
                            {
                                var a = nikseBitmap.GetAlpha(p.X, p.Y);
                                if (a <= 150)
                                {
                                    ok = false;
                                    break;
                                }
                            }
                            else if (p.X >= 0 && p.Y >= 0)
                            {
                                ok = false;
                                break;
                            }
                        }

                        index++;
                    }

                    index = 0;
                    while (index < oc.LinesBackground.Count && ok)
                    {
                        var op = oc.LinesBackground[index];
                        foreach (var point in op.GetPoints())
                        {
                            var p = new Point(point.X + targetItem.X, point.Y + targetItem.Y - oc.MarginTop);
                            if (p.X >= 0 && p.Y >= 0 && p.X < nikseBitmap.Width && p.Y < nikseBitmap.Height)
                            {
                                var a = nikseBitmap.GetAlpha(p.X, p.Y);
                                if (a > 150)
                                {
                                    ok = false;
                                    break;
                                }
                            }
                            else if (p.X >= 0 && p.Y >= 0)
                            {
                                ok = false;
                                break;
                            }
                        }

                        index++;
                    }

                    if (ok)
                    {
                        var size = GetTotalSize(listIndex, list, oc.ExpandCount);
                        if (Math.Abs(size.X - oc.Width) < 3 && Math.Abs(size.Y - oc.Height) < 3)
                        {
                            return oc;
                        }
                    }
                }
            }

            for (var i = 0; i < OcrCharactersExpanded.Count; i++)
            {
                var oc = OcrCharactersExpanded[i];
                if (oc.ExpandCount > 1 && oc.Width > w && targetItem.X + oc.Width < nikseBitmap.Width)
                {
                    bool ok = true;
                    var index = 0;
                    while (index < oc.LinesForeground.Count && ok)
                    {
                        var op = oc.LinesForeground[index];
                        foreach (var point in op.ScaledGetPoints(oc, oc.Width, oc.Height - 1))
                        {
                            var p = new Point(point.X + targetItem.X, point.Y + targetItem.Y - oc.MarginTop);
                            if (p.X >= 0 && p.Y >= 0 && p.X < nikseBitmap.Width && p.Y < nikseBitmap.Height)
                            {
                                var a = nikseBitmap.GetAlpha(p.X, p.Y);
                                if (a <= 150)
                                {
                                    ok = false;
                                    break;
                                }
                            }
                            else if (p.X >= 0 && p.Y >= 0)
                            {
                                ok = false;
                                break;
                            }
                        }

                        index++;
                    }

                    index = 0;
                    while (index < oc.LinesBackground.Count && ok)
                    {
                        var op = oc.LinesBackground[index];
                        foreach (var point in op.ScaledGetPoints(oc, oc.Width, oc.Height - 1))
                        {
                            var p = new Point(point.X + targetItem.X, point.Y + targetItem.Y - oc.MarginTop);
                            if (p.X >= 0 && p.Y >= 0 && p.X < nikseBitmap.Width && p.Y < nikseBitmap.Height)
                            {
                                var a = nikseBitmap.GetAlpha(p.X, p.Y);
                                if (a > 150)
                                {
                                    ok = false;
                                    break;
                                }
                            }
                            else if (p.X >= 0 && p.Y >= 0)
                            {
                                ok = false;
                                break;
                            }
                        }

                        index++;
                    }

                    if (ok)
                    {
                        var size = GetTotalSize(listIndex, list, oc.ExpandCount);
                        var widthPercent = size.Y * 100.0 / size.X;
                        if (Math.Abs(widthPercent - oc.WidthPercent) < 15 &&
                            Math.Abs(size.X - oc.Width) < 25 && Math.Abs(size.Y - oc.Height) < 20)
                        {
                            return oc;
                        }
                    }
                }
            }

            return null;
        }

        private static Point GetTotalSize(int listIndex, List<ImageSplitterItem> items, int count)
        {
            if (listIndex + count > items.Count)
            {
                return new Point(-100, -100);
            }

            var minimumX = int.MaxValue;
            int maximumX = int.MinValue;
            int minimumY = int.MaxValue;
            int maximumY = int.MinValue;

            for (var idx = listIndex; idx < listIndex + count; idx++)
            {
                var item = items[idx];
                if (item.NikseBitmap == null)
                {
                    return new Point(-100, -100);
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

            return new Point(maximumX - minimumX, maximumY - minimumY);
        }

        public NOcrChar GetMatch(NikseBitmap bitmap, int topMargin, bool deepSeek, int maxWrongPixels)
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

            if (maxWrongPixels >= 1)
            {
                foreach (var oc in OcrCharacters)
                {
                    if (Math.Abs(bitmap.Width - oc.Width) < 4 && Math.Abs(bitmap.Height - oc.Height) < 4 && Math.Abs(oc.MarginTop - topMargin) < 8)
                    {
                        if (IsMatch(bitmap, oc, 1))
                        {
                            return oc;
                        }
                    }
                }
            }

            if (maxWrongPixels >= 1)
            {
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
            }

            if (maxWrongPixels >= 2)
            {
                var errorsAllowed = Math.Min(3, maxWrongPixels);
                foreach (var oc in OcrCharacters)
                {
                    if (Math.Abs(widthPercent - oc.WidthPercent) < 20 && Math.Abs(oc.MarginTop - topMargin) < 15)
                    {
                        if (IsMatch(bitmap, oc, errorsAllowed))
                        {
                            return oc;
                        }
                    }
                }
            }

            if (maxWrongPixels >= 10)
            {
                var errorsAllowed = Math.Min(20, maxWrongPixels);
                foreach (var oc in OcrCharacters)
                {
                    if (!oc.IsSensitive && Math.Abs(widthPercent - oc.WidthPercent) < 20 && Math.Abs(oc.MarginTop - topMargin) < 15 && oc.LinesForeground.Count + oc.LinesBackground.Count > 40)
                    {
                        if (IsMatch(bitmap, oc, errorsAllowed))
                        {
                            return oc;
                        }
                    }
                }
            }

            if (maxWrongPixels >= 10)
            {
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
            }

            if (deepSeek)
            {
                foreach (var oc in OcrCharacters)
                {
                    if (Math.Abs(widthPercent - oc.WidthPercent) < 60 && Math.Abs(oc.MarginTop - topMargin) < 17 && oc.LinesForeground.Count + oc.LinesBackground.Count > 50)
                    {
                        if (IsMatch(bitmap, oc, maxWrongPixels))
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
            foreach (var op in oldChar.LinesForeground)
            {
                c.LinesForeground.Add(new NOcrPoint(MakePointItalic(op.Start, oldChar.Height, movePixelsLeft, unItalicFactor), MakePointItalic(op.End, oldChar.Height, movePixelsLeft, unItalicFactor)));
            }

            foreach (var op in oldChar.LinesBackground)
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
                        var a = bitmap.GetAlpha(point.X, point.Y);
                        if (a <= 150)
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
                        var a = bitmap.GetAlpha(point.X, point.Y);
                        if (a > 150)
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

        public static List<string> GetDatabases()
        {
            return Directory
                .GetFiles(Configuration.OcrDirectory.TrimEnd(Path.DirectorySeparatorChar), "*.nocr")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(p => p)
                .ToList();
        }
    }
}
