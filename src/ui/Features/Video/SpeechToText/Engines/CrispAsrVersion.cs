using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// Cached <c>crispasr --version</c> probe. Used by the speech-to-text engine settings
/// dialog and by Chatterbox / Qwen3 (CrispASR) TTS settings to show the user which
/// CrispASR runtime they have installed. <c>crispasr --version</c> on v0.6.10 prints
/// something like:
///
///   <c>=== build info ===</c>
///   <c>  version       : 0.6.10</c>
///   <c>  git sha       : 962a3913</c>
///   <c>  git date      : 2026-05-23T19:54:13Z</c>
///   <c>  ...</c>
///
/// Older releases printed a single banner line like
/// <c>crispasr 0.6.7 (git ..., Release) [backends: ...]</c>; we accept either form.
/// </summary>
public static class CrispAsrVersion
{
    // Path -> (mtime, version). Cache invalidates when the binary on disk changes
    // (re-download / update) so we don't show a stale version after an update.
    private static readonly object _cacheLock = new();
    private static string? _cachedExePath;
    private static DateTime _cachedExeMtimeUtc;
    private static string? _cachedVersion;

    // Matches either `version : 0.6.10` (structured --version output) or
    // `crispasr 0.6.10` (legacy single-line banner).
    private static readonly Regex VersionRegex = new(
        @"(?:^\s*version\s*:\s*|\bcrispasr\s+)([\w.\-+]+)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    /// <summary>
    /// Returns the CrispASR version string (e.g. "0.6.10") for the binary at
    /// <paramref name="exePath"/>, or null when the binary is missing, fails to launch,
    /// or doesn't emit a recognisable version line within a short timeout. Safe to
    /// call repeatedly — each unique (path, mtime) pair is probed at most once.
    /// </summary>
    public static string? TryGet(string exePath)
    {
        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
        {
            return null;
        }

        DateTime mtime;
        try
        {
            mtime = File.GetLastWriteTimeUtc(exePath);
        }
        catch
        {
            return null;
        }

        lock (_cacheLock)
        {
            if (_cachedExePath == exePath && _cachedExeMtimeUtc == mtime)
            {
                return _cachedVersion;
            }
        }

        var version = Probe(exePath);

        lock (_cacheLock)
        {
            _cachedExePath = exePath;
            _cachedExeMtimeUtc = mtime;
            _cachedVersion = version;
        }

        return version;
    }

    private static string? Probe(string exePath)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = Path.GetDirectoryName(exePath) ?? string.Empty,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            psi.ArgumentList.Add("--version");

            using var process = Process.Start(psi);
            if (process == null)
            {
                return null;
            }

            // --version exits immediately after printing one line; 5 s is generous.
            if (!process.WaitForExit(5000))
            {
                try { process.Kill(entireProcessTree: true); } catch { /* best-effort */ }
                return null;
            }

            // Some builds print the banner on stdout, others on stderr — read both.
            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            var combined = stdout + "\n" + stderr;

            var match = VersionRegex.Match(combined);
            return match.Success ? match.Groups[1].Value : null;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"CrispAsrVersion: failed to probe '{exePath}'");
            return null;
        }
    }
}
