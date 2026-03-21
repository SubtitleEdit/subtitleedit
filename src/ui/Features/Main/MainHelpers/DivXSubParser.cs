using Nikse.SubtitleEdit.Core.ContainerFormats;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

public static class DivXSubParser
{
    public static List<XSub> ImportSubtitleFromDivX(string fileName)
    {
        var list = new List<XSub>();
        byte[] buffer = new byte[4096];
        const byte TagStart = 0x5B; // '['

        using var stream = File.OpenRead(fileName);

        while (stream.Position < stream.Length - 50)
        {
            long startPosition = stream.Position;
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead < 30)
            {
                break;
            }

            ReadOnlySpan<byte> span = buffer.AsSpan(0, bytesRead);

            for (int i = 0; i <= bytesRead - 26; i++)
            {
                // Look for '[' followed by a digit and a colon (e.g., "[0:")
                if (span[i] == TagStart && IsPotentialTimecode(span.Slice(i + 1)))
                {
                    // Sync stream to the start of the actual subtitle data
                    stream.Position = startPosition + i + 1;

                    if (TryReadXSub(stream, out var xSub))
                    {
                        if (xSub != null)
                        {
                            list.Add(xSub);
                        }
                        // Move loop index forward to skip the header we just processed
                        i += 50;
                    }
                }
            }

            // To handle tags split across buffer boundaries, 
            // we back up slightly for the next read
            if (stream.Position < stream.Length)
            {
                stream.Position -= 30;
            }
        }

        return list;
    }

    private static bool IsPotentialTimecode(ReadOnlySpan<byte> s)
    {
        // Quick validation of [00:00:00.000-00:00:00.000] pattern
        return s.Length >= 25 &&
               s[2] == 0x3a && s[5] == 0x3a && s[8] == 0x2e && // : : .
               s[12] == 0x2d &&                                // -
               s[15] == 0x3a && s[18] == 0x3a && s[21] == 0x2e && // : : .
               s[25] == 0x5d;                                  // ]
    }

    private static bool TryReadXSub(FileStream stream, out XSub? xSub)
    {
        xSub = null;
        Span<byte> header = stackalloc byte[26 + 14 + 12]; // Timecode + Metadata + Colors

        if (stream.Read(header) < header.Length) return false;

        string timeCode = Encoding.ASCII.GetString(header.Slice(0, 25));

        // Use BinaryPrimitives for fast, allocation-free Little Endian parsing
        var meta = header.Slice(26);
        ushort width = BinaryPrimitives.ReadUInt16LittleEndian(meta.Slice(0, 2));
        ushort height = BinaryPrimitives.ReadUInt16LittleEndian(meta.Slice(2, 2));
        // Skipping x, y, xEnd, yEnd (8 bytes) as they weren't used in your logic
        ushort rleLength = BinaryPrimitives.ReadUInt16LittleEndian(meta.Slice(12, 2));

        var colorBuffer = header.Slice(header.Length - 12).ToArray();

        // Read the RLE Data
        var rleData = new byte[rleLength];
        if (stream.Read(rleData) != rleLength)
        {
            return false;
        }

        if (width > 0 && height > 0)
        {
            xSub = new XSub(timeCode, width, height, colorBuffer, rleData);
            return true;
        }

        return false;
    }
}
