using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Plugins
{
    public class OnlinePluginMetadataProvider
    {
        private readonly string _githubUrl;

        public OnlinePluginMetadataProvider(string githubUrl)
        {
            _githubUrl = githubUrl ?? throw new ArgumentNullException(nameof(githubUrl));
        }

        public Task<IReadOnlyCollection<PluginInfo>> GetPluginsAsync()
        {
            // get from metadata file in github
            var xdoc = XDocument.Load(_githubUrl);

            var pluginInfos = new List<PluginInfo>();
            foreach (var xElement in xdoc.Root.Elements("Plugin"))
            {
                var pluginInfo = ReadFormXElement(xElement);
                if (pluginInfo is null)
                {
                    continue;
                }
                
                pluginInfos.Add(pluginInfo);
            }

            return Task.FromResult<IReadOnlyCollection<PluginInfo>>(pluginInfos);
        }
        
        private PluginInfo ReadFormXElement(XElement element)
        {
            // try parse version
            if(!decimal.TryParse(element.Element(nameof(PluginInfo.Version))?.Value, out var version))
            {
                return null;
            }

            // read name
            var name = element.Element(nameof(PluginInfo.Name))?.Value;
            
            // read description
            var description = element.Element(nameof(PluginInfo.Description))?.Value;
            
            return new PluginInfo(name, description, version);
        }
    }
}