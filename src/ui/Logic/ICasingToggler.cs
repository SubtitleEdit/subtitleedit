using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic
{
    public interface ICasingToggler
    {
        string ToggleCasing(string input, SubtitleFormat format, string? overrideFromStringInit = null);
    }
}