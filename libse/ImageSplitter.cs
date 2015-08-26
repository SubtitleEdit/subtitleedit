//using System;
//using System.Collections.Generic;
//using System.Drawing;

//namespace Nikse.SubtitleEdit.Logic
//{
//    public class ImageSplitter
//    {
//        public static bool IsColorClose(Color a, Color b, int tolerance)
//        {
//            if (a.A < 120 && b.A < 120)
//                return true; // transparent

//            if (a.A > 250 && a.R > 90 && a.G > 90 && a.B > 90 &&
//                b.A > 250 && b.R > 90 && b.G > 90 && b.B > 90)
//                return true; // dark, non transparent

//            int diff = (a.R + a.G + a.B) - (b.R + b.G + b.B);
//            return diff < tolerance && diff > -tolerance;
//        }

//        public static Bitmap Copy(Bitmap sourceBitmap, Rectangle section)
//        {
//            // Create the new bitmap and associated graphics object
//            var bmp = new Bitmap(section.Width, section.Height);
//            Graphics g = Graphics.FromImage(bmp);

//            // Draw the specified section of the source bitmap to the new one
//            g.DrawImage(sourceBitmap, 0, 0, section, GraphicsUnit.Pixel);

//            // Clean up
//            g.Dispose();

//            // Return the bitmap
//            return bmp;
//        }

//        public static Bitmap CropTopAndBottom(Bitmap bmp, out int topCropping)
//        {
//            int startTop = 0;
//            int maxTop = bmp.Height-2;
//            if (maxTop > bmp.Height)
//                maxTop = bmp.Height;
//            for (int y = 0; y < maxTop; y++)
//            {
//                bool allTransparent = true;
//                for (int x = 1; x < bmp.Width - 1; x++)
//                {
//                    Color c = bmp.GetPixel(x, y);
//                    if (c.A != 0)
//                    {
//                        allTransparent = false;
//                        break;
//                    }
//                }
//                if (!allTransparent)
//                    break;
//                startTop++;
//            }
//            if (startTop > 9)
//                startTop -= 5; // if top space > 9, then allways leave blank 5 pixels on top (so . is not confused with ').
//            topCropping = startTop;

//            for (int y = bmp.Height-1; y > 3; y--)
//            {
//                bool allTransparent = true;
//                for (int x = 1; x < bmp.Width-1; x++)
//                {
//                    Color c = bmp.GetPixel(x, y);
//                    if (c.A != 0)
//                    {
//                        allTransparent = false;
//                        break;
//                    }
//                }
//                if (allTransparent == false)
//                    return Copy(bmp, new Rectangle(0, startTop, bmp.Width - 1, y-startTop+1));
//            }
//            return bmp;
//        }

//        public static Bitmap CropTopAndBottom(Bitmap bmp, out int topCropping, int maxDifferentPixelsOnLine)
//        {
//            int startTop = 0;
//            int maxTop = bmp.Height - 2;
//            if (maxTop > bmp.Height)
//                maxTop = bmp.Height;

//            for (int y = 0; y < maxTop; y++)
//            {
//                int difference = 0;
//                bool allTransparent = true;
//                for (int x = 1; x < bmp.Width - 1; x++)
//                {
//                    Color c = bmp.GetPixel(x, y);
//                    if (c.A != 0)
//                    {
//                        difference++;
//                        if (difference >= maxDifferentPixelsOnLine)
//                        {
//                            allTransparent = false;
//                            break;
//                        }
//                    }
//                }
//                if (!allTransparent)
//                    break;
//                startTop++;
//            }
//            if (startTop > 9)
//                startTop -= 5; // if top space > 9, then allways leave blank 5 pixels on top (so . is not confused with ').
//            topCropping = startTop;

//            for (int y = bmp.Height - 1; y > 3; y--)
//            {
//                int difference = 0;
//                bool allTransparent = true;
//                for (int x = 1; x < bmp.Width - 1; x++)
//                {
//                    Color c = bmp.GetPixel(x, y);
//                    if (c.A != 0)
//                    {
//                        difference++;
//                        if (difference >= maxDifferentPixelsOnLine)
//                        {
//                            allTransparent = false;
//                            break;
//                        }
//                    }
//                }
//                if (allTransparent == false)
//                    return Copy(bmp, new Rectangle(0, startTop, bmp.Width - 1, y - startTop + 1));
//            }
//            return bmp;
//        }

