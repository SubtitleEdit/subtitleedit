using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Nikse.SubtitleEdit.Core
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

        private static Graphics _graphics;
        private static Font _defaultFont;

        public TextSplitResult(List<string> lines)
        {
            Lines = lines;
            LengthPixels = new List<float>();
            if (Configuration.Settings.Tools.AutoBreakUsePixelWidth)
            {
                if (_graphics == null)
                {
                    _graphics = Graphics.FromHwnd(IntPtr.Zero);
                    _defaultFont = SystemFonts.DefaultFont;
                }

                var lineOneWidth = _graphics.MeasureString(Lines[0], _defaultFont).Width;
                LengthPixels.Add(lineOneWidth);

                var lineTwoWidth = _graphics.MeasureString(Lines[1], _defaultFont).Width;
                LengthPixels.Add(lineTwoWidth);

                if (Math.Abs(SpaceLengthPixels) < 0.01)
                {
                    SpaceLengthPixels = _graphics.MeasureString(" ", _defaultFont).Width;
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
