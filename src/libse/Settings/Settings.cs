using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class Settings
    {
        public GeneralSettings General { get; set; }
        public ToolsSettings Tools { get; set; }
        public SubtitleSettings SubtitleSettings { get; set; }
        public ProxySettings Proxy { get; set; }
        public BeautifyTimeCodesSettings BeautifyTimeCodes { get; set; }
        public RemoveTextForHearingImpairedSettings RemoveTextForHearingImpaired { get; set; }

        public void Reset()
        {
            General = new GeneralSettings();
            Tools = new ToolsSettings();
            SubtitleSettings = new SubtitleSettings();
            Proxy = new ProxySettings();
            BeautifyTimeCodes = new BeautifyTimeCodesSettings();
            RemoveTextForHearingImpaired = new RemoveTextForHearingImpairedSettings();
        }

        public Settings()
        {
            Reset();
        }

        public static string ToHtml(SKColor c)
        {
            return Utilities.ColorToHexWithTransparency(c);
        }

        public static SKColor FromHtml(string hex)
        {
            var s = hex.Trim().TrimStart('#');

            if (s.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
            {
                var arr = s.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length >= 3)
                {
                    try
                    {
                        return ColorUtils.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                    }
                    catch
                    {
                        return SKColors.White;
                    }
                }

                return SKColors.White;
            }


            if (s.Length == 6)
            {
                try
                {
                    return ColorTranslator.FromHtml("#" + s);
                }
                catch
                {
                    return SKColors.White;
                }
            }

            if (s.Length == 8)
            {
                if (!int.TryParse(s.Substring(6, 2), NumberStyles.HexNumber, null, out var alpha))
                {
                    alpha = 255; // full solid color
                }

                s = s.Substring(0, 6);
                try
                {
                    var c = HtmlUtil.GetColorFromString("#" + s);
                    return ColorUtils.FromArgb(alpha, c);
                }
                catch
                {
                    return SKColors.White;
                }
            }

            return SKColors.White;
        }
    }
}
