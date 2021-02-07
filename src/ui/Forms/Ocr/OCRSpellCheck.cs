using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class OcrSpellCheck : Form
    {
        public enum Action
        {
            Abort,
            AddToUserDictionary,
            AddToNames,
            AddToNamesOnly,
            AlwaysUseSuggestion,
            ChangeAndSave,
            ChangeOnce,
            ChangeWholeText,
            ChangeAllWholeText,
            SkipAll,
            SkipWholeText,
            SkipOnce,
            UseSuggestion,
            InspectCompareMatches,
        }

        public bool IsBinaryImageCompareOrNOcr
        {
            get => buttonEditImageDb.Visible;
            set => buttonEditImageDb.Visible = value;
        }
        public Action ActionResult { get; private set; }
        public string Word { get; private set; }
        public string Paragraph { get; private set; }
        public string OriginalWholeText { get; private set; }

        private string _originalWord;
        private readonly Form _blinkForm;

        public OcrSpellCheck(Form blinkForm)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _blinkForm = blinkForm;
            Text = LanguageSettings.Current.SpellCheck.Title;
            buttonAddToDictionary.Text = LanguageSettings.Current.SpellCheck.AddToUserDictionary;
            buttonChange.Text = LanguageSettings.Current.SpellCheck.Change;
            buttonChangeAll.Text = LanguageSettings.Current.SpellCheck.ChangeAll;
            buttonSkipAll.Text = LanguageSettings.Current.SpellCheck.SkipAll;
            buttonSkipOnce.Text = LanguageSettings.Current.SpellCheck.SkipOnce;
            buttonUseSuggestion.Text = LanguageSettings.Current.SpellCheck.Use;
            buttonUseSuggestionAlways.Text = LanguageSettings.Current.SpellCheck.UseAlways;
            buttonAbort.Text = LanguageSettings.Current.SpellCheck.Abort;
            groupBoxEditWholeText.Text = LanguageSettings.Current.SpellCheck.EditWholeText;
            buttonChangeWholeText.Text = LanguageSettings.Current.SpellCheck.Change;
            buttonSkipText.Text = LanguageSettings.Current.SpellCheck.SkipOnce;
            buttonEditWholeText.Text = LanguageSettings.Current.SpellCheck.EditWholeText;
            buttonEditWord.Text = LanguageSettings.Current.SpellCheck.EditWordOnly;
            groupBoxText.Text = LanguageSettings.Current.General.Text;
            GroupBoxEditWord.Text = LanguageSettings.Current.SpellCheck.WordNotFound;
            buttonEditImageDb.Text = LanguageSettings.Current.VobSubOcr.EditImageDb;
            groupBoxSuggestions.Text = LanguageSettings.Current.SpellCheck.Suggestions;
            groupBoxTextAsImage.Text = LanguageSettings.Current.SpellCheck.ImageText;
            buttonAddToNames.Text = LanguageSettings.Current.SpellCheck.AddToNamesAndIgnoreList;
            buttonChangeAllWholeText.Text = LanguageSettings.Current.SpellCheck.ChangeAll;
            buttonGoogleIt.Text = LanguageSettings.Current.Main.VideoControls.GoogleIt;
            UiUtil.FixLargeFonts(this, buttonAddToNames);
            richTextBoxParagraph.DetectUrls = false;
        }

        private void ButtonAbortClick(object sender, EventArgs e)
        {
            ActionResult = Action.Abort;
            DialogResult = DialogResult.Abort;
        }

        internal void Initialize(string word, List<string> suggestions, string line, Bitmap bitmap, bool isBinaryImageCompareOrNOcr)
        {
            IsBinaryImageCompareOrNOcr = isBinaryImageCompareOrNOcr;
            _originalWord = word;
            OriginalWholeText = line;
            pictureBoxText.SizeMode = PictureBoxSizeMode.Zoom;
            if (isBinaryImageCompareOrNOcr)
            {
                groupBoxTextAsImage.BackColor = Color.DimGray;
                groupBoxTextAsImage.ForeColor = Color.White;
                pictureBoxText.BackColor = Color.Transparent;
            }
            pictureBoxText.Image = bitmap;
            textBoxWord.Text = word;
            richTextBoxParagraph.Text = line;
            textBoxWholeText.Text = line;
            listBoxSuggestions.Items.Clear();
            foreach (string suggestion in suggestions)
            {
                listBoxSuggestions.Items.Add(suggestion);
            }

            if (listBoxSuggestions.Items.Count > 0)
            {
                listBoxSuggestions.SelectedIndex = 0;
            }

            HighLightWord(richTextBoxParagraph, word);
            ButtonEditWordClick(null, null);
        }

        private static void HighLightWord(RichTextBox richTextBoxParagraph, string word)
        {
            if (word != null && richTextBoxParagraph.Text.Contains(word))
            {
                const string expectedWordBoundaryChars = " <>-\"”„“«»[]'‘`´¶()♪¿¡.…—!?,:;/\r\n؛،؟\u200E\u200F\u202A\u202B\u202C\u202D\u202E\u00A0";
                for (int i = 0; i < richTextBoxParagraph.Text.Length; i++)
                {
                    if (richTextBoxParagraph.Text.Substring(i).StartsWith(word, StringComparison.Ordinal))
                    {
                        bool startOk = i == 0;
                        if (!startOk)
                        {
                            startOk = expectedWordBoundaryChars.Contains(richTextBoxParagraph.Text[i - 1]);
                        }

                        if (startOk)
                        {
                            bool endOk = (i + word.Length == richTextBoxParagraph.Text.Length);
                            if (!endOk)
                            {
                                endOk = expectedWordBoundaryChars.Contains(richTextBoxParagraph.Text[i + word.Length]);
                            }

                            if (endOk)
                            {
                                richTextBoxParagraph.SelectionStart = i + 1;
                                richTextBoxParagraph.SelectionLength = word.Length;
                                while (richTextBoxParagraph.SelectedText != word && richTextBoxParagraph.SelectionStart > 0)
                                {
                                    richTextBoxParagraph.SelectionStart = richTextBoxParagraph.SelectionStart - 1;
                                    richTextBoxParagraph.SelectionLength = word.Length;
                                }
                                if (richTextBoxParagraph.SelectedText == word)
                                {
                                    richTextBoxParagraph.SelectionColor = Color.Red;
                                }
                            }
                        }
                    }
                }

                richTextBoxParagraph.SelectionLength = 0;
                richTextBoxParagraph.SelectionStart = 0;
            }
        }

        private void ButtonEditWholeTextClick(object sender, EventArgs e)
        {
            groupBoxEditWholeText.Visible = true;
            groupBoxEditWholeText.Enabled = true;
            GroupBoxEditWord.Visible = false;
            GroupBoxEditWord.Enabled = false;
            buttonEditWord.Enabled = true;
            buttonEditWholeText.Enabled = false;
            textBoxWholeText.Focus();
        }

        private void ButtonEditWordClick(object sender, EventArgs e)
        {
            groupBoxEditWholeText.Visible = false;
            groupBoxEditWholeText.Enabled = false;
            GroupBoxEditWord.Visible = true;
            GroupBoxEditWord.Enabled = true;
            buttonEditWord.Enabled = false;
            buttonEditWholeText.Enabled = true;
            textBoxWord.Focus();
        }

        private void ButtonChangeAllClick(object sender, EventArgs e)
        {
            if (_originalWord == textBoxWord.Text.Trim())
            {
                MessageBox.Show("Word was not changed!");
                return;
            }

            Word = textBoxWord.Text;
            ActionResult = Action.ChangeAndSave;
            DialogResult = DialogResult.OK;
        }

        private void ButtonChangeClick(object sender, EventArgs e)
        {
            Word = textBoxWord.Text;
            ActionResult = Action.ChangeOnce;
            DialogResult = DialogResult.OK;
        }

        private void ButtonSkipOnceClick(object sender, EventArgs e)
        {
            ActionResult = Action.SkipOnce;
            DialogResult = DialogResult.OK;
        }

        private void ButtonSkipAllClick(object sender, EventArgs e)
        {
            Word = textBoxWord.Text;
            ActionResult = Action.SkipAll;
            DialogResult = DialogResult.OK;
        }

        private void ButtonUseSuggestionClick(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex >= 0)
            {
                Word = listBoxSuggestions.SelectedItem.ToString();
                ActionResult = Action.UseSuggestion;
                DialogResult = DialogResult.OK;
            }
        }

        private void ButtonUseSuggestionAlwaysClick(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex >= 0)
            {
                Word = listBoxSuggestions.Items[listBoxSuggestions.SelectedIndex].ToString();
                ActionResult = Action.AlwaysUseSuggestion;
                DialogResult = DialogResult.OK;
            }
        }

        private void ButtonChangeWholeTextClick(object sender, EventArgs e)
        {
            Paragraph = textBoxWholeText.Text.Trim();
            ActionResult = Action.ChangeWholeText;
            DialogResult = DialogResult.OK;
        }

        private void buttonChangeAllWholeText_Click(object sender, EventArgs e)
        {
            Paragraph = textBoxWholeText.Text.Trim();
            ActionResult = Action.ChangeAllWholeText;
            DialogResult = DialogResult.OK;
        }

        private void ButtonAddToNamesClick(object sender, EventArgs e)
        {
            Word = textBoxWord.Text.Trim();
            ActionResult = Action.AddToNames;
            DialogResult = DialogResult.OK;
        }

        private void ButtonAddToDictionaryClick(object sender, EventArgs e)
        {
            string s = textBoxWord.Text.Trim();
            Word = s;
            if (s.Length == 0 || s.Contains(' '))
            {
                MessageBox.Show(LanguageSettings.Current.SpellCheck.SpacesNotAllowed);
                ActionResult = Action.SkipOnce;
                return;
            }
            ActionResult = Action.AddToUserDictionary;
            DialogResult = DialogResult.OK;
        }

        private void ButtonSkipTextClick(object sender, EventArgs e)
        {
            Word = textBoxWord.Text;
            ActionResult = Action.SkipWholeText;
            DialogResult = DialogResult.OK;
        }

        private void TextBoxWordKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && buttonChange.Enabled)
            {
                ButtonChangeClick(null, null);
                e.SuppressKeyPress = true;
            }
        }

        private void textBoxWord_TextChanged(object sender, EventArgs e)
        {
            buttonChange.Enabled = textBoxWord.Text != _originalWord;
            buttonChangeAll.Enabled = buttonChange.Enabled;
        }

        private void textBoxWholeText_TextChanged(object sender, EventArgs e)
        {
            buttonChangeWholeText.Enabled = textBoxWholeText.Text != OriginalWholeText;
            buttonChangeAllWholeText.Enabled = buttonChangeWholeText.Enabled;
        }

        private void buttonGoogleIt_Click(object sender, EventArgs e)
        {
            string text = textBoxWord.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                UiUtil.OpenUrl("https://www.google.com/search?q=" + Utilities.UrlEncode(text));
            }
        }

        private void OcrSpellCheck_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.G)
            {
                e.SuppressKeyPress = true;
                buttonGoogleIt_Click(null, null);
            }
        }

        private void buttonEditImageDb_Click(object sender, EventArgs e)
        {
            ActionResult = Action.InspectCompareMatches;
            DialogResult = DialogResult.OK;
        }

        private void addXToNamesNoiseListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(richTextBoxParagraph.SelectedText))
            {
                Word = richTextBoxParagraph.SelectedText.Trim();
                ActionResult = Action.AddToNamesOnly;
                DialogResult = DialogResult.OK;
            }
        }

        private void addXToUserDictionaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(richTextBoxParagraph.SelectedText))
            {
                string s = richTextBoxParagraph.SelectedText.Trim();
                Word = s;
                if (s.Length == 0 || s.Contains(' '))
                {
                    MessageBox.Show(LanguageSettings.Current.SpellCheck.SpacesNotAllowed);
                    ActionResult = Action.SkipOnce;
                    return;
                }
                ActionResult = Action.AddToUserDictionary;
                DialogResult = DialogResult.OK;
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool showAddItems = false;
            if (!string.IsNullOrWhiteSpace(richTextBoxParagraph.SelectedText))
            {
                string word = richTextBoxParagraph.SelectedText.Trim();
                addXToNamesnoiseListToolStripMenuItem.Text = string.Format(LanguageSettings.Current.SpellCheck.AddXToNames, word);
                addXToUserDictionaryToolStripMenuItem.Text = string.Format(LanguageSettings.Current.SpellCheck.AddXToUserDictionary, word);
                showAddItems = true;
            }
            addXToNamesnoiseListToolStripMenuItem.Visible = showAddItems;
            addXToUserDictionaryToolStripMenuItem.Visible = showAddItems;
        }

        private void OcrSpellCheck_Shown(object sender, EventArgs e)
        {
            HighLightWord(richTextBoxParagraph, textBoxWord.Text);
            ButtonEditWordClick(null, null);
            textBoxWord.DeselectAll();

            if (ActiveForm == null)
            {
                TaskbarList.StartBlink(
                    _blinkForm, 
                    Configuration.Settings.VobSubOcr.UnfocusedAttentionBlinkCount,
                    Configuration.Settings.VobSubOcr.UnfocusedAttentionPlaySoundCount,
                    2);
            }
        }

        private void OcrSpellCheck_FormClosing(object sender, FormClosingEventArgs e)
        {
            TaskbarList.StopBlink(_blinkForm);
        }
    }
}
