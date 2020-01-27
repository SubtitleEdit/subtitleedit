namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class EbuPesDataField
    {
        public int DataUnitId { get; set; }
        public int DataUnitLength { get; set; }
        public byte[] DataField { get; set; }
        public EbuPesDataFieldText FieldText;
    }
}
