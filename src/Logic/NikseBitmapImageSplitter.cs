using System;
using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Logic
{
    public class NikseBitmapImageSplitter
    {
        public static bool IsColorClose(Color a, Color b, int tolerance)
        {
            if (a.A < 120 && b.A < 120)
                return true; // transparent

            if (a.A > 250 && a.R > 90 && a.G > 90 && a.B > 90 &&
                b.A > 250 && b.R > 90 && b.G > 90 && b.B > 90)
                return true; // dark, non transparent

            int diff = (a.R + a.G + a.B) - (b.R + b.G + b.B);
            return diff < tolerance && diff > -tolerance;
        }

        public static bool IsColorClose(byte aa, byte ar, byte ag, byte ab, Color b, int tolerance)
        {
            if (aa < 120 && b.A < 120)
                return true; // transparent

            if (aa > 250 && ar > 90 && ag > 90 && ab > 90 &&
                b.A > 250 && b.R > 90 && b.G > 90 && b.B > 90)
                return true; // dark, non transparent

            int diff = (ar + ag + ab) - (b.R + b.G + b.B);
            return diff < tolerance && diff > -tolerance;
        }

        public static NikseBitmap CropTopAndBottom(NikseBitmap bmp, out int topCropping)
        {
            int startTop = 0;
            int maxTop = bmp.Height - 2;
            if (maxTop > bmp.Height)
                maxTop = bmp.Height;
            for (int y = 0; y < maxTop; y++)
            {
                bool allTransparent = true;
                for (int x = 1; x < bmp.Width - 1; x++)
                {
                    int a = bmp.GetAlpha(x, y);
                    if (a != 0)
                    {
                        allTransparent = false;
                        break;
                    }
                }
                if (!allTransparent)
                    break;
                startTop++;
            }
            //if (startTop > 9)
            //startTop -= 5; // if top space > 9, then allways leave blank 5 pixels on top (so . is not confused with ').
            topCropping = startTop;

            int h = bmp.Height;
            bool bottomCroppingDone = false;
            for (int y = bmp.Height - 1; y > 3; y--)
            {
                for (int x = 1; x < bmp.Width - 1; x++)
                {
                    int a = bmp.GetAlpha(x, y);
                    if (a != 0)
                    {
                        bottomCroppingDone = true;
                        break;
                    }
                }
                h = y;
                if (bottomCroppingDone)
                    break;
            }
            return bmp.CopyRectangle(new Rectangle(0, startTop, bmp.Width, h - startTop + 1));
        }

        public static NikseBitmap CropTopAndBottom(NikseBitmap bmp, out int topCropping, int maxDifferentPixelsOnLine)
        {
            int startTop = 0;
            int maxTop = bmp.Height - 2;
            if (maxTop > bmp.Height)
                maxTop = bmp.Height;

            for (int y = 0; y < maxTop; y++)
            {
                int difference = 0;
                bool allTransparent = true;
                for (int x = 1; x < bmp.Width - 1; x++)
                {
                    int a = bmp.GetAlpha(x, y);
                    if (a != 0)
                    {
                        difference++;
                        if (difference >= maxDifferentPixelsOnLine)
                        {
                            allTransparent = false;
                            break;
                        }
                    }
                }
                if (!allTransparent)
                    break;
                startTop++;
            }
            if (startTop > 9)
                startTop -= 5; // if top space > 9, then allways leave blank 5 pixels on top (so . is not confused with ').
            topCropping = startTop;

            for (int y = bmp.Height - 1; y > 3; y--)
            {
                int difference = 0;
                bool allTransparent = true;
                for (int x = 1; x < bmp.Width - 1; x++)
                {
                    int a = bmp.GetAlpha(x, y);
                    if (a != 0)
                    {
                        difference++;
                        if (difference >= maxDifferentPixelsOnLine)
                        {
                            allTransparent = false;
                            break;
                        }
                    }
                }
                if (allTransparent == false)
                    return bmp.CopyRectangle(new Rectangle(0, startTop, bmp.Width - 1, y - startTop + 1));
            }
            return bmp;
        }

        public static List<ImageSplitterItem> SplitVertical(Bitmap bmp)
        {
            return SplitVertical(new NikseBitmap(bmp));
        }

        public static List<ImageSplitterItem> SplitVertical(Bitmap bmp, int lineMinHeight)
        {
            return SplitVertical(new NikseBitmap(bmp), lineMinHeight);
        }

        public static List<ImageSplitterItem> SplitVertical(NikseBitmap bmp)
        { // split into lines
            int startY = 0;
            int size = 0;
            var parts = new List<ImageSplitterItem>();
            for (int y = 0; y < bmp.Height; y++)
            {
                bool allTransparent = true;
                for (int x = 0; x < bmp.Width; x++)
                {
                    int a = bmp.GetAlpha(x, y);
                    if (a != 0)
                    {
                        allTransparent = false;
                        break;
                    }
                }
                if (allTransparent)
                {
                    if (size > 2 && size < 6)
                    {
                        size++; // at least 5 pixels, like top of 'i'
                    }
                    else
                    {
                        if (size > 2)
                        {
                            NikseBitmap part = bmp.CopyRectangle(new Rectangle(0, startY, bmp.Width, size + 1));
                            //                            part.Save("c:\\line_0_to_width.bmp");
                            parts.Add(new ImageSplitterItem(0, startY, part));
                            //                            bmp.Save("c:\\original.bmp");
                        }
                        size = 0;
                        startY = y;
                    }
                }
                else
                {
                    size++;
                }
            }
            if (size > 2)
            {
                if (size == bmp.Height)
                {
                    if (size > 100)
                        return SplitVerticalTransparentOrBlack(bmp);
                    else
                        parts.Add(new ImageSplitterItem(0, startY, bmp));
                }
                else
                {
                    parts.Add(new ImageSplitterItem(0, startY, bmp.CopyRectangle(new Rectangle(0, startY, bmp.Width, size + 1))));
                }
            }
            return parts;
        }

        public static List<ImageSplitterItem> SplitVerticalTransparentOrBlack(NikseBitmap bmp)
        {
            int startY = 0;
            int size = 0;
            var parts = new List<ImageSplitterItem>();
            for (int y = 0; y < bmp.Height; y++)
            {
                bool allTransparent = true;
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    if (c.A > 20 && c.R + c.G + c.B > 20)
                    {
                        allTransparent = false;
                        break;
                    }
                }
                if (allTransparent)
                {
                    if (size > 2 && size <= 15)
                    {
                        size++; // at least 15 pixels, like top of 'i' or top of 'È'
                    }
                    else
                    {
                        if (size > 8)
                        {
                            NikseBitmap part = bmp.CopyRectangle(new Rectangle(0, startY, bmp.Width, size + 1));
                            //                            part.Save("c:\\line_0_to_width.bmp");
                            parts.Add(new ImageSplitterItem(0, startY, part));
                            //                            bmp.Save("c:\\original.bmp");
                        }
                        size = 0;
                        startY = y;
                    }
                }
                else
                {
                    size++;
                }
            }
            if (size > 2)
            {
                if (size == bmp.Height)
                    parts.Add(new ImageSplitterItem(0, startY, bmp));
                else
                    parts.Add(new ImageSplitterItem(0, startY, bmp.CopyRectangle(new Rectangle(0, startY, bmp.Width, size + 1))));
            }
            return parts;
        }

        public static List<ImageSplitterItem> SplitVertical(NikseBitmap bmp, int minLineHeight)
        { // split into lines
            int startY = 0;
            int size = 0;
            var parts = new List<ImageSplitterItem>();
            for (int y = 0; y < bmp.Height; y++)
            {
                bool allTransparent = true;
                for (int x = 0; x < bmp.Width; x++)
                {
                    int a = bmp.GetAlpha(x, y);
                    if (a != 0)
                    {
                        allTransparent = false;
                        break;
                    }
                }
                if (allTransparent)
                {
                    if (size > 2 && size <= minLineHeight)
                    {
                        size++; // at least 'lineMinHeight' pixels, like top of 'i'
                    }
                    else
                    {
                        if (size > 2)
                        {
                            NikseBitmap part = bmp.CopyRectangle(new Rectangle(0, startY, bmp.Width, size + 1));
                            //                            part.Save("c:\\line_0_to_width.bmp");
                            parts.Add(new ImageSplitterItem(0, startY, part));
                            //                            bmp.Save("c:\\original.bmp");
                        }
                        size = 0;
                        startY = y;
                    }
                }
                else
                {
                    size++;
                }
            }
            if (size > 2)
            {
                if (size == bmp.Height)
                {
                    if (size > 100)
                        return SplitVerticalTransparentOrBlack(bmp);
                    else
                        parts.Add(new ImageSplitterItem(0, startY, bmp));
                }
                else
                {
                    parts.Add(new ImageSplitterItem(0, startY, bmp.CopyRectangle(new Rectangle(0, startY, bmp.Width, size + 1))));
                }
            }
            return parts;
        }

        public static int IsBitmapsAlike(Bitmap bmp1, Bitmap bmp2)
        {
            int different = 0;
            int maxDiff = bmp1.Width * bmp1.Height / 5;

            for (int x = 1; x < bmp1.Width; x++)
            {
                for (int y = 1; y < bmp1.Height; y++)
                {
                    if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
                        different++;
                }
                if (different > maxDiff)
                    return different + 10;
            }
            return different;
        }

        private static List<ImageSplitterItem> SplitHorizontal(ImageSplitterItem verticalItem, int xOrMorePixelsMakesSpace)
        { // split line into letters
            NikseBitmap bmp = verticalItem.NikseBitmap;
            var parts = new List<ImageSplitterItem>();
            int size = 0;
            int startX = 0;
            int lastEndX = 0;
            int y = 0;
            bool spaceJustAdded = false;

            for (int x = 0; x < bmp.Width - 1; x++)
            {
                bool allTransparent = IsVerticalLineTransparent(bmp, ref y, x);

                // check if line is transparent and cursive
                bool cursiveOk = false;
                int tempY = 0;
                if (allTransparent == false &&
                    size > 5 &&
                    y > 3 &&
                    x < bmp.Width - 2 &&
                    !IsVerticalLineTransparent(bmp, ref tempY, x + 1))
                {
                    //Add space?
                    if (lastEndX > 0 && lastEndX + xOrMorePixelsMakesSpace < startX)
                    {
                        int cleanCount = 0;
                        for (int j = lastEndX; j < startX; j++)
                        {
                            int y1 = j;
                            if (IsVerticalLineTransparentAlphaOnly(bmp, ref y1, j))
                                cleanCount++;
                        }
                        if (cleanCount > 0 && !spaceJustAdded)
                        {
                            parts.Add(new ImageSplitterItem(" "));
                            spaceJustAdded = true;
                        }
                    }

                    var cursivePoints = new List<Point>();

                    cursiveOk = IsCursiveVerticalLineTransparent(bmp, size, y, x, cursivePoints);

                    if (cursiveOk)
                    {
                        // make letter image
                        int end = x + 1 - startX;
                        if (startX > 0)
                        {
                            startX--;
                            end++;
                        }
                        NikseBitmap b1 = bmp.CopyRectangle(new Rectangle(startX, 0, end, bmp.Height));
                        //                         b1.Save(@"d:\temp\cursive.bmp"); // just for debugging

                        // make non-black/transparent stuff from other letter transparent
                        foreach (Point p in cursivePoints)
                        {
                            for (int fixY = p.Y; fixY < bmp.Height; fixY++)
                                b1.SetPixel(p.X - startX, fixY, Color.Transparent);
                        }

                        RemoveBlackBarRight(b1);
                        //                                                b1.Save(@"d:\temp\cursive-cleaned.bmp"); // just for debugging

                        // crop and save image
                        int addY;
                        b1 = CropTopAndBottom(b1, out addY);
                        parts.Add(new ImageSplitterItem(startX, verticalItem.Y + addY, b1));
                        spaceJustAdded = false;
                        size = 0;
                        startX = x + 1;
                        lastEndX = x;
                    }
                }

                if (!cursiveOk)
                {
                    if (allTransparent)
                    {
                        if (size > 0)
                        {
                            if (size > 1)
                            {
                                //Add space?
                                if (lastEndX > 0 && lastEndX + xOrMorePixelsMakesSpace < startX)
                                {
                                    int cleanCount = 0;
                                    for (int j = lastEndX; j < startX; j++)
                                    {
                                        int y1 = j;
                                        if (IsVerticalLineTransparentAlphaOnly(bmp, ref y1, j))
                                            cleanCount++;
                                    }
                                    if (cleanCount > 2 && !spaceJustAdded)
                                    {
                                        parts.Add(new ImageSplitterItem(" "));
                                    }
                                }

                                if (startX > 0)
                                    startX--;
                                lastEndX = x;
                                int end = x + 1 - startX;
                                NikseBitmap part = bmp.CopyRectangle(new Rectangle(startX, 0, end, bmp.Height));
                                RemoveBlackBarRight(part);
                                int addY;
                                //                            part.Save("c:\\before" + startX.ToString() + ".bmp"); // just for debugging
                                part = CropTopAndBottom(part, out addY);
                                //                            part.Save("c:\\after" + startX.ToString() + ".bmp"); // just for debugging
                                parts.Add(new ImageSplitterItem(startX, verticalItem.Y + addY, part));
                                spaceJustAdded = false;
                                //                                part.Save(@"d:\temp\cursive.bmp"); // just for debugging
                            }
                            size = 0;
                        }
                        startX = x + 1;
                    }
                    else
                    {
                        size++;
                    }
                }
            }

            if (size > 0)
            {
                if (lastEndX > 0 && lastEndX + xOrMorePixelsMakesSpace < startX && !spaceJustAdded)
                    parts.Add(new ImageSplitterItem(" "));

                if (startX > 0)
                    startX--;
                lastEndX = bmp.Width - 1;
                int end = lastEndX + 1 - startX;
                NikseBitmap part = bmp.CopyRectangle(new Rectangle(startX, 0, end, bmp.Height - 1));
                int addY;
                part = CropTopAndBottom(part, out addY);
                parts.Add(new ImageSplitterItem(startX, verticalItem.Y + addY, part));
                //part.Save(@"d:\temp\cursive.bmp"); // just for debugging
            }
            return parts;
        }

        private static void RemoveBlackBarRight(NikseBitmap bmp)
        {
            int xRemoveBlackBar = bmp.Width - 1;
            for (int yRemoveBlackBar = 0; yRemoveBlackBar < bmp.Height; yRemoveBlackBar++)
            {
                byte[] c = bmp.GetPixelColors(xRemoveBlackBar, yRemoveBlackBar);
                if (c[0] == 0 || IsColorClose(c[0], c[1], c[2], c[3], Color.Black, 280))
                {
                    if (bmp.GetAlpha(xRemoveBlackBar - 1, yRemoveBlackBar) == 0)
                        bmp.SetPixel(xRemoveBlackBar, yRemoveBlackBar, Color.Transparent);
                }
            }
        }

        private static bool IsCursiveVerticalLineTransparent(NikseBitmap bmp, int size, int y, int x, List<Point> cursivePoints)
        {
            bool cursiveOk = true;
            int newY = y;
            int newX = x;
            while (cursiveOk && newY < bmp.Height - 1)
            {
                Color c0 = bmp.GetPixel(newX, newY);
                if (c0.A == 0 || IsColorClose(c0, Color.Black, 280))
                {
                    newY++;
                }
                else
                {
                    byte[] c1 = bmp.GetPixelColors(newX - 1, newY - 1);
                    byte[] c2 = bmp.GetPixelColors(newX - 1, newY);
                    if ((c1[0] == 0 || IsColorClose(c1[0], c1[1], c1[2], c1[3], Color.Black, 280)) && // still dark color...
                        (c2[0] == 0 || IsColorClose(c2[0], c2[1], c2[2], c2[3], Color.Black, 280)))
                    {
                        cursivePoints.Add(new Point(newX, newY));
                        if (newX > 1)
                            newX--;
                        else
                            cursiveOk = false;

                        newY++;
                    }
                    else
                    {
                        cursiveOk = false;
                    }
                }

                if (newX < x - size)
                    cursiveOk = false;
            }
            return cursiveOk;
        }

        private static bool IsVerticalLineTransparent(NikseBitmap bmp, ref int y, int x)
        {
            for (y = 0; y < bmp.Height - 1; y++)
            {
                var argb = bmp.GetPixelColors(x, y);
                if (argb[0] < 10 || IsColorClose(argb[0], argb[1], argb[2], argb[3], Color.Black, 280)) // still dark color...
                {
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsVerticalLineTransparentAlphaOnly(NikseBitmap bmp, ref int y, int x)
        {
            bool allTransparent = true;
            for (y = 0; y < bmp.Height - 1; y++)
            {
                int a = bmp.GetAlpha(x, y);
                if (a == 0) // still dark color...
                {
                }
                else
                {
                    allTransparent = false;
                    break;
                }
            }
            return allTransparent;
        }

        public static List<ImageSplitterItem> SplitBitmapToLetters(NikseBitmap bmp, int xOrMorePixelsMakesSpace, bool rightToLeft, bool topToBottom)
        {
            var list = new List<ImageSplitterItem>();

            // split into seperate lines
            List<ImageSplitterItem> verticalBitmaps = SplitVertical(bmp, xOrMorePixelsMakesSpace);

            if (!topToBottom)
                verticalBitmaps.Reverse();

            // split into letters
            int lineCount = 0;
            foreach (ImageSplitterItem b in verticalBitmaps)
            {
                if (lineCount > 0)
                    list.Add(new ImageSplitterItem(Environment.NewLine));
                var line = new List<ImageSplitterItem>();
                foreach (ImageSplitterItem item in SplitHorizontal(b, xOrMorePixelsMakesSpace))
                {
                    item.ParentY = item.Y;
                    line.Add(item);
                }
                if (rightToLeft)
                    line.Reverse();
                foreach (ImageSplitterItem item in line)
                    list.Add(item);
                lineCount++;
            }

            return list;
        }

        public static List<ImageSplitterItem> SplitBitmapToLettersNew(NikseBitmap bmp, int xOrMorePixelsMakesSpace, bool rightToLeft, bool topToBottom, int minLineHeight)
        {
            var list = new List<ImageSplitterItem>();

            // split into seperate lines
            List<ImageSplitterItem> verticalBitmaps = SplitVertical(bmp, minLineHeight);

            if (!topToBottom)
                verticalBitmaps.Reverse();

            // split into letters
            int lineCount = 0;
            foreach (ImageSplitterItem b in verticalBitmaps)
            {
                if (lineCount > 0)
                    list.Add(new ImageSplitterItem(Environment.NewLine));
                var line = new List<ImageSplitterItem>();
                foreach (ImageSplitterItem item in SplitHorizontalNew(b, xOrMorePixelsMakesSpace))
                {
                    item.ParentY = item.Y;
                    line.Add(item);
                }
                if (rightToLeft)
                    line.Reverse();
                foreach (ImageSplitterItem item in line)
                    list.Add(item);
                lineCount++;
            }

            return list;
        }

        private static IEnumerable<ImageSplitterItem> SplitHorizontalNew(ImageSplitterItem lineSplitterItem, int xOrMorePixelsMakesSpace)
        {
            var bmp = new NikseBitmap(lineSplitterItem.NikseBitmap);
            bmp.AddTransparentLineRight();
            var parts = new List<ImageSplitterItem>();
            int startX = 0;
            int width = 0;
            int spacePixels = 0;
            int subtractSpacePixels = 0;
            for (int x = 0; x < bmp.Width; x++)
            {
                bool right;
                bool clean;
                List<Point> points = IsVerticalLineTransparetNew(bmp, x, out right, out clean);

                if (points != null && clean)
                {
                    spacePixels++;
                }

                if (right && points != null)
                {
                    int add = FindMaxX(points, x) - x;
                    width = width + add;
                    subtractSpacePixels = add;
                }

                if (points == null)
                {
                    width++;
                }
                else if (width > 1)
                {
                    var bmp0 = new NikseBitmap(bmp);
                    // remove pixels after current;
                    int k = 0;
                    foreach (Point p in points)
                    {
                        bmp0.MakeVerticalLinePartTransparent(p.X, p.X + k, p.Y);
                        k++;
                    }
                    width = FindMaxX(points, x) - startX;
                    width++;
                    NikseBitmap b1 = bmp0.CopyRectangle(new Rectangle(startX, 0, width, bmp.Height));

                    if (spacePixels >= xOrMorePixelsMakesSpace && parts.Count > 0)
                        parts.Add(new ImageSplitterItem(" "));

                    int addY;
                    b1 = CropTopAndBottom(b1, out addY);
                    parts.Add(new ImageSplitterItem(startX + lineSplitterItem.X, addY + lineSplitterItem.Y, b1)); //y is what?

                    // remove pixels before next letter;
                    const int begin = 0;
                    foreach (Point p in points)
                    {
                        bmp.MakeVerticalLinePartTransparent(begin, p.X, p.Y);
                    }
                    width = 1;
                    startX = FindMinX(points, x);
                    spacePixels = -subtractSpacePixels;
                    subtractSpacePixels = 0;
                }
                else
                {
                    width = 1;
                    startX = FindMinX(points, x);
                }
            }
            return parts;
        }

        private static int FindMinX(List<Point> points, int x)
        {
            foreach (Point p in points)
            {
                if (p.X < x)
                    x = p.X;
            }
            return x;
        }
        private static int FindMaxX(List<Point> points, int x)
        {
            foreach (Point p in points)
            {
                if (p.X > x)
                    x = p.X;
            }
            return x;
        }

        private static List<Point> IsVerticalLineTransparetNew(NikseBitmap bmp, int x, out bool right, out bool clean)
        {
            right = false;
            bool left = false;
            clean = true;
            var points = new List<Point>();
            int y = 0;
            while (y < bmp.Height)
            {
                if (bmp.GetAlpha(x, y) > 100)
                {
                    clean = false;
                    if (x == 0)
                        return null;

                    if (x < bmp.Width - 1 && y < bmp.Height - 1 && bmp.GetAlpha(x + 1, y) == 0 && bmp.GetAlpha(x + 1, y + 1) == 0)
                    {
                        //if pixels to the left - move right?
                        if (bmp.GetAlpha(x - 1, y) > 0)
                        {
                            x++; //(requires search for min/max x in points
                            right = true;
                        }
                        else if (x > 0 && bmp.GetAlpha(x - 1, y) == 0)
                        {
                            x--; //(requires search for min/max x in points
                            left = true;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (x < bmp.Width - 1 && y == bmp.Height - 1 && bmp.GetAlpha(x + 1, y) == 0 && bmp.GetAlpha(x + 1, y - 1) == 0)
                    {
                        //if pixels to the left - move right?
                        if (bmp.GetAlpha(x - 1, y) > 0)
                        {
                            x++; //(requires search for min/max x in points
                            right = true;
                        }
                        else
                        {
                            return null;
                        }
                        right = true;
                    }
                    else if (bmp.GetAlpha(x - 1, y) == 0)
                    {
                        x--;
                        left = true;
                    }
                    else if (y > 5 && bmp.GetAlpha(x - 1, y - 1) == 0)
                    {
                        x--;
                        y--;
                        left = true;
                        while (points.Count > 0 && points[points.Count - 1].Y > y)
                            points.RemoveAt(points.Count - 1);
                    }
                    else if (y > 5 && bmp.GetAlpha(x - 1, y - 2) == 0)
                    {
                        x--;
                        y -= 2;
                        left = true;
                        while (points.Count > 0 && points[points.Count - 1].Y > y)
                            points.RemoveAt(points.Count - 1);
                    }
                    else
                    {
                        return null;
                    }

                    if (left && right)
                        return null;
                }
                else
                {
                    points.Add(new Point(x, y));
                    y++;
                }
            }
            return points;
        }

        public static List<ImageSplitterItem> SplitBitmapToLetters(List<ImageSplitterItem> verticalBitmaps, int xOrMorePixelsMakesSpace, bool rightToLeft, bool topToBottom)
        {
            var list = new List<ImageSplitterItem>();
            if (!topToBottom)
                verticalBitmaps.Reverse();

            // split into letters
            int lineCount = 0;
            foreach (ImageSplitterItem b in verticalBitmaps)
            {
                if (lineCount > 0)
                    list.Add(new ImageSplitterItem(Environment.NewLine));
                var line = new List<ImageSplitterItem>();
                foreach (ImageSplitterItem item in SplitHorizontal(b, xOrMorePixelsMakesSpace))
                {
                    item.ParentY = item.Y;
                    line.Add(item);
                }
                if (rightToLeft)
                    line.Reverse();
                foreach (ImageSplitterItem item in line)
                    list.Add(item);
                lineCount++;
            }

            return list;
        }

        internal static int IsBitmapsAlike(ManagedBitmap bmp1, NikseBitmap bmp2)
        {
            int different = 0;
            int maxDiff = bmp1.Width * bmp1.Height / 5;

            for (int x = 1; x < bmp1.Width; x++)
            {
                for (int y = 1; y < bmp1.Height; y++)
                {
                    if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
                        different++;
                }
                if (different > maxDiff)
                    return different + 10;
            }
            return different;
        }

        internal static int IsBitmapsAlike(NikseBitmap bmp1, ManagedBitmap bmp2)
        {
            int different = 0;
            int maxDiff = bmp1.Width * bmp1.Height / 5;

            for (int x = 1; x < bmp1.Width; x++)
            {
                for (int y = 1; y < bmp1.Height; y++)
                {
                    if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
                        different++;
                }
                if (different > maxDiff)
                    return different + 10;
            }
            return different;
        }

        internal static int IsBitmapsAlike(Ocr.Binary.BinaryOcrBitmap bmp1, NikseBitmap bmp2)
        {
            int different = 0;
            int maxDiff = bmp1.Width * bmp1.Height / 5;

            for (int x = 1; x < bmp1.Width; x++)
            {
                for (int y = 1; y < bmp1.Height; y++)
                {
                    //if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
                    if (bmp1.GetPixel(x, y) > 0 && bmp2.GetAlpha(x, y) < 100)
                        different++;
                }
                if (different > maxDiff)
                    return different + 10;
            }
            return different;
        }

        internal static int IsBitmapsAlike(NikseBitmap bmp1, Ocr.Binary.BinaryOcrBitmap bmp2)
        {
            int different = 0;
            int maxDiff = bmp1.Width * bmp1.Height / 5;

            for (int x = 1; x < bmp1.Width; x++)
            {
                for (int y = 1; y < bmp1.Height; y++)
                {
                    if (bmp1.GetAlpha(x, y) < 100 && bmp2.GetPixel(x, y) > 0)
                        different++;
                }
                if (different > maxDiff)
                    return different + 10;
            }
            return different;
        }
    }
}
