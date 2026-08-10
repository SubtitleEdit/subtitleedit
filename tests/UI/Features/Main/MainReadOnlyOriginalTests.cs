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
/// An original subtitle that does not line up 1:1 with the working subtitle is opened as a read-only
/// reference: it is shown side by side, but it is never counted as changed and never written back.
/// Before this, the mismatching lines were dropped and the truncated result was saved over the user's
/// file (issue #13449).
/// </summary>
public class MainReadOnlyOriginalTests
{
    [AvaloniaFact]
    public void ReadOnlyOriginal_ChangedText_IsNotReportedAsChanges()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var line = AddLine(vm, "Translated text", "Reference text");
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = true;
            SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
            SetPrivateField(vm, "_changeSubtitleHashOriginal", vm.GetFastHashOriginal());

            line.OriginalText = "Changed reference text";

            Assert.False(vm.HasChanges());
            Assert.False(vm.CanEditOriginal);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void EditableOriginal_ChangedText_IsStillReportedAsChanges()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var line = AddLine(vm, "Translated text", "Original text");
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = false;
            SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
            SetPrivateField(vm, "_changeSubtitleHashOriginal", vm.GetFastHashOriginal());

            line.OriginalText = "Changed original text";

            Assert.True(vm.HasChanges());
            Assert.True(vm.CanEditOriginal);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public async Task ReadOnlyOriginal_SaveSubtitleOriginal_LeavesTheFileUntouched()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = Path.Combine(Path.GetTempPath(), $"se-readonly-original-{Guid.NewGuid():N}.srt");
        const string onDisk = "1\r\n00:00:00,000 --> 00:00:02,000\r\nReference line one\r\n\r\n" +
                              "2\r\n00:00:02,000 --> 00:00:04,000\r\nReference line two\r\n\r\n";
        await File.WriteAllTextAsync(fileName, onDisk);

