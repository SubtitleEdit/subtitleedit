using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Plugins;

public interface IPluginRunner
{
    /// <summary>
    /// Writes the request to a temp file, launches the plugin process, waits for it to exit,
    /// then reads and parses the response. Cancelling the token kills the process.
    /// Never throws - failures are returned as a non-succeeded <see cref="PluginRunResult"/>.
    /// </summary>
    Task<PluginRunResult> RunAsync(InstalledPlugin plugin, PluginRequest request, CancellationToken cancellationToken);
}
