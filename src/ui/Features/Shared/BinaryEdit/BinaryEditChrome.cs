using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

/// <summary>
/// The pieces only the image-based subtitle editor (Binary edit) and its tool dialogs use: the
/// segmented toggles, the settings column beside a framed image preview and slider sections; the
/// rest of their editor look comes from <see cref="EditorChrome"/>.
/// </summary>
internal static class BinaryEditChrome
{
    /// <summary>
    /// Window background, the editor styles, compact combo/text/time code inputs and the
    /// segmented toggle buttons.
    /// </summary>
    internal static void Apply(Window window)
    {
        EditorChrome.ApplyWindow(window, EditorWindowStyles.VideoTool);

        static Style Presenter(System.Func<Selector?, Selector> toggle, params (AvaloniaProperty Property, object Value)[] setters)
        {
            var style = new Style(x => toggle(x).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            foreach (var (property, value) in setters)
            {
                style.Setters.Add(new Setter(property, value));
            }

            return style;
        }

        // Segmented toggle (video preview / position map): flat halves, the chosen one in the accent
        var styles = window.Styles;
        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("segment"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(5)),
                new Setter(TemplatedControl.ForegroundProperty, EditorChrome.IconBrush),
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(12, 3)),
                new Setter(Layoutable.MinHeightProperty, 24.0),
            },
        });
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("segment").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush), (ContentPresenter.ForegroundProperty, EditorChrome.StrongTextBrush)));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("segment").Class(":checked"),
            (ContentPresenter.BackgroundProperty, EditorChrome.AccentBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("segment").Class(":checked").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, EditorChrome.AccentHoverBrush), (ContentPresenter.ForegroundProperty, Brushes.White)));

        // Compact seconds field (duration), like the time code field
        styles.Add(new Style(x => x.OfType<Nikse.SubtitleEdit.Controls.SecondsUpDown>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(Layoutable.MinHeightProperty, 28.0),
            },
        });
    }

    /// <summary>
    /// A tool dialog: settings column on the left, image preview on the right, button bar below.
    /// </summary>
    internal static Grid MakePreviewDialogRoot(Control settings, Control preview, Border buttonBar, double settingsWidth = 300)
    {
        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(settingsWidth)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(new Border
        {
            Child = new ScrollViewer
            {
                Content = settings,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            },
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        }, 0, 0);
        content.Add(new Border
        {
            Child = preview,
            Padding = new Thickness(14),
        }, 0, 1);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(content, 0, 0);
        root.Add(buttonBar, 1, 0);
        return root;
    }

    /// <summary>
    /// A rounded frame around a large image preview, with <paramref name="background"/> behind the
    /// transparent subtitle bitmap.
    /// </summary>
    internal static Border MakePreviewFrame(Control content, IBrush? background)
    {
        return new Border
        {
            Child = content,
            Background = background,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
        };
    }

    /// <summary>
    /// A slider section: title and the current value in the header, the slider below.
    /// </summary>
    internal static Border MakeSliderSection(string title, Slider slider, string valueProperty, Control? below = null, bool first = false)
    {
        AutomationProperties.SetName(slider, title);
        var value = new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(valueProperty),
        };

        Control content = slider;
        if (below != null)
        {
            var panel = new StackPanel { Spacing = 4 };
            panel.Children.Add(slider);
            panel.Children.Add(below);
            content = panel;
        }

        return EditorChrome.MakeEditorSection(title, content, value, first);
    }

    /// <summary>
    /// A dim label of fixed width and the control after it, so the rows line up.
    /// </summary>
    internal static Grid MakeRow(string label, Control content, double labelWidth)
    {
        label = label.TrimEnd(':', ' '); // some strings end with a colon from the old "Label:" layout
        if (string.IsNullOrEmpty(AutomationProperties.GetName(content)) && content is not Panel)
        {
            AutomationProperties.SetName(content, label);
        }

        return EditorChrome.MakeLabeledRow(label, content, labelWidth);
    }
}
