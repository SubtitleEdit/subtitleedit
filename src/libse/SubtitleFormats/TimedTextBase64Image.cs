using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TimedTextBase64Image : SubtitleFormat
    {
        public class Base64PngImage : IBinaryParagraphWithPosition
        {
            public bool IsForced { get; set; }

            public string Text { get; set; }

            public SKBitmap GetBitmap()
            {
                var data = Convert.FromBase64String(Text);
                using (var stream = new SKMemoryStream(data))
                {
                    return SKBitmap.Decode(stream);
                }
            }

            public SKSize GetScreenSize()
            {
                using (var bmp = GetBitmap())
                {
                    return new SKSize(bmp.Width, bmp.Height);
                }
            }

            public Position GetPosition()
            {
                return new Position(0, 0);
            }

            public TimeCode StartTimeCode { get; set; }
            public TimeCode EndTimeCode { get; set; }
        }

        public override string Extension => ".xml";

        public override string Name => "Timed Text Base64 Image";

        public override string ToText(Subtitle subtitle, string title)
        {
            // meta data:
            //    <smpte:image xml:id='img0' imagetype='PNG' encoding='Base64'>iV...</smpte:image>

            // body:
            //  <div smpte:backgroundImage='#img0' xml:id='caption0' ttm:role='caption' begin='00:01:38.200' end='00:01:39.200' region='region1'/>

            var xmlStructure =
@"<?xml version='1.0' encoding='utf-8'?>
<tt xml:lang='fr' xmlns:smpte='http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt' xmlns='http://www.w3.org/ns/ttml' xmlns:tts='http://www.w3.org/ns/ttml#styling' xmlns:ttm='http://www.w3.org/ns/ttml#metadata' xmlns:ttp='http://www.w3.org/ns/ttml#parameter' tts:extent='720px 576px'>
<head>
  <ttp:profile use='http://www.w3.org/ns/ttml/profile/sdp-us'/>
   <metadata>
   </metadata>
  <layout>
    <region tts:extent='100% 100%' tts:origin='0px 0px' xml:id='region1'/>
  </layout>
</head>
<body>
</body>
</tt>".Replace('\'', '"');

            var xml = new XmlDocument();
            xml.LoadXml(xmlStructure);
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("smpte", "http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt");
            nsmgr.AddNamespace("tt", "http://www.w3.org/ns/ttml");

            var body = xml.DocumentElement.SelectSingleNode("//tt:body", nsmgr);
            var metaData = xml.DocumentElement.SelectSingleNode("//tt:metadata", nsmgr);
            foreach (var p in subtitle.Paragraphs)
            {
                XmlNode image = xml.CreateElement("image");
                image.InnerText = p.Number.ToString(CultureInfo.InvariantCulture);
                metaData.AppendChild(image);

                XmlNode div = xml.CreateElement("div");
                div.InnerText = p.StartTime.ToString();
                body.AppendChild(div);
            }

            return ToUtf8XmlString(xml);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var xmlString = JoinLines(lines);
            if (!xmlString.Contains("smpte:backgroundImage") || !xmlString.Contains("smpte:image") || !xmlString.Contains("imagetype="))
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

            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("smpte", "http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt");
            nsmgr.AddNamespace("tt", "http://www.w3.org/ns/ttml");
            var images = xml.DocumentElement.SelectNodes("//smpte:image", nsmgr);

            // resolve smpte:backgroundImage="#id" fragment references against the
            // <smpte:image xml:id="id"> elements - index pairing breaks as soon as the
            // document has a wrapper <div> or reuses an image for several captions
            var imagesById = new Dictionary<string, string>();
            foreach (XmlNode image in images)
            {
                var id = image.Attributes?["xml:id"]?.Value ?? image.Attributes?["id"]?.Value;
                if (!string.IsNullOrEmpty(id) && !imagesById.ContainsKey(id))
                {
                    imagesById.Add(id, image.InnerText.Trim());
                }
            }

            Paragraph last = null;
            var referencingNodes = xml.DocumentElement.SelectNodes("//*[@smpte:backgroundImage]", nsmgr);
            var imageIndex = 0;
            foreach (XmlNode node in referencingNodes)
            {
                if (node.Attributes?["begin"] == null || node.Attributes["end"] == null)
                {
                    continue;
                }

                // look the attribute up by namespace so any prefix binding works
                var reference = node.Attributes["backgroundImage", "http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt"]?.Value ??
                                node.Attributes["smpte:backgroundImage"]?.Value;
                if (reference == null)
                {
                    continue;
                }

                string text;
                if (reference.StartsWith('#') && imagesById.TryGetValue(reference.Substring(1), out var base64))
                {
                    text = base64;
                }
                else if (reference.StartsWith('#'))
                {
                    _errorCount++;
                    continue;
                }
                else if (imageIndex < images.Count)
                {
                    // no fragment reference (or ids missing) - fall back to document order
                    text = images[imageIndex].InnerText.Trim();
                    imageIndex++;
                }
                else
                {
                    continue;
                }

                var p = new Paragraph { Text = text };

                // Time codes
                TimedText10.ExtractTimeCodes(node, subtitle, out var begin, out var end);
                p.StartTime.TotalMilliseconds = begin.TotalMilliseconds;
                p.EndTime.TotalSeconds = end.TotalSeconds;

                if (last != null && last.Text == p.Text && Math.Abs(last.EndTime.TotalMilliseconds - p.EndTime.TotalMilliseconds) < 3000)
                {
                    last.EndTime.TotalMilliseconds = p.EndTime.TotalMilliseconds;
                }
                else
                {
                    subtitle.Paragraphs.Add(p);
                }

                last = p;
            }

            subtitle.Renumber();
        }
    }
}
