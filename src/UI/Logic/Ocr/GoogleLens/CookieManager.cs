using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

public static class CookieManager
{
    private const string CookieFileName = "lens_cookies.json";

    public static Dictionary<string, Cookie> LoadCookies()
    {
        try
        {
            if (File.Exists(CookieFileName))
            {
                var json = File.ReadAllText(CookieFileName);
                var cookies = JsonSerializer.Deserialize<Dictionary<string, Cookie>>(json);
                if (cookies != null)
                {
                    Console.WriteLine($"Loaded {cookies.Count} cookies from {CookieFileName}");
                    return cookies;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load cookies: {ex.Message}");
        }

        return new Dictionary<string, Cookie>();
    }

    public static void SaveCookies(Dictionary<string, Cookie> cookies)
    {
        try
        {
            var json = JsonSerializer.Serialize(cookies, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(CookieFileName, json);
            Console.WriteLine($"Saved {cookies.Count} cookies to {CookieFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not save cookies: {ex.Message}");
        }
    }
}
