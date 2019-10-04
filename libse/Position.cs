namespace Nikse.SubtitleEdit.Core
{
    public class Position
    {
        public int Left { get; set; }
        public int Top { get; set; }

        public Position(int left, int top)
        {
            Left = left;
            Top = top;
        }
    }
}
