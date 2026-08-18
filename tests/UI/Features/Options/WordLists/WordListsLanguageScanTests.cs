using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Options.WordLists;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UITests.Features.Options.WordLists;

/// <summary>
/// The word-list editor's language dropdown lists whatever the dictionaries folder holds. OCR
/// replace lists are named after the three-letter ISO code, and a language must show up whether the
/// shipped list ("ell_OCRFixReplaceList.xml"), the user's own ("ell_OCRFixReplaceList_User.xml"),
/// or both are on disk (issue #13814).
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
    public void OcrFixList_PutsTheLanguageInTheDropdown()
    {
        Touch("ell_OCRFixReplaceList.xml");

        var vm = MakeViewModel();

        Assert.Contains(vm.Languages, l => l.TwoLetterISOLanguageName == "el");
    }

    [AvaloniaFact]
    public void UserOcrFixListAlone_PutsTheLanguageInTheDropdown()
    {
        Touch("ell_OCRFixReplaceList_User.xml");

        var vm = MakeViewModel();

        Assert.Contains(vm.Languages, l => l.TwoLetterISOLanguageName == "el");
    }

    [AvaloniaFact]
    public void ShippedAndUserList_IsListedOnce()
    {
        Touch("ell_OCRFixReplaceList.xml");
        Touch("ell_OCRFixReplaceList_User.xml");

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
