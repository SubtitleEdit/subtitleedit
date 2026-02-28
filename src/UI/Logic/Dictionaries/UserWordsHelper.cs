using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.Dictionaries;
public static class UserWordsHelper
{
    public static string LoadUserWordList(List<string> userWordList, string languageName)
    {
        var folder = Se.DictionariesFolder;

        userWordList.Clear();
        var userWordDictionary = new XmlDocument();
        var userWordListXmlFileName = Path.Combine(folder, languageName + "_user.xml");

        if (!File.Exists(userWordListXmlFileName) && languageName != null && languageName.Length > 2)
        {
            var newFileName = Path.Combine(folder, languageName.Substring(0, 2).ToLowerInvariant() + "_user.xml");
            if (File.Exists(newFileName))
            {
                userWordListXmlFileName = newFileName;
            }
        }

        if (File.Exists(userWordListXmlFileName))
        {
            userWordDictionary.Load(userWordListXmlFileName);
            if (userWordDictionary != null && userWordDictionary.DocumentElement != null)
            {
                var nodes = userWordDictionary.DocumentElement.SelectNodes("word");
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        string s = node.InnerText.ToLowerInvariant();
                        if (!userWordList.Contains(s))
                        {
                            userWordList.Add(s);
                        }
                    }
                }
            }
        }

        return userWordListXmlFileName;
    }

    public static bool AddToUserDictionary(string word, string languageName)
    {
        var words = new List<string>();
        var userWordFileName = LoadUserWordList(words, languageName);
        if (words.Contains(word))
        {
            return false;
        }

        words.Add(word);

        bool flowControl = SaveWords(words, userWordFileName);
        if (!flowControl)
        {
            return false;
        }

        return true;
    }

    public static bool RemoveWord(string word, string languageName)
    {
        var words = new List<string>();
        var userWordFileName = LoadUserWordList(words, languageName);
        if (!words.Contains(word))
        {
            return false;
        }

        words.Remove(word);

        bool flowControl = SaveWords(words, userWordFileName);
        if (!flowControl)
        {
            return false;
        }

        return true;
    }

    private static bool SaveWords(List<string> words, string userWordFileName)
    {
        words.Sort();
        var doc = new XmlDocument();
        if (File.Exists(userWordFileName))
        {
            doc.Load(userWordFileName);
        }
        else
        {
            doc.LoadXml("<Words />");
        }

        if (doc.DocumentElement == null)
        {
            return false;
        }

        doc.DocumentElement.RemoveAll();
        foreach (var w in words)
        {
            XmlNode node = doc.CreateElement("word");
            node.InnerText = w;
            doc.DocumentElement?.AppendChild(node);
        }
        doc.Save(userWordFileName);

        return true;
    }
}
