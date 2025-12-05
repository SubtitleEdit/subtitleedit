using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    /// <summary>
    /// See https://fanyi.baidu.com/home or https://pyvideotrans.com/baidu
    /// Baidu Translation API documentation: https://fanyi-api.baidu.com/doc/21
    /// </summary>
    public class BaiduTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;
        private string _appId;
        private string _secretKey;

        public static string StaticName { get; set; } = "Baidu Translate";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://fanyi.baidu.com";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();

            var apiUrl = Configuration.Settings.Tools.BaiduUrl;
            if (string.IsNullOrEmpty(apiUrl))
            {
                apiUrl = "https://fanyi-api.baidu.com";
            }

            _httpClient.BaseAddress = new Uri(apiUrl.TrimEnd('/'));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            // BaiduApiKey should contain "appid|secretKey" format
            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.BaiduApiKey))
            {
                var parts = Configuration.Settings.Tools.BaiduApiKey.Split('|');
                if (parts.Length == 2)
                {
                    _appId = parts[0].Trim();
                    _secretKey = parts[1].Trim();
                }
                else
                {
                    _appId = Configuration.Settings.Tools.BaiduApiKey;
                    _secretKey = string.Empty;
                }
            }
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return ListLanguages();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return ListLanguages();
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_appId) || string.IsNullOrEmpty(_secretKey))
            {
                Error = "Baidu API credentials not configured. Please set App ID and Secret Key.";
                throw new Exception(Error);
            }

            // Generate random salt and sign
            var salt = new Random().Next(100000, 999999).ToString();
            var sign = CalculateMd5Hash(_appId + text + salt + _secretKey);

            // Build request URL with parameters
            var fromLang = ConvertLanguageCode(sourceLanguageCode);
            var toLang = ConvertLanguageCode(targetLanguageCode);

            var url = $"/api/trans/vip/translate?q={Uri.EscapeDataString(text)}&from={fromLang}&to={toLang}&appid={_appId}&salt={salt}&sign={sign}";

            var result = await _httpClient.GetAsync(url, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();

            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Baidu Translate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
                result.EnsureSuccessStatusCode();
            }

            var parser = new SeJsonParser();

            // Check for error response
            var errorCode = parser.GetFirstObject(json, "error_code");
            if (!string.IsNullOrEmpty(errorCode))
            {
                var errorMsg = parser.GetFirstObject(json, "error_msg");
                Error = $"Baidu API Error {errorCode}: {errorMsg}";
                SeLogger.Error("Baidu Translate error: " + Error);
                throw new Exception(Error);
            }

            // Parse translation result
            var transResult = parser.GetArrayElementsByName(json, "trans_result");
            if (transResult.Count == 0)
            {
                return string.Empty;
            }

            var firstResult = transResult[0];
            var dst = parser.GetFirstObject(firstResult, "dst");

            if (string.IsNullOrEmpty(dst))
            {
                return string.Empty;
            }

            var outputText = Json.DecodeJsonText(dst).Trim();
            return outputText;
        }

        private static string CalculateMd5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private static string ConvertLanguageCode(string code)
        {
            // Baidu uses specific language codes
            switch (code.ToLowerInvariant())
            {
                case "zh":
                case "zh-cn":
                    return "zh";
                case "zh-tw":
                case "cht":
                    return "cht";
                case "en":
                    return "en";
                case "ja":
                    return "jp";
                case "ko":
                    return "kor";
                case "es":
                    return "spa";
                case "fr":
                    return "fra";
                case "de":
                    return "de";
                case "tr":
                    return "tr";
                case "pt":
                    return "pt";
                case "ru":
                    return "ru";
                case "it":
                    return "it";
                case "ar":
                    return "ara";
                case "th":
                    return "th";
                case "vi":
                    return "vie";
                case "id":
                    return "id";
                default:
                    return code;
            }
        }

        public static List<TranslationPair> ListLanguages()
        {
            // Based on Baidu Translate API supported languages
            return new List<TranslationPair>
            {
                MakePair("Arabic", "ar"),
                MakePair("Chinese (Simplified)", "zh"),
                MakePair("Chinese (Traditional)", "zh-TW"),
                MakePair("Czech", "cs"),
                MakePair("Danish", "da"),
                MakePair("Dutch", "nl"),
                MakePair("English", "en"),
                MakePair("Estonian", "et"),
                MakePair("Finnish", "fi"),
                MakePair("French", "fr"),
                MakePair("German", "de"),
                MakePair("Greek", "el"),
                MakePair("Hungarian", "hu"),
                MakePair("Indonesian", "id"),
                MakePair("Italian", "it"),
                MakePair("Japanese", "ja"),
                MakePair("Korean", "ko"),
                MakePair("Polish", "pl"),
                MakePair("Portuguese", "pt"),
                MakePair("Romanian", "ro"),
                MakePair("Russian", "ru"),
                MakePair("Slovenian", "sl"),
                MakePair("Spanish", "es"),
                MakePair("Swedish", "sv"),
                MakePair("Thai", "th"),
                MakePair("Turkish", "tr"),
                MakePair("Vietnamese", "vi"),
            };
        }

        private static TranslationPair MakePair(string name, string code)
        {
            return new TranslationPair(name, code, code);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}