using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Titra : SubtitleFormat
    {
        //* 1 : 01:01:31:19 01:01:33:04 22c
        private static readonly Regex RegexTimeCodes = new Regex(@"^\* \d+ :\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d\t\d+c", RegexOptions.Compiled);
        private int _maxMsDiv10;

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Titra"; }
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

        private static int GetMaxCharsForDuration(double durationSeconds)
        {
            return (int)Math.Round(15.7 * durationSeconds);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"TVS - TITRA FILM

Titre VO :   L'heure d'été
Titre VST :
Création :   23/10/2009 - 16:31
Révision :   26/10/2009 - 17:48
Langue VO :  Français
Langue VST : Espagnol
Bobine :     e01

BEWARE : No more than 40 characters ON A LINE
ATTENTION : Pas plus de 40 caractères PAR LIGNE

");
            const string writeFormant = "* {0} :\t{1}\t{2}\t{3}{4}{5}";
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                var text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                sb.AppendLine(string.Format(writeFormant, index, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), GetMaxCharsForDuration(p.Duration.TotalSeconds) + "c", Environment.NewLine, text));
                sb.AppendLine();
                if (!text.Contains(Environment.NewLine))
                    sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22 00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            _maxMsDiv10 = 0;
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    try
                    {
                        var arr = line.Split('\t');
                        string start = arr[1];
                        string end = arr[2];

                        string[] startParts = start.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), string.Empty);
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    catch
                    {
                        _errorCount += 10;
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = line;
                    else
                        p.Text = p.Text + Environment.NewLine + line;
                }
                else
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            var hour = int.Parse(parts[0]);
            var minutes = int.Parse(parts[1]);
            var seconds = int.Parse(parts[2]);
            var frames = int.Parse(parts[3]);

            if (frames > _maxMsDiv10)
                _maxMsDiv10 = frames;

            return new TimeCode(hour, minutes, seconds, FramesToMillisecondsMax999(frames));
        }

    }
}
