using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

internal class HeaderData
{
    public static Dictionary<string, object> Config = [];
    public static Dictionary<string, dynamic> Cookies = [];
    public static Dictionary<string, string> GenerateHeaders()
    {
        var headers = new Dictionary<string, string>
        {
        { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7" },
        { "Accept-Encoding", "gzip, deflate, br" },
        { "Accept-Language", "en-US,en;q=0.9" },
        { "Cache-Control", "max-age=0" },
        { "Origin", "https://lens.google.com" },
        { "Referer", "https://lens.google.com/" },
        { "Sec-Ch-Ua", $"\"Not A(Brand\";v=\"99\", \"Google Chrome\";v=\"{Config["majorChromeVersion"]}\", \"Chromium\";v=\"{Config["majorChromeVersion"]}\"" },
        { "Sec-Ch-Ua-Arch", "\"x86\"" },
        { "Sec-Ch-Ua-Bitness", "\"64\"" },
        { "Sec-Ch-Ua-Full-Version", $"\"{Config["chromeVersion"]}\"" },
        { "Sec-Ch-Ua-Full-Version-List", $"\"Not A(Brand\";v=\"99.0.0.0\", \"Google Chrome\";v=\"{Config["majorChromeVersion"]}\", \"Chromium\";v=\"{Config["majorChromeVersion"]}\"" },
        { "Sec-Ch-Ua-Mobile", "?0" },
        { "Sec-Ch-Ua-Model", "\"\"" },
        { "Sec-Ch-Ua-Platform", "\"Windows\"" },
        { "Sec-Ch-Ua-Platform-Version", "\"15.0.0\"" },
        { "Sec-Ch-Ua-Wow64", "?0" },
        { "Sec-Fetch-Dest", "document" },
        { "Sec-Fetch-Mode", "navigate" },
        { "Sec-Fetch-Site", "same-origin" },
        { "Sec-Fetch-User", "?1" },
        { "Upgrade-Insecure-Requests", "1" },
        { "User-Agent", Config["userAgent"] as string ?? string.Empty }
        };
        return headers;
    }
    public static void PopulateConfig()
    {
        string chromeVersion = "124.0.6367.60";
        string majorChromeVersion = chromeVersion.Split('.')[0];
        Config = new Dictionary<string, object>
        {
            { "chromeVersion", chromeVersion },
            { "majorChromeVersion", majorChromeVersion },
            { "sbisrc", $"Google Chrome {chromeVersion} (Official) Windows" },
            { "userAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36" },
            { "endpoint", Constants.LENS_ENDPOINT },
            { "viewport", new int[] { 1920, 1080 } },
            { "headers", new Dictionary<string, string>() },
            { "fetchOptions", new Dictionary<string, string>() }
        };
    }
}
