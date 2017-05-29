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
    public class LibreOfficeProvider : DictionaryProvider
    {
        private readonly string ResourceFile;

        public LibreOfficeProvider(string resourceFile)
        {
            if (string.IsNullOrEmpty(resourceFile))
            {
                resourceFile = "Nikse.SubtitleEdit.Resources.HunspellDictionaries.xml.gz";
            }
            ResourceFile = resourceFile;
            Name = "Libre Office";
            LoadDictionary();
        }

        protected override void LoadDictionary()
        {
            Dictionaries = new List<DictionaryInfo>();
            Assembly asm = Assembly.GetExecutingAssembly();
            var strm = asm.GetManifestResourceStream(ResourceFile);

            if (strm == null)
            {
                throw new NullReferenceException("Resouce file stream");
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
                // skip dictionary (invalid url)
                if (!Uri.TryCreate(node.SelectSingleNode("DownloadLink").InnerText, UriKind.Absolute, out Uri downloadLink))
                {
                    continue;
                }
                string englishName = node.SelectSingleNode("EnglishName").InnerText;
                string nativeName = node.SelectSingleNode("NativeName").InnerText;
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
                    DownloadLinks = new List<Uri>() { downloadLink }
                };
                Dictionaries.Add(dicInfo);
            }

        }
    }
}
