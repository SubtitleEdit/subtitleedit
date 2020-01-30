using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Nikse.SetupSupport.WordLists
{
    internal class NoBreakAfterList
    {
        private class NoBreakAfterItem : IEquatable<NoBreakAfterItem>
        {
            private readonly int _hashCode;

            public bool IsRegex { get; }
            public string Text { get; }

            private NoBreakAfterItem(bool isRegex, string text)
            {
                Text = text;
                IsRegex = isRegex;
                _hashCode = Tuple.Create(isRegex, text).GetHashCode();
            }

            public static NoBreakAfterItem FromNode(XmlElement node)
            {
                if (node != null)
                {
                    var text = node.InnerText;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var isRegex = node.GetAttribute("IsRegex").Trim().Equals("True", StringComparison.OrdinalIgnoreCase);
                        return new NoBreakAfterItem(isRegex, text);
                    }
                }
                return null;
            }

            public bool Equals(NoBreakAfterItem other)
            {
                return !ReferenceEquals(other, null) && IsRegex.Equals(other.IsRegex) && Text.Equals(other.Text, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as NoBreakAfterItem);
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }

        private readonly string _copyName;
        private readonly string _fileName;

        public NoBreakAfterList(FileInfo file)
        {
            _fileName = file.FullName;
            _copyName = Path.ChangeExtension(_fileName, "se@inst.xml");
        }

        public bool CanUpdate()
        {
            var document = new XmlDocument { XmlResolver = null };
            document.Load(_fileName);
            var root = document.DocumentElement;
            if (root.Name.Equals("NoBreakAfterList", StringComparison.Ordinal))
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
                    var target = new XmlDocument { XmlResolver = null };
                    target.Load(_copyName);
                    var targetRoot = target.DocumentElement;

                    var origin = new XmlDocument { XmlResolver = null };
                    origin.Load(_fileName);
                    var originRoot = origin.DocumentElement;

                    var update = targetRoot.GetAttribute("Update").Trim().ToUpperInvariant();
                    if (update.Equals("REPLACE", StringComparison.Ordinal) || update.Equals("OVERWRITE", StringComparison.Ordinal))
                    {
                        update = update[0] + update.Substring(1).ToLowerInvariant();
                        originRoot.SetAttribute("Update", update);
                        origin.Save(_fileName);

                        return true;
                    }

                    var items = new HashSet<NoBreakAfterItem>();
                    foreach (var node in targetRoot.SelectNodes("Item").OfType<XmlElement>().ToList())
                    {
                        var item = NoBreakAfterItem.FromNode(node);
                        if (item == null || !items.Add(item))
                        {
                            targetRoot.RemoveChild(node);
                        }
                    }

                    targetRoot = origin.ImportNode(targetRoot, true) as XmlElement;
                    foreach (var node in originRoot.SelectNodes("Item").OfType<XmlElement>().ToList())
                    {
                        var item = NoBreakAfterItem.FromNode(node);
                        if (item == null || !items.Add(item))
                        {
                            originRoot.RemoveChild(node);
                        }
                        else
                        {
                            targetRoot.AppendChild(node);
                        }
                    }

                    origin.ReplaceChild(targetRoot, originRoot);
                    targetRoot.SetAttribute("Update", "Merge");
                    origin.Save(_fileName);
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
