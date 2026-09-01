using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

/// <summary>
/// One value of mpv's "sub-justify" option - how the lines of a multi-line preview subtitle are
/// justified inside the text block. "auto" leaves it to the alignment, which is mpv's own default.
/// </summary>
public class MpvJustifyDisplay
{
    public string Code { get; }
    public string DisplayName { get; }

    public MpvJustifyDisplay(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    public override string ToString() => DisplayName;

    public static MpvJustifyDisplay[] GetAll()
    {
        return
        [
            new MpvJustifyDisplay("auto", Se.Language.General.Auto),
            new MpvJustifyDisplay("left", Se.Language.General.Left),
            new MpvJustifyDisplay("center", Se.Language.General.Center),
            new MpvJustifyDisplay("right", Se.Language.General.Right),
        ];
    }
}