        try
        {
            AddLine(vm, "Translated text", "Reference line one");
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = true;
            SetPrivateField(vm, "_subtitleFileNameOriginal", fileName);

            var saved = await InvokeSaveSubtitleOriginal(vm);

            Assert.False(saved);
            Assert.Equal(onDisk, await File.ReadAllTextAsync(fileName));
        }
        finally
        {
            CloseWindow(window, vm);
            File.Delete(fileName);
        }
    }

    [AvaloniaFact]
    public void ImportAsReadOnlyReference_KeepsEveryOriginalLineInMemory()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var reference = ImportSampleReference(vm);

            Assert.True(vm.IsOriginalReadOnly);
            Assert.False(vm.CanEditOriginal);

            // The reference itself keeps all three lines, so nothing is lost.
            var subtitleOriginal = (Subtitle)GetField("_subtitleOriginal").GetValue(vm)!;
            Assert.Equal(3, subtitleOriginal.Paragraphs.Count);
            Assert.Equal(reference.Paragraphs[1].Text, subtitleOriginal.Paragraphs[1].Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ImportAsReadOnlyReference_ShowsUnmatchedLinesAsReferenceOnlyRows()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            // The reference-only line sits between the two translated rows, in time order.
            Assert.Equal(3, vm.Subtitles.Count);

            Assert.False(vm.Subtitles[0].IsReferenceOnly);
            Assert.Equal("Translated one", vm.Subtitles[0].Text);
            Assert.Equal("Reference one", vm.Subtitles[0].OriginalText);

            Assert.True(vm.Subtitles[1].IsReferenceOnly);
            Assert.Equal(string.Empty, vm.Subtitles[1].Text);
            Assert.Equal("Reference only - no translation", vm.Subtitles[1].OriginalText);

            Assert.False(vm.Subtitles[2].IsReferenceOnly);
            Assert.Equal("Translated two", vm.Subtitles[2].Text);
            Assert.Equal("Reference two", vm.Subtitles[2].OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ReferenceOnlyRows_AreNotPartOfTheSavedSubtitle()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            var saved = vm.GetUpdateSubtitle();

            Assert.Equal(2, saved.Paragraphs.Count);
            Assert.Equal("Translated one", saved.Paragraphs[0].Text);
            Assert.Equal("Translated two", saved.Paragraphs[1].Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ReferenceOnlyRows_DoNotMakeTheWorkingSubtitleLookChanged()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", string.Empty, 0, 2000);
            AddLine(vm, "Translated two", string.Empty, 4000, 6000);
            var hashBefore = vm.GetFastHash();

            ImportReference(vm, BuildSampleReference());

            Assert.Equal(hashBefore, vm.GetFastHash());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ReferenceOnlyRows_TakeNoNumber()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            Assert.Equal(1, vm.Subtitles[0].Number);
            Assert.Equal(2, vm.Subtitles[2].Number);

            // The display-only row shows no number at all.
            Assert.Equal(string.Empty, vm.Subtitles[1].NumberDisplay);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ReapplyReadOnlyReference_RealignsAfterTheWorkingRowsChange()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            // Delete the second translated row: its reference line now has no counterpart either,
            // so after re-matching there are two reference-only rows and one translated row.
            vm.Subtitles.Remove(vm.Subtitles.Single(p => p.Text == "Translated two"));

            InvokeReapplyReadOnlyReference(vm);

            Assert.Equal(3, vm.Subtitles.Count);
            Assert.Equal(new[] { false, true, true }, vm.Subtitles.Select(p => p.IsReferenceOnly));
            Assert.Equal(
                new[] { "Reference one", "Reference only - no translation", "Reference two" },
                vm.Subtitles.Select(p => p.OriginalText));

            // Still only the one real line is saved.
            Assert.Single(vm.GetUpdateSubtitle().Paragraphs);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void CloseOriginal_RemovesTheReferenceOnlyRows()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);
            Assert.Equal(3, vm.Subtitles.Count);

            InvokeFileCloseOriginal(vm);

            Assert.Equal(2, vm.Subtitles.Count);
            Assert.DoesNotContain(vm.Subtitles, p => p.IsReferenceOnly);
            Assert.False(vm.IsOriginalReadOnly);
            Assert.False(vm.ShowColumnOriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// Working subtitle: two translated lines with a gap. Reference: the same two plus a line in the
    /// gap that the translation never got - the case from issue #13449.
    /// </summary>
    private static Subtitle BuildSampleReference()
    {
        var reference = new Subtitle();
        reference.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
        reference.Paragraphs.Add(new Paragraph("Reference only - no translation", 2000, 4000));
        reference.Paragraphs.Add(new Paragraph("Reference two", 4000, 6000));
        return reference;
    }

    private static Subtitle ImportSampleReference(MainViewModel vm)
    {
        AddLine(vm, "Translated one", string.Empty, 0, 2000);
        AddLine(vm, "Translated two", string.Empty, 4000, 6000);

        var reference = BuildSampleReference();
        ImportReference(vm, reference);
        return reference;
    }

    private static void ImportReference(MainViewModel vm, Subtitle reference)
    {
        var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
        InvokeImportOriginalSubtitle(vm, "reference.srt", reference, match, isReadOnly: true);
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

    private static SubtitleLineViewModel AddLine(
        MainViewModel vm, string text, string originalText, int startMs = 0, int endMs = 2000)
    {
        var line = new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            OriginalText = originalText,
            // The grid is always renumbered on load, so start from a numbered state like the app does.
            Number = vm.Subtitles.Count + 1,
        };

        vm.Subtitles.Add(line);
        return line;
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

    private static async Task<bool> InvokeSaveSubtitleOriginal(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "SaveSubtitleOriginal", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("SaveSubtitleOriginal not found");

        return await (Task<bool>)method.Invoke(vm, new object[] { true })!;
    }

    private static void InvokeImportOriginalSubtitle(
        MainViewModel vm, string fileName, Subtitle subtitle, ImportOriginalHelper.OriginalMatch match, bool isReadOnly)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ImportOriginalSubtitle not found");

        method.Invoke(vm, new object?[] { 0, fileName, subtitle, match, isReadOnly });
    }

    private static void InvokeReapplyReadOnlyReference(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ReapplyReadOnlyReference", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ReapplyReadOnlyReference not found");

        method.Invoke(vm, null);
    }

    private static void InvokeFileCloseOriginal(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "FileCloseOriginal", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("FileCloseOriginal not found");

        method.Invoke(vm, null);
    }

    private static void SetPrivateField(MainViewModel vm, string name, object value)
    {
        GetField(name).SetValue(vm, value);
    }

    private static FieldInfo GetField(string name)
    {
        return typeof(MainViewModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
               ?? throw new InvalidOperationException($"Field not found: {name}");
    }
}
