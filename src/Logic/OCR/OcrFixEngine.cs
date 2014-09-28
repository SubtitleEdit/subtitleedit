﻿using Nikse.SubtitleEdit.Forms;
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

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class OcrFixEngine
    {
        public enum AutoGuessLevel
        {
            None,
            Cautious,
            Aggressive
        }

        // Dictionaries/spell checking/fixing
        private Dictionary<string, string> _wordReplaceList;
        private Dictionary<string, string> _partialLineWordBoundaryReplaceList;
        private Dictionary<string, string> _partialLineAlwaysReplaceList;
        private Dictionary<string, string> _beginLineReplaceList;
        private Dictionary<string, string> _endLineReplaceList;
        private Dictionary<string, string> _wholeLineReplaceList;
        private Dictionary<string, string> _partialWordReplaceListAlways;
        private Dictionary<string, string> _partialWordReplaceList;
        private Dictionary<string, string> _regExList;

        private string _replaceListXmlFileName;
        private string _userWordListXmlFileName;
        private string _fiveLetterWordListLanguageName;

        private HashSet<string> _namesEtcList = new HashSet<string>();
        private HashSet<string> _namesEtcListUppercase = new HashSet<string>();
        private HashSet<string> _namesEtcListWithApostrophe = new HashSet<string>();
        private HashSet<string> _namesEtcMultiWordList = new HashSet<string>(); // case sensitive phrases
        private HashSet<string> _abbreviationList;
        private HashSet<string> _userWordList = new HashSet<string>();
        private HashSet<string> _wordSkipList = new HashSet<string>();
        private Hunspell _hunspell;
        private readonly OcrSpellCheck _spellCheck;
        private readonly Form _parentForm;
        private string _spellCheckDictionaryName;
        private string _threeLetterIsoLanguageName;

        private static readonly Regex RegexAloneI = new Regex(@"\bi\b", RegexOptions.Compiled);
        private static readonly Regex RegexAloneIasL = new Regex(@"\bl\b", RegexOptions.Compiled);
        private static readonly Regex RegexSpaceBetweenNumbers = new Regex(@"\d \d", RegexOptions.Compiled);
        private static readonly Regex RegExLowercaseL = new Regex("[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏ]l[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏ]", RegexOptions.Compiled);
        private static readonly Regex RegExUppercaseI = new Regex("[a-zæøåöääöéèàùâêîôûëï]I.", RegexOptions.Compiled);
        private static readonly Regex RegExNumber1 = new Regex(@"\d\ 1", RegexOptions.Compiled);
        private static readonly Regex RegExQuestion = new Regex(@"\S\?[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏa-zæøåäöéèàùâêîôûëï]", RegexOptions.Compiled);
        private static readonly Regex RegExIandZero = new Regex(@"[a-zæøåöääöéèàùâêîôûëï][I1]", RegexOptions.Compiled);
        private static readonly Regex RegExTime1 = new Regex(@"[a-zæøåöääöéèàùâêîôûëï][0]", RegexOptions.Compiled);
        private static readonly Regex RegExTime2 = new Regex(@"0[a-zæøåöääöéèàùâêîôûëï]", RegexOptions.Compiled);
        private static readonly Regex HexNumber = new Regex(@"^#?[\dABDEFabcdef]+$", RegexOptions.Compiled);
        private static readonly Regex StartEndEndsWithNumber = new Regex(@"^\d+.+\d$", RegexOptions.Compiled);

        public bool Abort { get; set; }
        public List<string> AutoGuessesUsed { get; set; }
        public List<string> UnknownWordsFound { get; set; }
        public bool IsDictionaryLoaded { get; private set; }

        public CultureInfo DictionaryCulture { get; private set; }

        /// <summary>
        /// Advanced OCR fixing via replace/spelling dictionaries + some hardcoded rules
        /// </summary>
        /// <param name="threeLetterIsoLanguageName">E.g. eng for English</param>
        /// <param name="parentForm">Used for centering/show spell check dialog</param>
        public OcrFixEngine(string threeLetterIsoLanguageName, string hunspellName, Form parentForm)
        {
            if (threeLetterIsoLanguageName == "per")
                threeLetterIsoLanguageName = "fas";

            _threeLetterIsoLanguageName = threeLetterIsoLanguageName;
            _parentForm = parentForm;

            _spellCheck = new OcrSpellCheck { StartPosition = FormStartPosition.Manual };
            _spellCheck.Location = new Point(parentForm.Left + (parentForm.Width / 2 - _spellCheck.Width / 2),
                                             parentForm.Top + (parentForm.Height / 2 - _spellCheck.Height / 2));

            LoadReplaceLists(threeLetterIsoLanguageName);
            LoadSpellingDictionaries(threeLetterIsoLanguageName, hunspellName); // Hunspell etc.

            AutoGuessesUsed = new List<string>();
            UnknownWordsFound = new List<string>();
        }

        private void LoadReplaceLists(string languageId)
        {
            _wordReplaceList = new Dictionary<string, string>();
            _partialLineWordBoundaryReplaceList = new Dictionary<string, string>();
            _partialLineAlwaysReplaceList = new Dictionary<string, string>();
            _beginLineReplaceList = new Dictionary<string, string>();
            _endLineReplaceList = new Dictionary<string, string>();
            _wholeLineReplaceList = new Dictionary<string, string>();
            _partialWordReplaceListAlways = new Dictionary<string, string>();
            _partialWordReplaceList = new Dictionary<string, string>();
            _regExList = new Dictionary<string, string>();

            _replaceListXmlFileName = Utilities.DictionaryFolder + languageId + "_OCRFixReplaceList.xml";
            if (File.Exists(_replaceListXmlFileName))
            {
                var doc = new XmlDocument();
                try
                {
                    doc.Load(_replaceListXmlFileName);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Unable to load " + _replaceListXmlFileName + ": " + exception.Message + Environment.NewLine);
                }

                _wordReplaceList = LoadReplaceList(doc, "WholeWords");
                _partialWordReplaceListAlways = LoadReplaceList(doc, "PartialWordsAlways");
                _partialWordReplaceList = LoadReplaceList(doc, "PartialWords");
                _partialLineWordBoundaryReplaceList = LoadReplaceList(doc, "PartialLines");
                _partialLineAlwaysReplaceList = LoadReplaceList(doc, "PartialAlwaysLines");
                _beginLineReplaceList = LoadReplaceList(doc, "BeginLines");
                _endLineReplaceList = LoadReplaceList(doc, "EndLines");
                _wholeLineReplaceList = LoadReplaceList(doc, "WholeLines");
                _regExList = LoadRegExList(doc, "RegularExpressions");
            }
        }

        private void LoadSpellingDictionaries(string threeLetterIsoLanguageName, string hunspellName)
        {
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
                return;

            if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_gb", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_GB.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", new CultureInfo("en-GB"), "en_GB.dic", true);
                return;
            }
            else if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_ca", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_CA.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", new CultureInfo("en-CA"), "en_CA.dic", true);
                return;
            }
            else if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_au", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_AU.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", new CultureInfo("en-AU"), "en_AU.dic", true);
                return;
            }
            else if (!string.IsNullOrEmpty(hunspellName) && threeLetterIsoLanguageName == "eng" && hunspellName.Equals("en_za", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dictionaryFolder, "en_ZA.dic")))
            {
                LoadSpellingDictionariesViaDictionaryFileName("eng", new CultureInfo("en-ZA"), "en_ZA.dic", true);
                return;
            }
            else if (threeLetterIsoLanguageName == "eng" && File.Exists(Path.Combine(dictionaryFolder, "en_US.dic")))
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
                        if (!name.StartsWith("hyph"))
                        {
                            try
                            {
                                name = name.Replace("_", "-");
                                if (name.Length > 5)
                                    name = name.Substring(0, 5);
                                var ci = new CultureInfo(name);
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
            if (_fiveLetterWordListLanguageName.Length > 5)
                _fiveLetterWordListLanguageName = _fiveLetterWordListLanguageName.Substring(0, 5);
            string dictionary = Utilities.DictionaryFolder + _fiveLetterWordListLanguageName;
            if (resetSkipList)
            {
                _wordSkipList = new HashSet<string> { Configuration.Settings.Tools.MusicSymbol, "*", "%", "#", "+", "$" };
            }

            // Load names etc list (names/noise words)
            _namesEtcList = new HashSet<string>();
            _namesEtcMultiWordList = new HashSet<string>();
            Utilities.LoadNamesEtcWordLists(_namesEtcList, _namesEtcMultiWordList, _fiveLetterWordListLanguageName);

            _namesEtcListUppercase = new HashSet<string>();
            foreach (string name in _namesEtcList)
                _namesEtcListUppercase.Add(name.ToUpper());

            _namesEtcListWithApostrophe = new HashSet<string>();
            if (threeLetterIsoLanguageName.Equals("eng", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string namesItem in _namesEtcList)
                {
                    if (!namesItem.EndsWith('s'))
                        _namesEtcListWithApostrophe.Add(namesItem + "'s");
                    else
                        _namesEtcListWithApostrophe.Add(namesItem + "'");
                }
            }

            // Load user words
            _userWordList = new HashSet<string>();
            _userWordListXmlFileName = Utilities.LoadUserWordList(_userWordList, _fiveLetterWordListLanguageName);

            // Find abbreviations
            _abbreviationList = new HashSet<string>();
            foreach (string name in _namesEtcList)
            {
                if (name.EndsWith('.'))
                    _abbreviationList.Add(name);
            }
            if (threeLetterIsoLanguageName.Equals("eng", StringComparison.OrdinalIgnoreCase))
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

                string[] parts = _spellCheckDictionaryName.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
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
                        ci = new CultureInfo("sr-Latn-RS");
                    else
                        ci = new CultureInfo(value);
                }
                catch
                {
                    ci = CultureInfo.CurrentCulture;
                }
                LoadSpellingDictionariesViaDictionaryFileName(ci.ThreeLetterISOLanguageName, ci, spellCheckDictionaryName, false);
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

        internal static Dictionary<string, string> LoadRegExList(XmlDocument doc, string name)
        {
            var list = new Dictionary<string, string>();
            if (doc.DocumentElement != null)
            {
                XmlNode node = doc.DocumentElement.SelectSingleNode(name);
                if (node != null)
                {
                    foreach (XmlNode item in node.ChildNodes)
                    {
                        if (item.Attributes != null && item.Attributes["replaceWith"] != null && item.Attributes["find"] != null)
                        {
                            string to = item.Attributes["replaceWith"].InnerText;
                            string from = item.Attributes["find"].InnerText;
                            if (!list.ContainsKey(from))
                                list.Add(from, to);
                        }
                    }
                }
            }
            return list;
        }

        public string FixOcrErrors(string text, int index, string lastLine, bool logSuggestions, AutoGuessLevel autoGuess)
        {
            var sb = new StringBuilder();
            var word = new StringBuilder();

            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                text = text.Replace("ﬁ", "fi"); // fb01
                text = text.Replace("ﬂ", "fl"); // fb02
                text = text.Replace("ν", "v"); // NOTE: first 'v' is a special unicode character!!!!
            }

            text = ReplaceWordsBeforeLineFixes(text);

            text = FixCommenOcrLineErrors(text, lastLine);

            string lastWord = null;
            for (int i = 0; i < text.Length; i++)
            {
                if (" ¡¿,.!?:;()[]{}+-£\"#&%\r\n".Contains(text[i])) // removed $
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
                            bool doFixWord = true;
                            if (word.Length == 1 && sb.Length > 1 && sb.EndsWith('-'))
                                doFixWord = false;
                            if (doFixWord)
                                fixedWord = FixCommonWordErrors(word.ToString());
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
                if (word.Length == 1 && sb.Length > 1 && sb.EndsWith('-'))
                    doFixWord = false;
                if (doFixWord)
                    fixedWord = FixCommonWordErrors(word.ToString());
                else
                    fixedWord = word.ToString();

                sb.Append(fixedWord);
            }

            text = FixCommenOcrLineErrors(sb.ToString(), lastLine);
            int wordsNotFound;
            text = FixUnknownWordsViaGuessOrPrompt(out wordsNotFound, text, index, null, true, false, logSuggestions, autoGuess);
            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                text = FixLowercaseIToUppercaseI(text, lastLine);
                if (SpellCheckDictionaryName.StartsWith("en_", StringComparison.Ordinal) || _threeLetterIsoLanguageName == "eng")
                {
                    string oldText = text;
                    text = FixCommonErrors.FixAloneLowercaseIToUppercaseLine(RegexAloneI, oldText, text, 'i');
                    text = FixCommonErrors.FixAloneLowercaseIToUppercaseLine(RegexAloneIasL, oldText, text, 'l');
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

                text = RemoveSpaceBetweenNumbers(text);
            }
            return text;
        }

        private static string FixFrenchLApostrophe(string text, string tag, string lastLine)
        {
            bool endingBeforeThis = string.IsNullOrEmpty(lastLine) || lastLine.EndsWith('.') || lastLine.EndsWith('!') || lastLine.EndsWith('?') ||
                                    lastLine.EndsWith(".</i>") || lastLine.EndsWith("!</i>") || lastLine.EndsWith("?</i>") ||
                                    lastLine.EndsWith(".</font>") || lastLine.EndsWith("!</font>") || lastLine.EndsWith("?</font>");
            if (text.StartsWith(tag.TrimStart(), StringComparison.Ordinal) && text.Length > 3)
            {
                if (endingBeforeThis || Utilities.UppercaseLetters.Contains(text[2]))
                {
                    text = @"L" + text.Substring(1);
                }
                else if (Utilities.LowercaseLetters.Contains(text[2]))
                {
                    text = @"l" + text.Substring(1);
                }
            }
            else if (text.StartsWith("<i>" + tag.TrimStart(), StringComparison.Ordinal) && text.Length > 6)
            {
                if (endingBeforeThis || Utilities.UppercaseLetters.Contains(text[5]))
                {
                    text = text.Remove(3, 1).Insert(3, "L");
                }
                else if (Utilities.LowercaseLetters.Contains(text[5]))
                {
                    text = text.Remove(3, 1).Insert(3, "l");
                }
            }

            int start = text.IndexOf(tag, StringComparison.Ordinal);
            while (start > 0)
            {
                lastLine = Utilities.RemoveHtmlTags(text.Substring(0, start)).TrimEnd().TrimEnd('-').TrimEnd();
                endingBeforeThis = string.IsNullOrEmpty(lastLine) || lastLine.EndsWith('.') || lastLine.EndsWith('!') || lastLine.EndsWith('?');
                if (start < text.Length - 4)
                {
                    if (start == 1 && text.StartsWith('-'))
                        endingBeforeThis = true;

                    if (endingBeforeThis || Utilities.UppercaseLetters.Contains(text[start + 3]))
                    {
                        text = text.Remove(start + 1, 1).Insert(start + 1, "L");
                    }
                    else if (Utilities.LowercaseLetters.Contains(text[start + 3]))
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
                lastLine = Utilities.RemoveHtmlTags(text.Substring(0, start)).TrimEnd().TrimEnd('-').TrimEnd();
                endingBeforeThis = string.IsNullOrEmpty(lastLine) || lastLine.EndsWith('.') || lastLine.EndsWith('!') || lastLine.EndsWith('?') || lastLine.EndsWith(".</i>");
                if (start < text.Length - 5)
                {
                    if (endingBeforeThis || Utilities.UppercaseLetters.Contains(text[start + 2 + Environment.NewLine.Length]))
                    {
                        text = text.Remove(start + Environment.NewLine.Length, 1).Insert(start + Environment.NewLine.Length, "L");
                    }
                    else if (Utilities.LowercaseLetters.Contains(text[start + 2 + Environment.NewLine.Length]))
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
                lastLine = Utilities.RemoveHtmlTags(text.Substring(0, start)).TrimEnd().TrimEnd('-').TrimEnd();
                endingBeforeThis = string.IsNullOrEmpty(lastLine) || lastLine.EndsWith('.') || lastLine.EndsWith('!') || lastLine.EndsWith('?') || lastLine.EndsWith(".</i>");
                if (start < text.Length - 8)
                {
                    if (endingBeforeThis || Utilities.UppercaseLetters.Contains(text[start + 5 + Environment.NewLine.Length]))
                    {
                        text = text.Remove(start + Environment.NewLine.Length + 3, 1).Insert(start + Environment.NewLine.Length + 3, "L");
                    }
                    else if (Utilities.LowercaseLetters.Contains(text[start + 5 + Environment.NewLine.Length]))
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
                if (" ¡¿,.!?:;()[]{}+-£\"#&%\r\n".Contains(text[i])) // removed $
                {
                    if (word.Length > 0)
                    {
                        string fixedWord;
                        if (lastWord != null && lastWord.Contains("COLOR=", StringComparison.OrdinalIgnoreCase))
                            fixedWord = word.ToString();
                        else
                            fixedWord = FixCommonWordErrorsQuick(word.ToString());
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
                string fixedWord = FixCommonWordErrorsQuick(word.ToString());
                sb.Append(fixedWord);
            }
            return sb.ToString();
        }

        private static string RemoveSpaceBetweenNumbers(string text)
        {
            Match match = RegexSpaceBetweenNumbers.Match(text);
            while (match.Success)
            {
                bool doFix = true;

                if (match.Index + 4 < text.Length && text[match.Index + 3] == '/' && char.IsDigit(text[match.Index + 4]))
                    doFix = false;

                if (doFix)
                {
                    text = text.Remove(match.Index + 1, 1);
                    match = RegexSpaceBetweenNumbers.Match(text);
                }
                else
                {
                    match = RegexSpaceBetweenNumbers.Match(text, match.Index + 1);
                }
            }
            return text;
        }

        private string FixCommonWordErrors(string word)
        {
            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                word = word.Replace("ﬁ", "fi");
                word = word.Replace("ν", "v"); // NOTE: first 'v' is a special unicode character!!!!

                while (word.Contains("--"))
                    word = word.Replace("--", "-");

                if (word.Contains('’'))
                    word = word.Replace('’', '\'');

                if (word.Contains('`'))
                    word = word.Replace('`', '\'');

                if (word.Contains('‘'))
                    word = word.Replace('‘', '\'');

                if (word.Contains('—'))
                    word = word.Replace('—', '-');

                if (word.Contains('|'))
                    word = word.Replace("|", "l");

                if (word.Contains("vx/"))
                    word = word.Replace("vx/", "w");

                if (word.Contains('¤'))
                {
                    var regex = new Regex("[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏa-zæøåäöéèàùâêîôûëï]¤");
                    if (regex.IsMatch(word))
                        word = word.Replace("¤", "o");
                }
            }

            //always replace list
            foreach (string letter in _partialWordReplaceListAlways.Keys)
                word = word.Replace(letter, _partialWordReplaceListAlways[letter]);

            string pre = string.Empty;
            string post = string.Empty;

            if (word.StartsWith("<i>", StringComparison.Ordinal))
            {
                pre += "<i>";
                word = word.Remove(0, 3);
            }
            while (word.Length > 2 && word.StartsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                pre += Environment.NewLine;
                word = word.Substring(2);
            }

            while (word.Length > 1 && word[0] == '-')
            {
                pre += "-";
                word = word.Substring(1);
            }
            while (word.Length > 1 && word[0] == '.')
            {
                pre += ".";
                word = word.Substring(1);
            }
            while (word.Length > 1 && word[0] == '"')
            {
                pre += "\"";
                word = word.Substring(1);
            }
            if (word.Length > 1 && word[0] == '(')
            {
                pre += "(";
                word = word.Substring(1);
            }
            if (word.StartsWith("<i>", StringComparison.Ordinal))
            {
                pre += "<i>";
                word = word.Remove(0, 3);
            }
            while (word.Length > 2 && word.EndsWith(Environment.NewLine))
            {
                post += Environment.NewLine;
                word = word.Substring(0, word.Length - 2);
            }
            while (word.Length > 1 && word.EndsWith('"'))
            {
                post = post + "\"";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.Length > 1 && word.EndsWith('.'))
            {
                post = post + ".";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith(',') && word.Length > 1)
            {
                post = post + ",";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith('?') && word.Length > 1)
            {
                post = post + "?";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith('!') && word.Length > 1)
            {
                post = post + "!";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith(')') && word.Length > 1)
            {
                post = post + ")";
                word = word.Substring(0, word.Length - 1);
            }
            if (word.EndsWith("</i>", StringComparison.Ordinal))
            {
                post = post + "</i>";
                word = word.Remove(word.Length - 4, 4);
            }
            string preWordPost = pre + word + post;
            if (word.Length == 0)
                return preWordPost;

            if (word.Contains('?'))
            {
                Match match = RegExQuestion.Match(word);
                if (match.Success)
                    word = word.Insert(match.Index + 2, " ");
            }

            foreach (string from in _wordReplaceList.Keys)
            {
                if (word.Length == from.Length)
                {
                    if (word == from)
                        return pre + _wordReplaceList[from] + post;
                }
                else if (word.Length + post.Length == from.Length)
                {
                    if (string.CompareOrdinal(word + post, from) == 0)
                        return pre + _wordReplaceList[from];
                }
                if (pre.Length + word.Length + post.Length == from.Length && string.CompareOrdinal(preWordPost, from) == 0)
                {
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
                if (word.Length == from.Length)
                {
                    if (string.CompareOrdinal(word, from) == 0)
                        return pre + _wordReplaceList[from] + post;
                }
                else if (word.Length + post.Length == from.Length)
                {
                    if (string.CompareOrdinal(word + post, from) == 0)
                        return pre + _wordReplaceList[from];
                }
                if (pre.Length + word.Length + post.Length == from.Length && string.CompareOrdinal(preWordPost, from) == 0)
                {
                    return _wordReplaceList[from];
                }
            }

            return preWordPost;
        }

        private string FixCommonWordErrorsQuick(string word)
        {
            //always replace list
            foreach (string letter in _partialWordReplaceListAlways.Keys)
                word = word.Replace(letter, _partialWordReplaceListAlways[letter]);

            string pre = string.Empty;
            string post = string.Empty;

            if (word.StartsWith("<i>", StringComparison.Ordinal))
            {
                pre += "<i>";
                word = word.Remove(0, 3);
            }
            while (word.StartsWith(Environment.NewLine) && word.Length > 2)
            {
                pre += Environment.NewLine;
                word = word.Substring(2);
            }

            while (word.Length > 1 && word[0] == '-')
            {
                pre += "-";
                word = word.Substring(1);
            }
            while (word.Length > 1 && word[0] == '.')
            {
                pre += ".";
                word = word.Substring(1);
            }
            while (word.Length > 1 && word[0] == '"')
            {
                pre += "\"";
                word = word.Substring(1);
            }
            if (word.Length > 1 && word[0] == '(')
            {
                pre += "(";
                word = word.Substring(1);
            }
            if (word.StartsWith("<i>", StringComparison.Ordinal))
            {
                pre += "<i>";
                word = word.Remove(0, 3);
            }
            while (word.EndsWith(Environment.NewLine) && word.Length > 2)
            {
                post += Environment.NewLine;
                word = word.Substring(0, word.Length - 2);
            }
            while (word.EndsWith('"') && word.Length > 1)
            {
                post = post + "\"";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith('.') && word.Length > 1)
            {
                post = post + ".";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith(',') && word.Length > 1)
            {
                post = post + ",";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith('?') && word.Length > 1)
            {
                post = post + "?";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith('!') && word.Length > 1)
            {
                post = post + "!";
                word = word.Substring(0, word.Length - 1);
            }
            while (word.EndsWith(')') && word.Length > 1)
            {
                post = post + ")";
                word = word.Substring(0, word.Length - 1);
            }
            if (word.EndsWith("</i>", StringComparison.Ordinal))
            {
                post = post + "</i>";
                word = word.Remove(word.Length - 4, 4);
            }

            string preWordPost = pre + word + post;
            if (word.Length == 0)
                return preWordPost;

            foreach (string from in _wordReplaceList.Keys)
            {
                if (word.Length == from.Length)
                {
                    if (string.CompareOrdinal(word, from) == 0)
                        return pre + _wordReplaceList[from] + post;
                }
                else if (word.Length + post.Length == from.Length)
                {
                    if (string.CompareOrdinal(word + post, from) == 0)
                        return pre + _wordReplaceList[from];
                }
                if (pre.Length + word.Length + post.Length == from.Length && string.CompareOrdinal(preWordPost, from) == 0)
                {
                    return _wordReplaceList[from];
                }
            }

            return preWordPost;
        }

        public static string Fix0InsideLowerCaseWord(string word)
        {
            if (StartEndEndsWithNumber.IsMatch(word))
                return word;

            if (word.Contains('1') ||
                word.Contains('2') ||
                word.Contains('3') ||
                word.Contains('4') ||
                word.Contains('5') ||
                word.Contains('6') ||
                word.Contains('7') ||
                word.Contains('8') ||
                word.Contains('9') ||
                word.EndsWith("a.m", StringComparison.Ordinal) ||
                word.EndsWith("p.m", StringComparison.Ordinal) ||
                word.EndsWith("am", StringComparison.Ordinal) ||
                word.EndsWith("pm", StringComparison.Ordinal))
                return word;

            if (HexNumber.IsMatch(word))
                return word;

            if (word.LastIndexOf('0') > 0)
            {
                Match match = RegExTime1.Match(word);
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
                        match = RegExTime1.Match(word);
                    }
                }

                match = RegExTime2.Match(word);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        if (word[match.Index] == '0')
                        {
                            if (match.Index == 0 || !@"123456789".Contains(word[match.Index - 1]))
                            {
                                string oldText = word;
                                word = word.Substring(0, match.Index) + "o";
                                if (match.Index + 1 < oldText.Length)
                                    word += oldText.Substring(match.Index + 1);
                            }
                        }
                        match = RegExTime2.Match(word, match.Index + 1);
                    }
                }
            }
            return word;
        }

        public static string FixIor1InsideLowerCaseWord(string word)
        {
            if (StartEndEndsWithNumber.IsMatch(word))
                return word;

            if (word.Contains('2') ||
                word.Contains('3') ||
                word.Contains('4') ||
                word.Contains('5') ||
                word.Contains('6') ||
                word.Contains('7') ||
                word.Contains('8') ||
                word.Contains('9'))
                return word;

            if (HexNumber.IsMatch(word))
                return word;

            if (word.LastIndexOf('I') > 0 || word.LastIndexOf('1') > 0)
            {
                Match match = RegExIandZero.Match(word);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        if (word[match.Index + 1] == 'I' || word[match.Index + 1] == '1')
                        {
                            bool doFix = true;
                            if (word[match.Index + 1] == 'I' && match.Index >= 1 && word.Substring(match.Index - 1).StartsWith("Mc"))
                                doFix = false;
                            if (word[match.Index + 1] == 'I' && match.Index >= 2 && word.Substring(match.Index - 2).StartsWith("Mac"))
                                doFix = false;

                            if (doFix)
                            {
                                string oldText = word;
                                word = word.Substring(0, match.Index + 1) + "l";
                                if (match.Index + 2 < oldText.Length)
                                    word += oldText.Substring(match.Index + 2);
                            }
                        }
                        match = RegExIandZero.Match(word, match.Index + 1);
                    }
                }
            }
            return word;
        }

        public static string FixLowerCaseLInsideUpperCaseWord(string word)
        {
            if (word.Length > 3 && word.Replace("l", string.Empty).ToUpper() == word.Replace("l", string.Empty))
            {
                if (!word.Contains('<') && !word.Contains('>') && !word.Contains('\''))
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
                if (input.StartsWith('~'))
                    input = ("- " + input.Remove(0, 1)).Replace("  ", " ");

                input = input.Replace(Environment.NewLine + "~", Environment.NewLine + "- ").Replace("  ", " ");

                if (input.Length < 10 && input.Length > 4 && !input.Contains(Environment.NewLine) && input.StartsWith("II") && input.EndsWith("II"))
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
            string[] lines = input.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];

                if (i > 0)
                    lastLine = lines[i - 1];
                lastLine = Utilities.RemoveHtmlTags(lastLine);

                if (string.IsNullOrEmpty(lastLine) ||
                    lastLine.EndsWith('.') ||
                    lastLine.EndsWith('!') ||
                    lastLine.EndsWith('?'))
                {
                    var st = new StripableText(l);
                    if (st.StrippedText.StartsWith('i') && !st.Pre.EndsWith('[') && !st.Pre.EndsWith('(') && !st.Pre.EndsWith("...", StringComparison.Ordinal))
                    {
                        if (string.IsNullOrEmpty(lastLine) || (!lastLine.EndsWith("...", StringComparison.Ordinal) && !EndsWithAbbreviation(lastLine, _abbreviationList)))
                            l = st.Pre + "I" + st.StrippedText.Remove(0, 1) + st.Post;
                    }
                }
                sb.AppendLine(l);
            }
            return sb.ToString().TrimEnd('\r').TrimEnd('\n').TrimEnd('\r').TrimEnd('\n');
        }

        private static bool EndsWithAbbreviation(string line, HashSet<string> abbreviationList)
        {
            if (string.IsNullOrEmpty(line) || abbreviationList == null)
                return false;

            line = line.ToLower();
            foreach (string abbreviation in abbreviationList)
            {
                if (line.EndsWith(" " + abbreviation.ToLower()))
                    return true;
            }

            if (line.Length > 5 && line[line.Length - 3] == '.' && Utilities.AllLetters.Contains(line[line.Length - 2]))
                return true;

            return false;
        }

        public string FixOcrErrorsViaHardcodedRules(string input, string lastLine, HashSet<string> abbreviationList)
        {
            if (!Configuration.Settings.Tools.OcrFixUseHardcodedRules)
                return input;

            input = input.Replace(",...", "...");

            if (input.StartsWith("..") && !input.StartsWith("..."))
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
                if (input.Length > 5 && input.StartsWith("..") && Utilities.AllLettersAndNumbers.Contains(input[2]))
                    input = "..." + input.Remove(0, 2);
                if (input.Length > 7 && input.StartsWith("<i>..") && Utilities.AllLettersAndNumbers.Contains(input[5]))
                    input = "<i>..." + input.Remove(0, 5);

                if (input.Length > 5 && input.StartsWith(".. ") && Utilities.AllLettersAndNumbers.Contains(input[3]))
                    input = "..." + input.Remove(0, 3);
                if (input.Length > 7 && input.StartsWith("<i>.. ") && Utilities.AllLettersAndNumbers.Contains(input[6]))
                    input = "<i>..." + input.Remove(0, 6);
                if (input.Contains(Environment.NewLine + ".. "))
                    input = input.Replace(Environment.NewLine + ".. ", Environment.NewLine + "...");
                if (input.Contains(Environment.NewLine + "<i>.. "))
                    input = input.Replace(Environment.NewLine + "<i>.. ", Environment.NewLine + "<i>...");

                if (input.StartsWith(". .."))
                    input = "..." + input.Remove(0, 4);
                if (input.StartsWith(".. ."))
                    input = "..." + input.Remove(0, 4);
                if (input.StartsWith(". . ."))
                    input = "..." + input.Remove(0, 5);
                if (input.StartsWith("... "))
                    input = input.Remove(3, 1);
            }

            input = pre + input;

            if (hasDotDot)
            {
                if (input.StartsWith("<i>. .."))
                    input = "<i>..." + input.Remove(0, 7);
                if (input.StartsWith("<i>.. ."))
                    input = "<i>..." + input.Remove(0, 7);

                if (input.StartsWith("<i>. . ."))
                    input = "<i>..." + input.Remove(0, 8);
                if (input.StartsWith("<i>... "))
                    input = input.Remove(6, 1);
                if (input.StartsWith(". . <i>."))
                    input = "<i>..." + input.Remove(0, 8);

                if (input.StartsWith("...<i>") && (input.IndexOf("</i>", StringComparison.Ordinal) > input.IndexOf(' ')))
                    input = "<i>..." + input.Remove(0, 6);

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

                if (input.EndsWith(".</i> . ."))
                    input = input.Remove(input.Length - 9, 9) + "...</i>";
                if (input.EndsWith(".</i>.."))
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

                if (input.StartsWith("- ...") && lastLine != null && lastLine.EndsWith("...") && !(input.Contains(Environment.NewLine + "-")))
                    input = input.Remove(0, 2);
                if (input.StartsWith("-...") && lastLine != null && lastLine.EndsWith("...") && !(input.Contains(Environment.NewLine + "-")))
                    input = input.Remove(0, 1);
            }

            if (input.Length > 2 && input[0] == '-' && Utilities.UppercaseLetters.Contains(input[1]))
            {
                input = input.Insert(1, " ");
            }

            if (input.Length > 5 && input.StartsWith("<i>-") && Utilities.UppercaseLetters.Contains(input[4]))
            {
                input = input.Insert(4, " ");
            }

            int idx = input.IndexOf(Environment.NewLine + "-", StringComparison.Ordinal);
            if (idx > 0 && idx + Environment.NewLine.Length + 1 < input.Length && Utilities.UppercaseLetters.Contains(input[idx + Environment.NewLine.Length + 1]))
            {
                input = input.Insert(idx + Environment.NewLine.Length + 1, " ");
            }

            idx = input.IndexOf(Environment.NewLine + "<i>-", StringComparison.Ordinal);
            if (idx > 0 && Utilities.UppercaseLetters.Contains(input[idx + Environment.NewLine.Length + 4]))
            {
                input = input.Insert(idx + Environment.NewLine.Length + 4, " ");
            }

            if (string.IsNullOrEmpty(lastLine) ||
                lastLine.EndsWith('.') ||
                lastLine.EndsWith('!') ||
                lastLine.EndsWith('?') ||
                lastLine.EndsWith(']') ||
                lastLine.EndsWith('♪'))
            {
                lastLine = Utilities.RemoveHtmlTags(lastLine);
                var st = new StripableText(input);
                if (lastLine == null || (!lastLine.EndsWith("...") && !EndsWithAbbreviation(lastLine, abbreviationList)))
                {
                    if (st.StrippedText.Length > 0 && !char.IsUpper(st.StrippedText[0]) && !st.Pre.EndsWith('[') && !st.Pre.EndsWith('(') && !st.Pre.EndsWith("..."))
                    {
                        var uppercaseLetter = char.ToUpper(st.StrippedText[0]);
                        if (st.StrippedText.Length > 1 && uppercaseLetter == 'L' && @"abcdfghjklmnpqrstvwxz".Contains(st.StrippedText[1]))
                            uppercaseLetter = 'I';
                        if ((st.StrippedText.StartsWith("lo ") || st.StrippedText == "lo.") && _threeLetterIsoLanguageName == "ita")
                            uppercaseLetter = 'I';
                        if ((st.StrippedText.StartsWith("k ") || st.StrippedText.StartsWith("m ") || st.StrippedText.StartsWith("n ") || st.StrippedText.StartsWith("r ") || st.StrippedText.StartsWith("s ") || st.StrippedText.StartsWith("t ")) &&
                            st.Pre.EndsWith('\'') && _threeLetterIsoLanguageName == "nld")
                            uppercaseLetter = st.StrippedText[0];
                        if ((st.StrippedText.StartsWith("l-I'll ") || st.StrippedText.StartsWith("l-l'll ")) && _threeLetterIsoLanguageName == "eng")
                        {
                            uppercaseLetter = 'I';
                            st.StrippedText = "I-I" + st.StrippedText.Remove(0, 3);
                        }
                        st.StrippedText = uppercaseLetter + st.StrippedText.Substring(1);
                        input = st.Pre + st.StrippedText + st.Post;
                    }
                }
            }

            // lines ending with ". should often end at ... (of no other quotes exists near by)
            if ((lastLine == null || !lastLine.Contains('"')) &&
                input.EndsWith("\".") && input.IndexOf('"') == input.LastIndexOf('"') && input.Length > 3)
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
                Match match = RegExNumber1.Match(input);
                while (match.Success)
                {
                    bool doFix = true;

                    if (match.Index + 4 < input.Length && input[match.Index + 3] == '/' && char.IsDigit(input[match.Index + 4]))
                        doFix = false;

                    if (doFix)
                    {
                        input = input.Substring(0, match.Index + 1) + input.Substring(match.Index + 2);
                        match = RegExNumber1.Match(input);
                    }
                    else
                    {
                        match = RegExNumber1.Match(input, match.Index + 1);
                    }
                }
            }

            // change '' to "
            input = input.Replace("''", "\"");

            // change 'sequeI of' to 'sequel of'
            if (input.Contains('I'))
            {
                var match = RegExUppercaseI.Match(input);
                while (match.Success)
                {
                    bool doFix = true;
                    if (match.Index >= 1 && input.Substring(match.Index - 1).StartsWith("Mc"))
                        doFix = false;
                    if (match.Index >= 2 && input.Substring(match.Index - 2).StartsWith("Mac"))
                        doFix = false;

                    if (doFix)
                        input = input.Substring(0, match.Index + 1) + "l" + input.Substring(match.Index + 2);

                    if (match.Index + 1 < input.Length)
                        match = RegExUppercaseI.Match(input, match.Index + 1);
                    else
                        break; // end while
                }
            }

            // change 'NlCE' to 'NICE'
            if (input.Contains('l'))
            {
                var match = RegExLowercaseL.Match(input);
                while (match.Success)
                {
                    input = input.Substring(0, match.Index + 1) + "I" + input.Substring(match.Index + 2);
                    match = RegExLowercaseL.Match(input);
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
            while (newText.Length > 1 && @" -""['¶(".Contains(newText[0]))
            {
                pre += newText[0];
                newText = newText.Substring(1);
            }
            if (newText.StartsWith("<i>"))
            {
                pre += "<i>";
                newText = newText.Remove(0, 3);
            }

            // begin line
            string[] lines = newText.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (string l in lines)
            {
                string s = l;
                foreach (string from in _beginLineReplaceList.Keys)
                {
                    if (s.Contains(from))
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
                        if (s.StartsWith('"') && !from.StartsWith('"') && s.StartsWith("\"" + from))
                            s = s.Replace("\"" + from, "\"" + _beginLineReplaceList[from]);
                    }
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

            foreach (string from in _partialLineWordBoundaryReplaceList.Keys)
            {
                if (newText.Contains(from))
                    newText = ReplaceWord(newText, from, _partialLineWordBoundaryReplaceList[from]);
            }

            foreach (string from in _partialLineAlwaysReplaceList.Keys)
            {
                if (newText.Contains(from))
                    newText = newText.Replace(from, _partialLineAlwaysReplaceList[from]);
            }

            foreach (string findWhat in _regExList.Keys)
            {
                newText = Regex.Replace(newText, findWhat, _regExList[findWhat], RegexOptions.Multiline);
            }

            return newText;
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
                int start = tempLine.IndexOf(name, StringComparison.Ordinal);
                if (start >= 0)
                {
                    if (start == 0 || (Environment.NewLine + @" ¡¿,.!?:;()[]{}+-$£""”“#&%…—♪").Contains(tempLine[start - 1]))
                    {
                        int end = start + name.Length;
                        if (end >= tempLine.Length || (Environment.NewLine + @" ¡¿,.!?:;()[]{}+-$£""”“#&%…—♪").Contains(tempLine[end]))
                            tempLine = tempLine.Remove(start, name.Length);
                    }
                }
            }

            string[] words = tempLine.Replace("</i>", string.Empty).Split((Environment.NewLine + " ¡¿,.!?:;()[]{}+-£\"”“#&%…—♪").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i].TrimStart('\'');
                string wordNotEndTrimmed = word;
                word = word.TrimEnd('\'');
                string wordNoItalics = HtmlUtils.RemoveOpenCloseTags(word, HtmlUtils.TagItalic);
                if (!IsWordKnownOrNumber(wordNoItalics, line) && !localIgnoreWords.Contains(wordNoItalics))
                {
                    bool correct = DoSpell(word);
                    if (!correct)
                        correct = DoSpell(word.Trim('\''));
                    if (!correct && word.Length > 3 && !word.EndsWith("ss") && !string.IsNullOrEmpty(_threeLetterIsoLanguageName) &&
                        (_threeLetterIsoLanguageName == "eng" || _threeLetterIsoLanguageName == "dan" || _threeLetterIsoLanguageName == "swe" || _threeLetterIsoLanguageName == "nld"))
                        correct = DoSpell(word.TrimEnd('s'));
                    if (!correct)
                        correct = DoSpell(wordNoItalics);
                    if (!correct && _userWordList.Contains(wordNoItalics))
                        correct = true;

                    if (!correct && !line.Contains(word))
                        correct = true; // already fixed

                    if (!correct && Configuration.Settings.Tools.SpellCheckEnglishAllowInQuoteAsIng && wordNotEndTrimmed.EndsWith('\'') &&
                        SpellCheckDictionaryName.StartsWith("en_") && word.EndsWith("in", StringComparison.OrdinalIgnoreCase))
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
                            if (nf.StartsWith("<i>"))
                                nf = nf.Remove(0, 3);
                            UnknownWordsFound.Add(string.Format("#{0}: {1}", index + 1, nf));
                        }

                        if (autoFix && autoGuess != AutoGuessLevel.None)
                        {
                            var guesses = new List<string>();
                            if (word.Length > 5 && autoGuess == AutoGuessLevel.Aggressive)
                            {
                                guesses = (List<string>)CreateGuessesFromLetters(word);

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
                                word = word.Remove(word.Length - 4, 4);

                            SpellCheckOcrTextResult res = SpellCheckOcrText(line, bitmap, word, suggestions);

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
                return HtmlUtils.RemoveOpenCloseTags(words[index - 1] + "-" + word, HtmlUtils.TagItalic);

            return null;
        }

        private static string GetDashedWordAfter(string word, string line, string[] words, int index)
        {
            if (index < words.Length - 1 && line.Contains(word + "-" + words[index + 1].Replace("</i>", string.Empty)))
                return HtmlUtils.RemoveOpenCloseTags(word + "-" + words[index + 1], HtmlUtils.TagItalic);

            return null;
        }

        private static string GetWordWithDominatedCasing(string word)
        {
            int lowercase = 0;
            int uppercase = 0;
            for (int i = 0; i < word.Length; i++)
            {
                if (Utilities.LowercaseLetters.Contains(word[i]))
                    lowercase++;
                else if (Utilities.UppercaseLetters.Contains(word[i]))
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
                        if (s.Contains(' '))
                            _namesEtcMultiWordList.Add(s);
                        else
                        {
                            _namesEtcList.Add(s);
                            _namesEtcListUppercase.Add(s.ToUpper());
                            if (_fiveLetterWordListLanguageName.StartsWith("en"))
                            {
                                if (!s.EndsWith('s'))
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
                case OcrSpellCheck.Action.ChangeAllWholeText:
                    SaveWordToWholeLineList(_spellCheck.OriginalWholeText);
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
            }
            if (result.Fixed)
            {
                result.Line = ReplaceWord(line, word, result.Word);
            }
            return result;
        }

        private static string ReplaceWord(string text, string word, string newWord)
        {
            var sb = new StringBuilder();
            if (word != null && text != null && text.Contains(word))
            {
                int appendFrom = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (text.Substring(i).StartsWith(word) && i >= appendFrom)
                    {
                        bool startOk = i == 0;
                        if (!startOk)
                            startOk = (@" ¡¿<>-""”“()[]'‘`´¶♪¿¡.…—!?,:;/" + Environment.NewLine).Contains(text[i - 1]);
                        if (!startOk && word.StartsWith(' '))
                            startOk = true;
                        if (startOk)
                        {
                            bool endOk = (i + word.Length == text.Length);
                            if (!endOk)
                                endOk = (@" ¡¿<>-""”“()[]'‘`´¶♪¿¡.…—!?,:;/" + Environment.NewLine).Contains(text[i + word.Length]);
                            if (!endOk)
                                endOk = newWord.EndsWith(' ');
                            if (endOk)
                            {
                                sb.Append(newWord);
                                appendFrom = i + word.Length;
                            }
                        }
                    }
                    if (i >= appendFrom)
                        sb.Append(text[i]);
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

        private void SaveWordToWholeLineList(string line)
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
                    if (!_wholeLineReplaceList.ContainsKey(line))
                        _wholeLineReplaceList.Add(line, _spellCheck.Paragraph);
                    XmlNode wholeWordsNode = doc.DocumentElement.SelectSingleNode("WholeLines");
                    if (wholeWordsNode != null)
                    {
                        XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "Line", null);
                        XmlAttribute aFrom = doc.CreateAttribute("from");
                        XmlAttribute aTo = doc.CreateAttribute("to");
                        aFrom.InnerText = line;
                        aTo.InnerText = _spellCheck.Paragraph;
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
                    int index = s.IndexOf(letter, StringComparison.Ordinal);
                    s = AddToGuessList(list, s, index, letter, _partialWordReplaceList[letter]);
                    AddToGuessList(list, word, index, letter, _partialWordReplaceList[letter]);
                    i++;
                }
                s = word;
                i = 0;
                while (s.Contains(letter) && i < 10)
                {
                    int index = s.LastIndexOf(letter, StringComparison.Ordinal);
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

            if (_wordSkipList.Contains(word))
                return true;

            if (_namesEtcList.Contains(word.Trim('\'')))
                return true;

            if (_namesEtcListUppercase.Contains(word.Trim('\'')))
                return true;

            if (_userWordList.Contains(word.ToLower()))
                return true;

            if (_userWordList.Contains(word.Trim('\'').ToLower()))
                return true;

            if (word.Length > 2 && _namesEtcListUppercase.Contains(word))
                return true;

            if (word.Length > 2 && _namesEtcListWithApostrophe.Contains(word))
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
            var words = HtmlUtils.RemoveOpenCloseTags(line, HtmlUtils.TagItalic).Split((Environment.NewLine + " ¡¿,.!?:;()[]{}+-$£\"#&%…“”").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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
                else if (word.Length > 3)
                    numberOfCorrectWords++;
            }
            return wordsNotFound;
        }

    }
}
