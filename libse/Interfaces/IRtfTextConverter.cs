namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IRtfTextConverter
    {
        string RtfToText(string rtf);
        string TextToRtf(string text);
    }
}
