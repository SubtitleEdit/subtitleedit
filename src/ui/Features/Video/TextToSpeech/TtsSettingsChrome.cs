using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// The layout of the local TTS engine settings dialogs (CrispASR and audio.cpp engines) on top of
/// <see cref="EditorChrome"/>: an engine header, a "Models" section with the runtime/model status
/// rows, a "Voice" section with the per-request knobs, and a button bar with Close.
/// </summary>
internal static class TtsSettingsChrome
{
    internal const double DefaultLabelWidth = 150;
    internal const double SliderWidth = 200;

    /// <summary>
    /// A fixed width with height-only auto-sizing: SizeToContent.WidthAndHeight comes out far
    /// too wide on macOS (OmniVoice), and a fixed width lets long descriptions wrap.
    /// </summary>
    internal const double WindowWidth = 620;

    /// <summary>
    /// Window chrome: editor background and styles, fixed width, height from the content.
    /// </summary>
    internal static void Apply(Window window, double width = WindowWidth)
    {
        window.Width = width;
        window.SizeToContent = SizeToContent.Height;
        window.CanResize = false;
        EditorChrome.ApplyWindow(window);
    }

    /// <summary>
    /// The settings (scrollable on short screens) on the panel color, the button bar below.
    /// </summary>
    internal static Grid MakeContent(Control header, Border buttonBar, params Border[] sections)
    {
        var stack = new StackPanel();
        stack.Children.Add(header);
        foreach (var section in sections)
        {
            stack.Children.Add(section);
        }

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(new Border
        {
            Background = EditorChrome.PanelBrush,
            Child = new ScrollViewer
            {
                Content = stack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            },
        }, 0, 0);
        root.Add(buttonBar, 1, 0);
        return root;
    }

