using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;
using UITests;

namespace Tests.Features.Main;

// The waveform toolbar's rule-profile picker (#16236). These tests change the global Se.Settings
// (toolbar items and the active profile), so they restore what was there afterwards.
public class WaveformToolbarRuleProfileTests : IDisposable
{
    private readonly List<SeWaveformToolbarItem> _originalToolbarItems = Se.Settings.Waveform.ToolbarItems;
    private readonly bool _originalShowToolbar = Se.Settings.Waveform.ShowToolbar;
    private readonly string _originalProfileName = Se.Settings.General.CurrentProfile;

    public void Dispose()
    {
        Se.Settings.Waveform.ToolbarItems = _originalToolbarItems;
        Se.Settings.Waveform.ShowToolbar = _originalShowToolbar;

        var originalProfile = Se.Settings.General.Profiles.FirstOrDefault(p => p.Name == _originalProfileName);
        if (originalProfile != null)
        {
            Se.ApplyRuleProfile(originalProfile);
            Se.ApplyRuleSettingsToLibSe();
        }
    }

    [Fact]
    public void Defaults_RuleProfileItemIsHidden()
    {
        var waveform = new SeWaveform();

        Assert.False(waveform.ToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.RuleProfile).IsVisible);
    }

    [Fact]
    public void EnsureAllToolbarItems_AddsRuleProfileItemHidden_ToLegacySettings()
    {
        var waveform = new SeWaveform();
        waveform.ToolbarItems.RemoveAll(p => p.Type == SeWaveformToolbarItemType.RuleProfile);

        waveform.EnsureAllToolbarItems();

        var item = waveform.ToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.RuleProfile);
        Assert.False(item.IsVisible);
        Assert.Equal(157, item.SortOrder);
    }

    [AvaloniaFact]
    public void Picker_ListsProfiles_AndSelectingOneAppliesIt()
    {
        Se.Settings.Waveform.ShowToolbar = true;
        Se.Settings.Waveform.ToolbarItems = new SeWaveform().ToolbarItems;
        Se.Settings.Waveform.ToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.RuleProfile).IsVisible = true;

        var defaultProfile = Se.Settings.General.Profiles.Single(p => p.Name == "Default");
        var netflix = Se.Settings.General.Profiles.Single(p => p.Name == "Netflix (English)");
        Assert.NotEqual(defaultProfile.SubtitleLineMaximumLength, netflix.SubtitleLineMaximumLength);
        Se.ApplyRuleProfile(defaultProfile);
        Se.ApplyRuleSettingsToLibSe();

        var (window, vm) = CreateMainViewModel();
        try
        {
            InitLayout.MakeLayout(vm.MainView!, vm, 12);
            Dispatcher.UIThread.RunJobs();

            var comboBox = GetRuleProfileComboBox(vm);
            Assert.Equal(Se.Settings.General.Profiles.Select(p => p.Name), comboBox.Items.Cast<string>());
            Assert.Equal("Default", comboBox.SelectedItem);

            comboBox.SelectedItem = "Netflix (English)";
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("Netflix (English)", Se.Settings.General.CurrentProfile);
            Assert.Equal(netflix.SubtitleLineMaximumLength, Se.Settings.General.SubtitleLineMaximumLength);
            Assert.Equal(netflix.SubtitleLineMaximumLength, Configuration.Settings.General.SubtitleLineMaximumLength);

            // The closed box must show the new name too - a template that assigns the text once is
            // recycled and kept showing "Default".
            Assert.Equal("Netflix (English)", GetSelectionBoxText(comboBox));

            // Re-selecting the active profile is a no-op, and the Choose profile dialog's path
            // (ApplyRuleProfile from the view model) keeps the combo in step.
            vm.SelectedRuleProfile = "Netflix (English)";
            Assert.Equal("Netflix (English)", Se.Settings.General.CurrentProfile);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static string? GetSelectionBoxText(ComboBox comboBox)
    {
        Dispatcher.UIThread.RunJobs();
        comboBox.UpdateLayout();

        // The first TextBlock is the (empty) placeholder; the templated item's is the one with text.
        return comboBox.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).FirstOrDefault(t => !string.IsNullOrEmpty(t));
    }

    private static ComboBox GetRuleProfileComboBox(MainViewModel vm)
    {
        var waveformGrid = Assert.IsType<Grid>(vm.AudioVisualizer!.GetLogicalParent());
        return waveformGrid.GetLogicalDescendants()
            .OfType<ComboBox>()
            .Single(c => AutomationProperties.GetName(c) == "Rule profile");
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var vm = (MainViewModel)view.DataContext!;
        var format = new SubRip();
        for (var i = 0; i < 5; i++)
        {
            var text = $"Line {i + 1}: this is a subtitle line long enough to show a colour.";
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, i * 3000, i * 3000 + 2500), format) { Number = i + 1 });
        }

        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.SuppressSaveChangesPromptOnClose(vm);
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
