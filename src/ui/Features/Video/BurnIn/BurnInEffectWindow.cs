using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class BurnInEffectWindow : Window
{
    private readonly BurnInEffectViewModel _vm;

    public BurnInEffectWindow(BurnInEffectViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Effect;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        // Create a StackPanel to hold all checkboxes
        var checkBoxPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = new Thickness(10)
        };

        // Create checkboxes for each effect
        foreach (var effect in vm.Effects)
        {
            var checkBox = new CheckBox
            {
                Content = effect.Name,
                Tag = effect, // Store the effect item for later reference
                Name = effect.Type.ToString(),
            };
            vm.CheckBoxes.Add(checkBox);


            // Handle checkbox state changes
            checkBox.IsCheckedChanged += (sender, e) =>
            {
                var cb = sender as CheckBox;
                var effectItem = cb?.Tag as BurnInEffectItem;
                if (effectItem != null && cb != null)
                {
                    if (cb.IsChecked == true)
                    {
                        vm.AddSelectedEffect(effectItem);
                    }
                    else
                    {
                        vm.RemoveSelectedEffect(effectItem);
                    }
                }
            };

            checkBoxPanel.Children.Add(checkBox);
        }

        grid.Add(checkBoxPanel, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.VideoPlayerControl?.Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}