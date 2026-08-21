using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// The short-lived subtitle files handed to ffmpeg (burn-in, transparent render, cut, re-encode,
/// blank video), and the sweep that removes them again when the window closes.
/// <para>
/// These used to be written as <c>Path.GetTempFileName() + extension</c>, which leaks two files
/// per call: the empty <c>tmpXXXX.tmp</c> that <c>GetTempFileName()</c> itself creates and whose
/// name is then never used, plus the <c>tmpXXXX.tmp.ass</c> actually written. Neither was ever
/// deleted, so a session of previews left a pile of gibberish-named files in the temp folder
/// (#13332).
/// </para>
/// </summary>
public sealed class TempSubtitleFiles
{
    /// <summary>
    /// Name prefix, so a file that does survive (killed process, locked file) is recognisably
    /// Subtitle Edit's rather than one more anonymous <c>tmpXXXX</c>.
    /// </summary>
    private const string FileNamePrefix = "se-sub-";

    private readonly List<string> _fileNames = new();

    /// <summary>
    /// Writes the subtitle to a fresh temp file in the given format and tracks it for
    /// <see cref="Delete"/>.
    /// </summary>
    public string Write(Subtitle subtitle, SubtitleFormat format)
    {
        var fileName = Path.Combine(Path.GetTempPath(), FileNamePrefix + Guid.NewGuid().ToString("N") + format.Extension);
        File.WriteAllText(fileName, format.ToText(subtitle, string.Empty));
        _fileNames.Add(fileName);
        return fileName;
    }

    /// <summary>
    /// Returns a fresh temp file name with the given extension and tracks it for
    /// <see cref="Delete"/>. For callers that write the content themselves.
    /// </summary>
    public string GetFileName(string extension)
    {
        var fileName = Path.Combine(Path.GetTempPath(), FileNamePrefix + Guid.NewGuid().ToString("N") + extension);
        _fileNames.Add(fileName);
        return fileName;
    }

    /// <summary>
    /// Removes every file written so far. Best effort - a file an ffmpeg process still holds open
    /// is not worth an error dialog on close.
    /// </summary>
    public void Delete()
    {
        foreach (var fileName in _fileNames)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch (Exception ex)
            {
                Se.LogError(ex, $"Could not delete the temporary subtitle file \"{fileName}\"");
            }
        }

        _fileNames.Clear();
    }
}
