using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Sami : SubtitleFormat
    {
        public override string Extension => ".smi";

        public override string Name => "SAMI";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string l in lines)
            {
                sb.AppendLine(l);
            }

            if (Name == "SAMI" && sb.ToString().Contains("</SYNC>"))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var ci = CultureInfo.GetCultureInfo(language);
            language = CultureInfo.CreateSpecificCulture(ci.Name).Name;
            string languageTag = $"{language.Replace("-", string.Empty).ToUpperInvariant()}CC";
            string languageName = ci.EnglishName;
            string languageStyle = $".{languageTag} [ name: {languageName}; lang: {language.Replace("_", "-")} ; SAMIType: CC ; ]";
            languageStyle = languageStyle.Replace("[", "{").Replace("]", "}");

            string header =
@"<SAMI>
<HEAD>
<TITLE>_TITLE_</TITLE>
<SAMIParam>
  Metrics {time:ms;}
  Spec {MSFT:1.0;}
</SAMIParam>
<STYLE TYPE=""text/css"">
<!--
  P { font-family: Arial; font-weight: normal; color: white; background-color: black; text-align: center; }
  _LANGUAGE-STYLE_
-->
</STYLE>
</HEAD>
<BODY>
<-- Open play menu, choose Captions and Subtiles, On if available -->
<-- Open tools menu, Security, Show local captions when present -->";

            bool useExtra = false;
            if (!string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.StartsWith("<style", StringComparison.OrdinalIgnoreCase))
            {
                useExtra = true;
                header =
@"<SAMI>
<HEAD>
<TITLE>_TITLE_</TITLE>
<SAMIParam>
  Metrics {time:ms;}
  Spec {MSFT:1.0;}
</SAMIParam>
" + subtitle.Header.Trim() + @"
</HEAD>
<BODY>
<-- Open play menu, choose Captions and Subtiles, On if available -->
<-- Open tools menu, Security, Show local captions when present -->";
            }

            // Example text (start numbers are milliseconds)
            //<SYNC Start=65264><P>Let's go!
            //<SYNC Start=66697><P><BR>

            string paragraphWriteFormat = @"<SYNC Start={0}><P Class={3}>{2}" + Environment.NewLine +
                                           @"<SYNC Start={1}><P Class={3}>&nbsp;";
            string paragraphWriteFormatOpen = @"<SYNC Start={0}><P Class={2}>{1}";
            if (Name == new SamiModern().Name)
            {
                paragraphWriteFormat = "<SYNC Start=\"{0}\"><P Class=\"{3}\">{2}</P></SYNC>" + Environment.NewLine +
                                       "<SYNC Start=\"{1}\"><P Class=\"{3}\">&nbsp;</P></SYNC>";
                paragraphWriteFormatOpen = "<SYNC Start=\"{0}\"><P Class=\"{2}\">{1}</P></SYNC>";
            }
            else if (Name == new SamiYouTube().Name)
            {
                paragraphWriteFormat = "<SYNC Start=\"{0}\"><P Class=\"{3}\">{2}</P></SYNC>" + Environment.NewLine +
                                       "<SYNC Start=\"{1}\"><P Class=\"{3}\"></P></SYNC>";
                paragraphWriteFormatOpen = "<SYNC Start=\"{0}\"><P Class=\"{2}\">{1}</P></SYNC>";
            }

            int count = 1;
            var sb = new StringBuilder();
            sb.AppendLine(header.Replace("_TITLE_", title).Replace("_LANGUAGE-STYLE_", languageStyle));
            var totalLine = new StringBuilder();
            var partialLine = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                Paragraph next = subtitle.GetParagraphOrDefault(count);
                string text = p.Text;

                if (text.Contains('<') && text.Contains('>'))
                {
                    bool tagOn = false;
                    for (int i = 0; i < text.Length; i++)
                    {
                        string t = text.Substring(i);
                        if (t.StartsWith('<') &&
                            (t.StartsWith("<font", StringComparison.Ordinal) ||
                             t.StartsWith("<div", StringComparison.Ordinal) ||
                             t.StartsWith("<i", StringComparison.Ordinal) ||
                             t.StartsWith("<b", StringComparison.Ordinal) ||
                             t.StartsWith("<s", StringComparison.Ordinal) ||
                             t.StartsWith("</", StringComparison.Ordinal)))
                        {
                            totalLine.Append(EncodeText(partialLine.ToString()));
                            partialLine.Clear();
                            tagOn = true;
                            totalLine.Append('<');
                        }
                        else if (t.StartsWith('>') && tagOn)
                        {
                            tagOn = false;
                            totalLine.Append('>');
                        }
                        else if (!tagOn)
                        {
                            partialLine.Append(text[i]);
                        }
                        else
                        {
                            totalLine.Append(text[i]);
                        }
                    }

                    totalLine.Append(EncodeText(partialLine.ToString()));
                    text = totalLine.ToString();
                    totalLine.Clear();
                    partialLine.Clear();
                }
                else
                {
                    text = EncodeText(text);
                }

                if (Name == new SamiModern().Name)
                {
                    text = text.Replace(Environment.NewLine, "<br />");
                }
                else
                {
                    text = text.Replace(Environment.NewLine, "<br>");
                }

                string currentClass = languageTag;
                if (useExtra && !string.IsNullOrEmpty(p.Extra))
                {
                    currentClass = p.Extra;
                }

                var startMs = (long)(Math.Round(p.StartTime.TotalMilliseconds));
                var endMs = (long)(Math.Round(p.EndTime.TotalMilliseconds));
                if (next != null && Math.Abs(((long)Math.Round(next.StartTime.TotalMilliseconds)) - endMs) < 1)
                {
                    sb.AppendLine(string.Format(paragraphWriteFormatOpen, startMs, text, currentClass));
                }
                else
                {
                    sb.AppendLine(string.Format(paragraphWriteFormat, startMs, endMs, text, currentClass));
                }

                count++;
            }
            sb.AppendLine("</BODY>");
            sb.AppendLine("</SAMI>");
            return sb.ToString().Trim();
        }

        private static string EncodeText(string text)
        {
            switch (Configuration.Settings.SubtitleSettings.SamiHtmlEncodeMode)
            {
                case 1:
                    return WebUtility.HtmlEncode(text);
                case 2:
                    return HtmlUtil.EncodeNamed(text);
                case 3:
                    return HtmlUtil.EncodeNumeric(text);
            }
            return text;
        }

        public static List<string> GetStylesFromHeader(string header)
        {
            var list = new List<string>();
            if (!string.IsNullOrEmpty(header) && header.StartsWith("<style", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string line in header.SplitToLines())
                {
                    string s = line.Trim();
                    if (s.StartsWith('.') && s.IndexOf(' ') > 2)
                    {
                        string name = s.Substring(1, s.IndexOf(' ') - 1);
                        list.Add(name);
                    }
                }
            }
            else
            {
                list.Add("ENUSCC");
            }
            return list;
        }

        public static List<string> GetStylesFromSubtitle(Subtitle subtitle)
        {
            string language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var ci = CultureInfo.GetCultureInfo(language);
            language = CultureInfo.CreateSpecificCulture(ci.Name).Name;
            string languageTag = $"{language.Replace("-", string.Empty).ToUpperInvariant()}CC";
            return new List<string> { languageTag };
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string l in lines)
            {
                sb.AppendLine(l.Replace("<SYNC Start= \"", "<SYNC Start=\"").Replace("<SYNC Start = \"", "<SYNC Start=\"").Replace("<SYNC Start =\"", "<SYNC Start=\"").Replace("<SYNC  Start=\"", "<SYNC Start=\""));
            }

            string allInput = sb.ToString();
            string allInputLower = allInput.ToLowerInvariant();
            if (!allInputLower.Contains("<sync "))
            {
                return;
            }

            int styleStart = allInputLower.IndexOf("<style", StringComparison.Ordinal);
            if (styleStart > 0)
            {
                int styleEnd = allInputLower.IndexOf("</style>", StringComparison.Ordinal);
                if (styleEnd > 0)
                {
                    subtitle.Header = allInput.Substring(styleStart, styleEnd - styleStart + 8);
                }
            }

            const string syncTag = "<sync start=";
            const string syncTagEnc = "<sync encrypted=\"true\" start=";
            int syncStartPos = allInputLower.IndexOf(syncTag, StringComparison.Ordinal);
            bool hasEncryptedTags = allInput.IndexOf(syncTagEnc, StringComparison.Ordinal) >= 0;
            int index = syncStartPos + syncTag.Length;

            int syncStartPosEnc = -1;
            if (hasEncryptedTags)
            {
                syncStartPosEnc = allInputLower.IndexOf(syncTagEnc, StringComparison.Ordinal);
                if (syncStartPosEnc >= 0 && syncStartPosEnc < syncStartPos || syncStartPos == -1)
                {
                    syncStartPos = syncStartPosEnc;
                    index = syncStartPosEnc + syncTagEnc.Length;
                }
            }

            var p = new Paragraph();
            const string expectedChars = @"""'0123456789";
            while (syncStartPos >= 0)
            {
                string millisecondsAsString = string.Empty;
                while (index < allInput.Length && expectedChars.Contains(allInput[index]))
                {
                    if (allInput[index] != '"' && allInput[index] != '\'')
                    {
                        millisecondsAsString += allInput[index];
                    }

                    index++;
                }

                while (index < allInput.Length && allInput[index] != '>')
                {
                    index++;
                }

                if (index < allInput.Length && allInput[index] == '>')
                {
                    index++;
                }

                int syncEndPos = allInputLower.IndexOf(syncTag, index, StringComparison.Ordinal);
                if (hasEncryptedTags)
                {
                    int syncEndPosEnc = allInputLower.IndexOf(syncTagEnc, index, StringComparison.Ordinal);
                    if (syncStartPosEnc >= 0 && syncStartPosEnc < syncStartPos || syncEndPos == -1)
                    {
                        syncEndPos = syncEndPosEnc;
                    }
                }

                string text;
                if (syncEndPos >= 0)
                {
                    text = allInput.Substring(index, syncEndPos - index);
                }
                else
                {
                    text = allInput.Substring(index);
                }

                string textToLower = text.ToLowerInvariant();
                if (textToLower.Contains(" class="))
                {
                    var className = new StringBuilder();
                    int startClass = textToLower.IndexOf(" class=", StringComparison.Ordinal);
                    int indexClass = startClass + 7;
                    while (indexClass < textToLower.Length && (Utilities.LowercaseLettersWithNumbers + @"'""").Contains(textToLower[indexClass]))
                    {
                        className.Append(text[indexClass]);
                        indexClass++;
                    }
                    p.Extra = className.ToString().Trim(' ', '\'', '"');
                }

                if (text.Contains("ID=\"Source\"") || text.Contains("ID=Source"))
                {
                    int sourceIndex = text.IndexOf("ID=\"Source\"", StringComparison.Ordinal);
                    if (sourceIndex < 0)
                    {
                        sourceIndex = text.IndexOf("ID=Source", StringComparison.Ordinal);
                    }

                    int st = sourceIndex - 1;
                    while (st > 0 && text.Substring(st, 2).ToUpperInvariant() != "<P")
                    {
                        st--;
                    }
                    if (st > 0)
                    {
                        text = text.Substring(0, st) + text.Substring(sourceIndex);
                    }
                    int et = st;
                    while (et < text.Length - 5 && text.Substring(et, 3).ToUpperInvariant() != "<P>" && text.Substring(et, 4).ToUpperInvariant() != "</P>")
                    {
                        et++;
                    }
                    text = text.Substring(0, st) + text.Substring(et);
                }
                text = text.Replace(Environment.NewLine, " ");
                text = text.Replace("  ", " ");

                text = text.TrimEnd();
                text = Regex.Replace(text, @"<br {0,2}/?>", Environment.NewLine, RegexOptions.IgnoreCase);

                while (text.Contains("  "))
                {
                    text = text.Replace("  ", " ");
                }

                text = text.Replace("</BODY>", string.Empty).Replace("</SAMI>", string.Empty).TrimEnd();
                text = text.Replace("</body>", string.Empty).Replace("</sami>", string.Empty).TrimEnd();

                int endSyncPos = text.ToUpperInvariant().IndexOf("</SYNC>", StringComparison.OrdinalIgnoreCase);
                if (text.IndexOf('>') > 0 && (text.IndexOf('>') < endSyncPos || endSyncPos == -1))
                {
                    text = text.Remove(0, text.IndexOf('>') + 1);
                }

                text = text.TrimEnd();

                if (text.EndsWith("</sync>", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Substring(0, text.Length - 7).TrimEnd();
                }

                if (text.EndsWith("</p>", StringComparison.Ordinal) || text.EndsWith("</P>", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Substring(0, text.Length - 4).TrimEnd();
                }

                text = RemoveDiv(text).Trim();
                text = text.Replace("&nbsp;", " ").Replace("&NBSP;", " ");
                text = text.Replace("</p>", string.Empty).Replace("</sync>", string.Empty).Replace("</body>", string.Empty);
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = string.Empty;
                }

                if (text.Contains("<font color=") && !text.Contains("</font>"))
                {
                    text += "</font>";
                }

                if (text.StartsWith("<FONT COLOR=", StringComparison.Ordinal) && !text.Contains("</font>") && !text.Contains("</FONT>"))
                {
                    text += "</FONT>";
                }

                if (text.Contains('<') && text.Contains('>'))
                {
                    var total = new StringBuilder();
                    var partial = new StringBuilder();
                    bool tagOn = false;
                    for (int i = 0; i < text.Length && i < 999; i++)
                    {
                        string tmp = text.Substring(i);
                        if (tmp.StartsWith('<') &&
                            (tmp.StartsWith("<font", StringComparison.Ordinal) ||
                             tmp.StartsWith("<div", StringComparison.Ordinal) ||
                             tmp.StartsWith("<i", StringComparison.Ordinal) ||
                             tmp.StartsWith("<b", StringComparison.Ordinal) ||
                             tmp.StartsWith("<s", StringComparison.Ordinal) ||
                             tmp.StartsWith("</", StringComparison.Ordinal)))
                        {
                            total.Append(WebUtility.HtmlDecode(partial.ToString()));
                            partial.Clear();
                            tagOn = true;
                            total.Append('<');
                        }
                        else if (text.Substring(i).StartsWith('>') && tagOn)
                        {
                            tagOn = false;
                            total.Append('>');
                        }
                        else if (!tagOn)
                        {
                            partial.Append(text[i]);
                        }
                        else
                        {
                            total.Append(text[i]);
                        }
                    }
                    total.Append(WebUtility.HtmlDecode(partial.ToString()));
                    text = total.ToString();
                }
                else
                {
                    text = WebUtility.HtmlDecode(text);
                }

                var cleanText = text.FixExtraSpaces();
                cleanText = cleanText.Trim();

                if (!string.IsNullOrEmpty(p.Text) && !string.IsNullOrEmpty(millisecondsAsString))
                {
                    p.EndTime = new TimeCode(long.Parse(millisecondsAsString));
                    subtitle.Paragraphs.Add(p);
                    p = new Paragraph();
                }

                p.Text = cleanText;
                long l;
                if (long.TryParse(millisecondsAsString, out l))
                {
                    p.StartTime = new TimeCode(l);
                }

                if (syncEndPos <= 0)
                {
                    syncStartPos = -1;
                }
                else
                {
                    syncStartPos = allInputLower.IndexOf(syncTag, syncEndPos, StringComparison.Ordinal);
                    index = syncStartPos + syncTag.Length;

                    if (hasEncryptedTags)
                    {
                        syncStartPosEnc = allInputLower.IndexOf(syncTagEnc, syncEndPos, StringComparison.Ordinal);
                        if (syncStartPosEnc >= 0 && syncStartPosEnc < syncStartPos || syncStartPos == -1)
                        {
                            syncStartPos = syncStartPosEnc;
                            index = syncStartPosEnc + syncTagEnc.Length;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(p.Text) && !subtitle.Paragraphs.Contains(p))
            {
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();

            if (subtitle.Paragraphs.Count > 0 &&
                (subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text.ToUpperInvariant().Trim() == "</BODY>" ||
                subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text.ToUpperInvariant().Trim() == "<BODY>"))
            {
                subtitle.Paragraphs.RemoveAt(subtitle.Paragraphs.Count - 1);
            }

            foreach (Paragraph p2 in subtitle.Paragraphs)
            {
                p2.Text = p2.Text.Replace('\u00A0', ' '); // non-breaking space to normal space
            }
        }

        private string RemoveDiv(string text)
        {
            int indexOfDiv = text.IndexOf("<div ", StringComparison.Ordinal);
            if (indexOfDiv < 0)
            {
                indexOfDiv = text.IndexOf("<div>", StringComparison.Ordinal);
            }

            int maxLoop = 10;
            while (indexOfDiv > 0 && maxLoop >= 0)
            {
                int indexOfStartEnd = text.IndexOf(">", indexOfDiv + 1, StringComparison.Ordinal);
                if (indexOfStartEnd > 0)
                {
                    text = text.Remove(indexOfDiv, indexOfStartEnd - indexOfDiv + 1);
                    text = text.Replace("</div>", string.Empty);

                    indexOfDiv = text.IndexOf("<div ", StringComparison.Ordinal);
                    if (indexOfDiv < 0)
                    {
                        indexOfDiv = text.IndexOf("<div>", StringComparison.Ordinal);
                    }
                }
                maxLoop--;
            }
            return text;
        }

        public override bool HasStyleSupport => true;
    }
}
