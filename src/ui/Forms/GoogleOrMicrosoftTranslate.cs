using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GoogleOrMicrosoftTranslate : Form
    {
        public string TranslatedText { get; set; }

        private readonly GoogleTranslationService _googleTranslationService=new GoogleTranslationService();
        private readonly MicrosoftTranslationService _microsoftTranslationService=new MicrosoftTranslationService(Configuration.Settings.Tools.MicrosoftTranslatorApiKey, Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint, Configuration.Settings.Tools.MicrosoftTranslatorCategory);

        public GoogleOrMicrosoftTranslate()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _googleTranslationService.Init();
            _microsoftTranslationService.Init();

            InitLanguageComboboxes();

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

        private void InitLanguageComboboxes()
        {
            var googleSourceLanguages = _googleTranslationService.GetSupportedSourceLanguages();
            var microsoftSourceLanguages = _microsoftTranslationService.GetSupportedSourceLanguages();
            IEnumerable<TranslationPair> intersectFromLanguages = googleSourceLanguages.Intersect(microsoftSourceLanguages);
            GenericTranslate.FillComboWithLanguages(comboBoxFrom, intersectFromLanguages);

            var googleTargetLanguages = _googleTranslationService.GetSupportedSourceLanguages();
            var microsoftTargetLanguages = _microsoftTranslationService.GetSupportedSourceLanguages();
            IEnumerable<TranslationPair> intersectTargetLanguages = googleTargetLanguages.Intersect(microsoftTargetLanguages);
            GenericTranslate.FillComboWithLanguages(comboBoxTo, intersectTargetLanguages);
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
            GenericTranslate.SelectLanguageCode(comboBoxFrom, defaultFromLanguage);
            GenericTranslate.SelectLanguageCode(comboBoxTo, defaultToLanguage);
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
                string from = ((TranslationPair)comboBoxFrom.SelectedItem).Code;
                string to = ((TranslationPair)comboBoxTo.SelectedItem).Code;
                buttonGoogle.Text = string.Empty;

                // google translate
                buttonGoogle.Text = _googleTranslationService.Translate(from, to, new List<Paragraph> { new Paragraph { Text = textBoxSourceText.Text } }).FirstOrDefault();

                // ms translator
                if (!string.IsNullOrEmpty(Configuration.Settings.Tools.MicrosoftTranslatorApiKey) && !string.IsNullOrEmpty(Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint))
                {
                    var result = _microsoftTranslationService.Translate(from, to, new List<Paragraph> { new Paragraph { Text = textBoxSourceText.Text } });
                    buttonMicrosoft.Text = result[0];
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
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
