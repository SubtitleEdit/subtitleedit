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
/// A translation made from the current rows (auto-translate, translate via copy/paste, "make empty
/// translation") moves the pre-translation text into the rows' original column and points the
/// original's file name at the subtitle it was translated from. The text has to reach the original
/// subtitle too: it is what every wholesale rebuild re-fills the column from, and saving the
/// original serializes it - so leaving it empty wrote a file with time codes and no text at all
/// over the user's source subtitle (issue #14091).
/// </summary>
public class MainTranslationOriginalTests
{
    [AvaloniaFact]
    public void CaptureOriginalFromTranslatedRows_PutsThePreTranslationTextInTheOriginalSubtitle()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Vertaalde regel een", "Translated line one", 0, 2000);
            AddLine(vm, "Vertaalde regel twee", "Translated line two", 2000, 4000);
            vm.ShowColumnOriginalText = true;

            InvokeCaptureOriginalFromTranslatedRows(vm);

            var subtitleOriginal = (Subtitle)GetField("_subtitleOriginal").GetValue(vm)!;
            Assert.Equal(2, subtitleOriginal.Paragraphs.Count);
            Assert.Equal("Translated line one", subtitleOriginal.Paragraphs[0].Text);
            Assert.Equal("Translated line two", subtitleOriginal.Paragraphs[1].Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The rebuild that most tools go through re-fills the original column from the original
    /// subtitle. Before the capture it was empty, so the column came back blank - and a save then
    /// wrote that blank original over the source file.
    /// </summary>
    [AvaloniaFact]
    public void AfterCapture_ARebuildKeepsTheOriginalColumn()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Vertaalde regel een", "Translated line one", 0, 2000);
            AddLine(vm, "Vertaalde regel twee", "Translated line two", 2000, 4000);
            vm.ShowColumnOriginalText = true;
            InvokeCaptureOriginalFromTranslatedRows(vm);

            var subtitle = vm.GetUpdateSubtitle();
            var subtitleOriginal = (Subtitle)GetField("_subtitleOriginal").GetValue(vm)!;
            InvokeSetSubtitles(vm, subtitle, subtitleOriginal);

            Assert.Equal(
                new[] { "Translated line one", "Translated line two" },
                vm.Subtitles.Select(p => p.OriginalText));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The last line of defense: whatever blanked the original column, an original without a single
    /// line of text is never written back over the user's file.
    /// </summary>
    [AvaloniaFact]
    public async Task SaveSubtitleOriginal_WithoutAnyText_LeavesTheFileUntouched()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = Path.Combine(Path.GetTempPath(), $"se-empty-original-{Guid.NewGuid():N}.srt");
        const string onDisk = "1\r\n00:00:00,000 --> 00:00:02,000\r\nSource line one\r\n\r\n" +
                              "2\r\n00:00:02,000 --> 00:00:04,000\r\nSource line two\r\n\r\n";
        await File.WriteAllTextAsync(fileName, onDisk);

        try
        {
            AddLine(vm, "Vertaalde regel een", string.Empty, 0, 2000);
            AddLine(vm, "Vertaalde regel twee", string.Empty, 2000, 4000);
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = false;
            SetPrivateField(vm, "_subtitleFileNameOriginal", fileName);

            var saved = await InvokeSaveSubtitleOriginal(vm, isAutoSave: false);

            Assert.False(saved);
            Assert.Equal(onDisk, await File.ReadAllTextAsync(fileName));
        }
        finally
        {
            CloseWindow(window, vm);
            File.Delete(fileName);
        }
    }

    /// <summary>
    /// The guard covers a one-line subtitle too: a single-paragraph original with no text
    /// overwrites the source file just as irreversibly as a longer one.
    /// </summary>
    [AvaloniaFact]
    public async Task SaveSubtitleOriginal_SingleLineWithoutText_LeavesTheFileUntouched()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = Path.Combine(Path.GetTempPath(), $"se-empty-original-single-{Guid.NewGuid():N}.srt");
        const string onDisk = "1\r\n00:00:00,000 --> 00:00:02,000\r\nSource line one\r\n\r\n";
        await File.WriteAllTextAsync(fileName, onDisk);

