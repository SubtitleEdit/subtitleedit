using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SetupSupport.WordLists
{
    internal class SpellCheckList
    {
        private readonly string _cultureName;
        private readonly string _copyName;
        private readonly string _fileName;

        public SpellCheckList(FileInfo file)
        {
            _fileName = file.FullName;
            _copyName = Path.ChangeExtension(_fileName, "se@inst.xml");
            _cultureName = Regex.Replace(file.Name, @"\A([^_]+)_([^_]+)_user.xml\z", "${1}-${2}", RegexOptions.CultureInvariant);
        }

        public bool CanUpdate()
        {
            var document = new XmlDocument { XmlResolver = null };
            document.Load(_fileName);
            var root = document.DocumentElement;
            if (root.Name.Equals("words", StringComparison.Ordinal))
            {
                if (root.GetAttribute("Update").Trim().Equals("Never", StringComparison.OrdinalIgnoreCase) ||
                    root.GetAttribute("DoNotUpdate").Trim().Equals("True", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                document.Save(_copyName);
            }
            return true;
        }

        public bool Merge()
        {
            if (File.Exists(_copyName))
            {
                try
                {
                    var document = new XmlDocument { XmlResolver = null };
                    document.Load(_copyName);
                    var root = document.DocumentElement;

                    var update = root.GetAttribute("Update").Trim().ToUpperInvariant();
                    if (update.Equals("REPLACE", StringComparison.Ordinal) || update.Equals("OVERWRITE", StringComparison.Ordinal))
                    {
                        update = update[0] + update.Substring(1).ToLowerInvariant();
                        document.Load(_fileName);
                        document.DocumentElement.SetAttribute("Update", update);
                        document.Save(_fileName);

                        return true;
                    }

                    var phrases = new HashSet<string>();
                    foreach (XmlNode node in root.SelectNodes("word"))
                    {
                        var text = node.InnerText.Trim().ToLowerInvariant();
                        if (text.Length > 0)
                        {
                             phrases.Add(text);
                        }
                    }
                    root.RemoveAll();

                    document.Load(_fileName);
                    root = document.DocumentElement;
                    foreach (XmlNode node in root.SelectNodes("word"))
                    {
                        var text = node.InnerText.Trim().ToLowerInvariant();
                        if (text.Length > 0)
                        {
                             phrases.Add(text);
                        }
                    }
                    root.RemoveAll();

                    StringComparer comparer;
                    try
                    {
                        comparer = StringComparer.Create(CultureInfo.GetCultureInfo(_cultureName), false);
                    }
                    catch
                    {
                        comparer = StringComparer.InvariantCulture;
                    }

                    foreach (var text in phrases.OrderBy(p => p, comparer).ThenBy(p => p, StringComparer.Ordinal))
                    {
                        var node = document.CreateElement("word");
                        node.InnerText = text;
                        root.AppendChild(node);
                    }
                    root.SetAttribute("Update", "Merge");
                    document.Save(_fileName);
                }
                finally
                {
                    File.Delete(_copyName);
                }
            }
            return true;
        }

    }
}
