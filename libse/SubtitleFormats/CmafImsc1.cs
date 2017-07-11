using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// CMFT - "Common Media application Format Text"
    /// </summary>
    public class CmafImsc1 : SubtitleFormat
    {

        public override string Extension => ".cmft";

        public const string NameOfFormat = "CmafImsc1";

        public override string Name => NameOfFormat;

        public override bool IsTimeBased => true;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".cmft", StringComparison.OrdinalIgnoreCase))
            {
                var parser = new CmafParser(fileName);
                return parser.Subtitle.Paragraphs.Count > 0;
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var parser = new CmafParser(fileName);
            subtitle.Paragraphs.AddRange(parser.Subtitle.Paragraphs);
        }

    }
}
