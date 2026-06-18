using Nikse.SubtitleEdit.Core.Common;
using System.Net;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Export;

public class ExportHandlerHtmlIndex : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.HtmlIndex;
    public string Extension => "";
    public bool UseFileName => false;
    public string Title => "Export to images with HTML index";

    private string _folderName = string.Empty;
    private readonly StringBuilder _html = new();
    private int _count = 0;

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _folderName = fileOrFolderName;
        if (!Directory.Exists(_folderName))
        {
            Directory.CreateDirectory(_folderName);
        }

        _count = 0;
        _html.Clear();
        _html.AppendLine("<!DOCTYPE html>");
        _html.AppendLine("<html>");
        _html.AppendLine("<head>");
        _html.AppendLine("  <meta charset=\"UTF-8\" />");
        _html.AppendLine("  <title>Subtitle images</title>");
        _html.AppendLine("</head>");
        _html.AppendLine("<body>");
    }

    public void CreateParagraph(ImageParameter param) { }

    public void WriteParagraph(ImageParameter param)
    {
        _count++;
        var imageFileName = $"{_count:0000}.png";
        var filePath = Path.Combine(_folderName, imageFileName);
        File.WriteAllBytes(filePath, param.Bitmap.ToPngArray());

        var start = new TimeCode(param.StartTime).ToString();
        var end = new TimeCode(param.EndTime).ToString();
        _html.AppendFormat("#{0}:{1}-->{2}<div style='text-align:center'><img src='{3}' />",
            _count, start, end, imageFileName);

        if (!string.IsNullOrEmpty(param.Text))
        {
            var text = EncodeText(param.Text);
            _html.Append("<br /><div style='font-size:22px; background-color:#F5F5F5'>")
                .Append(text).Append("</div>");
        }

        _html.AppendLine("</div><br /><hr />");
    }

    private static string EncodeText(string text)
    {
        // Protect known inline tags with control-char sentinels before HTML-encoding.
        // Control chars \x01/\x02 cannot appear in subtitle text, eliminating sentinel collisions.
        var s = HtmlUtil.FixUpperTags(text)
            .Replace("<i>", "\x01i\x02").Replace("</i>", "\x01/i\x02")
            .Replace("<b>", "\x01b\x02").Replace("</b>", "\x01/b\x02")
            .Replace("<u>", "\x01u\x02").Replace("</u>", "\x01/u\x02");

        return WebUtility.HtmlEncode(s)
            .Replace("\x01i\x02", "<i>").Replace("\x01/i\x02", "</i>")
            .Replace("\x01b\x02", "<b>").Replace("\x01/b\x02", "</b>")
            .Replace("\x01u\x02", "<u>").Replace("\x01/u\x02", "</u>")
            .Replace(Environment.NewLine, "<br />");
    }

    public void WriteFooter()
    {
        _html.AppendLine("</body>");
        _html.AppendLine("</html>");
        var htmlPath = Path.Combine(_folderName, "index.html");
        File.WriteAllText(htmlPath, _html.ToString(), Encoding.UTF8);
    }
}
