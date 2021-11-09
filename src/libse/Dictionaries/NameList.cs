using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Dictionaries
{
    public class NameList
    {
        private readonly string _dictionaryFolder;
        private readonly HashSet<string> _namesList;
        private readonly HashSet<string> _namesMultiList;
        private readonly HashSet<string> _blackList;
        public string LanguageName { get; private set; }

        public NameList(string dictionaryFolder, string languageName, bool useOnlineNameList, string namesUrl)
        {
            _dictionaryFolder = dictionaryFolder;
            LanguageName = languageName;

            _namesList = new HashSet<string>();
            _namesMultiList = new HashSet<string>();
            _blackList = new HashSet<string>();

            LoadNamesList(GetLocalNamesUserFileName()); // e.g: en_names_user.xml (culture sensitive)
            LoadNamesList(GetLocalNamesFileName()); // e.g: en_names.xml (culture sensitive)
            if (useOnlineNameList && !string.IsNullOrEmpty(namesUrl))
            {
                try
                {
                    LoadNamesList(Configuration.Settings.WordLists.NamesUrl);
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                }
            }
            else
            {
                LoadNamesList(Path.Combine(_dictionaryFolder, "names.xml"));
            }
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
                using (var reader = XmlReader.Create(fileNameOrUrl))
                {
                    reader.MoveToContent();
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement)
                        {
                            if (reader.Name == "name")
                            {
                                var name = reader.ReadElementContentAsString().Trim();
                                if (name.Length > 0)
                                {
                                    if (name.Contains(' '))
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
                                    if (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement)
                                    {
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
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to read name list file: " + fileNameOrUrl, ex);
            }
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
                var nameListXml = new XmlDocument { XmlResolver = null };
                if (File.Exists(fileName))
                {
                    nameListXml.Load(fileName);
                }
                else
                {
                    nameListXml.LoadXml("<names><blacklist></blacklist></names>");
                }

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

            if (name.Contains(' '))
            {
                if (!_namesMultiList.Contains(name))
                {
                    _namesMultiList.Add(name);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (!_namesList.Contains(name))
                {
                    _namesList.Add(name);
                }
                else
                {
                    return false;
                }
            }

            // <two-letter-iso-code>_names.xml, e.g "en_names.xml"
            var fileName = GetLocalNamesUserFileName();
            var nameListXml = new XmlDocument { XmlResolver = null };
            if (File.Exists(fileName))
            {
                nameListXml.Load(fileName);
            }
            else
            {
                nameListXml.LoadXml("<names><blacklist></blacklist></names>");
            }

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

        public bool IsInNamesMultiWordList(string input, string word)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            var text = input.Replace(Environment.NewLine, " ");
            text = text.FixExtraSpaces();

            if (_namesMultiList.Contains(word))
            {
                return true;
            }
            foreach (string multiWordName in _namesMultiList)
            {
                if (text.FastIndexOf(multiWordName) >= 0)
                {
                    if (multiWordName.StartsWith(word + " ", StringComparison.Ordinal) || multiWordName.EndsWith(" " + word, StringComparison.Ordinal) || multiWordName.Contains(" " + word + " "))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ContainsCaseInsensitive(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            foreach (var n in name.Contains(' ') ? _namesMultiList : _namesList)
            {
                if (name.Equals(n, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
