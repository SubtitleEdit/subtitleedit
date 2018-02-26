﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nikse.SubtitleEdit.Core.Dictionaries;

namespace Nikse.SubtitleEdit.Core
{
    public class FixCasing
    {
        public bool FixNormal = true;
        public bool FixNormalOnlyAllUppercase = false;
        public bool FixMakeLowercase = false;
        public bool FixMakeUppercase = false;

        private readonly string _language;
        private readonly List<string> _names;

        public FixCasing(string language)
        {
            _language = language;
            var nameList = new NameList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            _names = nameList.GetAllNames();

            // Longer names must be first
            _names.Sort((s1, s2) => s2.Length.CompareTo(s1.Length));
        }

        public int NoOfLinesChanged { get; set; }

        public void Fix(Subtitle subtitle)
        {
            var subCulture = GetCultureInfoFromLanguage(_language);
            Paragraph last = null;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (last != null)
                {
                    p.Text = Fix(p.Text, last.Text, _names, subCulture, p.StartTime.TotalMilliseconds - last.EndTime.TotalMilliseconds);
                }
                else
                {
                    p.Text = Fix(p.Text, string.Empty, _names, subCulture, 10000);
                }

                // fix casing of English alone i to I
                if (FixNormal && _language.StartsWith("en", StringComparison.Ordinal))
                {
                    p.Text = FixEnglishAloneILowerToUpper(p.Text);
                    p.Text = FixCasingAfterTitles(p.Text);
                }

                last = p;
            }
        }

        private static CultureInfo GetCultureInfoFromLanguage(string language)
        {
            try
            {
                return CultureInfo.GetCultureInfo(language);
            }
            catch
            {
                return CultureInfo.CurrentUICulture;
            }
        }

        private static string FixEnglishAloneILowerToUpper(string text)
        {
            const string pre = " >¡¿♪♫([";
            const string post = " <!?.:;,♪♫)]";

            if (text.StartsWith("I-i ", StringComparison.Ordinal))
                text = text.Remove(0, 3).Insert(0, "I-I");
            if (text.StartsWith("I-if ", StringComparison.Ordinal))
                text = text.Remove(0, 4).Insert(0, "I-If ");

            for (var indexOfI = text.IndexOf('i'); indexOfI >= 0; indexOfI = text.IndexOf('i', indexOfI + 1))
            {
                if (indexOfI == 0 || pre.Contains(text[indexOfI - 1]))
                {
                    if (text.Substring(indexOfI).StartsWith("i-i ", StringComparison.Ordinal))
                    {
                        text = text.Remove(indexOfI, 3).Insert(indexOfI, "I-I");
                    }
                    else if (text.Substring(indexOfI).StartsWith("i-if ", StringComparison.Ordinal))
                    {
                        text = text.Remove(indexOfI, 4).Insert(indexOfI, "I-If");
                    }
                    else if (indexOfI + 1 == text.Length || post.Contains(text[indexOfI + 1]))
                    {
                        text = text.Remove(indexOfI, 1).Insert(indexOfI, "I");
                    }
                    else if (indexOfI > 1 && indexOfI < text.Length - 2 && "\r\n".Contains(text[indexOfI + 1]) && text[indexOfI - 1] == ' ')
                    {
                        text = text.Remove(indexOfI, 1).Insert(indexOfI, "I");
                    }
                }
                if (indexOfI > 1 && indexOfI < text.Length - 2 && "\r\n".Contains(text[indexOfI - 1]) && " .?!".Contains(text[indexOfI + 1]))
                {
                    text = text.Remove(indexOfI, 1).Insert(indexOfI, "I");
                }
                else if (indexOfI > 1 && "\r\n ".Contains(text[indexOfI - 1]) && text.Substring(indexOfI).StartsWith("i-i ", StringComparison.Ordinal))
                {
                    text = text.Remove(indexOfI, 3).Insert(indexOfI, "I-I");
                }
                else if (indexOfI >= 1 && indexOfI < text.Length - 2 && "“\"".Contains(text[indexOfI - 1]) && " .?!".Contains(text[indexOfI + 1]))
                {
                    text = text.Remove(indexOfI, 1).Insert(indexOfI, "I");
                }
                else if (indexOfI > 2 && text.Substring(indexOfI - 2).StartsWith("I-i ", StringComparison.Ordinal))
                {
                    text = text.Remove(indexOfI - 2, 3).Insert(indexOfI - 2, "I-I");
                }
                else if (indexOfI > 2 && text.Substring(indexOfI - 2).StartsWith("I-it's ", StringComparison.Ordinal))
                {
                    text = text.Remove(indexOfI - 2, 3).Insert(indexOfI - 2, "I-I");
                }
                else if (text.Substring(indexOfI).StartsWith("i'll ", StringComparison.Ordinal))
                {
                    text = text.Remove(indexOfI, 1).Insert(indexOfI, "I");
                }
                else if (text.Substring(indexOfI).StartsWith("i've ", StringComparison.Ordinal))
                {
                    text = text.Remove(indexOfI, 1).Insert(indexOfI, "I");
                }
                else if (text.Substring(indexOfI).StartsWith("i'm ", StringComparison.Ordinal))
                {
                    text = text.Remove(indexOfI, 1).Insert(indexOfI, "I");
                }
                else if (text.Substring(indexOfI).StartsWith("i'd ", StringComparison.Ordinal))
                {
                    text = text.Remove(indexOfI, 1).Insert(indexOfI, "I");
                }
            }
            return text;
        }

        private string FixCasingAfterTitles(string text)
        {
            var titles = new[] { "Mrs.", "Miss.", "Mr.", "Ms.", "Dr." };
            var notChangeWords = new[] { "does", "has", "will", "is", "and", "for", "but", "or", "of" };
            for (int i = 0; i < text.Length - 4; i++)
            {
                var start = text.Substring(i);
                foreach (var title in titles)
                {
                    if (start.StartsWith(title, StringComparison.OrdinalIgnoreCase))
                    {
                        var idx = i + title.Length;
                        if (idx < text.Length - 2 && text[idx] == ' ')
                        {
                            idx++;
                            var words = text.Substring(idx).Split(' ', '\r', '\n', ',', '"', '?', '!', '.', '\'');
                            if (words.Length > 0 && !notChangeWords.Contains(words[0]))
                            {
                                var upper = text[idx].ToString().ToUpper();
                                text = text.Remove(idx, 1).Insert(idx, upper);
                            }
                        }
                        break;
                    }
                }
            }
            return text;
        }

        private string Fix(string text, string lastLine, List<string> nameList, CultureInfo subtitleCulture, double millisecondsFromLast)
        {
            string original = text;
            if (FixNormal)
            {
                if (FixNormalOnlyAllUppercase && text != text.ToUpper(subtitleCulture))
                    return text;

                if (text.Length > 1)
                {
                    // first all to lower
                    text = text.ToLower(subtitleCulture).Trim();
                    text = text.FixExtraSpaces();
                    var st = new StrippableText(text);
                    st.FixCasing(nameList, false, true, true, lastLine, millisecondsFromLast); // fix all casing but names (that's a seperate option)
                    text = st.MergedString;
                }
            }
            else if (FixMakeUppercase)
            {
                var st = new StrippableText(text);
                text = st.Pre + st.StrippedText.ToUpper(subtitleCulture) + st.Post;
                text = HtmlUtil.FixUpperTags(text); // tags inside text
            }
            else if (FixMakeLowercase)
            {
                text = text.ToLower(subtitleCulture);
            }
            if (original != text)
                NoOfLinesChanged++;
            return text;
        }

    }
}
