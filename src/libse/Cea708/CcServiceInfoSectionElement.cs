namespace Nikse.SubtitleEdit.Core.Cea708
{
    public class CcServiceInfoSectionElement
    {
        public bool CsnSize { get; set; }
        public int CaptionServiceNumber { get; set; }
        public byte[] ServiceDataByte { get; set; }

        public CcServiceInfoSectionElement()
        {
            ServiceDataByte = new byte[6];
        }
    }
}