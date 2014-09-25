using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic
{
    public class OcrFixReplaceList
    {

        private static readonly Regex RegExQuestion = new Regex(@"\S\?[A-ZÆØÅÄÖÉÈÀÙÂÊÎÔÛËÏa-zæøåäöéèàùâêîôûëï]", RegexOptions.Compiled);
        private static readonly Regex RegExIandZero = new Regex(@"[a-zæøåöääöéèàùâêîôûëï][I1]", RegexOptions.Compiled);
        private static readonly Regex RegExTime1 = new Regex(@"[a-zæøåöääöéèàùâêîôûëï][0]", RegexOptions.Compiled);
        private static readonly Regex RegExTime2 = new Regex(@"0[a-zæøåöääöéèàùâêîôûëï]", RegexOptions.Compiled);
        private static readonly Regex HexNumber = new Regex(@"^#?[\dABDEFabcdef]+$", RegexOptions.Compiled);
        private static readonly Regex StartEndEndsWithNumber = new Regex(@"^\d+.+\d$", RegexOptions.Compiled);

        private readonly Dictionary<string, string> _wordReplaceList;
        private readonly Dictionary<string, string> _partialLineWordBoundaryReplaceList;
        private readonly Dictionary<string, string> _partialLineAlwaysReplaceList;
        private readonly Dictionary<string, string> _beginLineReplaceList;
        private readonly Dictionary<string, string> _endLineReplaceList;
        private readonly Dictionary<string, string> _wholeLineReplaceList;
        private readonly Dictionary<string, string> _partialWordReplaceListAlways;
        private readonly Dictionary<string, string> _partialWordReplaceList;
        private readonly Dictionary<string, string> _regExList;
        private readonly string _replaceListXmlFileName;

        public OcrFixReplaceList(string languageId)
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

            _replaceListXmlFileName = Configuration.DictionariesFolder + languageId + "_OCRFixReplaceList.xml";
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

        private static Dictionary<string, string> LoadReplaceList(XmlDocument doc, string name)
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

        private static Dictionary<string, string> LoadRegExList(XmlDocument doc, string name)
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

        public string FixCommonWordErrors(string word)
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
                var match = RegExQuestion.Match(word);
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
                var match = RegExIandZero.Match(word);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        if (word[match.Index + 1] == 'I' || word[match.Index + 1] == '1')
                        {
                            bool doFix = word[match.Index + 1] != 'I' && match.Index >= 1 && word.Substring(match.Index - 1).StartsWith("Mc");
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

        public string FixCommonWordErrorsQuick(string word)
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

        public void SaveWordToWordList(string word, string spellCheckWord)
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
                    _wordReplaceList.Add(word, spellCheckWord);
                XmlNode wholeWordsNode = doc.DocumentElement.SelectSingleNode("WholeWords");
                if (wholeWordsNode != null)
                {
                    XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "Word", null);
                    XmlAttribute aFrom = doc.CreateAttribute("from");
                    XmlAttribute aTo = doc.CreateAttribute("to");
                    aFrom.InnerText = word;
                    aTo.InnerText = spellCheckWord;
                    newNode.Attributes.Append(aFrom);
                    newNode.Attributes.Append(aTo);
                    wholeWordsNode.AppendChild(newNode);
                    doc.Save(_replaceListXmlFileName);
                }
            }
        }

        public void SaveWordToWholeLineList(string line, string spellCheckLine)
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
                        _wholeLineReplaceList.Add(line, spellCheckLine);
                    XmlNode wholeWordsNode = doc.DocumentElement.SelectSingleNode("WholeLines");
                    if (wholeWordsNode != null)
                    {
                        XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "Line", null);
                        XmlAttribute aFrom = doc.CreateAttribute("from");
                        XmlAttribute aTo = doc.CreateAttribute("to");
                        aFrom.InnerText = line;
                        aTo.InnerText = spellCheckLine;
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

        public static string ReplaceWord(string text, string word, string newWord)
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

    }
}