using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFixCommonErrorsProfile
{
    public string ProfileName { get; set; } = "Default";
    public List<string> SelectedRules { get; set; } = new();

    public static List<string> DefaultFixes = new List<string>
    {
        nameof(FixOverlappingDisplayTimes),
        nameof(FixShortDisplayTimes),
        nameof(FixLongDisplayTimes),
        nameof(FixInvalidItalicTags),
        nameof(FixUnneededSpaces),
        nameof(FixLongLines),
        nameof(FixShortLines),
    };
}