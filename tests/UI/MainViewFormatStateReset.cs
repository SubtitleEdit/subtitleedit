using System.Reflection;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Xunit.v3;

[assembly: UITests.MainViewFormatStateReset]

namespace UITests;

/// <summary>
/// Puts the main view's format-driven process-wide state back to "no main window open" after
/// every test.
///
/// When the format selector changes, MainViewModel pushes the format into statics that every
/// line reads: EBU STL forces frame mode through <c>UseFrameModeOverride</c> and turns on the
/// teletext line length, ASSA/SSA skip empty lines in the line count and ignore {comment}
/// blocks in CPS. Nothing turns them off when the window closes, so a test that switched the
/// format left them on for whatever ran next - "300" parsed as 3 s 0 frames in a seconds-mode
/// SecondsUpDown test, Compare rounded to frames, a real empty line stopped counting - but only
/// in the orders where no other main view test reset the format in between. No main window
/// outlives a test, so after each one the right value is the one with no format applied.
///
/// This runs in After (before the test class is disposed), so a class that snapshots one of
/// these in its constructor still restores its own value afterwards.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class MainViewFormatStateReset : BeforeAfterTestAttribute
{
    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        var general = Se.Settings.General;
        if (general.UseFrameModeOverride != null)
        {
            // The main view mirrors the effective mode into libse when the override changes.
            general.UseFrameModeOverride = null;
            Configuration.Settings.General.UseTimeFormatHHMMSSFF = general.UseFrameMode;
        }

        SubtitleLineViewModel.UseTeletextLineLength = false;
        SubtitleLineViewModel.SkipEmptyLinesInLineCount = false;
        CalcFactory.IgnoreAssaCommentBlocks = false;
    }
}
