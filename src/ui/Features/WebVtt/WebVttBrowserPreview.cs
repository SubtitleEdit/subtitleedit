using System;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Features.WebVtt;

/// <summary>
/// Builds a stand-alone HTML page that plays the loaded video with the subtitle attached as a
/// WebVTT text track, so the cues can be checked in a browser's own WebVTT renderer.
/// </summary>
public static class WebVttBrowserPreview
{
    /// <summary>Video containers a browser can be expected to play from a local file.</summary>
    public static bool IsSupportedVideoFile(string? videoFileName)
    {
        if (string.IsNullOrEmpty(videoFileName))
        {
            return false;
        }

        var extension = Path.GetExtension(videoFileName).ToLowerInvariant();
        return extension is ".mp4" or ".m4v" or ".webm" or ".ogv" or ".ogg" or ".mov";
    }

    public static string GenerateHtml(string webVttText, string videoFileName)
    {
        // The subtitle goes in as a data URI: a browser will not load a file:// track from a
        // file:// page (cross-origin), and base64 also side-steps escaping the cue text.
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(webVttText));
        var extension = Path.GetExtension(videoFileName).TrimStart('.').ToLowerInvariant();
        var mimeType = extension switch
        {
            "webm" => "webm",
            "ogv" or "ogg" => "ogg",
            _ => "mp4",
        };

        return $@"<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <title>WebVTT preview</title>
  <style>
    body {{ background: #202020; color: #eee; font-family: sans-serif; margin: 0; padding: 1em; }}
    video {{ max-width: 100%; }}
  </style>
</head>
<body>
  <video controls preload=""metadata"">
    <source src=""{ToFileUri(videoFileName)}"" type=""video/{mimeType}"" />
    <track label=""Preview"" kind=""subtitles"" srclang=""en"" src=""data:text/vtt;base64,{base64}"" default>
  </video>
</body>
</html>";
    }

    private static string ToFileUri(string fileName)
    {
        try
        {
            return new Uri(Path.GetFullPath(fileName)).AbsoluteUri;
        }
        catch
        {
            return "file://" + fileName;
        }
    }
}
