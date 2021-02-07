using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Drawing;

namespace Nikse.SubtitleEdit.Logic
{
    public static class TextWidth
    {
        private static readonly Graphics Graphics = Graphics.FromHwnd(IntPtr.Zero);
        private static readonly object GdiLock = new object();

        public static int CalcPixelWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            using (var measureFont = new Font(Configuration.Settings.General.MeasureFontName, Configuration.Settings.General.MeasureFontSize, Configuration.Settings.General.MeasureFontBold ? FontStyle.Bold : FontStyle.Regular))
            {
                lock (GdiLock)
                {
                    // MeasureString adds padding, se we'll calculate the length of 2x the text + 
                    // padding, and substract the length of 1x the text + padding.
                    // I.e. [testtest] - [test] = length of 'test' without padding.
                    var measuredWidth = Graphics.MeasureString(text, measureFont).Width;
                    var measuredDoubleWidth = Graphics.MeasureString(text + text, measureFont).Width;
                    return (int)Math.Round(measuredDoubleWidth - measuredWidth);
                }
            }
        }
    }
}
