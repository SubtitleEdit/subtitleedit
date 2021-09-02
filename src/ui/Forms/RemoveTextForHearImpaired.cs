using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class FormRemoveTextForHearImpaired : PositionAndSizeForm
    {
        public Subtitle Subtitle;
        public int TotalFixes { get; private set; }
        private readonly LanguageStructure.RemoveTextFromHearImpaired _language;
        private readonly RemoveTextForHI _removeTextForHiLib;
        private Dictionary<Paragraph, string> _fixes;
        private readonly Main _mainForm;
        private readonly List<Paragraph> _unchecked = new List<Paragraph>();
        private readonly List<Paragraph> _edited = new List<Paragraph>();
        private readonly List<Paragraph> _editedOld = new List<Paragraph>();
        private static readonly Color _listBackMarkColor = Configuration.Settings.General.UseDarkTheme? Color.PaleVioletRed : Color.PeachPuff;

        public FormRemoveTextForHearImpaired(Main main, Subtitle subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _mainForm = main;
            _removeTextForHiLib = new RemoveTextForHI(GetSettings(subtitle));

            checkBoxRemoveTextBetweenSquares.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets;
            checkBoxRemoveTextBetweenParentheses.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses;
            checkBoxRemoveTextBetweenBrackets.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets;
            checkBoxRemoveTextBetweenQuestionMarks.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks;
            checkBoxRemoveTextBetweenCustomTags.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom;
            checkBoxOnlyIfInSeparateLine.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeparateLines;
            checkBoxRemoveTextBeforeColon.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon;
            checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase;
            checkBoxColonSeparateLine.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine;
            checkBoxRemoveInterjections.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections;
            checkBoxInterjectionOnlySeparateLine.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjectionsOnlyOnSeparateLine;
            checkBoxRemoveWhereContains.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContains;
            checkBoxRemoveIfAllUppercase.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase;
            checkBoxInterjectionOnlySeparateLine.Enabled = checkBoxRemoveInterjections.Checked;

            contextMenuStrip1.Items[0].Text = LanguageSettings.Current.Main.Menu.ContextMenu.SelectAll;
            contextMenuStrip1.Items[1].Text = LanguageSettings.Current.Main.Menu.Edit.InverseSelection;

            _language = LanguageSettings.Current.RemoveTextFromHearImpaired;
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
            checkBoxInterjectionOnlySeparateLine.Text = _language.OnlyIfInSeparateLine;
            buttonEditInterjections.Text = _language.EditInterjections;
            buttonEditInterjections.Left = checkBoxRemoveInterjections.Left + checkBoxRemoveInterjections.Width;
            listViewFixes.Columns[0].Text = LanguageSettings.Current.General.Apply;
            listViewFixes.Columns[1].Text = LanguageSettings.Current.General.LineNumber;
            listViewFixes.Columns[2].Text = LanguageSettings.Current.General.Before;
            listViewFixes.Columns[3].Text = LanguageSettings.Current.General.After;
            labelText.Text = LanguageSettings.Current.General.Text;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonApply.Text = _language.Apply;
            UiUtil.FixLargeFonts(this, buttonOK);
            listViewFixes_SelectedIndexChanged(null, null);
        }

        public void Initialize(Subtitle subtitle)
        {
            comboBoxRemoveIfTextContains.Left = checkBoxRemoveWhereContains.Left + checkBoxRemoveWhereContains.Width;
            Subtitle = new Subtitle(subtitle);
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
            if (Subtitle == null)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            _removeTextForHiLib.Settings = GetSettings(Subtitle);
            _removeTextForHiLib.Warnings = new List<int>();
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            int count = 0;
            _fixes = new Dictionary<Paragraph, string>();
            for (int index = 0; index < Subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = Subtitle.Paragraphs[index];
                _removeTextForHiLib.WarningIndex = index - 1;
                if (_edited.Contains(p))
                {
                    count++;
                    var old = _editedOld.First(x => x.Id == p.Id);
                    AddToListView(old, p.Text);
                    _fixes.Add(old, p.Text);
                }
                else
                {
                    string newText = _removeTextForHiLib.RemoveTextFromHearImpaired(p.Text, Subtitle, index);
                    if (p.Text.RemoveChar(' ') != newText.RemoveChar(' '))
                    {
                        count++;
                        AddToListView(p, newText);
                        _fixes.Add(p, newText);
                    }
                }

            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(_language.LinesFoundX, count);
            Cursor.Current = Cursors.Default;
        }

        private void AddToListView(Paragraph p, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = !_unchecked.Contains(p) };
            if (_removeTextForHiLib.Warnings != null && _removeTextForHiLib.Warnings.Contains(_removeTextForHiLib.WarningIndex))
            {
                item.UseItemStyleForSubItems = true;
                item.BackColor = _listBackMarkColor;
            }
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(UiUtil.GetListViewTextFromString(p.Text));
            item.SubItems.Add(UiUtil.GetListViewTextFromString(newText));
            listViewFixes.Items.Add(item);
        }

        private void FormRemoveTextForHearImpaired_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#remove_text_for_hi");
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets = checkBoxRemoveTextBetweenSquares.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses = checkBoxRemoveTextBetweenParentheses.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets = checkBoxRemoveTextBetweenBrackets.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks = checkBoxRemoveTextBetweenQuestionMarks.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom = checkBoxRemoveTextBetweenCustomTags.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore = comboBoxCustomStart.Text;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter = comboBoxCustomEnd.Text;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeparateLines = checkBoxOnlyIfInSeparateLine.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon = checkBoxRemoveTextBeforeColon.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase = checkBoxRemoveTextBeforeColonOnlyUppercase.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine = checkBoxColonSeparateLine.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections = checkBoxRemoveInterjections.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjectionsOnlyOnSeparateLine = checkBoxInterjectionOnlySeparateLine.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContains = checkBoxRemoveWhereContains.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase = checkBoxRemoveIfAllUppercase.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContainsText = comboBoxRemoveIfTextContains.Text;

            ApplyChanges();
            DialogResult = DialogResult.OK;
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            ApplyChanges();
            GeneratePreview();
        }

        private void ApplyChanges()
        {
            if (Subtitle == null)
            {
                return;
            }

            int fixes = RemoveTextFromHearImpaired();
            Subtitle.Renumber();
            if (_mainForm != null && fixes > 0)
            {
                TotalFixes += fixes;
                _mainForm.ReloadFromSubtitle(new Subtitle(Subtitle), LanguageSettings.Current.Main.BeforeRemovalOfTextingForHearingImpaired);
            }
        }

        public int RemoveTextFromHearImpaired()
        {
            _unchecked.Clear();
            int fixes = 0;

            for (int i = listViewFixes.Items.Count - 1; i >= 0; i--)
            {
                var item = listViewFixes.Items[i];
                var p = (Paragraph)item.Tag;
                if (item.Checked)
                {
                    string newText = _fixes[p];
                    if (string.IsNullOrWhiteSpace(newText))
                    {
                        Subtitle.Paragraphs.Remove(p);
                    }
                    else
                    {
                        p.Text = newText;
                    }
                    fixes++;
                }
                else
                {
                    _unchecked.Add(p);
                }
            }
            return fixes;
        }

        private void CheckBoxRemoveTextBetweenCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void checkBoxRemoveInterjections_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxInterjectionOnlySeparateLine.Enabled = checkBoxRemoveInterjections.Checked;
            GeneratePreview();
        }

        private void buttonEditInterjections_Click(object sender, EventArgs e)
        {
            using (var editInterjections = new Interjections())
            {
                editInterjections.Initialize(Configuration.Settings.Tools.Interjections);
                if (editInterjections.ShowDialog(this) == DialogResult.OK)
                {
                    Configuration.Settings.Tools.Interjections = editInterjections.GetInterjectionsSemiColonSeparatedString();
                    _removeTextForHiLib.ReloadInterjection();
                    if (checkBoxRemoveInterjections.Checked)
                    {
                        GeneratePreview();
                    }
                }
            }
        }

        private void FormRemoveTextForHearImpaired_Resize(object sender, EventArgs e)
        {
            int availableWidth = (listViewFixes.Width - (columnHeaderApply.Width + columnHeaderLine.Width + 20)) / 2;
            columnHeaderBefore.Width = availableWidth;
            columnHeaderAfter.Width = -2;
        }

        private void FormRemoveTextForHearImpaired_Shown(object sender, EventArgs e)
        {
            FormRemoveTextForHearImpaired_Resize(sender, e);
        }

        private void checkBoxRemoveTextBeforeColon_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxRemoveTextBeforeColonOnlyUppercase.Enabled = checkBoxRemoveTextBeforeColon.Checked;
            checkBoxColonSeparateLine.Enabled = checkBoxRemoveTextBeforeColon.Checked;
            GeneratePreview();
        }

        private void checkBoxRemoveIfAllUppercase_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        public RemoveTextForHISettings GetSettings(Subtitle subtitle)
        {
            var settings = new RemoveTextForHISettings(subtitle)
            {
                OnlyIfInSeparateLine = checkBoxOnlyIfInSeparateLine.Checked,
                RemoveIfAllUppercase = checkBoxRemoveIfAllUppercase.Checked,
                RemoveTextBeforeColon = checkBoxRemoveTextBeforeColon.Checked,
                RemoveTextBeforeColonOnlyUppercase = checkBoxRemoveTextBeforeColonOnlyUppercase.Checked,
                ColonSeparateLine = checkBoxColonSeparateLine.Checked,
                RemoveWhereContains = checkBoxRemoveWhereContains.Checked,
                RemoveIfTextContains = new List<string>(),
                RemoveTextBetweenCustomTags = checkBoxRemoveTextBetweenCustomTags.Checked,
                RemoveInterjections = checkBoxRemoveInterjections.Checked,
                RemoveInterjectionsOnlySeparateLine = checkBoxRemoveInterjections.Checked && checkBoxInterjectionOnlySeparateLine.Checked,
                RemoveTextBetweenSquares = checkBoxRemoveTextBetweenSquares.Checked,
                RemoveTextBetweenBrackets = checkBoxRemoveTextBetweenBrackets.Checked,
                RemoveTextBetweenQuestionMarks = checkBoxRemoveTextBetweenQuestionMarks.Checked,
                RemoveTextBetweenParentheses = checkBoxRemoveTextBetweenParentheses.Checked,
                CustomStart = comboBoxCustomStart.Text,
                CustomEnd = comboBoxCustomEnd.Text
            };
            foreach (string item in comboBoxRemoveIfTextContains.Text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                settings.RemoveIfTextContains.Add(item.Trim());
            }
            return settings;
        }

        private void FormRemoveTextForHearImpaired_Load(object sender, EventArgs e)
        {
            // only works when used from "Form Load"
            comboBoxCustomStart.Text = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore;
            comboBoxCustomEnd.Text = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter;
            comboBoxRemoveIfTextContains.Text = Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContainsText;
        }

        private void toolStripMenuItemSelAll_Click(object sender, EventArgs e)
        {
            DoSelection(true);
        }

        private void toolStripMenuItemInvertSel_Click(object sender, EventArgs e)
        {
            DoSelection(false);
        }

        private void DoSelection(bool selectAll)
        {
            if (listViewFixes.Items.Count == 0)
            {
                return;
            }

            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Checked = selectAll || !item.Checked;
            }
        }

        private void listViewFixes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!(e.Item.Tag is Paragraph p))
            {
                return;
            }

            if (e.Item.Checked)
            {
                if (_unchecked.Contains(p))
                {
                    _unchecked.Add(p);
                }
            }
            else
            {
                if (!_unchecked.Contains(p))
                {
                    _unchecked.Remove(p);
                }
            }
        }

        private void checkBoxInterjectionOnlySeparateLine_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void listViewFixes_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idx = listViewFixes.SelectedIndices.Count == 1 ? listViewFixes.SelectedIndices[0] : -1;
            if (idx < 0)
            {
                textBoxAfterText.Visible = false;
                labelText.Visible = false;
                return;
            }

            textBoxAfterText.TextChanged -= textBoxAfterText_TextChanged;
            textBoxAfterText.Visible = true;
            labelText.Visible = true;
            var item = listViewFixes.Items[idx];
            var p = (Paragraph)item.Tag;
            textBoxAfterText.Tag = item;
            var text = _fixes[p];
            textBoxAfterText.Text = text;
            textBoxAfterText.TextChanged += textBoxAfterText_TextChanged;
        }

        private void textBoxAfterText_TextChanged(object sender, EventArgs e)
        {
            var text = textBoxAfterText.Text.Trim();
            var item = (ListViewItem)textBoxAfterText.Tag;
            var p = (Paragraph)item.Tag;
            _editedOld.Add(new Paragraph(p, false) { Text = p.Text });
            item.SubItems[3].Text = text;
            _fixes[p] = text;
            var o = Subtitle.GetParagraphOrDefaultById(p.Id);
            o.Text = text;
            _edited.Add(p);
        }
    }
}
