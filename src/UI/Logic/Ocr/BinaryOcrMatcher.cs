using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.Ocr;

public interface IBinaryOcrMatcher
{
    BinaryOcrMatcher.CompareMatch? GetCompareMatch(ImageSplitterItem2 targetItem, out BinaryOcrMatcher.CompareMatch? secondBestGuess, List<ImageSplitterItem2> list, int listIndex, BinaryOcrDb binaryOcrDb);
    bool IsLatinDb { get; set; }
}

public class BinaryOcrMatcher : IBinaryOcrMatcher
{
    public bool IsLatinDb { get; set; }
    private double _numericUpDownMaxErrorPct = 6;

    private long _ocrLowercaseHeightsTotal;
    private int _ocrLowercaseHeightsTotalCount;
    private long _ocrUppercaseHeightsTotal;
    private int _ocrUppercaseHeightsTotalCount;

    private static readonly Lock BinOcrDbMoveFirstLock = new Lock();

    public class CompareMatch
    {
        public string? Text { get; set; }
        public bool Italic { get; set; }
        public int ExpandCount { get; set; }
        public string? Name { get; set; }
        public NOcrChar? NOcrCharacter { get; set; }
        public ImageSplitterItem? ImageSplitterItem { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public List<ImageSplitterItem>? Extra { get; set; }

        public CompareMatch(string? text, bool italic, int expandCount, string? name)
        {
            Text = text;
            Italic = italic;
            ExpandCount = expandCount;
            Name = name;
        }

        public CompareMatch(string text, bool italic, int expandCount, string name, NOcrChar character)
            : this(text, italic, expandCount, name)
        {
            NOcrCharacter = character;
        }

        public CompareMatch(string text, bool italic, int expandCount, string name, ImageSplitterItem imageSplitterItem)
            : this(text, italic, expandCount, name)
        {
            ImageSplitterItem = imageSplitterItem;
        }

        public override string ToString()
        {
            if (Italic)
            {
                return Text + " (italic)";
            }

            if (Text == null)
            {
                return string.Empty;
            }

            return Text;
        }
    }

