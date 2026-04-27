using Nikse.SubtitleEdit.Logic.Ocr.GoogleLens.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

public class LensCore
{
    protected Dictionary<string, object> _config = new();
    protected Dictionary<string, Cookie> _cookies = new();
    protected Func<HttpRequestMessage, Task<HttpResponseMessage>>? _fetch;
    private static readonly HttpClient _sharedHttpClient;

    // Expose cookies for persistence
    public Dictionary<string, Cookie> Cookies => _cookies;

    static LensCore()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.All
        };

        _sharedHttpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public LensCore(Dictionary<string, object>? config = null, Func<HttpRequestMessage, Task<HttpResponseMessage>>? fetch = null)
    {
        if (config != null && config.GetType() != typeof(Dictionary<string, object>))
        {
            throw new ArgumentException("Lens constructor expects a dictionary");
        }

        _fetch = fetch ?? DefaultFetchAsync;

        var chromeVersion = config?.GetValueOrDefault("chromeVersion") as string ?? "124.0.6367.60";
        var majorChromeVersion = config?.GetValueOrDefault("majorChromeVersion") as string ?? chromeVersion.Split('.')[0];

        _config = new Dictionary<string, object>
        {
            { "chromeVersion", chromeVersion },
            { "majorChromeVersion", majorChromeVersion },
            { "userAgent", config?.GetValueOrDefault("userAgent") as string ?? $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{majorChromeVersion}.0.0.0 Safari/537.36" },
            { "endpoint", Constants.LENS_PROTO_ENDPOINT },
            { "viewport", config?.GetValueOrDefault("viewport") ?? new[] { 1920, 1080 } },
            { "headers", new Dictionary<string, string>() },
            { "fetchOptions", new Dictionary<string, object>() },
            { "targetLanguage", config?.GetValueOrDefault("targetLanguage") as string ?? "en" },
            { "region", config?.GetValueOrDefault("region") as string ?? "US" },
            { "timeZone", config?.GetValueOrDefault("timeZone") as string ?? "America/New_York" }
        };

        if (config != null)
        {
            foreach (var kvp in config)
            {
                _config[kvp.Key] = kvp.Value;
            }
        }

        // Lowercase all headers
        var headers = _config["headers"] as Dictionary<string, string>;
        if (headers != null)
        {
            var lowercaseHeaders = new Dictionary<string, string>();
            foreach (var kvp in headers)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    lowercaseHeaders[kvp.Key.ToLower()] = kvp.Value;
                }
            }
            _config["headers"] = lowercaseHeaders;
        }

        ParseCookies();
    }

    public void UpdateOptions(Dictionary<string, object> options)
    {
        foreach (var kvp in options)
        {
            _config[kvp.Key] = kvp.Value;
        }
        ParseCookies();
    }

    protected byte[] CreateLensProtoRequest(byte[] imageBytes, int width, int height)
    {
        var targetLanguage = _config.GetValueOrDefault("targetLanguage") as string ?? "en";
        var region = _config.GetValueOrDefault("region") as string ?? "US";
        var timeZone = _config.GetValueOrDefault("timeZone") as string ?? "America/New_York";

        var request = new LensProtoRequest
        {
            ImageBytes = imageBytes,
            Width = width,
            Height = height,
            RequestId = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{new Random().Next(0, 1000000)}",
            TargetLanguage = targetLanguage,
            Region = region,
            TimeZone = timeZone
        };

        return request.Serialize();
    }

    protected LensResult ParseLensProtoResponse(LensProtoResponse serverResponse, int[] originalImageDimensions)
    {
        if (serverResponse.HasError)
        {
            Console.WriteLine($"Lens server returned error: Type={serverResponse.ErrorType}");
            if (serverResponse.ErrorType != 0) // UNKNOWN_TYPE = 0
            {
                return new LensResult(string.Empty, new List<Segment>());
            }
        }

        if (!serverResponse.HasObjectsResponse || serverResponse.TextData?.TextLayout == null)
        {
            return new LensResult(string.Empty, new List<Segment>());
        }

        var textData = serverResponse.TextData;
        var textLayout = textData.TextLayout;
        var detectedLanguage = textData.ContentLanguage;

        if (string.IsNullOrEmpty(detectedLanguage) && textLayout.Paragraphs.Count > 0)
        {
            detectedLanguage = textLayout.Paragraphs[0].ContentLanguage ?? string.Empty;
        }

        var segments = new List<Segment>();

        foreach (var paragraph in textLayout.Paragraphs)
        {
            foreach (var line in paragraph.Lines)
            {
                var lineTextContent = string.Empty;
                var words = line.Words;

                for (int i = 0; i < words.Count; i++)
                {
                    var word = words[i];
                    lineTextContent += word.PlainText;

                    if (word.HasTextSeparator)
                    {
                        lineTextContent += word.TextSeparator;
                    }
                    else if (i < words.Count - 1)
                    {
                        lineTextContent += " ";
                    }
                }

                lineTextContent = Regex.Replace(lineTextContent, @"\s+", " ").Trim();

                if (!string.IsNullOrEmpty(lineTextContent))
                {
                    BoundingBox? boundingBox = null;

                    // Try to get bounding box from line geometry
                    if (line.Geometry?.BoundingBox != null)
                    {
                        var protoBox = line.Geometry.BoundingBox;
                        if (protoBox.CoordinateType == 1) // NORMALIZED
                        {
                            var boxData = new double[]
                            {
                                protoBox.CenterX,
                                protoBox.CenterY,
                                protoBox.Width,
                                protoBox.Height
                            };
                            boundingBox = new BoundingBox(boxData, originalImageDimensions);
                        }
                    }

                    // Fallback to paragraph geometry
                    if (boundingBox == null && paragraph.Geometry?.BoundingBox != null)
                    {
                        var protoBox = paragraph.Geometry.BoundingBox;
                        if (protoBox.CoordinateType == 1) // NORMALIZED
                        {
                            var boxData = new double[]
                            {
                                protoBox.CenterX,
                                protoBox.CenterY,
                                protoBox.Width,
                                protoBox.Height
                            };
                            boundingBox = new BoundingBox(boxData, originalImageDimensions);
                        }
                    }

                    // Default bounding box if none found
                    if (boundingBox == null)
                    {
                        boundingBox = new BoundingBox(new double[] { 0.5, 0.5, 1, 1 }, originalImageDimensions);
                    }

                    segments.Add(new Segment(lineTextContent, boundingBox));
                }
            }
        }

        return new LensResult(detectedLanguage, segments);
    }

    protected async Task<LensProtoResponse> SendProtoRequest(byte[] serializedRequest, string twoLetterLanguageCode)
    {
        var targetLanguage = twoLetterLanguageCode;
        var endpoint = _config.GetValueOrDefault("endpoint") as string ?? Constants.LENS_PROTO_ENDPOINT;

        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/x-protobuf" },
            { "X-Goog-Api-Key", Constants.LENS_API_KEY },
            { "User-Agent", _config["userAgent"] as string ?? "" },
            { "Accept", "*/*" },
            { "Accept-Encoding", "gzip, deflate, br" },
            { "Accept-Language", !string.IsNullOrEmpty(targetLanguage) ? $"{targetLanguage},en;q=0.9" : "en-US,en;q=0.9" }
        };

        var configHeaders = _config["headers"] as Dictionary<string, string>;
        if (configHeaders != null)
        {
            foreach (var kvp in configHeaders)
            {
                headers[kvp.Key] = kvp.Value;
            }
        }

        GenerateCookieHeader(headers);

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new ByteArrayContent(serializedRequest)
        };

        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-protobuf");

        foreach (var kvp in headers)
        {
            if (kvp.Key.ToLower() != "content-type")
            {
                request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }
        }

        var response = await _fetch!(request);

        if (response.Headers.TryGetValues("set-cookie", out var cookies))
        {
            SetCookies(cookies);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new LensError($"Lens returned status code {(int)response.StatusCode}", (int)response.StatusCode, response.Headers, errorBody);
        }

        var responseBytes = await response.Content.ReadAsByteArrayAsync();
        return LensProtoResponse.Deserialize(responseBytes);
    }

    public async Task<LensResult> ScanByData(byte[] uint8Array, string mime, int[] originalDimensions, string twoLetterLanguageCode)
    {
        if (!Constants.SUPPORTED_MIMES.Contains(mime) && mime != "image/gif")
        {
            Console.WriteLine($"MIME type {mime} might not be directly supported by the proto API, conversion recommended.");
        }

        var actualDimensions = Helper.ImageDimensionsFromData(uint8Array);

        var serializedRequest = CreateLensProtoRequest(uint8Array, actualDimensions.Width, actualDimensions.Height);
        var serverResponse = await SendProtoRequest(serializedRequest, twoLetterLanguageCode);

        return ParseLensProtoResponse(serverResponse, originalDimensions ?? new[] { actualDimensions.Width, actualDimensions.Height });
    }

    protected void GenerateCookieHeader(Dictionary<string, string> header)
    {
        if (_cookies.Count > 0)
        {
            var validCookies = _cookies.Where(kvp => kvp.Value.Expires > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                                       .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            _cookies = validCookies;

            if (validCookies.Count > 0)
            {
                header["cookie"] = string.Join("; ", validCookies.Select(kvp => $"{kvp.Key}={kvp.Value.Value}"));
            }
        }
    }

    protected void SetCookies(IEnumerable<string> combinedCookieHeader)
    {
        if (combinedCookieHeader == null) return;

        try
        {
            foreach (var cookieHeader in combinedCookieHeader)
            {
                var cookie = ParseCookie(cookieHeader);
                if (cookie != null)
                {
                    _cookies[cookie.Name] = cookie;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse or set cookies: {ex.Message}");
        }
    }

    protected Cookie? ParseCookie(string cookieHeader)
    {
        var parts = cookieHeader.Split(';');
        var cookieParts = parts[0].Split('=', 2);
        if (cookieParts.Length != 2) return null;

        var cookie = new Cookie
        {
            Name = cookieParts[0].Trim(),
            Value = cookieParts[1].Trim(),
            Expires = long.MaxValue
        };

        foreach (var part in parts.Skip(1))
        {
            var subParts = part.Split('=', 2);
            if (subParts.Length == 2 && subParts[0].Trim().ToLower() == "expires")
            {
                if (DateTimeOffset.TryParse(subParts[1].Trim(), out var expires))
                {
                    cookie.Expires = expires.ToUnixTimeMilliseconds();
                }
            }
        }

        return cookie;
    }

    protected void ParseCookies()
    {
        var headers = _config.GetValueOrDefault("headers") as Dictionary<string, string>;
        var cookieHeader = headers?.GetValueOrDefault("cookie");

        if (!string.IsNullOrEmpty(cookieHeader))
        {
            var cookiePairs = cookieHeader.Split("; ");
            foreach (var pair in cookiePairs)
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2)
                {
                    _cookies[parts[0]] = new Cookie
                    {
                        Name = parts[0],
                        Value = parts[1],
                        Expires = long.MaxValue
                    };
                }
            }
        }
    }

    protected async Task<HttpResponseMessage> DefaultFetchAsync(HttpRequestMessage request)
    {
        return await _sharedHttpClient.SendAsync(request);
    }
}
