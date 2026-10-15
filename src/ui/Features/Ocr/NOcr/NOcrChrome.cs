using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Edit;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

/// <summary>
/// Small building blocks shared by the nOCR dialogs (add character, inspect, history, database
/// edit, train, ...) in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
internal static class NOcrChrome
{
    /// <summary>
    /// Window background, the editor styles (compact combo boxes, text boxes with class "text"),
    /// plus toggle tool buttons (draw mode, train bold/italic) and a compact split button.
    /// </summary>
    internal static void Apply(Window window)
    {
        EditDialogChrome.Apply(window);

        var styles = window.Styles;
        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)),
                new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(10, 3)),
                new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center),
            },
        });
        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool").Class(":pointerover").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush),
                new Setter(ContentPresenter.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                new Setter(ContentPresenter.ForegroundProperty, EditorChrome.StrongTextBrush),
            },
        });
        foreach (var isHover in new[] { false, true })
        {
            styles.Add(new Style(x =>
            {
                var selector = x.OfType<ToggleButton>().Class("tool").Class(":checked");
                return (isHover ? selector.Class(":pointerover") : selector).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter");
            })
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, isHover ? EditorChrome.AccentHoverBrush : EditorChrome.AccentBrush),
                    new Setter(ContentPresenter.BorderBrushProperty, isHover ? EditorChrome.AccentHoverBrush : EditorChrome.AccentBrush),
                    new Setter(ContentPresenter.ForegroundProperty, Brushes.White),
                },
            });
        }

        styles.Add(new Style(x => x.OfType<SplitButton>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
    }

    /// <summary>
    /// A text box in the editor look (class "text") with an accessible name.
    /// </summary>
    internal static TextBox StyleTextBox(TextBox textBox, string accessibleName)
    {
        textBox.Classes.Add("text");
        AutomationProperties.SetName(textBox, accessibleName);
        return textBox;
    }

    /// <summary>
    /// A toggle tool button with a text (and optional icon), its checked state bound two-way.
    /// </summary>
    internal static ToggleButton MakeToggle(string text, string isCheckedProperty, object source, string? icon = null)
    {
        Control content = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center };
        if (icon != null)
        {
            content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new Icon { Value = icon, FontSize = 14, VerticalAlignment = VerticalAlignment.Center },
                    content,
                },
            };
        }

        var toggle = new ToggleButton
        {
            Content = content,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(isCheckedProperty) { Source = source, Mode = BindingMode.TwoWay },
        };
        toggle.Classes.Add("tool");
        AutomationProperties.SetName(toggle, text);
        return toggle;
    }

    /// <summary>
    /// A flat button with an icon and a short text, as in the other editor-look dialogs.
    /// </summary>
    internal static Button MakeAction(string icon, string text, System.Windows.Input.ICommand? command)
    {
        return Sync.SyncTimingChrome.MakeTextToolButton(icon, text, command);
    }

    /// <summary>
    /// Dim text bound to <paramref name="property"/>.
    /// </summary>
    internal static TextBlock MakeInfo(string property, object? source = null)
    {
        var binding = new Binding(property);
        if (source != null)
        {
            binding.Source = source;
        }

        return new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = binding,
        };
    }

    /// <summary>
    /// A thin frame around the nOCR drawing canvas (the canvas paints its own background).
    /// </summary>
    internal static Border MakeCanvasFrame(Control canvas)
    {
        return new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.FieldBorderBrush,
            Child = canvas,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };
    }

    /// <summary>
    /// A section like <see cref="EditorChrome.MakeEditorSection"/>, with a line to its left
    /// instead of on top - for columns side by side.
    /// </summary>
    internal static Border MakeColumn(string title, Control content, Control? headerRight = null, bool first = false)
    {
        // Same header height as the fill columns, so the headers of side-by-side columns line up
        return MakeFillColumn(title, content, headerRight, first);
    }

    /// <summary>
    /// A column section whose content scrolls when the window is short, so its actions stay
    /// reachable.
    /// </summary>
    internal static Border MakeScrollingColumn(string title, Control content, bool first = false)
    {
        var scrollViewer = new ScrollViewer
        {
            Content = content,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
        return MakeFillColumn(title, scrollViewer, first: first);
    }

    /// <summary>
    /// A column section whose content fills the height below the header (lists, the canvas).
    /// </summary>
    internal static Border MakeFillColumn(string title, Control content, Control? headerRight = null, bool first = false)
    {
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 10), MinHeight = 28 };
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            headerRight.VerticalAlignment = VerticalAlignment.Center;
            header.Children.Add(headerRight);
        }

        header.Children.Add(EditorChrome.MakeSectionHeader(title));
        DockPanel.SetDock(header, Dock.Top);
        var dock = new DockPanel { LastChildFill = true };
        dock.Children.Add(header);
        dock.Children.Add(content);
        return new Border
        {
            Child = dock,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(first ? 0 : 1, 0, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    /// <summary>
    /// Zoom out, zoom in and the zoom factor text, for the header of the canvas section.
    /// </summary>
    internal static StackPanel MakeZoomBar(System.Windows.Input.ICommand zoomOut, System.Windows.Input.ICommand zoomIn, string zoomInfoProperty, string? isEnabledProperty = null)
    {
        var buttonZoomOut = EditorChrome.CreateToolButton(IconNames.MagnifyMinus, Se.Language.Ocr.NOcrZoomOut, zoomOut, 28);
        var buttonZoomIn = EditorChrome.CreateToolButton(IconNames.MagnifyPlus, Se.Language.Ocr.NOcrZoomIn, zoomIn, 28);
        if (isEnabledProperty != null)
        {
            buttonZoomOut.Bind(InputElement.IsEnabledProperty, new Binding(isEnabledProperty));
            buttonZoomIn.Bind(InputElement.IsEnabledProperty, new Binding(isEnabledProperty));
        }

        var info = MakeInfo(zoomInfoProperty);
        info.TextWrapping = TextWrapping.NoWrap;
        info.VerticalAlignment = VerticalAlignment.Center;
        info.Margin = new Thickness(6, 0, 0, 0);
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonZoomOut, buttonZoomIn, info },
        };
    }

    /// <summary>
    /// The info button next to "Lines to draw": a click shows the drawing tips.
    /// </summary>
    internal static Button MakeDrawingTipsButton(System.Func<System.Threading.Tasks.Task> showDrawingTips)
    {
        var button = EditorChrome.CreateToolButton(IconNames.Information, Se.Language.General.Help, null, 28);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(button, Se.Language.Ocr.NOcrDrawHelp);
        }

        button.Click += (_, _) => _ = showDrawingTips();
        return button;
    }

    /// <summary>
    /// The canvas with its tools (zoom, draw mode, ...) in a wrapping row above it.
    /// </summary>
    internal static DockPanel MakeCanvasContent(Control canvasFrame, params Control[] tools)
    {
        var toolbar = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(-6, 0, 0, 8) };
        foreach (var tool in tools)
        {
            tool.Margin = new Thickness(0, 0, 18, 4);
            toolbar.Children.Add(tool);
        }

        DockPanel.SetDock(toolbar, Dock.Top);
        var dock = new DockPanel { LastChildFill = true };
        dock.Children.Add(toolbar);
        dock.Children.Add(canvasFrame);
        return dock;
    }

    /// <summary>
    /// "Draw mode" with the foreground/background toggles beside it.
    /// </summary>
    internal static StackPanel MakeDrawModeBar(ToggleButton foreground, ToggleButton background)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.Ocr.DrawMode, new Thickness(0, 0, 2, 0)),
                foreground,
                background,
            },
        };
    }

    /// <summary>
    /// A switch with its label, the checked state bound two-way to <paramref name="property"/>.
    /// </summary>
    internal static (ToggleSwitch Switch, Grid Row) MakeSwitchRow(string property, string text)
    {
        var toggle = Video.VideoSubDialogChrome.MakeSwitch(property, text);
        return (toggle, Video.VideoSubDialogChrome.MakeSwitchRow(toggle, text));
    }

    /// <summary>
    /// The sections on the panel color above the button bar.
    /// </summary>
    internal static Grid MakeRoot(Control sections, Border buttonBar)
    {
        return Sync.SyncTimingChrome.MakeRoot(sections, buttonBar);
    }
}
