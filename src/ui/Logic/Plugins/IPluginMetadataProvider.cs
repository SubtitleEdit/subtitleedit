using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Plugins
{
    public interface IPluginMetadataProvider
    {
       Task<IReadOnlyCollection<PluginInfoItem>> GetPlugins();
    }
}