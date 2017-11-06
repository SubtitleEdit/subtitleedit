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

        public static readonly string SplitChars = " -.,?!:;\"“”()[]{}|<>/+\r\n¿¡…—–♪♫„«»‹›؛،؟";

        private static readonly char[] PeriodAndDash = { '.', '-' };
        private static readonly char[] SplitChars2 = { ' ', '.', ',', '?', '!', ':', ';', '"', '“', '”', '(', ')', '[', ']', '{', '}', '|', '<', '>', '/', '+', '\r', '\n', '¿', '¡', '…', '—', '–', '♪', '♫', '„', '«', '»', '‹', '›', '؛', '،', '؟' };

        private readonly NameList _nameList;
        private readonly HashSet<string> _names;
        private readonly HashSet<string> _namesListUppercase = new HashSet<string>();
        private readonly HashSet<string> _namesListWithApostrophe = new HashSet<string>();
        private readonly HashSet<string> _wordsWithDashesOrPeriods = new HashSet<string>();
        private readonly HashSet<string> _userWordList = new HashSet<string>();
        private readonly HashSet<string> _userPhraseList = new HashSet<string>();
        private readonly string _languageName;
        private readonly IDoSpell _doSpell;

        public SpellCheckWordLists(string dictionaryFolder, string languageName, IDoSpell doSpell)
        {
            if (languageName == null)
                throw new NullReferenceException(nameof(languageName));
            if (doSpell == null)
                throw new NullReferenceException(nameof(doSpell));

            _languageName = languageName;
            _doSpell = doSpell;
            _nameList = new NameList(Configuration.DictionariesDirectory, languageName, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            _names = _nameList.GetNames();
            var namesMultiWordList = _nameList.GetMultiNames();

            foreach (string namesItem in _names)
                _namesListUppercase.Add(namesItem.ToUpper());

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
            foreach (var word in namesMultiWordList)
            {
                if (word.Contains(PeriodAndDash))
                    _wordsWithDashesOrPeriods.Add(word);
            }
            foreach (string name in _names)
            {
                if (name.Contains(PeriodAndDash))
                    _wordsWithDashesOrPeriods.Add(name);
            }
            foreach (string word in _userWordList)
            {
                if (word.Contains(PeriodAndDash))
                    _wordsWithDashesOrPeriods.Add(word);
            }
            foreach (var phrase in _userPhraseList)
            {
                if (phrase.Contains(PeriodAndDash))
                    _wordsWithDashesOrPeriods.Add(phrase);
            }
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
                return;

            _names.Remove(word);
            _namesListUppercase.Remove(word.ToUpper());
            if (_languageName.StartsWith("en_", StringComparison.Ordinal) && !word.EndsWith('s'))
            {
                _names.Remove(word + "s");
                _namesListUppercase.Remove(word.ToUpper() + "S");
            }
            if (!word.EndsWith('s'))
            {
                _namesListWithApostrophe.Remove(word + "'s");
                _namesListUppercase.Remove(word.ToUpper() + "'S");
            }
            if (!word.EndsWith('\''))
                _namesListWithApostrophe.Remove(word + "'");

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
            if (string.IsNullOrEmpty(word) || _names.Contains(word))
                return false;

            _names.Add(word);
            _namesListUppercase.Add(word.ToUpper());
            if (_languageName.StartsWith("en_", StringComparison.Ordinal) && !word.EndsWith('s'))
            {
                _names.Add(word + "s");
                _namesListUppercase.Add(word.ToUpper() + "S");
            }
            if (!word.EndsWith('s'))
            {
                _namesListWithApostrophe.Add(word + "'s");
                _namesListUppercase.Add(word.ToUpper() + "'S");
            }
            if (!word.EndsWith('\''))
                _namesListWithApostrophe.Add(word + "'");

            var namesList = new NameList(Configuration.DictionariesDirectory, _languageName, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            namesList.Add(word);
            return true;
        }

        public bool AddUserWord(string word)
        {
            if (word == null)
                return false;

            word = word.Trim().ToLower();
            if (word.Length == 0 || _userWordList.Contains(word))
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
            return _names.Contains(word) || ((word.StartsWith('\'') || word.EndsWith('\'')) && _names.Contains(word.Trim('\'')));
        }

        public bool HasNameExtended(string word, string text)
        {
            return _namesListUppercase.Contains(word) || _namesListWithApostrophe.Contains(word) || _nameList.IsInNamesMultiWordList(text, word);
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
