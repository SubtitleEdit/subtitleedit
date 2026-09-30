using System.IO;

namespace Nikse.SubtitleEdit.Features.Files.ImportDvd;

public class DvdVobFileItem
{
    public string FileName { get; }
    public string Name { get; }
    public string Folder { get; }
    public long Size { get; }

    public DvdVobFileItem(string fileName)
    {
        FileName = fileName;
        Name = Path.GetFileName(fileName);
        Folder = Path.GetDirectoryName(fileName) ?? string.Empty;
        try
        {
            Size = new FileInfo(fileName).Length;
        }
        catch
        {
            // ignore - shown as 0
        }
    }

    public override string ToString()
    {
        return Name;
    }
}