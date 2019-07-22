﻿using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// CMFT - "Common Media application Format Text"
    /// </summary>
    public class Cmaft : BinaryFormat
    {

        public override string Extension => ".cmft";

        public const string NameOfFormat = "CMAF Text";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".cmft", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var parser = new CmafParser(fileName);
                    return parser.Subtitle.Paragraphs.Count > 0;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var parser = new CmafParser(fileName);
            subtitle.Paragraphs.Clear();
            subtitle.Paragraphs.AddRange(parser.Subtitle.Paragraphs);
        }

        public override void Save(string fileName, Subtitle subtitle)
        {
            throw new NotImplementedException();
        }
    }
}
