namespace Nikse.SubtitleEdit.Features.Shared.PickVobSubLanguage;

public class VobSubLanguageDisplay
{
    public int StreamId { get; set; }
    public string StreamIdHex { get; set; }
    public string Language { get; set; }
    public int Count { get; set; }

    public VobSubLanguageDisplay()
    {
        StreamIdHex = string.Empty;
        Language = string.Empty;
    }
}
