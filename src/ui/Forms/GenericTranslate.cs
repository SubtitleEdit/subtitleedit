using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate.Processor;
using Nikse.SubtitleEdit.Core.Translate.Service;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GenericTranslate : PositionAndSizeForm
    {
        public Subtitle TranslatedSubtitle { get; private set; }
        private Subtitle _subtitle;
        private bool _breakTranslation;
        private const string SplitterString = "+-+";
        private AbstractTranslationService _translationService;

        private enum FormattingType
        {
            None,
            Italic,
            ItalicTwoLines
        }

        private FormattingType[] _formattingTypes;
        private bool[] _autoSplit;

        private string _targetLanguageIsoCode;
        private string _sourceLanguageIsoCode;
        
        public GenericTranslate()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = Configuration.Settings.Language.GoogleTranslate.Title;
            labelSource.Text = Configuration.Settings.Language.GoogleTranslate.From;
            labelTarget.Text = Configuration.Settings.Language.GoogleTranslate.To;
            buttonTranslate.Text = Configuration.Settings.Language.GoogleTranslate.Translate;
            labelPleaseWait.Text = Configuration.Settings.Language.GoogleTranslate.PleaseWait;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            labelApiKeyNotFound.Text = string.Empty;

            subtitleListViewSource.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            subtitleListViewTarget.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            subtitleListViewSource.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListViewSource.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            subtitleListViewTarget.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListViewTarget.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            UiUtil.InitializeSubtitleFont(subtitleListViewSource);
            UiUtil.InitializeSubtitleFont(subtitleListViewTarget);
            subtitleListViewSource.AutoSizeColumns();
            subtitleListViewSource.AutoSizeColumns();
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        internal void Initialize(Subtitle subtitle, Subtitle target, string title, Encoding encoding)
        {
            if (title != null)
            {
                Text = title;
            }

            labelPleaseWait.Visible = false;
            progressBar1.Visible = false;
            _subtitle = subtitle;
            this.buttonTranslate.Enabled = false;

            if (target != null)
            {
                TranslatedSubtitle = new Subtitle(target);
                subtitleListViewTarget.Fill(TranslatedSubtitle);
            }
            else
            {
                TranslatedSubtitle = new Subtitle(subtitle);
                foreach (var paragraph in TranslatedSubtitle.Paragraphs)
                {
                    paragraph.Text = string.Empty;
                }
            }

            _sourceLanguageIsoCode = EvaluateDefaultSourceLanguageCode(encoding,_subtitle);
            _targetLanguageIsoCode = EvaluateDefaultTargetLanguageCode(_sourceLanguageIsoCode);

            InitTranslationServices();
            InitParagraphHandlingStrategies();

            subtitleListViewSource.Fill(subtitle);
            Translate_Resize(null, null);

            _formattingTypes = new FormattingType[_subtitle.Paragraphs.Count];
            _autoSplit = new bool[_subtitle.Paragraphs.Count];
        }

        private void InitParagraphHandlingStrategies()
        {
            foreach (var translationProcessor in TranslationProcessorRepository.TranslationProcessors)
            {
                comboBoxParagraphHandling.Items.Add(translationProcessor);
            }

            if (comboBoxParagraphHandling.Items.Count > 0)
            {
                comboBoxParagraphHandling.SelectedIndex = 0;
            }
        }

        private void InitTranslationServices()
        {
            var translationServices = TranslationServiceRepository.TranslatorEngines;
            foreach (var translationService in translationServices)
            {
                translationService.MessageLogEvent += (sender, message) => HandleMessage(message, translationService.GetName());
                if (translationService.Initialize())
                {
                    comboBoxTranslationServices.Items.Add(translationService);
                }
            }
            if (comboBoxTranslationServices.Items.Count > 0)
            {
                comboBoxTranslationServices.SelectedIndex = 0;
            }
        }

        private void HandleMessage(string message, string translationServiceName)
        {
            var language = Configuration.Settings.Language.GoogleTranslate;
            if (message.Equals(language.GoogleApiKeyNeeded))
            {
                if (Configuration.Settings.Tools.GoogleApiV2KeyInfoShow)
                {
                    using (var form = new DialogDoNotShowAgain("Subtitle Edit", language.GoogleApiKeyNeeded))
                    {
                        form.ShowDialog(this);
                        Configuration.Settings.Tools.GoogleApiV2KeyInfoShow = !form.DoNoDisplayAgain;
                    }
                }
            }
            else if (message.Equals(language.GoogleNoApiKeyWarning)) {
                if (Configuration.Settings.Tools.GoogleTranslateNoKeyWarningShow)
                {
                    using (var form = new DialogDoNotShowAgain("Subtitle Edit", language.GoogleNoApiKeyWarning))
                    {
                        form.ShowDialog(this);
                        Configuration.Settings.Tools.GoogleTranslateNoKeyWarningShow = !form.DoNoDisplayAgain;
                    }
                }
            }
            else
            {
                MessageBox.Show(message, translationServiceName);
            }
        }


        private void ComboBoxTranslationServiceChanged(object sender, EventArgs e)
        {
            _translationService = (AbstractTranslationService)comboBoxTranslationServices.SelectedItem;
            ReadLanguageSettings();
            SetupLanguageSettings();

            EvaluateTranslateButtonStatus();
        }

        private void EvaluateTranslateButtonStatus()
        {
            buttonTranslate.Enabled = comboBoxSource.SelectedItem != null && comboBoxTarget.SelectedItem != null && _translationService!=null;
        }

        private void ComboBoxLanguageChanged(object sender, EventArgs e)
        {
            EvaluateTranslateButtonStatus();
        }

        private void SetupLanguageSettings()
        {
            FillComboWithLanguages(comboBoxSource, _translationService.GetSupportedSourceLanguages());
            SelectLanguageCode(comboBoxSource, _sourceLanguageIsoCode);

            FillComboWithLanguages(comboBoxTarget, _translationService.GetSupportedTargetLanguages());
            SelectLanguageCode(comboBoxTarget, _targetLanguageIsoCode);
        }

        private void ReadLanguageSettings()
        {
            if (comboBoxTarget.SelectedItem != null)
            {
                _targetLanguageIsoCode = ((TranslationPair)comboBoxTarget.SelectedItem).Code;
            }

            if (comboBoxSource.SelectedItem != null)
            {
                _sourceLanguageIsoCode = ((TranslationPair)comboBoxSource.SelectedItem).Code;
            }
        }

        public static string EvaluateDefaultSourceLanguageCode(Encoding encoding,Subtitle subtitle)
        {
            string defaultSourceLanguageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(encoding); // Guess language via encoding
            if (string.IsNullOrEmpty(defaultSourceLanguageCode))
            {
                defaultSourceLanguageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle); // Guess language based on subtitle contents
            }

            //convert new Hebrew code (he) to old Hebrew code (iw)  http://www.mathguide.de/info/tools/languagecode.html
            //brummochse: why get it converted to the old code?
            if (defaultSourceLanguageCode == "he")
            {
                defaultSourceLanguageCode = "iw";
            }

            return defaultSourceLanguageCode;
        }
        public static string EvaluateDefaultTargetLanguageCode(string defaultSourceLanguage)
        {
            var installedLanguages = new List<InputLanguage>();
            foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
            {
                installedLanguages.Add(language);
            }

            string uiCultureTargetLanguage = Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage;
            if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                foreach (string s in Utilities.GetDictionaryLanguages())
                {
                    string temp = s.Replace("[", string.Empty).Replace("]", string.Empty);
                    if (temp.Length > 4)
                    {
                        temp = temp.Substring(temp.Length - 5, 2).ToLowerInvariant();
                        if (temp != defaultSourceLanguage && installedLanguages.Any(p => p.Culture.TwoLetterISOLanguageName.Contains(temp)))
                        {
                            uiCultureTargetLanguage = temp;
                            break;
                        }
                    }
                }
            }

            if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                foreach (InputLanguage language in installedLanguages)
                {
                    if (language.Culture.TwoLetterISOLanguageName != defaultSourceLanguage)
                    {
                        uiCultureTargetLanguage = language.Culture.TwoLetterISOLanguageName;
                        break;
                    }
                }
            }

            if (uiCultureTargetLanguage == defaultSourceLanguage && defaultSourceLanguage == "en")
            {
                uiCultureTargetLanguage = "es";
            }

            if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                uiCultureTargetLanguage = "en";
            }

            return uiCultureTargetLanguage;
        }

        public static void SelectLanguageCode(ComboBox comboBox, string languageIsoCode)
        {
            int i = 0;
            foreach (TranslationPair item in comboBox.Items)
            {
                if (item.Code == languageIsoCode)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }

                i++;
            }
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            if (buttonTranslate.Text == Configuration.Settings.Language.General.Cancel)
            {
                buttonTranslate.Enabled = false;
                _breakTranslation = true;
                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;
                return;
            }

            ReadLanguageSettings();
  

            Translate();
        }

        private void Translate()
        {
           var translator = (ITranslationProcessor)comboBoxParagraphHandling.SelectedItem;

            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            _breakTranslation = false;
            buttonTranslate.Text = Configuration.Settings.Language.General.Cancel;
            Cursor.Current = Cursors.WaitCursor;

            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Visible = true;
            labelPleaseWait.Visible = true;
            try
            {
                var selectedItems = subtitleListViewSource.SelectedItems;
                var startIndex = selectedItems.Count <= 0 ? 0 : selectedItems[0].Index;
                progressBar1.Minimum = startIndex;
                var selectedParagraphs=_subtitle.Paragraphs.GetRange(startIndex, _subtitle.Paragraphs.Count - startIndex);
                translator.Translate(_translationService, _sourceLanguageIsoCode, _targetLanguageIsoCode, selectedParagraphs, targetParagraphs =>
                {
                    FillTranslatedText(targetParagraphs);
                    int lastIndex = TranslatedSubtitle.Paragraphs.FindIndex(x => x.Number == targetParagraphs.Keys.Last());
                    progressBar1.Value = lastIndex;
                    Application.DoEvents();
                    return _breakTranslation;
                });
            }
            catch (TranslationException translationException)
            {
                MessageBox.Show(translationException.Message + Environment.NewLine +
                                translationException.InnerException?.Source + ": " + translationException.InnerException?.Message);
            }
            finally
            {
                labelPleaseWait.Visible = false;
                progressBar1.Visible = false;
                Cursor.Current = Cursors.Default;
                buttonTranslate.Text = Configuration.Settings.Language.GoogleTranslate.Translate;
                buttonTranslate.Enabled = true;
                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;

                Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage = _targetLanguageIsoCode;
            }
        }

        private void FillTranslatedText(Dictionary<int, string> targetTexts)
        {
            int lastIndex = 0;
            foreach (var targetText in targetTexts)
            {
                int paragraphNumber = targetText.Key;
                string paragraphTargetText = targetText.Value;
                lastIndex=TranslatedSubtitle.Paragraphs.FindIndex(x => x.Number == paragraphNumber);

                var cleanText = CleanText(paragraphTargetText, lastIndex);
                TranslatedSubtitle.Paragraphs[lastIndex].Text = cleanText;
            }
            subtitleListViewTarget.BeginUpdate();
            subtitleListViewTarget.Fill(TranslatedSubtitle);
            subtitleListViewTarget.SelectIndexAndEnsureVisible(lastIndex);
            subtitleListViewTarget.EndUpdate();
        }

        private string CleanText(string s, int index)
        {
            string cleanText = s.Replace("</p>", string.Empty).Trim();
            int indexOfP = cleanText.IndexOf(SplitterString.Trim(), StringComparison.Ordinal);
            if (indexOfP >= 0 && indexOfP < 4)
            {
                cleanText = cleanText.Remove(0, indexOfP);
            }

            cleanText = cleanText.Replace(SplitterString, string.Empty).Trim();
            if (cleanText.Contains('\n') && !cleanText.Contains('\r'))
            {
                cleanText = cleanText.Replace("\n", Environment.NewLine);
            }

            cleanText = cleanText.Replace(" ...", "...");
            cleanText = cleanText.Replace("<br/>", Environment.NewLine);
            cleanText = cleanText.Replace("<br />", Environment.NewLine);
            cleanText = cleanText.Replace("< br/>", Environment.NewLine);
            cleanText = cleanText.Replace("<br/ >", Environment.NewLine);
            cleanText = cleanText.Replace("<br / >", Environment.NewLine);
            cleanText = cleanText.Replace("< br />", Environment.NewLine);
            cleanText = cleanText.Replace("< br / >", Environment.NewLine);
            cleanText = cleanText.Replace("< br/ >", Environment.NewLine);
            cleanText = cleanText.Replace(Environment.NewLine + " ", Environment.NewLine);
            cleanText = cleanText.Replace(" " + Environment.NewLine, Environment.NewLine);
            cleanText = cleanText.Replace("<I>", "<i>");
            cleanText = cleanText.Replace("< I>", "<i>");
            cleanText = cleanText.Replace("</ i>", "</i>");
            cleanText = cleanText.Replace("</ I>", "</i>");
            cleanText = cleanText.Replace("</I>", "</i>");
            cleanText = cleanText.Replace("< i >", "<i>");
            if (cleanText.StartsWith("<i> ", StringComparison.Ordinal))
            {
                cleanText = cleanText.Remove(3, 1);
            }

            if (cleanText.EndsWith(" </i>", StringComparison.Ordinal))
            {
                cleanText = cleanText.Remove(cleanText.Length - 5, 1);
            }

            cleanText = cleanText.Replace(Environment.NewLine + "<i> ", Environment.NewLine + "<i>");
            cleanText = cleanText.Replace(" </i>" + Environment.NewLine, "</i>" + Environment.NewLine);

            if (_autoSplit[index])
            {
                cleanText = Utilities.AutoBreakLine(cleanText);
            }

            if (Utilities.GetNumberOfLines(cleanText) == 1 && Utilities.GetNumberOfLines(_subtitle.Paragraphs[index].Text) == 2)
            {
                if (!_autoSplit[index])
                {
                    cleanText = Utilities.AutoBreakLine(cleanText);
                }
            }

            if (_formattingTypes[index] == FormattingType.ItalicTwoLines || _formattingTypes[index] == FormattingType.Italic)
            {
                cleanText = "<i>" + cleanText + "</i>";
            }

            return cleanText;
        }

        public static void FillComboWithLanguages(ComboBox comboBox, IEnumerable<TranslationPair> languages)
        {
            comboBox.Items.Clear();
            foreach (var language in languages)
            {
                comboBox.Items.Add(language);
            }
        }
    
        private void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(_translationService.GetUrl());
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = subtitleListViewTarget.Items.Count > 0 ? DialogResult.OK : DialogResult.Cancel;
        }

        private void FormTranslate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && labelPleaseWait.Visible == false)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.Escape && labelPleaseWait.Visible)
            {
                _breakTranslation = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#translation");
            }
        }

        private void Translate_Resize(object sender, EventArgs e)
        {
            int width = (Width / 2) - (subtitleListViewSource.Left * 3) + 19;
            subtitleListViewSource.Width = width;
            subtitleListViewTarget.Width = width;

            int height = Height - (subtitleListViewSource.Top + buttonTranslate.Height + 60);
            subtitleListViewSource.Height = height;
            subtitleListViewTarget.Height = height;

            comboBoxSource.Left = subtitleListViewSource.Left + (subtitleListViewSource.Width - comboBoxSource.Width);
            labelSource.Left = comboBoxSource.Left - 5 - labelSource.Width;

            subtitleListViewTarget.Left = width + (subtitleListViewSource.Left * 2);
            labelTarget.Left = subtitleListViewTarget.Left;
            comboBoxTarget.Left = labelTarget.Left + labelTarget.Width + 5;
            buttonTranslate.Left = comboBoxTarget.Left + comboBoxTarget.Width + 9;
            labelPleaseWait.Left = buttonTranslate.Left + buttonTranslate.Width + 9;
            progressBar1.Left = labelPleaseWait.Left;
            progressBar1.Width = subtitleListViewTarget.Width - (progressBar1.Left - subtitleListViewTarget.Left);
        }

        private void SyncListViews(ListView listViewSelected, SubtitleListView listViewOther)
        {
            if (listViewSelected.SelectedItems.Count > 0)
            {
                var first = listViewSelected.TopItem.Index;
                int index = listViewSelected.SelectedItems[0].Index;
                if (index < listViewOther.Items.Count)
                {
                    listViewOther.SelectIndexAndEnsureVisible(index, false);
                    if (first >= 0)
                    {
                        listViewOther.TopItem = listViewOther.Items[first];
                    }
                }
            }
        }

        private void subtitleListViewSource_DoubleClick(object sender, EventArgs e)
        {
            SyncListViews(subtitleListViewSource, subtitleListViewTarget);
        }

        private void subtitleListViewTarget_DoubleClick(object sender, EventArgs e)
        {
            SyncListViews(subtitleListViewTarget, subtitleListViewSource);
        }

        public string GetFileNameWithTargetLanguage(string oldFileName, string videoFileName, Subtitle oldSubtitle, SubtitleFormat subtitleFormat)
        {
            if (!string.IsNullOrEmpty(_targetLanguageIsoCode))
            {
                if (!string.IsNullOrEmpty(videoFileName))
                {
                    return Path.GetFileNameWithoutExtension(videoFileName) + "." + _targetLanguageIsoCode.ToLowerInvariant() + subtitleFormat.Extension;
                }

                if (!string.IsNullOrEmpty(oldFileName))
                {
                    var s = Path.GetFileNameWithoutExtension(oldFileName);
                    if (oldSubtitle != null)
                    {
                        var lang = "." + LanguageAutoDetect.AutoDetectGoogleLanguage(oldSubtitle);
                        if (lang.Length == 3 && s.EndsWith(lang, StringComparison.OrdinalIgnoreCase))
                        {
                            s = s.Remove(s.Length - 3);
                        }
                    }
                    return s + "." + _targetLanguageIsoCode.ToLowerInvariant() + subtitleFormat.Extension;
                }
            }
            return null;
        }

     
    }
}
