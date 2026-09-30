using System.Runtime.CompilerServices;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace SeConv.Core;

/// <summary>
/// One-time process initialization for seconv. Runs automatically before any other
/// code in this assembly executes (module initializer), so it works for both the
/// CLI entry point and unit tests.
/// </summary>
internal static class Bootstrap
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Register legacy code pages (CP850, CP437, windows-1252, etc.) used by binary
        // subtitle formats. Modern .NET ships only unicode + ASCII out of the box.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // EBU STL save requires a UI helper even in batch mode (libse design).
        Ebu.EbuUiHelper ??= new HeadlessEbuUiHelper();

        // Splitting run-together words via the word split list also splits valid words missing
        // from the dictionary ("backlit" -> "back lit"), so it is off by default like in the GUI
        // (discussion #15423). tools.ocrUseWordSplitList in a --settings file turns it back on.
        Configuration.Settings.Tools.OcrUseWordSplitList = false;
    }
}
