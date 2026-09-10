using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryMoveCaptions;

public class BinaryMoveCaptionsWindow : Window
{
    public BinaryMoveCaptionsWindow(BinaryMoveCaptionsViewModel vm)
    {
        var l = Se.Language.Tools.ImageBasedEdit;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = l.MoveCaptions;
        Width = 460;
        Height = 400;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
        };

        panel.Children.Add(new TextBlock { Text = l.Letterbox, FontWeight = Avalonia.Media.FontWeight.Bold });
        var comboBoxRatio = UiUtil.MakeComboBox(vm.LetterboxRatios, vm, nameof(vm.SelectedLetterboxRatio), null);
        comboBoxRatio.HorizontalAlignment = HorizontalAlignment.Stretch;
        panel.Children.Add(comboBoxRatio);

        var barHeightRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        barHeightRow.Children.Add(UiUtil.MakeLabel(l.BarHeightPx));
        var numericBarHeight = UiUtil.MakeNumericUpDownInt(0, 8192, 0, 130, vm, nameof(vm.BarHeight));
        numericBarHeight.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsBarHeightEditable)));
        barHeightRow.Children.Add(numericBarHeight);
        panel.Children.Add(barHeightRow);

        panel.Children.Add(new TextBlock
        {
            Text = l.MoveCaptionsTo,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 0),
        });
        panel.Children.Add(UiUtil.MakeRadioButton(l.MoveCaptionsIntoBars, vm, nameof(vm.MoveIntoBars), "mode"));
        panel.Children.Add(UiUtil.MakeRadioButton(l.MoveCaptionsIntoPicture, vm, nameof(vm.MoveIntoPicture), "mode"));

        var offsetRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 10, 0, 0) };
        offsetRow.Children.Add(UiUtil.MakeLabel(l.MoveCaptionsOffset));
        offsetRow.Children.Add(UiUtil.MakeNumericUpDownInt(0, 8192, 10, 130, vm, nameof(vm.Offset)));
        panel.Children.Add(offsetRow);

        panel.Children.Add(new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.InfoText)),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 16, 0, 0),
            FontSize = 12,
        });

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
            },
            Margin = UiUtil.MakeWindowMargin(),
        };
        mainGrid.Add(panel, 0, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        mainGrid.Add(UiUtil.MakeButtonBar(buttonOk, buttonCancel), 1, 0);

        Content = mainGrid;

        UiUtil.FocusOnFirstActivation(this, comboBoxRatio); // an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
