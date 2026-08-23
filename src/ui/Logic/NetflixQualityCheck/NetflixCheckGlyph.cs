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

    // Every character of every line is probed against the allowed set, so the probe is the whole
    // cost of this check. A BMP bitmap answers it with one array load instead of hashing an int;
    // astral code points (rare, and outside the table) still go to the hash set.
    private static bool[]? _netflixGlyphsBmp = null;

    public string Name { get; set; }

    public NetflixCheckGlyph(string name)
    {
        Name = name;
    }

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

        var set = new HashSet<int>(list);
        var bmp = new bool[0x10000];
        foreach (var codePoint in set)
        {
            if ((uint)codePoint < (uint)bmp.Length)
            {
                bmp[codePoint] = true;
            }
        }

        _netflixGlyphsBmp = bmp;
        _netflixGlyphs = set;
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
        var allowedGlyphsBmp = _netflixGlyphsBmp!;

        foreach (var paragraph in subtitle.Paragraphs)
        {
            var text = paragraph.Text;
            for (int pos = 0, actualPos = 0; pos < text.Length; actualPos++)
            {
                var c = text[pos];
                var reportPos = pos; // pos advances below; the report wants the character's own index
                int curCodepoint;
                if (char.IsSurrogate(c))
                {
                    // Throws on a lone surrogate, exactly as ConvertToUtf32 did before.
                    curCodepoint = char.ConvertToUtf32(text, pos);
                    pos += 2;
                }
                else
                {
                    curCodepoint = c;
                    pos++;
                }

                var allowed = (uint)curCodepoint < (uint)allowedGlyphsBmp.Length
                    ? allowedGlyphsBmp[curCodepoint]
                    : allowedGlyphsSet.Contains(curCodepoint);
                if (!allowed)
                {
                    var timeCode = paragraph.StartTime.ToHHMMSSFF();
                    var context = NetflixQualityController.StringContext(text, reportPos, 6);
                    var comment = string.Format(Se.Language.Tools.NetflixCheckAndFix.GlyphCheckReport, $"U+{curCodepoint:X}", actualPos);

                    controller.AddRecord(paragraph, timeCode, context, comment, false);
                }
            }
        }
    }

}
