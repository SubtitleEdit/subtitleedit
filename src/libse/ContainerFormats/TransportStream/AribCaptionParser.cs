using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    /// <summary>
    /// Parses ARIB STD-B24 caption data (Japanese/Brazilian ISDB broadcast closed captions) from
    /// synchronized PES packets - the data group / caption statement layer of STD-B24 part 3 chapter 9.
    /// Caption text decoding is done by <see cref="AribB24Decoder"/>.
    /// </summary>
    public class AribCaptionParser
    {
        /// <summary>Paragraphs per language index (0-7); times are unadjusted PTS milliseconds</summary>
        public SortedDictionary<int, List<Paragraph>> ParagraphsByLanguage { get; } = new SortedDictionary<int, List<Paragraph>>();

        /// <summary>ISO 639-2 language code per language index, from caption management data</summary>
        public Dictionary<int, string> LanguageCodes { get; } = new Dictionary<int, string>();

        private AribB24Decoder _decoder;
        private AribB24Decoder.AribProfile _profile;
        private readonly Dictionary<int, Paragraph> _openParagraphs = new Dictionary<int, Paragraph>();
        private int _activeDataGroupSet; // 0x00 = group A, 0x20 = group B - statements from the inactive set are duplicates

        public AribCaptionParser(AribB24Decoder.AribProfile profile)
        {
            _profile = profile;
            _decoder = new AribB24Decoder(profile);
        }

        /// <summary>
        /// True if the PES payload looks like an ARIB synchronized/asynchronous PES for caption data:
        /// data_identifier 0x80 (captions) or 0x81 (superimpose) followed by private_stream_id 0xFF.
        /// </summary>
        public static bool IsAribCaptionPayload(byte[] payload)
        {
            return payload != null &&
                   payload.Length > 8 &&
                   (payload[0] == 0x80 || payload[0] == 0x81) &&
                   payload[1] == 0xff;
        }

        /// <summary>
        /// Parse one caption PES payload (starting at data_identifier).
        /// </summary>
        /// <param name="payload">PES data starting with data_identifier</param>
        /// <param name="ptsMilliseconds">Presentation timestamp of the PES packet in milliseconds</param>
        public void ParsePesPayload(byte[] payload, double ptsMilliseconds)
        {
            if (!IsAribCaptionPayload(payload))
            {
                return;
            }

            var index = 3 + (payload[2] & 0x0f); // data_identifier, private_stream_id, PES_data_packet_header_length
            if (index + 5 > payload.Length)
            {
                return;
            }

            var dataGroupId = payload[index] >> 2;
            var dataGroupSize = (payload[index + 3] << 8) | payload[index + 4];
            index += 5;
            var dataGroupEnd = index + dataGroupSize;
            if (dataGroupEnd > payload.Length)
            {
                dataGroupEnd = payload.Length;
            }

            var groupSet = dataGroupId & 0x20;
            var groupType = dataGroupId & 0x0f;
            if (groupType == 0)
            {
                ParseCaptionManagementData(payload, index, dataGroupEnd, groupSet);
            }
            else if (groupType <= 8 && groupSet == _activeDataGroupSet)
            {
                ParseCaptionStatementData(payload, index, dataGroupEnd, groupType - 1, ptsMilliseconds);
            }
        }

        /// <summary>
        /// Close any still-open paragraphs - call after the last PES payload has been parsed.
        /// </summary>
        public void Flush()
        {
            foreach (var languageIndex in new List<int>(_openParagraphs.Keys))
            {
                var paragraph = _openParagraphs[languageIndex];
                CloseParagraph(languageIndex, paragraph.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds);
            }

            foreach (var list in ParagraphsByLanguage.Values)
            {
                MergeContinuations(list);
            }
        }

        /// <summary>
        /// Roll-up/paint-on captions (common in Brazilian live broadcasts) repaint the screen for
        /// every added character or word - collapse such statement chains into one paragraph
        /// holding the final text.
        /// </summary>
        private static void MergeContinuations(List<Paragraph> paragraphs)
        {
            for (var i = paragraphs.Count - 1; i > 0; i--)
            {
                var previous = paragraphs[i - 1];
                var current = paragraphs[i];
                if (current.Text.StartsWith(previous.Text, StringComparison.Ordinal) &&
                    Math.Abs(current.StartTime.TotalMilliseconds - previous.EndTime.TotalMilliseconds) < 500)
                {
                    previous.Text = current.Text;
                    previous.EndTime.TotalMilliseconds = current.EndTime.TotalMilliseconds;
                    paragraphs.RemoveAt(i);
                }
            }
        }

        private void ParseCaptionManagementData(byte[] payload, int index, int end, int groupSet)
        {
            _activeDataGroupSet = groupSet;

            if (index >= end)
            {
                return;
            }

            var timeControlMode = payload[index] >> 6;
            index++;
            if (timeControlMode == 0b10)
            {
                index += 5; // OTM - offset time
            }

            if (index >= end)
            {
                return;
            }

            var numberOfLanguages = payload[index];
            index++;
            for (var i = 0; i < numberOfLanguages && index < end; i++)
            {
                var languageTag = payload[index] >> 5;
                var displayMode = payload[index] & 0x0f;
                index++;
                if (displayMode == 0b1100 || displayMode == 0b1101 || displayMode == 0b1110)
                {
                    index++; // DC - display condition designation
                }

                if (index + 4 > end)
                {
                    return;
                }

                var languageCode = Encoding.ASCII.GetString(payload, index, 3);
                index += 4; // ISO 639-2 language code + format byte
                LanguageCodes[languageTag] = languageCode;

                // Brazilian/Philippine ISDB uses the same data structures but Latin character sets
                if (_profile == AribB24Decoder.AribProfile.ProfileA &&
                    (languageCode == "por" || languageCode == "spa"))
                {
                    _profile = AribB24Decoder.AribProfile.Latin;
                    _decoder = new AribB24Decoder(_profile);
                }
            }
        }

        private void ParseCaptionStatementData(byte[] payload, int index, int end, int languageIndex, double ptsMilliseconds)
        {
            if (index >= end)
            {
                return;
            }

            var timeControlMode = payload[index] >> 6;
            index++;
            if (timeControlMode == 0b01 || timeControlMode == 0b10)
            {
                index += 5; // STM - presentation start time
            }

            if (index + 3 > end)
            {
                return;
            }

            var dataUnitLoopLength = (payload[index] << 16) | (payload[index + 1] << 8) | payload[index + 2];
            index += 3;
            var loopEnd = index + dataUnitLoopLength;
            if (loopEnd > end)
            {
                loopEnd = end;
            }

            _decoder.Reset();
            var text = string.Empty;
            while (index + 5 <= loopEnd)
            {
                if (payload[index] != 0x1f) // unit_separator
                {
                    break;
                }

                var dataUnitParameter = payload[index + 1];
                var dataUnitSize = (payload[index + 2] << 16) | (payload[index + 3] << 8) | payload[index + 4];
                index += 5;
                if (dataUnitParameter == 0x20) // statement body text - other units are DRCS/bitmap data
                {
                    text = _decoder.Decode(payload, index, dataUnitSize);
                }

                index += dataUnitSize;
            }

            CloseParagraph(languageIndex, ptsMilliseconds);
            if (text.Length > 0)
            {
                _openParagraphs[languageIndex] = new Paragraph(text, ptsMilliseconds, 0);
            }
        }

        private void CloseParagraph(int languageIndex, double endMilliseconds)
        {
            if (!_openParagraphs.TryGetValue(languageIndex, out var paragraph))
            {
                return;
            }

            _openParagraphs.Remove(languageIndex);

            var maxEnd = paragraph.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
            paragraph.EndTime.TotalMilliseconds = Math.Min(endMilliseconds, maxEnd);
            if (paragraph.EndTime.TotalMilliseconds <= paragraph.StartTime.TotalMilliseconds)
            {
                return; // zero/negative duration - out of order or duplicate timestamps
            }

            if (!ParagraphsByLanguage.TryGetValue(languageIndex, out var list))
            {
                list = new List<Paragraph>();
                ParagraphsByLanguage.Add(languageIndex, list);
            }

            list.Add(paragraph);
        }
    }
}
