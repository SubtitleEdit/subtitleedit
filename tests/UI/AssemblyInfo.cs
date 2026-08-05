using Xunit;

// The Avalonia headless UI tests share static application state and the global
// Se.Settings instance. xUnit runs test classes in parallel by default, so two classes
// can run at the same time while one mutates a setting the other one is reading
// (e.g. SubtitleLineMaximumLength, MinimumBetweenLines), and timing-sensitive
// keyboard/input tests starve on slow 2-core CI runners. That produced random CI
// failures - a different victim test on every run, all green in isolation and green on
// re-run. Running the UI test classes one at a time removes the concurrency that both
// failure modes depend on. Measured locally: no runtime cost (full suite ~28 s either
// way). Pure-logic test assemblies (LibSE tests) are unaffected and stay parallel.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
