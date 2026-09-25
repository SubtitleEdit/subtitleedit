using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Shared.PromptFilesSaved;
using Nikse.SubtitleEdit.Logic.Media;
using System.IO;
using Xunit;

namespace UITests.Features.Shared;

/// <summary>
/// The batch burn-in "done" dialog: one card per output, a summary of where they went, and the
/// jobs that produced nothing kept visible with their status.
/// </summary>
public class PromptFilesSavedTests
{
    [AvaloniaFact]
    public void OneOutputFolder_IsNamedOnceInTheHeader()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-prompt-files-saved");
        var files = new[]
        {
            new SavedFileItem(Path.Combine(folder, "a.mp4"), true, "Done"),
            new SavedFileItem(Path.Combine(folder, "b.mp4"), true, "Done"),
            new SavedFileItem(Path.Combine(folder, "c.mp4"), false, "Skipped"),
        };

        var vm = new PromptFilesSavedViewModel(new FolderHelper());
        vm.Initialize("Video files generated", "2 of 3 videos generated", files, "1:02");
        _ = new PromptFilesSavedWindow(vm);

        Assert.False(vm.AllSucceeded);
        Assert.True(vm.HasOutputFolder);
        Assert.Contains(folder, vm.FolderSummary);
        Assert.All(vm.Files, p => Assert.False(p.ShowFolder));
        Assert.True(vm.HasElapsedChip);
        Assert.Equal("Skipped", vm.Files[2].StatusText);
    }

    [AvaloniaFact]
    public void SeveralOutputFolders_ShowTheFolderPerFile()
    {
        var files = new[]
        {
            new SavedFileItem(Path.Combine(Path.GetTempPath(), "one", "a.mp4"), true, "Done"),
            new SavedFileItem(Path.Combine(Path.GetTempPath(), "two", "b.mp4"), true, "Done"),
        };

        var vm = new PromptFilesSavedViewModel(new FolderHelper());
        vm.Initialize("Video files generated", "2 videos generated", files);

        Assert.True(vm.AllSucceeded);
        Assert.Equal(string.Format(Nikse.SubtitleEdit.Logic.Config.Se.Language.General.SavedInXFolders, 2), vm.FolderSummary);
        Assert.All(vm.Files, p => Assert.True(p.ShowFolder));
        Assert.False(vm.HasElapsedChip);
    }
}
