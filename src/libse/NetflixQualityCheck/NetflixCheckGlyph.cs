using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckGlyph : INetflixQualityChecker
    {
        private static HashSet<int> LoadNetflixGlyphs()
        {
            using (var ms = new MemoryStream(Properties.Resources.netflix_glyphs2_gz))
            {
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                using (var unzip = new StreamReader(zip))
                {
                    var lines = unzip.ReadToEnd().SplitToLines();
                    var list = new List<int>(lines.Count);
                    foreach (var line in lines)
                    {
                        list.Add(int.Parse(line, System.Globalization.NumberStyles.HexNumber));
                    }

                    if (!list.Contains(10))
                    {
                        list.Add(10);
                    }

                    if (!list.Contains(13))
                    {
                        list.Add(13);
                    }

                    return new HashSet<int>(list);
                }
            }
        }

        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            // Load allowed glyphs
            var allowedGlyphsSet = LoadNetflixGlyphs();

            foreach (var paragraph in subtitle.Paragraphs)
            {
                for (int pos = 0, actualPos = 0; pos < paragraph.Text.Length; pos += char.IsSurrogatePair(paragraph.Text, pos) ? 2 : 1, actualPos++)
                {
                    int curCodepoint = char.ConvertToUtf32(paragraph.Text, pos);

                    if (!allowedGlyphsSet.Contains(curCodepoint))
                    {
                        var timeCode = paragraph.StartTime.ToHHMMSSFF();
                        var context = NetflixQualityController.StringContext(paragraph.Text, pos, 6);
                        var comment = string.Format(NetflixLanguage.GlyphCheckReport, $"U+{curCodepoint:X}", actualPos);

                        controller.AddRecord(paragraph, timeCode, context, comment);
                    }
                }
            }
        }

    }
}
