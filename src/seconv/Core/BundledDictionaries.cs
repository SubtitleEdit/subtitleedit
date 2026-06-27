using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace SeConv.Core;

/// <summary>
/// Extracts the dictionaries bundled into the seconv assembly (Hunspell + *_OCRFixReplaceList.xml)
/// to a per-user cache folder on first use, so the "Fix common OCR errors" pass works without the
/// caller supplying <c>--dictionary-folder</c>. (#11744)
/// </summary>
internal static class BundledDictionaries
{
    private const string ResourceName = "SeConv.Dictionaries.zip";
    private static string? _folder;

    /// <summary>
    /// Returns the folder with the extracted bundled dictionaries, or an empty string if the bundle
    /// is missing or extraction fails. Extracted once per process; re-extracts when the bundle size
    /// changes (i.e. after an upgrade).
    /// </summary>
    public static string GetFolder()
    {
        if (_folder != null)
        {
            return _folder;
        }

        _folder = string.Empty;
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(ResourceName);
            if (stream == null)
            {
                return _folder;
            }

            var dir = Path.Combine(Path.GetTempPath(), "seconv-dictionaries");
            var marker = Path.Combine(dir, ".bundle-size");
            var token = stream.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (!File.Exists(marker) || File.ReadAllText(marker) != token)
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, recursive: true);
                }

                Directory.CreateDirectory(dir);
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    zip.ExtractToDirectory(dir, overwriteFiles: true);
                }

                File.WriteAllText(marker, token);
            }

            _folder = dir;
        }
        catch
        {
            _folder = string.Empty;
        }

        return _folder;
    }
}
