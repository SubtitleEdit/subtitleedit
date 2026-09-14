using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.Letterbox;

public class LetterboxWindow : Window
{
    private readonly LetterboxViewModel _vm;

    public LetterboxWindow(LetterboxViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.Letterbox.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Video.Letterbox;

        var checkBoxEnabled = UiUtil.MakeCheckBox(l.Enabled, vm, nameof(vm.Enabled));
        checkBoxEnabled.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsPlayerSupported)));

        var labelTop = UiUtil.MakeLabel(l.TopHeight);
        labelTop.VerticalAlignment = VerticalAlignment.Center;

        var sliderTop = new Slider
        {
            Minimum = 0,
            Maximum = 40,
            Width = 220,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding(nameof(vm.TopHeightPercent)) { Mode = BindingMode.TwoWay },
        };
        sliderTop.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsPlayerSupported)));

        // TextBlock, not UiUtil.MakeLabel: a Label is a ContentControl, so a TextBlock.Text binding on it never shows
        var labelTopValue = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5, 0, 0, 0),
            MinWidth = 40,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TopHeightPercent)) { StringFormat = "{0:0}%" },
        };

        var labelBottom = UiUtil.MakeLabel(l.BottomHeight);
        labelBottom.VerticalAlignment = VerticalAlignment.Center;

        var sliderBottom = new Slider
        {
            Minimum = 0,
            Maximum = 40,
            Width = 220,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding(nameof(vm.BottomHeightPercent)) { Mode = BindingMode.TwoWay },
        };
        sliderBottom.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsPlayerSupported)));

        var labelBottomValue = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5, 0, 0, 0),
            MinWidth = 40,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.BottomHeightPercent)) { StringFormat = "{0:0}%" },
        };

        var notSupportedLabel = new TextBlock
        {
            Text = l.NotSupportedByCurrentPlayer,
            Foreground = Brushes.OrangeRed,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 480,
        };
        notSupportedLabel.Bind(IsVisibleProperty, new Binding("!" + nameof(vm.IsPlayerSupported)));

        var infoLabel = UiUtil.MakeLabel(l.Info);
        infoLabel.MaxWidth = 480;

        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonApply, buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(checkBoxEnabled, 0, 0, columnSpan: 3);
        grid.Add(notSupportedLabel, 1, 0, columnSpan: 3);
        grid.Add(labelTop, 2, 0);
        grid.Add(sliderTop, 2, 1);
        grid.Add(labelTopValue, 2, 2);
        grid.Add(labelBottom, 3, 0);
        grid.Add(sliderBottom, 3, 1);
        grid.Add(labelBottomValue, 3, 2);
        grid.Add(infoLabel, 4, 0, columnSpan: 3);
        grid.Add(panelButtons, 5, 0, columnSpan: 3);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, checkBoxEnabled); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.RevertIfNotConfirmed();
    }
}
