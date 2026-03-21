using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitFooter
{
    public static Grid Make(MainViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions = new RowDefinitions
            {
                new RowDefinition { Height = GridLength.Auto },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(5, 0, 5, 0),
        };

        var waveformBusyIcon = new Icon
        {
            Value = IconNames.Electron,
            FontSize = 18,
            Margin = new Thickness(0, 0, 5, 0),
            Animation = IconAnimation.Spin,
            // [ToolTip.TipProperty] = Se.Language.General.GeneratingWaveform,
        };
        waveformBusyIcon.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsWaveformGenerating)));
        grid.Add(waveformBusyIcon, 0);

        var labelWaveFormText = UiUtil.MakeLabel()
            .WithBindText(vm, nameof(vm.WaveformGeneratingText))
            .WithBindVisible(vm, nameof(vm.IsWaveformGenerating))
            .WithMarginRight(15);
        labelWaveFormText.Opacity = 0.5;
        grid.Add(labelWaveFormText, 0, 1);

        vm.StatusTextLeftLabel = new TextBlock
        {
            Text = string.Empty,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
        };
        grid.Add(vm.StatusTextLeftLabel, 0, 2);
        vm.StatusTextLeftLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusTextLeft)));

        var right = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 4),
            DataContext = vm,
        };
        right.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusTextRight)));

        var panelRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Icon
                {
                    Value = IconNames.LockClock,
                    FontSize = 20,
                    [ToolTip.TipProperty] = Se.Language.General.LockTimeCodes,
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.LockTimeCodes)),
                    Margin = new Thickness(0, 0, 15, 0),
                },

                new Button
                {
                    Content = new Icon
                    {
                        Value = IconNames.Filter,
                        FontSize = 20,
                        [ToolTip.TipProperty] = Se.Language.General.LayerFilterOn,
                    },
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowLayerFilterIcon)),
                    [!Button.CommandProperty] = new Binding(nameof(vm.ShowPickLayerFilterCommand)),

                    // make it look like just an icon
                    Background = null,
                    BorderBrush = null,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 0, 15, 0),
                },

                new Button
                {
                    Content = new Icon
                    {
                        Value = IconNames.TimerMinus,
                        FontSize = 20,
                        [ToolTip.TipProperty] = Se.Language.Main.Menu.SmpteTiming,
                    },
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsSmpteTimingEnabled)),
                    [!Button.CommandProperty] = new Binding(nameof(vm.ShowSmpteTimingCommand)),

                    // make it look like just an icon
                    Background = null,
                    BorderBrush = null,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 0, 15, 0),
                },

                new Button
                {
                    Content = new Icon
                    {
                        Value = IconNames.TimerSettings,
                        FontSize = 20,
                        [ToolTip.TipProperty] = Se.Language.General.OffsetTimeCodes,
                    },
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsVideoOffsetVisible)),
                    [!Button.CommandProperty] = new Binding(nameof(vm.ShowVideoSetOffsetCommand)),

                    // make it look like just an icon
                    Background = null,
                    BorderBrush = null,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                },
                UiUtil.MakeLabel()
                    .WithBindText(vm, nameof(vm.VideoOffsetText))
                    .WithBindVisible(vm, nameof(vm.IsVideoOffsetVisible)).WithMarginRight(15),

                right,
            },
        };

        grid.Add(panelRight, 0, 3);

        return grid;
    }
}