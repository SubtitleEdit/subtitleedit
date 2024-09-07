﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ErrorsGoTo : Form
    {
        private readonly Subtitle _subtitle;

        public ErrorsGoTo(Subtitle subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.Main.Errors;
            buttonExport.Text = LanguageSettings.Current.MultipleReplace.Export;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            columnHeaderNumber.Text = LanguageSettings.Current.General.NumberSymbol;
            columnHeaderStartTime.Text = LanguageSettings.Current.General.StartTime;
            columnHeaderText.Text = LanguageSettings.Current.Main.Errors;

            _subtitle = new Subtitle(subtitle);

            ListErrors(subtitle);

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void ListErrors(Subtitle subtitle)
        {
            listViewErrors.BeginUpdate();
            listViewErrors.Items.Clear();
            foreach (var p in _subtitle.Paragraphs)
            {
                var errors = GetErrors(p, _subtitle.Paragraphs.IndexOf(p), subtitle.Paragraphs);

                p.Bookmark = string.Empty;
                if (errors.Count > 0)
                {
                    p.Bookmark = string.Join(" -- ", errors);
                    var item = new ListViewItem("#" + p.Number) { Tag = p };
                    item.SubItems.Add(p.StartTime.ToShortDisplayString());
                    item.SubItems.Add(p.Bookmark);
                    listViewErrors.Items.Add(item);
                }
            }

            listViewErrors.EndUpdate();

            labelCount.Text = $"{LanguageSettings.Current.FindDialog.Count}: {listViewErrors.Items.Count}";
        }

        private List<string> GetErrors(Paragraph paragraph, int i, IReadOnlyList<Paragraph> paragraphs)
        {
            var errors = new List<string>();

            if (paragraph.StartTime.IsMaxTime || paragraph.EndTime.IsMaxTime)
            {
                errors.Add("START/END: No time code");
            }
            else
            {
                if (paragraph.WordsPerMinute > Configuration.Settings.General.SubtitleMaximumWordsPerMinute)
                {
                    errors.Add($"WPM: {paragraph.WordsPerMinute:#,###.00} > {Configuration.Settings.General.SubtitleMaximumWordsPerMinute}");
                }

                var charactersPerSecond = paragraph.GetCharactersPerSecond();
                if (charactersPerSecond > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                {
                    errors.Add($"CPS: {charactersPerSecond:#,###.00} > {Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds}");
                }

                if (paragraph.DurationTotalMilliseconds < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                {
                    errors.Add($"Min duration: {paragraph.DurationTotalMilliseconds:#,###.00} < {Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds}");
                }

                if (paragraph.DurationTotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    errors.Add($"Max duration: {paragraph.DurationTotalMilliseconds:#,###.00} > {Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds}");
                }

                if (i > 0 && i < paragraphs.Count)
                {
                    var prev = paragraphs[i - 1];
                    if (paragraph.StartTime.TotalMilliseconds < prev.EndTime.TotalMilliseconds && !prev.EndTime.IsMaxTime)
                    {
                        errors.Add("Overlap with prev");
                    }
                }

                if (i >= 0 && i < paragraphs.Count - 1)
                {
                    var next = paragraphs[i + 1];
                    var gapMilliseconds = (int)Math.Round(next.StartTime.TotalMilliseconds - paragraph.EndTime.TotalMilliseconds);
                    if (gapMilliseconds < 0)
                    {
                        errors.Add("Overlap with next");
                    }
                    else if (gapMilliseconds < Configuration.Settings.General.MinimumMillisecondsBetweenLines)
                    {
                        errors.Add($"Gap to next too small ({gapMilliseconds} < {Configuration.Settings.General.MinimumMillisecondsBetweenLines})");
                    }
                }
            }

            var s = HtmlUtil.RemoveHtmlTags(paragraph.Text, true);
            foreach (var line in s.SplitToLines())
            {
                var count = line.CountCharacters(false);
                if (count > Configuration.Settings.General.SubtitleLineMaximumLength)
                {
                    errors.Add($"Line too long: {count} > {Configuration.Settings.General.SubtitleLineMaximumLength}");
                }
            }

            var noOfLines = paragraph.NumberOfLines;
            if (s.CountCharacters(false) <= Configuration.Settings.General.SubtitleLineMaximumLength * noOfLines)
            {
                if (noOfLines > Configuration.Settings.General.MaxNumberOfLines)
                {
                    errors.Add($"Too many lines: {noOfLines} > {Configuration.Settings.General.MaxNumberOfLines}");
                }
            }

            if (Configuration.Settings.Tools.ListViewSyntaxColorWideLines)
            {
                s = HtmlUtil.RemoveHtmlTags(paragraph.Text, true);
                foreach (var line in s.SplitToLines())
                {
                    var pixels = TextWidth.CalcPixelWidth(line);
                    if (pixels > Configuration.Settings.General.SubtitleLineMaximumPixelWidth)
                    {
                        errors.Add($"Line too wide in pixels: {pixels} > {Configuration.Settings.General.SubtitleLineMaximumPixelWidth}");
                    }
                }
            }

            return errors;
        }

        public int ErrorIndex { get; private set; }

        private void listViewErrors_DoubleClick(object sender, EventArgs e)
        {
            if (listViewErrors.SelectedItems.Count > 0)
            {
                var p = (Paragraph)listViewErrors.SelectedItems[0].Tag;
                ErrorIndex = _subtitle.Paragraphs.IndexOf(p);
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewErrors.SelectedItems.Count > 0)
            {
                var p = (Paragraph)listViewErrors.SelectedItems[0].Tag;
                ErrorIndex = _subtitle.Paragraphs.IndexOf(p);
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ErrorsGoTo_KeyDown(object sender, KeyEventArgs e)
        {
            if (listViewErrors.Focused && e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp(string.Empty);
                e.SuppressKeyPress = true;
            }
        }

        private void ErrorsGoTo_ResizeEnd(object sender, EventArgs e)
        {
            listViewErrors.AutoSizeLastColumn();
        }

        private void ErrorsGoTo_Shown(object sender, EventArgs e)
        {
            ErrorsGoTo_ResizeEnd(sender, e);
            listViewErrors.Focus();
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            ExportErrorsAsCsv(_subtitle, this);
        }

        public static void ExportErrorsAsCsv(Subtitle subtitle, Form form)
        {
            using (var saveDialog = new SaveFileDialog { FileName = string.Empty, Filter = "CSV|*.csv" })
            {
                if (saveDialog.ShowDialog(form) != DialogResult.OK)
                {
                    return;
                }

                var sb = new StringBuilder();
                foreach (var p in subtitle.Paragraphs.Where(p => p.Bookmark != null))
                {
                    sb.AppendLine(MakeParagraphCsvLine(p));
                }

                File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);
            }
        }

        private static string MakeParagraphCsvLine(Paragraph paragraph)
        {
            const string separator = ",";
            var sb = new StringBuilder();
            sb.Append(paragraph.Number + separator);
            sb.Append(ToCsvText(paragraph.StartTime.ToDisplayString()) + separator);
            sb.Append(ToCsvText(paragraph.EndTime.ToDisplayString()) + separator);
            sb.Append(ToCsvText(paragraph.Duration.ToShortDisplayString()) + separator);
            sb.Append(ToCsvText(paragraph.Text.Replace(Environment.NewLine, "<br />")) + separator);
            sb.Append(ToCsvText(paragraph.Bookmark) + separator);
            return sb.ToString();
        }

        private static string ToCsvText(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append("\"");
            foreach (var nextChar in input)
            {
                sb.Append(nextChar);
                if (nextChar == '"')
                {
                    sb.Append("\"");
                }
            }

            sb.Append("\"");
            return sb.ToString();
        }
    }
}
