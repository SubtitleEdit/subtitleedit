using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixGlyphChecker : INetflixQualityChecker
    {
        private static int[] LoadNetflixGlyphs()
        {
            int[] glyphs;

            using (MemoryStream ms = new MemoryStream(Properties.Resources.NetflixAllowedGlyphs))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    const int codepointSize = 4;
                    long n = ms.Length / codepointSize;
                    glyphs = new int[n];

                    for (int i = 0; i < n; i++)
                    {
                        glyphs[i] = br.ReadInt32();
                    }
                }
            }
                
            return glyphs;
        }

        public void Check(Subtitle subtitle, NetflixQualityReportBuilder report)
        {
            // Load allowed glyphs
            int[] allowedGlyphsArr = LoadNetflixGlyphs();
            HashSet<int> allowedGlyphsSet = new HashSet<int>(allowedGlyphsArr);
            // New line characters
            allowedGlyphsSet.Add(13);
            allowedGlyphsSet.Add(10);

            foreach (Paragraph paragraph in subtitle.Paragraphs)
            {
                for (int pos = 0, actualPos = 0; pos < paragraph.Text.Length;
                    pos += char.IsSurrogatePair(paragraph.Text, pos) ? 2 : 1, actualPos++)
                {
                    int curCodepoint = char.ConvertToUtf32(paragraph.Text, pos);

                    if (!allowedGlyphsSet.Contains(curCodepoint))
                    {
                        string timecode = paragraph.StartTime.ToHHMMSSFF();
                        string context = NetflixQualityReportBuilder.StringContext(paragraph.Text, pos, 6);
                        string comment = string.Format(Configuration.Settings.Language.NetflixQualityCheck.GlyphCheckReport,
                            string.Format("U+{0:X}", curCodepoint), actualPos);

                        report.AddRecord(timecode, context, comment);
                    }
                }
            }
        }

    }
}
