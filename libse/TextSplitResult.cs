using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Nikse.SubtitleEdit.Core
{
    public class TextSplitResult
    {
        public TextSplitResult(List<string> lines)
        {
            Lines = lines;
        }

        public List<string> Lines { get; set; }

        public double TotalLength
        {
            get { return Lines.Sum(p => p.Length); }
        }

        public double TotalLengthPixels
        {
            get
            {
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    var sum = Lines.Sum(p => g.MeasureString(p, new Font(SystemFonts.DefaultFont.FontFamily, 10)).Width);
                    return Math.Max(0, sum - g.MeasureString(" ", new Font(SystemFonts.DefaultFont.FontFamily, 10)).Width);
                }
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
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return Lines.Select(line => g.MeasureString(line, new Font(SystemFonts.DefaultFont.FontFamily, 10)).Width).Select(w => Math.Abs(avg - w)).Sum();
            }
        }
    }
}
