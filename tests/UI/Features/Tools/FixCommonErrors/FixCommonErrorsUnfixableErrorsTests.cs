using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;

namespace UITests.Features.Tools.FixCommonErrors;

// Errors that the rules find but cannot fix (e.g. a display time below the minimum with the next
// line too close to extend into) are reported through IFixCallbacks.LogStatus/AddToTotalErrors.
// They used to be dropped on the floor, so a subtitle full of them showed the green "Nothing to
// fix" and no way to see what was wrong (#13645).
public class FixCommonErrorsUnfixableErrorsTests : IDisposable
{
    private readonly List<SeFixCommonErrorsProfile> _originalProfiles;
    private readonly int _minimumDisplayMs = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
    private readonly int _minimumGapMs = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
    private readonly bool _allowMoveStartTime = Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime;

    private readonly int _analysingMinimumVisibleMs = FixCommonErrorsViewModel.AnalysingMinimumVisibleMilliseconds;
    private readonly int _analysingPaintDelayMs = FixCommonErrorsViewModel.AnalysingPaintDelayMilliseconds;

    public FixCommonErrorsUnfixableErrorsTests()
    {
        // The scan keeps its "Analysing..." feedback on screen for a quarter second; nothing here
        // looks at it.
        FixCommonErrorsViewModel.AnalysingMinimumVisibleMilliseconds = 0;
        FixCommonErrorsViewModel.AnalysingPaintDelayMilliseconds = 0;

        // The rule reads libse's Configuration.Settings, a mirror that any earlier test can have
        // left with other values (a zero minimum gap turns the "unfixable" line below into one
        // that can be extended by 20 ms). Pin what the scenario depends on.
        Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;
        Configuration.Settings.General.MinimumMillisecondsBetweenLines = 24;
        Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime = false;

        // Se.Settings is static and starts out without profiles in tests; the scan does nothing
        // without one. Restored in Dispose so no other test sees this profile.
        _originalProfiles = Se.Settings.Tools.FixCommonErrors.Profiles;
        Se.Settings.Tools.FixCommonErrors.Profiles = new List<SeFixCommonErrorsProfile>
        {
            new()
            {
                ProfileName = "Default",
                SelectedRules = new List<string> { nameof(FixShortDisplayTimes) },
            },
        };
    }

    public void Dispose()
    {
        FixCommonErrorsViewModel.AnalysingMinimumVisibleMilliseconds = _analysingMinimumVisibleMs;
        FixCommonErrorsViewModel.AnalysingPaintDelayMilliseconds = _analysingPaintDelayMs;
        Se.Settings.Tools.FixCommonErrors.Profiles = _originalProfiles;
        Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = _minimumDisplayMs;
        Configuration.Settings.General.MinimumMillisecondsBetweenLines = _minimumGapMs;
        Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime = _allowMoveStartTime;
    }

    /// <summary>
    /// A 300 ms line (below the 1000 ms minimum) that starts at zero and is followed back-to-back
    /// (next line starts where this one ends, so there is no room whatever the minimum-gap
    /// setting is - it is static and other tests may change it): it cannot be made longer at all,
    /// not even partially, and cannot be started earlier (it starts at zero), which is the branch
    /// that logs "Unable to fix text number...".
    /// </summary>
    private static FixCommonErrorsViewModel BuildViewModelWithUnfixableShortDisplayTime()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello there", 0, 300));
        subtitle.Paragraphs.Add(new Paragraph("Goodbye there", 300, 3000));
        subtitle.Renumber();

        var vm = new FixCommonErrorsViewModel(new FakeNamesList(), null!, null!);
        vm.Initialize(subtitle, new SubRip());
        return vm;
    }

    [AvaloniaFact]
    public async Task Scan_ReportsErrorsThatCouldNotBeFixed()
    {
        var vm = BuildViewModelWithUnfixableShortDisplayTime();

        await vm.DoRefreshFixes();

        Assert.Empty(vm.Fixes);
        Assert.True(vm.ErrorsFoundIsVisible);
        Assert.Equal(Se.Language.Tools.FixCommonErrors.NothingFixableBut, vm.ErrorsFoundText);

        // One log line per error, so the log window can show what is wrong with which line.
        var entry = Assert.Single(vm.LogEntries);
        Assert.Contains(Se.Language.Tools.FixCommonErrors.FixShortDisplayTimes, entry);
        Assert.Contains("Hello there", entry);
    }

    [AvaloniaFact]
    public async Task Scan_DoesNotClaimNothingToFix_WhenSubtitleHasUnfixableErrors()
    {
        var vm = BuildViewModelWithUnfixableShortDisplayTime();

        await vm.DoRefreshFixes();

        Assert.False(vm.NothingToFixIsVisible);
    }

    [AvaloniaFact]
    public async Task Scan_KeepsNothingToFix_WhenSubtitleIsClean()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello there", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("Goodbye there", 3000, 5000));
        subtitle.Renumber();

        var vm = new FixCommonErrorsViewModel(new FakeNamesList(), null!, null!);
        vm.Initialize(subtitle, new SubRip());

        await vm.DoRefreshFixes();

        Assert.True(vm.NothingToFixIsVisible);
        Assert.False(vm.ErrorsFoundIsVisible);
        Assert.Empty(vm.LogEntries);
    }

    /// <summary>
    /// Each scan describes the subtitle as it is now, so a re-scan must not stack up the errors
    /// (or the log) from the previous one.
    /// </summary>
    [AvaloniaFact]
    public async Task Rescan_RebuildsTheLogInsteadOfAppendingToIt()
    {
        var vm = BuildViewModelWithUnfixableShortDisplayTime();

        await vm.DoRefreshFixes();
        await vm.DoRefreshFixes();

        Assert.Single(vm.LogEntries);
    }

    /// <summary>
    /// The other half of the log: what an apply pass actually changed. It is a history, so unlike
    /// the scan half it accumulates - and it keeps the log worth opening on a subtitle with no
    /// errors left, which is why the status bar shows a plain "Log" link there.
    /// </summary>
    [AvaloniaFact]
    public async Task Apply_LogsWhatWasFixed()
    {
        Se.Settings.Tools.FixCommonErrors.Profiles[0].SelectedRules = new List<string> { nameof(FixUnneededSpaces) };

        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello  there", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("Goodbye  there", 3000, 5000));
        subtitle.Renumber();

        var vm = new FixCommonErrorsViewModel(new FakeNamesList(), null!, null!);
        vm.Initialize(subtitle, new SubRip());
        await vm.DoRefreshFixes();
        Assert.Empty(vm.AppliedLogEntries); // a scan changes nothing, so it logs no fixes

        await vm.DoApplyFixesCommand.ExecuteAsync(null);

        var entry = Assert.Single(vm.AppliedLogEntries);
        Assert.Contains(Se.Language.Tools.FixCommonErrors.RemoveUnneededSpaces, entry);
        Assert.Contains(string.Format(Se.Language.Tools.FixCommonErrors.XFixesApplied, 2), entry);

        // Nothing is broken in this subtitle, so the log is only reachable through its own link.
        Assert.True(vm.LogIsVisible);
        Assert.False(vm.ErrorsFoundIsVisible);
    }

    private sealed class FakeNamesList : INamesList
    {
        public void Load(string dictionaryFolder, string languageCode)
        {
        }

        public bool IsName(string candidate) => false;

        public HashSet<string> GetAbbreviations() => new();
    }
}
