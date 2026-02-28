using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

public class NetflixCheckGlyph : INetflixQualityChecker
{
    private static HashSet<int>? _netflixGlyphs = null;

    private static HashSet<int> LoadNetflixGlyphs()
    {
        if (_netflixGlyphs != null)
        {
            return _netflixGlyphs;
        }

        var glyphFileName = Path.Combine(Se.DataFolder, "netflix_glyphs.txt");
        if (!File.Exists(glyphFileName))
        {
            using var _ = Unpack();
        }

        var lines = File.ReadAllText(glyphFileName).SplitToLines();
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

        _netflixGlyphs = new HashSet<int>(list);
        return _netflixGlyphs;
    }

    private static async Task Unpack()
    {
        var zipUri = new Uri("avares://SubtitleEdit/Assets/NetflixGlyphs.zip");
        await using var zipStream = AssetLoader.Open(zipUri);
        var zipUnpacker = new ZipUnpacker();
        zipUnpacker.UnpackZipStream(zipStream, Se.DataFolder);
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

                    controller.AddRecord(paragraph, timeCode, context, comment, false);
                }
            }
        }
    }

}
