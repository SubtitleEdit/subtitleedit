using System;
using System.Globalization;
using System.IO;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public class SpellCheckDictionaryDisplay
{
    public string Name { get; set; } = string.Empty;
    public string DictionaryFileName { get; set; } = string.Empty;

    public override string ToString()
    {
        return Name;
    }

    public static string GetTwoLetterLanguageCode(SpellCheckDictionaryDisplay? language)
    {
        if (language == null)
        {
            return "en";
        }

        var fileNameOnly = System.IO.Path.GetFileNameWithoutExtension(language.DictionaryFileName);

        try
        {
            var ci = CultureInfo.GetCultureInfo(fileNameOnly.Replace('_', '-'));
            return ci.TwoLetterISOLanguageName;
        }
        catch
        {
            // ignore
        }


        if (fileNameOnly.Contains("English", StringComparison.OrdinalIgnoreCase))
        {
            return "en";
        }

        if (fileNameOnly.Contains("Spanish", StringComparison.OrdinalIgnoreCase))
        {
            return "es";
        }

        if (fileNameOnly.Contains("French", StringComparison.OrdinalIgnoreCase))
        {
            return "fr";
        }

        if (fileNameOnly.Contains("German", StringComparison.OrdinalIgnoreCase))
        {
            return "de";
        }

        if (fileNameOnly.Contains("Italian", StringComparison.OrdinalIgnoreCase))
        {
            return "it";
        }

        if (fileNameOnly.Contains("Dutch", StringComparison.OrdinalIgnoreCase))
        {
            return "nl";
        }

        if (fileNameOnly.Contains("Portuguese", StringComparison.OrdinalIgnoreCase))
        {
            return "pt";
        }

        if (fileNameOnly.Contains("Russian", StringComparison.OrdinalIgnoreCase))
        {
            return "ru";
        }

        if (fileNameOnly.Contains("Swedish", StringComparison.OrdinalIgnoreCase))
        {
            return "sv";
        }

        if (fileNameOnly.Contains("Danish", StringComparison.OrdinalIgnoreCase))
        {
            return "da";
        }

        if (fileNameOnly.Contains("Norwegian", StringComparison.OrdinalIgnoreCase))
        {
            return "no";
        }

        if (fileNameOnly.Contains("Finnish", StringComparison.OrdinalIgnoreCase))
        {
            return "fi";
        }

        if (fileNameOnly.Contains("Polish", StringComparison.OrdinalIgnoreCase))
        {
            return "pl";
        }

        if (fileNameOnly.Contains("Greek", StringComparison.OrdinalIgnoreCase))
        {
            return "el";
        }

        if (fileNameOnly.Contains("Turkish", StringComparison.OrdinalIgnoreCase))
        {
            return "tr";
        }

        if (fileNameOnly.Contains("Hungarian", StringComparison.OrdinalIgnoreCase))
        {
            return "hu";
        }

        if (fileNameOnly.Contains("Czech", StringComparison.OrdinalIgnoreCase))
        {
            return "cs";
        }

        if (fileNameOnly.Contains("Slovak", StringComparison.OrdinalIgnoreCase))
        {
            return "sk";
        }

        if (fileNameOnly.Contains("Romanian", StringComparison.OrdinalIgnoreCase))
        {
            return "ro";
        }

        if (fileNameOnly.Contains("Bulgarian", StringComparison.OrdinalIgnoreCase))
        {
            return "bg";
        }

        if (fileNameOnly.Contains("Croatian", StringComparison.OrdinalIgnoreCase))
        {
            return "hr";
        }

        if (language.Name.Contains("Serbian", StringComparison.OrdinalIgnoreCase))
        {
            return "sr";
        }

        if (fileNameOnly.Contains("Ukrainian", StringComparison.OrdinalIgnoreCase))
        {
            return "uk";
        }

        if (fileNameOnly.Contains("Hebrew", StringComparison.OrdinalIgnoreCase))
        {
            return "he";
        }

        if (fileNameOnly.Contains("Arabic", StringComparison.OrdinalIgnoreCase))
        {
            return "ar";
        }

        if (fileNameOnly.Contains("Hindi", StringComparison.OrdinalIgnoreCase))
        {
            return "hi";
        }

        if (fileNameOnly.Contains("Japanese", StringComparison.OrdinalIgnoreCase))
        {
            return "ja";
        }

        if (fileNameOnly.Contains("Chinese", StringComparison.OrdinalIgnoreCase))
        {
            return "zh";
        }

        if (fileNameOnly.Contains("Korean", StringComparison.OrdinalIgnoreCase))
        {
            return "ko";
        }

        return "en";
    }

    public string GetThreeLetterCode()
    {
        var twoLetterCode = GetTwoLetterLanguageCode(this);
        var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(twoLetterCode);
        return threeLetterCode ?? "eng";
    }

    internal string? GetFiveLetterLanguageName()
    {
        var fileName = Path.GetFileNameWithoutExtension(DictionaryFileName).Replace('-', '_');
        return GetFiveLetterLanguageName(fileName);
    }

    internal static string? GetFiveLetterLanguageName(string fileName)
    {
        if (fileName == "es_ANY" || fileName == "es-ANY")
        {
            return "es_ES";
        }

        if (fileName == "russian_aot" || fileName == "russian-aot")
        {
            return "ru_RU";
        }

        if (fileName == "fr_classique" || fileName == "fr_toutesvariantes")
        {
            return "fr_FR";
        }

        if (fileName == "eu")
        {
            return "eu-ES";
        }

        if (fileName.Length == 2)
        {
            fileName = fileName.ToLowerInvariant() + "_" + fileName.ToUpperInvariant();
        }

        if (fileName.Length < 5)
        {
            return null;
        }

        var name = fileName.Substring(0, 2).ToLowerInvariant() + fileName.Substring(2, 1) + fileName.Substring(3, 2).ToUpperInvariant();
        if (!fileName.Contains('_'))
        {
            return null;
        }

        if (fileName.Length > 5)
        {
            fileName = fileName.Substring(0, 5);
        }

        return fileName;
    }
}
