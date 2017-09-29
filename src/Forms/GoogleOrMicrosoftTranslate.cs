using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

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
                    comboBox.Items.RemoveAt(i);
            }
        }

        internal void InitializeFromLanguage(string defaultFromLanguage, string defaultToLanguage)
        {
            if (defaultFromLanguage == defaultToLanguage)
                defaultToLanguage = "en";

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
                string from = (comboBoxFrom.SelectedItem as GoogleTranslate.ComboBoxItem).Value;
                string to = (comboBoxTo.SelectedItem as GoogleTranslate.ComboBoxItem).Value;
                string languagePair = from + "|" + to;

                buttonGoogle.Text = string.Empty;

                // google translate
                bool romanji = languagePair.EndsWith("|romanji", StringComparison.InvariantCulture);
                if (romanji)
                    languagePair = from + "|ja";
                var screenScrapingEncoding = GoogleTranslate.GetScreenScrapingEncoding(languagePair);
                buttonGoogle.Text = GoogleTranslate.TranslateTextViaScreenScraping(textBoxSourceText.Text, languagePair, screenScrapingEncoding, romanji);

                using (var gt = new GoogleTranslate())
                {
                    Subtitle subtitle = new Subtitle();
                    subtitle.Paragraphs.Add(new Paragraph(0, 0, textBoxSourceText.Text));
                    gt.Initialize(subtitle, string.Empty, false, System.Text.Encoding.UTF8);
                    from = FixMsLocale(from);
                    to = FixMsLocale(to);
                    gt.DoMicrosoftTranslate(from, to);
                    buttonMicrosoft.Text = gt.TranslatedSubtitle.Paragraphs[0].Text;
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
                return from;
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
                DialogResult = DialogResult.Cancel;
        }

    }
}
