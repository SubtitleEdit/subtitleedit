using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class FinalDraftTemplate2 : SubtitleFormat
    {
        public override string Extension => ".fdx";

        public override string Name => "Final Draft Template 2";

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            var actor = string.Empty;
            lines.ForEach(line => sb.AppendLine(line));
            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(sb.ToString().Trim());
                foreach (XmlNode node in xml.SelectNodes("FinalDraft/Content/Paragraph"))
                {
                    try
                    {
                        var paragraphType = node.Attributes["Type"];
                        sb.Clear();
                        foreach (XmlNode text in node.SelectNodes("Text"))
                        {
                            sb.AppendLine(text.InnerText);
                        }

                        if (paragraphType != null && paragraphType.InnerText.Equals("Character", StringComparison.OrdinalIgnoreCase))
                        {
                            actor = sb.ToString().Replace(Environment.NewLine, " ").Trim();
                            actor = actor.Replace("( CONT'D )", string.Empty).Trim();
                            actor = actor.Replace("( CONT’D )", string.Empty).Trim();
                            actor = actor.Replace("(CONT'D)", string.Empty).Trim();
                            actor = actor.Replace("(CONT’D)", string.Empty).Trim();
                            actor = actor.ToUpper();
                        }
                        else if (paragraphType != null && paragraphType.InnerText.Equals("Dialogue", StringComparison.OrdinalIgnoreCase))
                        {
                            var p = new Paragraph(Utilities.AutoBreakLine(sb.ToString().Trim()), 0, 0);
                            if (!string.IsNullOrWhiteSpace(actor))
                                p.Actor = actor;
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                subtitle.Renumber();
            }
            catch
            {
                _errorCount = 1;
                return;
            }
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine(p.Text);
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }
    }
}
