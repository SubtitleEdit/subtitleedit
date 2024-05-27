using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Forms.Options;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public sealed partial class GoogleOrMicrosoftTranslate : Form
    {
        public string TranslatedText { get; set; }

        private IAutoTranslator _googleTranslationService;
        private IAutoTranslator _microsoftTranslationService;
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

        private void InitLanguageComboBoxes()
        {
            var googleSourceLanguages = _googleTranslationService.GetSupportedSourceLanguages();
            var googleTargetLanguages = _googleTranslationService.GetSupportedTargetLanguages();

            var microsoftSourceLanguages = _microsoftTranslationService.GetSupportedSourceLanguages();
            var microsoftTargetLanguages = _microsoftTranslationService.GetSupportedTargetLanguages();

            var targetLanguages = googleTargetLanguages.Intersect(microsoftTargetLanguages).ToList();
            var fromLanguages = googleSourceLanguages.Intersect(microsoftSourceLanguages).ToList();

            AutoTranslate.FillComboWithLanguages(comboBoxFrom, fromLanguages);
            AutoTranslate.FillComboWithLanguages(comboBoxTo, targetLanguages);

            AutoTranslate.SelectLanguageCode(comboBoxFrom, _fromLanguage);
            AutoTranslate.SelectLanguageCode(comboBoxTo, _toLanguage);
        }

        internal void InitializeFromLanguage(string defaultFromLanguage)
        {
            _toLanguage = Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage;
            _fromLanguage = defaultFromLanguage;
            if (_toLanguage != defaultFromLanguage)
            {
                return;
            }

            foreach (var s in Utilities.GetDictionaryLanguages())
            {
                var temp = s.Replace("[", string.Empty).Replace("]", string.Empty);
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

        internal void Initialize(Paragraph paragraph)
        {
            textBoxSourceText.Text = paragraph.Text;
        }

        private void GoogleOrMicrosoftTranslate_Shown(object sender, EventArgs e)
        {
            _googleTranslationService = new GoogleTranslateV1();
            _googleTranslationService.Initialize();

            _microsoftTranslationService = new MicrosoftTranslator();
            _microsoftTranslationService.Initialize();

            InitLanguageComboBoxes();

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
                        buttonGoogle.Text = _googleTranslationService.Translate(textBoxSourceText.Text, _fromLanguage, _toLanguage, CancellationToken.None).Result;
                    }
                    if (_microsoftTranslationService != null)
                    {
                        var result = _microsoftTranslationService.Translate(textBoxSourceText.Text, _fromLanguage, _toLanguage, CancellationToken.None).Result;
                        buttonMicrosoft.Text = result;
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

        private void comboBoxFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxFrom.SelectedIndex > 0 && comboBoxFrom.Text == LanguageSettings.Current.General.ChangeLanguageFilter)
            {
                using (var form = new DefaultLanguagesChooser(Configuration.Settings.General.DefaultLanguages))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        Configuration.Settings.General.DefaultLanguages = form.DefaultLanguages;
                    }
                }

                InitLanguageComboBoxes();
            }
        }

        private void comboBoxTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTo.SelectedIndex > 0 && comboBoxTo.Text == LanguageSettings.Current.General.ChangeLanguageFilter)
            {
                using (var form = new DefaultLanguagesChooser(Configuration.Settings.General.DefaultLanguages))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        Configuration.Settings.General.DefaultLanguages = form.DefaultLanguages;
                    }
                }

                InitLanguageComboBoxes();
            }
        }
    }
}
