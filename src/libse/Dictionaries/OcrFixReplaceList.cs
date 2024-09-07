﻿using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Dictionaries
{
    public class OcrFixReplaceList
    {
        private static readonly Regex RegExQuestion = new Regex(@"\S\?[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏa-zæøåäöéèàùâêîôûëï]", RegexOptions.Compiled);
        private static readonly Regex RegExIAndZero = new Regex(@"[a-zæøåöääöéèàùâêîôûëï][I1]", RegexOptions.Compiled);
        private static readonly Regex RegExTime1 = new Regex(@"[a-zæøåöääöéèàùâêîôûëï]0", RegexOptions.Compiled);
        private static readonly Regex RegExTime2 = new Regex(@"0[a-zæøåöääöéèàùâêîôûëï]", RegexOptions.Compiled);
        private static readonly Regex HexNumber = new Regex(@"^#?[\dABDEFabcdef]+$", RegexOptions.Compiled);
        private static readonly Regex StartsAndEndsWithNumber = new Regex(@"^\d+.+\d$", RegexOptions.Compiled);

        public readonly Dictionary<string, string> WordReplaceList;
        public readonly Dictionary<string, string> PartialLineWordBoundaryReplaceList;
        private readonly Dictionary<string, string> _partialLineAlwaysReplaceList;
        private readonly Dictionary<string, string> _beginLineReplaceList;
        private readonly Dictionary<string, string> _endLineReplaceList;
        private readonly Dictionary<string, string> _wholeLineReplaceList;
        private readonly Dictionary<string, string> _partialWordAlwaysReplaceList;
        private readonly Dictionary<string, string> _partialWordReplaceList;
        private readonly Dictionary<string, string> _regExList;
        private readonly string _replaceListXmlFileName;

        private const string ReplaceListFileNamePostFix = "_OCRFixReplaceList.xml";

        public string ErrorMessage { get; set; }

        public OcrFixReplaceList(string replaceListXmlFileName)
        {
            _replaceListXmlFileName = replaceListXmlFileName;
            WordReplaceList = new Dictionary<string, string>();
            PartialLineWordBoundaryReplaceList = new Dictionary<string, string>();
            _partialLineAlwaysReplaceList = new Dictionary<string, string>();
            _beginLineReplaceList = new Dictionary<string, string>();
            _endLineReplaceList = new Dictionary<string, string>();
            _wholeLineReplaceList = new Dictionary<string, string>();
            _partialWordAlwaysReplaceList = new Dictionary<string, string>();
            _partialWordReplaceList = new Dictionary<string, string>();
            _regExList = new Dictionary<string, string>();

            var doc = LoadXmlReplaceListDocument();
            var userDoc = LoadXmlReplaceListUserDocument();

            WordReplaceList = LoadReplaceList(doc, "WholeWords");
            _partialWordAlwaysReplaceList = LoadReplaceList(doc, "PartialWordsAlways");
            _partialWordReplaceList = LoadReplaceList(doc, "PartialWords");
            PartialLineWordBoundaryReplaceList = LoadReplaceList(doc, "PartialLines");
            _partialLineAlwaysReplaceList = LoadReplaceList(doc, "PartialLinesAlways");
            _beginLineReplaceList = LoadReplaceList(doc, "BeginLines");
            _endLineReplaceList = LoadReplaceList(doc, "EndLines");
            _wholeLineReplaceList = LoadReplaceList(doc, "WholeLines");
            _regExList = LoadRegExList(doc, "RegularExpressions");

            foreach (var kp in LoadReplaceList(userDoc, "RemovedWholeWords"))
            {
                if (WordReplaceList.ContainsKey(kp.Key))
                {
                    WordReplaceList.Remove(kp.Key);
                }
            }
            foreach (var kp in LoadReplaceList(userDoc, "WholeWords"))
            {
                if (!WordReplaceList.ContainsKey(kp.Key))
                {
                    WordReplaceList.Add(kp.Key, kp.Value);
                }
            }

            foreach (var kp in LoadReplaceList(userDoc, "RemovedPartialLines"))
            {
                if (PartialLineWordBoundaryReplaceList.ContainsKey(kp.Key))
                {
                    PartialLineWordBoundaryReplaceList.Remove(kp.Key);
                }
            }
            foreach (var kp in LoadReplaceList(userDoc, "PartialLines"))
            {
                if (!PartialLineWordBoundaryReplaceList.ContainsKey(kp.Key))
                {
                    PartialLineWordBoundaryReplaceList.Add(kp.Key, kp.Value);
                }
            }

            foreach (var kp in LoadReplaceList(userDoc, "RemovedBeginLines"))
            {
                if (_beginLineReplaceList.ContainsKey(kp.Key))
                {
                    _beginLineReplaceList.Remove(kp.Key);
                }
            }
            foreach (var kp in LoadReplaceList(userDoc, "BeginLines"))
            {
                if (!_beginLineReplaceList.ContainsKey(kp.Key))
                {
                    _beginLineReplaceList.Add(kp.Key, kp.Value);
                }
            }

            foreach (var kp in LoadReplaceList(userDoc, "RemovedEndLines"))
            {
                if (_endLineReplaceList.ContainsKey(kp.Key))
                {
                    _endLineReplaceList.Remove(kp.Key);
                }
            }
            foreach (var kp in LoadReplaceList(userDoc, "EndLines"))
            {
                if (!_endLineReplaceList.ContainsKey(kp.Key))
                {
                    _endLineReplaceList.Add(kp.Key, kp.Value);
                }
            }

            foreach (var kp in LoadReplaceList(userDoc, "RemovedWholeLines"))
            {
                if (_wholeLineReplaceList.ContainsKey(kp.Key))
                {
                    _wholeLineReplaceList.Remove(kp.Key);
                }
            }
            foreach (var kp in LoadReplaceList(userDoc, "WholeLines"))
            {
                if (!_wholeLineReplaceList.ContainsKey(kp.Key))
                {
                    _wholeLineReplaceList.Add(kp.Key, kp.Value);
                }
            }

            foreach (var kp in LoadRegExList(userDoc, "RemovedRegularExpressions"))
            {
                if (_regExList.ContainsKey(kp.Key))
                {
                    _regExList.Remove(kp.Key);
                }
            }
            foreach (var kp in LoadRegExList(userDoc, "RegularExpressions"))
            {
                if (!_regExList.ContainsKey(kp.Key))
                {
                    _regExList.Add(kp.Key, kp.Value);
                }
            }
        }

        public static OcrFixReplaceList FromLanguageId(string languageId)
        {
            return new OcrFixReplaceList(Configuration.DictionariesDirectory + languageId + ReplaceListFileNamePostFix);
        }

        private static Dictionary<string, string> LoadReplaceList(XmlDocument doc, string name)
        {
            var list = new Dictionary<string, string>();
            if (!IsValidXmlDocument(doc, name))
            {
                return list;
            }

            var node = doc.DocumentElement?.SelectSingleNode(name);
            if (node != null)
            {
                foreach (XmlNode item in node.ChildNodes)
                {
                    if (!HasValidAttributes(item, false) || item.Attributes == null)
                    {
                        continue;
                    }

                    var to = item.Attributes["to"].Value;
                    var from = item.Attributes["from"].Value;
                    if (!list.ContainsKey(from))
                    {
                        list.Add(from, to);
                    }
                }
            }

            return list;
        }

        private static Dictionary<string, string> LoadRegExList(XmlDocument doc, string name)
        {
            var list = new Dictionary<string, string>();
            if (!IsValidXmlDocument(doc, name))
            {
                return list;
            }

            var node = doc.DocumentElement?.SelectSingleNode(name);
            if (node != null)
            {
                foreach (XmlNode item in node.ChildNodes)
                {
                    if (!HasValidAttributes(item, true) || item.Attributes == null)
                    {
                        continue;
                    }

                    var to = item.Attributes["replaceWith"].Value;
                    var from = item.Attributes["find"].Value;
                    if (!list.ContainsKey(from))
                    {
                        list.Add(from, to);
                    }
                }
            }

            return list;
        }

        private static bool IsValidXmlDocument(XmlDocument doc, string elementName)
        {
            return doc.DocumentElement?.SelectSingleNode(elementName) != null;
        }

        private static bool HasValidAttributes(XmlNode node, bool isRegex)
        {
            if (node?.Attributes == null)
            {
                return false;
            }

            if (isRegex)
            {
                if (node.Attributes["find"] != null && node.Attributes["replaceWith"] != null)
                {
                    return RegexUtils.IsValidRegex(node.Attributes["find"].Value);
                }
            }
            else
            {
                if (node.Attributes["from"] != null && node.Attributes["to"] != null)
                {
                    return (node.Attributes["from"].Value != node.Attributes["to"].Value);
                }
            }
            return false;
        }

        public string FixOcrErrorViaLineReplaceList(string input, Subtitle subtitle, int index)
        {
            // Whole fromLine
            foreach (var from in _wholeLineReplaceList.Keys)
            {
                if (input == from)
                {
                    return _wholeLineReplaceList[from];
                }
            }

            var newText = input;
            var pre = string.Empty;
            if (newText.StartsWith("<i>", StringComparison.Ordinal))
            {
                pre += "<i>";
                newText = newText.Remove(0, 3);
            }
            while (newText.Length > 1 && @" -""['¶(".Contains(newText[0]))
            {
                pre += newText[0];
                newText = newText.Substring(1);
            }
            if (newText.StartsWith("<i>", StringComparison.Ordinal))
            {
                pre += "<i>";
                newText = newText.Remove(0, 3);
            }

            // begin fromLine
            var lines = newText.SplitToLines();
            var sb = new StringBuilder(input.Length + 2);
            foreach (var l in lines)
            {
                var s = l;
                foreach (string from in _beginLineReplaceList.Keys)
                {
                    if (s.FastIndexOf(from) >= 0)
                    {
                        var with = _beginLineReplaceList[from];
                        if (s.StartsWith(from, StringComparison.Ordinal))
                        {
                            s = s.Remove(0, from.Length).Insert(0, with);
                        }
                        s = s.Replace(". " + from, ". " + with);
                        s = s.Replace("! " + from, "! " + with);
                        s = s.Replace("? " + from, "? " + with);
                        if (s.StartsWith("\"" + from, StringComparison.Ordinal) && !from.StartsWith('"'))
                        {
                            s = s.Replace("\"" + from, "\"" + with);
                        }
                    }
                }
                sb.AppendLine(s);
            }
            newText = pre + sb.ToString().TrimEnd(Utilities.NewLineChars);

            var post = string.Empty;
            if (newText.EndsWith("</i>", StringComparison.Ordinal))
            {
                newText = newText.Remove(newText.Length - 4, 4);
                post = "</i>";
            }

            foreach (var from in _endLineReplaceList.Keys)
            {
                if (newText.EndsWith(from, StringComparison.Ordinal))
                {
                    var position = (newText.Length - from.Length);
                    var toText = _endLineReplaceList[from];

                    if (!SkipAddLineEnding(subtitle, from, toText, index))
                    {
                        newText = newText.Remove(position).Insert(position, toText);
                    }
                }
            }
            newText += post;

            foreach (var from in PartialLineWordBoundaryReplaceList.Keys)
            {
                if (newText.FastIndexOf(from) >= 0)
                {
                    newText = ReplaceWord(newText, from, PartialLineWordBoundaryReplaceList[from]);
                }
            }

            foreach (var from in _partialLineAlwaysReplaceList.Keys)
            {
                if (newText.FastIndexOf(from) >= 0)
                {
                    newText = newText.Replace(from, _partialLineAlwaysReplaceList[from]);
                }
            }

            if (_replaceRegExes == null || _regExList.Count != _replaceRegExes.Count)
            {
                _replaceRegExes = new List<Regex>();
                foreach (var findWhat in _regExList.Keys)
                {
                    var regex = new Regex(findWhat, RegexOptions.Multiline | RegexOptions.Compiled);
                    _replaceRegExes.Add(regex);
                    newText = regex.Replace(newText, _regExList[findWhat]);
                }
            }
            else
            {
                var i = 0;
                foreach (var findWhat in _regExList.Keys)
                {
                    var regex = _replaceRegExes[i];
                    newText = regex.Replace(newText, _regExList[findWhat]);
                    i++;
                }
            }

            return newText;
        }

        private bool SkipAddLineEnding(Subtitle subtitle, string from, string toText, int index)
        {
            if (!toText.EndsWith('.') || from.EndsWith('.'))
            {
                return false;
            }

            var p = subtitle.GetParagraphOrDefault(index);
            var next = subtitle.GetParagraphOrDefault(index+1);
            if (p == null || next == null)
            {
                return false;
            }

            if (next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > 600) 
            { 
                return false;
            }

            var nextText = HtmlUtil.RemoveHtmlTags(next.Text, true);
            if (string.IsNullOrEmpty(nextText))
            {
                return false;
            }

            var firstLetter = nextText[0];
            if (char.IsLetter(firstLetter) && firstLetter == char.ToLowerInvariant(firstLetter))
            {
                return true;
            }

            return false;
        }

        private List<Regex> _replaceRegExes;

        private static void AddToGuessList(List<string> list, string guess)
        {
            if (string.IsNullOrEmpty(guess))
            {
                return;
            }

            if (!list.Contains(guess))
            {
                list.Add(guess);
            }
        }

        public IEnumerable<string> CreateGuessesFromLetters(string word, string threeLetterIsoLanguageName)
        {
            var list = new List<string>();
            var previousGuesses = new List<string>();
            foreach (var letter in _partialWordReplaceList.Keys)
            {
                var indexes = new List<int>();
                for (var i = 0; i <= word.Length - letter.Length; i++)
                {
                    if (word.Substring(i).StartsWith(letter, StringComparison.Ordinal))
                    {

                        if (i == word.Length - letter.Length && !_partialWordReplaceList[letter].Contains(' '))
                        {
                            var guess = word.Remove(i, letter.Length).Insert(i, _partialWordReplaceList[letter]);
                            AddToGuessList(list, guess);
                        }
                        else
                        {
                            indexes.Add(i);
                            var guess = word.Remove(i, letter.Length).Insert(i, _partialWordReplaceList[letter]);
                            AddToGuessList(list, guess);
                        }
                    }
                }

                if (indexes.Count > 1)
                {
                    if (!_partialWordReplaceList[letter].Contains(' '))
                    {
                        var multiGuess = word;
                        for (var i = indexes.Count - 1; i >= 0; i--)
                        {
                            var idx = indexes[i];
                            multiGuess = multiGuess.Remove(idx, letter.Length).Insert(idx, _partialWordReplaceList[letter]);
                            AddToGuessList(list, multiGuess);
                        }

                        AddToGuessList(list, word.Replace(letter, _partialWordReplaceList[letter]));
                    }
                }
                else if (indexes.Count > 0)
                {
                    AddToGuessList(list, word.Replace(letter, _partialWordReplaceList[letter]));
                }

                if (indexes.Count > 0)
                {
                    for (var i = indexes.Count - 1; i >= 0; i--)
                    {
                        var idx = indexes[i];
                        if (idx > 1 && idx < word.Length - 2)
                        {
                            var guess = word.Remove(idx, letter.Length).Insert(idx, _partialWordReplaceList[letter]);
                            AddToGuessList(list, guess);
                        }
                    }
                }

                foreach (var previousGuess in previousGuesses)
                {
                    for (var i = 0; i < previousGuess.Length - letter.Length; i++)
                    {
                        if (previousGuess.Substring(i).StartsWith(letter, StringComparison.Ordinal))
                        {
                            var guess = previousGuess.Remove(i, letter.Length).Insert(i, _partialWordReplaceList[letter]);
                            AddToGuessList(list, guess);
                        }
                    }
                }

                previousGuesses = new List<string>(list);
            }

            if (threeLetterIsoLanguageName != "dan" &&
                threeLetterIsoLanguageName != "eng" &&
                threeLetterIsoLanguageName != "swe")
            {
                return list;
            }

            // do not keep one letter consonants for languages like Danish, English, Swedish
            var results = new List<string>();
            foreach (var s in list)
            {
                var keep = true;
                var words = s.Split(' ');
                foreach (var w in words)
                {
                    if (w.Length == 1 && char.IsLetter(w[0]) && !"aeiouæøåöüäAEIOUÆØÅÖÜÄ".Contains(w))
                    {
                        keep = false;
                    }
                }
                if (keep)
                {
                    results.Add(s);
                }
            }

            return results;
        }

        public string FixCommonWordErrors(string input)
        {
            var word = input;
            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                // common Latin ligatures from legacy encodings;
                // Unicode includes them only for compatibility and discourages their use
                word = word.Replace("ﬀ", "ff");
                word = word.Replace("ﬁ", "fi");
                word = word.Replace("ﬂ", "fl");
                word = word.Replace("ﬃ", "ffi");
                word = word.Replace("ﬄ", "ffl");
                if (!_replaceListXmlFileName.Contains("\\ell" + ReplaceListFileNamePostFix))
                {
                    word = word.Replace('ν', 'v'); // first 'v' is U+03BD GREEK SMALL LETTER NU
                }
                word = word.Replace('’', '\'');
                word = word.Replace('`', '\'');
                word = word.Replace('´', '\'');
                word = word.Replace('‘', '\'');
            }

            //always replace list
            foreach (var letter in _partialWordAlwaysReplaceList.Keys)
            {
                word = word.Replace(letter, _partialWordAlwaysReplaceList[letter]);
            }

            var pre = string.Empty;
            var post = string.Empty;

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

            while (word.Length > Environment.NewLine.Length && word.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                post = Environment.NewLine + post;
                word = word.Substring(0, word.Length - Environment.NewLine.Length);
            }
            if (word.EndsWith("</i>", StringComparison.Ordinal))
            {
                post = "</i>" + post;
                word = word.Remove(word.Length - 4, 4);
            }
            while (word.Length > 1 && "\".,!?)]:;".Contains(word[word.Length - 1]))
            {
                post = word[word.Length - 1] + post;
                word = word.Substring(0, word.Length - 1);
            }
            if (word.EndsWith("</i>", StringComparison.Ordinal))
            {
                post = "</i>" + post;
                word = word.Remove(word.Length - 4, 4);
            }

            var preWordPost = pre + word + post;
            if (word.Length == 0)
            {
                return preWordPost;
            }

            if (word.Contains('?'))
            {
                var match = RegExQuestion.Match(word);
                if (match.Success)
                {
                    word = word.Insert(match.Index + 2, " ");
                }
            }

            if (GetReplaceWord(pre, word, post, out var res))
            {
                return res;
            }

            var oldWord = word;
            if (Configuration.Settings.Tools.OcrFixUseHardcodedRules)
            {
                // uppercase I or 1 inside lowercase fromWord (will be replaced by lowercase L)
                word = FixIor1InsideLowerCaseWord(word);

                // uppercase 0 inside lowercase fromWord (will be replaced by lowercase L)
                word = Fix0InsideLowerCaseWord(word);

                // uppercase I or 1 inside lowercase fromWord (will be replaced by lowercase L)
                word = FixIor1InsideLowerCaseWord(word);

                word = FixLowerCaseLInsideUpperCaseWord(word); // eg. SCARLETTl => SCARLETTI
            }


            if (oldWord != word)
            {
                // Retry fromWord replace list
                if (GetReplaceWord(pre, word, post, out var result))
                {
                    return result;
                }
            }

            return preWordPost;
        }

        private bool GetReplaceWord(string pre, string word, string post, out string result)
        {
            if (string.IsNullOrEmpty(pre) && string.IsNullOrEmpty(post))
            {
                if (WordReplaceList.ContainsKey(word))
                {
                    result = WordReplaceList[word];
                    return true;
                }

                result = null;
                return false;
            }

            if (WordReplaceList.ContainsKey(pre + word + post))
            {
                result = WordReplaceList[pre + word + post];
                return true;
            }

            if (WordReplaceList.ContainsKey(pre + word))
            {
                result = WordReplaceList[pre + word] + post;
                return true;
            }

            if (WordReplaceList.ContainsKey(word + post))
            {
                result = pre + WordReplaceList[word + post];
                return true;
            }

            if (WordReplaceList.ContainsKey(word))
            {
                result = pre + WordReplaceList[word] + post;
                return true;
            }

            result = null;
            return false;
        }

        public static string FixLowerCaseLInsideUpperCaseWord(string input)
        {
            var word = input;
            if (word.Length > 3)
            {
                var wordNoLowercaseL = word.RemoveChar('l');
                if (wordNoLowercaseL.ToUpperInvariant().Equals(wordNoLowercaseL, StringComparison.Ordinal))
                {
                    if (!word.Contains('<') && !word.Contains('>') && !word.Contains('\''))
                    {
                        word = word.Replace('l', 'I');
                    }
                }
            }

            return word;
        }

        public static string FixIor1InsideLowerCaseWord(string input)
        {
            var word = input;
            if (StartsAndEndsWithNumber.IsMatch(word))
            {
                return word;
            }

            if (word.Contains(new[] { '2', '3', '4', '5', '6', '7', '8', '9' }))
            {
                return word;
            }

            if (HexNumber.IsMatch(word))
            {
                return word;
            }

            if (word.LastIndexOf('I') > 0 || word.LastIndexOf('1') > 0)
            {
                var match = RegExIAndZero.Match(word);
                while (match.Success)
                {
                    if (word[match.Index + 1] == 'I' || word[match.Index + 1] == '1')
                    {
                        var doFix = word[match.Index + 1] != 'I' && match.Index >= 1 && word.Substring(match.Index - 1).StartsWith("Mc", StringComparison.Ordinal);
                        if (word[match.Index + 1] == 'I' && match.Index >= 2 && word.Substring(match.Index - 2).StartsWith("Mac", StringComparison.Ordinal))
                        {
                            doFix = false;
                        }

                        if (doFix)
                        {
                            var oldText = word;
                            word = word.Substring(0, match.Index + 1) + "l";
                            if (match.Index + 2 < oldText.Length)
                            {
                                word += oldText.Substring(match.Index + 2);
                            }
                        }
                    }
                    match = RegExIAndZero.Match(word, match.Index + 1);
                }
            }
            return word;
        }

        public static string Fix0InsideLowerCaseWord(string input)
        {
            var word = input;
            if (StartsAndEndsWithNumber.IsMatch(word))
            {
                return word;
            }

            if (word.Contains(new[] { '1', '2', '3', '4', '5', '6', '7', '8', '9' }) ||
                word.EndsWith("a.m", StringComparison.Ordinal) ||
                word.EndsWith("p.m", StringComparison.Ordinal) ||
                word.EndsWith("am", StringComparison.Ordinal) ||
                word.EndsWith("pm", StringComparison.Ordinal))
            {
                return word;
            }

            if (HexNumber.IsMatch(word))
            {
                return word;
            }

            if (word.LastIndexOf('0') > 0)
            {
                var match = RegExTime1.Match(word);
                while (match.Success)
                {
                    if (word[match.Index + 1] == '0')
                    {
                        var oldText = word;
                        word = word.Substring(0, match.Index + 1) + "o";
                        if (match.Index + 2 < oldText.Length)
                        {
                            word += oldText.Substring(match.Index + 2);
                        }
                    }
                    match = RegExTime1.Match(word);
                }

                const string expectedDigits = "123456789";
                match = RegExTime2.Match(word);
                while (match.Success)
                {
                    if (word[match.Index] == '0')
                    {
                        if (match.Index == 0 || !expectedDigits.Contains(word[match.Index - 1]))
                        {
                            var oldText = word;
                            word = word.Substring(0, match.Index) + "o";
                            if (match.Index + 1 < oldText.Length)
                            {
                                word += oldText.Substring(match.Index + 1);
                            }
                        }
                    }
                    match = RegExTime2.Match(word, match.Index + 1);
                }
            }
            return word;
        }

        public string FixCommonWordErrorsQuick(string input)
        {
            var word = input;

            //always replace list
            foreach (var letter in _partialWordAlwaysReplaceList.Keys)
            {
                word = word.Replace(letter, _partialWordAlwaysReplaceList[letter]);
            }

            var pre = string.Empty;
            var post = string.Empty;

            if (word.StartsWith("<i>", StringComparison.Ordinal))
            {
                pre += "<i>";
                word = word.Remove(0, 3);
            }
            while (word.StartsWith(Environment.NewLine, StringComparison.Ordinal) && word.Length > 2)
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
            while (word.EndsWith(Environment.NewLine, StringComparison.Ordinal) && word.Length > 2)
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

            var preWordPost = pre + word + post;
            if (word.Length == 0)
            {
                return preWordPost;
            }

            if (GetReplaceWord(pre, word, post, out var res))
            {
                return res;
            }

            return preWordPost;
        }

        public bool RemoveWordOrPartial(string word)
        {
            if (PartialLineWordBoundaryReplaceList.ContainsKey(word))
            {
                if (DeletePartialLineFromWordList(word))
                {
                    PartialLineWordBoundaryReplaceList.Remove(word);
                    return true;
                }
                return false;
            }
            if (DeleteWordFromWordList(word))
            {
                if (WordReplaceList.ContainsKey(word))
                {
                    WordReplaceList.Remove(word);
                }
                return true;
            }
            return false;
        }

        private bool DeleteWordFromWordList(string fromWord)
        {
            const string replaceListName = "WholeWords";

            var doc = LoadXmlReplaceListDocument();
            var list = LoadReplaceList(doc, replaceListName);

            var userDoc = LoadXmlReplaceListUserDocument();
            var userList = LoadReplaceList(userDoc, replaceListName);

            return DeleteFromList(fromWord, userDoc, replaceListName, "Word", list, userList);
        }

        private bool DeletePartialLineFromWordList(string fromWord)
        {
            const string replaceListName = "PartialLines";

            var doc = LoadXmlReplaceListDocument();
            var list = LoadReplaceList(doc, replaceListName);

            var userDoc = LoadXmlReplaceListUserDocument();
            var userList = LoadReplaceList(userDoc, replaceListName);

            return DeleteFromList(fromWord, userDoc, replaceListName, "LinePart", list, userList);
        }

        private bool DeleteFromList(string word, XmlDocument userDoc, string replaceListName, string elementName, Dictionary<string, string> dictionary, Dictionary<string, string> userDictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }
            if (userDictionary == null)
            {
                throw new ArgumentNullException(nameof(userDictionary));
            }

            bool removed = false;
            if (userDictionary.ContainsKey(word))
            {
                userDictionary.Remove(word);
                XmlNode wholeWordsNode = userDoc.DocumentElement?.SelectSingleNode(replaceListName);
                if (wholeWordsNode != null)
                {
                    wholeWordsNode.RemoveAll();
                    foreach (var kvp in userDictionary)
                    {
                        XmlNode newNode = userDoc.CreateNode(XmlNodeType.Element, elementName, null);
                        XmlAttribute aFrom = userDoc.CreateAttribute("from");
                        XmlAttribute aTo = userDoc.CreateAttribute("to");
                        aFrom.InnerText = kvp.Key;
                        aTo.InnerText = kvp.Value;
                        if (newNode.Attributes != null)
                        {
                            newNode.Attributes.Append(aTo);
                            newNode.Attributes.Append(aFrom);
                            wholeWordsNode.AppendChild(newNode);
                        }
                    }
                    userDoc.Save(ReplaceListXmlFileNameUser);
                    removed = true;
                }
            }
            if (dictionary.ContainsKey(word))
            {
                XmlNode wholeWordsNode = userDoc.DocumentElement?.SelectSingleNode("Removed" + replaceListName);
                if (wholeWordsNode != null)
                {
                    XmlNode newNode = userDoc.CreateNode(XmlNodeType.Element, elementName, null);
                    XmlAttribute aFrom = userDoc.CreateAttribute("from");
                    XmlAttribute aTo = userDoc.CreateAttribute("to");
                    aFrom.InnerText = word;
                    aTo.InnerText = string.Empty;
                    if (newNode.Attributes != null)
                    {
                        newNode.Attributes.Append(aTo);
                        newNode.Attributes.Append(aFrom);
                        wholeWordsNode.AppendChild(newNode);
                        userDoc.Save(ReplaceListXmlFileNameUser);
                        removed = true;
                    }
                }
            }
            return removed;
        }

        private XmlDocument LoadXmlReplaceListDocument()
        {
            const string xmlText = "<ReplaceList><WholeWords/><PartialLines/><BeginLines/><EndLines/><WholeLines/><RegularExpressions/></ReplaceList>";
            var doc = new XmlDocument();
            if (File.Exists(_replaceListXmlFileName))
            {
                try
                {
                    doc.Load(_replaceListXmlFileName);
                }
                catch (Exception exception)
                {
                    doc.LoadXml(xmlText);
                    ErrorMessage = $"Unable to load ocr replace list {_replaceListXmlFileName}: " + exception.Message;
                }
            }
            else
            {
                doc.LoadXml(xmlText);
            }
            return doc;
        }

        private string ReplaceListXmlFileNameUser => Path.Combine(Path.GetDirectoryName(_replaceListXmlFileName) ?? throw new InvalidOperationException(), Path.GetFileNameWithoutExtension(_replaceListXmlFileName) + "_User" + Path.GetExtension(_replaceListXmlFileName));

        private XmlDocument LoadXmlReplaceListUserDocument()
        {
            const string xmlText = "<ReplaceList><WholeWords/><PartialLines/><BeginLines/><EndLines/><WholeLines/><RegularExpressions/><RemovedWholeWords/><RemovedPartialLines/><RemovedBeginLines/><RemovedEndLines/><RemovedWholeLines/><RemovedRegularExpressions/></ReplaceList>";
            var doc = new XmlDocument();
            if (File.Exists(ReplaceListXmlFileNameUser))
            {
                try
                {
                    doc.Load(ReplaceListXmlFileNameUser);
                }
                catch (Exception exception)
                {
                    doc.LoadXml(xmlText);
                    ErrorMessage = $"Unable to load ocr replace list {ReplaceListXmlFileNameUser}: " + exception.Message;
                }
            }
            else
            {
                doc.LoadXml(xmlText);
            }
            return doc;
        }

        public bool AddWordOrPartial(string fromWord, string toWord)
        {
            if (fromWord.Contains(' '))
            {
                if (SavePartialLineToWordList(fromWord, toWord))
                {
                    if (!PartialLineWordBoundaryReplaceList.ContainsKey(fromWord))
                    {
                        PartialLineWordBoundaryReplaceList.Add(fromWord, toWord);
                    }
                    return true;
                }
                return false;
            }

            if (SaveWordToWordList(fromWord, toWord))
            {
                if (!WordReplaceList.ContainsKey(fromWord))
                {
                    WordReplaceList.Add(fromWord, toWord);
                }
                return true;
            }

            return false;
        }

        private bool SaveWordToWordList(string fromWord, string toWord)
        {
            const string replaceListName = "WholeWords";

            var userDoc = LoadXmlReplaceListUserDocument();
            var userList = LoadReplaceList(userDoc, replaceListName);

            return SaveToList(fromWord, toWord, userDoc, replaceListName, "Word", userList);
        }

        private bool SavePartialLineToWordList(string fromWord, string toWord)
        {
            const string replaceListName = "PartialLines";

            var userDoc = LoadXmlReplaceListUserDocument();
            var userList = LoadReplaceList(userDoc, replaceListName);

            return SaveToList(fromWord, toWord, userDoc, replaceListName, "LinePart", userList);
        }

        private bool SaveToList(string fromWord, string toWord, XmlDocument userDoc, string replaceListName, string elementName, Dictionary<string, string> userDictionary)
        {
            if (userDictionary == null)
            {
                throw new ArgumentNullException(nameof(userDictionary));
            }

            if (userDictionary.ContainsKey(fromWord))
            {
                return false;
            }

            userDictionary.Add(fromWord, toWord);
            XmlNode wholeWordsNode = userDoc.DocumentElement?.SelectSingleNode(replaceListName);

            if (wholeWordsNode == null)
            {
                wholeWordsNode = userDoc.CreateElement(replaceListName);
                userDoc.DocumentElement?.AppendChild(wholeWordsNode);
            }

            XmlNode newNode = userDoc.CreateNode(XmlNodeType.Element, elementName, null);
            XmlAttribute aFrom = userDoc.CreateAttribute("from");
            XmlAttribute aTo = userDoc.CreateAttribute("to");
            aTo.InnerText = toWord;
            aFrom.InnerText = fromWord;
            if (newNode.Attributes != null)
            {
                newNode.Attributes.Append(aFrom);
                newNode.Attributes.Append(aTo);
                wholeWordsNode.AppendChild(newNode);
                userDoc.Save(ReplaceListXmlFileNameUser);
            }

            return true;
        }

        public void AddToWholeLineList(string fromLine, string toLine)
        {
            var userDocument = LoadXmlReplaceListUserDocument();
            if (!_wholeLineReplaceList.ContainsKey(fromLine))
            {
                _wholeLineReplaceList.Add(fromLine, toLine);
            }
            XmlNode wholeWordsNode = userDocument.DocumentElement?.SelectSingleNode("WholeLines");
            if (wholeWordsNode != null)
            {
                XmlNode newNode = userDocument.CreateNode(XmlNodeType.Element, "Line", null);
                XmlAttribute aFrom = userDocument.CreateAttribute("from");
                XmlAttribute aTo = userDocument.CreateAttribute("to");
                aTo.InnerText = toLine;
                aFrom.InnerText = fromLine;
                if (newNode.Attributes != null)
                {
                    newNode.Attributes.Append(aFrom);
                    newNode.Attributes.Append(aTo);
                    wholeWordsNode.AppendChild(newNode);
                    userDocument.Save(ReplaceListXmlFileNameUser);
                }
            }
        }

        public static string ReplaceWord(string text, string word, string newWord)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(word))
            {
                return text;
            }

            var sb = new StringBuilder(text.Length);
            if (text.Contains(word))
            {
                const string separatorChars = @" ¡¿<>-""”“()[]'‘`´¶♪¿¡.…—!?,:;/";
                var appendFrom = 0;
                for (var i = 0; i < text.Length; i++)
                {
                    if (text[i] == word[0] && i >= appendFrom && text.Substring(i).StartsWith(word, StringComparison.Ordinal))
                    {
                        var startOk = i == 0;
                        if (!startOk)
                        {
                            var prevChar = text[i - 1];
                            startOk = char.IsPunctuation(prevChar) || char.IsWhiteSpace(prevChar) || separatorChars.Contains(prevChar);
                        }
                        if (!startOk && word.StartsWith(' '))
                        {
                            startOk = true;
                        }
                        if (startOk)
                        {
                            var endOk = i + word.Length == text.Length;
                            if (!endOk)
                            {
                                var nextChar = text[i + word.Length];
                                endOk = char.IsPunctuation(nextChar) || char.IsWhiteSpace(nextChar) || separatorChars.Contains(nextChar);
                            }
                            if (!endOk)
                            {
                                endOk = newWord.EndsWith(' ');
                            }
                            if (endOk)
                            {
                                sb.Append(newWord);
                                appendFrom = i + word.Length;
                            }
                        }
                    }
                    if (i >= appendFrom)
                    {
                        sb.Append(text[i]);
                    }
                }
            }
            return sb.ToString();
        }
    }
}
