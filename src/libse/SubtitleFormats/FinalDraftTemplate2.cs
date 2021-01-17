using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class FinalDraftTemplate2 : SubtitleFormat
    {
        public override string Extension => ".fdx";

        public override string Name => "Final Draft Template 2";

        private List<string> _paragraphTypes;

        public List<string> GetParagraphTypes(List<string> lines)
        {
            LoadSubtitle(new Subtitle(), lines, null);
            return _paragraphTypes;
        }
        public List<string> ActiveParagraphTypes { get; set; } = new List<string> { "Dialogue", "Parenthetical" };

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var lowercaseChosencategories = ActiveParagraphTypes.Select(p => p.ToLowerInvariant()).ToList();
            _paragraphTypes = new List<string>();
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
                        if (paragraphType != null)
                        {
                            if (paragraphType != null && !_paragraphTypes.Contains(paragraphType.InnerText))
                            {
                                _paragraphTypes.Add(paragraphType.InnerText);
                            }

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
                                actor = actor.ToUpperInvariant();
                            }
                            else if (paragraphType != null && lowercaseChosencategories.Contains(paragraphType.InnerText.ToLowerInvariant()))
                            {
                                var p = new Paragraph(Utilities.AutoBreakLine(sb.ToString().Trim()), 0, 0);
                                if (!string.IsNullOrWhiteSpace(actor))
                                {
                                    p.Actor = actor;
                                }

                                subtitle.Paragraphs.Add(p);
                            }
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
