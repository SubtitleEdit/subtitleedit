using System.Collections.ObjectModel;
using System.Linq;
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
    private static readonly string[] EngineNames =
    {
        SeAiReview.EngineLlamaCpp,
        SeAiReview.EngineOllama,
        SeAiReview.EngineOpenAiCompatible,
    };

    /// <summary>
    /// Fills <paramref name="engines"/> with the engine names and returns the name to select
    /// (<paramref name="selectName"/> when it is one of them, otherwise llama.cpp).
    /// Call it again after installing/updating llama.cpp: the rows are built from a one-off status
    /// snapshot (see <see cref="StatusDots.ComboItemTemplate{T}"/>), so re-filling the collection is
    /// what makes the dot re-evaluate - otherwise an updated engine keeps showing the amber
    /// "update available" dot until the window is reopened.
    /// </summary>
    public static string Populate(ObservableCollection<string> engines, string? selectName)
    {
        engines.Clear();
        foreach (var name in EngineNames)
        {
            engines.Add(name);
        }

        return EngineNames.Contains(selectName) ? selectName! : SeAiReview.EngineLlamaCpp;
    }

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
