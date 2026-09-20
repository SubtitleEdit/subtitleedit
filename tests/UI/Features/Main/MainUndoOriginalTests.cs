using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.UndoRedo;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// Opening and closing an original subtitle are undo steps of their own, and undo/redo restore the
/// original as a unit: the column, the file name, the read-only and display-only-row modes, and the
/// original subtitle behind the rows. Before this, undo only rebuilt the rows, so undoing past
/// "open original" left an empty original column and edit box behind (issue #14634).
/// </summary>
public class MainUndoOriginalTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory =
        Path.Combine(Path.GetTempPath(), "se-undo-original-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [AvaloniaFact]
    public async Task OpenOriginal_ThenUndo_RemovesTheOriginalCompletely_AndRedoBringsItBack()
    {
        var (window, vm) = CreateMainViewModel();
        AddLine(vm, "Translated one", 0, 2000);
        AddLine(vm, "Translated two", 2000, 4000);
        SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
        var undoRedo = GetUndoRedoManager(vm);
        undoRedo.Do(vm.MakeUndoRedoObject("loaded"));

        var original = new Subtitle();
        original.Paragraphs.Add(new Paragraph("Original one", 0, 2000));
        original.Paragraphs.Add(new Paragraph("Original two", 2000, 4000));
        InvokeImportOriginalSubtitle(vm, "original.srt", original, match: null, isReadOnly: false);
        await SettleAsync(window);

        // The open is a named step, not a "Changes detected" tick.
        Assert.Equal(2, undoRedo.UndoCount);
        Assert.Contains("original.srt", undoRedo.UndoList[^1].Description);
        Assert.True(vm.ShowColumnOriginalText);
        Assert.False(vm.HasChanges());

        vm.UndoCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(new[] { string.Empty, string.Empty }, vm.Subtitles.Select(p => p.OriginalText));
        Assert.False(vm.ShowColumnOriginalText);
        Assert.Empty(GetOriginalFileName(vm));
        Assert.Empty(GetOriginalSubtitle(vm).Paragraphs);
        // No "save changes to the original?" on the way out for an original that no longer exists.
        Assert.False(vm.HasChanges());

        vm.RedoCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(new[] { "Original one", "Original two" }, vm.Subtitles.Select(p => p.OriginalText));
        Assert.True(vm.ShowColumnOriginalText);
        Assert.Equal("original.srt", GetOriginalFileName(vm));
        Assert.Equal(new[] { "Original one", "Original two" }, GetOriginalSubtitle(vm).Paragraphs.Select(p => p.Text));
        Assert.False(vm.HasChanges());
    }

    [AvaloniaFact]
    public async Task CloseReadOnlyReference_ThenUndo_BringsTheReferenceBackWithItsRows()
    {
        var (window, vm) = CreateMainViewModel();
        AddLine(vm, "Translated one", 0, 2000);
        AddLine(vm, "Translated two", 4000, 6000);
        SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
        var undoRedo = GetUndoRedoManager(vm);
        undoRedo.Do(vm.MakeUndoRedoObject("loaded"));

        var reference = new Subtitle();
        reference.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
        reference.Paragraphs.Add(new Paragraph("Reference only - no translation", 2000, 4000));
        reference.Paragraphs.Add(new Paragraph("Reference two", 4000, 6000));
        var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
        InvokeImportOriginalSubtitle(vm, "reference.srt", reference, match, isReadOnly: true);
        await SettleAsync(window);

        Assert.Equal(3, vm.Subtitles.Count);
        Assert.True(vm.Subtitles[1].IsReferenceOnly);
        Assert.True(vm.IsOriginalReadOnly);
        Assert.True(vm.IsShowingOriginalNonMatchingLines);

        await InvokeFileCloseOriginal(vm);
        await SettleAsync(window);

        Assert.Equal(2, vm.Subtitles.Count);
        Assert.False(vm.ShowColumnOriginalText);
        Assert.False(vm.IsOriginalReadOnly);
        Assert.False(vm.IsShowingOriginalNonMatchingLines);
        Assert.Equal(3, undoRedo.UndoCount);

        vm.UndoCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(3, vm.Subtitles.Count);
        Assert.True(vm.Subtitles[1].IsReferenceOnly);
        Assert.Equal("Reference only - no translation", vm.Subtitles[1].OriginalText);
        Assert.Equal(new[] { "Reference one", "Reference two" },
            vm.Subtitles.Where(p => !p.IsReferenceOnly).Select(p => p.OriginalText));
        Assert.True(vm.ShowColumnOriginalText);
        Assert.True(vm.IsOriginalReadOnly);
        Assert.True(vm.IsShowingOriginalNonMatchingLines);
        Assert.Equal("reference.srt", GetOriginalFileName(vm));
        Assert.Equal(3, GetOriginalSubtitle(vm).Paragraphs.Count);
        Assert.False(vm.HasChanges());

        vm.RedoCommand.Execute(null);
        await SettleAsync(window);

        Assert.Equal(2, vm.Subtitles.Count);
        Assert.False(vm.ShowColumnOriginalText);
        Assert.Empty(GetOriginalFileName(vm));
        Assert.Empty(GetOriginalSubtitle(vm).Paragraphs);
    }

    /// <summary>
    /// Start-up and Reopen bring back a remembered translation/original pair. That is one load to
    /// the user, so nothing may be waiting on the undo stack afterwards - the first Ctrl+Z used to
    /// take the original away.
    /// </summary>
    [AvaloniaFact]
    public async Task ReopeningATranslationWithItsOriginal_LeavesNothingToUndo()
    {
        var (window, vm) = CreateMainViewModel();
        var translationFileName = WriteSrt("translation.srt", "Translated one", "Translated two");
        var originalFileName = WriteSrt("original.srt", "Original one", "Original two");

        await vm.SubtitleOpen(translationFileName, skipLoadVideo: true);
        await SettleAsync(window);
        Assert.Equal(2, vm.Subtitles.Count);

        await InvokeRestoreRememberedOriginal(vm, 0, originalFileName, translationFileName);
        await SettleAsync(window);

        var undoRedo = GetUndoRedoManager(vm);
        Assert.True(vm.ShowColumnOriginalText);
        Assert.Equal(new[] { "Original one", "Original two" }, vm.Subtitles.Select(p => p.OriginalText));
        Assert.Equal(1, undoRedo.UndoCount);
        Assert.False(undoRedo.CanUndo);

        vm.UndoCommand.Execute(null);
        await SettleAsync(window);

        Assert.True(vm.ShowColumnOriginalText);
        Assert.Equal(new[] { "Original one", "Original two" }, vm.Subtitles.Select(p => p.OriginalText));
        Assert.Equal(originalFileName, GetOriginalFileName(vm));
    }

    private string WriteSrt(string name, params string[] lines)
    {
        Directory.CreateDirectory(_tempDirectory);
        var fileName = Path.Combine(_tempDirectory, name);
        var subtitle = new Subtitle();
        for (var i = 0; i < lines.Length; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph(lines[i], i * 2000, i * 2000 + 1500));
        }

        File.WriteAllText(fileName, new Nikse.SubtitleEdit.Core.SubtitleFormats.SubRip().ToText(subtitle, string.Empty));
        return fileName;
    }

    private (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        _windows.Add(window);
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        return (window, vm);
    }

    private static void AddLine(MainViewModel vm, string text, int startMs, int endMs)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            Number = vm.Subtitles.Count + 1,
        });
    }

    private static async Task SettleAsync(Window window)
    {
        for (var round = 0; round < 2; round++)
        {
            for (var pump = 0; pump < 8; pump++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            await Task.Delay(50);
        }
    }

    private static IUndoRedoManager GetUndoRedoManager(MainViewModel vm) =>
        (IUndoRedoManager)GetField("_undoRedoManager").GetValue(vm)!;

    private static string GetOriginalFileName(MainViewModel vm) =>
        (string?)GetField("_subtitleFileNameOriginal").GetValue(vm) ?? string.Empty;

    private static Subtitle GetOriginalSubtitle(MainViewModel vm) =>
        (Subtitle)GetField("_subtitleOriginal").GetValue(vm)!;

    private static void InvokeImportOriginalSubtitle(
        MainViewModel vm, string fileName, Subtitle subtitle, ImportOriginalHelper.OriginalMatch? match, bool isReadOnly)
    {
        GetMethod("ImportOriginalSubtitle").Invoke(vm, new object?[] { 0, fileName, subtitle, match, isReadOnly });
    }

    private static async Task InvokeFileCloseOriginal(MainViewModel vm)
    {
        // A read-only reference closes without a save prompt, so the task completes at once.
        await (Task)GetMethod("FileCloseOriginal").Invoke(vm, null)!;
    }

    private static async Task InvokeRestoreRememberedOriginal(
        MainViewModel vm, int selectedIndex, string originalFileName, string subtitleFileName)
    {
        await (Task)GetMethod("RestoreRememberedOriginal").Invoke(vm, new object?[] { selectedIndex, originalFileName, subtitleFileName })!;
    }

    private static void SetPrivateField(MainViewModel vm, string name, object value) =>
        GetField(name).SetValue(vm, value);

    private static FieldInfo GetField(string name) =>
        typeof(MainViewModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"Field not found: {name}");

    private static MethodInfo GetMethod(string name) =>
        typeof(MainViewModel).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"Method not found: {name}");
}
