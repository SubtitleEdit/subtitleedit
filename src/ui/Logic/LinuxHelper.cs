using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic;

public static class LinuxHelper
{
    public static bool MakeExecutable(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        // 1. Modern .NET Approach (.NET 7+)
        // Uses native syscalls instead of spawning a shell process.
        try
        {
            // Verify we are actually on a Unix-like system
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Get current permissions
                var currentMode = File.GetUnixFileMode(filePath);

                // Bitwise OR to add Execute permissions for User, Group, and Other
                // Equivalent to chmod +x
                var newMode = currentMode | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;

                File.SetUnixFileMode(filePath, newMode);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Native .NET permission change failed: {ex.Message}");
            // Fallback to chmod command if the native method fails
        }

        // 2. Legacy Process Approach (Fallback)
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    // Do not hardcode "/bin/chmod". Use "chmod" and let the PATH find it.
                    // chmod is often in /usr/bin/ on modern distros.
                    FileName = "chmod",
                    Arguments = $"+x \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            // Read output before WaitForExit to avoid deadlocks if output buffer fills
            // (Though chmod output is usually minimal)
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine($"chmod process failed: {error}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"chmod execution failed: {ex.Message}");
            return false;
        }
    }
}