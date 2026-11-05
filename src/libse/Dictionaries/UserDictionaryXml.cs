using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Dictionaries
{
    /// <summary>
    /// Reads and writes the two word lists of a "&lt;lang&gt;_user.xml" / "&lt;lang&gt;_se.xml" file:
    /// the words the spell checker should accept, and the words it should reject even when the
    /// Hunspell dictionary knows them (e.g. Dutch "weetje", which hides the OCR error "weet je").
    /// <code>
    /// &lt;words&gt;
    ///   &lt;word&gt;accepted&lt;/word&gt;
    ///   &lt;removed&gt;
    ///     &lt;word&gt;weetje&lt;/word&gt;
    ///   &lt;/removed&gt;
    /// &lt;/words&gt;
    /// </code>
    /// The removed words sit one level down, so older versions reading "word" children of the
    /// root never see them as accepted words.
    /// </summary>
    public static class UserDictionaryXml
    {
        public const string RemovedElementName = "removed";

        public static List<string> ReadWords(XmlDocument doc)
        {
            return ReadNormalizedWords(doc.DocumentElement);
        }

        public static List<string> ReadRemovedWords(XmlDocument doc)
        {
            return ReadNormalizedWords(doc.DocumentElement?.SelectSingleNode(RemovedElementName));
        }

        /// <summary>
        /// Replaces the accepted words and keeps everything else in the file (the removed words).
        /// </summary>
        public static void WriteWords(XmlDocument doc, List<string> words)
        {
            var root = doc.DocumentElement;
            if (root == null)
            {
                return;
            }

            RemoveWordNodes(root);
            words.Sort();
            var anchor = root.FirstChild;
            foreach (var word in words)
            {
                XmlNode node = doc.CreateElement("word");
                node.InnerText = word;
                if (anchor == null)
                {
                    root.AppendChild(node);
                }
                else
                {
                    root.InsertBefore(node, anchor);
                }
            }
        }

        public static void WriteRemovedWords(XmlDocument doc, List<string> words)
        {
            var root = doc.DocumentElement;
            if (root == null)
            {
                return;
            }

            var removedNode = root.SelectSingleNode(RemovedElementName);
            if (removedNode == null)
            {
                if (words.Count == 0)
                {
                    return;
                }

                removedNode = doc.CreateElement(RemovedElementName);
                root.AppendChild(removedNode);
            }

            RemoveWordNodes(removedNode);
            if (words.Count == 0)
            {
                root.RemoveChild(removedNode);
                return;
            }

            words.Sort();
            foreach (var word in words)
            {
                XmlNode node = doc.CreateElement("word");
                node.InnerText = word;
                removedNode.AppendChild(node);
            }
        }

        /// <summary>
        /// Adding a word to one list takes it out of the other, so a word is never both
        /// accepted and rejected by the same file.
        /// </summary>
        public static void RemoveFromRemovedWords(XmlDocument doc, string normalizedWord)
        {
            var removed = ReadRemovedWords(doc);
            if (removed.Remove(normalizedWord))
            {
                WriteRemovedWords(doc, removed);
            }
        }

        private static List<string> ReadNormalizedWords(XmlNode parent)
        {
            var list = new List<string>();
            var nodes = parent?.SelectNodes("word");
            if (nodes == null)
            {
                return list;
            }

            foreach (XmlNode node in nodes)
            {
                var word = Utilities.NormalizeUserDictionaryWord(node.InnerText);
                if (word.Length > 0 && !list.Contains(word))
                {
                    list.Add(word);
                }
            }

            return list;
        }

        private static void RemoveWordNodes(XmlNode parent)
        {
            var nodes = parent.SelectNodes("word");
            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode node in nodes)
            {
                parent.RemoveChild(node);
            }
        }
    }
}
