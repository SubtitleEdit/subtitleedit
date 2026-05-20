using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public static class ProcessExtensions
{
    // Centralizes the CA1416 suppression: Process.Start() is flagged as unsupported on
    // platforms (iOS/tvOS/browser) that Subtitle Edit does not target.
    public static void StartProcess(this Process process)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        process.Start();
#pragma warning restore CA1416 // Validate platform compatibility
    }

    public static async Task StartAndWaitAsync(this Process process, CancellationToken cancellationToken)
    {
        process.StartProcess();
        await process.WaitForExitAsync(cancellationToken);
    }
}
