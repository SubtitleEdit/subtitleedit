using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Plugins
{
    public class PluginUpdateChecker
    {
        private readonly LocalPluginMetadataProvider _localPluginMetadataProvider;
        private readonly OnlinePluginMetadataProvider _onlinePluginMetadataProvider;
        private DateTime _lastCheckDate;

        /// <summary>
        /// Stores the timestamp of last time check update check was made.
        /// </summary>
        public DateTime LastCheckDate => _lastCheckDate;

        public PluginUpdateChecker(PluginUpdateCheckerOptions options)
        {
            _localPluginMetadataProvider = new LocalPluginMetadataProvider(options.PluginDirectory);
            _onlinePluginMetadataProvider = new OnlinePluginMetadataProvider(options.GithubUrl);
        }

        public async Task<PluginUpdateCheckResult> CheckAsync()
        {
            _lastCheckDate = DateTime.Now;

            var installedPlugins = _localPluginMetadataProvider.GetInstalledPlugins();
            // no plugin is currently installed
            if (!installedPlugins.Any())
            {
                return new PluginUpdateCheckResult();
            }

            // plugin repository
            var onlinePlugins = await _onlinePluginMetadataProvider.GetPluginsAsync();
            if (!onlinePlugins.Any())
            {
                return new PluginUpdateCheckResult();
            }

            var pluginUpdateCheckResult = new PluginUpdateCheckResult();
            foreach (var installedPlugin in installedPlugins)
            {
                var updateCheckInfo = installedPlugin.CheckUpdate(onlinePlugins);
                if (updateCheckInfo.IsNewUpdateAvailable())
                {
                    pluginUpdateCheckResult.Add(updateCheckInfo);
                }
            }

            return pluginUpdateCheckResult;
        }
    }

    public class PluginUpdateCheckResult
    {
        private ICollection<PluginUpdate> _pluginUpdates;

        public bool Available => PluginUpdates.Any();

        public IEnumerable<PluginUpdate> PluginUpdates => _pluginUpdates ?? Array.Empty<PluginUpdate>();

        /// <summary>
        /// Adds available update information
        /// </summary>
        public void Add(PluginUpdate pluginUpdate)
        {
            _pluginUpdates = _pluginUpdates ?? new List<PluginUpdate>();
            _pluginUpdates.Add(pluginUpdate);
        }
    }

    /// <summary>
    /// Represents the update checker options for a plugin.
    /// </summary>
    public class PluginUpdateCheckerOptions
    {
        /// <summary>
        /// Gets or sets the directory path where the plugin is located.
        /// </summary>
        public string PluginDirectory { get; set; }

        /// <summary>
        /// Gets or sets the GitHub URL where the plugin's updates are being tracked.
        /// </summary>
        public string GithubUrl { get; set; }
    }
}