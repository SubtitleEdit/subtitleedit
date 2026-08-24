using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Core;

public class BugHunt20260824Test
{
    [Fact]
    public void AssaCheckForErrors_PaddedFormatLineDoesNotThrow()
    {
        // "Format:" plus whitespace trims to 7 chars while the raw line is longer than the
        // guard's 10 - Substring(8) on the trimmed string must not be reached.
        var header = "[V4+ Styles]\nFormat:      \nStyle: Default,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1";
        var result = AdvancedSubStationAlpha.CheckForErrors(header);
        Assert.NotNull(result);
    }

    [Fact]
    public void AssaCheckForErrors_PaddedFormatLineWithFieldsStillParses()
    {
        var header = "[V4+ Styles]\n   Format: Name, Fontname, Fontsize   \nStyle: ,Arial,20";
        Assert.Contains("'Name' is empty", AdvancedSubStationAlpha.CheckForErrors(header));
    }
}
