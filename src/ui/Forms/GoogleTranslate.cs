using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GoogleTranslate : PositionAndSizeForm
    {
        public Subtitle TranslatedSubtitle { get; private set; }
        private Subtitle _subtitle;
        private bool _breakTranslation;
        private bool _googleTranslate = true;
        private const string SplitterString = "+-+";
        private ITranslator _translator;

        private enum FormattingType
        {
            None,
            Italic,
            ItalicTwoLines
        }

        private static string GoogleTranslateUrl => new GoogleTranslator2(Configuration.Settings.Tools.GoogleApiV2Key).GetUrl();

        private FormattingType[] _formattingTypes;
        private bool[] _autoSplit;

        private string _targetTwoLetterIsoLanguageName;

        public class ComboBoxItem
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public ComboBoxItem(string text, string value)
            {
                if (text.Length > 1)
                {
                    text = char.ToUpper(text[0]) + text.Substring(1).ToLowerInvariant();
                }

                Text = text;

                Value = value;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        public GoogleTranslate()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.GoogleTranslate.Title;
            labelFrom.Text = LanguageSettings.Current.GoogleTranslate.From;
            labelTo.Text = LanguageSettings.Current.GoogleTranslate.To;
            buttonTranslate.Text = LanguageSettings.Current.GoogleTranslate.Translate;
            labelPleaseWait.Text = LanguageSettings.Current.GoogleTranslate.PleaseWait;
            linkLabelPoweredByGoogleTranslate.Text = LanguageSettings.Current.GoogleTranslate.PoweredByGoogleTranslate;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            labelApiKeyNotFound.Text = string.Empty;

            subtitleListViewFrom.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            subtitleListViewTo.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
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

        internal void Initialize(Subtitle subtitle, Subtitle target, string title, bool googleTranslate, Encoding encoding)
        {
            if (title != null)
            {
                Text = title;
            }

            _googleTranslate = googleTranslate;
            if (!_googleTranslate)
            {
                _translator = new MicrosoftTranslator(Configuration.Settings.Tools.MicrosoftTranslatorApiKey, Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint, Configuration.Settings.Tools.MicrosoftTranslatorCategory);
                linkLabelPoweredByGoogleTranslate.Text = LanguageSettings.Current.GoogleTranslate.PoweredByMicrosoftTranslate;
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


            string defaultFromLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(encoding); // Guess language via encoding
            if (string.IsNullOrEmpty(defaultFromLanguage))
            {
                defaultFromLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle); // Guess language based on subtitle contents
            }

            if (defaultFromLanguage == "he")
            {
                defaultFromLanguage = "iw";
            }

            FillComboWithLanguages(comboBoxFrom);
            int i = 0;
            foreach (ComboBoxItem item in comboBoxFrom.Items)
            {
                if (item.Value == defaultFromLanguage)
                {
                    comboBoxFrom.SelectedIndex = i;
                    break;
                }
                i++;
            }

            var installedLanguages = new List<InputLanguage>();
            foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
            {
                installedLanguages.Add(language);
            }

            FillComboWithLanguages(comboBoxTo);
            i = 0;
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

            comboBoxTo.SelectedIndex = 0;
            foreach (ComboBoxItem item in comboBoxTo.Items)
            {
                if (item.Value == uiCultureTargetLanguage)
                {
                    comboBoxTo.SelectedIndex = i;
                    break;
                }
                i++;
            }

            subtitleListViewFrom.Fill(subtitle);
            GoogleTranslate_Resize(null, null);

            _formattingTypes = new FormattingType[_subtitle.Paragraphs.Count];
            _autoSplit = new bool[_subtitle.Paragraphs.Count];
        }

        private void Translate(string source, string target, ITranslator translator, int maxTextSize, int maximumRequestArrayLength = 100)
        {
            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            _breakTranslation = false;
            buttonTranslate.Text = LanguageSettings.Current.General.Cancel;
            Cursor.Current = Cursors.WaitCursor;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            labelPleaseWait.Visible = true;
            var sourceParagraphs = new List<Paragraph>();
            try
            {
                var log = new StringBuilder();
                var sourceLength = 0;
                var selectedItems = subtitleListViewFrom.SelectedItems;
                var startIndex = selectedItems.Count <= 0 ? 0 : selectedItems[0].Index;
                var start = startIndex;
                int index = startIndex;
                for (int i = startIndex; i < _subtitle.Paragraphs.Count; i++)
                {
                    Paragraph p = _subtitle.Paragraphs[i];
                    sourceLength += Utilities.UrlEncode(p.Text).Length;
                    if ((sourceLength >= maxTextSize || sourceParagraphs.Count >= maximumRequestArrayLength) && sourceParagraphs.Count > 0)
                    {
                        var result = translator.Translate(source, target, sourceParagraphs, log);
                        FillTranslatedText(result, start, index - 1);
                        sourceLength = 0;
                        sourceParagraphs.Clear();
                        progressBar1.Refresh();
                        Application.DoEvents();
                        start = index;
                    }
                    sourceParagraphs.Add(p);
                    index++;
                    progressBar1.Value = index;
                    if (_breakTranslation)
                    {
                        break;
                    }
                }

                if (sourceParagraphs.Count > 0)
                {
                    var result = translator.Translate(source, target, sourceParagraphs, log);
                    FillTranslatedText(result, start, index - 1);
                }
            }
            catch (WebException webException)
            {
                if (translator.GetType() == typeof(GoogleTranslator1))
                {
                    MessageBox.Show("Free API quota exceeded?" + Environment.NewLine +
                                    Environment.NewLine +
                                    webException.Source + ": " + webException.Message);
                }
                else if (translator.GetType() == typeof(GoogleTranslator2) && webException.Message.Contains("(400) Bad Request"))
                {
                    MessageBox.Show("API key invalid (or perhaps billing is not enabled)?" + Environment.NewLine +
                                    Environment.NewLine +
                                    webException.Source + ": " + webException.Message);
                }
                else if (translator.GetType() == typeof(GoogleTranslator2) && webException.Message.Contains("(403) Forbidden."))
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
                buttonTranslate.Text = LanguageSettings.Current.GoogleTranslate.Translate;
                buttonTranslate.Enabled = true;
                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;

                Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage = _targetTwoLetterIsoLanguageName;
            }
        }

        private void FillTranslatedText(List<string> translatedLines, int start, int end)
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

            _targetTwoLetterIsoLanguageName = ((ComboBoxItem)comboBoxTo.SelectedItem).Value;
            Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage = _targetTwoLetterIsoLanguageName;
            var source = ((ComboBoxItem)comboBoxFrom.SelectedItem).Value;

            var language = LanguageSettings.Current.GoogleTranslate;
            if (_googleTranslate && string.IsNullOrEmpty(Configuration.Settings.Tools.GoogleApiV2Key))
            {
                if (Configuration.Settings.Tools.GoogleApiV2KeyInfoShow)
                {
                    using (var form = new DialogDoNotShowAgain("Subtitle Edit", language.GoogleApiKeyNeeded))
                    {
                        form.ShowDialog(this);
                        Configuration.Settings.Tools.GoogleApiV2KeyInfoShow = !form.DoNoDisplayAgain;
                    }
                }

                if (Configuration.Settings.Tools.GoogleTranslateNoKeyWarningShow)
                {
                    using (var form = new DialogDoNotShowAgain("Subtitle Edit", language.GoogleNoApiKeyWarning))
                    {
                        form.ShowDialog(this);
                        Configuration.Settings.Tools.GoogleTranslateNoKeyWarningShow = !form.DoNoDisplayAgain;
                    }
                }

                labelApiKeyNotFound.Left = linkLabelPoweredByGoogleTranslate.Left + linkLabelPoweredByGoogleTranslate.Width + 20;
                labelApiKeyNotFound.Text = language.GoogleNoApiKeyWarning;

                Translate(source, _targetTwoLetterIsoLanguageName, new GoogleTranslator1(), Configuration.Settings.Tools.GoogleApiV1ChunkSize);
                return;
            }

            if (!_googleTranslate && string.IsNullOrEmpty(Configuration.Settings.Tools.MicrosoftTranslatorApiKey))
            {
                MessageBox.Show(language.MsClientSecretNeeded);
                return;
            }


            if (_googleTranslate)
            {
                Translate(source, _targetTwoLetterIsoLanguageName, new GoogleTranslator2(Configuration.Settings.Tools.GoogleApiV2Key), 1000);
            }
            else
            {
                Translate(source, _targetTwoLetterIsoLanguageName, new MicrosoftTranslator(Configuration.Settings.Tools.MicrosoftTranslatorApiKey, Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint, Configuration.Settings.Tools.MicrosoftTranslatorCategory), 1000, MicrosoftTranslator.MaximumRequestArrayLength);
            }
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
            if (!_googleTranslate)
            {
                foreach (var bingLanguageCode in _translator.GetTranslationPairs())
                {
                    comboBox.Items.Add(new ComboBoxItem(bingLanguageCode.Name, bingLanguageCode.Code));
                }
                return;
            }

            FillComboWithGoogleLanguages(comboBox);
        }

        public void FillComboWithGoogleLanguages(ComboBox comboBox)
        {
            var translator = new GoogleTranslator2(Configuration.Settings.Tools.GoogleApiV2Key);
            foreach (var pair in translator.GetTranslationPairs())
            {
                comboBox.Items.Add(new ComboBoxItem(pair.Name, pair.Code));
            }
        }

        private void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(_googleTranslate ? GoogleTranslateUrl : _translator.GetUrl());
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
            if (!string.IsNullOrEmpty(_targetTwoLetterIsoLanguageName))
            {
                if (!string.IsNullOrEmpty(videoFileName))
                {
                    return Path.GetFileNameWithoutExtension(videoFileName) + "." + _targetTwoLetterIsoLanguageName.ToLowerInvariant() + subtitleFormat.Extension;
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
                    return s + "." + _targetTwoLetterIsoLanguageName.ToLowerInvariant() + subtitleFormat.Extension;
                }
            }
            return null;
        }
    }
}
