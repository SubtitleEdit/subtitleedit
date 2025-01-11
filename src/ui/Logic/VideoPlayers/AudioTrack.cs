using Nikse.SubtitleEdit.Core.Common;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public class AudioTrack
    {
        public int TrackNumber { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }

        public AudioTrack(int trackNumber, string name, int index)
        {
            TrackNumber = trackNumber;
            Name = name;
            Index = index;
        }

        public override string ToString()
        {
            if (Name == null)
            {
                return TrackNumber.ToString(CultureInfo.InvariantCulture);
            }

            return $"{TrackNumber}: {Name.CapitalizeFirstLetter()}".TrimEnd(':').TrimEnd();
        }
    }
}
