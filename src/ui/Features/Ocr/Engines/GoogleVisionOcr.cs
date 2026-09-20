using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Http;
using Nikse.SubtitleEdit.UiLogic.Ocr.Service;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// OCR via Google Cloud Vision API - see https://cloud.google.com/vision/docs/ocr
/// </summary>
public class GoogleVisionOcr
{
    private readonly HttpClient _httpClient;

    public GoogleVisionOcr()
    {
        _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
        _httpClient.BaseAddress = new Uri("https://vision.googleapis.com/v1/images:annotate");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public string GetName()
    {
        return "Google Cloud Vision API";
    }

    public int GetMaxImageSize()
    {
        return 20_000_000;
    }

    public int GetMaximumRequestArraySize()
    {
        return 16;
    }

    public static List<OcrLanguage> GetLanguages()
    {
        // Currently (Sep 1, 2022) supported and experimental languages from https://cloud.google.com/vision/docs/languages
        var list = new List<OcrLanguage>();
        var codes = new List<string>
        {
            "af",
            "am",
            "ar",
            "as",
            "az",
            "az-Cyrl",
            "be",
            "bg",
            "bn",
            "bo",
            "bs",
            "ca",
            "ceb",
            "chr",
            "cs",
            "cy",
            "da",
            "de",
            "dv",
            "dz",
            "el",
            "en",
            "eo",
            "es",
            "et",
            "eu",
            "fa",
            "fi",
            "fil",
            "fr",
            "ga",
            "gl",
            "grc",
            "gu",
            "hi",
            "hr",
            "ht",
            "hu",
            "hy",
            "id",
            "is",
            "it",
            "iw",
            "ja",
            "jv",
            "ka",
            "kk",
            "km",
            "kn",
            "ko",
            "ky",
            "la",
            "lo",
            "lt",
            "lv",
            "mk",
            "ml",
            "mn",
            "mr",
            "ms",
            "mt",
            "my",
            "ne",
            "nl",
            "no",
            "or",
            "pa",
            "pl",
            "ps",
            "pt",
            "ro",
            "ru",
            "ru-PETR1708",
            "sa",
            "si",
            "sk",
            "sl",
            "sq",
            "sr",
            "sr-Latn",
            "sv",
            "sw",
            "syr",
            "ta",
            "te",
            "th",
            "ti",
            "tl",
            "tr",
            "uk",
            "ur",
            "uz",
            "uz-Cyrl",
            "vi",
            "yi",
            "zh",
            "zu",
        };

        foreach (var code in codes)
        {
            list.Add(new OcrLanguage { Code = code });
        }

        return list;
    }

    public string GetUrl()
    {
        return "https://cloud.google.com/vision/docs/ocr";
    }

    public async Task<string> Ocr(SKBitmap bitmap, string apiKey, string language, CancellationToken cancellationToken)
    {
        var requestBody = new RequestBody();
        var request = new RequestBody.Request(bitmap.ToBase64String(), language);
        requestBody.requests.Add(request);

        // Convert to JSON string
        string requestBodyString;
        using (var memoryStream = new MemoryStream())
        {
            new DataContractJsonSerializer(typeof(RequestBody)).WriteObject(memoryStream, requestBody);
            requestBodyString = Encoding.Default.GetString(memoryStream.ToArray());
        }

        // Do request
        var uri = $"?key={apiKey}";
        string content;
        try
        {
            var result = await _httpClient.PostAsync(uri, new StringContent(requestBodyString), cancellationToken);
            if ((int)result.StatusCode == 400)
            {
                throw new OcrException("API key invalid (or perhaps billing/API is not enabled)?");
            }

            if ((int)result.StatusCode == 403)
            {
                throw new OcrException("\"Perhaps billing is not enabled (or API not enabled or API key is invalid)?\"");
            }

            if (!result.IsSuccessStatusCode)
            {
                throw new OcrException($"An error occurred calling Cloud Vision API - status code: {result.StatusCode}");
            }

            content = await result.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException httpException)
        {
            throw new OcrException("Error calling Cloud Vision API: " + httpException.Message, httpException);
        }

        return string.Join(Environment.NewLine, JsonToStringList(language, content));
    }

    /// <summary>
    /// Rebuilds the subtitle text from the Vision response - one implementation, kept in
    /// libuilogic so it can be unit tested.
    /// </summary>
    public static List<string> JsonToStringList(string language, string content)
    {
        return GoogleCloudVisionApi.JsonToStringList(language, content);
    }

    [DataContract, Serializable]
    public class RequestBody
    {
        [DataMember]
        public List<Request> requests { get; set; }

        public RequestBody()
        {
            requests = new List<Request>();
        }

        [DataContract, Serializable]
        public class Request
        {
            [DataMember] public Image image { get; set; }

            [DataMember] public ImageContext imageContext { get; set; }

            [DataMember] public List<Feature> features { get; set; }

            public Request(string imageContent, string language)
            {
                image = new Image(imageContent);
                imageContext = new ImageContext(new List<string>() { language, "en" }); // English as fallback
                features = new List<Feature> { new("TEXT_DETECTION", 1) };
            }

            [DataContract, Serializable]
            public class Image
            {
                [DataMember] public string content { get; set; }

                public Image(string content)
                {
                    this.content = content;
                }
            }

            [DataContract, Serializable]
            public class ImageContext
            {
                [DataMember] public List<string> languageHints { get; set; }

                public ImageContext(List<string> languageHints)
                {
                    this.languageHints = languageHints;
                }
            }

            [DataContract, Serializable]
            public class Feature
            {
                [DataMember] public string type { get; set; }
                [DataMember] public int maxResults { get; set; }

                public Feature(string type, int maxResults)
                {
                    this.type = type;
                    this.maxResults = maxResults;
                }
            }
        }
    }
}
