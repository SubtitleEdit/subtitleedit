namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public interface IExportHandler
{
    ExportImageType ExportImageType { get; }
    string Extension { get; }
    string Title { get; }
    bool UseFileName { get; }
    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter);
    void CreateParagraph(ImageParameter param);
    void WriteParagraph(ImageParameter param);
    public void WriteFooter();
}