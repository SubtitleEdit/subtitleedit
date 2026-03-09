using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.Common;

public static class WaveformContextMenuPluginLoader
{
    private static List<IWaveformContextMenuPlugin>? _plugins;

    public static List<IWaveformContextMenuPlugin> GetPlugins()
    {
        if (_plugins != null) return _plugins;

        _plugins = new List<IWaveformContextMenuPlugin>();

        if (!Directory.Exists(Configuration.PluginsDirectory)) return _plugins;

        foreach (var dll in Directory.GetFiles(Configuration.PluginsDirectory, "*.dll"))
        {
            try
            {
                var assembly = System.Reflection.Assembly.Load(FileUtil.ReadAllBytesShared(dll));
                foreach (var type in assembly.GetExportedTypes())
                {
                    try
                    {
                        var obj = Activator.CreateInstance(type);
                        if (obj is IWaveformContextMenuPlugin plugin)
                            _plugins.Add(plugin);
                    }
                    catch { }
                }
            }
            catch { }
        }

        return _plugins;
    }
}
