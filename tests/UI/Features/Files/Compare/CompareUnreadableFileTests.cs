using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace UITests.Features.Files.Compare;

/// <summary>
/// A file Compare can't read - like a dropped .sup or .sub - says so instead of doing nothing,
/// and leaves what is shown alone (#15623).
/// </summary>
public class CompareUnreadableFileTests : IDisposable
{
    private readonly SeCompare _savedSettings = Se.Settings.File.Compare;
    private readonly string _unreadableFile = Path.Combine(Path.GetTempPath(), $"compare-unreadable-{Guid.NewGuid():N}.sup");
    private Window? _window;

    public CompareUnreadableFileTests()
    {
        Se.Settings.File.Compare = new SeCompare();
        File.WriteAllBytes(_unreadableFile, [0x50, 0x47, 0x00, 0x01, 0x02, 0x03, 0xFF, 0xFE]);
    }

    public void Dispose()
    {
        _window?.Close();
        Se.Settings.File.Compare = _savedSettings;
        File.Delete(_unreadableFile);
    }

    [AvaloniaFact]
    public void Reference_ThatCantBeRead_ShowsAnError_AndKeepsTheReference()
    {
        var (vm, window) = Show();

        _ = vm.LoadRightFileAsync(_unreadableFile);
        Settle();

        AssertErrorShown(window);
        Assert.Equal("right.srt", vm.RightFileName);
        Assert.Equal("Two", vm.Rows.Single().Right.Line!.Text);
    }

    [AvaloniaFact]
    public void Current_ThatCantBeRead_ShowsAnError_AndKeepsTheCurrentSubtitleEditable()
    {
        var (vm, window) = Show();

        _ = vm.LoadLeftFileAsync(_unreadableFile);
        Settle();

        AssertErrorShown(window);
        Assert.Equal("left.srt", vm.LeftFileName);
        Assert.True(vm.IsLeftEditable);
    }

    private void AssertErrorShown(Window window)
    {
        var messageBox = Assert.Single(window.OwnedWindows.OfType<MessageBox>());
        var texts = messageBox.GetVisualDescendants().OfType<TextBlock>().Select(p => p.Text ?? string.Empty).ToList();
        Assert.Contains(texts, p => p.Contains(Se.Language.General.UnknownSubtitleFormat) && p.Contains(Path.GetFileName(_unreadableFile)));
        messageBox.Close();
    }

    private (CompareViewModel vm, Window window) Show()
    {
        var vm = new CompareViewModel(new FileHelper(), new FolderHelper());
        var left = new ObservableCollection<SubtitleLineViewModel> { new(new Paragraph("One", 0, 1500), null!) { Number = 1 } };
        var right = new ObservableCollection<SubtitleLineViewModel> { new(new Paragraph("Two", 0, 1500), null!) { Number = 1 } };
        vm.Initialize(left, "left.srt", right, "right.srt", false);

        var window = new CompareWindow(vm);
        _window = window;
        window.Show();
        Settle();
        return (vm, window);
    }

    private static void Settle()
    {
        for (var pump = 0; pump < 12; pump++)
        {
            Dispatcher.UIThread.RunJobs();
        }
    }
}
