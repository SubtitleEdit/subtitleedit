using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class ProgramMapTableStream
    {
        public const int StreamTypePrivateData = 6;
        public List<ProgramMapTableDescriptor> Descriptors;
        public int StreamType { get; set; }
        public int ElementaryPid { get; set; }
        public int Size { get; set; }

        public ProgramMapTableStream(byte[] data, int index)
        {
            StreamType = data[index];

            // 13 bytes (skip first 3)
            ElementaryPid = (data[index + 1] & 0b00011111) * 256 + // first 5 bytes
                            data[index + 2]; // last 8 bytes

            var esInfoLength = (data[index + 3] & 0b00001111) * 256 + // first 5 bytes
                               data[index + 4]; // last 8 bytes

            Descriptors = ProgramMapTableDescriptor.ReadDescriptors(data, esInfoLength, index + 5);
            Size = 5 + Descriptors.Sum(p => p.Size);
        }

        internal string GetLanguage()
        {
            foreach (var descriptor in Descriptors)
            {
                if (descriptor.Tag != ProgramMapTableDescriptor.TagCaDescriptor)
                {
                    var sb = new StringBuilder();
                    foreach (var b in descriptor.Content)
                    {
                        if (b >= 32)
                        {
                            sb.Append(Convert.ToChar(b));
                        }
                        else
                        {
                            break;
                        }
                    }
                    var l = sb.ToString().TrimEnd(' ', '$', '(', ')');
                    return l.Length > 3 ? l.Substring(0, 3) : l;
                }
            }
            return string.Empty;
        }
    }
}