    public CompareMatch? GetCompareMatch(ImageSplitterItem2 targetItem, out CompareMatch? secondBestGuess, List<ImageSplitterItem2> list, int listIndex, BinaryOcrDb binaryOcrDb)
    {
        double maxDiff = _numericUpDownMaxErrorPct;
        secondBestGuess = null;
        int index = 0;
        int smallestDifference = 10000;
        var target = targetItem.NikseBitmap;
        if (binaryOcrDb == null || target == null)
        {
            return null;
        }

        var bob = new BinaryOcrBitmap(target) { X = targetItem.X, Y = targetItem.Top };

        // precise expanded match
        for (var k = 0; k < binaryOcrDb.CompareImagesExpanded.Count; k++)
        {
            var b = binaryOcrDb.CompareImagesExpanded[k];
            if (bob.Hash == b.Hash && bob.Width == b.Width && bob.Height == b.Height && bob.NumberOfColoredPixels == b.NumberOfColoredPixels)
            {
                bool ok = false;
                for (int i = 0; i < b.ExpandedList.Count; i++)
                {
                    if (listIndex + i + 1 < list.Count && list[listIndex + i + 1].NikseBitmap != null)
                    {
                        var bobNext = new BinaryOcrBitmap(list[listIndex + i + 1].NikseBitmap!);
                        if (b.ExpandedList[i].Hash == bobNext.Hash)
                        {
                            ok = true;
                        }
                        else
                        {
                            ok = false;
                            break;
                        }
                    }
                    else
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    return new CompareMatch(b.Text, b.Italic, b.ExpandCount, b.Key);
                }
            }
        }

        // allow for error %
        for (int k = 0; k < binaryOcrDb.CompareImagesExpanded.Count; k++)
        {
            var b = binaryOcrDb.CompareImagesExpanded[k];
            if (Math.Abs(bob.Width - b.Width) < 3 && Math.Abs(bob.Height - b.Height) < 3 && Math.Abs(bob.NumberOfColoredPixels - b.NumberOfColoredPixels) < 5 && GetPixelDifPercentage(b, bob, target, maxDiff) <= maxDiff)
            {
                bool ok = false;
                for (int i = 0; i < b.ExpandedList.Count; i++)
                {
                    if (listIndex + i + 1 < list.Count && list[listIndex + i + 1].NikseBitmap != null)
                    {
                        var bobNext = new BinaryOcrBitmap(list[listIndex + i + 1].NikseBitmap!);
                        if (b.ExpandedList[i].Hash == bobNext.Hash)
                        {
                            ok = true;
                        }
                        else if (Math.Abs(b.ExpandedList[i].Y - bobNext.Y) < 6 && GetPixelDifPercentage(b.ExpandedList[i], bobNext, list[listIndex + i + 1].NikseBitmap!, maxDiff) <= maxDiff)
                        {
                            ok = true;
                        }
                        else
                        {
                            ok = false;
                            break;
                        }
                    }
                    else
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    return new CompareMatch(b.Text, b.Italic, b.ExpandCount, b.Key);
                }
            }
        }

        FindBestMatch(ref index, ref smallestDifference, out var hit, target, binaryOcrDb, bob, maxDiff);
        if (maxDiff > 0)
        {
            if (target.Width > 16 && target.Height > 16 && (hit == null || smallestDifference * 100.0 / (target.Width * target.Height) > maxDiff))
            {
                var t2 = target.CopyRectangle(new NikseRectangle(0, 1, target.Width, target.Height));
                FindBestMatch(ref index, ref smallestDifference, out hit, t2, binaryOcrDb, bob, maxDiff);
            }
            if (target.Width > 16 && target.Height > 16 && (hit == null || smallestDifference * 100.0 / (target.Width * target.Height) > maxDiff))
            {
                var t2 = target.CopyRectangle(new NikseRectangle(1, 0, target.Width, target.Height));
                FindBestMatch(ref index, ref smallestDifference, out hit, t2, binaryOcrDb, bob, maxDiff);
            }
            if (target.Width > 16 && target.Height > 16 && (hit == null || smallestDifference * 100.0 / (target.Width * target.Height) > maxDiff))
            {
                var t2 = target.CopyRectangle(new NikseRectangle(0, 0, target.Width - 1, target.Height));
                FindBestMatch(ref index, ref smallestDifference, out hit, t2, binaryOcrDb, bob, maxDiff);
            }
        }

        if (hit != null)
        {
            double differencePercentage = smallestDifference * 100.0 / (target.Width * target.Height);
            if (differencePercentage <= maxDiff)
            {
                string? text = hit.Text;
                if (smallestDifference > 0)
                {
                    int h = hit.Height;
                    if (text == "V" || text == "W" || text == "U" || text == "S" || text == "Z" || text == "O" || text == "X" || text == "Ø" || text == "C")
                    {
                        if (_ocrLowercaseHeightsTotal > 10 && h - _ocrLowercaseHeightsTotal / _ocrLowercaseHeightsTotalCount < 2.0)
                        {
                            text = text.ToLowerInvariant();
                        }
                    }
                    else if (text == "v" || text == "w" || text == "u" || text == "s" || text == "z" || text == "o" || text == "x" || text == "ø" || text == "c")
                    {
                        if (_ocrUppercaseHeightsTotal > 10 && _ocrUppercaseHeightsTotal / _ocrUppercaseHeightsTotalCount - h < 2)
                        {
                            text = text.ToUpperInvariant();
                        }
                    }
                }
                else
                {
                    SetBinOcrLowercaseUppercase(hit.Height, text);
                }

                if (differencePercentage > 0)
                {
                    bool dummy;
                    if ((hit.Text == "l" || hit.Text == "!") && bob.IsLowercaseI(out dummy))
                    {
                        hit = null;
                    }
                    else if ((hit.Text == "i" || hit.Text == "!") && bob.IsLowercaseL())
                    {
                        hit = null;
                    }
                    else if ((hit.Text == "o" || hit.Text == "O") && bob.IsC())
                    {
                        return new CompareMatch(hit.Text == "o" ? "c" : "C", false, 0, null);
                    }
                    else if ((hit.Text == "c" || hit.Text == "C") && !bob.IsC() && bob.IsO())
                    {
                        return new CompareMatch(hit.Text == "c" ? "o" : "O", false, 0, null);
                    }
                }

                if (hit != null)
                {
                    if (differencePercentage < 9 && (text == "e" || text == "d" || text == "a"))
                    {
                        _ocrLowercaseHeightsTotalCount++;
                        _ocrLowercaseHeightsTotal += bob.Height;
                    }

                    return new CompareMatch(text, hit.Italic, hit.ExpandCount, hit.Key);
                }
            }

            if (hit != null)
            {
                secondBestGuess = new CompareMatch(hit.Text, hit.Italic, hit.ExpandCount, hit.Key);
            }
        }

        if (maxDiff > 1 && IsLatinDb)
        {
            if (bob.IsPeriod())
            {
                ImageSplitterItem2? next = null;
                if (listIndex + 1 < list.Count)
                {
                    next = list[listIndex + 1];
                }

                if (next?.NikseBitmap == null)
                {
                    return new CompareMatch(".", false, 0, null);
                }

                var nextBob = new BinaryOcrBitmap(next.NikseBitmap) { X = next.X, Y = next.Top };
                if (!nextBob.IsPeriodAtTop(GetLastBinOcrLowercaseHeight())) // avoid italic ":"
                {
                    return new CompareMatch(".", false, 0, null);
                }
            }

            if (bob.IsComma())
            {
                ImageSplitterItem2? next = null;
                if (listIndex + 1 < list.Count)
                {
                    next = list[listIndex + 1];
                }

                if (next?.NikseBitmap == null)
                {
                    return new CompareMatch(",", false, 0, null);
                }

                var nextBob = new BinaryOcrBitmap(next.NikseBitmap) { X = next.X, Y = next.Top };
                if (!nextBob.IsPeriodAtTop(GetLastBinOcrLowercaseHeight())) // avoid italic ";"
                {
                    return new CompareMatch(",", false, 0, null);
                }
            }

            if (bob.IsApostrophe())
            {
                return new CompareMatch("'", false, 0, null);
            }

            if (bob.IsLowercaseJ()) // "j" detection must be before "i"
            {
                return new CompareMatch("j", false, 0, null);
            }

            if (bob.IsLowercaseI(out var italicLowercaseI))
            {
                return new CompareMatch("i", italicLowercaseI, 0, null);
            }

            if (bob.IsColon())
            {
                return new CompareMatch(":", false, 0, null);
            }

            if (bob.IsExclamationMark())
            {
                return new CompareMatch("!", false, 0, null);
            }

            if (bob.IsDash())
            {
                return new CompareMatch("-", false, 0, null);
            }
        }

        return null;
    }

