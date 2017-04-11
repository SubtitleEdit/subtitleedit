using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Dictionaries
{
    public class NamesList
    {
        private readonly string _dictionaryFolder;
        private readonly HashSet<string> _namesList;
        private readonly HashSet<string> _namesMultiList;
        private readonly HashSet<string> _blackList;
        private readonly string _languageName;

        public NamesList(string dictionaryFolder, string languageName, bool useOnlineNamesEtc, string namesEtcUrl)
        {
            _dictionaryFolder = dictionaryFolder;
            _languageName = languageName;

            _namesList = new HashSet<string>();
            _namesMultiList = new HashSet<string>();
            _blackList = new HashSet<string>();

            // Should be called 1st in order to init blacklist.
            LoadNamesList(GetLocalNamesFileName()); // e.g: en_names.xml (culture insensitive)

            if (useOnlineNamesEtc && !string.IsNullOrEmpty(namesEtcUrl))
            {
                try
                {
                    // load name from https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/master/Dictionaries/names.xml
                    var xml = Utilities.DownloadString(Configuration.Settings.WordLists.NamesUrl);
                    var nameListXml = new XmlDocument();
                    nameListXml.LoadXml(xml);
                    // name present in blacklist won't be loaded!
                    LoadNamesList(nameListXml);
                }
#if DEBUG
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                }
#else
                catch
                {
                    // ignore
                }
#endif
            }
            else
            {
                // names present in blacklist won't be loaded!
                LoadNamesList(Path.Combine(_dictionaryFolder, "names.xml"));
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

        private void InitBlackList(XmlDocument namesXml)
        {
            if (namesXml == null || namesXml.DocumentElement == null)
            {
                return;
            }
            XmlNode xnode = namesXml.DocumentElement.SelectSingleNode("blacklist");
            // No blacklist element present.
            if (xnode == null)
            {
                return;
            }
            foreach (XmlNode node in xnode.SelectNodes("name"))
            {
                string name = node.InnerText.Trim();
                if (name.Length > 0)
                {
                    _blackList.Add(name);
                }
            }
        }

        /// <summary>
        /// Returns two letters ISO language name (Neutral culture).
        /// </summary>
        /// <returns></returns>
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
            if (!File.Exists(fileName))
            {
                return;
            }
            var nameListXml = new XmlDocument();
            nameListXml.Load(fileName);
            if (nameListXml.DocumentElement == null)
            {
                return;
            }
            LoadNamesList(nameListXml);
        }

        private void LoadNamesList(XmlDocument nameListXml)
        {
            // Initialize blacklist.
            InitBlackList(nameListXml);

            foreach (XmlNode node in nameListXml.DocumentElement.SelectNodes("name"))
            {
                string name = node.InnerText.Trim();
                // skip names in blacklist
                if (_blackList.Contains(name))
                {
                    continue;
                }
                if (name.Contains(' ') && !_namesMultiList.Contains(name))
                {
                    _namesMultiList.Add(name);
                }
                else if (!_namesList.Contains(name))
                {
                    _namesList.Add(name);
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

            foreach (string s in _namesMultiList)
            {
                if (s.Contains(word) && text.Contains(s))
                {
                    if (s.StartsWith(word + " ", StringComparison.Ordinal) || s.EndsWith(" " + word, StringComparison.Ordinal) || s.Contains(" " + word + " "))
                        return true;
                    if (word == s)
                        return true;
                }
            }
            return false;
        }

    }
}
