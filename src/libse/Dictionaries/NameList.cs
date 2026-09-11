using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Dictionaries
{
    public class NameList
    {
        private readonly string _dictionaryFolder;
        private readonly HashSet<string> _namesList;
        private readonly HashSet<string> _namesMultiList;
        private readonly HashSet<string> _namesMultiListUppercase;
        private readonly HashSet<string> _blackList;

        // Lazy reverse index for IsInNamesMultiWordList, see GetMultiNamePartIndex.
        private Dictionary<string, List<string>> _namesMultiListParts;
        public string LanguageName { get; private set; }

        public NameList(string dictionaryFolder, string languageName, bool useOnlineNameList, string namesUrl)
        {
            _dictionaryFolder = dictionaryFolder;
            LanguageName = languageName;

            _namesList = new HashSet<string>();
            _namesMultiList = new HashSet<string>();
            _namesMultiListUppercase = new HashSet<string>();
            _blackList = new HashSet<string>();

            LoadNamesList(GetLocalNamesUserFileName()); // e.g: en_names_user.xml (culture sensitive)
            LoadNamesList(GetLocalNamesFileName()); // e.g: en_names.xml (culture sensitive)
            LoadNamesList(GetLocalCultureNamesFileName()); // e.g: pt_BR_names.xml (region specific)
            if (useOnlineNameList && !string.IsNullOrEmpty(namesUrl))
            {
                try
                {
                    //LoadNamesList(Configuration.Settings.WordLists.NamesUrl);
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

            foreach (var name in _namesMultiList)
            {
                _namesMultiListUppercase.Add(name.ToUpperInvariant());
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
                throw new Exception("NameList: Unable to read name list file: " + fileNameOrUrl, ex);
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
                _namesMultiListParts = null;
                _namesListCaseInsensitive = null;
                _namesMultiListCaseInsensitive = null;

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
                _namesListCaseInsensitive = null;
                _namesMultiListCaseInsensitive = null;
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

            // This runs once per word of every line during spell check (and up to four times per
            // word from the OCR fix engine), so the common answer - "this word is not part of any
            // multi-word name" - must be a single lookup. Scanning all several hundred multi-word
            // names, with three string concatenations each, plus normalizing the whole line first,
            // was pure waste for every ordinary word.
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
        /// Each space-delimited part of every multi-word name, mapped to the names containing it -
        /// exactly the names the scan in <see cref="IsInNamesMultiWordList"/> could accept for that
        /// word. Built on first use and dropped whenever the multi-word list changes.
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

        // Case-insensitive views of the two name sets, built on first use and dropped whenever
        // the sets change (same lifetime as _namesMultiListParts). The lookup used to walk the
        // whole set with OrdinalIgnoreCase Equals per name candidate per line.
        private Dictionary<string, string> _namesListCaseInsensitive;
        private Dictionary<string, string> _namesMultiListCaseInsensitive;

        private static Dictionary<string, string> BuildCaseInsensitiveIndex(HashSet<string> names)
        {
            var index = new Dictionary<string, string>(names.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var n in names)
            {
                // First entry in set order wins, as the replaced foreach did.
                if (!index.ContainsKey(n))
                {
                    index.Add(n, n);
                }
            }

            return index;
        }

        public bool ContainsCaseInsensitive(string name, out string newName)
        {
            newName = null;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            Dictionary<string, string> index;
            if (name.IndexOf(' ') >= 0)
            {
                index = _namesMultiListCaseInsensitive ?? (_namesMultiListCaseInsensitive = BuildCaseInsensitiveIndex(_namesMultiList));
            }
            else
            {
                index = _namesListCaseInsensitive ?? (_namesListCaseInsensitive = BuildCaseInsensitiveIndex(_namesList));
            }

            if (index.TryGetValue(name, out var found))
            {
                newName = found;
                return true;
            }

            return false;
        }

        public static async Task<NameList> CreateAsync(string dictionaryFolder, string languageName, bool useOnlineNameList, string namesUrl)
        {
            return await Task.Run(() => new NameList(dictionaryFolder, languageName, useOnlineNameList, namesUrl));
        }
    }
}