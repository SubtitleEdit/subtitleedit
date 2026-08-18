using Nikse.SubtitleEdit.Logic;
using System.Diagnostics;

namespace UITests.Logic;

/// <summary>
/// StartInfo.EnvironmentVariables[name] reads like the .NET Framework API that answered null for an
/// unset variable, but on .NET it throws KeyNotFoundException - which is how Speech to text died
/// with "The given key 'DYLD_LIBRARY_PATH' was not present in the dictionary" on macOS, where
/// nothing sets that variable (issue #13816).
/// </summary>
public class ProcessEnvironmentHelperTests
{
    [Fact]
    public void GetOrNull_VariableNotSet_ReturnsNull()
    {
        var startInfo = new ProcessStartInfo();

        Assert.Null(ProcessEnvironmentHelper.GetOrNull(startInfo, "SE_TEST_DYLD_LIBRARY_PATH"));
    }

    [Fact]
    public void GetOrNull_VariableSet_ReturnsTheValue()
    {
        var startInfo = new ProcessStartInfo();
        startInfo.Environment["SE_TEST_DYLD_LIBRARY_PATH"] = "/opt/lib";

        Assert.Equal("/opt/lib", ProcessEnvironmentHelper.GetOrNull(startInfo, "SE_TEST_DYLD_LIBRARY_PATH"));
    }

    /// <summary>The indexer this helper exists to avoid - pinning why it cannot be used directly.</summary>
    [Fact]
    public void EnvironmentVariablesIndexer_UnsetVariable_Throws()
    {
        var startInfo = new ProcessStartInfo();

        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(
            () => startInfo.EnvironmentVariables["SE_TEST_DYLD_LIBRARY_PATH"]);
    }
}
