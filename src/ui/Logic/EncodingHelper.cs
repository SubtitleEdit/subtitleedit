using Nikse.SubtitleEdit.Core.Common;
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
    /// sentinel by detecting the source file's encoding. Falls back to UTF-8 with BOM
    /// when the name is empty, unknown, or detection fails.
    /// </summary>
    public static Encoding ResolveEncoding(string? displayName, string? sourceFile)
    {
        if (string.Equals(displayName, TryToUseSourceEncoding, System.StringComparison.Ordinal))
        {
            if (!string.IsNullOrEmpty(sourceFile) && File.Exists(sourceFile))
            {
                return LanguageAutoDetect.GetEncodingFromFile(sourceFile);
            }
            return new UTF8Encoding(true);
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
}
