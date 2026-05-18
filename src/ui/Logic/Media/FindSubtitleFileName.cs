using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace Nikse.SubtitleEdit.Logic.Media;

public static class FindSubtitleFileName
{
    public static bool TryFindSubtitleFileName(string videoFileName, out string subtitleFileName)
    {
        subtitleFileName = string.Empty;

        if (string.IsNullOrWhiteSpace(videoFileName))
        {
            return false;
        }

        var rawDirectory = Path.GetDirectoryName(videoFileName);
        // Drop targets always carry an absolute path, but treat a missing directory
        // as "current directory" so relative-path callers still get the fallback scan.
        var directory = string.IsNullOrEmpty(rawDirectory) ? "." : rawDirectory;
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(videoFileName);
        var extensions = SubtitleFormat.AllSubtitleFormats
            .Select(p => p.Extension)
            .Where(e => !string.IsNullOrEmpty(e))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (TryFindExactOrTrimmed(directory, nameWithoutExtension, extensions, out subtitleFileName))
        {
            return true;
        }

        // Fall back: scan the directory for files like "{name}.{lang}.{subExt}" (e.g. "movie.en.srt")
        if (Directory.Exists(directory))
        {
            try
            {
                foreach (var candidate in Directory.EnumerateFiles(directory, nameWithoutExtension + ".*"))
                {
                    var ext = Path.GetExtension(candidate);
                    if (!string.IsNullOrEmpty(ext) &&
                        extensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                    {
                        subtitleFileName = candidate;
                        return true;
                    }
                }
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is SecurityException ||
                ex is ArgumentException)
            {
                // ignore - directory not accessible or path invalid
            }
        }

        return false;
    }

    private static bool TryFindExactOrTrimmed(string directory, string nameWithoutExtension, IList<string> extensions, out string subtitleFileName)
    {
        subtitleFileName = string.Empty;

        if (string.IsNullOrEmpty(nameWithoutExtension))
        {
            return false;
        }

        foreach (var extension in extensions)
        {
            var candidate = Path.Combine(directory, nameWithoutExtension + extension);
            if (File.Exists(candidate))
            {
                subtitleFileName = candidate;
                return true;
            }
        }

        var dotIndex = nameWithoutExtension.LastIndexOf('.');
        if (dotIndex > 0 && TryFindExactOrTrimmed(directory, nameWithoutExtension.Substring(0, dotIndex), extensions, out subtitleFileName))
        {
            return true;
        }

        var underscoreIndex = nameWithoutExtension.LastIndexOf('_');
        if (underscoreIndex > 0 && TryFindExactOrTrimmed(directory, nameWithoutExtension.Substring(0, underscoreIndex), extensions, out subtitleFileName))
        {
            return true;
        }

        return false;
    }
}
