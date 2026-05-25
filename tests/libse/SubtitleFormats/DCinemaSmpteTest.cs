using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class DCinemaSmpteTest
{
    // Regression: MsToFramesMaxFrameRate used to clamp against the global
    // Configuration.Settings.General.CurrentFrameRate instead of the parameter,
    // so calling the helper at a different frame rate than the active project
    // setting produced an out-of-range frame number on the last frame of a
    // second.
    [Fact]
    public void MsToFramesMaxFrameRate_2014_ClampsAgainstParameter_NotGlobal()
    {
        var originalGlobal = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            // Global is 25 fps, but we ask for frames at 24 fps. 999 ms at 24 fps
            // rounds to frame 24 — invalid (valid range 0..23) — and must clamp
            // to 23. Previously this saw `24 >= 25` (false) and returned 24.
            Configuration.Settings.General.CurrentFrameRate = 25.0;
            var frames = DCinemaSmpte2014.MsToFramesMaxFrameRate(999, 24);
            Assert.Equal(23, frames);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = originalGlobal;
        }
    }

    [Fact]
    public void MsToFramesMaxFrameRate_2010_ClampsAgainstParameter_NotGlobal()
    {
        var originalGlobal = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25.0;
            var frames = DCinemaSmpte2010.MsToFramesMaxFrameRate(999, 24);
            Assert.Equal(23, frames);
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = originalGlobal;
        }
    }
}
