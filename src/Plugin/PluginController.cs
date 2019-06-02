using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Plugin
{
    internal class PluginController
    {
        private const string ContractName = "IPlugin";

        public PluginController(string pluginDirectory)
        {
            PluginDirectory = pluginDirectory;
            Plugins = new List<Plugin>();
        }

        public void LoadPlugins(PluginInvokeContext invokeContext)
        {
            // invalid plugin path
            if (!Path.IsPathRooted(PluginDirectory))
            {
                Debug.WriteLine($"LoadPlugins: Unrooted-path \"{PluginDirectory}\"");
                return;
            }

            foreach (string pluginFile in Directory.GetFiles(PluginDirectory, "*.dll"))
            {
                try
                {
                    // load plugin into current appdomain
                    Assembly assembly = Assembly.Load(File.ReadAllBytes(pluginFile));
                    if (assembly == null)
                    {
                        continue;
                    }

                    // by convention the entry point class must be named same as the plugin file
                    string objectName = Path.GetFileNameWithoutExtension(pluginFile);

                    // get plugin type using the default namespace
                    Type pluginType = assembly.GetType(DefaultNamespace + objectName);
                    if (pluginType == null)
                    {
                        continue;
                    }

                    // all plugin must implement IPlugin contract
                    Type contractType = pluginType.GetInterface(ContractName);
                    if (contractType == null)
                    {
                        continue;
                    }

                    object instance = Activator.CreateInstance(pluginType);
                    if (instance == null)
                    {
                        continue;
                    }

                    // maps the values from *instance *plugin
                    Plugin plugin = Build(instance, contractType, pluginFile);
                    if (plugin == null)
                    {
                        continue;
                    }

                    // add plugin into loaded plugins list
                    Plugins.Add(plugin);

                    if (ShouldInvokeOnLoad(pluginType))
                    {
                        // TODO
                        // the result from thesse plugin are not awaited... these plugin are intended to update the UI only
                        // for that when accessing the controls that belong to the UI check if can Invoke..?
                        if (ShouldRunOnWorkerThread(pluginType))
                        {
                            Task.Factory.StartNew(() =>
                            {
                                plugin.Invoke(invokeContext);
                            }, CancellationToken.None);
                        }
                        else
                        {
                            // single threaded plugin
                            plugin.Invoke(invokeContext);
                        }
                    }

                }
                catch (Exception ex)
                {
                    // todo: remove ErrorLoadingPluginXErrorY from language (this message isn't intended for UI-user anymore)
                    Debug.WriteLine(string.Format(Configuration.Settings.Language.Main.ErrorLoadingPluginXErrorY, pluginFile, ex.Message));
                }
            }
        }

        private static bool ShouldInvokeOnLoad(Type pluginType)
        {
            return pluginType.GetCustomAttributes(false)
                 .Any(attr => (bool)attr.GetType().GetProperty("InvokeOnLoad")?.GetValue(attr, null));
        }

        private static bool ShouldRunOnWorkerThread(Type pluginType)
        {
            return pluginType.GetCustomAttributes(false)
                 .Any(attr => (bool)attr.GetType().GetProperty("IsBackground")?.GetValue(attr, null));
        }

        private Plugin Build(object instance, Type interfaceType, string fileName)
        {
            // default binding flags
            BindingFlags bindings = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            MethodInfo entryPoint = default;
            foreach (string methodName in GetRegisteredPMethods())
            {
                MethodInfo methodInfo = interfaceType.GetMethod(methodName, bindings);
                // registered method not found
                if (methodInfo == null)
                {
                    return default;
                }

                // entry point method
                if (methodName.Equals("DoAction", StringComparison.OrdinalIgnoreCase))
                {
                    entryPoint = methodInfo;
                }
            }

            var plugin = new Plugin(entryPoint.Name)
            {
                FileName = fileName,
            };
            foreach (string propName in GetRegisteredProperties())
            {
                PropertyInfo contractPropInfo = interfaceType.GetProperty(propName, bindings);

                // validations
                if (contractPropInfo == null)
                {
                    return null;
                }
                PropertyInfo pluginProp = typeof(Plugin).GetProperty(propName, bindings);
                if (pluginProp == null)
                {
                    return null;
                }

                // copy the metadata values
                if (pluginProp.PropertyType.IsAssignableFrom(contractPropInfo.PropertyType))
                {
                    pluginProp.SetValue(plugin, contractPropInfo.GetValue(instance, null), null);
                }
                else if (contractPropInfo.Name.Equals("ActionType", StringComparison.OrdinalIgnoreCase))
                {
                    string at = (string)contractPropInfo.GetValue(instance, null);
                    //var actioType = (ActionType)Convert.ChangeType(at, typeof(ActionType));
                    plugin.ActionType = (ActionType)Enum.Parse(typeof(ActionType), at, true);
                }
                else if (contractPropInfo.PropertyType == typeof(decimal))
                {
                    plugin.Version = Convert.ToDouble(contractPropInfo.GetValue(instance, null));
                }
            }

            return plugin;
        }

        public void Uninstall(Plugin plugin)
        {
            try
            {
                File.Delete(plugin.FileName);
                Plugins.Remove(plugin);
                // make sure it's remove from the UI
                // log plugin uninstalled
            }
            catch
            {
                Debug.WriteLine($"Fail uninstalling {plugin.Name}");
            }
        }

        public void Reload()
        {
            // when reloading don't past the context
            LoadPlugins(null);
            // log plugins reloaded
        }

        public void Install(Plugin plugin)
        {
            Plugins.Add(plugin);
            Reload();
        }

        /// <summary>
        /// Return names of registered properties
        /// </summary>
        private static string[] GetRegisteredProperties() => new[] { "Name", "Text", "Version", "Description", "ActionType", "Shortcut" };

        /// <summary>
        /// Return names of registered methods
        /// </summary>
        private static string[] GetRegisteredPMethods() => new[] { "DoAction" };

        /// <summary>
        /// The plugin directory where to get the plugins from when loading.
        /// </summary>
        public string PluginDirectory { get; }

        /// <summary>
        /// The list of loaded successfully loaded plugins.
        /// </summary>
        public IList<Plugin> Plugins { get; }

        public const string DefaultNamespace = "Nikse.SubtitleEdit.PluginLogic.";

    }
}
