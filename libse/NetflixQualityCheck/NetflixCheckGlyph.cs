using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckGlyph : INetflixQualityChecker
    {
        private static HashSet<int> LoadNetflixGlyphs()
        {
            using (var ms = new MemoryStream(Properties.Resources.NetflixAllowedGlyphs_bin))
            {
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                using (var br = new BinaryReader(zip))
                {
                    const int codepointSize = 4;
                    long n = ms.Length / codepointSize;
                    var glyphs = new int[n + 2];

                    for (int i = 0; i < n; i++)
                    {
                        glyphs[i] = br.ReadInt32();
                    }

                    // New line characters
                    glyphs[n] = 13;
                    glyphs[n + 1] = 10;

                    return new HashSet<int>(glyphs);
                }
            }
        }

        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            // Load allowed glyphs
            var allowedGlyphsSet = LoadNetflixGlyphs();

            foreach (Paragraph paragraph in subtitle.Paragraphs)
            {
                for (int pos = 0, actualPos = 0; pos < paragraph.Text.Length; pos += char.IsSurrogatePair(paragraph.Text, pos) ? 2 : 1, actualPos++)
                {
                    int curCodepoint = char.ConvertToUtf32(paragraph.Text, pos);

                    if (!allowedGlyphsSet.Contains(curCodepoint))
                    {
                        string timecode = paragraph.StartTime.ToHHMMSSFF();
                        string context = NetflixQualityController.StringContext(paragraph.Text, pos, 6);
                        string comment = string.Format(Configuration.Settings.Language.NetflixQualityCheck.GlyphCheckReport, $"U+{curCodepoint:X}", actualPos);

                        controller.AddRecord(paragraph, timecode, context, comment);
                    }
                }
            }
        }

    }
}
