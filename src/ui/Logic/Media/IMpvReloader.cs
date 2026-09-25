using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IMpvReloader
{
    /// <returns>False when mpv did not take the subtitle and the caller should retry - see
    /// <see cref="MpvReloader.RefreshMpv"/>.</returns>
    /// <param name="subtitleIsOwned">True when nothing else holds <paramref name="subtitle"/>, so
    /// it may be mutated and read off the UI thread without the defensive deep copy.</param>
    Task<bool> RefreshMpv(LibMpvDynamicPlayer mpv, Subtitle subtitle, Subtitle? subtitleSecondary, SubtitleFormat uiFormat, bool subtitleIsOwned = false);
    void Reset();
    bool SmpteMode { get; set; }
    bool SubtitlesVisible { get; set; }

    /// <summary>
    /// Hides the subtitles on the video regardless of <see cref="SubtitlesVisible"/>, without
    /// changing the user's own choice (screen privacy mode, #15300).
    /// </summary>
    bool SubtitlesForceHidden { get; set; }

    bool SubtitlesEffectivelyVisible { get; }
}
