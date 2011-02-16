using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using NHunspell;
using Nikse.SubtitleEdit.Forms;

namespace Nikse.SubtitleEdit.Logic.OCR
{
    public class OcrFixEngine
    {
        // Dictionaries/spellchecking/fixing
        Dictionary<string, string> _wordReplaceList;
        Dictionary<string, string> _partialLineReplaceList;
        Dictionary<string, string> _beginLineReplaceList;
        Dictionary<string, string> _endLineReplaceList;
        Dictionary<string, string> _wholeLineReplaceList;
        Dictionary<string, string> _partialWordReplaceList;
        string _replaceListXmlFileName;
        string _userWordListXmlFileName;
        string _fiveLetterWordListLanguageName;
        List<string> _namesEtcList = new List<string>();
        List<string> _namesEtcListUppercase = new List<string>();
        List<string> _namesEtcListWithApostrophe = new List<string>();
        List<string> _namesEtcMultiWordList = new List<string>(); // case sensitive phrases
        List<string> _abbreviationList;
        List<string> _userWordList = new List<string>();
        List<string> _wordSkipList = new List<string>();
        Hunspell _hunspell;
        readonly OcrSpellCheck _spellCheck;
        readonly Form _parentForm;
        private string _spellCheckDictionaryName;

        public bool Abort { get; set; }
        public List<string> AutoGuessesUsed { get; set; }
        public List<string> UnknownWordsFound { get; set; }
        public bool IsDictionaryLoaded { get; private set; }
        
        public CultureInfo DictionaryCulture { get; private set; }

        /// <summary>
        /// Advanced ocr fixing via replace/spelling dictionaries + some hardcoded rules
        /// </summary>
        /// <param name="threeLetterIsoLanguageName">E.g. eng for English</param>
        /// <param name="parentForm">Used for centering/show spellcheck dialog</param>
        public OcrFixEngine(string threeLetterIsoLanguageName, Form parentForm)
        {
            _parentForm = parentForm;          

            _spellCheck = new OcrSpellCheck {StartPosition = FormStartPosition.Manual};
            _spellCheck.Location = new Point(parentForm.Left + (parentForm.Width / 2 - _spellCheck.Width / 2),
                                             parentForm.Top + (parentForm.Height / 2 - _spellCheck.Height / 2));

            LoadReplaceLists(threeLetterIsoLanguageName);
            LoadSpellingDictionaries(threeLetterIsoLanguageName); // Hunspell etc.        

            AutoGuessesUsed = new List<string>();
            UnknownWordsFound = new List<string>();
        }

        private void LoadReplaceLists(string languageId)
        {
            _wordReplaceList = new Dictionary<string, string>();
            _partialLineReplaceList = new Dictionary<string, string>();
            _beginLineReplaceList = new Dictionary<string, string>();
            _endLineReplaceList = new Dictionary<string, string>();
            _wholeLineReplaceList = new Dictionary<string, string>();
            _partialWordReplaceList = new Dictionary<string, string>();

            _replaceListXmlFileName = Utilities.DictionaryFolder + languageId + "_OCRFixReplaceList.xml";
            if (File.Exists(_replaceListXmlFileName))
            {
                var doc = new XmlDocument();
                doc.Load(_replaceListXmlFileName);

                _wordReplaceList = LoadReplaceList(doc, "WholeWords");
                _partialWordReplaceList = LoadReplaceList(doc, "PartialWords");
                _partialLineReplaceList = LoadReplaceList(doc, "PartialLines");
                _beginLineReplaceList = LoadReplaceList(doc, "BeginLines");
                _endLineReplaceList = LoadReplaceList(doc, "EndLines");
                _wholeLineReplaceList = LoadReplaceList(doc, "WholeLines");
            }
        }

