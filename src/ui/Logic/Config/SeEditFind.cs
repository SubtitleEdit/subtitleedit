using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeEditFind
{
    public bool FindWholeWords { get; set; }
    public string FindSearchType { get; set; }

    /// <summary>
    /// Which columns the replace window works on in translator mode - see <see cref="FindScope"/>.
    /// </summary>
    public string ReplaceIn { get; set; }


    public SeEditFind()
    {
        FindWholeWords = false;
        FindSearchType = nameof(FindMode.CaseInsensitive);
        ReplaceIn = nameof(FindScope.TextAndOriginal);
    }
}
