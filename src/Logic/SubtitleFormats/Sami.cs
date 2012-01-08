using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

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

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string language = Utilities.AutoDetectLanguageName("en_US", subtitle);
            CultureInfo ci = CultureInfo.GetCultureInfo(language.Replace("_", "-"));
            string languageTag = string.Format("{0}CC", language.Replace("_", string.Empty).ToUpper());
            string languageName = ci.EnglishName;
            if (ci.Parent != null)
                languageName = ci.Parent.EnglishName;
            string languageStyle = string.Format(".{0} [ name: {1}; lang: {2} ; SAMIType: CC ; ]", languageTag, languageName, language.Replace("_", "-"));
            languageStyle = languageStyle.Replace("[", "{").Replace("]", "}");

            const string header =
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
            // Example text (start numbers are milliseconds)
            //<SYNC Start=65264><P>Let's go!
            //<SYNC Start=66697><P><BR>

            const string paragraphWriteFormat =
@"<SYNC Start={0}><P Class={3}>{2}</P></SYNC>
<SYNC Start={1}><P Class={3}>&nbsp;</P></SYNC>";

            var sb = new StringBuilder();
            sb.AppendLine(header.Replace("_TITLE_", title).Replace("_LANGUAGE-STYLE_", languageStyle));
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(paragraphWriteFormat, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds, p.Text.Replace(Environment.NewLine, "<br />"), languageTag));
            }
            sb.AppendLine("</BODY>");
            sb.AppendLine("</SAMI>");
            return sb.ToString().Trim();
        }

        public List<string> GetClasses(Subtitle subtitle)
        {
            var list = new List<string>();
            if (!string.IsNullOrEmpty(subtitle.Header) && subtitle.Header.ToLower().StartsWith("<style"))
            {
                foreach (string line in subtitle.Header.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    string s = line.Trim();
                    if (s.StartsWith(".") && s.IndexOf(" ") > 2)
                    {
                        string name = s.Substring(1, s.IndexOf(" ") - 1);
                        list.Add(name);
                    }
                }
            }
            return list;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string l in lines)
                sb.AppendLine(l);
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
                while (index < allInput.Length && "0123456789".Contains(allInput[index].ToString()))
                {
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
                    while (indexClass < textToLower.Length && Utilities.GetLetters(false, true, true).Contains(textToLower[indexClass].ToString()))
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
                string cleanText = string.Empty;
                bool tagOn = false;
                for (int i = 0; i < text.Length; i++)
                { // Remove html tags from text
                    if (text[i] == '<')
                        tagOn = true;
                    else if (text[i] == '>')
                        tagOn = false;
                    else if (!tagOn)
                        cleanText += text[i];
                }
                cleanText = cleanText.Replace("&nbsp;", string.Empty).Replace("&NBSP;", string.Empty);
                cleanText = cleanText.Trim();

                if (!string.IsNullOrEmpty(p.Text))
                {
                    p.EndTime = new TimeCode(TimeSpan.FromMilliseconds(long.Parse(millisecAsString)));
                    subtitle.Paragraphs.Add(p);
                    p = new Paragraph();
                }

                p.Text = cleanText;
                p.StartTime = new TimeCode(TimeSpan.FromMilliseconds(long.Parse(millisecAsString)));

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
            subtitle.Renumber(1);
        }
    }
}
