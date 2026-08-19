using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4
{
    /// <summary>
    /// The DVD color lookup table of a VobSub track in an MP4 (handler "subp"). MP4Box puts
    /// the 16 palette entries of the source idx file in the decoder specific info of the
    /// "mp4s" sample entry, as the DVD stores them: 4 bytes per entry, (0, Y, Cr, Cb).
    /// </summary>
    public static class Mp4VobSubPalette
    {
        private const int PaletteEntries = 16;

        /// <summary>
        /// Palette from an "mp4s" sample entry payload (everything after the 8-byte box
        /// header), or null when the entry carries no usable palette.
        /// </summary>
        public static List<SKColor> FromMp4sSampleEntry(byte[] payload)
        {
            if (payload == null)
            {
                return null;
            }

            var index = 8; // 6 reserved bytes + 2 bytes data reference index
            while (index + 8 <= payload.Length)
            {
                var boxSize = (long)BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(index, 4));
                if (boxSize < 8 || index + boxSize > payload.Length)
                {
                    return null;
                }

                if (Encoding.ASCII.GetString(payload, index + 4, 4) == "esds" && boxSize > 12 &&
                    TryFindDecoderSpecificInfo(payload, index + 12, index + (int)boxSize, out var offset, out var length))
                {
                    return FromDecoderSpecificInfo(payload, offset, length);
                }

                index += (int)boxSize;
            }

            return null;
        }

        /// <summary>
        /// Walks the MPEG-4 descriptors of an esds box down to the DecoderSpecificInfo (tag 5).
        /// </summary>
        private static bool TryFindDecoderSpecificInfo(byte[] data, int index, int end, out int dsiOffset, out int dsiLength)
        {
            dsiOffset = 0;
            dsiLength = 0;
            while (index < end)
            {
                var tag = data[index];
                index++;

                var length = 0;
                var sizeByte = 0x80;
                for (var i = 0; i < 4 && (sizeByte & 0x80) != 0 && index < end; i++)
                {
                    sizeByte = data[index];
                    index++;
                    length = (length << 7) | (sizeByte & 0x7F);
                }

                if (length < 0 || index + length > end)
                {
                    return false;
                }

                switch (tag)
                {
                    case 0x03: // ES_Descriptor - descend past ES_ID and the optional fields its flags announce
                        if (index + 3 > end)
                        {
                            return false;
                        }

                        var flags = data[index + 2];
                        index += 3;
                        if ((flags & 0x80) != 0)
                        {
                            index += 2; // dependsOn_ES_ID
                        }

                        if ((flags & 0x40) != 0)
                        {
                            if (index >= end)
                            {
                                return false;
                            }

                            index += 1 + data[index]; // URL
                        }

                        if ((flags & 0x20) != 0)
                        {
                            index += 2; // OCR_ES_ID
                        }

                        break;
                    case 0x04: // DecoderConfigDescriptor - descend past the fixed 13 byte header
                        index += 13;
                        break;
                    case 0x05: // DecoderSpecificInfo
                        dsiOffset = index;
                        dsiLength = length;
                        return true;
                    default:
                        index += length;
                        break;
                }
            }

            return false;
        }

        private static List<SKColor> FromDecoderSpecificInfo(byte[] data, int offset, int length)
        {
            if (length < PaletteEntries * 4)
            {
                return null;
            }

            var palette = new List<SKColor>(PaletteEntries);
            for (var i = 0; i < PaletteEntries; i++)
            {
                var entry = offset + i * 4;
                var rgb = BluRaySupPalette.YCbCr2Rgb(data[entry + 1], data[entry + 3], data[entry + 2], true);
                palette.Add(new SKColor((byte)rgb[0], (byte)rgb[1], (byte)rgb[2]));
            }

            return palette;
        }
    }
}
