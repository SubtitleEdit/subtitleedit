using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Plugins;

public interface IPluginCatalog
{
    /// <summary>
    /// Scans the plugins folder and returns every plugin with a valid manifest.
    /// Results are not cached - call again to pick up newly installed plugins.
    /// </summary>
    IReadOnlyList<InstalledPlugin> GetPlugins();
}
