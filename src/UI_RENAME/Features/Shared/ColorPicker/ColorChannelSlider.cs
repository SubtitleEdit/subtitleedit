using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

public class ColorChannelSlider : Grid
{
    public static readonly StyledProperty<byte> ValueProperty =
        AvaloniaProperty.Register<ColorChannelSlider, byte>(nameof(Value));

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<ColorChannelSlider, string>(nameof(Label), "");

    public static readonly StyledProperty<Color> StartColorProperty =
        AvaloniaProperty.Register<ColorChannelSlider, Color>(nameof(StartColor), Colors.Black);

    public static readonly StyledProperty<Color> EndColorProperty =
        AvaloniaProperty.Register<ColorChannelSlider, Color>(nameof(EndColor), Colors.White);

    public byte Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public Color StartColor
    {
        get => GetValue(StartColorProperty);
        set => SetValue(StartColorProperty, value);
    }

    public Color EndColor
    {
        get => GetValue(EndColorProperty);
        set => SetValue(EndColorProperty, value);
    }

    public event EventHandler<byte>? ValueChanged;

    private readonly Slider _slider;
    private readonly TextBlock _label;
    private readonly TextBlock _valueLabel;
    private readonly Border _gradientBorder;

    public ColorChannelSlider()
    {
        ColumnDefinitions = new ColumnDefinitions("40,*,40");
        Margin = new Thickness(0, 2);

        _label = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 10, 0)
        };
        _label.Bind(TextBlock.TextProperty, new Binding { Source = this, Path = nameof(Label) });
        SetColumn(_label, 0);

        _gradientBorder = new Border
        {
            ClipToBounds = true,
            Height = 24,
            Child = new Border
            {
                Background = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop { Color = StartColor, Offset = 0 },
                        new GradientStop { Color = EndColor, Offset = 1 }
                    }
                }
            }
        };

        _slider = new Slider
        {
            Minimum = 0,
            Maximum = 255,
            TickFrequency = 1,
            IsSnapToTickEnabled = true,
            Background = Brushes.Transparent,
            Margin = new Thickness(0, -2, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(240, 240, 240)), // Light gray thumb
        };
        _slider.ValueChanged += (s, e) =>
        {
            Value = (byte)_slider.Value;
            ValueChanged?.Invoke(this, Value);
        };

        var sliderContainer = new Grid
        {
            RowDefinitions = new RowDefinitions("*"),
            Children = { _gradientBorder, _slider }
        };
        SetColumn(sliderContainer, 1);

        _valueLabel = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(10, 0, 0, 0),
            MinWidth = 30
        };
        SetColumn(_valueLabel, 2);

        Children.Add(_label);
        Children.Add(sliderContainer);
        Children.Add(_valueLabel);

        PropertyChanged += (s, e) =>
        {
            if (e.Property == ValueProperty)
            {
                if (Math.Abs(_slider.Value - Value) > 0.01)
                {
                    _slider.Value = Value;
                }
                _valueLabel.Text = Value.ToString();
            }
            else if (e.Property == StartColorProperty || e.Property == EndColorProperty)
            {
                UpdateGradient();
            }
        };
    }

    private void UpdateGradient()
    {
        if (_gradientBorder.Child is Border border && border.Background is LinearGradientBrush brush)
        {
            if (brush.GradientStops.Count >= 2)
            {
                brush.GradientStops[0] = new GradientStop { Color = StartColor, Offset = 0 };
                brush.GradientStops[1] = new GradientStop { Color = EndColor, Offset = 1 };
            }
        }
    }
}
