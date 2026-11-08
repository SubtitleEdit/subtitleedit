using System;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Track Box
    /// </summary>
    public class Trak : Box
    {
        public Mdia Mdia { get; set; }
        public Tkhd Tkhd { get; set; }
        public Edts Edts { get; set; }
        public Tref Tref { get; set; }

        /// <summary>
        /// Track title from the track's user data box (udta/name, or the QuickTime udta/©nam),
        /// e.g. "English" or "Simplified Chinese". Empty when the track has none. Streaming
        /// services often leave the mdhd language at "und" and only name the tracks.
        /// </summary>
        public string TrackName { get; private set; } = string.Empty;

        // Track titles are short; anything larger is a misread size.
        private const ulong MaxNameSize = 4096;

        public Trak(Stream fs, ulong maximumLength)
        {
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "mdia")
                {
                    Mdia = new Mdia(fs, Position);
                }
                else if (Name == "tkhd")
                {
                    Tkhd = new Tkhd(fs);
                }
                else if (Name == "edts")
                {
                    Edts = new Edts(fs, Position);
                }
                else if (Name == "tref")
                {
                    Tref = new Tref(fs, Position);
                }
                else if (Name == "udta")
                {
                    ReadUserData(fs, Position);
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }

        private void ReadUserData(Stream fs, ulong udtaEnd)
        {
            while (fs.Position < (long)udtaEnd)
            {
                if (!InitializeSizeAndName(fs))
                {
                    break;
                }

                // "©nam" starts with the byte 0xA9, which is not valid UTF-8 on its own.
                var isQuickTimeName = Buffer[4] == 0xA9 && Buffer[5] == (byte)'n' && Buffer[6] == (byte)'a' && Buffer[7] == (byte)'m';
                var payloadSize = Size >= 8 ? Size - 8 : 0;
                if ((Name == "name" || isQuickTimeName) && payloadSize > 0 && payloadSize <= MaxNameSize)
                {
                    var payload = new byte[payloadSize];
                    if (fs.Read(payload, 0, payload.Length) == payload.Length)
                    {
                        var name = isQuickTimeName ? ReadQuickTimeString(payload) : ReadPlainString(payload, 0);
                        // "name" (the ISO/ffmpeg one) wins over "©nam" when both are present.
                        if (!string.IsNullOrWhiteSpace(name) && (Name == "name" || TrackName.Length == 0))
                        {
                            TrackName = name.Trim();
                        }
                    }
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }

            Position = udtaEnd;
        }

        /// <summary>
        /// QuickTime international text: 16-bit length, 16-bit language, text. Some muxers
        /// write the iTunes form instead - a "data" box with 8 bytes of type/locale first.
        /// </summary>
        private static string ReadQuickTimeString(byte[] payload)
        {
            if (payload.Length >= 16 && payload[4] == 'd' && payload[5] == 'a' && payload[6] == 't' && payload[7] == 'a')
            {
                return ReadPlainString(payload, 16);
            }

            if (payload.Length < 4)
            {
                return string.Empty;
            }

            var length = (payload[0] << 8) | payload[1];
            return Encoding.UTF8.GetString(payload, 4, Math.Min(length, payload.Length - 4)).TrimEnd('\0');
        }

        private static string ReadPlainString(byte[] payload, int index)
        {
            var end = Array.IndexOf(payload, (byte)0, index);
            if (end < 0)
            {
                end = payload.Length;
            }

            return Encoding.UTF8.GetString(payload, index, end - index);
        }
    }
}
