namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class Delay : CommandBase
    {
        public int Milliseconds { get; set; }

        public Delay(int lineIndex, byte[] bytes, int index)
        {
            LineIndex = lineIndex;
            Milliseconds = bytes[index] * 100;
        }
    }
}
