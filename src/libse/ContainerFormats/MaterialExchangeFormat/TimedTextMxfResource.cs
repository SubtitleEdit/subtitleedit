using System;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat
{
    /// <summary>
    /// An ancillary resource (font or PNG image) of a SMPTE ST 429-5 timed text track file. The
    /// XML refers to it by "urn:uuid:" + <see cref="Id"/> (LoadFont / Image).
    /// </summary>
    public class TimedTextMxfResource
    {
        public const string MimeTypeFont = "application/x-font-opentype";
        public const string MimeTypePng = "image/png";

        public Guid Id { get; set; }
        public string MimeType { get; set; }
        public byte[] Data { get; set; }

        public TimedTextMxfResource(Guid id, string mimeType, byte[] data)
        {
            Id = id;
            MimeType = mimeType;
            Data = data;
        }
    }
}
