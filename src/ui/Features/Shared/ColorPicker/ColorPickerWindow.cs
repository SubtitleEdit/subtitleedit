using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

/// <summary>
/// Color picker, in the editor look (see <see cref="EditorChrome"/>): wheel, brightness bar and
/// preview on the left, the R/G/B(/A) sliders and hex on the right, recent colors below.
/// </summary>
public class ColorPickerWindow : Window
{
    public ColorPickerWindow(ColorPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ChooseColor;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var colorView = MakeColorView(vm, out var hexTextBox);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(colorView, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, hexTextBox); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Opened += (_, _) => ConfigureImagePicker(vm);
    }

    private void ConfigureImagePicker(ColorPickerViewModel vm)
    {
        // Use this dialog's editor, including when opened through a style/settings dialog
        // or from a second editor window.
        var source = ImageColorSource.Find(this);
        if (source != null)
        {
            vm.CanSelectFromImage = source.CanPick();
            vm.PickFromImageAsync = source.PickAsync;
        }
    }

    private static StackPanel MakeColorView(ColorPickerViewModel vm, out TextBox hexTextBox)
    {
        // Color wheel
        var colorWheel = new ColorWheelControl
        {
            Width = 200,
            Height = 200,
            Margin = new Thickness(0, 0, 12, 0),
        };
        colorWheel.Bind(ColorWheelControl.HueProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.Hue),
            Mode = BindingMode.TwoWay
        });
        colorWheel.Bind(ColorWheelControl.SaturationProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.Saturation),
            Mode = BindingMode.TwoWay
        });

        // Brightness (tone) bar, like the Subtitle Edit 4 color dialog (issue #15296)
        var brightnessBar = new ColorBrightnessBar
        {
            Height = 200,
        };
        brightnessBar.Bind(ColorBrightnessBar.ValueProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.Brightness),
            Mode = BindingMode.TwoWay
        });
        brightnessBar.Bind(ColorBrightnessBar.TopColorProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.BrightnessTopColor),
        });

        var wheelPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { colorWheel, brightnessBar }
        };

        // Selected color preview
        var selectedColorBorder = new Border
        {
            Width = 80,
            Height = 80,
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(0, 14, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
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
            Children = { wheelPanel, selectedColorBorder }
        };

        // RGBA Sliders
        var slidersPanel = CreateSlidersPanel(vm, out hexTextBox);

        var sections = EditorChrome.MakeSideBySide(new Border { Child = leftPanel, Padding = new Thickness(16, 16, 16, 14) }, new Border { Child = slidersPanel, Padding = new Thickness(16, 16, 16, 14) });

        // Recent colors
        var recentColorsPanel = CreateLastColorsPanel(vm);

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                sections,
                recentColorsPanel,
            },
        };
    }

    private static StackPanel CreateSlidersPanel(ColorPickerViewModel vm, out TextBox hexInputTextBox)
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
        alphaSlider.Bind(ColorChannelSlider.IsVisibleProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.ShowAlpha),
        });

        // Hex input
        var hexLabel = EditorChrome.MakeDimLabel(Se.Language.General.Hex, new Thickness(0, 10, 10, 0));

        var hexTextBox = new TextBox
        {
            Margin = new Thickness(0, 10, 0, 0),
            // 8 chars (AARRGGBB) when an alpha channel is shown, otherwise 6 chars (RRGGBB)
            MaxLength = vm.ShowAlpha ? 8 : 6,
            MinWidth = vm.ShowAlpha ? 95 : 75,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        hexTextBox.Classes.Add("text");
        AutomationProperties.SetName(hexTextBox, Se.Language.General.Hex);
        hexTextBox.Bind(TextBox.TextProperty, new Binding
        {
            Source = vm,
            Path = nameof(vm.HexColor),
            Mode = BindingMode.TwoWay
        });

        hexInputTextBox = hexTextBox;

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

        var selectFromImage = UiUtil.MakeButton(Se.Language.Assa.ImageColorPicker + "...", vm.SelectFromImageCommand);
        selectFromImage.HorizontalAlignment = HorizontalAlignment.Left;
        selectFromImage.Margin = new Thickness(0, 8, 0, 0);
        selectFromImage.Bind(IsVisibleProperty, new Binding(nameof(vm.CanSelectFromImage)));
        panel.Children.Add(selectFromImage);

        return panel;
    }

    private static Border CreateLastColorsPanel(ColorPickerViewModel vm)
    {

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

        return EditorChrome.MakeEditorSection(Se.Language.Tools.RecentColors, colorsGrid);
    }

    private static Border CreateColorBox(ColorPickerViewModel vm, string propertyName, int column)
    {
        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(5),
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
                vm.SelectRecentColor(color);
            }
        };

        return border;
    }
}
