using System.Runtime.CompilerServices;
using System.Text;

namespace LibSETests;

/// <summary>
/// Module initializer that runs once before all tests in the assembly.
/// </summary>
internal static class TestInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Register code pages encoding provider to support encodings like iso-8859-2, windows-1252, etc.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}
