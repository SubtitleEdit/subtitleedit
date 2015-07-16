namespace Nikse.SubtitleEdit.Logic.ContainerFormats.Ebml
{
    internal class Element
    {
        private readonly ElementId _id;
        private readonly long _dataPosition;
        private readonly long _dataSize;

        public Element(ElementId id, long dataPosition, long dataSize)
        {
            _id = id;
            _dataPosition = dataPosition;
            _dataSize = dataSize;
        }

        public ElementId Id
        {
            get
            {
                return _id;
            }
        }

        public long DataPosition
        {
            get
            {
                return _dataPosition;
            }
        }

        public long DataSize
        {
            get
            {
                return _dataSize;
            }
        }

        public long EndPosition
        {
            get
            {
                return _dataPosition + _dataSize;
            }
        }

        public override string ToString()
        {
            return string.Format(@"{0} ({1})", _id, _dataSize);
        }
    }
}