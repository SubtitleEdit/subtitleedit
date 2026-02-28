namespace Nikse.SubtitleEdit.Logic;

public partial class FindService
{
    private class FindMatch
    {
        public int Index { get; }
        public string Value { get; }

        public FindMatch(int index, string value)
        {
            Index = index;
            Value = value;
        }
    }
}