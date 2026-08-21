using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.TextToSpeech;

/// <summary>
/// A text-to-speech run writes one clip per subtitle line per pipeline step, and until #13332
/// nothing ever removed any of them - they went loose into the system temp folder (and before
/// that, next to the engine's own voices). <see cref="TtsRunFolder"/> is what makes the sweep on
/// window close a single directory delete.
/// </summary>
public class TtsRunFolderTests
{
    [Fact]
    public void Create_PutsTheRunInItsOwnFolderUnderTheFallbackBase()
    {
        using var _ = new SettingsScope("Video.TextToSpeech.GenerationFolder");
        Se.Settings.Video.TextToSpeech.GenerationFolder = string.Empty;

        var baseFolder = NewTestFolder();
        try
        {
            var runFolder = TtsRunFolder.Create(baseFolder);

            Assert.True(Directory.Exists(runFolder));
            Assert.Equal(baseFolder, Path.GetDirectoryName(runFolder));
            Assert.StartsWith("se-tts-", Path.GetFileName(runFolder), StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(baseFolder, recursive: true);
        }
    }

    [Fact]
    public void Create_UsesTheConfiguredGenerationFolderOverTheFallback()
    {
        using var _ = new SettingsScope("Video.TextToSpeech.GenerationFolder");

        var configured = NewTestFolder();
        var fallback = NewTestFolder();
        Se.Settings.Video.TextToSpeech.GenerationFolder = configured;
        try
        {
            var runFolder = TtsRunFolder.Create(fallback);

            Assert.Equal(configured, Path.GetDirectoryName(runFolder));
            Assert.Empty(Directory.GetFileSystemEntries(fallback));
        }
        finally
        {
            Directory.Delete(configured, recursive: true);
            Directory.Delete(fallback, recursive: true);
        }
    }

    [Fact]
    public void Create_FallsBackToTempWhenTheConfiguredFolderCannotBeCreated()
    {
        using var _ = new SettingsScope("Video.TextToSpeech.GenerationFolder");

        // A file where the folder should be: the same failure shape as an unplugged drive or a
        // generation folder the user has since deleted - which must not take the whole run down.
        var blocker = Path.Combine(Path.GetTempPath(), "se-tts-test-blocker-" + Guid.NewGuid().ToString("N"));
        File.WriteAllText(blocker, string.Empty);
        Se.Settings.Video.TextToSpeech.GenerationFolder = blocker;
        try
        {
            var runFolder = TtsRunFolder.Create(null);

            Assert.True(Directory.Exists(runFolder));
            Assert.Equal(
                Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar),
                Path.GetDirectoryName(runFolder)?.TrimEnd(Path.DirectorySeparatorChar));
            Directory.Delete(runFolder, recursive: true);
        }
        finally
        {
            File.Delete(blocker);
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheRunFolderAndEverythingInIt()
    {
        using var _ = new SettingsScope("Video.TextToSpeech.GenerationFolder");
        Se.Settings.Video.TextToSpeech.GenerationFolder = string.Empty;

        var baseFolder = NewTestFolder();
        try
        {
            var runFolder = TtsRunFolder.Create(baseFolder);
            File.WriteAllText(Path.Combine(runFolder, "0_clip.wav"), "x");
            Directory.CreateDirectory(Path.Combine(runFolder, "clone-references"));
            File.WriteAllText(Path.Combine(runFolder, "clone-references", "ref.wav"), "x");

            await TtsRunFolder.DeleteAsync(runFolder);

            Assert.False(Directory.Exists(runFolder));
        }
        finally
        {
            Directory.Delete(baseFolder, recursive: true);
        }
    }

    [Fact]
    public async Task DeleteAsync_LeavesFoldersItDidNotCreateAlone()
    {
        // The guard that keeps a mis-set generation folder from taking the user's own files with
        // it: only "se-tts-*" folders are ever deleted recursively.
        var notARunFolder = NewTestFolder();
        File.WriteAllText(Path.Combine(notARunFolder, "the-users-own-file.wav"), "x");
        try
        {
            await TtsRunFolder.DeleteAsync(notARunFolder);

            Assert.True(Directory.Exists(notARunFolder));
            Assert.True(File.Exists(Path.Combine(notARunFolder, "the-users-own-file.wav")));
        }
        finally
        {
            Directory.Delete(notARunFolder, recursive: true);
        }
    }

    [Fact]
    public async Task DeleteAsync_IsBestEffortForAFolderThatIsAlreadyGone()
    {
        using var _ = new SettingsScope("Video.TextToSpeech.GenerationFolder");
        Se.Settings.Video.TextToSpeech.GenerationFolder = string.Empty;

        var baseFolder = NewTestFolder();
        try
        {
            var runFolder = TtsRunFolder.Create(baseFolder);
            Directory.Delete(runFolder);

            await TtsRunFolder.DeleteAsync(runFolder);
            await TtsRunFolder.DeleteAsync(null);
            await TtsRunFolder.DeleteAsync(string.Empty);
        }
        finally
        {
            Directory.Delete(baseFolder, recursive: true);
        }
    }

    [Fact]
    public void DeleteIfEmpty_RemovesAFolderNothingWasWrittenInto()
    {
        // Opening the window and closing it again without generating must not leave a folder
        // behind, not even for users who turned the sweep off.
        using var _ = new SettingsScope("Video.TextToSpeech.GenerationFolder");
        Se.Settings.Video.TextToSpeech.GenerationFolder = string.Empty;

        var baseFolder = NewTestFolder();
        try
        {
            var empty = TtsRunFolder.Create(baseFolder);
            var used = TtsRunFolder.Create(baseFolder);
            File.WriteAllText(Path.Combine(used, "0_clip.wav"), "x");

            TtsRunFolder.DeleteIfEmpty(empty);
            TtsRunFolder.DeleteIfEmpty(used);

            Assert.False(Directory.Exists(empty));
            Assert.True(Directory.Exists(used));
        }
        finally
        {
            Directory.Delete(baseFolder, recursive: true);
        }
    }

    private static string NewTestFolder()
    {
        // Deliberately not an "se-tts-" name: these are the *base* folders, and DeleteAsync must
        // refuse to touch anything that is not a run folder.
        var folder = Path.Combine(Path.GetTempPath(), "SubtitleEditTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }
}
