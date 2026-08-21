using System.Diagnostics;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Reading an environment variable off a <see cref="ProcessStartInfo"/> before setting it.
/// <para>
/// <c>StartInfo.EnvironmentVariables[name]</c> looks like the .NET Framework API that answered null
/// for a variable that is not set, but on .NET (Core) that indexer is a wrapper over a generic
/// dictionary and throws <see cref="System.Collections.Generic.KeyNotFoundException"/> instead. Any
/// "read the old value, then append to it" code therefore crashes on exactly the machines where the
/// variable is unset - Speech to text died with "The given key 'DYLD_LIBRARY_PATH' was not present
/// in the dictionary" on macOS, where nothing sets that variable (issue #13816).
/// </para>
/// </summary>
public static class ProcessEnvironmentHelper
{
    /// <summary>The variable's value, or null when the process will not inherit one.</summary>
    public static string? GetOrNull(ProcessStartInfo startInfo, string name)
    {
        return startInfo.Environment.TryGetValue(name, out var value) ? value : null;
    }
}
