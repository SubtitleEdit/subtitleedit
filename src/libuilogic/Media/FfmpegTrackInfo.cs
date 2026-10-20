using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.Media
{
    public class FfmpegTrackInfo
    {
        public FfmpegTrackType TrackType { get; set; }
        public string TrackInfo { get; set; } = string.Empty;
        // ISO 639-2/T language tag (e.g. "eng") captured from the ffmpeg
        // "Stream #0:N(LANG): TYPE:" prefix. Empty when ffmpeg did not report one.
        public string Language { get; set; } = string.Empty;

        // The global ffmpeg stream index N from "Stream #0:N" - the same number used in
        // "-map 0:N". -1 when ffmpeg did not report one.
        public int StreamIndex { get; set; } = -1;

        // ffmpeg's stream type word for FfmpegTrackType.Other ("Data", "Attachment", "Unknown"),
        // e.g. to leave data streams (MOV timecode, SCTE-35) out of a stream copy.
        public string TypeName { get; set; } = string.Empty;

        private static readonly Regex BitRateRegex = new Regex(@"\d+ kb/s", RegexOptions.Compiled);

        public int BitRate
        {
            get
            {
                var match = BitRateRegex.Match(TrackInfo);
                if (match.Success)
                {
                    var kb = match.Value.Replace(" kb/s", string.Empty);
                    if (int.TryParse(kb, out var number))
                    {
                        return number * 1024;
                    }
                }

                return 0;
            }
        }
    }
}
