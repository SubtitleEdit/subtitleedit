using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Help.CheckForUpdates;
using Nikse.SubtitleEdit.Logic.Config;
using UITests;

namespace Tests.Features.Help;

/// <summary>
/// The gear in the check-for-updates window is an icon-only button built in code, so a dropped
/// child would not show up in a build. These tests assert the button reaches the logical tree and
/// that the dialog behind it saves the update settings the way the Settings window does -
/// including leaving the channel on its "auto" default when the dropdown was not touched - and
/// that the re-check a channel change triggers cannot be overwritten by a slower earlier reply.
/// </summary>
public class CheckForUpdatesSettingsTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private sealed class NeverCheckingUpdateService : IUpdateCheckService
    {
        public Task<UpdateCheckResult> CheckForUpdates() => Task.FromResult(new UpdateCheckResult());
    }

    /// <summary>Hands out one pending check per call, so a test can complete them in any order.</summary>
    private sealed class PendingUpdateService : IUpdateCheckService
    {
        public List<TaskCompletionSource<UpdateCheckResult>> Pending { get; } = new();

        public Task<UpdateCheckResult> CheckForUpdates()
        {
            var tcs = new TaskCompletionSource<UpdateCheckResult>();
            Pending.Add(tcs);
            return tcs.Task;
        }
    }

    private sealed class NoAutoUpdateService : IAutoUpdateService
    {
        public AutoUpdateKind Kind => AutoUpdateKind.None;
        public bool IsInstallPending => false;
        public Task<string> DownloadAsync(string version, IProgress<float>? progress, CancellationToken cancellationToken) => throw new NotSupportedException();
        public void StartInstallAfterExit(string downloadedFile) => throw new NotSupportedException();
    }

    [AvaloniaFact]
    public void CheckForUpdatesWindow_HasSettingsButton()
    {
        var vm = new CheckForUpdatesViewModel(new NeverCheckingUpdateService(), new NoAutoUpdateService(), new StubWindowService());
        var window = new CheckForUpdatesWindow(vm);
        _windows.Add(window);

        var button = window.GetLogicalDescendants().OfType<Button>().FirstOrDefault(b =>
            AutomationProperties.GetName(b) == Se.Language.General.Settings);

        Assert.NotNull(button);
        Assert.Same(vm.OpenSettingsCommand, button!.Command);
    }

    [AvaloniaFact]
    public void OpenSettings_IsDisabledWhileDownloading()
    {
        var vm = new CheckForUpdatesViewModel(new NeverCheckingUpdateService(), new NoAutoUpdateService(), new StubWindowService());

        Assert.True(vm.OpenSettingsCommand.CanExecute(null));
        vm.IsDownloading = true;
        Assert.False(vm.OpenSettingsCommand.CanExecute(null));
        vm.IsDownloading = false;
        Assert.True(vm.OpenSettingsCommand.CanExecute(null));
    }

    /// <summary>
    /// The channel can change (gear dialog) while the first check still waits on GitHub. The
    /// check started last is the one whose result must stay on screen, whichever reply lands last.
    /// </summary>
    [AvaloniaFact]
    public async Task CheckForUpdates_LaterCheckWinsOverSlowerEarlierReply()
    {
        var service = new PendingUpdateService();
        var vm = new CheckForUpdatesViewModel(service, new NoAutoUpdateService(), new StubWindowService());

        var first = vm.CheckForUpdates();
        var second = vm.CheckForUpdates();
        Assert.Equal(2, service.Pending.Count);

        service.Pending[1].SetResult(new UpdateCheckResult { ChangeLogText = "beta log", LatestVersion = "5.4.0-beta1", IsNewVersionAvailable = true });
        await second;
        Assert.Equal("beta log", vm.ChangeLogText);
        Assert.True(vm.IsDownloadLinkVisible);

        service.Pending[0].SetResult(new UpdateCheckResult { ChangeLogText = "stable log", LatestVersion = "5.3.0", IsNewVersionAvailable = false });
        await first;
        Assert.Equal("beta log", vm.ChangeLogText);
        Assert.True(vm.IsDownloadLinkVisible);
        Assert.Equal(string.Format(Se.Language.Help.CheckForUpdatesNewVersionAvailable, "5.4.0-beta1"), vm.StatusText);
    }

    [AvaloniaFact]
    public void Ok_SavesStartupFlagAndChannel()
    {
        using var scope = new SettingsScope("General.CheckForUpdatesOnStartup", "General.CheckForUpdatesChannel");
        Se.Settings.General.CheckForUpdatesOnStartup = true;
        Se.Settings.General.CheckForUpdatesChannel = UpdateCheckService.ChannelStable;

        var vm = new CheckForUpdatesSettingsViewModel();
        vm.LoadValues();
        Assert.True(vm.CheckForUpdatesOnStartup);
        Assert.Equal(Se.Language.Options.Settings.CheckForUpdatesChannelStable, vm.SelectedUpdateChannel);

        vm.CheckForUpdatesOnStartup = false;
        vm.SelectedUpdateChannel = Se.Language.Options.Settings.CheckForUpdatesChannelStableAndBeta;
        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.True(vm.ChannelChanged);
        Assert.False(Se.Settings.General.CheckForUpdatesOnStartup);
        Assert.Equal(UpdateCheckService.ChannelBeta, Se.Settings.General.CheckForUpdatesChannel);
    }

    /// <summary>
    /// An empty channel means "follow the kind of build you run"; only a real change may replace it.
    /// </summary>
    [AvaloniaFact]
    public void Ok_UntouchedChannelKeepsAutoDefault()
    {
        using var scope = new SettingsScope("General.CheckForUpdatesOnStartup", "General.CheckForUpdatesChannel");
        Se.Settings.General.CheckForUpdatesChannel = string.Empty;

        var vm = new CheckForUpdatesSettingsViewModel();
        vm.LoadValues();
        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.False(vm.ChannelChanged);
        Assert.Equal(string.Empty, Se.Settings.General.CheckForUpdatesChannel);
    }

    [AvaloniaFact]
    public void SettingsWindow_HasChannelComboAndStartupSwitch()
    {
        var vm = new CheckForUpdatesSettingsViewModel();
        vm.LoadValues();
        var window = new CheckForUpdatesSettingsWindow(vm);
        _windows.Add(window);

        var combo = window.GetLogicalDescendants().OfType<ComboBox>().FirstOrDefault(c =>
            AutomationProperties.GetName(c) == Se.Language.Options.Settings.CheckForUpdatesChannel);
        var toggle = window.GetLogicalDescendants().OfType<ToggleSwitch>().FirstOrDefault(t =>
            AutomationProperties.GetName(t) == Se.Language.Options.Settings.CheckForUpdatesOnStartup);

        Assert.NotNull(combo);
        Assert.Equal(vm.IsStartupCheckVisible, toggle != null);
    }
}
