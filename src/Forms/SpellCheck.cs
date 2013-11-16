using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Enums;
using Nikse.SubtitleEdit.Logic.SpellCheck;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SpellCheck : Form
    {
        SpellCheckAction _action = SpellCheckAction.Skip;
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

        List<string> _namesEtcList = new List<string>();
        List<string> _namesEtcMultiWordList = new List<string>();
        List<string> _namesEtcListUppercase = new List<string>();
        List<string> _namesEtcListWithApostrophe = new List<string>();
        List<string> _skipAllList = new List<string>();
        List<string> _wordsWithDashesOrPeriods = new List<string>();

        Dictionary<string, string> _changeAllDictionary = new Dictionary<string, string>();
        List<string> _userWordList = new List<string>();
        List<string> _userPhraseList = new List<string>();
        XmlDocument _userWordDictionary = new XmlDocument();
        string _prefix = string.Empty;
        string _postfix = string.Empty;
        Hunspell _hunspell;
        string _dictionaryFolder;
        Paragraph _currentParagraph;
        int _currentIndex;
        string _currentWord;
        string[] _words;
        int _wordsIndex;
        Subtitle _subtitle;
        string _originalWord;

        int _noOfSkippedWords = 0;
        int _noOfChangedWords = 0;
        int _noOfCorrectWords = 0;
        int _noOfNamesEtc = 0;
        int _noOfAddedWords = 0;
        bool _firstChange = true;
        string _languageName;
        Main _mainWindow;

        string _currentDictionary = null;

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
                int start = name.LastIndexOf("[");
                int end = name.LastIndexOf("]");
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

        public void Initialize(string languageName, string word, List<string> suggestions, string paragraph, string progress)
        {
            _originalWord = word;
            _suggestions = suggestions;
            groupBoxWordNotFound.Visible = true;
            groupBoxEditWholeText.Visible = false;
            Text = Configuration.Settings.Language.SpellCheck.Title + " [" + languageName + "] - " + progress;
            textBoxWord.Text = word;
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

        private void ShowActiveWordWithColor(string word)
        {
            richTextBoxParagraph.SelectAll();
            richTextBoxParagraph.SelectionColor = Color.Black;
            richTextBoxParagraph.SelectionLength = 0;

            var regEx = Utilities.MakeWordSearchRegex(word);
            Match match = regEx.Match(richTextBoxParagraph.Text);
            if (match.Success)
            {
                richTextBoxParagraph.SelectionStart = match.Index;
                richTextBoxParagraph.SelectionLength = word.Length;
                while (richTextBoxParagraph.SelectedText != word && richTextBoxParagraph.SelectionStart > 0)
                {
                    richTextBoxParagraph.SelectionStart = richTextBoxParagraph.SelectionStart - 1;
                    richTextBoxParagraph.SelectionLength = word.Length;
                }
                richTextBoxParagraph.SelectionColor = Color.Red;
            }
            else if (richTextBoxParagraph.Text.Length > 0)
            {
                int idx = richTextBoxParagraph.Text.IndexOf(word);
                while (idx >= 0)
                {
                    bool ok = idx == 0 || richTextBoxParagraph.Text[idx - 1] == ' ' || richTextBoxParagraph.Text[idx - 1] == '"' || idx == richTextBoxParagraph.Text.Length - word.Length ||
                                       Environment.NewLine.EndsWith(richTextBoxParagraph.Text[idx - 1].ToString());

                    if (ok)
                    {
                        richTextBoxParagraph.SelectionStart = idx;
                        richTextBoxParagraph.SelectionLength = word.Length;
                        while (richTextBoxParagraph.SelectedText != word && richTextBoxParagraph.SelectionStart > 0)
                        {
                            richTextBoxParagraph.SelectionStart = richTextBoxParagraph.SelectionStart - 1;
                            richTextBoxParagraph.SelectionLength = word.Length;
                        }
                        richTextBoxParagraph.SelectionColor = Color.Red;
                    }

                    idx += word.Length;
                    if (idx < richTextBoxParagraph.Text.Length)
                        idx = richTextBoxParagraph.Text.IndexOf(word, idx);
                    else
                        idx = -1;
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
        }

        private void ButtonAbortClick(object sender, EventArgs e)
        {
            ShowEndStatusMessage(Configuration.Settings.Language.SpellCheck.SpellCheckAborted);
            DialogResult = DialogResult.Abort;
        }

        private void ButtonChangeClick(object sender, EventArgs e)
        {
            DoAction(SpellCheckAction.Change);
        }

        private void ButtonUseSuggestionClick(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex >= 0)
            {
                textBoxWord.Text = listBoxSuggestions.SelectedItem.ToString();
                DoAction(SpellCheckAction.Change);
            }
        }

        private void ButtonSkipAllClick(object sender, EventArgs e)
        {
            DoAction(SpellCheckAction.SkipAll);
        }

        private void ButtonSkipOnceClick(object sender, EventArgs e)
        {
            DoAction(SpellCheckAction.Skip);
        }

        private void ButtonAddToDictionaryClick(object sender, EventArgs e)
        {
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
            _userWordDictionary = new XmlDocument();
            if (File.Exists(Utilities.DictionaryFolder + _languageName + "_user.xml"))
            {
                _userWordDictionary.Load(Utilities.DictionaryFolder + _languageName + "_user.xml");
                foreach (XmlNode node in _userWordDictionary.DocumentElement.SelectNodes("word"))
                {
                    string word = node.InnerText.Trim().ToLower();
                    if (word.Contains(" "))
                        _userPhraseList.Add(word);
                    else
                        _userWordList.Add(word);
                }
            }
            else
            {
                _userWordDictionary.LoadXml("<words />");
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
            DoAction(SpellCheckAction.ChangeAll);
        }

        private void ButtonUseSuggestionAlwaysClick(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex >= 0)
            {
                textBoxWord.Text = listBoxSuggestions.SelectedItem.ToString();
                DoAction(SpellCheckAction.ChangeAll);
            }
        }

        private void SpellCheck_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult = DialogResult.Abort;
            }
        }

        private void ButtonAddToNamesClick(object sender, EventArgs e)
        {
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
            DoAction(SpellCheckAction.Skip);
        }

        private void ButtonChangeWholeTextClick(object sender, EventArgs e)
        {
            DoAction(SpellCheckAction.ChangeWholeText);
        }

        private void ContextMenuStrip1Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (richTextBoxParagraph.SelectedText.Trim().Length > 0)
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
            if (richTextBoxParagraph.SelectedText.Trim().Length > 0)
            {
                ChangeWord = richTextBoxParagraph.SelectedText.Trim();
                DoAction(SpellCheckAction.AddToNamesEtc);
            }
        }

        private void CheckBoxAutoChangeNamesCheckedChanged(object sender, EventArgs e)
        {
            if (textBoxWord.Text.Length < 2)
                return;

            string s = textBoxWord.Text.Substring(0, 1).ToUpper() + textBoxWord.Text.Substring(1);
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
                    _skipAllList.Add(_currentWord.ToUpper());
                    break;
                case SpellCheckAction.AddToDictionary:
                    if (_userWordList.IndexOf(ChangeWord) < 0)
                    {
                        _noOfAddedWords++;
                        string s = ChangeWord.Trim().ToLower();
                        if (s.Contains(" "))
                            _userPhraseList.Add(s);
                        else
                            _userWordList.Add(s);
                        XmlNode node = _userWordDictionary.CreateElement("word");
                        node.InnerText = s;
                        _userWordDictionary.DocumentElement.AppendChild(node);
                        _userWordDictionary.Save(_dictionaryFolder + _languageName + "_user.xml");
                    }
                    break;
                case SpellCheckAction.AddToNamesEtc:
                    if (_currentWord == ChangeWord)
                    {
                        _namesEtcList.Add(_currentWord);
                        _namesEtcListUppercase.Add(_currentWord.ToUpper());
                        if (!_currentWord.EndsWith("s"))
                            _namesEtcListWithApostrophe.Add(_currentWord + "'s");
                        else if (!_currentWord.EndsWith("'"))
                            _namesEtcListWithApostrophe.Add(_currentWord + "'");
                        Utilities.AddWordToLocalNamesEtcList(_currentWord, _languageName);
                    }
                    else
                    {
                        if (!_namesEtcList.Contains(ChangeWord))
                            Utilities.AddWordToLocalNamesEtcList(ChangeWord, _languageName);
                        _namesEtcListUppercase.Add(ChangeWord.ToUpper());
                        _namesEtcListUppercase.Add(_currentWord.ToUpper());
                        if (!_currentWord.EndsWith("s"))
                        _noOfChangedWords++;
                        if (!_changeAllDictionary.ContainsKey(_currentWord))
                            _changeAllDictionary.Add(_currentWord, ChangeWord);
                        _mainWindow.CorrectWord(_prefix + ChangeWord + _postfix, _currentParagraph, _prefix + _currentWord + _postfix, ref _firstChange);
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

        private void PrepareNextWord()
        {
            while (true)
            {
                if (_wordsIndex + 1 < _words.Length)
                {
                    _wordsIndex++;
                    _currentWord = _words[_wordsIndex];
                }
                else
                {
                    if (_currentIndex + 1 < _subtitle.Paragraphs.Count)
                    {
                        _currentIndex++;
                        _currentParagraph = _subtitle.Paragraphs[_currentIndex];
                        string s = Utilities.RemoveHtmlTags(_currentParagraph.Text);

                        SetWords(s);

                        _wordsIndex = 0;
                        if (_words.Length == 0)
                        {
                            _currentWord = string.Empty;
                        }
                        else
                        {
                            _currentWord = _words[_wordsIndex];
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
                    !_currentWord.Contains("0") &&
                    !_currentWord.Contains("1") &&
                    !_currentWord.Contains("2") &&
                    !_currentWord.Contains("3") &&
                    !_currentWord.Contains("4") &&
                    !_currentWord.Contains("5") &&
                    !_currentWord.Contains("6") &&
                    !_currentWord.Contains("7") &&
                    !_currentWord.Contains("8") &&
                    !_currentWord.Contains("9") &&
                    !_currentWord.Contains("%") &&
                    !_currentWord.Contains("&") &&
                    !_currentWord.Contains("@") &&
                    !_currentWord.Contains("$") &&
                    !_currentWord.Contains("*") &&
                    !_currentWord.Contains("=") &&
                    !_currentWord.Contains("£") &&
                    !_currentWord.Contains("#") &&
                    !_currentWord.Contains("_") &&
                    !_currentWord.Contains("½") &&
                    !_currentWord.Contains("^") &&
                    !_currentWord.Contains("£")
                    )
                {
                    _prefix = string.Empty;
                    _postfix = string.Empty;
                    if (_currentWord.Length > 1)
                    {
                        if (_currentWord.StartsWith("'"))
                        {
                            _prefix = "'";
                            _currentWord = _currentWord.Substring(1);
                        }
                        if (_currentWord.StartsWith("`"))
                        {
                            _prefix = "`";
                            _currentWord = _currentWord.Substring(1);
                        }

                    }
                    if (_currentWord.Length > 1)
                    {
                        //if (_currentWord.EndsWith("'"))
                        //{
                        //    _postfix = "'";
                        //    _currentWord = _currentWord.Substring(0, _currentWord.Length - 1);
                        //}
                        //if (_currentWord.EndsWith("`"))
                        //{
                        //    _postfix = "`";
                        //    _currentWord = _currentWord.Substring(0, _currentWord.Length - 1);
                        //}
                    }

                    if (_namesEtcList.IndexOf(_currentWord) >= 0)
                    {
                        _noOfNamesEtc++;
                    }
                    else if (_skipAllList.IndexOf(_currentWord.ToUpper()) >= 0)
                    {
                        _noOfSkippedWords++;
                    }
                    else if (_userWordList.IndexOf(_currentWord.ToLower()) >= 0)
                    {
                        _noOfCorrectWords++;
                    }
                    else if (_changeAllDictionary.ContainsKey(_currentWord))
                    {
                        _noOfChangedWords++;
                        _mainWindow.CorrectWord(_changeAllDictionary[_currentWord], _currentParagraph, _currentWord, ref _firstChange);
                    }
                    else if (_namesEtcListUppercase.IndexOf(_currentWord) >= 0)
                    {
                        _noOfNamesEtc++;
                    }
                    else if (_namesEtcListWithApostrophe.IndexOf(_currentWord) >= 0)
                    {
                        _noOfNamesEtc++;
                    }
                    else if (Utilities.IsInNamesEtcMultiWordList(_namesEtcMultiWordList, _currentParagraph.Text, _currentWord)) //TODO: verify this!
                    {
                        _noOfNamesEtc++;
                    }
                    else if (Utilities.IsWordInUserPhrases(_userPhraseList, _wordsIndex, _words))
                    {
                        _noOfCorrectWords++;
                    }
                    else
                    {
                        bool correct;

                        if (_currentWord.Length > 1)
                        {
                            correct = DoSpell(_currentWord);
                            if (!correct && (_currentWord.EndsWith("'") || _currentWord.EndsWith("`")))
                                correct = DoSpell(_currentWord.TrimEnd('\'').TrimEnd('`'));
                        }
                        else
                        {
                            correct = false;
                            if (_currentWord == "'")
                                correct = true;
                            else if (_languageName.StartsWith("en_") && (_currentWord.ToLower() == "a" || _currentWord == "I"))
                                correct = true;
                            else if (_languageName.StartsWith("da_") && _currentWord.ToLower() == "i")
                                correct = true;
                        }

                        if (!correct && Configuration.Settings.Tools.SpellCheckEnglishAllowInQuoteAsIng && 
                            _languageName.StartsWith("en_") && _currentWord.ToLower().EndsWith("in'"))
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
                            }

                            if (AutoFixNames && _currentWord.Length > 1 && suggestions.Contains(_currentWord.Substring(0, 1).ToUpper() + _currentWord.Substring(1)))
                            {
                                ChangeWord = _currentWord.Substring(0, 1).ToUpper() + _currentWord.Substring(1);
                                Action = SpellCheckAction.ChangeAll;
                            }
                            else if (AutoFixNames && _currentWord.Length > 1 && suggestions.Contains(_currentWord.ToUpper()))
                            {
                                ChangeWord = _currentWord.ToUpper();
                                Action = SpellCheckAction.ChangeAll;
                            }
                            else if (AutoFixNames && _currentWord.Length > 1 && _namesEtcList.Contains(_currentWord.Substring(0, 1).ToUpper() + _currentWord.Substring(1)))
                            {
                                ChangeWord = _currentWord.Substring(0, 1).ToUpper() + _currentWord.Substring(1);
                                Action = SpellCheckAction.ChangeAll;
                            }
                            else
                            {
                                if (_postfix != null && _postfix == "'")
                                    Initialize(_languageName, _currentWord + _postfix, suggestions, _currentParagraph.Text, string.Format(Configuration.Settings.Language.Main.LineXOfY, (_currentIndex + 1), _subtitle.Paragraphs.Count));
                                else
                                    Initialize(_languageName, _currentWord, suggestions, _currentParagraph.Text, string.Format(Configuration.Settings.Language.Main.LineXOfY, (_currentIndex + 1), _subtitle.Paragraphs.Count));
                                if (!this.Visible)
                                    this.ShowDialog(_mainWindow);
                                return; // wait for user input
                            }
                        }

                    }
                }
            }
        }

        private string SetWords(string s)
        {
            List<string> replaceIds = new List<string>();
            List<string> replaceNames = new List<string>();
            s = Utilities.RemoveHtmlTags(s);
            s = GetTextWithoutUserWordsAndNames(replaceIds, replaceNames, s);
            _words = s.Split(" -.,?!:;\"“”()[]{}|<>/+\r\n¿¡…—–♪♫„“".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            _words = FixWordsInserUserWordAndNames(replaceIds, replaceNames, _words);
            return s;
        }

        /// <summary>
        /// Removes words with dash'es that are correct, so spell check can ignore the combination (do not split correct words with dash'es)
        /// </summary>
        private string GetTextWithoutUserWordsAndNames(List<string> replaceIds, List<string> replaceNames, string text)
        {

            string[] wordsWithDash = text.Split(" .,?!:;\"“”()[]{}|<>/+\r\n¿¡…—–♪♫„“".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string w in wordsWithDash)
            {
                if (w.Contains("-") && DoSpell(w) && !_wordsWithDashesOrPeriods.Contains(w))
                    _wordsWithDashesOrPeriods.Add(w);
            }

            if (text.Contains(".") || text.Contains("-"))
            {
                int i = 0;
                string id = string.Format("_@{0}_", i);
                foreach (string wordWithDashesOrPeriods in _wordsWithDashesOrPeriods)
                {
                    bool found = true;
                    int startSearchIndex = 0;
                    while (found)
                    {
                        int indexStart = text.IndexOf(wordWithDashesOrPeriods, startSearchIndex);

                        if (indexStart >= 0)
                        {
                            found = true;
                            int endIndexPlus = indexStart + wordWithDashesOrPeriods.Length;
                            bool startOk = indexStart == 0 || (" ['\"" + Environment.NewLine).Contains(text.Substring(indexStart - 1, 1));
                            bool endOk = endIndexPlus == text.Length;
                            if (!endOk && endIndexPlus < text.Length && ("!?:;. ]").Contains(text.Substring(endIndexPlus, 1)))
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

        private string[] FixWordsInserUserWordAndNames(List<string> replaceIds, List<string> replaceNames, string[] words)
        {
            if (replaceIds.Count == 0)
                return words;

            for (int i = 0; i < words.Length; i++)
            {
                if (replaceIds.Contains(words[i]))
                    words[i] = replaceNames[replaceIds.IndexOf(words[i])];
            }
            return words;
        }

        private void ShowEndStatusMessage(string completedMessage)
        {
            LanguageStructure.Main mainLanguage = Configuration.Settings.Language.Main;
            if (_noOfChangedWords > 0 || _noOfAddedWords > 0 || _noOfSkippedWords > 0 || completedMessage == Configuration.Settings.Language.SpellCheck.SpellCheckCompleted)
            {
                this.Hide();
                MessageBox.Show(completedMessage + Environment.NewLine +
                                Environment.NewLine +
                                string.Format(mainLanguage.NumberOfCorrectedWords, _noOfChangedWords) + Environment.NewLine +
                                string.Format(mainLanguage.NumberOfSkippedWords, _noOfSkippedWords) + Environment.NewLine +
                                string.Format(mainLanguage.NumberOfCorrectWords, _noOfCorrectWords) + Environment.NewLine +
                                string.Format(mainLanguage.NumberOfWordsAddedToDictionary, _noOfAddedWords) + Environment.NewLine +
                                string.Format(mainLanguage.NumberOfNameHits, _noOfNamesEtc), _mainWindow.Title + " - " + mainLanguage.SpellCheck);
            }
        }

        public void ContinueSpellcheck(Subtitle subtitle)
        {
            _subtitle = subtitle;

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
            _dictionaryFolder = dictionaryFolder;
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
                int start = name.LastIndexOf("[");
                int end = name.LastIndexOf("]");
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

            Utilities.LoadNamesEtcWordLists(_namesEtcList, _namesEtcMultiWordList, _languageName);
            foreach (string namesItem in _namesEtcList)
                _namesEtcListUppercase.Add(namesItem.ToUpper());

            if (_languageName.ToLower().StartsWith("en_"))
            {
                foreach (string namesItem in _namesEtcList)
                {
                    if (!namesItem.EndsWith("s"))
                        _namesEtcListWithApostrophe.Add(namesItem + "'s");
                    else if (!namesItem.EndsWith("'"))
                        _namesEtcListWithApostrophe.Add(namesItem + "'");
                }
            }

            _userWordList = new List<string>();
            _userPhraseList = new List<string>();
            _userWordDictionary = new XmlDocument();
            if (File.Exists(dictionaryFolder + _languageName + "_user.xml"))
            {
                _userWordDictionary.Load(dictionaryFolder + _languageName + "_user.xml");
                foreach (XmlNode node in _userWordDictionary.DocumentElement.SelectNodes("word"))
                {
                    string word = node.InnerText.Trim().ToLower();
                    if (word.Contains(" "))
                        _userPhraseList.Add(word);
                    else
                        _userWordList.Add(word);
                }
            }
            else
            {
                _userWordDictionary.LoadXml("<words />");
            }

            // Add names/userdic with "." or " " or "-"
            _wordsWithDashesOrPeriods = new List<string>();
            _wordsWithDashesOrPeriods.AddRange(_namesEtcMultiWordList);
            foreach (string name in _namesEtcList)
            {
                if (name.Contains(".") || name.Contains("-"))
                    _wordsWithDashesOrPeriods.Add(name);
            }
            foreach (string word in _userWordList)
            {
                if (word.Contains(".") || word.Contains("-"))
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

    }
}
