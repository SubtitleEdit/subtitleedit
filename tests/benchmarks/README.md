# LibSE benchmarks

[BenchmarkDotNet](https://benchmarkdotnet.org/) micro-benchmarks for libse hot paths.

## Running

Benchmarks **must** be run in Release; BenchmarkDotNet refuses to produce numbers from a Debug build.

```bash
# everything
dotnet run --project tests/benchmarks -c Release -- --filter '*'

# one class
dotnet run --project tests/benchmarks -c Release -- --filter '*BinaryPrimitives*'

# quick pass while iterating (3 warmups / 3 iterations - indicative, not publishable)
dotnet run --project tests/benchmarks -c Release -- --filter '*' --job short
```

Results land in `BenchmarkDotNet.Artifacts/results/` as markdown, HTML and CSV.

## A note on reading the numbers

These benchmarks measure individual binary reads, which land in the sub-nanosecond range - close
to the floor of what BenchmarkDotNet can resolve on a single call. Each benchmark therefore sweeps
a 64 KB buffer and uses `OperationsPerInvoke` to normalize back to ns/read, so the reported figure
reflects the read itself rather than harness overhead.

Ratios between implementations are meaningful; absolute values are machine-specific and will differ
between architectures. Endianness intrinsics in particular differ sharply between arm64 (`rev`) and
x64 (`movbe`/`bswap`), so numbers from an Apple Silicon machine do not transfer to a CI x64 runner.

## Benchmarks

### `BinaryPrimitivesBenchmarks`

Compares the hand-rolled byte-shifting helpers libse used to carry against the
`System.Buffers.Binary.BinaryPrimitives` calls that replaced them.

The legacy implementations are kept in the benchmark file as verbatim private copies, because the
originals were deleted from the library. `GlobalSetup` asserts that both implementations agree on
every offset of the test buffer before any timing runs - a benchmark comparing two implementations
that return different values would be meaningless.
