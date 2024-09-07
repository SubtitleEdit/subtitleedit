namespace Nikse.SubtitleEdit.Core.Cea608
{
    public class CcData
    {
        public CcData(int type, int data1, int data2)
        {
            Type = type;
            Data1 = data1;
            Data2 = data2;
        }

        public int Type { get; set; }
        public int Data1 { get; set; }
        public int Data2 { get; set; }
        public ulong Time { get; set; }
    }
}
