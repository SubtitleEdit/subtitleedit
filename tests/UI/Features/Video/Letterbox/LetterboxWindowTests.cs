using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Video.Letterbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Video.Letterbox;

public class LetterboxWindowTests : IDisposable
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

    private static SettingsScope NewScope() => new(
        "Video.Letterbox.Enabled",
        "Video.Letterbox.TopHeightPercent",
        "Video.Letterbox.BottomHeightPercent");

    private LetterboxWindow BuildWindow()
    {
        var vm = new LetterboxViewModel();
        vm.Initialize(null);
        var window = new LetterboxWindow(vm);
        _windows.Add(window);
        return window;
    }

    [AvaloniaFact]
    public void Window_Constructs()
    {
        using var _ = NewScope();
        var window = BuildWindow();

        Assert.NotNull(window.Content);
    }

    [AvaloniaFact]
    public void Window_HasEnabledCheckBoxAndBothSliders()
    {
        using var _ = NewScope();
        var window = BuildWindow();

        Assert.Single(window.GetLogicalDescendants().OfType<CheckBox>());
        Assert.Equal(2, window.GetLogicalDescendants().OfType<Slider>().Count());
    }

    [AvaloniaFact]
    public void Window_HasApplyOkAndCancelButtons()
    {
        using var _ = NewScope();
        var window = BuildWindow();

        // CheckBox is itself an Avalonia Button (via ToggleButton), so counting Button-typed
        // descendants across the whole window also catches the Enabled checkbox - scope to the
        // button bar (the StackPanel UiUtil.MakeButtonBar returns) instead of guessing a total.
        var buttonBar = window.GetLogicalDescendants().OfType<StackPanel>()
            .Single(panel => panel.Children.OfType<Button>().Count() == panel.Children.Count && panel.Children.Count > 1);

        Assert.Equal(3, buttonBar.Children.Count);
    }
}
