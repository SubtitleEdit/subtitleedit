using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public partial class TranslateViaCopyPaste : Form
    {
        private readonly Subtitle _subtitle;
        private readonly Subtitle _subtitleOriginal;
        private bool _abort;
        public Subtitle TranslatedSubtitle { get; private set; }

        public TranslateViaCopyPaste(Subtitle sub)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            if (Configuration.Settings.General.UseDarkTheme)
            {
                listViewTranslate.GridLines = Configuration.Settings.General.DarkThemeShowListViewGridLines;
            }

            Text = LanguageSettings.Current.GoogleTranslate.AutoTranslateViaCopyPaste;
            labelMaxTextSize.Text = LanguageSettings.Current.GoogleTranslate.CopyPasteMaxSize;
            checkBoxAutoCopyToClipboard.Text = LanguageSettings.Current.GoogleTranslate.AutoCopyToClipboard;
            labelLineSeparator.Text = LanguageSettings.Current.GoogleTranslate.AutoCopyLineSeparator;

            _subtitle = new Subtitle(sub);
            _subtitleOriginal = new Subtitle(sub);
            foreach (var p in _subtitle.Paragraphs)
            {
                p.Text = string.Empty;
            }

            GeneratePreview();
            RestoreSettings();

            if (listViewTranslate.Items.Count > 0)
            {
                listViewTranslate.Items[0].Selected = true;
                listViewTranslate.Items[0].Focused = true;
            }

            listViewTranslate.Columns[0].Text = LanguageSettings.Current.General.LineNumber;
            listViewTranslate.Columns[1].Text = LanguageSettings.Current.General.OriginalText;
            listViewTranslate.Columns[2].Text = LanguageSettings.Current.General.Text;

            buttonTranslate.Text = LanguageSettings.Current.GoogleTranslate.Translate;
            buttonOk.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            numericUpDownMaxBytes.Left = labelMaxTextSize.Left + labelMaxTextSize.Width + 5;
            checkBoxAutoCopyToClipboard.Left = numericUpDownMaxBytes.Left + numericUpDownMaxBytes.Width + 25;
            labelLineSeparator.Left = checkBoxAutoCopyToClipboard.Left + checkBoxAutoCopyToClipboard.Width + 20;
            textBoxLineSeparator.Left = labelLineSeparator.Left + labelLineSeparator.Width + 5;
            buttonTranslate.Left = textBoxLineSeparator.Left + textBoxLineSeparator.Width + 20;
            progressBarTranslate.Left = buttonTranslate.Left + buttonTranslate.Width + 20;
            progressBarTranslate.Width = Width - progressBarTranslate.Left - 28;

            UiUtil.InitializeSubtitleFont(listViewTranslate);
            UiUtil.FixLargeFonts(this, buttonCancel);
        }

        private void Translate(string source, string target, CopyPasteTranslator translator, int maxTextSize)
        {
            buttonOk.Enabled = false;
            buttonCancel.Enabled = false;
            _abort = false;
            progressBarTranslate.Maximum = _subtitleOriginal.Paragraphs.Count;
            progressBarTranslate.Value = 0;
            progressBarTranslate.Visible = true;

            try
            {
                var log = new StringBuilder();
                var selectedItems = listViewTranslate.SelectedItems;
                var startIndex = selectedItems.Count <= 0 ? 0 : selectedItems[0].Index;
                var start = startIndex;
                int index = startIndex;
                var blocks = translator.BuildBlocks(maxTextSize, source, startIndex);
                for (int i = 0; i < blocks.Count; i++)
                {
                    var block = blocks[i];
                    using (var form = new TranslateBlock(block, string.Format(LanguageSettings.Current.GoogleTranslate.TranslateBlockXOfY, i + 1, blocks.Count), checkBoxAutoCopyToClipboard.Checked))
                    {
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            var result = translator.GetTranslationResult(target, form.TargetText, block);
                            FillTranslatedText(result, start, index);
                            progressBarTranslate.Refresh();
                            Application.DoEvents();
                            index += block.Paragraphs.Count;
                            start = index;
                            progressBarTranslate.Value = Math.Min(index, progressBarTranslate.Maximum);
                        }
                        else
                        {
                            _abort = true;
                        }
                    }

                    if (_abort)
                    {
                        break;
                    }
                }
            }
            finally
            {
                progressBarTranslate.Visible = false;
                buttonTranslate.Enabled = true;
                buttonOk.Enabled = true;
                buttonCancel.Enabled = true;
            }
        }

        private void FillTranslatedText(List<string> translatedLines, int start, int end)
        {
            int index = start;
            listViewTranslate.BeginUpdate();
            foreach (ListViewItem item in listViewTranslate.SelectedItems)
            {
                item.Selected = false;
            }
            foreach (string s in translatedLines)
            {
                if (index < listViewTranslate.Items.Count)
                {
                    var item = listViewTranslate.Items[index];
                    _subtitle.Paragraphs[index].Text = s;
                    item.SubItems[2].Text = s.Replace(Environment.NewLine, "<br />");
                    if (listViewTranslate.CanFocus)
                        listViewTranslate.EnsureVisible(index);
                }
                index++;
            }

            if (index > 0 && index < listViewTranslate.Items.Count + 1)
            {
                listViewTranslate.EnsureVisible(index - 1);
                listViewTranslate.Items[index - 1].Selected = true;
            }

            listViewTranslate.EndUpdate();
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
            {
                return;
            }

            try
            {
                _abort = false;
                int start = 0;
                if (listViewTranslate.SelectedItems.Count > 0)
                {
                    start = listViewTranslate.SelectedItems[0].Index;
                }
                for (int index = start; index < _subtitle.Paragraphs.Count; index++)
                {
                    if (index < listViewTranslate.Items.Count)
                    {
                        var listViewItem = listViewTranslate.Items[index];
                        if (!string.IsNullOrWhiteSpace(listViewItem.SubItems[2].Text))
                        {
                            if (progressBarTranslate.Value < progressBarTranslate.Maximum)
                                progressBarTranslate.Value++;
                            continue;
                        }
                    }

                    Paragraph p = _subtitleOriginal.Paragraphs[index];
                    string text = p.Text;
                    var before = text;
                    var after = string.Empty;
                    AddToListView(p, before, after);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void AddToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem(p.Number.ToString(CultureInfo.InvariantCulture)) { Tag = p };
            item.SubItems.Add(p.Text.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(after.Replace(Environment.NewLine, "<br />"));
            listViewTranslate.Items.Add(item);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            SaveSettings();
            TranslatedSubtitle = new Subtitle(_subtitle);
            DialogResult = DialogResult.OK;
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            _abort = false;
            buttonTranslate.Enabled = false;
            progressBarTranslate.Maximum = _subtitle.Paragraphs.Count;
            progressBarTranslate.Value = 0;
            progressBarTranslate.Visible = true;

            try
            {
                var translator = new CopyPasteTranslator(_subtitleOriginal.Paragraphs, textBoxLineSeparator.Text);
                Translate(string.Empty, string.Empty, translator, (int)numericUpDownMaxBytes.Value);
            }
            finally
            {
                buttonTranslate.Enabled = true;
                progressBarTranslate.Visible = false;
            }
        }

        private void buttonCancelTranslate_Click(object sender, EventArgs e)
        {
            _abort = true;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.F2)
            {
                if (textBoxLog.Visible)
                {
                    textBoxLog.Visible = false;
                }
                else
                {
                    textBoxLog.Visible = true;
                    textBoxLog.BringToFront();
                }
            }
        }

        private void RestoreSettings()
        {
            textBoxLineSeparator.Text = Configuration.Settings.Tools.TranslateViaCopyPasteSeparator;
            numericUpDownMaxBytes.Value = Configuration.Settings.Tools.TranslateViaCopyPasteMaxSize;
            checkBoxAutoCopyToClipboard.Checked = Configuration.Settings.Tools.TranslateViaCopyPasteAutoCopyToClipboard;
        }

        private void SaveSettings()
        {
            Configuration.Settings.Tools.TranslateViaCopyPasteSeparator = textBoxLineSeparator.Text;
            Configuration.Settings.Tools.TranslateViaCopyPasteMaxSize = (int)numericUpDownMaxBytes.Value;
            Configuration.Settings.Tools.TranslateViaCopyPasteAutoCopyToClipboard = checkBoxAutoCopyToClipboard.Checked;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            var subtract = listViewTranslate.Columns[0].Width + 20;
            var width = listViewTranslate.Width / 2 - subtract;
            listViewTranslate.Columns[1].Width = width;
            listViewTranslate.AutoSizeLastColumn();
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            var subtract = listViewTranslate.Columns[0].Width + 20;
            var width = listViewTranslate.Width / 2 - subtract;
            listViewTranslate.Columns[1].Width = width;
            listViewTranslate.AutoSizeLastColumn();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            MainForm_ResizeEnd(sender, e);
        }
    }
}
