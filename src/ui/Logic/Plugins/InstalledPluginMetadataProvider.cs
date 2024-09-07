using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Forms;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.Plugins
{
    public class InstalledPluginMetadataProvider : IPluginMetadataProvider
    {
        public IReadOnlyCollection<PluginInfoItem> GetPlugins()
        {
            var installedPlugins = new List<PluginInfoItem>();
            foreach (var pluginFileName in Configuration.GetPlugins())
            {
                Main.GetPropertiesAndDoAction(pluginFileName, out var name, out _, out var version, out var description, out var actionType, out _, out var mi);
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(actionType) && mi != null)
                {
                    installedPlugins.Add(new PluginInfoItem
                    {
                        Name = name.Trim('.', ' '),
                        Description = description,
                        Version = version.ToString(CultureInfo.InvariantCulture),
                        ActionType = actionType,
                        FileName = pluginFileName,
                    });
                }
            }

            return installedPlugins;
        }
    }
}