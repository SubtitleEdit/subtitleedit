using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.AudioToText;

/// <summary>
/// Metadata read from the header of a GGUF file (https://github.com/ggml-org/ggml/blob/master/docs/gguf.md):
/// the key/value section and the tensor names. Only the header is read, never the tensor
/// data, so this is cheap even for multi-gigabyte model files. Used to tell the different
/// "Qwen3 ASR GGUF" layouts apart before a user-supplied model is copied into place.
/// </summary>
public sealed class GgufHeader
{
    private const uint GgufMagic = 0x46554747; // "GGUF" little-endian
    private const int MaxStringLength = 64 * 1024 * 1024;
    private const ulong MaxKeyValueCount = 1_000_000;
    private const ulong MaxTensorCount = 10_000_000;
    private const ulong MaxArrayCount = 100_000_000;

    private enum ValueType : uint
    {
        UInt8 = 0,
        Int8 = 1,
        UInt16 = 2,
        Int16 = 3,
        UInt32 = 4,
        Int32 = 5,
        Float32 = 6,
        Bool = 7,
        String = 8,
        Array = 9,
        UInt64 = 10,
        Int64 = 11,
        Float64 = 12,
    }

    public uint Version { get; }

    /// <summary>All metadata keys, including array-valued ones.</summary>
    public IReadOnlyCollection<string> Keys => _keys;

    /// <summary>String-valued metadata (e.g. "general.architecture").</summary>
    public IReadOnlyDictionary<string, string> StringValues => _stringValues;

    public IReadOnlyList<string> TensorNames => _tensorNames;

    public string? Architecture => _stringValues.TryGetValue("general.architecture", out var arch) ? arch : null;

    private readonly HashSet<string> _keys;
    private readonly Dictionary<string, string> _stringValues;
    private readonly List<string> _tensorNames;

    private GgufHeader(uint version, HashSet<string> keys, Dictionary<string, string> stringValues, List<string> tensorNames)
    {
        Version = version;
        _keys = keys;
        _stringValues = stringValues;
        _tensorNames = tensorNames;
    }

    public bool HasKey(string key) => _keys.Contains(key);

