using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class FormRemoveTextForHearImpaired : PositionAndSizeForm
    {
        private Subtitle _subtitle;
        private readonly LanguageStructure.RemoveTextFromHearImpaired _language;
        private RemoveTextForHI _removeTextForHILib;
        private Dictionary<Paragraph, string> _fixes;

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
            Utilities.FixLargeFonts(this, buttonOK);
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
            Height = h;
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
            _fixes = new Dictionary<Paragraph, string>();
            for (int index = 0; index < _subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = _subtitle.Paragraphs[index];
                _removeTextForHILib.WarningIndex = index - 1;
                string newText = _removeTextForHILib.RemoveTextFromHearImpaired(p.Text);
                if (p.Text.Replace(" ", string.Empty) != newText.Replace(" ", string.Empty))
                {
                    count++;
                    AddToListView(p, newText);
                    _fixes.Add(p, newText);
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
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(p.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(newText.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
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
            for (int i = listViewFixes.Items.Count - 1; i >= 0; i--)
            {
                var item = listViewFixes.Items[i];
                if (item.Checked)
                {
                    var p = (Paragraph)item.Tag;
                    string newText = _fixes[p];
                    if (string.IsNullOrWhiteSpace(newText))
                    {
                        _subtitle.Paragraphs.Remove(p);
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
            int availableWidth = (listViewFixes.Width - (columnHeaderApply.Width + columnHeaderLine.Width + 20)) / 2;

            columnHeaderBefore.Width = availableWidth;
            columnHeaderAfter.Width = availableWidth;
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
            var settings = new RemoveTextForHISettings
            {
                OnlyIfInSeparateLine = checkBoxOnlyIfInSeparateLine.Checked,
                RemoveIfAllUppercase = checkBoxRemoveIfAllUppercase.Checked,
                RemoveTextBeforeColon = checkBoxRemoveTextBeforeColon.Checked,
                RemoveTextBeforeColonOnlyUppercase = checkBoxRemoveTextBeforeColonOnlyUppercase.Checked,
                ColonSeparateLine = checkBoxColonSeparateLine.Checked,
                RemoveWhereContains = checkBoxRemoveWhereContains.Checked,
                RemoveIfTextContains = comboBoxRemoveIfTextContains.Text,
                RemoveTextBetweenCustomTags = checkBoxRemoveTextBetweenCustomTags.Checked,
                RemoveInterjections = checkBoxRemoveInterjections.Checked,
                RemoveTextBetweenSquares = checkBoxRemoveTextBetweenSquares.Checked,
                RemoveTextBetweenBrackets = checkBoxRemoveTextBetweenBrackets.Checked,
                RemoveTextBetweenQuestionMarks = checkBoxRemoveTextBetweenQuestionMarks.Checked,
                RemoveTextBetweenParentheses = checkBoxRemoveTextBetweenParentheses.Checked,
                CustomStart = comboBoxCustomStart.Text,
                CustomEnd = comboBoxCustomEnd.Text
            };
            return settings;
        }

    }
}