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

        public static InterjectionsLists LoadInterjections(string twoLetterIsoLanguageName)
        {
            var seFileName = twoLetterIsoLanguageName + SeFileName;
            var userFileName = twoLetterIsoLanguageName + UserFileName;
            var interjections = new List<string>();
            var se = LoadInterjections(seFileName, out _, out var skipIfStartsWithList);
            var user = LoadInterjections(userFileName, out var ignoreList, out var skipIfStartsWithList2);

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

            skipIfStartsWithList.AddRange(skipIfStartsWithList2);
            return new InterjectionsLists
            {
                Interjections = interjections.OrderBy(p => p).ToList(),
                SkipIfStartsWith = skipIfStartsWithList.OrderByDescending(p=>p.Length).ToList(),
            };
        }

        public static void SaveInterjections(string twoLetterIsoLanguageName, List<string> interjections, List<string> skipList)
        {
            var seFileName = twoLetterIsoLanguageName + SeFileName;
            var userFileName = twoLetterIsoLanguageName + UserFileName;
            var se = LoadInterjections(seFileName, out _, out _);
            var ignoreList = new List<string>();

            foreach (var w in se)
            {
                if (!interjections.Contains(w) && !ignoreList.Contains(w))
                {
                    ignoreList.Add(w);
                }
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<interjections><ignore></ignore><skipIfStartsWith></skipIfStartsWith></interjections>");
          
            var ignoreNode = xmlDocument.DocumentElement.SelectSingleNode("ignore");
            foreach (var w in ignoreList)
            {
                var node = xmlDocument.CreateElement( "word");
                node.InnerText = w;
                ignoreNode.AppendChild(node);
            }

            var skipNode = xmlDocument.DocumentElement.SelectSingleNode("skipIfStartsWith");
            foreach (var w in skipList)
            {
                var node = xmlDocument.CreateElement("text");
                node.InnerText = w;
                skipNode.AppendChild(node);
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

            if (!Directory.Exists(Configuration.DictionariesDirectory))
            {
                Directory.CreateDirectory(Configuration.DictionariesDirectory);
            }

            var fullFileName = Path.Combine(Configuration.DictionariesDirectory, userFileName);
            xmlDocument.Save(fullFileName);
        }

        private static List<string> LoadInterjections(string fileName, out List<string> ignoreList, out List<string> skipIfStartsWithList)
        {
            ignoreList = new List<string>();
            skipIfStartsWithList = new List<string>();
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

                foreach (XmlNode node in xmlDocument.DocumentElement.SelectNodes("skipIfStartsWith/text"))
                {
                    var w = node.InnerText.Trim();
                    if (!string.IsNullOrEmpty(w) && !skipIfStartsWithList.Contains(w))
                    {
                        skipIfStartsWithList.Add(w);
                    }
                }
            }

            return interjections;
        }
    }
}
