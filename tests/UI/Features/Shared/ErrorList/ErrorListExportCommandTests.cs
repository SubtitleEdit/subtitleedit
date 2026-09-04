using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Shared;

/// <summary>
/// The export menu of "List errors" (#14379): what the picker returns is what gets written,
/// a cancelled picker writes nothing, and every target exports the rows as shown - so an
/// active summary-card filter is part of the export.
/// </summary>
public class ErrorListExportCommandTests
{
    /// <summary>Hands out a file name in a temp folder instead of opening a picker.</summary>
    private sealed class SaveToTempFileHelper : StubFileHelper
    {
        public string Folder { get; } = Path.Combine(Path.GetTempPath(), "SubtitleEdit.ErrorListExport", Guid.NewGuid().ToString("N"));
        public string? Cancelled { get; set; }
        public string LastSuggestedFileName { get; private set; } = string.Empty;

        public override Task<string> PickSaveFile(Visual sender, string extension, string suggestedFileName, string title)
        {
            LastSuggestedFileName = suggestedFileName;
            if (Cancelled != null)
            {
                return Task.FromResult(string.Empty);
            }

            Directory.CreateDirectory(Folder);
            return Task.FromResult(Path.Combine(Folder, "errors" + extension));
        }
    }

    /// <summary>The "file saved" prompt is a dialog the test has no use for - it is discarded by the caller.</summary>
    private sealed class NoDialogWindowService : IWindowService
    {
        public T ShowWindow<T>(Window owner, Action<T>? configure = null) where T : Window => throw new NotSupportedException();

        public TViewModel ShowWindow<T, TViewModel>(Window owner, Action<T, TViewModel>? configure = null)
            where T : Window where TViewModel : class => throw new NotSupportedException();

        public TViewModel ShowIndependentWindow<T, TViewModel>(Action<T, TViewModel>? configure = null)
            where T : Window where TViewModel : class => throw new NotSupportedException();

        public Task<T> ShowDialogAsync<T>(Window owner, Action<T>? configure = null) where T : Window => throw new NotSupportedException();

        public Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
            Window owner,
            Action<TViewModel>? configureViewModel = null,
            Action<TWindow>? configureWindow = null)
            where TWindow : Window where TViewModel : class => Task.FromResult<TViewModel>(null!);

