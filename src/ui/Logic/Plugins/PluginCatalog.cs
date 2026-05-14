using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Nikse.SubtitleEdit.Logic.Plugins;

public class PluginCatalog : IPluginCatalog
{
    public IReadOnlyList<InstalledPlugin> GetPlugins()
    {
        var result = new List<InstalledPlugin>();
        var root = Se.PluginsFolder;
        if (!Directory.Exists(root))
        {
            return result;
        }

        foreach (var folder in Directory.EnumerateDirectories(root))
        {
            var manifestPath = Path.Combine(folder, PluginConstants.ManifestFileName);
            if (!File.Exists(manifestPath))
            {
                continue;
            }

            try
            {
                using var stream = File.OpenRead(manifestPath);
                var manifest = JsonSerializer.Deserialize(stream, PluginJsonContext.Default.PluginManifest);
                if (manifest == null || string.IsNullOrWhiteSpace(manifest.Name))
                {
                    continue;
                }

                var usesDotnet = string.Equals(manifest.Runtime, PluginConstants.RuntimeDotnet, StringComparison.OrdinalIgnoreCase);
                var launchPath = ResolveLaunchPath(folder, manifest, usesDotnet);

                result.Add(new InstalledPlugin
                {
                    Manifest = manifest,
                    FolderPath = folder,
                    ManifestPath = manifestPath,
                    LaunchPath = launchPath,
                    UsesDotnetRuntime = usesDotnet,
                });
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Failed to read plugin manifest: " + manifestPath);
            }
        }

        return result;
    }

    private static string? ResolveLaunchPath(string folder, PluginManifest manifest, bool usesDotnet)
    {
        if (usesDotnet)
        {
            if (string.IsNullOrWhiteSpace(manifest.Entry))
            {
                return null;
            }

            var entryPath = Path.Combine(folder, manifest.Entry);
            return File.Exists(entryPath) ? entryPath : null;
        }

        var executable = GetExecutableForCurrentOs(manifest.Executables);
        if (string.IsNullOrWhiteSpace(executable))
        {
            return null;
        }

        var path = Path.Combine(folder, executable);
        return File.Exists(path) ? path : null;
    }

    private static string? GetExecutableForCurrentOs(PluginExecutables? executables)
    {
        if (executables == null)
        {
            return null;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return executables.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return executables.Macos;
        }

        return executables.Linux;
    }
}
