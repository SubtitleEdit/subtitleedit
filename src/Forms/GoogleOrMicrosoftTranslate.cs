using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Translate;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GoogleOrMicrosoftTranslate : Form
    {
        public string TranslatedText { get; set; }

        public GoogleOrMicrosoftTranslate()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            using (var gt = new GoogleTranslate())
            {
                gt.FillComboWithGoogleLanguages(comboBoxFrom);
                gt.FillComboWithGoogleLanguages(comboBoxTo);
            }
            RemovedLanguagesNotInMicrosoftTranslate(comboBoxFrom);
            RemovedLanguagesNotInMicrosoftTranslate(comboBoxTo);

            Text = Configuration.Settings.Language.GoogleOrMicrosoftTranslate.Title;
            labelGoogleTranslate.Text = Configuration.Settings.Language.GoogleOrMicrosoftTranslate.GoogleTranslate;
            labelMicrosoftTranslate.Text = Configuration.Settings.Language.GoogleOrMicrosoftTranslate.MicrosoftTranslate;
            labelFrom.Text = Configuration.Settings.Language.GoogleOrMicrosoftTranslate.From;
            labelTo.Text = Configuration.Settings.Language.GoogleOrMicrosoftTranslate.To;
            labelSourceText.Text = Configuration.Settings.Language.GoogleOrMicrosoftTranslate.SourceText;
            buttonGoogle.Text = Configuration.Settings.Language.GoogleOrMicrosoftTranslate.GoogleTranslate;
            buttonMicrosoft.Text = Configuration.Settings.Language.GoogleOrMicrosoftTranslate.MicrosoftTranslate;
            buttonTranslate.Text = Configuration.Settings.Language.GoogleOrMicrosoftTranslate.Translate;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonCancel);
            buttonGoogle.Text = string.Empty;
            buttonMicrosoft.Text = string.Empty;
        }

        private static void RemovedLanguagesNotInMicrosoftTranslate(ComboBox comboBox)
        {
            for (int i = comboBox.Items.Count - 1; i > 0; i--)
            {
                var item = (GoogleTranslate.ComboBoxItem)comboBox.Items[i];
                if (item.Value != FixMsLocale(item.Value))
                {
                    comboBox.Items.RemoveAt(i);
                }
            }
        }

        internal void InitializeFromLanguage(string defaultFromLanguage)
        {
            string defaultToLanguage = Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage;
            if (defaultToLanguage == defaultFromLanguage)
            {
                foreach (string s in Utilities.GetDictionaryLanguages())
                {
                    string temp = s.Replace("[", string.Empty).Replace("]", string.Empty);
                    if (temp.Length > 4)
                    {
                        temp = temp.Substring(temp.Length - 5, 2).ToLowerInvariant();

                        if (temp != defaultToLanguage)
                        {
                            defaultToLanguage = temp;
                            break;
                        }
                    }
                }
            }

            int i = 0;
            comboBoxFrom.SelectedIndex = 0;
            foreach (GoogleTranslate.ComboBoxItem item in comboBoxFrom.Items)
            {
                if (item.Value == defaultFromLanguage)
                {
                    comboBoxFrom.SelectedIndex = i;
                    break;
                }
                i++;
            }

            i = 0;
            comboBoxTo.SelectedIndex = 0;
            foreach (GoogleTranslate.ComboBoxItem item in comboBoxTo.Items)
            {
                if (item.Value == defaultToLanguage)
                {
                    comboBoxTo.SelectedIndex = i;
                    break;
                }
                i++;
            }
        }

        internal void Initialize(Paragraph paragraph)
        {
            textBoxSourceText.Text = paragraph.Text;
        }

        private void GoogleOrMicrosoftTranslate_Shown(object sender, EventArgs e)
        {
            Refresh();
            Translate();
        }

        private void Translate()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                string from = ((GoogleTranslate.ComboBoxItem)comboBoxFrom.SelectedItem).Value;
                string to = ((GoogleTranslate.ComboBoxItem)comboBoxTo.SelectedItem).Value;
                buttonGoogle.Text = string.Empty;

                // google translate
                buttonGoogle.Text = new GoogleTranslator1().Translate(from, to, new List<Paragraph> { new Paragraph { Text = textBoxSourceText.Text } }, new StringBuilder()).FirstOrDefault();

                // ms translator
                if (!string.IsNullOrEmpty(Configuration.Settings.Tools.MicrosoftTranslatorApiKey) && !string.IsNullOrEmpty(Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint))
                {
                    var translator = new MicrosoftTranslator(Configuration.Settings.Tools.MicrosoftTranslatorApiKey, Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint);
                    var result = translator.Translate(from, to, new List<Paragraph> { new Paragraph { Text = textBoxSourceText.Text } }, new StringBuilder());
                    buttonMicrosoft.Text = result[0];
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private static string FixMsLocale(string from)
        {
            if ("ar bg zh-CHS zh-CHT cs da nl en et fi fr de el ht he hu id it ja ko lv lt no pl pt ro ru sk sl es sv th tr uk vi".Contains(from))
            {
                return from;
            }

            return "en";
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            Translate();
        }

        private void buttonGoogle_Click(object sender, EventArgs e)
        {
            TranslatedText = buttonGoogle.Text;
            DialogResult = DialogResult.OK;
        }

        private void buttonMicrosoft_Click(object sender, EventArgs e)
        {
            TranslatedText = buttonMicrosoft.Text;
            DialogResult = DialogResult.OK;
        }

        private void GoogleOrMicrosoftTranslate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

    }
}