        private void LoadSpellingDictionaries(string threeLetterIsoLanguageName)
        {
            IsDictionaryLoaded = false;
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                if (culture.ThreeLetterISOLanguageName == threeLetterIsoLanguageName)
                {
                    string dictionaryFolder = Utilities.DictionaryFolder;
                    if (!Directory.Exists(dictionaryFolder))
                        return;

                    string dictionaryFileName = null;
                    foreach (string dic in Directory.GetFiles(dictionaryFolder, "*.dic"))
                    {
                        string name = Path.GetFileNameWithoutExtension(dic);
                        if (!name.StartsWith("hyph"))
                        {
                            try
                            {
                                var ci = new CultureInfo(name.Replace("_", "-"));
                                if (ci.ThreeLetterISOLanguageName == threeLetterIsoLanguageName || string.Compare(ci.ThreeLetterWindowsLanguageName, threeLetterIsoLanguageName, true) == 0)
                                {
                                    dictionaryFileName = dic;
                                    break;
                                }
                            }
                            catch (Exception exception)
                            {
                                System.Diagnostics.Debug.WriteLine(exception.Message);
                            }
                        }
                    }

                    if (dictionaryFileName == null)
                        return;

                    LoadSpellingDictionariesViaDictionaryFileName(threeLetterIsoLanguageName, culture, dictionaryFileName, true);
                    return;
                }
            }
            return;
        }

        private void LoadSpellingDictionariesViaDictionaryFileName(string threeLetterIsoLanguageName, CultureInfo culture, string dictionaryFileName, bool resetSkipList)
        {
            _fiveLetterWordListLanguageName = Path.GetFileName(dictionaryFileName).Substring(0, 5);
            string dictionary = Utilities.DictionaryFolder + _fiveLetterWordListLanguageName;
            if (resetSkipList)
            {
                _wordSkipList = new List<string>();
                _wordSkipList.Add(Configuration.Settings.Tools.MusicSymbol);
                _wordSkipList.Add("*");
                _wordSkipList.Add("%");
                _wordSkipList.Add("#");
                _wordSkipList.Add("+");
            }

            // Load names etc list (names/noise words)
            _namesEtcList = new List<string>();
            _namesEtcMultiWordList = new List<string>();
            Utilities.LoadNamesEtcWordLists(_namesEtcList, _namesEtcMultiWordList, _fiveLetterWordListLanguageName);

            _namesEtcListUppercase = new List<string>();
            foreach (string name in _namesEtcList)
                _namesEtcListUppercase.Add(name.ToUpper());

            _namesEtcListWithApostrophe = new List<string>();
            if (threeLetterIsoLanguageName.ToLower() == "eng")
            {
                foreach (string namesItem in _namesEtcList)
                {
                    if (!namesItem.EndsWith("s"))
                        _namesEtcListWithApostrophe.Add(namesItem + "'s");
                    else
                        _namesEtcListWithApostrophe.Add(namesItem + "'");
                }
            }

            // Load user words
            _userWordList = new List<string>();
            _userWordListXmlFileName = Utilities.LoadUserWordList(_userWordList, _fiveLetterWordListLanguageName);

            // Find abbreviations
            _abbreviationList = new List<string>();
            foreach (string name in _namesEtcList)
            {
                if (name.EndsWith("."))
                    _abbreviationList.Add(name);
            }
            foreach (string name in _userWordList)
            {
                if (name.EndsWith("."))
                    _abbreviationList.Add(name);
            }

            // Load NHunspell spellchecker
            _hunspell = new Hunspell(dictionary + ".aff", dictionary + ".dic");
            IsDictionaryLoaded = true;
            _spellCheckDictionaryName = dictionary;
            DictionaryCulture = culture;
        }

