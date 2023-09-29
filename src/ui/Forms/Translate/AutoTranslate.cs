using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public sealed partial class AutoTranslate : Form
    {
        public Subtitle TranslatedSubtitle { get; private set; }
        private Subtitle _subtitle;
        private Encoding _encoding;
        private SubtitleFormat _subtitleFormat;
        private IAutoTranslator _autoTranslator;
        private int _translationProgressIndex = -1;
        private bool _translationProgressDirty = true;
        private bool _breakTranslation;

        public AutoTranslate(Subtitle subtitle, Subtitle selectedLines, string title, Encoding encoding, SubtitleFormat subtitleFormat)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.GoogleTranslate.Title;
            buttonTranslate.Text = LanguageSettings.Current.GoogleTranslate.Translate;
            labelPleaseWait.Text = LanguageSettings.Current.GoogleTranslate.PleaseWait;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            subtitleListViewSource.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            subtitleListViewTarget.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            subtitleListViewSource.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListViewSource.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            subtitleListViewTarget.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListViewTarget.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            UiUtil.InitializeSubtitleFont(subtitleListViewSource);
            UiUtil.InitializeSubtitleFont(subtitleListViewTarget);
            subtitleListViewSource.AutoSizeColumns();
            subtitleListViewSource.AutoSizeColumns();
            UiUtil.FixLargeFonts(this, buttonOK);
            ActiveControl = buttonTranslate;

            if (!string.IsNullOrEmpty(title))
            {
                Text = title;
            }

            nikseComboBoxUrl.Items.Clear();
            nikseComboBoxUrl.Items.Add("https://winstxnhdw-nllb-api.hf.space/api/v2/");
            nikseComboBoxUrl.Items.Add("http://localhost:7860/api/v2/");
            nikseComboBoxUrl.SelectedIndex = 0;
            nikseComboBoxUrl.UsePopupWindow = true;

            labelPleaseWait.Visible = false;
            progressBar1.Visible = false;
            _subtitle = new Subtitle(subtitle);
            _encoding = encoding;
            _subtitleFormat = subtitleFormat;

            if (selectedLines != null)
            {
                TranslatedSubtitle = new Subtitle(selectedLines);
                TranslatedSubtitle.Renumber();
                subtitleListViewTarget.Fill(TranslatedSubtitle);
            }
            else
            {
                TranslatedSubtitle = new Subtitle(_subtitle);
                foreach (var paragraph in TranslatedSubtitle.Paragraphs)
                {
                    paragraph.Text = string.Empty;
                }
            }

            subtitleListViewSource.Fill(_subtitle);
            AutoTranslate_Resize(null, null);

            _autoTranslator = new AutoTranslator();
            SetupLanguageSettings();
            UpdateTranslation();
        }

        private void SetupLanguageSettings()
        {
            FillComboWithLanguages(comboBoxSource, _autoTranslator.GetSupportedSourceLanguages());
            var sourceLanguageIsoCode = EvaluateDefaultSourceLanguageCode(_encoding, _subtitle);
            SelectLanguageCode(comboBoxSource, sourceLanguageIsoCode);

            FillComboWithLanguages(comboBoxTarget, _autoTranslator.GetSupportedTargetLanguages());
            var targetLanguageIsoCode = EvaluateDefaultTargetLanguageCode(sourceLanguageIsoCode);
            SelectLanguageCode(comboBoxTarget, targetLanguageIsoCode);
        }

        public static void SelectLanguageCode(NikseComboBox comboBox, string languageIsoCode)
        {
            var i = 0;
            var threeLetterLanguageCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(languageIsoCode);
            foreach (TranslationPair item in comboBox.Items)
            {
                if (item.Code.StartsWith(threeLetterLanguageCode))
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
                i++;
            }

            if (comboBox.SelectedIndex < 0 && comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        public static void FillComboWithLanguages(NikseComboBox comboBox, IEnumerable<TranslationPair> languages)
        {
            comboBox.Items.Clear();
            foreach (var language in languages)
            {
                comboBox.Items.Add(language);
            }
        }

        public static string EvaluateDefaultSourceLanguageCode(Encoding encoding, Subtitle subtitle)
        {
            var defaultSourceLanguageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(encoding); // Guess language via encoding
            if (string.IsNullOrEmpty(defaultSourceLanguageCode))
            {
                defaultSourceLanguageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle); // Guess language based on subtitle contents
            }

            return defaultSourceLanguageCode;
        }

        public static string EvaluateDefaultTargetLanguageCode(string defaultSourceLanguage)
        {
            var installedLanguages = new List<string>();
            foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
            {
                var iso639 = Iso639Dash2LanguageCode.GetTwoLetterCodeFromEnglishName(language.LayoutName);
                if (!string.IsNullOrEmpty(iso639) && !installedLanguages.Contains(iso639))
                {
                    installedLanguages.Add(iso639.ToLowerInvariant());
                }
            }

            var uiCultureTargetLanguage = Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage;
            if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                foreach (var s in Utilities.GetDictionaryLanguages())
                {
                    var temp = s.Replace("[", string.Empty).Replace("]", string.Empty);
                    if (temp.Length > 4)
                    {
                        temp = temp.Substring(temp.Length - 5, 2).ToLowerInvariant();
                        if (temp != defaultSourceLanguage && installedLanguages.Any(p => p.Contains(temp)))
                        {
                            uiCultureTargetLanguage = temp;
                            break;
                        }
                    }
                }
            }

            if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                foreach (var language in installedLanguages)
                {
                    if (language != defaultSourceLanguage)
                    {
                        uiCultureTargetLanguage = language;
                        break;
                    }
                }
            }

            if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                var name = CultureInfo.CurrentCulture.Name;
                if (name.Length > 2)
                {
                    name = name.Remove(0, name.Length - 2);
                }
                var iso = IsoCountryCodes.ThreeToTwoLetterLookup.FirstOrDefault(p => p.Value == name);
                if (!iso.Equals(default(KeyValuePair<string, string>)))
                {
                    var iso639 = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(iso.Key);
                    if (!string.IsNullOrEmpty(iso639))
                    {
                        uiCultureTargetLanguage = iso639;
                    }
                }
            }

            // Set target language to something different than source language
            if (uiCultureTargetLanguage == defaultSourceLanguage && defaultSourceLanguage == "en")
            {
                uiCultureTargetLanguage = "es";
            }
            else if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                uiCultureTargetLanguage = "en";
            }

            return uiCultureTargetLanguage;
        }

        private void AutoTranslate_Resize(object sender, System.EventArgs e)
        {
            var width = (Width / 2) - (subtitleListViewSource.Left * 3) + 19;
            subtitleListViewSource.Width = width;
            subtitleListViewTarget.Width = width;

            var height = Height - (subtitleListViewSource.Top + buttonTranslate.Height + 60);
            subtitleListViewSource.Height = height;
            subtitleListViewTarget.Height = height;

            comboBoxSource.Left = subtitleListViewSource.Left + (subtitleListViewSource.Width - comboBoxSource.Width);
            labelSource.Left = comboBoxSource.Left - 5 - labelSource.Width;

            subtitleListViewTarget.Left = width + (subtitleListViewSource.Left * 2);
            subtitleListViewTarget.Width = Width - subtitleListViewTarget.Left - 32;
            labelTarget.Left = subtitleListViewTarget.Left;
            comboBoxTarget.Left = labelTarget.Left + labelTarget.Width + 5;
            buttonTranslate.Left = comboBoxTarget.Left + comboBoxTarget.Width + 9;
            labelPleaseWait.Left = buttonTranslate.Left + buttonTranslate.Width + 9;
            progressBar1.Left = labelPleaseWait.Left;
            progressBar1.Width = subtitleListViewTarget.Width - (progressBar1.Left - subtitleListViewTarget.Left);
        }

        private async void buttonTranslate_Click(object sender, System.EventArgs e)
        {
            if (buttonTranslate.Text == LanguageSettings.Current.General.Cancel)
            {
                buttonTranslate.Enabled = false;
                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;
                _breakTranslation = true;
                Application.DoEvents();
                buttonOK.Refresh();
                return;
            }

            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            _breakTranslation = false;
            buttonTranslate.Text = LanguageSettings.Current.General.Cancel;

            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            progressBar1.Maximum = TranslatedSubtitle.Paragraphs.Count;
            progressBar1.Visible = true;
            labelPleaseWait.Visible = true;

            _autoTranslator.Initialize(nikseComboBoxUrl.Text);

            var timerUpdate = new Timer();
            timerUpdate.Interval = 1000;
            timerUpdate.Tick += TimerUpdate_Tick;
            timerUpdate.Start();

            if (comboBoxSource.SelectedItem is TranslationPair source &&
                comboBoxTarget.SelectedItem is TranslationPair target)
            {
                var start = subtitleListViewTarget.SelectedIndex >= 0 ? subtitleListViewTarget.SelectedIndex : 0;
                for (var index = start; index < _subtitle.Paragraphs.Count; index++)
                {
                    var p = _subtitle.Paragraphs[index];
                    var translation = await _autoTranslator.Translate(p.Text, source.Code, target.Code);
                    TranslatedSubtitle.Paragraphs[index].Text = translation;
                    _translationProgressIndex = index;
                    _translationProgressDirty = true;
                    progressBar1.Value = index;
                    Application.DoEvents();
                    if (_breakTranslation)
                    {
                        break;
                    }
                }
            }

            timerUpdate.Stop();

            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            buttonOK.Enabled = true;
            buttonCancel.Enabled = true;
            _breakTranslation = false;
            buttonTranslate.Text = LanguageSettings.Current.General.Cancel;

            timerUpdate.Dispose();
            _translationProgressDirty = true;

            UpdateTranslation();

            buttonOK.Focus();
        }

        private void TimerUpdate_Tick(object sender, System.EventArgs e)
        {
            UpdateTranslation();
        }

        private void UpdateTranslation()
        {
            if (!_translationProgressDirty)
            {
                return;
            }

            subtitleListViewTarget.BeginUpdate();
            subtitleListViewTarget.Fill(TranslatedSubtitle);
            _translationProgressDirty = true;
            subtitleListViewTarget.SelectIndexAndEnsureVisible(_translationProgressIndex);
            subtitleListViewTarget.EndUpdate();
        }

        private void AutoTranslate_ResizeEnd(object sender, System.EventArgs e)
        {
            AutoTranslate_Resize(null, null);
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            var isEmpty = TranslatedSubtitle == null || TranslatedSubtitle.Paragraphs.All(p => string.IsNullOrEmpty(p.Text));
            DialogResult = isEmpty ? DialogResult.Cancel : DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
