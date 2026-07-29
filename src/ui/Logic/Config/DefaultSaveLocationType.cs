namespace Nikse.SubtitleEdit.Logic.Config;

public enum DefaultSaveLocationType
{
    /// <summary>Folder of the file the suggested name came from (subtitle or video) - the default.</summary>
    SourceFileFolder,

    /// <summary>Let the OS file picker open in whatever folder it was last used in.</summary>
    LastUsedFolder,

    VideoFileFolder,
    SubtitleFileFolder,
    CustomFolder,
}
