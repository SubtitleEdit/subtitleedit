using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers;

/// <summary>
/// Describes the OpenGL implementation a player control actually received, so a
/// silent fallback to a CPU rasterizer (e.g. a Flatpak whose NVIDIA GL extension
/// no longer matches the host driver) shows up in the player badge and the log
/// instead of presenting as a broken video preview.
/// </summary>
public static class OpenGlRendererInfo
{
    private static readonly string[] SoftwareRendererMarkers =
    {
        "llvmpipe",
        "softpipe",
        "swrast",
        "swiftshader",
        "software rasterizer",
    };

    public static bool IsSoftwareRenderer(string? renderer)
    {
        if (string.IsNullOrWhiteSpace(renderer))
        {
            return false;
        }

        foreach (var marker in SoftwareRendererMarkers)
        {
            if (renderer.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Short name for the player badge, e.g. "OpenGL" or "OpenGL (llvmpipe)".
    /// </summary>
    public static string GetPlayerSubName(string? renderer)
    {
        if (!IsSoftwareRenderer(renderer))
        {
            return "OpenGL";
        }

        var shortName = renderer!.Trim();
        var cut = shortName.IndexOfAny(new[] { '(', ',' });
        if (cut > 0)
        {
            shortName = shortName.Substring(0, cut).Trim();
        }

        return string.IsNullOrEmpty(shortName) ? "OpenGL (software)" : $"OpenGL ({shortName})";
    }

    /// <summary>
    /// Writes a single line to the error log when the context is a software rasterizer.
    /// </summary>
    public static void LogIfSoftwareRenderer(string playerName, string? vendor, string? renderer, string? version)
    {
        if (!IsSoftwareRenderer(renderer))
        {
            return;
        }

        try
        {
            SeLogger.Error(
                $"{playerName}: video is rendering in software (GL_VENDOR={vendor}, GL_RENDERER={renderer}, GL_VERSION={version}). " +
                "Playback will be slow or may not display correctly. Your GPU driver is not providing hardware OpenGL to this application " +
                "(for Flatpak on NVIDIA, install the org.freedesktop.Platform.GL.nvidia-* extension matching the host driver version).");
        }
        catch
        {
            // logging must never break player init
        }
    }
}