        public string SpellCheckDictionaryName
        {
            get
            {
                string[] parts = _spellCheckDictionaryName.Split(Path.DirectorySeparatorChar.ToString().ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                    return parts[parts.Length - 1];
                return string.Empty;
            }
            set
            {
                string _spellCheckDictionaryName = Path.Combine(Utilities.DictionaryFolder, value);
                CultureInfo ci;
                try
                {
                    ci = new CultureInfo(value);
                }
                catch
                {
                    ci = CultureInfo.CurrentCulture;
                }
                LoadSpellingDictionariesViaDictionaryFileName(ci.ThreeLetterISOLanguageName, ci, _spellCheckDictionaryName, false);
            }
        }

        internal static Dictionary<string, string> LoadReplaceList(XmlDocument doc, string name)
        {
            var list = new Dictionary<string, string>();
            if (doc.DocumentElement != null)
            {
                XmlNode node = doc.DocumentElement.SelectSingleNode(name);
                if (node != null)
                {
                    foreach (XmlNode item in node.ChildNodes)
                    {
                        if (item.Attributes != null && item.Attributes["to"] != null && item.Attributes["from"] != null)
                        {
                            string to = item.Attributes["to"].InnerText;
                            string from = item.Attributes["from"].InnerText;
                            if (!list.ContainsKey(from))
                                list.Add(from, to);
                        }
                    }
                }
            }
            return list;
        }

        public string FixOcrErrors(string text, int index, string lastLine, bool logSuggestions, bool useAutoGuess)
        {
            var sb = new StringBuilder();
            var word = new StringBuilder();
            
            text = FixCommenOcrLineErrors(text, lastLine);

            string lastWord = null;
            for (int i = 0; i < text.Length; i++)
            {
                if (" ,.!?:;()[]{}+-$£\"#&%\r\n".Contains(text[i].ToString()))
                {
                    if (word.Length > 0)
                    {
                        string fixedWord;
                        if (lastWord != null && lastWord.ToUpper().Contains("COLOR="))
                        {
                            fixedWord = word.ToString();
                        }
                        else
                        {
                            fixedWord = FixCommonWordErrors(word.ToString(), lastWord);
                        }
                        sb.Append(fixedWord);
                        lastWord = fixedWord;
                        word = new StringBuilder();
                    }
                    sb.Append(text[i]);
                }
                else
                {
                    word.Append(text[i]);
                }
            }
            if (word.Length > 0) // last word
            {
                string fixedWord = FixCommonWordErrors(word.ToString(), lastWord);
                sb.Append(fixedWord);
            }
            text = FixCommenOcrLineErrors(sb.ToString(), lastLine);
            int wordsNotFound;
            return FixUnknownWordsViaGuessOrPrompt(out wordsNotFound, text, index, null, true, false, logSuggestions, useAutoGuess);
        }


        private string FixCommonWordErrors(string word, string lastWord)
        {
            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                while (word.Contains("--"))
                    word = word.Replace("--", "-");

                if (word.Contains("’"))
                    word = word.Replace('’', '\'');

                if (word.Contains("`"))
                    word = word.Replace('`', '\'');

                if (word.Contains("‘"))
                    word = word.Replace('‘', '\'');

                if (word.Contains("—"))
                    word = word.Replace('—', '-');

                if (word.Contains("|"))
                    word = word.Replace("|", "l");

                if (word.Contains("vx/"))
                    word = word.Replace("vx/", "w");

                if (word.Contains("¤"))
                {
                    var regex = new Regex("[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏa-zæøåäöéèàùâêîôûëï]¤");
                    if (regex.IsMatch(word))
                        word = word.Replace("¤", "o");
                }
            }

            string pre = string.Empty;
            string post = string.Empty;

            while (word.StartsWith(Environment.NewLine) && word.Length > 2)
            {
                pre += Environment.NewLine;
                word = word.Substring(2);
            }

            while (word.StartsWith("-") && word.Length > 1)
            {
                pre += "-";
                word = word.Substring(1);
            }
            while (word.StartsWith(".") && word.Length > 1)
            {
                pre += ".";
                word = word.Substring(1);
            }
            while (word.StartsWith("\"") && word.Length > 1)
            {
                pre += "\"";
                word = word.Substring(1);
            }
            if (word.StartsWith("(") && word.Length > 1)
            {
                pre += "(";
                word = word.Substring(1);
            }
            while (word.EndsWith(Environment.NewLine) && word.Length > 2)
            {
                post += Environment.NewLine;
                word = word.Substring(0, word.Length - 2);
            }
            while (word.EndsWith("\"") && word.Length > 1)
            {
                post = post + "\"";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith(".") && word.Length > 1)
            {
                post = post + ".";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith(",") && word.Length > 1)
            {
                post = post + ",";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith("?") && word.Length > 1)
            {
                post = post + "?";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith("!") && word.Length > 1)
            {
                post = post + "!";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith(")") && word.Length > 1)
            {
                post = post + ")";
                word = word.Substring(0, word.Length - 1);
            }

            if (word.Contains("?"))
            {
                var regex = new Regex(@"\S\?[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏa-zæøåäöéèàùâêîôûëï]");
                Match match = regex.Match(word);
                if (match.Success)
                    word = word.Insert(match.Index + 2, " ");
            }

            foreach (string from in _wordReplaceList.Keys)
            {
                if (word == from)
                    return pre + _wordReplaceList[from] + post;
                if (word + post == from)
                    return pre + _wordReplaceList[from];
                if (pre + word + post == from)
                    return _wordReplaceList[from];
            }

            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                // uppercase I or 1 inside lowercase word (will be replaced by lowercase L)
                word = FixIor1InsideLowerCaseWord(word);

                // uppercase 0 inside lowercase word (will be replaced by lowercase L)
                word = Fix0InsideLowerCaseWord(word);

                // uppercase I or 1 inside lowercase word (will be replaced by lowercase L)
                word = FixIor1InsideLowerCaseWord(word);

                word = FixLowerCaseLInsideUpperCaseWord(word); // eg. SCARLETTl => SCARLETTI
            }

