using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

public class Constants
{
    public const string LENS_PROTO_ENDPOINT = "https://lensfrontend-pa.googleapis.com/v1/crupload";
    public const string LENS_API_KEY = "AIzaSyDr2UxVnv_U85AbhhY8XSHSIavUW0DC-sY";
    
    public const string LENS_ENDPOINT = "https://lens.google.com/v3/upload";
    public const string LENS_API_ENDPOINT = "https://lens.google.com/uploadbyurl";

    public static readonly List<string> SUPPORTED_MIMES = new List<string>
    {
        "image/x-icon",
        "image/bmp",
        "image/jpeg",
        "image/png",
        "image/tiff",
        "image/webp",
        "image/heic",
        "image/gif"
    };

    public static readonly Dictionary<string, string> MIME_TO_EXT = new Dictionary<string, string>
    {
        { "image/x-icon", "ico" },
        { "image/bmp", "bmp" },
        { "image/jpeg", "jpg" },
        { "image/png", "png" },
        { "image/tiff", "tiff" },
        { "image/webp", "webp" },
        { "image/heic", "heic" },
        { "image/gif", "gif" }
    };

    public static readonly Dictionary<string, string> EXT_TO_MIME = new Dictionary<string, string>
    {
        { "ico", "image/x-icon" },
        { "bmp", "image/bmp" },
        { "jpg", "image/jpeg" },
        { "png", "image/png" },
        { "tiff", "image/tiff" },
        { "webp", "image/webp" },
        { "heic", "image/heic" },
        { "gif", "image/gif" }
    };
}
