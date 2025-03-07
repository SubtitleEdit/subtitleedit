namespace Nikse.SubtitleEdit.Controls.Interfaces
{
    public interface ISubtitleEditTextBox
    {
        int SelectionStart { get; set; }
        int SelectionLength { get; set; }
        string Text { get; set; }
    }
}