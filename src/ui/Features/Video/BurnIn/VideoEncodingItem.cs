using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class VideoEncodingItem
{
    public string Codec { get; set; }
    public string Name { get; set; }

    public VideoEncodingItem(string codec, string name)
    {
        Codec = codec;
        Name = name;
    }

    public override string ToString()
    {
        return $"{Codec}: {Name}";
    }

    public static readonly List<VideoEncodingItem> VideoEncodings = BuildVideoEncodings();

    // Hardware encoders are platform-bound: NVENC/AMF/QSV need Windows or Linux drivers, and
    // VideoToolbox only exists on macOS. Offering all of them on every platform just gives the
    // user choices that fail at encode time (#13382), so list only what can actually run here.
    private static List<VideoEncodingItem> BuildVideoEncodings()
    {
        var items = new List<VideoEncodingItem>
        {
            new VideoEncodingItem("libx264", "H.264/AVC (CPU)"),
            new VideoEncodingItem("libx265", "H.265/HEVC (CPU)"),
            new VideoEncodingItem("libvpx-vp9", "VP9 (CPU)"),
            new VideoEncodingItem("prores_ks", "ProRes (CPU)"),
            //new VideoEncodingItem("libaom-av1", "AOM AV1 Encoder"),
            //new VideoEncodingItem("libsvtav1", "SVT-AV1 Encoder"),
        };

        if (Configuration.IsRunningOnMac)
        {
            items.Add(new VideoEncodingItem("h264_videotoolbox", "H.264/AVC (Apple VideoToolbox)"));
            items.Add(new VideoEncodingItem("hevc_videotoolbox", "H.265/HEVC (Apple VideoToolbox)"));
            items.Add(new VideoEncodingItem("prores_videotoolbox", "ProRes (Apple VideoToolbox)"));
        }
        else
        {
            items.Add(new VideoEncodingItem("h264_nvenc", "H.264/AVC (NVIDIA GPU)"));
            items.Add(new VideoEncodingItem("hevc_nvenc", "H.265/HEVC (NVIDIA GPU)"));
            items.Add(new VideoEncodingItem("h264_amf", "H.264/AVC (AMD GPU)"));
            items.Add(new VideoEncodingItem("hevc_amf", "H.265/HEVC (AMD GPU)"));
            items.Add(new VideoEncodingItem("h264_qsv", "H.264/AVC (Intel QSV)"));
            items.Add(new VideoEncodingItem("hevc_qsv", "H.265/HEVC (Intel QSV)"));
            //items.Add(new VideoEncodingItem("av1_nvenc", "NVIDIA AV1 Encoder"));
            //items.Add(new VideoEncodingItem("av1_qsv", "Intel QSV AV1 Encoder"));
            //items.Add(new VideoEncodingItem("av1_amf", "AMD AV1 Encoder (AMF)"));
        }

        return items;
    }

    /// <summary>
    /// The offered encoders that the ffmpeg SE is going to run was not built with, so the caller
    /// can hide them. Nothing in SE checks <c>ffmpeg -encoders</c> before building the command
    /// line, so a codec that is merely listed here fails at encode time with "Unknown encoder"
    /// once the job is already running - and which encoders exist varies per build. The Flatpak
    /// bundles an ffmpeg without x265, NVENC, AMF or QSV, and distro packages differ again.
    /// <para>
    /// Fails open in both directions: an empty <paramref name="availableEncoders"/> means the
    /// probe did not work (no ffmpeg yet, a timeout, an unparsable banner) rather than "nothing is
    /// supported", and a result that would hide every entry is discarded too. In both cases the
    /// user keeps the full list and the old behaviour.
    /// </para>
    /// Pure - exposed for testing.
    /// </summary>
    internal static List<VideoEncodingItem> GetUnsupported(
        IList<VideoEncodingItem> offered,
        IReadOnlySet<string> availableEncoders)
    {
        if (offered.Count == 0 || availableEncoders.Count == 0)
        {
            return new List<VideoEncodingItem>();
        }

        var unsupported = offered.Where(p => !availableEncoders.Contains(p.Codec)).ToList();
        return unsupported.Count < offered.Count ? unsupported : new List<VideoEncodingItem>();
    }
}
