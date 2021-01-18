using Nikse.SubtitleEdit.Core.Common;
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

        public override string Extension => ".txt";

        public override string Name => "Titra";

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
            const string writeFormat = "* {0} :\t{1}\t{2}\t{3}{4}{5}";
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                var text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                sb.AppendLine(string.Format(writeFormat, index, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), GetMaxCharsForDuration(p.Duration.TotalSeconds) + "c", Environment.NewLine, text));
                sb.AppendLine();
                if (!text.Contains(Environment.NewLine))
                {
                    sb.AppendLine();
                }
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

                        string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), string.Empty);
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
                    {
                        p.Text = line;
                    }
                    else
                    {
                        p.Text = p.Text + Environment.NewLine + line;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }

    }
}
