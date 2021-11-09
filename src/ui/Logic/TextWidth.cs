using Nikse.SubtitleEdit.Core.Common;
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
                // MeasureString adds padding, se we'll calculate the length of 2x the text + 
                // padding, and substract the length of 1x the text + padding.
                // I.e. [testtest] - [test] = length of 'test' without padding.
                int measuredWidth = TextRenderer.MeasureText(text, measureFont, Size.Empty, TextFormatFlags.NoPadding).Width;
                int measuredDoubleWidth = TextRenderer.MeasureText(text + text, measureFont, Size.Empty, TextFormatFlags.NoPadding).Width;
                return measuredDoubleWidth - measuredWidth;
            }
        }
    }
}
