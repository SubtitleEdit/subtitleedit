using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public static class TextWidth
    {
        public static int CalcPixelWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            using (var measureFont = new Font(Configuration.Settings.General.MeasureFontName, Configuration.Settings.General.MeasureFontSize, Configuration.Settings.General.MeasureFontBold ? FontStyle.Bold : FontStyle.Regular))
            {
                return (int)Math.Round(TextRenderer.MeasureText(text, measureFont, Size.Empty, TextFormatFlags.NoPadding).Width * 0.969f /* Correction value to remove extra padding */) + 1 /* Border */;
            }
        }
    }
}
