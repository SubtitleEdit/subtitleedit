﻿using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// PNS subtitle format
    ///
    ///00 00 + control bytes = 16 bytes
    ///
    ///START-MIN
    ///-----
    ///|
    ///|           START-FRAME
    ///|           --
    ///|           |     END-MIN
    ///|           |     -----
    ///|           |     |           END-FRAME
    ///|           |     |           --
    ///|           |     |           |
    ///|           |     |           |     TEXT-LENGTH
    ///|           |     |           |     --
    ///|           |     |           |     |
    ///02 00 00 00 02 01 7B 00 00 00 00 01 2D 00
    /// </summary>
    public class Pns : SubtitleFormat
    {
        public override string Extension => ".pns";

        public const string NameOfFormat = "PNS";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName) && fileName.EndsWith(".pns", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var fi = new FileInfo(fileName);
                    if (fi.Length > 100 && fi.Length < 1024000) // not too small or too big
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

                        if (buffer[00] != 0 &&
                            buffer[01] == 0)
                        {
                            var sub = new Subtitle();
                            LoadSubtitle(sub, null, fileName);
                            return sub.Paragraphs.Count > 0;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            var buffer = FileUtil.ReadAllBytesShared(fileName);

            int index = 40;
            while (index < buffer.Length)
            {
                var p = GetParagraph(ref index, buffer);
                if (p != null)
                {
                    subtitle.Paragraphs.Add(p);
                }
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private Paragraph GetParagraph(ref int index, byte[] buffer)
        {
            while (index < buffer.Length - 20)
            {
                // pattern for control codes
                if (buffer.Length > index + 20 &&
                    buffer[index + 00] == 0 &&
                    buffer[index + 01] == 0 &&
                    //buffer[index + 02] == 0 && // start min
                    //buffer[index + 03] == 0 &&
                    buffer[index + 04] == 0 &&
                    buffer[index + 05] == 0 &&
                    //buffer[index + 06] == 0 && // start frame
                    buffer[index + 07] <= 1 &&
                    //buffer[index + 08] == 0 && // end min
                    //buffer[index + 09] == 0 &&
                    buffer[index + 10] == 0 &&
                    buffer[index + 11] == 0 &&
                    //buffer[index + 12] == 0 && // end frame
                    buffer[index + 13] <= 1 &&
                    //buffer[index + 14] == 0 && // text length
                    buffer[index + 15] == 0)
                {
                    int startSeconds = buffer[index + 3] * 256 + buffer[index + 2];
                    int startFrame = buffer[index + 6];

                    int endSeconds = buffer[index + 9] * 256 + buffer[index + 8];
                    int endFrame = buffer[index + 12];

                    int textLength = buffer[index + 14];

                    if (buffer.Length > index + 15 + textLength)
                    {
                        var sb = new StringBuilder();
                        var skipCount = 0;
                        for (int j = index + 16; j < index + 16 + textLength; j++)
                        {
                            if (skipCount > 0)
                            {
                                skipCount--;
                                continue;
                            }

                            var v = buffer[j];

                            if (v < 32 && v != 0xd && v != 0xa)
                            {
                                if (v == 1)
                                {
                                    skipCount = 1;
                                }
                                else if (v == 2)
                                {
                                    skipCount = 2;
                                }
                                else if (v == 5)
                                {
                                    sb.Append("<i>");
                                }
                                else if (v == 15)
                                {
                                    sb.Append("</i>");
                                }
                            }
                            else
                            {
                                sb.Append(Encoding.GetEncoding(1250).GetString(buffer, j, 1)); // encoding?
                            }
                        }

                        var text = sb.ToString();
                        text = text.Replace("\0", string.Empty);
                        text = text.Replace("\n", "\r");
                        text = text.Replace("\r", Environment.NewLine);
                        text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                        text = string.Join(Environment.NewLine, text.SplitToLines());
                        text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                        if (text.StartsWith("</i>"))
                        {
                            text = text.Remove(0, "</i>".Length);
                        }

                        text = text.Trim();

                        text = FixItalics(text);

                        index += 15 + textLength;
                        var p = new Paragraph(text, startSeconds * 1000 + FramesToMillisecondsMax999(startFrame), endSeconds * 1000 + FramesToMillisecondsMax999(endFrame));
                        return p;
                    }
                    index += 100;
                    return null;
                }
                index++;
            }
            index++;
            return null;
        }

        private string FixItalics(string text)
        {
            if (text.StartsWith("<i>", StringComparison.Ordinal) && !text.Contains("</i>"))
            {
                return "<i>" + text.Replace("<i>", string.Empty) + "</i>";
            }

            return text;
        }
    }
}
