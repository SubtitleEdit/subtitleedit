using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>Candidate paragraph-id generators, 1000 ids per op, each with its own call site.</summary>
[MemoryDiagnoser]
public class GuidGeneratorBenchmarks
{
    private const int N = 1000;

    [Benchmark(Baseline = true)]
    public Guid NewGuid()
    {
        var g = Guid.Empty;
        for (var i = 0; i < N; i++) { g = Guid.NewGuid(); }
        return g;
    }

    [Benchmark]
    public Guid CreateVersion7()
    {
        var g = Guid.Empty;
        for (var i = 0; i < N; i++) { g = Guid.CreateVersion7(); }
        return g;
    }

    [Benchmark]
    public Guid RandomSharedV4()
    {
        var g = Guid.Empty;
        for (var i = 0; i < N; i++) { g = RandomV4(); }
        return g;
    }

    [Benchmark]
    public Guid BatchedCryptoV4()
    {
        var g = Guid.Empty;
        for (var i = 0; i < N; i++) { g = BatchedV4(); }
        return g;
    }

    [Benchmark]
    public Guid CounterV8()
    {
        var g = Guid.Empty;
        for (var i = 0; i < N; i++) { g = Counter(); }
        return g;
    }

    [Benchmark]
    public long LongInterlocked()
    {
        var id = 0L;
        for (var i = 0; i < N; i++) { id = Interlocked.Increment(ref _longCounter); }
        return id;
    }

    [Benchmark]
    public long LongPlainIncrement()
    {
        var id = 0L;
        for (var i = 0; i < N; i++) { id = ++_longCounterPlain; }
        return id;
    }

    private static long _longCounter;
    private static long _longCounterPlain;

    private static Guid RandomV4()
    {
        Span<byte> b = stackalloc byte[16];
        Random.Shared.NextBytes(b);
        b[7] = (byte)((b[7] & 0x0F) | 0x40);
        b[8] = (byte)((b[8] & 0x3F) | 0x80);
        return new Guid(b);
    }

    [ThreadStatic] private static byte[]? _pool;
    [ThreadStatic] private static int _poolPos;

    private static Guid BatchedV4()
    {
        var pool = _pool ??= new byte[4096];
        if (_poolPos == 0 || _poolPos >= pool.Length)
        {
            RandomNumberGenerator.Fill(pool);
            _poolPos = 0;
        }

        var b = pool.AsSpan(_poolPos, 16);
        _poolPos += 16;
        b[7] = (byte)((b[7] & 0x0F) | 0x40);
        b[8] = (byte)((b[8] & 0x3F) | 0x80);
        return new Guid(b);
    }

    private static readonly ulong Tail = (BitConverter.ToUInt64(Guid.NewGuid().ToByteArray(), 8) & 0x3FFF_FFFF_FFFF_FFFFUL) | 0x8000_0000_0000_0000UL;
    private static long _counter;

    private static Guid Counter()
    {
        var c = (ulong)Interlocked.Increment(ref _counter);
        var t = Tail;
        return new Guid((uint)(c >> 28), (ushort)(c >> 12), (ushort)(0x8000 | (c & 0x0FFF)),
            (byte)(t >> 56), (byte)(t >> 48), (byte)(t >> 40), (byte)(t >> 32),
            (byte)(t >> 24), (byte)(t >> 16), (byte)(t >> 8), (byte)t);
    }
}
