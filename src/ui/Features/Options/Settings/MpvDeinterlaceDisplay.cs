using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

/// <summary>
/// One value of mpv's "deinterlace" option for the video player (#15827).
/// </summary>
public class MpvDeinterlaceDisplay
{
    public string Code { get; }
    public string DisplayName { get; }

    public MpvDeinterlaceDisplay(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    public override string ToString() => DisplayName;

    public static MpvDeinterlaceDisplay[] GetAll()
    {
        return
        [
            new MpvDeinterlaceDisplay("no", Se.Language.Options.Settings.DeinterlaceOff),
            new MpvDeinterlaceDisplay("auto", Se.Language.Options.Settings.DeinterlaceAuto),
            new MpvDeinterlaceDisplay("yes", Se.Language.Options.Settings.DeinterlaceAlways),
        ];
    }
}
