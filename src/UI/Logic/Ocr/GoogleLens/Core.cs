using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

public class Core
{
    private Func<HttpRequestMessage, Task<HttpResponseMessage>> _fetch;

    public Core(Dictionary<string, object>? config = null, Func<HttpRequestMessage, Task<HttpResponseMessage>>? fetch = null)
    {
        if (config == null)
        {
            config = new Dictionary<string, object>();
        }

        if (fetch != null)
        {
            _fetch = fetch;
        }
        else
        {
            _fetch = DefaultFetchAsync;
        }

        HeaderData.PopulateConfig(); 

        foreach (var kvp in config)
        {
            HeaderData.Config[kvp.Key] = kvp.Value;
        }

        // Lowercase all headers
        var headers = (Dictionary<string, string>)HeaderData.Config["headers"];
        foreach (var key in headers.Keys.ToList())
        {
            var value = headers[key];
            headers.Remove(key);
            headers[key.ToLower()] = value;
        }

        ParseCookies();
    }
    
    public async Task<List<string>> ScanByFileAsync(string path)
    {
        var file = await File.ReadAllBytesAsync(path);
        return await ScanByBufferAsync(file);
    }

    public async Task<List<string>> ScanByBufferAsync(byte[] buffer)
    {
        SKEncodedImageFormat? format = null;
        
        try
        {
            using var stream = new MemoryStream(buffer);
            using var codec = SKCodec.Create(stream);
            if (codec != null)
            {
                format = codec.EncodedFormat;
            }
        }
        catch
        {
            throw new Exception("File type not supported");
        }

        var (Width, Height) = Helper.ImageDimensionsFromData(buffer);
        if (Width == 0 && Height == 0)
        {
            throw new Exception("Could not determine image dimensions");
        }

        // Google Lens does not accept images larger than 1000x1000
        if (Width > 1000 || Height > 1000)
        {
            buffer = await Helper.ResizeImageAsync(buffer, 1000, 1000);
        }

        string mimeType = format switch
        {
            SKEncodedImageFormat.Png => "image/png",
            SKEncodedImageFormat.Jpeg => "image/jpeg",
            SKEncodedImageFormat.Webp => "image/webp",
            _ => "image/png"
        };

        return await ScanByData(buffer, mimeType, [Width, Height]);
    }
     
    public async Task<List<string>> FetchAsync(HttpContent formdata, int[] originalDimensions, bool secondTry = false)
    {
        var paramsList = new Dictionary<string, string>
        {
            { "s", "4" },
            { "re", "df" },
            { "stcs", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() },
            { "vpw", HeaderData.Config["viewport"].ToString()! },
            { "vph", HeaderData.Config["viewport"].ToString()! },
            { "ep", "subb" }
        };

        var url = $"{HeaderData.Config["endpoint"]}?{new FormUrlEncodedContent(paramsList).ReadAsStringAsync().Result}";
        var headers = HeaderData.GenerateHeaders();

        foreach (var kvp in (Dictionary<string, string>)HeaderData.Config["headers"])
        {
            headers[kvp.Key] = kvp.Value;
        }

        GenerateCookieHeader(headers);

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = formdata
        };

        foreach (var kvp in headers)
        {
            request.Headers.Add(kvp.Key, kvp.Value);
        }

        var response = await _fetch(request);
        var text = await response.Content.ReadAsStringAsync();
        byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
        
