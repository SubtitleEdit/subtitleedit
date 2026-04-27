using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Compression;

public interface IZipUnpacker
{
    /// <summary>
    /// Unpacks a zip stream to the output path.
    /// </summary>
    /// <param name="zipStream">Zip data.</param>
    /// <param name="outputPath">Where to unpack.</param>
    /// <param name="skipFolderLevel">Path to skip from zip archive. Must be the begging of a path.</param>
    /// <param name="allToOutputPath">Unpack all files to outputPath. Do not use folder structure from zip archive.</param>
    /// <param name="allowedExtensions">If not empty, then files must match extension.</param>
    /// <param name="outputFileNames">If not null, then output file names will be added.</param>
    void UnpackZipStream(
        Stream zipStream,
        string outputPath,
        string skipFolderLevel,
        bool allToOutputPath,
        List<string> allowedExtensions,
        List<string>? outputFileNames);

    /// <summary>
    /// Unpacks a zip stream to the output path.
    /// </summary>
    /// <param name="zipStream">Zip data.</param>
    /// <param name="outputPath">Where to unpack.</param>
    void UnpackZipStream(Stream zipStream, string outputPath);

    /// <summary>
    /// Unpacks a file name in a zip stream to an UTF8 string
    /// </summary>
    /// <param name="zipStream">Zip data.</param>
    /// <param name="fileName">File name to return as UTF8 text.</param>
    string? UnpackTextFileFromZipStream(Stream zipStream, string fileName);
}