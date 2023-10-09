using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class InterjectionsRepository
    {

        private const string UserFileName = "_interjections_user.xml";
        private const string SeFileName = "_interjections_se.xml";

        public static List<string> LoadInterjections(string twoLetterIsoLanguageName)
        {
            var seFileName = twoLetterIsoLanguageName + SeFileName;
            var userFileName = twoLetterIsoLanguageName + UserFileName;
            var interjections = new List<string>();
            var se = LoadInterjections(seFileName, out _);
            var user = LoadInterjections(userFileName, out var ignoreList);

            foreach (var w in se)
            {
                if (!interjections.Contains(w) && !ignoreList.Contains(w))
                {
                    interjections.Add(w);
                }
            }

            foreach (var w in user)
            {
                if (!interjections.Contains(w) && !ignoreList.Contains(w))
                {
                    interjections.Add(w);
                }
            }

            return interjections.OrderBy(p => p).ToList();
        }

        public static void SaveInterjections(string twoLetterIsoLanguageName, List<string> interjections)
        {
            var seFileName = twoLetterIsoLanguageName + SeFileName;
            var userFileName = twoLetterIsoLanguageName + UserFileName;
            var se = LoadInterjections(seFileName, out _);
            var ignoreList = new List<string>();

            foreach (var w in se)
            {
                if (!interjections.Contains(w) && !ignoreList.Contains(w))
                {
                    ignoreList.Add(w);
                }
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<interjections><ignore></ignore></interjections>");
            var ignoreNode = xmlDocument.DocumentElement.SelectSingleNode("ignore");
            foreach (var w in ignoreList)
            {
                var node = xmlDocument.CreateElement( "word");
                node.InnerText = w;
                ignoreNode.AppendChild(node);
            }

            foreach (var w in interjections)
            {
                if (!se.Contains(w))
                {
                    var node = xmlDocument.CreateElement("word");
                    node.InnerText = w;
                    xmlDocument.DocumentElement.AppendChild(node);
                }
            }

            var fullFileName = Path.Combine(Configuration.DictionariesDirectory, userFileName);
            xmlDocument.Save(fullFileName);
        }

        private static List<string> LoadInterjections(string fileName, out List<string> ignoreList)
        {
            ignoreList = new List<string>();
            var interjections = new List<string>();
            var fullFileName = Path.Combine(Configuration.DictionariesDirectory, fileName);
            if (File.Exists(fullFileName))
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(fullFileName);
                foreach (XmlNode node in xmlDocument.DocumentElement.SelectNodes("word"))
                {
                    var w = node.InnerText.Trim();
                    if (!string.IsNullOrEmpty(w) && !interjections.Contains(w))
                    {
                        interjections.Add(w);
                    }
                }

                foreach (XmlNode node in xmlDocument.DocumentElement.SelectNodes("ignore/word"))
                {
                    var w = node.InnerText.Trim();
                    if (!string.IsNullOrEmpty(w) && !ignoreList.Contains(w))
                    {
                        ignoreList.Add(w);
                    }
                }

            }

            return interjections;
        }
    }
}
