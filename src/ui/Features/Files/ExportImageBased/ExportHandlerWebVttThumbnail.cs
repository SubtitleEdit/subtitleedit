using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.IO;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerWebVttThumbnail : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.WebVttThumbnail;
    public string Extension => ".vtt";
    public bool UseFileName => false;
    public string Title => string.Format(Se.Language.General.ExportToX, "WebVTT thumbnails");

    private string _folderName = string.Empty;
    private int _imagesSavedCount = 0;
    private Subtitle _subtitle = new Subtitle();

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _subtitle = new Subtitle(); 
        _folderName = fileOrFolderName;
        if (!Directory.Exists(_folderName))
        {
            Directory.CreateDirectory(_folderName);
        }

        _imagesSavedCount = 0;
    }

    public void CreateParagraph(ImageParameter param)
    {
    }

    public void WriteParagraph(ImageParameter param)
    {
        _imagesSavedCount++;

        var numberString = $"{_imagesSavedCount:0000}";
        var fileName = Path.Combine(numberString + ".png");

        File.WriteAllBytes(Path.Combine(_folderName, fileName), param.Bitmap.ToPngArray());
        
        var p = new Paragraph(fileName, param.StartTime.TotalMilliseconds, param.EndTime.TotalMilliseconds);
        _subtitle.Paragraphs.Add(p);    
    }

    public void WriteFooter()
    {
        var format = new WebVTT();
        var text = format.ToText(_subtitle, "index.vtt");
        FileUtil.WriteAllTextWithDefaultUtf8(Path.Combine(_folderName, "index.vtt"), text);
    }
}