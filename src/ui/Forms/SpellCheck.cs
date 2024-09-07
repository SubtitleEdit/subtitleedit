﻿using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SpellCheck;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Forms.BinaryEdit;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SpellCheck;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SpellCheck : Form, IDoSpell
    {
        private List<UndoObject> _undoList = new List<UndoObject>();
        private List<string> _suggestions;
        private string _wordSplitListLanguage;
        private string[] _wordSplitList;
        private string _currentAction;
        public SpellCheckAction Action { get; set; } = SpellCheckAction.Skip;
        public string ChangeWord
        {
            get => textBoxWord.Text;
            set => textBoxWord.Text = value;
        }
        public string ChangeWholeText => textBoxWholeText.Text;
        public bool AutoFixNames => checkBoxAutoChangeNames.Checked;
        private readonly SubtitleFormat _subtitleFormat;
        private SpellCheckWordLists _spellCheckWordLists;
        private List<string> _skipAllList = new List<string>();
        private HashSet<string> _skipOneList = new HashSet<string>();
        private Dictionary<string, string> _changeAllDictionary;
        private string _prefix = string.Empty;
        private string _postfix = string.Empty;
        private Hunspell _hunspell;
        private Paragraph _currentParagraph;
        private int _currentIndex;
        private string _currentWord;
        private SpellCheckWord _currentSpellCheckWord;
        private List<SpellCheckWord> _words;
        private int _wordsIndex;
        private Subtitle _subtitle;
        private string _originalWord;
        private int _noOfSkippedWords;
        private int _noOfChangedWords;
        private int _noOfCorrectWords;
        private int _noOfNames;
        private int _noOfAddedWords;
        private bool _firstChange = true;
        private string _languageName;
        private Main _mainWindow;
        private string _currentDictionary;
        private string _imageSubFileName;
        private List<IBinaryParagraphWithPosition> _binSubtitles;
        private ContextMenuStrip _bookmarkContextMenu;
        private readonly Main _mainForm;

        public class SuggestionParameter
        {
            public string InputWord { get; set; }
            public List<string> Suggestions { get; set; }
            public Hunspell Hunspell { get; set; }
            public bool Success { get; set; }

            public SuggestionParameter(string word, Hunspell hunspell)
            {
                InputWord = word;
                Suggestions = new List<string>();
                Hunspell = hunspell;
                Success = false;
            }
        }

        public string LanguageString
        {
            get
            {
                var name = comboBoxDictionaries.SelectedItem.ToString();
                var start = name.LastIndexOf('[');
                var end = name.LastIndexOf(']');
                if (start < 0 || end <= start)
                {
                    return null;
                }

                start++;
                name = name.Substring(start, end - start);
                return name;
            }
        }

        public SpellCheck(SubtitleFormat subtitleFormat, string imageSubFileName, Main mainForm)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _subtitleFormat = subtitleFormat;
            _imageSubFileName = imageSubFileName;
            _mainForm = mainForm;
            labelActionInfo.Text = string.Empty;
            Text = LanguageSettings.Current.SpellCheck.Title;
            labelFullText.Text = LanguageSettings.Current.SpellCheck.FullText;
            labelLanguage.Text = LanguageSettings.Current.SpellCheck.Language;
            groupBoxWordNotFound.Text = LanguageSettings.Current.SpellCheck.WordNotFound;
            buttonAddToDictionary.Text = LanguageSettings.Current.SpellCheck.AddToUserDictionary;
            buttonChange.Text = LanguageSettings.Current.SpellCheck.Change;
            buttonChangeAll.Text = LanguageSettings.Current.SpellCheck.ChangeAll;
            buttonSkipAll.Text = LanguageSettings.Current.SpellCheck.SkipAll;
            buttonSkipOnce.Text = LanguageSettings.Current.SpellCheck.SkipOnce;
            buttonUseSuggestion.Text = LanguageSettings.Current.SpellCheck.Use;
            buttonUseSuggestionAlways.Text = LanguageSettings.Current.SpellCheck.UseAlways;
            buttonAbort.Text = LanguageSettings.Current.SpellCheck.Abort;
            buttonEditWholeText.Text = LanguageSettings.Current.SpellCheck.EditWholeText;
            checkBoxAutoChangeNames.Text = LanguageSettings.Current.SpellCheck.AutoFixNames;
            checkBoxAutoChangeNames.Checked = Configuration.Settings.Tools.SpellCheckAutoChangeNameCasing;
            groupBoxEditWholeText.Text = LanguageSettings.Current.SpellCheck.EditWholeText;
            buttonChangeWholeText.Text = LanguageSettings.Current.SpellCheck.Change;
            buttonSkipText.Text = LanguageSettings.Current.SpellCheck.SkipOnce;
            groupBoxSuggestions.Text = LanguageSettings.Current.SpellCheck.Suggestions;
            buttonAddToNames.Text = LanguageSettings.Current.SpellCheck.AddToNamesAndIgnoreList;
            buttonGoogleIt.Text = LanguageSettings.Current.Main.VideoControls.GoogleIt;
            deleteToolStripMenuItem.Text = LanguageSettings.Current.General.DeleteCurrentLine;
            useLargerFontForThisWindowToolStripMenuItem.Text = LanguageSettings.Current.General.UseLargerFontForThisWindow;
            useLargerFontForThisWindowToolStripMenuItem1.Text = LanguageSettings.Current.General.UseLargerFontForThisWindow;
            bookmarkCommentToolStripMenuItem.Text = LanguageSettings.Current.Settings.ToggleBookmarksWithComment;
            bookmarkCommentToolStripMenuItem.ShortcutKeys = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralToggleBookmarksWithText);

            var gs = Configuration.Settings.General;
            var textBoxFont = gs.SubtitleTextBoxFontBold ? new Font(gs.SubtitleFontName, gs.SubtitleTextBoxFontSize, FontStyle.Bold) : new Font(gs.SubtitleFontName, gs.SubtitleTextBoxFontSize);
            richTextBoxParagraph.Font = textBoxFont;
            textBoxWholeText.Font = textBoxFont;

            UiUtil.FixLargeFonts(this, buttonAbort);
            richTextBoxParagraph.DetectUrls = false;

            LoadImageSub(_imageSubFileName);

            if (Configuration.Settings.Tools.SpellCheckUseLargerFont)
            {
                useLargerFontForThisWindowToolStripMenuItem_Click(null, null);
            }

            SetSearchEngine();
        }

        private void SetSearchEngine()
        {
            contextMenuStripSearchEngine.Items.Add(
                new ToolStripMenuItem(
                    "Google",
                    null,
                    SearchEngineGoogle)
                {
                    Tag = "Google"
                });

            if (!string.IsNullOrEmpty(Configuration.Settings.VideoControls.CustomSearchText1) &&
                !string.IsNullOrEmpty(Configuration.Settings.VideoControls.CustomSearchUrl1))
            {
                contextMenuStripSearchEngine.Items.Add(
                new ToolStripMenuItem(
                    Configuration.Settings.VideoControls.CustomSearchText1,
                    null,
                    SearchEngineCustom1)
                {
                    Tag = Configuration.Settings.VideoControls.CustomSearchText1
                });
            }

            if (!string.IsNullOrEmpty(Configuration.Settings.VideoControls.CustomSearchText2) &&
                !string.IsNullOrEmpty(Configuration.Settings.VideoControls.CustomSearchUrl2))
            {
                contextMenuStripSearchEngine.Items.Add(
                    new ToolStripMenuItem(
                        Configuration.Settings.VideoControls.CustomSearchText2,
                        null,
                        SearchEngineCustom2)
                    {
                        Tag = Configuration.Settings.VideoControls.CustomSearchText2
                    });
            }

            if (!string.IsNullOrEmpty(Configuration.Settings.VideoControls.CustomSearchText3) &&
                !string.IsNullOrEmpty(Configuration.Settings.VideoControls.CustomSearchUrl3))
            {
                contextMenuStripSearchEngine.Items.Add(
                    new ToolStripMenuItem(
                        Configuration.Settings.VideoControls.CustomSearchText3,
                        null,
                        SearchEngineCustom3)
                    {
                        Tag = Configuration.Settings.VideoControls.CustomSearchText3
                    });
            }

            if (!string.IsNullOrEmpty(Configuration.Settings.VideoControls.CustomSearchText4) &&
                !string.IsNullOrEmpty(Configuration.Settings.VideoControls.CustomSearchUrl4))
            {
                contextMenuStripSearchEngine.Items.Add(
                    new ToolStripMenuItem(
                        Configuration.Settings.VideoControls.CustomSearchText4,
                        null,
                        SearchEngineCustom4)
                    {
                        Tag = Configuration.Settings.VideoControls.CustomSearchText4
                    });
            }

            if (!string.IsNullOrEmpty(Configuration.Settings.VideoControls.CustomSearchText5) &&
                !string.IsNullOrEmpty(Configuration.Settings.VideoControls.CustomSearchUrl5))
            {
                contextMenuStripSearchEngine.Items.Add(
                    new ToolStripMenuItem(
                        Configuration.Settings.VideoControls.CustomSearchText5,
                        null,
                        SearchEngineCustom5)
                    {
                        Tag = Configuration.Settings.VideoControls.CustomSearchText5
                    });
            }

            if (Configuration.Settings.Tools.SpellCheckSearchEngine == "Google")
            {
                SearchEngineGoogle(null, null);
            }
            else if (!string.IsNullOrEmpty(Configuration.Settings.Tools.SpellCheckSearchEngine) &&
                  (Configuration.Settings.VideoControls.CustomSearchText1 +
                   Configuration.Settings.VideoControls.CustomSearchText2 +
                   Configuration.Settings.VideoControls.CustomSearchText3 +
                   Configuration.Settings.VideoControls.CustomSearchText4 +
                   Configuration.Settings.VideoControls.CustomSearchText5
                  ).Contains(Configuration.Settings.Tools.SpellCheckSearchEngine))
            {
                buttonGoogleIt.Text = Configuration.Settings.Tools.SpellCheckSearchEngine;
            }
        }

        private void SearchEngineGoogle(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.SpellCheckSearchEngine = "Google";
            buttonGoogleIt.Text = LanguageSettings.Current.Main.VideoControls.GoogleIt;
            foreach (ToolStripMenuItem item in contextMenuStripSearchEngine.Items)
            {
                item.Checked = (string)item.Tag == Configuration.Settings.Tools.SpellCheckSearchEngine;
            }
        }

        private void SearchEngineCustom1(object sender, EventArgs e)
        {
            SearchEngineCustom(Configuration.Settings.VideoControls.CustomSearchText1, Configuration.Settings.VideoControls.CustomSearchUrl1);
        }

        private void SearchEngineCustom2(object sender, EventArgs e)
        {
            SearchEngineCustom(Configuration.Settings.VideoControls.CustomSearchText2, Configuration.Settings.VideoControls.CustomSearchUrl2);
        }

        private void SearchEngineCustom3(object sender, EventArgs e)
        {
            SearchEngineCustom(Configuration.Settings.VideoControls.CustomSearchText3, Configuration.Settings.VideoControls.CustomSearchUrl3);
        }

        private void SearchEngineCustom4(object sender, EventArgs e)
        {
            SearchEngineCustom(Configuration.Settings.VideoControls.CustomSearchText4, Configuration.Settings.VideoControls.CustomSearchUrl4);
        }

        private void SearchEngineCustom5(object sender, EventArgs e)
        {
            SearchEngineCustom(Configuration.Settings.VideoControls.CustomSearchText5, Configuration.Settings.VideoControls.CustomSearchUrl5);
        }

        private void SearchEngineCustom(string text, string url)
        {
            Configuration.Settings.Tools.SpellCheckSearchEngine = text;
            buttonGoogleIt.Text = text;
            foreach (ToolStripMenuItem item in contextMenuStripSearchEngine.Items)
            {
                item.Checked = (string)item.Tag == Configuration.Settings.Tools.SpellCheckSearchEngine;
            }
        }


        private void LoadImageSub(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            if (FileUtil.IsBluRaySup(fileName))
            {
                MakeUiReadyForSourceImages(fileName);
                var bluRaySubtitles = BluRaySupParser.ParseBluRaySup(_imageSubFileName, new StringBuilder());
                BinEdit.FixShortDisplayTimes(bluRaySubtitles);
                _binSubtitles = new List<IBinaryParagraphWithPosition>(bluRaySubtitles);
                return;
            }

            if (fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                MakeUiReadyForSourceImages(fileName);
                _binSubtitles = new List<IBinaryParagraphWithPosition>();
                var bdnXml = new BdnXml();
                var enc = LanguageAutoDetect.GetEncodingFromFile(fileName, true);
                var list = new List<string>(File.ReadAllLines(fileName, enc));
                if (bdnXml.IsMine(list, fileName))
                {
                    _subtitle = new Subtitle();
                    bdnXml.LoadSubtitle(_subtitle, list, fileName);
                    _binSubtitles = new List<IBinaryParagraphWithPosition>();
                    var res = BinEdit.BdnXmlParagraph.GetResolution(fileName);
                    foreach (var p in _subtitle.Paragraphs)
                    {
                        IBinaryParagraphWithPosition bp = new BinEdit.BdnXmlParagraph(p, fileName, res.Width, res.Height);
                        _binSubtitles.Add(bp);
                    }

                    return;
                }
            }

            if (FileUtil.IsVobSub(fileName))
            {
                MakeUiReadyForSourceImages(fileName);
                _binSubtitles = new List<IBinaryParagraphWithPosition>();
                var vobSubParser = new VobSubParser(true);
                var idxFileName = Path.ChangeExtension(fileName, ".idx");
                vobSubParser.OpenSubIdx(fileName, idxFileName);
                var vobSubMergedPackList = vobSubParser.MergeVobSubPacks();
                var palette = vobSubParser.IdxPalette;
                vobSubParser.VobSubPacks.Clear();
                var languageStreamIds = new List<int>();
                foreach (var pack in vobSubMergedPackList)
                {
                    if (pack.SubPicture.Delay.TotalMilliseconds > 500 && !languageStreamIds.Contains(pack.StreamId))
                    {
                        languageStreamIds.Add(pack.StreamId);
                    }
                }

                if (languageStreamIds.Count > 1)
                {
                    vobSubMergedPackList = vobSubMergedPackList.Where(p => p.StreamId == languageStreamIds[0]).ToList();
                }

                var max = vobSubMergedPackList.Count;
                for (var i = 0; i < max; i++)
                {
                    var vobSubPack = vobSubMergedPackList[i];
                    vobSubPack.Palette = palette;
                    _binSubtitles.Add(vobSubPack);
                }

                return;
            }

            pictureBoxBdSup.Visible = false;
        }

        private void MakeUiReadyForSourceImages(string fileName)
        {
            _imageSubFileName = fileName;
            if (_binSubtitles == null)
            {
                Width += 200;
            }

            groupBoxSuggestions.Top = groupBoxWordNotFound.Top;
            groupBoxSuggestions.Height = buttonAbort.Top - groupBoxSuggestions.Top - 15;
            pictureBoxBdSup.Visible = true;
        }

        public void Initialize(string languageName, SpellCheckWord word, List<string> suggestions, string paragraph, string progress)
        {
            _originalWord = word.Text;
            _suggestions = suggestions;
            groupBoxWordNotFound.Visible = true;
            groupBoxEditWholeText.Visible = false;
            buttonEditWholeText.Text = LanguageSettings.Current.SpellCheck.EditWholeText;
            Text = LanguageSettings.Current.SpellCheck.Title + " [" + languageName + "] - " + progress;
            textBoxWord.Text = word.Text;
            textBoxWholeText.Text = paragraph;
            listBoxSuggestions.Items.Clear();
            foreach (string suggestion in suggestions)
            {
                listBoxSuggestions.Items.Add(suggestion);
            }

            if (listBoxSuggestions.Items.Count > 0)
            {
                listBoxSuggestions.SelectedIndex = 0;
            }

            richTextBoxParagraph.Text = paragraph;

            FillSpellCheckDictionaries(languageName);
            ShowActiveWordWithColor(word);
            Action = SpellCheckAction.Skip;
            DialogResult = DialogResult.None;

            DisplayImageSub();
        }

        private void DisplayImageSub()
        {
            if (_binSubtitles != null)
            {
                pictureBoxBdSup.Image?.Dispose();
                var imageSub = _binSubtitles.FirstOrDefault(p => Math.Abs(p.StartTimeCode.TotalMilliseconds - _currentParagraph.StartTime.TotalMilliseconds) < 0.01);
                if (imageSub == null && _subtitle.Paragraphs.Count == _binSubtitles.Count)
                {
                    imageSub = _binSubtitles[_currentIndex];
                }

                if (imageSub == null)
                {
                    var mean = _currentParagraph.StartTime.TotalMilliseconds + _currentParagraph.DurationTotalMilliseconds / 2;
                    imageSub = _binSubtitles.FirstOrDefault(p => mean >= p.StartTimeCode.TotalMilliseconds && mean <= _currentParagraph.EndTime.TotalMilliseconds);
                }

                if (imageSub != null)
                {
                    var charImage = imageSub.GetBitmap();
                    var n = new NikseBitmap(charImage);
                    n.CropSidesAndBottom(2, Color.FromArgb(0, 0, 0, 0), true);
                    n.CropTop(2, Color.FromArgb(0, 0, 0, 0));
                    n.CropSidesAndBottom(2, Color.Transparent, true);
                    n.CropTop(2, Color.Transparent);
                    charImage.Dispose();
                    charImage = n.GetBitmap();

                    var w = Width - pictureBoxBdSup.Left - 20;
                    var h = groupBoxSuggestions.Top - pictureBoxBdSup.Top - 7;
                    pictureBoxBdSup.Image = charImage;
                    if (charImage.Width > w || charImage.Height > h)
                    {
                        pictureBoxBdSup.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBoxBdSup.Width = w;
                        pictureBoxBdSup.Height = h;
                    }
                    else
                    {
                        pictureBoxBdSup.SizeMode = PictureBoxSizeMode.AutoSize;
                    }

                    pictureBoxBdSup.Visible = true;
                    pictureBoxBdSup.BringToFront();
                }
            }
        }

        private string[] LoadWordSplitList(string languageName)
        {
            if (languageName == null)
            {
                return Array.Empty<string>();
            }

            var twoLetterLanguageName = languageName.Trim('[');
            if (twoLetterLanguageName.Length < 2)
            {
                return Array.Empty<string>();
            }

            twoLetterLanguageName = twoLetterLanguageName.Substring(0, 2);
            if (_wordSplitListLanguage == languageName)
            {
                return _wordSplitList;
            }

            _wordSplitListLanguage = languageName;
            var threeLetterIsoLanguageName = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(twoLetterLanguageName);
            return StringWithoutSpaceSplitToWords.LoadWordSplitList(threeLetterIsoLanguageName, null);
        }

        private void FillSpellCheckDictionaries(string languageName)
        {
            comboBoxDictionaries.SelectedIndexChanged -= ComboBoxDictionariesSelectedIndexChanged;
            comboBoxDictionaries.Items.Clear();
            foreach (string name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
                if (name.Contains("[" + languageName + "]"))
                {
                    comboBoxDictionaries.SelectedIndex = comboBoxDictionaries.Items.Count - 1;
                }
            }
            comboBoxDictionaries.SelectedIndexChanged += ComboBoxDictionariesSelectedIndexChanged;
        }

        private void ShowActiveWordWithColor(SpellCheckWord word)
        {
            richTextBoxParagraph.SelectAll();
            richTextBoxParagraph.SelectionColor = Configuration.Settings.General.UseDarkTheme ? Configuration.Settings.General.DarkThemeForeColor : Color.Black;
            richTextBoxParagraph.SelectionFont = textBoxWholeText.Font;
            richTextBoxParagraph.SelectionLength = 0;

            for (int i = 0; i < 10; i++)
            {
                int idx = word.Index - i;
                if (idx >= 0 && idx < richTextBoxParagraph.Text.Length && richTextBoxParagraph.Text.Substring(idx).StartsWith(word.Text, StringComparison.Ordinal))
                {
                    richTextBoxParagraph.SelectionStart = idx;
                    richTextBoxParagraph.SelectionLength = word.Text.Length;
                    richTextBoxParagraph.SelectionColor = Color.Red;
                    break;
                }
                idx = word.Index + i;
                if (idx >= 0 && idx < richTextBoxParagraph.Text.Length && richTextBoxParagraph.Text.Substring(idx).StartsWith(word.Text, StringComparison.Ordinal))
                {
                    richTextBoxParagraph.SelectionStart = idx;
                    richTextBoxParagraph.SelectionLength = word.Text.Length;
                    richTextBoxParagraph.SelectionColor = Color.Red;
                    break;
                }
            }
        }

        private void FormSpellCheck_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Action = SpellCheckAction.Abort;
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#spellcheck");
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.G)
            {
                e.SuppressKeyPress = true;
                UiUtil.OpenUrl("https://www.google.com/search?q=" + Utilities.UrlEncode(textBoxWord.Text));
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Z)
            {
                if (buttonUndo.Visible)
                {
                    buttonUndo_Click(null, null);
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyData == UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainFileOpen))
            {
                e.SuppressKeyPress = true;
                BeginInvoke(new Action(OpenImageBasedSourceFile));
            }
            else if (e.KeyData == UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralToggleBookmarks))
            {
                e.SuppressKeyPress = true;
                ToggleBookmark();
            }
            else if (e.KeyData == UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralEditBookmarks))
            {
                e.SuppressKeyPress = true;
                var p = _subtitle.GetParagraphOrDefault(_currentIndex);
                if (p == null)
                {
                    return;
                }

                if (p.Bookmark != null)
                {
                    _mainForm.EditBookmark(_currentIndex, this);
                }
            }
        }

        private void OpenImageBasedSourceFile()
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = LanguageSettings.Current.Main.BluRaySupFiles + "|*.sup|BDN xml/png|*.xml|VobSub|*.sub";
                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                LoadImageSub(openFileDialog1.FileName);
                DisplayImageSub();
            }
        }

        private void ButtonAbortClick(object sender, EventArgs e)
        {
            ShowEndStatusMessage(LanguageSettings.Current.SpellCheck.SpellCheckAborted);
            DialogResult = DialogResult.Abort;
        }

        private void ButtonChangeClick(object sender, EventArgs e)
        {
            PushUndo($"{LanguageSettings.Current.SpellCheck.Change}: {_currentWord + " > " + textBoxWord.Text}", SpellCheckAction.Change);
            DoAction(SpellCheckAction.Change);
        }

        private void ButtonUseSuggestionClick(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex >= 0)
            {
                textBoxWord.Text = listBoxSuggestions.SelectedItem.ToString();
                PushUndo($"{LanguageSettings.Current.SpellCheck.Change}: {_currentWord + " > " + textBoxWord.Text}", SpellCheckAction.Change);
                DoAction(SpellCheckAction.Change);
            }
        }

        private void ButtonSkipAllClick(object sender, EventArgs e)
        {
            PushUndo($"{LanguageSettings.Current.SpellCheck.SkipAll}: {textBoxWord.Text}", SpellCheckAction.SkipAll);
            DoAction(SpellCheckAction.SkipAll);
        }

        private void ButtonSkipOnceClick(object sender, EventArgs e)
        {
            PushUndo($"{LanguageSettings.Current.SpellCheck.SkipOnce}: {textBoxWord.Text}", SpellCheckAction.Skip);
            DoAction(SpellCheckAction.Skip);
        }

        private void ButtonAddToDictionaryClick(object sender, EventArgs e)
        {
            PushUndo($"{LanguageSettings.Current.SpellCheck.AddToUserDictionary}: {textBoxWord.Text}", SpellCheckAction.AddToDictionary);
            DoAction(SpellCheckAction.AddToDictionary);
        }

        private void ComboBoxDictionariesSelectedIndexChanged(object sender, EventArgs e)
        {
            Configuration.Settings.General.SpellCheckLanguage = LanguageString;
            Configuration.Settings.Save();
            _languageName = LanguageString;
            var dictionary = Utilities.DictionaryFolder + _languageName;
            LoadDictionaries(Utilities.DictionaryFolder, dictionary, _languageName);
            _wordsIndex--;
            PrepareNextWord();
        }

        private void LoadHunspell(string dictionary)
        {
            _currentDictionary = dictionary;
            _hunspell?.Dispose();
            _hunspell = Hunspell.GetHunspell(dictionary);
        }

        public bool DoSpell(string word)
        {
            return _hunspell.Spell(word);
        }

        public List<string> DoSuggest(string word)
        {
            var parameter = new SuggestionParameter(word, _hunspell);
            var suggestThread = new System.Threading.Thread(DoWork);
            suggestThread.Start(parameter);
            suggestThread.Join(3000); // wait max 3 seconds
            if (!parameter.Success)
            {
                LoadHunspell(_currentDictionary);
            }

            return parameter.Suggestions;
        }

        public static void DoWork(object data)
        {
            var parameter = (SuggestionParameter)data;
            parameter.Suggestions = parameter.Hunspell.Suggest(parameter.InputWord);
            parameter.Success = true;
        }

        private void ButtonChangeAllClick(object sender, EventArgs e)
        {
            PushUndo($"{LanguageSettings.Current.SpellCheck.ChangeAll}: {_currentWord + " > " + textBoxWord.Text}", SpellCheckAction.ChangeAll);
            DoAction(SpellCheckAction.ChangeAll);
        }

        private void ButtonUseSuggestionAlwaysClick(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex >= 0)
            {
                textBoxWord.Text = listBoxSuggestions.SelectedItem.ToString();
                PushUndo($"{LanguageSettings.Current.SpellCheck.ChangeAll}: {_currentWord + " > " + textBoxWord.Text}", SpellCheckAction.ChangeAll);
                DoAction(SpellCheckAction.ChangeAll);
            }
        }

        private void SpellCheck_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.SpellCheckAutoChangeNameCasing = AutoFixNames;
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult = DialogResult.Abort;
            }
        }

        private void ButtonAddToNamesClick(object sender, EventArgs e)
        {
            PushUndo($"{LanguageSettings.Current.SpellCheck.AddToNamesAndIgnoreList}: {textBoxWord.Text}", SpellCheckAction.AddToNames);
            DoAction(SpellCheckAction.AddToNames);
        }

        private void ButtonEditWholeTextClick(object sender, EventArgs e)
        {
            if (groupBoxWordNotFound.Visible)
            {
                groupBoxWordNotFound.Visible = false;
                groupBoxEditWholeText.Visible = true;
                buttonEditWholeText.Text = LanguageSettings.Current.SpellCheck.EditWordOnly;
                textBoxWholeText.Focus();
            }
            else
            {
                groupBoxWordNotFound.Visible = true;
                groupBoxEditWholeText.Visible = false;
                buttonEditWholeText.Text = LanguageSettings.Current.SpellCheck.EditWholeText;
                textBoxWord.Focus();
            }
        }

        private void ButtonSkipTextClick(object sender, EventArgs e)
        {
            PushUndo($"{LanguageSettings.Current.SpellCheck.SkipOnce}", SpellCheckAction.Skip);
            DoAction(SpellCheckAction.SkipWholeLine);
        }

        private void ButtonChangeWholeTextClick(object sender, EventArgs e)
        {
            PushUndo($"{LanguageSettings.Current.SpellCheck.EditWholeText}", SpellCheckAction.ChangeWholeText);
            DoAction(SpellCheckAction.ChangeWholeText);
        }

        private void ContextMenuStrip1Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool showAddItems = false;
            if (!string.IsNullOrWhiteSpace(richTextBoxParagraph.SelectedText))
            {
                string word = richTextBoxParagraph.SelectedText.Trim();
                addXToNamesnoiseListToolStripMenuItem.Text = string.Format(LanguageSettings.Current.SpellCheck.AddXToNames, word);
                addXToUserDictionaryToolStripMenuItem.Text = string.Format(LanguageSettings.Current.SpellCheck.AddXToUserDictionary, word);
                showAddItems = true;
            }
            addXToNamesnoiseListToolStripMenuItem.Visible = showAddItems;
            addXToUserDictionaryToolStripMenuItem.Visible = showAddItems;
            toolStripSeparator1.Visible = showAddItems;
        }

        private void AddXToNamesNoiseListToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(richTextBoxParagraph.SelectedText))
            {
                ChangeWord = richTextBoxParagraph.SelectedText.Trim();
                DoAction(SpellCheckAction.AddToNamesOnly);
            }
        }

        private void AddXToUserDictionaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(richTextBoxParagraph.SelectedText))
            {
                ChangeWord = richTextBoxParagraph.SelectedText.Trim();
                DoAction(SpellCheckAction.AddToDictionary);
            }
        }

        private void CheckBoxAutoChangeNamesCheckedChanged(object sender, EventArgs e)
        {
            if (textBoxWord.Text.Length < 2)
            {
                return;
            }

            DoAutoFixNames(textBoxWord.Text, _suggestions);
        }

        private void ListBoxSuggestionsMouseDoubleClick(object sender, MouseEventArgs e)
        {
            ButtonUseSuggestionAlwaysClick(null, null);
        }

        public void DoAction(SpellCheckAction action)
        {
            switch (action)
            {
                case SpellCheckAction.Change:
                    _noOfChangedWords++;
                    _mainWindow.CorrectWord(_prefix + ChangeWord + _postfix, _currentParagraph, _prefix + _currentWord + _postfix, ref _firstChange, _wordsIndex);
                    break;
                case SpellCheckAction.ChangeAll:
                    _noOfChangedWords++;
                    if (!_changeAllDictionary.ContainsKey(_currentWord))
                    {
                        _changeAllDictionary.Add(_currentWord, ChangeWord);
                        _spellCheckWordLists.UseAlwaysListAdd(_currentWord, ChangeWord);
                    }
                    _mainWindow.CorrectWord(_prefix + ChangeWord + _postfix, _currentParagraph, _prefix + _currentWord + _postfix, ref _firstChange, -1);
                    break;
                case SpellCheckAction.Skip:
                    _noOfSkippedWords++;
                    string key = _currentIndex + "-" + _wordsIndex + "-" + _currentWord;
                    if (!_skipOneList.Contains(key))
                    {
                        _skipOneList.Add(key);
                    }

                    break;
                case SpellCheckAction.SkipWholeLine:
                    _wordsIndex = int.MaxValue - 1; // Go to next line
                    break;
                case SpellCheckAction.SkipAll:
                    _noOfSkippedWords++;
                    _skipAllList.Add(ChangeWord.ToUpperInvariant());
                    if (ChangeWord.EndsWith('\'') || ChangeWord.StartsWith('\''))
                    {
                        _skipAllList.Add(ChangeWord.ToUpperInvariant().Trim('\''));
                    }

                    break;
                case SpellCheckAction.AddToDictionary:
                    if (_spellCheckWordLists.AddUserWord(ChangeWord))
                    {
                        _noOfAddedWords++;
                    }

                    break;
                case SpellCheckAction.AddToNames:
                    _spellCheckWordLists.AddName(ChangeWord);
                    if (string.Compare(ChangeWord, _currentWord, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        return; // don't prepare next word if change was more than just casing
                    }

                    if (ChangeWord != _currentWord)
                    {
                        _changeAllDictionary.Add(_currentWord, ChangeWord);
                        _mainWindow.CorrectWord(_prefix + ChangeWord + _postfix, _currentParagraph, _prefix + _currentWord + _postfix, ref _firstChange, -1);
                    }
                    break;
                case SpellCheckAction.AddToNamesOnly:
                    _spellCheckWordLists.AddName(ChangeWord);
                    SetWords(_currentParagraph.Text);
                    break;
                case SpellCheckAction.ChangeWholeText:
                    _mainWindow.ShowStatus(string.Format(LanguageSettings.Current.Main.SpellCheckChangedXToY, _currentParagraph.Text.Replace(Environment.NewLine, " "), ChangeWholeText.Replace(Environment.NewLine, " ")));
                    _currentParagraph.Text = ChangeWholeText;
                    _mainWindow.ChangeWholeTextMainPart(ref _noOfChangedWords, ref _firstChange, _currentIndex, _currentParagraph);
                    _currentIndex--; // re-spell-check current line
                    _wordsIndex = int.MaxValue - 1;
                    break;
                case SpellCheckAction.DeleteLine:
                    _mainWindow.DeleteLine();
                    _currentIndex--; // re-spell-check current line
                    _wordsIndex = int.MaxValue - 1;
                    break;
            }
            labelActionInfo.Text = string.Empty;
            PrepareNextWord();
            CheckActions();
        }

        private void CheckActions()
        {
            if (string.IsNullOrEmpty(_currentAction))
            {
                return;
            }

            if (_currentAction == LanguageSettings.Current.SpellCheck.Change)
            {
                ShowActionInfo(_currentAction, _currentWord + " > " + textBoxWord.Text);
            }
            else if (_currentAction == LanguageSettings.Current.SpellCheck.ChangeAll)
            {
                ShowActionInfo(_currentAction, _currentWord + " > " + textBoxWord.Text);
            }
            else
            {
                ShowActionInfo(_currentAction, textBoxWord.Text);
            }
        }

        private void PrepareNextWord()
        {
            while (true)
            {
                if (_wordsIndex + 1 < _words.Count)
                {
                    _wordsIndex++;
                    _currentSpellCheckWord = _words[_wordsIndex];
                    _currentWord = _currentSpellCheckWord.Text;
                }
                else
                {
                    if (_wordsIndex != int.MaxValue - 1 && _skipOneList.Count > 0)
                    {
                        _skipOneList = new HashSet<string>();
                    }

                    if (_currentIndex + 1 < _subtitle.Paragraphs.Count)
                    {
                        _currentIndex++;
                        _currentParagraph = _subtitle.Paragraphs[_currentIndex];

                        SetWords(_currentParagraph.Text);
                        _wordsIndex = 0;
                        if (_words.Count == 0)
                        {
                            _currentWord = string.Empty;
                        }
                        else
                        {
                            _currentWord = _words[_wordsIndex].Text;
                            _currentSpellCheckWord = _words[_wordsIndex];
                        }
                    }
                    else
                    {
                        ShowEndStatusMessage(LanguageSettings.Current.SpellCheck.SpellCheckCompleted);
                        DialogResult = DialogResult.OK;
                        return;
                    }
                }

                panelBookmark.Hide();
                if (_currentParagraph.Bookmark != null)
                {
                    pictureBoxBookmark.Show();
                }
                else
                {
                    pictureBoxBookmark.Hide();
                }

                int minLength = 2;
                if (Configuration.Settings.Tools.CheckOneLetterWords)
                {
                    minLength = 1;
                }

                if (_currentWord.RemoveControlCharacters().Trim().Length >= minLength)
                {
                    _prefix = string.Empty;
                    _postfix = string.Empty;
                    if (_currentWord.Length > 0)
                    {
                        var trimChars = "'`*#\u200E\u200F\u202A\u202B\u202C\u202D\u202E\u200B\uFEFF";
                        var charHit = true;
                        while (charHit)
                        {
                            charHit = false;
                            foreach (char c in trimChars)
                            {
                                if (_currentWord.StartsWith(c))
                                {
                                    _prefix += c;
                                    _currentWord = _currentWord.Substring(1);
                                    charHit = true;
                                }
                                if (_currentWord.EndsWith(c))
                                {
                                    _postfix = c + _postfix;
                                    _currentWord = _currentWord.Remove(_currentWord.Length - 1);
                                    charHit = true;
                                }
                            }
                        }
                    }
                    string key = _currentIndex + "-" + _wordsIndex + "-" + _currentWord;
                    if (_currentWord.Length < minLength || _currentWord == "&")
                    {
                        // ignore short/empty words and special chars
                    }
                    else if (IsBetweenActiveAssaTags(_words[_wordsIndex].Index, _currentParagraph, _subtitleFormat))

                    {
                        // ignore words between {} in ASSA/SSA
                    }
                    else if (_spellCheckWordLists.HasName(_currentWord))
                    {
                        _noOfNames++;
                    }
                    else if (_skipAllList.Contains(_currentWord.ToUpperInvariant())
                        || (_currentWord.StartsWith('\'') || _currentWord.EndsWith('\'')) && _skipAllList.Contains(_currentWord.Trim('\'').ToUpperInvariant()))
                    {
                        _noOfSkippedWords++;
                    }
                    else if (_skipOneList.Contains(key))
                    {
                        // "skip one" again (after change whole text)
                    }
                    else if (Utilities.IsNumber(_currentWord))
                    {
                        _noOfSkippedWords++;
                    }
                    else if (_spellCheckWordLists.HasUserWord(_currentWord))
                    {
                        _noOfCorrectWords++;
                    }
                    else if (_postfix.Length > 0 && _spellCheckWordLists.HasUserWord(_currentWord + _postfix))
                    {
                        _noOfCorrectWords++;
                    }
                    else if (_changeAllDictionary.ContainsKey(_currentWord) && NotSameSpecialEnding(_currentSpellCheckWord, _changeAllDictionary[_currentWord]))
                    {
                        _noOfChangedWords++;
                        _mainWindow.CorrectWord(_changeAllDictionary[_currentWord], _currentParagraph, _currentWord, ref _firstChange, -1);
                    }
                    else if (_currentWord.EndsWith('\'') && _changeAllDictionary.ContainsKey(_currentWord.Trim('\'')))
                    {
                        _noOfChangedWords++;
                        _mainWindow.CorrectWord(_changeAllDictionary[_currentWord.Trim('\'')], _currentParagraph, _currentWord.Trim('\''), ref _firstChange, -1);
                    }
                    else if (_spellCheckWordLists.HasNameExtended(_currentWord, _currentParagraph.Text)) // TODO: Verify this!
                    {
                        _noOfNames++;
                    }
                    else if (_spellCheckWordLists.IsWordInUserPhrases(_wordsIndex, _words))
                    {
                        _noOfCorrectWords++;
                    }
                    else
                    {
                        bool correct;

                        if (_prefix == "'" && _currentWord.Length >= 1 && (DoSpell(_prefix + _currentWord) || _spellCheckWordLists.HasUserWord(_prefix + _currentWord)))
                        {
                            correct = true;
                        }
                        else if (_currentWord.Length > 1)
                        {
                            correct = DoSpell(_currentWord);
                            if (!correct && "`'".Contains(_currentWord[_currentWord.Length - 1]))
                            {
                                correct = DoSpell(_currentWord.TrimEnd('\'').TrimEnd('`'));
                            }

                            if (!correct && _currentWord.EndsWith("'s", StringComparison.Ordinal) && _currentWord.Length > 4)
                            {
                                correct = DoSpell(_currentWord.TrimEnd('s').TrimEnd('\''));
                            }

                            if (!correct && _currentWord.EndsWith('\'') && DoSpell(_currentWord.TrimEnd('\'')))
                            {
                                _currentWord = _currentWord.TrimEnd('\'');
                                correct = true;
                            }

                            if (!correct)
                            {
                                string removeUnicode = _currentWord.Replace("\u200b", string.Empty); // zero width space
                                removeUnicode = removeUnicode.Replace("\u2060", string.Empty); // word joiner
                                removeUnicode = removeUnicode.Replace("\ufeff", string.Empty); // zero width no-break space
                                correct = DoSpell(removeUnicode);
                            }

                            if (!correct && _wordsIndex > 1 && _words.Count > _wordsIndex &&
                                _words[_wordsIndex - 1].Text.ToLowerInvariant() == "www" &&
                                (_words[_wordsIndex + 1].Text.ToLowerInvariant() == "com" ||
                                 _words[_wordsIndex + 1].Text.ToLowerInvariant() == "org" ||
                                 _words[_wordsIndex + 1].Text.ToLowerInvariant() == "net") &&
                                _currentParagraph.Text.IndexOf(_words[_wordsIndex - 1].Text + "." +
                                                               _currentWord + "." +
                                                               _words[_wordsIndex + 1].Text, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                correct = true; // do not spell check urls
                            }

                            if (!correct && (_languageName.StartsWith("ar_", StringComparison.Ordinal) || _languageName == "ar"))
                            {
                                var trimmed = _currentWord.Trim('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ',', '،');
                                if (trimmed != _currentWord)
                                {
                                    if (_spellCheckWordLists.HasName(trimmed))
                                    {
                                        _noOfNames++;
                                        correct = true;
                                    }

                                    if (!correct && (_skipAllList.Contains(trimmed.ToUpperInvariant()) || _spellCheckWordLists.HasUserWord(trimmed) || DoSpell(trimmed)))
                                    {
                                        correct = true;
                                    }
                                }
                            }

                            // check if dash concatenated word with previous or next word is in spell check dictionary
                            if (!correct && _wordsIndex > 0 && (_currentParagraph.Text[_currentSpellCheckWord.Index - 1] == '-' || _currentParagraph.Text[_currentSpellCheckWord.Index - 1] == '‑'))
                            {
                                var wordWithDash = _words[_wordsIndex - 1].Text + "-" + _currentWord;
                                correct = DoSpell(wordWithDash);
                                if (!correct)
                                {
                                    wordWithDash = _words[_wordsIndex - 1].Text + "‑" + _currentWord; // non break hyphen
                                    correct = DoSpell(wordWithDash);
                                }
                                if (!correct)
                                {
                                    correct = _spellCheckWordLists.HasUserWord(wordWithDash);
                                }
                                if (!correct)
                                {
                                    correct = _spellCheckWordLists.HasUserWord(wordWithDash.Replace("‑", "-"));
                                }

                                if (!correct && _spellCheckWordLists.HasUserWord("-" + _currentWord))
                                {
                                    correct = true;
                                }
                            }
                            if (!correct && _wordsIndex < _words.Count - 1 && _words[_wordsIndex + 1].Index - 1 < _currentParagraph.Text.Length &&
                                (_currentParagraph.Text[_words[_wordsIndex + 1].Index - 1] == '-' || _currentParagraph.Text[_words[_wordsIndex + 1].Index - 1] == '‑'))
                            {
                                var wordWithDash = _currentWord + "-" + _words[_wordsIndex + 1].Text;
                                correct = DoSpell(wordWithDash);
                                if (!correct)
                                {
                                    wordWithDash = _currentWord + "‑" + _words[_wordsIndex + 1].Text; // non break hyphen
                                    correct = DoSpell(wordWithDash);
                                }
                                if (!correct)
                                {
                                    correct = _spellCheckWordLists.HasUserWord(wordWithDash.Replace("‑", "-"));
                                }
                                if (!correct && _spellCheckWordLists.HasName(wordWithDash.Replace("‑", "-")))
                                {
                                    correct = true;
                                    _noOfNames++;
                                }
                            }
                            if (!correct && _currentWord.EndsWith('\u2014')) // em dash
                            {
                                var wordWithoutDash = _currentWord.TrimEnd('\u2014');
                                correct = DoSpell(wordWithoutDash);
                                if (!correct)
                                {
                                    correct = _spellCheckWordLists.HasUserWord(wordWithoutDash);
                                }
                                if (!correct && _spellCheckWordLists.HasName(wordWithoutDash))
                                {
                                    correct = true;
                                    _noOfNames++;
                                }
                            }

                            if (!correct && _wordsIndex > 0) // check name concat with previous word
                            {
                                var wordConCat = _words[_wordsIndex - 1].Text +  " " + _currentWord;
                                if (_spellCheckWordLists.HasNameExtended(wordConCat, _currentParagraph.Text))
                                {
                                    correct = true;
                                }
                            }
                        }
                        else
                        {
                            correct = false;
                            if (_currentWord == "'")
                            {
                                correct = true;
                            }
                            else if (_languageName.StartsWith("en", StringComparison.Ordinal) && (_currentWord.Equals("a", StringComparison.OrdinalIgnoreCase) || _currentWord == "I"))
                            {
                                correct = true;
                            }
                            else if (_languageName.StartsWith("da", StringComparison.Ordinal) && _currentWord.Equals("i", StringComparison.OrdinalIgnoreCase))
                            {
                                correct = true;
                            }
                            else if (_languageName.StartsWith("fr", StringComparison.Ordinal))
                            {
                                if (_currentWord.Equals("a", StringComparison.OrdinalIgnoreCase) ||
                                    _currentWord.Equals("à", StringComparison.OrdinalIgnoreCase) ||
                                    _currentWord.Equals("y", StringComparison.OrdinalIgnoreCase))
                                {
                                    correct = true;
                                }
                            }
                            else if (_languageName.StartsWith("es", StringComparison.Ordinal))
                            {
                                if (_currentWord.Equals("a", StringComparison.OrdinalIgnoreCase) ||
                                    _currentWord.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                                    _currentWord.Equals("o", StringComparison.OrdinalIgnoreCase) ||
                                    _currentWord.Equals("u", StringComparison.OrdinalIgnoreCase))
                                {
                                    correct = true;
                                }
                            }
                        }

                        if (!correct && Configuration.Settings.Tools.SpellCheckEnglishAllowInQuoteAsIng &&
                            _languageName.StartsWith("en_", StringComparison.Ordinal) && _words[_wordsIndex].Text.EndsWith("in'", StringComparison.OrdinalIgnoreCase))
                        {
                            correct = DoSpell(_currentWord.TrimEnd('\'') + "g");
                        }

                        if (correct)
                        {
                            _noOfCorrectWords++;
                        }
                        else
                        {
                            _mainWindow.FocusParagraph(_currentIndex);

                            var suggestions = new List<string>();

                            if ((_currentWord == "Lt's" || _currentWord == "Lt'S") && _languageName.StartsWith("en_", StringComparison.Ordinal))
                            {
                                suggestions.Add("It's");
                            }
                            else
                            {
                                if (_wordSplitList == null)
                                {
                                    _wordSplitList = LoadWordSplitList(_languageName);
                                }

                                var splitWords = StringWithoutSpaceSplitToWords.SplitWord(_wordSplitList, _currentWord);
                                if (splitWords != _currentWord)
                                {
                                    suggestions.Add(splitWords);
                                }

                                if (_currentWord.ToUpperInvariant() != "LT'S" && _currentWord.ToUpperInvariant() != "SOX'S" && !_currentWord.ToUpperInvariant().StartsWith("HTTP", StringComparison.Ordinal)) // TODO: Get fixed nhunspell
                                {
                                    suggestions.AddRange(DoSuggest(_currentWord)); // TODO: 0.9.6 fails on "Lt'S"
                                }

                                if (_languageName.StartsWith("fr_", StringComparison.Ordinal) && (_currentWord.StartsWith("I'", StringComparison.Ordinal) || _currentWord.StartsWith("I’", StringComparison.Ordinal)))
                                {
                                    if (_currentWord.Length > 3 && char.IsLower(_currentWord[2]) && _currentSpellCheckWord.Index > 3)
                                    {
                                        string ending = _currentParagraph.Text.Substring(0, _currentSpellCheckWord.Index - 1).Trim();
                                        if (ending.Length > 1 && !".!?".Contains(ending[ending.Length - 1]))
                                        {
                                            for (int i = 0; i < suggestions.Count; i++)
                                            {
                                                if (suggestions[i].StartsWith("L'", StringComparison.Ordinal) || suggestions[i].StartsWith("L’", StringComparison.Ordinal))
                                                {
                                                    suggestions[i] = @"l" + suggestions[i].Substring(1);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            suggestions.Remove(_currentWord);
                            if (_currentWord.Length == 1 && _currentWord == "L" && _languageName.StartsWith("en_", StringComparison.Ordinal))
                            {
                                suggestions.Remove("I");
                                suggestions.Insert(0, "I");
                            }

                            suggestions = suggestions.Distinct().ToList();

                            if (DoAutoFixNames(_currentWord, suggestions))
                            {
                                return;
                            }

                            if (_prefix != null && _prefix == "''" && _currentWord.EndsWith("''", StringComparison.Ordinal))
                            {
                                _prefix = string.Empty;
                                _currentSpellCheckWord.Index += 2;
                                _currentWord = _currentWord.Trim('\'');
                            }
                            if (_prefix != null && _prefix == "'" && _currentWord.EndsWith('\''))
                            {
                                _prefix = string.Empty;
                                _currentSpellCheckWord.Index++;
                                _currentWord = _currentWord.Trim('\'');
                            }

                            if (_postfix != null && _postfix == "'")
                            {
                                _currentSpellCheckWord.Text = _currentWord + _postfix;
                                Initialize(_languageName, _currentSpellCheckWord, suggestions, _currentParagraph.Text, string.Format(LanguageSettings.Current.Main.LineXOfY, (_currentIndex + 1), _subtitle.Paragraphs.Count));
                            }
                            else
                            {
                                _currentSpellCheckWord.Text = _currentWord;
                                Initialize(_languageName, _currentSpellCheckWord, suggestions, _currentParagraph.Text, string.Format(LanguageSettings.Current.Main.LineXOfY, (_currentIndex + 1), _subtitle.Paragraphs.Count));
                            }
                            if (!Visible)
                            {
                                ShowDialog(_mainWindow);
                            }

                            return; // wait for user input
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Do not allow changing "Who is lookin' at X" with "lokin" word to "lokin'" via repalce word.
        /// </summary>
        private bool NotSameSpecialEnding(SpellCheckWord spellCheckWord, string replaceWord)
        {
            if (spellCheckWord.Index + spellCheckWord.Length + 1 >= _currentParagraph.Text.Length)
            {
                return true;
            }

            var wordPlusOne = _currentParagraph.Text.Substring(spellCheckWord.Index, spellCheckWord.Length + 1).TrimStart();
            if (replaceWord.EndsWith('\'') && !replaceWord.EndsWith("''", StringComparison.Ordinal) && wordPlusOne == replaceWord)
            {
                return false;
            }
            else if (replaceWord.EndsWith('"') && !replaceWord.EndsWith("\"\"", StringComparison.Ordinal) && wordPlusOne == replaceWord)
            {
                return false;
            }

            return true;
        }

        private static bool IsBetweenActiveAssaTags(int currentIndex, Paragraph currentParagraph, SubtitleFormat subtitleFormat)
        {
            if (subtitleFormat.Name != AdvancedSubStationAlpha.NameOfFormat &&
                subtitleFormat.Name != SubStationAlpha.NameOfFormat)
            {
                return false;
            }

            var s = currentParagraph.Text.Substring(0, currentIndex);
            var lastIndexOfStart = s.LastIndexOf('{');
            var lastIndexOfEnd = s.LastIndexOf('}');
            return lastIndexOfStart > lastIndexOfEnd;
        }

        private bool DoAutoFixNames(string word, List<string> suggestions)
        {
            if (AutoFixNames && word.Length > 3)
            {
                if (Configuration.Settings.Tools.SpellCheckAutoChangeNamesUseSuggestions)
                {
                    if (suggestions.Contains(word.ToUpperInvariant()))
                    { // does not work well with two letter words like "da" and "de" which get auto-corrected to "DA" and "DE"
                        ChangeWord = word.ToUpperInvariant();
                        DoAction(SpellCheckAction.ChangeAll);
                        return true;
                    }

                    if (suggestions.Contains(char.ToUpperInvariant(word[0]) + word.Substring(1)))
                    {
                        ChangeWord = char.ToUpperInvariant(word[0]) + word.Substring(1);
                        DoAction(SpellCheckAction.ChangeAll);
                        return true;
                    }
                }

                if (_spellCheckWordLists.HasName(char.ToUpper(word[0]) + word.Substring(1)))
                {
                    ChangeWord = char.ToUpper(word[0]) + word.Substring(1);
                    DoAction(SpellCheckAction.ChangeAll);
                    return true;
                }

                if (word.StartsWith("mc", StringComparison.InvariantCultureIgnoreCase) && _spellCheckWordLists.HasName(char.ToUpper(word[0]) + word.Substring(1, 1) + char.ToUpper(word[2]) + word.Remove(0, 3)))
                {
                    ChangeWord = char.ToUpper(word[0]) + word.Substring(1, 1) + char.ToUpper(word[2]) + word.Remove(0, 3);
                    DoAction(SpellCheckAction.ChangeAll);
                    return true;
                }

                if (_spellCheckWordLists.HasName(word.ToUpperInvariant()))
                {
                    ChangeWord = word.ToUpperInvariant();
                    DoAction(SpellCheckAction.ChangeAll);
                    return true;
                }
            }
            return false;
        }

        private void SetWords(string s)
        {
            s = _spellCheckWordLists.ReplaceHtmlTagsWithBlanks(s);
            s = _spellCheckWordLists.ReplaceAssTagsWithBlanks(s);
            s = _spellCheckWordLists.ReplaceKnownWordsOrNamesWithBlanks(s);
            _words = SpellCheckWordLists.Split(s);
        }

        private void ShowEndStatusMessage(string completedMessage)
        {
            LanguageStructure.Main mainLanguage = LanguageSettings.Current.Main;
            if (_noOfChangedWords > 0 || _noOfAddedWords > 0 || _noOfSkippedWords > 0 || completedMessage == LanguageSettings.Current.SpellCheck.SpellCheckCompleted)
            {
                Hide();
                if (Configuration.Settings.Tools.SpellCheckShowCompletedMessage)
                {
                    var form = new DialogDoNotShowAgain(_mainWindow.Title + " - " + mainLanguage.SpellCheck,
                                    completedMessage + Environment.NewLine +
                                    Environment.NewLine +
                                    string.Format(mainLanguage.NumberOfCorrectedWords, _noOfChangedWords) + Environment.NewLine +
                                    string.Format(mainLanguage.NumberOfSkippedWords, _noOfSkippedWords) + Environment.NewLine +
                                    string.Format(mainLanguage.NumberOfCorrectWords, _noOfCorrectWords) + Environment.NewLine +
                                    string.Format(mainLanguage.NumberOfWordsAddedToDictionary, _noOfAddedWords) + Environment.NewLine +
                                    string.Format(mainLanguage.NumberOfNameHits, _noOfNames));
                    form.ShowDialog(_mainWindow);
                    Configuration.Settings.Tools.SpellCheckShowCompletedMessage = !form.DoNoDisplayAgain;
                    form.Dispose();
                }
                else
                {
                    if (_noOfChangedWords > 0)
                    {
                        _mainWindow.ShowStatus(completedMessage + "  " + string.Format(mainLanguage.NumberOfCorrectedWords, _noOfChangedWords));
                    }
                    else
                    {
                        _mainWindow.ShowStatus(completedMessage);
                    }
                }
            }
        }

        public void ContinueSpellCheck(Subtitle subtitle)
        {
            _subtitle = subtitle;

            buttonUndo.Visible = false;
            _undoList = new List<UndoObject>();

            if (_currentIndex >= subtitle.Paragraphs.Count)
            {
                _currentIndex = 0;
            }

            _currentParagraph = _subtitle.GetParagraphOrDefault(_currentIndex);
            if (_currentParagraph == null)
            {
                return;
            }

            SetWords(_currentParagraph.Text);
            _wordsIndex = -1;

            PrepareNextWord();
        }

        public void DoSpellCheck(bool autoDetect, Subtitle subtitle, string dictionaryFolder, Main mainWindow, int startLine)
        {
            _subtitle = subtitle;
            LanguageStructure.Main mainLanguage = LanguageSettings.Current.Main;
            _mainWindow = mainWindow;

            _skipAllList = new List<string>();

            _noOfSkippedWords = 0;
            _noOfChangedWords = 0;
            _noOfCorrectWords = 0;
            _noOfNames = 0;
            _noOfAddedWords = 0;
            _firstChange = true;

            if (!string.IsNullOrEmpty(Configuration.Settings.General.SpellCheckLanguage) && File.Exists(Path.Combine(dictionaryFolder, Configuration.Settings.General.SpellCheckLanguage + ".dic")))
            {
                _languageName = Configuration.Settings.General.SpellCheckLanguage;
            }
            else
            {
                string name = Utilities.GetDictionaryLanguages()[0];
                int start = name.LastIndexOf('[');
                int end = name.LastIndexOf(']');
                if (start > 0 && end > start)
                {
                    start++;
                    name = name.Substring(start, end - start);
                    _languageName = name;
                }
                else
                {
                    MessageBox.Show(string.Format(mainLanguage.InvalidLanguageNameX, name));
                    return;
                }
            }
            if (autoDetect || string.IsNullOrEmpty(_languageName))
            {
                _languageName = LanguageAutoDetect.AutoDetectLanguageName(_languageName, subtitle);
                if (_languageName != null && _languageName.Length > 3)
                {
                    string start = _languageName.Substring(0, 2);
                    if (_languageName.StartsWith(start, StringComparison.Ordinal) && Configuration.Settings.General.SpellCheckLanguage != null &&
                        Configuration.Settings.General.SpellCheckLanguage.StartsWith(start, StringComparison.Ordinal) && _languageName != Configuration.Settings.General.SpellCheckLanguage)
                    {
                        foreach (var dictionaryName in Utilities.GetDictionaryLanguages())
                        {
                            if (dictionaryName.Contains(Configuration.Settings.General.SpellCheckLanguage))
                            {
                                _languageName = Configuration.Settings.General.SpellCheckLanguage;
                                break;
                            }
                        }
                    }
                }
            }
            string dictionary = Utilities.DictionaryFolder + _languageName;

            LoadDictionaries(dictionaryFolder, dictionary, _languageName);

            _currentIndex = 0;
            if (startLine >= 0 && startLine < _subtitle.Paragraphs.Count)
            {
                _currentIndex = startLine;
            }

            _currentParagraph = _subtitle.Paragraphs[_currentIndex];
            SetWords(_currentParagraph.Text);
            _wordsIndex = -1;

            PrepareNextWord();
        }

        private void LoadDictionaries(string dictionaryFolder, string dictionary, string languageName)
        {
            _skipAllList = new List<string>();
            _spellCheckWordLists = new SpellCheckWordLists(dictionaryFolder, languageName, this);
            _changeAllDictionary = _spellCheckWordLists.GetUseAlwaysList();
            LoadHunspell(dictionary);
        }

        private void textBoxWord_TextChanged(object sender, EventArgs e)
        {
            buttonChange.Enabled = textBoxWord.Text != _originalWord;
            buttonChangeAll.Enabled = buttonChange.Enabled;
        }

        private void buttonAddToDictionary_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(LanguageSettings.Current.SpellCheck.AddToUserDictionary, textBoxWord.Text);
        }

        private void ShowActionInfo(string label, string text)
        {
            labelActionInfo.Text = $"{label}: {text.Trim()}";
            _currentAction = label;
        }

        private void buttonAddToDictionary_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonAddToNames_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(LanguageSettings.Current.SpellCheck.AddToNamesAndIgnoreList, textBoxWord.Text);
        }

        private void buttonAddToNames_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonSkipOnce_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(LanguageSettings.Current.SpellCheck.SkipOnce, textBoxWord.Text);
        }

        private void buttonSkipOnce_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonSkipAll_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(LanguageSettings.Current.SpellCheck.SkipAll, textBoxWord.Text);
        }

        private void buttonSkipAll_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonChange_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(LanguageSettings.Current.SpellCheck.Change, _currentWord + " > " + textBoxWord.Text);
        }

        private void buttonChange_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonChangeAll_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(LanguageSettings.Current.SpellCheck.ChangeAll, _currentWord + " > " + textBoxWord.Text);
        }

        private void buttonChangeAll_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonSpellCheckDownload_Click(object sender, EventArgs e)
        {
            using (var gd = new GetDictionaries())
            {
                gd.ShowDialog(this);

                FillSpellCheckDictionaries(LanguageAutoDetect.AutoDetectLanguageName(null, _subtitle));
                if (gd.LastDownload != null && gd.LastDownload.Length > 3 && comboBoxDictionaries.Items.Count > 0)
                {
                    var lc = Path.GetFileNameWithoutExtension(gd.LastDownload.Substring(0, 4).Replace('_', '-'));
                    for (int i = 0; i < comboBoxDictionaries.Items.Count; i++)
                    {
                        string item = (string)comboBoxDictionaries.Items[i];
                        if (item.Contains("[" + lc) || item.Contains(gd.SelectedEnglishName))
                        {
                            comboBoxDictionaries.SelectedIndex = i;
                            break;
                        }
                    }
                }
                if (comboBoxDictionaries.Items.Count > 0 && comboBoxDictionaries.SelectedIndex < 0)
                {
                    comboBoxDictionaries.SelectedIndex = 0;
                }
                ComboBoxDictionariesSelectedIndexChanged(null, null);
            }
        }

        private void PushUndo(string text, SpellCheckAction action)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (action == SpellCheckAction.ChangeAll && _changeAllDictionary.ContainsKey(_currentWord))
            {
                return;
            }

            string format = LanguageSettings.Current.SpellCheck.UndoX;
            string undoText = string.Format(format, text.RemoveChar('&'));

            _undoList.Add(new UndoObject
            {
                CurrentIndex = _currentIndex,
                UndoText = undoText,
                UndoWord = textBoxWord.Text.Trim(),
                Action = action,
                CurrentWord = _currentWord,
                Subtitle = new Subtitle(_subtitle),
                NoOfSkippedWords = _noOfSkippedWords,
                NoOfChangedWords = _noOfChangedWords,
                NoOfCorrectWords = _noOfCorrectWords,
                NoOfNames = _noOfNames,
                NoOfAddedWords = _noOfAddedWords,
            });
            buttonUndo.Text = undoText;
            buttonUndo.Visible = true;
        }

        private void buttonUndo_Click(object sender, EventArgs e)
        {
            if (_undoList.Count > 0)
            {
                var undo = _undoList[_undoList.Count - 1];
                _currentIndex = undo.CurrentIndex - 1;
                _wordsIndex = int.MaxValue - 1;
                _noOfSkippedWords = undo.NoOfSkippedWords;
                _noOfChangedWords = undo.NoOfChangedWords;
                _noOfCorrectWords = undo.NoOfCorrectWords;
                _noOfNames = undo.NoOfNames;
                _noOfAddedWords = undo.NoOfAddedWords;

                switch (undo.Action)
                {
                    case SpellCheckAction.Change:
                        _subtitle = _mainWindow.UndoFromSpellCheck(undo.Subtitle);
                        break;
                    case SpellCheckAction.ChangeAll:
                        _subtitle = _mainWindow.UndoFromSpellCheck(undo.Subtitle);
                        _changeAllDictionary.Remove(undo.CurrentWord);
                        _spellCheckWordLists.UseAlwaysListRemove(undo.CurrentWord);
                        break;
                    case SpellCheckAction.Skip:
                        break;
                    case SpellCheckAction.SkipAll:
                        _skipAllList.Remove(undo.UndoWord.ToUpperInvariant());
                        if (undo.UndoWord.EndsWith('\'') || undo.UndoWord.StartsWith('\''))
                        {
                            _skipAllList.Remove(undo.UndoWord.ToUpperInvariant().Trim('\''));
                        }

                        break;
                    case SpellCheckAction.AddToDictionary:
                        _spellCheckWordLists.RemoveUserWord(undo.UndoWord);
                        break;
                    case SpellCheckAction.AddToNames:
                        _spellCheckWordLists.RemoveName(undo.UndoWord);
                        break;
                    case SpellCheckAction.ChangeWholeText:
                        _subtitle = _mainWindow.UndoFromSpellCheck(undo.Subtitle);
                        break;
                }

                _undoList.RemoveAt(_undoList.Count - 1);
                if (_undoList.Count > 0)
                {
                    buttonUndo.Text = _undoList[_undoList.Count - 1].UndoText;
                }
                else
                {
                    buttonUndo.Visible = false;
                }
            }
            PrepareNextWord();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                if (_hunspell != null)
                {
                    _hunspell.Dispose();
                    _hunspell = null;
                }
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void buttonGoogleIt_Click(object sender, EventArgs e)
        {
            var q = textBoxWord.Text.Trim();
            if (string.IsNullOrWhiteSpace(q))
            {
                return;
            }

            var url = "https://www.google.com/search?q={0}";

            if (Configuration.Settings.VideoControls.CustomSearchText1 == buttonGoogleIt.Text)
            {
                url = Configuration.Settings.VideoControls.CustomSearchUrl1;
            }
            else if (Configuration.Settings.VideoControls.CustomSearchText2 == buttonGoogleIt.Text)
            {
                url = Configuration.Settings.VideoControls.CustomSearchUrl2;
            }
            else if (Configuration.Settings.VideoControls.CustomSearchText3 == buttonGoogleIt.Text)
            {
                url = Configuration.Settings.VideoControls.CustomSearchUrl3;
            }
            else if (Configuration.Settings.VideoControls.CustomSearchText4 == buttonGoogleIt.Text)
            {
                url = Configuration.Settings.VideoControls.CustomSearchUrl4;
            }
            else if (Configuration.Settings.VideoControls.CustomSearchText5 == buttonGoogleIt.Text)
            {
                url = Configuration.Settings.VideoControls.CustomSearchUrl5;
            }

            try
            {
                UiUtil.OpenUrl(string.Format(url, q));
            }
            catch (Exception exception)
            {
                MessageBox.Show("Invalid search url: " + exception.Message);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Configuration.Settings.General.PromptDeleteLines || MessageBox.Show(LanguageSettings.Current.Main.DeleteOneLinePrompt, LanguageSettings.Current.SpellCheck.Title, MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
            {
                DoAction(SpellCheckAction.DeleteLine);
            }
        }

        private void openImagedBasedSourceFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenImageBasedSourceFile();
        }

        private void pictureBoxBookmark_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (panelBookmark.Visible)
                {
                    panelBookmark.Hide();
                }
                else
                {
                    var p = _subtitle.GetParagraphOrDefault(_currentIndex);
                    if (p != null && !string.IsNullOrEmpty(p.Bookmark))
                    {
                        labelBookmark.Text = p.Bookmark;
                        labelBookmark.Show();
                        try
                        {
                            using (var graphics = CreateGraphics())
                            {
                                var textSize = graphics.MeasureString(p.Bookmark, Font);
                                labelBookmark.Text = p.Bookmark;
                                panelBookmark.Left = pictureBoxBookmark.Right + 9;
                                panelBookmark.Top = pictureBoxBookmark.Top - 4;
                                panelBookmark.Width = (int)textSize.Width + 20;
                                panelBookmark.Height = (int)textSize.Height + 20;
                                panelBookmark.Show();
                            }
                        }
                        catch
                        {
                            // ignore
                            panelBookmark.Show();
                        }
                    }
                }
            }
        }

        private void pictureBoxBookmark_MouseEnter(object sender, EventArgs e)
        {
            if (_bookmarkContextMenu != null)
            {
                return;
            }

            _bookmarkContextMenu = MakeContextMenu(_currentIndex);
            pictureBoxBookmark.ContextMenuStrip = _bookmarkContextMenu;
        }

        public ContextMenuStrip MakeContextMenu(int index)
        {
            var bookmarkContextMenu = new ContextMenuStrip();

            // edit bookmark
            var menuItem = new ToolStripMenuItem(LanguageSettings.Current.Main.Menu.ContextMenu.EditBookmark);
            menuItem.Click += (sender2, e2) => { _mainForm.EditBookmark(index, this); };
            bookmarkContextMenu.Items.Add(menuItem);

            // remove bookmark
            menuItem = new ToolStripMenuItem(LanguageSettings.Current.Main.Menu.ContextMenu.RemoveBookmark);
            menuItem.Click += (sender2, e2) =>
            {
                _mainForm.RemoveBookmark(index);
                pictureBoxBookmark.Hide();
            };
            bookmarkContextMenu.Items.Add(menuItem);

            UiUtil.FixFonts(bookmarkContextMenu);
            return bookmarkContextMenu;
        }

        private void bookmarkCommentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var p = _subtitle.GetParagraphOrDefault(_currentIndex);
            if (p == null)
            {
                return;
            }

            _mainForm.ToggleBookmarks(string.IsNullOrEmpty(p.Bookmark), this);
            if (p.Bookmark != null)
            {
                pictureBoxBookmark.Show();
                labelBookmark.Text = p.Bookmark;
            }
            else
            {
                pictureBoxBookmark.Hide();
                labelBookmark.Hide();
            }
        }

        private void ToggleBookmark()
        {
            var p = _subtitle.GetParagraphOrDefault(_currentIndex);
            if (p == null)
            {
                return;
            }

            _mainForm.ToggleBookmarks(false, this);
            if (p.Bookmark != null)
            {
                pictureBoxBookmark.Show();
            }
            else
            {
                pictureBoxBookmark.Hide();
            }
        }

        private void useLargerFontForThisWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Font = useLargerFontForThisWindowToolStripMenuItem.Checked ? new Font(Font.FontFamily, Font.Size - 2, FontStyle.Regular) : new Font(Font.FontFamily, Font.Size + 2, FontStyle.Regular);
            useLargerFontForThisWindowToolStripMenuItem.Checked = !useLargerFontForThisWindowToolStripMenuItem.Checked;
            useLargerFontForThisWindowToolStripMenuItem1.Checked = useLargerFontForThisWindowToolStripMenuItem.Checked;
            Configuration.Settings.Tools.SpellCheckUseLargerFont = useLargerFontForThisWindowToolStripMenuItem.Checked;
        }
    }
}
