using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Http;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate
    /// </summary>
    public class MicrosoftTranslator : IAutoTranslator, IDisposable
    {
        public const string SignUpUrl = "https://learn.microsoft.com/en-us/azure/ai-services/translator/create-translator-resource";
        public const string GoToUrl = "https://www.bing.com/translator";
        private const string LanguagesUrl = "https://api.cognitive.microsofttranslator.com/languages?api-version=3.0&scope=translation";
        private const string TranslateUrl = "translate?api-version=3.0&from={0}&to={1}";
        private const string RegionalTokenEndpoint = "https://{0}.api.cognitive.microsoft.com/sts/v1.0/issueToken";
        private const string SecurityHeaderName = "Ocp-Apim-Subscription-Key";
        // Azure issues tokens valid for 10 minutes; refresh with a margin so
        // long translation runs do not start failing with 401 mid-run.
        private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(8);
        private static List<TranslationPair>? _translationPairs;
        private string _accessToken = string.Empty;
        private DateTime _accessTokenFetchedUtc;
        private string _apiKey = string.Empty;
        private string _tokenEndpoint = string.Empty;
        private string _category = string.Empty;
        private IDownloader _httpClient = null!;

        public static string StaticName { get; set; } = "Bing Microsoft Translator";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://www.bing.com/translator";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _apiKey = Configuration.Settings.Tools.MicrosoftTranslatorApiKey;
            _tokenEndpoint = GetTokenEndpoint(Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint, Configuration.Settings.Tools.MicrosoftTranslatorRegion);
            _category = Configuration.Settings.Tools.MicrosoftTranslatorCategory;

            try
            {
                _accessToken = GetAccessToken(_apiKey, _tokenEndpoint);
                _accessTokenFetchedUtc = DateTime.UtcNow;
            }
            catch (Exception e)
            {
                throw new Exception("Can't get Access Token: " + e.Message, e);
            }
        }

        /// <summary>
        /// Regional and multi-service Azure resources must get their token from a URL with the
        /// region in the host name. The Azure portal shows "global" for global resources, which
        /// use the configured endpoint like an empty region does.
        /// </summary>
        internal static string GetTokenEndpoint(string tokenEndpoint, string? region)
        {
            var normalizedRegion = (region ?? string.Empty).Replace(" ", string.Empty).ToLowerInvariant();
            if (normalizedRegion.Length == 0 || normalizedRegion == "global")
            {
                return tokenEndpoint;
            }

            return string.Format(RegionalTokenEndpoint, normalizedRegion);
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return GetTranslationPairs();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return GetTranslationPairs();
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var url = string.Format(TranslateUrl, sourceLanguageCode, targetLanguageCode);
            if (!string.IsNullOrEmpty(_category))
            {
                url += "&category=" + _category.Trim();
            }

            // One array element per line, like SE 3.5 did: the merged request text has one row
            // (or sentence) per line, and sent as a single element Bing sometimes handed back a
            // whole block untranslated - the source language again, or the English pivot - while
            // the same rows translated fine as separate elements. The reply also keeps the line
            // count exactly, so the merge/split helper reads the rows straight back.
            var lines = text.SplitToLines();
            var json = BuildRequestJson(lines);
            var httpClient = GetTranslateClient();
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await httpClient.PostAsync(url, content, cancellationToken);
            var jsonResult = await result.Content.ReadAsStringAsync(cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                Error = jsonResult;

                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception("API key is not valid!" + Environment.NewLine + Environment.NewLine + jsonResult);
                }

                throw new Exception("An error occurred during translate:" + Environment.NewLine + Environment.NewLine + jsonResult);
            }

            return string.Join(Environment.NewLine, MapResponseToLines(lines, ParseResponse(jsonResult)));
        }

        /// <summary>
        /// The request body: one element per non-blank line. Blank lines are not sent (Bing
        /// rejects an empty Text) and are put back by <see cref="MapResponseToLines"/>.
        /// </summary>
        internal static string BuildRequestJson(List<string> lines)
        {
            var jsonBuilder = new StringBuilder("[");
            var first = true;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (!first)
                {
                    jsonBuilder.Append(',');
                }

                first = false;
                jsonBuilder.Append("{\"Text\":\"" + Json.EncodeJsonText(line) + "\"}");
            }

            jsonBuilder.Append(']');
            return jsonBuilder.ToString();
        }

        /// <summary>
        /// The translation of each element of the reply, in request order.
        /// </summary>
        internal static List<string> ParseResponse(string jsonResult)
        {
            var results = new List<string>();
            var parser = new JsonParser();
            var x = (List<object>)parser.Parse(jsonResult);
            foreach (var xElement in x)
            {
                var dict = (Dictionary<string, object>)xElement;
                var y = (List<object>)dict["translations"];
                var parts = new List<string>();
                foreach (var o in y)
                {
                    var textDictionary = (Dictionary<string, object>)o;
                    var res = (string)textDictionary["text"];
                    res = res.Replace("<br />", Environment.NewLine);
                    res = res.Replace("<br/>", Environment.NewLine);
                    res = res.Replace("<br>", Environment.NewLine);
                    parts.Add(res);
                }

                results.Add(string.Join(Environment.NewLine, parts));
            }

            return results;
        }

        /// <summary>
        /// Puts the translations back at the lines they came from, keeping blank lines blank. A
        /// line without a translation (a short reply) is left empty rather than given its source
        /// text: the blank-line check of the merge/split helper then rejects the reply and the
        /// rows are retried, instead of an untranslated line being written out as translated.
        /// </summary>
        internal static List<string> MapResponseToLines(List<string> lines, List<string> translations)
        {
            var output = new List<string>(lines.Count);
            var translationIndex = 0;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    output.Add(string.Empty);
                }
                else if (translationIndex < translations.Count)
                {
                    output.Add(translations[translationIndex++].Trim());
                }
                else
                {
                    output.Add(string.Empty);
                }
            }

            return output;
        }

        private IDownloader GetTranslateClient()
        {
            if (_httpClient != null && DateTime.UtcNow - _accessTokenFetchedUtc > AccessTokenLifetime)
            {
                _accessToken = GetAccessToken(_apiKey, _tokenEndpoint);
                _accessTokenFetchedUtc = DateTime.UtcNow;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }

            if (_httpClient == null)
            {
                _httpClient = DownloaderFactory.MakeHttpClient();
                _httpClient.BaseAddress = new Uri("https://api.cognitive.microsofttranslator.com/");
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }

            return _httpClient;
        }

        private static string GetAccessToken(string apiKey, string tokenEndpoint)
        {
            return Task.Run(async () =>
            {
                using (var httpClient = DownloaderFactory.MakeHttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(SecurityHeaderName, apiKey);
                    var response = await httpClient.PostAsync(tokenEndpoint, new StringContent(string.Empty)).ConfigureAwait(false);
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        SeLogger.Error($"{StaticName}: Error getting access token via {tokenEndpoint}: status code={response.StatusCode} {result}");
                        var hint = response.StatusCode == HttpStatusCode.Unauthorized
                            ? " Check the API key and, for a regional or multi-service Azure resource, the region."
                            : string.Empty;
                        throw new Exception($"Could not get access token via {tokenEndpoint}: {result}{hint}");
                    }

                    return result;
                }
            }).GetAwaiter().GetResult();
        }

        private static List<TranslationPair> GetTranslationPairs()
        {
            if (_translationPairs != null)
            {
                return _translationPairs;
            }

            // Unlike the other engines this list is not hardcoded - it is fetched from the service,
            // so it is current by construction. The flip side is that picking the engine with no
            // network (or behind a blocking proxy) used to throw straight out of this synchronous
            // call and into the language combo box; answer with an empty list instead, and do not
            // cache it, so the next attempt tries again.
            try
            {
                return Task.Run(async () =>
                {
                    using (var httpClient = DownloaderFactory.MakeHttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=UTF-8");
                        var json = await httpClient.GetStringAsync(LanguagesUrl).ConfigureAwait(false);
                        _translationPairs = FillTranslationPairsFromJson(json);
                        return _translationPairs;
                    }
                }).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                SeLogger.Error(exception, $"{StaticName}: could not fetch the language list from {LanguagesUrl}");
                return new List<TranslationPair>();
            }
        }

        private static List<TranslationPair> FillTranslationPairsFromJson(string json)
        {
            var list = new List<TranslationPair>();
            var parser = new JsonParser();
            var x = (Dictionary<string, object>)parser.Parse(json);
            foreach (var k in x.Keys)
            {
                if (x[k] is Dictionary<string, object> v)
                {
                    foreach (var innerKey in v.Keys)
                    {
                        if (v[innerKey] is Dictionary<string, object> l)
                        {
                            list.Add(new TranslationPair(l["name"]?.ToString() ?? string.Empty, innerKey, innerKey));
                        }
                    }
                }
            }
            return list;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
