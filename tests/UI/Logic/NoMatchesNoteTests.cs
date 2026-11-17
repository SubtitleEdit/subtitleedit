using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The "No matches" note over a filtered list shows only while there is search text and the
/// list is empty - not for an empty search, and not while anything matches.
/// </summary>
public class NoMatchesNoteTests
{
    [AvaloniaFact]
    public void ShowsOnlyForSearchTextWithEmptyList()
    {
        var items = new ObservableCollection<string> { "English", "Danish" };
        var search = new TextBox();
        var note = EditorChrome.MakeNoMatchesNote(items, search);
        Assert.False(note.IsVisible);

        search.Text = "xyz";
        items.Clear();
        Dispatcher.UIThread.RunJobs();
        Assert.True(note.IsVisible);

        items.Add("Danish");
        Dispatcher.UIThread.RunJobs();
        Assert.False(note.IsVisible);

        items.Clear();
        search.Text = string.Empty;
        Dispatcher.UIThread.RunJobs();
        Assert.False(note.IsVisible);
    }
}
