using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Dictionaries
{
    /// <summary>
    /// Language specific abbreviations like "dhr." or "enz.", used to tell an abbreviation period
    /// apart from a sentence ending so the following word is not capitalized (issue #13082).
    ///
    /// Read from "&lt;two-letter-iso&gt;_abbreviations.xml" in the dictionary folder, e.g.
    /// "nl_abbreviations.xml", plus an optional region specific file like
    /// "pt_BR_abbreviations.xml". Names ending with a period (e.g. "Dr." in names.xml) are a
    /// second source and are added by the names list itself.
    /// </summary>
    public static class AbbreviationList
    {
        /// <summary>
        /// Returns a case insensitive set of abbreviations (each including the trailing period)
        /// for the given language. Never null; an empty set when there is no list for it.
        /// </summary>
        public static HashSet<string> Load(string dictionaryFolder, string languageName)
        {
            // Case insensitive: subtitles use both "Dr." and "dr.", and it is an abbreviation
            // either way - so a single entry per abbreviation is enough in the xml files.
            var abbreviations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(dictionaryFolder) || string.IsNullOrEmpty(languageName))
            {
                return abbreviations;
            }

            // Converts e.g. nl_NL => nl (neutral culture).
            var twoLetterIsoLanguageName = languageName.Length > 2 ? languageName.Substring(0, 2) : languageName;
            AddFromFile(abbreviations, Path.Combine(dictionaryFolder, twoLetterIsoLanguageName + "_abbreviations.xml"));
            if (languageName.Length > 2)
            {
                AddFromFile(abbreviations, Path.Combine(dictionaryFolder, languageName + "_abbreviations.xml"));
            }

            return abbreviations;
        }

        private static void AddFromFile(HashSet<string> abbreviations, string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            try
            {
                var doc = new XmlDocument { XmlResolver = null };
                doc.Load(fileName);
                var nodes = doc.DocumentElement?.SelectNodes("Item");
                if (nodes == null)
                {
                    return;
                }

                foreach (XmlNode node in nodes)
                {
                    var abbreviation = node.InnerText.Trim();
                    if (abbreviation.Length > 1 && abbreviation.EndsWith('.'))
                    {
                        abbreviations.Add(abbreviation);
                    }
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("AbbreviationList: Unable to read " + fileName + ": " + exception.Message);
            }
        }
    }
}
