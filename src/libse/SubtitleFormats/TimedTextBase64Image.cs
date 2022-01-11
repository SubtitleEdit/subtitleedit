using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
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

            public Bitmap GetBitmap()
            {
                var data = Convert.FromBase64String(Text);
                using (var stream = new MemoryStream(data, 0, data.Length))
                {
                    var bitmap = (Bitmap)Image.FromStream(stream);
                    return bitmap;
                }
            }

            public Size GetScreenSize()
            {
                using (var bmp = GetBitmap())
                {
                    return new Size(bmp.Width, bmp.Height);
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
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));

            var xmlString = sb.ToString();
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
            var bgImages = xml.DocumentElement.SelectNodes("//tt:div", nsmgr);
            Paragraph last = null;
            for (var i = 0; i < Math.Min(images.Count, bgImages.Count); i++)
            {
                var image = images[i];
                var text = image.InnerText;

                var bgImage = bgImages[i];
                if (bgImage.Attributes?["begin"] != null && bgImage.Attributes["end"] != null)
                {
                    var p = new Paragraph { Text = text };

                    // Time codes
                    TimedText10.ExtractTimeCodes(bgImage, subtitle, out var begin, out var end);
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
            }

            subtitle.Renumber();
        }
    }
}
