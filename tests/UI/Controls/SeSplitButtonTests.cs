using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls;
using System;
using System.Collections.Generic;

namespace UITests.Controls;

/// <summary>
/// Issue #15197: Enter on the focused "Remux" split button closed the dialog instead of
/// remuxing. Avalonia's SplitButton clicks on Enter key-up and leaves the key-down unhandled,
/// so the window's IsDefault Done button (which listens for unhandled Enter key-downs) ran
/// first. Space, which the default button ignores, worked - hence the report.
/// </summary>
public class SeSplitButtonTests : IDisposable
{
    // A window left open outlives the test: it keeps the application-wide activation and focused
    // element, so a later test's input is delivered to it instead.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private (Window Window, SplitButton Split, int[] Clicks) Open(SplitButton split)
    {
        // Clicks[0] = split button primary clicks, Clicks[1] = default button clicks.
        var clicks = new int[2];
        split.Command = new RelayCommand(() => clicks[0]++);
        var defaultButton = new Button
        {
            Content = "Done",
            IsDefault = true,
            Command = new RelayCommand(() => clicks[1]++),
        };

        var window = new Window
        {
            Content = new StackPanel { Children = { split, defaultButton } },
        };
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        split.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.True(split.IsFocused, "the test needs focus to start on the split button");
        return (window, split, clicks);
    }

    // A full key stroke: KeyPressQwerty alone is only the key-down.
    private static void Type(Window window, PhysicalKey key)
    {
        window.KeyPressQwerty(key, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        window.KeyReleaseQwerty(key, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    // One click for the whole key stroke: the key-down clicks, the key-up must not click again.
    [AvaloniaFact]
    public void Enter_ClicksThePrimaryPart_Once_NotTheDefaultButton()
    {
        var (window, _, clicks) = Open(new SeSplitButton { Content = "Remux" });

        Type(window, PhysicalKey.Enter);

        Assert.Equal(1, clicks[0]);
        Assert.Equal(0, clicks[1]);
    }

    [AvaloniaFact]
    public void Space_StillClicksThePrimaryPart_Once()
    {
        var (window, _, clicks) = Open(new SeSplitButton { Content = "Remux" });

        Type(window, PhysicalKey.Space);

        Assert.Equal(1, clicks[0]);
        Assert.Equal(0, clicks[1]);
    }

    [AvaloniaFact]
    public void Enter_WhileDisabled_ClicksNothing()
    {
        var split = new SeSplitButton { Content = "Remux" };
        var (window, _, clicks) = Open(split);
        split.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();

        Type(window, PhysicalKey.Enter);

        Assert.Equal(0, clicks[0]);
        // A disabled control does not take part in the route, so Enter reaches the window and
        // clicks the default button - the same as Enter on any other disabled control.
        Assert.Equal(1, clicks[1]);
    }

    /// <summary>
    /// The upstream behaviour the subclass exists for. If this ever fails, Avalonia has fixed
    /// SplitButton and SeSplitButton can go.
    /// </summary>
    [AvaloniaFact]
    public void PlainSplitButton_Enter_ClicksTheDefaultButtonInstead()
    {
        var (window, _, clicks) = Open(new SplitButton { Content = "Remux" });

        Type(window, PhysicalKey.Enter);

        Assert.Equal(1, clicks[1]);
    }
}
