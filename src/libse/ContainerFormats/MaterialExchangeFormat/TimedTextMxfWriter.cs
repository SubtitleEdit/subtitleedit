using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat
{
    /// <summary>
    /// Writes a SMPTE ST 429-5 timed text track file: a D-Cinema SMPTE subtitle XML (ST 428-7)
    /// clip-wrapped in OP-Atom MXF, each font/PNG it references in its own generic stream
    /// partition. This is the subtitle asset of a SMPTE DCP. The layout follows asdcplib (the
    /// reference implementation DCP tools and verifiers are built on) byte for byte, except
    /// for the identification strings and the random instance ids.
    /// </summary>
    public class TimedTextMxfWriter
    {
        /// <summary>
        /// The MXF asset id - the id the DCP's CPL and PKL refer to the track file by. It must not
        /// be the subtitle XML's own Id.
        /// </summary>
        public Guid AssetId { get; set; } = Guid.NewGuid();

        public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The track's duration in edit units. 0 = from StartTime to the latest TimeOut, like
        /// asdcplib - but in a DCP every asset of a reel must have the reel's duration (DCP-o-matic's
        /// verifier rejects a shorter subtitle track), so set it to the picture's length.
        /// </summary>
        public long ContainerDuration { get; set; }

        public List<TimedTextMxfResource> Resources { get; } = new List<TimedTextMxfResource>();

        /// <summary>
        /// Generates the instance ids of the metadata sets - replaceable for reproducible output in tests.
        /// </summary>
        public Func<Guid> NewInstanceId { get; set; } = Guid.NewGuid;

        public string CompanyName { get; set; } = "Nikse";
        public string ProductName { get; set; } = "Subtitle Edit";

        private const uint IndexSid = 129;
        private const uint BodySid = 1;
        private const uint FirstResourceStreamId = 10;

        private static readonly byte[] OperationalPatternAtom = Hex("060e2b34040101020d01020110000000");
        private static readonly byte[] OperationalPatternGenericStream = Hex("060e2b34040101010d01020101010100");
        private static readonly byte[] EssenceContainerMultiple = Hex("060e2b34040101030d010301027f0100");
        private static readonly byte[] EssenceContainerTimedText = Hex("060e2b340401010a0d01030102130101");
        private static readonly byte[] DataDefinitionTimecode = Hex("060e2b34040101010103020101000000");
        private static readonly byte[] DataDefinitionData = Hex("060e2b34040101010103020203000000");
        private static readonly byte[] TimedTextEssenceKey = Hex("060e2b34010201010d01030117010b01");
        private static readonly byte[] GenericStreamDataKey = Hex("060e2b340101010c0d01050901000000");
        private static readonly byte[] FillKey = Hex("060e2b34010101020301021001000000");
        private static readonly byte[] PrimerKey = Hex("060e2b34020501010d01020101050100");
        private static readonly byte[] IndexTableSegmentKey = Hex("060e2b34025301010d01020101100100");
        private static readonly byte[] RandomIndexPackKey = Hex("060e2b34020501010d01020101110100");
        private static readonly byte[] UmidPrefix = Hex("060a2b340101010501010f2013000000");

        // Local tag -> UL, all static so the primer is always the same; unused entries are allowed
        private static readonly (ushort Tag, string Ul)[] PrimerEntries =
        {
            (0x0201, "060e2b34010101020407010000000000"),
            (0x0202, "060e2b34010101020702020101030000"),
            (0x1001, "060e2b34010101020601010406090000"),
            (0x1101, "060e2b34010101020601010301000000"),
            (0x1102, "060e2b34010101020601010302000000"),
            (0x1201, "060e2b34010101020702010301040000"),
            (0x1501, "060e2b34010101020702010301050000"),
            (0x1502, "060e2b34010101020404010102060000"),
            (0x1503, "060e2b34010101010404010105000000"),
            (0x1901, "060e2b34010101020601010405010000"),
            (0x1902, "060e2b34010101020601010405020000"),
            (0x2701, "060e2b34010101020601010601000000"),
            (0x3001, "060e2b34010101010406010100000000"),
            (0x3002, "060e2b34010101010406010200000000"),
            (0x3004, "060e2b34010101020601010401020000"),
            (0x3006, "060e2b34010101050601010305000000"),
            (0x3b02, "060e2b34010101020702011002040000"),
            (0x3b03, "060e2b34010101020601010402010000"),
            (0x3b05, "060e2b34010101020301020105000000"),
            (0x3b06, "060e2b34010101020601010406040000"),
            (0x3b07, "060e2b34010101020301020104000000"),
            (0x3b08, "060e2b34010101040601010401080000"),
            (0x3b09, "060e2b34010101050102020300000000"),
            (0x3b0a, "060e2b34010101050102021002010000"),
            (0x3b0b, "060e2b34010101050102021002020000"),
            (0x3c01, "060e2b34010101020520070102010000"),
            (0x3c02, "060e2b34010101020520070103010000"),
            (0x3c03, "060e2b34010101020520070104000000"),
            (0x3c04, "060e2b34010101020520070105010000"),
            (0x3c05, "060e2b34010101020520070107000000"),
            (0x3c06, "060e2b34010101020702011002030000"),
            (0x3c07, "060e2b3401010102052007010a000000"),
            (0x3c08, "060e2b34010101020520070106010000"),
            (0x3c09, "060e2b34010101020520070101000000"),
            (0x3c0a, "060e2b34010101010101150200000000"),
            (0x3e01, "060e2b34010101030403030200000000"),
            (0x3f05, "060e2b34010101040406020100000000"),
            (0x3f06, "060e2b34010101040103040500000000"),
            (0x3f07, "060e2b34010101040103040400000000"),
            (0x3f08, "060e2b34010101040404040101000000"),
            (0x3f09, "060e2b34010101050404040106000000"),
            (0x3f0a, "060e2b34010101050404040205000000"),
            (0x3f0b, "060e2b34010101050530040600000000"),
            (0x3f0c, "060e2b340101010507020103010a0000"),
            (0x3f0d, "060e2b34010101050702020101020000"),
            (0x3f0e, "060e2b34010101050404040107000000"),
            (0x4401, "060e2b34010101010101151000000000"),
            (0x4402, "060e2b34010101010103030201000000"),
            (0x4403, "060e2b34010101020601010406050000"),
            (0x4404, "060e2b34010101020702011002050000"),
            (0x4405, "060e2b34010101020702011001030000"),
            (0x4701, "060e2b34010101020601010402030000"),
            (0x4801, "060e2b34010101020107010100000000"),
            (0x4802, "060e2b34010101020107010201000000"),
            (0x4803, "060e2b34010101020601010402040000"),
            (0x4804, "060e2b34010101020104010300000000"),
            (0x4b01, "060e2b34010101020530040500000000"),
            (0x4b02, "060e2b34010101020702010301030000"),
            (0xfff9, "060e2b34010101070409020100000000"), // TimedTextResourceSubDescriptor MIMEMediaType
            (0xfffa, "060e2b340101010c0101151300000000"), // TimedTextResourceSubDescriptor AncillaryResourceID
            (0xfffb, "060e2b340101010e0301010202160000"), // TimedTextDescriptor RFC5646LanguageTagList
            (0xfffc, "060e2b34010101080102010501000000"), // TimedTextDescriptor NamespaceURI
            (0xfffd, "060e2b340101010c0409050000000000"), // TimedTextDescriptor UCSEncoding
            (0xfffe, "060e2b340101010c0101151200000000"), // TimedTextDescriptor ResourceID
            (0xffff, "060e2b34010101090601010406100000"), // GenericDescriptor SubDescriptors
        };

        /// <summary>
        /// The most fonts/images one track file can carry: the descriptor's SubDescriptors batch
        /// (8 + 16 bytes per resource) is a local set item, whose length is a 16-bit value.
        /// </summary>
        public const int MaxResources = (ushort.MaxValue - 8) / 16;

        public void Write(string fileName, string xml)
        {
            // Fail before the file is created/truncated
            ThrowIfTooManyResources();
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Write(fs, xml);
            }
        }

        public void Write(Stream stream, string xml)
        {
            ThrowIfTooManyResources();
            var info = TimedTextInfo.Parse(xml);
            if (ContainerDuration > 0)
            {
                info.Duration = ContainerDuration;
            }

            // A LoadFont/Image the track does not carry plays without its font or image
            var missing = info.ResourceReferences.FirstOrDefault(id => Resources.All(r => r.Id != id));
            if (missing != Guid.Empty)
            {
                throw new InvalidDataException("Timed text XML references resource urn:uuid:" + missing + ", which is not in the track file");
            }
            var xmlBytes = new UTF8Encoding(false).GetBytes(xml);

            var metadata = MakeHeaderMetadata(info);

            // asdcplib reserves 16 KiB for the header plus room per resource sub-descriptor
            long headerSize = 16384;
            if (Resources.Count > 0)
            {
                headerSize += 12 + Resources.Sum(r => 88 + 2 * r.MimeType.Length);
            }

            const int partitionPackSize = 16 + 4 + 120;
            byte[] fill = null;
            var fillLength = headerSize - partitionPackSize - metadata.Length - 20;
            if (fillLength >= 0)
            {
                fill = Klv(FillKey, new byte[fillLength]);
            }
            else
            {
                headerSize = partitionPackSize + metadata.Length;
            }

            var bodyPosition = headerSize;
            var position = bodyPosition + partitionPackSize + KlvSize(xmlBytes.Length);
            var resourcePositions = new List<long>();
            foreach (var resource in Resources)
            {
                resourcePositions.Add(position);
                position += partitionPackSize + KlvSize(resource.Data.Length);
            }

            var footerPosition = position;
            var indexSegment = MakeIndexTableSegment(info);
            var lastPartition = resourcePositions.Count > 0 ? resourcePositions[resourcePositions.Count - 1] : bodyPosition;

            WriteBytes(stream, PartitionPack(0x02, 0, 0, footerPosition, headerSize - partitionPackSize, 0, 0, 0, OperationalPatternAtom));
            WriteBytes(stream, metadata);
            if (fill != null)
            {
                WriteBytes(stream, fill);
            }

            WriteBytes(stream, PartitionPack(0x03, bodyPosition, 0, 0, 0, 0, 0, BodySid, OperationalPatternAtom));
            WriteKlv(stream, TimedTextEssenceKey, xmlBytes);

            for (var i = 0; i < Resources.Count; i++)
            {
                var previous = i == 0 ? bodyPosition : resourcePositions[i - 1];
                WriteBytes(stream, PartitionPack(0x03, resourcePositions[i], previous, 0, 0, 0, 0, FirstResourceStreamId + (uint)i, OperationalPatternGenericStream));
                WriteKlv(stream, GenericStreamDataKey, Resources[i].Data);
            }

            WriteBytes(stream, PartitionPack(0x04, footerPosition, lastPartition, footerPosition, 0, indexSegment.Length, IndexSid, 0, OperationalPatternAtom));
            WriteBytes(stream, indexSegment);

            var rip = new MemoryStream();
            WriteUInt32(rip, 0);
            WriteUInt64(rip, 0);
            WriteUInt32(rip, BodySid);
            WriteUInt64(rip, (ulong)bodyPosition);
            for (var i = 0; i < Resources.Count; i++)
            {
                WriteUInt32(rip, FirstResourceStreamId + (uint)i);
                WriteUInt64(rip, (ulong)resourcePositions[i]);
            }

            WriteUInt32(rip, 0);
            WriteUInt64(rip, (ulong)footerPosition);
            WriteUInt32(rip, (uint)(16 + 4 + rip.Length + 4));
            WriteKlv(stream, RandomIndexPackKey, rip.ToArray());
        }

        private byte[] MakeHeaderMetadata(TimedTextInfo info)
        {
            var preface = NewInstanceId();
            var identification = NewInstanceId();
            var contentStorage = NewInstanceId();
            var essenceContainerData = NewInstanceId();
            var materialPackage = NewInstanceId();
            var materialTracks = new[] { NewInstanceId(), NewInstanceId() };
            var materialSequences = new[] { NewInstanceId(), NewInstanceId() };
            var materialComponents = new[] { NewInstanceId(), NewInstanceId() };
            var sourcePackage = NewInstanceId();
            var sourceTracks = new[] { NewInstanceId(), NewInstanceId() };
            var sourceSequences = new[] { NewInstanceId(), NewInstanceId() };
            var sourceComponents = new[] { NewInstanceId(), NewInstanceId() };
            var descriptor = NewInstanceId();
            var subDescriptors = Resources.Select(_ => NewInstanceId()).ToList();
            var thisGeneration = NewInstanceId();
            var materialPackageUmid = NewInstanceId();
            var productUid = NewInstanceId();

            var now = Timestamp(ModifiedUtc);
            var version = (Assembly.GetEntryAssembly() ?? typeof(TimedTextMxfWriter).Assembly).GetName().Version ?? new Version(0, 0, 0, 0);
            var productVersion = ProductVersion(version);
            var editRate = Rational(info.EditRateNumerator, info.EditRateDenominator);
            var duration = UInt64(info.Duration);

            var ms = new MemoryStream();

            var primer = new MemoryStream();
            WriteUInt32(primer, (uint)PrimerEntries.Length);
            WriteUInt32(primer, 18);
            foreach (var entry in PrimerEntries)
            {
                WriteUInt16(primer, entry.Tag);
                WriteBytes(primer, Hex(entry.Ul));
            }

            WriteKlv(ms, PrimerKey, primer.ToArray());

            WriteSet(ms, 0x2f, preface,
                (0x3b02, now),
                (0x3b05, UInt16(0x0102)),
                (0x3b07, UInt32(1)),
                (0x3b08, Uuid(sourcePackage)),
                (0x3b06, Batch(Uuid(identification))),
                (0x3b03, Uuid(contentStorage)),
                (0x3b09, OperationalPatternAtom),
                (0x3b0a, Batch(EssenceContainerMultiple, EssenceContainerTimedText)),
                (0x3b0b, Batch(16)));

            WriteSet(ms, 0x30, identification,
                (0x3c09, Uuid(thisGeneration)),
                (0x3c01, Utf16(CompanyName)),
                (0x3c02, Utf16(ProductName)),
                (0x3c03, productVersion),
                (0x3c04, Utf16(version.ToString(3))),
                (0x3c05, Uuid(productUid)),
                (0x3c06, now),
                (0x3c07, productVersion),
                (0x3c08, Utf16("dotnet")));

            WriteSet(ms, 0x18, contentStorage,
                (0x1901, Batch(Uuid(sourcePackage), Uuid(materialPackage))),
                (0x1902, Batch(Uuid(essenceContainerData))));

            WriteSet(ms, 0x23, essenceContainerData,
                (0x2701, Umid(AssetId)),
                (0x3f06, UInt32(IndexSid)),
                (0x3f07, UInt32(BodySid)));

            WriteSet(ms, 0x36, materialPackage,
                (0x4401, Umid(materialPackageUmid)),
                (0x4402, Utf16("Material Package")),
                (0x4405, now),
                (0x4404, now),
                (0x4403, Batch(Uuid(materialTracks[0]), Uuid(materialTracks[1]))));
            WriteTracks(ms, materialTracks, materialSequences, materialComponents, editRate, duration, info.TimecodeRate, Umid(AssetId), 2);

            WriteSet(ms, 0x37, sourcePackage,
                (0x4401, Umid(AssetId)),
                (0x4402, Utf16("File Package: SMPTE 429-5 clip wrapping of D-Cinema Timed Text data")),
                (0x4405, now),
                (0x4404, now),
                (0x4403, Batch(Uuid(sourceTracks[0]), Uuid(sourceTracks[1]))),
                (0x4701, Uuid(descriptor)));
            WriteTracks(ms, sourceTracks, sourceSequences, sourceComponents, editRate, duration, info.TimecodeRate, new byte[32], 0);

            var descriptorItems = new List<(ushort, byte[])>();
            if (subDescriptors.Count > 0)
            {
                descriptorItems.Add((0xffff, Batch(subDescriptors.Select(Uuid).ToArray())));
            }

            descriptorItems.Add((0x3006, UInt32(2)));
            descriptorItems.Add((0x3001, editRate));
            descriptorItems.Add((0x3002, duration));
            descriptorItems.Add((0x3004, EssenceContainerTimedText));
            descriptorItems.Add((0x3e01, new byte[16]));
            descriptorItems.Add((0xfffe, Uuid(info.ResourceId)));
            descriptorItems.Add((0xfffd, Utf16("UTF-8")));
            descriptorItems.Add((0xfffc, Utf16(info.NamespaceUri)));
            if (!string.IsNullOrEmpty(info.Language))
            {
                descriptorItems.Add((0xfffb, Utf16(info.Language)));
            }

            WriteSet(ms, 0x64, descriptor, descriptorItems.ToArray());

            for (var i = 0; i < Resources.Count; i++)
            {
                WriteSet(ms, 0x65, subDescriptors[i],
                    (0xfffa, Uuid(Resources[i].Id)),
                    (0xfff9, Utf16(Resources[i].MimeType)),
                    (0x3f07, UInt32(FirstResourceStreamId + (uint)i)));
            }

            return ms.ToArray();
        }

        /// <summary>
        /// A package's timecode track (1) and timed text track (2), each with a sequence of one component.
        /// </summary>
        private static void WriteTracks(Stream ms, Guid[] tracks, Guid[] sequences, Guid[] components, byte[] editRate, byte[] duration, int timecodeRate, byte[] sourcePackageUmid, uint sourceTrackId)
        {
            WriteSet(ms, 0x3b, tracks[0],
                (0x4801, UInt32(1)),
                (0x4804, UInt32(0)),
                (0x4802, Utf16("Timecode Track")),
                (0x4803, Uuid(sequences[0])),
                (0x4b01, editRate),
                (0x4b02, UInt64(0)));
            WriteSet(ms, 0x0f, sequences[0],
                (0x0201, DataDefinitionTimecode),
                (0x0202, duration),
                (0x1001, Batch(Uuid(components[0]))));
            WriteSet(ms, 0x14, components[0],
                (0x0201, DataDefinitionTimecode),
                (0x0202, duration),
                (0x1502, UInt16((ushort)timecodeRate)),
                (0x1501, UInt64(0)),
                (0x1503, new byte[] { 0 }));

            WriteSet(ms, 0x3b, tracks[1],
                (0x4801, UInt32(2)),
                (0x4804, UInt32(0)),
                (0x4802, Utf16("Timed Text Track")),
                (0x4803, Uuid(sequences[1])),
                (0x4b01, editRate),
                (0x4b02, UInt64(0)));
            WriteSet(ms, 0x0f, sequences[1],
                (0x0201, DataDefinitionData),
                (0x0202, duration),
                (0x1001, Batch(Uuid(components[1]))));
            WriteSet(ms, 0x11, components[1],
                (0x0201, DataDefinitionData),
                (0x0202, duration),
                (0x1201, UInt64(0)),
                (0x1101, sourcePackageUmid),
                (0x1102, UInt32(sourceTrackId)));
        }

        /// <summary>
        /// The footer index: one edit unit (the clip-wrapped XML) at stream offset 0.
        /// </summary>
        private byte[] MakeIndexTableSegment(TimedTextInfo info)
        {
            var ms = new MemoryStream();
            WriteLocalSetItems(ms,
                (0x3c0a, Uuid(NewInstanceId())),
                (0x3f0b, Rational(info.EditRateNumerator, info.EditRateDenominator)),
                (0x3f0c, UInt64(0)),
                (0x3f0d, UInt64(1)),
                (0x3f05, UInt32(0)),
                (0x3f06, UInt32(IndexSid)),
                (0x3f07, UInt32(BodySid)),
                (0x3f08, new byte[] { 0 }),
                (0x3f0e, new byte[] { 0 }),
                (0x3f09, BatchOfSize(6, new byte[6])),
                (0x3f0a, BatchOfSize(11, new byte[] { 0, 0, 0x80, 0, 0, 0, 0, 0, 0, 0, 0 })));
            return Klv(IndexTableSegmentKey, ms.ToArray());
        }

        private static byte[] PartitionPack(byte kind, long thisPartition, long previousPartition, long footerPartition, long headerByteCount, long indexByteCount, uint indexSid, uint bodySid, byte[] operationalPattern)
        {
            var key = Hex("060e2b34020501010d01020101000400");
            key[13] = kind;
            if (operationalPattern == OperationalPatternGenericStream)
            {
                key[14] = 0x11; // generic stream partition
            }

            var ms = new MemoryStream();
            WriteUInt16(ms, 1); // major version
            WriteUInt16(ms, 2); // minor version
            WriteUInt32(ms, 1); // KAG size
            WriteUInt64(ms, (ulong)thisPartition);
            WriteUInt64(ms, (ulong)previousPartition);
            WriteUInt64(ms, (ulong)footerPartition);
            WriteUInt64(ms, (ulong)headerByteCount);
            WriteUInt64(ms, (ulong)indexByteCount);
            WriteUInt32(ms, indexSid);
            WriteUInt64(ms, 0); // body offset
            WriteUInt32(ms, bodySid);
            WriteBytes(ms, operationalPattern);
            WriteBytes(ms, Batch(EssenceContainerMultiple, EssenceContainerTimedText));
            return Klv(key, ms.ToArray());
        }

        private void ThrowIfTooManyResources()
        {
            if (Resources.Count > MaxResources)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "A D-Cinema MXF timed text track file can hold at most {0} images/fonts, this one has {1} - split the subtitle into more reels.",
                    MaxResources, Resources.Count));
            }
        }

        private static void WriteSet(Stream stream, byte setType, Guid instanceId, params (ushort Tag, byte[] Value)[] items)
        {
            var key = Hex("060e2b34025301010d01010101010000");
            key[14] = setType;
            var ms = new MemoryStream();
            WriteLocalSetItems(ms, (0x3c0a, Uuid(instanceId)));
            WriteLocalSetItems(ms, items);
            WriteKlv(stream, key, ms.ToArray());
        }

        private static void WriteLocalSetItems(Stream stream, params (ushort Tag, byte[] Value)[] items)
        {
            foreach (var item in items)
            {
                if (item.Value.Length > ushort.MaxValue)
                {
                    // The local set length is 16 bits - a cast would silently corrupt the header
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                        "MXF local set item 0x{0:x4} is {1} bytes, the limit is {2} (a track file can hold at most {3} images/fonts).",
                        item.Tag, item.Value.Length, ushort.MaxValue, MaxResources));
                }

                WriteUInt16(stream, item.Tag);
                WriteUInt16(stream, (ushort)item.Value.Length);
                WriteBytes(stream, item.Value);
            }
        }

        private static long KlvSize(long length) => 16 + BerLength(length).Length + length;

        private static byte[] Klv(byte[] key, byte[] value)
        {
            var ms = new MemoryStream();
            WriteKlv(ms, key, value);
            return ms.ToArray();
        }

        private static void WriteKlv(Stream stream, byte[] key, byte[] value)
        {
            WriteBytes(stream, key);
            WriteBytes(stream, BerLength(value.Length));
            WriteBytes(stream, value);
        }

        /// <summary>
        /// Four-byte BER length like asdcplib, eight bytes for values of 16 MiB or more.
        /// </summary>
        private static byte[] BerLength(long length)
        {
            if (length < 0x1000000)
            {
                return new[] { (byte)0x83, (byte)(length >> 16), (byte)(length >> 8), (byte)length };
            }

            var bytes = new byte[9];
            bytes[0] = 0x88;
            for (var i = 0; i < 8; i++)
            {
                bytes[8 - i] = (byte)(length >> (8 * i));
            }

            return bytes;
        }

        private static byte[] Batch(params byte[][] items) => BatchOfSize(16, items);

        private static byte[] Batch(int itemSize) => BatchOfSize(itemSize);

        private static byte[] BatchOfSize(int itemSize, params byte[][] items)
        {
            var ms = new MemoryStream();
            WriteUInt32(ms, (uint)items.Length);
            WriteUInt32(ms, (uint)itemSize);
            foreach (var item in items)
            {
                WriteBytes(ms, item);
            }

            return ms.ToArray();
        }

        /// <summary>
        /// UUID in RFC 4122 byte order (Guid.ToByteArray swaps the first three fields).
        /// </summary>
        private static byte[] Uuid(Guid guid) => Hex(guid.ToString("N"));

        private static byte[] Umid(Guid material)
        {
            var umid = new byte[32];
            Array.Copy(UmidPrefix, umid, 16);
            Array.Copy(Uuid(material), 0, umid, 16, 16);
            return umid;
        }

        private static byte[] Rational(int numerator, int denominator)
        {
            var bytes = new byte[8];
            Array.Copy(UInt32((uint)numerator), 0, bytes, 0, 4);
            Array.Copy(UInt32((uint)denominator), 0, bytes, 4, 4);
            return bytes;
        }

        private static byte[] Timestamp(DateTime utc)
        {
            return new[]
            {
                (byte)(utc.Year >> 8), (byte)utc.Year, (byte)utc.Month, (byte)utc.Day,
                (byte)utc.Hour, (byte)utc.Minute, (byte)utc.Second, (byte)(utc.Millisecond / 4),
            };
        }

        private static byte[] ProductVersion(Version version)
        {
            var ms = new MemoryStream();
            WriteUInt16(ms, (ushort)Math.Max(0, version.Major));
            WriteUInt16(ms, (ushort)Math.Max(0, version.Minor));
            WriteUInt16(ms, (ushort)Math.Max(0, version.Build));
            WriteUInt16(ms, (ushort)Math.Max(0, version.Revision));
            WriteUInt16(ms, 0); // release type: unknown
            return ms.ToArray();
        }

        private static byte[] Utf16(string s) => Encoding.BigEndianUnicode.GetBytes(s ?? string.Empty);

        private static byte[] UInt16(ushort value) => new[] { (byte)(value >> 8), (byte)value };

        private static byte[] UInt32(uint value) => new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };

        private static byte[] UInt64(long value) => UInt64((ulong)value);

        private static byte[] UInt64(ulong value)
        {
            var bytes = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                bytes[7 - i] = (byte)(value >> (8 * i));
            }

            return bytes;
        }

        private static void WriteUInt16(Stream stream, ushort value) => WriteBytes(stream, UInt16(value));

        private static void WriteUInt32(Stream stream, uint value) => WriteBytes(stream, UInt32(value));

        private static void WriteUInt64(Stream stream, ulong value) => WriteBytes(stream, UInt64(value));

        private static void WriteBytes(Stream stream, byte[] bytes) => stream.Write(bytes, 0, bytes.Length);

        private static byte[] Hex(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// What the descriptor needs from the subtitle XML: Id, namespace, language, edit rate,
        /// and the duration in edit units from StartTime to the latest TimeOut (like asdcplib).
        /// </summary>
        private class TimedTextInfo
        {
            public string NamespaceUri { get; private set; }
            public Guid ResourceId { get; private set; }
            public string Language { get; private set; }
            public int EditRateNumerator { get; private set; }
            public int EditRateDenominator { get; private set; }
            public int TimecodeRate { get; private set; }
            public long Duration { get; set; }
            public List<Guid> ResourceReferences { get; } = new List<Guid>();

            public static TimedTextInfo Parse(string xml)
            {
                var doc = new XmlDocument { XmlResolver = null };
                doc.LoadXml(xml);
                var root = doc.DocumentElement ?? throw new InvalidDataException("Timed text XML has no root element");
                var info = new TimedTextInfo { NamespaceUri = root.NamespaceURI };

                var id = Child(root, "Id");
                if (id == null || !Guid.TryParse(id.Trim().Replace("urn:uuid:", string.Empty), out var resourceId))
                {
                    throw new InvalidDataException("Timed text XML has no urn:uuid Id");
                }

                info.ResourceId = resourceId;
                info.Language = Child(root, "Language")?.Trim();

                var editRate = (Child(root, "EditRate") ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (editRate.Length != 2 ||
                    !int.TryParse(editRate[0], NumberStyles.None, CultureInfo.InvariantCulture, out var numerator) ||
                    !int.TryParse(editRate[1], NumberStyles.None, CultureInfo.InvariantCulture, out var denominator) ||
                    numerator <= 0 || denominator <= 0)
                {
                    throw new InvalidDataException("Timed text XML has no valid EditRate");
                }

                info.EditRateNumerator = numerator;
                info.EditRateDenominator = denominator;
                info.TimecodeRate = (int)Math.Round(numerator / (double)denominator);

                var start = FramesFromTimeCode(Child(root, "StartTime"), info.TimecodeRate);
                long end = 0;
                foreach (var element in root.GetElementsByTagName("*").OfType<XmlElement>().Where(e => e.LocalName == "Subtitle"))
                {
                    end = Math.Max(end, FramesFromTimeCode(element.GetAttribute("TimeOut"), info.TimecodeRate));
                }

                if (end <= start)
                {
                    throw new InvalidDataException("Timed text XML has no subtitles");
                }

                info.Duration = end - start;
                info.ResourceReferences.AddRange(GetResourceReferences(root, "LoadFont"));
                info.ResourceReferences.AddRange(GetResourceReferences(root, "Image"));
                return info;
            }

            public static IEnumerable<Guid> GetResourceReferences(XmlElement root, string localName)
            {
                foreach (var element in root.GetElementsByTagName("*").OfType<XmlElement>().Where(e => e.LocalName == localName))
                {
                    var text = element.InnerText.Trim();
                    if (text.StartsWith("urn:uuid:", StringComparison.OrdinalIgnoreCase) && Guid.TryParse(text.Substring(9), out var id))
                    {
                        yield return id;
                    }
                }
            }

            private static string Child(XmlElement root, string localName)
            {
                return root.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.LocalName == localName)?.InnerText;
            }

            private static long FramesFromTimeCode(string timeCode, int frameRate)
            {
                return TryParseTimeCode(timeCode, frameRate, out var frames) ? frames : 0;
            }
        }

        /// <summary>
        /// The urn:uuid ids of the fonts the XML loads (LoadFont) - the ids to embed the fonts under.
        /// </summary>
        public static List<Guid> GetLoadFontIds(string xml)
        {
            var doc = new XmlDocument { XmlResolver = null };
            doc.LoadXml(xml);
            return doc.DocumentElement == null
                ? new List<Guid>()
                : TimedTextInfo.GetResourceReferences(doc.DocumentElement, "LoadFont").Distinct().ToList();
        }

        /// <summary>
        /// HH:MM:SS:FF as a frame count at the given (integer) time code rate.
        /// </summary>
        public static bool TryParseTimeCode(string timeCode, int frameRate, out long frames)
        {
            frames = 0;
            if (frameRate <= 0)
            {
                return false;
            }

            var parts = (timeCode ?? string.Empty).Trim().Split(':');
            var values = new int[4];
            if (parts.Length != 4)
            {
                return false;
            }

            for (var i = 0; i < 4; i++)
            {
                if (!int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out values[i]))
                {
                    return false;
                }
            }

            if (values[1] >= 60 || values[2] >= 60 || values[3] >= frameRate)
            {
                return false;
            }

            frames = ((values[0] * 60L + values[1]) * 60 + values[2]) * frameRate + values[3];
            return true;
        }

        /// <summary>
        /// A duration as HH:MM:SS:FF at the given (integer) time code rate, rounded to whole frames.
        /// </summary>
        public static string ToTimeCode(TimeSpan duration, int frameRate)
        {
            return ToTimeCode((long)Math.Round(duration.TotalSeconds * frameRate), frameRate);
        }

        /// <summary>
        /// A frame count as HH:MM:SS:FF at the given (integer) time code rate.
        /// </summary>
        public static string ToTimeCode(long frames, int frameRate)
        {
            var seconds = frames / frameRate;
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}:{3:00}", seconds / 3600, seconds / 60 % 60, seconds % 60, frames % frameRate);
        }
    }
}
