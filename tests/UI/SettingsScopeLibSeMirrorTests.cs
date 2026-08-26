using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests;

/// <summary>
/// SettingsScope has to put libse's mirror of a setting back, not just the SE 5 property.
///
/// Se.UpdateLibSeSettings copies part of Se.Settings into libse's own Configuration singleton,
/// one way. So a test that changes a mirrored setting inside a scope, and runs that sync while
/// the scope is open, leaves the mirror holding the changed value for the rest of the run - the
/// scope restores the SE 5 side and nothing restores libse's.
///
/// UseFrameMode is the one that bites: TimeCode.ToDisplayString reads the libse flag, so the leak
/// surfaces much later as a time-format assertion failing in a test that never mentions frames
/// ("00:11:23:12" where "00:11:23.520" was expected) - a ~40%-of-runs flake locally before this
/// was fixed. A single failure is enough to matter: CI retries the whole suite on any failure,
/// and it is the retry that has twice collapsed into hundreds of teardown errors.
/// </summary>
public class SettingsScopeLibSeMirrorTests
{
    [Fact]
    public void Dispose_RestoresTheLibSeMirror_NotJustTheSe5Setting()
    {
        var originalSe5 = Se.Settings.General.UseFrameMode;
        var originalLibSe = Configuration.Settings.General.UseTimeFormatHHMMSSFF;
        try
        {
            Se.Settings.General.UseFrameMode = false;
            Configuration.Settings.General.UseTimeFormatHHMMSSFF = false;

            using (var _ = new SettingsScope("General.UseFrameMode"))
            {
                Se.Settings.General.UseFrameMode = true;

                // What Se.UpdateLibSeSettings does to the shared mirror while the scope is open.
                Configuration.Settings.General.UseTimeFormatHHMMSSFF = Se.Settings.General.UseFrameMode;
            }

            Assert.False(Se.Settings.General.UseFrameMode);
            Assert.False(Configuration.Settings.General.UseTimeFormatHHMMSSFF);
        }
        finally
        {
            Se.Settings.General.UseFrameMode = originalSe5;
            Configuration.Settings.General.UseTimeFormatHHMMSSFF = originalLibSe;
        }
    }

    [Fact]
    public void Dispose_LeavesTheMirrorAlone_WhenFrameModeWasNotScoped()
    {
        var original = Configuration.Settings.General.UseTimeFormatHHMMSSFF;
        try
        {
            Configuration.Settings.General.UseTimeFormatHHMMSSFF = true;

            using (var _ = new SettingsScope("General.MaxNumberOfLines"))
            {
                Se.Settings.General.MaxNumberOfLines = 3;
            }

            // Only the scoped setting's mirror is managed; an unrelated scope must not reach
            // across and rewrite libse state it never captured.
            Assert.True(Configuration.Settings.General.UseTimeFormatHHMMSSFF);
        }
        finally
        {
            Configuration.Settings.General.UseTimeFormatHHMMSSFF = original;
        }
    }
}
