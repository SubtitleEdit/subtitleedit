using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Resolves the folder an engine writes its generated audio into.
/// <para>
/// Callers hand <see cref="ITtsEngine.Speak"/> a per-run output folder (the temp wave folder),
/// but every engine used to ignore it and write into its own <c>TextToSpeech/&lt;Engine&gt;/</c>
/// folder instead. Nothing ever cleaned those up, so a few generation runs left hundreds of
/// stray clips sitting next to the engine's voices (#13332).
/// </para>
/// <para>
/// Falls back to the engine's own folder when the caller passes nothing usable, so synthesis
/// still works for callers that don't have a run folder (and for the engine unit tests).
/// </para>
/// </summary>
public static class TtsOutputFolder
{
    public static string Resolve(string? outputFolder, Func<string> engineFolder)
    {
        if (!string.IsNullOrWhiteSpace(outputFolder))
        {
            try
            {
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                return outputFolder;
            }
            catch (Exception ex)
            {
                Se.LogError(ex, $"TextToSpeech: cannot write to output folder \"{outputFolder}\" - using the engine folder instead");
            }
        }

        return engineFolder();
    }
}
