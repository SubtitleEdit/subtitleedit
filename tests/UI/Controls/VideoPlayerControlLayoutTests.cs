using System.Collections.Generic;
using System.Linq;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Options.Settings.VideoControlsItems;
using Nikse.SubtitleEdit.Logic.Config;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// The order and visibility of the controls under the video player can be customized (#15286).
/// </summary>
public class VideoPlayerControlLayoutTests
{
    private static Grid GetControlsGrid(VideoPlayerControl control)
    {
        var slider = GetPositionSlider(control);
        return (Grid)slider.Parent!;
    }

    private static Slider GetPositionSlider(VideoPlayerControl control)
    {
        return control.GetLogicalDescendants().OfType<Slider>()
            .First(p => AutomationProperties.GetName(p) == Se.Language.General.VideoPosition);
    }

    private static Button GetButton(VideoPlayerControl control, string automationName)
    {
        return control.GetLogicalDescendants().OfType<Button>()
            .First(p => AutomationProperties.GetName(p) == automationName);
    }

    private static List<SeVideoControlsItem> Order(params SeVideoControlsItemType[] types)
    {
        return types.Select((t, i) => new SeVideoControlsItem { Type = t, SortOrder = i }).ToList();
    }

    [AvaloniaFact]
    public void Default_KeepsTheClassicLayout()
    {
        var control = new VideoPlayerControl(new EmptyVideoPlayer());
        control.ApplyControlsLayout(SeVideoControlsItem.MakeDefaults());
        var grid = GetControlsGrid(control);

        Assert.Equal(5, grid.ColumnDefinitions.Count);
        Assert.True(grid.ColumnDefinitions[3].Width.IsStar);
        Assert.Equal(0, Grid.GetColumn(GetButton(control, Se.Language.General.Play)));
        Assert.Equal(1, Grid.GetColumn(GetButton(control, Se.Language.General.Stop)));
        Assert.Equal(3, Grid.GetColumn(GetPositionSlider(control)));

        var texts = grid.Children.OfType<TextBlock>().ToList();
        Assert.Equal(3, texts.Count);
        Assert.Contains(texts, p => p.HorizontalAlignment == HorizontalAlignment.Center && Grid.GetColumn(p) == 3 && Grid.GetColumnSpan(p) == 1);
    }

    [AvaloniaFact]
    public void Reordered_ButtonsFollowListOrder()
    {
        var control = new VideoPlayerControl(new EmptyVideoPlayer());
        control.ApplyControlsLayout(Order(
            SeVideoControlsItemType.PositionSlider,
            SeVideoControlsItemType.Play,
            SeVideoControlsItemType.Stop));

        Assert.Equal(0, Grid.GetColumn(GetPositionSlider(control)));
        Assert.Equal(1, Grid.GetColumn(GetButton(control, Se.Language.General.Play)));
        Assert.Equal(2, Grid.GetColumn(GetButton(control, Se.Language.General.Stop)));
    }

    [AvaloniaFact]
    public void FileNameBeforePositionText_SwapsTheCaptions()
    {
        var control = new VideoPlayerControl(new EmptyVideoPlayer());
        var items = SeVideoControlsItem.MakeDefaults();
        items.Single(p => p.Type == SeVideoControlsItemType.VideoFileName).SortOrder = 1;

        control.ApplyControlsLayout(items);

        var texts = GetControlsGrid(control).Children.OfType<TextBlock>().Where(p => p.VerticalAlignment == VerticalAlignment.Bottom).ToList();
        Assert.Equal(2, texts.Count);
        Assert.Contains(texts, p => p.HorizontalAlignment == HorizontalAlignment.Left);
        Assert.Contains(texts, p => p.HorizontalAlignment == HorizontalAlignment.Right);
    }

    [AvaloniaFact]
    public void HiddenItems_AreNotShown_ButHiddenSliderKeepsItsColumn()
    {
        var control = new VideoPlayerControl(new EmptyVideoPlayer());
        var grid = GetControlsGrid(control);
        var items = SeVideoControlsItem.MakeDefaults();
        foreach (var item in items.Where(p => p.Type is SeVideoControlsItemType.Play or SeVideoControlsItemType.PositionSlider or SeVideoControlsItemType.PlayerName))
        {
            item.IsVisible = false;
        }

        control.ApplyControlsLayout(items);

        Assert.DoesNotContain(grid.Children, p => p is Slider);
        Assert.DoesNotContain(grid.Children, p => p is Button b && AutomationProperties.GetName(b) == Se.Language.General.Play);
        Assert.Single(grid.ColumnDefinitions, p => p.Width.IsStar);
        Assert.Equal(2, grid.Children.OfType<TextBlock>().Count());
    }

