using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic.Media;

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
    private const string BoxText = @"{\p1}m 0 0 l 10 0 10 10 0 10{\p0}";

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

    /// <summary>
    /// "Save as" onto the file the translation was made from replaces that file with the
    /// translation. The same Ctrl+S then saved the (unsaved) original to its old file name - the
    /// same file - so the translation was overwritten straight away and the file looked untouched
    /// (issue #15999).
    /// </summary>
    [AvaloniaFact]
    public async Task Save_TranslationSavedAsOverTheOriginalFile_KeepsTheTranslation()
    {
        var fileName = Path.Combine(Path.GetTempPath(), $"se-translation-over-original-{Guid.NewGuid():N}.srt");
        await File.WriteAllTextAsync(fileName, "1\r\n00:00:00,000 --> 00:00:02,000\r\nSource line one\r\n\r\n");
        var (window, vm) = CreateMainViewModel(new PickFileHelper(fileName));

        try
        {
            SetUpTranslationOf(vm, fileName);

            await vm.CommandFileSaveCommand.ExecuteAsync(null);

            var onDisk = await File.ReadAllTextAsync(fileName);
            Assert.Contains("Vertaalde regel een", onDisk);
            Assert.DoesNotContain("Source line one", onDisk);
            Assert.Equal(string.Empty, GetField("_subtitleFileNameOriginal").GetValue(vm));

            // A second Ctrl+S must not bring the original back over the translation either.
            await vm.CommandFileSaveCommand.ExecuteAsync(null);

            onDisk = await File.ReadAllTextAsync(fileName);
            Assert.Contains("Vertaalde regel een", onDisk);
            Assert.DoesNotContain("Source line one", onDisk);

            // The original was never written anywhere, so closing must still offer to save it.
            Assert.NotEqual(GetField("_changeSubtitleHashOriginal").GetValue(vm), vm.GetFastHashOriginal());
        }
        finally
        {
            CloseWindow(window, vm);
            File.Delete(fileName);
        }
    }

    /// <summary>
    /// The mirror: "Save original as" onto the translation's file hands that file to the original,
    /// so the translation becomes untitled and the next Ctrl+S asks where it goes (#15999).
    /// </summary>
    [AvaloniaFact]
    public async Task SaveOriginalAs_OverTheTranslationFile_DetachesTheTranslation()
    {
        var translationFileName = Path.Combine(Path.GetTempPath(), $"se-translation-{Guid.NewGuid():N}.srt");
        var (window, vm) = CreateMainViewModel(new PickFileHelper(translationFileName));

        try
        {
            SetUpTranslationOf(vm, string.Empty);
            SetPrivateField(vm, "_subtitleFileName", translationFileName);
            SetPrivateField(vm, "_converted", false);

            var method = typeof(MainViewModel).GetMethod(
                             "SaveSubtitleOriginalAs", BindingFlags.Instance | BindingFlags.NonPublic)
                         ?? throw new InvalidOperationException("SaveSubtitleOriginalAs not found");
            var saved = await (Task<bool>)method.Invoke(vm, null)!;

            Assert.True(saved);
            Assert.Contains("Source line one", await File.ReadAllTextAsync(translationFileName));
            Assert.Equal(string.Empty, GetField("_subtitleFileName").GetValue(vm));
            Assert.Equal(translationFileName, GetField("_subtitleFileNameOriginal").GetValue(vm));
        }
        finally
        {
            CloseWindow(window, vm);
            File.Delete(translationFileName);
        }
    }

    // The state a translation made from the loaded file leaves behind: the rows hold the
    // translation, the original column (and original subtitle) the source text, and the original
    // points at the source file while the translation is still untitled.
    private static void SetUpTranslationOf(MainViewModel vm, string originalFileName)
    {
        vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(f => f is SubRip);
        AddLine(vm, "Vertaalde regel een", "Source line one", 0, 2000);
        vm.IsOriginalReadOnly = false;
        vm.ShowColumnOriginalText = true;
        InvokeCaptureOriginalFromTranslatedRows(vm);
        SetPrivateField(vm, "_subtitleFileNameOriginal", originalFileName);
        SetPrivateField(vm, "_subtitleFileName", string.Empty);
        SetPrivateField(vm, "_converted", true);
    }

    private sealed class PickFileHelper : StubFileHelper
    {
        private readonly string _fileName;

        public PickFileHelper(string fileName)
        {
            _fileName = fileName;
        }

        public override Task<FileHelperSubtitleSavePickerResult?> PickSaveSubtitleFileAs(Visual sender, SubtitleFormat currentFormat, string suggestedFileName, string title)
            => Task.FromResult<FileHelperSubtitleSavePickerResult?>(new FileHelperSubtitleSavePickerResult { FileName = _fileName, SubtitleFormat = currentFormat });

        public override Task<string> PickSaveSubtitleFile(Visual sender, SubtitleFormat currentFormat, string suggestedFileName, string title)
            => Task.FromResult(_fileName);
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel(IFileHelper? fileHelper = null)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        if (fileHelper != null)
        {
            services.AddSingleton(fileHelper);
        }

        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    /// <summary>
    /// A rebuild with the non-matching lines on screen leaves every row blank, and the rows without
    /// an original line are skipped - so the rebuilt original had no lines at all, and the check for
    /// an all-blank original passed. The empty file then read back as "Unknown subtitle format"
    /// (issue #16499).
    /// </summary>
    [AvaloniaFact]
    public async Task SaveSubtitleOriginal_ShowingNonMatchingLinesAllBlank_LeavesTheFileUntouched()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = await WriteOriginalFile(lineCount: 3);
        var onDisk = await File.ReadAllTextAsync(fileName);
        try
        {
            ImportOriginal(vm, fileName, mismatch: true);
            Assert.True(vm.IsShowingOriginalNonMatchingLines);

            var blank = new Subtitle(vm.GetUpdateSubtitle(), false);
            InvokeSetSubtitles(vm, blank, null);

            var saved = await InvokeSaveSubtitleOriginal(vm, isAutoSave: false);

            Assert.False(saved);
            Assert.Equal(onDisk, await File.ReadAllTextAsync(fileName));
        }
        finally
        {
            File.Delete(fileName);
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public async Task InsertBackgroundBoxRows_KeepsTheOriginalColumnAndPutsEachBoxAfterItsLine()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = await WriteOriginalFile(lineCount: 2);
        try
        {
            ImportOriginal(vm, fileName, mismatch: false);

            InvokeInsertBackgroundBoxRows(vm, MakeBoxes(vm));

            Assert.Equal(
                new[] { "Trans 1", BoxText, "Trans 2", BoxText },
                vm.Subtitles.Select(p => p.Text));
            Assert.Equal(
                new[] { "Source 1", string.Empty, "Source 2", string.Empty },
                vm.Subtitles.Select(p => p.OriginalText));
            Assert.Equal(new[] { 1, 2, 3, 4 }, vm.Subtitles.Select(p => p.Number));
        }
        finally
        {
            File.Delete(fileName);
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The boxes belong to the working subtitle only: the saved original must be the same lines it
    /// was, with no blank line per box.
    /// </summary>
    [AvaloniaFact]
    public async Task InsertBackgroundBoxRows_ThenSavingTheOriginal_AddsNoLinesToIt()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = await WriteOriginalFile(lineCount: 2);
        try
        {
            ImportOriginal(vm, fileName, mismatch: false);

            InvokeInsertBackgroundBoxRows(vm, MakeBoxes(vm));
            var saved = await InvokeSaveSubtitleOriginal(vm, isAutoSave: false);

            Assert.True(saved);
            var reloaded = Subtitle.Parse(fileName)!;
            Assert.Equal(new[] { "Source 1", "Source 2" }, reloaded.Paragraphs.Select(p => p.Text));
        }
        finally
        {
            File.Delete(fileName);
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public async Task InsertBackgroundBoxRows_WithNonMatchingLinesOnScreen_KeepsEveryOriginalLine()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = await WriteOriginalFile(lineCount: 3);
        try
        {
            ImportOriginal(vm, fileName, mismatch: true);

            InvokeInsertBackgroundBoxRows(vm, MakeBoxes(vm));
            var saved = await InvokeSaveSubtitleOriginal(vm, isAutoSave: false);

            Assert.True(saved);
            var reloaded = Subtitle.Parse(fileName)!;
            Assert.Equal(new[] { "Source 1", "Source 2", "Source 3" }, reloaded.Paragraphs.Select(p => p.Text));
        }
        finally
        {
            File.Delete(fileName);
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public async Task InsertBackgroundBoxRows_LeavesACleanOriginalClean()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = await WriteOriginalFile(lineCount: 2);
        try
        {
            ImportOriginal(vm, fileName, mismatch: false);

            InvokeInsertBackgroundBoxRows(vm, MakeBoxes(vm));

            Assert.Equal((int)GetField("_changeSubtitleHashOriginal").GetValue(vm)!, vm.GetFastHashOriginal());
        }
        finally
        {
            File.Delete(fileName);
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The rebuilds re-fill the rows from the original by index, so the original has to keep its
    /// (blank) line for a box in memory - only the saved file leaves it out.
    /// </summary>
    [AvaloniaFact]
    public async Task InsertBackgroundBoxRows_ThenARebuild_KeepsTheOriginalOnItsRow()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = await WriteOriginalFile(lineCount: 2);
        try
        {
            ImportOriginal(vm, fileName, mismatch: false);
            InvokeInsertBackgroundBoxRows(vm, MakeBoxes(vm));
            await InvokeSaveSubtitleOriginal(vm, isAutoSave: false);

            var rebuilt = new Subtitle(vm.GetUpdateSubtitle(), false);
            var method = typeof(MainViewModel).GetMethod(
                             "SetSubtitlesKeepingOriginal", BindingFlags.Instance | BindingFlags.NonPublic)
                         ?? throw new InvalidOperationException("SetSubtitlesKeepingOriginal not found");
            method.Invoke(vm, new object[] { rebuilt });

            Assert.Equal(
                new[] { "Source 1", string.Empty, "Source 2", string.Empty },
                vm.Subtitles.Select(p => p.OriginalText));
        }
        finally
        {
            File.Delete(fileName);
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// Opening the original again once the boxes exist: every original line matches, so nothing
    /// marks the rows as not lining up - and each box added a blank line to the saved file.
    /// </summary>
    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task OriginalOpenedAfterTheBoxes_SavesWithoutBlankLines(bool showAllLines)
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = await WriteOriginalFile(lineCount: 2);
        try
        {
            AddLine(vm, "Trans 1", string.Empty, 0, 2000);
            AddLine(vm, BoxText, string.Empty, 0, 2000);
            AddLine(vm, "Trans 2", string.Empty, 2000, 4000);
            AddLine(vm, BoxText, string.Empty, 2000, 4000);

            var original = Subtitle.Parse(fileName)!;
            var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, original);
            if (showAllLines)
            {
                InvokeImportOriginalSubtitle(vm, fileName, original, match, false);
            }
            else
            {
                InvokeImportOriginalSubtitle(vm, fileName, match.Projection, null, false);
            }

            var saved = await InvokeSaveSubtitleOriginal(vm, isAutoSave: false);

            Assert.True(saved);
            var reloaded = Subtitle.Parse(fileName)!;
            Assert.Equal(new[] { "Source 1", "Source 2" }, reloaded.Paragraphs.Select(p => p.Text));
            Assert.Equal(new[] { 1, 2 }, reloaded.Paragraphs.Select(p => p.Number));
        }
        finally
        {
            File.Delete(fileName);
            CloseWindow(window, vm);
        }
    }

    /// <summary>A shape that really is in the original stays there, blank or not.</summary>
    [AvaloniaFact]
    public async Task SavingTheOriginal_KeepsADrawingOfItsOwn()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = await WriteOriginalFile(lineCount: 2);
        try
        {
            ImportOriginal(vm, fileName, mismatch: false);
            vm.Subtitles[1].Text = BoxText;
            vm.Subtitles[1].OriginalText = BoxText;

            var saved = await InvokeSaveSubtitleOriginal(vm, isAutoSave: false);

            Assert.True(saved);
            Assert.Equal(2, Subtitle.Parse(fileName)!.Paragraphs.Count);
        }
        finally
        {
            File.Delete(fileName);
            CloseWindow(window, vm);
        }
    }

    private static async Task<string> WriteOriginalFile(int lineCount)
    {
        var fileName = Path.Combine(Path.GetTempPath(), $"se-box-original-{Guid.NewGuid():N}.srt");
        var subtitle = new Subtitle();
        for (var i = 0; i < lineCount; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph($"Source {i + 1}", i * 2000, (i + 1) * 2000));
        }

        await File.WriteAllTextAsync(fileName, subtitle.ToText(new SubRip()));
        return fileName;
    }

    private static void ImportOriginal(MainViewModel vm, string fileName, bool mismatch)
    {
        AddLine(vm, "Trans 1", string.Empty, 0, 2000);
        AddLine(vm, "Trans 2", string.Empty, 2000, 4000);

        var original = Subtitle.Parse(fileName)!;
        var match = mismatch ? ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, original) : null;
        InvokeImportOriginalSubtitle(vm, fileName, original, match, false);
    }

    private static List<Paragraph> MakeBoxes(MainViewModel vm)
    {
        return vm.Subtitles
            .Where(p => !p.IsReferenceOnly)
            .Select(p => new Paragraph(BoxText, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds))
            .ToList();
    }

    private static void InvokeInsertBackgroundBoxRows(MainViewModel vm, List<Paragraph> boxes)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "InsertBackgroundBoxRows", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("InsertBackgroundBoxRows not found");

        method.Invoke(vm, new object[] { boxes });
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

    private static void InvokeSetSubtitles(MainViewModel vm, Subtitle subtitle, Subtitle? subtitleOriginal)
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
