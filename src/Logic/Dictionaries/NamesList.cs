using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.Dictionaries
{
    public class NamesList
    {

        //private string _dictionaryFolder;
        //private List<string> _namesList;
        //private List<string> _namesMultiList;

        //public NamesList(string dictionaryFolder, string languageName)
        //{
        //    _dictionaryFolder = dictionaryFolder;

        //    _namesList = new List<string>();
        //    _namesMultiList = new List<string>();

        //    LoadNamesList(Path.Combine(_dictionaryFolder, "names_etc.xml"), _namesList, _namesMultiList);
        //    LoadNamesList(Path.Combine(_dictionaryFolder, GetLocalNamesFileName(languageName)), _namesList, _namesMultiList);
        //    LoadNamesList(Path.Combine(_dictionaryFolder, GetLocalNamesUserFileName(languageName)), _namesList, _namesMultiList);
        //}

        //private string GetLocalNamesFileName(string languageName)
        //{
        //    if (languageName.Length == 2)
        //    {
        //        string[] files = Directory.GetFiles(_dictionaryFolder, languageName + "_??_names_etc.xml");
        //        if (files.Length > 0)
        //            return localNamesFileName = files[0];
        //    }
        //    return Path.Combine(_dictionaryFolder, languageName + "_names_etc.xml");
        //}

        //private string GetLocalNamesUserFileName(string languageName)
        //{
        //    if (languageName.Length == 2)
        //    {
        //        string[] files = Directory.GetFiles(_dictionaryFolder, languageName + "_??_names_etc_user.xml");
        //        if (files.Length > 0)
        //            return localNamesFileName = files[0];
        //    }
        //    return Path.Combine(_dictionaryFolder, languageName + "_names_etc_user.xml");
        //}

        //private void LoadNamesList(string fileName, List<string> _namesList, List<string> _namesMultiList)
        //{
        //    if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        //        return;

        //    var namesDoc = new XmlDocument();
        //    namesDoc.Load(fileName)
        //    if (namesDoc == null || namesDoc.DocumentElement == null)
        //        return;

        //    foreach (XmlNode node in namesDoc.DocumentElement.SelectNodes("name"))
        //    {
        //        string s = node.InnerText.Trim();
        //        if (s.Contains(' '))
        //        {
        //            if (!namesEtcMultiWordList.Contains(s))
        //                namesEtcMultiWordList.Add(s);
        //        }
        //        else
        //        {
        //            if (!namesEtcList.Contains(s))
        //                namesEtcList.Add(s);
        //        }
        //    }
        //}

        public static bool RemoveFromLocalNamesEtcList(string word, string languageName)
        {
            word = word.Trim();
            if (word.Length > 1)
            {
                var localNamesEtc = new List<string>();
                string userNamesEtcXmlFileName = LoadLocalNamesEtc(localNamesEtc, localNamesEtc, languageName);

                if (!localNamesEtc.Contains(word))
                    return false;
                localNamesEtc.Remove(word);
                localNamesEtc.Sort();

                var namesEtcDoc = new XmlDocument();
                if (File.Exists(userNamesEtcXmlFileName))
                    namesEtcDoc.Load(userNamesEtcXmlFileName);
                else
                    namesEtcDoc.LoadXml("<ignore_words />");

                XmlNode de = namesEtcDoc.DocumentElement;
                if (de != null)
                {
                    de.RemoveAll();
                    foreach (var name in localNamesEtc)
                    {
                        XmlNode node = namesEtcDoc.CreateElement("name");
                        node.InnerText = name;
                        de.AppendChild(node);
                    }
                    namesEtcDoc.Save(userNamesEtcXmlFileName);
                }
                return true;
            }
            return false;
        }

        public static bool AddWordToLocalNamesEtcList(string word, string languageName)
        {
            word = word.Trim();
            if (word.Length > 1)
            {
                var localNamesEtc = new List<string>();
                string userNamesEtcXmlFileName = LoadLocalNamesEtc(localNamesEtc, localNamesEtc, languageName);

                if (localNamesEtc.Contains(word))
                    return false;
                localNamesEtc.Add(word);
                localNamesEtc.Sort();

                var namesEtcDoc = new XmlDocument();
                if (File.Exists(userNamesEtcXmlFileName))
                    namesEtcDoc.Load(userNamesEtcXmlFileName);
                else
                    namesEtcDoc.LoadXml("<ignore_words />");

                XmlNode de = namesEtcDoc.DocumentElement;
                if (de != null)
                {
                    de.RemoveAll();
                    foreach (var name in localNamesEtc)
                    {
                        XmlNode node = namesEtcDoc.CreateElement("name");
                        node.InnerText = name;
                        de.AppendChild(node);
                    }
                    namesEtcDoc.Save(userNamesEtcXmlFileName);
                }
                return true;
            }
            return false;
        }

        public static string LoadNamesEtcWordLists(List<string> namesEtcList, List<string> namesEtcMultiWordList, string languageName)
        {
            namesEtcList.Clear();
            namesEtcMultiWordList.Clear();

            LoadGlobalNamesEtc(namesEtcList, namesEtcMultiWordList);

            string userNamesEtcXmlFileName = LoadLocalNamesEtc(namesEtcList, namesEtcMultiWordList, languageName);
            return userNamesEtcXmlFileName;
        }

        public static string LoadNamesEtcWordLists(HashSet<string> namesEtcList, HashSet<string> namesEtcMultiWordList, string languageName)
        {
            namesEtcList.Clear();
            namesEtcMultiWordList.Clear();

            LoadGlobalNamesEtc(namesEtcList, namesEtcMultiWordList);

            string userNamesEtcXmlFileName = LoadLocalNamesEtc(namesEtcList, namesEtcMultiWordList, languageName);
            return userNamesEtcXmlFileName;
        }

        public static void LoadGlobalNamesEtc(List<string> namesEtcList, List<string> namesEtcMultiWordList)
        {
            // Load names etc list (names/noise words)
            var namesEtcDoc = new XmlDocument();
            bool loaded = false;
            if (Configuration.Settings.WordLists.UseOnlineNamesEtc && !string.IsNullOrEmpty(Configuration.Settings.WordLists.NamesEtcUrl))
            {
                try
                {
                    var xml = Utilities.DownloadString(Configuration.Settings.WordLists.NamesEtcUrl);
                    namesEtcDoc.LoadXml(xml);
                    loaded = true;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
                }
            }
            if (!loaded && File.Exists(Utilities.DictionaryFolder + "names_etc.xml"))
            {
                namesEtcDoc.Load(Utilities.DictionaryFolder + "names_etc.xml");
            }
            if (namesEtcDoc.DocumentElement != null)
                foreach (XmlNode node in namesEtcDoc.DocumentElement.SelectNodes("name"))
                {
                    string s = node.InnerText.Trim();
                    if (s.Contains(' '))
                    {
                        if (!namesEtcMultiWordList.Contains(s))
                            namesEtcMultiWordList.Add(s);
                    }
                    else
                    {
                        if (!namesEtcList.Contains(s))
                            namesEtcList.Add(s);
                    }
                }
        }

        public static void LoadGlobalNamesEtc(HashSet<string> namesEtcList, HashSet<string> namesEtcMultiWordList)
        {
            // Load names etc list (names/noise words)
            var namesEtcDoc = new XmlDocument();
            bool loaded = false;
            if (Configuration.Settings.WordLists.UseOnlineNamesEtc && !string.IsNullOrEmpty(Configuration.Settings.WordLists.NamesEtcUrl))
            {
                try
                {
                    var xml = Utilities.DownloadString(Configuration.Settings.WordLists.NamesEtcUrl);
                    namesEtcDoc.LoadXml(xml);
                    loaded = true;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
                }
            }
            if (!loaded && File.Exists(Utilities.DictionaryFolder + "names_etc.xml"))
            {
                namesEtcDoc.Load(Utilities.DictionaryFolder + "names_etc.xml");
            }
            if (namesEtcDoc.DocumentElement != null)
                foreach (XmlNode node in namesEtcDoc.DocumentElement.SelectNodes("name"))
                {
                    string s = node.InnerText.Trim();
                    if (s.Contains(' '))
                    {
                        if (!namesEtcMultiWordList.Contains(s))
                            namesEtcMultiWordList.Add(s);
                    }
                    else
                    {
                        if (!namesEtcList.Contains(s))
                            namesEtcList.Add(s);
                    }
                }
        }

        public static string LoadLocalNamesEtc(List<string> namesEtcList, List<string> namesEtcMultiWordList, string languageName)
        {
            string userNamesEtcXmlFileName = Utilities.DictionaryFolder + languageName + "_names_etc.xml";
            if (languageName.Length == 2)
            {
                string[] files = Directory.GetFiles(Utilities.DictionaryFolder, languageName + "_??_names_etc.xml");
                if (files.Length > 0)
                    userNamesEtcXmlFileName = files[0];
            }

            if (File.Exists(userNamesEtcXmlFileName))
            {
                var namesEtcDoc = new XmlDocument();
                namesEtcDoc.Load(userNamesEtcXmlFileName);
                foreach (XmlNode node in namesEtcDoc.DocumentElement.SelectNodes("name"))
                {
                    string s = node.InnerText.Trim();
                    if (s.Contains(' '))
                    {
                        if (!namesEtcMultiWordList.Contains(s))
                            namesEtcMultiWordList.Add(s);
                    }
                    else
                    {
                        if (!namesEtcList.Contains(s))
                            namesEtcList.Add(s);
                    }
                }
            }
            return userNamesEtcXmlFileName;
        }

        public static string LoadLocalNamesEtc(HashSet<string> namesEtcList, HashSet<string> namesEtcMultiWordList, string languageName)
        {
            string userNamesEtcXmlFileName = Utilities.DictionaryFolder + languageName + "_names_etc.xml";
            if (languageName.Length == 2)
            {
                string[] files = Directory.GetFiles(Utilities.DictionaryFolder, languageName + "_??_names_etc.xml");
                if (files.Length > 0)
                    userNamesEtcXmlFileName = files[0];
            }

            if (File.Exists(userNamesEtcXmlFileName))
            {
                var namesEtcDoc = new XmlDocument();
                namesEtcDoc.Load(userNamesEtcXmlFileName);
                foreach (XmlNode node in namesEtcDoc.DocumentElement.SelectNodes("name"))
                {
                    string s = node.InnerText.Trim();
                    if (s.Contains(' '))
                    {
                        if (!namesEtcMultiWordList.Contains(s))
                            namesEtcMultiWordList.Add(s);
                    }
                    else
                    {
                        if (!namesEtcList.Contains(s))
                            namesEtcList.Add(s);
                    }
                }
            }
            return userNamesEtcXmlFileName;
        }

        public static bool IsInNamesEtcMultiWordList(List<string> namesEtcMultiWordList, string line, string word)
        {
            string text = line.Replace(Environment.NewLine, " ");
            text = text.Replace("  ", " ");

            foreach (string s in namesEtcMultiWordList)
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

        public static bool IsInNamesEtcMultiWordList(HashSet<string> namesEtcMultiWordList, string line, string word)
        {
            string text = line.Replace(Environment.NewLine, " ");
            text = text.Replace("  ", " ");

            foreach (string s in namesEtcMultiWordList)
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
