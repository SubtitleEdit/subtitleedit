using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class MsOfficeWorkbook : SubtitleFormat
    {
        public override string Extension => ".xml";

        public static readonly string NameOfFormat = "MS Office Workbook";

        public override string Name => NameOfFormat;

        public static string NamespaceSpreadsheet => "urn:schemas-microsoft-com:office:spreadsheet";
        public static string NamespaceHtml => "http://www.w3.org/TR/REC-html40";
        public static string NamespaceExcel => "urn:schemas-microsoft-com:office:excel";
        public static string NamespaceOffice => "urn:schemas-microsoft-com:office:office";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xmlAsString = sb.ToString().Trim();
            return xmlAsString.Contains("urn:schemas-microsoft-com:office:excel") && base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var xml = new XmlDocument();
            var xmlNamespaceManager = MakeNamespaceManager(xml);
            var xmlStructure = @"<?xml version='1.0' encoding='utf -8' ?>
<Workbook xmlns:ss='urn:schemas-microsoft-com:office:spreadsheet' xmlns:html='http://www.w3.org/TR/REC-html40' xmlns='urn:schemas-microsoft-com:office:spreadsheet' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns:o='urn:schemas-microsoft-com:office:office'>
    <DocumentProperties xmlns='urn:schemas-microsoft-com:office:office'><Author>Author</Author>
        <LastAuthor>No Author</LastAuthor>
        <Created>2020-08-05T17:33:25Z</Created>
    </DocumentProperties>
    <Styles>
        <Style ss:ID='s0'><Alignment ss:WrapText='1'></Alignment>
        </Style>
        <Style ss:ID='s00'>
            <Font ss:FontName='Calibri' ss:Size='12' ss:Bold='1'></Font>
            <Interior ss:Color='#c0c0c0' ss:Pattern='Solid'></Interior>
            <Alignment ss:Vertical='Center'></Alignment>
            <Borders>
                <Border ss:LineStyle='Continuous' ss:Color='#000000' ss:Weight='0.3' ss:Position='Bottom'></Border>
                <Border ss:LineStyle='Continuous' ss:Color='#000000' ss:Weight='0.3' ss:Position='Right'></Border>
            </Borders>
        </Style>
        <Style ss:ID='s2'><Alignment ss:WrapText='1' ss:Horizontal='Center'></Alignment>
            <Font ss:FontName='Calibri' ss:Size='12'></Font>
        </Style>
    </Styles>
    <Worksheet ss:Name='Sheet1'>
        <Table>
            <Column ss:Width='80'></Column>
            <Column ss:Width='80'></Column>
            <Column ss:Width='300'></Column>
            <Column ss:Width='200'></Column>
            <Column ss:Width='200'></Column>
            <Column ss:Width='50'></Column>
            <Column ss:Width='50'></Column>
            <Column ss:Width='50'></Column>
            <Column ss:Width='50'></Column>
            <Column ss:Width='120'></Column>
            <Row ss:Height='30'>
                <Cell ss:StyleID='s00'><Data ss:Type='String'>Start</Data>
                </Cell>        
                <Cell ss:StyleID='s00'><Data ss:Type='String'>End</Data>
                </Cell>        
                <Cell ss:StyleID='s00'><Data ss:Type='String'>Text</Data>
                </Cell>        
                <Cell ss:StyleID='s00'><Data ss:Type='String'>Description</Data>
                </Cell>        
                <Cell ss:StyleID='s00'><Data ss:Type='String'>Comment</Data>
                </Cell>        
                <Cell ss:StyleID='s00'><Data ss:Type='String'>Good</Data>
                </Cell>        
                <Cell ss:StyleID='s00'><Data ss:Type='String'>Marked</Data>
                </Cell>        
                <Cell ss:StyleID='s00'><Data ss:Type='String'>Position</Data>
                </Cell>        
                <Cell ss:StyleID='s00'><Data ss:Type='String'>Line Shift</Data>
                </Cell>        
                <Cell ss:StyleID='s00'><Data ss:Type='String'>Actors</Data>
                </Cell>
            </Row>
         </Table>
    </Worksheet>
</Workbook>".Replace('\'', '"');

            xml.LoadXml(xmlStructure);
            var table = xml.DocumentElement?.SelectSingleNode("//ss:Table", xmlNamespaceManager);
            foreach (var p in subtitle.Paragraphs)
            {
                var paragraph = MakeParagraph(xml, p);
                table?.AppendChild(paragraph);
            }

            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty).Replace(" xmlns:tts=\"http://www.w3.org/ns/10/ttml#style\">", ">");
        }

        private static XmlNamespaceManager MakeNamespaceManager(XmlDocument xml)
        {
            var xmlNamespaceManager = new XmlNamespaceManager(xml.NameTable);
            xmlNamespaceManager.AddNamespace("ss", NamespaceSpreadsheet);
            xmlNamespaceManager.AddNamespace("html", NamespaceHtml);
            xmlNamespaceManager.AddNamespace(string.Empty, "urn:schemas-microsoft-com:office:spreadsheet");
            xmlNamespaceManager.AddNamespace("x", NamespaceExcel);
            xmlNamespaceManager.AddNamespace("o", NamespaceOffice);
            return xmlNamespaceManager;
        }

        private static XmlNode MakeParagraph(XmlDocument xml, Paragraph p)
        {
            var row = xml.CreateElement("Row", NamespaceSpreadsheet);
            var height = xml.CreateAttribute("ss:Height", NamespaceSpreadsheet);
            height.InnerText = "30";
            row.Attributes.Append(height);
            MakeCell(xml, row, p.StartTime.ToHHMMSSFF());
            MakeCell(xml, row, p.EndTime.ToHHMMSSFF());
            MakeCell(xml, row, HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, "\n"), true));
            MakeCell(xml, row, string.Empty);
            MakeCell(xml, row, string.Empty);
            MakeCell(xml, row, "0");
            MakeAlignmentCell(xml, p, row);
            MakeCell(xml, row, "None");
            MakeCell(xml, row, p.Actor);
            return row;
        }

        private static void MakeCell(XmlDocument xml, XmlElement row, string text)
        {
            var cell = xml.CreateElement("Cell", NamespaceSpreadsheet);
            var styleId = xml.CreateAttribute("ss:StyleID", NamespaceSpreadsheet);
            styleId.InnerText = "s0";
            cell.Attributes.Append(styleId);
            row.AppendChild(cell);
            var data = xml.CreateElement("Data", NamespaceSpreadsheet);
            var dataType = xml.CreateAttribute("ss:Type", NamespaceSpreadsheet);
            dataType.InnerText = "String";
            data.Attributes.Append(dataType);
            cell.AppendChild(data);
            data.InnerText = text;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            try
            {
                xml.LoadXml(sb.ToString().RemoveControlCharactersButWhiteSpace().Trim());
            }
            catch
            {
                xml.LoadXml(sb.ToString());
            }

            var xmlNamespaceManager = MakeNamespaceManager(xml);
            var rows = xml.SelectNodes("//ss:Row", xmlNamespaceManager);
            if (rows == null)
            {
                return;
            }

            foreach (XmlNode node in rows)
            {
                var cells = node.SelectNodes("ss:Cell", xmlNamespaceManager);
                if (cells?.Count >= 9)
                {
                    var start = cells[0].SelectSingleNode("ss:Data", xmlNamespaceManager)?.InnerText;
                    var end = cells[1].SelectSingleNode("ss:Data", xmlNamespaceManager)?.InnerText;
                    var text = cells[2].SelectSingleNode("ss:Data", xmlNamespaceManager)?.InnerText;
                    var alignment = cells[7].SelectSingleNode("ss:Data", xmlNamespaceManager)?.InnerText;

                    try
                    {
                        text = string.Join(Environment.NewLine, text.SplitToLines().ToArray());
                        var p = new Paragraph(
                            DecodeTimeCodeFrames(start, new[] { ':' }),
                            DecodeTimeCodeFrames(end, new[] { ':' }),
                            GetAlignment(alignment) + text);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                        if (_errorCount > 20)
                        {
                            return;
                        }
                    }
                }
            }

            subtitle.Renumber();
        }

        private static void MakeAlignmentCell(XmlDocument xml, Paragraph p, XmlElement row)
        {
            var topAlign = p.Text.StartsWith("{\\an7}", StringComparison.Ordinal) ||
                           p.Text.StartsWith("{\\an8}", StringComparison.Ordinal) ||
                           p.Text.StartsWith("{\\an9}", StringComparison.Ordinal);

            var leftAlign = p.Text.StartsWith("{\\an7}", StringComparison.Ordinal) ||
                            p.Text.StartsWith("{\\an4}", StringComparison.Ordinal) ||
                            p.Text.StartsWith("{\\an1}", StringComparison.Ordinal);

            var rightAlign = p.Text.StartsWith("{\\an9}", StringComparison.Ordinal) ||
                             p.Text.StartsWith("{\\an6}", StringComparison.Ordinal) ||
                             p.Text.StartsWith("{\\an3}", StringComparison.Ordinal);

            var verticalCenter = p.Text.StartsWith("{\\an4}", StringComparison.Ordinal) ||
                                 p.Text.StartsWith("{\\an5}", StringComparison.Ordinal) ||
                                 p.Text.StartsWith("{\\an6}", StringComparison.Ordinal);

            var hAlign = "HCenter";
            if (leftAlign)
            {
                hAlign = "HLeft";
            }
            else if (rightAlign)
            {
                hAlign = "HRight";
            }

            var vAlign = "VBottom";
            if (topAlign)
            {
                vAlign = "VTop";
            }
            else if (verticalCenter)
            {
                vAlign = "VCenter";
            }

            MakeCell(xml, row, $"{hAlign} {vAlign}");
        }

        private static string GetAlignment(string alignment)
        {
            if (string.IsNullOrEmpty(alignment))
            {
                return string.Empty;
            }

            var arr = alignment.Split();
            if (arr.Length != 2)
            {
                return string.Empty;
            }

            var hAlign = arr[0];
            var vAlign = arr[1];

            if (hAlign == "HLeft")
            {
                if (vAlign == "VTop")
                {
                    return "{\\an7}";
                }

                if (vAlign == "VCenter")
                {
                    return "{\\an4}";
                }

                return "{\\an1}";
            }

            if (hAlign == "HRight")
            {
                if (vAlign == "VTop")
                {
                    return "{\\an9}";
                }

                if (vAlign == "VCenter")
                {
                    return "{\\an6}";
                }

                return "{\\an3}";
            }

            if (vAlign == "VTop")
            {
                return "{\\an8}";
            }

            if (vAlign == "VCenter")
            {
                return "{\\an5}";
            }

            return string.Empty;
        }
    }
}
