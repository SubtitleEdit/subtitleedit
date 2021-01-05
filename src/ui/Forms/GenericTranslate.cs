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
        private ITranslationService _translationService;

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
            Text = LanguageSettings.Current.GoogleTranslate.Title;
            labelSource.Text = LanguageSettings.Current.GoogleTranslate.From;
            labelTarget.Text = LanguageSettings.Current.GoogleTranslate.To;
            buttonTranslate.Text = LanguageSettings.Current.GoogleTranslate.Translate;
            labelPleaseWait.Text = LanguageSettings.Current.GoogleTranslate.PleaseWait;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            labelApiKeyNotFound.Text = string.Empty;

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

            AddTranslationService(GoogleTranslationInitializer.Init(this));
            AddTranslationService(MicrosoftTranslationInitializer.Init());
            AddTranslationService(new NikseDkTranslationService());

            if (comboBoxTranslationServices.Items.Count > 0)
            {
                comboBoxTranslationServices.SelectedIndex = 0;
            }
        }

        public void AddTranslationService(ITranslationService translationService)
        {
            if (translationService != null)
            {
                comboBoxTranslationServices.Items.Add(translationService);
            }
        }

     private void ComboBoxTranslationServiceChanged(object sender, EventArgs e)
        {
            _translationService = (ITranslationService)comboBoxTranslationServices.SelectedItem;
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

            //brummochse: I don't understand what the next code is doing and why?!
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
            if (buttonTranslate.Text == LanguageSettings.Current.General.Cancel)
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
            buttonTranslate.Text = LanguageSettings.Current.General.Cancel;
            Cursor.Current = Cursors.WaitCursor;
          
            progressBar1.Visible = true;
            labelPleaseWait.Visible = true;
            try
            {
                List<Paragraph> selectedParagraphs= GetSelectedParagraphs();
                progressBar1.Minimum = 0;
                progressBar1.Maximum = selectedParagraphs.Count;
                translator.Translate(_translationService, _sourceLanguageIsoCode, _targetLanguageIsoCode, selectedParagraphs, targetParagraphs =>
                {
                    FillTranslatedText(targetParagraphs);
                    progressBar1.Value = selectedParagraphs.FindIndex(x=>x.Number== targetParagraphs.Keys.Last());
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
                buttonTranslate.Text = LanguageSettings.Current.GoogleTranslate.Translate;
                buttonTranslate.Enabled = true;
                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;

                Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage = _targetLanguageIsoCode;
            }
        }

        private List<Paragraph> GetSelectedParagraphs()
        {
            List<Paragraph> selectedParagraphs = new List<Paragraph>();
            ListView.SelectedListViewItemCollection selectedItems = subtitleListViewSource.SelectedItems;
            if (selectedItems.Count > 0)
            {
                foreach (ListViewItem selectedItem in selectedItems)
                {
                    selectedParagraphs.Add(_subtitle.Paragraphs[selectedItem.Index]);
                }
            }
            else
            {
                selectedParagraphs = _subtitle.Paragraphs;
            }
            return selectedParagraphs;
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

    public class GoogleTranslationInitializer
    {
        public static GoogleTranslationService Init(IWin32Window owner = null)
        {
            GoogleTranslationService googleTranslationService = null;
            try
            {
                var language = LanguageSettings.Current.GoogleTranslate;
                if (string.IsNullOrEmpty(Configuration.Settings.Tools.GoogleApiV2Key))
                {
                    if (owner != null && Configuration.Settings.Tools.GoogleApiV2KeyInfoShow)
                    {
                        using (var form = new DialogDoNotShowAgain("Subtitle Edit", language.GoogleApiKeyNeeded))
                        {
                            form.ShowDialog(owner);
                            Configuration.Settings.Tools.GoogleApiV2KeyInfoShow = !form.DoNoDisplayAgain;
                        }
                    }

                    if (owner != null && Configuration.Settings.Tools.GoogleTranslateNoKeyWarningShow)
                    {
                        using (var form = new DialogDoNotShowAgain("Subtitle Edit", language.GoogleNoApiKeyWarning))
                        {
                            form.ShowDialog(owner);
                            Configuration.Settings.Tools.GoogleTranslateNoKeyWarningShow = !form.DoNoDisplayAgain;
                        }
                    }

                    googleTranslationService = new GoogleTranslationService(new GoogleTranslator1());
                }
                else
                {
                    googleTranslationService = new GoogleTranslationService(new GoogleTranslator2(Configuration.Settings.Tools.GoogleApiV2Key));
                }
            }
            catch (Exception e)
            {
                if (owner != null)
                {
                    MessageBox.Show(e.Message + Environment.NewLine + e.InnerException?.Source + ": " + e.InnerException?.Message, "GoogleTranslationService");
                }
            }
            return googleTranslationService;
        }
    }

    public class MicrosoftTranslationInitializer
    {
        public static MicrosoftTranslationService Init(bool showError = false)
        {
            if (string.IsNullOrEmpty(Configuration.Settings.Tools.MicrosoftTranslatorApiKey))
            {
                if (showError)
                {
                    MessageBox.Show(LanguageSettings.Current.GoogleTranslate.MsClientSecretNeeded, "MicrosoftTranslationService");
                }

                return null;
            }

            MicrosoftTranslationService microsoftTranslationService = null;
            try
            {
                microsoftTranslationService = new MicrosoftTranslationService(Configuration.Settings.Tools.MicrosoftTranslatorApiKey, Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint, Configuration.Settings.Tools.MicrosoftTranslatorCategory);
            }
            catch (Exception e)
            {
                if (showError)
                {
                    MessageBox.Show(e.Message + Environment.NewLine + e.InnerException?.Source + ": " + e.InnerException?.Message, "MicrosoftTranslationService");
                }
            }
            return microsoftTranslationService;

        }
    }
}
