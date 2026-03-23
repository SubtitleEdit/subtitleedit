using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class CultureExtensions
    {
        /// <summary>
        /// Get three letter ISO code from CultureInfo object (three letter ISO code seems to be blank sometimes on Mono/Wine).
        /// </summary>
        /// <param name="cultureInfo">CultureInfo object</param>
        /// <returns>Three letter ISO language code, if failure then string.Empty is returned.</returns>
        public static string GetThreeLetterIsoLanguageName(this CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                return string.Empty;
            }

            var cultureThreeLetterIsoLanguageName = cultureInfo.ThreeLetterISOLanguageName;
            if (string.IsNullOrEmpty(cultureThreeLetterIsoLanguageName))
            {
                cultureThreeLetterIsoLanguageName = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(cultureInfo.TwoLetterISOLanguageName);
            }

            return cultureThreeLetterIsoLanguageName;
        }
    }
}
