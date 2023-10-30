namespace Nikse.SubtitleEdit.Core.Cea608
{
    public class StyledUnicodeChar
    {
        public PenState PenState { get; set; }

        public string Uchar { get; set; } = Constants.EmptyChar;
        public string Foreground { get; set; }
        public bool? Underline { get; set; }
        public bool? Italics { get; set; }
        public string Background { get; set; }
        public bool? Flash { get; set; }

        public StyledUnicodeChar()
        {
            PenState = new PenState(null, false, false, null, false);
        }

        public void Reset()
        {
            Uchar = Constants.EmptyChar;
            PenState.Reset();
        }

        public void SetChar(string uchar, PenState newPenState)
        {
            Uchar = uchar;
            PenState.Copy(newPenState);
        }

        public void SetPenState(PenState newPenState)
        {
            PenState.Copy(newPenState);
        }

        public bool Equals(StyledUnicodeChar other)
        {
            return Uchar == other.Uchar && PenState.Equals(other.PenState);
        }

        public void Copy(StyledUnicodeChar newChar)
        {
            Uchar = newChar.Uchar;
            PenState.Copy(newChar.PenState);
        }

        public bool IsEmpty()
        {
            return Uchar == Constants.EmptyChar && PenState.IsDefault();
        }
    }
}
