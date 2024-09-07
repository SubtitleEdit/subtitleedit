using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class FfmpegTrackInfo
    {
        public FfmpegTrackType TrackType { get; set; }
        public string TrackInfo { get; set; }

        public int BitRate
        {
            get
            {
                var regex = new Regex(@"\d+ kb/s");
                var match = regex.Match(TrackInfo);
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
