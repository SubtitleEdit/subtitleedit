namespace Nikse.SubtitleEdit.Logic.ContainerFormats.Matroska
{
    internal class MatroskaTrackInfo
    {
        public int TrackNumber { get; set; }
        public string Uid { get; set; }
        public bool IsVideo { get; set; }
        public bool IsAudio { get; set; }
        public bool IsSubtitle { get; set; }
        public string CodecId { get; set; }
        public string CodecPrivate { get; set; }
        public int DefaultDuration { get; set; }
        public string Language { get; set; }

        public string Name { get; set; }
        public int ContentCompressionAlgorithm { get; set; }
        public int ContentEncodingType { get; set; }
    }
}
