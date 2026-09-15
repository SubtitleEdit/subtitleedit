using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;
using UITests;

namespace Tests.Features;

// These tests change the global Se.Settings, so restore what was there afterwards - other tests
// in this assembly read the same instance.
public class WaveformToolbarMoveLinesTests : IDisposable
{
    private static readonly SeWaveformToolbarItemType[] MoveLinesTypes =
    [
        SeWaveformToolbarItemType.MoveSelectedLines,
        SeWaveformToolbarItemType.MoveSelectedLinesAndFollowing,
        SeWaveformToolbarItemType.MoveAllLines,
    ];

    private readonly int _originalStepMs = Se.Settings.General.MoveSelectedLinesStepMs;
    private readonly int _originalCustom1Ms = Se.Settings.General.MoveSelectedLinesAndForwardCustom1Ms;
    private readonly int _originalCustom2Ms = Se.Settings.General.MoveSelectedLinesAndForwardCustom2Ms;
    private readonly List<SeWaveformToolbarItem> _originalToolbarItems = Se.Settings.Waveform.ToolbarItems;
    private readonly bool _originalShowToolbar = Se.Settings.Waveform.ShowToolbar;

    public void Dispose()
    {
        Se.Settings.General.MoveSelectedLinesStepMs = _originalStepMs;
        Se.Settings.General.MoveSelectedLinesAndForwardCustom1Ms = _originalCustom1Ms;
        Se.Settings.General.MoveSelectedLinesAndForwardCustom2Ms = _originalCustom2Ms;
        Se.Settings.Waveform.ToolbarItems = _originalToolbarItems;
        Se.Settings.Waveform.ShowToolbar = _originalShowToolbar;
    }

    [Fact]
    public void Defaults_MoveLinesItemsAreHidden()
    {
        var waveform = new SeWaveform();

        foreach (var type in MoveLinesTypes)
        {
            Assert.False(waveform.ToolbarItems.Single(p => p.Type == type).IsVisible, $"{type} should be off by default");
        }
    }

    [Fact]
    public void EnsureAllToolbarItems_AddsMoveLinesItemsHidden_ToLegacySettings()
    {
        var waveform = new SeWaveform();
        waveform.ToolbarItems.RemoveAll(p => MoveLinesTypes.Contains(p.Type));

        waveform.EnsureAllToolbarItems();

        foreach (var type in MoveLinesTypes)
        {
            Assert.False(waveform.ToolbarItems.Single(p => p.Type == type).IsVisible);
        }
    }

    [Fact]
    public void ConfigureDialog_ListsMoveLinesItems_WithRealNames()
    {
        var waveform = new SeWaveform();
        var vm = new WaveformToolbarItemsViewModel();
        vm.Initialize(waveform.ToolbarItems);

        foreach (var type in MoveLinesTypes)
        {
            var display = vm.ToolbarItems.Single(p => p.Type == type);
            Assert.False(string.IsNullOrWhiteSpace(display.Name));
            Assert.NotEqual(type.ToString(), display.Name);
        }
    }

    [Fact]
    public void GetMoveLinesSteps_SortsBySize()
    {
        Se.Settings.General.MoveSelectedLinesStepMs = 100;
        Se.Settings.General.MoveSelectedLinesAndForwardCustom1Ms = 1000;
        Se.Settings.General.MoveSelectedLinesAndForwardCustom2Ms = 10;

        var steps = InitWaveform.GetMoveLinesSteps(MoveLinesScope.SelectedAndForward);

        Assert.Equal([(2, 10), (0, 100), (1, 1000)], steps);
    }

    [Fact]
    public void GetMoveLinesSteps_SkipsDuplicateAndNonPositiveValues()
    {
        Se.Settings.General.MoveSelectedLinesStepMs = 100;
        Se.Settings.General.MoveSelectedLinesAndForwardCustom1Ms = 100;
        Se.Settings.General.MoveSelectedLinesAndForwardCustom2Ms = 0;

        var steps = InitWaveform.GetMoveLinesSteps(MoveLinesScope.SelectedAndForward);

        Assert.Equal([(0, 100)], steps);
    }

    [Fact]
    public void GetMoveLinesCommandName_NamesExistingShortcutCommands()
    {
        foreach (var scope in Enum.GetValues<MoveLinesScope>())
        {
            for (var slot = 0; slot <= 2; slot++)
            {
                foreach (var back in new[] { true, false })
                {
                    var name = ShortcutsMain.GetMoveLinesCommandName(scope, slot, back);
                    Assert.True(ShortcutsMain.CommandTranslationLookup.ContainsKey(name), $"Unknown command name: {name}");
                }
            }
        }
    }

    [AvaloniaFact]
    public void Toolbar_ShowsStepButtons_AndRefreshesAfterSlotChange()
    {
        Se.Settings.General.MoveSelectedLinesStepMs = 100;
        Se.Settings.General.MoveSelectedLinesAndForwardCustom1Ms = 10;
        Se.Settings.General.MoveSelectedLinesAndForwardCustom2Ms = 1000;
        Se.Settings.Waveform.ShowToolbar = true;
        Se.Settings.Waveform.ToolbarItems = new SeWaveform().ToolbarItems;
        Se.Settings.Waveform.ToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.MoveSelectedLinesAndFollowing).IsVisible = true;

        var (window, vm) = CreateMainViewModel();
        try
        {
            InitLayout.MakeLayout(vm.MainView!, vm, 12);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(["-1000", "-100", "-10", "+10", "+100", "+1000"], GetMoveLinesButtonLabels(vm));

            var back100 = GetMoveLinesButtons(vm).Single(b => (string)b.Content! == "-100");
            Assert.Same(vm.MoveSelectedLinesAndForwardXMsBackCommand, back100.Command);
            Assert.Equal("Move selected lines and all following 100 ms back", AutomationProperties.GetName(back100));

            // The custom slots are changed in the Shortcuts window, which only reloads shortcuts.
            Se.Settings.General.MoveSelectedLinesAndForwardCustom2Ms = 500;
            vm.RefreshWaveformMoveLinesButtons!.Invoke();

            Assert.Equal(["-500", "-100", "-10", "+10", "+100", "+500"], GetMoveLinesButtonLabels(vm));
            Assert.Same(vm.MoveSelectedLinesAndForwardCustom2ForwardCommand, GetMoveLinesButtons(vm).Last().Command);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static List<Button> GetMoveLinesButtons(MainViewModel vm)
    {
        var waveformGrid = Assert.IsType<Grid>(vm.AudioVisualizer!.GetLogicalParent());
        return waveformGrid.GetLogicalDescendants()
            .OfType<Button>()
            .Where(b => b.IsVisible && b.Content is string s && (s.StartsWith('-') || s.StartsWith('+')))
            .ToList();
    }

    private static List<string> GetMoveLinesButtonLabels(MainViewModel vm)
    {
        return GetMoveLinesButtons(vm).Select(b => (string)b.Content!).ToList();
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
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
