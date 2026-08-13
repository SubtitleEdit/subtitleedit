using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// "Edit original" mode (#13594): a runtime toggle that opens the original subtitle for editing -
/// including one opened as a read-only reference - and returns it to its opening state when left,
/// never losing edits silently. See <c>MainViewModel.ToggleEditOriginalMode</c>.
/// </summary>
public class MainEditOriginalModeTests
{
    [AvaloniaFact]
    public void EnterFromReadOnlyReference_MakesTheOriginalEditableWithACleanBaseline()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm, isReadOnly: true);
            SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
            Assert.True(vm.IsOriginalReadOnly);
            Assert.False(vm.CanEditOriginal);

            InvokeToggleEditOriginalMode(vm);

            Assert.True(vm.IsEditOriginalMode);
            Assert.False(vm.IsOriginalReadOnly);
            Assert.True(vm.CanEditOriginal);

            // Entering alone is not a change - the dirty baseline starts clean.
            Assert.False(vm.HasChanges());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void LeaveWithoutEdits_ReturnsTheOriginalToReadOnly()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm, isReadOnly: true);
            InvokeToggleEditOriginalMode(vm);
            Assert.True(vm.IsEditOriginalMode);

            InvokeToggleEditOriginalMode(vm);

            Assert.False(vm.IsEditOriginalMode);
            Assert.True(vm.IsOriginalReadOnly);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void EnterFromEditableOriginal_LeavingKeepsItEditableWithoutPrompting()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm, isReadOnly: false);
            Assert.True(vm.CanEditOriginal);

            InvokeToggleEditOriginalMode(vm);
            Assert.True(vm.IsEditOriginalMode);

            // Even with unsaved edits: the original was editable before the mode, so leaving
            // changes nothing about how the edits are tracked - no prompt, no state flip.
            vm.Subtitles[0].OriginalText = "Edited in the mode";
            InvokeToggleEditOriginalMode(vm);

            Assert.False(vm.IsEditOriginalMode);
            Assert.False(vm.IsOriginalReadOnly);
            Assert.Equal("Edited in the mode", vm.Subtitles[0].OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// In the mode, a display-only row's timings are editable - they are the original line's own,
    /// and editing the original is the point. Outside the mode they stay off.
    /// </summary>
    [AvaloniaFact]
    public void EditOriginalMode_MakesReferenceOnlyRowTimingsEditable()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm, isReadOnly: true);
            vm.SelectedSubtitle = vm.Subtitles.Single(p => p.IsReferenceOnly);
            Assert.False(vm.AreTimeCodesEditable);

            InvokeToggleEditOriginalMode(vm);
            Assert.True(vm.AreTimeCodesEditable);

            InvokeToggleEditOriginalMode(vm);
            Assert.False(vm.AreTimeCodesEditable);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void CloseOriginal_EndsTheMode()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm, isReadOnly: true);
            InvokeToggleEditOriginalMode(vm);
            Assert.True(vm.IsEditOriginalMode);

            // No edits were made, so the close prompts for nothing and tears the original down.
            InvokeFileCloseOriginal(vm);

            Assert.False(vm.IsEditOriginalMode);
            Assert.False(vm.IsOriginalReadOnly);
            Assert.False(vm.ShowColumnOriginalText);
            Assert.DoesNotContain(vm.Subtitles, p => p.IsReferenceOnly);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The discard path of leaving the mode: the original snapshot taken at entry is restored and
    /// the grid re-derived from it, exactly like opening the file again.
    /// </summary>
    [AvaloniaFact]
    public void DiscardOriginalEdits_RestoresTheSnapshotIntoTheGrid()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm, isReadOnly: true);
            InvokeToggleEditOriginalMode(vm);

            vm.Subtitles[0].OriginalText = "Tampered while editing";
            var referenceRow = vm.Subtitles.Single(p => p.IsReferenceOnly);
            referenceRow.OriginalText = "Tampered reference row";

            InvokeDiscardOriginalEdits(vm);

            Assert.Equal("Reference one", vm.Subtitles[0].OriginalText);
            var restoredRow = Assert.Single(vm.Subtitles, p => p.IsReferenceOnly);
            Assert.Equal("Reference only - no translation", restoredRow.OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static void ImportSampleReference(MainViewModel vm, bool isReadOnly)
    {
        AddLine(vm, "Translated one", 0, 2000);
        AddLine(vm, "Translated two", 4000, 6000);

        var reference = new Subtitle();
        reference.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
        reference.Paragraphs.Add(new Paragraph("Reference only - no translation", 2000, 4000));
        reference.Paragraphs.Add(new Paragraph("Reference two", 4000, 6000));

        var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
        var method = typeof(MainViewModel).GetMethod(
                         "ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ImportOriginalSubtitle not found");
        method.Invoke(vm, new object?[] { 0, "reference.srt", reference, match, isReadOnly });
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

    // The tested paths never show the save prompt, so the tasks complete synchronously.
    private static void InvokeToggleEditOriginalMode(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ToggleEditOriginalMode", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ToggleEditOriginalMode not found");

        ((Task)method.Invoke(vm, null)!).GetAwaiter().GetResult();
    }

    private static void InvokeFileCloseOriginal(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "FileCloseOriginal", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("FileCloseOriginal not found");

        ((Task)method.Invoke(vm, null)!).GetAwaiter().GetResult();
    }

    private static void InvokeDiscardOriginalEdits(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "DiscardOriginalEdits", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("DiscardOriginalEdits not found");

        method.Invoke(vm, null);
    }

    private static void SetPrivateField(MainViewModel vm, string name, object value)
    {
        var field = typeof(MainViewModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException($"Field not found: {name}");
        field.SetValue(vm, value);
    }
}
