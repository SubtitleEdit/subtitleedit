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

        var start = param.StartTime.ToString(@"hh\:mm\:ss\,fff");
        var end = param.EndTime.ToString(@"hh\:mm\:ss\,fff");
        _html.AppendFormat("#{0}:{1}-->{2}<div style='text-align:center'><img src='{3}' />",
            _count, start, end, imageFileName);

        if (!string.IsNullOrEmpty(param.Text))
        {
            var text = WebUtility.HtmlEncode(param.Text
                    .Replace("<i>", "@1__").Replace("</i>", "@2__"))
                .Replace("@1__", "<i>").Replace("@2__", "</i>")
                .Replace(Environment.NewLine, "<br />");
            _html.Append("<br /><div style='font-size:22px; background-color:#F5F5F5'>")
                .Append(text).Append("</div>");
        }

        _html.AppendLine("</div><br /><hr />");
    }

    public void WriteFooter()
    {
        _html.AppendLine("</body>");
        _html.AppendLine("</html>");
        var htmlPath = Path.Combine(_folderName, "index.html");
        File.WriteAllText(htmlPath, _html.ToString(), Encoding.UTF8);
    }
}
