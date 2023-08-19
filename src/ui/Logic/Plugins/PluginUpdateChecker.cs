using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Plugins
{
    public class PluginUpdateChecker
    {
        private readonly IPluginMetadataProvider _localPluginMetadataProvider;
        private readonly IPluginMetadataProvider _onlinePluginMetadataProvider;

        public PluginUpdateChecker(
            IPluginMetadataProvider localPluginMetadataProvider,
            IPluginMetadataProvider onlinePluginMetadataProvider)
        {
            _localPluginMetadataProvider = localPluginMetadataProvider;
            _onlinePluginMetadataProvider = onlinePluginMetadataProvider;
        }

    //    public PluginUpdateCheckResult Check()
    //    {
    //        var installedPlugins = _localPluginMetadataProvider.GetPlugins();
    //        if (!installedPlugins.Any())
    //        {
    //            return new PluginUpdateCheckResult();
    //        }

    //        // plugin repository
    //        var onlinePlugins = _onlinePluginMetadataProvider.GetPlugins();
    //        if (!onlinePlugins.Any())
    //        {
    //            return new PluginUpdateCheckResult();
    //        }

    //        var pluginUpdateCheckResult = new PluginUpdateCheckResult();
    //        foreach (var installedPlugin in installedPlugins)
    //        {
    //            var updateCheckInfo = installedPlugin.CheckUpdate(onlinePlugins);
    //            if (updateCheckInfo.IsNewUpdateAvailable())
    //            {
    //                pluginUpdateCheckResult.Add(updateCheckInfo);
    //            }
    //        }

    //        return pluginUpdateCheckResult;
    //    }
    //}

    //public class PluginUpdateCheckResult
    //{
    //    private ICollection<PluginUpdate> _pluginUpdates;

    //    public bool Available => PluginUpdates.Any();

    //    public IEnumerable<PluginUpdate> PluginUpdates => _pluginUpdates ?? Array.Empty<PluginUpdate>();

    //    /// <summary>
    //    /// Adds available update information
    //    /// </summary>
    //    public void Add(PluginUpdate pluginUpdate)
    //    {
    //        _pluginUpdates = _pluginUpdates ?? Array.Empty<PluginUpdate>();
    //        _pluginUpdates.Add(pluginUpdate);
    //    }
    }
}