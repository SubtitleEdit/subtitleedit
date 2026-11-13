using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.Bookmarks;

namespace UITests.Features.Bookmarks;

/// <summary>
/// A bookmark uses one of three colors, picked with the swatches above the note.
/// </summary>
public class BookmarkEditColorTests
{
    [AvaloniaFact]
    public void Swatches_StartAtTheLinesColor_AndClickPicksAColor()
    {
        var vm = new BookmarkEditViewModel();
        vm.Initialize([new SubtitleLineViewModel { Bookmark = "note", BookmarkColor = 1 }], []);
        var window = new BookmarkEditWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            Assert.Equal(1, vm.ColorIndex);
            var swatches = window.GetVisualDescendants().OfType<Button>()
                .Where(b => b.Content is Optris.Icons.Avalonia.Icon)
                .ToList();
            Assert.Equal(3, swatches.Count);

            swatches[2].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(2, vm.ColorIndex);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void RemovingTheBookmark_ResetsItsColor()
    {
        var line = new SubtitleLineViewModel { Bookmark = "note", BookmarkColor = 2 };
        line.Bookmark = null;
        Assert.Equal(0, line.BookmarkColor);
    }
}
