using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AudacityLabels : SubtitleFormat
    {
        public override string Extension => ".txt";

        public override string Name => "Audacity labels";

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (var line in lines)
            {
                var arr = line.Split('\t');
                if (arr.Length == 3 &&
                    double.TryParse(arr[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var start) &&
                    double.TryParse(arr[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var end))
                {
                    subtitle.Paragraphs.Add(new Paragraph(arr[2], start * TimeCode.BaseUnit, end * TimeCode.BaseUnit));
                }
                else
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }
    }
}