//        public static List<ImageSplitterItem> SplitVertical(Bitmap bmp)
//        { // split into lines
//            int startY = 0;
//            int size = 0;
//            var parts = new List<ImageSplitterItem>();
//            for (int y = 0; y < bmp.Height; y++)
//            {
//                bool allTransparent = true;
//                for (int x = 0; x < bmp.Width; x++)
//                {
//                    Color c = bmp.GetPixel(x, y);
//                    if (c.A != 0)
//                    {
//                        allTransparent = false;
//                        break;
//                    }
//                }
//                if (allTransparent)
//                {
//                    if (size > 2 && size < 6)
//                    {
//                        size++; // at least 5 pixels, like top of 'i'
//                    }
//                    else
//                    {
//                        if (size > 2)
//                        {
//                            Bitmap part = Copy(bmp, new Rectangle(0, startY, bmp.Width, size+1));
////                            part.Save("c:\\line_0_to_width.bmp");
//                            parts.Add(new ImageSplitterItem(0, startY, part));
////                            bmp.Save("c:\\original.bmp");
//                        }
//                        size = 0;
//                        startY = y;
//                    }
//                }
//                else
//                {
//                    size++;
//                }

//            }
//            if (size > 2)
//            {
//                Bitmap part = Copy(bmp, new Rectangle(0, startY, bmp.Width, size+1));
//                parts.Add(new ImageSplitterItem(0, startY, part));
//            }
//            return parts;
//        }

//        public static List<ImageSplitterItem> SplitVertical(Bitmap bmp, int lineMinHeight)
//        { // split into lines
//            int startY = 0;
//            int size = 0;
//            var parts = new List<ImageSplitterItem>();
//            for (int y = 0; y < bmp.Height; y++)
//            {
//                bool allTransparent = true;
//                for (int x = 0; x < bmp.Width; x++)
//                {
//                    Color c = bmp.GetPixel(x, y);
//                    if (c.A != 0)
//                    {
//                        allTransparent = false;
//                        break;
//                    }
//                }
//                if (allTransparent)
//                {
//                    if (size > 2 && size <= lineMinHeight)
//                    {
//                        size++; // at least 5 pixels, like top of 'i'
//                    }
//                    else
//                    {
//                        if (size > 2)
//                        {
//                            Bitmap part = Copy(bmp, new Rectangle(0, startY, bmp.Width, size + 1));
//                            //                            part.Save("c:\\line_0_to_width.bmp");
//                            parts.Add(new ImageSplitterItem(0, startY, part));
//                            //                            bmp.Save("c:\\original.bmp");
//                        }
//                        size = 0;
//                        startY = y;
//                    }
//                }
//                else
//                {
//                    size++;
//                }

//            }
//            if (size > 2)
//            {
//                Bitmap part = Copy(bmp, new Rectangle(0, startY, bmp.Width, size + 1));
//                parts.Add(new ImageSplitterItem(0, startY, part));
//            }
//            return parts;
//        }

//        public static int IsBitmapsAlike(Bitmap bmp1, Bitmap bmp2)
//        {
//            int different = 0;
//            int maxDiff = (int)(bmp1.Width * bmp1.Height / 5.0);

//            for (int x = 1; x < bmp1.Width; x++)
//            {
//                for (int y = 1; y < bmp1.Height; y++)
//                {
//                    if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
//                    {
//                        different++;
//                    }
//                }
//                if (different > maxDiff)
//                    return different + 10;
//            }
//            return different;
//        }

//        public static int IsBitmapsAlike(FastBitmap bmp1, Bitmap bmp2)
//        {
//            int different = 0;

//            for (int x = 1; x < bmp1.Width; x++)
//            {
//                for (int y = 1; y < bmp1.Height; y++)
//                {
//                    Color c1 = bmp1.GetPixel(x, y);
//                    Color c2 = bmp1.GetPixel(x, y);
//                    if (!IsColorClose(c1, c2, 20))
//                        different++;
//                }
//            }
//            return different;
//        }

//        private static List<ImageSplitterItem> SplitHorizontal(ImageSplitterItem verticalItem, int xOrMorePixelsMakesSpace)
//        { // split line into letters
//            Bitmap bmp = verticalItem.NikseBitmap.GetBitmap();
//            var parts = new List<ImageSplitterItem>();
//            int size = 0;
//            int startX = 0;
//            int lastEndX = 0;
//            int y = 0;
//            bool spaceJustAdded = false;

