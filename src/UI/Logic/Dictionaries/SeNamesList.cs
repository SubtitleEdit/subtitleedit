using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic.Dictionaries;

public class SeNamesList : INamesList
{
    private string _dictionaryFolder = string.Empty;
    private HashSet<string> _namesList = new();
    private HashSet<string> _namesMultiList = new();
    private HashSet<string> _namesMultiListUppercase = new();
    private HashSet<string> _blackList = new();
    private readonly HashSet<string> _abbreviationList = new();
    public string LanguageName { get; private set; } = "en";

    public void Load(string dictionaryFolder, string languageCode)
    {
        _dictionaryFolder = dictionaryFolder;
        LanguageName = languageCode;

        _namesList = new HashSet<string>();
        _namesMultiList = new HashSet<string>();
        _namesMultiListUppercase = new HashSet<string>();
        _blackList = new HashSet<string>();

        LoadNamesList(GetLocalNamesUserFileName()); // e.g: en_names_user.xml (culture sensitive)
        LoadNamesList(GetLocalNamesFileName()); // e.g: en_names.xml (culture sensitive)
        LoadNamesList(Path.Combine(_dictionaryFolder, "names.xml"));

        foreach (var name in _blackList)
        {
            if (_namesList.Contains(name))
            {
                _namesList.Remove(name);
            }

            if (_namesMultiList.Contains(name))
            {
                _namesMultiList.Remove(name);
            }
        }

        foreach (var name in _namesMultiList)
        {
            _namesMultiListUppercase.Add(name.ToUpperInvariant());
        }

        _abbreviationList.Clear();
        foreach (var name in _namesList)
        {
            if (name.EndsWith('.') && !name.Contains(' '))
            {
                _abbreviationList.Add(name);
            }
        }
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
        var collection = name.Contains(" ") ? _namesMultiList : _namesList;
        return collection.Add(name);
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

        var text = input.Replace(Environment.NewLine, " ");
        text = text.FixExtraSpaces();

        if (_namesMultiList.Contains(word))
        {
            return true;
        }

        if (_namesMultiListUppercase.Contains(word))
        {
            return true;
        }

        foreach (var multiWordName in _namesMultiList)
        {
            if (text.FastIndexOf(multiWordName) < 0)
            {
                continue;
            }

            if (multiWordName.StartsWith(word + " ", StringComparison.Ordinal) || multiWordName.EndsWith(" " + word, StringComparison.Ordinal) || multiWordName.Contains(" " + word + " "))
            {
                return true;
            }
        }

        return false;
    }

    public bool ContainsCaseInsensitive(string name, out string newName)
    {
        newName = string.Empty;
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        foreach (var n in name.IndexOf(' ') >= 0 ? _namesMultiList : _namesList)
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

    private void LoadNamesList(string fileNameOrUrl)
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
                        if (name.IndexOf(' ') >= 0)
                        {
                            _namesMultiList.Add(name);
                        }
                        else
                        {
                            _namesList.Add(name);
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
                                _blackList.Add(name);
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