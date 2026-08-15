using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Tools.FixCommonErrors;

/// <summary>
/// Construction tests for the windows behind "errors that could not be fixed" (#13645): the log
/// window itself, and the fix common errors window that shows the clickable warning leading to it.
/// Both layouts are built entirely in code, so nothing else catches a broken binding or a control
/// that throws while being built.
/// </summary>
public class FixCommonErrorsLogWindowTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    // WindowService only touches the provider when it creates a child window, which these
    // construction tests never do.
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [AvaloniaFact]
    public void LogWindow_ShowsOneLinePerLoggedError()
    {
        var vm = new FixCommonErrorsLogViewModel();
        vm.Initialize(new[] { "Fix short display times: Unable to fix text number 1: Hello" }, 0);
        var window = new FixCommonErrorsLogWindow(vm);
        _windows.Add(window);

        Assert.NotNull(window.Content);
        Assert.Contains("Unable to fix text number 1", vm.LogText);
        Assert.False(vm.ImportantMessagesIsVisible);

        var textBox = window.GetLogicalDescendants().OfType<TextBox>().FirstOrDefault();
        Assert.NotNull(textBox);
        Assert.True(textBox.IsReadOnly);
    }

    [AvaloniaFact]
    public void LogWindow_CountsImportantMessages()
    {
        var vm = new FixCommonErrorsLogViewModel();
        vm.Initialize(new[] { "a", "b" }, 2);

        Assert.True(vm.ImportantMessagesIsVisible);
        Assert.Contains("2", vm.ImportantMessagesText);
    }

    [AvaloniaFact]
    public void FixCommonErrorsWindow_Constructs_WithTheErrorsFoundWarning()
    {
        var vm = new FixCommonErrorsViewModel(null!, new WindowService(new NullServiceProvider()), null!);
        var window = new FixCommonErrorsWindow(vm);
        _windows.Add(window);

        Assert.NotNull(window.Content);
    }
}
