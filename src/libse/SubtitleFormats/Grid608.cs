using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// CCExtractor's Grid 608 format ("--out=g608", see docs/G608.TXT): SubRip-like blocks where
    /// each caption is the verbatim 15-row CEA-608 grid. Every row is 96 characters: 32 of text,
    /// 32 of per-cell color (0-9, 9 = transparent/empty) and 32 of per-cell font
    /// (R=regular, I=italics, U=underline, B=underline+italics).
    /// Without this reader the files load as SubRip with 15 rows of grid codes per cue.
    /// </summary>
    public class Grid608 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d,\d\d\d --> \d\d:\d\d:\d\d,\d\d\d$", RegexOptions.Compiled);
        private static readonly Regex RegexGridRow = new Regex(@"^.{32}[0-9E]{32}[RIUBE]{32}\s*$", RegexOptions.Compiled);

        private const int GridRows = 15;
        private const int GridColumns = 32;

        public override string Extension => ".g608";

        public override string Name => "Grid 608";

        public override bool IsMine(List<string> lines, string fileName)
        {
            // require at least one well-formed grid row so plain SubRip files never match
            if (!lines.Exists(l => l.Length >= 3 * GridColumns && RegexGridRow.IsMatch(l)))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            var count = 1;
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine(count.ToString());
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)} --> {EncodeTimeCode(p.EndTime)}");

                var textLines = HtmlUtil.RemoveHtmlTags(p.Text, true).SplitToLines();
                var firstTextRow = Math.Max(0, GridRows - textLines.Count);
                for (var row = 0; row < GridRows; row++)
                {
                    var text = string.Empty;
                    if (row >= firstTextRow && row - firstTextRow < textLines.Count)
                    {
                        text = textLines[row - firstTextRow];
                        if (text.Length > GridColumns)
                        {
                            text = text.Substring(0, GridColumns);
                        }
                    }

                    var colors = new StringBuilder(GridColumns);
                    for (var column = 0; column < GridColumns; column++)
                    {
                        colors.Append(column < text.Length && text[column] != ' ' ? '0' : '9');
                    }

                    sb.AppendLine(text.PadRight(GridColumns) + colors + new string('R', GridColumns));
                }

                sb.AppendLine();
                count++;
            }

            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00},{time.Milliseconds:000}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            _errorCount = 0;

            Paragraph paragraph = null;
            var text = new StringBuilder();
            foreach (var line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    AddParagraph(subtitle, paragraph, text);
                    text.Clear();
                    try
                    {
                        var parts = line.Split(new[] { ':', ',', ' ', '-', '>' }, StringSplitOptions.RemoveEmptyEntries);
                        paragraph = new Paragraph(
                            new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3])),
                            new TimeCode(int.Parse(parts[4]), int.Parse(parts[5]), int.Parse(parts[6]), int.Parse(parts[7])),
                            string.Empty);
                    }
                    catch
                    {
                        _errorCount++;
                        paragraph = null;
                    }
                }
                else if (paragraph != null && line.Length >= 3 * GridColumns && RegexGridRow.IsMatch(line))
                {
                    // CCExtractor writes NUL bytes for grid cells a stream never touched - blanks
                    var textCells = line.Substring(0, GridColumns).ToCharArray();
                    for (var column = 0; column < textCells.Length; column++)
                    {
                        if (char.IsControl(textCells[column]))
                        {
                            textCells[column] = ' ';
                        }
                    }

                    var textBlock = new string(textCells);
                    var rowText = textBlock.Trim();
                    if (rowText.Length > 0)
                    {
                        // whole-row italics is the common case worth keeping (font block: I = italics)
                        var fontBlock = line.Substring(2 * GridColumns, GridColumns);
                        var firstCell = 0;
                        while (firstCell < GridColumns && textBlock[firstCell] == ' ')
                        {
                            firstCell++;
                        }

                        var isItalic = true;
                        for (var column = 0; column < rowText.Length && isItalic; column++)
                        {
                            if (textBlock[firstCell + column] != ' ' && fontBlock[firstCell + column] != 'I' && fontBlock[firstCell + column] != 'B')
                            {
                                isItalic = false;
                            }
                        }

                        if (text.Length > 0)
                        {
                            text.Append(Environment.NewLine);
                        }

                        text.Append(isItalic ? "<i>" + rowText + "</i>" : rowText);
                    }
                }
                else if (paragraph == null && !string.IsNullOrWhiteSpace(line) && !Utilities.IsInteger(line.Trim()))
                {
                    _errorCount++;
                }
            }

            AddParagraph(subtitle, paragraph, text);
            subtitle.Renumber();
        }

        private static void AddParagraph(Subtitle subtitle, Paragraph paragraph, StringBuilder text)
        {
            if (paragraph == null)
            {
                return;
            }

            paragraph.Text = text.ToString()
                .Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine)
                .Trim();
            if (paragraph.Text.Length > 0)
            {
                subtitle.Paragraphs.Add(paragraph);
            }
        }
    }
}