    private void SetBinOcrLowercaseUppercase(int height, string? text)
    {
        if (text == null)
        {
            return;
        }

        if (text == "e" || text == "a")
        {
            _ocrLowercaseHeightsTotalCount++;
            _ocrLowercaseHeightsTotal += height;
        }

        if (text == "E" || text == "H" || text == "R" || text == "D" || text == "T")
        {
            _ocrUppercaseHeightsTotalCount++;
            _ocrUppercaseHeightsTotal += height;
        }
    }

    private static double GetPixelDifPercentage(BinaryOcrBitmap expanded, BinaryOcrBitmap bobNext, NikseBitmap2 nbmpNext, double maxDiff)
    {
        var difColoredPercentage = (Math.Abs(expanded.NumberOfColoredPixels - bobNext.NumberOfColoredPixels)) * 100.0 / (bobNext.Width * bobNext.Height);
        if (difColoredPercentage > 1 && expanded.Width < 3 || bobNext.Width < 3)
        {
            return 100;
        }

        int dif = int.MaxValue;
        if (expanded.Height == bobNext.Height && expanded.Width == bobNext.Width)
        {
            dif = NikseBitmapImageSplitter2.IsBitmapsAlike(nbmpNext, expanded);
        }
        else if (maxDiff > 0)
        {
            if (expanded.Height == bobNext.Height && expanded.Width == bobNext.Width + 1)
            {
                dif = NikseBitmapImageSplitter2.IsBitmapsAlike(nbmpNext, expanded);
            }
            else if (expanded.Height == bobNext.Height && expanded.Width == bobNext.Width - 1)
            {
                dif = NikseBitmapImageSplitter2.IsBitmapsAlike(expanded, nbmpNext);
            }
            else if (expanded.Width == bobNext.Width && expanded.Height == bobNext.Height + 1)
            {
                dif = NikseBitmapImageSplitter2.IsBitmapsAlike(nbmpNext, expanded);
            }
            else if (expanded.Width == bobNext.Width && expanded.Height == bobNext.Height - 1)
            {
                dif = NikseBitmapImageSplitter2.IsBitmapsAlike(expanded, nbmpNext);
            }
        }

        var percentage = dif * 100.0 / (bobNext.Width * bobNext.Height);
        return percentage;
    }