//            for (int x = 0; x < bmp.Width - 1; x++)
//            {
//                bool allTransparent = IsVerticalLineTransparent(bmp, ref y, x);

//                // check if line is transparent and cursive
//                bool cursiveOk = false;
//                int tempY = 0;
//                if (allTransparent == false &&
//                    size > 5 &&
//                    y > 3 &&
//                    x < bmp.Width-2 &&
//                    !IsVerticalLineTransparent(bmp, ref tempY, x + 1))
//                {

//                    //Add space?
//                    if (lastEndX > 0 && lastEndX + xOrMorePixelsMakesSpace < startX)
//                    {
//                        int cleanCount = 0;
//                        for (int j = lastEndX; j < startX; j++)
//                        {
//                            int y1 = j;
//                            if (IsVerticalLineTransparent2(bmp, ref y1, j))
//                                cleanCount++;
//                        }
//                        if (cleanCount > 0 && !spaceJustAdded)
//                        {
//                            parts.Add(new ImageSplitterItem(" "));
//                            spaceJustAdded = true;
//                        }
//                    }

//                    var cursivePoints = new List<Point>();

//                    cursiveOk = IsCursiveVerticalLineTransparent(bmp, size, y, x, cursivePoints);

//                    if (cursiveOk)
//                    {
//                        // make letter image
//                        int end = x + 1 - startX;
//                        if (startX > 0)
//                        {
//                            startX--;
//                            end++;
//                        }
//                        Bitmap b1 = Copy(bmp, new Rectangle(startX, 0, end, bmp.Height));
////                         b1.Save(@"d:\temp\cursive.bmp"); // just for debugging

//                        // make non-black/transparent stuff from other letter transparent
//                        foreach (Point p in cursivePoints)
//                        {
//                            for (int fixY = p.Y; fixY < bmp.Height; fixY++)
//                                b1.SetPixel(p.X - startX, fixY, Color.Transparent);
//                        }

//                        RemoveBlackBarRight(b1);
////                                                b1.Save(@"d:\temp\cursive-cleaned.bmp"); // just for debugging

//                        // crop and save image
//                        int addY;
//                        b1 = CropTopAndBottom(b1, out addY);
//                        parts.Add(new ImageSplitterItem(startX, verticalItem.Y + addY, b1));
//                        spaceJustAdded = false;
//                        size = 0;
//                        startX = x + 1;
//                        lastEndX = x;
//                    }
//                }

//                if (!cursiveOk)
//                {
//                    if (allTransparent)
//                    {
//                        if (size > 0)
//                        {
//                            if (size > 1)
//                            {
//                                //Add space?
//                                if (lastEndX > 0 && lastEndX + xOrMorePixelsMakesSpace < startX)
//                                {
//                                    int cleanCount = 0;
//                                    for (int j = lastEndX; j < startX; j++)
//                                    {
//                                        int y1=j;
//                                        if (IsVerticalLineTransparent2(bmp, ref y1, j))
//                                            cleanCount++;
//                                    }
//                                    if (cleanCount > 2 && !spaceJustAdded)
//                                    {
//                                        parts.Add(new ImageSplitterItem(" "));
//                                        spaceJustAdded = true;
//                                    }
//                                }

//                                if (startX > 0)
//                                    startX--;
//                                lastEndX = x;
//                                int end = x + 1 - startX;
//                                Bitmap part = Copy(bmp, new Rectangle(startX, 0, end, bmp.Height));
//                                RemoveBlackBarRight(part);
//                                int addY;
//                                //                            part.Save("c:\\before" + startX.ToString() + ".bmp"); // just for debugging
//                                part = CropTopAndBottom(part, out addY);
//                                //                            part.Save("c:\\after" + startX.ToString() + ".bmp"); // just for debugging
//                                parts.Add(new ImageSplitterItem(startX, verticalItem.Y + addY, part));
//                                spaceJustAdded = false;
////                                part.Save(@"d:\temp\cursive.bmp"); // just for debugging
//                            }
//                            size = 0;
//                        }
//                        startX = x + 1;
//                    }
//                    else
//                    {
//                        size++;
//                    }
//                }
//            }

//            if (size > 0)
//            {
//                if (lastEndX > 0 && lastEndX + xOrMorePixelsMakesSpace < startX && !spaceJustAdded)
//                    parts.Add(new ImageSplitterItem(" "));

