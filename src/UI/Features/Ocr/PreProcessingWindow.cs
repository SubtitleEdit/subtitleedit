using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class PreProcessingWindow : Window
{
    public PreProcessingWindow(PreProcessingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.PreProcessingTitle;
        CanResize = true;
        Width = 800;
        Height = 600;
        MinWidth = 700;
        MinHeight = 500;
        vm.Window = this;
        DataContext = vm;

        var checkBoxCropTransparent = new CheckBox
        {
            Content = Se.Language.Ocr.CropTransparent,
            Margin = new Thickness(0, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.CropTransparentColors)) { Mode = BindingMode.TwoWay },
        };
        checkBoxCropTransparent.IsCheckedChanged += (s, e) => vm.RequestPreviewUpdate(); 

        var checkBoxInverseColors = new CheckBox
        {
            Content = Se.Language.Ocr.InverseColors,
            Margin = new Thickness(0, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.InverseColors)) { Mode = BindingMode.TwoWay },
        };
        checkBoxInverseColors.IsCheckedChanged += (s, e) => vm.RequestPreviewUpdate();

        var checkBoxBinarize = new CheckBox
        {
            Content = Se.Language.Ocr.Binarize,
            Margin = new Thickness(0, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.Binarize)) { Mode = BindingMode.TwoWay },
        };
        checkBoxBinarize.IsCheckedChanged += (s, e) => vm.RequestPreviewUpdate();

        var checkBoxOneColor = new CheckBox
        {
            Content = Se.Language.Ocr.OneColor,
            Margin = new Thickness(0, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ToOneColor)) { Mode = BindingMode.TwoWay },
        };
        checkBoxOneColor.IsCheckedChanged += (s, e) => vm.RequestPreviewUpdate(); 
        var labelOneColorThreshold = UiUtil.MakeLabel(Se.Language.Ocr.DarknessThreshold);
        var numericUpDownOneColorThreshold = UiUtil.MakeNumericUpDownInt(1, 255, 128, 200, vm, nameof(vm.OneColorDarknessThreshold))
            .WithBindEnabled(nameof(vm.ToOneColor));
        numericUpDownOneColorThreshold.ValueChanged += (s, e) => vm.RequestPreviewUpdate();
        var panelOneColorThreshold = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                labelOneColorThreshold,
                numericUpDownOneColorThreshold,
            }
        };

        var checkBoxRemoveBorders = new CheckBox
        {
            Content = Se.Language.Ocr.RemoveBorders,
            Margin = new Thickness(0, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.RemoveBorders)) { Mode = BindingMode.TwoWay },
        };
        checkBoxRemoveBorders.IsCheckedChanged += (s, e) => vm.RequestPreviewUpdate();
        var labelBorderSize = UiUtil.MakeLabel(Se.Language.Ocr.BorderSize);
        var numericUpDownBorderSize = UiUtil.MakeNumericUpDownInt(1, 20, 2, 200, vm, nameof(vm.BorderSize))
            .WithBindEnabled(nameof(vm.RemoveBorders));
        numericUpDownBorderSize.ValueChanged += (s, e) => vm.RequestPreviewUpdate();
        var panelRemoveBorderSize = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                labelBorderSize,
                numericUpDownBorderSize
            }
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(checkBoxCropTransparent, 0);
        grid.Add(checkBoxInverseColors, 0,1);
        grid.Add(checkBoxBinarize, 0,2);
        
        grid.Add(checkBoxOneColor, 1);
        grid.Add(panelOneColorThreshold, 1,1, 1, 2);
        
        grid.Add(checkBoxRemoveBorders, 2);
        grid.Add(panelRemoveBorderSize, 2,1, 1, 2);
        
        grid.Add(MakeOriginalImageView(vm), 3, 0,1, 3 );
        grid.Add(MakePostProcessedImageView(vm), 4, 0, 1, 3);
        grid.Add(panelButtons, 5, 0,1,3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Control MakeOriginalImageView(PreProcessingViewModel vm)
    {
        var image = new Image
        {
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
            Source = vm.SourceBitmap,
            MaxHeight = 200,
        };

        return UiUtil.MakeBorderForControl(image);
    }

    private static Border MakePostProcessedImageView(PreProcessingViewModel vm)
    {
        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewImage)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
            MaxHeight = 200,
        };

        return UiUtil.MakeBorderForControl(image);
    }
}
