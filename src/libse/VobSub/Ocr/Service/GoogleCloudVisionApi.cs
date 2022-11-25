using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

            return JsonToStringList(language, content);
        }

        internal static List<string> JsonToStringList(string language, string content)
        {
            if (Configuration.Settings.Tools.OcrGoogleCloudVisionSeHandlesTextMerge)
            {
                var annotations = GetAnnotations(content).ToList();
                var lines = new List<List<Annotation>>();
                Annotation last = null;
                var lineThreshold = Math.Max(9, annotations.Average(p => p.Height) / 4.0);
                var lineIndex = 0;

                // Split to lines
                foreach (var a in annotations.OrderBy(p => p.GetMediumY()))
                {
                    if (last != null)
                    {
                        var diff = Math.Abs(last.GetMediumY() - a.GetMediumY());
                        if (diff > lineThreshold && last.Vertices.Max(p => p.Y) <= a.Vertices.Min(p => p.Y))
                        {
                            lineIndex++;
                            lines.Add(new List<Annotation>());
                            lines[lineIndex].Add(a);
                        }
                        else
                        {
                            lines[lineIndex].Add(a);
                        }
                    }
                    else
                    {
                        lines.Add(new List<Annotation>());
                        lines[lineIndex].Add(a);
                    }

                    last = a;
                }

                // Merge lines ordered by X
                var sb = new StringBuilder();
                var spaceThreshold = Math.Max(12, annotations.Average(p => p.Width) / 2.7);
                foreach (var line in lines)
                {
                    var sbLine = new StringBuilder();
                    last = null;
                    foreach (var l in line.OrderBy(p => p.Vertices.Min(p2 => p2.X)))
                    {
                        if (last != null)
                        {
                            var diff = l.Vertices.Min(p => p.X) - last.Vertices.Max(p => p.X);
                            if (diff > spaceThreshold || last.DetectedBreak == "SPACE" || last.DetectedBreak == "EOL_SURE_SPACE" || last.DetectedBreak == "LINE_BREAK")
                            {
                                sbLine.Append(" ");
                            }

                            sbLine.Append(l.Text);
                        }
                        else
                        {
                            sbLine.Append(l.Text);
                        }

                        last = l;
                    }

                    if (language == "fr")
                    {
                        sb.AppendLine(sbLine.ToString().Trim());
                    }
                    else
                    {
                        sb.AppendLine(sbLine.ToString().Trim()
                            .Replace(" .", ".")
                            .Replace(" ,", ",")
                            .Replace(" ?", "?")
                            .Replace(" !", "!"));
                    }
                }

                var ocrResult = sb.ToString().Trim();
                if (ocrResult.Length > 0)
                {
                    return new List<string> { ocrResult };
                }
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
                                        // The first annotation contains the whole text
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

        private static IEnumerable<Annotation> GetAnnotations(string content)
        {
            var jsonParser = new SeJsonParser();
            var fullTextAnnotation = jsonParser.GetFirstObject(content, "fullTextAnnotation");
            var pages = jsonParser.GetArrayElementsByName(fullTextAnnotation, "pages");
            var annotations = new List<Annotation>();
            foreach (var page in pages)
            {
                var paragraphs = jsonParser.GetArrayElementsByName(page, "paragraphs");
                foreach (var paragraph in paragraphs)
                {
                    var words = jsonParser.GetArrayElementsByName(paragraph, "words");
                    foreach (var word in words)
                    {
                        var symbols = jsonParser.GetArrayElementsByName(word, "symbols");
                        foreach (var symbol in symbols)
                        {
                            var text = jsonParser.GetFirstObject(symbol, "text");
                            var detectedBreak = jsonParser.GetFirstObject(symbol, "detectedBreak");
                            if (!string.IsNullOrEmpty(detectedBreak))
                            {
                                detectedBreak = jsonParser.GetFirstObject(detectedBreak, "type");
                            }

                            var vertices = new List<Point>();
                            foreach (var point in jsonParser.GetArrayElementsByName(symbol, "vertices"))
                            {
                                var x = jsonParser.GetFirstObject(point, "x");
                                var y = jsonParser.GetFirstObject(point, "y");
                                if (int.TryParse(x, out var xNumber) && int.TryParse(y, out var yNumber))
                                {
                                    vertices.Add(new Point(xNumber, yNumber));
                                }
                            }

                            if (!string.IsNullOrEmpty(text) && vertices.Count > 0)
                            {
                                annotations.Add(new Annotation
                                {
                                    Text = text,
                                    Vertices = vertices,
                                    DetectedBreak = detectedBreak,
                                });
                            }
                        }
                    }
                }
            }

            return annotations;
        }

        public class Annotation
        {
            public string Text { get; set; }
            public List<Point> Vertices { get; set; }
            public string DetectedBreak { get; set; }

            public int Width
            {
                get
                {
                    var min = Vertices.Min(p => p.X);
                    var max = Vertices.Max(p => p.X);
                    return max - min;
                }
            }

            public int Height
            {
                get
                {
                    var min = Vertices.Min(p => p.Y);
                    var max = Vertices.Max(p => p.Y);
                    return max - min;
                }
            }

            public double GetMediumY()
            {
                const int addY = 5;
                var lowChars = new[] { ",", "." };
                var highChars = new[] { "'", "\"" };
                var min = Vertices.Min(p => p.Y);
                var max = Vertices.Max(p => p.Y);
                var medium = max + min / 2.0;

                if (lowChars.Contains(Text))
                {
                    return medium - addY;
                }

                if (highChars.Contains(Text))
                {
                    return medium + addY;
                }

                return medium;
            }
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
                    image = new Image(imageContent);
                    imageContext = new ImageContext(new List<string>() { language, "en" }); // English as fallback
                    features = new List<Feature> { new Feature("TEXT_DETECTION", 1) };
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
