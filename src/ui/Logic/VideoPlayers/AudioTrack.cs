using Nikse.SubtitleEdit.Core.Common;

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
            return $"{TrackNumber}: {Name.CapitalizeFirstLetter()}".TrimEnd(':').TrimEnd();
        }
    }
}
