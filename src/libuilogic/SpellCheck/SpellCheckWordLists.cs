using System.Text;
using System.Xml;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.UiLogic.SpellCheck;

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
    private static readonly char[] ApostropheChars = { '\'', '‘', '’' };
    private static readonly char[] SplitChars2 = { ' ', '.', ',', '?', '!', ':', ';', '"', '“', '”', '(', ')', '[', ']', '{', '}', '|', '<', '>', '/', '+', '\r', '\n', '¿', '¡', '…', '—', '–', '♪', '♫', '„', '«', '»', '‹', '›', '؛', '،', '؟' };

    private readonly NameList _nameList;
    private readonly HashSet<string> _names;
    private readonly HashSet<string> _namesListUppercase = new HashSet<string>();
    private readonly HashSet<string> _namesListWithApostrophe = new HashSet<string>();
    private readonly HashSet<string> _wordsWithDashesOrPeriods = new HashSet<string>();
    private readonly HashSet<string> _userWordList = new HashSet<string>();
    private readonly HashSet<string> _userPhraseList = new HashSet<string>();
    private readonly string _dictionaryFolder;
    private readonly Dictionary<string, string> _useAlwaysList = new Dictionary<string, string>();
    private readonly string _languageName;
    private readonly IDoSpell _doSpell;

    public SpellCheckWordLists(string fiveLetterName, IDoSpell doSpell)
    {
        _dictionaryFolder = SpellCheckConfig.DictionariesFolder();
        _languageName = fiveLetterName ?? throw new NullReferenceException(nameof(fiveLetterName));
        _doSpell = doSpell ?? throw new NullReferenceException(nameof(doSpell));
        _nameList = new NameList(_dictionaryFolder, fiveLetterName, false, string.Empty);
        _names = _nameList.GetNames();
        var namesMultiWordList = _nameList.GetMultiNames();
        if (Configuration.Settings.Tools.RememberUseAlwaysList)
        {
            LoadUseAlwaysList();
        }

        foreach (var namesItem in _names)
        {
            _namesListUppercase.Add(namesItem.ToUpperInvariant());
        }

        if (fiveLetterName.StartsWith("en_", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var namesItem in _names)
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

        var paths = new[]
        {
            Path.Combine(_dictionaryFolder, fiveLetterName + "_user.xml"),
            Path.Combine(_dictionaryFolder, fiveLetterName + "_se.xml"),
        };

        var xmlDoc = new XmlDocument();
        foreach (var path in paths)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            xmlDoc.Load(path);
            var xmlNodeList = xmlDoc.DocumentElement?.SelectNodes("word");
            if (xmlNodeList != null)
            {
                foreach (XmlNode node in xmlNodeList)
                {
                    var word = Utilities.NormalizeUserDictionaryWord(node.InnerText);
                    if (word.Length == 0)
                    {
                        continue;
                    }

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
        foreach (var word in GetSingleUnifiedCollection(namesMultiWordList))
        {
            if (word.Contains(PeriodAndDash))
            {
                _wordsWithDashesOrPeriods.Add(word);
            }
        }
    }

    private IEnumerable<string> GetSingleUnifiedCollection(IEnumerable<string> namesMultiWordList)
    {
        return namesMultiWordList
            .Union(_names)
            .Union(_userWordList)
            .Union(_userPhraseList);
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
                        var to = item.Attributes["to"]?.Value;
                        var from = item.Attributes["from"]?.Value;
                        if (to != null && from != null && !_useAlwaysList.ContainsKey(from))
                        {
                            _useAlwaysList.Add(from, to);
                        }
                    }
                }
            }
        }
    }

    private string GetUseAlwaysListFileName()
    {
        return Path.Combine(_dictionaryFolder, _languageName + "_UseAlways.xml");
    }

    public void UseAlwaysListRemove(string key)
    {
        SaveUseAlwaysList(null, null, key);
    }

    private void SaveUseAlwaysList(string? newKey = null, string? newValue = null, string? oldKey = null)
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
        word = Utilities.NormalizeUserDictionaryWord(word);
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

    public bool IsWordInUserPhrases(int index, List<SpellCheckWord> words)
    {
        string current = Utilities.NormalizeUserDictionaryWord(words[index].Text);
        string prev = "-";
        if (index > 0)
        {
            prev = Utilities.NormalizeUserDictionaryWord(words[index - 1].Text);
        }

        string next = "-";
        if (index < words.Count - 1)
        {
            next = Utilities.NormalizeUserDictionaryWord(words[index + 1].Text);
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

        var namesList = new NameList(_dictionaryFolder, _languageName, false, string.Empty);
        namesList.Add(word);
        return true;
    }

    public bool AddUserWord(string word)
    {
        if (word == null)
        {
            return false;
        }

        word = Utilities.NormalizeUserDictionaryWord(word);
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
        return _namesListUppercase.Contains(word) ||
               _namesListWithApostrophe.Contains(word) ||
               _nameList.IsInNamesMultiWordList(text, word) ||
               IsPartOfKnownDashOrPeriodName(word, text);
    }

    /// <summary>
    /// True when <paramref name="word"/> is a dash/period-delimited part of a known combined name or
    /// word (e.g. "Soo" or "bin" of "Soo-bin") that actually appears in <paramref name="text"/>. The
    /// spell-check tokenizer splits on '-' and '.', so without this a hyphenated name added to the
    /// names list would be flagged part by part (#10126).
    /// </summary>
    private bool IsPartOfKnownDashOrPeriodName(string word, string text)
    {
        if (string.IsNullOrEmpty(word))
        {
            return false;
        }

        foreach (var combined in _wordsWithDashesOrPeriods)
        {
            var parts = combined.Split(PeriodAndDash, StringSplitOptions.RemoveEmptyEntries);
            if (Array.IndexOf(parts, word) >= 0 && text.Contains(combined, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasUserWord(string word)
    {
        string s = Utilities.NormalizeUserDictionaryWord(word);
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
                    AddWord(list, sb.ToString(), i - sb.Length);
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
            AddWord(list, sb.ToString(), s.Length - sb.Length);
        }

        return list;
    }

    // A token consisting solely of apostrophes/quote marks (e.g. the trailing ' in "...universum.'")
    // is punctuation, not a word. The apostrophe is intentionally not a split char so contractions
    // ('n, don't) and quote-wrapped words ('Met de waarheid') stay intact, but a lone quote must not
    // be spell-checked - it has no word characters and would just be flagged as unknown. (#12143)
    private static void AddWord(List<SpellCheckWord> list, string text, int index)
    {
        if (text.Trim(ApostropheChars).Length == 0)
        {
            return;
        }

        list.Add(new SpellCheckWord { Text = text, Index = index });
    }

    public List<string> GetAllNames()
    {
        return _nameList.GetAllNames();
    }
}
