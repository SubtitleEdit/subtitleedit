using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public static class FileNameHelper
    {
        public static string GetFileNameWithTargetLanguage(string oldFileName, string videoFileName, Subtitle oldSubtitle, SubtitleFormat subtitleFormat, string sourceIsoCode, string targetIsoCode)
        {
            if (string.IsNullOrEmpty(targetIsoCode))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(videoFileName))
            {
                return Path.GetFileNameWithoutExtension(videoFileName) + "." + targetIsoCode.ToLowerInvariant() + subtitleFormat.Extension;
            }

            if (!string.IsNullOrEmpty(oldFileName))
            {
                var s = Path.GetFileNameWithoutExtension(oldFileName);
                if (s.EndsWith("." + sourceIsoCode, StringComparison.OrdinalIgnoreCase))
                {
                    s = s.Substring(0, s.Length - sourceIsoCode.Length - 1);
                }

                if (oldSubtitle != null)
                {
                    var lang = "." + LanguageAutoDetect.AutoDetectGoogleLanguage(oldSubtitle);
                    if (lang.Length == 3 && s.EndsWith(lang, StringComparison.OrdinalIgnoreCase))
                    {
                        s = s.Remove(s.Length - 3);
                    }
                }

                return s + "." + targetIsoCode.ToLowerInvariant() + subtitleFormat.Extension;
            }

            return null;
        }
    }
}
