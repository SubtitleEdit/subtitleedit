namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// UniPac
    /// </summary>
    public class PacUnicode : Pac
    {
        public override string Extension => ".fpc";

        public override string Name => "PAC Unicode (UniPac)";

        public override bool IsFPC => true;
    }
}
