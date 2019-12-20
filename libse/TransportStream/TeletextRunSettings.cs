using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.TransportStream
{
    public class TeletextRunSettings
    {
        public Dictionary<int, Paragraph> PageNumberAndParagraph { get; set; } = new Dictionary<int, Paragraph>();

        public ulong StartMs { get; set; }
        public bool SubtractStartMs { get; private set; }
        private bool _startMsInitialized;

        private readonly Dictionary<int, ulong> _lastTimestamp = new Dictionary<int, ulong>();
        private readonly Dictionary<int, ulong> _addTimestamp = new Dictionary<int, ulong>();

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

        public void InitializeStartMs(ulong firstTimestamp)
        {
            if (_startMsInitialized)
            {
                return;
            }

            if (StartMs > 1000 && (firstTimestamp >= StartMs || firstTimestamp < StartMs && firstTimestamp + 500 > StartMs))
            {
                SubtractStartMs = true;
            }
            _startMsInitialized = true;
        }
    }
}