    [AvaloniaFact]
    public void ApplyAgain_ReusesTheSameControls()
    {
        var control = new VideoPlayerControl(new EmptyVideoPlayer());
        var slider = GetPositionSlider(control);

        control.ApplyControlsLayout(Order(SeVideoControlsItemType.Volume, SeVideoControlsItemType.PositionSlider));
        control.ApplyControlsLayout(SeVideoControlsItem.MakeDefaults());

        Assert.Same(slider, GetPositionSlider(control));
        Assert.Equal(5, GetControlsGrid(control).ColumnDefinitions.Count);
    }

    [Fact]
    public void Normalize_DropsDuplicatesAndAddsMissing()
    {
        var items = new List<SeVideoControlsItem>
        {
            new() { Type = SeVideoControlsItemType.Volume, SortOrder = 5 },
            new() { Type = SeVideoControlsItemType.Volume, SortOrder = 6, IsVisible = false },
            new() { Type = (SeVideoControlsItemType)999, SortOrder = 7 },
        };

        var result = SeVideoControlsItem.Normalize(items);

        Assert.Equal(SeVideoControlsItem.MakeDefaults().Count, result.Count);
        Assert.Equal(SeVideoControlsItemType.Volume, result[0].Type);
        Assert.True(result[0].IsVisible);
        Assert.Equal(result.Count, result.Select(p => p.Type).Distinct().Count());
    }

    [Fact]
    public void Dialog_OkReturnsOrderAndVisibility()
    {
        var vm = new VideoControlsItemsViewModel();
        vm.Initialize(SeVideoControlsItem.MakeDefaults());

        vm.SelectedItem = vm.Items.Single(p => p.Type == SeVideoControlsItemType.VideoFileName);
        vm.MoveUpCommand.Execute(null);
        vm.Items.Single(p => p.Type == SeVideoControlsItemType.Stop).IsVisible = false;
        vm.OkCommand.Execute(null);

        Assert.False(vm.ResultItems.Single(p => p.Type == SeVideoControlsItemType.Stop).IsVisible);
        var fileName = vm.ResultItems.Single(p => p.Type == SeVideoControlsItemType.VideoFileName);
        var positionText = vm.ResultItems.Single(p => p.Type == SeVideoControlsItemType.PositionText);
        Assert.True(fileName.SortOrder < positionText.SortOrder);
    }

    [Fact]
    public void Migrate_OldShowStopAndFullscreenSettings_BecomeItemVisibility()
    {
        var video = new SeVideo { ControlsItems = null!, ShowStopButton = false, ShowFullscreenButton = true };

        Se.MigrateVideoControlsItems(video);

        Assert.False(video.ControlsItems.Single(p => p.Type == SeVideoControlsItemType.Stop).IsVisible);
        Assert.True(video.ControlsItems.Single(p => p.Type == SeVideoControlsItemType.FullScreen).IsVisible);
        Assert.Null(video.ShowStopButton); // not written back
        Assert.Null(video.ShowFullscreenButton);
    }

    [AvaloniaFact]
    public void Reordered_VolumeFirst_KeepsSpacingToTheNextItem()
    {
        var control = new VideoPlayerControl(new EmptyVideoPlayer());
        control.ApplyControlsLayout(Order(
            SeVideoControlsItemType.Volume,
            SeVideoControlsItemType.Play,
            SeVideoControlsItemType.Stop));

        var play = GetButton(control, Se.Language.General.Play);
        Assert.Equal(10, play.Margin.Left);
        Assert.Equal(3, GetButton(control, Se.Language.General.Stop).Margin.Left);
    }

    [AvaloniaFact]
    public void HiddenStopItem_IsNotShown()
    {
        var control = new VideoPlayerControl(new EmptyVideoPlayer());
        var items = SeVideoControlsItem.MakeDefaults();
        items.Single(p => p.Type == SeVideoControlsItemType.Stop).IsVisible = false;

        control.ApplyControlsLayout(items);

        Assert.DoesNotContain(GetControlsGrid(control).Children, p => p is Button b && AutomationProperties.GetName(b) == Se.Language.General.Stop);
    }
}
