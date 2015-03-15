using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.Dictionaries
{
    public class NamesList
    {
        private readonly string _dictionaryFolder;
        private readonly HashSet<string> _namesList;
        private readonly HashSet<string> _namesMultiList;
        private readonly string _languageName;

        public NamesList(string dictionaryFolder, string languageName, bool useOnlineNamesEtc, string namesEtcUrl)
        {
            _dictionaryFolder = dictionaryFolder;
            _languageName = languageName;

            _namesList = new HashSet<string>();
            _namesMultiList = new HashSet<string>();

            if (useOnlineNamesEtc && !string.IsNullOrEmpty(namesEtcUrl))
            {
                try
                {
                    var xml = Utilities.DownloadString(Configuration.Settings.WordLists.NamesEtcUrl);
                    var namesDoc = new XmlDocument();
                    namesDoc.LoadXml(xml);
                    LoadNames(_namesList, _namesMultiList, namesDoc);
                }
                catch (Exception exception)
                {
                    LoadNamesList(Path.Combine(_dictionaryFolder, "names_etc.xml"), _namesList, _namesMultiList);
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                }
            }
            else
            {
                LoadNamesList(Path.Combine(_dictionaryFolder, "names_etc.xml"), _namesList, _namesMultiList);
            }

            LoadNamesList(GetLocalNamesFileName(), _namesList, _namesMultiList);

            var userFile = GetLocalNamesUserFileName();
            LoadNamesList(userFile, _namesList, _namesMultiList);
            UnloadRemovedNames(userFile);
        }

        public List<string> GetAllNames()
        {
            var list = new List<string>();
            foreach (var name in _namesList)
            {
                list.Add(name);
            }
            foreach (var name in _namesMultiList)
            {
                list.Add(name);
            }
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

        private void UnloadRemovedNames(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                return;

            var namesDoc = new XmlDocument();
            namesDoc.Load(fileName);
            if (namesDoc.DocumentElement == null)
                return;

            foreach (XmlNode node in namesDoc.DocumentElement.SelectNodes("removed_name"))
            {
                string s = node.InnerText.Trim();
                if (s.Contains(' '))
                {
                    if (_namesMultiList.Contains(s))
                        _namesMultiList.Remove(s);
                }
                else if (_namesList.Contains(s))
                {
                    _namesList.Remove(s);
                }
            }
        }

        private string GetLocalNamesFileName()
        {
            if (_languageName.Length == 2)
            {
                string[] files = Directory.GetFiles(_dictionaryFolder, _languageName + "_??_names_etc.xml");
                if (files.Length > 0)
                    return files[0];
            }
            return Path.Combine(_dictionaryFolder, _languageName + "_names_etc.xml");
        }

        private string GetLocalNamesUserFileName()
        {
            if (_languageName.Length == 2)
            {
                string[] files = Directory.GetFiles(_dictionaryFolder, _languageName + "_??_names_etc_user.xml");
                if (files.Length > 0)
                    return files[0];
            }
            return Path.Combine(_dictionaryFolder, _languageName + "_names_etc_user.xml");
        }

        private static void LoadNamesList(string fileName, HashSet<string> namesList, HashSet<string> namesMultiList)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                return;

            var namesDoc = new XmlDocument();
            namesDoc.Load(fileName);
            if (namesDoc.DocumentElement == null)
                return;

            LoadNames(namesList, namesMultiList, namesDoc);
        }

        private static void LoadNames(HashSet<string> namesList, HashSet<string> namesMultiList, XmlDocument namesDoc)
        {
            foreach (XmlNode node in namesDoc.DocumentElement.SelectNodes("name"))
            {
                string s = node.InnerText.Trim();
                if (s.Contains(' ') && !namesMultiList.Contains(s))
                {
                    namesMultiList.Add(s);
                }
                else if (!namesList.Contains(s))
                {
                    namesList.Add(s);
                }
            }
        }

        public bool Remove(string name)
        {
            name = name.Trim();
            if (name.Length > 1 && _namesList.Contains(name) || _namesMultiList.Contains(name))
            {
                if (_namesList.Contains(name))
                    _namesList.Remove(name);
                if (_namesMultiList.Contains(name))
                    _namesMultiList.Remove(name);

                var userList = new HashSet<string>();
                var fileName = GetLocalNamesUserFileName();
                LoadNamesList(fileName, userList, userList);

                var namesDoc = new XmlDocument();
                if (File.Exists(fileName))
                {
                    namesDoc.Load(fileName);
                }
                else
                {
                    namesDoc.LoadXml("<ignore_words />");
                }

                if (userList.Contains(name))
                {
                    userList.Remove(name);
                    XmlNode nodeToRemove = null;
                    foreach (XmlNode node in namesDoc.DocumentElement.SelectNodes("name"))
                    {
                        if (node.InnerText == name)
                        {
                            nodeToRemove = node;
                            break;
                        }
                    }
                    if (nodeToRemove != null)
                        namesDoc.DocumentElement.RemoveChild(nodeToRemove);
                }
                else
                {
                    XmlNode node = namesDoc.CreateElement("removed_name");
                    node.InnerText = name;
                    namesDoc.DocumentElement.AppendChild(node);
                }
                namesDoc.Save(fileName);
                return true;
            }
            return false;
        }

        public bool Add(string name)
        {
            name = name.Trim();
            if (name.Length > 1 && name.ContainsLetter())
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

                var fileName = GetLocalNamesUserFileName();
                var namesEtcDoc = new XmlDocument();
                if (File.Exists(fileName))
                    namesEtcDoc.Load(fileName);
                else
                    namesEtcDoc.LoadXml("<ignore_words />");

                XmlNode de = namesEtcDoc.DocumentElement;
                if (de != null)
                {
                    XmlNode node = namesEtcDoc.CreateElement("name");
                    node.InnerText = name;
                    de.AppendChild(node);
                    namesEtcDoc.Save(fileName);
                }
                return true;
            }
            return false;
        }

        public bool IsInNamesEtcMultiWordList(string text, string word)
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