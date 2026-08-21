using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Owns the scratch folder a text-to-speech run writes its per-line clips into.
/// <para>
/// Every step of the pipeline (engine synthesis, silence trim, VAD compression, time-stretch,
/// post-processing) writes a new file per subtitle line, so a single dub of a few hundred lines
/// produces thousands of clips. They used to land loose in the system temp folder - and before
/// that, next to the engine's voices - with nothing ever removing them (#13332).
/// </para>
/// <para>
/// Giving each run its own <c>se-tts-&lt;guid&gt;</c> folder makes the sweep on window close a
/// single directory delete, and lets the user point the runs somewhere they can see them via
/// <see cref="SeVideoTextToSpeech.GenerationFolder"/>.
/// </para>
/// </summary>
public static class TtsRunFolder
{
    /// <summary>
    /// Prefix of every run folder. <see cref="Delete"/> refuses to remove anything else, so a
    /// mis-set <c>GenerationFolder</c> can never take the user's own files with it.
    /// </summary>
    internal const string RunFolderPrefix = "se-tts-";

    private const int DeleteAttempts = 3;
    private const int DeleteRetryDelayMs = 500;

    /// <summary>
    /// Creates a fresh run folder and returns its path.
    /// </summary>
    /// <param name="fallbackBaseFolder">
    /// Base folder to use when no generation folder is configured (the caller's own scratch
    /// location, normally the system temp folder).
    /// </param>
    public static string Create(string? fallbackBaseFolder)
    {
        var baseFolder = Se.Settings.Video.TextToSpeech.GenerationFolder;
        if (string.IsNullOrWhiteSpace(baseFolder))
        {
            baseFolder = fallbackBaseFolder;
        }

        if (string.IsNullOrWhiteSpace(baseFolder))
        {
            baseFolder = Path.GetTempPath();
        }

        var runFolder = Path.Combine(baseFolder, RunFolderPrefix + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(runFolder);
            return runFolder;
        }
        catch (Exception ex)
        {
            // A configured folder that has gone away (unplugged drive, deleted output folder)
            // must not take the whole run down - fall back to temp, which always exists.
            Se.LogError(ex, $"TextToSpeech: cannot create the generation folder \"{runFolder}\" - using the system temp folder instead");
        }

        var tempRunFolder = Path.Combine(Path.GetTempPath(), RunFolderPrefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRunFolder);
        return tempRunFolder;
    }

    /// <summary>
    /// Removes a run folder and everything in it, retrying a few times.
    /// <para>
    /// Closing the window cancels the pipeline rather than waiting for it, so an ffmpeg or engine
    /// process may still hold its last clip open for a moment - on Windows that fails the whole
    /// recursive delete. Best effort throughout: a leftover folder is not worth an error dialog
    /// on close.
    /// </para>
    /// </summary>
    public static async Task DeleteAsync(string? runFolder)
    {
        if (string.IsNullOrEmpty(runFolder) || !IsRunFolder(runFolder))
        {
            return;
        }

        for (var attempt = 1; attempt <= DeleteAttempts; attempt++)
        {
            try
            {
                if (!Directory.Exists(runFolder))
                {
                    return;
                }

                Directory.Delete(runFolder, true);
                return;
            }
            catch (Exception ex)
            {
                if (attempt == DeleteAttempts)
                {
                    Se.LogError(ex, $"TextToSpeech: could not delete the generation folder \"{runFolder}\"");
                    return;
                }
            }

            await Task.Delay(DeleteRetryDelayMs).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Removes a run folder only when nothing was ever written into it - opening the Text to
    /// speech window and closing it again without generating must not leave an empty folder
    /// behind, not even for users who turned the sweep off.
    /// </summary>
    public static void DeleteIfEmpty(string? runFolder)
    {
        if (string.IsNullOrEmpty(runFolder) || !IsRunFolder(runFolder))
        {
            return;
        }

        try
        {
            if (Directory.Exists(runFolder) && Directory.GetFileSystemEntries(runFolder).Length == 0)
            {
                Directory.Delete(runFolder);
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"TextToSpeech: could not delete the empty generation folder \"{runFolder}\"");
        }
    }

    /// <summary>
    /// True when the path is a folder this class created - i.e. safe to delete recursively.
    /// </summary>
    internal static bool IsRunFolder(string folder)
    {
        var name = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return name.StartsWith(RunFolderPrefix, StringComparison.Ordinal);
    }
}
