using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class IssXml : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled); //00:02:56:02

        public override string Extension => ".ats";

        public override string Name => "ATS ISS";

        public override string ToText(Subtitle subtitle, string title)
        {
            string xmlStructure =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine +
                "<ISS>" + Environment.NewLine +
                "   <Project ScreenHeight=\"480\" ScreenWidth=\"720\" OffsetTc=\"0\" TcMemo=\"\" TcEnd=\"\" TcStart=\"\" TcUser=\"\" TransMemo=\"\" TransEnd=\"\" TransStart=\"\" TransUser=\"\" CreateMemo=\"\" CreateDate=\"2012-06-30\" CreateUser=\"\" MovieFile=\"C:\\Documents and Settings\\Imac\\My Documents\\밀레니엄3부_1.5.mpg\" Title=\"프로젝트명\"/>" + Environment.NewLine +
                "   <StyleList CurStyle=\"\">" + Environment.NewLine +
                "       <Style Name=\"해설자\">" + Environment.NewLine +
                "           <FontAttr BoxGradientType=\"0\" BoxGradient=\"0\" BoxOpacity2=\"100\" BoxColor2=\"0,0,0\" BoxOpacity=\"100\" BoxColor=\"0,255,0\" BoxMargin=\"(0,0)\" Box=\"0\" ShadowDepth=\"(2,2)\" ShadowOpacity=\"10\" ShadowAntiAlias=\"1\" ShadowColor=\"128,128,128\" Shadow=\"0\" OutlineHardness2=\"50\" OutlineWidth2=\"3\" OutlineAntiAlias2=\"1\" OutlineColor2=\"0,0,0\" Outline2=\"0\" OutlineHardness=\"50\" OutlineWidth=\"2\" OutlineAntiAlias=\"1\" OutlineColor=\"0,0,0\" Outline=\"0\" GradientType=\"0\" Gradient=\"0\" Opacity2=\"100\" AntiAlias2=\"1\" Color2=\"255,255,255\" Opacity=\"100\" AntiAlias=\"1\" Color=\"255,0,255\" StrikeOut=\"0\" Underline=\"0\" Italic=\"0\" Bold=\"1\" CharSet=\"129\" Width=\"12\" Height=\"14\" FaceName=\"굴림\"/>" + Environment.NewLine +
                "           <ParaAttr ORDER=\"LEFT\" ALIGN=\"MIDDLE\" JUSTIFY=\"CENTER\"/>" + Environment.NewLine +
                "       </Style>" + Environment.NewLine +
                "       <Style Name=\"보통\">" + Environment.NewLine +
                "           <FontAttr BoxGradientType=\"0\" BoxGradient=\"0\" BoxOpacity2=\"100\" BoxColor2=\"0,0,0\" BoxOpacity=\"100\" BoxColor=\"0,255,0\" BoxMargin=\"(0,0)\" Box=\"0\" ShadowDepth=\"(2,2)\" ShadowOpacity=\"10\" ShadowAntiAlias=\"1\" ShadowColor=\"128,128,128\" Shadow=\"0\" OutlineHardness2=\"50\" OutlineWidth2=\"3\" OutlineAntiAlias2=\"1\" OutlineColor2=\"0,0,0\" Outline2=\"0\" OutlineHardness=\"50\" OutlineWidth=\"2\" OutlineAntiAlias=\"1\" OutlineColor=\"0,0,0\" Outline=\"1\" GradientType=\"0\" Gradient=\"0\" Opacity2=\"100\" AntiAlias2=\"1\" Color2=\"255,255,255\" Opacity=\"100\" AntiAlias=\"1\" Color=\"255,255,255\" StrikeOut=\"0\" Underline=\"0\" Italic=\"0\" Bold=\"0\" CharSet=\"129\" Width=\"12\" Height=\"12\" FaceName=\"굴림체\"/>" + Environment.NewLine +
                "           <ParaAttr ORDER=\"CENTER\" ALIGN=\"MIDDLE\" JUSTIFY=\"CENTER\"/>" + Environment.NewLine +
                "       </Style>" + Environment.NewLine +
                "   </StyleList>" + Environment.NewLine +
                "   <Environment DefaultStyle=\"해설자\"/>" + Environment.NewLine +
                "   <VoiceCheckInOut VoiceOutCheckFrame=\"3\" VoiceOutCheckPoint=\"4\" VoiceInCheckFrame=\"3\" VoiceInCheckPoint=\"8\"/><MoveUserTcFrame Value=\"3\"/>" + Environment.NewLine +
                "   <AutoSave DurationMin=\"10\" Use=\"1\"/>" + Environment.NewLine +
                "   <AutoMoveByEnter Use=\"1\"/>" + Environment.NewLine +
                "   <AutoMoveByCharCount Use=\"0\"/>" + Environment.NewLine +
                "   <TrackList>" + Environment.NewLine +
                "       <Track Name=\"기본 트랙\" DisplayChannel=\"0\" Display=\"1\" RowCount=\"280\" Bottom=\"470\" Right=\"710\" Top=\"320\" Left=\"10\" StyleName=\"해설자\">" + Environment.NewLine +
                "           <FcpProperty Use2997DropFrame=\"1\" LineColor=\"0,0,0\" UseLineColor=\"0\" TextColor=\"255,255,255\" UseTextColor=\"0\" CenterY=\"0\" CenterX=\"0\" UseCenter=\"0\" TextOpacity=\"100\" LineWidth=\"50\" UseLineWidth=\"0\" Aspect=\"1.000000\" FontSize=\"26\" UseFontSize=\"0\" Alignment=\"2\" UseAlignment=\"0\" Style=\"1\" UseStyle=\"0\" FontName=\"Stone Sans ITC TT\" UseFontName=\"0\" Tracking=\"0.800000\" LineSoftness=\"15\" Leading=\"-10.000000\"/>" + Environment.NewLine +
                "           <ChannelDefineList>" + Environment.NewLine +
                "               <ChannelDefine Name=\"기본 채널\" Bottom=\"470\" Right=\"710\" Top=\"320\" Left=\"10\" StyleName=\"해설자\"/>" + Environment.NewLine +
                "               <ChannelDefine Name=\"영어\" Bottom=\"470\" Right=\"710\" Top=\"320\" Left=\"10\" StyleName=\"해설자\"/>" + Environment.NewLine +
                "               <ChannelDefine Name=\"시간\" Bottom=\"470\" Right=\"710\" Top=\"320\" Left=\"10\" StyleName=\"해설자\"/>" + Environment.NewLine +
                "           </ChannelDefineList>" + Environment.NewLine +
                "           <StItemList />" + Environment.NewLine +
                "       </Track>" + Environment.NewLine +
                "   </TrackList>" + Environment.NewLine +
                "</ISS>";

            string xmlTrackStructure =
                "<StTextList>" + Environment.NewLine +
                "    <StText StyleName=\"해설자\" Mark=\"0\"></StText>" + Environment.NewLine +
                "    <StText StyleName=\"해설자\" Mark=\"0\"/>" + Environment.NewLine +
                "    <StText StyleName=\"해설자\" Mark=\"0\">00:03:57:16</StText>" + Environment.NewLine +
                "</StTextList>";

            var xml = new XmlDocument { XmlResolver = null };
            xml.LoadXml(xmlStructure);
            // TODO: Set variables...
            XmlNode trackNode = xml.DocumentElement.SelectSingleNode("TrackList/Track/StItemList");

            int number = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // starttime + text
                XmlNode stItem = xml.CreateElement("StItem");
                stItem.InnerXml = xmlTrackStructure;

                XmlAttribute memo = xml.CreateAttribute("Memo");
                memo.InnerText = string.Empty;
                stItem.Attributes.Append(memo);

                XmlAttribute tc = xml.CreateAttribute("TC");
                tc.InnerText = ((int)Math.Round(p.StartTime.TotalMilliseconds)).ToString();
                stItem.Attributes.Append(tc);

                XmlAttribute row = xml.CreateAttribute("Row");
                row.InnerText = number.ToString();
                stItem.Attributes.Append(row);

                XmlNodeList list = stItem.SelectNodes("StTextList/StText");
                list[0].InnerText = p.Text;
                list[2].InnerText = p.StartTime.ToHHMMSSFF();
                trackNode.AppendChild(stItem);
                number++;

                // endtime
                stItem = xml.CreateElement("StItem");
                stItem.InnerXml = xmlTrackStructure;

                memo = xml.CreateAttribute("Memo");
                memo.InnerText = string.Empty;
                stItem.Attributes.Append(memo);

                tc = xml.CreateAttribute("TC");
                tc.InnerText = ((int)Math.Round(p.EndTime.TotalMilliseconds)).ToString();
                stItem.Attributes.Append(tc);

                row = xml.CreateAttribute("Row");
                row.InnerText = number.ToString();
                stItem.Attributes.Append(row);

                list = stItem.SelectNodes("StTextList/StText");
                list[0].InnerText = string.Empty;
                list[2].InnerText = p.EndTime.ToString();
                trackNode.AppendChild(stItem);
                number++;
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            if (!sb.ToString().Contains("<StTextList"))
            {
                _errorCount++;
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(sb.ToString().Trim());

                XmlNode use2997DropFrame = xml.DocumentElement.SelectSingleNode("TrackList/Track/FcpProperty");
                if (use2997DropFrame != null && use2997DropFrame.Attributes["Use2997DropFrame"] != null && use2997DropFrame.Attributes["Use2997DropFrame"].InnerText == "1")
                {
                    Configuration.Settings.General.CurrentFrameRate = 29.97;
                }

                foreach (XmlNode node in xml.SelectNodes("//StItem"))
                {
                    Paragraph p = new Paragraph();
                    p.StartTime = new TimeCode(long.Parse(node.Attributes["TC"].InnerText));
                    try
                    {
                        string text = string.Empty;
                        XmlNodeList list = node.SelectNodes("StTextList/StText");

                        if (list.Count == 3 && RegexTimeCodes.IsMatch(list[2].InnerText))
                        {
                            p.StartTime.TotalMilliseconds = TimeCode.ParseHHMMSSFFToMilliseconds(list[2].InnerText);
                        }

                        if (list.Count > 1)
                        {
                            text = (list[0].InnerText + Environment.NewLine + list[1].InnerText).Trim();
                        }
                        else if (list.Count == 1)
                        {
                            text = list[0].InnerText.Trim();
                        }

                        p.Text = text;
                        subtitle.Paragraphs.Add(p);
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

            int i = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                i++;
                var next = subtitle.GetParagraphOrDefault(i);
                if (next != null)
                {
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds;
                }
            }
            subtitle.RemoveEmptyLines();
        }

    }
}
