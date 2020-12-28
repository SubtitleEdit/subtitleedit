using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Nikse.SubtitleEdit.Logic
{
    public static class TextDesigner
    {
        public static Bitmap MakeTextBitmapAssa(string text, int xPos, int yPos, Font font, int width, int height, float outlineWidth, float shadowWidth,
            Bitmap backgroundImage, Color textColor, Color outlineColor, Color shadowColor, bool opaqueBox)
        {
            // outline width should be about double for SSA/ASSA
            outlineWidth *= 2.0f;

            var newImage = new Bitmap(width, height);
            var outlineImage = new Bitmap(width, height);
            var shadowImage = new Bitmap(width, height);
            var g = Graphics.FromImage(newImage);
            var gOutline = Graphics.FromImage(outlineImage);
            var gShadow = Graphics.FromImage(shadowImage);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var stringFormat = new StringFormat();
            var path = new GraphicsPath();

            if (opaqueBox)
            {
                if (outlineWidth > 0)
                {
                    var noOutlineBmp = MakeTextBitmapAssa(text, 0, 0, font, width, height, 0, 0, null, textColor, outlineColor, shadowColor, false);
                    var nikseBmp = new NikseBitmap(noOutlineBmp);
                    var w = nikseBmp.GetNonTransparentWidth();
                    var h = nikseBmp.GetNonTransparentHeight();

                    xPos = (int)Math.Round(xPos + outlineWidth);
                    yPos = (int)Math.Round(yPos + outlineWidth);

                    if (shadowWidth > 0)
                    {
                        using (var shadowBoxBrush = new SolidBrush(shadowColor))
                        {
                            g.FillRectangle(shadowBoxBrush, xPos - outlineWidth + shadowWidth, yPos - outlineWidth + shadowWidth, w + outlineWidth + outlineWidth, h + +outlineWidth + outlineWidth);
                        }
                    }

                    using (var boxBrush = new SolidBrush(outlineColor))
                    {
                        g.FillRectangle(boxBrush, xPos - outlineWidth, yPos - outlineWidth, w + outlineWidth + outlineWidth, h + +outlineWidth + outlineWidth);
                    }

                    path.AddString(text, font.FontFamily, (int)font.Style, font.Size, new Point(xPos, yPos), stringFormat);
                    noOutlineBmp.Dispose();
                }
            }
            else
            {
                path.AddString(text, font.FontFamily, (int)font.Style, font.Size, new Point(xPos, yPos), stringFormat);

                var shadowPath2 = (GraphicsPath)path.Clone();
                using (var shadowBrush = new SolidBrush(shadowColor))
                {
                    using (var p1 = new Pen(shadowColor, outlineWidth))
                    {
                        p1.LineJoin = LineJoin.Round;
                        using (var translateMatrix = new Matrix())
                        {
                            translateMatrix.Translate(shadowWidth, shadowWidth);
                            shadowPath2.Transform(translateMatrix);
                            g.DrawPath(p1, shadowPath2);
                            g.FillPath(shadowBrush, shadowPath2);
                            path.Reset();
                        }
                    }
                }

                path.AddString(text, font.FontFamily, (int)font.Style, font.Size, new Point(xPos, yPos), stringFormat);

                // Outline
                if (outlineWidth > 0)
                {
                    using (var pen = new Pen(outlineColor, outlineWidth))
                    {
                        pen.LineJoin = LineJoin.Round;
                        g.DrawPath(pen, path);
                        using (var outlineBrush = new SolidBrush(outlineColor))
                        {
                            g.FillPath(outlineBrush, path);
                        }
                        path.Reset();
                    }
                }
                path.AddString(text, font.FontFamily, (int)font.Style, font.Size, new Point(xPos, yPos), stringFormat);
            }

            // Text color
            var brush = new SolidBrush(textColor);
            g.FillPath(brush, path);

            path.Dispose();
            brush.Dispose();
            outlineImage.Dispose();
            gShadow.Dispose();
            gOutline.Dispose();
            g.Dispose();
            stringFormat.Dispose();

            var finalBitmap = new Bitmap(width, height);
            using (var gfx = Graphics.FromImage(finalBitmap))
            {
                if (backgroundImage != null)
                {
                    gfx.DrawImage(backgroundImage, new Point(0, 0));
                }

                gfx.DrawImage(newImage, new Point(0, 0));
            }

            newImage.Dispose();
            return finalBitmap;
        }

        public static Bitmap MakeTextBitmap(string text, int xPos, int yPos, Font font, int width, int height, float outlineWidth, float outline2Width, float shadowWidth,
            Bitmap backgroundImage, int shadowAlpha, Color textColor, Color outlineColor, Color outline2Color, Color shadowColor,
            bool opaqueBox,
            bool textGradient)
        {
            // outline width should be about double for SSA/ASSA
            outlineWidth *= 2.0f;
            outline2Width *= 2.0f;

            var newImage = new Bitmap(width, height);
            var outlineImage = new Bitmap(width, height);
            var shadowImage = new Bitmap(width, height);
            var g = Graphics.FromImage(newImage);
            var gOutline = Graphics.FromImage(outlineImage);
            var gShadow = Graphics.FromImage(shadowImage);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var stringFormat = new StringFormat();
            var path = new GraphicsPath();

            if (opaqueBox)
            {
                if (outlineWidth > 0)
                {
                    var noOutlineBmp = MakeTextBitmap(text, 0, 0, font, width, height, 0, 0, 0, null, 0, textColor, outlineColor, outline2Color, shadowColor, false, false);
                    var nikseBmp = new NikseBitmap(noOutlineBmp);
                    var w = nikseBmp.GetNonTransparentWidth();
                    var h = nikseBmp.GetNonTransparentHeight();


                    xPos = (int)Math.Round(xPos + outlineWidth);
                    yPos = (int)Math.Round(yPos + outlineWidth);

                    if (shadowWidth > 0)
                    {
                        using (var shadowBoxBrush = new SolidBrush(shadowColor))
                        {
                            g.FillRectangle(shadowBoxBrush, xPos - outlineWidth + shadowWidth, yPos - outlineWidth + shadowWidth, w + outlineWidth + outlineWidth, h + +outlineWidth + outlineWidth);
                        }

                        shadowWidth = 0;
                    }

                    using (var boxBrush = new SolidBrush(outlineColor))
                    {
                        g.FillRectangle(boxBrush, xPos - outlineWidth, yPos - outlineWidth, w + outlineWidth + outlineWidth, h + +outlineWidth + outlineWidth);
                    }

                    path.AddString(text, font.FontFamily, (int)font.Style, font.Size, new Point(xPos, yPos), stringFormat);
                    noOutlineBmp.Dispose();
                }
            }
            else
            {
                path.AddString(text, font.FontFamily, (int)font.Style, font.Size, new Point(xPos, yPos), stringFormat);

                // Second outline
                var penOut = new Pen(Color.FromArgb(255, outline2Color), outlineWidth + outline2Width);
                penOut.LineJoin = LineJoin.Round;
                g.DrawPath(penOut, path);

                // Outline
                if (outlineWidth > 0)
                {
                    using (var pen = new Pen(Color.FromArgb(255, outlineColor), outlineWidth))
                    {
                        pen.LineJoin = LineJoin.Round;
                        g.DrawPath(pen, path);
                    }
                }
                penOut.Dispose();
            }

            var shadowPath = (GraphicsPath)path.Clone();

            // Text color
            var brush = new SolidBrush(Color.FromArgb(255, textColor));
            if (textGradient)
            {
                using (var brushGradient = new LinearGradientBrush(new Rectangle(xPos, yPos, 30, 70),
                    Color.FromArgb(255, textColor),
                    Color.FromArgb(255, textColor.R / 10, textColor.R / 10, textColor.R / 10),
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(brushGradient, path);
                }
            }
            else
            {
                g.FillPath(brush, path);
            }


            // Shadow - draw in separate bitmap
            using (var shadowBrush = new SolidBrush(Color.FromArgb(byte.MaxValue, shadowColor)))
            {
                using (var p1 = new Pen(Color.FromArgb(byte.MaxValue, shadowColor), outlineWidth + outline2Width))
                {
                    p1.LineJoin = LineJoin.Round;
                    for (int i = 0; i < shadowWidth; i++)
                    {
                        using (var translateMatrix = new Matrix())
                        {
                            translateMatrix.Translate(1, 1);
                            shadowPath.Transform(translateMatrix);
                            gShadow.DrawPath(p1, shadowPath);
                            gShadow.FillPath(shadowBrush, shadowPath);
                        }
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var shadowPixel = shadowImage.GetPixel(x, y);
                    var newPixel = newImage.GetPixel(x, y);
                    if (newPixel.A > 100)
                    {
                        // close to text color
                        if (Math.Abs(textColor.R - newPixel.R) < 10 &&
                            Math.Abs(textColor.G - newPixel.G) < 10 &&
                            Math.Abs(textColor.B - newPixel.B) < 10)
                        {
                            newImage.SetPixel(x, y, Color.FromArgb(textColor.A, newPixel));
                        }

                        // close to outlineColor color
                        else if (Math.Abs(outlineColor.R - newPixel.R) < 10 &&
                                 Math.Abs(outlineColor.G - newPixel.G) < 10 &&
                                 Math.Abs(outlineColor.B - newPixel.B) < 10)
                        {
                            newImage.SetPixel(x, y, Color.FromArgb(outlineColor.A, newPixel));
                        }

                        // close to outline2Color color
                        else if (Math.Abs(outline2Color.R - newPixel.R) < 10 &&
                                 Math.Abs(outline2Color.G - newPixel.G) < 10 &&
                                 Math.Abs(outline2Color.B - newPixel.B) < 10)
                        {
                            newImage.SetPixel(x, y, Color.FromArgb(outline2Color.A, newPixel));
                        }
                        else
                        {
                            newImage.SetPixel(x, y, Color.FromArgb(textColor.A, newPixel));
                        }
                    }

                    if (shadowPixel.A > 1 && newPixel.A < shadowAlpha)
                    {
                        newImage.SetPixel(x, y, Color.FromArgb(shadowAlpha, shadowPixel));
                    }
                }
            }

            path.Dispose();
            brush.Dispose();
            shadowPath.Dispose();

            var finalBitmap = new Bitmap(width, height);
            using (var gfx = Graphics.FromImage(finalBitmap))
            {
                if (backgroundImage != null)
                {
                    gfx.DrawImage(backgroundImage, new Point(0, 0));
                }

                gfx.DrawImage(newImage, new Point(0, 0));
            }

            newImage.Dispose();
            outlineImage.Dispose();
            gShadow.Dispose();
            gOutline.Dispose();
            g.Dispose();
            stringFormat.Dispose();
            return finalBitmap;
        }

        public static Bitmap MakeBackgroundImage(int width, int height, int rectangleSize, bool dark)
        {
            var backgroundImage = new Bitmap(width, height);
            using (var g = Graphics.FromImage(backgroundImage))
            {
                for (int y = 0; y < backgroundImage.Height; y += rectangleSize)
                {
                    for (int x = 0; x < backgroundImage.Width; x += rectangleSize)
                    {
                        var c = dark ? Color.Black : Color.WhiteSmoke;
                        if (y % (rectangleSize * 2) == 0)
                        {
                            if (x % (rectangleSize * 2) == 0)
                            {
                                c = dark ? Color.DimGray : Color.LightGray;
                            }
                        }
                        else
                        {
                            if (x % (rectangleSize * 2) != 0)
                            {
                                c = dark ? Color.DimGray : Color.LightGray;
                            }
                        }

                        using (var brush = new SolidBrush(c))
                        {
                            g.FillRectangle(brush, x, y, rectangleSize, rectangleSize);
                        }
                    }
                }
            }

            return backgroundImage;
        }
    }
}
