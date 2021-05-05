using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SpellCheck;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Forms.Ocr;
using Nikse.SubtitleEdit.Logic.SpellCheck;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class OcrFixEngine : IDisposable, IDoSpell
    {
        public abstract class LogItem
        {
            public int Line { get; set; }
            public string Text { get; }

            protected LogItem(int line, string text)
            {
                Line = line;
                Text = text.Trim();
            }

            public override string ToString()
            {
                return $"#{Line}: {Text}";
            }
        }

        private class AutoGuess : LogItem
        {
            public AutoGuess(int index, string word, string guess, string line)
                : base(index + 1, string.Format(LanguageSettings.Current.VobSubOcr.UnknownWordToGuessInLine, word, guess, line.Replace(Environment.NewLine, " ")))
            {
            }
        }

        private class UnknownWord : LogItem
        {
            public UnknownWord(int index, string word)
                : base(index + 1, word)
            {
            }
        }

        public enum AutoGuessLevel
        {
            None,
            Cautious,
            Aggressive
        }

        private string _userWordListXmlFileName;
        private string _fiveLetterWordListLanguageName;

        private readonly OcrFixReplaceList _ocrFixReplaceList;
        private NameList _nameListObj;
        private HashSet<string> _nameList = new HashSet<string>();
        private HashSet<string> _nameListUppercase = new HashSet<string>();
        private HashSet<string> _nameListWithApostrophe = new HashSet<string>();
        private HashSet<string> _nameMultiWordList = new HashSet<string>(); // case sensitive phrases
        private List<string> _nameMultiWordListAndWordsWithPeriods;
        private HashSet<string> _abbreviationList;
        private HashSet<string> _userWordList = new HashSet<string>();
        private HashSet<string> _wordSkipList = new HashSet<string>();
        private readonly HashSet<string> _wordSpellOkList = new HashSet<string>();
        private Hunspell _hunspell;
        private Dictionary<string, string> _changeAllDictionary;
        private SpellCheckWordLists _spellCheckWordLists;
        private OcrSpellCheck _spellCheck;
        private readonly Form _parentForm;
        private string _spellCheckDictionaryName;
        private readonly string _threeLetterIsoLanguageName;

        private static readonly Regex RegexAloneIasL = new Regex(@"\bl\b", RegexOptions.Compiled);
        private static readonly Regex RegexLowercaseL = new Regex("[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏ]l[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏ]", RegexOptions.Compiled);
        private static readonly Regex RegexUppercaseI = new Regex("[a-zæøåöääöéèàùâêîôûëï]I.", RegexOptions.Compiled);
        private static readonly Regex RegexNumber1 = new Regex(@"(?<=\d) 1(?!/\d)", RegexOptions.Compiled);

        public bool Abort { get; set; }
        public OcrSpellCheck.Action LastAction { get; set; } = OcrSpellCheck.Action.Abort;
        public bool IsBinaryImageCompareOrNOcr { get; set; }
        public List<LogItem> AutoGuessesUsed { get; set; }
        public List<LogItem> UnknownWordsFound { get; set; }
        public bool IsDictionaryLoaded { get; private set; }

        private readonly HashSet<char> _expectedChars = new HashSet<char> { ' ', '¡', '¿', ',', '.', '!', '?', ':', ';', '(', ')', '[', ']', '{', '}', '+', '-', '£', '\\', '"', '”', '„', '“', '«', '»', '#', '&', '%', '\r', '\n', '؟' }; // removed $
        private readonly HashSet<char> _expectedCharsNoComma = new HashSet<char> { ' ', '¡', '¿', '.', '!', '?', ':', ';', '(', ')', '[', ']', '{', '}', '+', '-', '£', '\\', '"', '”', '„', '“', '«', '»', '#', '&', '%', '\r', '\n', '؟' }; // removed $ + comma

        /// <summary>
        /// Advanced OCR fixing via replace/spelling dictionaries + some hardcoded rules
        /// </summary>
        /// <param name="threeLetterIsoLanguageName">E.g. eng for English</param>
        /// <param name="hunspellName">Name of hunspell dictionary</param>
        /// <param name="parentForm">Used for centering/show spell check dialog</param>
        /// <param name="isBinaryImageCompare">Calling from OCR via "Image compare"</param>
        public OcrFixEngine(string threeLetterIsoLanguageName, string hunspellName, Form parentForm, bool isBinaryImageCompareOrNOcr = false)
        {
            if (string.IsNullOrEmpty(threeLetterIsoLanguageName))
            {
                if (hunspellName != null && hunspellName.Length >= 2)
                {
                    threeLetterIsoLanguageName = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(hunspellName.Substring(0, 2));
                }
                else
                {
                    threeLetterIsoLanguageName = string.Empty;
                }
            }

            IsBinaryImageCompareOrNOcr = isBinaryImageCompareOrNOcr;
            if (threeLetterIsoLanguageName == "per")
            {
                threeLetterIsoLanguageName = "fas";
            }

            _threeLetterIsoLanguageName = threeLetterIsoLanguageName;
            _parentForm = parentForm;

            if (parentForm.GetType() != typeof(FixCommonErrors))
            {
                _spellCheck = new OcrSpellCheck(_parentForm) { StartPosition = FormStartPosition.Manual, IsBinaryImageCompareOrNOcr = isBinaryImageCompareOrNOcr };
                _spellCheck.Location = new Point(parentForm.Left + (parentForm.Width / 2 - _spellCheck.Width / 2),
                    parentForm.Top + (parentForm.Height / 2 - _spellCheck.Height / 2));
            }

            _ocrFixReplaceList = OcrFixReplaceList.FromLanguageId(threeLetterIsoLanguageName);
            if (!string.IsNullOrEmpty(threeLetterIsoLanguageName) || !string.IsNullOrEmpty(hunspellName))
            {
                LoadSpellingDictionaries(threeLetterIsoLanguageName, hunspellName); // Hunspell etc.
            }

            AutoGuessesUsed = new List<LogItem>();
            UnknownWordsFound = new List<LogItem>();
        }

        private void LoadSpellingDictionaries(string threeLetterIsoLanguageName, string hunspellName)
        {
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
            {
                return;
            }

            if (!string.IsNullOrEmpty(hunspellName))
            {
                var directDicFile = Path.Combine(dictionaryFolder, hunspellName + ".dic");
                if (File.Exists(directDicFile))
                {
                    LoadSpellingDictionariesViaDictionaryFileName(threeLetterIsoLanguageName, directDicFile, true);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_gb", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_GB.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", "en_GB.dic", true);
                return;
            }
            if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en-gb", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en-GB.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", "en-GB.dic", true);
                return;
            }
            if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_ca", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_CA.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", "en_CA.dic", true);
                return;
            }
            if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_au", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_AU.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", "en_AU.dic", true);
                return;
            }
            if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_za", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_ZA.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", "en_ZA.dic", true);
                return;
            }
            if (threeLetterIsoLanguageName == "eng" && File.Exists(Path.Combine(dictionaryFolder, "en_US.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", "en_US.dic", true);
                return;
            }

            foreach (var culture in Iso639Dash2LanguageCode.List)
            {
                if (culture.ThreeLetterCode == threeLetterIsoLanguageName)
                {
                    string dictionaryFileName = null;
                    if (!string.IsNullOrEmpty(hunspellName) && hunspellName.StartsWith(culture.TwoLetterCode, StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, hunspellName + ".dic")))
                    {
                        dictionaryFileName = Path.Combine(dictionaryFolder, hunspellName + ".dic");
                        LoadSpellingDictionariesViaDictionaryFileName(threeLetterIsoLanguageName, dictionaryFileName, true);
                        return;
                    }
                    foreach (string dic in Directory.GetFiles(dictionaryFolder, "*.dic"))
                    {
                        string name = Path.GetFileNameWithoutExtension(dic);
                        if (!string.IsNullOrEmpty(name) && !name.StartsWith("hyph", StringComparison.Ordinal))
                        {
                            try
                            {
                                name = name.Replace('_', '-');
                                if (name.Length > 5)
                                {
                                    name = name.Substring(0, 5);
                                }

                                var ci = CultureInfo.GetCultureInfo(name);
                                if (ci.GetThreeLetterIsoLanguageName() == threeLetterIsoLanguageName ||
                                    ci.GetThreeLetterIsoLanguageName().Equals(threeLetterIsoLanguageName, StringComparison.OrdinalIgnoreCase))
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
                    {
                        return;
                    }

                    LoadSpellingDictionariesViaDictionaryFileName(threeLetterIsoLanguageName, dictionaryFileName, true);
                    return;
                }
            }

            string dicFileName = null;
            foreach (string dic in Directory.GetFiles(dictionaryFolder, "*.dic"))
            {
                string name = Path.GetFileNameWithoutExtension(dic);
                if (!string.IsNullOrEmpty(name) && !name.StartsWith("hyph", StringComparison.Ordinal))
                {
                    try
                    {
                        name = name.Replace('_', '-');
                        if (name.Length > 5)
                        {
                            name = name.Substring(0, 5);
                        }

                        var ci = CultureInfo.GetCultureInfo(name);
                        if (ci.GetThreeLetterIsoLanguageName() == threeLetterIsoLanguageName ||
                            ci.GetThreeLetterIsoLanguageName().Equals(threeLetterIsoLanguageName, StringComparison.OrdinalIgnoreCase))
                        {
                            dicFileName = dic;
                            break;
                        }
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                    }
                }
            }

            if (dicFileName == null)
            {
                return;
            }

            LoadSpellingDictionariesViaDictionaryFileName(threeLetterIsoLanguageName, dicFileName, true);
        }

        private void LoadSpellingDictionariesViaDictionaryFileName(string threeLetterIsoLanguageName, string dictionaryFileName, bool resetSkipList)
        {
            _fiveLetterWordListLanguageName = Path.GetFileNameWithoutExtension(dictionaryFileName);
            if (_fiveLetterWordListLanguageName != null && _fiveLetterWordListLanguageName.Length > 5)
            {
                _fiveLetterWordListLanguageName = _fiveLetterWordListLanguageName.Substring(0, 5);
            }

            string dictionary = Utilities.DictionaryFolder + _fiveLetterWordListLanguageName;
            if (resetSkipList)
            {
                _wordSkipList = new HashSet<string> { Configuration.Settings.Tools.MusicSymbol, "*", "%", "#", "+", "$" };
            }

            // Load names etc list (names/noise words)
            _nameListObj = new NameList(Configuration.DictionariesDirectory, _fiveLetterWordListLanguageName, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            _nameList = _nameListObj.GetNames();
            _nameMultiWordList = _nameListObj.GetMultiNames();
            _nameListUppercase = new HashSet<string>();
            _nameListWithApostrophe = new HashSet<string>();
            var nameListWithPeriods = new List<string>();
            _abbreviationList = new HashSet<string>();

            bool isEnglish = threeLetterIsoLanguageName.Equals("eng", StringComparison.OrdinalIgnoreCase);
            foreach (string name in _nameList)
            {
                _nameListUppercase.Add(name.ToUpperInvariant());
                if (isEnglish)
                {
                    if (!name.EndsWith('s'))
                    {
                        _nameListWithApostrophe.Add(name + "'s");
                    }
                    else
                    {
                        _nameListWithApostrophe.Add(name + "'");
                    }
                }

                // Abbreviations.
                if (name.EndsWith('.'))
                {
                    _abbreviationList.Add(name);
                }

                if (name.Contains(".", StringComparison.Ordinal))
                {
                    nameListWithPeriods.Add(name);
                }
            }

            _nameMultiWordListAndWordsWithPeriods = new List<string>(_nameMultiWordList.Concat(nameListWithPeriods));
            if (isEnglish)
            {
                if (!_abbreviationList.Contains("a.m."))
                {
                    _abbreviationList.Add("a.m.");
                }

                if (!_abbreviationList.Contains("p.m."))
                {
                    _abbreviationList.Add("p.m.");
                }

                if (!_abbreviationList.Contains("o.r."))
                {
                    _abbreviationList.Add("o.r.");
                }
            }

            // Load user words
            _userWordList = new HashSet<string>();
            _userWordListXmlFileName = Utilities.LoadUserWordList(_userWordList, _fiveLetterWordListLanguageName);
            foreach (string name in _userWordList)
            {
                if (name.EndsWith('.'))
                {
                    _abbreviationList.Add(name);
                }
            }

            // Load Hunspell spell checker
            try
            {
                if (dictionaryFileName.EndsWith(".dic", StringComparison.OrdinalIgnoreCase) && File.Exists(dictionaryFileName))
                {
                    dictionary = dictionaryFileName.Substring(0, dictionaryFileName.Length - 4);
                }
                else if (dictionaryFileName.EndsWith(".dic", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(Utilities.DictionaryFolder, dictionaryFileName)))
                {
                    var f = Path.Combine(Utilities.DictionaryFolder, dictionaryFileName);
                    dictionary = f.Substring(0, f.Length - 4);
                }
                else if (!File.Exists(dictionary + ".dic"))
                {
                    var fileMatches = Directory.GetFiles(Utilities.DictionaryFolder, _fiveLetterWordListLanguageName + "*.dic");
                    if (fileMatches.Length > 0)
                    {
                        dictionary = fileMatches[0].Substring(0, fileMatches[0].Length - 4);
                    }
                }
                _hunspell?.Dispose();
                _hunspell = Hunspell.GetHunspell(dictionary);
                IsDictionaryLoaded = true;
                _spellCheckDictionaryName = dictionary;
            }
            catch
            {
                IsDictionaryLoaded = false;
            }

            // load spell check "change all" list
            if (_hunspell != null)
            {
                var languageName = Path.GetFileName(_spellCheckDictionaryName);
                if (!string.IsNullOrEmpty(languageName))
                {
                    _spellCheckWordLists = new SpellCheckWordLists(Utilities.DictionaryFolder, languageName, this);
                    _changeAllDictionary = _spellCheckWordLists.GetUseAlwaysList();
                }
            }
        }

        public string SpellCheckDictionaryName
        {
            get
            {
                string[] parts = _spellCheckDictionaryName?.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                if (parts?.Length > 0)
                {
                    return parts[parts.Length - 1];
                }

                return string.Empty;
            }
        }

        internal static Dictionary<string, string> LoadReplaceList(XmlDocument doc, string name)
        {
            var list = new Dictionary<string, string>();
            XmlNode node = doc.DocumentElement?.SelectSingleNode(name);
            if (node != null)
            {
                foreach (XmlNode item in node.ChildNodes)
                {
                    if (item.Attributes?["to"] != null && item.Attributes["from"] != null)
                    {
                        string to = item.Attributes["to"].InnerText;
                        string from = item.Attributes["from"].InnerText;
                        if (!list.ContainsKey(from))
                        {
                            list.Add(from, to);
                        }
                    }
                }
            }
            return list;
        }

        internal static Dictionary<string, string> LoadRegExList(XmlDocument doc, string name)
        {
            var list = new Dictionary<string, string>();
            XmlNode node = doc.DocumentElement?.SelectSingleNode(name);
            if (node != null)
            {
                foreach (XmlNode item in node.ChildNodes)
                {
                    if (item.Attributes?["replaceWith"] != null && item.Attributes["find"] != null)
                    {
                        string to = item.Attributes["replaceWith"].InnerText;
                        string from = item.Attributes["find"].InnerText;
                        if (!list.ContainsKey(from))
                        {
                            list.Add(from, to);
                        }
                    }
                }
            }
            return list;
        }

        public string FixOcrErrors(string input, int index, string lastLine, bool logSuggestions, AutoGuessLevel autoGuess)
        {
            var text = input;
            while (text.Contains(Environment.NewLine + " ", StringComparison.Ordinal))
            {
                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
            }

            while (text.Contains(" " + Environment.NewLine, StringComparison.Ordinal))
            {
                text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
            }

            while (text.Contains(Environment.NewLine + Environment.NewLine, StringComparison.Ordinal))
            {
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            }

            text = text.Trim();

            // Try to prevent resizing when fixing Ocr-hardcoded.
            var sb = new StringBuilder(text.Length + 2);

            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                if (text.Length > 3 && text.StartsWith(". .") && char.IsLetter(text[3]))
                {
                    text = text.Remove(1, 1).Insert(1, ".");
                }
                else if (text.Length > 3 && text.StartsWith(".. ") && char.IsLetter(text[3]))
                {
                    text = text.Remove(2, 1).Insert(2, ".");
                }
                else if (text.Length > 3 && text.StartsWith("..") && char.IsLetter(text[2]))
                {
                    text = "." + text;
                }

                text = text.Replace("<i>-</i>", "-");
                text = text.Replace("<i>- </i>", "- ");
                text = text.Replace("<i> - </i>", "- ");
                text = text.Replace("<i> -</i>", "- ");
                text = text.Replace("- ", "-  ");
                text = text.Replace("<i>a</i>", "a");
                text = text.Replace("<i>.</i>", ".");
                text = text.TrimStart();

                int len = text.Length;
                for (int i = 0; i < len; i++)
                {
                    char ch = text[i];
                    switch (ch)
                    {
                        case 'ﬁ':
                            sb.Append("fi");
                            break;
                        case 'ﬂ': // fb02
                            sb.Append("fl");
                            break;
                        case 'ν': // NOTE: Special unicode character! (Greek character!)
                            if (_threeLetterIsoLanguageName == "ell" || _threeLetterIsoLanguageName == "gre")
                            {
                                sb.Append(ch); // Keep Greek 'ν'
                            }
                            else
                            {
                                sb.Append('v');
                            }
                            break;
                        case '‚': // #x201A (SINGLE LOW-9 QUOTATION MARK) to plain old comma
                            sb.Append(',');
                            break;
                        default:
                            sb.Append(ch);
                            break;
                    }
                }
                text = sb.ToString();
                sb.Clear();
            }

            text = ReplaceWordsBeforeLineFixes(text);

            text = FixCommonOcrLineErrors(text, lastLine);

            // check words split by only space and new line (as other split chars might by a part of from-replace-string, like "\/\/e're" contains slash)
            sb = new StringBuilder();
            var word = new StringBuilder();
            string lastWord = null;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != '.' && "\r\n ".Contains(text[i]))
                {
                    if (word.Length > 0)
                    {
                        var fixedWord = FixOcrErrorsWord(lastWord, word, sb);
                        lastWord = fixedWord;
                        word.Clear();
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
                FixOcrErrorsWord(lastWord, word, sb);
            }

            // check words split by many chars like "()/-" etc.
            text = sb.ToString();
            sb = new StringBuilder();
            word = new StringBuilder();
            lastWord = null;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != '.' && _expectedChars.Contains(text[i]))
                {
                    if (word.Length > 0)
                    {
                        var fixedWord = FixOcrErrorsWord(lastWord, word, sb);
                        lastWord = fixedWord;
                        word.Clear();
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
                FixOcrErrorsWord(lastWord, word, sb);
            }

            text = FixCommonOcrLineErrors(sb.ToString(), lastLine);
            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                text = FixLowercaseIToUppercaseI(text, lastLine);
                if (SpellCheckDictionaryName.StartsWith("en_", StringComparison.Ordinal) || _threeLetterIsoLanguageName == "eng")
                {
                    string oldText = text;
                    text = FixAloneLowercaseIToUppercaseI.FixAloneLowercaseIToUppercaseLine(RegexUtils.LittleIRegex, oldText, text, 'i');
                    text = FixAloneLowercaseIToUppercaseI.FixAloneLowercaseIToUppercaseLine(RegexAloneIasL, oldText, text, 'l');
                }
                else if (_threeLetterIsoLanguageName == "fra")
                {
                    text = FixFrenchLApostrophe(text, " I'", lastLine);
                    text = FixFrenchLApostrophe(text, " L'", lastLine);
                    text = FixFrenchLApostrophe(text, " l'", lastLine);
                    text = FixFrenchLApostrophe(text, " I’", lastLine);
                    text = FixFrenchLApostrophe(text, " L’", lastLine);
                    text = FixFrenchLApostrophe(text, " l’", lastLine);
                }

                text = Utilities.RemoveSpaceBetweenNumbers(text);
            }

            // must be last - counts/logs unknown words
            text = FixUnknownWordsViaGuessOrPrompt(out _, text, index, null, true, false, logSuggestions, autoGuess);

            return text;
        }

        private string FixOcrErrorsWord(string lastWord, StringBuilder word, StringBuilder sb)
        {
            string fixedWord;
            if (lastWord != null && lastWord.Contains("COLOR=", StringComparison.OrdinalIgnoreCase))
            {
                fixedWord = word.ToString();
            }
            else
            {
                var doFixWord = !(word.Length == 1 && sb.Length > 1 && sb.EndsWith('-'));
                if (doFixWord)
                {
                    fixedWord = word.ToString();

                    var post = string.Empty;
                    if (fixedWord.EndsWith('.') && fixedWord.IndexOf('.') == fixedWord.Length - 1)
                    {
                        post = ".";
                        fixedWord = fixedWord.Substring(0, fixedWord.Length - 1);
                    }
                    else if (fixedWord.EndsWith("...", StringComparison.Ordinal))
                    {
                        post = "...";
                        fixedWord = fixedWord.Substring(0, fixedWord.Length - 3);
                    }

                    fixedWord = FixWordViaReplaceList(fixedWord);
                    fixedWord = _ocrFixReplaceList.FixCommonWordErrors(fixedWord);

                    // Try using word lists for uppercase i inside words, e.g. "NIkolaj" to "Nikolaj"
                    if (fixedWord.Contains('I'))
                    {
                        var temp = fixedWord.Replace('I', 'i');
                        if (temp != fixedWord.ToUpperInvariant())
                        {
                            if (_nameList.Contains(temp))
                            {
                                fixedWord = temp;
                            }
                        }
                    }
                    fixedWord += post;
                }
                else
                {
                    fixedWord = word.ToString();
                }
            }

            sb.Append(fixedWord);
            return fixedWord;
        }

        private string FixWordViaReplaceList(string word)
        {
            if (_changeAllDictionary != null && _changeAllDictionary.ContainsKey(word))
            {
                return _changeAllDictionary[word];
            }
            return word;
        }

        internal static string FixFrenchLApostrophe(string input, string tag, string lastLine)
        {
            var text = input;
            bool endingBeforeThis = string.IsNullOrEmpty(lastLine) || lastLine.EndsWith('.') || lastLine.EndsWith('!') || lastLine.EndsWith('?') ||
                                    lastLine.EndsWith(".</i>", StringComparison.Ordinal) || lastLine.EndsWith("!</i>", StringComparison.Ordinal) || lastLine.EndsWith("?</i>", StringComparison.Ordinal) ||
                                    lastLine.EndsWith(".</font>", StringComparison.Ordinal) || lastLine.EndsWith("!</font>", StringComparison.Ordinal) || lastLine.EndsWith("?</font>", StringComparison.Ordinal);
            if (text.StartsWith(tag.TrimStart(), StringComparison.Ordinal) && text.Length > 3)
            {
                if (endingBeforeThis || char.IsUpper(text[2]))
                {
                    text = @"L" + text.Substring(1);
                }
                else if (char.IsLower(text[2]))
                {
                    text = @"l" + text.Substring(1);
                }
            }
            else if (text.StartsWith("<i>" + tag.TrimStart(), StringComparison.Ordinal) && text.Length > 6)
            {
                if (endingBeforeThis || char.IsUpper(text[5]))
                {
                    text = text.Remove(3, 1).Insert(3, "L");
                }
                else if (char.IsLower(text[5]))
                {
                    text = text.Remove(3, 1).Insert(3, "l");
                }
            }

            int start = text.IndexOf(tag, StringComparison.Ordinal);
            while (start > 0)
            {
                lastLine = HtmlUtil.RemoveHtmlTags(text.Substring(0, start)).TrimEnd().TrimEnd('-').TrimEnd();
                endingBeforeThis = string.IsNullOrEmpty(lastLine) || lastLine.EndsWith('.') || lastLine.EndsWith('!') || lastLine.EndsWith('?');
                if (start < text.Length - 4)
                {
                    if (start == 1 && text.StartsWith('-'))
                    {
                        endingBeforeThis = true;
                    }

                    if (start > 1)
                    {
                        string beforeThis = HtmlUtil.RemoveHtmlTags(text.Substring(0, start));
                        endingBeforeThis = beforeThis.EndsWith('.') || beforeThis.EndsWith('!') || beforeThis.EndsWith('?');
                    }

                    if (endingBeforeThis)
                    {
                        text = text.Remove(start + 1, 1).Insert(start + 1, "L");
                    }
                    else
                    {
                        text = text.Remove(start + 1, 1).Insert(start + 1, "l");
                    }
                }
                start = text.IndexOf(tag, start + 1, StringComparison.Ordinal);
            }

            tag = Environment.NewLine + tag.Trim();
            start = text.IndexOf(tag, StringComparison.Ordinal);
            while (start > 0)
            {
                lastLine = HtmlUtil.RemoveHtmlTags(text.Substring(0, start)).TrimEnd().TrimEnd('-').TrimEnd();
                endingBeforeThis = string.IsNullOrEmpty(lastLine) || lastLine.EndsWith('.') || lastLine.EndsWith('!') || lastLine.EndsWith('?') || lastLine.EndsWith(".</i>", StringComparison.Ordinal);
                if (start < text.Length - 5)
                {
                    if (start > 1)
                    {
                        string beforeThis = HtmlUtil.RemoveHtmlTags(text.Substring(0, start));
                        endingBeforeThis = beforeThis.EndsWith('.') || beforeThis.EndsWith('!') || beforeThis.EndsWith('?');
                    }

                    if (endingBeforeThis)
                    {
                        text = text.Remove(start + Environment.NewLine.Length, 1).Insert(start + Environment.NewLine.Length, "L");
                    }
                    else
                    {
                        text = text.Remove(start + Environment.NewLine.Length, 1).Insert(start + Environment.NewLine.Length, "l");
                    }
                }
                start = text.IndexOf(tag, start + 1, StringComparison.Ordinal);
            }

            tag = Environment.NewLine + "<i>" + tag.Trim();
            start = text.IndexOf(tag, StringComparison.Ordinal);
            while (start > 0)
            {
                lastLine = HtmlUtil.RemoveHtmlTags(text.Substring(0, start)).TrimEnd().TrimEnd('-').TrimEnd();
                endingBeforeThis = string.IsNullOrEmpty(lastLine) || lastLine.EndsWith('.') || lastLine.EndsWith('!') || lastLine.EndsWith('?') || lastLine.EndsWith(".</i>", StringComparison.Ordinal);
                if (start < text.Length - 8)
                {
                    if (endingBeforeThis || char.IsUpper(text[start + 5 + Environment.NewLine.Length]))
                    {
                        text = text.Remove(start + Environment.NewLine.Length + 3, 1).Insert(start + Environment.NewLine.Length + 3, "L");
                    }
                    else if (char.IsLower(text[start + 5 + Environment.NewLine.Length]))
                    {
                        text = text.Remove(start + Environment.NewLine.Length + 3, 1).Insert(start + Environment.NewLine.Length + 3, "l");
                    }
                }
                start = text.IndexOf(tag, start + 1, StringComparison.Ordinal);
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
                if (_expectedCharsNoComma.Contains(text[i])) // fix e.g. "don,t"
                {
                    if (word.Length > 0)
                    {
                        string fixedWord;
                        if (lastWord != null && lastWord.Contains("COLOR=", StringComparison.OrdinalIgnoreCase))
                        {
                            fixedWord = word.ToString();
                        }
                        else if (!word.ToString().Contains(','))
                        {
                            fixedWord = word.ToString();
                        }
                        else
                        {
                            fixedWord = _ocrFixReplaceList.FixCommonWordErrorsQuick(word.ToString());
                        }

                        sb.Append(fixedWord);
                        lastWord = fixedWord;
                        word.Clear();
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
                if (lastWord != null && lastWord.Contains("COLOR=", StringComparison.OrdinalIgnoreCase))
                {
                    fixedWord = word.ToString();
                }
                else if (!word.ToString().Contains(','))
                {
                    fixedWord = word.ToString();
                }
                else
                {
                    fixedWord = _ocrFixReplaceList.FixCommonWordErrorsQuick(word.ToString());
                }

                sb.Append(fixedWord);
            }

            lastWord = null;
            text = sb.ToString();
            sb = new StringBuilder();
            word = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (_expectedChars.Contains(text[i])) // removed $
                {
                    if (word.Length > 0)
                    {
                        string fixedWord;
                        if (lastWord != null && lastWord.Contains("COLOR=", StringComparison.OrdinalIgnoreCase))
                        {
                            fixedWord = word.ToString();
                        }
                        else
                        {
                            fixedWord = _ocrFixReplaceList.FixCommonWordErrorsQuick(word.ToString());
                        }

                        sb.Append(fixedWord);
                        lastWord = fixedWord;
                        word.Clear();
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
                string fixedWord = _ocrFixReplaceList.FixCommonWordErrorsQuick(word.ToString());
                sb.Append(fixedWord);
            }
            return sb.ToString();
        }

        private string FixCommonOcrLineErrors(string input, string lastLine)
        {
            var text = input;
            text = FixOcrErrorViaLineReplaceList(text);
            text = FixOcrErrorsViaHardcodedRules(text, lastLine, _abbreviationList);
            text = FixOcrErrorViaLineReplaceList(text);

            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                if (text.StartsWith('~'))
                {
                    text = ("- " + text.Remove(0, 1)).Replace("  ", " ");
                }

                text = text.Replace(Environment.NewLine + "~", Environment.NewLine + "- ").Replace("  ", " ");

                if (text.Length < 10 && text.Length > 4 && !text.Contains(Environment.NewLine, StringComparison.Ordinal) && text.StartsWith("II", StringComparison.Ordinal) && text.EndsWith("II", StringComparison.Ordinal))
                {
                    text = "\"" + text.Substring(2, text.Length - 4) + "\"";
                }

                // e.g. "selectionsu." -> "selections..."
                if (text.EndsWith("u.", StringComparison.Ordinal) && _hunspell != null)
                {
                    string[] words = text.Split(new[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        string lastWord = words[words.Length - 1].Trim();
                        if (lastWord.Length > 2 &&
                            char.IsLower(lastWord[0]) &&
                            !IsWordOrWordsCorrect(lastWord) &&
                            IsWordOrWordsCorrect(lastWord.Substring(0, lastWord.Length - 1)))
                        {
                            text = text.Substring(0, text.Length - 2) + "...";
                        }
                    }
                }

                // music notes
                if (text.StartsWith(".'", StringComparison.Ordinal) && text.EndsWith(".'", StringComparison.Ordinal))
                {
                    text = text.Replace(".'", Configuration.Settings.Tools.MusicSymbol);
                }
            }

            return text;
        }

        private string FixLowercaseIToUppercaseI(string input, string lastLine)
        {
            var sb = new StringBuilder();
            var lines = input.SplitToLines();
            for (int i = 0; i < lines.Count; i++)
            {
                string l = lines[i];

                if (i > 0)
                {
                    lastLine = lines[i - 1];
                }

                lastLine = HtmlUtil.RemoveHtmlTags(lastLine);

                if (string.IsNullOrEmpty(lastLine) ||
                    lastLine.EndsWith('.') ||
                    lastLine.EndsWith('!') ||
                    lastLine.EndsWith('?'))
                {
                    var st = new StrippableText(l);
                    if (st.StrippedText.StartsWith('i') && !st.Pre.EndsWith('[') && !st.Pre.EndsWith('(') && !st.Pre.EndsWith("...", StringComparison.Ordinal))
                    {
                        if (string.IsNullOrEmpty(lastLine) || (!lastLine.EndsWith("...", StringComparison.Ordinal) && !EndsWithAbbreviation(lastLine, _abbreviationList)))
                        {
                            l = st.Pre + "I" + st.StrippedText.Remove(0, 1) + st.Post;
                        }
                    }
                }
                sb.AppendLine(l);
            }
            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static bool EndsWithAbbreviation(string line, HashSet<string> abbreviationList)
        {
            if (string.IsNullOrEmpty(line))
            {
                return false;
            }
            if (line.Length > 5 && line[line.Length - 3] == '.' && char.IsLetter(line[line.Length - 2]))
            {
                return true;
            }
            if (abbreviationList != null)
            {
                foreach (string abbreviation in abbreviationList)
                {
                    if (line.EndsWith(" " + abbreviation, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public string FixOcrErrorsViaHardcodedRules(string input, string lastLine, HashSet<string> abbreviationList)
        {
            var text = input;
            if (!Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                return text;
            }

            text = text.Replace(",...", "...");

            if (text.StartsWith("..", StringComparison.Ordinal) && !text.StartsWith("...", StringComparison.Ordinal))
            {
                text = "." + text;
            }

            string pre = string.Empty;
            if (text.StartsWith("- ", StringComparison.Ordinal))
            {
                pre = "- ";
                text = text.Remove(0, 2);
            }
            else if (text.StartsWith('-'))
            {
                pre = "-";
                text = text.Remove(0, 1);
            }

            bool hasDotDot = text.Contains("..", StringComparison.Ordinal) || text.Contains(". .", StringComparison.Ordinal);
            if (hasDotDot)
            {
                if (text.Length > 5 && text.StartsWith("..", StringComparison.Ordinal) && Utilities.AllLettersAndNumbers.Contains(text[2]))
                {
                    text = "..." + text.Remove(0, 2);
                }

                if (text.Length > 7 && text.StartsWith("<i>..", StringComparison.Ordinal) && Utilities.AllLettersAndNumbers.Contains(text[5]))
                {
                    text = "<i>..." + text.Remove(0, 5);
                }

                if (text.Length > 5 && text.StartsWith(".. ", StringComparison.Ordinal) && Utilities.AllLettersAndNumbers.Contains(text[3]))
                {
                    text = "..." + text.Remove(0, 3);
                }

                if (text.Length > 7 && text.StartsWith("<i>.. ", StringComparison.Ordinal) && Utilities.AllLettersAndNumbers.Contains(text[6]))
                {
                    text = "<i>..." + text.Remove(0, 6);
                }

                if (text.Contains(Environment.NewLine + ".. ", StringComparison.Ordinal))
                {
                    text = text.Replace(Environment.NewLine + ".. ", Environment.NewLine + "...");
                }

                if (text.Contains(Environment.NewLine + "<i>.. ", StringComparison.Ordinal))
                {
                    text = text.Replace(Environment.NewLine + "<i>.. ", Environment.NewLine + "<i>...");
                }

                if (text.StartsWith(". ..", StringComparison.Ordinal))
                {
                    text = "..." + text.Remove(0, 4);
                }

                if (text.StartsWith(".. .", StringComparison.Ordinal))
                {
                    text = "..." + text.Remove(0, 4);
                }

                if (text.StartsWith(". . .", StringComparison.Ordinal))
                {
                    text = "..." + text.Remove(0, 5);
                }

                if (text.StartsWith("... ", StringComparison.Ordinal))
                {
                    text = text.Remove(3, 1);
                }
            }

            text = pre + text;

            if (hasDotDot)
            {
                if (text.StartsWith("<i>. ..", StringComparison.Ordinal))
                {
                    text = "<i>..." + text.Remove(0, 7);
                }

                if (text.StartsWith("<i>.. .", StringComparison.Ordinal))
                {
                    text = "<i>..." + text.Remove(0, 7);
                }

                if (text.StartsWith("<i>. . .", StringComparison.Ordinal))
                {
                    text = "<i>..." + text.Remove(0, 8);
                }

                if (text.StartsWith("<i>... ", StringComparison.Ordinal))
                {
                    text = text.Remove(6, 1);
                }

                if (text.StartsWith(". . <i>.", StringComparison.Ordinal))
                {
                    text = "<i>..." + text.Remove(0, 8);
                }

                if (text.StartsWith("...<i>", StringComparison.Ordinal) && (text.IndexOf("</i>", StringComparison.Ordinal) > text.IndexOf(' ')))
                {
                    text = "<i>..." + text.Remove(0, 6);
                }

                if (text.EndsWith(". ..", StringComparison.Ordinal))
                {
                    text = text.Remove(text.Length - 4, 4) + "...";
                }

                if (text.EndsWith(".. .", StringComparison.Ordinal))
                {
                    text = text.Remove(text.Length - 4, 4) + "...";
                }

                if (text.EndsWith(". . .", StringComparison.Ordinal))
                {
                    text = text.Remove(text.Length - 5, 5) + "...";
                }

                if (text.EndsWith(". ...", StringComparison.Ordinal))
                {
                    text = text.Remove(text.Length - 5, 5) + "...";
                }

                if (text.EndsWith(". ..</i>", StringComparison.Ordinal))
                {
                    text = text.Remove(text.Length - 8, 8) + "...</i>";
                }

                if (text.EndsWith(".. .</i>", StringComparison.Ordinal))
                {
                    text = text.Remove(text.Length - 8, 8) + "...</i>";
                }

                if (text.EndsWith(". . .</i>", StringComparison.Ordinal))
                {
                    text = text.Remove(text.Length - 9, 9) + "...</i>";
                }

                if (text.EndsWith(". ...</i>", StringComparison.Ordinal))
                {
                    text = text.Remove(text.Length - 9, 9) + "...</i>";
                }

                if (text.EndsWith(".</i> . .", StringComparison.Ordinal))
                {
                    text = text.Remove(text.Length - 9, 9) + "...</i>";
                }

                if (text.EndsWith(".</i>..", StringComparison.Ordinal))
                {
                    text = text.Remove(text.Length - 7, 7) + "...</i>";
                }

                text = text.Replace(".</i> . ." + Environment.NewLine, "...</i>" + Environment.NewLine);

                text = text.Replace(".. ?", "..?");
                text = text.Replace("..?", "...?");
                text = text.Replace("....?", "...?");

                text = text.Replace(".. !", "..!");
                text = text.Replace("..!", "...!");
                text = text.Replace("....!", "...!");

                text = text.Replace("... ?", "...?");
                text = text.Replace("... !", "...!");

                text = text.Replace("....", "...");
                text = text.Replace("....", "...");

                if (text.StartsWith("- ...", StringComparison.Ordinal) && lastLine != null && lastLine.EndsWith("...", StringComparison.Ordinal) && !text.Contains(Environment.NewLine + "-", StringComparison.Ordinal))
                {
                    text = text.Remove(0, 2);
                }

                if (text.StartsWith("-...", StringComparison.Ordinal) && lastLine != null && lastLine.EndsWith("...", StringComparison.Ordinal) && !text.Contains(Environment.NewLine + "-", StringComparison.Ordinal))
                {
                    text = text.Remove(0, 1);
                }
            }

            var lastLineNoTags = lastLine;
            if (!string.IsNullOrEmpty(lastLineNoTags))
            {
                lastLineNoTags = HtmlUtil.RemoveHtmlTags(lastLineNoTags);
                lastLineNoTags = lastLineNoTags.Trim('♪', '♫', ' ', '"');
                if (lastLine.EndsWith(']') && lastLineNoTags.IndexOf('[') > 0 &&
                    Utilities.CountTagInText(lastLineNoTags, '[') == 1 && Utilities.CountTagInText(lastLineNoTags, ']') == 1)
                {
                    lastLineNoTags = lastLineNoTags.Substring(0, lastLineNoTags.IndexOf('[')).Trim();
                }
            }

            if (string.IsNullOrEmpty(lastLine) || lastLineNoTags.HasSentenceEnding(Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(_threeLetterIsoLanguageName)))
            {
                lastLine = HtmlUtil.RemoveHtmlTags(lastLine);
                var st = new StrippableText(text);
                if (lastLine == null || !lastLine.EndsWith("...", StringComparison.Ordinal) && !EndsWithAbbreviation(lastLine, abbreviationList))
                {
                    if (st.StrippedText.Length > 0 && !char.IsUpper(st.StrippedText[0]) && !st.Pre.EndsWith('[') && !st.Pre.EndsWith('(') &&
                        !st.Pre.Contains("...", StringComparison.Ordinal) &&
                        !st.Pre.Contains('…'))
                    {
                        if (!HtmlUtil.StartsWithUrl(st.StrippedText))
                        {
                            var uppercaseLetter = char.ToUpper(st.StrippedText[0]);
                            if (st.StrippedText.Length > 1 && uppercaseLetter == 'L' && (st.StrippedText[1] == ' ' || char.IsLower(st.StrippedText[1])))
                            {
                                uppercaseLetter = 'I';
                            }
                            if (st.StrippedText.Length == 1 && uppercaseLetter == 'L')
                            {
                                uppercaseLetter = 'I';
                            }

                            if ((st.StrippedText.StartsWith("lo ", StringComparison.Ordinal) || st.StrippedText.Equals("lo.", StringComparison.Ordinal)) && _threeLetterIsoLanguageName.Equals("ita", StringComparison.Ordinal))
                            {
                                uppercaseLetter = 'I';
                            }

                            if ((st.StrippedText.StartsWith("k ", StringComparison.Ordinal) || st.StrippedText.StartsWith("m ", StringComparison.Ordinal) || st.StrippedText.StartsWith("n ", StringComparison.Ordinal) || st.StrippedText.StartsWith("r ", StringComparison.Ordinal) || st.StrippedText.StartsWith("s ", StringComparison.Ordinal) || st.StrippedText.StartsWith("t ", StringComparison.Ordinal)) &&
                                st.Pre.EndsWith('\'') && _threeLetterIsoLanguageName.Equals("nld", StringComparison.Ordinal))
                            {
                                uppercaseLetter = st.StrippedText[0];
                            }

                            if ((st.StrippedText.StartsWith("l-I'll ", StringComparison.Ordinal) || st.StrippedText.StartsWith("l-l'll ", StringComparison.Ordinal)) && _threeLetterIsoLanguageName.Equals("eng", StringComparison.Ordinal))
                            {
                                uppercaseLetter = 'I';
                                st.StrippedText = "I-I" + st.StrippedText.Remove(0, 3);
                            }

                            st.StrippedText = uppercaseLetter + st.StrippedText.Substring(1);
                            text = st.Pre + st.StrippedText + st.Post;
                        }
                    }
                }
            }

            // lines ending with ". should often end at ... (of no other quotes exists near by)
            if ((lastLine == null || !lastLine.Contains('"')) &&
                text.EndsWith("\".", StringComparison.Ordinal) && text.IndexOf('"') == text.LastIndexOf('"') && text.Length > 3)
            {
                var lastChar = text[text.Length - 3];
                if (!char.IsDigit(lastChar))
                {
                    int position = text.Length - 2;
                    text = text.Remove(position).Insert(position, "...");
                }
            }

            // change '<number><space>1' to '<number>1'
            if (text.Contains('1'))
            {
                var match = RegexNumber1.Match(text);
                while (match.Success)
                {
                    text = text.Remove(match.Index, 1);
                    match = RegexNumber1.Match(text, match.Index);
                }
            }

            // change '' to "
            text = text.Replace("''", "\"");

            // change 'sequeI of' to 'sequel of'
            if (text.Contains('I'))
            {
                var match = RegexUppercaseI.Match(text);
                while (match.Success)
                {
                    bool doFix = !(match.Index >= 1 && text.Substring(match.Index - 1).StartsWith("Mc", StringComparison.Ordinal));
                    if (match.Index >= 2 && text.Substring(match.Index - 2).StartsWith("Mac", StringComparison.Ordinal))
                    {
                        doFix = false;
                    }

                    if (doFix)
                    {
                        text = text.Substring(0, match.Index + 1) + "l" + text.Substring(match.Index + 2);
                    }

                    if (match.Index + 1 < text.Length)
                    {
                        match = RegexUppercaseI.Match(text, match.Index + 1);
                    }
                    else
                    {
                        break; // end while
                    }
                }
            }

            // change 'NlCE' to 'NICE'
            if (text.Contains('l'))
            {
                var match = RegexLowercaseL.Match(text);
                while (match.Success)
                {
                    text = text.Substring(0, match.Index + 1) + "I" + text.Substring(match.Index + 2);
                    match = RegexLowercaseL.Match(text);
                }
            }

            if (text.EndsWith(". \"</i>", StringComparison.Ordinal))
            {
                text = text.Remove(text.Length - 6, 1);
            }

            if (text.Contains(". \"</i>" + Environment.NewLine, StringComparison.Ordinal))
            {
                var idx = text.IndexOf(". \"</i>" + Environment.NewLine, StringComparison.Ordinal);
                if (idx > 0)
                {
                    text = text.Remove(idx + 1, 1);
                }
            }

            return text;
        }

        public string FixOcrErrorViaLineReplaceList(string input)
        {
            return _ocrFixReplaceList.FixOcrErrorViaLineReplaceList(input);
        }

        public string FixUnknownWordsViaGuessOrPrompt(out int wordsNotFound, string line, int index, Bitmap bitmap, bool autoFix, bool promptForFixingErrors, bool log, AutoGuessLevel autoGuess)
        {
            var localIgnoreWords = new List<string>();
            wordsNotFound = 0;

            if (promptForFixingErrors && line.Length == 1 && !IsWordKnownOrNumber(line, line))
            {
                SpellCheckOcrTextResult res = SpellCheckOcrText(line, bitmap, line, localIgnoreWords);
                if (res.FixedWholeLine || res.Fixed)
                {
                    return res.Line;
                }

                wordsNotFound++;
                return line;
            }

            if (_hunspell == null)
            {
                return line;
            }

            string tempLine = line;
            const string p = " ¡¿,.!?:;()[]{}+-$£\"„”“#&%…—♪\r\n";
            var trimChars = p.ToArray();
            bool hasAllUpperWord = false;
            foreach (var w in HtmlUtil.RemoveHtmlTags(line, true).Split(' ', '\r', '\n'))
            {
                var word = w.Trim(trimChars);
                if (word.Length > 1 && word == word.ToUpperInvariant())
                {
                    hasAllUpperWord = true;
                    break;
                }
            }

            foreach (string name in _nameMultiWordListAndWordsWithPeriods)
            {
                int start = tempLine.FastIndexOf(name);
                if (start < 0 && hasAllUpperWord)
                {
                    start = tempLine.FastIndexOf(name.ToUpperInvariant());
                }
                if (start == 0 || (start > 0 && p.Contains(tempLine[start - 1])))
                {
                    int end = start + name.Length;
                    if (end == tempLine.Length || p.Contains(tempLine[end]))
                    {
                        tempLine = tempLine.Remove(start, name.Length);
                    }
                }
            }

            int minLength = 2;
            if (Configuration.Settings.Tools.CheckOneLetterWords)
            {
                minLength = 1;
            }

            var words = new List<string>();
            var splitChars = SpellCheckWordLists.SplitChars.Where(ch => ch != '/' && ch != '|').ToArray();
            foreach (var w in tempLine
                .Replace("<i>", string.Empty).Replace("</i>", string.Empty)
                .Replace("<b>", string.Empty).Replace("</b>", string.Empty)
                .Replace("<u>", string.Empty).Replace("</u>", string.Empty)
                .Split(splitChars))
            {
                words.Add(w.Trim(trimChars));
            }

            for (int i = 0; i < words.Count && i < 1000; i++)
            {
                string word = words[i].TrimStart('\'');
                string wordNotEndTrimmed = word;
                word = word.TrimEnd('\'');
                if (!IsWordKnownOrNumber(word, line) && !localIgnoreWords.Contains(word))
                {
                    var correct = false;
                    if (word.Length > 1)
                    {
                        if (_wordSpellOkList.Contains(word))
                        {
                            correct = true;
                        }
                        else if (DoSpell(word))
                        {
                            correct = true;
                            _wordSpellOkList.Add(word);
                        }

                        if (!correct)
                        {
                            correct = word.Length > minLength + 1 && DoSpell(word.Trim('\''));
                        }

                        if (!correct && word.Length > 3 && !word.EndsWith("ss", StringComparison.Ordinal) && !string.IsNullOrEmpty(_threeLetterIsoLanguageName) &&
                            (_threeLetterIsoLanguageName == "eng" || _threeLetterIsoLanguageName == "dan" || _threeLetterIsoLanguageName == "swe" || _threeLetterIsoLanguageName == "nld"))
                        {
                            var w = word.TrimEnd('s');
                            correct = DoSpell(w);
                            if (!correct && w.EndsWith('\''))
                            {
                                correct = DoSpell(w.Remove(w.Length - 1, 1));
                            }
                        }
                    }
                    else
                    {
                        correct = !Configuration.Settings.Tools.CheckOneLetterWords; // hunspell allows too many single letter words
                    }

                    if (!correct && _userWordList.Contains(word))
                    {
                        correct = true;
                    }

                    if (!correct && !line.Contains(word, StringComparison.Ordinal))
                    {
                        correct = true; // already fixed
                    }

                    if (!correct && Configuration.Settings.Tools.SpellCheckEnglishAllowInQuoteAsIng && wordNotEndTrimmed.EndsWith('\'') &&
                        SpellCheckDictionaryName.StartsWith("en_", StringComparison.Ordinal) && word.EndsWith("in", StringComparison.OrdinalIgnoreCase))
                    {
                        correct = DoSpell(word + "g");
                    }

                    if (_threeLetterIsoLanguageName == "eng" && (word.Equals("a", StringComparison.OrdinalIgnoreCase) || word == "I"))
                    {
                        correct = true;
                    }
                    else if (_threeLetterIsoLanguageName == "dan" && word.Equals("i", StringComparison.OrdinalIgnoreCase))
                    {
                        correct = true;
                    }

                    if (!correct && _threeLetterIsoLanguageName.Equals("ara", StringComparison.Ordinal))
                    {
                        var trimmed = word.Trim('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ',', '،', '؟', '»');
                        if (trimmed != word)
                        {
                            if (_userWordList.Contains(trimmed))
                            {
                                correct = true;
                            }
                            else
                            {
                                correct = DoSpell(trimmed);
                            }
                        }
                    }

                    if (!correct)
                    {
                        //look for match via dash'ed word, e.g. sci-fi
                        string dashedWord = GetDashedWordBefore(word, line, words, i);
                        if (!string.IsNullOrEmpty(dashedWord))
                        {
                            correct = IsWordKnownOrNumber(dashedWord, line);
                            if (!correct)
                            {
                                correct = DoSpell(dashedWord);
                            }
                        }
                        if (!correct)
                        {
                            dashedWord = GetDashedWordAfter(word, line, words, i);
                            if (!string.IsNullOrEmpty(dashedWord))
                            {
                                correct = IsWordKnownOrNumber(dashedWord, line);
                                if (!correct)
                                {
                                    correct = DoSpell(dashedWord);
                                }
                            }
                        }

                        if (!correct && _spellCheckWordLists.HasUserWord("-" + word))
                        {
                            correct = true;
                        }
                    }

                    if (!correct && word.Contains('/') && !word.Contains("//", StringComparison.Ordinal))
                    {
                        correct = word.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                            .All(w => w.Length > 2 && (DoSpell(w) || IsWordKnownOrNumber(word, line)));
                    }

                    if (!correct && word.Length == 1 && i < words.Count - 1 && words[i + 1].Length == 1)
                    {
                        var abbreviation = word + "." + words[i + 1] + ".";
                        if (_abbreviationList.Contains(abbreviation) && line.Contains(abbreviation, StringComparison.Ordinal))
                        {
                            correct = true;
                            words[i + 1] = string.Empty;
                        }
                        else if (i < words.Count - 2 && words[i + 2].Length == 1)
                        {
                            abbreviation = word + "." + words[i + 1] + "." + words[i + 2] + ".";
                            if (_abbreviationList.Contains(abbreviation) && line.Contains(abbreviation, StringComparison.Ordinal))
                            {
                                correct = true;
                                words[i + 1] = string.Empty;
                                words[i + 2] = string.Empty;
                            }
                        }
                    }

                    if (word.Length == 0)
                    {
                        correct = true;
                    }

                    if (!correct)
                    {
                        wordsNotFound++;
                        if (log)
                        {
                            string nf = word;
                            if (nf.StartsWith("<i>", StringComparison.Ordinal))
                            {
                                nf = nf.Remove(0, 3);
                            }

                            if (nf.Trim().Length > 0)
                            {
                                UnknownWordsFound.Add(new UnknownWord(index, nf));
                            }
                        }

                        if (autoFix && autoGuess != AutoGuessLevel.None)
                        {
                            var guesses = new List<string>();

                            // Name starting with "l" instead of 'I'
                            if (word.StartsWith('l') && word.Length > 3 && !_nameList.Contains(word))
                            {
                                var w = "I" + word.Substring(1);
                                if (_nameList.Contains(w))
                                {
                                    guesses.Add(w);
                                }
                            }

                            if (!correct && autoFix && word.Length > 3 && char.IsUpper(word[0]) && !_nameList.Contains(word))
                            {
                                var rest = word.Substring(1);
                                if (rest != rest.ToUpperInvariant())
                                {
                                    var newWord = word[0] + rest.ToLowerInvariant();
                                    if (_nameList.Contains(newWord))
                                    {
                                        guesses.Add(newWord);
                                    }
                                }
                            }

                            var wordWithVerticalLine = word.Replace("|", "l");
                            if (word.Length > 3 && DoSpell(wordWithVerticalLine))
                            {
                                if (word == word.ToUpperInvariant())
                                {
                                    wordWithVerticalLine = wordWithVerticalLine.ToUpperInvariant();
                                }

                                guesses.Add(wordWithVerticalLine);
                            }

                            if (word.Length > 4 && autoGuess == AutoGuessLevel.Aggressive)
                            {
                                guesses.AddRange((List<string>)_ocrFixReplaceList.CreateGuessesFromLetters(word, _threeLetterIsoLanguageName));

                                if (word[0] == 'L')
                                {
                                    guesses.Add("I" + word.Substring(1));
                                }

                                if (word.Contains('$'))
                                {
                                    guesses.Add(word.Replace("$", "s"));
                                }

                                if (word.Contains('l') && word.RemoveChar('l').Length > 3)
                                {
                                    var lowerLToUpperI = word.Replace('l', 'I');
                                    if (lowerLToUpperI == lowerLToUpperI.ToUpperInvariant())
                                    {
                                        guesses.Add(lowerLToUpperI);
                                    }
                                }

                                string wordWithCasingChanged = GetWordWithDominatedCasing(word);
                                if (DoSpell(word.ToLowerInvariant()))
                                {
                                    guesses.Insert(0, wordWithCasingChanged);
                                }
                            }
                            else if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
                            {
                                if (word.Length > 2 && word[0] == 'L')
                                {
                                    guesses.Add("I" + word.Substring(1));
                                }

                                if (word.Length > 2 && word[0] == 'I' && char.IsLower(word[1]))
                                {
                                    guesses.Add("l" + word.Substring(1));
                                }

                                if (i == 0)
                                {
                                    guesses.Add(word.Replace(@"\/", "V"));
                                }
                                else
                                {
                                    guesses.Add(word.Replace(@"\/", "v"));
                                }

                                guesses.Add(word.Replace("ﬁ", "fi"));
                                guesses.Add(word.Replace("ﬁ", "fj"));
                                guesses.Add(word.Replace("ﬂ", "fl"));
                                if (word.Contains('$'))
                                {
                                    guesses.Add(word.Replace("$", "s"));
                                }

                                if (!word.EndsWith('€') && !word.StartsWith('€'))
                                {
                                    guesses.Add(word.Replace("€", "e"));
                                }

                                guesses.Add(word.Replace("/", "l"));
                                guesses.Add(word.Replace(")/", "y"));
                            }
                            foreach (string guess in guesses)
                            {
                                if (!(guess.Length == 2 && guess[1] == ' ') && IsWordOrWordsCorrect(guess))
                                {
                                    string replacedLine = OcrFixReplaceList.ReplaceWord(line, word, guess);
                                    if (replacedLine != line)
                                    {
                                        if (log)
                                        {
                                            AutoGuessesUsed.Add(new AutoGuess(index, word, guess, line));
                                        }

                                        line = replacedLine;
                                        wordsNotFound--;
                                        if (log && UnknownWordsFound.Count > 0)
                                        {
                                            UnknownWordsFound.RemoveAt(UnknownWordsFound.Count - 1);
                                        }

                                        correct = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!correct && promptForFixingErrors)
                        {
                            var suggestions = new List<string>();

                            if ((word == "Lt's" || word == "Lt'S") && SpellCheckDictionaryName.StartsWith("en_", StringComparison.Ordinal))
                            {
                                suggestions.Add("It's");
                            }
                            else
                            {
                                if (!string.Equals(word, "LT'S", StringComparison.InvariantCultureIgnoreCase) && !string.Equals(word, "SOX'S", StringComparison.InvariantCultureIgnoreCase)) // TODO: Get fixed nhunspell
                                {
                                    suggestions = DoSuggest(word); // 0.9.6 fails on "Lt'S"
                                }
                            }

                            if (word.StartsWith("<i>", StringComparison.Ordinal))
                            {
                                word = word.Remove(0, 3);
                            }

                            if (word.EndsWith("</i>", StringComparison.Ordinal))
                            {
                                word = word.Remove(word.Length - 4, 4);
                            }

                            SpellCheckOcrTextResult res = SpellCheckOcrText(line, bitmap, word, suggestions);
                            if (Abort)
                            {
                                return null;
                            }
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

        private static string GetDashedWordBefore(string word, string line, List<string> words, int index)
        {
            if (index > 0 && line.Contains(words[index - 1] + "-" + word, StringComparison.Ordinal))
            {
                return HtmlUtil.RemoveOpenCloseTags(words[index - 1] + "-" + word, HtmlUtil.TagItalic);
            }

            return null;
        }

        private static string GetDashedWordAfter(string word, string line, List<string> words, int index)
        {
            if (index < words.Count - 1 && line.Contains(word + "-" + words[index + 1].Replace("</i>", string.Empty), StringComparison.Ordinal))
            {
                return HtmlUtil.RemoveOpenCloseTags(word + "-" + words[index + 1], HtmlUtil.TagItalic);
            }

            return null;
        }

        private static string GetWordWithDominatedCasing(string word)
        {
            int lowercase = 0;
            int uppercase = 0;
            for (int i = 0; i < word.Length; i++)
            {
                var ch = word[i];
                if (char.IsLower(ch))
                {
                    lowercase++;
                }
                else if (char.IsUpper(ch))
                {
                    uppercase++;
                }
            }
            if (uppercase > lowercase)
            {
                return word.ToUpperInvariant();
            }

            return word.ToLowerInvariant();
        }

        /// <summary>
        /// SpellCheck for OCR
        /// </summary>
        /// <returns>True, if word is fixed</returns>
        private SpellCheckOcrTextResult SpellCheckOcrText(string line, Bitmap bitmap, string word, List<string> suggestions)
        {
            var result = new SpellCheckOcrTextResult { Fixed = false, FixedWholeLine = false, Line = null, Word = null };
            _spellCheck.Initialize(word, suggestions, line, bitmap, IsBinaryImageCompareOrNOcr);
            _spellCheck.ShowDialog(_parentForm);
            LastAction = _spellCheck.ActionResult;
            switch (_spellCheck.ActionResult)
            {
                case OcrSpellCheck.Action.Abort:
                    Abort = true;
                    break;
                case OcrSpellCheck.Action.AddToUserDictionary:
                    if (_userWordListXmlFileName != null)
                    {
                        Utilities.AddToUserDictionary(_spellCheck.Word.Trim().ToLowerInvariant(), _fiveLetterWordListLanguageName);
                        _userWordList.Add(_spellCheck.Word.Trim().ToLowerInvariant());
                    }
                    result.Word = _spellCheck.Word;
                    result.Fixed = true;
                    result.Line = line;
                    if (word == result.Word)
                    {
                        return result;
                    }

                    break;
                case OcrSpellCheck.Action.AddToNames:
                case OcrSpellCheck.Action.AddToNamesOnly:
                    result.Word = _spellCheck.Word;
                    result.Fixed = true;
                    try
                    {
                        string s = _spellCheck.Word.Trim();
                        _nameListObj?.Add(s);
                        if (s.Contains(' '))
                        {
                            _nameMultiWordList.Add(s);
                        }
                        else
                        {
                            _nameList.Add(s);
                            _nameListUppercase.Add(s.ToUpperInvariant());
                            if (_fiveLetterWordListLanguageName.StartsWith("en", StringComparison.Ordinal))
                            {
                                if (!s.EndsWith('s'))
                                {
                                    _nameListWithApostrophe.Add(s + "'s");
                                }
                                else
                                {
                                    _nameListWithApostrophe.Add(s + "'");
                                }
                            }
                        }
                    }
                    catch
                    {
                        _wordSkipList.Add(_spellCheck.Word);
                    }
                    result.Line = line;
                    if (word == result.Word || _spellCheck.ActionResult == OcrSpellCheck.Action.AddToNamesOnly)
                    {
                        return result;
                    }

                    break;
                case OcrSpellCheck.Action.AlwaysUseSuggestion:
                    try
                    {
                        _ocrFixReplaceList.AddWordOrPartial(word, _spellCheck.Word);
                        if (!word.Contains(' '))
                        {
                            _spellCheckWordLists?.UseAlwaysListAdd(word, _spellCheck.Word);
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception + Environment.NewLine + exception.StackTrace);
                        _wordSkipList.Add(word);
                    }
                    result.Fixed = true;
                    result.Word = _spellCheck.Word;
                    break;
                case OcrSpellCheck.Action.ChangeAndSave:
                    try
                    {
                        _ocrFixReplaceList.AddWordOrPartial(word, _spellCheck.Word);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception + Environment.NewLine + exception.StackTrace);
                        _wordSkipList.Add(word);
                    }
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
                case OcrSpellCheck.Action.ChangeAllWholeText:
                    _ocrFixReplaceList.AddToWholeLineList(_spellCheck.OriginalWholeText, _spellCheck.Paragraph);
                    result.Line = _spellCheck.Paragraph;
                    result.FixedWholeLine = true;
                    break;
                case OcrSpellCheck.Action.SkipAll:
                    _wordSkipList.Add(_spellCheck.Word);
                    _wordSkipList.Add(_spellCheck.Word.ToUpperInvariant());
                    if (_spellCheck.Word.Length > 1)
                    {
                        _wordSkipList.Add(char.ToUpper(_spellCheck.Word[0]) + _spellCheck.Word.Substring(1));
                    }

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
                case OcrSpellCheck.Action.InspectCompareMatches:
                    Abort = true;
                    break;
            }
            if (result.Fixed)
            {
                result.Line = OcrFixReplaceList.ReplaceWord(line, word, result.Word);
            }
            return result;
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
                    {
                        return true;
                    }

                    if (s.Length > 10 && s.Contains('/'))
                    {
                        string[] ar = s.Split('/');
                        if (ar.Length == 2)
                        {
                            if (ar[0].Length > 3 && ar[1].Length > 3)
                            {
                                string a = ar[0];
                                if (a == a.ToUpperInvariant())
                                {
                                    a = a[0] + a.Substring(1).ToLowerInvariant();
                                }

                                string b = ar[0];
                                if (b == b.ToUpperInvariant())
                                {
                                    b = b[0] + b.Substring(1).ToLowerInvariant();
                                }

                                if ((DoSpell(a) || IsWordKnownOrNumber(a, word)) &&
                                    (DoSpell(b) || IsWordKnownOrNumber(b, word)))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
            }
            return true;
        }

        public bool IsWordKnownOrNumber(string word, string line)
        {
            if (double.TryParse(word.TrimStart('\'').Replace("$", string.Empty).Replace("£", string.Empty).Replace("¢", string.Empty), out _))
            {
                return true;
            }

            if (_wordSkipList.Contains(word))
            {
                return true;
            }

            if (_nameList.Contains(word.Trim('\'')))
            {
                return true;
            }

            if (_nameListUppercase.Contains(word.Trim('\'')))
            {
                return true;
            }

            if (_userWordList.Contains(word.ToLowerInvariant()))
            {
                return true;
            }

            if (_userWordList.Contains(word.Trim('\'').ToLowerInvariant()))
            {
                return true;
            }

            if (word.Length > 2 && _nameListUppercase.Contains(word))
            {
                return true;
            }

            if (word.Length > 2 && _nameListWithApostrophe.Contains(word))
            {
                return true;
            }

            if (_nameListObj != null && _nameListObj.IsInNamesMultiWordList(line, word))
            {
                return true;
            }

            return false;
        }

        public int CountUnknownWordsViaDictionary(string line, out int numberOfCorrectWords)
        {
            numberOfCorrectWords = 0;
            if (_hunspell == null)
            {
                return 0;
            }

            int minLength = 2;
            if (Configuration.Settings.Tools.CheckOneLetterWords)
            {
                minLength = 1;
            }

            int wordsNotFound = 0;
            var words = HtmlUtil.RemoveOpenCloseTags(line, HtmlUtil.TagItalic).Split(" \r\n\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i].Trim(SpellCheckWordLists.SplitChars.ToArray());
                if (word.Length >= minLength)
                {
                    if (!IsWordKnownOrNumber(word, line))
                    {
                        bool correct = word.Length > 1 && _hunspell.Spell(word);
                        if (!correct)
                        {
                            correct = word.Length > 2 && _hunspell.Spell(word.Trim('\''));
                        }

                        if (!correct && word.Length == 1 && _threeLetterIsoLanguageName == "eng" && (word == "I" || word == "A" || word == "a"))
                        {
                            correct = true;
                        }

                        if (correct)
                        {
                            numberOfCorrectWords++;
                        }
                        else
                        {
                            wordsNotFound++;
                        }
                    }
                    else if (word.Length > 3)
                    {
                        numberOfCorrectWords++;
                    }
                }
            }
            return wordsNotFound;
        }

        public void Dispose()
        {
            if (_hunspell != null)
            {
                _hunspell.Dispose();
                _hunspell = null;
            }
            if (_spellCheck != null)
            {
                _spellCheck.Dispose();
                _spellCheck = null;
            }
        }

    }
}
