using Nikse.SubtitleEdit.Core.Common;
using System.Net.Http;
using System.Text.Json;

namespace Nikse.SubtitleEdit.UiLogic.Grammar;

/// <summary>Everything but the text that goes into a /v2/check call.</summary>
public class LanguageToolOptions
{
    /// <summary>A long code like "en-US", or "auto" to let the server detect the language.</summary>
    public string Language { get; set; } = LanguageToolLanguage.AutoCode;

    /// <summary>Variants to prefer when detecting, e.g. "en-US,de-DE" - only used with "auto".</summary>
    public string PreferredVariants { get; set; } = string.Empty;

    /// <summary>Turns on the rules LanguageTool considers too opinionated for the default level.</summary>
    public bool Picky { get; set; }

    /// <summary>Comma separated rule ids to switch off, e.g. "WHITESPACE_RULE,UPPERCASE_SENTENCE_START".</summary>
    public string DisabledRules { get; set; } = string.Empty;

    /// <summary>Premium/self-hosted account - both this and <see cref="ApiKey"/> are needed, or neither.</summary>
    public string Username { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Talks to a LanguageTool HTTP server - either languagetool.org or a self-hosted one
/// (e.g. the erikvl87/languagetool docker image). See https://languagetool.org/http-api/
/// </summary>
public class LanguageToolClient : IDisposable
{
    public const string DefaultServerUrl = "https://api.languagetool.org";

    // LanguageTool happily returns 40+ candidates for an unknown word; only the best few are useful
    // in a drop-down, and the rest just make the list hard to read.
    private const int MaxReplacements = 8;

    private readonly HttpClient _httpClient;

    public LanguageToolClient()
    {
        _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
        _httpClient.Timeout = TimeSpan.FromMinutes(2);
    }

    /// <summary>
    /// Builds an endpoint url from what the user typed. Takes "host", "https://host", "https://host/",
    /// "https://host/v2" and "https://host/v2/check" alike - people paste all of them.
    /// </summary>
    public static string GetEndpointUrl(string? serverUrl, string path)
    {
        var url = (serverUrl ?? string.Empty).Trim();
        if (url.Length == 0)
        {
            url = DefaultServerUrl;
        }

        if (!url.Contains("://"))
        {
            url = "https://" + url;
        }

        url = url.TrimEnd('/');
        foreach (var suffix in new[] { "/v2/check", "/v2/languages", "/v2" })
        {
            if (url.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                url = url.Substring(0, url.Length - suffix.Length);
                break;
            }
        }

        return url.TrimEnd('/') + path;
    }

    /// <summary>The languages the server has rules for - also the cheapest way to check it answers at all.</summary>
    public async Task<List<LanguageToolLanguage>> GetLanguagesAsync(string? serverUrl, CancellationToken cancellationToken)
    {
        var url = GetEndpointUrl(serverUrl, "/v2/languages");
        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            SeLogger.Error("LanguageTool: /v2/languages failed: " + body);
            throw new HttpRequestException(ShortError(body, response.StatusCode.ToString()));
        }

        return ParseLanguages(body);
    }

    public async Task<List<LanguageToolMatch>> CheckAsync(string? serverUrl, string dataJson, LanguageToolOptions options, CancellationToken cancellationToken)
    {
        var url = GetEndpointUrl(serverUrl, "/v2/check");
        var language = string.IsNullOrWhiteSpace(options.Language) ? LanguageToolLanguage.AutoCode : options.Language.Trim();
        var form = new List<KeyValuePair<string, string>>
        {
            new("data", dataJson),
            new("language", language),
        };

        if (language == LanguageToolLanguage.AutoCode && !string.IsNullOrWhiteSpace(options.PreferredVariants))
        {
            // Rejected by the server together with an explicit language, so only sent with "auto".
            form.Add(new KeyValuePair<string, string>("preferredVariants", options.PreferredVariants.Trim()));
        }

        if (options.Picky)
        {
            form.Add(new KeyValuePair<string, string>("level", "picky"));
        }

        if (!string.IsNullOrWhiteSpace(options.DisabledRules))
        {
            form.Add(new KeyValuePair<string, string>("disabledRules", options.DisabledRules.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.ApiKey))
        {
            form.Add(new KeyValuePair<string, string>("username", options.Username.Trim()));
            form.Add(new KeyValuePair<string, string>("apiKey", options.ApiKey.Trim()));
        }

        using var content = new FormUrlEncodedContent(form);
        using var response = await _httpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            SeLogger.Error("LanguageTool: /v2/check failed: " + body);
            throw new HttpRequestException(ShortError(body, response.StatusCode.ToString()));
        }

        return ParseMatches(body);
    }

    public static List<LanguageToolLanguage> ParseLanguages(string json)
    {
        var languages = new List<LanguageToolLanguage>();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
        {
            return languages;
        }

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var name = GetString(element, "name");
            var code = GetString(element, "code");
            var longCode = GetString(element, "longCode");
            if (longCode.Length == 0)
            {
                longCode = code;
            }

            if (longCode.Length == 0)
            {
                continue;
            }

            languages.Add(new LanguageToolLanguage
            {
                Name = name.Length == 0 ? longCode : name,
                Code = code,
                LongCode = longCode,
            });
        }

        return languages;
    }

