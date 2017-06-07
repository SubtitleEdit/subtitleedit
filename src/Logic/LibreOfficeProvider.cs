using Nikse.SubtitleEdit.Core;
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

        public void ProccessDownloadedData(string dictionaryFolder, byte[] data)
        {
            // TODO: Only extract if provider is Open Office
            using (var ms = new MemoryStream(data))
            using (ZipExtractor zip = ZipExtractor.Open(ms))
            {
                List<ZipExtractor.ZipFileEntry> dir = zip.ReadCentralDir();
                // Extract dic/aff files in dictionary folder
                bool found = false;
                ExtractDic(dictionaryFolder, zip, dir, ref found);

                if (!found) // check zip inside zip
                {
                    foreach (ZipExtractor.ZipFileEntry entry in dir)
                    {
                        if (entry.FilenameInZip.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var innerMs = new MemoryStream())
                            {
                                zip.ExtractFile(entry, innerMs);
                                ZipExtractor innerZip = ZipExtractor.Open(innerMs);
                                List<ZipExtractor.ZipFileEntry> innerDir = innerZip.ReadCentralDir();
                                ExtractDic(dictionaryFolder, innerZip, innerDir, ref found);
                            }
                        }
                    }
                }
            }
        }

        private static void ExtractDic(string dictionaryFolder, ZipExtractor zip, List<ZipExtractor.ZipFileEntry> dir, ref bool found)
        {
            foreach (ZipExtractor.ZipFileEntry entry in dir)
            {
                if (entry.FilenameInZip.EndsWith(".dic", StringComparison.OrdinalIgnoreCase) || entry.FilenameInZip.EndsWith(".aff", StringComparison.OrdinalIgnoreCase))
                {
                    string fileName = Path.GetFileName(entry.FilenameInZip);

                    // French fix
                    if (fileName.StartsWith("fr-moderne", StringComparison.Ordinal))
                        fileName = fileName.Replace("fr-moderne", "fr_FR");

                    // German fix
                    if (fileName.StartsWith("de_DE_frami", StringComparison.Ordinal))
                        fileName = fileName.Replace("de_DE_frami", "de_DE");

                    // Russian fix
                    if (fileName.StartsWith("russian-aot", StringComparison.Ordinal))
                        fileName = fileName.Replace("russian-aot", "ru_RU");

                    string path = Path.Combine(dictionaryFolder, fileName);
                    zip.ExtractFile(entry, path);

                    found = true;
                }
            }
        }
    }
}
