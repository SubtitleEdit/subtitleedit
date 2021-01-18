using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SpuImage : SubtitleFormat
    {
        public override string Extension => ".xml";

        public override string Name => "SPU Image";

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            string xmlString = sb.ToString();
            if (!xmlString.Contains("<subpictures") || !xmlString.Contains("<spu "))
            {
                return;
            }

            var xml = new XmlDocument { XmlResolver = null };
            try
            {
                xml.LoadXml(xmlString);
            }
            catch
            {
                _errorCount = 1;
                return;
            }

            foreach (XmlNode node in xml.DocumentElement.SelectNodes("stream/spu"))
            {
                try
                {
                    string start = node.Attributes["start"].InnerText;
                    string end = node.Attributes["end"].InnerText;
                    string text = node.Attributes["image"].InnerText;
                    text = text.Replace('/', Path.DirectorySeparatorChar);
                    subtitle.Paragraphs.Add(new Paragraph(text, ParseTimeCodeToMilliseconds(start), ParseTimeCodeToMilliseconds(end)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private static double ParseTimeCodeToMilliseconds(string seconds)
        {
            return double.Parse(seconds, CultureInfo.InvariantCulture) * TimeCode.BaseUnit;
        }

    }
}