    public static List<LanguageToolMatch> ParseMatches(string json)
    {
        var matches = new List<LanguageToolMatch>();
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("matches", out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return matches;
        }

        foreach (var element in array.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object ||
                !element.TryGetProperty("offset", out var offsetElement) ||
                offsetElement.ValueKind != JsonValueKind.Number ||
                !offsetElement.TryGetInt32(out var offset) ||
                !element.TryGetProperty("length", out var lengthElement) ||
                lengthElement.ValueKind != JsonValueKind.Number ||
                !lengthElement.TryGetInt32(out var length))
            {
                continue;
            }

            var replacements = new List<string>();
            if (element.TryGetProperty("replacements", out var replacementArray) && replacementArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var replacement in replacementArray.EnumerateArray())
                {
                    if (replacement.ValueKind == JsonValueKind.Object &&
                        replacement.TryGetProperty("value", out var value) &&
                        value.ValueKind == JsonValueKind.String)
                    {
                        var text = value.GetString();
                        if (!string.IsNullOrEmpty(text) && !replacements.Contains(text))
                        {
                            replacements.Add(text!);
                        }
                    }

                    if (replacements.Count >= MaxReplacements)
                    {
                        break;
                    }
                }
            }

            var ruleId = string.Empty;
            var ruleDescription = string.Empty;
            var issueType = string.Empty;
            var categoryId = string.Empty;
            var categoryName = string.Empty;
            if (element.TryGetProperty("rule", out var rule) && rule.ValueKind == JsonValueKind.Object)
            {
                ruleId = GetString(rule, "id");
                ruleDescription = GetString(rule, "description");
                issueType = GetString(rule, "issueType");
                if (rule.TryGetProperty("category", out var category) && category.ValueKind == JsonValueKind.Object)
                {
                    categoryId = GetString(category, "id");
                    categoryName = GetString(category, "name");
                }
            }

            matches.Add(new LanguageToolMatch
            {
                Offset = offset,
                Length = length,
                Message = GetString(element, "message"),
                ShortMessage = GetString(element, "shortMessage"),
                RuleId = ruleId,
                RuleDescription = ruleDescription,
                IssueType = issueType,
                CategoryId = categoryId,
                CategoryName = categoryName,
                Replacements = replacements,
            });
        }

        return matches;
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    /// <summary>LanguageTool answers errors as plain text ("Error: ..."), sometimes with a stack trace.</summary>
    private static string ShortError(string body, string fallback)
    {
        var s = (body ?? string.Empty).Trim();
        if (s.Length == 0)
        {
            return fallback;
        }

        var lineBreak = s.IndexOfAny(new[] { '\r', '\n' });
        if (lineBreak > 0)
        {
            s = s.Substring(0, lineBreak);
        }

        return s.Length > 300 ? s.Substring(0, 300) + "..." : s;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
