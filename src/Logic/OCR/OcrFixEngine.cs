using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic.SpellCheck;

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
        Dictionary<string, string> _partialWordReplaceListAlways;
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

        static Regex regexAloneI = new Regex(@"\bi\b", RegexOptions.Compiled);
        static Regex regexAloneIAsL = new Regex(@"\bl\b", RegexOptions.Compiled);
        static Regex regexSpaceBetweenNumbers = new Regex(@"\d \d", RegexOptions.Compiled);
        static Regex regExLowercaseL = new Regex("[A-ZÆØÅÄÖÉÁ]l[A-ZÆØÅÄÖÉÁ]", RegexOptions.Compiled);
        static Regex regExUppercaseI = new Regex("[a-zæøåöäé]I.", RegexOptions.Compiled);
        static Regex regExNumber1 = new Regex(@"\d\ 1", RegexOptions.Compiled);
        static Regex regExQuestion = new Regex(@"\S\?[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏa-zæøåäöéèàùâêîôûëï]", RegexOptions.Compiled);
        static Regex regExIandZero = new Regex(@"[a-zæøåäöé][I1]", RegexOptions.Compiled);
        static Regex regExTime1 = new Regex(@"[a-zæøåäöé][0]", RegexOptions.Compiled);
        static Regex regExTime2 = new Regex(@"0[a-zæøåäöé]", RegexOptions.Compiled);
        static Regex hexNumber = new Regex(@"^#?[\dABDEFabcdef]+$", RegexOptions.Compiled);
        static Regex startEndEndsWithNumber = new Regex(@"^\d+.+\d$", RegexOptions.Compiled);

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
            _partialWordReplaceListAlways = new Dictionary<string, string>();
            _partialWordReplaceList = new Dictionary<string, string>();

            _replaceListXmlFileName = Utilities.DictionaryFolder + languageId + "_OCRFixReplaceList.xml";
            if (File.Exists(_replaceListXmlFileName))
            {
                var doc = new XmlDocument();
                doc.Load(_replaceListXmlFileName);

                _wordReplaceList = LoadReplaceList(doc, "WholeWords");
                _partialWordReplaceListAlways = LoadReplaceList(doc, "PartialWordsAlways");
                _partialWordReplaceList = LoadReplaceList(doc, "PartialWords");
                _partialLineReplaceList = LoadReplaceList(doc, "PartialLines");
                _beginLineReplaceList = LoadReplaceList(doc, "BeginLines");
                _endLineReplaceList = LoadReplaceList(doc, "EndLines");
                _wholeLineReplaceList = LoadReplaceList(doc, "WholeLines");
            }
        }

        private void LoadSpellingDictionaries(string threeLetterIsoLanguageName)
        {
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
                return;

            // use US default
            if (threeLetterIsoLanguageName == "eng" && File.Exists(Path.Combine(dictionaryFolder, "en_US.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", new CultureInfo("en-US"), "en_US.dic", true);
                return;
            }

            foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                if (culture.ThreeLetterISOLanguageName == threeLetterIsoLanguageName)
                {

                    string dictionaryFileName = null;
                    foreach (string dic in Directory.GetFiles(dictionaryFolder, "*.dic"))
                    {
                        string name = Path.GetFileNameWithoutExtension(dic);
                        if (!name.StartsWith("hyph"))
                        {
                            try
                            {
                                name = name.Replace("_", "-");
                                if (name.Length > 5)
                                    name = name.Substring(0, 5);
                                var ci = new CultureInfo(name);
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
            _fiveLetterWordListLanguageName = Path.GetFileNameWithoutExtension(dictionaryFileName);
            if (_fiveLetterWordListLanguageName.Length > 5)
                _fiveLetterWordListLanguageName = _fiveLetterWordListLanguageName.Substring(0, 5);
            string dictionary = Utilities.DictionaryFolder + _fiveLetterWordListLanguageName;
            if (resetSkipList)
            {
                _wordSkipList = new List<string>();
                _wordSkipList.Add(Configuration.Settings.Tools.MusicSymbol);
                _wordSkipList.Add("*");
                _wordSkipList.Add("%");
                _wordSkipList.Add("#");
                _wordSkipList.Add("+");
                _wordSkipList.Add("$");
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
            if (threeLetterIsoLanguageName.ToLower() == "eng")
            {
                if (_abbreviationList.Contains("a.m."))
                    _abbreviationList.Add("a.m.");
                if (_abbreviationList.Contains("p.m."))
                    _abbreviationList.Add("p.m.");
                if (_abbreviationList.Contains("o.r."))
                    _abbreviationList.Add("o.r.");
            }

            foreach (string name in _userWordList)
            {
                if (name.EndsWith("."))
                    _abbreviationList.Add(name);
            }

            // Load Hunspell spellchecker
            try
            {
                if (!File.Exists(dictionary + ".dic"))
                {
                    var fileMatches = Directory.GetFiles(Utilities.DictionaryFolder, _fiveLetterWordListLanguageName + "*.dic");
                    if (fileMatches.Length > 0)
                        dictionary = fileMatches[0].Substring(0, fileMatches[0].Length - 4);
                }
                _hunspell = Hunspell.GetHunspell(dictionary);
                IsDictionaryLoaded = true;
                _spellCheckDictionaryName = dictionary;
                DictionaryCulture = culture;
            }
            catch
            {
                IsDictionaryLoaded = false;
            }

        }

        public string SpellCheckDictionaryName
        {
            get
            {
                if (_spellCheckDictionaryName == null)
                    return string.Empty;

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
                    if (value == "sh")
                        ci = new CultureInfo("sr-Latn-RS");
                    else
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

            text = ReplaceWordsBeforeLineFixes(text);

            text = FixCommenOcrLineErrors(text, lastLine);

            string lastWord = null;
            for (int i = 0; i < text.Length; i++)
            {
                if (" ¡¿,.!?:;()[]{}+-£\"#&%\r\n".Contains(text[i].ToString())) // removed $
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
                            bool doFixWord = true;
                            if (word.Length == 1 && sb.ToString().EndsWith("-") && sb.Length > 1)
                                doFixWord = false;
                            if (doFixWord)
                                fixedWord = FixCommonWordErrors(word.ToString(), lastWord);
                            else
                                fixedWord = word.ToString();
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
                string fixedWord;
                bool doFixWord = true;
                if (word.Length == 1 && sb.ToString().EndsWith("-") && sb.Length > 1)
                    doFixWord = false;
                if (doFixWord)
                    fixedWord = FixCommonWordErrors(word.ToString(), lastWord);
                else
                    fixedWord = word.ToString();

                sb.Append(fixedWord);
            }

            text = FixCommenOcrLineErrors(sb.ToString(), lastLine);
            int wordsNotFound;
            text =  FixUnknownWordsViaGuessOrPrompt(out wordsNotFound, text, index, null, true, false, logSuggestions, useAutoGuess);
            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                text = FixLowercaseIToUppercaseI(text, lastLine);
                if (SpellCheckDictionaryName.StartsWith("en_"))
                {
                    string oldText = text;
                    text = FixCommonErrors.FixAloneLowercaseIToUppercaseLine(regexAloneI, oldText, text, 'i');
                    text = FixCommonErrors.FixAloneLowercaseIToUppercaseLine(regexAloneIAsL, oldText, text, 'l');
                }
                text = RemoveSpaceBetweenNumbers(text);
            }
            return text;
        }

        private string ReplaceWordsBeforeLineFixes(string text)
        {
            string lastWord = null;
            var sb = new StringBuilder();
            var word = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (" ¡¿,.!?:;()[]{}+-£\"#&%\r\n".Contains(text[i].ToString())) // removed $
                {
                    if (word.Length > 0)
                    {
                        string fixedWord;
                        if (lastWord != null && lastWord.ToUpper().Contains("COLOR="))
                            fixedWord = word.ToString();
                        else
                            fixedWord = FixCommonWordErrorsQuick(word.ToString(), lastWord);
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
                string fixedWord = FixCommonWordErrorsQuick(word.ToString(), lastWord);
                sb.Append(fixedWord);
            }
            return sb.ToString();
        }

        private string RemoveSpaceBetweenNumbers(string text)
        {
            Match match = regexSpaceBetweenNumbers.Match(text);
            while (match.Success)
            {
                bool doFix = true;

                if (match.Index + 4 < text.Length && text[match.Index + 3] == '/' && "0123456789".Contains(text[match.Index + 4].ToString()))
                    doFix = false;

                if (doFix)
                {
                    text = text.Remove(match.Index + 1, 1);
                    match = regexSpaceBetweenNumbers.Match(text);
                }
                else
                {
                    match = regexSpaceBetweenNumbers.Match(text, match.Index+1);
                }
            }
            return text;
        }

        private string FixCommonWordErrors(string word, string lastWord)
        {
            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                word = word.Replace("ﬁ", "fi");

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

            //always replace list
            foreach (string letter in _partialWordReplaceListAlways.Keys)
                word = word.Replace(letter,_partialWordReplaceListAlways[letter]);

            string pre = string.Empty;
            string post = string.Empty;

            if (word.StartsWith("<i>"))
            {
                pre += "<i>";
                word = word.Remove(0, 3);
            }
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
            if (word.StartsWith("<i>"))
            {
                pre += "<i>";
                word = word.Remove(0, 3);
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
            if (word.EndsWith("</i>"))
            {
                post = post + "</i>";
                word = word.Remove(word.Length - 4, 4);
            }
            if (word.Length == 0)
                return pre + word + post;

            if (word.Contains("?"))
            {
                Match match = regExQuestion.Match(word);
                if (match.Success)
                    word = word.Insert(match.Index + 2, " ");
            }

            foreach (string from in _wordReplaceList.Keys)
            {
                if (from.Contains(word))
                {
                    if (word == from)
                        return pre + _wordReplaceList[from] + post;
                    if (word + post == from)
                        return pre + _wordReplaceList[from];
                    if (pre + word + post == from)
                        return _wordReplaceList[from];
                }
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
                if (from.Contains(word))
                {
                    if (word == from)
                        return pre + _wordReplaceList[from] + post;
                    if (word + post == from)
                        return pre + _wordReplaceList[from];
                    if (pre + word + post == from)
                        return _wordReplaceList[from];
                }
            }

            return pre + word + post;
        }

        private string FixCommonWordErrorsQuick(string word, string lastWord)
        {
            //always replace list
            foreach (string letter in _partialWordReplaceListAlways.Keys)
                word = word.Replace(letter, _partialWordReplaceListAlways[letter]);

            string pre = string.Empty;
            string post = string.Empty;

            if (word.StartsWith("<i>"))
            {
                pre += "<i>";
                word = word.Remove(0, 3);
            }
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
            if (word.StartsWith("<i>"))
            {
                pre += "<i>";
                word = word.Remove(0, 3);
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
            if (word.EndsWith("</i>"))
            {
                post = post + "</i>";
                word = word.Remove(word.Length - 4, 4);
            }
            if (word.Length == 0)
                return pre + word + post;

            foreach (string from in _wordReplaceList.Keys)
            {
                if (from.Contains(word))
                {
                    if (word == from)
                        return pre + _wordReplaceList[from] + post;
                    if (word + post == from)
                        return pre + _wordReplaceList[from];
                    if (pre + word + post == from)
                        return _wordReplaceList[from];
                }
            }


            return pre + word + post;
        }

        public static string Fix0InsideLowerCaseWord(string word)
        {
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
                word.Contains("9") ||
                word.EndsWith("a.m") ||
                word.EndsWith("p.m") ||
                word.EndsWith("am") ||
                word.EndsWith("pm"))
                return word;

            if (hexNumber.IsMatch(word))
                return word;

            if (word.LastIndexOf('0') > 0)
            {
                Match match = regExTime1.Match(word);
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
                        match = regExTime1.Match(word);
                    }
                }

                match = regExTime2.Match(word);
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
                        match = regExTime2.Match(word, match.Index + 1);
                    }
                }
            }
            return word;
        }


        public static string FixIor1InsideLowerCaseWord(string word)
        {
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

            if (hexNumber.IsMatch(word))
                return word;

            if (word.LastIndexOf('I') > 0 || word.LastIndexOf('1') > 0)
            {
                Match match = regExIandZero.Match(word);
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
                        match = regExIandZero.Match(word, match.Index + 1);
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
            input = FixOcrErrorViaLineReplaceList(input);
            input = FixOcrErrorsViaHardcodedRules(input, lastLine, _abbreviationList);
            input = FixOcrErrorViaLineReplaceList(input);

            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                if (input.Length < 10 && input.Length > 4 && !input.Contains(Environment.NewLine) && input.StartsWith("II") && input.EndsWith("II"))
                {
                    input = "\"" + input.Substring(2, input.Length - 4) + "\"";
                }

                // e.g. "selectionsu." -> "selections..."
                if (input.EndsWith("u.") && _hunspell != null )
                {
                    string[] words = input.Split(" .".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        string lastWord = words[words.Length - 1].Trim();
                        if (lastWord.Length > 2 &&
                            lastWord[0].ToString() == lastWord[0].ToString().ToLower() &&
                            !IsWordOrWordsCorrect(lastWord) &&
                            IsWordOrWordsCorrect(lastWord.Substring(0, lastWord.Length - 1)))
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

        private string FixLowercaseIToUppercaseI(string input, string lastLine)
        {
            var sb = new StringBuilder();
            string[] lines = input.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];

                    if (i > 0)
                        lastLine = lines[i - 1];
                    lastLine = Utilities.RemoveHtmlTags(lastLine);

                    if (string.IsNullOrEmpty(lastLine) ||
                        lastLine.EndsWith(".") ||
                        lastLine.EndsWith("!") ||
                        lastLine.EndsWith("?"))
                    {
                        StripableText st = new StripableText(l);
                        if (st.StrippedText.StartsWith("i") && !st.Pre.EndsWith("[") && !st.Pre.EndsWith("("))
                        {
                            if (string.IsNullOrEmpty(lastLine) || (!lastLine.EndsWith("...") && !EndsWithAbbreviation(lastLine, _abbreviationList)))
                            {
                                l = st.Pre + "I" + st.StrippedText.Remove(0, 1) + st.Post;
                            }
                        }
                    }
                    sb.AppendLine(l);
            }
            return sb.ToString().TrimEnd('\r').TrimEnd('\n').TrimEnd('\r').TrimEnd('\n');
        }

        private static bool EndsWithAbbreviation(string line, List<string> abbreviationList)
        {
            if (string.IsNullOrEmpty(line) || abbreviationList == null)
                return false;

            line = line.ToLower();
            foreach (string abbreviation in abbreviationList)
            {
                if (line.EndsWith(" " + abbreviation.ToLower()))
                    return true;
            }
            return false;
        }

        public static string FixOcrErrorsViaHardcodedRules(string input, string lastLine, List<string> abbreviationList)
        {
            if (!Configuration.Settings.Tools.OcrFixUseHardcodedRules)
                return input;

            input = input.Replace(",...", "...");

            if (input.StartsWith("..") && !input.StartsWith("..."))
                input = "." + input;           

            string pre = string.Empty;
            if (input.StartsWith("- "))
            {
                pre = "- ";
                input = input.Remove(0, 2);
            }
            else if (input.StartsWith("-"))
            {
                pre = "-";
                input = input.Remove(0, 1);
            }

            if (input.StartsWith(". .."))
                input = "..." + input.Remove(0, 4);
            if (input.StartsWith(".. ."))
                input = "..." + input.Remove(0, 4);
            if (input.StartsWith(". . ."))
                input = "..." + input.Remove(0, 5);
            if (input.StartsWith("... "))
                input = input.Remove(3, 1);
            input = pre + input;

            if (input.StartsWith("<i>. .."))
                input = "<i>..." + input.Remove(0, 7);
            if (input.StartsWith("<i>.. ."))
                input = "<i>..." + input.Remove(0, 7);
            if (input.StartsWith("<i>. . ."))
                input = "<i>..." + input.Remove(0, 8);
            if (input.StartsWith("<i>... "))
                input = input.Remove(6, 1);

            if (input.EndsWith(". .."))
                input = input.Remove(input.Length - 4, 4) + "...";
            if (input.EndsWith(".. ."))
                input = input.Remove(input.Length - 4, 4) + "...";
            if (input.EndsWith(". . ."))
                input = input.Remove(input.Length - 5, 5) + "...";
            if (input.EndsWith(". ..."))
                input = input.Remove(input.Length - 5, 5) + "...";

            if (input.EndsWith(". ..</i>"))
                input = input.Remove(input.Length - 8, 8) + "...</i>";
            if (input.EndsWith(".. .</i>"))
                input = input.Remove(input.Length - 8, 8) + "...</i>";
            if (input.EndsWith(". . .</i>"))
                input = input.Remove(input.Length - 9, 9) + "...</i>";
            if (input.EndsWith(". ...</i>"))
                input = input.Remove(input.Length - 9, 9) + "...</i>";

            input = input.Replace(".. ?", "..?");
            input = input.Replace("..?", "...?");
            input = input.Replace("....?", "...?");

            input = input.Replace(".. !", "..!");
            input = input.Replace("..!", "...!");
            input = input.Replace("....!", "...!");

            input = input.Replace("... ?", "...?");
            input = input.Replace("... !", "...!");

            input = input.Replace("....", "...");
            input = input.Replace("....", "...");

            if (input.StartsWith("- ...") && lastLine != null && lastLine.EndsWith("...") && !(input.Contains(Environment.NewLine + "-")))
                input = input.Remove(0, 2);
            if (input.StartsWith("-...") && lastLine != null && lastLine.EndsWith("...") && !(input.Contains(Environment.NewLine + "-")))
                input = input.Remove(0, 1);

            if (input.Length > 2 && input[0] == '-' && Utilities.GetLetters(true, false, false).Contains(input[1].ToString()))
            {
                input = input.Insert(1, " ");
            }

            if (input.Length > 5 && input.StartsWith("<i>-") && Utilities.GetLetters(true, false, false).Contains(input[4].ToString()))
            {
                input = input.Insert(4, " ");
            }

            int idx = input.IndexOf(Environment.NewLine + "-");
            if (idx > 0 && idx + Environment.NewLine.Length + 1 < input.Length && Utilities.GetLetters(true, false, false).Contains(input[idx + Environment.NewLine.Length + 1].ToString()))
            {
                input = input.Insert(idx + Environment.NewLine.Length + 1, " ");
            }

            idx = input.IndexOf(Environment.NewLine + "<i>-");
            if (idx > 0 && Utilities.GetLetters(true, false, false).Contains(input[idx + Environment.NewLine.Length + 4].ToString()))
            {
                input = input.Insert(idx + Environment.NewLine.Length + 4, " ");
            }


            if (string.IsNullOrEmpty(lastLine) ||
                lastLine.EndsWith(".") ||
                lastLine.EndsWith("!") ||
                lastLine.EndsWith("?") ||
                lastLine.EndsWith("]") ||
                lastLine.EndsWith("♪"))
            {
                lastLine = Utilities.RemoveHtmlTags(lastLine);
                StripableText st = new StripableText(input);
                if (lastLine == null || (!lastLine.EndsWith("...") && !EndsWithAbbreviation(lastLine, abbreviationList)))
                {
                    if (st.StrippedText.Length > 0 && st.StrippedText[0].ToString() != st.StrippedText[0].ToString().ToUpper() && !st.Pre.EndsWith("[") && !st.Pre.EndsWith("(") && !st.Pre.EndsWith("..."))
                    {
                        string uppercaseLetter = st.StrippedText[0].ToString().ToUpper();
                        if (st.StrippedText.Length > 1 && uppercaseLetter == "L" && "abcdfghjklmnpqrstvwxz".Contains(st.StrippedText[1].ToString()))
                            uppercaseLetter = "I";
                        st.StrippedText = st.StrippedText.Remove(0, 1).Insert(0, uppercaseLetter);
                        input = st.Pre + st.StrippedText + st.Post;
                    }
                }
            }

            // lines ending with ". should often end at ... (of no other quotes exists near by)
            if ((lastLine == null || !lastLine.Contains("\"")) && input != null &&
                input.EndsWith("\".") && input.IndexOf("\"") == input.LastIndexOf("\"") && input.Length > 3)
            {
                string lastChar = input.Substring(input.Length - 3, 1);
                if (!"0123456789".Contains(lastChar))
                {
                    int position = input.Length - 2;
                    input = input.Remove(position).Insert(position, "...");
                }
            }

            // change '<number><space>1' to '<number>1'
            if (input.Contains("1"))
            {
                Match match = regExNumber1.Match(input);
                while (match.Success)
                {
                    bool doFix = true;

                    if (match.Index + 4 < input.Length && input[match.Index + 3] == '/' && "0123456789".Contains(input[match.Index + 4].ToString()))
                        doFix = false;

                    if (doFix)
                    {
                        input = input.Substring(0, match.Index + 1) + input.Substring(match.Index + 2);
                        match = regExNumber1.Match(input);
                    }
                    else
                    {
                        match = regExNumber1.Match(input, match.Index + 1);
                    }
                }
            }

            // change '' to "
            input = input.Replace("''", "\"");

            // change 'sequeI of' to 'sequel of'
            if (input.Contains("I"))
            {
                var match = regExUppercaseI.Match(input);
                while (match.Success)
                {
                    input = input.Substring(0, match.Index + 1) + "l" + input.Substring(match.Index + 2);
                    match = regExUppercaseI.Match(input);
                }
            }

            // change 'NlCE' to 'NICE'
            if (input.Contains("l"))
            {
                var match = regExLowercaseL.Match(input);
                while (match.Success)
                {
                    input = input.Substring(0, match.Index + 1) + "I" + input.Substring(match.Index + 2);
                    match = regExLowercaseL.Match(input);
                }
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
            if (newText.StartsWith("<i>"))
            {
                pre += "<i>";
                newText = newText.Remove(0, 3);
            }
            while (newText.Length > 1 && " -\"['¶(".Contains(newText.Substring(0, 1)))
            {
                pre += newText.Substring(0, 1);
                newText = newText.Substring(1);
            }
            if (newText.StartsWith("<i>"))
            {
                pre += "<i>";
                newText = newText.Remove(0, 3);
            }

            // begin line
            string[] lines = newText.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            foreach (string l in lines)
            {
                string s = l;
                foreach (string from in _beginLineReplaceList.Keys)
                {
                    if (s.StartsWith(from))
                        s = s.Remove(0, from.Length).Insert(0, _beginLineReplaceList[from]);
                    if (s.Contains(". " + from))
                        s = s.Replace(". " + from, ". " + _beginLineReplaceList[from]);
                    if (s.Contains("! " + from))
                        s = s.Replace("! " + from, "! " + _beginLineReplaceList[from]);
                    if (s.Contains("? " + from))
                        s = s.Replace("? " + from, "? " + _beginLineReplaceList[from]);
                    if (s.Contains("." + Environment.NewLine + from))
                        s = s.Replace(". " + Environment.NewLine + from, ". " + Environment.NewLine + _beginLineReplaceList[from]);
                    if (s.Contains("! " + Environment.NewLine + from))
                        s = s.Replace("! " + Environment.NewLine + from, "! " + Environment.NewLine + _beginLineReplaceList[from]);
                    if (s.Contains("? " + Environment.NewLine + from))
                        s = s.Replace("? " + Environment.NewLine + from, "? " + Environment.NewLine + _beginLineReplaceList[from]);
                    if (s.StartsWith("\"") && !from.StartsWith("\"") && s.StartsWith("\"" + from))
                        s = s.Replace("\"" + from, "\"" + _beginLineReplaceList[from]);
                }
                sb.AppendLine(s);
            }
            newText = sb.ToString().TrimEnd('\r').TrimEnd('\n').TrimEnd('\r').TrimEnd('\n');
            newText = pre + newText;

            string post = string.Empty;
            if (newText.EndsWith("</i>"))
            {
                newText = newText.Remove(newText.Length - 4, 4);
                post = "</i>";
            }
            foreach (string from in _endLineReplaceList.Keys)
            {
                if (newText.EndsWith(from))
                {
                    int position = (newText.Length - from.Length);
                    newText = newText.Remove(position).Insert(position, _endLineReplaceList[from]);
                }
            }
            newText += post;

            foreach (string from in _partialLineReplaceList.Keys)
            {
                if (newText.Contains(from))
                    newText = ReplaceWord(newText, from, _partialLineReplaceList[from]);
            }
            return newText;
        }

        public string FixUnknownWordsViaGuessOrPrompt(out int wordsNotFound, string line, int index, Bitmap bitmap, bool autoFix, bool promptForFixingErrors, bool log, bool useAutoGuess)
        {
            List<string> localIgnoreWords = new List<string>();
            wordsNotFound = 0;

            if (promptForFixingErrors && line.Length == 1 && !IsWordKnownOrNumber(line, line))
            {
                SpellcheckOcrTextResult res = SpellcheckOcrText(line, bitmap, new string[1] { line}, 0, line, localIgnoreWords);
                if (res.FixedWholeLine || res.Fixed)
                    return res.Line;
                wordsNotFound++;
                return line;
            }

            if (_hunspell == null)
                return line;

            string tempLine = line;
            //foreach (string name in _namesEtcList)
            //{
            //    int start = tempLine.IndexOf(name);
            //    if (start >= 0)
            //    {
            //        if (start == 0 || (Environment.NewLine + " ¡¿,.!?:;()[]{}+-$£\"”“#&%…—").Contains(tempLine[start - 1].ToString()))
            //        {
            //            int end = start + name.Length;
            //            if (end >= tempLine.Length || (Environment.NewLine + " ¡¿,.!?:;()[]{}+-$£\"”“#&%…—").Contains(tempLine[end].ToString()))
            //                tempLine = tempLine.Remove(start, name.Length);
            //        }
            //    }
            //}

            foreach (string name in _namesEtcMultiWordList)
            {
                int start = tempLine.IndexOf(name);
                if (start >= 0)
                {
                    if (start == 0 || (Environment.NewLine + " ¡¿,.!?:;()[]{}+-$£\"”“#&%…—♪").Contains(tempLine[start - 1].ToString()))
                    {
                        int end = start + name.Length;
                        if (end >= tempLine.Length || (Environment.NewLine + " ¡¿,.!?:;()[]{}+-$£\"”“#&%…—♪").Contains(tempLine[end].ToString()))
                            tempLine = tempLine.Remove(start, name.Length);
                    }
                }
            }

            string[] words = tempLine.Split((Environment.NewLine + " ¡¿,.!?:;()[]{}+-£\"”“#&%…—♪").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i].TrimStart('\'');
                word = word.TrimEnd('\'');
                string wordNoItalics = word.Replace("<i>", string.Empty).Replace("</i>", string.Empty);
                if (!IsWordKnownOrNumber(wordNoItalics, line) && !localIgnoreWords.Contains(wordNoItalics))
                {
                    bool correct = DoSpell(word);
                    if (!correct)
                        correct = DoSpell(word.Trim('\''));
                    if (!correct)
                        correct = DoSpell(wordNoItalics);
                    if (!correct && _userWordList.Contains(wordNoItalics))
                        correct = true;

                    if (!correct && !line.Contains(word))
                        correct = true; // already fixed

                    if (!correct)
                    {
                        //look for match via dash'ed word, e.g. sci-fi
                        string dashedWord = GetDashedWordBefore(wordNoItalics, line, words, i);
                        if (!string.IsNullOrEmpty(dashedWord))
                        {
                            correct = IsWordKnownOrNumber(dashedWord, line);
                            if (!correct)
                                correct = DoSpell(dashedWord);
                        }
                        if (!correct)
                        {
                            dashedWord = GetDashedWordAfter(wordNoItalics, line, words, i);
                            if (!string.IsNullOrEmpty(dashedWord))
                            {
                                correct = IsWordKnownOrNumber(dashedWord, line);
                                if (!correct)
                                    correct = DoSpell(dashedWord);
                            }
                        }
                    }

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

                                if (word[0] == 'L')
                                    guesses.Add("I" + word.Substring(1));

                                if (word.Contains("$"))
                                    guesses.Add(word.Replace("$", "s"));

                                string wordWithCasingChanged = GetWordWithDominatedCasing(word);
                                if (DoSpell(word.ToLower()))
                                    guesses.Insert(0, wordWithCasingChanged);
                            }
                            else
                            {
                                if (word[0] == 'L')
                                    guesses.Add("I" + word.Substring(1));

                                if (word.Length > 2  && word[0] == 'I' && word[1].ToString().ToUpper() != word[1].ToString())
                                    guesses.Add("l" + word.Substring(1));

                                if (i==0)
                                    guesses.Add(word.Replace(@"\/", "V"));
                                else
                                    guesses.Add(word.Replace(@"\/", "v"));
                                guesses.Add(word.Replace("ﬁ", "fi"));
                                guesses.Add(word.Replace("ﬁ", "fj"));
                                guesses.Add(word.Replace("ﬂ", "fl"));
                                if (word.Contains("$"))
                                    guesses.Add(word.Replace("$", "s"));
                                if (!word.EndsWith("€") && !word.StartsWith("€"))
                                    guesses.Add(word.Replace("€", "e"));
                            }
                            foreach (string guess in guesses)
                            {
                                if (IsWordOrWordsCorrect(guess) && !guess.StartsWith("f "))
                                {
                                    string replacedLine = ReplaceWord(line, word, guess);
                                    if (replacedLine != line)
                                    {
                                        if (log)
                                            AutoGuessesUsed.Add(string.Format("#{0}: {1} -> {2} in line via '{3}': {4}", index + 1, word, guess, "OCRFixReplaceList.xml", line.Replace(Environment.NewLine, " ")));

                                        //line = line.Remove(match.Index, match.Value.Length).Insert(match.Index, guess);
                                        line = replacedLine;
                                        wordsNotFound--;
                                        correct = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!correct && promptForFixingErrors)
                        {
                            List<string> suggestions = new List<string>();

                            if ((word == "Lt's" || word == "Lt'S") && SpellCheckDictionaryName.StartsWith("en_"))
                            {
                                suggestions.Add("It's");
                            }
                            else
                            {
                                if (word.ToUpper() != "LT'S" && word.ToUpper() != "SOX'S") //TODO: get fixed nhunspell
                                    suggestions = DoSuggest(word); // 0.9.6 fails on "Lt'S"
                            }

                            if (word.StartsWith("<i>"))
                                word = word.Remove(0, 3);

                            if (word.EndsWith("</i>"))
                                word = word.Remove(word.Length-4, 4);

                            SpellcheckOcrTextResult res = SpellcheckOcrText(line, bitmap, words, i, word, suggestions);

                            if (res.FixedWholeLine)
                            {
                                return res.Line;
                            }
                            if (res.Fixed)
                            {
                                localIgnoreWords.Add(word);
                                line = res.Line;
                                wordsNotFound--;
                            }
                        }
                    }
                }
            }
            return line;
        }

        private string GetDashedWordBefore(string word, string line, string[] words, int index)
        {
            if (index > 0 && line.Contains(words[index - 1] + "-" + word))
                return (words[index - 1] + "-" + word).Replace("<i>", string.Empty).Replace("</i>", string.Empty);

            return null;
        }

        private string GetDashedWordAfter(string word, string line, string[] words, int index)
        {
            if (index < words.Length - 1 && line.Contains(word + "-" + words[index + 1].Replace("</i>", string.Empty)))
                return (word + "-" + words[index + 1]).Replace("<i>", string.Empty).Replace("</i>", string.Empty);

            return null;
        }

        private string GetWordWithDominatedCasing(string word)
        {
            string uppercaseLetters = Utilities.GetLetters(true, false, false);
            string lowercaseLetters = Utilities.GetLetters(false, true, false);
            int lowercase = 0;
            int uppercase = 0;
            for (int i = 0; i < word.Length; i++)
            {
                if (lowercaseLetters.Contains(word.Substring(i, 1)))
                    lowercase++;
                else if (uppercaseLetters.Contains(word.Substring(i, 1)))
                    uppercase++;
            }
            if (uppercase > lowercase)
                return word.ToUpper();
            else
                return word.ToLower();
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
                        {
                            _namesEtcList.Add(s);
                            _namesEtcListUppercase.Add(s.ToUpper());
                            if (_fiveLetterWordListLanguageName.StartsWith("en"))
                            {
                                if (!s.EndsWith("s"))
                                    _namesEtcListWithApostrophe.Add(s + "'s");
                                else
                                    _namesEtcListWithApostrophe.Add(s + "'");
                            }

                        }
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
                    _wordSkipList.Add(_spellCheck.Word.ToUpper());
                    if (_spellCheck.Word.Length > 1)
                        _wordSkipList.Add(_spellCheck.Word.Substring(0,1).ToUpper() + _spellCheck.Word.Substring(1));
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
                result.Line = ReplaceWord(line, word, result.Word);
            }
            return result;
        }

        private string ReplaceWord(string text, string word, string newWord)
        {
            StringBuilder sb = new StringBuilder();
            if (word != null && text != null && text.Contains(word))
            {
                int appendFrom = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (text.Substring(i).StartsWith(word) && i >= appendFrom)
                    {
                        bool startOk = i == 0;
                        if (!startOk)
                            startOk = (" ¡¿<>-\"”“()[]'‘`´¶♪¿¡.…—!?,:;/" + Environment.NewLine).Contains(text.Substring(i - 1, 1));
                        if (!startOk && word.StartsWith(" "))
                            startOk = true;
                        if (startOk)
                        {
                            bool endOK = (i + word.Length == text.Length);
                            if (!endOK)
                                endOK = (" ¡¿<>-\"”“()[]'‘`´¶♪¿¡.…—!?,:;/" + Environment.NewLine).Contains(text.Substring(i + word.Length, 1));
                            if (!endOK)
                                endOK = newWord.EndsWith(" ");
                            if (endOK)
                            {
                                sb.Append(newWord);
                                appendFrom = i + word.Length;
                            }
                        }
                    }
                    if (i >= appendFrom)
                        sb.Append(text.Substring(i, 1));
                }
            }
            return sb.ToString();
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

        public bool DoSpell(string word)
        {
            return _hunspell.Spell(word);
        }

        public List<string> DoSuggest(string word)
        {
            return _hunspell.Suggest(word);
        }

        public bool IsWordOrWordsCorrect(string word)
        {
            foreach (string s in word.Split(' '))
            {
                if (!DoSpell(s))
                {
                    if (IsWordKnownOrNumber(word, word))
                        return true;

                    if (s.Length > 10 && s.Contains("/"))
                    {
                        string[] ar = s.Split('/');
                        if (ar.Length == 2)
                        {
                            if (ar[0].Length > 3 && ar[1].Length > 3)
                            {
                                string a = ar[0];
                                if (a == a.ToUpper())
                                    a = a.Substring(0, 1) + a.Substring(1).ToLower();
                                string b = ar[0];
                                if (b == b.ToUpper())
                                    b = b.Substring(0, 1) + b.Substring(1).ToLower();

                                if ((DoSpell(a) || IsWordKnownOrNumber(a, word)) &&
                                    (DoSpell(b) || IsWordKnownOrNumber(b, word)))
                                    return true;
                            }
                        }
                    }
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

            if (_namesEtcList.IndexOf(word.Trim('\'')) >= 0)
                return true;

            if (_namesEtcListUppercase.IndexOf(word.Trim('\'')) >= 0)
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
            string[] words = line.Replace("<i>", string.Empty).Replace("</i>", string.Empty).Split((Environment.NewLine + " ¡¿,.!?:;()[]{}+-$£\"#&%…“”").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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
