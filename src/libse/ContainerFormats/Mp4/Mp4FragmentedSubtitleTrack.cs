using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4
{
    /// <summary>
    /// A text subtitle track extracted from a fragmented MP4 (DASH/CMAF), where the
    /// classic moov sample tables are empty and cues come from moof/traf/trun samples.
    /// </summary>
    public class Mp4FragmentedSubtitleTrack
    {
        /// <summary>tfhd track id; null when the fragment carries no track id (lone segment).</summary>
        public uint? TrackId { get; set; }

        public string Language { get; set; }

        /// <summary>Sample codec: "wvtt", "stpp", "tx3g", "stxt" or "sbtt".</summary>
        public string Codec { get; set; }

        public Subtitle Subtitle { get; set; } = new Subtitle();
    }
}
