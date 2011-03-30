using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{

// http://code.google.com/p/subtitleedit/issues/detail?id=18
//<DCSubtitle Version="1.0">
//<!-- *** www.digital-cinema-services.de *** -->
//<!-- *** ElMariKonverter *** -->
//<Id>urn:uuid:0b8a1af2-2073-493c-aea1-fa6b5601101a</Id>
//<MovieTitle>Im Himmel unter der Erde</MovieTitle>
//<ContentTitleText>de - Im Himmel unter der Erde</ContentTitleText>
//<AnnotationText>de - Im Himmel unter der Erde</AnnotationText>
//<IssueDate>2011-03-25T14:07:00</IssueDate>
//<StartTime>00:00:00:000</StartTime>
//<ReelNumber>1</ReelNumber>
//<Language>de</Language>
//<EditRate>24 1</EditRate>
//<TimeCodeRate>24</TimeCodeRate>
//<LoadFont Id="Font1" URI="Arial.ttf"/>
//<SubtitleList>
//<Font Id="Font1" Color="FFFFFFFF" Effect="outline" EffectColor="FF000000" Italic="no" Underlined="no" Script="normal" Size="52">
//<Subtitle SpotNumber="1" FadeUpTime="20" FadeDownTime="20" TimeIn="00:50:05:10" TimeOut="00:50:10:01">
//<Text Direction="horizontal" HAlign="center" HPosition="0" VAlign="bottom" VPosition="8"><Font Italic="yes">Meine Mutter und meine Schwester,</Font></Text>
//</Subtitle>

    class DCSubtitle : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".xml"; }
        }

        public override string Name
        {
            get { return "D-Cinema"; }
        }

        public override bool HasLineNumber
        {
            get { return true; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if (xmlAsString.Contains("<DCSubtitle") &&
                xmlAsString.Contains("<SubtitleList"))
            {
                XmlDocument xml = new XmlDocument();
                try
                {
                    xml.LoadXml(xmlAsString);

                    XmlNode subtitleList = xml.DocumentElement.SelectSingleNode("SubtitleList");
                    if (subtitleList != null)
                    {
                        var subtitles = subtitleList.SelectNodes("//Subtitle");
                        return subtitles != null && subtitles.Count > 0;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private static string ConvertToTimeString(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure = "<DCSubtitle Version=\"1.0\">" + 
@"   <Id>urn:uuid:0b8a1af2-2073-493c-aea1-fa6b5601101a</Id>
    <MovieTitle>[TITLE]</MovieTitle>
    <ContentTitleText>[TITLE]</ContentTitleText>
    <AnnotationText>[TITLE]</AnnotationText>
    <IssueDate>[DATE]</IssueDate>
    <StartTime>00:00:00:000</StartTime>
    <ReelNumber>1</ReelNumber>
    <Language>de</Language>
    <EditRate>24 1</EditRate>
    <TimeCodeRate>24</TimeCodeRate>
    <LoadFont Id=" + "\"Font1\" URI=\"Arial.ttf\"/>" + Environment.NewLine +
"    <SubtitleList>" + Environment.NewLine +
"         <Font Id=\"Font1\" Color=\"FFFFFFFF\" Effect=\"outline\" EffectColor=\"FF000000\" Italic=\"no\" Underlined=\"no\" Script=\"normal\" Size=\"52\">" + Environment.NewLine +
"         </Font>" + Environment.NewLine +
"    </SubtitleList>" + Environment.NewLine +
"</DCSubtitle>";
            XmlNode t = new XmlDocument().CreateElement("title");
            t.InnerText = title;
            xmlStructure = xmlStructure.Replace("[TITLE]", t.InnerXml);
            xmlStructure = xmlStructure.Replace("[DATE]", string.Format("{0:0000}:{1:00}:{2:00}T{3:HH:mm:ss}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now)); //2011-03-25T14:07:00

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode mainListFont = xml.DocumentElement.SelectSingleNode("//SubtitleList/Font");
            int no = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
//<Subtitle SpotNumber="1" FadeUpTime="20" FadeDownTime="20" TimeIn="00:50:05:10" TimeOut="00:50:10:01">
//<Text Direction="horizontal" HAlign="center" HPosition="0" VAlign="bottom" VPosition="8"><Font Italic="yes">Meine Mutter und meine Schwester,</Font></Text>
//</Subtitle>
                XmlNode subNode = xml.CreateElement("Subtitle");

                XmlAttribute id = xml.CreateAttribute("SpotNumber");
                id.InnerText = (no+1).ToString();
                subNode.Attributes.Append(id);

                XmlAttribute fadeUpTime = xml.CreateAttribute("FadeUpTime");
                fadeUpTime.InnerText = "20";
                subNode.Attributes.Append(fadeUpTime);

                XmlAttribute fadeDownTime = xml.CreateAttribute("FadeDownTime");
                fadeDownTime.InnerText = "20";
                subNode.Attributes.Append(fadeDownTime);

                XmlAttribute start = xml.CreateAttribute("TimeIn");
                start.InnerText = ConvertToTimeString(p.StartTime);
                subNode.Attributes.Append(start);

                XmlAttribute end = xml.CreateAttribute("TimeOut");
                end.InnerText = ConvertToTimeString(p.EndTime);
                subNode.Attributes.Append(end);

//                <Text VPosition="8" VAlign="bottom" HPosition="0" HAlign="center" Direction="horizontal">Er hat uns allen geholfen:</Text>                
                string[] lines = p.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                int vPos = 1 + lines.Length * 7;
                bool isItalic = false;
                foreach (string line in lines)
                { 
                    XmlNode textNode = xml.CreateElement("Text");

                    XmlAttribute vPosition = xml.CreateAttribute("VPosition");
                    vPosition.InnerText = vPos.ToString();
                    textNode.Attributes.Append(vPosition);

                    XmlAttribute vAlign = xml.CreateAttribute("VAlign");
                    vAlign.InnerText = "bottom";
                    textNode.Attributes.Append(vAlign);

                    XmlAttribute hAlign = xml.CreateAttribute("HAlign");
                    hAlign.InnerText = "center";
                    textNode.Attributes.Append(hAlign);

                    XmlAttribute direction = xml.CreateAttribute("Direction");
                    direction.InnerText = "horizontal";
                    textNode.Attributes.Append(direction);

                    int i = 0;
                    var txt = new StringBuilder();
                    var html = new StringBuilder();
                    XmlNode nodeTemp = xml.CreateElement("temp");
                    while (i<line.Length)
                    {
                        if (!isItalic && line.Substring(i).StartsWith("<i>"))
                        {
                            if (txt.Length > 0)
                            {
                                nodeTemp.InnerText = txt.ToString();
                                html.Append(nodeTemp.InnerXml);
                                txt = new StringBuilder();
                            }
                            isItalic = true;
                            i+=2;
                        }
                        else if (isItalic && line.Substring(i).StartsWith("</i>"))
                        {
                            if (txt.Length > 0)
                            {
                                XmlNode fontNode = xml.CreateElement("Font");

                                XmlAttribute italic = xml.CreateAttribute("Italic");
                                italic.InnerText = "yes";
                                fontNode.Attributes.Append(italic);

                                fontNode.InnerText = Utilities.RemoveHtmlTags(txt.ToString());
                                html.Append(fontNode.OuterXml);
                                txt = new StringBuilder();
                            }
                            isItalic = false;
                            i+=3;
                        }
                        else
                        {
                            txt.Append(line.Substring(i, 1));
                        }
                        i++;
                    }
                    if (isItalic)
                    {
                        if (txt.Length > 0)
                        {
                            XmlNode fontNode = xml.CreateElement("Font");

                            XmlAttribute italic = xml.CreateAttribute("Italic");
                            italic.InnerText = "yes";
                            fontNode.Attributes.Append(italic);

                            fontNode.InnerText = Utilities.RemoveHtmlTags(line);
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
                    
                    subNode.AppendChild(textNode);
                    vPos -= 7;
                }

                mainListFont.AppendChild(subNode);
                no++;
            }

            MemoryStream ms = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(ms, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            xml.Save(writer);
            return Encoding.UTF8.GetString(ms.ToArray()).Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            StringBuilder sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(sb.ToString());

            XmlNode subtitleList = xml.DocumentElement.SelectSingleNode("SubtitleList");
            foreach (XmlNode node in subtitleList.SelectNodes("//Subtitle"))
            {
                try
                {
                    StringBuilder pText = new StringBuilder();
                    string lastVPosition = string.Empty;
                    foreach (XmlNode innerNode in node.ChildNodes)
                    {
                        switch (innerNode.Name.ToString())
                        {
                            //case "br":
                            //    pText.AppendLine();
                            //    break;
                            case "Text":
                                if (innerNode.Attributes["VPosition"] != null)
                                {
                                    string vPosition = innerNode.Attributes["VPosition"].InnerText;
                                    if (vPosition != lastVPosition)
                                    {
                                        if (pText.Length > 0 && lastVPosition != string.Empty)
                                            pText.AppendLine();
                                        lastVPosition = vPosition;
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
                                            innerInnerNode.Attributes["Italic"].InnerText.ToLower() == "yes")
                                        {
                                            pText.Append("<i>" + innerInnerNode.InnerText + "</i>");
                                        }
                                        else
                                        {
                                            pText.Append(innerInnerNode.InnerText);
                                        }
                                    }                                    
                                }
                                break;
                            default:
                                pText.Append(innerNode.InnerText);
                                break;
                        }
                    }
                    string start = node.Attributes["TimeIn"].InnerText;
                    string end = node.Attributes["TimeOut"].InnerText;

                    subtitle.Paragraphs.Add(new Paragraph(GetTimeCode(start), GetTimeCode(end), pText.ToString()));
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

        private static TimeCode GetTimeCode(string s)
        {
            string[] parts = s.Split(new char[] { ':', '.', ',' });
            TimeSpan ts = new TimeSpan(0, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
            return new TimeCode(ts);
        }
    }
}


