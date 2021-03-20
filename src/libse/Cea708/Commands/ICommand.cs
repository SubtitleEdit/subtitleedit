namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public interface ICommand
    {
        int LineIndex { get; set; }
        byte[] GetBytes();
    }
}
