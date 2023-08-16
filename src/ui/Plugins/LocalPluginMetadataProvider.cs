using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Nikse.SubtitleEdit.Plugins
{
    public class LocalPluginMetadataProvider
    {
        private readonly string _pluginFolder;

        public LocalPluginMetadataProvider(string pluginFolder)
        {
            _pluginFolder = pluginFolder ?? throw new ArgumentNullException(nameof(pluginFolder));
        }

        public IReadOnlyCollection<LocalPlugin> GetPlugins()
        {
            var pluginFiles = Directory.GetFiles(_pluginFolder, "*.dll");

            // no plugin installed
            if (pluginFiles.Length == 0)
            {
                return new List<LocalPlugin>();
            }

            var installedPlugins = new List<LocalPlugin>();
            const string plugin = "IPlugin";
            
            foreach (var pluginFile in pluginFiles)
            {
                try
                {
                    var assembly = Assembly.Load(File.ReadAllBytes(pluginFile));

                    // all plugins must implement the "IPlugin" interface
                    var pluginType = assembly.GetTypes().FirstOrDefault(type => type.GetInterface(plugin) != null);

                    var pluginInfo = ParseFromType(pluginType);

                    // invalid plugin info
                    if (pluginInfo is null)
                    {
                        continue;
                    }

                    installedPlugins.Add(pluginInfo);
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            return installedPlugins;
        }

        private static LocalPlugin ParseFromType(Type pluginType)
        {
            if (pluginType is null)
            {
                return null;
            }

            var instance = Activator.CreateInstance(pluginType);

            // read name/text
            var name = (string)pluginType
                .GetProperty("Text", BindingFlags.Instance | BindingFlags.Public)
                ?.GetValue(instance, null);

            // read description
            var description = (string)pluginType
                .GetProperty(nameof(PluginInfo.Description), BindingFlags.Instance | BindingFlags.Public)
                ?.GetValue(instance, null);

            // read version
            // ReSharper disable once PossibleNullReferenceException
            var version = (decimal)pluginType
                .GetProperty(nameof(PluginInfo.Version), BindingFlags.Instance | BindingFlags.Public)
                .GetValue(instance, null);

            return new LocalPlugin(name, description, version);
        }
    }
}