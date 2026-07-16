using Nikse.SubtitleEdit.Core.Common;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// IMSC 1.1 image profile (TTML Profiles for Internet Media Subtitles and Captions 1.1, image
/// profile). Emits a single self-contained TTML file: each cue is a cropped PNG embedded as a
/// base64 &lt;smpte:image&gt; in the head, referenced from a &lt;div smpte:backgroundImage&gt; in
/// the body, positioned by a per-cue region with percentage origin/extent. Media timebase.
/// This is the standardized image-subtitle carriage used for streaming/broadcast delivery, and
/// round-trips through SE's Timed Text Base64 Image reader.
/// Spec: https://www.w3.org/TR/ttml-imsc1.1/
/// </summary>
public class ExportHandlerImscImage : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.ImscImage;
    public string Extension => ".ttml";
    public bool UseFileName => true;
    public string Title => string.Format("Export to {0}", "IMSC 1.1 image profile");

    private string _fileName = string.Empty;
    private int _width = 1920;
    private int _height = 1080;
    private int _count;
    private readonly StringBuilder _images = new();
    private readonly StringBuilder _regions = new();
    private readonly StringBuilder _divs = new();

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _fileName = fileOrFolderName;
        _width = imageParameter.ScreenWidth > 0 ? imageParameter.ScreenWidth : 1920;
        _height = imageParameter.ScreenHeight > 0 ? imageParameter.ScreenHeight : 1080;
        _count = 0;
        _images.Clear();
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

        var base64 = Convert.ToBase64String(param.Bitmap.ToPngArray());
        _images.Append("      <smpte:image xml:id=\"img").Append(id).Append("\" imagetype=\"PNG\" encoding=\"Base64\">")
            .Append(base64).Append("</smpte:image>").Append('\n');

        GetPlacement(param, out var x, out var y);
        var originX = Pct(x, _width);
        var originY = Pct(y, _height);
        var extentX = Pct(param.Bitmap.Width, _width);
        var extentY = Pct(param.Bitmap.Height, _height);

        _regions.Append("      <region xml:id=\"region").Append(id).Append("\" tts:origin=\"")
            .Append(originX).Append(' ').Append(originY).Append("\" tts:extent=\"")
            .Append(extentX).Append(' ').Append(extentY).Append("\"/>").Append('\n');

        _divs.Append("      <div smpte:backgroundImage=\"#img").Append(id).Append("\" region=\"region").Append(id)
            .Append("\" begin=\"").Append(ToTimeCode(param.StartTime)).Append("\" end=\"").Append(ToTimeCode(param.EndTime))
            .Append("\" ttm:role=\"caption\"/>").Append('\n');
    }

    public void WriteFooter()
    {
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        sb.Append("<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" ")
            .Append("xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" xmlns:ttm=\"http://www.w3.org/ns/ttml#metadata\" ")
            .Append("xmlns:smpte=\"http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt\" ")
            .Append("ttp:profile=\"http://www.w3.org/ns/ttml/profile/imsc1/image\" ttp:timeBase=\"media\" ")
            .Append("tts:extent=\"").Append(_width.ToString(CultureInfo.InvariantCulture)).Append("px ")
            .Append(_height.ToString(CultureInfo.InvariantCulture)).Append("px\" xml:lang=\"en\">\n");
        sb.Append("  <head>\n");
        sb.Append("    <metadata>\n");
        sb.Append(_images);
        sb.Append("    </metadata>\n");
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
        switch (param.Alignment)
        {
            case ExportAlignment.BottomLeft:
                x = 0;
                break;
            case ExportAlignment.BottomRight:
                x = _width - param.Bitmap.Width;
                break;
            case ExportAlignment.MiddleCenter:
                y = (_height - param.Bitmap.Height) / 2;
                break;
            case ExportAlignment.MiddleLeft:
                x = 0;
                y = (_height - param.Bitmap.Height) / 2;
                break;
            case ExportAlignment.MiddleRight:
                x = _width - param.Bitmap.Width;
                y = (_height - param.Bitmap.Height) / 2;
                break;
            case ExportAlignment.TopCenter:
                y = 0;
                break;
            case ExportAlignment.TopLeft:
                x = 0;
                y = 0;
                break;
            case ExportAlignment.TopRight:
                x = _width - param.Bitmap.Width;
                y = 0;
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