        // Check the content encoding and decompress accordingly
        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            responseBytes = Helper.DecompressGzip(responseBytes);
        }
        if (response.Content.Headers.ContentEncoding.Contains("deflate"))
        {
            responseBytes = Helper.DecompressDeflate(responseBytes);
        }
        if (response.Content.Headers.ContentEncoding.Contains("br"))
        {
            responseBytes = Helper.DecompressBrotli(responseBytes);
        }
        
        var responseBody = Encoding.UTF8.GetString(responseBytes);

        if (response.Headers.TryGetValues("set-cookie", out var setCookieValues))
        {
            SetCookies(setCookieValues);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Found)
        {
            if (secondTry)
            {
                throw new CoreError("Lens returned a 302 status code twice", (int)response.StatusCode, response.Headers, text);
            }

            var consentHeaders = HeaderData.GenerateHeaders();
            consentHeaders["Content-Type"] = "application/x-www-form-urlencoded";
            consentHeaders["Referer"] = "https://consent.google.com/";
            consentHeaders["Origin"] = "https://consent.google.com";

            GenerateCookieHeader(consentHeaders);

            var location = response.Headers.Location;
            if (location == null)
            {
                throw new Exception("Location header not found");
            }

            var redirectLink = new Uri(location.ToString());
            var params2 = new Dictionary<string, string>
            {
                { "x", "6" },
                { "set_eom", "true" },
                { "bl", "boq_identityfrontenduiserver_20240129.02_p0" },
                { "app", "0" }
            };

            await Task.Delay(500);

            var saveConsentRequest = new HttpRequestMessage(HttpMethod.Post, "https://consent.google.com/save")
            {
                Content = new FormUrlEncodedContent(params2)
            };

            foreach (var kvp in consentHeaders)
            {
                saveConsentRequest.Headers.Add(kvp.Key, kvp.Value);
            }

            var saveConsentResponse = await _fetch(saveConsentRequest);

            if (saveConsentResponse.StatusCode == System.Net.HttpStatusCode.SeeOther)
            {
                if (saveConsentResponse.Headers.TryGetValues("set-cookie", out var consentCookies))
                {
                    SetCookies(consentCookies);
                }
                await Task.Delay(500);
                return await FetchAsync(formdata, originalDimensions, true);
            }
        }

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new CoreError("Lens returned a non-200 status code", (int)response.StatusCode, response.Headers, text);
        }

        try
        {
            var afData = GetAFData(responseBody);
            return ParseResult(afData!, originalDimensions);
        }
        catch (Exception e)
        {
            throw new CoreError($"Could not parse response: {e.Message}", (int)response.StatusCode, response.Headers, text);
        }
    }
    
    public async Task<List<string>> ScanByData(byte[] uint8, string mime, int[] originalDimensions)
    {
        if (!Constants.SUPPORTED_MIMES.Contains(mime))
        {
            throw new Exception("File type not supported");
        }
        if (originalDimensions == null)
        {
            throw new Exception("Original dimensions not set");
        }

        string fileName = $"image.{Constants.MIME_TO_EXT[mime]}";
        var dimensions = Helper.ImageDimensionsFromData(uint8);

        var width = dimensions.Width;
        var height = dimensions.Height;

        if (width > 1000 || height > 1000)
        {
            throw new Exception("Image dimensions are larger than 1000x1000");
        }

        var file = new ByteArrayContent(uint8);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mime);
        var formdata = new MultipartFormDataContent
        {
            { file, "encoded_image", fileName },
            { new StringContent(width.ToString()), "original_width" },
            { new StringContent(height.ToString()), "original_height" },
            { new StringContent($"{width},{height}"), "processed_image_dimensions" }
        };

        return await FetchAsync(formdata, originalDimensions);
    }
    
    private void GenerateCookieHeader(Dictionary<string, string> header)
    {
        if (HeaderData.Cookies.Count > 0)
        {
            HeaderData.Cookies = HeaderData.Cookies.Where(kvp => kvp.Value.expires > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                              .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            header["cookie"] = string.Join("; ", HeaderData.Cookies.Select(kvp => $"{kvp.Key}={kvp.Value.value}"));
        }
    }

    private void SetCookies(IEnumerable<string> combinedCookieHeader)
    {
        foreach (var cookieHeader in combinedCookieHeader)
        {
            var cookie = ParseCookie(cookieHeader);
            if (cookie != null)
            {
                HeaderData.Cookies[cookie.Name] = cookie;
            }
        }
    }
    
    private Cookie? ParseCookie(string cookieHeader)
    {
        var parts = cookieHeader.Split(';');
        var cookieParts = parts[0].Split('=');
        if (cookieParts.Length != 2)
        {
            return null;
        }

        var cookie = new Cookie
        {
            Name = cookieParts[0].Trim(),
            Value = cookieParts[1].Trim(),
            Expires = DateTimeOffset.MaxValue.ToUnixTimeMilliseconds()
        };

        foreach (var part in parts)
        {
            var subParts = part.Split('=');
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

    public static JObject? GetAFData(string text)
    {
        var matches = Regex.Matches(text, @"AF_initDataCallback\((\{.*?\})\)", RegexOptions.Singleline);
        var lensCallback = matches.FirstOrDefault(m => m.Value.Contains("DetectedObject"));
        if (lensCallback == null)
        {
            Console.WriteLine(matches);
            throw new Exception("Could not find matching AF_initDataCallback");
        }

        var json = lensCallback.Groups[1].Value;
        return JObject.Parse(json);
    }

    public static List<string> ParseResult(JObject afData, int[] imageDimensions)
    {
        var data = afData["data"];
        var fullTextPart = data?[3];
        var textSegments = new List<string>();
        var textRegions = new List<double[]>();

        try
        {
            // Method 1: get text segments and regions directly
            if (fullTextPart?[4]?[0]?[0] != null)
            {
                textSegments = fullTextPart[4]![0]![0]!.ToObject<List<string>>()!;
            }
            if (data?[2]?[3]?[0] != null)
            {
                textRegions = data[2]![3]![0]!.Where(x => x[11]?.ToString()?.StartsWith("text:") == true)
                    .Select(x => x[1]!.ToObject<double[]>()!).ToList();
            }
        }
        catch
        {
            var bigParts = fullTextPart?[2]?[0];
            if (bigParts != null)
            {
                foreach (var bigPart in bigParts)
                {
                    var parts = bigPart[0];
                    if (parts == null)
                    {
                        continue;
                    }
                    
                    foreach (var part in parts)
                    {
                        if (part[0] == null || part[1] == null)
                        {
                            continue;
                        }
                        
                        var text = part[0]!.Select(t => t[0]?.ToString() + (t[3]?.ToString() ?? "")).Aggregate((a, b) => a + b);
                        var region = part[1]!.ToObject<double[]>();
                        if (region != null && region.Length >= 4)
                        {
                            var y = region[0];
                            var x = region[1];
                            double width = region[2];
                            double height = region[3];
                            double centerX = x + width / 2;
                            double centerY = y + height / 2;
                            region = [centerX, centerY, width, height];

                            textSegments.Add(text);
                            textRegions.Add(region);
                        }
                    }
                }
            }
        }
        return textSegments; 
    }

    private async Task<HttpResponseMessage> DefaultFetchAsync(HttpRequestMessage request)
    {
        using var client = new HttpClient();
        return await client.SendAsync(request);
    }
    
    private void ParseCookies()
    {
        if (HeaderData.Config.ContainsKey("headers") && ((Dictionary<string, string>)HeaderData.Config["headers"]).ContainsKey("cookie"))
        {
            var cookieHeader = ((Dictionary<string, string>)HeaderData.Config["headers"])["cookie"];
            var cookiePairs = cookieHeader.Split("; ").Select(c => c.Split('='));
            foreach (var pair in cookiePairs)
            {
                if (pair.Length >= 2)
                {
                    HeaderData.Config[pair[0]] = new
                    {
                        name = pair[0],
                        value = pair[1],
                        expires = DateTimeOffset.MaxValue.ToUnixTimeMilliseconds()
                    };
                }
            }
        }
    } 
}
