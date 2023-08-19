using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Nikse.SubtitleEdit.Plugins
{
    public interface ILocalPluginMetadataProvider
    {
        IReadOnlyCollection<LocalPlugin> GetInstalledPlugins();
    }

    public class LocalPluginMetadataProvider : ILocalPluginMetadataProvider
    {
        private const string KnownPluginExtension = "*.dll";

        private readonly string _pluginFolder;

        public LocalPluginMetadataProvider(string pluginFolder)
        {
            _pluginFolder = pluginFolder ?? throw new ArgumentNullException(nameof(pluginFolder));
        }

        public IReadOnlyCollection<LocalPlugin> GetInstalledPlugins()
        {
            var pluginFiles = Directory.GetFiles(_pluginFolder, KnownPluginExtension);

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
                    var localPlugin = ParseFromType(pluginType);
                    if (localPlugin?.IsValid() == true)
                    {
                        installedPlugins.Add(localPlugin);
                    }
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
            var symbolReader = new SymbolReader(pluginType, instance);

            // read the following properties from plugin assembly
            var text = symbolReader.ReadFromProperty<string>(SymbolReader.Text);
            var description = symbolReader.ReadFromProperty<string>(SymbolReader.Description);
            var version = symbolReader.ReadFromProperty<decimal>(SymbolReader.Version);

            return new LocalPlugin(text, description, version);
        }

        private class SymbolReader
        {
            public const string Text = "Text";
            public const string Description = "Description";
            public const string Version = "Version";
            
            private readonly Type _pluginType;
            private readonly object _instance;
            private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;

            public SymbolReader(Type pluginType, object instance)
            {
                _pluginType = pluginType;
                _instance = instance;
            }

            public TValue ReadFromProperty<TValue>(string propertyName)
            {
                var propInfo = _pluginType.GetProperty(propertyName, BindingFlags);
                if (propInfo is null)
                {
                    return default;
                }

                try
                {
                    return (TValue)propInfo.GetValue(_instance, null);
                }
                catch (Exception)
                {
                    return default;
                }
            }
        }
    }
}