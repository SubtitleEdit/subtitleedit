using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class PixelFormatItem
{
    public string Codec { get; set; }
    public string Name { get; set; }

    public PixelFormatItem(string codec, string name)
    {
        Codec = codec;
        Name = name;
    }

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(Codec))
        {
            return Name;
        }

        return $"{Codec}: {Name}";
    }

    /// <summary>"No pixel format" - whitespace, so nothing is written to the command line.</summary>
    public const string Blank = " ";

    private static readonly Dictionary<string, string> Names = new()
    {
        { Blank, Blank },
        { "yuv420p", "8-bit 4:2:0" },
        { "nv12", "8-bit 4:2:0" },
        { "yuv422p", "8-bit 4:2:2" },
        { "yuv444p", "8-bit 4:4:4" },
        { "yuv420p10le", "10-bit 4:2:0" },
        { "p010le", "10-bit 4:2:0" },
        { "yuv422p10le", "10-bit 4:2:2" },
        { "yuv444p10le", "10-bit 4:4:4" },
    };

    // What each encoder's "Supported pixel formats" line actually lists (ffmpeg -h encoder=...).
    // An unsupported "-pix_fmt" is not an error: ffmpeg says "Incompatible pixel format ...,
    // auto-selecting format ..." and encodes in something else - so offering e.g. 10-bit for
    // nvenc silently produced an 8-bit file. Only the formats the encoder can really use are
    // offered. The hardware encoders take nv12/p010le rather than the planar names, which is the
    // same picture with the chroma interleaved.
    private static readonly List<string> AllFormats = new()
    {
        Blank, "yuv420p", "yuv422p", "yuv444p", "yuv420p10le", "yuv422p10le", "yuv444p10le",
    };

    private static readonly Dictionary<string, List<string>> FormatsPerCodec = new()
    {
        { "h264_nvenc", new List<string> { Blank, "yuv420p", "yuv444p" } },
        { "hevc_nvenc", new List<string> { Blank, "yuv420p", "yuv444p", "p010le" } },
        { "h264_amf", new List<string> { Blank, "nv12" } },
        { "hevc_amf", new List<string> { Blank, "nv12" } },
        { "h264_qsv", new List<string> { Blank, "nv12" } },
        { "hevc_qsv", new List<string> { Blank, "nv12", "p010le" } },
        { "prores_ks", new List<string> { Blank, "yuv422p10le", "yuv444p10le" } },
    };

    /// <summary>
    /// A stored pixel format the chosen encoder does not have, mapped to its equivalent - a
    /// settings file holding "yuv420p" for QSV would otherwise reset the field to blank even
    /// though "nv12" is the very same 8-bit 4:2:0.
    /// </summary>
    private static readonly Dictionary<string, string> Equivalents = new()
    {
        { "yuv420p", "nv12" },
        { "nv12", "yuv420p" },
        { "yuv420p10le", "p010le" },
        { "p010le", "yuv420p10le" },
    };

    /// <summary>
    /// The pixel formats <paramref name="videoCodec"/> can encode. The first entry is the blank
    /// "let ffmpeg decide" one. Unknown codecs - and the macOS VideoToolbox encoders, which
    /// cannot be probed from here - keep the full list and the old behaviour.
    /// </summary>
    public static List<PixelFormatItem> GetPixelFormats(string? videoCodec)
    {
        var codes = videoCodec != null && FormatsPerCodec.TryGetValue(videoCodec, out var list)
            ? list
            : AllFormats;

        return codes.Select(p => new PixelFormatItem(p, Names[p])).ToList();
    }

    /// <summary>
    /// Maps a stored pixel format to one <paramref name="videoCodec"/> actually supports, or
    /// null when it has no equivalent there. Pure - exposed for testing.
    /// </summary>
    public static string? Migrate(string? videoCodec, string? pixelFormat)
    {
        if (string.IsNullOrWhiteSpace(pixelFormat))
        {
            return pixelFormat;
        }

        var codes = videoCodec != null && FormatsPerCodec.TryGetValue(videoCodec, out var list)
            ? list
            : AllFormats;

        if (codes.Contains(pixelFormat))
        {
            return pixelFormat;
        }

        return Equivalents.TryGetValue(pixelFormat, out var equivalent) && codes.Contains(equivalent)
            ? equivalent
            : null;
    }
}
