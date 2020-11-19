namespace Nikse.SubtitleEdit.Core.VobSub
{
    public class VobSubPack
    {
        public PacketizedElementaryStream PacketizedElementaryStream;
        public Mpeg2Header Mpeg2Header;
        public IdxParagraph IdxLine { get; private set; }

        private readonly byte[] _buffer;

        public byte[] Buffer
        {
            get
            {
                return _buffer;
            }
        }

        public VobSubPack(byte[] buffer, IdxParagraph idxLine)
        {
            _buffer = buffer;
            IdxLine = idxLine;

            if (VobSubParser.IsMpeg2PackHeader(buffer))
            {
                Mpeg2Header = new Mpeg2Header(buffer);
                PacketizedElementaryStream = new PacketizedElementaryStream(buffer, Mpeg2Header.Length);
            }
            else if (VobSubParser.IsPrivateStream1(buffer, 0))
            {
                PacketizedElementaryStream = new PacketizedElementaryStream(buffer, 0);
            }
        }
    }
}
