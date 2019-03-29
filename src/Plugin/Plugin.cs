using System;
using System.IO;
using System.Reflection;

namespace Nikse.SubtitleEdit.Plugin
{
    public enum ActionType
    {
        File,
        Tool,
        Sync,
        Translate,
        Spellcheck,
    }

    public class Plugin
    {
        private readonly string _entryPointName;

        public string FileName { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public string Shortcut { get; set; }
        public double Version { get; set; }
        public ActionType ActionType { get; set; }

        public Plugin(string entryPointName)
        {
            _entryPointName = entryPointName;
        }

        public string Invoke(PluginInvokeContext context)
        {
            // load plugin into app-domain and create instance
            Assembly assembly = Assembly.Load(File.ReadAllBytes(FileName));
            string pluginTypeName = Path.GetFileNameWithoutExtension(FileName);
            Type pluginType = assembly.GetType($"{PluginController.DefaultNamespace}{pluginTypeName}");
            object instance = Activator.CreateInstance(pluginType);

            MethodInfo EntryPoint = pluginType.GetInterface("IPlugin").GetMethod(_entryPointName);

            // invoke plugin action and get the result back
            string result = (string)EntryPoint.Invoke(instance,
                 new object[]
                 {
                    context.Context,
                    context.SrtText,
                    context.Frame,
                    context.UiLineBreak,
                    context.File,
                    context.Video,
                    context.RawText
                 });

            // clean up 
            if (instance is IDisposable disposableInstance)
            {
                disposableInstance.Dispose();
            }

            // always return a value in case of null result from plugin
            return result ?? string.Empty;
        }

    }
}