    /// <summary>
    /// Reads the header of the GGUF file at <paramref name="fileName"/>.
    /// Throws <see cref="InvalidDataException"/> when the file is not a GGUF file.
    /// </summary>
    public static GgufHeader Read(string fileName)
    {
        using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20);
        return Read(stream);
    }

    /// <summary>
    /// Reads a GGUF header from <paramref name="stream"/> (positioned at the file start).
    /// Throws <see cref="InvalidDataException"/> when the data is not a GGUF header.
    /// </summary>
    public static GgufHeader Read(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        uint magic;
        try
        {
            magic = reader.ReadUInt32();
        }
        catch (EndOfStreamException)
        {
            throw new InvalidDataException("Not a GGUF file (file is too short).");
        }

        if (magic != GgufMagic)
        {
            throw new InvalidDataException("Not a GGUF file (missing 'GGUF' magic).");
        }

        try
        {
            var version = reader.ReadUInt32();
            if (version < 2 || version > 3)
            {
                throw new InvalidDataException($"Unsupported GGUF version {version}.");
            }

            var tensorCount = reader.ReadUInt64();
            var keyValueCount = reader.ReadUInt64();
            if (tensorCount > MaxTensorCount || keyValueCount > MaxKeyValueCount)
            {
                throw new InvalidDataException("Not a valid GGUF file (implausible header counts).");
            }

            var keys = new HashSet<string>(StringComparer.Ordinal);
            var stringValues = new Dictionary<string, string>(StringComparer.Ordinal);
            for (ulong i = 0; i < keyValueCount; i++)
            {
                var key = ReadString(reader);
                var type = (ValueType)reader.ReadUInt32();
                keys.Add(key);

                if (type == ValueType.String)
                {
                    stringValues[key] = ReadString(reader);
                }
                else
                {
                    SkipValue(reader, type);
                }
            }

            var tensorNames = new List<string>((int)Math.Min(tensorCount, 65_536));
            for (ulong i = 0; i < tensorCount; i++)
            {
                tensorNames.Add(ReadString(reader));
                var dimensionCount = reader.ReadUInt32();
                if (dimensionCount > 8)
                {
                    throw new InvalidDataException("Not a valid GGUF file (tensor with more than 8 dimensions).");
                }

                Skip(reader, dimensionCount * 8L); // dims (uint64 each)
                Skip(reader, 4 + 8); // ggml type (uint32) + offset (uint64)
            }

            return new GgufHeader(version, keys, stringValues, tensorNames);
        }
        catch (EndOfStreamException)
        {
            throw new InvalidDataException("Not a valid GGUF file (header is truncated).");
        }
    }

    private static string ReadString(BinaryReader reader)
    {
        var length = reader.ReadUInt64();
        if (length > MaxStringLength)
        {
            throw new InvalidDataException("Not a valid GGUF file (implausible string length).");
        }

        var bytes = reader.ReadBytes((int)length);
        if (bytes.Length != (int)length)
        {
            throw new EndOfStreamException();
        }

        return Encoding.UTF8.GetString(bytes);
    }

    private static void SkipValue(BinaryReader reader, ValueType type)
    {
        switch (type)
        {
            case ValueType.UInt8:
            case ValueType.Int8:
            case ValueType.Bool:
                Skip(reader, 1);
                break;
            case ValueType.UInt16:
            case ValueType.Int16:
                Skip(reader, 2);
                break;
            case ValueType.UInt32:
            case ValueType.Int32:
            case ValueType.Float32:
                Skip(reader, 4);
                break;
            case ValueType.UInt64:
            case ValueType.Int64:
            case ValueType.Float64:
                Skip(reader, 8);
                break;
            case ValueType.String:
                ReadString(reader);
                break;
            case ValueType.Array:
                SkipArray(reader);
                break;
            default:
                throw new InvalidDataException($"Not a valid GGUF file (unknown value type {(uint)type}).");
        }
    }

    private static void SkipArray(BinaryReader reader)
    {
        var elementType = (ValueType)reader.ReadUInt32();
        var count = reader.ReadUInt64();
        if (count > MaxArrayCount)
        {
            throw new InvalidDataException("Not a valid GGUF file (implausible array length).");
        }

        var elementSize = elementType switch
        {
            ValueType.UInt8 or ValueType.Int8 or ValueType.Bool => 1,
            ValueType.UInt16 or ValueType.Int16 => 2,
            ValueType.UInt32 or ValueType.Int32 or ValueType.Float32 => 4,
            ValueType.UInt64 or ValueType.Int64 or ValueType.Float64 => 8,
            _ => 0,
        };

        if (elementSize > 0)
        {
            Skip(reader, (long)count * elementSize);
            return;
        }

        // Strings (e.g. tokenizer.ggml.tokens) and nested arrays have to be walked one by one.
        for (ulong i = 0; i < count; i++)
        {
            SkipValue(reader, elementType);
        }
    }

    private static void Skip(BinaryReader reader, long byteCount)
    {
        if (byteCount <= 0)
        {
            return;
        }

        var stream = reader.BaseStream;
        if (stream.CanSeek)
        {
            if (stream.Position + byteCount > stream.Length)
            {
                throw new EndOfStreamException();
            }

            stream.Seek(byteCount, SeekOrigin.Current);
            return;
        }

        var remaining = byteCount;
        var buffer = new byte[Math.Min(remaining, 64 * 1024)];
        while (remaining > 0)
        {
            var read = stream.Read(buffer, 0, (int)Math.Min(remaining, buffer.Length));
            if (read <= 0)
            {
                throw new EndOfStreamException();
            }

            remaining -= read;
        }
    }
}
