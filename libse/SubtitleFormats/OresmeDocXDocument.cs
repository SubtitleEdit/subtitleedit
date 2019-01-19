using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class OresmeDocXDocument : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "Oresme Docx document";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlAsString = sb.ToString().Trim();
            if (xmlAsString.Contains("<w:tc>"))
            {
                return base.IsMine(lines, fileName);
            }
            return false;
        }

        private const string Layout = @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<w:document xmlns:wpc='http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas' xmlns:mc='http://schemas.openxmlformats.org/markup-compatibility/2006' xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:r='http://schemas.openxmlformats.org/officeDocument/2006/relationships' xmlns:m='http://schemas.openxmlformats.org/officeDocument/2006/math' xmlns:v='urn:schemas-microsoft-com:vml' xmlns:wp14='http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing' xmlns:wp='http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing' xmlns:w10='urn:schemas-microsoft-com:office:word' xmlns:w='http://schemas.openxmlformats.org/wordprocessingml/2006/main' xmlns:w14='http://schemas.microsoft.com/office/word/2010/wordml' xmlns:wpg='http://schemas.microsoft.com/office/word/2010/wordprocessingGroup' xmlns:wpi='http://schemas.microsoft.com/office/word/2010/wordprocessingInk' xmlns:wne='http://schemas.microsoft.com/office/word/2006/wordml' xmlns:wps='http://schemas.microsoft.com/office/word/2010/wordprocessingShape' mc:Ignorable='w14 wp14'>
  <w:body>
    <w:tbl>
      <w:tblPr>
        <w:tblW w:w='0' w:type='auto'/>
        <w:tblBorders>
          <w:top w:val='single' w:sz='4' w:space='0' w:color='FFCC00'/>
          <w:left w:val='single' w:sz='4' w:space='0' w:color='FFCC00'/>
          <w:bottom w:val='single' w:sz='4' w:space='0' w:color='FFCC00'/>
          <w:right w:val='single' w:sz='4' w:space='0' w:color='FFCC00'/>
          <w:insideH w:val='single' w:sz='4' w:space='0' w:color='FFCC00'/>
          <w:insideV w:val='single' w:sz='4' w:space='0' w:color='FFCC00'/>
        </w:tblBorders>
        <w:tblLayout w:type='fixed'/>
        <w:tblCellMar>
          <w:left w:w='70' w:type='dxa'/>
          <w:right w:w='70' w:type='dxa'/>
        </w:tblCellMar>
        <w:tblLook w:val='0000' w:firstRow='0' w:lastRow='0' w:firstColumn='0' w:lastColumn='0' w:noHBand='0' w:noVBand='0'/>
      </w:tblPr>
      <w:tblGrid>
        <w:gridCol w:w='1240'/>
        <w:gridCol w:w='5560'/>
      </w:tblGrid>
    </w:tbl>
    <w:p w:rsidR='00D56C9E' w:rsidRDefault='00D56C9E'/>
    <w:sectPr w:rsidR='00D56C9E'>
      <w:pgSz w:w='12240' w:h='15840'/>
      <w:pgMar w:top='1440' w:right='1440' w:bottom='1440' w:left='1440' w:header='720' w:footer='720' w:gutter='0'/>
      <w:cols w:space='720'/>
    </w:sectPr>
  </w:body>
