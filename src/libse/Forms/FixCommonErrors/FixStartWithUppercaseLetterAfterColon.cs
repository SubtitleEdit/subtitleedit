using System;
using System.Globalization;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixStartWithUppercaseLetterAfterColon : IFixCommonError
    {
        public static class Language
        {
            public static string StartWithUppercaseLetterAfterColon { get; set; } = "Start with uppercase letter after colon/semicolon";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.StartWithUppercaseLetterAfterColon;
            var noOfFixes = 0;

            var count = subtitle.Paragraphs.Count;
            var isTurkish = IsTurkish(callbacks.Language);
            // paragraph
            for (var i = 0; i < count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (!callbacks.AllowFix(p, fixAction))
                {
                    continue;
                }
                
                var text = p.Text;
                var len = text.Length;
                
                // text
                for (var j = 0; j < len; j++)
                {
                    var ch = text[j];
                    if (ch == ':' || ch == ';')
                    {
                        var k = j + 1;
                        
                        // skip white space before formatting
                        while (k < len && text[k] == ' ') k++;
                        // skip formatting e.g: <i>, <b>,<font..>...
                        while (k < len && (text[k] == '<' || text[k] == '{'))
                        {
                            var closingPair = GetClosingPair(text[k]);
                            var closeIdx = text.IndexOf(closingPair, k + 1);
                            if (closeIdx < 0)
                            {
                                k++;
                                break;
                            }
                            k = closeIdx + 1;
                        }
                        // skip whitespace after formatting
                        while (k < len && text[k] == ' ') k++;

                        if (k < len)
                        {
                            // slice from k index
                            var textFromK = text.Substring(k);
                            
                            if (CanCapitalize(textFromK, callbacks) && !isTurkish)
                            {
                                text = text.Substring(0, k) + textFromK.CapitalizeFirstLetter();
                            }
                            else if (Helper.IsTurkishLittleI(text[k], callbacks.Encoding, callbacks.Language))
                            {
                                text = text.Remove(j, 1).Insert(j, Helper.GetTurkishUppercaseLetter(text[k], callbacks.Encoding).ToString(CultureInfo.InvariantCulture));
                            }
                        }
                    }
                }

                if (text != p.Text)
                {
                    noOfFixes++;
                    var oldText = subtitle.Paragraphs[i].Text;
                    subtitle.Paragraphs[i].Text = text;
                    callbacks.AddFixToListView(subtitle.Paragraphs[i], fixAction, oldText, p.Text);
                }
            }

            callbacks.UpdateFixStatus(noOfFixes, Language.StartWithUppercaseLetterAfterColon);

            char GetClosingPair(char ch) => ch == '<' ? '>' : '}';
        }

        private static bool IsTurkish(string lang) => lang.Equals("tr", StringComparison.OrdinalIgnoreCase);

        private static bool CanCapitalize(string input, IFixCallbacks callbacks)
        {
            return !IsAppleNaming(input) && IsFirstLetterConvertibleToUppercase(input);
        }

        /// <summary>
        /// Returns true if first character is convertible to uppercase otherwise false
        /// </summary>
        private static bool IsFirstLetterConvertibleToUppercase(string input)
        {
            return input.Length > 0 && char.IsLower(input[0]);
        }

        /// <summary>
        /// Check if word is one of the apple product name e.g; iPhone, iPad, iMac...
        /// </summary>
        private static bool IsAppleNaming(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            var len = input.Length;
            if (len < 3)
            {
                return false;
            }

            return input[0] == 'i' && char.IsUpper(input[1]) && char.IsLower(input[2]);
        }
    }
}