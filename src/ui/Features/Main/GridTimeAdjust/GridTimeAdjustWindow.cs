using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.GridTimeAdjust;

public class GridTimeAdjustWindow : Window
{
    public GridTimeAdjustWindow(GridTimeAdjustViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.Show + " / " + Se.Language.General.Hide);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var timeCodeUpDownStart = new TimeCodeUpDown
        {
            DataContext = vm,
            Width = 140,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.StartTime))
            {
                Mode = BindingMode.TwoWay,
            }
        };

        var timeCodeUpDownEnd = new TimeCodeUpDown
        {
            DataContext = vm,
            Width = 140,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.EndTime))
            {
                Mode = BindingMode.TwoWay,
            }
        };

        var secondsUpDownDuration = new SecondsUpDown
        {
            DataContext = vm,
            Width = 110,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!SecondsUpDown.ValueProperty] = new Binding(nameof(vm.Duration))
            {
                Mode = BindingMode.TwoWay,
            }
        };

        var numericStep = new NumericUpDown
        {
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = 140,
            Minimum = 1,
            Maximum = 60000,
            Increment = 50,
            FormatString = "F0",
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.StepMs))
            {
                Mode = BindingMode.TwoWay,
            },
        };
        numericStep.KeyDown += (sender, args) => vm.OnKeyDown(args);

        // Hàng trên: Show time, End time, Duration
        var panelStart = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.General.Show),
                timeCodeUpDownStart,
            },
        };

        var panelEnd = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.General.Hide),
                timeCodeUpDownEnd,
            },
        };

        var panelDuration = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.General.Duration),
                secondsUpDownDuration,
            },
        };

        var rowTopTimes = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children =
            {
                panelStart,
                panelEnd,
                panelDuration,
            },
        };

        // Hàng dưới: Adjustment step (ms)
        var panelStep = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            Children =
            {
                UiUtil.MakeLabel("Adjustment step (ms):"),
                numericStep,
            },
        };

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                rowTopTimes,
                panelStep,
                buttonPanel,
            },
        };

        Content = mainPanel;

        KeyDown += (sender, args) => vm.OnKeyDown(args);

        Opened += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (vm.FocusEndTime)
                {
                    timeCodeUpDownEnd.Focus();
                }
                else
                {
                    timeCodeUpDownStart.Focus();
                }
            });
        };
    }
}
