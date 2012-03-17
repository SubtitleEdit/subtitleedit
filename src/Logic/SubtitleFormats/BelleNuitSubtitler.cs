using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class BelleNuitSubtitler : SubtitleFormat
    {
        ///tc 00:00:35:09 00:00:38:05
        static readonly Regex RegexTimeCode = new Regex(@"^\/tc \d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".stp"; }
        }

        public override string Name
        {
            get { return "Belle Nuit Subtitler"; }
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
            const string paragraphWriteFormat = "/tc {0} {1}{2}{3}{2}";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), Environment.NewLine, p.Text));
            }

            var doc = new XmlDocument();
            doc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine + @"<xmldict>
	<key>document</key>
	<dict>
		<key>creator</key>
		<string>SICT</string>
		<key>type</key>
		<string>STLI</string>
		<key>version</key>
		<real>1.4</real>
		<key>applicationversion</key>
		<string>Belle Nuit Subtitler 1.7.8</string>
		<key>creationdate</key>
		<date>2012-03-13 16:30:32</date>
		<key>modificationdate</key>
		<date>2012-03-13 16:30:32</date>
	</dict>
	<key>mainleft</key>
	<integer>40</integer>
	<key>maintop</key>
	<integer>48</integer>
	<key>mainwidth</key>
	<integer>825</integer>
	<key>mainheight</key>
	<integer>886</integer>
	<key>styledt</key>
	<false/>
	<key>exportdt</key>
	<true/>
	<key>previewdt</key>
	<true/>
	<key>moviedt</key>
	<true/>
	<key>exportformat</key>
	<string>TIFF</string>
	<key>style</key>
	<dict>
		<key>font</key>
		<string>Geneva</string>
		<key>size</key>
		<integer>26</integer>
		<key>spacing</key>
		<real>1</real>
		<key>leading</key>
		<real>7</real>
		<key>bold</key>
		<false/>
		<key>italic</key>
		<false/>
		<key>underline</key>
		<false/>
		<key>vertical</key>
		<integer>486</integer>
		<key>halin</key>
		<integer>1</integer>
		<key>valign</key>
		<integer>2</integer>
		<key>standard</key>
		<string>PAL</string>
		<key>height</key>
		<integer>576</integer>
		<key>width</key>
		<integer>720</integer>
		<key>widthreal</key>
		<integer>768</integer>
		<key>antialiasing</key>
		<integer>4</integer>
		<key>left</key>
		<integer>40</integer>
		<key>right</key>
		<integer>680</integer>
		<key>wrapmethod</key>
		<integer>2</integer>
		<key>interlaced</key>
		<true/>
		<key>textcolor</key>
		<color>#FBFFF2</color>
		<key>textalpha</key>
		<real>1</real>
		<key>textsoft</key>
		<integer>0</integer>
		<key>bordercolor</key>
		<color>#F0F10</color>
		<key>borderalpha</key>
		<real>1</real>
		<key>bordersoft</key>
		<integer>0</integer>
		<key>borderwidth</key>
		<integer>6</integer>
		<key>rectcolor</key>
		<color>#0</color>
		<key>rectalpha</key>
		<real>0</real>
		<key>rectsoft</key>
		<integer>0</integer>
		<key>rectform</key>
		<integer>1</integer>
		<key>shadowcolor</key>
		<color>#7F7F7F</color>
		<key>shadowalpha</key>
		<real>0</real>
		<key>shadowsoft</key>
		<integer>0</integer>
		<key>shadowx</key>
		<integer>2</integer>
		<key>shadowy</key>
		<integer>2</integer>
		<key>framerate</key>
		<string>25</string>
	</dict>
	<key>folderpath</key>
	<string></string>
	<key>prefix</key>
	<string></string>
	<key>moviepath</key>
	<string></string>
	<key>movieoffset</key>
	<string>00:00:00:00</string>
	<key>moviesyncoption</key>
	<true/>
	<key>pagesetup</key>
	<null/>
	<key>titlelist</key>
</xmldict>");
            XmlNode node = doc.CreateElement("string");
            node.InnerText = sb.ToString().Trim() + Environment.NewLine + Environment.NewLine;
            doc.DocumentElement.AppendChild(node);

            var ms = new MemoryStream();
            var writer = new XmlTextWriter(ms, Encoding.UTF8) {Formatting = Formatting.Indented};
            doc.Save(writer);
            return Encoding.UTF8.GetString(ms.ToArray()).Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(sb.ToString());
                if (doc.DocumentElement == null || doc.DocumentElement.Name != "xmldict" || doc.DocumentElement.SelectSingleNode("string") == null)
                    return;
            }
            catch (Exception)
            {
                _errorCount = 1;
                return;
            }

            string text = null;
            string keyName = string.Empty;
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "key")
                {
                    keyName = node.InnerText;
                }
                else if (node.Name == "string" && keyName == "titlelist")
                {
                    text = node.InnerText;
                    break;
                }
            }
            if (text == null)
                return;

            subtitle.Paragraphs.Clear();
            Paragraph paragraph = null;
            sb = new StringBuilder();
            foreach (string line in text.Split(Environment.NewLine.ToCharArray()))
            {
                if (RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Substring(4, 11).Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            if (paragraph != null && sb.ToString().Trim().Length > 0)
                            {
                                paragraph.Text = sb.ToString().Trim().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine).Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                            }

                            var start = DecodeTimeCode(parts);
                            parts = line.Substring(16, 11).Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            var end = DecodeTimeCode(parts);
                            paragraph = new Paragraph { StartTime = start, EndTime = end };
                            subtitle.Paragraphs.Add(paragraph);
                            sb = new StringBuilder();
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                else if (paragraph != null)
                {
                    sb.AppendLine(line);
                }
                else
                {
                    _errorCount++;
                }
            }
            if (paragraph != null && sb.ToString().Trim().Length > 0)
            {
                paragraph.Text = sb.ToString().Trim().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine).Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            }
            subtitle.Renumber(1);
        }

        private string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFrames(time.Milliseconds));
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            int milliseconds = (int)((1000.0 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(frames));
            if (milliseconds > 999)
                milliseconds = 999;

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
        }

    }
}