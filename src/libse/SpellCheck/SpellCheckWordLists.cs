using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SpellCheck
{

    public class SpellCheckWordLists
    {

        public static readonly HashSet<char> SplitChars = new HashSet<char>
        {
            ' ', '-', '.', ',', '?', '!', ':', ';', '\\', '"', '“', '”', '(', ')', '[', ']', '{', '}', '|', '<', '>', '/', '+', '\r', '\n',
            '¿', '¡', '…', '—', '–', '♪', '♫', '„', '«', '»', '‹', '›', '؛', '،', '؟', '\u00A0', '\u1680', '\u2000', '\u2001', '\u2002', '\u2003',
            '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A', '\u200B', '\u200E', '\u200F', '\u2028', '\u2029', '\u202A',
            '\u202B', '\u202C', '\u202D', '\u202E', '\u202F', '\u3000', '\uFEFF'
        };

        private static readonly char[] PeriodAndDash = { '.', '-' };
        private static readonly char[] SplitChars2 = { ' ', '.', ',', '?', '!', ':', ';', '"', '“', '”', '(', ')', '[', ']', '{', '}', '|', '<', '>', '/', '+', '\r', '\n', '¿', '¡', '…', '—', '–', '♪', '♫', '„', '«', '»', '‹', '›', '؛', '،', '؟' };

        private readonly NameList _nameList;
        private readonly HashSet<string> _names;
        private readonly HashSet<string> _namesListUppercase = new HashSet<string>();
        private readonly HashSet<string> _namesListWithApostrophe = new HashSet<string>();
        private readonly HashSet<string> _wordsWithDashesOrPeriods = new HashSet<string>();
        private readonly HashSet<string> _userWordList = new HashSet<string>();
        private readonly HashSet<string> _userPhraseList = new HashSet<string>();
        private readonly string _dictionaryFolder;
        private HashSet<string> _skipAllList = new HashSet<string>();
        private readonly Dictionary<string, string> _useAlwaysList = new Dictionary<string, string>();
        private readonly string _languageName;
        private readonly IDoSpell _doSpell;

        public SpellCheckWordLists(string dictionaryFolder, string languageName, IDoSpell doSpell)
        {
            _dictionaryFolder = dictionaryFolder ?? throw new NullReferenceException(nameof(dictionaryFolder));
            _languageName = languageName ?? throw new NullReferenceException(nameof(languageName));
            _doSpell = doSpell ?? throw new NullReferenceException(nameof(doSpell));
            _nameList = new NameList(Configuration.DictionariesDirectory, languageName, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            _names = _nameList.GetNames();
            var namesMultiWordList = _nameList.GetMultiNames();
            if (Configuration.Settings.Tools.RememberUseAlwaysList)
            {
                LoadUseAlwaysList();
            }

            foreach (string namesItem in _names)
            {
                _namesListUppercase.Add(namesItem.ToUpperInvariant());
            }

            if (languageName.StartsWith("en_", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string namesItem in _names)
                {
                    if (!namesItem.EndsWith('s'))
                    {
                        _namesListWithApostrophe.Add(namesItem + "'s");
                        _namesListWithApostrophe.Add(namesItem + "’s");
                    }
                    else if (!namesItem.EndsWith('\''))
                    {
                        _namesListWithApostrophe.Add(namesItem + "'");
                    }
                }
            }

            if (File.Exists(dictionaryFolder + languageName + "_user.xml"))
            {
                var userWordDictionary = new XmlDocument();
                userWordDictionary.Load(dictionaryFolder + languageName + "_user.xml");
                var xmlNodeList = userWordDictionary.DocumentElement?.SelectNodes("word");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode node in xmlNodeList)
                    {
                        string word = node.InnerText.Trim().ToLowerInvariant();
                        if (word.Contains(' '))
                        {
                            _userPhraseList.Add(word);
                        }
                        else
                        {
                            _userWordList.Add(word);
                        }
                    }
                }
            }
            // Add names/userdic with "." or " " or "-"
            foreach (var word in namesMultiWordList)
            {
                if (word.Contains(PeriodAndDash))
                {
                    _wordsWithDashesOrPeriods.Add(word);
                }
            }
            foreach (string name in _names)
            {
                if (name.Contains(PeriodAndDash))
                {
                    _wordsWithDashesOrPeriods.Add(name);
                }
            }
            foreach (string word in _userWordList)
            {
                if (word.Contains(PeriodAndDash))
                {
                    _wordsWithDashesOrPeriods.Add(word);
                }
            }
            foreach (var phrase in _userPhraseList)
            {
                if (phrase.Contains(PeriodAndDash))
                {
                    _wordsWithDashesOrPeriods.Add(phrase);
                }
            }
        }

        public Dictionary<string, string> GetUseAlwaysList()
        {
            return new Dictionary<string, string>(_useAlwaysList);
        }

        private void LoadUseAlwaysList()
        {
            if (!Configuration.Settings.Tools.RememberUseAlwaysList)
            {
                return;
            }

            var fileName = GetUseAlwaysListFileName();
            var xmlDoc = new XmlDocument();
            if (File.Exists(fileName))
            {
                xmlDoc.Load(fileName);
                var xmlNodeList = xmlDoc.DocumentElement?.SelectNodes("Pair");
                if (xmlNodeList != null)
                {
                    foreach (XmlNode item in xmlNodeList)
                    {
                        if (item.Attributes?["from"] != null && item.Attributes["to"] != null)
                        {
                            var to = item.Attributes["to"].Value;
                            var from = item.Attributes["from"].Value;
                            if (!_useAlwaysList.ContainsKey(from))
                            {
                                _useAlwaysList.Add(from, to);
                            }
                        }
                    }
                }
            }
            else
            {
                xmlDoc.LoadXml("<UseAlways></UseAlways>");
            }
        }

        private string GetUseAlwaysListFileName()
        {
            return Path.Combine(_dictionaryFolder, _languageName + "_UseAlways.xml");
        }

        public void UseAlwaysListAdd(string newKey, string newValue)
        {
            SaveUseAlwaysList(newKey, newValue);
        }

        public void UseAlwaysListRemove(string key)
        {
            SaveUseAlwaysList(null, null, key);
        }

        private void SaveUseAlwaysList(string newKey = null, string newValue = null, string oldKey = null)
        {
            if (!Configuration.Settings.Tools.RememberUseAlwaysList)
            {
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<UseAlways></UseAlways>");

            if (newKey != null && newValue != null && !_useAlwaysList.ContainsKey(newKey.Trim()))
            {
                _useAlwaysList.Add(newKey.Trim(), newValue.Trim());
            }
            if (oldKey != null && _useAlwaysList.ContainsKey(oldKey.Trim()))
            {
                _useAlwaysList.Remove(oldKey.Trim());
            }
            _skipAllList = new HashSet<string>(_skipAllList.OrderBy(p => p).ToList());

            foreach (KeyValuePair<string, string> kvp in _useAlwaysList)
            {
                XmlNode node = xmlDoc.CreateElement("Pair");
                var f = xmlDoc.CreateAttribute("from");
                f.Value = kvp.Key;
                var t = xmlDoc.CreateAttribute("to");
                t.Value = kvp.Value;
                if (node.Attributes != null)
                {
                    node.Attributes.Append(f);
                    node.Attributes.Append(t);
                }
                xmlDoc.DocumentElement?.AppendChild(node);
            }
            xmlDoc.Save(GetUseAlwaysListFileName());
        }

        public void RemoveUserWord(string word)
        {
            _userWordList.Remove(word);
            _userPhraseList.Remove(word);
            Utilities.RemoveFromUserDictionary(word, _languageName);
        }

        public void RemoveName(string word)
        {
            if (word == null || word.Length <= 1 || !_names.Contains(word))
            {
                return;
            }

            _names.Remove(word);
            _namesListUppercase.Remove(word.ToUpperInvariant());
            if (_languageName.StartsWith("en_", StringComparison.Ordinal) && !word.EndsWith('s'))
            {
                _names.Remove(word + "s");
                _namesListUppercase.Remove(word.ToUpperInvariant() + "S");
            }
            if (!word.EndsWith('s'))
            {
                _namesListWithApostrophe.Remove(word + "'s");
                _namesListUppercase.Remove(word.ToUpperInvariant() + "'S");
            }
            if (!word.EndsWith('\''))
            {
                _namesListWithApostrophe.Remove(word + "'");
            }

            _nameList.Remove(word);
        }

        public string ReplaceKnownWordsOrNamesWithBlanks(string s)
        {
            var replaceIds = new List<string>();
            var replaceNames = new List<string>();
            GetTextWithoutUserWordsAndNames(replaceIds, replaceNames, s);
            foreach (string name in replaceNames)
            {
                int start = s.IndexOf(name, StringComparison.Ordinal);
                while (start >= 0)
                {
                    bool startOk = start == 0 || SplitChars.Contains(s[start - 1]) || char.IsControl(s[start - 1]);
                    if (startOk)
                    {
                        int end = start + name.Length;
                        bool endOk = end >= s.Length || SplitChars.Contains(s[end]) || char.IsControl(s[end]);
                        if (endOk)
                        {
                            s = s.Remove(start, name.Length).Insert(start, string.Empty.PadLeft(name.Length));
                        }
                    }

                    if (start + 1 < s.Length)
                    {
                        start = s.IndexOf(name, start + 1, StringComparison.Ordinal);
                    }
                    else
                    {
                        start = -1;
                    }
                }
            }
            return s;
        }

        public string ReplaceHtmlTagsWithBlanks(string s)
        {
            int start = s.IndexOf('<');
            while (start >= 0)
            {
                int end = s.IndexOf('>', start + 1);
                if (end < start)
                {
                    break;
                }

                int l = end - start + 1;
                s = s.Remove(start, l).Insert(start, string.Empty.PadLeft(l));
                end++;
                if (end >= s.Length)
                {
                    break;
                }

                start = s.IndexOf('<', end);
            }
            return s;
        }

        public string ReplaceAssTagsWithBlanks(string s)
        {
            int start = s.IndexOf("{\\", StringComparison.Ordinal);
            int end = s.IndexOf('}');
            if (start < 0 || end < 0 || end < start)
            {
                return s;
            }

            while (start >= 0)
            {
                end = s.IndexOf('}', start + 1);
                if (end < start)
                {
                    break;
                }

                int l = end - start + 1;
                s = s.Remove(start, l).Insert(start, string.Empty.PadLeft(l));
                end++;
                if (end >= s.Length)
                {
                    break;
                }

                start = s.IndexOf("{\\", end, StringComparison.Ordinal);
            }
            return s;
        }

        public bool IsWordInUserPhrases(int index, List<SpellCheckWord> words)
        {
            string current = words[index].Text;
            string prev = "-";
            if (index > 0)
            {
                prev = words[index - 1].Text;
            }

            string next = "-";
            if (index < words.Count - 1)
            {
                next = words[index + 1].Text;
            }

            foreach (string userPhrase in _userPhraseList)
            {
                if (userPhrase == current + " " + next)
                {
                    return true;
                }

                if (userPhrase == prev + " " + current)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes words with dash'es that are correct, so spell check can ignore the combination (do not split correct words with dash'es)
        /// </summary>
        private void GetTextWithoutUserWordsAndNames(List<string> replaceIds, List<string> replaceNames, string text)
        {
            string[] wordsWithDash = text.Split(SplitChars2, StringSplitOptions.RemoveEmptyEntries);
            foreach (string w in wordsWithDash)
            {
                if (w.Contains('-') && _doSpell.DoSpell(w) && !_wordsWithDashesOrPeriods.Contains(w))
                {
                    _wordsWithDashesOrPeriods.Add(w);
                }
            }

            if (text.Contains(PeriodAndDash))
            {
                int i = 0;
                foreach (string wordWithDashesOrPeriods in _wordsWithDashesOrPeriods)
                {
                    bool found = true;
                    int startSearchIndex = 0;
                    while (found)
                    {
                        int indexStart = text.IndexOf(wordWithDashesOrPeriods, startSearchIndex, StringComparison.Ordinal);

                        if (indexStart >= 0)
                        {
                            int endIndexPlus = indexStart + wordWithDashesOrPeriods.Length;
                            bool startOk = indexStart == 0 || (@" (['""" + "\r\n").Contains(text[indexStart - 1]);
                            bool endOk = endIndexPlus == text.Length;
                            if (!endOk && endIndexPlus < text.Length && @",!?:;. ])<'""".Contains(text[endIndexPlus]))
                            {
                                endOk = true;
                            }

                            if (startOk && endOk)
                            {
                                i++;
                                string id = $"_@{i}_";
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
        }

        public bool AddName(string word)
        {
            if (string.IsNullOrEmpty(word) || _names.Contains(word))
            {
                return false;
            }

            _names.Add(word);
            _namesListUppercase.Add(word.ToUpperInvariant());
            if (_languageName.StartsWith("en_", StringComparison.Ordinal) && !word.EndsWith('s'))
            {
                _names.Add(word + "s");
                _namesListUppercase.Add(word.ToUpperInvariant() + "S");
            }
            if (!word.EndsWith('s'))
            {
                _namesListWithApostrophe.Add(word + "'s");
                _namesListUppercase.Add(word.ToUpperInvariant() + "'S");
            }
            if (!word.EndsWith('\''))
            {
                _namesListWithApostrophe.Add(word + "'");
            }

            _wordsWithDashesOrPeriods.Add(word);

            var namesList = new NameList(Configuration.DictionariesDirectory, _languageName, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            namesList.Add(word);
            return true;
        }

        public bool AddUserWord(string word)
        {
            if (word == null)
            {
                return false;
            }

            word = word.Trim().ToLowerInvariant();
            if (word.Length == 0 || _userWordList.Contains(word))
            {
                return false;
            }

            if (word.Contains(' '))
            {
                _userPhraseList.Add(word);
            }
            else
            {
                _userWordList.Add(word);
            }

            Utilities.AddToUserDictionary(word, _languageName);
            return true;
        }

        public bool HasName(string word)
        {
            return _names.Contains(word) || (word.StartsWith('\'') || word.EndsWith('\'')) && _names.Contains(word.Trim('\''));
        }

        public bool HasNameExtended(string word, string text)
        {
            return _namesListUppercase.Contains(word) || _namesListWithApostrophe.Contains(word) || _nameList.IsInNamesMultiWordList(text, word);
        }

        public bool HasUserWord(string word)
        {
            string s = word.ToLowerInvariant();
            return _userWordList.Contains(s) || (s.StartsWith('\'') || s.EndsWith('\'')) && _userWordList.Contains(s.Trim('\''));
        }

        public static List<SpellCheckWord> Split(string s)
        {
            var list = new List<SpellCheckWord>();
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (SplitChars.Contains(s[i]) || char.IsControl(s[i]))
                {
                    if (sb.Length > 0)
                    {
                        list.Add(new SpellCheckWord { Text = sb.ToString(), Index = i - sb.Length });
                    }

                    sb.Clear();
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            if (sb.Length > 0)
            {
                list.Add(new SpellCheckWord { Text = sb.ToString(), Index = s.Length - sb.Length });
            }

            return list;
        }

    }
}
