
namespace Nikse.SubtitleEdit.Logic.VobSub
{
    public class VobSubPack
    {
        public readonly PacketizedElementaryStream PacketizedElementaryStream;
        public readonly Mpeg2Header Mpeg2Header;
        public IdxParagraph IdxLine { get; private set; }

        readonly byte[] _buffer;

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
