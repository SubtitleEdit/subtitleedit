namespace Nikse.SubtitleEdit.Logic.Config;

public class SeMergeSameText
{
    public int MaxMillisecondsBetweenLines { get; set; }
    public bool IncludeIncrementingLines { get; set; }

    public SeMergeSameText()
    {
        MaxMillisecondsBetweenLines = 250;
        IncludeIncrementingLines = true;
    }
}