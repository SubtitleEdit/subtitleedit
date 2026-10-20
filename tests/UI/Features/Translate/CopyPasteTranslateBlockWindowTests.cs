using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Translate;

namespace UITests.Features.Translate;

/// <summary>
/// The copy-paste translate block window's "Re-copy text to clipboard" button was wired to the
/// get-from-clipboard command, so clicking it read the clipboard (and closed the window) instead of
/// putting the source block back on it.
/// </summary>
public class CopyPasteTranslateBlockWindowTests
{
    [AvaloniaFact]
    public void ReCopyButton_UsesCopyToClipboardCommand()
    {
        var vm = new CopyPasteTranslateBlockViewModel();
        vm.Initialize(1, 2, "Hello world");
        var window = new CopyPasteTranslateBlockWindow(vm);
        try
        {
            var buttons = window.GetLogicalDescendants().OfType<Button>().ToList();

            var reCopyButtons = buttons.Where(b => ReferenceEquals(b.Command, vm.CopyToClipboardCommand)).ToList();
            Assert.Single(reCopyButtons);

            // Only the big "get from clipboard" button reads the clipboard.
            Assert.Single(buttons, b => ReferenceEquals(b.Command, vm.CopyFromClipboardCommand));
        }
        finally
        {
            window.Close();
        }
    }
}
