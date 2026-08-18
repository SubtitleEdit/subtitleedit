using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Platform.Storage;
using Nikse.SubtitleEdit.Features.Options.WordLists;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UITests.Features.Options.WordLists;

/// <summary>
/// The word-list editor's language dropdown lists whatever the dictionaries folder holds. It used to
/// build the list by walking every known culture and probing for the three-letter file name Subtitle
/// Edit writes, so an OCR replace list a user dropped in under the two-letter code was invisible -
/// no way to select the language, no way to edit the list (issue #13814).
/// </summary>
public class WordListsLanguageScanTests : IDisposable
{
    private sealed class StubFolderHelper : IFolderHelper
    {
        public Task<string> PickFolderAsync(Window window, string title) => Task.FromResult(string.Empty);
        public Task OpenFolder(Window window, string folder) => Task.CompletedTask;
        public Task OpenFolderWithFileSelected(Window window, string selectedFile) => Task.CompletedTask;
    }

    private readonly string _folder;
    private readonly string? _previousFolder;

    public WordListsLanguageScanTests()
    {
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

    private void Touch(string fileName)
    {
        File.WriteAllText(Path.Combine(_folder, fileName), "<ReplaceList><WholeWords/></ReplaceList>");
    }

    private static WordListsViewModel MakeViewModel() => new(new StubFolderHelper());

    [AvaloniaFact]
    public void TwoLetterOcrFixList_PutsTheLanguageInTheDropdown()
    {
        Touch("el_OCRFixReplaceList_User.xml");

        var vm = MakeViewModel();

        Assert.Contains(vm.Languages, l => l.TwoLetterISOLanguageName == "el");
    }

    [AvaloniaFact]
    public void ThreeLetterOcrFixList_StillPutsTheLanguageInTheDropdown()
    {
        Touch("ell_OCRFixReplaceList.xml");

        var vm = MakeViewModel();

        Assert.Contains(vm.Languages, l => l.TwoLetterISOLanguageName == "el");
    }

    [AvaloniaFact]
    public void OneLanguageUnderBothSpellings_IsListedOnce()
    {
        Touch("ell_OCRFixReplaceList.xml");
        Touch("el_OCRFixReplaceList_User.xml");

        var vm = MakeViewModel();

        Assert.Single(vm.Languages.Where(l => l.TwoLetterISOLanguageName == "el"));
    }

    [AvaloniaFact]
    public void FolderWithoutLists_ListsNothing()
    {
        var vm = MakeViewModel();

        Assert.Empty(vm.Languages);
    }
}
