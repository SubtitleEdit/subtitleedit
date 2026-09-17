using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Headless;
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
using System.Collections.Generic;
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

    /// <summary>
    /// One category at a time, like SE4 (#12087): with all sections on one page a screen reader
    /// user could not tell where one section ended and the next began, and getting back to the
    /// categories meant Shift+Tab through every setting already passed.
    /// </summary>
    [AvaloniaFact]
    public void OnlyTheSelectedCategory_IsInTheContent()
    {
        var (window, vm) = OpenSettings();
        try
        {
            Assert.Same(vm.Sections[0], vm.SelectedSection);
            Assert.Equal([vm.Sections[0]], ShownSections(vm));

            var videoPlayer = vm.Sections.First(s => s.Title == Se.Language.General.VideoPlayer);
            vm.ScrollToSectionCommand.Execute(videoPlayer.Title);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal([videoPlayer], ShownSections(vm));
            Assert.True(videoPlayer.Panel!.IsAttachedToVisualTree());
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void Search_ShowsEveryMatchingCategory_AndClearingItReturnsToTheSelectedOne()
    {
        var (window, vm) = OpenSettings();
        try
        {
            var searchBox = window.GetVisualDescendants().OfType<TextBox>().First();
            searchBox.Text = Se.Language.General.FontSize;
            Dispatcher.UIThread.RunJobs();
            Assert.True(ShownSections(vm).Count > 1, "A search should span every category");

            searchBox.Text = string.Empty;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal([vm.SelectedSection!], ShownSections(vm));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void CtrlPageDown_MovesToTheNextCategory_AndFocusesIt()
    {
        var (window, vm) = OpenSettings();
        try
        {
            var first = vm.Sections[0];
            var second = vm.Sections[1];

            vm.OnPreviewKeyDown(new KeyEventArgs { Key = Key.PageDown, KeyModifiers = KeyModifiers.Control, RoutedEvent = InputElement.KeyDownEvent });
            Assert.Same(second, vm.SelectedSection);
            Assert.True(PumpUntil(() => window.FocusManager?.GetFocusedElement() is Visual focused &&
                                      second.Panel!.IsVisualAncestorOf(focused)));

            vm.OnPreviewKeyDown(new KeyEventArgs { Key = Key.PageUp, KeyModifiers = KeyModifiers.Control, RoutedEvent = InputElement.KeyDownEvent });
            Assert.Same(first, vm.SelectedSection);

            // Wraps around from the first to the last category.
            vm.OnPreviewKeyDown(new KeyEventArgs { Key = Key.PageUp, KeyModifiers = KeyModifiers.Control, RoutedEvent = InputElement.KeyDownEvent });
            Assert.Same(vm.Sections[^1], vm.SelectedSection);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    /// <summary>
    /// The shortcut must work with focus on a setting inside the section, not only on the category
    /// buttons (#12087): the content's ScrollViewer, text boxes and combo boxes handle PageUp/PageDown
    /// themselves, so a bubbling window handler never saw the key.
    /// </summary>
    [AvaloniaTheory]
    [InlineData(typeof(TextBox))]
    [InlineData(typeof(ComboBox))]
    [InlineData(typeof(CheckBox))]
    [InlineData(typeof(NumericUpDown))]
    public void CtrlPageDown_WorksWithFocusOnASetting(Type controlType)
    {
        var (window, vm) = OpenSettings();
        try
        {
            Control? control = null;
            SettingsSection? section = null;
            foreach (var candidate in vm.Sections)
            {
                vm.SelectedSection = candidate;
                Dispatcher.UIThread.RunJobs();
                control = candidate.Panel!.GetVisualDescendants().OfType<Control>()
                    .FirstOrDefault(c => controlType.IsInstanceOfType(c) && c.IsEffectivelyVisible && c.IsEffectivelyEnabled);
                if (control != null)
                {
                    section = candidate;
                    break;
                }
            }

            Assert.NotNull(control);
            if (control is NumericUpDown)
            {
                control = control.GetVisualDescendants().OfType<TextBox>().First(); // the part that takes focus
            }

            Assert.True(control!.Focus(NavigationMethod.Tab));
            Dispatcher.UIThread.RunJobs();

            var expected = vm.Sections[(vm.Sections.IndexOf(section!) + 1) % vm.Sections.Count];
            window.KeyPressQwerty(PhysicalKey.PageDown, RawInputModifiers.Control);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(expected, vm.SelectedSection);

            window.KeyPressQwerty(PhysicalKey.PageUp, RawInputModifiers.Control);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(section, vm.SelectedSection);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static List<SettingsSection> ShownSections(SettingsViewModel vm)
    {
        return vm.Sections.Where(s => s.Panel != null && s.Panel.IsAttachedToVisualTree()).ToList();
    }

    [AvaloniaFact]
    public void Sections_AreNamedGroups_WithHeadingTitles()
    {
        var (window, vm) = OpenSettings();
        try
        {
            var sections = vm.Sections.ToList();
            Assert.NotEmpty(sections);
            foreach (var section in sections)
            {
                vm.SelectedSection = section;
                Dispatcher.UIThread.RunJobs();
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
