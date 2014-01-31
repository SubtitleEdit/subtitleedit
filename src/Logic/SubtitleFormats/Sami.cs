using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Sami : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".smi"; }
        }

        public override string Name
        {
            get { return "SAMI"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string l in lines)
                sb.AppendLine(l);
            if (sb.ToString().Contains("</SYNC>"))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string language = Utilities.AutoDetectLanguageName("en_US", subtitle);
            var ci = CultureInfo.GetCultureInfo(language.Replace("_", "-"));
            string languageTag = string.Format("{0}CC", language.Replace("_", string.Empty).ToUpper());
            string languageName = ci.EnglishName;
            if (ci.Parent != null)
                languageName = ci.Parent.EnglishName;
            string languageStyle = string.Format(".{0} [ name: {1}; lang: {2} ; SAMIType: CC ; ]", languageTag, languageName, language.Replace("_", "-"));
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
<-- Open tools menu, Security, Show local captions when present -->
";

            bool useExtra = false;
            if (!string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.ToLower().StartsWith("<style"))
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
<-- Open tools menu, Security, Show local captions when present -->
";
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

            int count = 1;
            var sb = new StringBuilder();
            sb.AppendLine(header.Replace("_TITLE_", title).Replace("_LANGUAGE-STYLE_", languageStyle));
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                Paragraph next = subtitle.GetParagraphOrDefault(count);
                string text = p.Text;

                if (text.Contains("<") && text.Contains(">"))
                {
                    var total = new StringBuilder();
                    var partial = new StringBuilder();
                    bool tagOn = false;
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (text.Substring(i).StartsWith("<font") ||
                            text.Substring(i).StartsWith("<div") ||
                            text.Substring(i).StartsWith("<i") ||
                            text.Substring(i).StartsWith("<b") ||
                            text.Substring(i).StartsWith("<s") ||
                            text.Substring(i).StartsWith("</"))
                        {
                            if (Configuration.Settings.SubtitleSettings.SamiFullHtmlEncode && Configuration.Settings.SubtitleSettings.SamiFullHtmlEncodeNumeric)
                                total.Append(Utilities.HtmlEncodeFullNumeric(partial.ToString()));
                            else if (Configuration.Settings.SubtitleSettings.SamiFullHtmlEncode)
                                total.Append(Utilities.HtmlEncodeFull(partial.ToString()));
                            else
                                total.Append(Utilities.HtmlEncode(partial.ToString()));
                            partial = new StringBuilder();
                            tagOn = true;
                            total.Append("<");
                        }
                        else if (text.Substring(i).StartsWith(">") && tagOn)
                        {
                            tagOn = false;
                            total.Append(">");
                        }
                        else if (!tagOn)
                        {
                            partial.Append(text.Substring(i, 1));
                        }
                        else if (tagOn)
                        {
                            total.Append(text.Substring(i, 1));
                        }
                    }
                    if (Configuration.Settings.SubtitleSettings.SamiFullHtmlEncode && Configuration.Settings.SubtitleSettings.SamiFullHtmlEncodeNumeric)
                        total.Append(Utilities.HtmlEncodeFullNumeric(partial.ToString()));
                    else if (Configuration.Settings.SubtitleSettings.SamiFullHtmlEncode)
                        total.Append(Utilities.HtmlEncodeFull(partial.ToString()));
                    else
                        total.Append(Utilities.HtmlEncode(partial.ToString()));
                    text = total.ToString();
                }
                else
                {
                    if (Configuration.Settings.SubtitleSettings.SamiFullHtmlEncode && Configuration.Settings.SubtitleSettings.SamiFullHtmlEncodeNumeric)
                        text = Utilities.HtmlEncodeFullNumeric(text);
                    else if (Configuration.Settings.SubtitleSettings.SamiFullHtmlEncode)
                        text = Utilities.HtmlEncodeFull(text);
                    else
                        text = Utilities.HtmlEncode(text);
                }

                if (Name == new SamiModern().Name)
                    text = text.Replace(Environment.NewLine, "<br />");
                else
                    text = text.Replace(Environment.NewLine, "<br>");

                string currentClass = languageTag;
                if (useExtra && !string.IsNullOrEmpty(p.Extra))
                    currentClass = p.Extra;
                if (next != null && Math.Abs(next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds) < 100)
                    sb.AppendLine(string.Format(paragraphWriteFormatOpen, p.StartTime.TotalMilliseconds, text, currentClass));
                else
                    sb.AppendLine(string.Format(paragraphWriteFormat, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds, text, currentClass));
                count++;
            }
            sb.AppendLine("</BODY>");
            sb.AppendLine("</SAMI>");
            return sb.ToString().Trim();
        }

        public static List<string> GetStylesFromHeader(string header)
        {
            var list = new List<string>();
            if (!string.IsNullOrEmpty(header) && header.ToLower().StartsWith("<style"))
            {
                foreach (string line in header.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    string s = line.Trim();
                    if (s.StartsWith(".") && s.IndexOf(" ") > 2)
                    {
                        string name = s.Substring(1, s.IndexOf(" ") - 1);
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

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string l in lines)
                sb.AppendLine(l.Replace("<SYNC Start= \"", "<SYNC Start=\"").Replace("<SYNC Start = \"", "<SYNC Start=\"").Replace("<SYNC Start =\"", "<SYNC Start=\"").Replace("<SYNC  Start=\"", "<SYNC Start=\""));
            string allInput = sb.ToString();
            string allInputLower = allInput.ToLower();

            int styleStart = allInputLower.IndexOf("<style");
            if (styleStart > 0)
            {
                int styleEnd = allInputLower.IndexOf("</style>");
                if (styleEnd > 0)
                {
                    subtitle.Header = allInput.Substring(styleStart, styleEnd - styleStart + 8);
                }
            }

            const string syncTag = "<sync start=";
            int syncStartPos = allInputLower.IndexOf(syncTag);
            int index = syncStartPos + syncTag.Length;
            var p = new Paragraph();
            while (syncStartPos >= 0)
            {
                string millisecAsString = string.Empty;
                while (index < allInput.Length && "\"'0123456789".Contains(allInput[index].ToString()))
                {
                    if (allInput[index] != '"' && allInput[index] != '\'')
                        millisecAsString += allInput[index];
                    index++;
                }

                while (index < allInput.Length && allInput[index] != '>')
                    index++;
                if (index < allInput.Length && allInput[index] == '>')
                    index++;

                int syncEndPos = allInputLower.IndexOf(syncTag, index);
                string text;
                if (syncEndPos >= 0)
                    text = allInput.Substring(index, syncEndPos - index);
                else
                    text = allInput.Substring(index);

                string textToLower = text.ToLower();
                if (textToLower.Contains(" class="))
                {
                    var className = new StringBuilder();
                    int startClass = textToLower.IndexOf(" class=");
                    int indexClass = startClass + 7;
                    while (indexClass < textToLower.Length && (Utilities.LowercaseLettersWithNumbers + "'\"").Contains(textToLower[indexClass].ToString()))
                    {
                        className.Append(text[indexClass].ToString());
                        indexClass++;
                    }
                    p.Extra = className.ToString().Trim(" '\"".ToCharArray());
                }

                if (text.Contains("ID=\"Source\"") || text.Contains("ID=Source"))
                {
                    int sourceIndex = text.IndexOf("ID=\"Source\"");
                    if (sourceIndex < 0)
                        sourceIndex = text.IndexOf("ID=Source");
                    int st = sourceIndex -1;
                    while (st > 0 && text.Substring(st, 2).ToUpper() != "<P")
                    {
                        st--;
                    }
                    if (st > 0)
                    {
                        text = text.Substring(0, st) + text.Substring(sourceIndex);
                    }
                    int et = st;
                    while (et < text.Length - 5 && text.Substring(et, 3).ToUpper() != "<P>" && text.Substring(et, 4).ToUpper() != "</P>")
                    {
                        et++;
                    }
                    text = text.Substring(0, st) + text.Substring(et);
                }
                text = text.Replace(Environment.NewLine, " ");
                text = text.Replace("  ", " ");

                text = text.TrimEnd();
                text = text.Replace("<BR>", Environment.NewLine);
                text = text.Replace("<BR/>", Environment.NewLine);
                text = text.Replace("<BR />", Environment.NewLine);
                text = text.Replace("<br>", Environment.NewLine);
                text = text.Replace("<br/>", Environment.NewLine);
                text = text.Replace("<br />", Environment.NewLine);
                while (text.Contains("  "))
                    text = text.Replace("  ", " ");
                text = text.Replace("</BODY>", string.Empty).Replace("</SAMI>", string.Empty).TrimEnd();

                if (text.IndexOf(">") > 0)
                    text = text.Remove(0, text.IndexOf(">")+1);
                text = text.TrimEnd();

                if (text.ToLower().EndsWith("</sync>"))
                    text = text.Substring(0, text.Length - 7).TrimEnd();

                if (text.EndsWith("</p>") || text.EndsWith("</P>"))
                    text = text.Substring(0, text.Length - 4).TrimEnd();

                text = text.Replace("&nbsp;", " ").Replace("&NBSP;", " ");

                if (text.Contains("<font color=") && !text.Contains("</font>"))
                    text += "</font>";
                if (text.StartsWith("<FONT COLOR=") && !text.Contains("</font>") && !text.Contains("</FONT>"))
                    text += "</FONT>";


                if (text.Contains("<") && text.Contains(">"))
                {
                    var total = new StringBuilder();
                    var partial = new StringBuilder();
                    bool tagOn = false;
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (text.Substring(i).StartsWith("<font") ||
                            text.Substring(i).StartsWith("<div") ||
                            text.Substring(i).StartsWith("<i") ||
                            text.Substring(i).StartsWith("<b") ||
                            text.Substring(i).StartsWith("<s") ||
                            text.Substring(i).StartsWith("</"))
                        {
                            total.Append(Utilities.HtmlDecode(partial.ToString()));
                            partial = new StringBuilder();
                            tagOn = true;
                            total.Append("<");
                        }
                        else if (text.Substring(i).StartsWith(">") && tagOn)
                        {
                            tagOn = false;
                            total.Append(">");
                        }
                        else if (!tagOn)
                        {
                            partial.Append(text.Substring(i, 1));
                        }
                        else if (tagOn)
                        {
                            total.Append(text.Substring(i, 1));
                        }
                    }
                    total.Append(Utilities.HtmlDecode(partial.ToString()));
                    text = total.ToString();
                }
                else
                {
                    text = Utilities.HtmlDecode(text);
                }

                string cleanText = text;
                while (cleanText.Contains("  "))
                    cleanText = cleanText.Replace("  ", " ");
                while (cleanText.Contains(Environment.NewLine + " "))
                    cleanText = cleanText.Replace(Environment.NewLine + " ", Environment.NewLine);
                while (cleanText.Contains(" " + Environment.NewLine))
                    cleanText = cleanText.Replace(" " + Environment.NewLine, Environment.NewLine);
                cleanText = cleanText.Trim();

                if (!string.IsNullOrEmpty(p.Text) && !string.IsNullOrEmpty(millisecAsString))
                {
                    p.EndTime = new TimeCode(TimeSpan.FromMilliseconds(long.Parse(millisecAsString)));
                    subtitle.Paragraphs.Add(p);
                    p = new Paragraph();
                }

                p.Text = cleanText;
                long l;
                if (long.TryParse(millisecAsString, out l))
                    p.StartTime = new TimeCode(TimeSpan.FromMilliseconds(l));

                if (syncEndPos <= 0)
                {
                    syncStartPos = -1;
                }
                else
                {
                    syncStartPos = allInputLower.IndexOf(syncTag, syncEndPos);
                    index = syncStartPos + syncTag.Length;
                }
            }
            if (p != null && !string.IsNullOrEmpty(p.Text) && subtitle.Paragraphs.IndexOf(p) == -1)
            {
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds +  Utilities.GetOptimalDisplayMilliseconds(p.Text);
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber(1);

            if (subtitle.Paragraphs.Count > 0 &&
                (subtitle.Paragraphs[subtitle.Paragraphs.Count-1].Text.ToUpper().Trim() == "</BODY>" ||
                subtitle.Paragraphs[subtitle.Paragraphs.Count-1].Text.ToUpper().Trim() == "<BODY>"))
                subtitle.Paragraphs.RemoveAt(subtitle.Paragraphs.Count - 1);
        }

        public override bool HasStyleSupport
        {
            get { return true; }
        }

    }
}