    private int GetLastBinOcrLowercaseHeight()
    {
        var lowercaseHeight = 25;
        if (_ocrLowercaseHeightsTotalCount > 5)
        {
            lowercaseHeight = (int)Math.Round((double)_ocrLowercaseHeightsTotal / _ocrLowercaseHeightsTotalCount);
        }

        return lowercaseHeight;
    }

    private int GetLastBinOcrUppercaseHeight()
    {
        var uppercaseHeight = 35;
        if (_ocrUppercaseHeightsTotalCount > 5)
        {
            uppercaseHeight = (int)Math.Round((double)_ocrUppercaseHeightsTotal / _ocrUppercaseHeightsTotalCount);
        }

        return uppercaseHeight;
    }

    private static void FindBestMatch(ref int index, ref int smallestDifference, out BinaryOcrBitmap? hit, NikseBitmap2 target, BinaryOcrDb binOcrDb, BinaryOcrBitmap bob, double maxDiff)
    {
        hit = null;
        var bobExactMatch = binOcrDb.FindExactMatch(bob);
        if (bobExactMatch >= 0)
        {
            var m = binOcrDb.CompareImages[bobExactMatch];
            index = bobExactMatch;
            smallestDifference = 0;
            hit = m;
            return;
        }

        var tWidth = target.Width;
        var tHeight = target.Height;
        if (maxDiff < 0.2 || tWidth < 3 || tHeight < 5)
        {
            return;
        }

        int numberOfForegroundColors = bob.NumberOfColoredPixels;
        const int minForeColorMatch = 90;

        foreach (var compareItem in binOcrDb.CompareImages)
        {
            if (compareItem.Width == tWidth && compareItem.Height == tHeight) // precise math in size
            {
                if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < 3)
                {
                    int dif = NikseBitmapImageSplitter2.IsBitmapsAlike(compareItem, target);
                    if (dif < smallestDifference)
                    {
                        if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                        {
                            continue;
                        }

                        smallestDifference = dif;
                        hit = compareItem;
                        if (dif < 3)
                        {
                            break; // foreach ending
                        }
                    }
                }
            }
        }

        if (smallestDifference > 1)
        {
            foreach (var compareItem in binOcrDb.CompareImages)
            {
                if (compareItem.Width == tWidth && compareItem.Height == tHeight) // precise math in size
                {
                    if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < 40)
                    {
                        int dif = NikseBitmapImageSplitter2.IsBitmapsAlike(compareItem, target);
                        if (dif < smallestDifference)
                        {
                            if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                            {
                                continue;
                            }

                            smallestDifference = dif;
                            hit = compareItem;
                            if (dif == 0)
                            {
                                break; // foreach ending
                            }
                        }
                    }
                }
            }
        }

