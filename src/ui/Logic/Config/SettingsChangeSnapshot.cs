using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// Snapshots <see cref="Se.Settings"/> for the "did anything change that has to be applied?"
/// check around the settings dialog. Two snapshots that compare equal mean the heavyweight
/// <c>MainViewModel.ApplySettings</c> - which rebuilds the layout and with it the video player -
/// can be skipped.
/// </summary>
public static class SettingsChangeSnapshot
{
    /// <summary>
    /// Serializes the settings, minus the remembered window geometry.
    /// <para>
    /// <c>General.WindowPositions</c> is excluded because it is written as a side effect of
    /// simply using the dialog: every window saves its position when it closes, so the settings
    /// window itself - and, in undocked mode, the video/waveform windows an Apply closes and
    /// re-creates - would make the two snapshots differ even when the user changed nothing that
    /// needs applying. Window geometry never needs applying (it is read when a window opens), so
    /// leaving it out only removes false positives (issue #14218).
    /// </para>
    /// </summary>
    public static string Take()
    {
        // Through the source-generated context, like Se.SaveSettings - so the snapshot sees
        // exactly the settings that get persisted.
        var node = JsonSerializer.SerializeToNode(Se.Settings, SeJsonContext.Default.Se);
        if (node is JsonObject root &&
            root.TryGetPropertyValue("General", out var generalNode) &&
            generalNode is JsonObject general)
        {
            general.Remove("WindowPositions");
        }

        return node?.ToJsonString() ?? string.Empty;
    }
}
