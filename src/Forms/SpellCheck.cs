using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using Nikse.SubtitleEdit.Logic.Enums;
using Nikse.SubtitleEdit.Logic.SpellCheck;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SpellCheck : Form
    {
        private class UndoObject
        {
            public int CurrentIndex { get; set; }
            public string UndoText { get; set; }
            public string UndoWord { get; set; }
            public string CurrentWord { get; set; }
            public SpellCheckAction Action { get; set; }
            public Subtitle Subtitle { get; set; }
            public int NoOfSkippedWords { get; set; }
            public int NoOfChangedWords { get; set; }
            public int NoOfCorrectWords { get; set; }
            public int NoOfNamesEtc { get; set; }
            public int NoOfAddedWords { get; set; }
        }
        private List<UndoObject> _undoList = new List<UndoObject>();

        private SpellCheckAction _action = SpellCheckAction.Skip;
        private List<string> _suggestions;
        private string _currentAction = null;
        public SpellCheckAction Action { get { return _action; } set { _action = value; } }
        public string ChangeWord { get { return textBoxWord.Text; } set { textBoxWord.Text = value; } }
        public string ChangeWholeText { get { return textBoxWholeText.Text; } }
        public bool AutoFixNames { get { return checkBoxAutoChangeNames.Checked; } }
        public int CurrentLineIndex
        {
            get { return _currentIndex; }
        }

        private List<string> _namesEtcList = new List<string>();
        private List<string> _namesEtcMultiWordList = new List<string>();
        private List<string> _namesEtcListUppercase = new List<string>();
        private List<string> _namesEtcListWithApostrophe = new List<string>();
        private List<string> _skipAllList = new List<string>();
        private List<string> _wordsWithDashesOrPeriods = new List<string>();

        private Dictionary<string, string> _changeAllDictionary = new Dictionary<string, string>();
        private List<string> _userWordList = new List<string>();
        private List<string> _userPhraseList = new List<string>();
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

        private int _noOfSkippedWords = 0;
        private int _noOfChangedWords = 0;
        private int _noOfCorrectWords = 0;
        private int _noOfNamesEtc = 0;
        private int _noOfAddedWords = 0;
        private bool _firstChange = true;
        private string _languageName;
        private Main _mainWindow;

        private string _currentDictionary = null;

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
                string name = comboBoxDictionaries.SelectedItem.ToString();
                int start = name.LastIndexOf('[');
                int end = name.LastIndexOf(']');
                if (start >= 0 && end > start)
                {
                    start++;
                    name = name.Substring(start, end - start);
                    return name;
                }
                return null;
            }
        }

        public SpellCheck()
        {
            InitializeComponent();
            labelActionInfo.Text = string.Empty;
            Text = Configuration.Settings.Language.SpellCheck.Title;
            labelFullText.Text = Configuration.Settings.Language.SpellCheck.FullText;
            labelLanguage.Text = Configuration.Settings.Language.SpellCheck.Language;
            groupBoxWordNotFound.Text = Configuration.Settings.Language.SpellCheck.WordNotFound;
            buttonAddToDictionary.Text = Configuration.Settings.Language.SpellCheck.AddToUserDictionary;
            buttonChange.Text = Configuration.Settings.Language.SpellCheck.Change;
            buttonChangeAll.Text = Configuration.Settings.Language.SpellCheck.ChangeAll;
            buttonSkipAll.Text = Configuration.Settings.Language.SpellCheck.SkipAll;
            buttonSkipOnce.Text = Configuration.Settings.Language.SpellCheck.SkipOnce;
            buttonUseSuggestion.Text = Configuration.Settings.Language.SpellCheck.Use;
            buttonUseSuggestionAlways.Text = Configuration.Settings.Language.SpellCheck.UseAlways;
            buttonAbort.Text = Configuration.Settings.Language.SpellCheck.Abort;
            buttonEditWholeText.Text = Configuration.Settings.Language.SpellCheck.EditWholeText;
            checkBoxAutoChangeNames.Text = Configuration.Settings.Language.SpellCheck.AutoFixNames;
            checkBoxAutoChangeNames.Checked = Configuration.Settings.Tools.SpellCheckAutoChangeNames;
            groupBoxEditWholeText.Text = Configuration.Settings.Language.SpellCheck.EditWholeText;
            buttonChangeWholeText.Text = Configuration.Settings.Language.SpellCheck.Change;
            buttonSkipText.Text = Configuration.Settings.Language.SpellCheck.SkipOnce;
            groupBoxSuggestions.Text = Configuration.Settings.Language.SpellCheck.Suggestions;
            buttonAddToNames.Text = Configuration.Settings.Language.SpellCheck.AddToNamesAndIgnoreList;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonAbort.Text, this.Font);
            if (textSize.Height > buttonAbort.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public void Initialize(string languageName, SpellCheckWord word, List<string> suggestions, string paragraph, string progress)
        {
            _originalWord = word.Text;
            _suggestions = suggestions;
            groupBoxWordNotFound.Visible = true;
            groupBoxEditWholeText.Visible = false;
            buttonEditWholeText.Text = Configuration.Settings.Language.SpellCheck.EditWholeText;
            Text = Configuration.Settings.Language.SpellCheck.Title + " [" + languageName + "] - " + progress;
            textBoxWord.Text = word.Text;
            textBoxWholeText.Text = paragraph;
            listBoxSuggestions.Items.Clear();
            foreach (string suggestion in suggestions)
            {
                listBoxSuggestions.Items.Add(suggestion);
            }
            if (listBoxSuggestions.Items.Count > 0)
                listBoxSuggestions.SelectedIndex = 0;

            richTextBoxParagraph.Text = paragraph;

            FillSpellCheckDictionaries(languageName);
            ShowActiveWordWithColor(word);
            _action = SpellCheckAction.Skip;
            DialogResult = DialogResult.None;
        }

        private void FillSpellCheckDictionaries(string languageName)
        {
            comboBoxDictionaries.SelectedIndexChanged -= ComboBoxDictionariesSelectedIndexChanged;
            comboBoxDictionaries.Items.Clear();
            foreach (string name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
                if (name.Contains("[" + languageName + "]"))
                    comboBoxDictionaries.SelectedIndex = comboBoxDictionaries.Items.Count - 1;
            }
            comboBoxDictionaries.SelectedIndexChanged += ComboBoxDictionariesSelectedIndexChanged;
        }

        private void ShowActiveWordWithColor(SpellCheckWord word)
        {
            richTextBoxParagraph.SelectAll();
            richTextBoxParagraph.SelectionColor = Color.Black;
            richTextBoxParagraph.SelectionLength = 0;

            for (int i = 0; i < 10; i++)
            {
                int idx = word.Index - i;
                if (idx >= 0 && idx < richTextBoxParagraph.Text.Length && richTextBoxParagraph.Text.Substring(idx).StartsWith(word.Text))
                {
                    richTextBoxParagraph.SelectionStart = idx;
                    richTextBoxParagraph.SelectionLength = word.Text.Length;
                    richTextBoxParagraph.SelectionColor = Color.Red;
                    break;
                }
                idx = word.Index + i;
                if (idx >= 0 && idx < richTextBoxParagraph.Text.Length && richTextBoxParagraph.Text.Substring(idx).StartsWith(word.Text))
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
                _action = SpellCheckAction.Abort;
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.F1)
            {
                Utilities.ShowHelp("#spellcheck");
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.G)
            {
                e.SuppressKeyPress = true;
                System.Diagnostics.Process.Start("http://www.google.com/search?q=" + Utilities.UrlEncode(textBoxWord.Text));
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Z)
            {
                if (buttonUndo.Visible)
                {
                    buttonUndo_Click(null, null);
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void ButtonAbortClick(object sender, EventArgs e)
        {
            ShowEndStatusMessage(Configuration.Settings.Language.SpellCheck.SpellCheckAborted);
            DialogResult = DialogResult.Abort;
        }

        private void ButtonChangeClick(object sender, EventArgs e)
        {
            PushUndo(string.Format("{0}: {1}", Configuration.Settings.Language.SpellCheck.Change, _currentWord + " > " + textBoxWord.Text), SpellCheckAction.Change);
            DoAction(SpellCheckAction.Change);
        }

        private void ButtonUseSuggestionClick(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex >= 0)
            {
                textBoxWord.Text = listBoxSuggestions.SelectedItem.ToString();
                PushUndo(string.Format("{0}: {1}", Configuration.Settings.Language.SpellCheck.Change, _currentWord + " > " + textBoxWord.Text), SpellCheckAction.Change);
                DoAction(SpellCheckAction.Change);
            }
        }

        private void ButtonSkipAllClick(object sender, EventArgs e)
        {
            PushUndo(string.Format("{0}: {1}", Configuration.Settings.Language.SpellCheck.SkipAll, textBoxWord.Text), SpellCheckAction.SkipAll);
            DoAction(SpellCheckAction.SkipAll);
        }

        private void ButtonSkipOnceClick(object sender, EventArgs e)
        {
            PushUndo(string.Format("{0}: {1}", Configuration.Settings.Language.SpellCheck.SkipOnce, textBoxWord.Text), SpellCheckAction.Skip);
            DoAction(SpellCheckAction.Skip);
        }

        private void ButtonAddToDictionaryClick(object sender, EventArgs e)
        {
            PushUndo(string.Format("{0}: {1}", Configuration.Settings.Language.SpellCheck.AddToUserDictionary, textBoxWord.Text), SpellCheckAction.AddToDictionary);
            DoAction(SpellCheckAction.AddToDictionary);
        }

        private void ComboBoxDictionariesSelectedIndexChanged(object sender, EventArgs e)
        {
            Configuration.Settings.General.SpellCheckLanguage = LanguageString;
            Configuration.Settings.Save();
            _languageName = LanguageString;
            string dictionary = Utilities.DictionaryFolder + _languageName;
            _userWordList = new List<string>();
            _userPhraseList = new List<string>();
            string fileName = Utilities.DictionaryFolder + _languageName + "_user.xml";
            if (File.Exists(fileName))
            {
                try
                {
                    var userWordDictionary = new XmlDocument();
                    userWordDictionary.Load(fileName);
                    foreach (XmlNode node in userWordDictionary.DocumentElement.SelectNodes("word"))
                    {
                        string word = node.InnerText.Trim().ToLower();
                        if (word.Contains(' '))
                            _userPhraseList.Add(word);
                        else
                            _userWordList.Add(word);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Unable to load user dictionary for " + _languageName + " from file: " + fileName + Environment.NewLine +
                        Environment.NewLine + exception.Source + ": " + exception.Message);
                }
            }

            _changeAllDictionary = new Dictionary<string, string>();
            LoadHunspell(dictionary);
            _wordsIndex--;
            PrepareNextWord();
        }

        private void LoadHunspell(string dictionary)
        {
            _currentDictionary = dictionary;
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
            suggestThread.Abort();
            if (!parameter.Success)
                LoadHunspell(_currentDictionary);
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
            PushUndo(string.Format("{0}: {1}", Configuration.Settings.Language.SpellCheck.ChangeAll, _currentWord + " > " + textBoxWord.Text), SpellCheckAction.ChangeAll);
            DoAction(SpellCheckAction.ChangeAll);
        }

        private void ButtonUseSuggestionAlwaysClick(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex >= 0)
            {
                textBoxWord.Text = listBoxSuggestions.SelectedItem.ToString();
                PushUndo(string.Format("{0}: {1}", Configuration.Settings.Language.SpellCheck.ChangeAll, _currentWord + " > " + textBoxWord.Text), SpellCheckAction.ChangeAll);
                DoAction(SpellCheckAction.ChangeAll);
            }
        }

        private void SpellCheck_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.SpellCheckAutoChangeNames = checkBoxAutoChangeNames.Checked;
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult = DialogResult.Abort;
            }
        }

        private void ButtonAddToNamesClick(object sender, EventArgs e)
        {
            PushUndo(string.Format("{0}: {1}", Configuration.Settings.Language.SpellCheck.AddToNamesAndIgnoreList, textBoxWord.Text), SpellCheckAction.AddToNamesEtc);
            DoAction(SpellCheckAction.AddToNamesEtc);
        }

        private void ButtonEditWholeTextClick(object sender, EventArgs e)
        {
            if (groupBoxWordNotFound.Visible)
            {
                groupBoxWordNotFound.Visible = false;
                groupBoxEditWholeText.Visible = true;
                buttonEditWholeText.Text = Configuration.Settings.Language.SpellCheck.EditWordOnly;
                textBoxWholeText.Focus();
            }
            else
            {
                groupBoxWordNotFound.Visible = true;
                groupBoxEditWholeText.Visible = false;
                buttonEditWholeText.Text = Configuration.Settings.Language.SpellCheck.EditWholeText;
                textBoxWord.Focus();
            }
        }

        private void ButtonSkipTextClick(object sender, EventArgs e)
        {
            PushUndo(string.Format("{0}", Configuration.Settings.Language.SpellCheck.SkipOnce), SpellCheckAction.Skip);
            DoAction(SpellCheckAction.Skip);
        }

        private void ButtonChangeWholeTextClick(object sender, EventArgs e)
        {
            PushUndo(string.Format("{0}", Configuration.Settings.Language.SpellCheck.EditWholeText), SpellCheckAction.ChangeWholeText);
            DoAction(SpellCheckAction.ChangeWholeText);
        }

        private void ContextMenuStrip1Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(richTextBoxParagraph.SelectedText))
            {
                string word = richTextBoxParagraph.SelectedText.Trim();
                addXToNamesnoiseListToolStripMenuItem.Text = string.Format(Configuration.Settings.Language.SpellCheck.AddXToNamesEtc, word);
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void AddXToNamesnoiseListToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(richTextBoxParagraph.SelectedText))
            {
                ChangeWord = richTextBoxParagraph.SelectedText.Trim();
                DoAction(SpellCheckAction.AddToNamesEtc);
            }
        }

        private void CheckBoxAutoChangeNamesCheckedChanged(object sender, EventArgs e)
        {
            if (textBoxWord.Text.Length < 2)
                return;

            string s = char.ToUpper(textBoxWord.Text[0]) + textBoxWord.Text.Substring(1);
            if (checkBoxAutoChangeNames.Checked && _suggestions != null && _suggestions.Contains(s))
            {
                ChangeWord = s;
                DoAction(SpellCheckAction.ChangeAll);
            }
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
                    _mainWindow.CorrectWord(_prefix + ChangeWord + _postfix, _currentParagraph, _prefix + _currentWord + _postfix, ref _firstChange);
                    break;
                case SpellCheckAction.ChangeAll:
                    _noOfChangedWords++;
                    if (!_changeAllDictionary.ContainsKey(_currentWord))
                        _changeAllDictionary.Add(_currentWord, ChangeWord);
                    _mainWindow.CorrectWord(_prefix + ChangeWord + _postfix, _currentParagraph, _prefix + _currentWord + _postfix, ref _firstChange);
                    break;
                case SpellCheckAction.Skip:
                    _noOfSkippedWords++;
                    break;
                case SpellCheckAction.SkipAll:
                    _noOfSkippedWords++;
                    _skipAllList.Add(ChangeWord.ToUpper());
                    if (ChangeWord.EndsWith('\'') || ChangeWord.StartsWith('\''))
                        _skipAllList.Add(ChangeWord.ToUpper().Trim('\''));
                    break;
                case SpellCheckAction.AddToDictionary:
                    if (_userWordList.IndexOf(ChangeWord) < 0)
                    {
                        _noOfAddedWords++;
                        string s = ChangeWord.Trim().ToLower();
                        if (s.Contains(' '))
                            _userPhraseList.Add(s);
                        else
                            _userWordList.Add(s);
                        Utilities.AddToUserDictionary(s, _languageName);
                    }
                    break;
                case SpellCheckAction.AddToNamesEtc:
                    if (ChangeWord.Length > 1 && !_namesEtcList.Contains(ChangeWord))
                    {
                        _namesEtcList.Add(ChangeWord);
                        _namesEtcListUppercase.Add(ChangeWord.ToUpper());
                        if (_languageName.StartsWith("en_") && !ChangeWord.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                        {
                            _namesEtcList.Add(ChangeWord + "s");
                            _namesEtcListUppercase.Add(ChangeWord.ToUpper() + "S");
                        }
                        if (!ChangeWord.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                        {
                            _namesEtcListWithApostrophe.Add(ChangeWord + "'s");
                            _namesEtcListUppercase.Add(ChangeWord.ToUpper() + "'S");
                        }
                        if (!ChangeWord.EndsWith('\''))
                            _namesEtcListWithApostrophe.Add(ChangeWord + "'");
                        NamesList.AddWordToLocalNamesEtcList(ChangeWord, _languageName);
                    }
                    break;
                case SpellCheckAction.ChangeWholeText:
                    _mainWindow.ShowStatus(string.Format(Configuration.Settings.Language.Main.SpellCheckChangedXToY, _currentParagraph.Text.Replace(Environment.NewLine, " "), ChangeWholeText.Replace(Environment.NewLine, " ")));
                    _currentParagraph.Text = ChangeWholeText;
                    _mainWindow.ChangeWholeTextMainPart(ref _noOfChangedWords, ref _firstChange, _currentIndex, _currentParagraph);

                    break;
                default:
                    break;
            }
            labelActionInfo.Text = string.Empty;
            PrepareNextWord();
            CheckActions();
        }

        private void CheckActions()
        {
            if (string.IsNullOrEmpty(_currentAction))
                return;

            if (_currentAction == Configuration.Settings.Language.SpellCheck.Change)
                ShowActionInfo(_currentAction, _currentWord + " > " + textBoxWord.Text);
            else if (_currentAction == Configuration.Settings.Language.SpellCheck.ChangeAll)
                ShowActionInfo(_currentAction, _currentWord + " > " + textBoxWord.Text);
            else
                ShowActionInfo(_currentAction, textBoxWord.Text);
        }

        public static bool IsWordInUserPhrases(List<string> userPhraseList, int index, List<SpellCheckWord> words)
        {
            string current = words[index].Text;
            string prev = "-";
            if (index > 0)
                prev = words[index - 1].Text;
            string next = "-";
            if (index < words.Count - 1)
                next = words[index + 1].Text;
            foreach (string userPhrase in userPhraseList)
            {
                if (userPhrase == current + " " + next)
                    return true;
                if (userPhrase == prev + " " + current)
                    return true;
            }
            return false;
        }

        private void PrepareNextWord()
        {
            while (true)
            {
                if (_wordsIndex + 1 < _words.Count)
                {
                    _wordsIndex++;
                    _currentWord = _words[_wordsIndex].Text;
                    _currentSpellCheckWord = _words[_wordsIndex];
                }
                else
                {
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
                        ShowEndStatusMessage(Configuration.Settings.Language.SpellCheck.SpellCheckCompleted);
                        DialogResult = DialogResult.OK;
                        return;
                    }
                }

                int minLength = 2;
                if (Configuration.Settings.Tools.SpellCheckOneLetterWords)
                    minLength = 1;

                if (_currentWord.Trim().Length >= minLength &&
                    !_currentWord.Contains('0') &&
                    !_currentWord.Contains('1') &&
                    !_currentWord.Contains('2') &&
                    !_currentWord.Contains('3') &&
                    !_currentWord.Contains('4') &&
                    !_currentWord.Contains('5') &&
                    !_currentWord.Contains('6') &&
                    !_currentWord.Contains('7') &&
                    !_currentWord.Contains('8') &&
                    !_currentWord.Contains('9') &&
                    !_currentWord.Contains('%') &&
                    !_currentWord.Contains('&') &&
                    !_currentWord.Contains('@') &&
                    !_currentWord.Contains('$') &&
                    !_currentWord.Contains('*') &&
                    !_currentWord.Contains('=') &&
                    !_currentWord.Contains('£') &&
                    !_currentWord.Contains('#') &&
                    !_currentWord.Contains('_') &&
                    !_currentWord.Contains('½') &&
                    !_currentWord.Contains('^') &&
                    !_currentWord.Contains('£')
                    )
                {
                    _prefix = string.Empty;
                    _postfix = string.Empty;
                    if (_currentWord.Length > 1)
                    {
                        if (_currentWord.StartsWith('\''))
                        {
                            _prefix = "'";
                            _currentWord = _currentWord.Substring(1);
                        }
                        if (_currentWord.StartsWith('`'))
                        {
                            _prefix = "`";
                            _currentWord = _currentWord.Substring(1);
                        }

                    }
                    if (_namesEtcList.Contains(_currentWord)
                        || (_currentWord.StartsWith('\'') || _currentWord.EndsWith('\'')) && _namesEtcList.Contains(_currentWord.Trim('\'')))
                    {
                        _noOfNamesEtc++;
                    }
                    else if (_skipAllList.Contains(_currentWord.ToUpper())
                        || (_currentWord.StartsWith('\'') || _currentWord.EndsWith('\'')) && _skipAllList.Contains(_currentWord.Trim('\'').ToUpper()))
                    {
                        _noOfSkippedWords++;
                    }
                    else if (_userWordList.Contains(_currentWord.ToLower())
                        || (_currentWord.StartsWith('\'') || _currentWord.EndsWith('\'')) && _userWordList.Contains(_currentWord.Trim('\'').ToLower()))
                    {
                        _noOfCorrectWords++;
                    }
                    else if (_changeAllDictionary.ContainsKey(_currentWord))
                    {
                        _noOfChangedWords++;
                        _mainWindow.CorrectWord(_changeAllDictionary[_currentWord], _currentParagraph, _currentWord, ref _firstChange);
                    }
                    else if (_changeAllDictionary.ContainsKey(_currentWord.Trim('\'')))
                    {
                        _noOfChangedWords++;
                        _mainWindow.CorrectWord(_changeAllDictionary[_currentWord], _currentParagraph, _currentWord.Trim('\''), ref _firstChange);
                    }
                    else if (_namesEtcListUppercase.Contains(_currentWord)
                        || _namesEtcListWithApostrophe.Contains(_currentWord)
                        || NamesList.IsInNamesEtcMultiWordList(_namesEtcMultiWordList, _currentParagraph.Text, _currentWord)) //TODO: verify this!
                    {
                        _noOfNamesEtc++;
                    }
                    else if (IsWordInUserPhrases(_userPhraseList, _wordsIndex, _words))
                    {
                        _noOfCorrectWords++;
                    }
                    else
                    {
                        bool correct;

                        if (_prefix == "'" && _currentWord.Length >= 1 && (DoSpell(_prefix + _currentWord) || _userWordList.Contains(_prefix + _currentWord)))
                        {
                            correct = true;
                        }
                        else if (_currentWord.Length > 1)
                        {
                            correct = DoSpell(_currentWord);
                            if (!correct && (_currentWord.EndsWith('\'') || _currentWord.EndsWith('`')))
                                correct = DoSpell(_currentWord.TrimEnd('\'').TrimEnd('`'));
                            if (!correct && _currentWord.EndsWith("'s") && _currentWord.Length > 4)
                                correct = DoSpell(_currentWord.TrimEnd('s').TrimEnd('\''));
                            if (!correct && _currentWord.EndsWith('\'') && DoSpell(_currentWord.TrimEnd('\'')))
                            {
                                _currentWord = _currentWord.TrimEnd('\'');
                                correct = true;
                            }
                        }
                        else
                        {
                            correct = false;
                            if (_currentWord == "'")
                                correct = true;
                            else if (_languageName.StartsWith("en_") && (_currentWord.Equals("a", StringComparison.OrdinalIgnoreCase) || _currentWord == "I"))
                                correct = true;
                            else if (_languageName.StartsWith("da_") && _currentWord.Equals("i", StringComparison.OrdinalIgnoreCase))
                                correct = true;
                        }

                        if (!correct && Configuration.Settings.Tools.SpellCheckEnglishAllowInQuoteAsIng &&
                            _languageName.StartsWith("en_") && _currentWord.EndsWith("in'", StringComparison.OrdinalIgnoreCase))
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

                            List<string> suggestions = new List<string>();

                            if ((_currentWord == "Lt's" || _currentWord == "Lt'S") && _languageName.StartsWith("en_"))
                            {
                                suggestions.Add("It's");
                            }
                            else
                            {
                                if (_currentWord.ToUpper() != "LT'S" && _currentWord.ToUpper() != "SOX'S") //TODO: get fixed nhunspell
                                    suggestions = DoSuggest(_currentWord); //TODO: 0.9.6 fails on "Lt'S"
                                if (_languageName.StartsWith("fr_") && (_currentWord.StartsWith("I'") || _currentWord.StartsWith("I’")))
                                {
                                    if (_currentWord.Length > 3 && Utilities.LowercaseLetters.Contains(_currentWord[2]))
                                    {
                                        if (_currentSpellCheckWord.Index > 3)
                                        {
                                            string ending = _currentParagraph.Text.Substring(0, _currentSpellCheckWord.Index - 1).Trim();
                                            if (!ending.EndsWith('.') && !ending.EndsWith('!') && !ending.EndsWith('?'))
                                            {
                                                for (int i = 0; i < suggestions.Count; i++)
                                                {
                                                    if (suggestions[i].StartsWith("L'") || suggestions[i].StartsWith("L’"))
                                                        suggestions[i] = @"l" + suggestions[i].Substring(1);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            suggestions.Remove(_currentWord);
                            if (_currentWord.Length == 1)
                            {
                                if ((_currentWord == "L") && _languageName.StartsWith("en_"))
                                {
                                    suggestions.Remove("I");
                                    suggestions.Insert(0, "I");
                                }
                            }

                            if (AutoFixNames && _currentWord.Length > 1 && suggestions.Contains(char.ToUpper(_currentWord[0]) + _currentWord.Substring(1)))
                            {
                                ChangeWord = char.ToUpper(_currentWord[0]) + _currentWord.Substring(1);
                                DoAction(SpellCheckAction.ChangeAll);
                                return;
                            }
                            else if (AutoFixNames && _currentWord.Length > 3 && suggestions.Contains(_currentWord.ToUpper()))
                            { // does not work well with two letter words like "da" and "de" which get auto-corrected to "DA" and "DE"
                                ChangeWord = _currentWord.ToUpper();
                                DoAction(SpellCheckAction.ChangeAll);
                                return;
                            }
                            else if (AutoFixNames && _currentWord.Length > 1 && _namesEtcList.Contains(char.ToUpper(_currentWord[0]) + _currentWord.Substring(1)))
                            {
                                ChangeWord = char.ToUpper(_currentWord[0]) + _currentWord.Substring(1);
                                DoAction(SpellCheckAction.ChangeAll);
                                return;
                            }
                            else
                            {
                                if (_prefix != null && _prefix == "''" && _currentWord.EndsWith("''"))
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
                                    Initialize(_languageName, _currentSpellCheckWord, suggestions, _currentParagraph.Text, string.Format(Configuration.Settings.Language.Main.LineXOfY, (_currentIndex + 1), _subtitle.Paragraphs.Count));
                                }
                                else
                                {
                                    _currentSpellCheckWord.Text = _currentWord;
                                    Initialize(_languageName, _currentSpellCheckWord, suggestions, _currentParagraph.Text, string.Format(Configuration.Settings.Language.Main.LineXOfY, (_currentIndex + 1), _subtitle.Paragraphs.Count));
                                }
                                if (!this.Visible)
                                    this.ShowDialog(_mainWindow);
                                return; // wait for user input
                            }
                        }

                    }
                }
            }
        }

        private static List<SpellCheckWord> Split(string s)
        {
            const string SplitChars = " -.,?!:;\"“”()[]{}|<>/+\r\n¿¡…—–♪♫„“";
            var list = new List<SpellCheckWord>();
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (SplitChars.Contains(s[i]))
                {
                    if (sb.Length > 0)
                        list.Add(new SpellCheckWord() { Text = sb.ToString(), Index = i - sb.Length });
                    sb = new StringBuilder();
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            if (sb.Length > 0)
                list.Add(new SpellCheckWord() { Text = sb.ToString(), Index = s.Length - 1 - sb.Length });
            return list;
        }

        private string SetWords(string s)
        {
            s = ReplaceHtmlTagsWithBlanks(s);
            s = ReplaceKnownWordsOrNamesWithBlanks(s);
            _words = Split(s);
            return s;
        }

        private string ReplaceKnownWordsOrNamesWithBlanks(string s)
        {
            List<string> replaceIds = new List<string>();
            List<string> replaceNames = new List<string>();
            GetTextWithoutUserWordsAndNames(replaceIds, replaceNames, s);
            foreach (string name in replaceNames)
            {
                int start = s.IndexOf(name, StringComparison.Ordinal);
                while (start >= 0)
                {
                    bool startOk = start == 0 || " -.,?!:;\"“”()[]{}|<>/+\r\n¿¡…—–♪♫„“".Contains(s[start - 1]);
                    if (startOk)
                    {
                        int end = start + name.Length;
                        bool endOk = end >= s.Length || " -.,?!:;\"“”()[]{}|<>/+\r\n¿¡…—–♪♫„“".Contains(s[end]);
                        if (endOk)
                            s = s.Remove(start, name.Length).Insert(start, string.Empty.PadLeft(name.Length));
                    }

                    if (start + 1 < s.Length)
                        start = s.IndexOf(name, start + 1, StringComparison.Ordinal);
                    else
                        start = -1;
                }
            }
            return s;
        }

        private static string ReplaceHtmlTagsWithBlanks(string s)
        {
            int start = s.IndexOf('<');
            while (start >= 0)
            {
                int end = s.IndexOf('>', start);
                if (end > 0)
                {
                    int l = end - start + 1;
                    s = s.Remove(start, l).Insert(start, string.Empty.PadLeft(l));
                    if (start + 1 < s.Length)
                        start = s.IndexOf('<', start + 1);
                    else
                        start = -1;
                }
                else
                {
                    return s;
                }
            }
            return s;
        }

        /// <summary>
        /// Removes words with dash'es that are correct, so spell check can ignore the combination (do not split correct words with dash'es)
        /// </summary>
        private string GetTextWithoutUserWordsAndNames(List<string> replaceIds, List<string> replaceNames, string text)
        {
            string[] wordsWithDash = text.Split(new[] { ' ', '.', ',', '?', '!', ':', ';', '"', '“', '”', '(', ')', '[', ']', '{', '}', '|', '<', '>', '/', '+', '\r', '\n', '¿', '¡', '…', '—', '–', '♪', '♫', '„', '“' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string w in wordsWithDash)
            {
                if (w.Contains('-') && DoSpell(w) && !_wordsWithDashesOrPeriods.Contains(w))
                    _wordsWithDashesOrPeriods.Add(w);
            }

            if (text.Contains('.') || text.Contains('-'))
            {
                int i = 0;
                string id = string.Format("_@{0}_", i);
                foreach (string wordWithDashesOrPeriods in _wordsWithDashesOrPeriods)
                {
                    bool found = true;
                    int startSearchIndex = 0;
                    while (found)
                    {
                        int indexStart = text.IndexOf(wordWithDashesOrPeriods, startSearchIndex, StringComparison.Ordinal);

                        if (indexStart >= 0)
                        {
                            found = true;
                            int endIndexPlus = indexStart + wordWithDashesOrPeriods.Length;
                            bool startOk = indexStart == 0 || (@" (['""" + Environment.NewLine).Contains(text[indexStart - 1]);
                            bool endOk = endIndexPlus == text.Length;
                            if (!endOk && endIndexPlus < text.Length && @",!?:;. ])<'""".Contains(text[endIndexPlus]))
                                endOk = true;
                            if (startOk && endOk)
                            {
                                i++;
                                id = string.Format("_@{0}_", i);
                                replaceIds.Add(id);
                                replaceNames.Add(wordWithDashesOrPeriods);
                                text = text.Remove(indexStart, wordWithDashesOrPeriods.Length).Insert(indexStart, id);
                            }
                            else
                            {
                                startSearchIndex = indexStart + 1;
                            }
                        }
                        else
                        {
                            found = false;
                        }
                    }
                }
            }
            return text;
        }

        private void ShowEndStatusMessage(string completedMessage)
        {
            LanguageStructure.Main mainLanguage = Configuration.Settings.Language.Main;
            if (_noOfChangedWords > 0 || _noOfAddedWords > 0 || _noOfSkippedWords > 0 || completedMessage == Configuration.Settings.Language.SpellCheck.SpellCheckCompleted)
            {
                this.Hide();
                if (Configuration.Settings.Tools.SpellCheckShowCompletedMessage)
                {
                    var form = new DialogDoNotShowAgain(_mainWindow.Title + " - " + mainLanguage.SpellCheck,
                                    completedMessage + Environment.NewLine +
                                    Environment.NewLine +
                                    string.Format(mainLanguage.NumberOfCorrectedWords, _noOfChangedWords) + Environment.NewLine +
                                    string.Format(mainLanguage.NumberOfSkippedWords, _noOfSkippedWords) + Environment.NewLine +
                                    string.Format(mainLanguage.NumberOfCorrectWords, _noOfCorrectWords) + Environment.NewLine +
                                    string.Format(mainLanguage.NumberOfWordsAddedToDictionary, _noOfAddedWords) + Environment.NewLine +
                                    string.Format(mainLanguage.NumberOfNameHits, _noOfNamesEtc));
                    form.ShowDialog(_mainWindow);
                    Configuration.Settings.Tools.SpellCheckShowCompletedMessage = !form.DoNoDisplayAgain;
                }
                else
                {
                    if (_noOfChangedWords > 0)
                        _mainWindow.ShowStatus(completedMessage + "  " + string.Format(mainLanguage.NumberOfCorrectedWords, _noOfChangedWords));
                    else
                        _mainWindow.ShowStatus(completedMessage);
                }
            }
        }

        public void ContinueSpellCheck(Subtitle subtitle)
        {
            _subtitle = subtitle;

            buttonUndo.Visible = false;
            _undoList = new List<UndoObject>();

            if (_currentIndex >= subtitle.Paragraphs.Count)
                _currentIndex = 0;

            _currentParagraph = _subtitle.GetParagraphOrDefault(_currentIndex);
            if (_currentParagraph == null)
                return;

            SetWords(_currentParagraph.Text);
            _wordsIndex = -1;

            PrepareNextWord();
        }

        public void DoSpellCheck(bool autoDetect, Subtitle subtitle, string dictionaryFolder, Main mainWindow, int startLine)
        {
            _subtitle = subtitle;
            LanguageStructure.Main mainLanguage = Configuration.Settings.Language.Main;
            _mainWindow = mainWindow;

            _namesEtcList = new List<string>();
            _namesEtcMultiWordList = new List<string>();
            _namesEtcListUppercase = new List<string>();
            _namesEtcListWithApostrophe = new List<string>();

            _skipAllList = new List<string>();

            _noOfSkippedWords = 0;
            _noOfChangedWords = 0;
            _noOfCorrectWords = 0;
            _noOfNamesEtc = 0;
            _noOfAddedWords = 0;
            _firstChange = true;

            if (!string.IsNullOrEmpty(Configuration.Settings.General.SpellCheckLanguage))
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
                _languageName = Utilities.AutoDetectLanguageName(_languageName, subtitle);
            string dictionary = Utilities.DictionaryFolder + _languageName;

            NamesList.LoadNamesEtcWordLists(_namesEtcList, _namesEtcMultiWordList, _languageName);
            foreach (string namesItem in _namesEtcList)
                _namesEtcListUppercase.Add(namesItem.ToUpper());

            if (_languageName.StartsWith("en_", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string namesItem in _namesEtcList)
                {
                    if (!namesItem.EndsWith('s'))
                    {
                        _namesEtcListWithApostrophe.Add(namesItem + "'s");
                        _namesEtcListWithApostrophe.Add(namesItem + "’s");
                    }
                    else if (!namesItem.EndsWith('\''))
                    {
                        _namesEtcListWithApostrophe.Add(namesItem + "'");
                    }
                }
            }

            _userWordList = new List<string>();
            _userPhraseList = new List<string>();
            if (File.Exists(dictionaryFolder + _languageName + "_user.xml"))
            {
                var userWordDictionary = new XmlDocument();
                userWordDictionary.Load(dictionaryFolder + _languageName + "_user.xml");
                foreach (XmlNode node in userWordDictionary.DocumentElement.SelectNodes("word"))
                {
                    string word = node.InnerText.Trim().ToLower();
                    if (word.Contains(' '))
                        _userPhraseList.Add(word);
                    else
                        _userWordList.Add(word);
                }
            }

            // Add names/userdic with "." or " " or "-"
            _wordsWithDashesOrPeriods = new List<string>();
            _wordsWithDashesOrPeriods.AddRange(_namesEtcMultiWordList);
            foreach (string name in _namesEtcList)
            {
                if (name.Contains('.') || name.Contains('-'))
                    _wordsWithDashesOrPeriods.Add(name);
            }
            foreach (string word in _userWordList)
            {
                if (word.Contains('.') || word.Contains('-'))
                    _wordsWithDashesOrPeriods.Add(word);
            }
            _wordsWithDashesOrPeriods.AddRange(_userPhraseList);

            _changeAllDictionary = new Dictionary<string, string>();
            LoadHunspell(dictionary);
            _currentIndex = 0;
            if (startLine >= 0 && startLine < _subtitle.Paragraphs.Count)
                _currentIndex = startLine;
            _currentParagraph = _subtitle.Paragraphs[_currentIndex];
            SetWords(_currentParagraph.Text);
            _wordsIndex = -1;

            PrepareNextWord();
        }

        private void textBoxWord_TextChanged(object sender, EventArgs e)
        {
            buttonChange.Enabled = textBoxWord.Text != _originalWord;
            buttonChangeAll.Enabled = buttonChange.Enabled;
        }

        private void buttonAddToDictionary_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(Configuration.Settings.Language.SpellCheck.AddToUserDictionary, textBoxWord.Text);
        }

        private void ShowActionInfo(string label, string text)
        {
            labelActionInfo.Text = string.Format("{0}: {1}", label, text.Trim());
            _currentAction = label;
        }

        private void buttonAddToDictionary_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonAddToNames_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(Configuration.Settings.Language.SpellCheck.AddToNamesAndIgnoreList, textBoxWord.Text);
        }

        private void buttonAddToNames_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonSkipOnce_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(Configuration.Settings.Language.SpellCheck.SkipOnce, textBoxWord.Text);
        }

        private void buttonSkipOnce_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonSkipAll_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(Configuration.Settings.Language.SpellCheck.SkipAll, textBoxWord.Text);
        }

        private void buttonSkipAll_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonChange_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(Configuration.Settings.Language.SpellCheck.Change, _currentWord + " > " + textBoxWord.Text);
        }

        private void buttonChange_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonChangeAll_MouseEnter(object sender, EventArgs e)
        {
            ShowActionInfo(Configuration.Settings.Language.SpellCheck.ChangeAll, _currentWord + " > " + textBoxWord.Text);
        }

        private void buttonChangeAll_MouseLeave(object sender, EventArgs e)
        {
            labelActionInfo.Text = string.Empty;
            _currentAction = null;
        }

        private void buttonSpellCheckDownload_Click(object sender, EventArgs e)
        {
            new GetDictionaries().ShowDialog(this);
            FillSpellCheckDictionaries(Utilities.AutoDetectLanguageName(null, _subtitle));
            if (comboBoxDictionaries.Items.Count > 0 && comboBoxDictionaries.SelectedIndex == -1)
                comboBoxDictionaries.SelectedIndex = 0;
            ComboBoxDictionariesSelectedIndexChanged(null, null);
        }

        private void PushUndo(string text, SpellCheckAction action)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (action == SpellCheckAction.ChangeAll && _changeAllDictionary.ContainsKey(_currentWord))
                return;

            string format = Configuration.Settings.Language.SpellCheck.UndoX;
            if (string.IsNullOrEmpty(format))
                format = "Undo: {0}";
            string undoText = string.Format(format, text);

            _undoList.Add(new UndoObject()
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
                NoOfNamesEtc = _noOfNamesEtc,
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
                _noOfNamesEtc = undo.NoOfNamesEtc;
                _noOfAddedWords = undo.NoOfAddedWords;

                switch (undo.Action)
                {
                    case SpellCheckAction.Change:
                        _subtitle = _mainWindow.UndoFromSpellCheck(undo.Subtitle);
                        break;
                    case SpellCheckAction.ChangeAll:
                        _subtitle = _mainWindow.UndoFromSpellCheck(undo.Subtitle);
                        _changeAllDictionary.Remove(undo.CurrentWord);
                        break;
                    case SpellCheckAction.Skip:
                        break;
                    case SpellCheckAction.SkipAll:
                        _skipAllList.Remove(undo.UndoWord.ToUpper());
                        if (undo.UndoWord.EndsWith('\'') || undo.UndoWord.StartsWith('\''))
                            _skipAllList.Remove(undo.UndoWord.ToUpper().Trim('\''));
                        break;
                    case SpellCheckAction.AddToDictionary:
                        _userWordList.Remove(undo.UndoWord);
                        _userPhraseList.Remove(undo.UndoWord);
                        Utilities.RemoveFromUserDictionary(undo.UndoWord, _languageName);
                        break;
                    case SpellCheckAction.AddToNamesEtc:
                        if (undo.UndoWord.Length > 1 && _namesEtcList.Contains(undo.UndoWord))
                        {
                            _namesEtcList.Remove(undo.UndoWord);
                            _namesEtcListUppercase.Remove(undo.UndoWord.ToUpper());
                            if (_languageName.StartsWith("en_") && !undo.UndoWord.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                            {
                                _namesEtcList.Remove(undo.UndoWord + "s");
                                _namesEtcListUppercase.Remove(undo.UndoWord.ToUpper() + "S");
                            }
                            if (!undo.UndoWord.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                            {
                                _namesEtcListWithApostrophe.Remove(undo.UndoWord + "'s");
                                _namesEtcListUppercase.Remove(undo.UndoWord.ToUpper() + "'S");
                            }
                            if (!undo.UndoWord.EndsWith('\''))
                                _namesEtcListWithApostrophe.Remove(undo.UndoWord + "'");

                            NamesList.RemoveFromLocalNamesEtcList(undo.UndoWord, _languageName);
                        }
                        break;
                    case SpellCheckAction.ChangeWholeText:
                        _subtitle = _mainWindow.UndoFromSpellCheck(undo.Subtitle);
                        break;
                    default:
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

    }
}
