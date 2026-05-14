using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SePlugins
{
    /// <summary>
    /// Per-plugin settings, keyed by plugin name. The value is the raw JSON blob the
    /// plugin returned in its last response, handed back unchanged on the next run.
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new();

    /// <summary>Names of installed plugins the user has disabled; they are hidden from the menu.</summary>
    public List<string> DisabledPluginNames { get; set; } = new();
}
