namespace Nikse.SubtitleEdit.Logic.Config;

public class SeMergeSameTimeCode
{
    public int MaxMillisecondsDifference { get; set; }
    public bool MergeDialog { get; set; }
    public bool AutoBreak { get; set; }

    public SeMergeSameTimeCode()
    {
        MaxMillisecondsDifference = 500;
        AutoBreak = true;
    }
}