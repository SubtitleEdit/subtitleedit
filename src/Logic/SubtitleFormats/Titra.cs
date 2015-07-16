﻿using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Titra : SubtitleFormat
    {
        //* 1 : 01:01:31:19 01:01:33:04 22c
        private static Regex regexTimeCodes = new Regex(@"^\* \d+ :\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d\t\d+c", RegexOptions.Compiled);
        private int _maxMsDiv10 = 0;

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
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                sb.AppendLine(string.Format("* {0} :\t{1}\t{2}\t{3}{4}{5}", index, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), GetMaxCharsForDuration(p.Duration.TotalSeconds) + "c", Environment.NewLine, text));
                sb.AppendLine();
                if (!text.Contains(Environment.NewLine))
                    sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
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
                if (regexTimeCodes.IsMatch(line))
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
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            int frames = int.Parse(parts[3]);

            if (frames > _maxMsDiv10)
                _maxMsDiv10 = frames;

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(frames));
        }

    }
}
