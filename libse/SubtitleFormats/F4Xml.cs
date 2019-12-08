using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class F4Xml : F4Text
    {
        public override string Extension => ".xml";

        public override string Name => "F4 Xml";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var xml = new XmlDocument();
            var template = @"<?xml version='1.0' encoding='utf-8' standalone='no'?>
<transcript>
    <head mediafile=''/>
    <content content=''/>
    <style>
    </style>
</transcript>".Replace("'", "\"");
            xml.LoadXml(template);
            xml.DocumentElement.SelectSingleNode("content").Attributes["content"].Value = ToF4Text(subtitle);

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            string xml = sb.ToString();
            if (!xml.Contains("<transcript", StringComparison.Ordinal))
            {
                return;
            }

            var doc = new XmlDocument { XmlResolver = null };
            doc.LoadXml(xml);
            var content = doc.DocumentElement.SelectSingleNode("content");
            if (content == null)
            {
                return;
            }

            var contentAttribute = content.Attributes["content"];
            if (contentAttribute == null)
            {
                return;
            }

            string text = contentAttribute.Value;
            LoadF4TextSubtitle(subtitle, text);
        }
    }
}
