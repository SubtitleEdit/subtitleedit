using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.SpellCheck
{

    public class SpellCheckWordLists
    {

        public static readonly string SplitChars = " -.,?!:;\"“”()[]{}|<>/+\r\n¿¡…—–♪♫„“";

        private static readonly char[] PeriodAndDash = { '.', '-' };
        private static readonly char[] SplitChars2 = { ' ', '.', ',', '?', '!', ':', ';', '"', '“', '”', '(', ')', '[', ']', '{', '}', '|', '<', '>', '/', '+', '\r', '\n', '¿', '¡', '…', '—', '–', '♪', '♫', '„', '“' };

        private readonly NamesList _namesList;
        private readonly HashSet<string> _namesEtcList;
        private readonly List<string> _namesEtcListUppercase = new List<string>();
        private readonly List<string> _namesEtcListWithApostrophe = new List<string>();
        private readonly List<string> _wordsWithDashesOrPeriods;
        private readonly List<string> _userWordList;
        private readonly List<string> _userPhraseList;
        private readonly string _languageName;
        private readonly IDoSpell _doSpell;

        public SpellCheckWordLists(string dictionaryFolder, string languageName, IDoSpell doSpell)
        {
            if (languageName == null)
                throw new NullReferenceException("languageName");
            if (doSpell == null)
                throw new NullReferenceException("doSpell");

            _languageName = languageName;
            _doSpell = doSpell;
            _namesList = new NamesList(Configuration.DictionariesFolder, languageName, Configuration.Settings.WordLists.UseOnlineNamesEtc, Configuration.Settings.WordLists.NamesEtcUrl);
            _namesEtcList = _namesList.GetNames();
            var namesEtcMultiWordList = _namesList.GetMultiNames();

            foreach (string namesItem in _namesEtcList)
                _namesEtcListUppercase.Add(namesItem.ToUpper());

            if (languageName.StartsWith("en_", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string namesItem in _namesEtcList)
                {
                    if (!namesItem.EndsWith('s'))
                    {
                        _namesEtcListWithApostrophe.Add(namesItem + "'s");
                        _namesEtcListWithApostrophe.Add(namesItem + "’s");
                    }
                    else if (!namesItem.EndsWith('\''))
                    {
                        _namesEtcListWithApostrophe.Add(namesItem + "'");
                    }
                }
            }

            _userWordList = new List<string>();
            _userPhraseList = new List<string>();
            if (File.Exists(dictionaryFolder + languageName + "_user.xml"))
            {
                var userWordDictionary = new XmlDocument();
                userWordDictionary.Load(dictionaryFolder + languageName + "_user.xml");
                if (userWordDictionary.DocumentElement != null)
                {
                    var xmlNodeList = userWordDictionary.DocumentElement.SelectNodes("word");
                    if (xmlNodeList != null)
                    {
                        foreach (XmlNode node in xmlNodeList)
                        {
                            string word = node.InnerText.Trim().ToLower();
                            if (word.Contains(' '))
                                _userPhraseList.Add(word);
                            else
                                _userWordList.Add(word);
                        }
                    }
                }
            }

            // Add names/userdic with "." or " " or "-"
            _wordsWithDashesOrPeriods = new List<string>();
            _wordsWithDashesOrPeriods.AddRange(namesEtcMultiWordList);
            foreach (string name in _namesEtcList)
            {
                if (name.Contains(PeriodAndDash))
                    _wordsWithDashesOrPeriods.Add(name);
            }
            foreach (string word in _userWordList)
            {
                if (word.Contains(PeriodAndDash))
                    _wordsWithDashesOrPeriods.Add(word);
            }
            _wordsWithDashesOrPeriods.AddRange(_userPhraseList);
        }


        public void RemoveUserWord(string word)
        {
            _userWordList.Remove(word);
            _userPhraseList.Remove(word);
            Utilities.RemoveFromUserDictionary(word, _languageName);
        }

        public void RemoveName(string word)
        {
            if (word == null || word.Length <= 1 || !_namesEtcList.Contains(word))
                return;

            _namesEtcList.Remove(word);
            _namesEtcListUppercase.Remove(word.ToUpper());
            if (_languageName.StartsWith("en_", StringComparison.Ordinal) && !word.EndsWith('s'))
            {
                _namesEtcList.Remove(word + "s");
                _namesEtcListUppercase.Remove(word.ToUpper() + "S");
            }
            if (!word.EndsWith('s'))
            {
                _namesEtcListWithApostrophe.Remove(word + "'s");
                _namesEtcListUppercase.Remove(word.ToUpper() + "'S");
            }
            if (!word.EndsWith('\''))
                _namesEtcListWithApostrophe.Remove(word + "'");

            _namesList.Remove(word);
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
                    bool startOk = start == 0 || SplitChars.Contains(s[start - 1]);
                    if (startOk)
                    {
                        int end = start + name.Length;
                        bool endOk = end >= s.Length || SplitChars.Contains(s[end]);
                        if (endOk)
                            s = s.Remove(start, name.Length).Insert(start, string.Empty.PadLeft(name.Length));
                    }

                    if (start + 1 < s.Length)
                        start = s.IndexOf(name, start + 1, StringComparison.Ordinal);
                    else
                        start = -1;
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
                    break;
                int l = end - start + 1;
                s = s.Remove(start, l).Insert(start, string.Empty.PadLeft(l));
                end++;
                if (end >= s.Length)
                    break;
                start = s.IndexOf('<', end);
            }
            return s;
        }

        public bool IsWordInUserPhrases(int index, List<SpellCheckWord> words)
        {
            string current = words[index].Text;
            string prev = "-";
            if (index > 0)
                prev = words[index - 1].Text;
            string next = "-";
            if (index < words.Count - 1)
                next = words[index + 1].Text;
            foreach (string userPhrase in _userPhraseList)
            {
                if (userPhrase == current + " " + next)
                    return true;
                if (userPhrase == prev + " " + current)
                    return true;
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
                    _wordsWithDashesOrPeriods.Add(w);
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
                                endOk = true;
                            if (startOk && endOk)
                            {
                                i++;
                                string id = string.Format("_@{0}_", i);
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
            if (word == null || word.Length <= 1 || _namesEtcList.Contains(word))
                return false;

            _namesEtcList.Add(word);
            _namesEtcListUppercase.Add(word.ToUpper());
            if (_languageName.StartsWith("en_", StringComparison.Ordinal) && !word.EndsWith('s'))
            {
                _namesEtcList.Add(word + "s");
                _namesEtcListUppercase.Add(word.ToUpper() + "S");
            }
            if (!word.EndsWith('s'))
            {
                _namesEtcListWithApostrophe.Add(word + "'s");
                _namesEtcListUppercase.Add(word.ToUpper() + "'S");
            }
            if (!word.EndsWith('\''))
                _namesEtcListWithApostrophe.Add(word + "'");

            var namesList = new NamesList(Configuration.DictionariesFolder, _languageName, Configuration.Settings.WordLists.UseOnlineNamesEtc, Configuration.Settings.WordLists.NamesEtcUrl);
            namesList.Add(word);
            return true;
        }

        public bool AddUserWord(string word)
        {
            if (word == null)
                return false;

            word = word.Trim().ToLower();
            if (word.Length <= 1 || _userWordList.IndexOf(word) >= 0)
                return false;

            if (word.Contains(' '))
                _userPhraseList.Add(word);
            else
                _userWordList.Add(word);
            Utilities.AddToUserDictionary(word, _languageName);
            return true;
        }

        public bool HasName(string word)
        {
            return _namesEtcList.Contains(word) || ((word.StartsWith('\'') || word.EndsWith('\'')) && _namesEtcList.Contains(word.Trim('\'')));
        }

        public bool HasNameExtended(string word, string text)
        {
            return _namesEtcListUppercase.Contains(word) || _namesEtcListWithApostrophe.Contains(word) || _namesList.IsInNamesEtcMultiWordList(text, word);
        }

        public bool HasUserWord(string word)
        {
            string s = word.ToLower();
            return _userWordList.Contains(s) || (s.StartsWith('\'') || s.EndsWith('\'')) && _userWordList.Contains(s.Trim('\''));
        }

        public static List<SpellCheckWord> Split(string s)
        {
            var list = new List<SpellCheckWord>();
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (SplitChars.Contains(s[i]))
                {
                    if (sb.Length > 0)
                        list.Add(new SpellCheckWord { Text = sb.ToString(), Index = i - sb.Length });
                    sb.Clear();
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            if (sb.Length > 0)
                list.Add(new SpellCheckWord { Text = sb.ToString(), Index = s.Length - sb.Length });
            return list;
        }

    }
}
