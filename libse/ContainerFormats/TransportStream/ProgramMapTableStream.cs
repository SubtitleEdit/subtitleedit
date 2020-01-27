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
            ElementaryPid = (data[index + 1] & Helper.B00011111) * 256 + // first 5 bytes
                            data[index + 2]; // last 8 bytes

            var esInfoLength = (data[index + 3] & Helper.B00001111) * 256 + // first 5 bytes
                               data[index + 4]; // last 8 bytes

            Descriptors = ProgramMapTableDescriptor.ReadDescriptors(data, esInfoLength, index + 5);
            Size = 5 + Descriptors.Sum(p => p.Size);
        }

        public string GetStreamTypeString()
        {
            switch (StreamType)
            {
                case 0x00:
                    return "ITU-T | ISO/IEC Reserved";
                case 0x01:
                    return "ISO/IEC 11172 Video";
                case 0x02:
                    return
                        "ITU-T Rec. H.262 | ISO/IEC 13818-2 Video or ISO/IEC 11172-2 constrained parameter video stream";
                case 0x03:
                    return "ISO/IEC 11172 Audio";
                case 0x04:
                    return "ISO/IEC 13818-3 Audio";
                case 0x05:
                    return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 private_sections";
                case 0x06:
                    return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 PES packets containing private data";
                case 0x07:
                    return "ISO/IEC 13522 MHEG";
                case 0x08:
                    return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 Annex A DSM-CC";
                case 0x09:
                    return "ITU-T Rec. H.222.1";
                case 0x0A:
                    return "ISO/IEC 13818-6 type A";
                case 0x0B:
                    return "ISO/IEC 13818-6 type B";
                case 0x0C:
                    return "ISO/IEC 13818-6 type C";
                case 0x0D:
                    return "ISO/IEC 13818-6 type D";
                case 0x0E:
                    return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 auxiliary";
                case 0x0F:
                    return "ISO/IEC 13818-7 Audio with ADTS transport syntax";
                case 0x10:
                    return "ISO/IEC 14496-2 Visual";
                case 0x11:
                    return "ISO/IEC 14496-3 Audio with the LATM transport syntax as defined in ISO/IEC 14496-3 / AMD 1";
                case 0x12:
                    return "ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in PES packets";
                case 0x13:
                    return "ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in ISO/IEC14496_sections";
                case 0x14:
                    return "ISO/IEC 13818-6 Synchronized Download Protocol";
                case int n when n > 0x15 && n <= 0x7F:
                    return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 Reserved";
                default:
                    return "User Private";
            }
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