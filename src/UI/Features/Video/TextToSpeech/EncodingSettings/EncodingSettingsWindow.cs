using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;

public class EncodingSettingsWindow : Window
{
    private readonly EncodingSettingsViewModel _vm;

    public EncodingSettingsWindow(EncodingSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.VideoEncodingSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.General.Encoding,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var comboEncoding = new ComboBox
        {
            ItemsSource = vm.Encodings,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 150,
        }.WithBindSelected(nameof(vm.SelectedEncoding));

        var checkBoxStereo = new CheckBox
        {
            Content = Se.Language.General.Stereo,
            IsChecked = vm.IsStereo,
            VerticalAlignment = VerticalAlignment.Center,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.IsStereo)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            [!CheckBox.IsEnabledProperty] = new Binding(nameof(vm.SelectedEncoding) + "." + nameof(EncodingDisplayItem.IsStereoEnabled)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(label, 0, 0);
        grid.Add(comboEncoding, 0, 1);
        grid.Add(checkBoxStereo, 1, 1);
        grid.Add(panelButtons, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
