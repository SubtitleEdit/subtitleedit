using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Plugins;

public class PluginDownloadService : IPluginDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly IZipUnpacker _zipUnpacker;
    private readonly IPluginCatalog _pluginCatalog;

    public PluginDownloadService(HttpClient httpClient, IZipUnpacker zipUnpacker, IPluginCatalog pluginCatalog)
    {
        _httpClient = httpClient;
        _zipUnpacker = zipUnpacker;
        _pluginCatalog = pluginCatalog;
    }

    public async Task<PluginIndex> GetIndexAsync(CancellationToken cancellationToken)
    {
        await using var stream = await _httpClient.GetStreamAsync(PluginConstants.OnlineIndexUrl, cancellationToken);
        var index = await JsonSerializer.DeserializeAsync(stream, PluginJsonContext.Default.PluginIndex, cancellationToken);
        return index ?? new PluginIndex();
    }

    public async Task InstallAsync(PluginIndexEntry entry, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var downloadUrl = PluginPlatform.ResolveDownloadUrl(entry);
        if (string.IsNullOrWhiteSpace(downloadUrl))
        {
            throw new InvalidOperationException($"Plugin '{entry.Name}' has no download for this platform ({PluginPlatform.GetCurrentKey() ?? "unknown"}).");
        }

        Directory.CreateDirectory(Se.PluginsFolder);

        // Unpack into a temp folder inside the plugins folder so the final move stays on the same volume.
        var tempDirectory = Path.Combine(Se.PluginsFolder, ".tmp-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(tempDirectory);

            using var zipStream = new MemoryStream();
            await DownloadHelper.DownloadFileAsync(_httpClient, downloadUrl, zipStream, progress, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            // Unzip and the subsequent file moves are synchronous; running them on a worker
            // thread keeps the UI responsive (notably for the cancel-download button) and lets
            // ThrowIfCancellationRequested between phases stop work as soon as the user reacts.
            await Task.Run(() =>
            {
                zipStream.Position = 0;
                _zipUnpacker.UnpackZipStream(zipStream, tempDirectory);
                cancellationToken.ThrowIfCancellationRequested();

                var rootEntries = Directory.GetFileSystemEntries(tempDirectory);
                string source;
                string targetName;
                if (rootEntries.Length == 1 && Directory.Exists(rootEntries[0]))
                {
                    source = rootEntries[0];
                    targetName = Path.GetFileName(rootEntries[0]);
                }
                else
                {
                    source = tempDirectory;
                    targetName = SanitizeFolderName(entry.Name);
                }

                // Remove any previously installed copy of the same plugin.
                var existing = _pluginCatalog.GetPlugins()
                    .FirstOrDefault(p => p.Manifest.Name.Equals(entry.Name, StringComparison.OrdinalIgnoreCase));
                if (existing != null && Directory.Exists(existing.FolderPath))
                {
                    Directory.Delete(existing.FolderPath, recursive: true);
                }

                cancellationToken.ThrowIfCancellationRequested();

                var targetPath = Path.Combine(Se.PluginsFolder, targetName);
                if (Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath, recursive: true);
                }

                Directory.Move(source, targetPath);
            }, cancellationToken);
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, recursive: true);
                }
            }
            catch
            {
                // ignore - leftover temp folder, harmless
            }
        }
    }

    private static string SanitizeFolderName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        return string.IsNullOrEmpty(cleaned) ? "Plugin" : cleaned;
    }
}
