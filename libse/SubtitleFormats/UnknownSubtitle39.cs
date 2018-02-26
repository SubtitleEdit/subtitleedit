﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle39 : UnknownSubtitle38
    {
        public override string Extension => ".rtf";

        public override string Name => "Unknown 39";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                return false;

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return base.ToText(subtitle, title).ToRtf();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);

            string rtf = sb.ToString().Trim();
            if (!rtf.StartsWith("{\\rtf", StringComparison.Ordinal))
                return;

            var list = rtf.FromRtf().SplitToLines();
            base.LoadSubtitle(subtitle, list, fileName);
        }

    }
}
