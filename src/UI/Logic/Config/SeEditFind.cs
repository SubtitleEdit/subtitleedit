using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeEditFind
{
    public bool FindWholeWords { get; set; }
    public string FindSearchType { get; set; }


    public SeEditFind()
    {
        FindWholeWords = false;
        FindSearchType = nameof(FindMode.CaseInsensitive);
    }
}
