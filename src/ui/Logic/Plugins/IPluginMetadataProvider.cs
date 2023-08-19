using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Plugins
{
    public interface IPluginMetadataProvider
    {
       IReadOnlyCollection<PluginInfoItem> GetPlugins();
    }
}