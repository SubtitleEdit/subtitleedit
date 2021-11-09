using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Core.Translate.Processor;
using Nikse.SubtitleEdit.Core.Translate.Service;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public sealed partial class GenericTranslate : PositionAndSizeForm
    {
        public Subtitle TranslatedSubtitle { get; private set; }
        public bool UseFreeGoogle { get; set; }
        private Subtitle _subtitle;
        private Encoding _encoding;
        private bool _breakTranslation;
        private const string SplitterString = "+-+";
        private ITranslationService _translationService;
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
            labelTranslationService.Text = LanguageSettings.Current.GoogleTranslate.Service;
            comboBoxTranslationServices.Left = labelTranslationService.Left + labelTranslationService.Width + 5;
            labelParagraphHandling.Text = LanguageSettings.Current.GoogleTranslate.LineMergeHandling;
            comboBoxParagraphHandling.Left = labelParagraphHandling.Left + labelParagraphHandling.Width + 5;
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
            if (!string.IsNullOrEmpty(title))
            {
                Text = title;
            }

            // required in translation engine atm
            subtitle.Renumber();

            labelPleaseWait.Visible = false;
            progressBar1.Visible = false;
            _subtitle = subtitle;
            _encoding = encoding;
            buttonTranslate.Enabled = false;

            if (target != null)
            {
                TranslatedSubtitle = new Subtitle(target);
                TranslatedSubtitle.Renumber();
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

            _sourceLanguageIsoCode = EvaluateDefaultSourceLanguageCode(encoding, _subtitle);
            _targetLanguageIsoCode = EvaluateDefaultTargetLanguageCode(_sourceLanguageIsoCode);

            InitTranslationServices();
            InitParagraphHandlingStrategies();

            subtitleListViewSource.Fill(subtitle);
            Translate_Resize(null, null);

            _autoSplit = new bool[_subtitle.Paragraphs.Count];
        }

        private void InitParagraphHandlingStrategies()
        {
            foreach (var translationProcessor in TranslationProcessorRepository.TranslationProcessors)
            {
                var t = translationProcessor.GetType();
                if (t == typeof(NextLineMergeTranslationProcessor))
                {
                    ((NextLineMergeTranslationProcessor)translationProcessor).TranslatedName = LanguageSettings.Current.GoogleTranslate.ProcessorMergeNext;
                }
                else if (t == typeof(SentenceMergingTranslationProcessor))
                {
                    ((SentenceMergingTranslationProcessor)translationProcessor).TranslatedName = LanguageSettings.Current.GoogleTranslate.ProcessorSentence;
                }
                else if (t == typeof(SingleParagraphTranslationProcessor))
                {
                    ((SingleParagraphTranslationProcessor)translationProcessor).TranslatedName = LanguageSettings.Current.GoogleTranslate.ProcessorSingle;
                }

                comboBoxParagraphHandling.Items.Add(translationProcessor);
            }

            if (comboBoxParagraphHandling.Items.Count > 0)
            {
                for (var index = 0; index < comboBoxParagraphHandling.Items.Count; index++)
                {
                    var item = comboBoxParagraphHandling.Items[index].ToString();
                    if (item == Configuration.Settings.Tools.TranslateMergeStrategy)
                    {
                        comboBoxParagraphHandling.SelectedIndex = index;
                        break;
                    }
                }

                if (comboBoxParagraphHandling.SelectedIndex == -1)
                {
                    comboBoxParagraphHandling.SelectedIndex = 0;
                }
            }
        }

        private void InitTranslationServices()
        {
            AddTranslationService(GoogleTranslationInitializer.Init(this));
            AddTranslationService(MicrosoftTranslationInitializer.Init());
            AddTranslationService(new NikseDkTranslationService());

            if (comboBoxTranslationServices.Items.Count > 0 && comboBoxTranslationServices.SelectedIndex < 0)
            {
                comboBoxTranslationServices.SelectedIndex = 0;
            }
        }

        public void AddTranslationService(ITranslationService translationService)
        {
            if (translationService != null)
            {
                comboBoxTranslationServices.Items.Add(translationService);
                if (translationService.GetType().ToString() == Configuration.Settings.Tools.TranslateLastService)
                {
                    comboBoxTranslationServices.SelectedIndex = comboBoxTranslationServices.Items.Count - 1;
                }
            }
        }

        private void EvaluateTranslateButtonStatus()
        {
            buttonTranslate.Enabled = comboBoxSource.SelectedItem != null && comboBoxTarget.SelectedItem != null && _translationService != null;
        }

        private void ComboBoxLanguageChanged(object sender, EventArgs e)
        {
            EvaluateTranslateButtonStatus();
        }

        private void SetupLanguageSettings()
        {
            FillComboWithLanguages(comboBoxSource, _translationService.GetSupportedSourceLanguages());
            _sourceLanguageIsoCode = EvaluateDefaultSourceLanguageCode(_encoding, _subtitle);
            SelectLanguageCode(comboBoxSource, _sourceLanguageIsoCode);

            FillComboWithLanguages(comboBoxTarget, _translationService.GetSupportedTargetLanguages());
            _targetLanguageIsoCode = EvaluateDefaultTargetLanguageCode(_sourceLanguageIsoCode);
            SelectLanguageCode(comboBoxTarget, _targetLanguageIsoCode);
        }

        private void ReadLanguageSettings()
        {
            if (comboBoxSource.SelectedItem != null)
            {
                _sourceLanguageIsoCode = ((TranslationPair)comboBoxSource.SelectedItem).Code;
            }

            if (comboBoxTarget.SelectedItem != null)
            {
                _targetLanguageIsoCode = ((TranslationPair)comboBoxTarget.SelectedItem).Code;
            }
        }

        public static string EvaluateDefaultSourceLanguageCode(Encoding encoding, Subtitle subtitle)
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

            var uiCultureTargetLanguage = Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage;
            if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                foreach (var s in Utilities.GetDictionaryLanguages())
                {
                    var temp = s.Replace("[", string.Empty).Replace("]", string.Empty);
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

            if (comboBox.SelectedIndex < 0 && comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
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

            var translationService = (ITranslationService)comboBoxTranslationServices.SelectedItem;
            if (translationService is GoogleTranslationService &&
                Configuration.Settings.Tools.GoogleTranslateNoKeyWarningShow &&
                string.IsNullOrEmpty(Configuration.Settings.Tools.GoogleApiV2Key))
            {
                using (var form = new DialogDoNotShowAgain("Subtitle Edit", LanguageSettings.Current.GoogleTranslate.GoogleNoApiKeyWarning))
                {
                    form.ShowDialog(this);
                    Configuration.Settings.Tools.GoogleTranslateNoKeyWarningShow = !form.DoNoDisplayAgain;
                }
            }

            ReadLanguageSettings();
            Translate();
        }

        public static bool IsAvailableNetworkActive()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                return (from face in interfaces
                        where face.OperationalStatus == OperationalStatus.Up
                        where (face.NetworkInterfaceType != NetworkInterfaceType.Tunnel) && (face.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        select face.GetIPv4Statistics()).Any(statistics => (statistics.BytesReceived > 0) && (statistics.BytesSent > 0));
            }

            return false;
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
                var selectedParagraphs = GetSelectedParagraphs();
                progressBar1.Minimum = 0;
                progressBar1.Maximum = selectedParagraphs.Count;
                translator.Translate(_translationService, _sourceLanguageIsoCode, _targetLanguageIsoCode, selectedParagraphs, targetParagraphs =>
                {
                    FillTranslatedText(targetParagraphs);
                    progressBar1.Value = selectedParagraphs.FindIndex(x => x.Number == targetParagraphs.Keys.Last());
                    Application.DoEvents();
                    return _breakTranslation;
                });
            }
            catch (TranslationException translationException)
            {
                if (translationException.InnerException != null && !IsAvailableNetworkActive())
                {
                    ShowNetworkError(translationException.InnerException);
                }
                else
                {
                    MessageBox.Show(translationException.Message + Environment.NewLine +
                                    translationException.InnerException?.Source + ": " + translationException.InnerException?.Message);
                }
            }
            catch (Exception exception)
            {
                SeLogger.Error(exception);
                ShowNetworkError(exception);
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

        private static void ShowNetworkError(Exception exception)
        {
            MessageBox.Show("Subtitle Edit was unable to connect to the translation service." + Environment.NewLine +
                            "Try again later or check your internet connection." + Environment.NewLine +
                            Environment.NewLine +
                            "Error: " + exception.Message);
        }

        private List<Paragraph> GetSelectedParagraphs()
        {
            var selectedParagraphs = new List<Paragraph>();
            var selectedItems = subtitleListViewSource.SelectedItems;
            if (selectedItems.Count > 1)
            {
                // use selected items
                foreach (ListViewItem selectedItem in selectedItems)
                {
                    selectedParagraphs.Add(_subtitle.Paragraphs[selectedItem.Index]);
                }
            }
            else if (selectedItems.Count == 1)
            {
                // use first selected index and forward
                var idx = selectedItems[0].Index;
                while (idx < _subtitle.Paragraphs.Count)
                {
                    selectedParagraphs.Add(_subtitle.Paragraphs[idx]);
                    idx++;
                }
            }
            else
            {
                // use all
                selectedParagraphs = _subtitle.Paragraphs;
            }
            return selectedParagraphs;
        }

        private void FillTranslatedText(Dictionary<int, string> targetTexts)
        {
            int lastIndex = 0;
            foreach (var targetText in targetTexts)
            {
                var paragraphNumber = targetText.Key;
                var paragraphTargetText = targetText.Value;
                lastIndex = TranslatedSubtitle.Paragraphs.FindIndex(x => x.Number == paragraphNumber);

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
            var cleanText = s.Replace("</p>", string.Empty).Trim();
            var indexOfP = cleanText.IndexOf(SplitterString.Trim(), StringComparison.Ordinal);
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
            else if (e.KeyData == UiUtil.HelpKeys)
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

        private static void SyncListViews(ListView listViewSelected, SubtitleListView listViewOther)
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

        private void GenericTranslate_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.TranslateLastService = _translationService.GetType().ToString();
            Configuration.Settings.Tools.TranslateMergeStrategy = comboBoxParagraphHandling.Text;
        }

        private void comboBoxTranslationServices_SelectedIndexChanged(object sender, EventArgs e)
        {
            _translationService = (ITranslationService)comboBoxTranslationServices.SelectedItem;
            SetupLanguageSettings();
            EvaluateTranslateButtonStatus();
        }
    }

    public class GoogleTranslationInitializer
    {
        public static GoogleTranslationService Init(IWin32Window owner = null)
        {
            GoogleTranslationService googleTranslationService = null;
            try
            {
                if (string.IsNullOrEmpty(Configuration.Settings.Tools.GoogleApiV2Key))
                {
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
