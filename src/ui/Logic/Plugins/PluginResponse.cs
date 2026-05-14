using System.Text.Json;

namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// Written by the plugin to the path given in <see cref="PluginRequest.ResponseFilePath"/>.
/// </summary>
public class PluginResponse
{
    public int ApiVersion { get; set; } = PluginConstants.ApiVersion;

    /// <summary>
    /// "ok": apply <see cref="Subtitle"/>; "cancelled": do nothing; "error": show <see cref="Message"/>.
    /// See <see cref="PluginConstants"/> Status* values.
    /// </summary>
    public string Status { get; set; } = PluginConstants.StatusCancelled;

    /// <summary>Message shown to the user (status bar on success, dialog on error).</summary>
    public string? Message { get; set; }

    /// <summary>The modified subtitle. Only <see cref="PluginSubtitle.Format"/> and <see cref="PluginSubtitle.Native"/> are read.</summary>
    public PluginSubtitle? Subtitle { get; set; }

    /// <summary>Plugin settings to persist; handed back unchanged in the next request.</summary>
    public JsonElement? Settings { get; set; }

    /// <summary>Optional description used for the undo history entry.</summary>
    public string? UndoDescription { get; set; }
}