//                if (startX > 0)
//                    startX--;
//                lastEndX = bmp.Width-1;
//                int end = lastEndX + 1 - startX;
//                Bitmap part = Copy(bmp, new Rectangle(startX, 0, end, bmp.Height - 1));
//                int addY;
//                part = CropTopAndBottom(part, out addY);
//                parts.Add(new ImageSplitterItem(startX, verticalItem.Y + addY, part));
//                //part.Save(@"d:\temp\cursive.bmp"); // just for debugging
//            }
//            return parts;
//        }

//        private static void RemoveBlackBarRight(Bitmap bmp)
//        {
//            int xRemoveBlackBar = bmp.Width-1;
//            for (int yRemoveBlackBar = 0; yRemoveBlackBar < bmp.Height; yRemoveBlackBar++)
//            {
//                Color c = bmp.GetPixel(xRemoveBlackBar, yRemoveBlackBar);
//                if (c.A == 0 || IsColorClose(c, Color.Black, 280))
//                {
//                    if (bmp.GetPixel(xRemoveBlackBar - 1, yRemoveBlackBar).A == 0)
//                        bmp.SetPixel(xRemoveBlackBar, yRemoveBlackBar, Color.Transparent);
//                }
//            }
//        }

//        private static bool IsCursiveVerticalLineTransparent(Bitmap bmp, int size, int y, int x, List<Point> cursivePoints)
//        {
//            bool cursiveOk = true;
//            int newY = y;
//            int newX = x;
//            while (cursiveOk && newY < bmp.Height - 1)
//            {
//                Color c0 = bmp.GetPixel(newX, newY);
//                if (c0.A == 0 || IsColorClose(c0, Color.Black, 280))
//                {
//                    newY++;
//                }
//                else
//                {
//                    Color c1 = bmp.GetPixel(newX - 1, newY - 1);
//                    Color c2 = bmp.GetPixel(newX - 1, newY);
//                    if ((c1.A == 0 || IsColorClose(c1, Color.Black, 280)) && // still dark color...
//                        (c2.A == 0 || IsColorClose(c2, Color.Black, 280)))
//                    {
//                        cursivePoints.Add(new Point(newX, newY));
//                        if (newX > 1)
//                            newX--;
//                        else
//                            cursiveOk = false;

//                        newY++;
//                    }
//                    else
//                    {
//                        cursiveOk = false;
//                    }
//                }

//                if (newX < x - size)
//                    cursiveOk = false;
//            }
//            return cursiveOk;
//        }

//        private static bool IsVerticalLineTransparent(Bitmap bmp, ref int y, int x)
//        {
//            bool allTransparent = true;
//            for (y = 0; y < bmp.Height - 1; y++)
//            {
//                Color c = bmp.GetPixel(x, y);
//                if (c.A == 0 || //c.ToArgb() == transparentColor.ToArgb() ||
//                    IsColorClose(c, Color.Black, 280)) // still dark color...
//                {
//                }
//                else
//                {
//                    allTransparent = false;
//                    break;
//                }
//            }
//            return allTransparent;
//        }

//        private static bool IsVerticalLineTransparent2(Bitmap bmp, ref int y, int x)
//        {
//            bool allTransparent = true;
//            for (y = 0; y < bmp.Height - 1; y++)
//            {
//                Color c = bmp.GetPixel(x, y);
//                if (c.A == 0) // still dark color...
//                {
//                }
//                else
//                {
//                    allTransparent = false;
//                    break;
//                }
//            }
//            return allTransparent;
//        }

//        public static List<ImageSplitterItem> SplitBitmapToLetters(Bitmap bmp, int xOrMorePixelsMakesSpace, bool rightToLeft, bool topToBottom)
//        {
//            var list = new List<ImageSplitterItem>();

//            // split into seperate lines
//            List<ImageSplitterItem> verticalBitmaps = SplitVertical(bmp, xOrMorePixelsMakesSpace);

//            if (!topToBottom)
//                verticalBitmaps.Reverse();

//            // split into letters
//            int lineCount = 0;
//            foreach (ImageSplitterItem b in verticalBitmaps)
//            {
//                if (lineCount > 0)
//                    list.Add(new ImageSplitterItem(Environment.NewLine));
//                var line = new List<ImageSplitterItem>();
//                foreach (ImageSplitterItem item in SplitHorizontal(b, xOrMorePixelsMakesSpace))
//                {
//                    item.ParentY = item.Y;
//                    line.Add(item);
//                }
//                if (rightToLeft)
//                    line.Reverse();
//                foreach (ImageSplitterItem item in line)
//                    list.Add(item);
//                lineCount++;
//            }

