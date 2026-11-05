using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;

namespace Nikse.SubtitleEdit.Logic.Dictionaries;

public class SeNamesList : INamesList
{
    private string _dictionaryFolder = string.Empty;
    private HashSet<string> _namesList = new();
    private HashSet<string> _namesMultiList = new();
    private HashSet<string> _namesMultiListUppercase = new();
    private HashSet<string> _blackList = new();

    // Lazy reverse index for IsInNamesMultiWordList, see GetMultiNamePartIndex.
    private Dictionary<string, List<string>>? _namesMultiListParts;

    // Case insensitive: "dr." is as much an abbreviation as "Dr.", and the name lists only
    // carry the capitalized form.
    private readonly HashSet<string> _abbreviationList = new(StringComparer.OrdinalIgnoreCase);
    public string LanguageName { get; private set; } = "en";

    // Bumped by every LoadAsync, so a slower earlier load cannot overwrite a later one.
    private int _loadGeneration;

    /// <summary>
    /// Reads the name files on a worker thread into fresh sets and swaps them in when done, so the
    /// UI thread is not blocked and a list that is in use is never seen half-filled.
    /// </summary>
    public async Task LoadAsync(string dictionaryFolder, string languageCode)
    {
        var generation = ++_loadGeneration;
        _dictionaryFolder = dictionaryFolder;
        LanguageName = languageCode;

        string[] fileNames =
        [
            GetLocalNamesUserFileName(), // e.g: en_names_user.xml (culture sensitive)
            GetLocalNamesFileName(), // e.g: en_names.xml (culture sensitive)
            GetLocalCultureNamesFileName(), // e.g: pt_BR_names.xml (region specific)
            Path.Combine(dictionaryFolder, "names.xml"),
        ];

        var loaded = await Task.Run(() => LoadNames(fileNames, dictionaryFolder, languageCode));
        if (generation != _loadGeneration)
        {
            return;
        }

        _namesList = loaded.Names;
        _namesMultiList = loaded.MultiNames;
        _namesMultiListUppercase = loaded.MultiNamesUppercase;
        _blackList = loaded.BlackList;
        _namesMultiListParts = null;
        _abbreviationList.Clear();
        _abbreviationList.UnionWith(loaded.Abbreviations);
    }

    private sealed record LoadedNames(
        HashSet<string> Names,
        HashSet<string> MultiNames,
        HashSet<string> MultiNamesUppercase,
        HashSet<string> BlackList,
        List<string> Abbreviations);

    private static LoadedNames LoadNames(string[] fileNames, string dictionaryFolder, string languageCode)
    {
        var names = new HashSet<string>();
        var multiNames = new HashSet<string>();
        var multiNamesUppercase = new HashSet<string>();
        var blackList = new HashSet<string>();

        foreach (var fileName in fileNames)
        {
            LoadNamesList(fileName, names, multiNames, blackList);
        }

        foreach (var name in blackList)
        {
            names.Remove(name);
            multiNames.Remove(name);
        }

        foreach (var name in multiNames)
        {
            multiNamesUppercase.Add(name.ToUpperInvariant());
        }

        var abbreviations = new List<string>();
        foreach (var name in names)
        {
            if (name.EndsWith('.') && !name.Contains(' '))
            {
                abbreviations.Add(name);
            }
        }

        // e.g. nl_abbreviations.xml - abbreviations that are not names ("enz.", "ca.", "nr.").
        abbreviations.AddRange(AbbreviationList.Load(dictionaryFolder, languageCode));

        return new LoadedNames(names, multiNames, multiNamesUppercase, blackList, abbreviations);
    }

    public List<string> GetAllNames()
    {
        var list = new List<string>(_namesList.Count + _namesMultiList.Count);
        list.AddRange(_namesList);
        list.AddRange(_namesMultiList);
        return list;
    }

    public HashSet<string> GetNames()
    {
        return _namesList;
    }

    public HashSet<string> GetMultiNames()
    {
        return _namesMultiList;
    }

    public bool Remove(string name)
    {
        name = name.Trim();
        if (name.Length > 1 && _namesList.Contains(name) || _namesMultiList.Contains(name))
        {
            // Try removing name from both lists
            _namesList.Remove(name);
            _namesMultiList.Remove(name);
            _namesMultiListParts = null;

            var fileName = GetLocalNamesUserFileName();
            var nameListXml = CreateDocument(fileName);

            // Add removed name to blacklist
            var nameNode = nameListXml.CreateElement("name");
            nameNode.InnerText = name;
            if (nameListXml.DocumentElement != null)
            {
                nameListXml.DocumentElement.SelectSingleNode("blacklist")?.AppendChild(nameNode);
                var nodeToRemove = default(XmlNode);

                // Remove remove-name from name-list
                var nameNodes = nameListXml.DocumentElement.SelectNodes("name");
                if (nameNodes != null)
                {
                    foreach (XmlNode node in nameNodes)
                    {
                        if (node.InnerText.Equals(name, StringComparison.Ordinal))
                        {
                            nodeToRemove = node;
                            break;
                        }
                    }
                }

                if (nodeToRemove != null)
                {
                    nameListXml.DocumentElement.RemoveChild(nodeToRemove);
                }
            }

            try
            {
                nameListXml.Save(fileName);
                return true;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("NamesList.Remove failed");
            }
        }

        return false;
    }

