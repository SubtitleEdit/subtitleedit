using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UITests.Features.Options;

/// <summary>
/// Screen-reader names in the Settings window, with the Windows-only rows (file type
/// associations, mpv/VLC/FFmpeg library downloads) switched on so CI on Linux covers them too
/// (#12087). Names are read from the automation peer - what NVDA announces - rather than from
/// the attached properties, so a caption that names itself counts, and so does a bad one.
/// </summary>
public class SettingsAccessibleNamesTests
{
    [AvaloniaFact]
    public void SettingsWindow_EveryInputIsNamed_AndNoTwoButtonsShareAName()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var vm = Locator.Services.GetRequiredService<SettingsViewModel>();
        vm.IsFileTypeAssociationsVisible = true;
        vm.IsLibMpvDownloadVisible = true;
        vm.IsLibVlcDownloadVisible = true;
        vm.IsFfmpegLibsDownloadVisible = true;

        var window = new SettingsWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            // The page shows one category at a time - walk each one so every setting is covered.
            var controls = new List<Control>();
            foreach (var section in vm.Sections)
            {
                vm.SelectedSection = section;
                Dispatcher.UIThread.RunJobs();
                controls.AddRange(window.GetLogicalDescendants().OfType<Control>()
                    .Where(c => c.TemplatedParent == null && c.IsEffectivelyVisible)
                    .Where(c => !controls.Contains(c)));
            }

            // "list" followed by nameless "check box, checked" in File type associations.
            var unnamed = controls
                .Where(c => AccessibleLabels.NeedsName(c) && string.IsNullOrWhiteSpace(PeerName(c)))
                .Select(c => $"{c.GetType().Name} in {c.Parent?.GetType().Name}")
                .ToList();
            Assert.True(unnamed.Count == 0, "Unnamed controls:\n" + string.Join("\n", unnamed));

            var srt = controls.OfType<CheckBox>().FirstOrDefault(c => PeerName(c) == ".srt");
            Assert.NotNull(srt);

            // Four "Download" buttons, and two sets of "Add/Remove/Move up/Move down" for the
            // favorites lists - a screen reader user cannot tell which one has focus.
            var duplicates = controls
                .Where(c => c is Button and not ToggleButton)
                .GroupBy(PeerName)
                .Where(g => g.Count() > 1)
                .Select(g => $"\"{g.Key}\" x{g.Count()}")
                .ToList();
            Assert.True(duplicates.Count == 0, "Buttons sharing a name:\n" + string.Join("\n", duplicates));

            var download = controls.OfType<Button>().Single(c => PeerName(c) == Se.Language.Options.Settings.DownloadMpv);
            Assert.False(string.IsNullOrEmpty(ControlAutomationPeer.CreatePeerForElement(download).GetHelpText()));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static string PeerName(Control control)
    {
        return ControlAutomationPeer.CreatePeerForElement(control).GetName();
    }
}
