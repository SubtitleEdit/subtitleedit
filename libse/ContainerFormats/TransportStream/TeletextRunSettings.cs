using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class TeletextRunSettings
    {
        private readonly Dictionary<int, ulong> _lastTimestamp = new Dictionary<int, ulong>();
        private readonly Dictionary<int, ulong> _addTimestamp = new Dictionary<int, ulong>();

        public Dictionary<int, Paragraph> PageNumberAndParagraph { get; }
        public ulong StartMs { get; }

        public TeletextRunSettings(ulong? startMs)
        {
            if (startMs.HasValue)
            {
                StartMs = startMs.Value;
            }
            PageNumberAndParagraph = new Dictionary<int, Paragraph>();
        }

        public ulong GetLastTimestamp(int pageNumber)
        {
            if (_lastTimestamp.ContainsKey(pageNumber))
            {
                return _lastTimestamp[pageNumber];
            }
            return 0;
        }

        public void SetLastTimestamp(int pageNumber, ulong timestamp)
        {
            if (_lastTimestamp.ContainsKey(pageNumber))
            {
                _lastTimestamp[pageNumber] = timestamp;
            }
            else
            {
                _lastTimestamp.Add(pageNumber, timestamp);
            }
        }

        internal void SetAddTimestamp(int pageNumber, ulong timestamp)
        {
            if (_addTimestamp.ContainsKey(pageNumber))
            {
                _addTimestamp[pageNumber] = timestamp;
            }
            else
            {
                _addTimestamp.Add(pageNumber, timestamp);
            }
        }

        internal ulong GetAddTimestamp(int pageNumber)
        {
            if (_addTimestamp.ContainsKey(pageNumber))
            {
                return _addTimestamp[pageNumber];
            }
            return 0;
        }

        private bool? _subtractStartMs;

        public ulong SubtractStartMs(ulong timestamp)
        {
            if (!_subtractStartMs.HasValue)
            {
                _subtractStartMs = StartMs > 1000 && (timestamp >= StartMs || timestamp < StartMs && timestamp + 500 > StartMs);
            }
            if (_subtractStartMs.Value)
            {
                if (StartMs <= timestamp)
                {
                    return timestamp - StartMs;
                }
                return 40;
            }
            return timestamp;
        }

    }
}
