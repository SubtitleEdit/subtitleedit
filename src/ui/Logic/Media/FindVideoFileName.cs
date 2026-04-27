using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Media;

public static class FindVideoFileName
{
    public static bool TryFindVideoFileName(string inputFileName, out string videoFileName)
    {
        videoFileName = string.Empty;

        if (string.IsNullOrWhiteSpace(inputFileName))
        {
            return false;
        }

        var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = TryFindVideoFileNameInner(inputFileName, out videoFileName, visitedPaths);

        if (!result && Se.Settings.Video.OpenSearchParentFolder)
        {
            // Try again with the folder one level up (if any)
            var directory = Path.GetDirectoryName(inputFileName);
            if (!string.IsNullOrEmpty(directory))
            {
                var parentDirectory = Path.GetDirectoryName(directory);
                if (!string.IsNullOrEmpty(parentDirectory))
                {
                    var fileName = Path.GetFileName(inputFileName);
                    var newInputFileName = Path.Combine(parentDirectory, fileName);
                    result = TryFindVideoFileNameInner(newInputFileName, out videoFileName, visitedPaths);
                }
            }
        }

        return result;
    }

    private static bool TryFindVideoFileNameInner(string inputFileName, out string videoFileName, HashSet<string> visitedPaths)
    {
        videoFileName = string.Empty;

        if (string.IsNullOrWhiteSpace(inputFileName))
        {
            return false;
        }

        // Prevent infinite recursion
        if (visitedPaths.Contains(inputFileName))
        {
            return false;
        }
        visitedPaths.Add(inputFileName);

        // Try appending video/audio extensions to the input file name
        foreach (var extension in Utilities.VideoFileExtensions.Concat(Utilities.AudioFileExtensions))
        {
            var candidateFileName = inputFileName + extension;
            if (File.Exists(candidateFileName))
            {
                videoFileName = candidateFileName;
                return true;
            }
        }

        // Try removing file extension (e.g., "movie.en.srt" -> "movie.en")
        var lastDotIndex = inputFileName.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            var withoutExtension = inputFileName.Substring(0, lastDotIndex);
            if (TryFindVideoFileNameInner(withoutExtension, out videoFileName, visitedPaths))
            {
                return true;
            }
        }

        // Try removing suffix after underscore (e.g., "movie_en" -> "movie")
        var lastUnderscoreIndex = inputFileName.LastIndexOf('_');
        if (lastUnderscoreIndex > 0)
        {
            var withoutSuffix = inputFileName.Substring(0, lastUnderscoreIndex);
            if (TryFindVideoFileNameInner(withoutSuffix, out videoFileName, visitedPaths))
            {
                return true;
            }
        }

        return false;
    }
}