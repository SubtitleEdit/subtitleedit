using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class OcrSpellCheck : Form
    {
        public enum Action
        {
            Abort,
            AddToUserDictionary,
            AddToNames,
            AllwaysUseSuggestion,
            ChangeAndSave,
            ChangeOnce,
            ChangeWholeText,
            ChangeAllWholeText,
            SkipAll,
            SkipWholeText,
            SkipOnce,
            UseSuggestion,
        }

        public Action ActionResult { get; private set; }
        public string Word { get; private set; }
        public string Paragraph { get; private set; }
        public string OriginalWholeText { get; private set; }

        private string _originalWord;

        public OcrSpellCheck()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.SpellCheck.Title;
            buttonAddToDictionary.Text = Configuration.Settings.Language.SpellCheck.AddToUserDictionary;
            buttonChange.Text = Configuration.Settings.Language.SpellCheck.Change;
            buttonChangeAll.Text = Configuration.Settings.Language.SpellCheck.ChangeAll;
            buttonSkipAll.Text = Configuration.Settings.Language.SpellCheck.SkipAll;
            buttonSkipOnce.Text = Configuration.Settings.Language.SpellCheck.SkipOnce;
            buttonUseSuggestion.Text = Configuration.Settings.Language.SpellCheck.Use;
            buttonUseSuggestionAlways.Text = Configuration.Settings.Language.SpellCheck.UseAlways;
            buttonAbort.Text = Configuration.Settings.Language.SpellCheck.Abort;
            groupBoxEditWholeText.Text = Configuration.Settings.Language.SpellCheck.EditWholeText;
            buttonChangeWholeText.Text = Configuration.Settings.Language.SpellCheck.Change;
            buttonSkipText.Text = Configuration.Settings.Language.SpellCheck.SkipOnce;
            buttonEditWholeText.Text = Configuration.Settings.Language.SpellCheck.EditWholeText;
            buttonEditWord.Text = Configuration.Settings.Language.SpellCheck.EditWordOnly;
            groupBoxText.Text = Configuration.Settings.Language.General.Text;
            GroupBoxEditWord.Text = Configuration.Settings.Language.SpellCheck.WordNotFound;
            groupBoxSuggestions.Text = Configuration.Settings.Language.SpellCheck.Suggestions;
            groupBoxTextAsImage.Text = Configuration.Settings.Language.SpellCheck.ImageText;
            buttonAddToNames.Text = Configuration.Settings.Language.SpellCheck.AddToNamesAndIgnoreList;
            buttonChangeAllWholeText.Text = Configuration.Settings.Language.SpellCheck.ChangeAll;
            buttonGoogleIt.Text = Configuration.Settings.Language.Main.VideoControls.GoogleIt;
            UiUtil.FixLargeFonts(this, buttonAddToNames);
        }

        private void ButtonAbortClick(object sender, EventArgs e)
        {
            ActionResult = Action.Abort;
            DialogResult = DialogResult.Abort;
        }

        internal void Initialize(string word, List<string> suggestions, string line, Bitmap bitmap)
        {
            _originalWord = word;
            OriginalWholeText = line;
            pictureBoxText.Image = bitmap;
            textBoxWord.Text = word;
            richTextBoxParagraph.Text = line;
            textBoxWholeText.Text = line;
            listBoxSuggestions.Items.Clear();
            foreach (string suggestion in suggestions)
                listBoxSuggestions.Items.Add(suggestion);
            if (listBoxSuggestions.Items.Count > 0)
                listBoxSuggestions.SelectedIndex = 0;

            HighLightWord(richTextBoxParagraph, word);
            ButtonEditWordClick(null, null);
        }

        private static void HighLightWord(RichTextBox richTextBoxParagraph, string word)
        {
            if (word != null && richTextBoxParagraph.Text.Contains(word))
            {
                const string expectedWordBoundaryChars = " <>-\"”„“«»[]'‘`´¶()♪¿¡.…—!?,:;/\r\n";
                for (int i = 0; i < richTextBoxParagraph.Text.Length; i++)
                {
                    if (richTextBoxParagraph.Text.Substring(i).StartsWith(word))
                    {
                        bool startOk = i == 0;
                        if (!startOk)
                            startOk = expectedWordBoundaryChars.Contains(richTextBoxParagraph.Text[i - 1]);
                        if (startOk)
                        {
                            bool endOk = (i + word.Length == richTextBoxParagraph.Text.Length);
                            if (!endOk)
                                endOk = expectedWordBoundaryChars.Contains(richTextBoxParagraph.Text[i + word.Length]);
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
            groupBoxEditWholeText.BringToFront();
            groupBoxEditWholeText.Enabled = true;
            GroupBoxEditWord.SendToBack();
            GroupBoxEditWord.Enabled = false;
            buttonEditWord.Enabled = true;
            buttonEditWholeText.Enabled = false;
            textBoxWholeText.Focus();
        }

        private void ButtonEditWordClick(object sender, EventArgs e)
        {
            groupBoxEditWholeText.SendToBack();
            groupBoxEditWholeText.Enabled = false;
            GroupBoxEditWord.BringToFront();
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
                ActionResult = Action.AllwaysUseSuggestion;
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
                // TODO: Localize!
                MessageBox.Show("Word should be one single word");
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
                System.Diagnostics.Process.Start("https://www.google.com/search?q=" + Utilities.UrlEncode(text));
        }

        private void OcrSpellCheck_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.G)
            {
                e.SuppressKeyPress = true;
                buttonGoogleIt_Click(null, null);
            }
        }

    }
}