        if (tWidth > 16 && tHeight > 16 && smallestDifference > 2) // for other than very narrow letter (like 'i' and 'l' and 'I'), try more sizes
        {
            foreach (var compareItem in binOcrDb.CompareImages)
            {
                if (compareItem.Width == tWidth && compareItem.Height == tHeight - 1)
                {
                    if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                    {
                        int dif = NikseBitmapImageSplitter2.IsBitmapsAlike(compareItem, target);
                        if (dif < smallestDifference)
                        {
                            if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                            {
                                continue;
                            }

                            smallestDifference = dif;
                            hit = compareItem;
                            if (dif == 0)
                            {
                                break; // foreach ending
                            }
                        }
                    }
                }
            }

            if (smallestDifference > 2)
            {
                foreach (var compareItem in binOcrDb.CompareImages)
                {
                    if (compareItem.Width == tWidth && compareItem.Height == tHeight + 1)
                    {
                        if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                        {
                            int dif = NikseBitmapImageSplitter2.IsBitmapsAlike(target, compareItem);
                            if (dif < smallestDifference)
                            {
                                if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                {
                                    continue;
                                }

                                smallestDifference = dif;
                                hit = compareItem;
                                if (dif == 0)
                                {
                                    break; // foreach ending
                                }
                            }
                        }
                    }
                }
            }

            if (smallestDifference > 3)
            {
                foreach (var compareItem in binOcrDb.CompareImages)
                {
                    if (compareItem.Width == tWidth + 1 && compareItem.Height == tHeight + 1)
                    {
                        if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                        {
                            int dif = NikseBitmapImageSplitter2.IsBitmapsAlike(target, compareItem);
                            if (dif < smallestDifference)
                            {
                                if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                {
                                    continue;
                                }

                                smallestDifference = dif;
                                hit = compareItem;
                                if (dif == 0)
                                {
                                    break; // foreach ending
                                }
                            }
                        }
                    }
                }
            }

            if (smallestDifference > 5)
            {
                foreach (var compareItem in binOcrDb.CompareImages)
                {
                    if (compareItem.Width == tWidth - 1 && compareItem.Height == tHeight - 1)
                    {
                        if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                        {
                            int dif = NikseBitmapImageSplitter2.IsBitmapsAlike(compareItem, target);
                            if (dif < smallestDifference)
                            {
                                if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                {
                                    continue;
                                }

                                smallestDifference = dif;
                                hit = compareItem;
                                if (dif == 0)
                                {
                                    break; // foreach ending
                                }
                            }
                        }
                    }
                }
            }

            if (smallestDifference > 5)
            {
                foreach (var compareItem in binOcrDb.CompareImages)
                {
                    if (compareItem.Width - 1 == tWidth && compareItem.Height == tHeight)
                    {
                        if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                        {
                            int dif = NikseBitmapImageSplitter2.IsBitmapsAlike(target, compareItem);
                            if (dif < smallestDifference)
                            {
                                if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                {
                                    continue;
                                }

                                smallestDifference = dif;
                                hit = compareItem;
                                if (dif == 0)
                                {
                                    break; // foreach ending
                                }
                            }
                        }
                    }
                }
            }

            if (smallestDifference > 9 && tWidth > 11)
            {
                foreach (var compareItem in binOcrDb.CompareImages)
                {
                    if (compareItem.Width == tWidth - 2 && compareItem.Height == tHeight)
                    {
                        if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                        {
                            int dif = NikseBitmapImageSplitter2.IsBitmapsAlike(compareItem, target);
                            if (dif < smallestDifference)
                            {
                                if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                {
                                    continue;
                                }

                                smallestDifference = dif;
                                hit = compareItem;
                                if (dif == 0)
                                {
                                    break; // foreach ending
                                }
                            }
                        }
                    }
                }
            }

            if (smallestDifference > 9 && tWidth > 14)
            {
                foreach (var compareItem in binOcrDb.CompareImages)
                {
                    if (compareItem.Width == tWidth - 3 && compareItem.Height == tHeight)
                    {
                        if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                        {
                            int dif = NikseBitmapImageSplitter2.IsBitmapsAlike(compareItem, target);
                            if (dif < smallestDifference)
                            {
                                if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                {
                                    continue;
                                }

                                smallestDifference = dif;
                                hit = compareItem;
                                if (dif == 0)
                                {
                                    break; // foreach ending
                                }
                            }
                        }
                    }
                }
            }

            if (smallestDifference > 9 && tWidth > 14)
            {
                foreach (var compareItem in binOcrDb.CompareImages)
                {
                    if (compareItem.Width == tWidth && compareItem.Height == tHeight - 3)
                    {
                        if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                        {
                            int dif = NikseBitmapImageSplitter2.IsBitmapsAlike(compareItem, target);
                            if (dif < smallestDifference)
                            {
                                if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                {
                                    continue;
                                }

                                smallestDifference = dif;
                                hit = compareItem;
                                if (dif == 0)
                                {
                                    break; // foreach ending
                                }
                            }
                        }
                    }
                }
            }

            if (smallestDifference > 9 && tWidth > 14)
            {
                foreach (var compareItem in binOcrDb.CompareImages)
                {
                    if (compareItem.Width - 2 == tWidth && compareItem.Height == tHeight)
                    {
                        if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                        {
                            int dif = NikseBitmapImageSplitter2.IsBitmapsAlike(target, compareItem);
                            if (dif < smallestDifference)
                            {
                                if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                {
                                    continue;
                                }

                                smallestDifference = dif;
                                hit = compareItem;
                                if (dif == 0)
                                {
                                    break; // foreach ending
                                }
                            }
                        }
                    }
                }
            }
        }

        if (smallestDifference == 0)
        {
            if (hit != null && binOcrDb.CompareImages.IndexOf(hit) > 200)
            {
                lock (BinOcrDbMoveFirstLock)
                {
                    binOcrDb.CompareImages.Remove(hit);
                    binOcrDb.CompareImages.Insert(0, hit);
                    index = 0;
                }
            }
        }
    }
}

