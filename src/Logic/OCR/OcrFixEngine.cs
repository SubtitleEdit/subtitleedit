using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Logic.SpellCheck;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Forms.Ocr;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class OcrFixEngine : IDisposable
    {
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
        private HashSet<string> _abbreviationList;
        private HashSet<string> _userWordList = new HashSet<string>();
        private HashSet<string> _wordSkipList = new HashSet<string>();
        private Hunspell _hunspell;
        private OcrSpellCheck _spellCheck;
        private readonly Form _parentForm;
        private string _spellCheckDictionaryName;
        private readonly string _threeLetterIsoLanguageName;

        private static readonly Regex RegexAloneIasL = new Regex(@"\bl\b", RegexOptions.Compiled);
        private static readonly Regex RegexLowercaseL = new Regex("[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏ]l[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏ]", RegexOptions.Compiled);
        private static readonly Regex RegexUppercaseI = new Regex("[a-zæøåöääöéèàùâêîôûëï]I.", RegexOptions.Compiled);
        private static readonly Regex RegexNumber1 = new Regex(@"(?<=\d) 1(?!/\d)", RegexOptions.Compiled);

        private static readonly char[] SplitChars = { ' ', '¡', '¿', ',', '.', '!', '?', ':', ';', '(', ')', '[', ']', '{', '}', '+', '-', '£', '"', '„', '”', '“', '«', '»', '#', '&', '%', '…', '—', '♪', '\r', '\n', '؟' };

        public bool Abort { get; set; }
        public OcrSpellCheck.Action LastAction { get; set; } = OcrSpellCheck.Action.Abort;
        public bool IsBinaryImageCompare { get; set; }
        public List<string> AutoGuessesUsed { get; set; }
        public List<string> UnknownWordsFound { get; set; }
        public bool IsDictionaryLoaded { get; private set; }

        public CultureInfo DictionaryCulture { get; private set; }
        private const string ExpectedChars = " ¡¿,.!?:;()[]{}+-£\"”„“«»#&%\r\n؟"; // removed $

        /// <summary>
        /// Advanced OCR fixing via replace/spelling dictionaries + some hardcoded rules
        /// </summary>
        /// <param name="threeLetterIsoLanguageName">E.g. eng for English</param>
        /// <param name="hunspellName">Name of hunspell dictionary</param>
        /// <param name="parentForm">Used for centering/show spell check dialog</param>
        /// <param name="isBinaryImageCompare">Calling from OCR via "Image compare"</param>
        public OcrFixEngine(string threeLetterIsoLanguageName, string hunspellName, Form parentForm, bool isBinaryImageCompare = false)
        {
            if (threeLetterIsoLanguageName == "per")
                threeLetterIsoLanguageName = "fas";

            _threeLetterIsoLanguageName = threeLetterIsoLanguageName;
            _parentForm = parentForm;

            _spellCheck = new OcrSpellCheck { StartPosition = FormStartPosition.Manual, IsBinaryImageCompare = isBinaryImageCompare };
            _spellCheck.Location = new Point(parentForm.Left + (parentForm.Width / 2 - _spellCheck.Width / 2),
                                             parentForm.Top + (parentForm.Height / 2 - _spellCheck.Height / 2));

            _ocrFixReplaceList = OcrFixReplaceList.FromLanguageId(threeLetterIsoLanguageName);
            LoadSpellingDictionaries(threeLetterIsoLanguageName, hunspellName); // Hunspell etc.

            AutoGuessesUsed = new List<string>();
            UnknownWordsFound = new List<string>();
        }

        private void LoadSpellingDictionaries(string threeLetterIsoLanguageName, string hunspellName)
        {
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
                return;

            if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_gb", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_GB.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", CultureInfo.GetCultureInfo("en-GB"), "en_GB.dic", true);
                return;
            }
            if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_ca", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_CA.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", CultureInfo.GetCultureInfo("en-CA"), "en_CA.dic", true);
                return;
            }
            if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_au", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_AU.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", CultureInfo.GetCultureInfo("en-AU"), "en_AU.dic", true);
                return;
            }
            if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_za", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_ZA.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", CultureInfo.GetCultureInfo("en-ZA"), "en_ZA.dic", true);
                return;
            }
            if (threeLetterIsoLanguageName == "eng" && File.Exists(Path.Combine(dictionaryFolder, "en_US.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", CultureInfo.GetCultureInfo("en-US"), "en_US.dic", true);
                return;
            }

            foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                if (culture.ThreeLetterISOLanguageName == threeLetterIsoLanguageName)
                {
                    string dictionaryFileName = null;
                    if (!string.IsNullOrEmpty(hunspellName) && hunspellName.StartsWith(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, hunspellName + ".dic")))
                    {
                        dictionaryFileName = Path.Combine(dictionaryFolder, hunspellName + ".dic");
                        LoadSpellingDictionariesViaDictionaryFileName(threeLetterIsoLanguageName, culture, dictionaryFileName, true);
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
                                    name = name.Substring(0, 5);
                                var ci = CultureInfo.GetCultureInfo(name);
                                if (ci.ThreeLetterISOLanguageName == threeLetterIsoLanguageName || ci.ThreeLetterWindowsLanguageName.Equals(threeLetterIsoLanguageName, StringComparison.OrdinalIgnoreCase))
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

            foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (culture.ThreeLetterISOLanguageName == threeLetterIsoLanguageName)
                {
                    string dictionaryFileName = null;
                    foreach (string dic in Directory.GetFiles(dictionaryFolder, "*.dic"))
                    {
                        string name = Path.GetFileNameWithoutExtension(dic);
                        if (!string.IsNullOrEmpty(name) && !name.StartsWith("hyph", StringComparison.Ordinal))
                        {
                            try
                            {
                                name = name.Replace('_', '-');
                                if (name.Length > 5)
                                    name = name.Substring(0, 5);
                                var ci = CultureInfo.GetCultureInfo(name);
                                if (ci.ThreeLetterISOLanguageName == threeLetterIsoLanguageName || ci.ThreeLetterWindowsLanguageName.Equals(threeLetterIsoLanguageName, StringComparison.OrdinalIgnoreCase))
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
        }

        private void LoadSpellingDictionariesViaDictionaryFileName(string threeLetterIsoLanguageName, CultureInfo culture, string dictionaryFileName, bool resetSkipList)
        {
            _fiveLetterWordListLanguageName = Path.GetFileNameWithoutExtension(dictionaryFileName);
            if (_fiveLetterWordListLanguageName != null && _fiveLetterWordListLanguageName.Length > 5)
                _fiveLetterWordListLanguageName = _fiveLetterWordListLanguageName.Substring(0, 5);
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
            _abbreviationList = new HashSet<string>();

            bool isEnglish = threeLetterIsoLanguageName.Equals("eng", StringComparison.OrdinalIgnoreCase);
            foreach (string name in _nameList)
            {
                _nameListUppercase.Add(name.ToUpper());
                if (isEnglish)
                {
                    if (!name.EndsWith('s'))
                        _nameListWithApostrophe.Add(name + "'s");
                    else
                        _nameListWithApostrophe.Add(name + "'");
                }
                // Abbreviations.
                if (name.EndsWith('.'))
                {
                    _abbreviationList.Add(name);
                }
            }
            if (isEnglish)
            {
                if (!_abbreviationList.Contains("a.m."))
                    _abbreviationList.Add("a.m.");
                if (!_abbreviationList.Contains("p.m."))
                    _abbreviationList.Add("p.m.");
                if (!_abbreviationList.Contains("o.r."))
                    _abbreviationList.Add("o.r.");
            }
            // Load user words
            _userWordList = new HashSet<string>();
            _userWordListXmlFileName = Utilities.LoadUserWordList(_userWordList, _fiveLetterWordListLanguageName);
            foreach (string name in _userWordList)
            {
                if (name.EndsWith('.'))
                    _abbreviationList.Add(name);
            }

            // Load Hunspell spell checker
            try
            {
                if (!File.Exists(dictionary + ".dic"))
                {
                    var fileMatches = Directory.GetFiles(Utilities.DictionaryFolder, _fiveLetterWordListLanguageName + "*.dic");
                    if (fileMatches.Length > 0)
                        dictionary = fileMatches[0].Substring(0, fileMatches[0].Length - 4);
                }
                _hunspell?.Dispose();
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
                string[] parts = _spellCheckDictionaryName?.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                if (parts?.Length > 0)
                    return parts[parts.Length - 1];
                return string.Empty;
            }
            set
            {
                string spellCheckDictionaryName = Path.Combine(Utilities.DictionaryFolder, value);
                CultureInfo ci;
                try
                {
                    if (value == "sh")
                        ci = CultureInfo.GetCultureInfo("sr-Latn-RS");
                    else
                        ci = CultureInfo.GetCultureInfo(value);
                }
                catch
                {
                    ci = CultureInfo.CurrentUICulture;
                }
                LoadSpellingDictionariesViaDictionaryFileName(ci.ThreeLetterISOLanguageName, ci, spellCheckDictionaryName, false);
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
                        if (!list.ContainsKey(@from))
                            list.Add(@from, to);
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
                            list.Add(from, to);
                    }
                }
            }
            return list;
        }

        public string FixOcrErrors(string text, int index, string lastLine, bool logSuggestions, AutoGuessLevel autoGuess)
        {
            while (text.Contains(Environment.NewLine + " "))
                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
            while (text.Contains(" " + Environment.NewLine))
                text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
            while (text.Contains(Environment.NewLine + Environment.NewLine))
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            text = text.Trim();

            // Try to prevent resizing when fixing Ocr-hardcoded.
            var sb = new StringBuilder(text.Length + 2);
            var word = new StringBuilder();

            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
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

            string lastWord = null;
            for (int i = 0; i < text.Length; i++)
            {
                if (ExpectedChars.Contains(text[i]))
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
                            bool doFixWord = !(word.Length == 1 && sb.Length > 1 && sb.EndsWith('-'));
                            if (doFixWord)
                                fixedWord = _ocrFixReplaceList.FixCommonWordErrors(word.ToString());
                            else
                                fixedWord = word.ToString();
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
                bool doFixWord = !(word.Length == 1 && sb.Length > 1 && sb.EndsWith('-'));
                if (doFixWord)
                    fixedWord = _ocrFixReplaceList.FixCommonWordErrors(word.ToString());
                else
                    fixedWord = word.ToString();

                sb.Append(fixedWord);
            }

            text = FixCommonOcrLineErrors(sb.ToString(), lastLine);
            text = FixUnknownWordsViaGuessOrPrompt(out _, text, index, null, true, false, logSuggestions, autoGuess);
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
            return text;
        }

        internal static string FixFrenchLApostrophe(string text, string tag, string lastLine)
        {
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
                        endingBeforeThis = true;

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
                if (ExpectedChars.Contains(text[i])) // removed $
                {
                    if (word.Length > 0)
                    {
                        string fixedWord;
                        if (lastWord != null && lastWord.Contains("COLOR=", StringComparison.OrdinalIgnoreCase))
                            fixedWord = word.ToString();
                        else
                            fixedWord = _ocrFixReplaceList.FixCommonWordErrorsQuick(word.ToString());
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
            input = FixOcrErrorViaLineReplaceList(input);
            input = FixOcrErrorsViaHardcodedRules(input, lastLine, _abbreviationList);
            input = FixOcrErrorViaLineReplaceList(input);

            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                if (input.StartsWith('~'))
                    input = ("- " + input.Remove(0, 1)).Replace("  ", " ");

                input = input.Replace(Environment.NewLine + "~", Environment.NewLine + "- ").Replace("  ", " ");

                if (input.Length < 10 && input.Length > 4 && !input.Contains(Environment.NewLine) && input.StartsWith("II", StringComparison.Ordinal) && input.EndsWith("II", StringComparison.Ordinal))
                {
                    input = "\"" + input.Substring(2, input.Length - 4) + "\"";
                }

                // e.g. "selectionsu." -> "selections..."
                if (input.EndsWith("u.", StringComparison.Ordinal) && _hunspell != null)
                {
                    string[] words = input.Split(new[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        string lastWord = words[words.Length - 1].Trim();
                        if (lastWord.Length > 2 &&
                            char.IsLower(lastWord[0]) &&
                            !IsWordOrWordsCorrect(lastWord) &&
                            IsWordOrWordsCorrect(lastWord.Substring(0, lastWord.Length - 1)))
                            input = input.Substring(0, input.Length - 2) + "...";
                    }
                }

                // music notes
                if (input.StartsWith(".'", StringComparison.Ordinal) && input.EndsWith(".'", StringComparison.Ordinal))
                {
                    input = input.Replace(".'", Configuration.Settings.Tools.MusicSymbol);
                }
            }

            return input;
        }

        private string FixLowercaseIToUppercaseI(string input, string lastLine)
        {
            var sb = new StringBuilder();
            var lines = input.SplitToLines();
            for (int i = 0; i < lines.Count; i++)
            {
                string l = lines[i];

                if (i > 0)
                    lastLine = lines[i - 1];
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
                            l = st.Pre + "I" + st.StrippedText.Remove(0, 1) + st.Post;
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
                        return true;
                }
            }
            return false;
        }

        public string FixOcrErrorsViaHardcodedRules(string input, string lastLine, HashSet<string> abbreviationList)
        {
            if (!Configuration.Settings.Tools.OcrFixUseHardcodedRules)
                return input;

            input = input.Replace(",...", "...");

            if (input.StartsWith("..", StringComparison.Ordinal) && !input.StartsWith("...", StringComparison.Ordinal))
                input = "." + input;

            string pre = string.Empty;
            if (input.StartsWith("- ", StringComparison.Ordinal))
            {
                pre = "- ";
                input = input.Remove(0, 2);
            }
            else if (input.StartsWith('-'))
            {
                pre = "-";
                input = input.Remove(0, 1);
            }

            bool hasDotDot = input.Contains("..") || input.Contains(". .");
            if (hasDotDot)
            {
                if (input.Length > 5 && input.StartsWith("..", StringComparison.Ordinal) && Utilities.AllLettersAndNumbers.Contains(input[2]))
                    input = "..." + input.Remove(0, 2);
                if (input.Length > 7 && input.StartsWith("<i>..", StringComparison.Ordinal) && Utilities.AllLettersAndNumbers.Contains(input[5]))
                    input = "<i>..." + input.Remove(0, 5);

                if (input.Length > 5 && input.StartsWith(".. ", StringComparison.Ordinal) && Utilities.AllLettersAndNumbers.Contains(input[3]))
                    input = "..." + input.Remove(0, 3);
                if (input.Length > 7 && input.StartsWith("<i>.. ", StringComparison.Ordinal) && Utilities.AllLettersAndNumbers.Contains(input[6]))
                    input = "<i>..." + input.Remove(0, 6);
                if (input.Contains(Environment.NewLine + ".. "))
                    input = input.Replace(Environment.NewLine + ".. ", Environment.NewLine + "...");
                if (input.Contains(Environment.NewLine + "<i>.. "))
                    input = input.Replace(Environment.NewLine + "<i>.. ", Environment.NewLine + "<i>...");

                if (input.StartsWith(". ..", StringComparison.Ordinal))
                    input = "..." + input.Remove(0, 4);
                if (input.StartsWith(".. .", StringComparison.Ordinal))
                    input = "..." + input.Remove(0, 4);
                if (input.StartsWith(". . .", StringComparison.Ordinal))
                    input = "..." + input.Remove(0, 5);
                if (input.StartsWith("... ", StringComparison.Ordinal))
                    input = input.Remove(3, 1);
            }

            input = pre + input;

            if (hasDotDot)
            {
                if (input.StartsWith("<i>. ..", StringComparison.Ordinal))
                    input = "<i>..." + input.Remove(0, 7);
                if (input.StartsWith("<i>.. .", StringComparison.Ordinal))
                    input = "<i>..." + input.Remove(0, 7);

                if (input.StartsWith("<i>. . .", StringComparison.Ordinal))
                    input = "<i>..." + input.Remove(0, 8);
                if (input.StartsWith("<i>... ", StringComparison.Ordinal))
                    input = input.Remove(6, 1);
                if (input.StartsWith(". . <i>.", StringComparison.Ordinal))
                    input = "<i>..." + input.Remove(0, 8);

                if (input.StartsWith("...<i>", StringComparison.Ordinal) && (input.IndexOf("</i>", StringComparison.Ordinal) > input.IndexOf(' ')))
                    input = "<i>..." + input.Remove(0, 6);

                if (input.EndsWith(". ..", StringComparison.Ordinal))
                    input = input.Remove(input.Length - 4, 4) + "...";
                if (input.EndsWith(".. .", StringComparison.Ordinal))
                    input = input.Remove(input.Length - 4, 4) + "...";
                if (input.EndsWith(". . .", StringComparison.Ordinal))
                    input = input.Remove(input.Length - 5, 5) + "...";
                if (input.EndsWith(". ...", StringComparison.Ordinal))
                    input = input.Remove(input.Length - 5, 5) + "...";

                if (input.EndsWith(". ..</i>", StringComparison.Ordinal))
                    input = input.Remove(input.Length - 8, 8) + "...</i>";
                if (input.EndsWith(".. .</i>", StringComparison.Ordinal))
                    input = input.Remove(input.Length - 8, 8) + "...</i>";
                if (input.EndsWith(". . .</i>", StringComparison.Ordinal))
                    input = input.Remove(input.Length - 9, 9) + "...</i>";
                if (input.EndsWith(". ...</i>", StringComparison.Ordinal))
                    input = input.Remove(input.Length - 9, 9) + "...</i>";

                if (input.EndsWith(".</i> . .", StringComparison.Ordinal))
                    input = input.Remove(input.Length - 9, 9) + "...</i>";
                if (input.EndsWith(".</i>..", StringComparison.Ordinal))
                    input = input.Remove(input.Length - 7, 7) + "...</i>";
                input = input.Replace(".</i> . ." + Environment.NewLine, "...</i>" + Environment.NewLine);

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

                if (input.StartsWith("- ...", StringComparison.Ordinal) && lastLine != null && lastLine.EndsWith("...", StringComparison.Ordinal) && !(input.Contains(Environment.NewLine + "-")))
                    input = input.Remove(0, 2);
                if (input.StartsWith("-...", StringComparison.Ordinal) && lastLine != null && lastLine.EndsWith("...", StringComparison.Ordinal) && !(input.Contains(Environment.NewLine + "-")))
                    input = input.Remove(0, 1);
            }

            if (input.Length > 2 && input[0] == '-' && char.IsUpper(input[1]))
            {
                input = input.Insert(1, " ");
            }

            if (input.Length > 5 && input.StartsWith("<i>-", StringComparison.Ordinal) && char.IsUpper(input[4]))
            {
                input = input.Insert(4, " ");
            }

            int nlLen = Environment.NewLine.Length;
            int idx = input.IndexOf(Environment.NewLine + "-", StringComparison.Ordinal);
            if (idx > 0 && idx + nlLen + 1 < input.Length && char.IsUpper(input[idx + nlLen + 1]))
            {
                input = input.Insert(idx + Environment.NewLine.Length + 1, " ");
            }

            idx = input.IndexOf(Environment.NewLine + "<i>-", StringComparison.Ordinal);
            if (idx > 0 && idx + nlLen + 4 < input.Length && char.IsUpper(input[idx + nlLen + 4]))
            {
                input = input.Insert(idx + nlLen + 4, " ");
            }

            if (string.IsNullOrEmpty(lastLine) ||
                lastLine.EndsWith('.') ||
                lastLine.EndsWith('!') ||
                lastLine.EndsWith('?') ||
                lastLine.EndsWith(']') ||
                lastLine.EndsWith('♪'))
            {
                lastLine = HtmlUtil.RemoveHtmlTags(lastLine);
                var st = new StrippableText(input);
                if (lastLine == null || (!lastLine.EndsWith("...", StringComparison.Ordinal) && !EndsWithAbbreviation(lastLine, abbreviationList)))
                {
                    if (st.StrippedText.Length > 0 && !char.IsUpper(st.StrippedText[0]) && !st.Pre.EndsWith('[') && !st.Pre.EndsWith('(') &&
                        !st.Pre.Contains("...", StringComparison.Ordinal) &&
                        !st.Pre.Contains('…'))
                    {
                        if (!HtmlUtil.StartsWithUrl(st.StrippedText))
                        {
                            var uppercaseLetter = char.ToUpper(st.StrippedText[0]);
                            if (st.StrippedText.Length > 1 && uppercaseLetter == 'L' && @"abcdfghjklmnpqrstvwxz".Contains(st.StrippedText[1]))
                                uppercaseLetter = 'I';
                            if ((st.StrippedText.StartsWith("lo ", StringComparison.Ordinal) || st.StrippedText.Equals("lo.", StringComparison.Ordinal)) && _threeLetterIsoLanguageName.Equals("ita", StringComparison.Ordinal))
                                uppercaseLetter = 'I';
                            if ((st.StrippedText.StartsWith("k ", StringComparison.Ordinal) || st.StrippedText.StartsWith("m ", StringComparison.Ordinal) || st.StrippedText.StartsWith("n ", StringComparison.Ordinal) || st.StrippedText.StartsWith("r ", StringComparison.Ordinal) || st.StrippedText.StartsWith("s ", StringComparison.Ordinal) || st.StrippedText.StartsWith("t ", StringComparison.Ordinal)) &&
                                st.Pre.EndsWith('\'') && _threeLetterIsoLanguageName.Equals("nld", StringComparison.Ordinal))
                                uppercaseLetter = st.StrippedText[0];
                            if ((st.StrippedText.StartsWith("l-I'll ", StringComparison.Ordinal) || st.StrippedText.StartsWith("l-l'll ", StringComparison.Ordinal)) && _threeLetterIsoLanguageName.Equals("eng", StringComparison.Ordinal))
                            {
                                uppercaseLetter = 'I';
                                st.StrippedText = "I-I" + st.StrippedText.Remove(0, 3);
                            }
                            st.StrippedText = uppercaseLetter + st.StrippedText.Substring(1);
                            input = st.Pre + st.StrippedText + st.Post;
                        }
                    }
                }
            }

            // lines ending with ". should often end at ... (of no other quotes exists near by)
            if ((lastLine == null || !lastLine.Contains('"')) &&
                input.EndsWith("\".", StringComparison.Ordinal) && input.IndexOf('"') == input.LastIndexOf('"') && input.Length > 3)
            {
                var lastChar = input[input.Length - 3];
                if (!char.IsDigit(lastChar))
                {
                    int position = input.Length - 2;
                    input = input.Remove(position).Insert(position, "...");
                }
            }

            // change '<number><space>1' to '<number>1'
            if (input.Contains('1'))
            {
                var match = RegexNumber1.Match(input);
                while (match.Success)
                {
                    input = input.Remove(match.Index, 1);
                    match = RegexNumber1.Match(input, match.Index);
                }
            }

            // change '' to "
            input = input.Replace("''", "\"");

            // change 'sequeI of' to 'sequel of'
            if (input.Contains('I'))
            {
                var match = RegexUppercaseI.Match(input);
                while (match.Success)
                {
                    bool doFix = !(match.Index >= 1 && input.Substring(match.Index - 1).StartsWith("Mc", StringComparison.Ordinal));
                    if (match.Index >= 2 && input.Substring(match.Index - 2).StartsWith("Mac", StringComparison.Ordinal))
                        doFix = false;

                    if (doFix)
                        input = input.Substring(0, match.Index + 1) + "l" + input.Substring(match.Index + 2);

                    if (match.Index + 1 < input.Length)
                        match = RegexUppercaseI.Match(input, match.Index + 1);
                    else
                        break; // end while
                }
            }

            // change 'NlCE' to 'NICE'
            if (input.Contains('l'))
            {
                var match = RegexLowercaseL.Match(input);
                while (match.Success)
                {
                    input = input.Substring(0, match.Index + 1) + "I" + input.Substring(match.Index + 2);
                    match = RegexLowercaseL.Match(input);
                }
            }

            if (input.EndsWith(". \"</i>", StringComparison.Ordinal))
                input = input.Remove(input.Length - 6, 1);

            if (input.Contains(". \"</i>" + Environment.NewLine, StringComparison.Ordinal))
            {
                idx = input.IndexOf(". \"</i>" + Environment.NewLine, StringComparison.Ordinal);
                if (idx > 0)
                {
                    input = input.Remove(idx + 1, 1);
                }
            }

            return input;
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
                    return res.Line;
                wordsNotFound++;
                return line;
            }

            if (_hunspell == null)
                return line;

            string tempLine = line;
            //foreach (string name in _nameList)
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

            const string p = " ¡¿,.!?:;()[]{}+-$£\"„”“#&%…—♪\r\n";
            foreach (string name in _nameMultiWordList)
            {
                int start = tempLine.FastIndexOf(name);
                if (start == 0 || (start > 0 && p.Contains(tempLine[start - 1])))
                {
                    int end = start + name.Length;
                    if (end == tempLine.Length || p.Contains(tempLine[end]))
                        tempLine = tempLine.Remove(start, name.Length);
                }
            }

            string[] words = tempLine.Replace("</i>", string.Empty).Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i].TrimStart('\'');
                string wordNotEndTrimmed = word;
                word = word.TrimEnd('\'');
                string wordNoItalics = HtmlUtil.RemoveOpenCloseTags(word, HtmlUtil.TagItalic);
                if (!IsWordKnownOrNumber(wordNoItalics, line) && !localIgnoreWords.Contains(wordNoItalics))
                {
                    bool correct = DoSpell(word);
                    if (!correct)
                        correct = DoSpell(word.Trim('\''));
                    if (!correct && word.Length > 3 && !word.EndsWith("ss", StringComparison.Ordinal) && !string.IsNullOrEmpty(_threeLetterIsoLanguageName) &&
                        (_threeLetterIsoLanguageName == "eng" || _threeLetterIsoLanguageName == "dan" || _threeLetterIsoLanguageName == "swe" || _threeLetterIsoLanguageName == "nld"))
                        correct = DoSpell(word.TrimEnd('s'));
                    if (!correct)
                        correct = DoSpell(wordNoItalics);
                    if (!correct && _userWordList.Contains(wordNoItalics))
                        correct = true;

                    if (!correct && !line.Contains(word))
                        correct = true; // already fixed

                    if (!correct && Configuration.Settings.Tools.SpellCheckEnglishAllowInQuoteAsIng && wordNotEndTrimmed.EndsWith('\'') &&
                        SpellCheckDictionaryName.StartsWith("en_", StringComparison.Ordinal) && word.EndsWith("in", StringComparison.OrdinalIgnoreCase))
                    {
                        correct = DoSpell(word + "g");
                    }

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

                    if (!correct && word.Contains('/') && !word.Contains("//"))
                    {
                        var slashedWords = word.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        bool allSlashedCorrect = true;
                        foreach (var slashedWord in slashedWords)
                        {
                            if (slashedWord.Length < 2)
                                allSlashedCorrect = false;
                            if (allSlashedCorrect && !(DoSpell(slashedWord) || IsWordKnownOrNumber(slashedWord, line)))
                                allSlashedCorrect = false;
                        }
                        correct = allSlashedCorrect;
                    }

                    if (word.Length == 0)
                        correct = true;

                    if (!correct)
                    {
                        wordsNotFound++;
                        if (log)
                        {
                            string nf = word;
                            if (nf.StartsWith("<i>", StringComparison.Ordinal))
                                nf = nf.Remove(0, 3);
                            if (nf.Trim().Length > 0)
                                UnknownWordsFound.Add($"#{index + 1}: {nf}");
                        }

                        if (autoFix && autoGuess != AutoGuessLevel.None)
                        {
                            var guesses = new List<string>();

                            // Name starting with "l" instead of 'I'
                            if (word.StartsWith('l') && word.Length > 3 && !_nameList.Contains(word))
                            {
                                var w = "I" + word.Substring(1);
                                if (_nameList.Contains(w))
                                    guesses.Add(w);
                            }

                            if (word.Length > 5 && autoGuess == AutoGuessLevel.Aggressive)
                            {
                                guesses = (List<string>)_ocrFixReplaceList.CreateGuessesFromLetters(word);

                                if (word[0] == 'L')
                                    guesses.Add("I" + word.Substring(1));

                                if (word.Contains('$'))
                                    guesses.Add(word.Replace("$", "s"));

                                string wordWithCasingChanged = GetWordWithDominatedCasing(word);
                                if (DoSpell(word.ToLower()))
                                    guesses.Insert(0, wordWithCasingChanged);
                            }
                            else if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
                            {
                                if (word[0] == 'L')
                                    guesses.Add("I" + word.Substring(1));

                                if (word.Length > 2 && word[0] == 'I' && char.IsLower(word[1]))
                                    guesses.Add("l" + word.Substring(1));

                                if (i == 0)
                                    guesses.Add(word.Replace(@"\/", "V"));
                                else
                                    guesses.Add(word.Replace(@"\/", "v"));
                                guesses.Add(word.Replace("ﬁ", "fi"));
                                guesses.Add(word.Replace("ﬁ", "fj"));
                                guesses.Add(word.Replace("ﬂ", "fl"));
                                if (word.Contains('$'))
                                    guesses.Add(word.Replace("$", "s"));
                                if (!word.EndsWith('€') && !word.StartsWith('€'))
                                    guesses.Add(word.Replace("€", "e"));
                                guesses.Add(word.Replace("/", "l"));
                                guesses.Add(word.Replace(")/", "y"));
                            }
                            foreach (string guess in guesses)
                            {
                                if (!guess.StartsWith("f ", StringComparison.Ordinal) && IsWordOrWordsCorrect(guess))
                                {
                                    string replacedLine = OcrFixReplaceList.ReplaceWord(line, word, guess);
                                    if (replacedLine != line)
                                    {
                                        if (log)
                                            AutoGuessesUsed.Add($"#{index + 1}: {word} -> {guess} in line via '{"OCRFixReplaceList.xml"}': {line.Replace(Environment.NewLine, " ")}");

                                        line = replacedLine;
                                        wordsNotFound--;
                                        if (log && UnknownWordsFound.Count > 0)
                                            UnknownWordsFound.RemoveAt(UnknownWordsFound.Count - 1);
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
                                if (word.ToUpper() != "LT'S" && word.ToUpper() != "SOX'S") // TODO: Get fixed nhunspell
                                    suggestions = DoSuggest(word); // 0.9.6 fails on "Lt'S"
                            }

                            if (word.StartsWith("<i>", StringComparison.Ordinal))
                                word = word.Remove(0, 3);

                            if (word.EndsWith("</i>", StringComparison.Ordinal))
                                word = word.Remove(word.Length - 4, 4);

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

        private static string GetDashedWordBefore(string word, string line, string[] words, int index)
        {
            if (index > 0 && line.Contains(words[index - 1] + "-" + word))
                return HtmlUtil.RemoveOpenCloseTags(words[index - 1] + "-" + word, HtmlUtil.TagItalic);

            return null;
        }

        private static string GetDashedWordAfter(string word, string line, string[] words, int index)
        {
            if (index < words.Length - 1 && line.Contains(word + "-" + words[index + 1].Replace("</i>", string.Empty)))
                return HtmlUtil.RemoveOpenCloseTags(word + "-" + words[index + 1], HtmlUtil.TagItalic);

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
                    lowercase++;
                else if (char.IsUpper(ch))
                    uppercase++;
            }
            if (uppercase > lowercase)
                return word.ToUpper();
            return word.ToLower();
        }

        /// <summary>
        /// SpellCheck for OCR
        /// </summary>
        /// <returns>True, if word is fixed</returns>
        private SpellCheckOcrTextResult SpellCheckOcrText(string line, Bitmap bitmap, string word, List<string> suggestions)
        {
            var result = new SpellCheckOcrTextResult { Fixed = false, FixedWholeLine = false, Line = null, Word = null };
            _spellCheck.Initialize(word, suggestions, line, bitmap);
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
                        Utilities.AddToUserDictionary(_spellCheck.Word.Trim().ToLower(), _fiveLetterWordListLanguageName);
                        _userWordList.Add(_spellCheck.Word.Trim().ToLower());
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
                        _nameListObj?.Add(s);
                        if (s.Contains(' '))
                            _nameMultiWordList.Add(s);
                        else
                        {
                            _nameList.Add(s);
                            _nameListUppercase.Add(s.ToUpper());
                            if (_fiveLetterWordListLanguageName.StartsWith("en", StringComparison.Ordinal))
                            {
                                if (!s.EndsWith('s'))
                                    _nameListWithApostrophe.Add(s + "'s");
                                else
                                    _nameListWithApostrophe.Add(s + "'");
                            }
                        }
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
                    _wordSkipList.Add(_spellCheck.Word.ToUpper());
                    if (_spellCheck.Word.Length > 1)
                        _wordSkipList.Add(char.ToUpper(_spellCheck.Word[0]) + _spellCheck.Word.Substring(1));
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
                        return true;

                    if (s.Length > 10 && s.Contains('/'))
                    {
                        string[] ar = s.Split('/');
                        if (ar.Length == 2)
                        {
                            if (ar[0].Length > 3 && ar[1].Length > 3)
                            {
                                string a = ar[0];
                                if (a == a.ToUpper())
                                    a = a[0] + a.Substring(1).ToLower();
                                string b = ar[0];
                                if (b == b.ToUpper())
                                    b = b[0] + b.Substring(1).ToLower();

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

        public bool IsWordKnownOrNumber(string word, string line)
        {
            if (double.TryParse(word.TrimStart('\'').Replace("$", string.Empty).Replace("£", string.Empty).Replace("¢", string.Empty), out _))
                return true;

            if (_wordSkipList.Contains(word))
                return true;

            if (_nameList.Contains(word.Trim('\'')))
                return true;

            if (_nameListUppercase.Contains(word.Trim('\'')))
                return true;

            if (_userWordList.Contains(word.ToLower()))
                return true;

            if (_userWordList.Contains(word.Trim('\'').ToLower()))
                return true;

            if (word.Length > 2 && _nameListUppercase.Contains(word))
                return true;

            if (word.Length > 2 && _nameListWithApostrophe.Contains(word))
                return true;

            if (_nameListObj != null && _nameListObj.IsInNamesMultiWordList(line, word))
                return true;

            return false;
        }

        public int CountUnknownWordsViaDictionary(string line, out int numberOfCorrectWords)
        {
            numberOfCorrectWords = 0;
            if (_hunspell == null)
                return 0;

            int wordsNotFound = 0;
            var words = HtmlUtil.RemoveOpenCloseTags(line, HtmlUtil.TagItalic).Split((Environment.NewLine + " ¡¿,.!?:;()[]{}+-$£\"#&%…„“”«»").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (word.Length > 1)
                {
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
                    else if (word.Length > 3)
                        numberOfCorrectWords++;
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
