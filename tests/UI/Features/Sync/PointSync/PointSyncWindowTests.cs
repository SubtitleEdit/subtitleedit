using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSync;
using Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;
using Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Sync.PointSync;

/// <summary>
/// Construction tests for the three point sync windows. Their layouts are built in code, so a bad
/// panel or a missing command only shows up when the window is actually constructed - which is
/// what these do. Both point sync windows gained a video-related button (issue #13341).
/// </summary>
public class PointSyncWindowTests : IDisposable
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

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static List<SubtitleLineViewModel> TwoLines()
        => new()
        {
            new() { StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(3) },
            new() { StartTime = TimeSpan.FromSeconds(5), EndTime = TimeSpan.FromSeconds(7) },
        };

    private T Track<T>(T window) where T : Window
    {
        _windows.Add(window);
        return window;
    }

    /// <summary>
    /// Button labels. WithIconLeft repacks the content into an icon + text panel, so the caption
    /// is on a TextBlock inside the button rather than on Button.Content.
    /// </summary>
    private static IEnumerable<string> ButtonTexts(Window window)
        => window.GetLogicalDescendants()
            .OfType<Button>()
            .SelectMany(b => b.GetLogicalDescendants().OfType<TextBlock>().Select(t => t.Text)
                .Append(b.Content as string))
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(t => t!);

    [AvaloniaFact]
    public void PointSyncWindow_Constructs()
    {
        var vm = new PointSyncViewModel(new FileHelper(), new WindowService(new NullServiceProvider()));
        var lines = TwoLines();
        vm.Initialize(lines, new List<SubtitleLineViewModel>(), string.Empty, string.Empty, null);

        var window = Track(new PointSyncWindow(vm));

        Assert.NotNull(window.Content);
    }

    [AvaloniaFact]
    public void PointSyncViaOtherWindow_HasBothSetSyncPointButtons()
    {
        // The other subtitle stays the usual source, and the video is the fallback for lines it
        // does not cover - so both buttons have to be there.
        var vm = new PointSyncViaOtherViewModel(new FileHelper(), new WindowService(new NullServiceProvider()));
        vm.Initialize(TwoLines(), string.Empty, string.Empty);

        var window = Track(new PointSyncViaOtherWindow(vm));

        var texts = ButtonTexts(window).ToList();
        Assert.Contains(Se.Language.Sync.SetSyncPoint, texts);
        Assert.Contains(Se.Language.Sync.SetSyncPointViaVideo, texts);
    }
}
