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
        private readonly string _languageName;

        public NameList(string dictionaryFolder, string languageName, bool useOnlinenames, string namesUrl)
        {
            _dictionaryFolder = dictionaryFolder;
            _languageName = languageName;

            _namesList = new HashSet<string>();
            _namesMultiList = new HashSet<string>();
            _blackList = new HashSet<string>();

            LoadNamesList(GetLocalNamesFileName()); // e.g: en_names.xml (culture insensitive)
            if (useOnlinenames && !string.IsNullOrEmpty(namesUrl))
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
                    _namesList.Remove(name);
                if (_namesMultiList.Contains(name))
                    _namesMultiList.Remove(name);
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

        /// <summary>
        /// Returns two letters ISO language name (Neutral culture).
        /// </summary>
        private string GetLocalNamesFileName()
        {
            // Converts e.g en_US => en (Neutral culture).
            string twoLetterISOLanguageName = _languageName;
            if (_languageName.Length > 2)
            {
                twoLetterISOLanguageName = _languageName.Substring(0, 2);
            }
            return Path.Combine(_dictionaryFolder, twoLetterISOLanguageName + "_names.xml");
        }

        private void LoadNamesList(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) ||
                (!File.Exists(fileName) &&
                 !fileName.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) &&
                 !fileName.StartsWith("\\", StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            using (XmlReader reader = XmlReader.Create(fileName))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "blacklist")
                        {
                            while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
                            {
                                if (reader.Name == "name")
                                {
                                    string name = reader.ReadElementContentAsString().Trim();
                                    if (name.Length > 0 && !_blackList.Contains(name))
                                    {
                                        _blackList.Add(name);
                                    }
                                }
                            }
                        }
                        else if (reader.Name == "name")
                        {
                            string name = reader.ReadElementContentAsString().Trim();
                            if (name.Length > 0)
                            {
                                if (name.Contains(' '))
                                {
                                    if (!_namesMultiList.Contains(name))
                                    {
                                        _namesMultiList.Add(name);
                                    }
                                }
                                else if (!_namesList.Contains(name))
                                {
                                    _namesList.Add(name);
                                }
                            }
                        }
                    }
                }
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

                var fileName = GetLocalNamesFileName();
                var nameListXml = new XmlDocument();
                if (File.Exists(fileName))
                {
                    nameListXml.Load(fileName);
                }
                else
                {
                    nameListXml.LoadXml("<names><blacklist></blacklist></names>");
                }

                // Add removed name to blacklist
                XmlNode xnode = nameListXml.CreateElement("name");
                xnode.InnerText = name;
                nameListXml.DocumentElement.SelectSingleNode("blacklist").AppendChild(xnode);
                XmlNode nodeToRemove = null;

                // Remove remove-name from name-list
                foreach (XmlNode node in nameListXml.DocumentElement.SelectNodes("name"))
                {
                    if (node.InnerText.Equals(name, StringComparison.Ordinal))
                    {
                        nodeToRemove = node;
                        break;
                    }
                }
                if (nodeToRemove != null)
                {
                    nameListXml.DocumentElement.RemoveChild(nodeToRemove);
                }
                try
                {
                    nameListXml.Save(fileName);
                    return true;
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("NamesList.RemoveRemove failed");
                }
            }
            return false;
        }

        public bool Add(string name)
        {
            name = name.RemoveControlCharacters().Trim();
            if (name.Length == 0 || _blackList.Contains(name))
            {
                return false;
            }
            if (name.ContainsLetter())
            {
                if (name.Contains(' '))
                {
                    if (!_namesMultiList.Contains(name))
                        _namesMultiList.Add(name);
                }
                else if (!_namesList.Contains(name))
                {
                    _namesList.Add(name);
                }

                // <neutral>_names.xml e.g: en_names.xml
                var fileName = GetLocalNamesFileName();
                var nameListXml = new XmlDocument();
                if (File.Exists(fileName))
                {
                    nameListXml.Load(fileName);
                }
                else
                {
                    nameListXml.LoadXml("<names><blacklist></blacklist></names>");
                }

                XmlNode de = nameListXml.DocumentElement;
                if (de != null)
                {
                    XmlNode node = nameListXml.CreateElement("name");
                    node.InnerText = name;
                    de.AppendChild(node);
                    nameListXml.Save(fileName);
                }
                return true;
            }
            return false;
        }

        public bool IsInNamesMultiWordList(string text, string word)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            text = text.Replace(Environment.NewLine, " ");
            text = text.FixExtraSpaces();

            if (_namesMultiList.Contains(word))
            {
                return true;
            }
            foreach (string multiWordName in _namesMultiList)
            {
                if (text.Contains(multiWordName))
                {
                    if (multiWordName.StartsWith(word + " ", StringComparison.Ordinal) || multiWordName.EndsWith(" " + word, StringComparison.Ordinal) || multiWordName.Contains(" " + word + " "))
                        return true;
                }
            }
            return false;
        }

    }
}
