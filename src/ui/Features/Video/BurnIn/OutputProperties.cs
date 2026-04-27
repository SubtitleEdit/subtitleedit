namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class OutputProperties 
{
    public string OutputFolder { get; set; }
    public bool UseOutputFolder { get; set; }
    public string Suffix { get; set; }

    public OutputProperties()
    {
        OutputFolder = string.Empty;
        UseOutputFolder = false;
        Suffix = string.Empty;
    }
}
