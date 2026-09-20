using Nikse.SubtitleEdit.Core.Common;
using System.Globalization;

namespace Nikse.SubtitleEdit.UiLogic.BatchConvert;

/// <summary>
/// Expands <see cref="TransportStreamExportSettings.FileNameAppend"/> for one track.
/// </summary>
public static class TransportStreamFileNameEnding
{
    /// <summary>
    /// Builds the ending for a track. <paramref name="languageCode"/> is the track's language
    /// as found in the stream (two- or three-letter, may be empty); when it is empty the
    /// <paramref name="trackId"/> (PID or teletext page) stands in for both codes, so two
    /// language-less tracks never collide.
    /// </summary>
    public static string Format(string? template, string? languageCode, int trackId)
    {
        if (string.IsNullOrEmpty(template))
        {
            return string.Empty;
        }

        var twoLetter = string.Empty;
        var threeLetter = string.Empty;
        var code = (languageCode ?? string.Empty).Trim().ToLowerInvariant();
        if (code.Length == 3)
        {
            threeLetter = code;
            twoLetter = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(code);
        }
        else if (code.Length == 2)
        {
            twoLetter = code;
            threeLetter = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(code);
        }
        else if (code.Length > 0)
        {
            twoLetter = code;
            threeLetter = code;
        }

        if (string.IsNullOrEmpty(twoLetter))
        {
            twoLetter = threeLetter;
        }

        if (string.IsNullOrEmpty(threeLetter))
        {
            threeLetter = twoLetter;
        }

        if (string.IsNullOrEmpty(twoLetter))
        {
            var id = trackId.ToString(CultureInfo.InvariantCulture);
            twoLetter = id;
            threeLetter = id;
        }

        return template
            .Replace(TransportStreamExportSettings.PlaceholderTwoLetterUppercase, twoLetter.ToUpperInvariant())
            .Replace(TransportStreamExportSettings.PlaceholderThreeLetterUppercase, threeLetter.ToUpperInvariant())
            .Replace(TransportStreamExportSettings.PlaceholderTwoLetter, twoLetter)
            .Replace(TransportStreamExportSettings.PlaceholderThreeLetter, threeLetter);
    }

    /// <summary>Sample shown in the settings dialog: "MyVideoFile" + ending + ".sup" with the tokens filled in.</summary>
    public static string MakeSample(string? template)
    {
        return "MyVideoFile" + Format(template, "eng", 0) + ".sup";
    }
}
