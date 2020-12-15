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

        private string _toLanguageIsoCode;
        private string _fromLanguageIsoCode;


        public class ComboBoxItem
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public ComboBoxItem(string text, string value)
            {
                Text = UpcaseFirstLetter(text);
                Value = value;
            }

            private static string UpcaseFirstLetter(string text)
            {
                if (text.Length > 1)
                {
                    text = char.ToUpper(text[0]) + text.Substring(1).ToLowerInvariant();
                }

                return text;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        public GenericTranslate()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = Configuration.Settings.Language.GoogleTranslate.Title;
            labelFrom.Text = Configuration.Settings.Language.GoogleTranslate.From;
            labelTo.Text = Configuration.Settings.Language.GoogleTranslate.To;
            buttonTranslate.Text = Configuration.Settings.Language.GoogleTranslate.Translate;
            labelPleaseWait.Text = Configuration.Settings.Language.GoogleTranslate.PleaseWait;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            labelApiKeyNotFound.Text = string.Empty;

            subtitleListViewFrom.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            subtitleListViewTo.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            subtitleListViewFrom.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListViewFrom.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            subtitleListViewTo.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListViewTo.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            UiUtil.InitializeSubtitleFont(subtitleListViewFrom);
            UiUtil.InitializeSubtitleFont(subtitleListViewTo);
            subtitleListViewFrom.AutoSizeColumns();
            subtitleListViewFrom.AutoSizeColumns();
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        internal void Initialize(Subtitle subtitle, Subtitle target, string title, Encoding encoding)
        {
            InitTranslationServices();
            InitParagraphHandlingStrategies();

            if (title != null)
            {
                Text = title;
            }

            labelPleaseWait.Visible = false;
            progressBar1.Visible = false;
            _subtitle = subtitle;


            if (target != null)
            {
                TranslatedSubtitle = new Subtitle(target);
                subtitleListViewTo.Fill(TranslatedSubtitle);
            }
            else
            {
                TranslatedSubtitle = new Subtitle(subtitle);
                foreach (var paragraph in TranslatedSubtitle.Paragraphs)
                {
                    paragraph.Text = string.Empty;
                }
            }

            _fromLanguageIsoCode = EvaluateDefaultFromLanguageCode(encoding);
            _toLanguageIsoCode = EvaluateUiCultureTargetLanguage(_fromLanguageIsoCode);

            subtitleListViewFrom.Fill(subtitle);
            GoogleTranslate_Resize(null, null);

            _formattingTypes = new FormattingType[_subtitle.Paragraphs.Count];
            _autoSplit = new bool[_subtitle.Paragraphs.Count];
        }

        private void InitParagraphHandlingStrategies()
        {
            this.comboBoxParagraphHandling.Items.Add(new SentenceMergingTranslationProcessor());
            this.comboBoxParagraphHandling.Items.Add(new SingleParagraphTranslationProcessor());
            this.comboBoxParagraphHandling.SelectedIndex = 1;
        }

        private void InitTranslationServices()
        {
            this.comboBoxTranslationServices.Items.AddRange(TranslationServiceManager.Instance.TranslatorEngines.ToArray());
        }

        private void ComboBoxTranslatorEngineChanged(object sender, EventArgs e)
        {
            _translationService = (ITranslationService)comboBoxTranslationServices.SelectedItem;
            _translationService.Init();
            ReadLanguageSettings();
            SetupLanguageSettings();
        }

        private void SetupLanguageSettings()
        {
            FillComboWithLanguages(comboBoxFrom);
            SelectLanguageCode(comboBoxFrom, _fromLanguageIsoCode);

            FillComboWithLanguages(comboBoxTo);
            SelectLanguageCode(comboBoxTo, _toLanguageIsoCode);
        }

        private void ReadLanguageSettings()
        {
            if (comboBoxTo.SelectedItem != null)
            {
                _toLanguageIsoCode = ((ComboBoxItem)comboBoxTo.SelectedItem).Value;
            }

            if (comboBoxFrom.SelectedItem != null)
            {
                _fromLanguageIsoCode = ((ComboBoxItem)comboBoxFrom.SelectedItem).Value;
            }
        }

        private string EvaluateUiCultureTargetLanguage(string defaultFromLanguage)
        {
            var installedLanguages = new List<InputLanguage>();
            foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
            {
                installedLanguages.Add(language);
            }

            string uiCultureTargetLanguage = Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage;
            if (uiCultureTargetLanguage == defaultFromLanguage)
            {
                foreach (string s in Utilities.GetDictionaryLanguages())
                {
                    string temp = s.Replace("[", string.Empty).Replace("]", string.Empty);
                    if (temp.Length > 4)
                    {
                        temp = temp.Substring(temp.Length - 5, 2).ToLowerInvariant();
                        if (temp != defaultFromLanguage && installedLanguages.Any(p => p.Culture.TwoLetterISOLanguageName.Contains(temp)))
                        {
                            uiCultureTargetLanguage = temp;
                            break;
                        }
                    }
                }
            }

            if (uiCultureTargetLanguage == defaultFromLanguage)
            {
                foreach (InputLanguage language in installedLanguages)
                {
                    if (language.Culture.TwoLetterISOLanguageName != defaultFromLanguage)
                    {
                        uiCultureTargetLanguage = language.Culture.TwoLetterISOLanguageName;
                        break;
                    }
                }
            }

            if (uiCultureTargetLanguage == defaultFromLanguage && defaultFromLanguage == "en")
            {
                uiCultureTargetLanguage = "es";
            }

            if (uiCultureTargetLanguage == defaultFromLanguage)
            {
                uiCultureTargetLanguage = "en";
            }

            return uiCultureTargetLanguage;
        }

        private void SelectLanguageCode(ComboBox comboBox, string defaultFromLanguage)
        {
            int i = 0;
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Value == defaultFromLanguage)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }

                i++;
            }
        }

        private string EvaluateDefaultFromLanguageCode(Encoding encoding)
        {
            string defaultFromLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(encoding); // Guess language via encoding
            if (string.IsNullOrEmpty(defaultFromLanguage))
            {
                defaultFromLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle); // Guess language based on subtitle contents
            }

            //convert new Hebrew code (he) to old Hebrew code (iw)  http://www.mathguide.de/info/tools/languagecode.html
            //brummochse: why get it converted to the old code?
            if (defaultFromLanguage == "he")
            {
                defaultFromLanguage = "iw";
            }

            return defaultFromLanguage;
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
                var selectedItems = subtitleListViewFrom.SelectedItems;
                var startIndex = selectedItems.Count <= 0 ? 0 : selectedItems[0].Index;
                progressBar1.Minimum = startIndex;
                var selectedParagraphs=_subtitle.Paragraphs.GetRange(startIndex, _subtitle.Paragraphs.Count - startIndex);
                translator.Translate(_translationService, _fromLanguageIsoCode, _toLanguageIsoCode, selectedParagraphs, targetParagraphs =>
                {
                    int index = targetParagraphs.Keys.First()+ startIndex;
                    FillTranslatedText(targetParagraphs.Values, index, index + targetParagraphs.Count - 1);
                    index += targetParagraphs.Count;
                    progressBar1.Value = index;
                    progressBar1.Refresh();
                    Application.DoEvents();
                    return _breakTranslation;
                });
            }
            catch (WebException webException)
            {
                if (_translationService.GetType() == typeof(GoogleTranslator1))
                {
                    MessageBox.Show("Free API quota exceeded?" + Environment.NewLine +
                                    Environment.NewLine +
                                    webException.Source + ": " + webException.Message);
                }
                else if (_translationService.GetType() == typeof(GoogleTranslator2) && webException.Message.Contains("(400) Bad Request"))
                {
                    MessageBox.Show("API key invalid (or perhaps billing is not enabled)?" + Environment.NewLine +
                                    Environment.NewLine +
                                    webException.Source + ": " + webException.Message);
                }
                else if (_translationService.GetType() == typeof(GoogleTranslator2) && webException.Message.Contains("(403) Forbidden."))
                {
                    MessageBox.Show("Perhaps billing is not enabled (or API key is invalid)?" + Environment.NewLine +
                                    Environment.NewLine +
                                    webException.Source + ": " + webException.Message);
                }
                else
                {
                    MessageBox.Show(webException.Source + ": " + webException.Message);
                }
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

                Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage = _toLanguageIsoCode;
            }
        }

        private void FillTranslatedText(IEnumerable<string> translatedLines, int start, int end)
        {
            int index = start;
            foreach (string s in translatedLines)
            {
                if (index < TranslatedSubtitle.Paragraphs.Count)
                {
                    var cleanText = CleanText(s, index);
                    TranslatedSubtitle.Paragraphs[index].Text = cleanText;
                }
                index++;
            }
            subtitleListViewTo.BeginUpdate();
            subtitleListViewTo.Fill(TranslatedSubtitle);
            subtitleListViewTo.SelectIndexAndEnsureVisible(end);
            subtitleListViewTo.EndUpdate();
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

        public void FillComboWithLanguages(ComboBox comboBox)
        {
            comboBox.Items.Clear();
            foreach (var bingLanguageCode in _translationService.GetTranslationPairs())
            {
                comboBox.Items.Add(new ComboBoxItem(bingLanguageCode.Name, bingLanguageCode.Code));
            }
        }
    
        private void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(_translationService.GetUrl());
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = subtitleListViewTo.Items.Count > 0 ? DialogResult.OK : DialogResult.Cancel;
        }

        private void FormGoogleTranslate_KeyDown(object sender, KeyEventArgs e)
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

        private void GoogleTranslate_Resize(object sender, EventArgs e)
        {
            int width = (Width / 2) - (subtitleListViewFrom.Left * 3) + 19;
            subtitleListViewFrom.Width = width;
            subtitleListViewTo.Width = width;

            int height = Height - (subtitleListViewFrom.Top + buttonTranslate.Height + 60);
            subtitleListViewFrom.Height = height;
            subtitleListViewTo.Height = height;

            comboBoxFrom.Left = subtitleListViewFrom.Left + (subtitleListViewFrom.Width - comboBoxFrom.Width);
            labelFrom.Left = comboBoxFrom.Left - 5 - labelFrom.Width;

            subtitleListViewTo.Left = width + (subtitleListViewFrom.Left * 2);
            labelTo.Left = subtitleListViewTo.Left;
            comboBoxTo.Left = labelTo.Left + labelTo.Width + 5;
            buttonTranslate.Left = comboBoxTo.Left + comboBoxTo.Width + 9;
            labelPleaseWait.Left = buttonTranslate.Left + buttonTranslate.Width + 9;
            progressBar1.Left = labelPleaseWait.Left;
            progressBar1.Width = subtitleListViewTo.Width - (progressBar1.Left - subtitleListViewTo.Left);
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

        private void subtitleListViewFrom_DoubleClick(object sender, EventArgs e)
        {
            SyncListViews(subtitleListViewFrom, subtitleListViewTo);
        }

        private void subtitleListViewTo_DoubleClick(object sender, EventArgs e)
        {
            SyncListViews(subtitleListViewTo, subtitleListViewFrom);
        }

        public string GetFileNameWithTargetLanguage(string oldFileName, string videoFileName, Subtitle oldSubtitle, SubtitleFormat subtitleFormat)
        {
            if (!string.IsNullOrEmpty(_toLanguageIsoCode))
            {
                if (!string.IsNullOrEmpty(videoFileName))
                {
                    return Path.GetFileNameWithoutExtension(videoFileName) + "." + _toLanguageIsoCode.ToLowerInvariant() + subtitleFormat.Extension;
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
                    return s + "." + _toLanguageIsoCode.ToLowerInvariant() + subtitleFormat.Extension;
                }
            }
            return null;
        }

    }
}
