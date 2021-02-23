using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Core.Translate.Service;
using Nikse.SubtitleEdit.Forms.Translate;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GoogleOrMicrosoftTranslate : Form
    {
        public string TranslatedText { get; set; }

        private GoogleTranslationService _googleTranslationService;
        private MicrosoftTranslationService _microsoftTranslationService;
        private string _toLanguage;
        private string _fromLanguage;

        public GoogleOrMicrosoftTranslate()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.GoogleOrMicrosoftTranslate.Title;
            labelGoogleTranslate.Text = LanguageSettings.Current.GoogleOrMicrosoftTranslate.GoogleTranslate;
            labelMicrosoftTranslate.Text = LanguageSettings.Current.GoogleOrMicrosoftTranslate.MicrosoftTranslate;
            labelFrom.Text = LanguageSettings.Current.GoogleOrMicrosoftTranslate.From;
            labelTo.Text = LanguageSettings.Current.GoogleOrMicrosoftTranslate.To;
            labelSourceText.Text = LanguageSettings.Current.GoogleOrMicrosoftTranslate.SourceText;
            buttonGoogle.Text = LanguageSettings.Current.GoogleOrMicrosoftTranslate.GoogleTranslate;
            buttonMicrosoft.Text = LanguageSettings.Current.GoogleOrMicrosoftTranslate.MicrosoftTranslate;
            buttonTranslate.Text = LanguageSettings.Current.GoogleOrMicrosoftTranslate.Translate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonCancel);
            buttonGoogle.Text = string.Empty;
            buttonMicrosoft.Text = string.Empty;
        }

        private void InitLanguageComboboxes()
        {
            var googleSourceLanguages = new List<TranslationPair>();
            var googleTargetLanguages = new List<TranslationPair>();

            var microsoftSourceLanguages = new List<TranslationPair>();
            var microsoftTargetLanguages = new List<TranslationPair>();


            if (_googleTranslationService != null)
            {
                googleSourceLanguages = _googleTranslationService.GetSupportedSourceLanguages();
                googleTargetLanguages = _googleTranslationService.GetSupportedSourceLanguages();
            }

            if (_microsoftTranslationService != null)
            {
                microsoftSourceLanguages = _microsoftTranslationService.GetSupportedSourceLanguages();
                microsoftTargetLanguages = _microsoftTranslationService.GetSupportedSourceLanguages();
            }

            IEnumerable<TranslationPair> targetLanguages;
            IEnumerable<TranslationPair> fromLanguages;
            if (_googleTranslationService == null || _microsoftTranslationService == null)
            {
                targetLanguages = googleTargetLanguages.Union(microsoftTargetLanguages);
                fromLanguages = googleSourceLanguages.Union(microsoftSourceLanguages);
            }
            else
            {
                targetLanguages = googleTargetLanguages.Intersect(microsoftTargetLanguages);
                fromLanguages = googleSourceLanguages.Intersect(microsoftSourceLanguages);
            }

            GenericTranslate.FillComboWithLanguages(comboBoxFrom, fromLanguages);
            GenericTranslate.FillComboWithLanguages(comboBoxTo, targetLanguages);

            GenericTranslate.SelectLanguageCode(comboBoxFrom, _fromLanguage);
            GenericTranslate.SelectLanguageCode(comboBoxTo, _toLanguage);
        }


        internal void InitializeFromLanguage(string defaultFromLanguage)
        {
            _toLanguage = Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage;
            _fromLanguage = defaultFromLanguage;
            if (_toLanguage == defaultFromLanguage)
            {
                foreach (string s in Utilities.GetDictionaryLanguages())
                {
                    string temp = s.Replace("[", string.Empty).Replace("]", string.Empty);
                    if (temp.Length > 4)
                    {
                        temp = temp.Substring(temp.Length - 5, 2).ToLowerInvariant();

                        if (temp != _toLanguage)
                        {
                            _toLanguage = temp;
                            break;
                        }
                    }
                }
            }
        }

        internal void Initialize(Paragraph paragraph)
        {
            textBoxSourceText.Text = paragraph.Text;
        }

        private void GoogleOrMicrosoftTranslate_Shown(object sender, EventArgs e)
        {
            _googleTranslationService = GoogleTranslationInitializer.Init();
            _microsoftTranslationService = MicrosoftTranslationInitializer.Init(true);

            InitLanguageComboboxes();

            Refresh();
            Translate();
        }

        private void Translate()
        {
            Cursor = Cursors.WaitCursor;
            try
            {

                buttonGoogle.Text = string.Empty;
                buttonMicrosoft.Text = string.Empty;

                if (_googleTranslationService != null || _microsoftTranslationService != null)
                {
                    _fromLanguage = ((TranslationPair)comboBoxFrom.SelectedItem).Code;
                    _toLanguage = ((TranslationPair)comboBoxTo.SelectedItem).Code;

                    if (_googleTranslationService != null)
                    {
                        buttonGoogle.Text = _googleTranslationService.Translate(_fromLanguage, _toLanguage, new List<Paragraph> { new Paragraph { Text = textBoxSourceText.Text } }).FirstOrDefault();
                    }
                    if (_microsoftTranslationService != null)
                    {
                        var result = _microsoftTranslationService.Translate(_fromLanguage, _toLanguage, new List<Paragraph> { new Paragraph { Text = textBoxSourceText.Text } });
                        buttonMicrosoft.Text = result[0];
                    }
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
