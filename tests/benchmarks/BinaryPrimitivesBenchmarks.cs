using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Nikse.SubtitleEdit.Benchmarks
{
    /// <summary>
    /// Compares the hand-rolled byte-shifting helpers that libse used to carry against the
    /// <see cref="BinaryPrimitives"/> calls that replaced them.
    ///
    /// The "Legacy" methods below are verbatim copies of the removed helpers (Box.GetUInt,
    /// Box.GetUInt64, Box.GetWord, GetCcDataHelper.GetUInt32, Tx3gTextOnly.GetUInt and
    /// VobSub.Helper.GetLittleEndian32) so the comparison stays apples-to-apples after those
    /// helpers were deleted from the library.
    ///
    /// Each benchmark sweeps a 64 KB buffer so the measured cost is the read itself rather than
    /// BenchmarkDotNet's per-invocation overhead; OperationsPerInvoke normalizes back to ns/read.
    /// </summary>
    // No MemoryDiagnoser: every benchmark reads primitives out of a buffer allocated once in
    // GlobalSetup, so allocation is zero by construction and measuring it only slows the run.
    [CategoriesColumn]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [HideColumns("Error", "StdDev", "Median", "RatioSD")]
    public class BinaryPrimitivesBenchmarks
    {
        private const int BufferSize = 64 * 1024;
        private const int Reads16 = BufferSize / 2;
        private const int Reads32 = BufferSize / 4;
        private const int Reads64 = BufferSize / 8;

        private byte[] _buffer = null!;

        [GlobalSetup]
        public void Setup()
        {
            // Deterministic pseudo-random content; the values matter only in that they exercise
            // every byte lane, so a fixed seed keeps runs comparable.
            _buffer = new byte[BufferSize];
            new Random(20260722).NextBytes(_buffer);

            // Fail loudly if the two implementations ever stop agreeing - a benchmark comparing
            // two different results is worse than no benchmark at all.
            for (var i = 0; i + 8 <= BufferSize; i++)
            {
                var span = _buffer.AsSpan(i);
                if (LegacyGetUInt32BigEndian(_buffer, i) != BinaryPrimitives.ReadUInt32BigEndian(span) ||
                    LegacyGetUInt64BigEndian(span) != BinaryPrimitives.ReadUInt64BigEndian(span) ||
                    LegacyGetWord(span) != BinaryPrimitives.ReadUInt16BigEndian(span) ||
                    LegacyGetLittleEndian32(_buffer, i) != BinaryPrimitives.ReadInt32LittleEndian(span))
                {
                    throw new InvalidOperationException($"Legacy and BinaryPrimitives reads disagree at offset {i}.");
                }
            }
        }

        // ---- 32-bit big-endian (MP4 box sizes, CEA-608 NAL sizes, tx3g box lengths) ----

        [Benchmark(Baseline = true, OperationsPerInvoke = Reads32), BenchmarkCategory("UInt32BE")]
        public ulong Legacy_UInt32BigEndian()
        {
            ulong sum = 0;
            var buffer = _buffer;
            for (var i = 0; i < BufferSize; i += 4)
            {
                sum += LegacyGetUInt32BigEndian(buffer, i);
            }

            return sum;
        }

        [Benchmark(OperationsPerInvoke = Reads32), BenchmarkCategory("UInt32BE")]
        public ulong BinaryPrimitives_UInt32BigEndian()
        {
            ulong sum = 0;
            var span = _buffer.AsSpan();
            for (var i = 0; i < BufferSize; i += 4)
            {
                sum += BinaryPrimitives.ReadUInt32BigEndian(span.Slice(i));
            }

            return sum;
        }

        // ---- 64-bit big-endian (MP4 large-box sizes) ----

        [Benchmark(Baseline = true, OperationsPerInvoke = Reads64), BenchmarkCategory("UInt64BE")]
        public ulong Legacy_UInt64BigEndian()
        {
            ulong sum = 0;
            var span = _buffer.AsSpan();
            for (var i = 0; i < BufferSize; i += 8)
            {
                sum += LegacyGetUInt64BigEndian(span.Slice(i));
            }

            return sum;
        }

        [Benchmark(OperationsPerInvoke = Reads64), BenchmarkCategory("UInt64BE")]
        public ulong BinaryPrimitives_UInt64BigEndian()
        {
            ulong sum = 0;
            var span = _buffer.AsSpan();
            for (var i = 0; i < BufferSize; i += 8)
            {
                sum += BinaryPrimitives.ReadUInt64BigEndian(span.Slice(i));
            }

            return sum;
        }

        // ---- 16-bit big-endian (tx3g text sizes) ----

        [Benchmark(Baseline = true, OperationsPerInvoke = Reads16), BenchmarkCategory("UInt16BE")]
        public ulong Legacy_WordBigEndian()
        {
            ulong sum = 0;
            var span = _buffer.AsSpan();
            for (var i = 0; i < BufferSize; i += 2)
            {
                sum += (ulong)LegacyGetWord(span.Slice(i));
            }

            return sum;
        }

        [Benchmark(OperationsPerInvoke = Reads16), BenchmarkCategory("UInt16BE")]
        public ulong BinaryPrimitives_WordBigEndian()
        {
            ulong sum = 0;
            var span = _buffer.AsSpan();
            for (var i = 0; i < BufferSize; i += 2)
            {
                sum += BinaryPrimitives.ReadUInt16BigEndian(span.Slice(i));
            }

            return sum;
        }

        // ---- 32-bit little-endian (VobSub SP header timestamps) ----

        [Benchmark(Baseline = true, OperationsPerInvoke = Reads32), BenchmarkCategory("Int32LE")]
        public long Legacy_Int32LittleEndian()
        {
            long sum = 0;
            var buffer = _buffer;
            for (var i = 0; i < BufferSize; i += 4)
            {
                sum += LegacyGetLittleEndian32(buffer, i);
            }

            return sum;
        }

        [Benchmark(OperationsPerInvoke = Reads32), BenchmarkCategory("Int32LE")]
        public long BinaryPrimitives_Int32LittleEndian()
        {
            long sum = 0;
            var span = _buffer.AsSpan();
            for (var i = 0; i < BufferSize; i += 4)
            {
                sum += BinaryPrimitives.ReadInt32LittleEndian(span.Slice(i));
            }

            return sum;
        }

        // ---- verbatim copies of the removed helpers ----

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint LegacyGetUInt32BigEndian(byte[] buffer, int pos)
        {
            return (uint)((buffer[pos + 0] << 24) + (buffer[pos + 1] << 16) + (buffer[pos + 2] << 8) + buffer[pos + 3]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong LegacyGetUInt64BigEndian(ReadOnlySpan<byte> span)
        {
            return (ulong)span[0] << 56 | (ulong)span[1] << 48 | (ulong)span[2] << 40 | (ulong)span[3] << 32 |
                   (ulong)span[4] << 24 | (ulong)span[5] << 16 | (ulong)span[6] << 8 | span[7];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LegacyGetWord(ReadOnlySpan<byte> span)
        {
            return (span[0] << 8) | span[1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LegacyGetLittleEndian32(byte[] buffer, int index)
        {
            return buffer[index + 3] << 24 | buffer[index + 2] << 16 | buffer[index + 1] << 8 | buffer[index + 0];
        }
    }
}
