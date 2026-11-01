using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// The layout picker draws its thumbnails from a table, one row per layout in <see cref="InitLayout"/>,
/// and marks the layout in use, which the picker never showed before.
/// </summary>
public class LayoutPickerTests
{
    [AvaloniaFact]
    public void EveryLayout_HasAThumbnailWithPanes()
    {
        for (var layoutNumber = 1; layoutNumber <= LayoutThumbnail.Count; layoutNumber++)
        {
            Assert.NotEmpty(LayoutThumbnail.Make(layoutNumber).Children);
        }

        Assert.Equal(InitLayout.LayoutWithoutVideo, LayoutThumbnail.Count);
    }

    [AvaloniaFact]
    public void OnlyTheCurrentLayout_IsMarked()
    {
        using var _ = new SettingsScope("General.LayoutNumber");
        Se.Settings.General.LayoutNumber = 3;
        var window = new LayoutWindow(new LayoutViewModel());
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var cards = window.GetLogicalDescendants().OfType<WrapPanel>().Single().Children.OfType<Border>().ToList();

        Assert.Equal(LayoutThumbnail.Count, cards.Count);
        Assert.Equal([2], cards.Select((card, index) => (card, index)).Where(c => c.card.BorderBrush == EditorChrome.AccentBrush).Select(c => c.index));
        window.Close();
    }
}
