using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Tests.Controls;

public class WrapPanelLineBreakTests
{
    [AvaloniaFact]
    public void LineBreak_MovesFollowingItemsToNewRow_EvenWhenEverythingFits()
    {
        var first = new Border { Width = 50, Height = 20 };
        var second = new Border { Width = 50, Height = 20 };
        var third = new Border { Width = 50, Height = 20 };
        var panel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { first, new WrapPanelLineBreak(), second, third },
        };

        panel.Measure(new Size(500, 500));
        panel.Arrange(new Rect(0, 0, 500, 500));

        Assert.Equal(0, first.Bounds.Y);
        Assert.Equal(20, second.Bounds.Y);
        Assert.Equal(20, third.Bounds.Y);
        Assert.Equal(40, panel.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void LineBreak_TakesNoSpace_WhenWidthIsUnconstrained()
    {
        var lineBreak = new WrapPanelLineBreak();

        lineBreak.Measure(Size.Infinity);

        Assert.Equal(new Size(0, 0), lineBreak.DesiredSize);
    }

    [Fact]
    public void Defaults_LineBreaksAreHidden_AndAddedToLegacySettings()
    {
        var types = new[] { SeWaveformToolbarItemType.LineBreak1, SeWaveformToolbarItemType.LineBreak2 };
        var waveform = new SeWaveform();
        Assert.All(types, t => Assert.False(waveform.ToolbarItems.Single(p => p.Type == t).IsVisible));

        waveform.ToolbarItems.RemoveAll(p => types.Contains(p.Type));
        waveform.EnsureAllToolbarItems();

        Assert.All(types, t => Assert.False(waveform.ToolbarItems.Single(p => p.Type == t).IsVisible));
    }
}
