using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Wsb : SubtitleFormat
    {
        public override string Extension => ".WSB";

        public override string Name => "WSB";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return base.IsMine(lines, fileName);
        }

        /// <summary>
        /// WSB is import-only, like the other read-only formats here (DlDd, FTE, SPU image...).
        /// </summary>
        /// <remarks>
        /// WSB records are binary: <see cref="LoadSubtitle"/> finds a cue by the "     10     "
        /// separator after the text and the 16 timecode digits that sit immediately before the
        /// 0x37 0x01 0x01 0x00 marker. The writer that used to live here emitted an unrelated
        /// plain-text layout ("0001 : 00031522,...", "80 80 80", "C1Y00 &lt;text&gt;") which no WSB
        /// tool reads and which this format's own reader parses as zero paragraphs - so saving
        /// as WSB and reopening silently lost the whole subtitle. It was written that way in
        /// 2011 and never matched the reader, in SE 4 either.
        ///
        /// Reconstructing a real WSB record needs the actual binary layout; emitting something
        /// shaped only around the two anchors above would round-trip inside Subtitle Edit while
        /// still being rejected by real WSB software, which is a worse failure than refusing.
        /// </remarks>
        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException("WSB is a read-only format in Subtitle Edit - it can be opened, but not saved.");
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //01072508010729007
            _errorCount = 0;
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var indexOf7001 = line.IndexOf("7\x01\x01\0", StringComparison.Ordinal);
                var indexOfTen = line.IndexOf("     10     ", StringComparison.Ordinal);
                if (indexOf7001 >= 0 && indexOfTen > 0)
                {
                    try
                    {
                        string text = line.Substring(0, indexOfTen).Trim();
                        string time = line.Substring(indexOf7001 - 16, 16);

                        var starTime = time.Substring(0, 8);
                        var endTime = time.Substring(8);

                        string[] startTimeParts = { starTime.Substring(0, 2), starTime.Substring(2, 2), starTime.Substring(4, 2), starTime.Substring(6, 2) };
                        string[] endTimeParts = { endTime.Substring(0, 2), endTime.Substring(2, 2), endTime.Substring(4, 2), endTime.Substring(6, 2) };

                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startTimeParts), DecodeTimeCodeFramesFourParts(endTimeParts), text);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        _errorCount++;
                    }
                }
                else if (p != null)
                {
                    _errorCount++;
                }
            }
            if (p != null && !string.IsNullOrEmpty(p.Text))
            {
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

    }
}
