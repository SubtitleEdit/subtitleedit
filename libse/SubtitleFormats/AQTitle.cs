using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AQTitle : SubtitleFormat
    {
        private enum ExpectingLine
        {
            TimeStart,
            Text,
            TimeEndOrText,
        }

        public override string Extension => ".aqt";

        public override string Name => "AQTitle";

        public override bool IsTimeBased => false;

        public override string ToText(Subtitle subtitle, string title)
        {
            //-->> 072058
            //<i>Meine Mutter und meine Schwester,

            //-->> 072169

            //-->> 072172
            //<i>die in Zürich lebt, und ich,

            //-->> 072247
            const string paragraphWriteFormat = "-->> {0}{3}{2}{3}-->> {1}{3}{3}";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = Utilities.RemoveSsaTags(p.Text);
                int noOfLines = Utilities.GetNumberOfLines(text);
                if (noOfLines > 2)
                {
                    text = Utilities.AutoBreakLine(text);
                }
                else if (noOfLines == 1)
                {
                    text += Environment.NewLine;
                }

                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text, Environment.NewLine));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var paragraph = new Paragraph();
            var expecting = ExpectingLine.TimeStart;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (line.StartsWith("-->> ", StringComparison.Ordinal))
                {
                    string timePart = line.Substring(4).Trim();
                    if (timePart.Length > 0)
                    {
                        try
                        {
                            var tc = DecodeTimeCode(timePart);
                            if (expecting == ExpectingLine.TimeStart)
                            {
                                paragraph = new Paragraph { StartTime = tc };
                                expecting = ExpectingLine.Text;
                            }
                            else if (expecting == ExpectingLine.TimeEndOrText)
                            {
                                paragraph.EndTime = tc;
                                subtitle.Paragraphs.Add(paragraph);
                                paragraph = new Paragraph();
                                expecting = ExpectingLine.TimeStart;
                            }
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.TimeStart;
                        }
                    }
                }
                else
                {
                    if (expecting == ExpectingLine.Text || expecting == ExpectingLine.TimeEndOrText)
                    {
                        if (line.Length > 0)
                        {
                            string text = line.Replace("|", Environment.NewLine);
                            if (string.IsNullOrEmpty(paragraph.Text))
                            {
                                paragraph.Text = text.Trim();
                            }
                            else
                            {
                                paragraph.Text += Environment.NewLine + text;
                            }

                            if (paragraph.Text.Length > 2000)
                            {
                                _errorCount += 100;
                                return;
                            }
                        }
                        expecting = ExpectingLine.TimeEndOrText;
                    }
                    else if (expecting == ExpectingLine.TimeStart && !string.IsNullOrWhiteSpace(line))
                    {
                        int ms = (int)paragraph.EndTime.TotalMilliseconds;
                        paragraph = new Paragraph { StartTime = { TotalMilliseconds = ms }, Text = line.Trim() };
                        expecting = ExpectingLine.TimeEndOrText;
                    }
                }
            }
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            int frames = MillisecondsToFrames(time.TotalMilliseconds) + 1;
            return frames.ToString(CultureInfo.InvariantCulture);
        }

        private static TimeCode DecodeTimeCode(string timePart)
        {
            int milliseconds = (int)Math.Round(TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate * int.Parse(timePart));
            return new TimeCode(milliseconds);
        }

    }
}
