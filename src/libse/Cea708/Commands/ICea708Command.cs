namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public interface ICea708Command
    {
        int LineIndex { get; set; }
        byte[] GetBytes();
    }
}
