namespace Nikse.SubtitleEdit.Core.Cea608
{
    public class PacData: SerializedPenState
    {
        public  int Row { get; set; }
        public string Color { get; set; }
        public int? Indent { get; set; }
    }
}
