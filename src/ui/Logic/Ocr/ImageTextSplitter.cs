using SkiaSharp;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Ocr;

public class TextSplitter
{
    // Class to store each letter bitmap with its position in the original image
    public class LetterBitmap
    {
        public SKBitmap Bitmap { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public LetterBitmap()
        {
            Bitmap = new SKBitmap();
        }
    }

    public static List<List<LetterBitmap>> SplitTextIntoLinesAndLetters(SKBitmap image)
    {
        var lineBitmaps = SplitIntoLines(image);
        var linesWithLetters = new List<List<LetterBitmap>>();

        var yOffset = 0;
        foreach (var lineBitmap in lineBitmaps)
        {
            var letters = SplitLineIntoLetters(lineBitmap, yOffset);
            linesWithLetters.Add(letters);
            yOffset += lineBitmap.Height;
        }

        return linesWithLetters;
    }

    private static List<SKBitmap> SplitIntoLines(SKBitmap image)
    {
        var lines = new List<SKBitmap>();
        var isInLine = false;
        var lineStart = 0;

        for (var y = 0; y < image.Height; y++)
        {
            var isLineRow = false;
            for (var x = 0; x < image.Width; x++)
            {
                var color = image.GetPixel(x, y);
                if (color.Alpha > 0) // Detect non-transparent or non-white pixels
                {
                    isLineRow = true;
                    break;
                }
            }

            if (isLineRow && !isInLine)
            {
                isInLine = true;
                lineStart = y;
            }
            else if (!isLineRow && isInLine)
            {
                isInLine = false;
                var lineHeight = y - lineStart;
                var lineBitmap = new SKBitmap(image.Width, lineHeight);
                using (var canvas = new SKCanvas(lineBitmap))
                {
                    canvas.DrawBitmap(image, new SKRect(0, lineStart, image.Width, y), new SKRect(0, 0, image.Width, lineHeight));
                }
                lines.Add(lineBitmap);
            }
        }

        return lines;
    }

    private static List<LetterBitmap> SplitLineIntoLetters(SKBitmap lineBitmap, int yOffset)
    {
        var letters = new List<LetterBitmap>();
        var isInLetter = false;
        var letterStart = 0;
        var whitespaceThreshold = 3; // Small padding to handle slanted or connected letters

        for (var x = 0; x < lineBitmap.Width; x++)
        {
            var isLetterColumn = false;
            for (var y = 0; y < lineBitmap.Height; y++)
            {
                var color = lineBitmap.GetPixel(x, y);
                if (color.Alpha > 0) // Detect non-transparent pixels
                {
                    isLetterColumn = true;
                    break;
                }
            }

            if (isLetterColumn && !isInLetter)
            {
                isInLetter = true;
                letterStart = x;
            }
            else if (!isLetterColumn && isInLetter)
            {
                // Check for whitespace columns within the threshold for slanted letters
                var isWhitespace = true;
                for (var offset = 1; offset <= whitespaceThreshold; offset++)
                {
                    if (x + offset >= lineBitmap.Width) break;

                    var hasContent = false;
                    for (var y = 0; y < lineBitmap.Height; y++)
                    {
                        if (lineBitmap.GetPixel(x + offset, y).Alpha > 0)
                        {
                            hasContent = true;
                            break;
                        }
                    }
                    if (hasContent)
                    {
                        isWhitespace = false;
                        break;
                    }
                }

                if (isWhitespace)
                {
                    isInLetter = false;
                    var letterWidth = x - letterStart;
                    var letterBitmap = new SKBitmap(letterWidth + whitespaceThreshold, lineBitmap.Height);
                    using (var canvas = new SKCanvas(letterBitmap))
                    {
                        canvas.DrawBitmap(lineBitmap, new SKRect(letterStart - whitespaceThreshold, 0, x + whitespaceThreshold, lineBitmap.Height), new SKRect(0, 0, letterWidth + whitespaceThreshold, lineBitmap.Height));
                    }

                    // Add the letter with its original coordinates
                    letters.Add(new LetterBitmap
                    {
                        Bitmap = letterBitmap,
                        X = letterStart,
                        Y = yOffset,
                        Width = letterWidth + whitespaceThreshold,
                        Height = lineBitmap.Height
                    });
                }
            }
        }

        // Handle trailing letter (if any) in the line
        if (isInLetter)
        {
            var letterWidth = lineBitmap.Width - letterStart;
            var letterBitmap = new SKBitmap(letterWidth + whitespaceThreshold, lineBitmap.Height);
            using (var canvas = new SKCanvas(letterBitmap))
            {
                canvas.DrawBitmap(lineBitmap, new SKRect(letterStart - whitespaceThreshold, 0, lineBitmap.Width, lineBitmap.Height), new SKRect(0, 0, letterWidth + whitespaceThreshold, lineBitmap.Height));
            }

            letters.Add(new LetterBitmap
            {
                Bitmap = letterBitmap,
                X = letterStart,
                Y = yOffset,
                Width = letterWidth + whitespaceThreshold,
                Height = lineBitmap.Height
            });
        }

        return letters;
    }

    //public static void Main(string[] args)
    //{
    //    using (var originalImage = SKBitmap.Decode("path/to/your/image.png"))
    //    {
    //        var linesWithLetters = SplitTextIntoLinesAndLetters(originalImage);

    //        var lineIndex = 0;
    //        foreach (var line in linesWithLetters)
    //        {
    //            var letterIndex = 0;
    //            foreach (var letter in line)
    //            {
    //                // Save each letter image along with its coordinates
    //                letter.Bitmap.Encode(new SKImageInfo(letter.Width, letter.Height), SKEncodedImageFormat.Png, 100)
    //                      .SaveTo(new SKFileWStream($"line_{lineIndex}_letter_{letterIndex}.png"));

    //                Console.WriteLine($"Line {lineIndex}, Letter {letterIndex}: X={letter.X}, Y={letter.Y}, Width={letter.Width}, Height={letter.Height}");
    //                letterIndex++;
    //            }
    //            lineIndex++;
    //        }
    //    }
    //}
}