    /// <summary>
    /// Engine name, its description and optional extra lines (like a license link).
    /// </summary>
    internal static Border MakeHeader(string title, string description, params Control[] extra)
    {
        var panel = new StackPanel { Spacing = 3 };
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeight.SemiBold,
            Foreground = EditorChrome.StrongTextBrush,
        });
        panel.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
        });
        foreach (var control in extra)
        {
            panel.Children.Add(control);
        }

        return new Border
        {
            Child = panel,
            Padding = new Thickness(14, 14, 14, 12),
        };
    }

    /// <summary>
    /// A license link for the header.
    /// </summary>
    internal static TextBlock MakeLicenseLink(string text, CommunityToolkit.Mvvm.Input.IRelayCommand command)
    {
        var link = UiUtil.MakeLink(text, command);
        link.FontSize = 12;
        link.Margin = new Thickness(0, 2, 0, 0);
        return link;
    }

    /// <summary>
    /// Rows of "label  content", the labels in one column of fixed width so the rows line up.
    /// </summary>
    internal static Grid MakeRows(double labelWidth, params (string Label, Control Content)[] rows)
    {
        var grid = new Grid
        {
            ColumnSpacing = 12,
            RowSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(labelWidth)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };

        for (var i = 0; i < rows.Length; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var label = EditorChrome.MakeDimLabel(rows[i].Label, new Thickness(0));
            label.TextTrimming = TextTrimming.CharacterEllipsis;
            label.MinHeight = 28;
            label.VerticalAlignment = VerticalAlignment.Top;
            label.Padding = new Thickness(0, 6, 0, 0);
            grid.Add(label, i, 0);
            grid.Add(rows[i].Content, i, 1);
        }

        return grid;
    }

    /// <summary>
    /// A status dot (installed/missing color from the view model) with its status text, and
    /// optional actions after it.
    /// </summary>
    internal static StackPanel MakeStatus(string brushBindingPath, string labelBindingPath, params Control[] actions)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            MinHeight = 28,
            Children =
            {
                new Ellipse
                {
                    Width = 9,
                    Height = 9,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Shape.FillProperty] = new Binding(brushBindingPath),
                },
                new TextBlock
                {
                    FontSize = 12.5,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(labelBindingPath),
                },
            },
        };

        foreach (var action in actions)
        {
            panel.Children.Add(action);
        }

        return panel;
    }

    /// <summary>
    /// The engine (runtime) re-download button; its text comes from the view model
    /// ("Download" / "Re-download").
    /// </summary>
    internal static Button MakeEngineDownloadButton(ICommand command, string textBindingPath)
    {
        var button = new Button
        {
            Command = command,
            Height = 28,
            Padding = new Thickness(8, 0, 10, 0),
            Margin = new Thickness(4, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new Optris.Icons.Avalonia.Icon { Value = IconNames.Download, FontSize = 14, VerticalAlignment = VerticalAlignment.Center },
                    new TextBlock { FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center, [!TextBlock.TextProperty] = new Binding(textBindingPath) },
                },
            },
        };
        button.Classes.Add("tool");
        AutomationProperties.SetName(button, Se.Language.General.Download);
        return button;
    }

    /// <summary>
    /// An icon-only download button for one model quant.
    /// </summary>
    internal static Button MakeModelDownloadButton(ICommand command, string modelKey)
    {
        var button = EditorChrome.CreateToolButton(IconNames.Download, Se.Language.General.Download + " " + modelKey, command, 26);
        button.CommandParameter = modelKey;
        button.Margin = new Thickness(2, 0, 0, 0);
        return button;
    }

    /// <summary>
    /// An icon-only "open folder" button, placed beside the row it belongs to.
    /// </summary>
    internal static Button MakeFolderButton(string tooltip, ICommand command)
    {
        var button = EditorChrome.CreateToolButton(IconNames.FolderOpen, tooltip, command, 26);
        button.Margin = new Thickness(2, 0, 0, 0);
        return button;
    }

    /// <summary>
    /// A read-only value bound to the view model, with optional actions after it.
    /// </summary>
    internal static StackPanel MakeValue(string bindingPath, params Control[] actions) =>
        MakeValuePanel(new TextBlock { [!TextBlock.TextProperty] = new Binding(bindingPath) }, actions);

    /// <summary>
    /// A fixed text value, with optional actions after it.
    /// </summary>
    internal static StackPanel MakeFixedValue(string text, params Control[] actions) =>
        MakeValuePanel(new TextBlock { Text = text }, actions);

    private static StackPanel MakeValuePanel(TextBlock text, Control[] actions)
    {
        text.FontSize = 12.5;
        text.VerticalAlignment = VerticalAlignment.Center;
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            MinHeight = 28,
            Children = { text },
        };

        foreach (var action in actions)
        {
            panel.Children.Add(action);
        }

        return panel;
    }

    /// <summary>
    /// A slider with its value text after it.
    /// </summary>
    internal static StackPanel MakeSlider(
        string accessibleName,
        double minimum,
        double maximum,
        double tickFrequency,
        string valueBindingPath,
        string labelBindingPath,
        string? isEnabledBindingPath = null)
    {
        var slider = new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Width = SliderWidth,
            TickFrequency = tickFrequency,
            IsSnapToTickEnabled = true,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, -6),
            [!RangeBase.ValueProperty] = new Binding(valueBindingPath),
        };
        AutomationProperties.SetName(slider, accessibleName);

        var label = new TextBlock
        {
            FontSize = 12.5,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(labelBindingPath),
        };

        if (isEnabledBindingPath != null)
        {
            slider[!Avalonia.Input.InputElement.IsEnabledProperty] = new Binding(isEnabledBindingPath);
            label[!Avalonia.Input.InputElement.IsEnabledProperty] = new Binding(isEnabledBindingPath);
        }

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            MinHeight = 28,
            Children = { slider, label },
        };
    }

    /// <summary>
    /// The install folder path (selectable, read-only) with an "open folder" button after it.
    /// </summary>
    internal static Grid MakeFolder(string bindingPath, ICommand openCommand)
    {
        var text = new TextBox
        {
            IsReadOnly = true,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Foreground = EditorChrome.DimTextBrush,
            Padding = new Thickness(0),
            MinHeight = 0,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 12,
            [!TextBox.TextProperty] = new Binding(bindingPath),
        };
        AutomationProperties.SetName(text, Se.Language.General.InstallFolder);

        var grid = new Grid
        {
            ColumnSpacing = 6,
            MinHeight = 28,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        grid.Add(text, 0, 0);
        grid.Add(MakeFolderButton(Se.Language.General.OpenContainingFolder, openCommand), 0, 1);
        return grid;
    }

    /// <summary>
    /// The bottom button bar with the accent close button on the right.
    /// </summary>
    internal static Border MakeButtonBar(Button close)
    {
        close.Classes.Add("primary");
        close.HorizontalAlignment = HorizontalAlignment.Right;
        return EditorChrome.WrapButtonBar(close);
    }
}
