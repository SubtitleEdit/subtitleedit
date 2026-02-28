using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Projektanker.Icons.Avalonia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public static class UiUtil
{
    public const int WindowMarginWidth = 12;
    public const int CornerRadius = 4;
    public const int SplitterWidthOrHeight = 4;

    public static ControlTheme DataGridNoBorderCellTheme => GetDataGridNoBorderCellTheme();

    private static ControlTheme GetDataGridNoBorderCellTheme()
    {
        var showVertical =
            Se.Settings.Appearance.GridLinesAppearance == DataGridGridLinesVisibility.Vertical.ToString() ||
            Se.Settings.Appearance.GridLinesAppearance == DataGridGridLinesVisibility.All.ToString();

        var compactMode = Se.Settings.Appearance.GridCompactMode;

        return new ControlTheme(typeof(DataGridCell))
        {
            Setters =
            {
                new Setter(DataGridCell.BackgroundProperty, Brushes.Transparent),
                new Setter(DataGridCell.FocusAdornerProperty, null),
                new Setter(DataGridCell.PaddingProperty, new Thickness(compactMode ? 0 : 4)),
                new Setter(DataGridCell.BorderBrushProperty, GetBorderBrush()),
                new Setter(DataGridCell.BorderThicknessProperty,
                    new Thickness(0, 0, showVertical ? 1 : 0, 0)), // 1px vertical line
            }
        };
    }

    public static ControlTheme DataGridNoBorderNoPaddingCellTheme => GetDataGridNoBorderNoPaddingCellTheme();

    private static ControlTheme GetDataGridNoBorderNoPaddingCellTheme()
    {
        var showVertical =
            Se.Settings.Appearance.GridLinesAppearance == DataGridGridLinesVisibility.Vertical.ToString() ||
            Se.Settings.Appearance.GridLinesAppearance == DataGridGridLinesVisibility.All.ToString();

        return new ControlTheme(typeof(DataGridCell))
        {
            Setters =
            {
                new Setter(DataGridCell.BackgroundProperty, Brushes.Transparent),
                new Setter(DataGridCell.FocusAdornerProperty, null),
                new Setter(DataGridCell.PaddingProperty, new Thickness(0)),
                new Setter(DataGridCell.BorderBrushProperty, GetBorderBrush()),
                new Setter(DataGridCell.BorderThicknessProperty,
                    new Thickness(0, 0, showVertical ? 1 : 0, 0)), // 1px vertical line
            }
        };
    }

    public static Button MakeButton(string text)
    {
        return MakeButton(text, null);
    }

    public static string GetDefaultFontName()
    {
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.FontName))
        {
            return Se.Settings.Appearance.FontName;
        }

        var systemFontNames = FontHelper.GetSystemFonts();
        var goodFontNames = new List<string>()
        {
            "Segoe UI",
            "San Francisco",
            "SF Pro Text",
            "Roboto",
            "Open Sans",
            "Lato",
            "Source Sans Pro",
            "Calibri",
            "Verdana",
            "Tahoma",
            "Inter",
            "Noto Sans",
            "System UI",
            "Arial",
        };

        foreach (var goodFontName in goodFontNames)
        {
            if (systemFontNames.Contains(goodFontName))
            {
                return goodFontName;
            }
        }

        return systemFontNames.First();
    }

    public static IBrush GetTextColor(double opacity = 1.0)
    {
        var app = Application.Current;
        if (app == null)
        {
            return new SolidColorBrush(Colors.Black, opacity);
        }

        var theme = app.ActualThemeVariant;
        if (theme == ThemeVariant.Dark)
        {
            return new SolidColorBrush(Colors.White, opacity);
        }

        return new SolidColorBrush(Colors.Black, opacity);
    }

    public static IBrush GetBorderBrush()
    {
        var app = Application.Current;
        if (app == null)
        {
            return new SolidColorBrush(Colors.Black);
        }

        var theme = app.ActualThemeVariant;
        if (theme == ThemeVariant.Dark)
        {
            return new SolidColorBrush(Colors.White, 0.5);
        }

        return new SolidColorBrush(Colors.Black, 0.5);
    }

    public static Color GetBorderColor()
    {
        var color = Colors.Black;

        var app = Application.Current;
        if (app != null)
        {
            var theme = app.ActualThemeVariant;
            if (theme == ThemeVariant.Dark)
            {
                color = Colors.White;
            }
        }

        return new Color(128, color.R, color.G, color.B);
    }

    public static Separator MakeVerticalSeperator(double height = 0.5, double opacity = 0.5, Thickness? margin = null,
        IBrush? backgroud = null)
    {
        return new Separator
        {
            Height = height,
            Margin = margin ?? new Thickness(5, 1),
            Background = backgroud ?? GetBorderBrush(),
            Opacity = opacity,
        };
    }

    public static Border MakeHorizontalSeperator(double width = 2.5, double opacity = 0.5, Thickness? margin = null,
        IBrush? backgroud = null)
    {
        return new Border
        {
            Width = width,
            Margin = margin ?? new Thickness(1, 5),
            Background = backgroud ?? GetBorderBrush(),
            Opacity = opacity,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Center
        };
    }

    public static Button MakeButton(string text, IRelayCommand? command, object? parameter = null)
    {
        var button = new Button
        {
            Content = text,
            Margin = new Thickness(4, 0),
            Padding = new Thickness(12, 6),
            MinWidth = 80,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
            CommandParameter = parameter
        };

        if (Se.Settings.Appearance.UseFocusedButtonBackgroundColor)
        {
            var focusStyle = new Style(x => x.OfType<Button>().Class(":focus"));
            focusStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Se.Settings.Appearance.FocusedButtonBackgroundColor.FromHexToColor())));
            //focusStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            button.Styles.Add(focusStyle);
        }

        return button;
    }

    public static Button MakeBrowseButton(IRelayCommand? command)
    {
        return new Button
        {
            Content = "...",
            Margin = new Thickness(4, 0),
            Padding = new Thickness(6, 6),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
        };
    }

    public static Button MakeButtonOk(IRelayCommand? command)
    {
        return MakeButton(Se.Language.General.Ok, command);
    }

    public static Button MakeButtonDone(IRelayCommand? command)
    {
        return MakeButton(Se.Language.General.Done, command);
    }

    public static Button MakeButtonCancel(IRelayCommand? command)
    {
        return MakeButton(Se.Language.General.Cancel, command);
    }

    public static Button MakeButton(IRelayCommand? command, string iconName)
    {
        var button = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
        };

        Attached.SetIcon(button, iconName);

        return button;
    }

    public static Button MakeButton(IRelayCommand? command, string iconName, int fontSize)
    {
        var button = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
            FontSize = fontSize,
        };

        Attached.SetIcon(button, iconName);

        return button;
    }

    public static Button MakeButton(IRelayCommand? command, string iconName, string hint)
    {
        var button = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
            [ToolTip.TipProperty] = hint,
        };

        Attached.SetIcon(button, iconName);

        return button;
    }


    public static Button MakeButtonBrowse(IRelayCommand? command, string? propertyIsVisiblePath = null)
    {
        var button = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
        };

        if (propertyIsVisiblePath != null)
        {
            button.Bind(Button.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
            });
        }

        Attached.SetIcon(button, IconNames.DotsHorizontal);

        return button;
    }

    public static Button BindIsEnabled(this Button control, object viewModal, string propertyIsEnabledPath)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Path = propertyIsEnabledPath,
            Mode = BindingMode.OneWay,
            Source = viewModal,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        return control;
    }

    public static ComboBox BindIsEnabled(this ComboBox control, object viewModal, string propertyIsEnabledPath)
    {
        control.Bind(ComboBox.IsEnabledProperty, new Binding
        {
            Path = propertyIsEnabledPath,
            Mode = BindingMode.OneWay,
            Source = viewModal,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        return control;
    }

    public static ComboBox BindIsEnabled(this ComboBox control, object viewModal, string propertyIsEnabledPath,
        IValueConverter converter)
    {
        control.Bind(ComboBox.IsEnabledProperty, new Binding
        {
            Path = propertyIsEnabledPath,
            Mode = BindingMode.OneWay,
            Source = viewModal,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Converter = converter,
        });

        return control;
    }

    public static Button BindIsEnabled(this Button control, object viewModal, string propertyIsEnabledPath,
        IValueConverter converter)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Path = propertyIsEnabledPath,
            Mode = BindingMode.OneWay,
            Source = viewModal,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Converter = converter,
        });

        return control;
    }

    public static TextBox BindIsEnabled(this TextBox control, object viewModal, string propertyIsEnabledPath,
        IValueConverter converter)
    {
        control.Bind(TextBox.IsEnabledProperty, new Binding
        {
            Path = propertyIsEnabledPath,
            Mode = BindingMode.OneWay,
            Source = viewModal,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Converter = converter,
        });

        return control;
    }

    public static ComboBox MakeComboBox<T>(
        ObservableCollection<T> sourceItems,
        object viewModal,
        string? propertySelectedPath,
        string? propertyIsVisiblePath)
    {
        var comboBox = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        comboBox.ItemsSource = sourceItems;
        comboBox.DataContext = viewModal;

        if (propertySelectedPath != null)
        {
            comboBox.Bind(ComboBox.SelectedItemProperty, new Binding
            {
                Path = propertySelectedPath,
                Mode = BindingMode.TwoWay,
            });
        }

        if (propertyIsVisiblePath != null)
        {
            comboBox.Bind(ComboBox.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
                Mode = BindingMode.TwoWay,
            });
        }

        return comboBox;
    }

    internal static ComboBox MakeComboBoxBindText<T>(ObservableCollection<T> sourceItems, object vm, string textPath,
        string propertySelectedIndexPath)
    {
        var comboBox = new ComboBox
        {
            ItemsSource = sourceItems,
            DataContext = vm,
            DisplayMemberBinding = new Binding(textPath),
        };

        comboBox.Bind(ComboBox.SelectedIndexProperty, new Binding
        {
            Path = propertySelectedIndexPath,
            Mode = BindingMode.TwoWay,
        });


        return comboBox;
    }

    public static ComboBox MakeComboBox<T>(
        ObservableCollection<T> sourceItems,
        object viewModal,
        string? propertySelectedPath)
    {
        return MakeComboBox(sourceItems, viewModal, propertySelectedPath, null);
    }

    public static TextBox MakeTextBox(double width, object viewModel, string propertyTextPath)
    {
        return MakeTextBox(width, viewModel, propertyTextPath, null);
    }

    public static TextBox MakeTextBox(double width, object viewModel, string? propertyTextPath,
        string? propertyIsVisiblePath)
    {
        var textBox = new TextBox
        {
            Width = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };

        textBox.DataContext = viewModel;

        if (propertyTextPath != null)
        {
            textBox.Bind(TextBox.TextProperty, new Binding
            {
                Path = propertyTextPath,
                Mode = BindingMode.TwoWay,
            });
        }

        if (propertyIsVisiblePath != null)
        {
            textBox.Bind(TextBox.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
                Mode = BindingMode.TwoWay,
            });
        }

        return textBox;
    }

    public static TextBlock MakeTextBlock(string text)
    {
        return new TextBlock
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    public static TextBlock MakeTextBlock(string text, object viewModel, string? textPropertyPath,
        string? visibilityPropertyPath)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
        };

        if (textPropertyPath != null)
        {
            textBlock.Bind(TextBlock.TextProperty, new Binding
            {
                Path = textPropertyPath,
                Mode = BindingMode.TwoWay,
            });
        }

        if (visibilityPropertyPath != null)
        {
            textBlock.Bind(TextBlock.IsVisibleProperty, new Binding
            {
                Path = visibilityPropertyPath,
                Mode = BindingMode.TwoWay,
            });
        }

        return textBlock;
    }

    public static CheckBox MakeCheckBox()
    {
        return new CheckBox
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    public static CheckBox MakeCheckBox(object viewModel, string? isCheckedPropertyPath)
    {
        var checkBox = new CheckBox
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
        };

        if (isCheckedPropertyPath != null)
        {
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding
            {
                Path = isCheckedPropertyPath,
                Mode = BindingMode.TwoWay,
            });
        }

        return checkBox;
    }

    public static CheckBox MakeCheckBox(string text, object viewModel, string? isCheckedPropertyPath)
    {
        var checkBox = new CheckBox
        {
            Content = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
        };

        if (isCheckedPropertyPath != null)
        {
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding
            {
                Path = isCheckedPropertyPath,
                Mode = BindingMode.TwoWay,
            });
        }

        return checkBox;
    }


    public static TextBlock MakeLink(string text, IRelayCommand command)
    {
        var link = new TextBlock
        {
            Text = text,
            Foreground = MakeLinkForeground(),
            TextDecorations = TextDecorations.Underline,
            Cursor = new Cursor(StandardCursorType.Hand),
            Margin = new Thickness(0),
            Padding = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        link.PointerPressed += (_, __) =>
        {
            if (command.CanExecute(null))
            {
                command.Execute(null);
            }
        };

        return link;
    }

    public static SolidColorBrush MakeLinkForeground()
    {
        return new SolidColorBrush(Color.FromArgb(255, 30, 144, 255));
    }

    public static TextBlock MakeLink(string text, IRelayCommand command, object viewModel, string propertyTextPath)
    {
        var link = new TextBlock
        {
            Text = text,
            Foreground = MakeLinkForeground(),
            TextDecorations = TextDecorations.Underline,
            Cursor = new Cursor(StandardCursorType.Hand),
            Margin = new Thickness(0),
            Padding = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        link.PointerPressed += (_, __) =>
        {
            if (command.CanExecute(null))
            {
                command.Execute(null);
            }
        };

        link.Bind(TextBlock.TextProperty, new Binding
        {
            Path = propertyTextPath,
            Mode = BindingMode.TwoWay,
        });

        return link;
    }

    public static TextBlock WithMarginRight(this TextBlock control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return control;
    }

    public static TextBox WithMarginRight(this TextBox control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return control;
    }

    public static TextBox WithMarginLeft(this TextBox control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static TextBox WithMargin(this TextBox control, int margin)
    {
        control.Margin = new Thickness(margin);
        return control;
    }

    public static SplitButton WithMarginLeft(this SplitButton control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static TextBox WithHeight(this TextBox control, double height)
    {
        var m = control.Margin;
        control.Height = height;
        return control;
    }

    public static TextBlock WithMarginLeft(this TextBlock control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static TextBlock WithAlignmentLeft(this TextBlock control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Left;
        return control;
    }

    public static TextBlock WithAlignmentTop(this TextBlock control)
    {
        control.VerticalAlignment = VerticalAlignment.Top;
        return control;
    }

    public static TextBlock WithMarginBottom(this TextBlock control, int marginBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, m.Right, marginBottom);
        return control;
    }

    public static Label WithMarginBottom(this Label control, int marginBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, m.Right, marginBottom);
        return control;
    }

    public static Border WithMarginBottom(this Border control, int marginBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, m.Right, marginBottom);
        return control;
    }

    public static Border WithMinWidth(this Border control, int minWidth)
    {
        control.MinWidth = minWidth;
        return control;
    }

    public static Border WithMinHeight(this Border control, int minHeight)
    {
        control.MinHeight = minHeight;
        return control;
    }

    public static Border WithHeight(this Border control, int height)
    {
        control.Height = height;
        return control;
    }

    public static Border WithMarginRight(this Border control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return control;
    }

    public static TextBlock WithMarginTop(this TextBlock control, int topBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, topBottom, m.Right, m.Bottom);
        return control;
    }

    public static ComboBox WithMarginBottom(this ComboBox control, int marginBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, m.Right, marginBottom);
        return control;
    }

    public static ComboBox WithMargin(this ComboBox control, int left, int top, int right, int bottom)
    {
        control.Margin = new Thickness(left, top, right, bottom);
        return control;
    }

    public static Button WithMarginBottom(this Button control, int marginBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, m.Right, marginBottom);
        return control;
    }

    public static TextBlock WithBackgroundColor(this TextBlock control, IBrush brush)
    {
        control.Background = brush;
        return control;
    }

    public static Button WithLeftAlignment(this Button control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Left;
        return control;
    }

    public static SplitButton WithLeftAlignment(this SplitButton control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Left;
        return control;
    }

    public static Button WithRightAlignment(this Button control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Right;
        return control;
    }

    public static Button WithTopAlignment(this Button control)
    {
        control.VerticalAlignment = VerticalAlignment.Top;
        return control;
    }

    public static Button WithCenterAlignment(this Button control)
    {
        control.VerticalAlignment = VerticalAlignment.Center;
        return control;
    }

    public static Button WithBottomAlignment(this Button control)
    {
        control.VerticalAlignment = VerticalAlignment.Bottom;
        return control;
    }

    public static Button WithIconRight(this Button control, string iconName)
    {
        var label = new TextBlock() { Text = control.Content?.ToString(), Padding = new Thickness(0, 0, 4, 0) };
        var image = new ContentControl();
        Attached.SetIcon(image, iconName);
        var stackPanelApplyFixes = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { label, image }
        };

        control.Content = stackPanelApplyFixes;

        return control;
    }

    public static Button WithIconLeft(this Button control, string iconName)
    {
        var label = new TextBlock() { Text = control.Content?.ToString(), Padding = new Thickness(4, 0, 0, 0) };
        var image = new ContentControl();
        Attached.SetIcon(image, iconName);
        var stackPanelApplyFixes = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { image, label }
        };

        control.Content = stackPanelApplyFixes;

        return control;
    }

    public static Button WithCommandParameter<T>(this Button control, T parameter)
    {
        control.CommandParameter = parameter;
        return control;
    }

    public static ComboBox WithLeftAlignment(this ComboBox control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Left;
        return control;
    }

    public static ComboBox WithTopAlignment(this ComboBox control)
    {
        control.VerticalAlignment = VerticalAlignment.Top;
        return control;
    }

    public static Button WithBindEnabled(this Button control, string isEnabledPropertyPath)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static SplitButton WithBindEnabled(this SplitButton control, string isEnabledPropertyPath)
    {
        control.Bind(SplitButton.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static SplitButton WithBindIsVisible(this SplitButton control, string isVisiblePropertyPath)
    {
        control.Bind(SplitButton.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static ComboBox WithBindEnabled(this ComboBox control, string isEnabledPropertyPath)
    {
        control.Bind(ComboBox.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static ComboBox WithBindVisible(this ComboBox control, string isVisiblePropertyPath)
    {
        control.Bind(ComboBox.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static T WithHorizontalAlignmentStretch<T>(this T control) where T : Control
    {
        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        return control;
    }

    public static NumericUpDown WithBindEnabled(this NumericUpDown control, string isEnabledPropertyPath)
    {
        control.Bind(NumericUpDown.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindContent(this Button control, string contentPropertyPath)
    {
        control.Bind(Button.ContentProperty, new Binding
        {
            Path = contentPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static CheckBox WithBindEnabled(this CheckBox control, string isEnabledPropertyPath)
    {
        control.Bind(CheckBox.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBox WithBindEnabled(this TextBox control, string isEnabledPropertyPath)
    {
        control.Bind(TextBox.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBox WithBindEnabled(this TextBox control, string isEnabledPropertyPath, IValueConverter converter)
    {
        control.Bind(TextBox.IsEnabledProperty, new Binding
        {
            Converter = converter,
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindEnabled(this Button control, string isEnabledPropertyPath, IValueConverter converter)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Converter = converter,
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBox WithBindIsVisible(this TextBox control, string isVisiblePropertyPath)
    {
        control.Bind(TextBox.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBox WithBindIsVisible(this TextBox control, string isEnabledPropertyPath,
        IValueConverter converter)
    {
        control.Bind(TextBox.IsVisibleProperty, new Binding
        {
            Converter = converter,
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindIsVisible(this Button control, string isVisiblePropertyPath)
    {
        control.Bind(Button.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindIsVisible(this Button control, object viewModel, string isVisiblePropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(Button.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindIsVisible(this Button control, string isVisiblePropertyPath, IValueConverter converter)
    {
        control.Bind(Button.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
            Converter = converter,
        });

        return control;
    }

    public static Border WithBindIsVisible(this Border control, string isVisiblePropertyPath, IValueConverter converter)
    {
        control.Bind(Border.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
            Converter = converter,
        });

        return control;
    }

    public static Border WithBindIsVisible(this Border control, string isVisiblePropertyPath)
    {
        control.Bind(Border.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindIsEnabled(this Button control, string isEnabledPropertyPath)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Button WithBindIsEnabled(this Button control, string isEnabledPropertyPath, IValueConverter converter)
    {
        control.Bind(Button.IsEnabledProperty, new Binding
        {
            Path = isEnabledPropertyPath,
            Mode = BindingMode.TwoWay,
            Converter = converter,
        });

        return control;
    }

    public static ComboBox WithBindSelected(this ComboBox control, string selectedPropertyBinding)
    {
        control.Bind(ComboBox.SelectedItemProperty, new Binding
        {
            Path = selectedPropertyBinding,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBlock WithMargin(this TextBlock control, int margin)
    {
        control.Margin = new Thickness(margin);
        return control;
    }

    public static TextBlock WithPadding(this TextBlock control, int padding)
    {
        control.Padding = new Thickness(padding);
        return control;
    }

    public static TextBlock WithFontSize(this TextBlock control, double fontSize)
    {
        control.FontSize = fontSize;
        return control;
    }

    public static StackPanel WithMarginTop(this StackPanel control, int marginTop)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, marginTop, m.Right, m.Bottom);
        return control;
    }

    public static StackPanel WithMarginBottom(this StackPanel control, int marginBottom)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, m.Right, marginBottom);
        return control;
    }

    public static StackPanel WithAlignmentLeft(this StackPanel control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Left;
        return control;
    }

    public static StackPanel WithSpacing(this StackPanel control, int spacing)
    {
        control.Spacing = spacing;
        return control;
    }

    public static StackPanel WithAlignmentTop(this StackPanel control)
    {
        control.VerticalAlignment = VerticalAlignment.Top;
        return control;
    }

    public static Label WithAlignmentTop(this Label control)
    {
        control.VerticalAlignment = VerticalAlignment.Top;
        return control;
    }

    public static Label WithAlignmentBottom(this Label control)
    {
        control.VerticalAlignment = VerticalAlignment.Bottom;
        return control;
    }

    public static Label WithAlignmentRight(this Label control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Right;
        return control;
    }

    public static Label WithAlignmentCenter(this Label control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Center;
        return control;
    }

    public static Label HorizontalContentAlignmentCenter(this Label control)
    {
        control.HorizontalContentAlignment = HorizontalAlignment.Center;
        return control;
    }

    public static Label WithBold(this Label control)
    {
        control.FontWeight = FontWeight.Bold;
        return control;
    }

    public static Label WithMarginLeft(this Label control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static Label WithOpacity(this Label control, double opacity)
    {
        control.Opacity = opacity;
        return control;
    }

    public static Label WithMinWidth(this Label control, int minWidth)
    {
        control.MinWidth = minWidth;
        return control;
    }

    public static Label WithMinHeight(this Label control, int minHeight)
    {
        control.MinHeight = minHeight;
        return control;
    }

    public static Label WithMarginRight(this Label control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return control;
    }

    public static Label WithMarginTop(this Label control, int marginTop)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, marginTop, m.Right, m.Bottom);
        return control;
    }

    public static Label WithFontSize(this Label control, int fontSize)
    {
        control.FontSize = fontSize;
        return control;
    }

    public static ComboBox WithMarginTop(this ComboBox control, int marginTop)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, marginTop, m.Right, m.Bottom);
        return control;
    }

    public static ComboBox WithMarginLeft(this ComboBox control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static ComboBox WithMarginRight(this ComboBox control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return control;
    }

    public static Button WithMarginTop(this Button control, int marginTop)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, marginTop, m.Right, m.Bottom);
        return control;
    }

    public static SplitButton WithMarginTop(this SplitButton control, int marginTop)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, marginTop, m.Right, m.Bottom);
        return control;
    }

    public static Button WithFontSize(this Button control, double fontSize)
    {
        control.FontSize = fontSize;
        return control;
    }

    public static Button WithMarginLeft(this Button control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static CheckBox WithMarginLeft(this CheckBox control, int marginLeft)
    {
        var m = control.Margin;
        control.Margin = new Thickness(marginLeft, m.Top, m.Right, m.Bottom);
        return control;
    }

    public static Button WithMarginRight(this Button control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return control;
    }

    public static CheckBox WithMarginRight(this CheckBox control, int marginRight)
    {
        var m = control.Margin;
        control.Margin = new Thickness(m.Left, m.Top, marginRight, m.Bottom);
        return control;
    }

    public static Button Compact(this Button control)
    {
        var m = control.Padding;
        control.Padding = new Thickness(8, m.Top, 8, m.Bottom);
        control.MinWidth = 0;
        return control;
    }

    public static Button WithMargin(this Button control, int margin)
    {
        control.Margin = new Thickness(margin);
        return control;
    }

    public static Button WithPadding(this Button control, int padding)
    {
        control.Padding = new Thickness(padding);
        return control;
    }

    public static Button WithMargin(this Button control, int left, int top, int right, int bottom)
    {
        control.Margin = new Thickness(left, top, right, bottom);
        return control;
    }

    public static Button WithBold(this Button control)
    {
        control.FontWeight = FontWeight.Bold;
        return control;
    }

    public static TextBlock WithMinwidth(this TextBlock control, int width)
    {
        control.MinWidth = width;
        return control;
    }

    public static Button WithMinWidth(this Button control, int width)
    {
        control.MinWidth = width;
        return control;
    }

    public static SplitButton WithMinWidth(this SplitButton control, int width)
    {
        control.MinWidth = width;
        return control;
    }

    public static Button WithMinHeight(this Button control, int height)
    {
        control.MinHeight = height;
        return control;
    }

    public static Button WithParameter(this Button control, object parameter)
    {
        control.CommandParameter = parameter;
        return control;
    }

    public static ComboBox WithMinWidth(this ComboBox control, int width)
    {
        control.MinWidth = width;
        return control;
    }

    public static ComboBox WithWidth(this ComboBox control, int width)
    {
        control.Width = width;
        return control;
    }

    public static StackPanel MakeButtonBar(params Control[] buttons)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(10, 20, 10, 10),
            Spacing = 0,
            Height = double.NaN, // Allow it to grow vertically if needed
        };

        stackPanel.Children.AddRange(buttons);

        return stackPanel;
    }

    public static StackPanel MakeControlBarLeft(params Control[] buttons)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(10),
            Spacing = 0,
        };

        stackPanel.Children.AddRange(buttons);

        return stackPanel;
    }

    public static Border MakeSeparatorForHorizontal(object vm)
    {
        return new Border
        {
            DataContext = vm,
            Width = 1,
            Background = GetBorderBrush(),
            Margin = new Thickness(5, 5, 5, 5),
            VerticalAlignment = VerticalAlignment.Stretch,
        };
    }

    public static Border MakeBorderForControl(Control control)
    {
        return new Border
        {
            Child = control,
            BorderThickness = new Thickness(1),
            BorderBrush = GetTextColor(0.3d),
            Padding = new Thickness(5),
            CornerRadius = new CornerRadius(CornerRadius),
        };
    }

    public static Border MakeBorderForControlNoPadding(Control control)
    {
        return new Border
        {
            Child = control,
            BorderThickness = new Thickness(1),
            BorderBrush = GetTextColor(0.3d),
            CornerRadius = new CornerRadius(CornerRadius),
        };
    }

    public static T BindIsVisible<T>(this T control, object vm, string visibilityPropertyPath) where T : Visual
    {
        control.DataContext = vm;
        control.Bind(Visual.IsVisibleProperty, new Binding
        {
            Path = visibilityPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static T BindText<T>(this T control, object vm, string textPropertyPath) where T : TextBox
    {
        control.DataContext = vm;
        control.Bind(TextBox.TextProperty, new Binding
        {
            Path = textPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static WindowIcon? GetSeIcon()
    {
        return new WindowIcon(AssetLoader.Open(new Uri("avares://SubtitleEdit/Assets/se.ico")));
    }

    public static Control RemoveControlFromParent(this Control control)
    {
        if (control.Parent is Panel parent)
        {
            parent.Children.Remove(control);
        }
        else if (control.Parent is Decorator decorator)
        {
            if (decorator.Child == control)
            {
                decorator.Child = null;
            }
        }
        else if (control.Parent is ContentControl contentControl)
        {
            if (contentControl.Content == control)
            {
                contentControl.Content = null;
            }
        }

        return control;
    }

    public static Control AddControlToParent(this Control control, Control parent)
    {
        if (parent is Panel panel)
        {
            panel.Children.Add(control);
        }
        else if (parent is Decorator decorator)
        {
            decorator.Child = control;
        }
        else if (parent is ContentControl contentControl2)
        {
            contentControl2.Content = control;
        }

        return control;
    }

    internal static Thickness MakeWindowMargin()
    {
        return new Thickness(WindowMarginWidth, WindowMarginWidth * 2, WindowMarginWidth, WindowMarginWidth);
    }

    internal static ColorPicker MakeColorPicker(object vm, string colorPropertyPath)
    {
        return new ColorPicker
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            IsAlphaEnabled = true,
            IsAlphaVisible = true,
            IsColorSpectrumSliderVisible = false,
            IsColorComponentsVisible = true,
            IsColorModelVisible = false,
            IsColorPaletteVisible = false,
            IsAccentColorsVisible = false,
            IsColorSpectrumVisible = true,
            IsComponentTextInputVisible = true,
            [!ColorPicker.ColorProperty] = new Binding(colorPropertyPath)
            {
                Source = vm,
                Mode = BindingMode.TwoWay
            },
        };
    }

    internal static Label MakeLabel(string text = "")
    {
        return new Label
        {
            Content = text,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    internal static Label MakeLabel(string text, string propertyIsVisiblePath)
    {
        var label = new Label
        {
            Content = text,
            VerticalAlignment = VerticalAlignment.Center,
        };

        label.Bind(ComboBox.IsVisibleProperty, new Binding
        {
            Path = propertyIsVisiblePath,
            Mode = BindingMode.TwoWay,
        });

        return label;
    }

    internal static Label MakeLabel(Binding binding)
    {
        var label = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
        };

        label.Bind(Label.ContentProperty, binding);

        return label;
    }

    internal static RadioButton MakeRadioButton(string text, object viewModel, string isCheckedPropertyPath)
    {
        return MakeRadioButton(text, viewModel, isCheckedPropertyPath, null);
    }

    internal static RadioButton MakeRadioButton(string text, object viewModel, string isCheckedPropertyPath,
        string? groupName)
    {
        var control = new RadioButton
        {
            Content = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
            GroupName = groupName,
        };

        if (isCheckedPropertyPath != null)
        {
            control.Bind(RadioButton.IsCheckedProperty, new Binding
            {
                Path = isCheckedPropertyPath,
                Mode = BindingMode.TwoWay,
            });
        }

        return control;
    }

    public static NumericUpDown MakeNumericUpDownInt(int min, int max, int defaultValue, double width, object viewModel,
        string? propertyValuePath = null, string? propertyIsVisiblePath = null)
    {
        var control = new NumericUpDown
        {
            Width = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
            Minimum = min,
            Maximum = max,
            Increment = 1,
            FormatString = "F0",
            Foreground = GetTextColor(),
        };

        if (propertyValuePath != null)
        {
            control.Bind(NumericUpDown.ValueProperty, new Binding
            {
                Path = propertyValuePath,
                Mode = BindingMode.TwoWay,
                Converter = new NullableIntConverter { DefaultValue = defaultValue },
            });
        }

        if (propertyIsVisiblePath != null)
        {
            control.Bind(NumericUpDown.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
                Mode = BindingMode.TwoWay,
            });
        }

        return control;
    }


    public static NumericUpDown MakeNumericUpDownTwoDecimals(decimal min, decimal max, double width, object viewModel,
        string? propertyValuePath = null, string? propertyIsVisiblePath = null)
    {
        var control = new NumericUpDown
        {
            Width = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
            Minimum = min,
            Maximum = max,
            Increment = 0.01m,
            FormatString = "F2", // Force two decimals
            Foreground = GetTextColor(),
        };

        if (propertyValuePath != null)
        {
            control.Bind(NumericUpDown.ValueProperty, new Binding
            {
                Path = propertyValuePath,
                Mode = BindingMode.TwoWay,
            });
        }

        if (propertyIsVisiblePath != null)
        {
            control.Bind(NumericUpDown.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
                Mode = BindingMode.TwoWay,
            });
        }

        return control;
    }

    public static NumericUpDown MakeNumericUpDownThreeDecimals(decimal min, decimal max, double width, object viewModel,
        string? propertyValuePath = null, string? propertyIsVisiblePath = null)
    {
        var control = new NumericUpDown
        {
            Width = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
            Minimum = min,
            Maximum = max,
            Increment = 0.01m,
            FormatString = "F3" // Force three decimals
        };

        if (propertyValuePath != null)
        {
            control.Bind(NumericUpDown.ValueProperty, new Binding
            {
                Path = propertyValuePath,
                Mode = BindingMode.TwoWay,
            });
        }

        if (propertyIsVisiblePath != null)
        {
            control.Bind(NumericUpDown.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
                Mode = BindingMode.TwoWay,
            });
        }

        return control;
    }

    public static NumericUpDown MakeNumericUpDownOneDecimal(decimal min, decimal max, double width, object viewModel,
        string? propertyValuePath = null, string? propertyIsVisiblePath = null)
    {
        var control = new NumericUpDown
        {
            Width = width,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
            Minimum = min,
            Maximum = max,
            Increment = 0.1m,
            FormatString = "F1",
        };

        if (propertyValuePath != null)
        {
            control.Bind(NumericUpDown.ValueProperty, new Binding
            {
                Path = propertyValuePath,
                Mode = BindingMode.TwoWay,
            });
        }

        if (propertyIsVisiblePath != null)
        {
            control.Bind(NumericUpDown.IsVisibleProperty, new Binding
            {
                Path = propertyIsVisiblePath,
                Mode = BindingMode.TwoWay,
            });
        }

        return control;
    }

    public static Label WithBindText(this Label control, object viewModel, string contentPropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(Label.ContentProperty, new Binding
        {
            Path = contentPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Label WithBindText(this Label control, object viewModel, string contentPropertyPath, IValueConverter valueConverter)
    {
        control.DataContext = viewModel;
        control.Bind(Label.ContentProperty, new Binding
        {
            Path = contentPropertyPath,
            Mode = BindingMode.TwoWay,
            Converter = valueConverter,
        });

        return control;
    }


    public static Label WithBindText(this Label control, object viewModel, Binding binding)
    {
        control.DataContext = viewModel;
        control.Bind(Label.ContentProperty, binding);

        return control;
    }

    public static TextBlock WithBindText(this TextBlock control, object viewModel, string contentPropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(TextBlock.TextProperty, new Binding
        {
            Path = contentPropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Label WithBindVisible(this Label control, object viewModel, string visiblePropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(Label.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Label WithBorderColorAsColor(this Label control)
    {
        control.Foreground = GetBorderBrush();

        return control;
    }

    public static Grid WithBindVisible(this Grid control, object viewModel, string visiblePropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(Grid.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBlock WithBindVisible(this TextBlock control, object viewModel, string visiblePropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(TextBlock.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static TextBlock WithBindEnabed(this TextBlock control, object viewModel, string visiblePropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(TextBlock.IsEnabledProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static Label WithBindVisible(this Label control, object viewModel, string visiblePropertyPath,
        IValueConverter converter)
    {
        control.DataContext = viewModel;
        control.Bind(Label.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
            Converter = converter,
        });

        return control;
    }

    public static StackPanel WithBindVisible(this StackPanel control, object viewModel, string visiblePropertyPath)
    {
        control.DataContext = viewModel;
        control.Bind(StackPanel.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
        });

        return control;
    }

    public static StackPanel WithBindVisible(this StackPanel control, object viewModel, string visiblePropertyPath,
        IValueConverter converter)
    {
        control.DataContext = viewModel;
        control.Bind(StackPanel.IsVisibleProperty, new Binding
        {
            Path = visiblePropertyPath,
            Mode = BindingMode.TwoWay,
            Converter = converter,
        });

        return control;
    }

    private static bool IsDarkTheme()
    {
        var app = Application.Current;
        if (app == null)
        {
            return false;
        }

        var theme = app.ActualThemeVariant;
        return theme == ThemeVariant.Dark;
    }

    public static void DrawCheckerboardBackground(SKCanvas canvas, int width, int height, int squareSize = 16)
    {
        // Define colors for the checkerboard pattern        
        var lightColor = SKColor.Parse("#EEEEEE");
        var darkColor = SKColor.Parse("#BBBBBB");

        if (UiUtil.IsDarkTheme())
        {
            lightColor = SKColor.Parse("#333333"); // Darker color for light squares in dark theme
            darkColor = SKColor.Parse("#555555"); // Lighter color for dark squares in dark theme
        }

        using (var lightPaint = new SKPaint { Color = lightColor, Style = SKPaintStyle.Fill })
        using (var darkPaint = new SKPaint { Color = darkColor, Style = SKPaintStyle.Fill })
        {
            // Calculate number of squares needed
            var cols = (int)Math.Ceiling((double)width / squareSize);
            var rows = (int)Math.Ceiling((double)height / squareSize);

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    // Determine if this square should be light or dark
                    var isLight = (row + col) % 2 == 0;
                    var paint = isLight ? lightPaint : darkPaint;

                    // Calculate square position and size
                    var rect = new SKRect(
                        col * squareSize,
                        row * squareSize,
                        Math.Min((col + 1) * squareSize, width),
                        Math.Min((row + 1) * squareSize, height)
                    );

                    canvas.DrawRect(rect, paint);
                }
            }
        }
    }

    public static void SetFontName(string fontName)
    {
        if (Application.Current == null || string.IsNullOrEmpty(Se.Settings.Appearance.FontName))
        {
            return;
        }

        Application.Current.Styles.Add(new Style(x => x.OfType<TextBlock>())
        {
            Setters =
            {
                new Setter(TextBlock.FontFamilyProperty, new FontFamily(fontName)),
            }
        });

        Application.Current.Styles.Add(new Style(x => x.OfType<TextBox>())
        {
            Setters =
            {
                new Setter(TextBox.FontFamilyProperty, new FontFamily(fontName)),
            }
        });

        Application.Current.Styles.Add(new Style(x => x.OfType<Button>())
        {
            Setters =
            {
                new Setter(Button.FontFamilyProperty, new FontFamily(fontName)),
            }
        });

        Application.Current.Styles.Add(new Style(x => x.OfType<Avalonia.Controls.MenuItem>())
        {
            Setters =
            {
                new Setter(Avalonia.Controls.MenuItem.FontFamilyProperty, new FontFamily(fontName)),
            }
        });

        Application.Current.Styles.Add(new Style(x => x.OfType<Label>())
        {
            Setters =
            {
                new Setter(Label.FontFamilyProperty, new FontFamily(fontName)),
            }
        });

        Application.Current.Styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(ComboBox.FontFamilyProperty, new FontFamily(fontName)),
            }
        });
    }

    public static StackPanel MakeHorizontalPanel(params Control[] controls)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5,
        };

        stackPanel.Children.AddRange(controls);

        return stackPanel;
    }

    public static StackPanel MakeVerticalPanel(params Control[] controls)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5,
        };

        stackPanel.Children.AddRange(controls);

        return stackPanel;
    }

    public static Color LightenColor(Color color, byte adjustValue)
    {
        var r = (byte)Math.Min(255, color.R + adjustValue);
        var g = (byte)Math.Min(255, color.G + adjustValue);
        var b = (byte)Math.Min(255, color.B + adjustValue);

        return Color.FromArgb(color.A, r, g, b);
    }

    internal static DataGridGridLinesVisibility GetGridLinesVisibility()
    {
        if (Se.Settings.Appearance.GridLinesAppearance == DataGridGridLinesVisibility.Horizontal.ToString())
        {
            return DataGridGridLinesVisibility.Horizontal;
        }

        if (Se.Settings.Appearance.GridLinesAppearance == DataGridGridLinesVisibility.Vertical.ToString())
        {
            return DataGridGridLinesVisibility.Vertical;
        }

        if (Se.Settings.Appearance.GridLinesAppearance == DataGridGridLinesVisibility.All.ToString())
        {
            return DataGridGridLinesVisibility.All;
        }

        return DataGridGridLinesVisibility.None;
    }

    internal static Color GetDarkThemeBackgroundColor()
    {
        return Se.Settings.Appearance.DarkModeBackgroundColor.FromHexToColor();
    }

    internal static void ReplaceControl(Control old, Control replacement)
    {
        var replacementParent = replacement.Parent;
        if (replacementParent != null)
        {
            if (replacementParent is Panel panelReplacement)
            {
                panelReplacement.Children.Remove(replacement);
            }
            else if (replacementParent is ContentControl contentControl)
            {
                contentControl.Content = null;
            }
            else if (replacementParent is Grid grid)
            {
                grid.Children.Remove(replacement);
            }
        }

        var parent = old.Parent;
        if (parent is Panel panel)
        {
            var index = panel.Children.IndexOf(old);
            if (index >= 0)
            {
                panel.Children[index] = replacement;
            }
        }
        else if (parent is ContentControl contentControl)
        {
            contentControl.Content = replacement;
        }
        else if (parent is Grid grid)
        {
            var index = grid.Children.IndexOf(old);
            if (index >= 0)
            {
                grid.Children[index] = replacement;
            }
        }
    }

    public static string? MakeToolTip(string hint, List<ShortCut> shortcuts, string shortcutName = "")
    {
        string shortcutString = MakeShortcutsString(shortcuts, shortcutName);

        return Se.Settings.Appearance.ShowHints
            ? string.Format(hint, shortcutString).Trim()
            : null;
    }

    public static string MakeShortcutsString(List<ShortCut> shortcuts, string shortcutName)
    {
        var shortcut = shortcuts.FirstOrDefault(s => s.Name == shortcutName);
        var shortcutString = string.Empty;
        if (shortcut is { Keys.Count: > 0 })
        {
            shortcutString = string.Join("+", shortcut.Keys.Select(k => ShortcutManager.GetKeyDisplayName(k.ToString())));
            shortcutString = $"({shortcutString})";
        }

        return shortcutString;
    }

    internal static void InitializeWindow(Window window, string name)
    {
        window.Icon = GetSeIcon();
        window.Name = name;
    }

    public static void SaveWindowPosition(Window? window)
    {
        if (!Se.Settings.General.RememberPositionAndSize || window == null || window.Name == null)
        {
            return;
        }

        var state = SeWindowPosition.SaveState(window);

        var existing = Se.Settings.General.WindowPositions.FirstOrDefault(wp => wp.WindowName == state.WindowName);
        if (existing != null)
        {
            Se.Settings.General.WindowPositions.Remove(existing);
        }

        Se.Settings.General.WindowPositions.Add(state);
    }

    public static void RestoreWindowPosition(Window? window)
    {
        if (!Se.Settings.General.RememberPositionAndSize || window == null)
        {
            return;
        }

        var existing = Se.Settings.General.WindowPositions.FirstOrDefault(wp => wp.WindowName == window.Name);
        if (existing == null)
        {
            return;
        }

        // Reconstruct the last known rect
        var desired = new PixelPoint(existing.X, existing.Y);
        var windowRect = new PixelRect(desired, new PixelSize((int)existing.Width, (int)existing.Height));

        var screens = window.Screens.All;
        bool fits = screens.Any(s => s.Bounds.Intersects(windowRect));

        if (!fits)
        {
            // fallback: check if the old screen bounds still exist
            var targetScreen = screens.FirstOrDefault(s =>
                s.Bounds.X == existing.ScreenX &&
                s.Bounds.Y == existing.ScreenY &&
                s.Bounds.Width == existing.ScreenWidth &&
                s.Bounds.Height == existing.ScreenHeight);

            if (targetScreen != null)
            {
                // center on that screen
                var px = targetScreen.Bounds.X + (targetScreen.Bounds.Width - (int)existing.Width) / 2;
                var py = targetScreen.Bounds.Y + (targetScreen.Bounds.Height - (int)existing.Height) / 2;
                desired = new PixelPoint(px, py);
            }
            else
            {
                // ultimate fallback: center on primary screen
                var primary = window.Screens.Primary;
                if (primary != null)
                {
                    var px = primary.Bounds.X + (primary.Bounds.Width - (int)existing.Width) / 2;
                    var py = primary.Bounds.Y + (primary.Bounds.Height - (int)existing.Height) / 2;
                    desired = new PixelPoint(px, py);
                }
            }
        }

        window.Position = desired;

        if (existing.IsFullScreen)
        {
            window.WindowState = WindowState.FullScreen;
        }
        else if (existing.IsMaximized)
        {
            window.WindowState = WindowState.Maximized;
        }
        else
        {
            if (existing.Width > 0 && existing.Height > 0)
            {
                window.Width = existing.Width;
                window.Height = existing.Height;
            }

            window.WindowState = WindowState.Normal;
        }
    }

    public static void ShowHelp(string helpName)
    {
        var helpUrl = string.Format($"http://subtitleedit.github.io/subtitleedit/{helpName}.html");
        OpenUrl(helpUrl);
    }

    public static void ShowHelp()
    {
        var helpUrl = string.Format($"http://subtitleedit.github.io/subtitleedit");
        OpenUrl(helpUrl);
    }

    public static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // UseShellExecute might not work on some platforms, try platform-specific commands
            if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", url);
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    internal static bool IsHelp(KeyEventArgs e)
    {
        return e.Key == Key.F1;
    }
}