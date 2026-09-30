using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream;
using Nikse.SubtitleEdit.Core.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// A ".subs" dump of one PSP UMD Video subtitle stream, read with
    /// <see cref="UmdVideoSubtitleReader"/> (which also reads the .MPS/.PMF itself).
    /// </summary>
    public class PlayStationSubs : SubtitleFormat, IBinaryParagraphList
    {
        public override string Extension => ".subs";

        public const string NameOfFormat = "PlayStation Subs";

        public override string Name => NameOfFormat;

        private List<UmdVideoSubtitle> _pictures = new List<UmdVideoSubtitle>();

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName) || !fileName.EndsWith(".subs", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                return UmdVideoSubtitleReader.Read(fileName).Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            _pictures = UmdVideoSubtitleReader.Read(fileName).Values.FirstOrDefault() ?? new List<UmdVideoSubtitle>();
            foreach (var picture in _pictures)
            {
                subtitle.Paragraphs.Add(new Paragraph(string.Empty, picture.StartTime.TotalMilliseconds, picture.EndTime.TotalMilliseconds));
            }

            subtitle.Renumber();
        }

        public SKBitmap GetSubtitleBitmap(int index, bool crop = true)
        {
            return index >= 0 && index < _pictures.Count ? _pictures[index].GetBitmap() ?? new SKBitmap(1, 1) : new SKBitmap(1, 1);
        }

        public bool GetIsForced(int index)
        {
            return false;
        }
    }
}
