using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class ProgramMapTableDescriptor
    {
        public const int TagCaDescriptor = 9;

        public int Tag { get; set; }
        public byte[] CaSystemId { get; set; } = new byte[2];
        public uint CaPid { get; set; }
        public byte[] PrivateDataBytes { get; set; }
        public byte[] Content { get; set; }
        public string ContentAsString { get; set; }
        public int Size { get; set; }

        public ProgramMapTableDescriptor(byte[] data, int index)
        {
            Tag = data[index];
            var length = data[index + 1];
            Size = length + 2;
            if (Tag == TagCaDescriptor)
            {
                Buffer.BlockCopy(data, index + 2, CaSystemId, 0, 2);

                // 13 bytes (skip first 3)
                CaPid = (uint)((data[index + 4] & Helper.B00011111) * 256 + // first 5 bytes
                         data[index + 5]); // last 8 bytes

                PrivateDataBytes = new byte[length - 4];
                Buffer.BlockCopy(data, index + 6, PrivateDataBytes, 0, length - 4);
            }
            else
            {
                Content = new byte[length];
                if (index + 2 + length < data.Length && length > 0)
                {
                    Buffer.BlockCopy(data, index + 2, Content, 0, length);
                    ContentAsString = Encoding.UTF8.GetString(Content);
                }
            }
        }

        public static List<ProgramMapTableDescriptor> ReadDescriptors(byte[] data, int size, int index)
        {
            var total = 0;
            var descriptors = new List<ProgramMapTableDescriptor>();
            while (total < size)
            {
                var descriptor = new ProgramMapTableDescriptor(data, total + index);
                descriptors.Add(descriptor);
                total += descriptor.Size;
            }
            return descriptors;
        }
    }
}
