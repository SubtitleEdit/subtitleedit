using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Renames a user-imported cloning voice on disk. Every clone engine lists its voices straight
/// from its voices folder - one reference WAV per voice, named after the file with underscores
/// shown as spaces - so a rename is a file move: the WAV, any sidecar beside it with the same base
/// name (the <c>.txt</c> transcript, engine JSON, ...) and the cached
/// <see cref="CloneReferenceTail"/> copy, which is keyed by file name and would otherwise be left
/// behind as an orphan.
/// </summary>
public static class VoiceFileRename
{
    /// <summary>
    /// The reference recording <paramref name="voice"/> clones from, or null when the voice is
    /// not a renamable file-backed clone: an engine preset, the "Default" speaker, the per-line
    /// clone marker, or a reference staged for a single line of one run.
    /// </summary>
    public static string? GetReferenceFilePath(Voice voice)
    {
        var filePath = voice.EngineVoice switch
        {
            ChatterboxVoice v => v.FilePath,
            Confucius4TtsVoice v => v.FilePath,
            CosyVoice3Voice v => v.FilePath,
            DotsTtsVoice v => v.FilePath,
            F5TtsVoice v => v.FilePath,
            IndexTtsVoice v => v.FilePath,
            MossTtsVoice v => v.FilePath,
            OmniVoice v => v.FilePath,
            OmniVoiceCrispAsrVoice v => v.FilePath,
            PocketTtsVoice v => v.FilePath,
            Qwen3TtsVoice v => v.FilePath,
            VibeVoice v => v.FilePath,
            VoxCPM2Voice v => v.FilePath,
            ZonosTtsVoice v => v.FilePath,
            _ => string.Empty,
        };

        if (string.IsNullOrEmpty(filePath) || PerLineReferenceStaging.IsStaged(filePath))
        {
            return null;
        }

        return filePath;
    }

    public static bool CanRename(Voice? voice) =>
        voice != null && GetReferenceFilePath(voice) is { } path && File.Exists(path);

    /// <summary>
    /// Moves the voice's files to <paramref name="newName"/>. Returns the new reference file
    /// name, or null with <paramref name="error"/> set when nothing was changed.
    /// </summary>
    public static string? Rename(Voice voice, string newName, out string error)
    {
        error = string.Empty;
        var oldFileName = GetReferenceFilePath(voice);
        if (oldFileName == null || !File.Exists(oldFileName))
        {
            error = "Voice file not found";
            return null;
        }

        newName = newName.Trim();
        if (string.IsNullOrEmpty(newName))
        {
            error = "Name is empty";
            return null;
        }

        // The engines show '_' as ' ', so store spaces as underscores to round-trip the name.
        var newBaseName = newName.Replace(' ', '_');
        if (newBaseName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || newBaseName.Contains("..") ||
            newBaseName.StartsWith(PerLineReferenceStaging.Prefix, StringComparison.OrdinalIgnoreCase))
        {
            error = "Name contains invalid characters";
            return null;
        }

        var folder = Path.GetDirectoryName(oldFileName) ?? string.Empty;
        var oldBaseName = Path.GetFileNameWithoutExtension(oldFileName);
        var extension = Path.GetExtension(oldFileName);
        var newFileName = Path.Combine(folder, newBaseName + extension);
        if (string.Equals(oldBaseName, newBaseName, StringComparison.Ordinal))
        {
            return oldFileName;
        }

        var sameFileDifferentCase = string.Equals(oldBaseName, newBaseName, StringComparison.OrdinalIgnoreCase);
        if (!sameFileDifferentCase && File.Exists(newFileName))
        {
            error = $"A voice named '{newName}' already exists";
            return null;
        }

        try
        {
            // Sidecars first (a rename that fails half-way is still a usable voice); WAV last.
            foreach (var sidecar in Directory.GetFiles(folder, oldBaseName + ".*"))
            {
                if (string.Equals(sidecar, oldFileName, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(Path.GetFileNameWithoutExtension(sidecar), oldBaseName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var target = Path.Combine(folder, newBaseName + Path.GetExtension(sidecar));
                File.Move(sidecar, target, overwrite: sameFileDifferentCase);
            }

            File.Move(oldFileName, newFileName, overwrite: sameFileDifferentCase);

            // The prepared copy is keyed on the reference's file name; the next synthesis makes
            // a fresh one under the new name, so the old one would only be an orphan.
            var prepared = CloneReferenceTail.GetPreparedFileName(oldFileName);
            foreach (var stale in new[] { prepared, prepared + ".stamp" }.Where(File.Exists))
            {
                File.Delete(stale);
            }

            Se.WriteToolsLog($"TTS voice renamed: '{oldFileName}' -> '{newFileName}'");
            return newFileName;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Renaming TTS voice '{oldFileName}' to '{newFileName}' failed");
            error = ex.Message;
            return null;
        }
    }
}
