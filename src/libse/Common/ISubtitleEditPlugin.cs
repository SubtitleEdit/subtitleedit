using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.Common;

/// <summary>
/// Context passed to a plugin when executed from the Plugins menu.
/// </summary>
public class SubtitleEditPluginContext
{
    /// <summary>Full path to the currently loaded video file, or empty string.</summary>
    public string VideoFileName { get; set; } = string.Empty;

    /// <summary>Subtitle lines currently selected in the grid.</summary>
    public List<Paragraph> SelectedParagraphs { get; set; } = new();

    /// <summary>All subtitle lines in the current document.</summary>
    public Subtitle Subtitle { get; set; } = new();

    /// <summary>
    /// Output folder chosen by the user (only populated when
    /// <see cref="ISubtitleEditPlugin.NeedsOutputFolder"/> is true).
    /// </summary>
    public string? OutputFolder { get; set; }
}

/// <summary>
/// Plugin interface for top-level Plugins menu items.
/// Implement this interface and place the compiled DLL in the Plugins directory.
/// </summary>
public interface ISubtitleEditPlugin
{
    /// <summary>Internal identifier.</summary>
    string Name { get; }

    /// <summary>Text shown in the Plugins menu.</summary>
    string MenuItemText { get; }

    /// <summary>
    /// Set to true if the host application should prompt the user for an
    /// output folder before calling <see cref="ExecuteAsync"/>.
    /// </summary>
    bool NeedsOutputFolder { get; }

    Task ExecuteAsync(SubtitleEditPluginContext context);
}
