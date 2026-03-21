using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

public class ColorPickerWindow : Window
{
    public ColorPickerWindow(ColorPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ColorPickerTitle;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        vm.Window = this;
        DataContext = vm;

        var colorView = MakeColorView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(colorView, 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Grid MakeColorView(ColorPickerViewModel vm)
    {
        // Main layout grid
        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,10,*"),
            RowDefinitions = new RowDefinitions("Auto,10,Auto"),
        };

        // Color wheel
        var colorWheel = new ColorWheelControl
        {
            Width = 200,
            Height = 200,
            Margin = new Thickness(10),
        };
        colorWheel.Bind(ColorWheelControl.SelectedColorProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.SelectedColor),
            Mode = BindingMode.TwoWay
        });
        colorWheel.ColorChanged += (s, color) =>
        {
            vm.UpdateFromColorWheel(color);
        };

        // Selected color preview
        var selectedColorBorder = new Border
        {
            Width = 80,
            Height = 80,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            Margin = new Thickness(10, 10, 10, 0),
            VerticalAlignment = VerticalAlignment.Top,
        };
        selectedColorBorder.Bind(Border.BackgroundProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.SelectedColor),
            Converter = new ColorToBrushConverter(),
        });

        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { colorWheel, selectedColorBorder }
        };
        Grid.SetColumn(leftPanel, 0);
        Grid.SetRow(leftPanel, 0);

        // RGBA Sliders
        var slidersPanel = CreateSlidersPanel(vm);
        Grid.SetColumn(slidersPanel, 2);
        Grid.SetRow(slidersPanel, 0);

        // Recent colors
        var recentColorsPanel = CreateLastColorsPanel(vm);
        Grid.SetColumn(recentColorsPanel, 0);
        Grid.SetRow(recentColorsPanel, 2);
        Grid.SetColumnSpan(recentColorsPanel, 3);

        mainGrid.Children.Add(leftPanel);
        mainGrid.Children.Add(slidersPanel);
        mainGrid.Children.Add(recentColorsPanel);

        return mainGrid;
    }

    private static StackPanel CreateSlidersPanel(ColorPickerViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            MinWidth = 250,
        };

        // Red slider
        var redSlider = new ColorChannelSlider
        {
            Label = "R",
        };
        redSlider.Bind(ColorChannelSlider.ValueProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.Red),
            Mode = BindingMode.TwoWay
        });
        redSlider.Bind(ColorChannelSlider.StartColorProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.RedGradientStart),
        });
        redSlider.Bind(ColorChannelSlider.EndColorProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.RedGradientEnd),
        });

        // Green slider
        var greenSlider = new ColorChannelSlider
        {
            Label = "G",
        };
        greenSlider.Bind(ColorChannelSlider.ValueProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.Green),
            Mode = BindingMode.TwoWay
        });
        greenSlider.Bind(ColorChannelSlider.StartColorProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.GreenGradientStart),
        });
        greenSlider.Bind(ColorChannelSlider.EndColorProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.GreenGradientEnd),
        });

        // Blue slider
        var blueSlider = new ColorChannelSlider
        {
            Label = "B",
        };
        blueSlider.Bind(ColorChannelSlider.ValueProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.Blue),
            Mode = BindingMode.TwoWay
        });
        blueSlider.Bind(ColorChannelSlider.StartColorProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.BlueGradientStart),
        });
        blueSlider.Bind(ColorChannelSlider.EndColorProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.BlueGradientEnd),
        });

        // Alpha slider
        var alphaSlider = new ColorChannelSlider
        {
            Label = "A",
        };
        alphaSlider.Bind(ColorChannelSlider.ValueProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.Alpha),
            Mode = BindingMode.TwoWay
        });
        alphaSlider.Bind(ColorChannelSlider.StartColorProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.AlphaGradientStart),
        });
        alphaSlider.Bind(ColorChannelSlider.EndColorProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.AlphaGradientEnd),
        });

        // Hex input
        var hexLabel = new TextBlock
        {
            Text = Se.Language.General.Hex,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 10, 10, 0),
        };

        var hexTextBox = new TextBox
        {
            Margin = new Thickness(0, 10, 0, 0),
            MinWidth = 150,
        };
        hexTextBox.Bind(TextBox.TextProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.HexColor),
            Mode = BindingMode.TwoWay
        });

        var hexPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { hexLabel, hexTextBox }
        };

        panel.Children.Add(redSlider);
        panel.Children.Add(greenSlider);
        panel.Children.Add(blueSlider);
        panel.Children.Add(alphaSlider);
        panel.Children.Add(hexPanel);

        return panel;
    }

    private static StackPanel CreateLastColorsPanel(ColorPickerViewModel vm)
    {
        var label = new TextBlock
        {
            Text = Se.Language.Tools.RecentColors,
            Margin = new Thickness(0, 0, 0, 5),
        };

        var colorsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*,*,*,*"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 30,
            Margin = new Thickness(0, 0, 0, 0),
        };

        // Create 8 color boxes
        var colorBoxes = new[]
        {
            CreateColorBox(vm, nameof(vm.LastColorPickerColor), 0),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor1), 1),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor2), 2),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor3), 3),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor4), 4),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor5), 5),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor6), 6),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor7), 7),
        };

        foreach (var box in colorBoxes)
        {
            colorsGrid.Children.Add(box);
        }

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 10, 10, 0),
            Children = { label, colorsGrid }
        };

        return panel;
    }

    private static Border CreateColorBox(ColorPickerViewModel vm, string propertyName, int column)
    {
        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            Margin = new Thickness(2),
            Cursor = new Cursor(StandardCursorType.Hand),
        };

        Grid.SetColumn(border, column);

        // Bind the background color to the view model property
        border.Bind(Border.BackgroundProperty, new Binding
        {
            Path = propertyName,
            Converter = new ColorToBrushConverter(),
        });

        // Handle click to select the color
        border.PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(border).Properties.IsLeftButtonPressed)
            {
                var color = propertyName switch
                {
                    nameof(vm.LastColorPickerColor) => vm.LastColorPickerColor,
                    nameof(vm.LastColorPickerColor1) => vm.LastColorPickerColor1,
                    nameof(vm.LastColorPickerColor2) => vm.LastColorPickerColor2,
                    nameof(vm.LastColorPickerColor3) => vm.LastColorPickerColor3,
                    nameof(vm.LastColorPickerColor4) => vm.LastColorPickerColor4,
                    nameof(vm.LastColorPickerColor5) => vm.LastColorPickerColor5,
                    nameof(vm.LastColorPickerColor6) => vm.LastColorPickerColor6,
                    nameof(vm.LastColorPickerColor7) => vm.LastColorPickerColor7,
                    _ => Colors.White
                };
                vm.SelectedColor = color;
            }
        };

        return border;
    }
}