</w:document>";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure = Layout.Replace("'", "\"");

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            XmlNode div = xml.DocumentElement.SelectSingleNode("w:body/w:tbl", nsmgr);
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                div.AppendChild(CreateXmlParagraph(xml, p));

                if (i < subtitle.Paragraphs.Count - 1 && Math.Abs(p.EndTime.TotalMilliseconds - subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds) > 100)
                {
                    var endP = new Paragraph(string.Empty, p.EndTime.TotalMilliseconds, 0);
                    div.AppendChild(CreateXmlParagraph(xml, endP));
                }
            }

            string s = ToUtf8XmlString(xml);
            return s;
        }

        private static XmlNode CreateXmlParagraph(XmlDocument xml, Paragraph p)
        {
            XmlNode paragraph = xml.CreateElement("w:tr", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            var tc1 = xml.CreateElement("w:tc", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            paragraph.AppendChild(tc1);

            //<w:tcPr>
            //  <w:tcW w:w='1240' w:type='dxa'/>
            //</w:tcPr>
            var n1 = xml.CreateElement("w:tcPr", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            var n1sub = xml.CreateElement("w:tcW", "http://schemas.openxmlformats.org/wordprocessingml/2006/main"); // <w:tcW w:w='1240' w:type='dxa'/>
            n1.AppendChild(n1sub);
            var n1suba1 = xml.CreateAttribute("w:w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n1suba1.InnerText = "1240";
            n1sub.Attributes.Append(n1suba1);
            var n1suba2 = xml.CreateAttribute("w:type", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n1suba2.InnerText = "dxa";
            n1sub.Attributes.Append(n1suba2);
            tc1.AppendChild(n1);

            //<w:p w:rsidR='00D56C9E' w:rsidRDefault='00D56C9E'>
            //  <w:pPr>
            //    <w:pStyle w:val='TimeCode'/>
            //  </w:pPr>
            //  <w:r>
            //    <w:t>[TIMECODE]</w:t>
            //  </w:r>
            //</w:p>
            var n2 = xml.CreateElement("w:p", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            var n1a1 = xml.CreateAttribute("w:rsidR", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n1a1.InnerText = "00D56C9E";
            n2.Attributes.Append(n1a1);
            var n1a2 = xml.CreateAttribute("w:rsidRDefault", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n1a2.InnerText = "00D56C9E";
            n2.Attributes.Append(n1a2);

            var n2sub1 = xml.CreateElement("w:pPr", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            var n2sub1sub = xml.CreateElement("w:pStyle", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n2sub1.AppendChild(n2sub1sub);
            var n2sub1Suba1 = xml.CreateAttribute("w:val", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n2sub1Suba1.InnerText = "TimeCode";
            n2sub1sub.Attributes.Append(n2sub1Suba1);
            n2.AppendChild(n2sub1);

            var n2sub2 = xml.CreateElement("w:r", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            var n2sub2sub = xml.CreateElement("w:t", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n2sub2sub.InnerText = ToTimeCode(p.StartTime);
            n2sub2.AppendChild(n2sub2sub);
            n2.AppendChild(n2sub2);
            tc1.AppendChild(n2);

            //<w:tc>
            //  <w:tcPr>
            //    <w:tcW w:w='5560' w:type='dxa'/>
            //    <w:vAlign w:val='bottom'/>
            //  </w:tcPr>
            //  <w:p w:rsidR='00D56C9E' w:rsidRDefault='00D56C9E'>
            //    <w:pPr>
            //      <w:pStyle w:val='PopOn'/>
            //    </w:pPr>
            //    <w:proofErr w:type='spellStart'/>
            //  </w:p>
            //</w:tc>
            var tc2 = xml.CreateElement("w:tc", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            paragraph.AppendChild(tc2);

            var n3sub1 = xml.CreateElement("w:tcPr", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            tc2.AppendChild(n3sub1);

            var n3sub1sub1 = xml.CreateElement("w:tcW", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            var n3suba1 = xml.CreateAttribute("w:w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n3suba1.InnerText = "5560";
            n3sub1sub1.Attributes.Append(n3suba1);
            var n3suba2 = xml.CreateAttribute("w:type", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n3suba2.InnerText = "dxa";
            n3sub1sub1.Attributes.Append(n3suba2);
            n3sub1.AppendChild(n3sub1sub1);

            var n3sub1sub2 = xml.CreateElement("w:vAlign", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            var n3sub1sub2a1 = xml.CreateAttribute("w:val", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n3sub1sub2a1.InnerText = "bottom";
            n3sub1sub2.Attributes.Append(n3sub1sub2a1);
            n3sub1.AppendChild(n3sub1sub2);

            var n3sub1sub3 = xml.CreateElement("w:p", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            var n3sub1sub3a1 = xml.CreateAttribute("w:rsidR", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n3sub1sub3a1.InnerText = "00D56C9E";
            n3sub1sub3.Attributes.Append(n3sub1sub3a1);
            var n3sub1sub3a2 = xml.CreateAttribute("w:rsidRDefault", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n3sub1sub3a2.InnerText = "00D56C9E";
            n3sub1sub3.Attributes.Append(n3sub1sub3a2);
            var n3sub1sub3sub1 = xml.CreateElement("w:pPr", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n3sub1sub3.AppendChild(n3sub1sub3sub1);
            var n3sub1sub3sub1sub = xml.CreateElement("w:pStyle", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            var n3sub1sub3sub1suba1 = xml.CreateAttribute("w:val", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            n3sub1sub3sub1suba1.InnerText = "PopOn";
            n3sub1sub3sub1sub.Attributes.Append(n3sub1sub3sub1suba1);
            n3sub1sub3sub1.AppendChild(n3sub1sub3sub1sub);

            var lines = HtmlUtil.RemoveHtmlTags(p.Text, true).Replace(Environment.NewLine, "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var n3sub1sub3sub2 = xml.CreateElement("w:r", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
                n3sub1sub3.AppendChild(n3sub1sub3sub2);
                if (i > 0)
                {
                    var lineBreak = xml.CreateElement("w:br", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
                    n3sub1sub3sub2.AppendChild(lineBreak);
                }
                var text = xml.CreateElement("w:t", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
                text.InnerText = lines[i];
                n3sub1sub3sub2.AppendChild(text);
            }
            tc2.AppendChild(n3sub1sub3);

            return paragraph;
        }

        private static string ToTimeCode(TimeCode timeCode)
        {
            return timeCode.ToHHMMSSFF(); //10:00:07:27
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(sb.ToString().Trim());
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("//w:tr", nsmgr))
            {
                try
                {
                    Paragraph p = new Paragraph();
                    XmlNode t = node.SelectSingleNode("w:tc/w:p/w:r/w:t", nsmgr);
                    if (t != null)
                    {
                        p.StartTime = DecodeTimeCodeFrames(t.InnerText.Trim(), SplitCharColon);
                        sb.Clear();
                        foreach (XmlNode wrNode in node.SelectNodes("w:tc/w:p/w:r", nsmgr))
                        {
                            foreach (XmlNode child in wrNode.ChildNodes)
                            {
                                if (child.Name == "w:t")
                                {
                                    bool isTimeCode = child.InnerText.Length == 11 && child.InnerText.RemoveChar(':').Length == 8;
                                    if (!isTimeCode)
                                    {
                                        sb.Append(child.InnerText);
                                    }
                                }
                                else if (child.Name == "w:br")
                                {
                                    sb.AppendLine();
                                }
                            }
                        }
                        p.Text = sb.ToString();
                        subtitle.Paragraphs.Add(p);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                subtitle.Paragraphs[i].EndTime.TotalMilliseconds = subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds;
            }
            subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds = 2500;
            subtitle.RemoveEmptyLines();
            for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                if (Math.Abs(subtitle.Paragraphs[i].EndTime.TotalMilliseconds - subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds) < 0.01)
                {
                    subtitle.Paragraphs[i].EndTime.TotalMilliseconds = subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds - 1;
                }
            }
            subtitle.Renumber();
        }

    }
}
