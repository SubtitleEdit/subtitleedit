using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IMpvReloader
{
    /// <returns>False when mpv did not take the subtitle and the caller should retry - see
    /// <see cref="MpvReloader.RefreshMpv"/>.</returns>
    Task<bool> RefreshMpv(LibMpvDynamicPlayer mpv, Subtitle subtitle, Subtitle? subtitleSecondary, SubtitleFormat uiFormat);
    void Reset();
    bool SmpteMode { get; set; }
    bool SubtitlesVisible { get; set; }
}
