using Nikse.SubtitleEdit.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic
{
    // controller
    public class GitHubProvider : DictionaryProvider
    {
        private readonly string ResourceFile;

        public GitHubProvider(string resourceFile)
        {
            ResourceFile = resourceFile;
            Name = "Github";
            LoadDictionary();
        }

        protected override void LoadDictionary()
        {
            Dictionaries = new List<DictionaryInfo>();
            Assembly asm = Assembly.GetExecutingAssembly();
            var strm = asm.GetManifestResourceStream(ResourceFile);

            if (strm == null)
            {
                throw new NullReferenceException("Resource file stream");
            }

            var doc = new XmlDocument();
            using (var rdr = new StreamReader(strm))
            using (var zip = new GZipStream(rdr.BaseStream, CompressionMode.Decompress))
            using (var reader = XmlReader.Create(zip, new XmlReaderSettings { IgnoreProcessingInstructions = true }))
            {
                doc.Load(reader);
            }
            foreach (XmlNode node in doc.DocumentElement.SelectNodes("Dictionary"))
            {
                var downloadLinks = new List<Uri>(2);

                bool skipDictionary = false;
                foreach (XmlNode linksNode in node.SelectSingleNode("Links").SelectNodes("Link"))
                {
                    // skip dictionary (invalid url)
                    if (!Uri.TryCreate(linksNode.InnerText, UriKind.Absolute, out Uri downloadLink))
                    {
                        skipDictionary = true;
                        break;
                    }
                    downloadLinks.Add(downloadLink);
                }

                // one or more download links is/are invalid
                if (skipDictionary)
                {
                    continue;
                }

                string englishName = node.SelectSingleNode("EnglishName").InnerText;
                string nativeName = node.SelectSingleNode("NativeName")?.InnerText;
                string description = node.SelectSingleNode("Description").InnerText;
                string name = englishName;
                if (!string.IsNullOrEmpty(nativeName))
                {
                    name += " - " + nativeName;
                }
                var dicInfo = new DictionaryInfo
                {
                    EnglishName = englishName,
                    NativeName = nativeName,
                    Description = description,
                    DownloadLinks = downloadLinks
                };
                Dictionaries.Add(dicInfo);
            }
        }

        public void ProccessDownloadedData(string dictionaryFolder, byte[] data)
        {

        }
    }
}
