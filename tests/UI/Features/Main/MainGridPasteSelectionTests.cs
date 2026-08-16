using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// Ctrl+V in the subtitle grid must leave the pasted lines selected, like SE4 - the selection
/// (and with it the edit text box) used to stay on the line selected before the paste (#13705).
/// </summary>
public class MainGridPasteSelectionTests
{
    [AvaloniaFact]
    public void Paste_SelectsThePastedLine()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Existing one", 1000, 2000);
            AddLine(vm, "Existing two", 3000, 4000);
            SelectRow(vm, 0);

            Paste(window, vm, Srt(("Pasted one", "00:00:10,000", "00:00:11,000")));

            Assert.Equal(3, vm.Subtitles.Count);
            Assert.Equal("Pasted one", vm.Subtitles[1].Text);

            // The pasted line is the selected one, so the edit box shows it.
            Assert.Same(vm.Subtitles[1], vm.SelectedSubtitle);
            Assert.Same(vm.Subtitles[1], Assert.Single(vm.SubtitleGridSelectedItems));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Paste_SelectsAllPastedLines_WithTheFirstAsTheCurrentRow()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Existing one", 1000, 2000);
            AddLine(vm, "Existing two", 3000, 4000);
            SelectRow(vm, 0);

            Paste(window, vm, Srt(
                ("Pasted one", "00:00:10,000", "00:00:11,000"),
                ("Pasted two", "00:00:12,000", "00:00:13,000")));

            Assert.Equal(4, vm.Subtitles.Count);
            var selected = vm.SubtitleGridSelectedItems;
            Assert.Equal(2, selected.Count);
            Assert.Same(vm.Subtitles[1], selected[0]);
            Assert.Same(vm.Subtitles[2], selected[1]);
            Assert.Same(vm.Subtitles[1], vm.SelectedSubtitle);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    // Pasting with nothing selected appends at the end (#13200); the appended lines get selected
    // too, so the grid never keeps pointing at a line the user did not paste.
    [AvaloniaFact]
    public void Paste_WithoutSelection_SelectsTheAppendedLine()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Existing one", 1000, 2000);
            vm.SubtitleGrid.SelectedItem = null;
            vm.SelectedSubtitleIndex = null;
            Dispatcher.UIThread.RunJobs();

            Paste(window, vm, Srt(("Pasted one", "00:00:10,000", "00:00:11,000")));

            Assert.Equal(2, vm.Subtitles.Count);
            Assert.Same(vm.Subtitles[1], vm.SelectedSubtitle);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static void Paste(Window window, MainViewModel vm, string clipboardText)
    {
        ClipboardHelper.SetTextAsync(window, clipboardText).GetAwaiter().GetResult();
        Dispatcher.UIThread.RunJobs();

        vm.SubtitleGridPasteCommand.ExecuteAsync(null).GetAwaiter().GetResult();
        Dispatcher.UIThread.RunJobs();
    }

    private static string Srt(params (string Text, string Start, string End)[] lines)
    {
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < lines.Length; i++)
        {
            sb.AppendLine((i + 1).ToString());
            sb.AppendLine(lines[i].Start + " --> " + lines[i].End);
            sb.AppendLine(lines[i].Text);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static void SelectRow(MainViewModel vm, int index)
    {
        Dispatcher.UIThread.RunJobs();
        vm.SubtitleGrid.SelectedItem = vm.Subtitles[index];
        Dispatcher.UIThread.RunJobs();
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

    private static void AddLine(MainViewModel vm, string text, int startMs, int endMs)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            Number = vm.Subtitles.Count + 1,
        });
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