    public bool Add(string name)
    {
        name = name.RemoveControlCharacters().Trim();
        if (name.Length == 0 || _blackList.Contains(name) || !name.ContainsLetter())
        {
            return false;
        }

        if (!TryAdd(name))
        {
            return false;
        }

        // <two-letter-iso-code>_names.xml, e.g "en_names.xml"
        var fileName = GetLocalNamesUserFileName();
        var nameListXml = CreateDocument(fileName);

        var de = nameListXml.DocumentElement;
        if (de != null)
        {
            var node = nameListXml.CreateElement("name");
            node.InnerText = name;
            de.AppendChild(node);
            nameListXml.Save(fileName);
        }

        return true;
    }

    private bool TryAdd(string name)
    {
        if (name.Contains(" "))
        {
            _namesMultiListParts = null;
            return _namesMultiList.Add(name);
        }

        return _namesList.Add(name);
    }

    private static XmlDocument CreateDocument(string fileName)
    {
        var xmlDocument = new XmlDocument { XmlResolver = null };
        if (File.Exists(fileName))
        {
            xmlDocument.Load(fileName);
        }
        else
        {
            xmlDocument.LoadXml("<names><blacklist></blacklist></names>");
        }

        return xmlDocument;
    }

    public bool IsInNamesMultiWordList(string input, string word)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(word))
        {
            return false;
        }

        if (_namesMultiList.Contains(word))
        {
            return true;
        }

        if (_namesMultiListUppercase.Contains(word))
        {
            return true;
        }

        // Per word of every line during spell check - see NameList.IsInNamesMultiWordList: the
        // "not part of any multi-word name" answer must cost one lookup, not a scan of all
        // several hundred names with three string concatenations each.
        if (!GetMultiNamePartIndex().TryGetValue(word, out var candidates))
        {
            return false;
        }

        var text = input.Replace(Environment.NewLine, " ");
        text = text.FixExtraSpaces();

        foreach (var multiWordName in candidates)
        {
            if (text.FastIndexOf(multiWordName) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Each space-delimited part of every multi-word name, mapped to the names containing it.
    /// Built on first use and dropped whenever the multi-word list changes.
    /// </summary>
    private Dictionary<string, List<string>> GetMultiNamePartIndex()
    {
        var index = _namesMultiListParts;
        if (index != null)
        {
            return index;
        }

        index = new Dictionary<string, List<string>>();
        foreach (var multiWordName in _namesMultiList)
        {
            foreach (var part in multiWordName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!index.TryGetValue(part, out var names))
                {
                    names = new List<string>();
                    index[part] = names;
                }

                names.Add(multiWordName);
            }
        }

        _namesMultiListParts = index;
        return index;
    }

    public bool ContainsCaseInsensitive(string name, out string newName)
    {
        newName = string.Empty;
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        foreach (var n in name.Contains(' ') ? _namesMultiList : _namesList)
        {
            if (name.Equals(n, StringComparison.OrdinalIgnoreCase))
            {
                newName = n;
                return true;
            }
        }

        return false;
    }

    public bool IsName(string candidate)
    {
        return _namesList.Contains(candidate);
    }

    public HashSet<string> GetAbbreviations()
    {
        return _abbreviationList;
    }

    private string GetLocalNamesUserFileName()
    {
        var fileName = GetLocalNamesFileName();
        return fileName.Remove(fileName.Length - 4) + "_user.xml";
    }

    /// <summary>
    /// Returns two letters ISO language name (Neutral culture).
    /// </summary>
    private string GetLocalNamesFileName()
    {
        // Converts e.g en_US => en (Neutral culture).
        string twoLetterIsoLanguageName = LanguageName;
        if (LanguageName.Length > 2)
        {
            twoLetterIsoLanguageName = LanguageName.Substring(0, 2);
        }

        return Path.Combine(_dictionaryFolder, twoLetterIsoLanguageName + "_names.xml");
    }

    /// <summary>
    /// Region specific names file, e.g. pt_BR_names.xml. Portuguese ships one list per region
    /// (pt_PT / pt_BR) and no neutral pt_names.xml, so without this they were never loaded.
    /// Returns an empty string for a neutral language name; LoadNamesList ignores a missing file.
    /// </summary>
    private string GetLocalCultureNamesFileName()
    {
        if (LanguageName.Length <= 2)
        {
            return string.Empty;
        }

        return Path.Combine(_dictionaryFolder, LanguageName + "_names.xml");
    }

    private static void LoadNamesList(string fileNameOrUrl, HashSet<string> names, HashSet<string> multiNames, HashSet<string> blackList)
    {
        if (string.IsNullOrEmpty(fileNameOrUrl) ||
            !File.Exists(fileNameOrUrl) &&
            !fileNameOrUrl.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) &&
            !fileNameOrUrl.StartsWith("\\", StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        try
        {
            using var reader = XmlReader.Create(fileNameOrUrl);
            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement)
                {
                    continue;
                }

                if (reader.Name == "name")
                {
                    var name = reader.ReadElementContentAsString().Trim();
                    if (name.Length > 0)
                    {
                        if (name.Contains(' '))
                        {
                            multiNames.Add(name);
                        }
                        else
                        {
                            names.Add(name);
                        }
                    }
                }
                else if (reader.Name == "blacklist")
                {
                    while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
                    {
                        if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement)
                        {
                            continue;
                        }

                        if (reader.Name == "name")
                        {
                            var name = reader.ReadElementContentAsString().Trim();
                            if (name.Length > 0)
                            {
                                blackList.Add(name);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("SeNamesList: Unable to read name list file: " + fileNameOrUrl, ex);
        }
    }
}