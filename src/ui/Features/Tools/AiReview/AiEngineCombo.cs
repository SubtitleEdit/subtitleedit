using Avalonia.Controls.Templates;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

/// <summary>
/// The engine combo shared by Tools -> AI review and the subtitle text box AI assistant. Both list
/// the same three engines as plain strings, so the install-status dot lives here rather than being
/// duplicated (and drifting) in each window.
/// </summary>
public static class AiEngineCombo
{
    /// <summary>
    /// Renders "[dot] engine name". Only llama.cpp is downloadable, so it is the only row with a dot:
    /// grey when nothing is installed, amber when the pinned release is newer than the installed
    /// build, green otherwise. Ollama and OpenAI-compatible are external services SE does not
    /// install, so they get <see cref="DownloadDotStatus.None"/> and an empty dot slot.
    /// </summary>
    public static FuncDataTemplate<string> ItemTemplate()
    {
        return StatusDots.ComboItemTemplate<string>(
            name => name,
            _ => null,
            GetDotStatus);
    }

    private static DownloadDotStatus GetDotStatus(string engineName)
    {
        if (engineName != SeAiReview.EngineLlamaCpp)
        {
            return DownloadDotStatus.None;
        }

        return StatusDots.From(
            LlamaCppServerManager.IsEngineInstalled(),
            LlamaCppUpdateStatus.GetEngineUpdateStatus());
    }
}
