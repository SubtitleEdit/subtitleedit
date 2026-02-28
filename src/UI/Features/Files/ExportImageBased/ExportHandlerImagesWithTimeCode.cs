using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerImagesWithTimeCode : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.ImagesWithTimeCode;
    public string Extension => "";
    public bool UseFileName => false;
    public string Title => string.Format(Se.Language.General.ExportToX, "images with time code in file name");

    private string _folderName = string.Empty;
    private int _count = 0;

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _folderName = fileOrFolderName;
        if (!Directory.Exists(_folderName))
        {
            Directory.CreateDirectory(_folderName);
        }

        _count = 0;
    }

    public void CreateParagraph(ImageParameter param)
    {

    }

    public void WriteParagraph(ImageParameter param)
    {
        // 0_00_01_042__0_00_03_919_01.png
        _count++;
        var imageFileName = $"{param.StartTime.Hours}_{param.StartTime.Minutes:00}_{param.StartTime.Seconds:00}_{param.StartTime.Milliseconds:000}__" +
                       $"{param.EndTime.Hours}_{param.EndTime.Minutes:00}_{param.EndTime.Seconds:00}_{param.EndTime.Milliseconds:000}_{_count:00}.png";


        var fileName = Path.Combine(_folderName, imageFileName);
        File.WriteAllBytes(fileName, param.Bitmap.ToPngArray());
    }

    public void WriteFooter()
    {
    }
}