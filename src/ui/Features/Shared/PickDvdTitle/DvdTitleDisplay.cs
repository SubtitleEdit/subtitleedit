using Nikse.SubtitleEdit.Core.VobSub;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.PickDvdTitle;

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
        return Name;
    }
}