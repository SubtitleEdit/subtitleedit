using System;
using System.Collections.Generic;
using System.Text.Json;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Config.Language;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

/// <summary>
/// Maps translated UI strings back to the built-in English ones, so the Settings search also
/// finds a setting by the English name used in docs and tutorials (#15945).
/// </summary>
public static class EnglishLabelLookup
{
    private static Dictionary<string, string>? _map;
    private static SeLanguage? _mapLanguage;

    /// <summary>The built-in English strings.</summary>
    public static SeLanguage English { get; } = new();

    /// <summary>
    /// The English text of <paramref name="label"/>, or null when the UI is English, the label
    /// is not a translated UI string, or the translation equals the English text. Several English
    /// strings with the same translation are joined by a newline.
    /// </summary>
    public static string? GetEnglish(string? label)
    {
        if (string.IsNullOrEmpty(label) || Se.LoadedLanguageName == null)
        {
            return null;
        }

        if (_map == null || !ReferenceEquals(_mapLanguage, Se.Language))
        {
            _map = Build(Se.Language, English);
            _mapLanguage = Se.Language;
        }

        return _map.TryGetValue(label, out var english) ? english : null;
    }

    internal static Dictionary<string, string> Build(SeLanguage translated, SeLanguage english)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            var translatedRoot = JsonSerializer.SerializeToElement(translated, SeLanguageJsonContext.Default.SeLanguage);
            var englishRoot = JsonSerializer.SerializeToElement(english, SeLanguageJsonContext.Default.SeLanguage);
            Walk(translatedRoot, englishRoot, map);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to build English label lookup");
        }

        return map;
    }

    private static void Walk(JsonElement translated, JsonElement english, Dictionary<string, string> map)
    {
        if (translated.ValueKind == JsonValueKind.Object && english.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in translated.EnumerateObject())
            {
                if (english.TryGetProperty(property.Name, out var englishValue))
                {
                    Walk(property.Value, englishValue, map);
                }
            }

            return;
        }

        if (translated.ValueKind != JsonValueKind.String || english.ValueKind != JsonValueKind.String)
        {
            return;
        }

        var translatedText = translated.GetString();
        var englishText = english.GetString();
        if (string.IsNullOrEmpty(translatedText) || string.IsNullOrEmpty(englishText) ||
            translatedText == englishText)
        {
            return;
        }

        if (!map.TryGetValue(translatedText, out var existing))
        {
            map[translatedText] = englishText;
        }
        else if (Array.IndexOf(existing.Split('\n'), englishText) < 0)
        {
            map[translatedText] = existing + "\n" + englishText;
        }
    }
}
