using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SpellCheck;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SpellCheck : Form, IDoSpell
    {
        private List<UndoObject> _undoList = new List<UndoObject>();
        private List<string> _suggestions;
        private string _currentAction;
        public SpellCheckAction Action { get; set; } = SpellCheckAction.Skip;
        public string ChangeWord
        {
            get => textBoxWord.Text;
            set => textBoxWord.Text = value;
        }
        public string ChangeWholeText => textBoxWholeText.Text;
        public bool AutoFixNames => checkBoxAutoChangeNames.Checked;

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
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
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
            buttonGoogleIt.Text = Configuration.Settings.Language.Main.VideoControls.GoogleIt;
            deleteToolStripMenuItem.Text = Configuration.Settings.Language.General.DeleteCurrentLine;
            UiUtil.FixLargeFonts(this, buttonAbort);
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
            {
                listBoxSuggestions.SelectedIndex = 0;
            }

            richTextBoxParagraph.Text = paragraph;

            FillSpellCheckDictionaries(languageName);
            ShowActiveWordWithColor(word);
            Action = SpellCheckAction.Skip;
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
                {
                    comboBoxDictionaries.SelectedIndex = comboBoxDictionaries.Items.Count - 1;
                }
            }
            comboBoxDictionaries.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBoxDictionaries.AutoCompleteMode = AutoCompleteMode.Append;
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
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#spellcheck");
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.G)
            {
                e.SuppressKeyPress = true;
                UiUtil.OpenURL("https://www.google.com/search?q=" + Utilities.UrlEncode(textBoxWord.Text));
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
            PushUndo($"{Configuration.Settings.Language.SpellCheck.Change}: {_currentWord + " > " + textBoxWord.Text}", SpellCheckAction.Change);
            DoAction(SpellCheckAction.Change);
        }

        private void ButtonUseSuggestionClick(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex >= 0)
            {
                textBoxWord.Text = listBoxSuggestions.SelectedItem.ToString();
                PushUndo($"{Configuration.Settings.Language.SpellCheck.Change}: {_currentWord + " > " + textBoxWord.Text}", SpellCheckAction.Change);
                DoAction(SpellCheckAction.Change);
            }
        }

        private void ButtonSkipAllClick(object sender, EventArgs e)
        {
            PushUndo($"{Configuration.Settings.Language.SpellCheck.SkipAll}: {textBoxWord.Text}", SpellCheckAction.SkipAll);
            DoAction(SpellCheckAction.SkipAll);
        }

        private void ButtonSkipOnceClick(object sender, EventArgs e)
        {
            PushUndo($"{Configuration.Settings.Language.SpellCheck.SkipOnce}: {textBoxWord.Text}", SpellCheckAction.Skip);
            DoAction(SpellCheckAction.Skip);
        }

        private void ButtonAddToDictionaryClick(object sender, EventArgs e)
        {
            PushUndo($"{Configuration.Settings.Language.SpellCheck.AddToUserDictionary}: {textBoxWord.Text}", SpellCheckAction.AddToDictionary);
            DoAction(SpellCheckAction.AddToDictionary);
        }

        private void ComboBoxDictionariesSelectedIndexChanged(object sender, EventArgs e)
        {
            Configuration.Settings.General.SpellCheckLanguage = LanguageString;
            Configuration.Settings.Save();
            _languageName = LanguageString;
            string dictionary = Utilities.DictionaryFolder + _languageName;
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
            suggestThread.Abort();
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
            PushUndo($"{Configuration.Settings.Language.SpellCheck.ChangeAll}: {_currentWord + " > " + textBoxWord.Text}", SpellCheckAction.ChangeAll);
            DoAction(SpellCheckAction.ChangeAll);
        }

        private void ButtonUseSuggestionAlwaysClick(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex >= 0)
            {
                textBoxWord.Text = listBoxSuggestions.SelectedItem.ToString();
                PushUndo($"{Configuration.Settings.Language.SpellCheck.ChangeAll}: {_currentWord + " > " + textBoxWord.Text}", SpellCheckAction.ChangeAll);
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
            PushUndo($"{Configuration.Settings.Language.SpellCheck.AddToNamesAndIgnoreList}: {textBoxWord.Text}", SpellCheckAction.AddToNames);
            DoAction(SpellCheckAction.AddToNames);
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
            PushUndo($"{Configuration.Settings.Language.SpellCheck.SkipOnce}", SpellCheckAction.Skip);
            DoAction(SpellCheckAction.SkipWholeLine);
        }

        private void ButtonChangeWholeTextClick(object sender, EventArgs e)
        {
            PushUndo($"{Configuration.Settings.Language.SpellCheck.EditWholeText}", SpellCheckAction.ChangeWholeText);
            DoAction(SpellCheckAction.ChangeWholeText);
        }

        private void ContextMenuStrip1Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool showAddItems = false;
            if (!string.IsNullOrWhiteSpace(richTextBoxParagraph.SelectedText))
            {
                string word = richTextBoxParagraph.SelectedText.Trim();
                addXToNamesnoiseListToolStripMenuItem.Text = string.Format(Configuration.Settings.Language.SpellCheck.AddXToNames, word);
                addXToUserDictionaryToolStripMenuItem.Text = string.Format(Configuration.Settings.Language.SpellCheck.AddXToUserDictionary, word);
                showAddItems = true;
            }
            addXToNamesnoiseListToolStripMenuItem.Visible = showAddItems;
            addXToUserDictionaryToolStripMenuItem.Visible = showAddItems;
            toolStripSeparator1.Visible = showAddItems;
        }

        private void AddXToNamesnoiseListToolStripMenuItemClick(object sender, EventArgs e)
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
                    _mainWindow.ShowStatus(string.Format(Configuration.Settings.Language.Main.SpellCheckChangedXToY, _currentParagraph.Text.Replace(Environment.NewLine, " "), ChangeWholeText.Replace(Environment.NewLine, " ")));
                    _currentParagraph.Text = ChangeWholeText;
                    _mainWindow.ChangeWholeTextMainPart(ref _noOfChangedWords, ref _firstChange, _currentIndex, _currentParagraph);
                    _currentIndex--; // re-spellcheck current line
                    _wordsIndex = int.MaxValue - 1;
                    break;
                case SpellCheckAction.DeleteLine:
                    _mainWindow.DeleteLine(_currentIndex, _currentParagraph);
                    _currentIndex--; // re-spellcheck current line
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

            if (_currentAction == Configuration.Settings.Language.SpellCheck.Change)
            {
                ShowActionInfo(_currentAction, _currentWord + " > " + textBoxWord.Text);
            }
            else if (_currentAction == Configuration.Settings.Language.SpellCheck.ChangeAll)
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
                    _currentWord = _words[_wordsIndex].Text;
                    _currentSpellCheckWord = _words[_wordsIndex];
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
                        ShowEndStatusMessage(Configuration.Settings.Language.SpellCheck.SpellCheckCompleted);
                        DialogResult = DialogResult.OK;
                        return;
                    }
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
                    else if (IsNumber(_currentWord))
                    {
                        _noOfSkippedWords++;
                    }
                    else if (_spellCheckWordLists.HasUserWord(_currentWord))
                    {
                        _noOfCorrectWords++;
                    }
                    else if (_changeAllDictionary.ContainsKey(_currentWord))
                    {
                        _noOfChangedWords++;
                        _mainWindow.CorrectWord(_changeAllDictionary[_currentWord], _currentParagraph, _currentWord, ref _firstChange, -1);
                    }
                    else if (_changeAllDictionary.ContainsKey(_currentWord.Trim('\'')))
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
                            }
                            if (!correct && _wordsIndex < _words.Count - 1 && (_currentParagraph.Text[_words[_wordsIndex + 1].Index - 1] == '-' || _currentParagraph.Text[_words[_wordsIndex + 1].Index - 1] == '‑'))
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
                                    correct = _spellCheckWordLists.HasUserWord(wordWithDash);
                                }
                                if (!correct)
                                {
                                    correct = _spellCheckWordLists.HasUserWord(wordWithDash.Replace("‑", "-"));
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
                            else if (_languageName.StartsWith("en_", StringComparison.Ordinal) && (_currentWord.Equals("a", StringComparison.OrdinalIgnoreCase) || _currentWord == "I"))
                            {
                                correct = true;
                            }
                            else if (_languageName.StartsWith("da_", StringComparison.Ordinal) && _currentWord.Equals("i", StringComparison.OrdinalIgnoreCase))
                            {
                                correct = true;
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
                                if (_currentWord.ToUpperInvariant() != "LT'S" && _currentWord.ToUpperInvariant() != "SOX'S" && !_currentWord.ToUpperInvariant().StartsWith("HTTP", StringComparison.Ordinal)) // TODO: Get fixed nhunspell
                                {
                                    suggestions = DoSuggest(_currentWord); // TODO: 0.9.6 fails on "Lt'S"
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

                            if (AutoFixNames && _currentWord.Length > 1 && suggestions.Contains(char.ToUpper(_currentWord[0]) + _currentWord.Substring(1)))
                            {
                                ChangeWord = char.ToUpper(_currentWord[0]) + _currentWord.Substring(1);
                                DoAction(SpellCheckAction.ChangeAll);
                                return;
                            }
                            if (AutoFixNames && _currentWord.Length > 1)
                            {
                                if (_currentWord.Length > 3 && suggestions.Contains(_currentWord.ToUpperInvariant()))
                                { // does not work well with two letter words like "da" and "de" which get auto-corrected to "DA" and "DE"
                                    ChangeWord = _currentWord.ToUpperInvariant();
                                    DoAction(SpellCheckAction.ChangeAll);
                                    return;
                                }
                                if (_spellCheckWordLists.HasName(char.ToUpper(_currentWord[0]) + _currentWord.Substring(1)))
                                {
                                    ChangeWord = char.ToUpper(_currentWord[0]) + _currentWord.Substring(1);
                                    DoAction(SpellCheckAction.ChangeAll);
                                    return;
                                }
                                if (_currentWord.Length > 3 && _currentWord.StartsWith("mc", StringComparison.InvariantCultureIgnoreCase) && _spellCheckWordLists.HasName(char.ToUpper(_currentWord[0]) + _currentWord.Substring(1, 1) + char.ToUpper(_currentWord[2]) + _currentWord.Remove(0, 3)))
                                {
                                    ChangeWord = char.ToUpper(_currentWord[0]) + _currentWord.Substring(1, 1) + char.ToUpper(_currentWord[2]) + _currentWord.Remove(0, 3);
                                    DoAction(SpellCheckAction.ChangeAll);
                                    return;
                                }
                                if (_spellCheckWordLists.HasName(_currentWord.ToUpperInvariant()))
                                {
                                    ChangeWord = _currentWord.ToUpperInvariant();
                                    DoAction(SpellCheckAction.ChangeAll);
                                    return;
                                }
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
                                Initialize(_languageName, _currentSpellCheckWord, suggestions, _currentParagraph.Text, string.Format(Configuration.Settings.Language.Main.LineXOfY, (_currentIndex + 1), _subtitle.Paragraphs.Count));
                            }
                            else
                            {
                                _currentSpellCheckWord.Text = _currentWord;
                                Initialize(_languageName, _currentSpellCheckWord, suggestions, _currentParagraph.Text, string.Format(Configuration.Settings.Language.Main.LineXOfY, (_currentIndex + 1), _subtitle.Paragraphs.Count));
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

        private static readonly Regex RegexIsNumber = new Regex("^\\d+$", RegexOptions.Compiled);
        private static readonly Regex RegexIsEpisodeNumber = new Regex("^\\d+x\\d+$", RegexOptions.Compiled); // e.g. 12x02
        private static bool IsNumber(string s)
        {
            s = s.Trim('$', '£', '%', '*');
            if (RegexIsNumber.IsMatch(s))
            {
                return true;
            }

            if (RegexIsEpisodeNumber.IsMatch(s))
            {
                return true;
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
            LanguageStructure.Main mainLanguage = Configuration.Settings.Language.Main;
            if (_noOfChangedWords > 0 || _noOfAddedWords > 0 || _noOfSkippedWords > 0 || completedMessage == Configuration.Settings.Language.SpellCheck.SpellCheckCompleted)
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
            LanguageStructure.Main mainLanguage = Configuration.Settings.Language.Main;
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
                    string start = _languageName.Substring(0, 3);
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
            ShowActionInfo(Configuration.Settings.Language.SpellCheck.AddToUserDictionary, textBoxWord.Text);
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

            string format = Configuration.Settings.Language.SpellCheck.UndoX;
            if (string.IsNullOrEmpty(format))
            {
                format = "Undo: {0}";
            }

            string undoText = string.Format(format, text);

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
            string text = textBoxWord.Text.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                UiUtil.OpenURL("https://www.google.com/search?q=" + Utilities.UrlEncode(text));
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Configuration.Settings.General.PromptDeleteLines || MessageBox.Show(Configuration.Settings.Language.Main.DeleteOneLinePrompt, Configuration.Settings.Language.SpellCheck.Title, MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
            {
                DoAction(SpellCheckAction.DeleteLine);
            }
        }
    }
}
