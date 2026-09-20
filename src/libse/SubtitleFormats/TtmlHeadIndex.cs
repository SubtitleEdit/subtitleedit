using System;
using System.Collections.Generic;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// The document's "//ttml:style" and "//ttml:region" elements grouped by id, built once
    /// per load. TimedTextImsc11.ReadParagraph (and the TimedText10 / Rosetta readers) used to create an XmlNamespaceManager and run a
    /// document-wide XPath for every style name of every span, and
    /// TimedTextImsc11.GetAssStyleFromRegion did the same for every paragraph - so a file with
    /// styled spans walked the whole document once per span per style name.
    /// </summary>
    internal sealed class TtmlHeadIndex
    {
        private static readonly List<XmlNode> NoNodes = new List<XmlNode>();

        private readonly Dictionary<string, List<XmlNode>> _styles = new Dictionary<string, List<XmlNode>>();
        private readonly Dictionary<string, List<XmlNode>> _regions = new Dictionary<string, List<XmlNode>>();

        /// <summary>
        /// False when the document has no "ttml:head" - the scans this replaces either threw
        /// (styles) or returned early (regions) in that case, so nothing may be resolved.
        /// </summary>
        public bool HasHead { get; private set; }

        public static TtmlHeadIndex Build(XmlDocument xml)
        {
            var index = new TtmlHeadIndex();
            try
            {
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                var head = xml.DocumentElement?.SelectSingleNode("ttml:head", nsmgr);
                if (head == null)
                {
                    return index;
                }

                index.HasHead = true;

                // "//ttml:style" is document-absolute even when evaluated from head, which is
                // what the replaced scans did - keep selecting from the document.
                AddAll(index._styles, xml.SelectNodes("//ttml:style", nsmgr));
                AddAll(index._regions, xml.SelectNodes("//ttml:region", nsmgr));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }

            return index;
        }

        private static void AddAll(Dictionary<string, List<XmlNode>> target, XmlNodeList nodes)
        {
            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode node in nodes)
            {
                var id = node.Attributes?["xml:id"]?.Value ?? node.Attributes?["id"]?.Value;
                if (id == null)
                {
                    continue;
                }

                // Several elements may share an id; the replaced loops applied every match in
                // document order, so keep them all in that order.
                if (!target.TryGetValue(id, out var list))
                {
                    list = new List<XmlNode>(1);
                    target[id] = list;
                }

                list.Add(node);
            }
        }

        public List<XmlNode> GetStyles(string id) => _styles.TryGetValue(id, out var list) ? list : NoNodes;

        public List<XmlNode> GetRegions(string id) => _regions.TryGetValue(id, out var list) ? list : NoNodes;
    }
}
