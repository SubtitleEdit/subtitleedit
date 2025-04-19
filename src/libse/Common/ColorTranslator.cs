using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.Common
{
    internal class ColorTranslator
    {
        internal static SKColor FromHtml(string htmlColor)
        {
            return SKColor.Parse(htmlColor);
        }
    }
}