using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Logic;

public static class EncodingHelper
{
    // Stable sentinel for the batch-convert "use the source file's encoding" option.
    // Stored verbatim in settings, so kept English (matches "UTF-8 with BOM" et al.).
    public const string TryToUseSourceEncoding = "Try to use source encoding";

    public static List<TextEncoding> GetEncodings()
    {
        var encodingList = new List<TextEncoding>();
        encodingList.Insert(TextEncoding.Utf8WithBomIndex, new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom));
        encodingList.Insert(TextEncoding.Utf8WithoutBomIndex, new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithoutBom));
        foreach (var encodingInfo in Encoding.GetEncodings())
        {
            var encoding = encodingInfo.GetEncoding();
            if (encoding.CodePage >= 874 && !encoding.IsEbcdic() && !encoding.CodePage.Equals(Encoding.UTF8.CodePage))
            {
                var item = new TextEncoding(encoding, null);
                encodingList.Add(item);
            }
        }

        return encodingList;
    }

    public static List<Encoding> GetRawEncodings()
    {
        var encodingList = new List<Encoding>();
        foreach (var encodingInfo in Encoding.GetEncodings())
        {
            var encoding = encodingInfo.GetEncoding();
            if (encoding.CodePage >= 874 && !encoding.IsEbcdic() && !encoding.CodePage.Equals(Encoding.UTF8.CodePage))
            {
                encodingList.Add(encoding);
            }
        }

        return encodingList;
    }

    /// <summary>
    /// Resolves a target-encoding <see cref="TextEncoding.DisplayName"/> (as stored in
    /// settings) to a concrete <see cref="Encoding"/>. Honors the <see cref="TryToUseSourceEncoding"/>
    /// sentinel by detecting the source file's encoding - a binary source (container, image
    /// or binary subtitle format) has no text encoding, so it gets <see cref="GetBinarySourceEncoding"/>.
    /// Falls back to UTF-8 with BOM when the name is empty or unknown.
    /// </summary>
    public static Encoding ResolveEncoding(string? displayName, string? sourceFile)
    {
        if (string.Equals(displayName, TryToUseSourceEncoding, System.StringComparison.Ordinal))
        {
            if (string.IsNullOrEmpty(sourceFile) || !File.Exists(sourceFile))
            {
                return GetBinarySourceEncoding(Se.Settings.General.DefaultEncoding);
            }

            var detected = LanguageAutoDetect.GetEncodingFromFile(sourceFile);
            if (IsBinaryFile(sourceFile, detected))
            {
                return GetBinarySourceEncoding(Se.Settings.General.DefaultEncoding);
            }

            // Detection returns the shared Encoding.UTF8 (which always writes a BOM) for
            // UTF-8 with or without BOM and for plain ASCII - keep the source's BOM choice (#15489).
            if (detected.CodePage == Encoding.UTF8.CodePage)
            {
                return new UTF8Encoding(FileUtil.HasUtf8Bom(sourceFile));
            }

            return detected;
        }

        if (string.IsNullOrEmpty(displayName) ||
            string.Equals(displayName, TextEncoding.Utf8WithBom, System.StringComparison.Ordinal))
        {
            return new UTF8Encoding(true);
        }

        if (string.Equals(displayName, TextEncoding.Utf8WithoutBom, System.StringComparison.Ordinal))
        {
            return new UTF8Encoding(false);
        }

        var match = GetEncodings().FirstOrDefault(e => e.DisplayName == displayName);
        return match?.Encoding ?? new UTF8Encoding(true);
    }

    /// <summary>
    /// The encoding for text written from a source that has none (e.g. OCR of a .sup, or a
    /// track from a .mkv): the default encoding when it is UTF-8 with or without BOM, otherwise
    /// UTF-8 with BOM - a single-byte default code page could not hold arbitrary OCR'd or
    /// extracted text.
    /// </summary>
    public static Encoding GetBinarySourceEncoding(string? defaultEncodingName)
    {
        return new UTF8Encoding(!string.Equals(defaultEncodingName, TextEncoding.Utf8WithoutBom, System.StringComparison.Ordinal));
    }

    /// <summary>
    /// A text subtitle never contains a NUL byte unless it is UTF-16/32, which detection
    /// already recognized - so a NUL in the first 8 KB means a binary file.
    /// </summary>
    private static bool IsBinaryFile(string fileName, Encoding detected)
    {
        if (detected.CodePage is 1200 or 1201 or 12000 or 12001)
        {
            return false;
        }

        try
        {
            using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var buffer = new byte[8192];
            var count = stream.Read(buffer, 0, buffer.Length);
            return System.Array.IndexOf(buffer, (byte)0, 0, count) >= 0;
        }
        catch
        {
            return false;
        }
    }
}