            // Retry word replace list
            foreach (string from in _wordReplaceList.Keys)
            {
                if (word == from)
                    return pre + _wordReplaceList[from] + post;
                if (word + post == from)
                    return pre + _wordReplaceList[from];
                if (pre + word + post == from)
                    return _wordReplaceList[from];
            }

            return pre + word + post;
        }

        public static string Fix0InsideLowerCaseWord(string word)
        {
            var startEndEndsWithNumber = new Regex(@"^\d+.+\d$");
            if (startEndEndsWithNumber.IsMatch(word))
                return word;

            if (word.Contains("1") ||
                word.Contains("2") ||
                word.Contains("3") ||
                word.Contains("4") ||
                word.Contains("5") ||
                word.Contains("6") ||
                word.Contains("7") ||
                word.Contains("8") ||
                word.Contains("9"))
                return word;

            var hexNumber = new Regex(@"^#?[\dABDEFabcdef]+$");
            if (hexNumber.IsMatch(word))
                return word;

            if (word.LastIndexOf('0') > 0)
            {
                var re = new Regex(@"[a-zæøåäöé][0]", RegexOptions.Compiled);
                Match match = re.Match(word);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        if (word[match.Index + 1] == '0')
                        {
                            string oldText = word;
                            word = word.Substring(0, match.Index + 1) + "o";
                            if (match.Index + 2 < oldText.Length)
                                word += oldText.Substring(match.Index + 2);
                        }
                        match = re.Match(word);
                    }
                }

                re = new Regex(@"0[a-zæøåäöé]", RegexOptions.Compiled);
                match = re.Match(word);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        if (word[match.Index] == '0')
                        {
                            if (match.Index == 0 || !"123456789".Contains(word[match.Index - 1].ToString()))
                            {
                                string oldText = word;
                                word = word.Substring(0, match.Index) + "o";
                                if (match.Index + 1 < oldText.Length)
                                    word += oldText.Substring(match.Index + 1);
                            }
                        }
                        match = re.Match(word, match.Index + 1);
                    }
                }
            }
            return word;
        }

        public static string FixIor1InsideLowerCaseWord(string word)
        {
            var startEndEndsWithNumber = new Regex(@"^\d+.+\d$");
            if (startEndEndsWithNumber.IsMatch(word))
                return word;

            if (word.Contains("2") ||
                word.Contains("3") ||
                word.Contains("4") ||
                word.Contains("5") ||
                word.Contains("6") ||
                word.Contains("7") ||
                word.Contains("8") ||
                word.Contains("9"))
                return word;

            var hexNumber = new Regex(@"^#?[\dABDEFabcdef]+$");
            if (hexNumber.IsMatch(word))
                return word;

            if (word.LastIndexOf('I') > 0 || word.LastIndexOf('1') > 0)
            {
                var re = new Regex(@"[a-zæøåäöé][I1]", RegexOptions.Compiled);
                Match match = re.Match(word);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        if (word[match.Index + 1] == 'I' || word[match.Index + 1] == '1')
                        {
                            string oldText = word;
                            word = word.Substring(0, match.Index + 1) + "l";
                            if (match.Index + 2 < oldText.Length)
                                word += oldText.Substring(match.Index + 2);
                        }
                        match = re.Match(word, match.Index + 1);
                    }
                }
            }
            return word;
        }

        public static string FixLowerCaseLInsideUpperCaseWord(string word)
        {
            if (word.Length > 3 && word.Replace("l", string.Empty).ToUpper() == word.Replace("l", string.Empty))
            {
                if (!word.Contains("<") && !word.Contains(">") && !word.Contains("'"))
                {
                    word = word.Replace("l", "I");
                }
            }
            return word;
        }

        private string FixCommenOcrLineErrors(string input, string lastLine)
        {
            input = FixOcrErrorsViaHardcodedRules(input, lastLine, _abbreviationList);
            input = FixOcrErrorViaLineReplaceList(input);

            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                // e.g. "selectionsu." -> "selections..."
                if (input.EndsWith("u.") && _hunspell != null)
                {
                    string[] words = input.Split(" .".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        string lastWord = words[words.Length - 1].Trim();
                        if (lastWord.Length > 2 &&
                            !IsWordOrWordsCorrect(_hunspell, lastWord) &&
                            IsWordOrWordsCorrect(_hunspell, lastWord.Substring(0, lastWord.Length - 1)))
                            input = input.Substring(0, input.Length - 2) + "...";
                    }
                }

                // music notes
                if (input.StartsWith(".'") && input.EndsWith(".'"))
                {
                    input = input.Replace(".'", Configuration.Settings.Tools.MusicSymbol);
                }
            }

            return input;
        }

        private static bool EndsWithAbbreviation(string line, List<string> abbreviationList)
        {
            if (string.IsNullOrEmpty(line) || abbreviationList == null)
                return false;

            abbreviationList.Add("a.m.");
            abbreviationList.Add("p.m.");
            abbreviationList.Add("o.r.");
            foreach (string abbreviation in abbreviationList)
            {
                if (line.ToLower().EndsWith(" " + abbreviation.ToLower()))
                    return true;
            }
            return false;
        }

        public static string FixOcrErrorsViaHardcodedRules(string input, string lastLine, List<string> abbreviationList)
        {
            if (!Configuration.Settings.Tools.OcrFixUseHardcodedRules)
                return input;

            if (lastLine == null ||
                lastLine.EndsWith(".") ||
                lastLine.EndsWith("!") ||
                lastLine.EndsWith("?"))
            {
                if (lastLine == null || (!lastLine.EndsWith("...") && !EndsWithAbbreviation(lastLine, abbreviationList)))
                {
                    if (input.Length > 0 && input[0].ToString() != input[0].ToString().ToUpper())
                        input = input.Remove(0, 1).Insert(0, input[0].ToString().ToUpper());
                }
            }

            // lines ending with ". should often end at ... (of no other quotes exists near by)
            if ((lastLine == null || !lastLine.Contains("\"")) && input != null &&
                input.EndsWith("\".") && input.IndexOf("\"") == input.LastIndexOf("\""))
            {
                int position = input.Length - 2;
                input = input.Remove(position).Insert(position, "...");
            }

            // change '<number><space>1' to '<number>1'
            var regEx = new Regex(@"\d\ 1");
            Match match = regEx.Match(input);
            while (match.Success)
            {
                input = input.Substring(0, match.Index + 1) + input.Substring(match.Index + 2);
                match = regEx.Match(input);
            }

            // change '' to "
            input = input.Replace("''", "\"");

            // change 'sequeI of' to 'sequel of'
            regEx = new Regex("[a-zæøåöäé]I.");
            match = regEx.Match(input);
            while (match.Success)
            {
                input = input.Substring(0, match.Index + 1) + "l" + input.Substring(match.Index + 2);
                match = regEx.Match(input);
            }

            // change 'NlCE' to 'NICE'
            regEx = new Regex("[A-ZÆØÅÄÖÉÁ]l[A-ZÆØÅÄÖÉÁ]");
            match = regEx.Match(input);
            while (match.Success)
            {
                input = input.Substring(0, match.Index + 1) + "I" + input.Substring(match.Index + 2);
                match = regEx.Match(input);
            }

            return input;
        }

        public string FixOcrErrorViaLineReplaceList(string input)
        {
            // Whole line
            foreach (string from in _wholeLineReplaceList.Keys)
            {
                if (input == from)
                    return _wholeLineReplaceList[from];
            }

            string newText = input;
            string pre = string.Empty;
            while (newText.Length > 1 && " -\"['¶(".Contains(newText.Substring(0, 1)))
            {
                pre += newText.Substring(0, 1);
                newText = newText.Substring(1);
            }

            // begin line
            foreach (string from in _beginLineReplaceList.Keys)
            {
                if (newText.StartsWith(from))
                    newText = newText.Remove(0, from.Length).Insert(0, _beginLineReplaceList[from]);
                if (newText.Contains(". " + from))
                    newText = newText.Replace(". " + from, ". " + _beginLineReplaceList[from]);
                if (newText.Contains("! " + from))
                    newText = newText.Replace("! " + from, "! " + _beginLineReplaceList[from]);
                if (newText.Contains("? " + from))
                    newText = newText.Replace("? " + from, "? " + _beginLineReplaceList[from]);
                if (newText.Contains("." + Environment.NewLine + from))
                    newText = newText.Replace(". " + Environment.NewLine + from, ". " + Environment.NewLine + _beginLineReplaceList[from]);
                if (newText.Contains("! " + Environment.NewLine + from))
                    newText = newText.Replace("! " + Environment.NewLine + from, "! " + Environment.NewLine + _beginLineReplaceList[from]);
                if (newText.Contains("? " + Environment.NewLine + from))
                    newText = newText.Replace("? " + Environment.NewLine + from, "? " + Environment.NewLine + _beginLineReplaceList[from]);
            }
            newText = pre + newText;

            foreach (string from in _endLineReplaceList.Keys)
            {
                if (newText.EndsWith(from))
                {
                    int position = (newText.Length - from.Length);
                    newText = newText.Remove(position).Insert(position, _endLineReplaceList[from]);
                }
            }

            foreach (string from in _partialLineReplaceList.Keys)
            {
                if (newText.Contains(from))
                {
                    var regex = new Regex(@"\b" + from + @"\b");
                    Match match = regex.Match(newText);
                    if (match.Success)
                    {
                        newText = newText.Remove(match.Index, match.Value.Length).Insert(match.Index, _partialLineReplaceList[from]);
                    }
                }
            }
            return newText;
        }

        public string FixUnknownWordsViaGuessOrPrompt(out int wordsNotFound, string line, int index, Bitmap bitmap, bool autoFix, bool promptForFixingErrors, bool log, bool useAutoGuess)
        {
            wordsNotFound = 0;
            if (_hunspell == null)
                return line;

            string[] words = line.Split((Environment.NewLine + " ,.!?:;()[]{}+-$£\"”“#&%…—").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (!IsWordKnownOrNumber(word, line))
                {
                    bool correct = _hunspell.Spell(word);
                    if (!correct)
                        correct = _hunspell.Spell(word.Trim('\''));

                    if (!correct)
                    {
                        wordsNotFound++;
                        if (log)
                            UnknownWordsFound.Add(string.Format("#{0}: {1}", index + 1, word));

                        if (autoFix && useAutoGuess)
                        {
                            List<string> guesses = new List<string>();

                            if (word.Length > 5)
                            {
                                guesses = (List<string>)CreateGuessesFromLetters(word);
                            }
                            else
                            {
                                if (i==0)
                                    guesses.Add(word.Replace(@"\/", "V"));
                                else
                                    guesses.Add(word.Replace(@"\/", "v"));
                                guesses.Add(word.Replace("ﬁ", "fi"));
                                guesses.Add(word.Replace("ﬁ", "fj"));
                                guesses.Add(word.Replace("ﬂ", "fl"));
                                if (!word.EndsWith("€") && !word.StartsWith("€"))
                                    guesses.Add(word.Replace("€", "e"));
                            }
                            foreach (string guess in guesses)
                            {
                                if (IsWordOrWordsCorrect(_hunspell, guess))
                                {
                                    var regex = new Regex(@"\b" + word + @"\b");
                                    Match match = regex.Match(line);
                                    if (match.Success)
                                    {
                                        if (log)
                                            AutoGuessesUsed.Add(string.Format("#{0}: {1} -> {2} in line via '{3}': {4}", index + 1, word, guess, "OCRFixReplaceList.xml", line.Replace(Environment.NewLine, " ")));

                                        line = line.Remove(match.Index, match.Value.Length).Insert(match.Index, guess);
                                        wordsNotFound--;
                                        correct = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!correct && promptForFixingErrors)
                        {
                            List<string> suggestions = _hunspell.Suggest(word);
                            SpellcheckOcrTextResult res = SpellcheckOcrText(line, bitmap, words, i, word, suggestions);
                            if (res.FixedWholeLine)
                            {
                                return res.Line;
                            }
                            if (res.Fixed)
                            {
                                line = res.Line;
                                wordsNotFound--;
                            }
                        }
                    }
                }
            }
            return line;
        }

        /// <summary>
        /// Spellcheck for ocr
        /// </summary>
        /// <returns>True, if word is fixed</returns>
        private SpellcheckOcrTextResult SpellcheckOcrText(string line, Bitmap bitmap, string[] words, int i, string word, List<string> suggestions)
        {
            var result = new SpellcheckOcrTextResult { Fixed = false, FixedWholeLine = false, Line = null, Word = null };
            _spellCheck.Initialize(word, suggestions, line, words, i, bitmap);
            _spellCheck.ShowDialog(_parentForm);
            switch (_spellCheck.ActionResult)
            {
                case OcrSpellCheck.Action.Abort:
                    Abort = true;
                    result.FixedWholeLine = true;
                    result.Line = line;
                    break;
                case OcrSpellCheck.Action.AddToUserDictionary:
                    if (_userWordListXmlFileName != null)
                    {
                        _userWordList.Add(_spellCheck.Word.Trim().ToLower());
                        Utilities.AddToUserDictionary(_spellCheck.Word.Trim().ToLower(), _fiveLetterWordListLanguageName);
                    }
                    result.Word = _spellCheck.Word;
                    result.Fixed = true;
                    result.Line = line;
                    if (word == result.Word)
                        return result;
                    break;
                case OcrSpellCheck.Action.AddToNames:
                    result.Word = _spellCheck.Word;
                    result.Fixed = true;
                    try
                    {
                        string s = _spellCheck.Word.Trim();
                        if (s.Contains(" "))
                            _namesEtcMultiWordList.Add(s);
                        else
                            _namesEtcList.Add(s);
                        Utilities.AddWordToLocalNamesEtcList(s, _fiveLetterWordListLanguageName);
                    }
                    catch
                    {
                        _wordSkipList.Add(_spellCheck.Word);
                    }
                    result.Line = line;
                    if (word == result.Word)
                        return result;
                    break;
                case OcrSpellCheck.Action.AllwaysUseSuggestion:
                    SaveWordToWordList(word);
                    result.Fixed = true;
                    result.Word = _spellCheck.Word;
                    break;
                case OcrSpellCheck.Action.ChangeAndSave:
                    SaveWordToWordList(word);
                    result.Fixed = true;
                    result.Word = _spellCheck.Word;
                    break;
                case OcrSpellCheck.Action.ChangeOnce:
                    result.Fixed = true;
                    result.Word = _spellCheck.Word;
                    break;
                case OcrSpellCheck.Action.ChangeWholeText:
                    result.Line = _spellCheck.Paragraph;
                    result.FixedWholeLine = true;
                    break;
                case OcrSpellCheck.Action.SkipAll:
                    _wordSkipList.Add(_spellCheck.Word);
                    break;
                case OcrSpellCheck.Action.SkipOnce:
                    break;
                case OcrSpellCheck.Action.SkipWholeText:
                    result.Line = line;
                    result.FixedWholeLine = true;
                    break;
                case OcrSpellCheck.Action.UseSuggestion:
                    result.Word = _spellCheck.Word;
                    result.Fixed = true;
                    break;
                default:
                    break;
            }
            if (result.Fixed)
            {
                var regEx = Utilities.MakeWordSearchRegex(word);
                Match match = regEx.Match(line);
                if (match.Success)
                {
                    result.Line = line.Remove(match.Index, word.Length).Insert(match.Index, result.Word);
                }
                else // some word containing a number or other strange character
                {
                    if (line.EndsWith(" " + word))
                    {
                        result.Line = line.Substring(0, line.Length - word.Length) + result.Word;
                    }
                    else if (line.StartsWith(word + " ") || line.StartsWith(word + ",") || line.StartsWith(word + "."))
                    {
                        result.Line = result.Word + line.Substring(word.Length);
                    }
                    else
                    {
                        regEx = Utilities.MakeWordSearchRegexWithNumbers(word);
                        match = regEx.Match(line);
                        if (match.Success)
                        {
                            int startIndex = match.Index;
                            if (match.Value.StartsWith(" "))
                                startIndex++;
                            result.Line = line.Remove(startIndex, word.Length).Insert(startIndex, result.Word);
                        }
                        else
                        {
                            result.Fixed = false;
                            MessageBox.Show("Unable to find word via regex: " + word);
                        }
                    }
                }
            }
            return result;
        }

        private void SaveWordToWordList(string word)
        {
            try
            {
                if (_replaceListXmlFileName != null)
                {
                    var doc = new XmlDocument();
                    if (File.Exists(_replaceListXmlFileName))
                    {
                        try
                        {
                            doc.Load(_replaceListXmlFileName);
                        }
                        catch
                        {
                            doc.LoadXml("<ReplaceList><WholeWords/><PartialLines/><BeginLines/><EndLines/><WholeLines/></ReplaceList>");
                        }
                    }
                    else
                    {
                        doc.LoadXml("<ReplaceList><WholeWords/><PartialLines/><BeginLines/><EndLines/><WholeLines/></ReplaceList>");                    
                    }
                    if (!_wordReplaceList.ContainsKey(word))
                        _wordReplaceList.Add(word, _spellCheck.Word);
                    XmlNode wholeWordsNode = doc.DocumentElement.SelectSingleNode("WholeWords");
                    if (wholeWordsNode != null)
                    {
                        XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "Word", null);
                        XmlAttribute aFrom = doc.CreateAttribute("from");
                        XmlAttribute aTo = doc.CreateAttribute("to");
                        aFrom.InnerText = word;
                        aTo.InnerText = _spellCheck.Word;
                        newNode.Attributes.Append(aFrom);
                        newNode.Attributes.Append(aTo);
                        wholeWordsNode.AppendChild(newNode);
                        doc.Save(_replaceListXmlFileName);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception + Environment.NewLine + exception.StackTrace);
                _wordSkipList.Add(word);
            }
        }

        public bool IsWordOrWordsCorrect(Hunspell hunspell, string word)
        {
            foreach (string s in word.Split(' '))
            {
                if (!hunspell.Spell(s))
                {
                    if (!IsWordKnownOrNumber(word, word))
                        return false;
                }
            }
            return true;
        }

        private static string AddToGuessList(List<string> list, string word, int index, string letter, string replaceLetters)
        {
            if (string.IsNullOrEmpty(word) || index < 0 || index + letter.Length - 1 >= word.Length)
                return word;

            string s = word.Remove(index, letter.Length);
            if (index >= s.Length)
                s += replaceLetters;
            else
                s = s.Insert(index, replaceLetters);

            if (!list.Contains(s))
                list.Add(s);

            return s;
        }

        public IEnumerable<string> CreateGuessesFromLetters(string word)
        {
            var list = new List<string>();
            foreach (string letter in _partialWordReplaceList.Keys)
            {
                string s = word;
                int i = 0;
                while (s.Contains(letter) && i < 10)
                {
                    int index = s.IndexOf(letter);
                    s = AddToGuessList(list, s, index, letter, _partialWordReplaceList[letter]);
                    AddToGuessList(list, word, index, letter, _partialWordReplaceList[letter]);
                    i++;
                }
                s = word;
                i = 0;
                while (s.Contains(letter) && i < 10)
                {
                    int index = s.LastIndexOf(letter);
                    s = AddToGuessList(list, s, index, letter, _partialWordReplaceList[letter]);
                    AddToGuessList(list, word, index, letter, _partialWordReplaceList[letter]);
                    i++;
                }
            }
            return list;
        }

        public bool IsWordKnownOrNumber(string word, string line)
        {
            double number;
            if (double.TryParse(word.TrimStart('\'').Replace("$", string.Empty).Replace("£", string.Empty).Replace("¢", string.Empty), out number))
                return true;

            if (_wordSkipList.IndexOf(word) >= 0)
                return true;

            if (_namesEtcList.IndexOf(word) >= 0)
                return true;

            if (_namesEtcList.IndexOf(word.Trim('\'')) >= 0)
                return true;

            if (_userWordList.IndexOf(word.ToLower()) >= 0)
                return true;

            if (_userWordList.IndexOf(word.Trim('\'').ToLower()) >= 0)
                return true;

            if (word.Length > 2 && _namesEtcListUppercase.IndexOf(word) >= 0)
                return true;

            if (word.Length > 2 && _namesEtcListWithApostrophe.IndexOf(word) >= 0)
                return true;

            if (Utilities.IsInNamesEtcMultiWordList(_namesEtcMultiWordList, line, word))
                return true;

            return false;
        }

        public int CountUnknownWordsViaDictionary(string line, out int numberOfCorrectWords)
        {
            numberOfCorrectWords = 0;
            if (_hunspell == null)
                return 0;

            int wordsNotFound = 0;
            string[] words = line.Split((Environment.NewLine + " ,.!?:;()[]{}+-$£\"#&%…“”").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (!IsWordKnownOrNumber(word, line))
                {
                    bool correct = _hunspell.Spell(word);
                    if (!correct)
                        correct = _hunspell.Spell(word.Trim('\''));

                    if (correct)
                        numberOfCorrectWords++;
                    else
                        wordsNotFound++;

                }
            }
            return wordsNotFound;
        }

    }
}
