using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Dictionaries
{
    /// <summary>
    /// Reads and writes the "do not break after" lists. The shipped list,
    /// &lt;lang&gt;_NoBreakAfterList.xml, is overwritten on every app upgrade when the bundled
    /// dictionaries are unpacked again, so user edits live in a separate overlay,
    /// &lt;lang&gt;_NoBreakAfterList_User.xml: &lt;Item&gt; entries are added to the shipped list and
    /// &lt;Removed&gt; entries take shipped items out of it (#16375).
    /// </summary>
    public static class NoBreakAfterListFile
    {
        public const string FileNameSuffix = "_NoBreakAfterList.xml";
        public const string UserFileNameSuffix = "_NoBreakAfterList_User.xml";

        private const string RootElement = "NoBreakAfterList";
        private const string ItemElement = "Item";
        private const string RemovedElement = "Removed";
        private const string RegExAttribute = "RegEx";

        public sealed class Entry : IEquatable<Entry>
        {
            public string Text { get; }
            public bool IsRegex { get; }

            public Entry(string text, bool isRegex)
            {
                Text = text;
                IsRegex = isRegex;
            }

            public bool Equals(Entry other)
            {
                return other != null && IsRegex == other.IsRegex && string.Equals(Text, other.Text, StringComparison.Ordinal);
            }

            public override bool Equals(object obj) => Equals(obj as Entry);

            public override int GetHashCode() => (Text ?? string.Empty).GetHashCode() * 2 + (IsRegex ? 1 : 0);

            public override string ToString() => Text;
        }

        /// <summary>
        /// The overlay file name for a shipped list file name ("en_NoBreakAfterList.xml" gives
        /// "en_NoBreakAfterList_User.xml").
        /// </summary>
        public static string GetUserFileName(string fileName)
        {
            if (fileName.EndsWith(FileNameSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - FileNameSuffix.Length) + UserFileNameSuffix;
            }

            return Path.ChangeExtension(fileName, null) + "_User.xml";
        }

        /// <summary>
        /// The items of one list file (the &lt;Item&gt; entries only). A missing file is empty.
        /// </summary>
        public static List<Entry> LoadItems(string fileName)
        {
            return Load(fileName, ItemElement);
        }

        /// <summary>
        /// The effective list: the shipped items minus the user's removed items, plus the user's
        /// added items, in that order and without duplicates.
        /// </summary>
        public static List<Entry> LoadMerged(string fileName)
        {
            var userFileName = GetUserFileName(fileName);
            var removed = new HashSet<Entry>(Load(userFileName, RemovedElement));
            var result = new List<Entry>();
            var seen = new HashSet<Entry>();
            foreach (var entry in LoadItems(fileName).Concat(LoadItems(userFileName)))
            {
                if (!removed.Contains(entry) && seen.Add(entry))
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        /// <summary>
        /// Saves <paramref name="wanted"/> as the effective list for <paramref name="fileName"/> by
        /// writing only the difference against the shipped file to the overlay file, so the next
        /// upgrade can replace the shipped file without losing the edits. With no difference the
        /// overlay file is deleted.
        /// </summary>
        public static void SaveUserChanges(string fileName, IEnumerable<Entry> wanted)
        {
            var shipped = LoadItems(fileName);
            var shippedSet = new HashSet<Entry>(shipped);
            var wantedList = wanted.Distinct().ToList();
            var wantedSet = new HashSet<Entry>(wantedList);

            var added = wantedList.Where(p => !shippedSet.Contains(p)).ToList();
            var removed = shipped.Where(p => !wantedSet.Contains(p)).Distinct().ToList();
            WriteUserFile(GetUserFileName(fileName), added, removed);
        }

        /// <summary>
        /// Writes a plain list file (used for a language the user adds, which has no shipped file).
        /// </summary>
        public static void SaveItems(string fileName, IEnumerable<Entry> items)
        {
            var doc = new XmlDocument();
            doc.LoadXml("<" + RootElement + " />");
            AppendEntries(doc, ItemElement, items);
            doc.Save(fileName);
        }

        /// <summary>
        /// Before the overlay file existed, the editor wrote straight into the shipped file, and the
        /// next upgrade overwrote those edits. Call this after the shipped file was replaced, with
        /// the items it had before: items that the old file had and the new one lacks are kept as
        /// user additions. Removals can't be told apart from upstream changes and are not carried over.
        /// </summary>
        public static void PreserveAddedItems(string fileName, IEnumerable<Entry> previousItems)
        {
            var userFileName = GetUserFileName(fileName);
            var shipped = new HashSet<Entry>(LoadItems(fileName));
            var userItems = LoadItems(userFileName);
            var userRemoved = Load(userFileName, RemovedElement);
            var known = new HashSet<Entry>(userItems.Concat(userRemoved));

            var missing = previousItems.Distinct().Where(p => !shipped.Contains(p) && !known.Contains(p)).ToList();
            if (missing.Count == 0)
            {
                return;
            }

            WriteUserFile(userFileName, userItems.Concat(missing).ToList(), userRemoved);
        }

        private static void WriteUserFile(string userFileName, List<Entry> added, List<Entry> removed)
        {
            if (added.Count == 0 && removed.Count == 0)
            {
                if (File.Exists(userFileName))
                {
                    File.Delete(userFileName);
                }

                return;
            }

            var doc = new XmlDocument();
            doc.LoadXml("<" + RootElement + " />");
            AppendEntries(doc, ItemElement, added);
            AppendEntries(doc, RemovedElement, removed);
            doc.Save(userFileName);
        }

        private static void AppendEntries(XmlDocument doc, string elementName, IEnumerable<Entry> entries)
        {
            foreach (var entry in entries)
            {
                XmlNode node = doc.CreateElement(elementName);
                node.InnerText = entry.Text;
                if (entry.IsRegex)
                {
                    var attribute = doc.CreateAttribute(RegExAttribute);
                    attribute.InnerText = true.ToString();
                    node.Attributes.Append(attribute);
                }

                doc.DocumentElement.AppendChild(node);
            }
        }

        private static List<Entry> Load(string fileName, string elementName)
        {
            var entries = new List<Entry>();
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return entries;
            }

            var doc = new XmlDocument();
            doc.Load(fileName);
            var nodes = doc.DocumentElement?.SelectNodes(elementName);
            if (nodes == null)
            {
                return entries;
            }

            foreach (XmlNode node in nodes)
            {
                if (string.IsNullOrEmpty(node.InnerText))
                {
                    continue;
                }

                var isRegex = node.Attributes?[RegExAttribute] != null &&
                              node.Attributes[RegExAttribute].InnerText.Equals("true", StringComparison.OrdinalIgnoreCase);
                entries.Add(new Entry(node.InnerText, isRegex));
            }

            return entries;
        }
    }
}
