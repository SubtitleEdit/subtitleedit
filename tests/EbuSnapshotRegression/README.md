# EBU import frame-rate regression tests

Run `dotnet test tests/EbuSnapshotRegression/EbuSnapshotRegression.csproj`.

The 13 synthetic cases build STL data in memory and deterministically update the global frame rate immediately before selected TTI blocks. They cover the 25-fps control, changes to 23.976 fps, repeated changes, header and override behavior, and the 999 ms cap. No external subtitle files are required.

TestLibSE builds a separate assembly from the production LibSE sources, replacing Ebu.cs only in this test assembly with EbuTestReader.generated.cs. The replacement adds an internal BeforeReadTti callback and its invocation. The production project does not compile this hook. The copied reader must remain identical to production after removal of the test-only comment and those two hook lines, ignoring the UTF-8 BOM and trailing whitespace. Compare these files whenever Ebu.cs changes.

Without the per-import snapshot, 12 of the 13 cases fail; with it all 13 pass. The original real-file validation is intentionally excluded from this portable project.
