using System;

namespace Nikse.SubtitleEdit.Core.CDG
{
    public class Packet
    {
        public Packet(byte[] data)
        {
            Command = (Command)(data[0] & 0x3F);
            Instruction = (Instruction)(data[1] & 0x3F);
            Array.Copy(data, 2, ParityQ, 0, 2);
            Array.Copy(data, 4, Data, 0, 16);
            Array.Copy(data, 20, ParityP, 0, 4);
        }

        public Command Command { get; }
        public Instruction Instruction { get; }
        public byte[] ParityQ { get; } = new byte[2];
        public byte[] Data { get; } = new byte[16];
        public byte[] ParityP { get; } = new byte[4];
    }
}