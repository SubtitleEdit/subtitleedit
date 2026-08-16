namespace Nikse.SubtitleEdit.Features.Video.Chapters;

public enum ChapterExportKind
{
    MatroskaXml,
    FfmpegMetadata,
    Ogm,
    YouTube,
}

/// <summary>
/// An entry in the export format picker. The kind, not the extension, decides which writer runs -
/// OGM and YouTube chapters are both plain ".txt".
/// </summary>
public class ChapterExportFormat
{
    public ChapterExportKind Kind { get; }

    public string Name { get; }

    public string Extension { get; }

    public ChapterExportFormat(ChapterExportKind kind, string name, string extension)
    {
        Kind = kind;
        Name = name;
        Extension = extension;
    }

    public override string ToString() => $"{Name} ({Extension})";
}
