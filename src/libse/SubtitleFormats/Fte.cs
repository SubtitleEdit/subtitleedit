using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Fte : SubtitleFormat
    {
        public override string Extension => ".fte";
        public override string Name => "FTE";

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            if (!File.Exists(fileName))
            {
                return;
            }

            const int subBufferLength = 73; // 9 bytes time code and 64 text
            _errorCount = 0;

            var bytes = FileUtil.ReadAllBytesShared(fileName);
            if (bytes.Length < 200)
            {
                return;
            }

            var index = 2;
            var encoding = Encoding.GetEncoding(1252);
            var add = 0;
            var last = 0;
            while (index + subBufferLength < bytes.Length)
            {
                var text1 = encoding.GetString(bytes, index, 31);
                var text2 = encoding.GetString(bytes, index + 31 + 2, 31);
                var timeString = encoding.GetString(bytes, index + 64, 7);
                if (int.TryParse(timeString, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
                {
                    var ms = number;
                    if (text1.StartsWith("# end", StringComparison.Ordinal))
                    {
                        add += last;
                    }
                    else
                    {
                        ms += add;
                        var p = new Paragraph((text1.Trim() + Environment.NewLine + text2.Trim()).Trim(), ms, ms + 2000);
                        subtitle.Paragraphs.Add(p);
                    }

                    last = ms;
                }

                index += subBufferLength;
            }

            subtitle.Renumber();
            subtitle.SetFixedDuration(null, 1000);
            subtitle.RecalculateDisplayTimes(Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds, null, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds);
        }
    }
}
