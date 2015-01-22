﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class FormRemoveTextForHearImpaired : PositionAndSizeForm
    {
        private Subtitle _subtitle;
        private readonly LanguageStructure.RemoveTextFromHearImpaired _language;
        private RemoveTextForHI _removeTextForHILib;

        public FormRemoveTextForHearImpaired()
        {
            InitializeComponent();

            _removeTextForHILib = new RemoveTextForHI(GetSettings());

            checkBoxRemoveTextBetweenSquares.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets;
            checkBoxRemoveTextBetweenParentheses.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses;
            checkBoxRemoveTextBetweenBrackets.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets;
            checkBoxRemoveTextBetweenQuestionMarks.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks;
            checkBoxRemoveTextBetweenCustomTags.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom;
            comboBoxCustomStart.Text = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore;
            comboBoxCustomEnd.Text = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter;
            checkBoxOnlyIfInSeparateLine.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeperateLines;
            checkBoxRemoveTextBeforeColon.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon;
            checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase;
            checkBoxColonSeparateLine.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine;
            checkBoxRemoveInterjections.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections;
            checkBoxRemoveWhereContains.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContains;
            checkBoxRemoveIfAllUppercase.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase;
            comboBoxRemoveIfTextContains.Text = Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContainsText;

            _language = Configuration.Settings.Language.RemoveTextFromHearImpaired;
            Text = _language.Title;
            groupBoxRemoveTextConditions.Text = _language.RemoveTextConditions;
            labelAnd.Text = _language.And;
            labelRemoveTextBetween.Text = _language.RemoveTextBetween;
            checkBoxRemoveTextBeforeColon.Text = _language.RemoveTextBeforeColon;
            checkBoxRemoveTextBeforeColonOnlyUppercase.Text = _language.OnlyIfTextIsUppercase;
            checkBoxOnlyIfInSeparateLine.Text = _language.OnlyIfInSeparateLine;
            checkBoxColonSeparateLine.Text = _language.OnlyIfInSeparateLine;
            checkBoxRemoveTextBetweenBrackets.Text = _language.Brackets;
            checkBoxRemoveTextBetweenParentheses.Text = _language.Parentheses;
            checkBoxRemoveTextBetweenQuestionMarks.Text = _language.QuestionMarks;
            checkBoxRemoveTextBetweenSquares.Text = _language.SquareBrackets;
            checkBoxRemoveWhereContains.Text = _language.RemoveTextIfContains;
            checkBoxRemoveIfAllUppercase.Text = _language.RemoveTextIfAllUppercase;
            checkBoxRemoveInterjections.Text = _language.RemoveInterjections;
            buttonEditInterjections.Text = _language.EditInterjections;
            buttonEditInterjections.Left = checkBoxRemoveInterjections.Left + checkBoxRemoveInterjections.Width;
            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.General.Before;
            listViewFixes.Columns[3].Text = Configuration.Settings.Language.General.After;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public static string RemoveStartEndNoise(string text)
        {
            const int i = ('i' | 'I');
            const int b = ('b' | 'B');
            const int u = ('u' | 'U');

            var s = text.Trim();
            // <i> <I> | <b> <B> | <u> <U>
            if (s.Length > 3 && s[0] == '<' && s[2] == '>' &&
                ((s[1] & i) == text[1] || (s[1] & b) == s[1] || (s[1] & u) == s[1]))
            {
                s = s.Substring(0, 3);
            }
            // </i> </I> | </b> </B> | </u> </U>
            int idx = (s.Length - 4);
            if (s.Length > 4 && s[idx + 2] == '/' && s[idx] == '<' &&
                 ((s[idx + 3] & i) == s[idx] || (s[idx + 3] & b) == s[idx] || (s[idx + 3] & u) == s[idx]))
            {
                s = s.Substring(0, s.Length - 4);
            }

            if (s.StartsWith('-') && s.Length > 2)
                s = s.TrimStart('-');

            return s.Trim();
        }

        public void Initialize(Subtitle subtitle)
        {
            if (Environment.OSVersion.Version.Major < 6) // 6 == Vista/Win2008Server/Win7
            {
                const string unicodeFontName = Utilities.WinXP2KUnicodeFontName;
                float fontSize = comboBoxCustomStart.Font.Size;
                comboBoxCustomStart.Font = new Font(unicodeFontName, fontSize);
                comboBoxCustomEnd.Font = new Font(unicodeFontName, fontSize);
                comboBoxRemoveIfTextContains.Font = new Font(unicodeFontName, fontSize);
            }
            comboBoxRemoveIfTextContains.Left = checkBoxRemoveWhereContains.Left + checkBoxRemoveWhereContains.Width;

            _subtitle = subtitle;
            GeneratePreview();
        }

        public void InitializeSettingsOnly()
        {
            comboBoxRemoveIfTextContains.Left = checkBoxRemoveWhereContains.Left + checkBoxRemoveWhereContains.Width;
            groupBoxLinesFound.Visible = false;
            int h = groupBoxRemoveTextConditions.Top + groupBoxRemoveTextConditions.Height + buttonOK.Height + 50;
            MinimumSize = new Size(MinimumSize.Width, h);
            this.Height = h;
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
                return;

            _removeTextForHILib.Settings = GetSettings();
            _removeTextForHILib.Warnings = new List<int>();
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            int count = 0;
            int prevIndex = -1;
            var settings = GetSettings();
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                string prevText = string.Empty;
                var prev = _subtitle.GetParagraphOrDefault(prevIndex);
                if (prev != null)
                    prevText = prev.Text;
                prevIndex++;

                _removeTextForHILib.WarningIndex = prevIndex;
                string newText = _removeTextForHILib.RemoveTextFromHearImpaired(p.Text);
                bool hit = p.Text.Replace(" ", string.Empty) != newText.Replace(" ", string.Empty);
                if (hit)
                {
                    count++;
                    AddToListView(p, newText);
                }
            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(_language.LinesFoundX, count);
        }

        private void AddToListView(Paragraph p, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };
            if (_removeTextForHILib.Warnings != null && _removeTextForHILib.Warnings.Contains(_removeTextForHILib.WarningIndex))
            {
                item.UseItemStyleForSubItems = true;
                item.BackColor = Color.PeachPuff;
            }
            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, p.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, newText.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);

            listViewFixes.Items.Add(item);
        }

        private void FormRemoveTextForHearImpaired_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
            else if (e.KeyCode == Keys.F1)
                Utilities.ShowHelp("#remove_text_for_hi");
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        public int RemoveTextFromHearImpaired()
        {
            int count = 0;
            var settings = GetSettings();
            for (int i = _subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                if (IsFixAllowed(p))
                {
                    string prevText = string.Empty;
                    var prev = _subtitle.GetParagraphOrDefault(i - 1);
                    if (prev != null)
                        prevText = prev.Text;

                    _removeTextForHILib.Settings = GetSettings();
                    string newText = _removeTextForHILib.RemoveTextFromHearImpaired(p.Text);
                    if (string.IsNullOrEmpty(newText))
                    {
                        _subtitle.Paragraphs.RemoveAt(i);
                    }
                    else
                    {
                        p.Text = newText;
                    }
                    count++;
                }
            }
            return count;
        }

        private bool IsFixAllowed(Paragraph p)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.Tag.ToString() == p.ToString())
                    return item.Checked;
            }
            return false;
        }

        private void CheckBoxRemoveTextBetweenCheckedChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void checkBoxRemoveInterjections_CheckedChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void buttonEditInterjections_Click(object sender, EventArgs e)
        {
            using (var editInterjections = new Interjections())
            {
                editInterjections.Initialize(Configuration.Settings.Tools.Interjections);
                if (editInterjections.ShowDialog(this) == DialogResult.OK)
                {
                    Configuration.Settings.Tools.Interjections = editInterjections.GetInterjectionsSemiColonSeperatedString();
                    _removeTextForHILib.ResetInterjections();
                    if (checkBoxRemoveInterjections.Checked)
                    {
                        Cursor = Cursors.WaitCursor;
                        GeneratePreview();
                        Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void FormRemoveTextForHearImpaired_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets = checkBoxRemoveTextBetweenSquares.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses = checkBoxRemoveTextBetweenParentheses.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets = checkBoxRemoveTextBetweenBrackets.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks = checkBoxRemoveTextBetweenQuestionMarks.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom = checkBoxRemoveTextBetweenCustomTags.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore = comboBoxCustomStart.Text;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter = comboBoxCustomEnd.Text;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeperateLines = checkBoxOnlyIfInSeparateLine.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon = checkBoxRemoveTextBeforeColon.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase = checkBoxRemoveTextBeforeColonOnlyUppercase.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine = checkBoxColonSeparateLine.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections = checkBoxRemoveInterjections.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContains = checkBoxRemoveWhereContains.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase = checkBoxRemoveIfAllUppercase.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContainsText = comboBoxRemoveIfTextContains.Text;
        }

        private void FormRemoveTextForHearImpaired_Resize(object sender, EventArgs e)
        {
            int availableWidth = listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width + 20);
            listViewFixes.Columns[2].Width = availableWidth / 2;
            listViewFixes.Columns[3].Width = availableWidth / 2;
        }

        private void checkBoxRemoveTextBeforeColon_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxRemoveTextBeforeColonOnlyUppercase.Enabled = checkBoxRemoveTextBeforeColon.Checked;
            checkBoxColonSeparateLine.Enabled = checkBoxRemoveTextBeforeColon.Checked;
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void checkBoxRemoveIfAllUppercase_CheckedChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        public RemoveTextForHISettings GetSettings()
        {
            var settings = new RemoveTextForHISettings();
            settings.OnlyIfInSeparateLine = checkBoxOnlyIfInSeparateLine.Checked;
            settings.RemoveIfAllUppercase = checkBoxRemoveIfAllUppercase.Checked;
            settings.RemoveTextBeforeColon = checkBoxRemoveTextBeforeColon.Checked;
            settings.RemoveTextBeforeColonOnlyUppercase = checkBoxRemoveTextBeforeColonOnlyUppercase.Checked;
            settings.ColonSeparateLine = checkBoxColonSeparateLine.Checked;
            settings.RemoveWhereContains = checkBoxRemoveWhereContains.Checked;
            settings.RemoveIfTextContains = comboBoxRemoveIfTextContains.Text;
            settings.RemoveTextBetweenCustomTags = checkBoxRemoveTextBetweenCustomTags.Checked;
            settings.RemoveInterjections = checkBoxRemoveInterjections.Checked;
            settings.RemoveTextBetweenSquares = checkBoxRemoveTextBetweenSquares.Checked;
            settings.RemoveTextBetweenBrackets = checkBoxRemoveTextBetweenBrackets.Checked;
            settings.RemoveTextBetweenQuestionMarks = checkBoxRemoveTextBetweenQuestionMarks.Checked;
            settings.RemoveTextBetweenParentheses = checkBoxRemoveTextBetweenParentheses.Checked;
            settings.CustomStart = comboBoxCustomStart.Text;
            settings.CustomEnd = comboBoxCustomEnd.Text;
            return settings;
        }

    }
}