        try
        {
            AddLine(vm, "Vertaalde regel een", string.Empty, 0, 2000);
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = false;
            SetPrivateField(vm, "_subtitleFileNameOriginal", fileName);

            var saved = await InvokeSaveSubtitleOriginal(vm, isAutoSave: false);

            Assert.False(saved);
            Assert.Equal(onDisk, await File.ReadAllTextAsync(fileName));
        }
        finally
        {
            CloseWindow(window, vm);
            File.Delete(fileName);
        }
    }

    /// <summary>
    /// The guard is about a whole original with nothing in it - a single blank line in an otherwise
    /// normal original is ordinary content and must still be saved.
    /// </summary>
    [AvaloniaFact]
    public async Task SaveSubtitleOriginal_WithOneEmptyLine_StillWritesTheFile()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = Path.Combine(Path.GetTempPath(), $"se-original-{Guid.NewGuid():N}.srt");
        await File.WriteAllTextAsync(fileName, "1\r\n00:00:00,000 --> 00:00:02,000\r\nOld\r\n\r\n");

        try
        {
            AddLine(vm, "Vertaalde regel een", "Source line one", 0, 2000);
            AddLine(vm, "Vertaalde regel twee", string.Empty, 2000, 4000);
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = false;
            SetPrivateField(vm, "_subtitleFileNameOriginal", fileName);

            var saved = await InvokeSaveSubtitleOriginal(vm, isAutoSave: false);

            Assert.True(saved);
            Assert.Contains("Source line one", await File.ReadAllTextAsync(fileName));
        }
        finally
        {
            CloseWindow(window, vm);
            File.Delete(fileName);
        }
    }

    /// <summary>
    /// With a read-only original open, the translate dialog is fed the working subtitle only - the
    /// display-only rows are not part of it. They are dropped before the translations are matched
    /// with the rows by index, so no row takes another row's translation and no reference row has
    /// its text overwritten with nothing.
    /// </summary>
    [AvaloniaFact]
    public void RemoveReferenceOnlyRows_LeavesTheRowsAlignedWithTheTranslatedSubtitle()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", string.Empty, 0, 2000);
            AddLine(vm, "Translated two", string.Empty, 4000, 6000);

            var reference = new Subtitle();
            reference.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
            reference.Paragraphs.Add(new Paragraph("Reference only - no translation", 2000, 4000));
            reference.Paragraphs.Add(new Paragraph("Reference two", 4000, 6000));
            var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
            InvokeImportOriginalSubtitle(vm, "reference.srt", reference, match, isReadOnly: true);

            // What the translate dialog was handed - the display-only row is not in it.
            var translated = vm.GetUpdateSubtitle();
            Assert.Equal(2, translated.Paragraphs.Count);
            Assert.Equal(3, vm.Subtitles.Count);

            InvokeRemoveReferenceOnlyRows(vm);

            Assert.Equal(translated.Paragraphs.Count, vm.Subtitles.Count);
            Assert.Equal(
                new[] { "Translated one", "Translated two" },
                vm.Subtitles.Select(p => p.Text));
        }
        finally
        {
            CloseWindow(window, vm);
        }
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

    private static void InvokeCaptureOriginalFromTranslatedRows(MainViewModel vm)
    {
        Invoke(vm, "CaptureOriginalFromTranslatedRows");
    }

    private static void InvokeRemoveReferenceOnlyRows(MainViewModel vm)
    {
        Invoke(vm, "RemoveReferenceOnlyRows");
    }

    private static void InvokeSetSubtitles(MainViewModel vm, Subtitle subtitle, Subtitle subtitleOriginal)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "SetSubtitles",
                         BindingFlags.Instance | BindingFlags.NonPublic,
                         null,
                         new[] { typeof(Subtitle), typeof(Subtitle) },
                         null)
                     ?? throw new InvalidOperationException("SetSubtitles(Subtitle, Subtitle) not found");

        method.Invoke(vm, new object?[] { subtitle, subtitleOriginal });
    }

    private static void InvokeImportOriginalSubtitle(
        MainViewModel vm, string fileName, Subtitle subtitle, ImportOriginalHelper.OriginalMatch? match, bool isReadOnly)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ImportOriginalSubtitle not found");

        method.Invoke(vm, new object?[] { 0, fileName, subtitle, match, isReadOnly });
    }

    private static async Task<bool> InvokeSaveSubtitleOriginal(MainViewModel vm, bool isAutoSave)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "SaveSubtitleOriginal", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("SaveSubtitleOriginal not found");

        return await (Task<bool>)method.Invoke(vm, new object[] { isAutoSave })!;
    }

    private static void Invoke(MainViewModel vm, string name)
    {
        var method = typeof(MainViewModel).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException($"{name} not found");

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
