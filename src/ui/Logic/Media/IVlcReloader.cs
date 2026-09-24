using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IVlcReloader
{
    /// <param name="subtitleIsOwned">True when nothing else holds <paramref name="subtitle"/>, so
    /// it may be mutated without the defensive deep copy.</param>
    Task RefreshVlc(LibVlcDynamicPlayer vlc, Subtitle subtitle, Subtitle? subtitleSecondary, SubtitleFormat uiFormat, bool subtitleIsOwned = false);
    void Reset();
    bool SmpteMode { get; set; }
}
