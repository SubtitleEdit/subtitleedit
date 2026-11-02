using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Translate;

/// <summary>
/// Find and replace work on the translation column (#16556). There is no text box to hold a caret,
/// so the last match is the search position; that is what most of these pin down.
/// </summary>
public class AutoTranslateFindReplaceTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static (AutoTranslateViewModel Vm, AutoTranslateWindow Window) OpenWindow(params string[] translations)
    {
        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        foreach (var translation in translations)
        {
            subtitle.Paragraphs.Add(new Paragraph("Source " + translation, 0, 2000));
        }

        vm.Initialize(subtitle);

        var window = new AutoTranslateWindow(vm) { Width = 1000, Height = 700 };
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        for (var i = 0; i < translations.Length; i++)
        {
            vm.OnTranslationEdited(vm.Rows[i], translations[i]);
        }

        // Without an owner the "continue from the top?" questions are skipped - nobody answers them here.
        vm.Window = null;
        return (vm, window);
    }

    private static ReplaceViewModel Replacer(AutoTranslateViewModel vm, string search, string replace)
    {
        var replacer = new ReplaceViewModel
        {
            SearchText = search,
            ReplaceText = replace,
            WholeWord = false,
            FindMode = FindService.FindMode.CaseSensitive,
        };
        replacer.InitializeFindData(new FindService(), new List<string>(), string.Empty, vm);
        return replacer;
    }

    private static FindViewModel Finder(AutoTranslateViewModel vm, string search)
    {
        var finder = new FindViewModel
        {
            SearchText = search,
            WholeWord = false,
            FindMode = FindService.FindMode.CaseSensitive,
        };
        finder.InitializeFindData(new FindService(), new List<string>(), string.Empty, vm);
        return finder;
    }

    [AvaloniaFact]
    public async Task FindNext_WalksTheMatchesInTheTranslationColumn()
    {
        var (vm, window) = OpenWindow("no match", "one cat, two cat", "another cat");
        var finder = Finder(vm, "cat");

        await finder.FindNextCommand.ExecuteAsync(null);
        Assert.Same(vm.Rows[1], vm.SelectedTranslateRow);

        await finder.FindNextCommand.ExecuteAsync(null);
        Assert.Same(vm.Rows[1], vm.SelectedTranslateRow);

        await finder.FindNextCommand.ExecuteAsync(null);
        Assert.Same(vm.Rows[2], vm.SelectedTranslateRow);
        Assert.Same(vm.Rows[2], vm.RowGrid!.SelectedItem);
        window.Close();
    }

    [AvaloniaFact]
    public async Task FindPrevious_WalksBackFromTheEndOfTheSelectedRow()
    {
        var (vm, window) = OpenWindow("cat one", "no match", "cat, cat");
        vm.SelectedTranslateRow = vm.Rows[2];
        var finder = Finder(vm, "cat");

        await finder.FindPreviousCommand.ExecuteAsync(null);
        Assert.Same(vm.Rows[2], vm.SelectedTranslateRow);

        await finder.FindPreviousCommand.ExecuteAsync(null);
        Assert.Same(vm.Rows[2], vm.SelectedTranslateRow);

        await finder.FindPreviousCommand.ExecuteAsync(null);
        Assert.Same(vm.Rows[0], vm.SelectedTranslateRow);
        window.Close();
    }

    [AvaloniaFact]
    public async Task Find_DoesNotMatchTheSourceColumn()
    {
        var (vm, window) = OpenWindow("translated");
        var finder = Finder(vm, "Source");

        await finder.FindNextCommand.ExecuteAsync(null);

        Assert.False(finder.ResultFound);
        Assert.False(string.IsNullOrEmpty(finder.CountResult));
        window.Close();
    }

    [AvaloniaFact]
    public async Task ReplaceAll_ChangesTranslationsAndLeavesTheSourceAlone()
    {
        var (vm, window) = OpenWindow("Source cat", "cat and cat", "dog");
        var replacer = Replacer(vm, "cat", "lion");

        await replacer.ReplaceAllCommand.ExecuteAsync(null);

        Assert.Equal("Source lion", vm.Rows[0].TranslatedText);
        Assert.Equal("lion and lion", vm.Rows[1].TranslatedText);
        Assert.Equal("dog", vm.Rows[2].TranslatedText);
        Assert.Equal("Source Source cat", vm.Rows[0].Text);
        Assert.Equal(3, replacer.ReplacedCount);
        window.Close();
    }

    [AvaloniaFact]
    public async Task Replace_ReplacesTheFoundMatchThenMovesToTheNext()
    {
        var (vm, window) = OpenWindow("cat and cat", "cat");
        var replacer = Replacer(vm, "cat", "cats");

        await replacer.FindNextCommand.ExecuteAsync(null);
        Assert.Equal("cat and cat", vm.Rows[0].TranslatedText);

        // The replacement holds the search text, so it must be stepped over, not found again.
        await replacer.ReplaceCommand.ExecuteAsync(null);
        Assert.Equal("cats and cat", vm.Rows[0].TranslatedText);

        await replacer.ReplaceCommand.ExecuteAsync(null);
        Assert.Equal("cats and cats", vm.Rows[0].TranslatedText);
        Assert.Same(vm.Rows[1], vm.SelectedTranslateRow);
        window.Close();
    }

    [AvaloniaFact]
    public async Task Replace_LeavesAMatchAloneOnceAnotherRowIsSelected()
    {
        var (vm, window) = OpenWindow("cat", "dog", "cat");
        var replacer = Replacer(vm, "cat", "lion");
        await replacer.FindNextCommand.ExecuteAsync(null);
        Assert.Same(vm.Rows[0], vm.SelectedTranslateRow);

        vm.SelectedTranslateRow = vm.Rows[1];
        await replacer.ReplaceCommand.ExecuteAsync(null);

        Assert.Equal("cat", vm.Rows[0].TranslatedText);
        Assert.Same(vm.Rows[2], vm.SelectedTranslateRow);
        window.Close();
    }

    [AvaloniaFact]
    public async Task Replace_WithAnEditedRow_DoesNotUseTheStaleMatch()
    {
        var (vm, window) = OpenWindow("a cat here");
        var replacer = Replacer(vm, "cat", "lion");
        await replacer.FindNextCommand.ExecuteAsync(null);

        vm.OnTranslationEdited(vm.Rows[0], "a dog here");
        await replacer.ReplaceCommand.ExecuteAsync(null);

        Assert.Equal("a dog here", vm.Rows[0].TranslatedText);
        window.Close();
    }

    [AvaloniaFact]
    public async Task ReplaceAll_ClearingEveryTranslation_DisablesOk()
    {
        var (vm, window) = OpenWindow("cat");
        Assert.True(vm.IsOkEnabled);
        var replacer = Replacer(vm, "cat", string.Empty);

        await replacer.ReplaceAllCommand.ExecuteAsync(null);

        Assert.False(vm.IsOkEnabled);
        window.Close();
    }

    [AvaloniaFact]
    public async Task Replace_WhileTranslating_ChangesNothing()
    {
        var (vm, window) = OpenWindow("cat");
        vm.IsTranslateEnabled = false;
        var replacer = Replacer(vm, "cat", "lion");

        await replacer.ReplaceAllCommand.ExecuteAsync(null);

        Assert.Equal("cat", vm.Rows[0].TranslatedText);
        vm.IsTranslateEnabled = true;
        window.Close();
    }

    [AvaloniaFact]
    public void ShowFind_CommitsAnOpenCellEdit()
    {
        var (vm, window) = OpenWindow("Hello");
        vm.Window = window;
        vm.SelectedTranslateRow = vm.Rows[0];
        Dispatcher.UIThread.RunJobs();

        var grid = vm.RowGrid!;
        var container = (Avalonia.Visual)grid.ContainerFromItem(vm.Rows[0])!;
        var cell = container.GetVisualDescendants().OfType<Border>()
            .Single(b => b.Cursor?.ToString() == new Cursor(StandardCursorType.Ibeam).ToString());
        cell.RaiseEvent(new PointerReleasedEventArgs(cell, new Pointer(0, PointerType.Mouse, true), cell,
            new Avalonia.Point(2, 2), 0, PointerPointProperties.None, KeyModifiers.None, MouseButton.Left)
        { RoutedEvent = InputElement.PointerReleasedEvent });
        Dispatcher.UIThread.RunJobs();

        var editor = Assert.IsType<TextBox>(cell.Child);
        editor.Text = "Edited";

        // The window service has no services in this test, so only the commit before it is checked.
        try
        {
            vm.ShowFind();
        }
        catch (Exception exception) when (exception is InvalidOperationException or NullReferenceException)
        {
        }

        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Edited", vm.Rows[0].TranslatedText);
        window.Close();
    }
}
