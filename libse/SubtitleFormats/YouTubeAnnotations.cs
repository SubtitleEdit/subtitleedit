using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class YouTubeAnnotations : SubtitleFormat
    {
        public interface IGetYouTubeAnnotationStyles
        {
            List<string> GetYouTubeAnnotationStyles(Dictionary<string, int> stylesWithCount);
        }

        public static IGetYouTubeAnnotationStyles GetYouTubeAnnotationStyles { get; set; }

        private bool _promtForStyles = true;

        public override string Extension => ".xml";

        public const string NameOfFormat = "YouTube Annotations";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            _promtForStyles = false;
            LoadSubtitle(subtitle, lines, fileName);
            _promtForStyles = true;
            return subtitle.Paragraphs.Count > 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                "<document>" + Environment.NewLine +
                "   <requestheader video_id=\"X\"/>" + Environment.NewLine +
                "  <annotations/>" + Environment.NewLine +
                "</document>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);

            XmlNode annotations = xml.DocumentElement.SelectSingleNode("annotations");

            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //<annotation id="annotation_126995" author="StopFear" type="text" style="speech">
                //  <TEXT>BUT now something inside is BROKEN!</TEXT>
                //  <segment>
                //    <movingRegion type="anchored">
                //      <anchoredRegion x="6.005" y="9.231" w="26.328" h="18.154" sx="40.647" sy="14.462" t="0:01:08.0" d="0"/>
                //      <anchoredRegion x="6.005" y="9.231" w="26.328" h="18.154" sx="40.647" sy="14.462" t="0:01:13.0" d="0"/>
                //    </movingRegion>
                //  </segment>
                //</annotation>

                XmlNode annotation = xml.CreateElement("annotation");

                XmlAttribute att = xml.CreateAttribute("id");
                att.InnerText = "annotation_" + count;
                annotation.Attributes.Append(att);

                att = xml.CreateAttribute("author");
                att.InnerText = "Subtitle Edit";
                annotation.Attributes.Append(att);

                att = xml.CreateAttribute("type");
                att.InnerText = "text";
                annotation.Attributes.Append(att);

                att = xml.CreateAttribute("style");
                att.InnerText = "speech";
                annotation.Attributes.Append(att);

                XmlNode text = xml.CreateElement("TEXT");
                text.InnerText = p.Text;
                annotation.AppendChild(text);

                XmlNode segment = xml.CreateElement("segment");
                annotation.AppendChild(segment);

                XmlNode movingRegion = xml.CreateElement("movingRegion");
                segment.AppendChild(movingRegion);

                att = xml.CreateAttribute("type");
                att.InnerText = "anchored";
                movingRegion.Attributes.Append(att);

                XmlNode anchoredRegion = xml.CreateElement("anchoredRegion");
                movingRegion.AppendChild(anchoredRegion);
                att = xml.CreateAttribute("t");
                att.InnerText = EncodeTime(p.StartTime);
                anchoredRegion.Attributes.Append(att);
                att = xml.CreateAttribute("d");
                att.InnerText = "0";
                anchoredRegion.Attributes.Append(att);

                anchoredRegion = xml.CreateElement("anchoredRegion");
                movingRegion.AppendChild(anchoredRegion);
                att = xml.CreateAttribute("t");
                att.InnerText = EncodeTime(p.EndTime);
                anchoredRegion.Attributes.Append(att);
                att = xml.CreateAttribute("d");
                att.InnerText = "0";
                anchoredRegion.Attributes.Append(att);

                annotations.AppendChild(annotation);
                count++;
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            if (!sb.ToString().Contains("</annotations>") || !sb.ToString().Contains("</TEXT>"))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                string xmlText = sb.ToString();
                xml.LoadXml(xmlText);
                var styles = new List<string> { "speech" };

                if (_promtForStyles)
                {
                    var stylesWithCount = new Dictionary<string, int>();
                    foreach (XmlNode node in xml.SelectNodes("//annotation"))
                    {
                        try
                        {
                            if (node.Attributes["style"] != null && node.Attributes["style"].Value != null)
                            {
                                string style = node.Attributes["style"].Value;

                                XmlNode textNode = node.SelectSingleNode("TEXT");
                                XmlNodeList regions = node.SelectNodes("segment/movingRegion/anchoredRegion");

                                if (regions.Count != 2)
                                {
                                    regions = node.SelectNodes("segment/movingRegion/rectRegion");
                                }

                                if (textNode != null && regions.Count == 2)
                                {
                                    if (stylesWithCount.ContainsKey(style))
                                    {
                                        stylesWithCount[style]++;
                                    }
                                    else
                                    {
                                        stylesWithCount.Add(style, 1);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                    }
                    if (stylesWithCount.Count > 1 && GetYouTubeAnnotationStyles != null)
                    {
                        styles = GetYouTubeAnnotationStyles.GetYouTubeAnnotationStyles(stylesWithCount);
                    }
                    else
                    {
                        styles.Clear();
                        foreach (var k in stylesWithCount.Keys)
                        {
                            styles.Add(k);
                        }
                    }
                }
                else
                {
                    styles.Add("popup");
                    styles.Add("anchored");
                }

                foreach (XmlNode node in xml.SelectNodes("//annotation"))
                {
                    try
                    {
                        if (node.Attributes["style"] != null && styles.Contains(node.Attributes["style"].Value))
                        {
                            XmlNode textNode = node.SelectSingleNode("TEXT");
                            XmlNodeList regions = node.SelectNodes("segment/movingRegion/anchoredRegion");

                            if (regions.Count != 2)
                            {
                                regions = node.SelectNodes("segment/movingRegion/rectRegion");
                            }

                            if (textNode != null && regions.Count == 2)
                            {
                                string startTime = regions[0].Attributes["t"].Value;
                                string endTime = regions[1].Attributes["t"].Value;
                                var p = new Paragraph();
                                p.StartTime = DecodeTimeCode(startTime);
                                p.EndTime = DecodeTimeCode(endTime);
                                p.Text = textNode.InnerText;
                                subtitle.Paragraphs.Add(p);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        _errorCount++;
                    }
                }
                subtitle.Sort(SubtitleSortCriteria.StartTime); // force order by start time
                subtitle.Renumber();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                _errorCount = 1;
                return;
            }
        }

        private static TimeCode DecodeTimeCode(string time)
        {
            string[] arr = time.Split(new[] { '.', ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 3)
            {
                return new TimeCode(0, int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
            }

            return new TimeCode(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), int.Parse(arr[3]));
        }

        private static string EncodeTime(TimeCode timeCode)
        {
            //0:01:08.0
            return $"{timeCode.Hours}:{timeCode.Minutes:00}:{timeCode.Seconds:00}.{timeCode.Milliseconds}";
        }

    }
}
