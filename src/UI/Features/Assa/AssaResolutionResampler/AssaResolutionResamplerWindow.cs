using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.ResolutionResampler;

public class AssaResolutionResamplerWindow : Window
{
    public AssaResolutionResamplerWindow(AssaResolutionResamplerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.ResolutionResamplerTitle;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 450;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 15,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        // Source resolution panel
        var sourcePanel = CreateResolutionPanel(
            Se.Language.Assa.ResolutionResamplerSourceRes,
            nameof(vm.SourceWidth),
            nameof(vm.SourceHeight),
            vm.SourceFromVideoCommand);
        Grid.SetRow(sourcePanel, 0);
        grid.Children.Add(sourcePanel);

        // Target resolution panel
        var targetPanel = CreateResolutionPanel(
            Se.Language.Assa.ResolutionResamplerTargetRes,
            nameof(vm.TargetWidth),
            nameof(vm.TargetHeight),
            vm.TargetFromVideoCommand);
        Grid.SetRow(targetPanel, 1);
        grid.Children.Add(targetPanel);

        // Options panel
        var optionsPanel = CreateOptionsPanel(vm);
        Grid.SetRow(optionsPanel, 2);
        grid.Children.Add(optionsPanel);

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        Grid.SetRow(panelButtons, 3);
        grid.Children.Add(panelButtons);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += vm.KeyDown;
    }

    private static Border CreateResolutionPanel(string title, string widthBinding, string heightBinding, System.Windows.Input.ICommand fromVideoCommand)
    {
        var panelGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 8,
            ColumnSpacing = 10,
        };

        // Title
        var titleLabel = new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        Grid.SetRow(titleLabel, 0);
        Grid.SetColumnSpan(titleLabel, 5);
        panelGrid.Children.Add(titleLabel);

        // Width
        var widthLabel = new TextBlock
        {
            Text = Se.Language.General.Width + ":",
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetRow(widthLabel, 1);
        Grid.SetColumn(widthLabel, 0);
        panelGrid.Children.Add(widthLabel);

        var widthBox = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 8192,
            Width = 120,
            Increment = 1,
            FormatString = "0",
            [!NumericUpDown.ValueProperty] = new Binding(widthBinding) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(widthBox, 1);
        Grid.SetColumn(widthBox, 1);
        panelGrid.Children.Add(widthBox);

        // Height
        var heightLabel = new TextBlock
        {
            Text = Se.Language.General.Height + ":",
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetRow(heightLabel, 1);
        Grid.SetColumn(heightLabel, 2);
        panelGrid.Children.Add(heightLabel);

        var heightBox = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 8192,
            Width = 120,
            Increment = 1,
            FormatString = "0",
            [!NumericUpDown.ValueProperty] = new Binding(heightBinding) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(heightBox, 1);
        Grid.SetColumn(heightBox, 3);
        panelGrid.Children.Add(heightBox);

        // From video button
        var fromVideoButton = new Button
        {
            Content = Se.Language.Assa.ResolutionResamplerFromVideo,
            Command = fromVideoCommand,
            Margin = new Thickness(10, 0, 0, 0),
        };
        Grid.SetRow(fromVideoButton, 1);
        Grid.SetColumn(fromVideoButton, 4);
        panelGrid.Children.Add(fromVideoButton);

        return new Border
        {
            Child = panelGrid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(10),
        };
    }

    private static Border CreateOptionsPanel(AssaResolutionResamplerViewModel vm)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
        };

        var titleLabel = new TextBlock
        {
            Text = Se.Language.General.Options,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        stackPanel.Children.Add(titleLabel);

        var checkBoxMargins = new CheckBox
        {
            Content = Se.Language.Assa.ResolutionResamplerChangeMargins,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ChangeMargins)) { Mode = BindingMode.TwoWay },
        };
        stackPanel.Children.Add(checkBoxMargins);

        var checkBoxFontSize = new CheckBox
        {
            Content = Se.Language.Assa.ResolutionResamplerChangeFontSize,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ChangeFontSize)) { Mode = BindingMode.TwoWay },
        };
        stackPanel.Children.Add(checkBoxFontSize);

        var checkBoxPositions = new CheckBox
        {
            Content = Se.Language.Assa.ResolutionResamplerChangePositions,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ChangePositions)) { Mode = BindingMode.TwoWay },
        };
        stackPanel.Children.Add(checkBoxPositions);

        var checkBoxDrawing = new CheckBox
        {
            Content = Se.Language.Assa.ResolutionResamplerChangeDrawing,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ChangeDrawing)) { Mode = BindingMode.TwoWay },
        };
        stackPanel.Children.Add(checkBoxDrawing);

        return new Border
        {
            Child = stackPanel,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(10),
        };
    }
}
