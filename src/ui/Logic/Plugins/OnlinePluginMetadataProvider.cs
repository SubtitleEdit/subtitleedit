using Nikse.SubtitleEdit.Core.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Logic.Plugins
{
    public class OnlinePluginMetadataProvider : IPluginMetadataProvider
    {
        private readonly string _githubUrl;

        public OnlinePluginMetadataProvider(string githubUrl)
        {
            _githubUrl = githubUrl ?? throw new ArgumentNullException(nameof(githubUrl));
        }

        public async Task<IReadOnlyCollection<PluginInfoItem>> GetPlugins()
        {
            XDocument xDocument;
            using (var httpClient = DownloaderFactory.MakeHttpClient())
            using (var downloadStream = new MemoryStream())
            {
                await httpClient.DownloadAsync(_githubUrl, downloadStream, null, CancellationToken.None);
                downloadStream.Position = 0;
                xDocument = XDocument.Load(downloadStream);
            }

            var pluginInfos = new List<PluginInfoItem>();
            if (xDocument.Root == null)
            {
                return pluginInfos;
            }

            foreach (var xElement in xDocument.Root.Elements("Plugin"))
            {
                var pluginInfo = ReadFormXElement(xElement);
                if (pluginInfo != null)
                {
                    pluginInfos.Add(pluginInfo);
                }
            }

            return pluginInfos;
        }

        private static PluginInfoItem ReadFormXElement(XElement element)
        {
            var name = element.Element(nameof(PluginInfoItem.Name))?.Value.Trim('.', ' ');
            var description = element.Element(nameof(PluginInfoItem.Description))?.Value;
            var version = element.Element(nameof(PluginInfoItem.Version))?.Value;
            var date = element.Element(nameof(PluginInfoItem.Date))?.Value;
            var url = element.Element(nameof(PluginInfoItem.Url))?.Value;

            return new PluginInfoItem { Name = name, Description = description, Version = version, Date = date, Url = url };
        }
    }
}