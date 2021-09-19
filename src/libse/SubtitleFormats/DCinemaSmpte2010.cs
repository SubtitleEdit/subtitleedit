using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DCinemaSmpte2010 : SubtitleFormat
    {
        //<?xml version="1.0" encoding="UTF-8"?>
        //<dcst:SubtitleReel xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:dcst="http://www.smpte-ra.org/schemas/428-7/2010/DCST">
        //  <Id>urn:uuid:7be835a3-cfb4-43d0-bb4b-f0b4c95e962e</Id>
        //  <ContentTitleText>2001, A Space Odissey</ContentTitleText>
        //  <AnnotationText>This is a subtitle file</AnnotationText>
        //  <IssueDate>2012-06-26T12:33:59.000-00:00</IssueDate>
        //  <ReelNumber>1</ReelNumber>
        //  <Language>fr</Language>
        //  <EditRate>25 1</EditRate>
        //  <TimeCodeRate>25</TimeCodeRate>
        //  <StartTime>00:00:00:00</StartTime>
        //  <LoadFont ID="theFontId">urn:uuid:3dec6dc0-39d0-498d-97d0-928d2eb78391</LoadFont>
        //  <SubtitleList
        //      <Font ID="theFontId" Size="39" Weight="normal" Color="FFFFFFFF">
        //      <Subtitle FadeDownTime="00:00:00:00" FadeUpTime="00:00:00:00" TimeOut="00:00:00:01" TimeIn="00:00:00:00" SpotNumber="1">
        //          <Text Vposition="10.0" Valign="bottom">Hallo</Text>
        //      </Subtitle>
        //  </SubtitleList
        //</dcst:SubtitleReel>

        public string Errors { get; private set; }

        private double _frameRate = 24;

        public int Version { get; set; }

        public override string Extension => ".xml";

        public override string Name => "D-Cinema SMPTE 2010";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();

            if (xmlAsString.Contains("http://www.smpte-ra.org/schemas/428-7/2007/DCST") ||
                xmlAsString.Contains("http://www.smpte-ra.org/schemas/428-7/2014/DCST"))
            {
                return false;
            }

            if (xmlAsString.Contains("<dcst:SubtitleReel") || xmlAsString.Contains("<SubtitleReel"))
            {
                var xml = new XmlDocument { XmlResolver = null };
                try
                {
                    xmlAsString = xmlAsString.Replace("<dcst:", "<").Replace("</dcst:", "</");
                    xmlAsString = xmlAsString.Replace("xmlns=\"http://www.smpte-ra.org/schemas/428-7/2010/DCST\"", string.Empty);
                    xml.LoadXml(xmlAsString);
                    var subtitles = xml.DocumentElement.SelectNodes("//Subtitle");
                    return subtitles != null && subtitles.Count > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return false;
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            Errors = null;
            var ss = Configuration.Settings.SubtitleSettings;

            if (!string.IsNullOrEmpty(ss.CurrentDCinemaEditRate))
            {
                string[] temp = ss.CurrentDCinemaEditRate.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                double d1, d2;
                if (temp.Length == 2 && double.TryParse(temp[0], out d1) && double.TryParse(temp[1], out d2))
                {
                    _frameRate = d1 / d2;
                }
            }

            string xmlStructure =
                "<dcst:SubtitleReel xmlns:dcst=\"http://www.smpte-ra.org/schemas/428-7/2010/DCST\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" + Environment.NewLine +
                "  <dcst:Id>" + DCinemaSmpte2007.GenerateId()  + "</dcst:Id>" + Environment.NewLine +
                "  <dcst:ContentTitleText></dcst:ContentTitleText> " + Environment.NewLine +
                "  <dcst:AnnotationText>This is a subtitle file</dcst:AnnotationText>" + Environment.NewLine +
                "  <dcst:IssueDate>2012-06-26T12:33:59.000-00:00</dcst:IssueDate>" + Environment.NewLine +
                "  <dcst:ReelNumber>1</dcst:ReelNumber>" + Environment.NewLine +
                "  <dcst:Language>en</dcst:Language>" + Environment.NewLine +
                "  <dcst:EditRate>25 1</dcst:EditRate>" + Environment.NewLine +
                "  <dcst:TimeCodeRate>25</dcst:TimeCodeRate>" + Environment.NewLine +
                "  <dcst:StartTime>00:00:00:00</dcst:StartTime> " + Environment.NewLine +
                "  <dcst:LoadFont ID=\"theFontId\">urn:uuid:3dec6dc0-39d0-498d-97d0-928d2eb78391</dcst:LoadFont>" + Environment.NewLine +
                "  <dcst:SubtitleList>" + Environment.NewLine +
                "    <dcst:Font ID=\"theFontId\" Size=\"39\" Weight=\"normal\" Color=\"FFFFFFFF\" Effect=\"border\" EffectColor=\"FF000000\">" + Environment.NewLine +
                "    </dcst:Font>" + Environment.NewLine +
                "  </dcst:SubtitleList>" + Environment.NewLine +
                "</dcst:SubtitleReel>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            xml.PreserveWhitespace = true;
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("dcst", xml.DocumentElement.NamespaceURI);

            if (string.IsNullOrEmpty(ss.CurrentDCinemaMovieTitle))
            {
                ss.CurrentDCinemaMovieTitle = title;
            }

            if (ss.CurrentDCinemaFontSize == 0 || string.IsNullOrEmpty(ss.CurrentDCinemaFontEffect))
            {
                Configuration.Settings.SubtitleSettings.InitializeDCinameSettings(true);
            }

            xml.DocumentElement.SelectSingleNode("dcst:ContentTitleText", nsmgr).InnerText = ss.CurrentDCinemaMovieTitle;
            if (string.IsNullOrEmpty(ss.CurrentDCinemaSubtitleId) || !ss.CurrentDCinemaSubtitleId.StartsWith("urn:uuid:"))
            {
                ss.CurrentDCinemaSubtitleId = "urn:uuid:" + Guid.NewGuid();
            }

            if (!ss.DCinemaAutoGenerateSubtitleId)
            {
                xml.DocumentElement.SelectSingleNode("dcst:Id", nsmgr).InnerText = ss.CurrentDCinemaSubtitleId;
            }

            xml.DocumentElement.SelectSingleNode("dcst:ReelNumber", nsmgr).InnerText = ss.CurrentDCinemaReelNumber;
            xml.DocumentElement.SelectSingleNode("dcst:IssueDate", nsmgr).InnerText = ss.CurrentDCinemaIssueDate;
            if (string.IsNullOrEmpty(ss.CurrentDCinemaLanguage))
            {
                ss.CurrentDCinemaLanguage = "en";
            }

            xml.DocumentElement.SelectSingleNode("dcst:Language", nsmgr).InnerText = ss.CurrentDCinemaLanguage;
            if (ss.CurrentDCinemaEditRate == null && ss.CurrentDCinemaTimeCodeRate == null)
            {
                if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 24) < 0.01)
                {
                    ss.CurrentDCinemaEditRate = "24 1";
                    ss.CurrentDCinemaTimeCodeRate = "24";
                }
                else
                {
                    ss.CurrentDCinemaEditRate = "25 1";
                    ss.CurrentDCinemaTimeCodeRate = "25";
                }
            }
            xml.DocumentElement.SelectSingleNode("dcst:EditRate", nsmgr).InnerText = ss.CurrentDCinemaEditRate;
            xml.DocumentElement.SelectSingleNode("dcst:TimeCodeRate", nsmgr).InnerText = ss.CurrentDCinemaTimeCodeRate;
            if (string.IsNullOrEmpty(ss.CurrentDCinemaStartTime))
            {
                ss.CurrentDCinemaStartTime = "00:00:00:00";
            }

            xml.DocumentElement.SelectSingleNode("dcst:StartTime", nsmgr).InnerText = ss.CurrentDCinemaStartTime;
            xml.DocumentElement.SelectSingleNode("dcst:LoadFont", nsmgr).InnerText = ss.CurrentDCinemaFontUri;
            int fontSize = ss.CurrentDCinemaFontSize;
            string loadedFontId = "Font1";
            if (!string.IsNullOrEmpty(ss.CurrentDCinemaFontId))
            {
                loadedFontId = ss.CurrentDCinemaFontId;
            }

            xml.DocumentElement.SelectSingleNode("dcst:LoadFont", nsmgr).Attributes["ID"].Value = loadedFontId;
            xml.DocumentElement.SelectSingleNode("dcst:SubtitleList/dcst:Font", nsmgr).Attributes["Size"].Value = fontSize.ToString();
            xml.DocumentElement.SelectSingleNode("dcst:SubtitleList/dcst:Font", nsmgr).Attributes["Color"].Value = "FF" + Utilities.ColorToHex(ss.CurrentDCinemaFontColor).TrimStart('#').ToUpperInvariant();
            xml.DocumentElement.SelectSingleNode("dcst:SubtitleList/dcst:Font", nsmgr).Attributes["ID"].Value = loadedFontId;
            xml.DocumentElement.SelectSingleNode("dcst:SubtitleList/dcst:Font", nsmgr).Attributes["Effect"].Value = ss.CurrentDCinemaFontEffect;
            xml.DocumentElement.SelectSingleNode("dcst:SubtitleList/dcst:Font", nsmgr).Attributes["EffectColor"].Value = "FF" + Utilities.ColorToHex(ss.CurrentDCinemaFontEffectColor).TrimStart('#').ToUpperInvariant();

            XmlNode mainListFont = xml.DocumentElement.SelectSingleNode("dcst:SubtitleList/dcst:Font", nsmgr);
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Text != null)
                {
                    XmlNode subNode = xml.CreateElement("dcst:Subtitle", "dcst");

                    XmlAttribute id = xml.CreateAttribute("SpotNumber");
                    id.InnerText = (no + 1).ToString();
                    subNode.Attributes.Append(id);

                    XmlAttribute fadeUpTime = xml.CreateAttribute("FadeUpTime");
                    fadeUpTime.InnerText = new TimeCode(FramesToMilliseconds(Configuration.Settings.SubtitleSettings.DCinemaFadeUpTime)).ToHHMMSSFF();
                    subNode.Attributes.Append(fadeUpTime);

                    XmlAttribute fadeDownTime = xml.CreateAttribute("FadeDownTime");
                    fadeDownTime.InnerText = new TimeCode(FramesToMilliseconds(Configuration.Settings.SubtitleSettings.DCinemaFadeDownTime)).ToHHMMSSFF();
                    subNode.Attributes.Append(fadeDownTime);

                    XmlAttribute start = xml.CreateAttribute("TimeIn");
                    start.InnerText = ConvertToTimeString(p.StartTime);
                    subNode.Attributes.Append(start);

                    XmlAttribute end = xml.CreateAttribute("TimeOut");
                    end.InnerText = ConvertToTimeString(p.EndTime);
                    subNode.Attributes.Append(end);

                    bool alignLeft = p.Text.StartsWith("{\\a1}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a5}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a9}", StringComparison.Ordinal) || // sub station alpha
                                     p.Text.StartsWith("{\\an1}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an4}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an7}", StringComparison.Ordinal); // advanced sub station alpha

                    bool alignRight = p.Text.StartsWith("{\\a3}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a7}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a11}", StringComparison.Ordinal) || // sub station alpha
                                      p.Text.StartsWith("{\\an3}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an6}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an9}", StringComparison.Ordinal); // advanced sub station alpha

                    bool alignVTop = p.Text.StartsWith("{\\a5}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a6}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a7}", StringComparison.Ordinal) || // sub station alpha
                                     p.Text.StartsWith("{\\an7}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an8}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an9}", StringComparison.Ordinal); // advanced sub station alpha

                    bool alignVCenter = p.Text.StartsWith("{\\a9}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a10}", StringComparison.Ordinal) || p.Text.StartsWith("{\\a11}", StringComparison.Ordinal) || // sub station alpha
                                        p.Text.StartsWith("{\\an4}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an5}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an6}", StringComparison.Ordinal); // advanced sub station alpha

                    string text = Utilities.RemoveSsaTags(p.Text);

                    var lines = text.SplitToLines();
                    int vPos = 1 + lines.Count * 7;
                    int vPosFactor = (int)Math.Round(fontSize / 7.4);
                    if (alignVTop)
                    {
                        vPos = Configuration.Settings.SubtitleSettings.DCinemaBottomMargin; // Bottom margin is normally 8
                    }
                    else if (alignVCenter)
                    {
                        vPos = (int)Math.Round((lines.Count * vPosFactor * -1) / 2.0);
                    }
                    else
                    {
                        vPos = (lines.Count * vPosFactor) - vPosFactor + Configuration.Settings.SubtitleSettings.DCinemaBottomMargin; // Bottom margin is normally 8
                    }

                    bool isItalic = false;
                    int fontNo = 0;
                    Stack<string> fontColors = new Stack<string>();
                    foreach (string line in lines)
                    {
                        XmlNode textNode = xml.CreateElement("dcst:Text", "dcst");

                        XmlAttribute vPosition = xml.CreateAttribute("Vposition");
                        vPosition.InnerText = vPos.ToString();
                        textNode.Attributes.Append(vPosition);

                        XmlAttribute vAlign = xml.CreateAttribute("Valign");
                        if (alignVTop)
                        {
                            vAlign.InnerText = "top";
                        }
                        else if (alignVCenter)
                        {
                            vAlign.InnerText = "center";
                        }
                        else
                        {
                            vAlign.InnerText = "bottom";
                        }

                        textNode.Attributes.Append(vAlign); textNode.Attributes.Append(vAlign);

                        XmlAttribute hAlign = xml.CreateAttribute("Halign");
                        if (alignLeft)
                        {
                            hAlign.InnerText = "left";
                        }
                        else if (alignRight)
                        {
                            hAlign.InnerText = "right";
                        }
                        else
                        {
                            hAlign.InnerText = "center";
                        }

                        textNode.Attributes.Append(hAlign);

                        XmlAttribute direction = xml.CreateAttribute("Direction");
                        direction.InnerText = "ltr";
                        textNode.Attributes.Append(direction);

                        int i = 0;
                        var txt = new StringBuilder();
                        var html = new StringBuilder();
                        XmlNode nodeTemp = xml.CreateElement("temp");
                        while (i < line.Length)
                        {
                            if (!isItalic && line.Substring(i).StartsWith("<i>", StringComparison.Ordinal))
                            {
                                if (txt.Length > 0)
                                {
                                    nodeTemp.InnerText = txt.ToString();
                                    html.Append(nodeTemp.InnerXml);
                                    txt.Clear();
                                }
                                isItalic = true;
                                i += 2;
                            }
                            else if (isItalic && line.Substring(i).StartsWith("</i>", StringComparison.Ordinal))
                            {
                                if (txt.Length > 0)
                                {
                                    XmlNode fontNode = xml.CreateElement("dcst:Font", "dcst");

                                    XmlAttribute italic = xml.CreateAttribute("Italic");
                                    italic.InnerText = "yes";
                                    fontNode.Attributes.Append(italic);

                                    if (line.Length > i + 5 && line.Substring(i + 4).StartsWith("</font>", StringComparison.Ordinal))
                                    {
                                        XmlAttribute fontColor = xml.CreateAttribute("Color");
                                        fontColor.InnerText = fontColors.Pop();
                                        fontNode.Attributes.Append(fontColor);
                                        fontNo--;
                                        i += 7;
                                    }

                                    fontNode.InnerText = HtmlUtil.RemoveHtmlTags(txt.ToString());
                                    html.Append(fontNode.OuterXml);
                                    txt.Clear();
                                }
                                isItalic = false;
                                i += 3;
                            }
                            else if (line.Substring(i).StartsWith("<font color=", StringComparison.Ordinal) && line.Substring(i + 3).Contains('>'))
                            {
                                int endOfFont = line.IndexOf('>', i);
                                if (txt.Length > 0)
                                {
                                    nodeTemp.InnerText = txt.ToString();
                                    html.Append(nodeTemp.InnerXml);
                                    txt.Clear();
                                }
                                var c = DCinemaInterop.GetDCinemaColorString(line.Substring(i + 12, endOfFont - (i + 12)));
                                fontColors.Push(c);
                                fontNo++;
                                i = endOfFont;
                            }
                            else if (fontNo > 0 && line.Substring(i).StartsWith("</font>", StringComparison.Ordinal))
                            {
                                if (txt.Length > 0)
                                {
                                    XmlNode fontNode = xml.CreateElement("dcst:Font", "dcst");

                                    XmlAttribute fontColor = xml.CreateAttribute("Color");
                                    fontColor.InnerText = fontColors.Pop();
                                    fontNode.Attributes.Append(fontColor);

                                    if (line.Length > i + 9 && line.Substring(i + 7).StartsWith("</i>", StringComparison.Ordinal))
                                    {
                                        XmlAttribute italic = xml.CreateAttribute("Italic");
                                        italic.InnerText = "yes";
                                        fontNode.Attributes.Append(italic);
                                        isItalic = false;
                                        i += 4;
                                    }

                                    fontNode.InnerText = HtmlUtil.RemoveHtmlTags(txt.ToString());
                                    html.Append(fontNode.OuterXml);
                                    txt.Clear();
                                }
                                fontNo--;
                                i += 6;
                            }
                            else
                            {
                                txt.Append(line[i]);
                            }
                            i++;
                        }

                        if (fontNo > 0)
                        {
                            if (txt.Length > 0)
                            {
                                XmlNode fontNode = xml.CreateElement("dcst:Font", "dcst");

                                XmlAttribute fontColor = xml.CreateAttribute("Color");
                                fontColor.InnerText = fontColors.Peek();
                                fontNode.Attributes.Append(fontColor);

                                if (isItalic)
                                {
                                    XmlAttribute italic = xml.CreateAttribute("Italic");
                                    italic.InnerText = "yes";
                                    fontNode.Attributes.Append(italic);
                                }

                                fontNode.InnerText = HtmlUtil.RemoveHtmlTags(txt.ToString());
                                html.Append(fontNode.OuterXml);
                            }
                            else if (html.Length > 0 && html.ToString().StartsWith("<dcst:Font ", StringComparison.Ordinal))
                            {
                                XmlDocument temp = new XmlDocument();
                                temp.LoadXml("<root>" + html.ToString().Replace("dcst:Font", "Font") + "</root>");
                                XmlNode fontNode = xml.CreateElement("dcst:Font");
                                fontNode.InnerXml = temp.DocumentElement.SelectSingleNode("Font").InnerXml;
                                foreach (XmlAttribute a in temp.DocumentElement.SelectSingleNode("Font").Attributes)
                                {
                                    XmlAttribute newA = xml.CreateAttribute(a.Name);
                                    newA.InnerText = a.InnerText;
                                    fontNode.Attributes.Append(newA);
                                }

                                XmlAttribute fontColor = xml.CreateAttribute("Color");
                                fontColor.InnerText = fontColors.Peek();
                                fontNode.Attributes.Append(fontColor);

                                html.Clear();
                                html.Append(fontNode.OuterXml);
                            }
                        }
                        else if (isItalic)
                        {
                            if (txt.Length > 0)
                            {
                                XmlNode fontNode = xml.CreateElement("dcst:Font", "dcst");

                                XmlAttribute italic = xml.CreateAttribute("Italic");
                                italic.InnerText = "yes";
                                fontNode.Attributes.Append(italic);

                                fontNode.InnerText = HtmlUtil.RemoveHtmlTags(txt.ToString());
                                html.Append(fontNode.OuterXml);
                            }
                        }
                        else
                        {
                            if (txt.Length > 0)
                            {
                                nodeTemp.InnerText = txt.ToString();
                                html.Append(nodeTemp.InnerXml);
                            }
                        }
                        textNode.InnerXml = html.ToString();
                        if (html.Length == 0)
                        {
                            textNode.InnerText = " "; // We need to have at least a single space character on exporting empty subtitles, because otherwise I will get errors on import.ou need to have at least a single space character on exporting empty subtitles, otherwise we will get errors on import.
                        }

                        subNode.AppendChild(textNode);
                        if (alignVTop)
                        {
                            vPos += vPosFactor;
                        }
                        else
                        {
                            vPos -= vPosFactor;
                        }
                    }
                    if (subNode.InnerXml.Length == 0)
                    { // Empty text is just one space
                        XmlNode textNode = xml.CreateElement("dcst:Text", "dcst");
                        textNode.InnerXml = " ";
                        subNode.AppendChild(textNode);

                        XmlAttribute vPosition = xml.CreateAttribute("Vposition");
                        vPosition.InnerText = vPos.ToString();
                        textNode.Attributes.Append(vPosition);

                        XmlAttribute vAlign = xml.CreateAttribute("Valign");
                        vAlign.InnerText = "bottom";
                        textNode.Attributes.Append(vAlign);
                    }
                    mainListFont.AppendChild(subNode);
                    no++;
                }
            }

            var result = ToUtf8XmlString(xml)
                .Replace("encoding=\"utf-8\"", "encoding=\"UTF-8\"")
                .Replace(" xmlns:dcst=\"dcst\"", string.Empty)
                .Replace("<dcst:", "<")
                .Replace("</dcst:", "</")
                .Replace("xmlns:dcst=\"http://www.smpte-ra.org/schemas/", "xmlns=\"http://www.smpte-ra.org/schemas/");

            const string res = "Nikse.SubtitleEdit.Resources.SMPTE-428-7-2010-DCST.xsd.gz";
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(res);
            if (stream != null)
            {
                try
                {
                    var xmld = new XmlDocument();
                    var rdr = new StreamReader(stream);
                    var zip = new GZipStream(rdr.BaseStream, CompressionMode.Decompress);
                    xmld.LoadXml(result);
                    using (var xr = XmlReader.Create(zip))
                    {
                        xmld.Schemas.Add(null, xr);
                        xmld.Validate(ValidationCallBack);
                    }
                }
                catch (Exception exception)
                {
                    Errors = "Error validating xml via SMPTE-428-7-2010-DCST.xsd: " + exception.Message;
                }
            }
            return FixDcsTextSameLine(result);
        }

        /// <summary>
        /// All space characters present inside the content of a Text element shall be rendered
        /// </summary>
        internal static string FixDcsTextSameLine(string xml)
        {
            int index = xml.IndexOf("<dcst:Text", StringComparison.Ordinal);
            int endIndex = 1;
            while (index > 0 && endIndex > 0)
            {
                endIndex = xml.IndexOf("</dcst:Text>", index, StringComparison.Ordinal);
                if (endIndex > 0)
                {
                    var part = xml.Substring(index, endIndex - index);
                    if (part.Contains(Environment.NewLine))
                    {
                        part = part.Replace(Environment.NewLine, " ");
                        while (part.Contains("  "))
                        {
                            part = part.Replace("  ", " ");
                        }
                        part = part.Replace("> <", "><");
                    }
                    xml = xml.Remove(index, endIndex - index).Insert(index, part);
                    index = xml.IndexOf("<dcst:Text", endIndex, StringComparison.Ordinal);
                }
            }
            return xml;
        }

        private void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            throw new Exception(e.Message);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().Replace("<dcst:", "<").Replace("</dcst:", "</").Replace("xmlns=\"http://www.smpte-ra.org/schemas/428-7/2010/DCST\"", string.Empty)); // tags might be prefixed with namespace (or not)... so we just remove them

            var ss = Configuration.Settings.SubtitleSettings;
            try
            {
                ss.InitializeDCinameSettings(true);
                XmlNode node = xml.DocumentElement.SelectSingleNode("Id");
                if (node != null)
                {
                    ss.CurrentDCinemaSubtitleId = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("ReelNumber");
                if (node != null)
                {
                    ss.CurrentDCinemaReelNumber = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("EditRate");
                if (node != null)
                {
                    ss.CurrentDCinemaEditRate = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("TimeCodeRate");
                if (node != null)
                {
                    ss.CurrentDCinemaTimeCodeRate = node.InnerText;
                    if (ss.CurrentDCinemaEditRate == "24")
                    {
                        Configuration.Settings.General.CurrentFrameRate = 24;
                    }
                    else if (ss.CurrentDCinemaEditRate == "25")
                    {
                        Configuration.Settings.General.CurrentFrameRate = 25;
                    }

                    if (BatchSourceFrameRate.HasValue)
                    {
                        Configuration.Settings.General.CurrentFrameRate = BatchSourceFrameRate.Value;
                    }
                }

                node = xml.DocumentElement.SelectSingleNode("StartTime");
                if (node != null)
                {
                    ss.CurrentDCinemaStartTime = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("Language");
                if (node != null)
                {
                    ss.CurrentDCinemaLanguage = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("ContentTitleText");
                if (node != null)
                {
                    ss.CurrentDCinemaMovieTitle = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("IssueDate");
                if (node != null)
                {
                    ss.CurrentDCinemaIssueDate = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("LoadFont");
                if (node != null)
                {
                    ss.CurrentDCinemaFontUri = node.InnerText;
                }

                node = xml.DocumentElement.SelectSingleNode("SubtitleList/Font");
                if (node != null)
                {
                    if (node.Attributes["ID"] != null)
                    {
                        ss.CurrentDCinemaFontId = node.Attributes["ID"].InnerText;
                    }

                    if (node.Attributes["Size"] != null)
                    {
                        ss.CurrentDCinemaFontSize = Convert.ToInt32(node.Attributes["Size"].InnerText);
                    }

                    if (node.Attributes["Color"] != null)
                    {
                        ss.CurrentDCinemaFontColor = System.Drawing.ColorTranslator.FromHtml("#" + node.Attributes["Color"].InnerText);
                    }

                    if (node.Attributes["Effect"] != null)
                    {
                        ss.CurrentDCinemaFontEffect = node.Attributes["Effect"].InnerText;
                    }

                    if (node.Attributes["EffectColor"] != null)
                    {
                        ss.CurrentDCinemaFontEffectColor = System.Drawing.ColorTranslator.FromHtml("#" + node.Attributes["EffectColor"].InnerText);
                    }
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//Subtitle"))
            {
                try
                {
                    var textLines = new List<DCinemaInterop.SubtitleLine>();
                    var pText = new StringBuilder();
                    var vAlignment = string.Empty;
                    string lastVPosition = string.Empty;
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        if (innerNode.Name == "Text")
                        {
                            if (innerNode.Attributes["Vposition"] != null)
                            {
                                var vAlignmentNode = innerNode.Attributes["Valign"];
                                if (vAlignmentNode != null)
                                {
                                    vAlignment = vAlignmentNode.InnerText;
                                }

                                string vPosition = innerNode.Attributes["Vposition"].InnerText;
                                if (vPosition != lastVPosition)
                                {
                                    if (pText.Length > 0 && lastVPosition.Length > 0)
                                    {
                                        textLines.Add(new DCinemaInterop.SubtitleLine(pText.ToString(), lastVPosition, vAlignment));
                                        pText.Clear();
                                    }

                                    lastVPosition = vPosition;
                                }
                            }

                            bool alignLeft = false;
                            bool alignRight = false;
                            bool alignVTop = false;
                            bool alignVCenter = false;
                            if (innerNode.Attributes["Halign"] != null)
                            {
                                string hAlign = innerNode.Attributes["Halign"].InnerText;
                                if (hAlign == "left")
                                {
                                    alignLeft = true;
                                }
                                else if (hAlign == "right")
                                {
                                    alignRight = true;
                                }
                            }

                            if (innerNode.Attributes["Valign"] != null)
                            {
                                string hAlign = innerNode.Attributes["Valign"].InnerText;
                                if (hAlign == "top")
                                {
                                    alignVTop = true;
                                }
                                else if (hAlign == "center")
                                {
                                    alignVCenter = true;
                                }
                            }

                            if (alignLeft || alignRight || alignVCenter || alignVTop)
                            {
                                if (!pText.ToString().StartsWith("{\\an", StringComparison.Ordinal))
                                {
                                    string pre = string.Empty;
                                    if (alignVTop)
                                    {
                                        if (alignLeft)
                                        {
                                            pre = "{\\an7}";
                                        }
                                        else if (alignRight)
                                        {
                                            pre = "{\\an9}";
                                        }
                                        else
                                        {
                                            pre = "{\\an8}";
                                        }
                                    }
                                    else if (alignVCenter)
                                    {
                                        if (alignLeft)
                                        {
                                            pre = "{\\an4}";
                                        }
                                        else if (alignRight)
                                        {
                                            pre = "{\\an6}";
                                        }
                                        else
                                        {
                                            pre = "{\\an5}";
                                        }
                                    }
                                    else
                                    {
                                        if (alignLeft)
                                        {
                                            pre = "{\\an1}";
                                        }
                                        else if (alignRight)
                                        {
                                            pre = "{\\an3}";
                                        }
                                    }

                                    string temp = pre + pText;
                                    pText.Clear();
                                    pText.Append(temp);
                                }
                            }

                            if (innerNode.ChildNodes.Count == 0)
                            {
                                pText.Append(innerNode.InnerText);
                            }
                            else
                            {
                                foreach (XmlNode innerInnerNode in innerNode)
                                {
                                    if (innerInnerNode.Name == "Font" && innerInnerNode.Attributes["Italic"] != null &&
                                        innerInnerNode.Attributes["Italic"].InnerText.Equals("yes", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (innerInnerNode.Attributes["Color"] != null)
                                        {
                                            pText.Append("<i><font color=\"" + DCinemaInterop.GetColorStringFromDCinema(innerInnerNode.Attributes["Color"].Value) + "\">" + innerInnerNode.InnerText + "</font><i>");
                                        }
                                        else
                                        {
                                            pText.Append("<i>" + innerInnerNode.InnerText + "</i>");
                                        }
                                    }
                                    else if (innerInnerNode.Name == "Font" && innerInnerNode.Attributes["Color"] != null)
                                    {
                                        if (innerInnerNode.Attributes["Italic"] != null && innerInnerNode.Attributes["Italic"].InnerText.Equals("yes", StringComparison.OrdinalIgnoreCase))
                                        {
                                            pText.Append("<i><font color=\"" + DCinemaInterop.GetColorStringFromDCinema(innerInnerNode.Attributes["Color"].Value) + "\">" + innerInnerNode.InnerText + "</font><i>");
                                        }
                                        else
                                        {
                                            pText.Append("<font color=\"" + DCinemaInterop.GetColorStringFromDCinema(innerInnerNode.Attributes["Color"].Value) + "\">" + innerInnerNode.InnerText + "</font>");
                                        }
                                    }
                                    else
                                    {
                                        pText.Append(innerInnerNode.InnerText);
                                    }
                                }
                            }
                        }
                        else
                        {
                            pText.Append(innerNode.InnerText);
                        }
                    }

                    if (pText.Length > 0)
                    {
                        textLines.Add(new DCinemaInterop.SubtitleLine(pText.ToString(), lastVPosition, vAlignment));
                    }

                    string text;
                    if (textLines.All(p => p.VerticalAlignment.ToLowerInvariant() == "bottom"))
                    {
                        text = string.Join(Environment.NewLine, textLines.OrderByDescending(p => p.GetVerticalPositionAsNumber()).Select(p => p.Text));
                    }
                    else
                    {
                        text = string.Join(Environment.NewLine, textLines.OrderBy(p => p.GetVerticalPositionAsNumber()).Select(p => p.Text));
                    }

                    string start = node.Attributes["TimeIn"].InnerText;
                    string end = node.Attributes["TimeOut"].InnerText;

                    if (node.ParentNode.Name == "Font" && node.ParentNode.Attributes["Italic"] != null && node.ParentNode.Attributes["Italic"].InnerText.Equals("yes", StringComparison.OrdinalIgnoreCase) &&
                        !pText.ToString().Contains("<i>"))
                    {
                        if (text.StartsWith("{\\an", StringComparison.Ordinal) && text.Length > 6)
                        {
                            text = text.Insert(6, "<i>") + "</i>";
                        }
                        else
                        {
                            text = "<i>" + text + "</i>";
                        }
                    }

                    subtitle.Paragraphs.Add(new Paragraph(GetTimeCode(start), GetTimeCode(end), text));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }

            if (subtitle.Paragraphs.Count > 0)
            {
                subtitle.Header = xml.OuterXml; // save id/language/font for later use
            }

            subtitle.Renumber();
        }

        private TimeCode GetTimeCode(string s)
        {
            var parts = s.Split(new[] { ':', '.', ',' });

            int milliseconds = (int)Math.Round(int.Parse(parts[3]) * (TimeCode.BaseUnit / _frameRate));
            if (milliseconds > 999)
            {
                milliseconds = 999;
            }

            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), milliseconds);
        }

        public static int MsToFramesMaxFrameRate(double milliseconds, double frameRate)
        {
            int frames = (int)Math.Round(milliseconds / (TimeCode.BaseUnit / frameRate));
            if (frames >= Configuration.Settings.General.CurrentFrameRate)
            {
                frames = (int)(frameRate - 0.01);
            }

            return frames;
        }

        private string ConvertToTimeString(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MsToFramesMaxFrameRate(time.Milliseconds, _frameRate):00}";
        }

    }
}
