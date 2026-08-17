using System.Diagnostics;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;

namespace UITests.Features.Video.TextToSpeech;

/// <summary>
/// Voice cloning hung on Windows because the ffmpeg processes it starts have both standard
/// streams redirected and nobody read them: the 4 KB pipe Windows gives an anonymous pipe fills
/// up on ffmpeg's banner alone, and ffmpeg then blocks forever on its next write while
/// WaitForExitAsync waits for an exit that never comes (#13768). StartAndWaitAsync now starts
/// the async read itself.
/// </summary>
public class ProcessExtensionsDrainTests
{
    [Fact]
    public async Task StartAndWaitAsync_ReadsRedirectedStandardError()
    {
        using var process = ChildWriting("hello-from-stderr", toStandardError: true);
        var received = new List<string>();
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (received)
                {
                    received.Add(e.Data);
                }
            }
        };

        await process.StartAndWaitAsync(TestContext.Current.CancellationToken);

        // The events only arrive when the async read was started - which is also what keeps the
        // pipe from filling up and wedging the child.
        Assert.Contains("hello-from-stderr", received);
    }

    [Fact]
    public async Task StartAndWaitAsync_ReadsRedirectedStandardOutput()
    {
        using var process = ChildWriting("hello-from-stdout", toStandardError: false);
        var received = new List<string>();
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (received)
                {
                    received.Add(e.Data);
                }
            }
        };

        await process.StartAndWaitAsync(TestContext.Current.CancellationToken);

        Assert.Contains("hello-from-stdout", received);
    }

    /// <summary>
    /// More output than any pipe buffer holds: without draining this child can never exit, so the
    /// timeout overload would kill it and throw instead of returning normally.
    /// </summary>
    [Fact]
    public async Task StartAndWaitAsync_WithTimeout_CompletesForAChildThatOutgrowsThePipeBuffer()
    {
        using var process = ChildWritingManyLines(20000);
        var lines = 0;
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                Interlocked.Increment(ref lines);
            }
        };

        await process.StartAndWaitAsync(TestContext.Current.CancellationToken, TimeSpan.FromMinutes(1));

        Assert.Equal(0, process.ExitCode);
        Assert.Equal(20000, lines);
    }

    private static Process ChildWriting(string text, bool toStandardError) =>
        Shell(OperatingSystem.IsWindows()
            ? (toStandardError ? $"echo {text} 1>&2" : $"echo {text}")
            : (toStandardError ? $"echo {text} >&2" : $"echo {text}"));

    private static Process ChildWritingManyLines(int count) =>
        Shell(OperatingSystem.IsWindows()
            ? $"for /L %i in (1,1,{count}) do @echo line-%i-padding-padding-padding-padding"
            : $"i=1; while [ $i -le {count} ]; do echo line-$i-padding-padding-padding-padding; i=$((i+1)); done");

    private static Process Shell(string command)
    {
        var startInfo = OperatingSystem.IsWindows()
            ? new ProcessStartInfo("cmd.exe", $"/c {command}")
            : new ProcessStartInfo("/bin/sh", $"-c \"{command}\"");

        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        return new Process { StartInfo = startInfo };
    }
}
