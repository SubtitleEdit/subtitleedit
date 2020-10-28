namespace Nikse.SubtitleEdit.Core.ContainerFormats.Ebml
{
    internal class Element
    {
        public ElementId Id { get; private set; }

        public long DataPosition { get; private set; }

        public long DataSize { get; private set; }

        public Element(ElementId id, long dataPosition, long dataSize)
        {
            Id = id;
            DataPosition = dataPosition;
            DataSize = dataSize;
        }

        public long EndPosition
        {
            get
            {
                return DataPosition + DataSize;
            }
        }

        public override string ToString()
        {
            return string.Format(@"{0} ({1})", Id, DataSize);
        }
    }
}
