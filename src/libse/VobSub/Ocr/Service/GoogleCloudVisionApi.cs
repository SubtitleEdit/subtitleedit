using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Nikse.SubtitleEdit.Core.VobSub.Ocr.Service
{
    /// <summary>
    /// OCR via Google Cloud Vision API - see https://cloud.google.com/vision/docs/ocr
    /// </summary>
    public class GoogleCloudVisionApi : IOcrStrategy
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

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

        public List<OcrLanguage> GetLanguages()
        {
            var list = new List<OcrLanguage>();
            var codes = new List<string>
                {
                    "ar",
                    "be",
                    "bg",
                    "bn",
                    "ca",
                    "cs",
                    "da",
                    "de",
                    "el",
                    "en",
                    "es",
                    "et",
                    "fa",
                    "fi",
                    "fil",
                    "fr",
                    "gu",
                    "hi",
                    "hr",
                    "hu",
                    "hy",
                    "id",
                    "is",
                    "it",
                    "iw",
                    "ja",
                    "km",
                    "kn",
                    "ko",
                    "lo",
                    "lt",
                    "lv",
                    "mk",
                    "ml",
                    "mr",
                    "ms",
                    "ne",
                    "nl",
                    "no",
                    "pa",
                    "pl",
                    "pt",
                    "ro",
                    "ru",
                    "ru-PETR1708",
                    "sk",
                    "sl",
                    "sq",
                    "sr",
                    "sr-Latn",
                    "sv",
                    "ta",
                    "te",
                    "th",
                    "tl",
                    "tr",
                    "uk",
                    "vi",
                    "yi",
                    "zh",
                };

            foreach (var code in codes)
            {
                list.Add(new OcrLanguage { Code = code });
            }

            return list;
        }

        public GoogleCloudVisionApi(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = HttpClientHelper.MakeHttpClient();
            _httpClient.BaseAddress = new Uri("https://vision.googleapis.com/v1/images:annotate");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public string GetUrl()
        {
            return "https://cloud.google.com/vision/docs/ocr";
        }

        public List<string> PerformOcr(string language, List<Bitmap> images)
        {
            var requestBody = new RequestBody();

            foreach (var image in images)
            {
                string imageBase64;
                using (var memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    imageBase64 = Convert.ToBase64String(memoryStream.ToArray());
                }

                var request = new RequestBody.Request(imageBase64, language);
                requestBody.requests.Add(request);
            }

            // Convert to JSON string
            string requestBodyString;
            using (var memoryStream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(RequestBody)).WriteObject(memoryStream, requestBody);
                requestBodyString = Encoding.Default.GetString(memoryStream.ToArray());
            }

            // Do request
            var uri = $"?key={_apiKey}";
            string content;
            try
            {
                var result = _httpClient.PostAsync(uri, new StringContent(requestBodyString)).Result;
                if ((int)result.StatusCode == 400)
                {
                    throw new OcrException("API key invalid (or perhaps billing is not enabled)?");
                }

                if ((int)result.StatusCode == 403)
                {
                    throw new OcrException("\"Perhaps billing is not enabled (or API key is invalid)?\"");
                }

                if (!result.IsSuccessStatusCode)
                {
                    throw new OcrException($"An error occurred calling Cloud Vision API - status code: {result.StatusCode}");
                }

                content = result.Content.ReadAsStringAsync().Result;
            }
            catch (WebException webException)
            {
                var message = string.Empty;
                if (webException.Message.Contains("(400) Bad Request"))
                {
                    message = "API key invalid (or perhaps billing is not enabled)?";
                }
                else if (webException.Message.Contains("(403) Forbidden."))
                {
                    message = "Perhaps billing is not enabled (or API key is invalid)?";
                }
                throw new OcrException(message, webException);
            }

            var resultList = new List<string>();
            var parser = new JsonParser();
            var jsonObject = (Dictionary<string, object>)parser.Parse(content);

            if (jsonObject.ContainsKey("responses"))
            {
                if (jsonObject["responses"] is List<object> responses)
                {
                    foreach (var responseObject in responses)
                    {
                        var result = string.Empty;

                        if (responseObject is Dictionary<string, object> response)
                        {
                            if (response.ContainsKey("textAnnotations"))
                            {
                                if (response["textAnnotations"] is List<object> textAnnotations)
                                {
                                    if (textAnnotations.Count > 0)
                                    {
                                        if (textAnnotations[0] is Dictionary<string, object> firstTextAnnotation)
                                        {
                                            if (firstTextAnnotation.ContainsKey("description"))
                                            {
                                                if (firstTextAnnotation["description"] is string description)
                                                {
                                                    result = OcrHelper.PostOcr(description, language);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        resultList.Add(result);
                    }
                }
            }

            return resultList;
        }

        [DataContract, Serializable]
        public class RequestBody
        {
            [DataMember]
            public List<Request> requests { get; set; }

            public RequestBody()
            {
                this.requests = new List<Request>();
            }


            [DataContract, Serializable]
            public class Request
            {
                [DataMember]
                public Image image { get; set; }
                [DataMember]
                public ImageContext imageContext { get; set; }
                [DataMember]
                public List<Feature> features { get; set; }

                public Request(string imageContent, string language)
                {
                    this.image = new Image(imageContent);
                    this.imageContext = new ImageContext(new List<string>() { language, "en" });
                    this.features = new List<Feature>() { new Feature("TEXT_DETECTION", 1) };
                }


                [DataContract, Serializable]
                public class Image
                {
                    [DataMember]
                    public string content { get; set; }

                    public Image(string content)
                    {
                        this.content = content;
                    }
                }

                [DataContract, Serializable]
                public class ImageContext
                {
                    [DataMember]
                    public List<string> languageHints { get; set; }

                    public ImageContext(List<string> languageHints)
                    {
                        this.languageHints = languageHints;
                    }
                }

                [DataContract, Serializable]
                public class Feature
                {
                    [DataMember]
                    public string type { get; set; }
                    [DataMember]
                    public int maxResults { get; set; }

                    public Feature(string type, int maxResults)
                    {
                        this.type = type;
                        this.maxResults = maxResults;
                    }
                }
            }
        }
    }
}
