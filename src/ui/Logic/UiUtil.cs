using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Platform.Windows;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Optris.Icons.Avalonia;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Nikse.SubtitleEdit.Logic;

public static class UiUtil
{
    public const int WindowMarginWidth = 12;
    public const int CornerRadius = 4;
    public const int SplitterWidthOrHeight = 4;

    /// <summary>
    /// Grid lines for <see cref="TableView"/>, which - unlike DataGrid - has no
    /// GridLinesVisibility property. Drawn as cell borders from the same
    /// Appearance.GridLinesAppearance setting (a <see cref="DataGridGridLinesVisibility"/>
    /// name) and compact-mode padding the DataGrid cell themes above use, so both
    /// controls honour the user's "Show grid lines" choice identically.
    /// </summary>
    public static ControlTheme TableViewCellTheme => GetTableViewCellTheme(noPadding: false);

    /// <summary>As <see cref="TableViewCellTheme"/> but with no cell padding, for cells hosting their own controls.</summary>
    public static ControlTheme TableViewNoPaddingCellTheme => GetTableViewCellTheme(noPadding: true);

    private static ControlTheme GetTableViewCellTheme(bool noPadding)
    {
        var showVertical =
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.Vertical) ||
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.All);

        var showHorizontal =
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.Horizontal) ||
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.All);

        var padding = noPadding || Se.Settings.Appearance.GridCompactMode ? 0 : 4;

        return new ControlTheme(typeof(TableViewCell))
        {
            Setters =
            {
                new Setter(TableViewCell.BackgroundProperty, Brushes.Transparent),
                new Setter(TableViewCell.PaddingProperty, new Thickness(padding)),
                new Setter(TableViewCell.BorderBrushProperty, GetBorderBrush()),
                new Setter(TableViewCell.BorderThicknessProperty,
                    new Thickness(0, 0, showVertical ? 1 : 0, showHorizontal ? 1 : 0)), // vertical and horizontal lines
                new Setter(TableViewCell.TemplateProperty, TableViewCellTemplate),
            }
        };
    }

    // TableViewCell's built-in template only template-binds Background, so the border
    // properties set above would never be drawn. Bind them through to the presenter
    // (which renders its own border) so the grid lines actually appear.
    private static readonly FuncControlTemplate<TableViewCell> TableViewCellTemplate =
        new((_, scope) => new ContentPresenter
        {
            Name = "PART_ContentPresenter",
            [!ContentPresenter.ContentProperty] = new TemplateBinding(ContentControl.ContentProperty),
            [!ContentPresenter.ContentTemplateProperty] = new TemplateBinding(ContentControl.ContentTemplateProperty),
            [!ContentPresenter.BackgroundProperty] = new TemplateBinding(TemplatedControl.BackgroundProperty),
            [!ContentPresenter.BorderBrushProperty] = new TemplateBinding(TemplatedControl.BorderBrushProperty),
            [!ContentPresenter.BorderThicknessProperty] = new TemplateBinding(TemplatedControl.BorderThicknessProperty),
            [!ContentPresenter.PaddingProperty] = new TemplateBinding(TemplatedControl.PaddingProperty),
            [!ContentPresenter.HorizontalContentAlignmentProperty] = new TemplateBinding(ContentControl.HorizontalContentAlignmentProperty),
            [!ContentPresenter.VerticalContentAlignmentProperty] = new TemplateBinding(ContentControl.VerticalContentAlignmentProperty),
        }.RegisterInNameScope(scope));

    public static ControlTheme DataGridNoBorderCellTheme => GetDataGridNoBorderCellTheme();

    private static ControlTheme GetDataGridNoBorderCellTheme()
    {
        var showVertical =
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.Vertical) ||
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.All);

        var showHorizontal =
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.Horizontal) ||
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.All);

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
                    new Thickness(0, 0, showVertical ? 1 : 0, showHorizontal ? 1 : 0)), // vertical and horizontal lines
            }
        };
    }

    public static ControlTheme DataGridNoBorderNoPaddingCellTheme => GetDataGridNoBorderNoPaddingCellTheme();

    private static ControlTheme GetDataGridNoBorderNoPaddingCellTheme()
    {
        var showVertical =
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.Vertical) ||
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.All);

        var showHorizontal =
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.Horizontal) ||
            Se.Settings.Appearance.GridLinesAppearance == nameof(DataGridGridLinesVisibility.All);

        return new ControlTheme(typeof(DataGridCell))
        {
            Setters =
            {
                new Setter(DataGridCell.BackgroundProperty, Brushes.Transparent),
                new Setter(DataGridCell.FocusAdornerProperty, null),
                new Setter(DataGridCell.PaddingProperty, new Thickness(0)),
                new Setter(DataGridCell.BorderBrushProperty, GetBorderBrush()),
                new Setter(DataGridCell.BorderThicknessProperty,
                    new Thickness(0, 0, showVertical ? 1 : 0, showHorizontal ? 1 : 0)), // vertical and horizontal lines
            }
        };
    }

    // On macOS the default UI font's ascent sits right at the cap height, so Avalonia's line box
    // clips the dots on tall diacritics (Ä/Ö/Ü) at the top - in text boxes and grid cells alike
    // (issue #11997). Giving the line a bit of extra leading (LineHeight) lifts the line box so the
    // diacritics fit; padding does not help a TextBox, but LineHeight fixes both TextBox and TextBlock.
    // Bound to the live FontSize (rather than a fixed value) so it scales with the chosen font size,
    // and applied only on macOS so Windows/Linux line spacing is unchanged.
    private static readonly IValueConverter DiacriticLineHeightConverter =
        new FuncValueConverter<double, double>(fontSize =>
            double.IsNaN(fontSize) || fontSize <= 0 ? double.NaN : fontSize * 1.4);

    public static void FixMacDiacriticClipping(Control? control)
    {
        if (control == null || !OperatingSystem.IsMacOS())
        {
            return;
        }

        control.Bind(TextBlock.LineHeightProperty, new Binding
        {
            Source = control,
            Path = nameof(TextBlock.FontSize),
            Converter = DiacriticLineHeightConverter,
        });
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

    public static IBrush GetAccentBrush()
    {
        var app = Application.Current;
        if (app != null)
        {
            app.TryGetResource("SystemAccentColor", app.ActualThemeVariant, out var resource);
            if (resource is Color color)
                return new SolidColorBrush(color);
        }
        return new SolidColorBrush(Colors.DodgerBlue);
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

    public static Separator MakeHorizontalSeparator(double height = 0.5, double opacity = 0.5, Thickness? margin = null,
        IBrush? background = null)
    {
        return new Separator
        {
            Height = height,
            Margin = margin ?? new Thickness(5, 1),
            Background = background ?? GetBorderBrush(),
            Opacity = opacity,
        };
    }

    public static Border MakeVerticalSeparator(double width = 2.5, double opacity = 0.5, Thickness? margin = null,
        IBrush? background = null)
    {
        return new Border
        {
            Width = width,
            Margin = margin ?? new Thickness(1, 5),
            Background = background ?? GetBorderBrush(),
            Opacity = opacity,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Center
        };
    }

    /// <summary>
    /// Creates the standard progress bar used by download and progress windows.
    /// </summary>
    public static ProgressBar MakeProgressBar(double height = 10)
    {
        return new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Height = height,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
    }

    public static Button MakeButton(string text, IRelayCommand? command, object? parameter = null)
    {
        var (displayText, accessKey) = ParseAccessKey(text);

        var button = new Button
        {
            Content = displayText,
            Margin = new Thickness(4, 0),
            Padding = new Thickness(12, 6),
            MinWidth = 80,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
            CommandParameter = parameter,
        };

        if (accessKey.HasValue)
        {
            button.HotKey = new KeyGesture(accessKey.Value, KeyModifiers.Alt);
        }

        if (Se.Settings.Appearance.UseFocusedButtonBackgroundColor)
        {
            var focusStyle = new Style(x => x.OfType<Button>().Class(":focus"));
            focusStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Se.Settings.Appearance.FocusedButtonBackgroundColor.FromHexToColor())));
            //focusStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            button.Styles.Add(focusStyle);
        }

        return button;
    }

    // Parses a single `_` access-key marker out of a button label and returns the visible text plus
    // the matching Avalonia Key. Mirrors the WinForms `&` convention used in the language files
    // (e.g. "_OK" → display "OK", Alt+O; "C_ancel" → display "Cancel", Alt+A).
    private static (string Display, Key? AccessKey) ParseAccessKey(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return (text ?? string.Empty, null);
        }

        var idx = text.IndexOf('_');
        if (idx < 0 || idx + 1 >= text.Length)
        {
            return (text, null);
        }

        var accessChar = text[idx + 1];
        var display = text.Remove(idx, 1);
        return TryGetAccessKey(accessChar, out var key) ? (display, key) : (display, null);
    }

    private static bool TryGetAccessKey(char c, out Key key)
    {
        var upper = char.ToUpperInvariant(c);
        if (upper >= 'A' && upper <= 'Z')
        {
            return Enum.TryParse(upper.ToString(), out key);
        }
        if (upper >= '0' && upper <= '9')
        {
            return Enum.TryParse("D" + upper, out key);
        }
        key = Key.None;
        return false;
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
        };

        Attached.SetIcon(button, iconName);

        // An icon-only button has no text content, so expose the hint as the accessible name for
        // screen readers (a ToolTip is not surfaced as the UIA Name). Done unconditionally - unlike the
        // visual tooltip, the name should be available even when hints are turned off (#11745).
        AutomationProperties.SetName(button, hint);

        if (Se.Settings.Appearance.ShowHints)
        {
            AttachHoverTooltip(button, hint);
        }

        return button;
    }

    // On macOS, Avalonia's built-in ToolTip hover service does not open on hover inside modal
    // dialogs - the popup itself works (a forced ToolTip.IsOpen renders it, and the pointer-over
    // state is detected), but the hover trigger never fires, so icon-button hints were invisible in
    // every dialog. Windows/Linux work fine and keep the native behaviour; on macOS we drive the
    // tooltip ourselves: open it after a short hover and close it when the pointer leaves. (#12013)
    public static void AttachHoverTooltip(Control control, string hint)
    {
        ToolTip.SetTip(control, hint);

        if (!OperatingSystem.IsMacOS())
        {
            return; // native ToolTip hover service works on Windows/Linux
        }

        System.Threading.CancellationTokenSource? hoverCts = null;

        control.PointerEntered += (_, _) =>
        {
            hoverCts?.Cancel();
            hoverCts = new System.Threading.CancellationTokenSource();
            var token = hoverCts.Token;
            DispatcherTimer.RunOnce(() =>
            {
                if (!token.IsCancellationRequested && control.IsPointerOver)
                {
                    ToolTip.SetIsOpen(control, true);
                }
            }, TimeSpan.FromMilliseconds(400));
        };

        control.PointerExited += (_, _) =>
        {
            hoverCts?.Cancel();
            ToolTip.SetIsOpen(control, false);
        };
    }


    public static Button MakeButtonBrowse(IRelayCommand? command, string? propertyIsVisiblePath = null, string? accessibleName = null)
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

        // The browse "…" glyph carries no text, so screen readers announce a nameless button. Callers
        // that can describe the target should pass an accessible name so NVDA can announce it (#11745).
        if (!string.IsNullOrEmpty(accessibleName))
        {
            AutomationProperties.SetName(button, accessibleName);
        }

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

    public static NumericUpDown BindIsEnabled(this NumericUpDown control, object viewModal, string propertyIsEnabledPath,
        IValueConverter converter)
    {
        control.Bind(NumericUpDown.IsEnabledProperty, new Binding
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

    /// <summary>
    /// A text box with a drop-down of preset values - the user can still type anything.
    /// Use where a handful of values cover most cases but the field is free text, e.g. the
    /// music/pilcrow symbols in "Remove text for hearing impaired" (SE 4 parity).
    /// </summary>
    public static ComboBox MakeEditableComboBox(double width, IEnumerable<string> presets, object viewModel,
        string propertyTextPath)
    {
        var comboBox = new ComboBox
        {
            Width = width,
            IsEditable = true,
            ItemsSource = presets.ToList(),
            DataContext = viewModel,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
        };

        comboBox.Bind(ComboBox.TextProperty, new Binding
        {
            Path = propertyTextPath,
            Mode = BindingMode.TwoWay,
        });

        return comboBox;
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

    /// <summary>
    /// Text box for API keys and similar secrets: masked by default so the value never shows
    /// in screenshots or screen shares, with an eye button to reveal it while editing.
    /// </summary>
    public static StackPanel MakeApiKeyTextBox(double width, object viewModel, string propertyTextPath,
        string? propertyIsVisiblePath = null)
    {
        var textBox = MakeTextBox(width, viewModel, propertyTextPath);
        textBox.PasswordChar = '●';
        AutomationProperties.SetName(textBox, Se.Language.General.ApiKey);

        var buttonReveal = MakeButton(null, IconNames.Eye);
        AutomationProperties.SetName(buttonReveal, Se.Language.General.ApiKey);
        buttonReveal.Click += (_, _) =>
        {
            textBox.RevealPassword = !textBox.RevealPassword;
            Attached.SetIcon(buttonReveal, textBox.RevealPassword ? IconNames.EyeOff : IconNames.Eye);
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { textBox, buttonReveal },
        };

        if (propertyIsVisiblePath != null)
        {
            panel.DataContext = viewModel;
            panel.Bind(StackPanel.IsVisibleProperty, new Binding(propertyIsVisiblePath));
        }

        return panel;
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

        // Replacing the text content with an icon+text panel loses the button's computed UIA
        // name - keep the original text as the accessible name so screen readers still
        // announce it (#11745/#12087 accessibility work).
        if (!string.IsNullOrEmpty(label.Text))
        {
            AutomationProperties.SetName(control, label.Text);
        }

        return control;
    }

    public static Button WithCommandParameter<T>(this Button control, T parameter)
    {
        control.CommandParameter = parameter;
        return control;
    }

    // Like WithIconLeft, but the text is bound to a view-model property instead of being fixed,
    // so the caption can change at runtime (e.g. "Download" vs "Re-download").
    public static Button WithIconLeftBindText(this Button control, string iconName, string textPropertyPath)
    {
        var label = new TextBlock { Padding = new Thickness(4, 0, 0, 0) };
        label.Bind(TextBlock.TextProperty, new Binding { Path = textPropertyPath });

        var image = new ContentControl();
        Attached.SetIcon(image, iconName);

        control.Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { image, label },
        };

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

    public static T WithBindIsVisible<T>(this T control, string isVisiblePropertyPath) where T : Control
    {
        control.Bind(Visual.IsVisibleProperty, new Binding
        {
            Path = isVisiblePropertyPath,
            Mode = BindingMode.OneWay,
        });

        return control;
    }

    public static T WithBindIsVisible<T>(this T control, object source, string isVisiblePropertyPath) where T : Control
    {
        control.Bind(Visual.IsVisibleProperty, new Binding(isVisiblePropertyPath)
        {
            Source = source,
            Mode = BindingMode.OneWay,
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

    public static ComboBox WithBindItemsSource(this ComboBox control, string itemsSourcePropertyBinding)
    {
        control.Bind(ItemsControl.ItemsSourceProperty, new Binding
        {
            Path = itemsSourcePropertyBinding,
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

    /// <summary>
    /// Sets an accessible name (UIA Name) so a screen reader announces this control instead of a bare
    /// "edit"/"combo box"/"button". Use when there is no separate visible label to point at (#11745).
    /// </summary>
    public static T WithAccessibleName<T>(this T control, string name) where T : Control
    {
        AutomationProperties.SetName(control, name);
        return control;
    }

    /// <summary>
    /// Links this control to an existing visible label so a screen reader announces the label as the
    /// control's name (UIA LabeledBy). Prefer this over a hard-coded name when a label already exists,
    /// as it stays correct for bound/localized label text (#11745).
    /// </summary>
    public static T WithLabeledBy<T>(this T control, Control label) where T : Control
    {
        AutomationProperties.SetLabeledBy(control, label);
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

    private static ImageBrush? _checkerboardBrush;

    /// <summary>
    /// Theme-independent checkerboard backdrop for image previews whose content can be pure
    /// white, pure black or transparent (e.g. OCR bitmaps and pre-processing output). Two
    /// mid-tone grays so both extremes stay visible in light and dark theme (issue #12692).
    /// </summary>
    public static ImageBrush GetCheckerboardBrush()
    {
        if (_checkerboardBrush != null)
        {
            return _checkerboardBrush;
        }

        const int tileSize = 16; // 2x2 squares of 8px
        const int squareSize = tileSize / 2;
        var bitmap = new Avalonia.Media.Imaging.WriteableBitmap(
            new PixelSize(tileSize, tileSize),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);
        using (var frameBuffer = bitmap.Lock())
        {
            unsafe
            {
                var pixels = (uint*)frameBuffer.Address;
                for (var y = 0; y < tileSize; y++)
                {
                    for (var x = 0; x < tileSize; x++)
                    {
                        var isDark = (x / squareSize + y / squareSize) % 2 == 0;
                        pixels[y * frameBuffer.RowBytes / 4 + x] = isDark ? 0xFF666666 : 0xFF999999;
                    }
                }
            }
        }

        _checkerboardBrush = new ImageBrush(bitmap)
        {
            TileMode = TileMode.Tile,
            DestinationRect = new RelativeRect(0, 0, tileSize, tileSize, RelativeUnit.Absolute),
        };
        return _checkerboardBrush;
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

    public static bool IsScrollBarSource(RoutedEventArgs e)
    {
        var current = e.Source as Control;
        while (current != null)
        {
            if (current is ScrollBar)
            {
                return true;
            }
            current = current.Parent as Control;
        }
        return false;
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

    internal static Button MakeColorPickerButton(object source, string colorPropertyPath, bool showAlpha = true)
    {
        var pathParts = colorPropertyPath.Split('.');

        (object? Owner, PropertyInfo? Property) ResolveLeaf()
        {
            object? current = source;
            for (var i = 0; i < pathParts.Length - 1; i++)
            {
                if (current == null)
                {
                    return (null, null);
                }
                var pi = current.GetType().GetProperty(pathParts[i], BindingFlags.Public | BindingFlags.Instance);
                current = pi?.GetValue(current);
            }
            if (current == null)
            {
                return (null, null);
            }
            var leafProp = current.GetType().GetProperty(pathParts[^1], BindingFlags.Public | BindingFlags.Instance);
            return (current, leafProp);
        }

        Color ReadColor()
        {
            var (owner, prop) = ResolveLeaf();
            return prop?.GetValue(owner) is Color c ? c : Colors.White;
        }

        var colorSwatch = new Border
        {
            Width = 30,
            Height = 20,
            CornerRadius = new CornerRadius(CornerRadius),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            Background = new SolidColorBrush(ReadColor()),
            VerticalAlignment = VerticalAlignment.Center,
        };

        var button = new Button
        {
            Content = colorSwatch,
            Padding = new Thickness(4, 2),
            VerticalAlignment = VerticalAlignment.Center,
        };

        var subscriptions = new List<(INotifyPropertyChanged Source, PropertyChangedEventHandler Handler)>();

        void RefreshSwatch()
        {
            var updated = ReadColor();
            if (Dispatcher.UIThread.CheckAccess())
            {
                colorSwatch.Background = new SolidColorBrush(updated);
            }
            else
            {
                Dispatcher.UIThread.Post(() => colorSwatch.Background = new SolidColorBrush(updated));
            }
        }

        void Unsubscribe()
        {
            foreach (var (src, handler) in subscriptions)
            {
                src.PropertyChanged -= handler;
            }
            subscriptions.Clear();
        }

        void Subscribe()
        {
            object? current = source;
            for (var i = 0; i < pathParts.Length; i++)
            {
                if (current is INotifyPropertyChanged inpc)
                {
                    var partName = pathParts[i];
                    var isLeaf = i == pathParts.Length - 1;
                    PropertyChangedEventHandler handler = (_, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.PropertyName) && e.PropertyName != partName)
                        {
                            return;
                        }
                        if (isLeaf)
                        {
                            RefreshSwatch();
                        }
                        else
                        {
                            Unsubscribe();
                            Subscribe();
                            RefreshSwatch();
                        }
                    };
                    inpc.PropertyChanged += handler;
                    subscriptions.Add((inpc, handler));
                }
                if (i < pathParts.Length - 1)
                {
                    if (current == null)
                    {
                        break;
                    }
                    var pi = current.GetType().GetProperty(pathParts[i], BindingFlags.Public | BindingFlags.Instance);
                    current = pi?.GetValue(current);
                }
            }
        }

        Subscribe();
        button.DetachedFromVisualTree += (_, _) => Unsubscribe();

        button.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(button) is not Window window)
            {
                return;
            }

            var (owner, prop) = ResolveLeaf();
            if (owner == null || prop == null)
            {
                return;
            }

            var currentColor = prop.GetValue(owner) is Color cc ? cc : Colors.White;
            var vm = new ColorPickerViewModel();
            vm.Initialize(currentColor);
            vm.ShowAlpha = showAlpha;
            var pickerWindow = new ColorPickerWindow(vm);
            await pickerWindow.ShowDialog(window);

            if (vm.OkPressed)
            {
                prop.SetValue(owner, vm.SelectedColor);
                colorSwatch.Background = new SolidColorBrush(vm.SelectedColor);
            }
        };

        return button;
    }

    internal static Label MakeLabel(string text = "")
    {
        return new Label
        {
            Content = text,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    internal static Label MakeLabel<TViewModel>(string text, Expression<Func<TViewModel, bool>> isVisibleExpression)
    {
        var label = new Label
        {
            Content = text,
            VerticalAlignment = VerticalAlignment.Center,
        };

        label.Bind(
            Label.IsVisibleProperty,
            CompiledBinding.Create(
                isVisibleExpression,
                mode: BindingMode.TwoWay
            )
        );

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

    /// <summary>
    /// Creates a label bound to a (possibly long) file name or path. The displayed text
    /// is truncated in the middle with an ellipsis so the start and the file name stay
    /// visible, the width is capped so a long path can't force a fixed-size window to grow,
    /// and the full value is shown as a tooltip.
    /// </summary>
    internal static Label MakeFilePathLabel(object viewModel, string fullPathPropertyPath, int maxLength = 50, int maxWidth = 400)
    {
        var label = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            MaxWidth = maxWidth,
            DataContext = viewModel,
        };

        label.Bind(Label.ContentProperty, new Binding
        {
            Path = fullPathPropertyPath,
            Mode = BindingMode.OneWay,
            Converter = new MiddleEllipsisConverter(),
            ConverterParameter = maxLength,
        });

        label.Bind(ToolTip.TipProperty, new Binding
        {
            Path = fullPathPropertyPath,
            Mode = BindingMode.OneWay,
            Converter = new NullOrEmptyToNullConverter(),
        });

        return label;
    }

    /// <summary>
    /// Binds a text block (e.g. a link) to a (possibly long) file name or path. The displayed
    /// text is truncated in the middle with an ellipsis so the start and the file name stay
    /// visible, the width is capped so a long path can't force a fixed-size window to grow,
    /// and the full value is shown as a tooltip.
    /// </summary>
    internal static TextBlock WithFilePathText(this TextBlock control, object viewModel, string fullPathPropertyPath, int maxLength = 50, int maxWidth = 400)
    {
        control.MaxWidth = maxWidth;
        control.DataContext = viewModel;

        control.Bind(TextBlock.TextProperty, new Binding
        {
            Path = fullPathPropertyPath,
            Mode = BindingMode.OneWay,
            Converter = new MiddleEllipsisConverter(),
            ConverterParameter = maxLength,
        });

        control.Bind(ToolTip.TipProperty, new Binding
        {
            Path = fullPathPropertyPath,
            Mode = BindingMode.OneWay,
            Converter = new NullOrEmptyToNullConverter(),
        });

        return control;
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
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
            Minimum = min,
            Maximum = max,
            Width = width,
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

        control.AddHandler(InputElement.PointerWheelChangedEvent, (s, e) =>
        {
            control.Value = Math.Clamp((control.Value ?? 0) + (e.Delta.Y > 0 ? control.Increment : -control.Increment),
                                        control.Minimum,
                                        control.Maximum);
            e.Handled = true;
        });

        ForwardAutomationNameToInnerTextBox(control);

        return control;
    }

    // Like MakeNumericUpDownInt but for double-typed (double?) view-model properties.
    // Uses NullableDoubleConverter so the actual value round-trips; NullableIntConverter
    // would fall back to its DefaultValue whenever the source is a double (not an int).
    public static NumericUpDown MakeNumericUpDownDouble(int min, int max, double defaultValue, double width, object viewModel,
        string? propertyValuePath = null, string? propertyIsVisiblePath = null)
    {
        var control = new NumericUpDown
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = viewModel,
            Minimum = min,
            Maximum = max,
            Width = width,
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
                Converter = new NullableDoubleConverter { DefaultValue = defaultValue },
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

        control.AddHandler(InputElement.PointerWheelChangedEvent, (s, e) =>
        {
            control.Value = Math.Clamp((control.Value ?? 0) + (e.Delta.Y > 0 ? control.Increment : -control.Increment),
                                        control.Minimum,
                                        control.Maximum);
            e.Handled = true;
        });

        ForwardAutomationNameToInnerTextBox(control);

        return control;
    }

    public static NumericUpDown MakeNumericUpDownTwoDecimals(decimal min, decimal max, double width, object viewModel,
        string? propertyValuePath = null, string? propertyIsVisiblePath = null, decimal defaultValue = 0)
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
                Converter = new NullableDecimalConverter { DefaultValue = defaultValue },
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

        MakeNumeriUpDownMouseWheelHandler(control);
        ForwardAutomationNameToInnerTextBox(control);

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

        MakeNumeriUpDownMouseWheelHandler(control);
        ForwardAutomationNameToInnerTextBox(control);

        return control;
    }

    public static NumericUpDown MakeNumericUpDownOneDecimal(decimal min, decimal max, double width, object viewModel,
        string? propertyValuePath = null, string? propertyIsVisiblePath = null, decimal defaultValue = 0)
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
                Converter = new NullableDecimalConverter { DefaultValue = defaultValue },
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

        MakeNumeriUpDownMouseWheelHandler(control);
        ForwardAutomationNameToInnerTextBox(control);

        return control;
    }

    private static void MakeNumeriUpDownMouseWheelHandler(NumericUpDown control)
    {
        control.AddHandler(InputElement.PointerWheelChangedEvent, (s, e) =>
        {
            control.Value = Math.Clamp((control.Value ?? 0) + (e.Delta.Y > 0 ? control.Increment : -control.Increment),
                                        control.Minimum,
                                        control.Maximum);
            e.Handled = true;
        });
    }

    /// <summary>
    /// Forwards the accessible name set on a <see cref="NumericUpDown"/> to its inner
    /// PART_TextBox. The text box is the element that actually receives keyboard focus,
    /// so without this a screen reader would announce the focused field with no name
    /// (issue #11553). Callers just set <c>AutomationProperties.Name</c> on the control.
    /// </summary>
    private static void ForwardAutomationNameToInnerTextBox(NumericUpDown control)
    {
        control.TemplateApplied += (_, e) =>
        {
            var textBox = e.NameScope.Find<TextBox>("PART_TextBox");
            textBox?.Bind(AutomationProperties.NameProperty, control.GetObservable(AutomationProperties.NameProperty));
        };
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

    public static Label WithBindText<TViewModel>(
     this Label control,
     TViewModel viewModel,
     Expression<Func<TViewModel, string>> contentExpression)
    {
        control.DataContext = viewModel;

        control.Bind(
            Label.ContentProperty,
            CompiledBinding.Create<TViewModel, string>(
                contentExpression,
                mode: BindingMode.OneWay
            )
        );

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

    public static Label WithBindVisible<TViewModel>(
        this Label control,
        TViewModel viewModel,
        Expression<Func<TViewModel, bool>> visibleExpression)
    {
        control.DataContext = viewModel;

        control.Bind(
            Label.IsVisibleProperty,
            CompiledBinding.Create(
                visibleExpression,
                mode: BindingMode.OneWay
            )
        );

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
            shortcutString = string.Join("+", ShortcutManager.OrderKeys(shortcut.Keys).Select(k => ShortcutManager.GetKeyDisplayName(k)));
            shortcutString = $"({shortcutString})";
        }

        return shortcutString;
    }

    internal static void InitializeWindow(Window window, string name)
    {
        window.Icon = GetSeIcon();
        window.Name = name;

        // On small or high-DPI screens (e.g. 1920x1080 at 150% = 1280x853 DIPs of
        // working area) SizeToContent windows can measure taller/wider than the screen,
        // leaving the bottom buttons unreachable - Avalonia does not cap them itself.
        // Clamp once when opened, and once more at Background priority so windows that
        // re-fit themselves in a posted callback (LockMinimumToContentSize in e.g. the
        // burn-in window runs at Loaded priority) get clamped again afterwards.
        window.Opened += (_, _) =>
        {
            ClampToWorkingArea(window);
            Dispatcher.UIThread.Post(() => ClampToWorkingArea(window), DispatcherPriority.Background);
        };
    }

    private static void ClampToWorkingArea(Window window)
    {
        if (window.WindowState != WindowState.Normal)
        {
            return;
        }

        var screen = window.Screens.ScreenFromWindow(window) ?? window.Screens.Primary;
        if (screen == null)
        {
            return;
        }

        var scale = window.DesktopScaling;
        if (scale <= 0)
        {
            return;
        }

        // Window sizes are in DIPs, the working area in physical pixels. Compare the
        // full frame so OS decorations (title bar, borders) are accounted for.
        var workingArea = screen.WorkingArea;
        var frameSize = window.FrameSize ?? window.Bounds.Size;
        var frameExtraWidth = Math.Max(0, frameSize.Width - window.Bounds.Width);
        var frameExtraHeight = Math.Max(0, frameSize.Height - window.Bounds.Height);
        var maxWidth = workingArea.Width / scale - frameExtraWidth;
        var maxHeight = workingArea.Height / scale - frameExtraHeight;
        if (maxWidth <= 0 || maxHeight <= 0)
        {
            return;
        }

        // A minimum size larger than the screen makes the window impossible to shrink
        // (e.g. windows that lock MinWidth/MinHeight to their measured content size).
        if (window.MinWidth > maxWidth)
        {
            window.MinWidth = maxWidth;
        }

        if (window.MinHeight > maxHeight)
        {
            window.MinHeight = maxHeight;
        }

        if (window.Bounds.Width > maxWidth || window.Bounds.Height > maxHeight)
        {
            // Stop content-driven sizing before shrinking, or the next measure pass
            // would just grow the window back.
            window.SizeToContent = SizeToContent.Manual;
            if (window.Bounds.Width > maxWidth)
            {
                window.Width = maxWidth;
            }

            if (window.Bounds.Height > maxHeight)
            {
                window.Height = maxHeight;
            }
        }

        // Keep the whole frame inside the working area so the bottom buttons stay
        // reachable (a centered too-tall window overflows both top and bottom).
        var frameWidthPx = (int)Math.Round(Math.Min(frameSize.Width, maxWidth + frameExtraWidth) * scale);
        var frameHeightPx = (int)Math.Round(Math.Min(frameSize.Height, maxHeight + frameExtraHeight) * scale);
        var maxX = workingArea.X + workingArea.Width - frameWidthPx;
        var maxY = workingArea.Y + workingArea.Height - frameHeightPx;
        var x = Math.Min(Math.Max(window.Position.X, workingArea.X), Math.Max(workingArea.X, maxX));
        var y = Math.Min(Math.Max(window.Position.Y, workingArea.Y), Math.Max(workingArea.Y, maxY));
        var position = new PixelPoint(x, y);
        if (position != window.Position)
        {
            window.Position = position;
        }
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
        var windowRect = new PixelRect(desired, new PixelSize(existing.Width, existing.Height));

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
                var px = targetScreen.Bounds.X + (targetScreen.Bounds.Width - existing.Width) / 2;
                var py = targetScreen.Bounds.Y + (targetScreen.Bounds.Height - existing.Height) / 2;
                desired = new PixelPoint(px, py);
            }
            else
            {
                // ultimate fallback: center on primary screen
                var primary = window.Screens.Primary;
                if (primary != null)
                {
                    var px = primary.Bounds.X + (primary.Bounds.Width - existing.Width) / 2;
                    var py = primary.Bounds.Y + (primary.Bounds.Height - existing.Height) / 2;
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
            // A non-resizable window gets its size from its content (SizeToContent), so a saved
            // size can only be stale - applying one from an older layout clips the window.
            if (existing.Width > 0 && existing.Height > 0 && window.CanResize)
            {
                window.Width = existing.Width;
                window.Height = existing.Height;
            }

            window.WindowState = WindowState.Normal;
        }
    }

    public static void ShowHelp(string helpName, string section = "")
    {
        var helpUrl = string.Format($"http://subtitleedit.github.io/subtitleedit/{helpName}.html");
        if (!string.IsNullOrEmpty(section))
        {
            helpUrl += $"#{section}";
        }

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

    internal static bool TryHandleWindowSystemMenu(KeyEventArgs e, Window? window)
    {
        if (!OperatingSystem.IsWindows() || window == null)
        {
            return false;
        }

        if (e.Key == Key.Space && e.KeyModifiers == KeyModifiers.Alt)
        {
            SystemMenu.Show(window);
            e.Handled = true;
            return true;
        }

        return false;
    }

    private static bool _windowsSystemMenuClassHandlerRegistered;

    /// <summary>
    /// Registers a single class-level handler so every <see cref="Window"/> in the app
    /// responds to Alt+Space by opening the standard Windows system menu. Call once at
    /// application startup. No-op on non-Windows platforms or if already registered.
    /// </summary>
    internal static void RegisterWindowsSystemMenuClassHandler()
    {
        if (!OperatingSystem.IsWindows() || _windowsSystemMenuClassHandlerRegistered)
        {
            return;
        }

        _windowsSystemMenuClassHandlerRegistered = true;

        InputElement.KeyDownEvent.AddClassHandler<Window>(
            (window, e) => TryHandleWindowSystemMenu(e, window),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    /// <summary>
    /// On macOS, Ctrl+Click does not trigger <c>ContextFlyout</c> reliably. Attach this
    /// handler on the control that owns the flyout — tunnel phase + handledEventsToo so
    /// descendants cannot swallow the event first — and force the flyout open via
    /// <c>MenuFlyout.ShowAt</c>. Use for control-scoped flyouts (DataGrid, ListBox,
    /// TextBox, Image, Border, etc. — anything with a hit-test surface).
    /// </summary>
    public static void AttachMacContextFlyoutHandler(Control flyoutOwner)
    {
        if (!OperatingSystem.IsMacOS() || flyoutOwner == null)
        {
            return;
        }

        flyoutOwner.AddHandler(InputElement.PointerReleasedEvent, (_, e) =>
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
                !e.KeyModifiers.HasFlag(KeyModifiers.Shift) &&
                (e.InitialPressMouseButton == MouseButton.Left || e.InitialPressMouseButton == MouseButton.Right) &&
                flyoutOwner.ContextFlyout is MenuFlyout menuFlyout)
            {
                menuFlyout.ShowAt(flyoutOwner, showAtPointer: true);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    /// <summary>
    /// Window-scoped variant for when the flyout owner is a layout container (Grid/Panel)
    /// with no Background — the container itself may not receive pointer events on empty
    /// areas, so attach on the window root instead. Ctrl+Click anywhere in the window
    /// opens the flyout at <paramref name="flyoutOwner"/>.
    /// </summary>
    public static void AttachMacContextFlyoutHandler(Window window, Control flyoutOwner)
    {
        if (!OperatingSystem.IsMacOS() || window == null || flyoutOwner == null)
        {
            return;
        }

        window.AddHandler(InputElement.PointerReleasedEvent, (_, e) =>
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
                !e.KeyModifiers.HasFlag(KeyModifiers.Shift) &&
                (e.InitialPressMouseButton == MouseButton.Left || e.InitialPressMouseButton == MouseButton.Right) &&
                flyoutOwner.ContextFlyout is MenuFlyout menuFlyout)
            {
                menuFlyout.ShowAt(flyoutOwner, showAtPointer: true);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    /// <summary>
    /// Makes Home/End jump to the first/last row of <paramref name="dataGrid"/> and select it.
    /// Avalonia's DataGrid only moves the cell cursor, which is why this is needed. Tunnel
    /// phase so the grid's own navigation does not consume the key first.
    /// </summary>
    public static void AttachHomeEndNavigation(DataGrid dataGrid)
    {
        if (dataGrid == null)
        {
            return;
        }

        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is not (Key.Home or Key.End) || e.Source is TextBox)
            {
                return;
            }

            if (dataGrid.ItemsSource is not IList items || items.Count == 0)
            {
                return;
            }

            var target = e.Key == Key.Home ? items[0] : items[^1];
            dataGrid.SelectedItem = target;
            dataGrid.ScrollIntoView(target, null);
            e.Handled = true;
        }, RoutingStrategies.Tunnel);
    }
}