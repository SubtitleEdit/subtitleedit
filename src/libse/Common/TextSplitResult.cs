using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class TextSplitResult
    {
        public List<string> Lines { get; set; }
        public List<float> LengthPixels { get; set; }
        public List<int> LengthCharacters { get; set; }
        public bool IsBottomHeavy => LengthPixels[1] + 2 > LengthPixels[0]; // allow a small diff of 2 pixels
        public static float SpaceLengthPixels { get; set; }
        public double TotalLength => Lines.Sum(p => p.Length);
        public double TotalLengthPixels => LengthPixels.Sum(p => p) - SpaceLengthPixels;

        // SkiaSharp resolves a default typeface the first time SKFont's object
        // initializer runs; on headless Linux setups without fontconfig (e.g. the
        // eclipse-temurin:21-jre-noble container reported in issue #11314) that
        // discovery throws. The fields used to have inline `= new SKFont {...}`
        // initializers, so a single failure broke the entire class's .cctor and
        // every subsequent line-wrap call hit a TypeInitializationException.
        //
        // Moving construction into an explicit static constructor lets us catch the
        // failure and leave _font / _paint as null. GetWidth then falls back to the
        // same length × 5 heuristic the >128 char branch already uses — pixel-
        // precise wrapping isn't possible, but the resulting layout is still a
        // valid two-line split, so AutoBreak / SCC line-wrap keep working.
        private static readonly SKFont _font;
        private static readonly SKPaint _paint;
        private static readonly bool _fontInitialized;

        static TextSplitResult()
        {
            try
            {
                _font = new SKFont { Size = 12f };
                _paint = new SKPaint { IsAntialias = false };
                _fontInitialized = true;
            }
            catch
            {
                _fontInitialized = false;
            }
        }

        public static float GetWidth(string text)
        {
            if (text.Length > 128 || !_fontInitialized)
            {
                return text.Length * 5;
            }

            try
            {
                return _font.MeasureText(text, _paint);
            }
            catch
            {
                return text.Length * 5;
            }
        }

        public TextSplitResult(List<string> lines)
        {
            Lines = lines;
            LengthPixels = new List<float>();
            if (Configuration.Settings.Tools.AutoBreakUsePixelWidth)
            {
                if (Lines[0].Length > 1000)
                {
                    SpaceLengthPixels = Lines[0].Length * 7;
                }
                else
                {
                    LengthPixels.Add(GetWidth(Lines[0]));
                }

                if (Lines[1].Length > 1000)
                {
                    SpaceLengthPixels = Lines[1].Length * 7;
                }
                else
                {
                    LengthPixels.Add(GetWidth(Lines[1]));
                }

                if (Math.Abs(SpaceLengthPixels) < 0.01)
                {
                    SpaceLengthPixels = GetWidth(" ");
                }
            }

            LengthCharacters = new List<int>();
            foreach (var line in lines)
            {
                LengthCharacters.Add(line.Length);
            }
        }

        public bool IsLineLengthOkay(int singleLineMaxLength)
        {
            return Lines[0].Length <= singleLineMaxLength && Lines[1].Length <= singleLineMaxLength;
        }

        public double DiffFromAverage()
        {
            var avg = TotalLength / Lines.Count;
            return Lines.Sum(line => Math.Abs(avg - line.Length));
        }

        public double DiffFromAveragePixel()
        {
            var avg = TotalLengthPixels / Lines.Count;
            return LengthPixels.Sum(w => Math.Abs(avg - w));
        }

        public double DiffFromAveragePixelBottomHeavy()
        {
            var avg = TotalLengthPixels / Lines.Count;
            double diff = 0;
            double bottomHeavyPercentageFactor = 0;
            if (Configuration.Settings.Tools.AutoBreakPreferBottomHeavy)
            {
                bottomHeavyPercentageFactor = Configuration.Settings.Tools.AutoBreakPreferBottomPercent / 100.0;
            }
            var bottomDiffPixels = avg * bottomHeavyPercentageFactor;
            diff += Math.Abs(avg - bottomDiffPixels - LengthPixels[0]);
            diff += Math.Abs(avg + bottomDiffPixels - LengthPixels[1]);
            return diff;
        }
    }
}
