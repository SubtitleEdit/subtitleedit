using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Plugins;

public class PluginRunner : IPluginRunner
{
    public async Task<PluginRunResult> RunAsync(InstalledPlugin plugin, PluginRequest request, CancellationToken cancellationToken)
    {
        if (!plugin.CanRun || plugin.LaunchPath == null)
        {
            return PluginRunResult.Error($"Plugin '{plugin.Manifest.Name}' has no executable for this operating system.");
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "SubtitleEditPlugins", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(tempDirectory);

            var requestPath = Path.Combine(tempDirectory, PluginConstants.RequestFileName);
            var responsePath = Path.Combine(tempDirectory, PluginConstants.ResponseFileName);

            request.TempDirectory = tempDirectory;
            request.ResponseFilePath = responsePath;

            await using (var requestStream = File.Create(requestPath))
            {
                await JsonSerializer.SerializeAsync(requestStream, request, PluginJsonContext.Default.PluginRequest, cancellationToken);
            }

            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = plugin.FolderPath,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (plugin.UsesDotnetRuntime)
            {
                startInfo.FileName = "dotnet";
                startInfo.ArgumentList.Add(plugin.LaunchPath);
            }
            else
            {
                EnsureExecutable(plugin.LaunchPath);
                startInfo.FileName = plugin.LaunchPath;
            }

            startInfo.ArgumentList.Add(requestPath);

            using var process = new Process { StartInfo = startInfo };

            try
            {
                if (!process.Start())
                {
                    return PluginRunResult.Error($"Could not start plugin '{plugin.Manifest.Name}'.");
                }
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Failed to start plugin: " + plugin.Manifest.Name);
                return PluginRunResult.Error($"Could not start plugin '{plugin.Manifest.Name}': {exception.Message}");
            }

            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                return PluginRunResult.Cancelled();
            }

            if (process.ExitCode != 0)
            {
                return PluginRunResult.Error($"Plugin '{plugin.Manifest.Name}' exited with code {process.ExitCode}.");
            }

            if (!File.Exists(responsePath))
            {
                return PluginRunResult.Error($"Plugin '{plugin.Manifest.Name}' did not produce a response.");
            }

            try
            {
                await using var responseStream = File.OpenRead(responsePath);
                var response = JsonSerializer.Deserialize(responseStream, PluginJsonContext.Default.PluginResponse);
                if (response == null)
                {
                    return PluginRunResult.Error($"Plugin '{plugin.Manifest.Name}' returned an empty response.");
                }

                return PluginRunResult.Ok(response);
            }
            catch (JsonException exception)
            {
                Se.LogError(exception, "Invalid plugin response: " + plugin.Manifest.Name);
                return PluginRunResult.Error($"Plugin '{plugin.Manifest.Name}' returned an invalid response.");
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Plugin run failed: " + plugin.Manifest.Name);
            return PluginRunResult.Error($"Plugin '{plugin.Manifest.Name}' failed: {exception.Message}");
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    // System.IO.Compression.ZipFile.ExtractToDirectory does not preserve Unix file
    // modes, so a self-contained plugin binary unpacked from a zip on macOS/Linux
    // arrives without its +x bit. Set it here before launching.
    private static void EnsureExecutable(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            var mode = File.GetUnixFileMode(path);
            var needed = UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
            if ((mode & needed) != needed)
            {
                File.SetUnixFileMode(path, mode | needed);
            }
        }
        catch
        {
            // ignore - if we can't chmod, Process.Start will fail with a clear permission error
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // ignore - process may have exited between the check and the kill
        }
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch
        {
            // ignore - temp folder, OS will clean it up eventually
        }
    }
}
