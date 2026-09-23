using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Keeps the computer from going to sleep on idle while a long job runs (e.g. a batch convert of
/// many files, #15222). The display may still turn off; only system sleep is held back. Dispose the
/// returned object to release it. Best effort: if the platform mechanism is missing, the job just
/// runs without it (logged, never thrown).
/// <list type="bullet">
/// <item>Windows: a power request (PowerCreateRequest/PowerSetRequest), listed by "powercfg /requests".</item>
/// <item>macOS: "caffeinate -i -w &lt;pid&gt;", which also ends by itself if Subtitle Edit dies.</item>
/// <item>Linux: the xdg-desktop-portal Inhibit call (works inside Flatpak, GNOME and KDE), falling
/// back to "systemd-inhibit --what=sleep". Both end when our connection/process goes away.</item>
/// </list>
/// </summary>
public static partial class SleepInhibitor
{
    public static async Task<IDisposable> AcquireAsync(string reason)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                return (IDisposable?)WindowsPowerRequest.Create(reason) ?? NoOp.Instance;
            }

            if (OperatingSystem.IsMacOS())
            {
                return StartHelperProcess("caffeinate", ["-i", "-w", Environment.ProcessId.ToString()]) ?? NoOp.Instance;
            }

            if (OperatingSystem.IsLinux())
            {
                IDisposable? portal = await PortalInhibit.CreateAsync(reason);
                if (portal != null)
                {
                    return portal;
                }

                // tail --pid ends the inhibitor if Subtitle Edit is killed before Dispose runs.
                return StartHelperProcess("systemd-inhibit",
                [
                    "--what=sleep", "--mode=block", "--who=Subtitle Edit", "--why=" + reason,
                    "tail", "--pid=" + Environment.ProcessId, "-f", "/dev/null",
                ]) ?? NoOp.Instance;
            }
        }
        catch (Exception exception)
        {
            SeLogger.Error(exception, "Unable to prevent sleep");
        }

        return NoOp.Instance;
    }

    private static IDisposable? StartHelperProcess(string fileName, string[] arguments)
    {
        var path = FindOnPath(fileName);
        if (path == null)
        {
            SeLogger.Error($"Unable to prevent sleep: '{fileName}' not found");
            return null;
        }

        var startInfo = new ProcessStartInfo(path)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        var process = Process.Start(startInfo);
        return process == null ? null : new HelperProcess(process);
    }

    private static string? FindOnPath(string fileName)
    {
        var paths = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        foreach (var directory in paths)
        {
            var candidate = Path.Combine(directory, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        // PATH can be minimal when launched from Finder / a desktop file.
        foreach (var directory in new[] { "/usr/bin", "/bin", "/usr/local/bin" })
        {
            var candidate = Path.Combine(directory, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private sealed class NoOp : IDisposable
    {
        public static readonly NoOp Instance = new();

        public void Dispose()
        {
        }
    }

    private sealed class HelperProcess(Process process) : IDisposable
    {
        public void Dispose()
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (Exception exception)
            {
                SeLogger.Error(exception, "Unable to stop sleep inhibitor process");
            }
            finally
            {
                process.Dispose();
            }
        }
    }

    private sealed class PortalInhibit(DBusConnection connection) : IDisposable
    {
        private const uint InhibitSuspend = 4;

        public static async Task<PortalInhibit?> CreateAsync(string reason)
        {
            var address = DBusAddress.Session;
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            // A connection of our own: the portal drops the inhibition when it closes, so Dispose
            // releases it (and so does the process exiting). Avalonia's shared connection would not.
            var connection = new DBusConnection(address);
            try
            {
                await connection.ConnectAsync();

                MessageBuffer message;
                using (var writer = connection.GetMessageWriter())
                {
                    writer.WriteMethodCallHeader(
                        destination: "org.freedesktop.portal.Desktop",
                        path: "/org/freedesktop/portal/desktop",
                        @interface: "org.freedesktop.portal.Inhibit",
                        member: "Inhibit",
                        signature: "sua{sv}");
                    writer.WriteString(string.Empty); // parent window
                    writer.WriteUInt32(InhibitSuspend);
                    var dictionary = writer.WriteDictionaryStart();
                    writer.WriteDictionaryEntryStart();
                    writer.WriteString("reason");
                    writer.WriteVariantString(reason);
                    writer.WriteDictionaryEnd(dictionary);
                    message = writer.CreateMessage();
                }

                await connection.CallMethodAsync(message, (m, _) => m.GetBodyReader().ReadObjectPathAsString(), null);
                return new PortalInhibit(connection);
            }
            catch (Exception exception)
            {
                SeLogger.Error(exception, "Unable to prevent sleep via xdg-desktop-portal");
                connection.Dispose();
                return null;
            }
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }

    private sealed partial class WindowsPowerRequest(IntPtr handle, IntPtr reasonString) : IDisposable
    {
        private const uint PowerRequestContextVersion = 0;
        private const uint PowerRequestContextSimpleString = 0x1;
        private const int PowerRequestSystemRequired = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct ReasonContext
        {
            public uint Version;
            public uint Flags;
            public IntPtr SimpleReasonString;
        }

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr PowerCreateRequest(ref ReasonContext context);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool PowerSetRequest(IntPtr powerRequest, int requestType);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool PowerClearRequest(IntPtr powerRequest, int requestType);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CloseHandle(IntPtr handle);

        public static WindowsPowerRequest? Create(string reason)
        {
            var reasonString = Marshal.StringToHGlobalUni("Subtitle Edit: " + reason);
            var context = new ReasonContext
            {
                Version = PowerRequestContextVersion,
                Flags = PowerRequestContextSimpleString,
                SimpleReasonString = reasonString,
            };

            var handle = PowerCreateRequest(ref context);
            if (handle == IntPtr.Zero || handle == new IntPtr(-1))
            {
                SeLogger.Error($"Unable to prevent sleep: PowerCreateRequest failed ({Marshal.GetLastPInvokeError()})");
                Marshal.FreeHGlobal(reasonString);
                return null;
            }

            if (!PowerSetRequest(handle, PowerRequestSystemRequired))
            {
                SeLogger.Error($"Unable to prevent sleep: PowerSetRequest failed ({Marshal.GetLastPInvokeError()})");
                CloseHandle(handle);
                Marshal.FreeHGlobal(reasonString);
                return null;
            }

            return new WindowsPowerRequest(handle, reasonString);
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            PowerClearRequest(handle, PowerRequestSystemRequired);
            CloseHandle(handle);
            Marshal.FreeHGlobal(reasonString);
        }
    }
}