//            return list;
//        }

//        public static List<ImageSplitterItem> SplitBitmapToLetters(List<ImageSplitterItem> verticalBitmaps, int xOrMorePixelsMakesSpace, bool rightToLeft, bool topToBottom)
//        {
//            var list = new List<ImageSplitterItem>();
//            if (!topToBottom)
//                verticalBitmaps.Reverse();

//            // split into letters
//            int lineCount = 0;
//            foreach (ImageSplitterItem b in verticalBitmaps)
//            {
//                if (lineCount > 0)
//                    list.Add(new ImageSplitterItem(Environment.NewLine));
//                var line = new List<ImageSplitterItem>();
//                foreach (ImageSplitterItem item in SplitHorizontal(b, xOrMorePixelsMakesSpace))
//                {
//                    item.ParentY = item.Y;
//                    line.Add(item);
//                }
//                if (rightToLeft)
//                    line.Reverse();
//                foreach (ImageSplitterItem item in line)
//                    list.Add(item);
//                lineCount++;
//            }

//            return list;
//        }

//        internal static unsafe int IsBitmapsAlike(NikseBitmap bmp1, NikseBitmap bmp2)
//        {
//            int different = 0;
//            int maxDiff = (int)(bmp1.Width * bmp1.Height / 5.0);

//            for (int x = 1; x < bmp1.Width; x++)
//            {
//                for (int y = 1; y < bmp1.Height; y++)
//                {
//                    if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
//                    {
//                        different++;
//                    }
//                }
//                if (different > maxDiff)
//                    return different + 10;
//            }
//            return different;
//        }

//        internal static unsafe int IsBitmapsAlike(ManagedBitmap bmp1, NikseBitmap bmp2)
//        {
//            int different = 0;
//            int maxDiff = (int)(bmp1.Width * bmp1.Height / 5.0);

//            for (int x = 1; x < bmp1.Width; x++)
//            {
//                for (int y = 1; y < bmp1.Height; y++)
//                {
//                    if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
//                    {
//                        different++;
//                    }
//                }
//                if (different > maxDiff)
//                    return different + 10;
//            }
//            return different;
//        }

//        internal static unsafe int IsBitmapsAlike(ManagedBitmap bmp1, Bitmap bmp2)
//        {
//            int different = 0;
//            int maxDiff = (int)(bmp1.Width * bmp1.Height / 5.0);

//            for (int x = 1; x < bmp1.Width; x++)
//            {
//                for (int y = 1; y < bmp1.Height; y++)
//                {
//                    if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
//                    {
//                        different++;
//                    }
//                }
//                if (different > maxDiff)
//                    return different + 10;
//            }
//            return different;
//        }

//        internal static unsafe int IsBitmapsAlike(NikseBitmap bmp1, ManagedBitmap bmp2)
//        {
//            int different = 0;
//            int maxDiff = (int)(bmp1.Width * bmp1.Height / 5.0);

//            for (int x = 1; x < bmp1.Width; x++)
//            {
//                for (int y = 1; y < bmp1.Height; y++)
//                {
//                    if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
//                    {
//                        different++;
//                    }
//                }
//                if (different > maxDiff)
//                    return different + 10;
//            }
//            return different;
//        }

//        internal static unsafe int IsBitmapsAlike(Bitmap bmp1, NikseBitmap bmp2)
//        {
//            int different = 0;
//            int maxDiff = (int)(bmp1.Width * bmp1.Height / 5.0);
//            NikseBitmap nbmp1 = new NikseBitmap(bmp1);

//            for (int x = 1; x < bmp1.Width; x++)
//            {
//                for (int y = 1; y < bmp1.Height; y++)
//                {
//                    if (!IsColorClose(nbmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
//                    {
//                        different++;
//                    }
//                }
//                if (different > maxDiff)
//                    return different + 10;
//            }
//            return different;
//        }

//        internal static unsafe int IsBitmapsAlike(NikseBitmap bmp1, Bitmap bmp2)
//        {
//            int different = 0;
//            int maxDiff = (int)(bmp1.Width * bmp1.Height / 5.0);

//            for (int x = 1; x < bmp1.Width; x++)
//            {
//                for (int y = 1; y < bmp1.Height; y++)
//                {
//                    if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
//                    {
//                        different++;
//                    }
//                }
//                if (different > maxDiff)
//                    return different + 10;
//            }
//            return different;
//        }
//    }
//}
