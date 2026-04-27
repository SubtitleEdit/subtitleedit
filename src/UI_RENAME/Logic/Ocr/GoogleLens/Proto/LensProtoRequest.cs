using Google.Protobuf;
using System;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens.Proto;

// Simplified protobuf message classes based on the lens overlay protocol
// These are manually created to match the protocol structure

public class LensProtoRequest
{
    public byte[] ImageBytes { get; set; } = Array.Empty<byte>();
    public int Width { get; set; }
    public int Height { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = "en";
    public string Region { get; set; } = "US";
    public string TimeZone { get; set; } = "America/New_York";

    public byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new CodedOutputStream(stream);

        // Field 1: ObjectsRequest (NOT field 2!)
        writer.WriteTag(1, WireFormat.WireType.LengthDelimited);
        var objectsRequest = SerializeObjectsRequest();
        writer.WriteBytes(ByteString.CopyFrom(objectsRequest));
        
        writer.Flush();
        return stream.ToArray();
    }

    private byte[] SerializeObjectsRequest()
    {
        using var stream = new MemoryStream();
        using var writer = new CodedOutputStream(stream);

        // Field 1: RequestContext
        writer.WriteTag(1, WireFormat.WireType.LengthDelimited);
        var requestContext = SerializeRequestContext();
        writer.WriteBytes(ByteString.CopyFrom(requestContext));

        // Field 3: ImageData (NOT field 2! Field 2 is not used)
        writer.WriteTag(3, WireFormat.WireType.LengthDelimited);
        var imageData = SerializeImageData();
        writer.WriteBytes(ByteString.CopyFrom(imageData));

        writer.Flush();
        return stream.ToArray();
    }

    private byte[] SerializeRequestContext()
    {
        using var stream = new MemoryStream();
        using var writer = new CodedOutputStream(stream);

        // Field 3: RequestId (NOT field 1!)
        writer.WriteTag(3, WireFormat.WireType.LengthDelimited);
        var requestId = SerializeRequestId();
        writer.WriteBytes(ByteString.CopyFrom(requestId));

        // Field 4: ClientContext (NOT field 2!)
        writer.WriteTag(4, WireFormat.WireType.LengthDelimited);
        var clientContext = SerializeClientContext();
        writer.WriteBytes(ByteString.CopyFrom(clientContext));

        writer.Flush();
        return stream.ToArray();
    }

    private byte[] SerializeRequestId()
    {
        using var stream = new MemoryStream();
        using var writer = new CodedOutputStream(stream);

        // Field 1: UUID as uint64 (NOT string!)
        writer.WriteTag(1, WireFormat.WireType.Varint);
        
        // Parse the RequestId string as a ulong
        if (ulong.TryParse(RequestId, out var uuidValue))
        {
            writer.WriteUInt64(uuidValue);
        }
        else
        {
            // Fallback to 0 if parsing fails
            writer.WriteUInt64(0);
        }

        // Field 2: SequenceId
        writer.WriteTag(2, WireFormat.WireType.Varint);
        writer.WriteInt32(1);

        // Field 3: ImageSequenceId
        writer.WriteTag(3, WireFormat.WireType.Varint);
        writer.WriteInt32(1);

        writer.Flush();
        return stream.ToArray();
    }

    private byte[] SerializeClientContext()
    {
        using var stream = new MemoryStream();
        using var writer = new CodedOutputStream(stream);

        // Don't write Platform and Surface - they might be using defaults!
        // The JavaScript output doesn't have these fields

        // Field 4: LocaleContext
        writer.WriteTag(4, WireFormat.WireType.LengthDelimited);
        var localeContext = SerializeLocaleContext();
        writer.WriteBytes(ByteString.CopyFrom(localeContext));

        // Field 17: ClientFilters  
        writer.WriteTag(17, WireFormat.WireType.LengthDelimited);
        var clientFilters = SerializeClientFilters();
        writer.WriteBytes(ByteString.CopyFrom(clientFilters));

        writer.Flush();
        return stream.ToArray();
    }

    private byte[] SerializeLocaleContext()
    {
        using var stream = new MemoryStream();
        using var writer = new CodedOutputStream(stream);

        // Field 1: Language
        writer.WriteTag(1, WireFormat.WireType.LengthDelimited);
        writer.WriteString(TargetLanguage);

        // Field 2: Region
        writer.WriteTag(2, WireFormat.WireType.LengthDelimited);
        writer.WriteString(Region);

        // Field 3: TimeZone
        writer.WriteTag(3, WireFormat.WireType.LengthDelimited);
        writer.WriteString(TimeZone);

        writer.Flush();
        return stream.ToArray();
    }

    private byte[] SerializeClientFilters()
    {
        using var stream = new MemoryStream();
        using var writer = new CodedOutputStream(stream);

        // Field 1: Filter (repeated)
        writer.WriteTag(1, WireFormat.WireType.LengthDelimited);
        var filter = SerializeFilter();
        writer.WriteBytes(ByteString.CopyFrom(filter));

        writer.Flush();
        return stream.ToArray();
    }

    private byte[] SerializeFilter()
    {
        using var stream = new MemoryStream();
        using var writer = new CodedOutputStream(stream);

        // Field 1: FilterType (AUTO_FILTER = 7, not 1!)
        writer.WriteTag(1, WireFormat.WireType.Varint);
        writer.WriteInt32(7);

        writer.Flush();
        return stream.ToArray();
    }

    private byte[] SerializeImageData()
    {
        using var stream = new MemoryStream();
        using var writer = new CodedOutputStream(stream);

        // Field 1: Payload
        writer.WriteTag(1, WireFormat.WireType.LengthDelimited);
        var payload = SerializeImagePayload();
        writer.WriteBytes(ByteString.CopyFrom(payload));

        // Field 3: ImageMetadata (NOT field 2! Field 2 might be blobstoreData)
        writer.WriteTag(3, WireFormat.WireType.LengthDelimited);
        var metadata = SerializeImageMetadata();
        writer.WriteBytes(ByteString.CopyFrom(metadata));

        writer.Flush();
        return stream.ToArray();
    }

    private byte[] SerializeImagePayload()
    {
        using var stream = new MemoryStream();
        using var writer = new CodedOutputStream(stream);

        // Field 1: ImageBytes
        writer.WriteTag(1, WireFormat.WireType.LengthDelimited);
        writer.WriteBytes(ByteString.CopyFrom(ImageBytes));

        writer.Flush();
        return stream.ToArray();
    }

    private byte[] SerializeImageMetadata()
    {
        using var stream = new MemoryStream();
        using var writer = new CodedOutputStream(stream);

        // Field 1: Width
        writer.WriteTag(1, WireFormat.WireType.Varint);
        writer.WriteInt32(Width);

        // Field 2: Height
        writer.WriteTag(2, WireFormat.WireType.Varint);
        writer.WriteInt32(Height);

        writer.Flush();
        return stream.ToArray();
    }
}
