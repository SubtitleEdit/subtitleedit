using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

/// <summary>
/// The pieces only the nOCR dialogs (add character, inspect, history, database edit, train, ...)
/// use: the drawing canvas with its zoom and draw mode tools, text toggles and the columns; the
/// rest of their editor look comes from <see cref="EditorChrome"/>.
/// </summary>
internal static class NOcrChrome
{
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
    /// A column section whose content fills the height below the header (lists, the canvas), with
    /// a line to its left instead of on top - for columns side by side.
    /// </summary>
    internal static Border MakeFillColumn(string title, Control content, Control? headerRight = null, bool first = false)
    {
        return EditorChrome.MakeDockedFillSection(title, content, headerRight, first, lineLeft: true, headerMinHeight: 28);
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

        var info = EditorChrome.MakeBoundInfo(zoomInfoProperty);
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
        var toggle = EditorChrome.MakeSwitch(property, text);
        return (toggle, EditorChrome.MakeSwitchRow(toggle, text));
    }
}
