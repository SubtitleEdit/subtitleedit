using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Cea708
{
    /// <summary>
    /// Turns a stream of CEA-708 cc_data triplets (cc_type 3 = DTVCC packet start, cc_type 2 =
    /// packet data) into paragraphs per caption service. Triplets must be added in presentation
    /// order; each packet is stamped with the time of its PACKET_START triplet.
    /// </summary>
    public class DtvccServiceDecoder
    {
        private sealed class ServiceState
        {
            public CommandState State { get; } = new CommandState();
            public List<double> PacketTimesMs { get; } = new List<double>();
            public List<Paragraph> Paragraphs { get; } = new List<Paragraph>();
        }

        private readonly SortedDictionary<int, ServiceState> _services = new SortedDictionary<int, ServiceState>();
        private readonly List<byte> _packetContent = new List<byte>();
        private byte _packetHeader;
        private double _packetTimeMs;
        private bool _hasPacket;

        /// <summary>
        /// Service number to decode, or null for all services (1..63).
        /// </summary>
        public int? OnlyService { get; set; }

        public void Add(int ccType, int data1, int data2, double timeMs)
        {
            if (ccType == 3)
            {
                DecodeCurrentPacket();
                _hasPacket = true;
                _packetHeader = (byte)data1;
                _packetTimeMs = timeMs;
                _packetContent.Clear();
                _packetContent.Add((byte)data2);
            }
            else if (ccType == 2 && _hasPacket)
            {
                _packetContent.Add((byte)data1);
                _packetContent.Add((byte)data2);
            }
        }

        /// <summary>
        /// Decodes the last packet and flushes text still buffered (no terminating display command).
        /// </summary>
        /// <returns>Paragraphs per service number (only services that produced text)</returns>
        public SortedDictionary<int, List<Paragraph>> Finish()
        {
            DecodeCurrentPacket();
            _hasPacket = false;

            var result = new SortedDictionary<int, List<Paragraph>>();
            foreach (var service in _services)
            {
                var s = service.Value;
                if (s.PacketTimesMs.Count > 0)
                {
                    var tailText = Cea708.Decode(s.PacketTimesMs.Count, Array.Empty<byte>(), s.State, flush: true);
                    Emit(s, tailText, s.PacketTimesMs[s.PacketTimesMs.Count - 1]);
                }

                if (s.Paragraphs.Count > 0)
                {
                    result.Add(service.Key, s.Paragraphs);
                }
            }

            return result;
        }

        private void DecodeCurrentPacket()
        {
            if (!_hasPacket)
            {
                return;
            }

            foreach (var block in ExtractServiceBlocks(_packetHeader, _packetContent))
            {
                if (OnlyService.HasValue && block.Key != OnlyService.Value)
                {
                    continue;
                }

                if (!_services.TryGetValue(block.Key, out var s))
                {
                    s = new ServiceState();
                    _services.Add(block.Key, s);
                }

                s.PacketTimesMs.Add(_packetTimeMs);
                var text = Cea708.Decode(s.PacketTimesMs.Count - 1, block.Value.ToArray(), s.State, flush: false);
                Emit(s, text, _packetTimeMs);
            }

            _hasPacket = false;
        }

        private static void Emit(ServiceState s, string text, double endMs)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            // state.StartLineIndex is set by Cea708.FlushText to the lineIndex of the first
            // SetText command that contributed to the just-emitted caption. Clamp defensively in
            // case the index isn't valid (e.g., flush with empty state).
            var times = s.PacketTimesMs;
            var startIndex = s.State.StartLineIndex >= 0 && s.State.StartLineIndex < times.Count
                ? s.State.StartLineIndex
                : times.Count - 1;
            s.Paragraphs.Add(new Paragraph(text.Trim(), times[startIndex], endMs));
        }

        /// <summary>
        /// Walks a DTVCC packet's service blocks and returns the bytes per service number.
        /// Service block header byte: bits 7-5 = service_number, bits 4-0 = block_size. If
        /// service_number == 7, an extended_service_number byte follows. service_number == 0 with
        /// block_size == 0 marks the end of the packet (NULL service block / padding).
        /// </summary>
        private static SortedDictionary<int, List<byte>> ExtractServiceBlocks(byte packetHeader, List<byte> content)
        {
            var result = new SortedDictionary<int, List<byte>>();

            // packet_size_code in the low 6 bits of the header: 0 → 128 bytes, n → n*2 bytes
            // (TOTAL packet length including the header). Clamp to what we actually have so a
            // malformed truncated packet doesn't walk past the buffer.
            var sizeCode = packetHeader & 0x3F;
            var declaredPacketBytes = sizeCode == 0 ? 128 : sizeCode * 2;
            var limit = Math.Min(content.Count, declaredPacketBytes - 1); // minus 1-byte packet header

            var i = 0;
            while (i < limit)
            {
                var header = content[i++];
                var serviceNumber = (header >> 5) & 0x07;
                var blockSize = header & 0x1F;

                if (serviceNumber == 0 && blockSize == 0)
                {
                    break;
                }

                if (serviceNumber == 7)
                {
                    if (i >= limit)
                    {
                        break;
                    }

                    serviceNumber = content[i++] & 0x3F;
                }

                if (i + blockSize > limit)
                {
                    break; // malformed: declared block runs past packet
                }

                if (serviceNumber > 0 && blockSize > 0)
                {
                    if (!result.TryGetValue(serviceNumber, out var bytes))
                    {
                        bytes = new List<byte>();
                        result.Add(serviceNumber, bytes);
                    }

                    for (var j = 0; j < blockSize; j++)
                    {
                        bytes.Add(content[i + j]);
                    }
                }

                i += blockSize;
            }

            return result;
        }
    }
}
