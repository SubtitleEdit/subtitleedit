using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IMpvReloader
{
    Task RefreshMpv(LibMpvDynamicPlayer mpv, Subtitle subtitle, SubtitleFormat uiFormat);
    void Reset();
    bool SmpteMode { get; set; }
}
