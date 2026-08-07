using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Detection for the xdg document portal used by sandboxed (Flatpak) builds on Linux.
/// When a file dialog targets a folder the sandbox cannot access directly (e.g. an SMB
/// share with only --filesystem=home granted), the portal grants access to exactly one
/// file name - the name confirmed in the dialog - exposed through a FUSE mount under
/// $XDG_RUNTIME_DIR/doc/. Writing any other name into that granted folder leaves a
/// hidden ".xdp-&lt;name&gt;-&lt;random&gt;" temp file on the real file system, so a file
/// extension appended after the dialog has closed can never materialize (issue #13308).
/// </summary>
public static class DocumentPortal
{
    /// <summary>
    /// True when running inside a Flatpak sandbox, where file dialogs may hand out
    /// document portal paths.
    /// </summary>
    public static bool IsSandboxed { get; } = OperatingSystem.IsLinux() && File.Exists("/.flatpak-info");

    /// <summary>
    /// True when the path goes through the document portal FUSE mount, i.e. writes are
    /// restricted to the single file name granted by the portal dialog.
    /// </summary>
    public static bool IsPortalPath(string? path)
    {
        if (!IsSandboxed || string.IsNullOrEmpty(path))
        {
            return false;
        }

        return IsPortalPath(path, Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR"));
    }

    internal static bool IsPortalPath(string path, string? runtimeDir)
    {
        if (!string.IsNullOrEmpty(runtimeDir) &&
            path.StartsWith(runtimeDir.TrimEnd('/') + "/doc/", StringComparison.Ordinal))
        {
            return true;
        }

        // XDG_RUNTIME_DIR is practically always /run/user/<uid>, so pattern-match that
        // layout as a fallback for when the environment variable is not set.
        var parts = path.Split('/');
        return parts.Length > 5 && parts[0].Length == 0 && parts[1] == "run" && parts[2] == "user" && parts[4] == "doc";
    }
}
