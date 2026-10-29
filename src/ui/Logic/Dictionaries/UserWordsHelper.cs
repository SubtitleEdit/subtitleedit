using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.Dictionaries;

public static class UserWordsHelper
{
    public static string LoadUserWordList(List<string> userWordList, string languageName)
    {
        userWordList.Clear();
        var userWordListXmlFileName = GetUserWordListFileName(languageName);
        var doc = LoadDocument(userWordListXmlFileName);
        if (doc != null)
        {
            userWordList.AddRange(UserDictionaryXml.ReadWords(doc));
        }

        return userWordListXmlFileName;
    }

    /// <summary>
    /// Words the spell checker must reject even though the Hunspell dictionary knows them, from
    /// the &lt;removed&gt; section of the same "&lt;lang&gt;_user.xml" file.
    /// </summary>
    public static string LoadRemovedWordList(List<string> removedWordList, string languageName)
    {
        removedWordList.Clear();
        var userWordListXmlFileName = GetUserWordListFileName(languageName);
        var doc = LoadDocument(userWordListXmlFileName);
        if (doc != null)
        {
            removedWordList.AddRange(UserDictionaryXml.ReadRemovedWords(doc));
        }

        return userWordListXmlFileName;
    }

    public static bool AddToUserDictionary(string word, string languageName)
    {
        if (string.IsNullOrEmpty(word))
        {
            return false;
        }

        word = Utilities.NormalizeUserDictionaryWord(word);
        if (word.Length == 0)
        {
            return false;
        }

        var words = new List<string>();
        var userWordFileName = LoadUserWordList(words, languageName);
        if (words.Contains(word))
        {
            return false;
        }

        words.Add(word);
        return Save(userWordFileName, doc =>
        {
            UserDictionaryXml.WriteWords(doc, words);
            UserDictionaryXml.RemoveFromRemovedWords(doc, word);
        });
    }

    public static bool RemoveWord(string word, string languageName)
    {
        word = Utilities.NormalizeUserDictionaryWord(word);
        if (word.Length == 0)
        {
            return false;
        }

        var words = new List<string>();
        var userWordFileName = LoadUserWordList(words, languageName);
        if (!words.Remove(word))
        {
            return false;
        }

        return Save(userWordFileName, doc => UserDictionaryXml.WriteWords(doc, words));
    }

    /// <summary>
    /// Makes the spell checker reject <paramref name="word"/>; also takes it out of the accepted
    /// user words, which would otherwise win.
    /// </summary>
    public static bool AddRemovedWord(string word, string languageName)
    {
        word = Utilities.NormalizeUserDictionaryWord(word);
        if (word.Length == 0 || word.Contains(' '))
        {
            return false;
        }

        var removedWords = new List<string>();
        var userWordFileName = LoadRemovedWordList(removedWords, languageName);
        if (removedWords.Contains(word))
        {
            return false;
        }

        removedWords.Add(word);
        var words = new List<string>();
        LoadUserWordList(words, languageName);
        var wasUserWord = words.Remove(word);
        return Save(userWordFileName, doc =>
        {
            UserDictionaryXml.WriteRemovedWords(doc, removedWords);
            if (wasUserWord)
            {
                UserDictionaryXml.WriteWords(doc, words);
            }
        });
    }

    public static bool RemoveRemovedWord(string word, string languageName)
    {
        word = Utilities.NormalizeUserDictionaryWord(word);
        if (word.Length == 0)
        {
            return false;
        }

        var removedWords = new List<string>();
        var userWordFileName = LoadRemovedWordList(removedWords, languageName);
        if (!removedWords.Remove(word))
        {
            return false;
        }

        return Save(userWordFileName, doc => UserDictionaryXml.WriteRemovedWords(doc, removedWords));
    }

    private static string GetUserWordListFileName(string languageName)
    {
        var folder = Se.DictionariesFolder;
        var userWordListXmlFileName = Path.Combine(folder, languageName + "_user.xml");
        if (!File.Exists(userWordListXmlFileName) && languageName != null && languageName.Length > 2)
        {
            var newFileName = Path.Combine(folder, languageName.Substring(0, 2).ToLowerInvariant() + "_user.xml");
            if (File.Exists(newFileName))
            {
                userWordListXmlFileName = newFileName;
            }
        }

        return userWordListXmlFileName;
    }

    private static XmlDocument? LoadDocument(string fileName)
    {
        if (!File.Exists(fileName))
        {
            return null;
        }

        var doc = new XmlDocument();
        doc.Load(fileName);
        return doc.DocumentElement == null ? null : doc;
    }

    private static bool Save(string userWordFileName, System.Action<XmlDocument> update)
    {
        var doc = LoadDocument(userWordFileName);
        if (doc == null)
        {
            doc = new XmlDocument();
            doc.LoadXml("<Words />");
        }

        update(doc);
        doc.Save(userWordFileName);
        return true;
    }
}