        public Task<TViewModel> ShowWithOwnerHiddenAsync<TWindow, TViewModel>(
            Window owner,
            IReadOnlyList<Window?> companions,
            Action<TViewModel>? configureViewModel = null)
            where TWindow : Window where TViewModel : class => throw new NotSupportedException();
    }

    private static List<ErrorListItem> MakeItems()
    {
        var items = new List<ErrorListItem>();
        var errors = new[]
        {
            new LineError(LineErrorType.CharactersPerSecond, "30 > 25"),
            new LineError(LineErrorType.Overlap, "from previous: 120 ms"),
            new LineError(LineErrorType.CharactersPerSecond, "42 > 25"),
        };

        for (var i = 0; i < errors.Length; i++)
        {
            var line = new SubtitleLineViewModel(new Paragraph("Line " + (i + 1), i * 2000, i * 2000 + 1500), null!)
            {
                Number = i + 1,
            };
            items.Add(new ErrorListItem(line, errors[i]));
        }

        return items;
    }

    private static (ErrorListViewModel Vm, ErrorListWindow Window, SaveToTempFileHelper FileHelper) MakeWindow()
    {
        var fileHelper = new SaveToTempFileHelper();
        var vm = new ErrorListViewModel(fileHelper, new NoDialogWindowService());
        vm.Initialize(MakeItems(), 100, "/movies/The Long Goodbye.srt");
        var window = new ErrorListWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (vm, window, fileHelper);
    }

    [AvaloniaFact]
    public void ExportMenu_HasTheFourTargets()
    {
        var (vm, window, _) = MakeWindow();

        var button = window.GetLogicalDescendants().OfType<Button>().First(b => Equals(b.Content, Se.Language.General.ExportDotDotDot));
        var items = ((MenuFlyout)button.Flyout!).Items.Cast<MenuItem>().ToList();

        Assert.Equal(
            new[]
            {
                Se.Language.General.CopyToClipboard,
                Se.Language.ErrorList.ExportAsText,
                Se.Language.ErrorList.ExportAsExcel,
                Se.Language.ErrorList.ExportAsHtml,
            },
            items.Select(i => i.Header as string));

        Assert.Equal(
            new object?[] { vm.CopyToClipboardCommand, vm.ExportTextCommand, vm.ExportExcelCommand, vm.ExportHtmlCommand },
            items.Select(i => i.Command));

        Assert.True(button.IsEnabled);
        window.Close();
    }

    [AvaloniaFact]
    public void ExportButton_IsDisabledWithoutErrors()
    {
        var vm = new ErrorListViewModel(new StubFileHelper(), new StubWindowService());
        vm.Initialize(new List<ErrorListItem>(), 100);
        var window = new ErrorListWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var button = window.GetLogicalDescendants().OfType<Button>().First(b => Equals(b.Content, Se.Language.General.ExportDotDotDot));

        Assert.False(vm.CanExport);
        Assert.False(button.IsEnabled);
        window.Close();
    }

    [AvaloniaFact]
    public async Task ExportHtml_WritesWhatThePickerReturned()
    {
        var (vm, window, fileHelper) = MakeWindow();

        await vm.ExportHtmlCommand.ExecuteAsync(null);

        var fileName = Path.Combine(fileHelper.Folder, "errors.html");
        Assert.True(File.Exists(fileName));
        var html = await File.ReadAllTextAsync(fileName);
        Assert.StartsWith("<!DOCTYPE html>", html);
        Assert.Contains("Line 3", html);

        // The suggested name follows the subtitle, so several files do not all land on "errors".
        Assert.StartsWith("The Long Goodbye-errors", fileHelper.LastSuggestedFileName);

        Directory.Delete(fileHelper.Folder, true);
        window.Close();
    }

    [AvaloniaFact]
    public async Task ExportText_AndExcel_WriteTheirOwnFormat()
    {
        var (vm, window, fileHelper) = MakeWindow();

        await vm.ExportTextCommand.ExecuteAsync(null);
        await vm.ExportExcelCommand.ExecuteAsync(null);

        Assert.Contains("Line 2", await File.ReadAllTextAsync(Path.Combine(fileHelper.Folder, "errors.txt")));

        // "PK": the workbook really is a zip, not a csv with an .xlsx name.
        var xlsx = await File.ReadAllBytesAsync(Path.Combine(fileHelper.Folder, "errors.xlsx"));
        Assert.Equal(new byte[] { 0x50, 0x4B }, xlsx.Take(2).ToArray());

        Directory.Delete(fileHelper.Folder, true);
        window.Close();
    }

    [AvaloniaFact]
    public async Task CancelledPicker_WritesNothing()
    {
        var (vm, window, fileHelper) = MakeWindow();
        fileHelper.Cancelled = "yes";

        await vm.ExportTextCommand.ExecuteAsync(null);

        Assert.False(Directory.Exists(fileHelper.Folder));
        window.Close();
    }

    [AvaloniaFact]
    public async Task Export_FollowsTheActiveCardFilter()
    {
        var (vm, window, fileHelper) = MakeWindow();

        vm.SetFilterCommand.Execute(vm.Cards.First(c => Equals(c.Key, LineErrorType.Overlap)));
        await vm.ExportTextCommand.ExecuteAsync(null);

        var text = await File.ReadAllTextAsync(Path.Combine(fileHelper.Folder, "errors.txt"));
        Assert.Contains("Line 2", text);
        Assert.DoesNotContain("Line 1", text);
        Assert.DoesNotContain("Line 3", text);

        Directory.Delete(fileHelper.Folder, true);
        window.Close();
    }
}
