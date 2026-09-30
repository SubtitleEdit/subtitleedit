using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Files.ImportDvd;

public class DvdTitleDisplay
{
    public DvdTitle Title { get; set; }
    public string Name { get; set; }
    public TimeSpan Duration { get; set; }
    public int Chapters { get; set; }
    public string Languages { get; set; }
    public string Status { get; set; }

    public DvdTitleDisplay(DvdTitle title, string name, string languages, string status)
    {
        Title = title;
        Name = name;
        Duration = title.ProgramChain.Duration;
        Chapters = title.ProgramChain.ProgramCount;
        Languages = languages;
        Status = status;
    }

    public override string ToString()
    {
        var text = $"{Name}:  {Duration:hh\\:mm\\:ss}  ({Chapters} {Se.Language.File.Chapters.ToLowerInvariant()})";
        return string.IsNullOrEmpty(Status) ? text : $"{text}  -  {Status}";
    }
}