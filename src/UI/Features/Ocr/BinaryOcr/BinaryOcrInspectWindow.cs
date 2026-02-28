using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

public class BinaryOcrInspectWindow : Window
{
    public BinaryOcrInspectWindow(BinaryOcrInspectViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.NOcrInspectImageMatches;
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Lines
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Controls
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var linesView = MakeLinesView(vm);
        var controlsView = MakeControlsView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk);

        grid.Add(linesView, 0);
        grid.Add(controlsView, 1);
        grid.Add(buttonBar, 2);

        Content = grid;

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;

        Activated += delegate
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        };
        PointerWheelChanged += vm.PointerWheelChanged;
        KeyDown += (_, e) => vm.KeyDown(e);
        KeyUp += (_, e) => vm.KeyUp(e);
        Loaded += (_, _) => vm.OnLoaded();
    }

    private static Border MakeLinesView(BinaryOcrInspectViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            Height = double.NaN,
        };

        vm.PanelLines = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Margin = new Thickness(5),
        };

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.SentenceBitmap)),
            Stretch = Stretch.Uniform,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            MaxWidth = 300,
            MaxHeight = 200,
        };

        grid.Add(vm.PanelLines, 0);
        grid.Add(image, 0, 1);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(10);
    }

    private static Border MakeControlsView(BinaryOcrInspectViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 20,
            Width = double.NaN,
        };

        vm.TextBoxNew = UiUtil.MakeTextBox(100, vm, nameof(vm.NewText))
            .WithBindEnabled(nameof(vm.IsEditControlsEnabled));

        var image = new Image
        {
            Margin = new Thickness(5),
            [!Image.SourceProperty] = new Binding(nameof(vm.CurrentBitmap)),
            Stretch = Stretch.Uniform,
            MinWidth = 30,
            MinHeight = 30,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        var panelCurrentImage = new StackPanel
        {
            Background = new SolidColorBrush(Colors.LightGray),
            Children = { image },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 5),
        };

        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.IsNewTextItalic))
            .WithBindEnabled(nameof(vm.IsEditControlsEnabled));
        checkBoxItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var panelCurrent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.CurrentImage).WithBold(),
                panelCurrentImage,
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ResolutionAndTopMargin)),
                UiUtil.MakeLabel(Se.Language.General.Match).WithBold().WithMarginTop(15),
                vm.TextBoxNew,
                checkBoxItalic,
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.MatchResolutionAndTopMargin)),
                UiUtil.MakeButton(Se.Language.General.Update, vm.UpdateCommand).WithMarginTop(25).WithLeftAlignment().WithBindEnabled(nameof(vm.IsEditControlsEnabled)),
                UiUtil.MakeButton(Se.Language.General.Delete, vm.DeleteCommand).WithMarginTop(5).WithLeftAlignment().WithBindEnabled(nameof(vm.IsEditControlsEnabled)),
                UiUtil.MakeButton(Se.Language.Ocr.AddBetterMatch, vm.AddBetterMatchCommand).WithMarginTop(5).WithLeftAlignment(),
            },
        };

        var panelMatch = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel("Binary OCR uses pixel-by-pixel comparison.").WithMarginTop(10),
                UiUtil.MakeLabel("No manual drawing required.").WithMarginTop(5),
            },
        };

        grid.Add(panelCurrent, 0);
        grid.Add(panelMatch, 0, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

   
}
