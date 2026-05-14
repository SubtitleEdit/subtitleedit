using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Plugins;

public interface IPluginDownloadService
{
    /// <summary>Downloads and parses the online plugin index.</summary>
    Task<PluginIndex> GetIndexAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Downloads the plugin .zip and unpacks it into the plugins folder. Any previously
    /// installed version of the same plugin is removed first.
    /// </summary>
    Task InstallAsync(PluginIndexEntry entry, IProgress<float>? progress, CancellationToken cancellationToken);
}
