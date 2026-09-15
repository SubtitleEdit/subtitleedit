using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xunit;

namespace UITests.Features.Options;

/// <summary>
/// The category buttons on the left of Settings used to only scroll the page: keyboard focus
/// stayed on the button, so for a screen reader user (and anyone tabbing) the categories did
/// nothing, and reaching a section meant tabbing through every setting above it (#12087).
/// Picking a category now moves focus to the section's first control, and each section is a
/// named group with its title as a heading.
/// </summary>
public class SettingsCategoryNavigationTests : IDisposable
{
    // The fade around the scroll runs on the animation clock, which can stall mid-fade in the
    // headless suite (opacity stuck at 0.59 depending on test order) - test the focus move without it.
    public SettingsCategoryNavigationTests()
    {
        SettingsViewModel.AnimateScrollToSection = false;
    }

    public void Dispose()
    {
        SettingsViewModel.AnimateScrollToSection = true;
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void PickingACategory_MovesFocusIntoItsSection(bool viaKeyboard)
    {
        var (window, vm) = OpenSettings();
        try
        {
            var section = vm.Sections.First(s => s.Title == Se.Language.General.VideoPlayer);
            var menuButton = window.GetVisualDescendants().OfType<Button>()
                .First(b => AutomationProperties.GetName(b) == section.Title);
            menuButton.Focus(viaKeyboard ? NavigationMethod.Tab : NavigationMethod.Pointer);
            Dispatcher.UIThread.RunJobs();

            vm.ScrollToSectionCommand.Execute(section.Title);

            var moved = PumpUntil(() => window.FocusManager?.GetFocusedElement() is Visual focused &&
                                        section.Panel!.IsVisualAncestorOf(focused));
            Assert.True(moved, $"Focus stayed on {window.FocusManager?.GetFocusedElement()}");

            // The focus rectangle only when the category was picked from the keyboard - a mouse
            // click should not leave a focus frame on a combo box in the content.
            var element = (Control)window.FocusManager!.GetFocusedElement()!;
            Assert.Equal(viaKeyboard, element.Classes.Contains(":focus-visible"));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void Sections_AreNamedGroups_WithHeadingTitles()
    {
        var (window, vm) = OpenSettings();
        try
        {
            var sections = vm.Sections.Where(s => s.Panel != null).ToList();
            Assert.NotEmpty(sections);
            foreach (var section in sections)
            {
                var peer = ControlAutomationPeer.CreatePeerForElement(section.Panel!);
                Assert.Equal(AutomationControlType.Group, peer.GetAutomationControlType());
                Assert.Equal(section.Title, peer.GetName());
                Assert.True(peer.IsControlElement());

                var header = section.Panel!.GetVisualDescendants().OfType<TextBlock>().First(t => t.Text == section.Title);
                Assert.Equal(2, AutomationProperties.GetHeadingLevel(header));
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static (SettingsWindow Window, SettingsViewModel Vm) OpenSettings()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var vm = Locator.Services.GetRequiredService<SettingsViewModel>();
        var window = new SettingsWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    /// <summary>
    /// The scroll runs as a background-priority dispatcher job with yields in between.
    /// </summary>
    private static bool PumpUntil(Func<bool> condition)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return true;
            }

            Thread.Sleep(10);
        }

        return false;
    }
}
