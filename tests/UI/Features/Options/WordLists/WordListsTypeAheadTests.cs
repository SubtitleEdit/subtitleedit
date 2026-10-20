using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Options.WordLists;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.ObjectModel;

namespace UITests.Features.Options.WordLists;

/// <summary>
/// Typing in a word list jumps to the first entry starting with the typed text, and letters typed
/// in quick succession narrow the match ("me" finds "Mette", not "Mark") - issue #16078.
/// </summary>
public class WordListsTypeAheadTests : IDisposable
{
    private sealed class StubFolderHelper : IFolderHelper
    {
        public Task<string> PickFolderAsync(Window window, string title, string? suggestedStartFolder = null) => Task.FromResult(string.Empty);
        public Task OpenFolder(Window window, string folder) => Task.CompletedTask;
        public Task OpenFolderWithFileSelected(Window window, string selectedFile) => Task.CompletedTask;
    }

    private readonly string _folder;
    private readonly string? _previousFolder;

    public WordListsTypeAheadTests()
    {
        // Empty dictionaries folder: no language, so the window's own load leaves the lists empty.
        _previousFolder = Se.DictionariesFolder;
        _folder = Path.Combine(Path.GetTempPath(), "se-wordlists-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        Se.DictionariesFolder = _folder;
    }

    public void Dispose()
    {
        Se.DictionariesFolder = _previousFolder!;
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, true);
        }
    }

    private static (WordListsWindow Window, WordListsViewModel Vm, ListBox[] ListBoxes) ShowWindow()
    {
        var vm = new WordListsViewModel(new StubFolderHelper(), new StubWindowService());
        var window = new WordListsWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        vm.Names = new ObservableCollection<string>(["Anna", "Mark", "Mette", "Zoe"]);
        vm.UserWords = new ObservableCollection<string>(["apple", "melon", "mesh", "zebra"]);
        vm.OcrFixes = new ObservableCollection<OcrFixItem>([new("ab", "x"), new("me", "y"), new("rn", "m")]);
        Dispatcher.UIThread.RunJobs();

        return (window, vm, window.GetLogicalDescendants().OfType<ListBox>().ToArray());
    }

    // A click lands on an item, not the list itself - focus the first item like the user would.
    private static void FocusItem(ListBox listBox)
    {
        listBox.ContainerFromIndex(0)!.Focus();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void Names_SingleLetter_JumpsToFirstMatch()
    {
        var (window, vm, listBoxes) = ShowWindow();
        try
        {
            FocusItem(listBoxes[0]);
            window.KeyTextInput("m");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Mark", vm.SelectedName);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Names_SeveralLetters_NarrowTheMatch()
    {
        var (window, vm, listBoxes) = ShowWindow();
        try
        {
            FocusItem(listBoxes[0]);
            window.KeyTextInput("m");
            window.KeyTextInput("e");
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("Mette", vm.SelectedName);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void UserWordsAndOcrFixes_JumpToFirstMatch()
    {
        var (window, vm, listBoxes) = ShowWindow();
        try
        {
            FocusItem(listBoxes[1]);
            window.KeyTextInput("mes");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("mesh", vm.SelectedUserWord);

            FocusItem(listBoxes[2]);
            window.KeyTextInput("r");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("rn", vm.SelectedOcrFix?.Find);
        }
        finally
        {
            window.Close();
        }
    }
}
