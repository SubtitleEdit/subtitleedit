using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Shared.Bookmarks;

namespace UITests.Features.Bookmarks;

/// <summary>
/// A bookmark note can span several lines: Shift+Enter adds a line break, plain Enter is OK.
/// </summary>
public class BookmarkEditMultiLineTests
{
    [AvaloniaFact]
    public void ShiftEnter_AddsLineBreak_AndEnter_PressesOk()
    {
        var vm = new BookmarkEditViewModel();
        var window = new BookmarkEditWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var textBox = window.GetVisualDescendants().OfType<TextBox>().First();
            textBox.Focus();
            Dispatcher.UIThread.RunJobs();

            window.KeyTextInput("first");
            window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.Shift);
            window.KeyTextInput("second");
            Dispatcher.UIThread.RunJobs();

            Assert.False(vm.OkPressed);
            Assert.Equal(2, (vm.BookmarkText ?? string.Empty).Split('\n').Length);
            Assert.StartsWith("first", vm.BookmarkText);
            Assert.EndsWith("second", vm.BookmarkText);

            window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.OkPressed);
            Assert.EndsWith("second", vm.BookmarkText);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
