using Nikse.SubtitleEdit.Core.Common;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// IMSC 1.1 image profile (TTML Profiles for Internet Media Subtitles and Captions 1.1, image
/// profile). Writes the TTML document plus one cropped PNG per cue next to it, named after the
/// document ("movie_0001.png", ...). Each cue is a &lt;div smpte:backgroundImage="movie_0001.png"&gt;
/// in the body, positioned by a per-cue region with percentage origin/extent. Media timebase.
/// The PNGs are separate files because the image profile prohibits embedded &lt;smpte:image&gt;
/// (IMSC 1.1 section 9.4.5). Round-trips through SE's Timed Text Image reader.
/// Spec: https://www.w3.org/TR/ttml-imsc1.1/
/// </summary>
public class ExportHandlerImscImage : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.ImscImage;
    public string Extension => ".ttml";
    public bool UseFileName => true;
    public string Title => string.Format("Export to {0}", "IMSC 1.1 image profile");

    private string _fileName = string.Empty;
    private string _folder = string.Empty;
    private string _imagePrefix = string.Empty;
    private int _width = 1920;
    private int _height = 1080;
    private int _count;
    private readonly StringBuilder _regions = new();
    private readonly StringBuilder _divs = new();

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _fileName = fileOrFolderName;
        _folder = Path.GetDirectoryName(Path.GetFullPath(fileOrFolderName)) ?? string.Empty;
        _imagePrefix = Path.GetFileNameWithoutExtension(fileOrFolderName);
        _width = imageParameter.ScreenWidth > 0 ? imageParameter.ScreenWidth : 1920;
        _height = imageParameter.ScreenHeight > 0 ? imageParameter.ScreenHeight : 1080;
        _count = 0;
        _regions.Clear();
        _divs.Clear();
    }

    public void CreateParagraph(ImageParameter param)
    {
    }

    public void WriteParagraph(ImageParameter param)
    {
        var id = _count;
        _count++;

        var imageFileName = GetImageFileName(_imagePrefix, _count);
        File.WriteAllBytes(Path.Combine(_folder, imageFileName), param.Bitmap.ToPngArray());

        GetPlacement(param, out var x, out var y);
        var originX = Pct(x, _width);
        var originY = Pct(y, _height);
        var extentX = Pct(param.Bitmap.Width, _width);
        var extentY = Pct(param.Bitmap.Height, _height);

        _regions.Append("      <region xml:id=\"region").Append(id).Append("\" tts:origin=\"")
            .Append(originX).Append(' ').Append(originY).Append("\" tts:extent=\"")
            .Append(extentX).Append(' ').Append(extentY).Append("\"/>").Append('\n');

        _divs.Append("      <div smpte:backgroundImage=\"").Append(SecurityElementEscape(imageFileName)).Append("\" region=\"region").Append(id)
            .Append("\" begin=\"").Append(ToTimeCode(param.StartTime)).Append("\" end=\"").Append(ToTimeCode(param.EndTime)).Append('"');
        if (param.IsForced)
        {
            _divs.Append(" itts:forcedDisplay=\"true\"");
        }

        _divs.Append(" ttm:role=\"caption\"/>").Append('\n');
    }

    public void WriteFooter()
    {
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        sb.Append("<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" ")
            .Append("xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" xmlns:ttm=\"http://www.w3.org/ns/ttml#metadata\" ")
            .Append("xmlns:smpte=\"http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt\" ")
            .Append("xmlns:itts=\"http://www.w3.org/ns/ttml/profile/imsc1#styling\" ")
            .Append("ttp:profile=\"http://www.w3.org/ns/ttml/profile/imsc1/image\" ttp:timeBase=\"media\" ")
            .Append("tts:extent=\"").Append(_width.ToString(CultureInfo.InvariantCulture)).Append("px ")
            .Append(_height.ToString(CultureInfo.InvariantCulture)).Append("px\" xml:lang=\"en\">\n");
        sb.Append("  <head>\n");
        sb.Append("    <layout>\n");
        sb.Append(_regions);
        sb.Append("    </layout>\n");
        sb.Append("  </head>\n");
        sb.Append("  <body>\n");
        sb.Append(_divs);
        sb.Append("  </body>\n");
        sb.Append("</tt>\n");

        File.WriteAllText(_fileName, sb.ToString(), new UTF8Encoding(false));
    }

    // Top-left placement of the cue bitmap on the screen, mirroring the BDN XML handler's
    // alignment/override logic so all image exporters position cues the same way.
    private void GetPlacement(ImageParameter param, out int x, out int y)
    {
        x = (_width - param.Bitmap.Width) / 2;
        y = _height - (param.Bitmap.Height + param.BottomTopMargin);
        // Honour the user's margins for the left/right/top placements too - the bitmap is
        // the trimmed glyph box, so a literal 0 glues the line to the frame edge.
        var border = param.LeftRightMargin;
        var topBorder = param.BottomTopMargin;
        switch (param.Alignment)
        {
            case ExportAlignment.BottomLeft:
                x = border;
                break;
            case ExportAlignment.BottomRight:
                x = _width - param.Bitmap.Width - border;
                break;
            case ExportAlignment.MiddleCenter:
                y = (_height - param.Bitmap.Height) / 2;
                break;
            case ExportAlignment.MiddleLeft:
                x = border;
                y = (_height - param.Bitmap.Height) / 2;
                break;
            case ExportAlignment.MiddleRight:
                x = _width - param.Bitmap.Width - border;
                y = (_height - param.Bitmap.Height) / 2;
                break;
            case ExportAlignment.TopCenter:
                y = topBorder;
                break;
            case ExportAlignment.TopLeft:
                x = border;
                y = topBorder;
                break;
            case ExportAlignment.TopRight:
                x = _width - param.Bitmap.Width - border;
                y = topBorder;
                break;
        }

        if (param.OverridePosition.HasValue &&
            param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < _width &&
            param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < _height)
        {
            x = param.OverridePosition.Value.X;
            y = param.OverridePosition.Value.Y;
        }

        if (x < 0)
        {
            x = 0;
        }

        if (y < 0)
        {
            y = 0;
        }
    }

    /// <summary>The cue's PNG, next to the TTML and named after it: "movie_0001.png".</summary>
    public static string GetImageFileName(string ttmlFileNameWithoutExtension, int number)
    {
        return ttmlFileNameWithoutExtension + "_" + number.ToString("0000", CultureInfo.InvariantCulture) + ".png";
    }

    private static string SecurityElementEscape(string value)
    {
        return System.Security.SecurityElement.Escape(value) ?? string.Empty;
    }

    private static string Pct(int value, int total)
    {
        var pct = total > 0 ? value * 100.0 / total : 0.0;
        return pct.ToString("0.##", CultureInfo.InvariantCulture) + "%";
    }

    private static string ToTimeCode(TimeSpan time)
    {
        return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:000}";
    }
}
