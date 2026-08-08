using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class TextSplitResult
    {
        public List<string> Lines { get; set; }

        /// <summary>
        /// Per-line pixel widths. Measured on first read - see <see cref="EnsurePixelsMeasured"/>.
        /// </summary>
        public List<float> LengthPixels
        {
            get
            {
                EnsurePixelsMeasured();
                return _lengthPixels;
            }
            set
            {
                _lengthPixels = value;
                _pixelsMeasured = true;
            }
        }

        public List<int> LengthCharacters { get; set; }
        public bool IsBottomHeavy => LengthPixels[1] + 2 > LengthPixels[0]; // allow a small diff of 2 pixels
        public static float SpaceLengthPixels { get; set; }
        public double TotalLength => SumLengths(Lines);
        public double TotalLengthPixels => SumPixels(LengthPixels) - SpaceLengthPixels;

        // These run inside the per-candidate sort keys of TextSplit (one candidate per space in
        // the line), where the LINQ Sum overloads allocated a closure and a boxed enumerator
        // per call - for lists that always hold two elements.
        private static double SumLengths(List<string> lines)
        {
            double total = 0;
            for (var i = 0; i < lines.Count; i++)
            {
                total += lines[i].Length;
            }

            return total;
        }

        private static float SumPixels(List<float> pixels)
        {
            // Accumulate in double and round to float at the end - same as Enumerable.Sum's
            // float overload, so the value is bit-identical to what the LINQ call produced.
            double total = 0;
            for (var i = 0; i < pixels.Count; i++)
            {
                total += pixels[i];
            }

            return (float)total;
        }

        private List<float> _lengthPixels;
        private bool _pixelsMeasured;

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

            LengthCharacters = new List<int>(lines.Count);
            foreach (var line in lines)
            {
                LengthCharacters.Add(line.Length);
            }
        }

        // TextSplit builds one of these per space in the line, but only the candidates that
        // survive the IsLineLengthOkay/IsDialog filters are ever ordered by pixel width - and
        // GetBestLengthSplit orders by character count, so its candidates need no measuring at
        // all. Measuring in the constructor meant a Skia MeasureText call per line of every
        // candidate regardless; deferring it to first read skips the ones nothing looks at.
        //
        // The body is the constructor's original code apart from the over-long-line fix in
        // EstimateOrMeasure below.
        private void EnsurePixelsMeasured()
        {
            if (_pixelsMeasured)
            {
                return;
            }

            _pixelsMeasured = true;
            _lengthPixels = new List<float>();
            if (!Configuration.Settings.Tools.AutoBreakUsePixelWidth)
            {
                return;
            }

            _lengthPixels.Add(EstimateOrMeasure(Lines[0]));
            _lengthPixels.Add(EstimateOrMeasure(Lines[1]));

            if (Math.Abs(SpaceLengthPixels) < 0.01)
            {
                SpaceLengthPixels = GetWidth(" ");
            }
        }

        // A line past the cutoff is estimated rather than measured, the same trick GetWidth
        // already uses above it for text over 128 characters.
        //
        // This used to assign the estimate to SpaceLengthPixels instead of adding it to the
        // list, which was wrong twice over. SpaceLengthPixels is a process-wide static holding
        // the width of a single space, and TotalLengthPixels subtracts it - so one long line
        // left it at (for example) 8400 instead of ~3.33 and skewed every later line break in
        // the process. And because nothing was added to the list, LengthPixels came back with
        // fewer than two entries, which IsBottomHeavy and DiffFromAveragePixelBottomHeavy index
        // directly.
        private static float EstimateOrMeasure(string line)
            => line.Length > 1000 ? line.Length * 7 : GetWidth(line);

        public bool IsLineLengthOkay(int singleLineMaxLength)
        {
            return Lines[0].Length <= singleLineMaxLength && Lines[1].Length <= singleLineMaxLength;
        }

        public double DiffFromAverage()
        {
            var lines = Lines;
            var avg = SumLengths(lines) / lines.Count;
            double diff = 0;
            for (var i = 0; i < lines.Count; i++)
            {
                diff += Math.Abs(avg - lines[i].Length);
            }

            return diff;
        }

        public double DiffFromAveragePixel()
        {
            var pixels = LengthPixels;
            var avg = (double)(SumPixels(pixels) - SpaceLengthPixels) / Lines.Count;
            double diff = 0;
            for (var i = 0; i < pixels.Count; i++)
            {
                diff += Math.Abs(avg - pixels[i]);
            }

            return diff;
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
