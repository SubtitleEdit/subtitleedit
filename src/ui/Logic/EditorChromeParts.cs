using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Which extra styles <see cref="EditorChrome.ApplyWindow"/> adds on top of the editor styles and
/// the compact combo boxes every editor window gets.
/// </summary>
[Flags]
internal enum EditorWindowStyles
{
    None = 0,

    /// <summary>Text boxes with class "text" on the field colors, padded, text centered.</summary>
    TextFields = 1 << 0,

    /// <summary>Like <see cref="TextFields"/>, the text not centered vertically.</summary>
    TextFieldsUncentered = 1 << 1,

    /// <summary>Like <see cref="TextFieldsUncentered"/>, with the theme's padding.</summary>
    TextFieldsBare = 1 << 2,

    /// <summary>"text" boxes keep the field color while hovered/focused.</summary>
    FieldStates = 1 << 3,

    /// <summary>"text" boxes keep the field color while hovered/focused, with an accent border on focus.</summary>
    FieldStatesAccent = 1 << 4,

    /// <summary>The text box of an AutoCompleteBox in the field look.</summary>
    AutoComplete = 1 << 5,

    /// <summary>Compact time code fields.</summary>
    TimeCodes = 1 << 6,

    /// <summary>Compact time code fields with compact inner text box and spinner.</summary>
    CompactTimeCodes = 1 << 7,

    /// <summary>A disabled primary button is faded.</summary>
    FadeDisabledPrimary = 1 << 8,

    /// <summary>A disabled primary button drops the accent (it looked clickable).</summary>
    MuteDisabledPrimary = 1 << 9,

    /// <summary>A split button with class "primary" in the accent (muted when disabled with <see cref="MuteDisabledPrimary"/>).</summary>
    PrimarySplitButton = 1 << 10,

    /// <summary>Small tab headers (34px) instead of the big Fluent ones.</summary>
    CompactTabs = 1 << 11,

    /// <summary>Picker lists: transparent list boxes with rounded rows and the soft accent selection,
    /// compact spinners, small tabs (32px) and check/radio buttons in the field font size.</summary>
    PickerLists = 1 << 12,

    /// <summary>Toggle buttons with class "tool": bordered, the accent while checked.</summary>
    ToolToggles = 1 << 13,

    /// <summary>Like <see cref="ToolToggles"/>, sized for a text and with a hover color.</summary>
    TextToolToggles = 1 << 14,

    /// <summary>Compact split buttons.</summary>
    CompactSplitButtons = 1 << 15,

    /// <summary>List boxes with class "compact": small rounded rows.</summary>
    CompactListRows = 1 << 16,

    /// <summary>The form dialogs (find, replace, line tools, pickers, ...): compact time codes and
    /// text fields that keep the field color, AutoCompleteBox included, a faded disabled primary.</summary>
    Dialog = TextFields | FieldStates | AutoComplete | TimeCodes | FadeDisabledPrimary,

    /// <summary>The video dialogs: text fields, a muted disabled primary, an accent split button.</summary>
    Video = TextFields | MuteDisabledPrimary | PrimarySplitButton,

    /// <summary>The pickers and prompts: <see cref="Dialog"/> with <see cref="PickerLists"/>.</summary>
    Picker = Dialog | PickerLists,

    /// <summary>The format properties and export settings forms: <see cref="Dialog"/> with compact
    /// time codes and tabs.</summary>
    Form = Dialog | CompactTimeCodes | CompactTabs,

    /// <summary>The video tool windows (video OCR, shot changes, open from URL, binary edit, ...):
    /// text fields, compact time codes and tabs, a muted disabled primary.</summary>
    VideoTool = TextFields | CompactTimeCodes | CompactTabs | MuteDisabledPrimary,

    /// <summary>The engine settings and download dialogs (OCR, llama.cpp, ffmpeg, ...): text fields
    /// with an accent border on focus, a muted disabled primary.</summary>
    EngineDialog = TextFieldsUncentered | FieldStatesAccent | MuteDisabledPrimary,

    /// <summary>The speech-to-text windows: text fields with an accent border on focus, toggle tool
    /// buttons, an accent split button.</summary>
    SpeechToText = TextFields | FieldStatesAccent | ToolToggles | PrimarySplitButton,
}

/// <summary>
/// The building blocks the editor-look windows share: window setup, the bottom button bar, the
/// progress strip, list frames and sections, switch and label rows, flat action buttons, hints
/// and the download dialog layout.
/// </summary>
internal static partial class EditorChrome
{
    /// <summary>The class of the compact text fields (see <see cref="EditorWindowStyles.TextFields"/>).</summary>
    internal const string TextClass = "text";

    private static readonly IBrush DownloadErrorBrush = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B));

    /// <summary>
    /// Window background, the editor styles, compact combo boxes and the given extra styles.
    /// </summary>
    internal static void ApplyWindow(Window window, EditorWindowStyles extra = EditorWindowStyles.None)
    {
        window.Background = WindowBrush;
        AddWindowStyles(window.Styles, extra);
    }

    /// <summary>
    /// The styles of <see cref="ApplyWindow"/> without the window background.
    /// </summary>
    internal static void AddWindowStyles(Styles styles, EditorWindowStyles extra = EditorWindowStyles.None)
    {
        AddEditorStyles(styles);
        styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });

        if (extra.HasFlag(EditorWindowStyles.TimeCodes) || extra.HasFlag(EditorWindowStyles.CompactTimeCodes))
        {
            styles.Add(new Style(x => x.OfType<TimeCodeUpDown>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.FontSizeProperty, 12.5),
                    new Setter(TemplatedControl.MinHeightProperty, 28.0),
                },
            });
        }

        if (extra.HasFlag(EditorWindowStyles.TextFields) || extra.HasFlag(EditorWindowStyles.TextFieldsUncentered) || extra.HasFlag(EditorWindowStyles.TextFieldsBare))
        {
            AddTextFieldStyles(styles, extra);
        }

        if (extra.HasFlag(EditorWindowStyles.FieldStatesAccent))
        {
            // Keep the field color when hovered/focused (the theme turns it black/white otherwise)
            foreach (var state in new[] { ":pointerover", ":focus" })
            {
                var focus = state == ":focus";
                styles.Add(new Style(x => x.OfType<TextBox>().Class(TextClass).Class(state).Template().OfType<Border>().Name("PART_BorderElement"))
                {
                    Setters =
                    {
                        new Setter(Border.BackgroundProperty, FieldBrush),
                        new Setter(Border.BorderBrushProperty, focus ? AccentBrush : SeparatorBrush),
                    },
                });
            }
        }

        if (extra.HasFlag(EditorWindowStyles.FieldStates))
        {
            // Keep the field color while hovered/focused (the theme turns a focused box black/white)
            foreach (var state in new[] { ":pointerover", ":focus" })
            {
                styles.Add(new Style(x => x.OfType<TextBox>().Class(TextClass).Class(state).Template().OfType<Border>().Name("PART_BorderElement"))
                {
                    Setters = { new Setter(Border.BackgroundProperty, FieldBrush) },
                });
                if (extra.HasFlag(EditorWindowStyles.AutoComplete))
                {
                    styles.Add(new Style(x => x.OfType<AutoCompleteBox>().Template().OfType<TextBox>().Class(state).Template().OfType<Border>().Name("PART_BorderElement"))
                    {
                        Setters = { new Setter(Border.BackgroundProperty, FieldBrush) },
                    });
                }
            }
        }

        if (extra.HasFlag(EditorWindowStyles.FadeDisabledPrimary))
        {
            // The accent of a primary button would hide that it is disabled (Restore selected
            // without a selection)
            styles.Add(new Style(x => x.OfType<Button>().Class("primary").Class(":disabled"))
            {
                Setters = { new Setter(Visual.OpacityProperty, 0.45) },
            });
        }

        if (extra.HasFlag(EditorWindowStyles.CompactTimeCodes))
        {
            styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<TextBox>())
            {
                Setters = { new Setter(TemplatedControl.MinHeightProperty, 26.0) },
            });
            styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<ButtonSpinner>())
            {
                Setters = { new Setter(TemplatedControl.MinHeightProperty, 26.0) },
            });
        }

        if (extra.HasFlag(EditorWindowStyles.CompactTabs))
        {
            AddTabStyle(styles, 34);
        }

        if (extra.HasFlag(EditorWindowStyles.MuteDisabledPrimary))
        {
            // A disabled primary (while working) must not keep the accent - it looked clickable
            styles.Add(new Style(x => x.OfType<Button>().Class("primary").Class(":disabled").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, HoverBrush),
                    new Setter(ContentPresenter.ForegroundProperty, FaintTextBrush),
                },
            });
        }

        if (extra.HasFlag(EditorWindowStyles.ToolToggles) || extra.HasFlag(EditorWindowStyles.TextToolToggles))
        {
            AddToolToggleStyles(styles, extra.HasFlag(EditorWindowStyles.TextToolToggles));
        }

        if (extra.HasFlag(EditorWindowStyles.PrimarySplitButton))
        {
            AddPrimarySplitButtonStyles(styles, extra.HasFlag(EditorWindowStyles.MuteDisabledPrimary));
        }

        if (extra.HasFlag(EditorWindowStyles.CompactSplitButtons))
        {
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

        if (extra.HasFlag(EditorWindowStyles.PickerLists))
        {
            AddPickerListStyles(styles);
        }

        if (extra.HasFlag(EditorWindowStyles.CompactListRows))
        {
            styles.Add(new Style(x => x.OfType<ListBox>().Class("compact").Descendant().OfType<ListBoxItem>())
            {
                Setters =
                {
                    new Setter(ListBoxItem.PaddingProperty, new Thickness(8, 3)),
                    new Setter(ListBoxItem.MarginProperty, new Thickness(2, 1)),
                    new Setter(ListBoxItem.CornerRadiusProperty, new CornerRadius(5)),
                    new Setter(ListBoxItem.FontSizeProperty, 12.5),
                },
            });
        }
    }

    private static void AddTextFieldStyles(Styles styles, EditorWindowStyles extra)
    {
        var bare = extra.HasFlag(EditorWindowStyles.TextFieldsBare);
        var centered = extra.HasFlag(EditorWindowStyles.TextFields) && !bare;
        var style = new Style(x => x.OfType<TextBox>().Class(TextClass));
        var autoCompleteStyle = new Style(x => x.OfType<AutoCompleteBox>().Template().OfType<TextBox>());
        var setters = new System.Collections.Generic.List<Setter>
        {
            new(TemplatedControl.FontSizeProperty, 12.5),
            new(TemplatedControl.MinHeightProperty, 28.0),
        };
        if (!bare)
        {
            setters.Add(new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 4)));
        }

        setters.Add(new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)));
        setters.Add(new Setter(TemplatedControl.BackgroundProperty, FieldBrush));
        setters.Add(new Setter(TemplatedControl.BorderBrushProperty, FieldBorderBrush));
        if (centered)
        {
            setters.Add(new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        }

        foreach (var setter in setters)
        {
            style.Setters.Add(setter);
            autoCompleteStyle.Setters.Add(new Setter(setter.Property!, setter.Value));
        }

        styles.Add(style);
        if (extra.HasFlag(EditorWindowStyles.AutoComplete))
        {
            styles.Add(autoCompleteStyle);
        }
    }

    private static void AddTabStyle(Styles styles, double minHeight)
    {
        // Tabs: small headers instead of the big Fluent ones
        styles.Add(new Style(x => x.OfType<TabItem>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 13.0),
                new Setter(Layoutable.MinHeightProperty, minHeight),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(12, 0)),
            },
        });
    }

    private static void AddPrimarySplitButtonStyles(Styles styles, bool mutedWhenDisabled)
    {
        // A split button as the main action: both halves in the accent
        foreach (var isHover in new[] { false, true })
        {
            styles.Add(new Style(x =>
            {
                var selector = x.OfType<SplitButton>().Class("primary").Template().OfType<Button>();
                return isHover
                    ? selector.Class(":pointerover").Template().OfType<ContentPresenter>()
                    : selector.Template().OfType<ContentPresenter>();
            })
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, isHover ? AccentHoverBrush : AccentBrush),
                    new Setter(ContentPresenter.ForegroundProperty, Brushes.White),
                },
            });
        }

        if (mutedWhenDisabled)
        {
            styles.Add(new Style(x => x.OfType<SplitButton>().Class("primary").Class(":disabled").Template().OfType<Button>().Template().OfType<ContentPresenter>())
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, HoverBrush),
                    new Setter(ContentPresenter.ForegroundProperty, FaintTextBrush),
                },
            });
        }
    }

    private static void AddToolToggleStyles(Styles styles, bool forText)
    {
        // Toggle tool buttons ("Enable VAD", draw mode, train bold/italic, ...)
        var baseStyle = new Style(x => x.OfType<ToggleButton>().Class("tool"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)),
                new Setter(TemplatedControl.BorderBrushProperty, FieldBorderBrush),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        };
        if (forText)
        {
            baseStyle.Setters.Add(new Setter(TemplatedControl.FontSizeProperty, 12.5));
            baseStyle.Setters.Add(new Setter(TemplatedControl.MinHeightProperty, 28.0));
            baseStyle.Setters.Add(new Setter(TemplatedControl.PaddingProperty, new Thickness(10, 3)));
            baseStyle.Setters.Add(new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        }

        styles.Add(baseStyle);
        if (forText)
        {
            styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool").Class(":pointerover").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, HoverBrush),
                    new Setter(ContentPresenter.BorderBrushProperty, FieldBorderBrush),
                    new Setter(ContentPresenter.ForegroundProperty, StrongTextBrush),
                },
            });
        }

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
                    new Setter(ContentPresenter.BackgroundProperty, isHover ? AccentHoverBrush : AccentBrush),
                    new Setter(ContentPresenter.BorderBrushProperty, isHover ? AccentHoverBrush : AccentBrush),
                    new Setter(ContentPresenter.ForegroundProperty, Brushes.White),
                },
            });
        }
    }

    private static void AddPickerListStyles(Styles styles)
    {
        styles.Add(new Style(x => x.OfType<ListBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(4)),
            },
        });
        styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 4)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(5)),
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(Layoutable.MinHeightProperty, 26.0),
            },
        });
        foreach (var hover in new[] { false, true })
        {
            styles.Add(new Style(x => (hover
                    ? x.OfType<ListBoxItem>().Class(":selected").Class(":pointerover")
                    : x.OfType<ListBoxItem>().Class(":selected"))
                .Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, AccentSoftBrush),
                    new Setter(ContentPresenter.ForegroundProperty, StrongTextBrush),
                },
            });
        }

        // Compact number fields that keep their spinners (layer, line numbers, scan settings)
        styles.Add(new Style(x => x.OfType<NumericUpDown>().Not(y => y.Class("field")))
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(Layoutable.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        AddTabStyle(styles, 32);
        styles.Add(new Style(x => x.OfType<CheckBox>())
        {
            Setters = { new Setter(TemplatedControl.FontSizeProperty, 12.5) },
        });
        styles.Add(new Style(x => x.OfType<RadioButton>())
        {
            Setters = { new Setter(TemplatedControl.FontSizeProperty, 12.5) },
        });
    }

    // ---- Buttons ----

    /// <summary>
    /// Marks <paramref name="button"/> as the window's accent primary (OK, Generate, Done, ...).
    /// </summary>
    internal static T AsPrimary<T>(this T button) where T : Button
    {
        button.Classes.Add("primary");
        return button;
    }

    /// <summary>
    /// Flat button with an icon and its label beside it, for toolbars and list actions. The label
    /// can follow <paramref name="textBinding"/> instead of <paramref name="text"/>.
    /// </summary>
    internal static Button MakeActionButton(string icon, string? text, System.Windows.Input.ICommand? command, string? textBinding = null)
    {
        var label = new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center };
        if (textBinding != null)
        {
            label.Bind(TextBlock.TextProperty, new Binding(textBinding) { Mode = BindingMode.OneWay });
        }

        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7, Margin = new Thickness(10, 0) };
        content.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 13, VerticalAlignment = VerticalAlignment.Center });
        content.Children.Add(label);
        var button = new Button
        {
            Content = content,
            Height = 30,
            Command = command,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");

        // Icon + label in a panel: a screen reader needs the name spelled out (#12087)
        if (textBinding != null)
        {
            button.Bind(AutomationProperties.NameProperty, new Binding(textBinding) { Mode = BindingMode.OneWay });
        }
        else
        {
            AutomationProperties.SetName(button, text);
        }

        return button;
    }

    /// <summary>
    /// A smaller flat button with an icon and a short text, for actions that need their words.
    /// The text keeps its access key ("_Find next" shows "Find next" with the F underlined and
    /// Alt+F clicks it).
    /// </summary>
    internal static Button MakeTextToolButton(string icon, string text, System.Windows.Input.ICommand? command)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
        content.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 12, VerticalAlignment = VerticalAlignment.Center });
        content.Children.Add(new AccessText { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button
        {
            Content = content,
            Command = command,
            Height = 28,
            Padding = new Thickness(8, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");

        // Icon + label in a panel: a screen reader needs the name spelled out (#12087)
        AutomationProperties.SetName(button, text.Replace("_", string.Empty));
        return button;
    }

    /// <summary>
    /// Turns a text button (icon + text) into a flat editor action.
    /// </summary>
    internal static T AsFlatAction<T>(this T button) where T : Button
    {
        button.Classes.Add("tool");
        button.Height = 30;
        button.Padding = new Thickness(10, 0);
        button.FontSize = 12.5;
        button.Margin = new Thickness(0);
        button.VerticalAlignment = VerticalAlignment.Center;
        return button;
    }

    /// <summary>
    /// A flat square tool button made from a button that already carries an icon (UiUtil's icon
    /// buttons, whose icon a view model may swap).
    /// </summary>
    internal static T AsToolButton<T>(this T button, double size = 28) where T : Button
    {
        button.Classes.Add("tool");
        button.Width = size;
        button.Height = size;
        button.MinWidth = 0;
        button.MinHeight = 0;
        button.Padding = new Thickness(0);
        button.FontSize = 14;
        return button;
    }

    /// <summary>
    /// A text action button of an action column: stretched, left aligned, icon and text, with the
    /// access key (Alt+letter) kept from <see cref="UiUtil.MakeButton(string, CommunityToolkit.Mvvm.Input.IRelayCommand?, object?)"/>.
    /// </summary>
    internal static Button StyleActionColumnButton(Button button)
    {
        button.Margin = new Thickness(0);
        button.MinWidth = 0;
        button.MinHeight = 30;
        button.Padding = new Thickness(10, 5);
        button.FontSize = 12.5;
        button.HorizontalAlignment = HorizontalAlignment.Stretch;
        button.HorizontalContentAlignment = HorizontalAlignment.Left;
        button.CornerRadius = new CornerRadius(6);
        return button;
    }

    /// <summary>
    /// An action column on the button bar color, with a line to its left - the buttons of the
    /// non-modal find/replace windows, which have no OK/Cancel.
    /// </summary>
    internal static Border MakeActionColumn(Control content)
    {
        return new Border
        {
            Child = content,
            Background = ButtonBarBrush,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
            Padding = new Thickness(14, 14, 14, 12),
        };
    }

    /// <summary>
    /// The sections on the panel color, the action column to the right.
    /// </summary>
    internal static Grid MakeSidePanelRoot(Control sections, Control actions)
    {
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(new Border { Background = PanelBrush, Child = sections }, 0, 0);
        grid.Add(actions, 0, 1);
        return grid;
    }

    /// <summary>
    /// Icon actions for a list section header (select all / none / invert, add, remove, ...).
    /// </summary>
    internal static StackPanel MakeListActions(params Control[] buttons)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
        };
        foreach (var button in buttons)
        {
            panel.Children.Add(button);
        }

        return panel;
    }

    internal static Button MakeSelectAllButton(System.Windows.Input.ICommand command) =>
        CreateToolButton("fa-solid fa-square-check", Config.Se.Language.General.SelectAll, command, 26);

    internal static Button MakeSelectNoneButton(System.Windows.Input.ICommand command) =>
        CreateToolButton("fa-regular fa-square", Config.Se.Language.General.SelectNone, command, 26);

    internal static Button MakeInvertSelectionButton(System.Windows.Input.ICommand command) =>
        CreateToolButton("fa-solid fa-right-left", Config.Se.Language.General.InvertSelection, command, 26);

    // ---- Bars and roots ----

    /// <summary>
    /// The bottom bar: optional status/summary on the left, the buttons on the right (pass them as
    /// Cancel, ..., primary).
    /// </summary>
    internal static Border MakeButtonBar(Control? left, params Control[] buttons)
    {
        return MakeButtonBar(left, false, buttons);
    }

    /// <summary>
    /// <see cref="MakeButtonBar(Control?, Control[])"/> with the buttons' own margins dropped (the
    /// UiUtil buttons have 4px side margins), so only the bar spaces them.
    /// </summary>
    internal static Border MakeFlushButtonBar(Control? left, params Control[] buttons)
    {
        return MakeButtonBar(left, true, buttons);
    }

    private static Border MakeButtonBar(Control? left, bool flush, Control[] buttons)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
        foreach (var button in buttons)
        {
            if (flush)
            {
                button.Margin = new Thickness(0);
            }

            panel.Children.Add(button);
        }

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        if (left != null)
        {
            left.VerticalAlignment = VerticalAlignment.Center;
            grid.Add(left, 0, 0);
        }

        grid.Add(panel, 0, 1);
        return WrapButtonBar(grid);
    }

    /// <summary>
    /// The bottom bar of the timing dialogs: the buttons docked right, the left content filling
    /// the rest.
    /// </summary>
    internal static Border MakeDockedButtonBar(Control? left, params Control[] buttons)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };
        foreach (var button in buttons)
        {
            panel.Children.Add(button);
        }

        var bar = new DockPanel();
        DockPanel.SetDock(panel, Dock.Right);
        bar.Children.Add(panel);
        if (left != null)
        {
            bar.Children.Add(left);
        }

        return WrapButtonBar(bar);
    }

    /// <summary>
    /// <paramref name="content"/> on the button bar color with the line on top.
    /// </summary>
    internal static Border WrapButtonBar(Control content)
    {
        return new Border
        {
            Child = content,
            Background = ButtonBarBrush,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }

    /// <summary>
    /// A dim status/summary text for the left side of the button bar, bound to <paramref name="property"/>.
    /// </summary>
    internal static TextBlock MakeStatusText(string property, object? dataContext = null)
    {
        var text = new TextBlock
        {
            Foreground = DimTextBrush,
            FontSize = 12.5,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 0, 12, 0),
            [!TextBlock.TextProperty] = new Binding(property),
        };
        if (dataContext != null)
        {
            text.DataContext = dataContext;
        }

        return text;
    }

    /// <summary>
    /// The strip above the button bar that holds a progress bar while working.
    /// </summary>
    internal static Border MakeProgressStrip(Control content)
    {
        return new Border
        {
            Child = content,
            Background = StatusBrush,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 8),
        };
    }

    /// <summary>
    /// Progress bar with the status text beside it, on the status color, shown while
    /// <paramref name="visibleProperty"/> is true.
    /// </summary>
    internal static Border MakeProgressStrip(ProgressBar progressBar, string textProperty, string visibleProperty)
    {
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        var statusText = MakeBoundHint(textProperty);
        statusText.FontSize = 12;
        statusText.MaxWidth = 360;

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(progressBar, 0, 0);
        grid.Add(statusText, 0, 1);

        var strip = MakeProgressStrip(grid);
        strip[!Visual.IsVisibleProperty] = new Binding(visibleProperty);
        return strip;
    }

    /// <summary>
    /// The content on the panel color above the button bar; the content takes the rest of the
    /// height (<paramref name="stretch"/>) or its own height.
    /// </summary>
    internal static Grid MakeRoot(Control content, Border buttonBar, bool stretch = true)
    {
        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(stretch ? new GridLength(1, GridUnitType.Star) : GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(new Border { Background = PanelBrush, Child = content }, 0, 0);
        root.Add(buttonBar, 1, 0);
        return root;
    }

    /// <summary>
    /// The panel color behind <paramref name="content"/>.
    /// </summary>
    internal static Border MakePanel(Control content)
    {
        return new Border
        {
            Child = content,
            Background = PanelBrush,
        };
    }

    /// <summary>
    /// A toolbar strip on the panel color with a line under it.
    /// </summary>
    internal static Border MakeToolbar(Control content)
    {
        return new Border
        {
            Child = content,
            Background = PanelBrush,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 6),
        };
    }

    // ---- Sections and frames ----

    /// <summary>
    /// A list (table view, list box) or text area in a rounded frame on the field color. With
    /// <paramref name="clearListBackground"/> the list's own background is cleared so the frame
    /// shows through.
    /// </summary>
    internal static Border MakeListFrame(Control list, bool clearListBackground = false)
    {
        if (clearListBackground && list is TemplatedControl templated)
        {
            templated.Background = Brushes.Transparent;
        }

        return new Border
        {
            Child = list,
            Background = FieldBrush,
            BorderBrush = FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
        };
    }

    /// <summary>
    /// A plain frame around a video player: no corner radius and no clipping, as the native video
    /// surface on macOS disappears inside either.
    /// </summary>
    internal static Border MakeVideoFrame(Control player)
    {
        return new Border
        {
            Child = player,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(1),
        };
    }

    /// <summary>
    /// A section header line: the small caps title, optional controls on the right.
    /// </summary>
    internal static DockPanel MakeSectionHeaderRow(string title, Control? headerRight = null, double minHeight = 26)
    {
        var header = new DockPanel { MinHeight = minHeight };
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        header.Children.Add(MakeSectionHeader(title));
        return header;
    }

    /// <summary>
    /// A section whose content (a list, a text area, ...) fills the rest of it: the small caps
    /// header (with optional controls on the right) above the content. The line is on top unless
    /// it is the <paramref name="first"/> section, or on the left for columns side by side.
    /// </summary>
    internal static Border MakeFillSection(string title, Control content, Control? headerRight = null, bool first = false, bool lineLeft = false)
    {
        var grid = new Grid
        {
            RowSpacing = 8,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(MakeSectionHeaderRow(title, headerRight), 0, 0);
        grid.Add(content, 1, 0);
        return new Border
        {
            Child = grid,
            BorderBrush = LineBrush,
            BorderThickness = lineLeft ? new Thickness(1, 0, 0, 0) : new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    /// <summary>
    /// A section whose list fills the rest of it: the small caps header (with optional actions on
    /// the right) above the list in a rounded frame.
    /// </summary>
    internal static Border MakeListSection(string title, Control list, Control? headerRight = null, bool first = false)
    {
        return MakeFillSection(title, MakeListFrame(list, clearListBackground: true), headerRight, first);
    }

    /// <summary>
    /// A docked fill section (lists and images that grow with the window): the header with a gap
    /// of <paramref name="headerGap"/> above the content, which takes the rest.
    /// </summary>
    internal static Border MakeDockedFillSection(string title, Control content, Control? headerRight = null, bool first = false, bool lineLeft = false, double headerMinHeight = 0, double headerGap = 10)
    {
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, headerGap) };
        if (headerMinHeight > 0)
        {
            header.MinHeight = headerMinHeight;
        }

        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            headerRight.VerticalAlignment = VerticalAlignment.Center;
            header.Children.Add(headerRight);
        }

        header.Children.Add(MakeSectionHeader(title));
        var panel = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(header, Dock.Top);
        panel.Children.Add(header);
        panel.Children.Add(content);
        return new Border
        {
            Child = panel,
            BorderBrush = LineBrush,
            BorderThickness = lineLeft ? new Thickness(first ? 0 : 1, 0, 0, 0) : new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    /// <summary>
    /// Two sections side by side with a line between them; the left one as wide as it needs.
    /// </summary>
    internal static Grid MakeSideBySide(Control left, Control right)
    {
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(left, 0, 0);
        grid.Add(new Border
        {
            Child = right,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);
        return grid;
    }

    /// <summary>
    /// Two panes of the same width side by side with a line between them.
    /// </summary>
    internal static Grid MakeTwoPanes(Control left, Control right)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        grid.Add(left, 0, 0);
        grid.Add(new Border
        {
            Child = right,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);
        return grid;
    }

    /// <summary>
    /// The title block at the top of a dialog: the title, a dim description under it.
    /// </summary>
    internal static Border MakeDialogHeader(string title, string? description, double maxWidth = double.PositiveInfinity)
    {
        var panel = new StackPanel { Spacing = 2 };
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        });
        if (!string.IsNullOrEmpty(description))
        {
            panel.Children.Add(new TextBlock
            {
                Text = description,
                FontSize = 12.5,
                Foreground = DimTextBrush,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = maxWidth,
                HorizontalAlignment = HorizontalAlignment.Left,
            });
        }

        return new Border { Child = panel, Padding = new Thickness(14, 14, 14, 12) };
    }

    // ---- Rows, fields and texts ----

    /// <summary>
    /// A dim label of fixed width and the control after it, so the rows line up. The label is cut
    /// with an ellipsis, or wraps with <paramref name="wrap"/>; <paramref name="alignTop"/> puts
    /// it in line with the first line of a taller control.
    /// </summary>
    internal static Grid MakeLabeledRow(string? label, Control content, double labelWidth, double spacing = 8, bool alignTop = false, bool wrap = false, string? labelBinding = null, string? visibleProperty = null)
    {
        return MakeLabeledRow(label, content, labelWidth, out _, spacing, alignTop, wrap, labelBinding, visibleProperty);
    }

    /// <inheritdoc cref="MakeLabeledRow(string?, Control, double, double, bool, bool, string?, string?)"/>
    internal static Grid MakeLabeledRow(string? label, Control content, double labelWidth, out TextBlock labelControl, double spacing = 8, bool alignTop = false, bool wrap = false, string? labelBinding = null, string? visibleProperty = null)
    {
        labelControl = MakeDimLabel(label ?? string.Empty, new Thickness(0, alignTop ? 6 : 0, 0, 0));
        if (wrap)
        {
            labelControl.TextWrapping = TextWrapping.Wrap;
        }
        else
        {
            labelControl.TextTrimming = TextTrimming.CharacterEllipsis;
        }

        if (alignTop)
        {
            labelControl.VerticalAlignment = VerticalAlignment.Top;
        }

        if (labelBinding != null)
        {
            labelControl.Bind(TextBlock.TextProperty, new Binding(labelBinding) { Mode = BindingMode.OneWay });
        }

        var row = MakeLabeledRow(labelControl, content, labelWidth, spacing, styleLabel: false);
        if (visibleProperty != null)
        {
            row.Bind(Visual.IsVisibleProperty, new Binding(visibleProperty));
        }

        return row;
    }

    /// <summary>
    /// <paramref name="label"/> (made a dim, one-line label unless <paramref name="styleLabel"/> is
    /// false) in a column of fixed width and the control after it.
    /// </summary>
    internal static Grid MakeLabeledRow(TextBlock label, Control content, double labelWidth, double spacing = 8, bool styleLabel = true)
    {
        if (styleLabel)
        {
            label.Foreground = DimTextBrush;
            label.FontSize = 12;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.TextTrimming = TextTrimming.CharacterEllipsis;
            label.Margin = new Thickness(0);
        }

        var grid = new Grid
        {
            ColumnSpacing = spacing,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(labelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(label, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

    /// <summary>
    /// A dim label above its control (for wide inputs like URLs), linked to it for screen readers.
    /// </summary>
    internal static StackPanel MakeLabelAbove(string label, Control content)
    {
        var text = MakeDimLabel(label, new Thickness(0));
        AutomationProperties.SetLabeledBy(content, text);
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(text);
        panel.Children.Add(content);
        return panel;
    }

    /// <summary>
    /// A dim label that wraps.
    /// </summary>
    internal static TextBlock MakeWrappingDimLabel(string text)
    {
        var label = MakeDimLabel(text, new Thickness(0));
        label.TextWrapping = TextWrapping.Wrap;
        return label;
    }

    /// <summary>
    /// Dim explanatory text that wraps.
    /// </summary>
    internal static TextBlock MakeHint(string? text, double fontSize = 11.5) => new()
    {
        Text = text,
        FontSize = fontSize,
        Foreground = DimTextBrush,
        TextWrapping = TextWrapping.Wrap,
    };

    /// <summary>
    /// Dim one-line text bound to <paramref name="property"/>, cut with an ellipsis.
    /// </summary>
    internal static TextBlock MakeBoundHint(string property, double fontSize = 11.5) => new()
    {
        FontSize = fontSize,
        Foreground = DimTextBrush,
        VerticalAlignment = VerticalAlignment.Center,
        TextTrimming = TextTrimming.CharacterEllipsis,
        [!TextBlock.TextProperty] = new Binding(property),
    };

    /// <summary>
    /// Dim text bound to <paramref name="property"/> that wraps (info lines like the image size).
    /// </summary>
    internal static TextBlock MakeBoundInfo(string property, object? source = null, double fontSize = 12)
    {
        var binding = new Binding(property);
        if (source != null)
        {
            binding.Source = source;
        }

        return new TextBlock
        {
            FontSize = fontSize,
            Foreground = DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = binding,
        };
    }

    /// <summary>
    /// A compact text field (class "text") bound two-way to <paramref name="property"/>.
    /// </summary>
    internal static TextBox MakeTextField(string property, string accessibleName, double width = double.NaN)
    {
        var textBox = new TextBox
        {
            Width = width,
            HorizontalAlignment = double.IsNaN(width) ? HorizontalAlignment.Stretch : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(property) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };
        textBox.Classes.Add(TextClass);
        AutomationProperties.SetName(textBox, accessibleName);
        return textBox;
    }

    /// <summary>
    /// Gives a text box the compact field look (class "text"), optionally with a screen reader name.
    /// </summary>
    internal static T AsTextField<T>(this T textBox, string? accessibleName = null) where T : TextBox
    {
        textBox.Classes.Add(TextClass);
        if (accessibleName != null)
        {
            AutomationProperties.SetName(textBox, accessibleName);
        }

        return textBox;
    }

    /// <summary>
    /// A multi-line text box on the field colors (class "text"), text from the top.
    /// </summary>
    internal static T AsTextArea<T>(this T textBox) where T : TextBox
    {
        textBox.Classes.Add(TextClass);
        textBox.VerticalContentAlignment = VerticalAlignment.Top;
        textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBox.VerticalAlignment = VerticalAlignment.Stretch;
        textBox.Padding = new Thickness(8, 6);
        return textBox;
    }

    /// <summary>
    /// A compact search box with the magnifier and clear icons, bound to <paramref name="property"/>.
    /// </summary>
    internal static TextBox MakeSearchBox(object source, string property, string? placeholder, double width = 240)
    {
        var textBox = new TextBox
        {
            Width = width,
            PlaceholderText = placeholder,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            [!TextBox.TextProperty] = new Binding(property) { Source = source, Mode = BindingMode.TwoWay },
        }.WithSearchAndClearIcons();
        textBox.Classes.Add(TextClass);
        AutomationProperties.SetName(textBox, Config.Se.Language.General.Search);
        return textBox;
    }

    /// <summary>
    /// Gives a number field the compact look (no spinner, see <see cref="MakeField"/>) and a screen
    /// reader name; with a <paramref name="width"/> it gets that width, left aligned with
    /// <paramref name="alignLeft"/>, vertically centered with <paramref name="center"/>.
    /// </summary>
    internal static NumericUpDown AsField(this NumericUpDown field, string accessibleName, double width = double.NaN, bool alignLeft = false, bool center = false, bool themeForeground = false)
    {
        field.Classes.Add("field");
        if (themeForeground)
        {
            field.ClearValue(TemplatedControl.ForegroundProperty); // the theme color, not the fixed text color
        }

        if (!double.IsNaN(width))
        {
            field.Width = width;
        }

        if (alignLeft)
        {
            field.HorizontalAlignment = HorizontalAlignment.Left;
        }

        if (center)
        {
            field.VerticalAlignment = VerticalAlignment.Center;
        }

        AutomationProperties.SetName(field, accessibleName);
        return field;
    }

    // ---- Switches ----

    /// <summary>
    /// A compact on/off switch bound two-way to <paramref name="property"/> (of
    /// <paramref name="source"/>, or the data context), named for screen readers.
    /// </summary>
    internal static ToggleSwitch MakeSwitch(string property, string? accessibleName, object? source = null)
    {
        var binding = new Binding(property) { Mode = BindingMode.TwoWay };
        if (source != null)
        {
            binding.Source = source;
        }

        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = binding,
        };
        if (!string.IsNullOrEmpty(accessibleName))
        {
            AutomationProperties.SetName(toggle, accessibleName);
        }

        return toggle;
    }

    /// <summary>
    /// A switch with its label beside it; the label wraps instead of being cut off.
    /// </summary>
    internal static Grid MakeSwitchRow(ToggleSwitch toggle, string text)
    {
        return MakeSwitchRow(toggle, MakeSwitchLabel(text));
    }

    /// <summary>
    /// A switch with its label beside it, and optional controls on the right.
    /// </summary>
    internal static Grid MakeSwitchRow(ToggleSwitch toggle, TextBlock label, params Control[] trailing)
    {
        label.FontSize = 12.5;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.TextWrapping = TextWrapping.Wrap;
        label.Margin = new Thickness(0);
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        row.Add(toggle, 0, 0);
        row.Add(label, 0, 1);
        for (var i = 0; i < trailing.Length; i++)
        {
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            row.Add(trailing[i], 0, i + 2);
        }

        return row;
    }

    /// <summary>
    /// A switch bound to <paramref name="property"/> with its label beside it.
    /// </summary>
    internal static Grid MakeSwitchRow(string property, string text)
    {
        return MakeSwitchRow(MakeSwitch(property, text), text);
    }

    /// <inheritdoc cref="MakeSwitchRow(string, string)"/>
    internal static Grid MakeSwitchRow(string property, string text, out ToggleSwitch toggle)
    {
        toggle = MakeSwitch(property, text);
        return MakeSwitchRow(toggle, text);
    }

    /// <summary>
    /// The label of a switch row: field font size, wrapping.
    /// </summary>
    internal static TextBlock MakeSwitchLabel(string? text) => new()
    {
        Text = text,
        FontSize = 12.5,
        VerticalAlignment = VerticalAlignment.Center,
        TextWrapping = TextWrapping.Wrap,
    };

    /// <summary>
    /// <see cref="MakeToggleRow(string, string)"/> that also returns the switch, for event hookups.
    /// </summary>
    internal static StackPanel MakeToggleRow(string property, string text, out ToggleSwitch toggle)
    {
        var row = MakeToggleRow(property, text);
        toggle = (ToggleSwitch)row.Children[0];
        return row;
    }

    /// <summary>
    /// A colored status dot with its (bound) text, e.g. "Installed" in green.
    /// </summary>
    internal static StackPanel MakeStatusDot(string brushBindingPath, string labelBindingPath)
    {
        var dot = new Ellipse
        {
            Width = 9,
            Height = 9,
            VerticalAlignment = VerticalAlignment.Center,
            [!Shape.FillProperty] = new Binding(brushBindingPath),
        };
        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12.5,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(8, 0, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(labelBindingPath),
        };
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { dot, text },
        };
    }

    // ---- Download dialogs ----

    /// <summary>
    /// The title of a download dialog (fixed text, or bind it afterwards).
    /// </summary>
    internal static TextBlock MakeDownloadTitle(string? text = null) => new()
    {
        Text = text,
        FontSize = 16,
        FontWeight = FontWeight.SemiBold,
        TextWrapping = TextWrapping.Wrap,
        MaxWidth = 460,
    };

    /// <summary>
    /// The progress bar of a download dialog, full width of the body.
    /// </summary>
    internal static ProgressBar MakeDownloadProgressBar(string valueProperty)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.MinWidth = 440;
        progressBar.Width = double.NaN;
        progressBar.Margin = new Thickness(0, 2, 0, 0);
        progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
        progressBar.Bind(RangeBase.ValueProperty, new Binding(valueProperty));
        return progressBar;
    }

    /// <summary>
    /// Dim status line under the progress bar of a download dialog.
    /// </summary>
    internal static TextBlock MakeDownloadStatus(string textProperty) => new()
    {
        FontSize = 12,
        Foreground = DimTextBrush,
        TextTrimming = TextTrimming.CharacterEllipsis,
        MaxWidth = 460,
        HorizontalAlignment = HorizontalAlignment.Left,
        [!TextBlock.TextProperty] = new Binding(textProperty),
    };

    /// <summary>
    /// The view models set Error on every failure path; shown in red, only when there is one.
    /// </summary>
    internal static TextBlock MakeDownloadError(string errorProperty)
    {
        var errorText = new TextBlock
        {
            Foreground = DownloadErrorBrush,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            MaxWidth = 460,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        errorText.Bind(TextBlock.TextProperty, new Binding(errorProperty));
        errorText.Bind(Visual.IsVisibleProperty, new Binding(errorProperty)
        {
            Converter = new FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)),
        });
        return errorText;
    }

    /// <summary>
    /// The body of a download dialog: the given rows (title, progress, status, error).
    /// </summary>
    internal static Border MakeDownloadBody(params Control[] rows)
    {
        var stack = new StackPanel { Spacing = 10 };
        foreach (var row in rows)
        {
            stack.Children.Add(row);
        }

        return new Border
        {
            Child = stack,
            Padding = new Thickness(20, 18),
        };
    }

    /// <summary>
    /// Binds <paramref name="control"/>'s IsEnabled to <paramref name="property"/> of <paramref name="source"/>.
    /// </summary>
    internal static T EnabledBy<T>(this T control, object source, string property) where T : Control
    {
        control.Bind(InputElement.IsEnabledProperty, new Binding(property) { Source = source });
        return control;
    }

    // ---- More shared pieces ----

    /// <summary>
    /// <see cref="AsPrimary{T}"/> with the button's own margin dropped.
    /// </summary>
    internal static T AsFlushPrimary<T>(T button) where T : Button => WithoutMargin(AsPrimary(button));

    /// <summary>
    /// Drops a control's own margin (UiUtil buttons have 4px side margins).
    /// </summary>
    internal static T WithoutMargin<T>(T control) where T : Control
    {
        control.Margin = new Thickness(0);
        return control;
    }

    /// <summary>
    /// The bottom bar with Cancel and the accent OK on the right, optional status on the left.
    /// </summary>
    internal static Border MakeCancelOkBar(Control? left, Button buttonCancel, Button buttonOk)
    {
        buttonOk.Classes.Add("primary");
        return MakeButtonBar(left, buttonCancel, buttonOk);
    }

    /// <summary>
    /// The docked bottom bar (see <see cref="MakeDockedButtonBar"/>) with Cancel and the accent OK.
    /// </summary>
    internal static Border MakeDockedCancelOkBar(Control? left, Button buttonCancel, Button buttonOk)
    {
        buttonOk.Classes.Add("primary");
        return MakeDockedButtonBar(left, buttonCancel, buttonOk);
    }

    /// <summary>
    /// The sections on the panel color above the docked button bar with the summary, Cancel and the
    /// accent OK.
    /// </summary>
    internal static Grid MakeCancelOkRoot(Control sections, Control? status, Button buttonCancel, Button buttonOk)
    {
        return MakeRoot(sections, MakeDockedCancelOkBar(status, buttonCancel, buttonOk));
    }

    /// <summary>
    /// The options (auto height) above one or more list sections that share the rest.
    /// </summary>
    internal static Grid MakeOptionsAndLists(Control? options, params Control[] lists)
    {
        var grid = new Grid();
        var row = 0;
        if (options != null)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            grid.Add(options, row++, 0);
        }

        foreach (var list in lists)
        {
            grid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)) { MinHeight = 120 });
            grid.Add(list, row++, 0);
        }

        return grid;
    }

    /// <summary>
    /// Sections stacked in a column.
    /// </summary>
    internal static StackPanel MakeSectionStack(params Control[] sections)
    {
        var panel = new StackPanel();
        foreach (var section in sections)
        {
            panel.Children.Add(section);
        }

        return panel;
    }

    /// <summary>
    /// "Label  [field]" rows with the labels in a column sized to the longest one (so translations
    /// do not get clipped).
    /// </summary>
    internal static Grid MakeFieldGrid(params (string Label, Control Field)[] rows)
    {
        var grid = new Grid
        {
            ColumnSpacing = 12,
            RowSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
        };
        for (var i = 0; i < rows.Length; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            grid.Add(MakeDimLabel(rows[i].Label.TrimEnd(':'), new Thickness(0)), i, 0);
            grid.Add(rows[i].Field, i, 1);
        }

        return grid;
    }

    /// <summary>
    /// A dim label, a field and the controls after it on one line.
    /// </summary>
    internal static StackPanel MakeLabeledField(string label, Control field, params Control[] after)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(MakeDimLabel(label, new Thickness(0)));
        panel.Children.Add(field);
        foreach (var control in after)
        {
            panel.Children.Add(control);
        }

        return panel;
    }

    /// <summary>
    /// A list header whose text comes from the view model (counts, "No changes needed", ...).
    /// </summary>
    internal static TextBlock MakeBoundSectionHeader(string binding)
    {
        return new TextBlock
        {
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(binding),
        };
    }

    /// <summary>
    /// A fill section (see <see cref="MakeFillSection(string, Control, Control?, bool, bool)"/>) with
    /// its own header control.
    /// </summary>
    internal static Border MakeFillSection(Control header, Control content, bool first = false, bool lineLeft = false)
    {
        var grid = new Grid
        {
            RowSpacing = 8,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(header, 0, 0);
        grid.Add(content, 1, 0);
        return new Border
        {
            Child = grid,
            BorderBrush = LineBrush,
            BorderThickness = lineLeft ? new Thickness(1, 0, 0, 0) : new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    /// <summary>
    /// A list pane without lines: the small caps header (optional actions on the right) above a
    /// framed list that takes the rest of the height, with <paramref name="margin"/> around it.
    /// </summary>
    internal static Grid MakeListPane(string title, Control list, Control? headerRight = null, Thickness? margin = null)
    {
        var header = MakeSectionHeaderRow(title, headerRight);
        header.Margin = new Thickness(0, 0, 0, 8);
        var grid = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
            Margin = margin ?? new Thickness(14, 12, 14, 14),
        };
        grid.Add(header, 0, 0);
        grid.Add(list is Border ? list : MakeListFrame(list), 1, 0);
        return grid;
    }

    /// <summary>
    /// The list section of the pickers: the small caps header (with optional controls on its
    /// right) over the list in a rounded field frame that fills the rest, optional controls below.
    /// </summary>
    internal static Border MakePickerListSection(string title, Control list, Control? headerRight = null, bool lineOnTop = false, Control? below = null)
    {
        var header = MakeSectionHeaderRow(title, headerRight, 28);
        header.Margin = new Thickness(0, 0, 0, 8);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        grid.Add(header, 0, 0);
        grid.Add(MakeListFrame(list, clearListBackground: true), 1, 0);
        if (below != null)
        {
            below.Margin = new Thickness(0, 10, 0, 0);
            grid.Add(below, 2, 0);
        }

        return new Border
        {
            Child = grid,
            BorderBrush = LineBrush,
            BorderThickness = new Thickness(0, lineOnTop ? 1 : 0, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    /// <summary>
    /// A list box with compact rounded rows (see <see cref="EditorWindowStyles.CompactListRows"/>)
    /// in a rounded frame on the field color.
    /// </summary>
    internal static Border MakeCompactListFrame(ListBox listBox, bool compact = true)
    {
        if (compact)
        {
            listBox.Classes.Add("compact");
        }

        return MakeListFrame(listBox, clearListBackground: true);
    }

    /// <summary>
    /// A small rounded count badge, e.g. "1,234", bound to the given path.
    /// </summary>
    internal static Border MakeCountBadge(Binding textBinding, IBrush foreground, IBrush background)
    {
        return new Border
        {
            Background = background,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(8, 2),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                [!TextBlock.TextProperty] = textBinding,
                FontSize = 11,
                FontWeight = FontWeight.SemiBold,
                Foreground = foreground,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
    }

    /// <summary>
    /// A header line: <paramref name="title"/> filling, optional controls docked on the right.
    /// </summary>
    internal static DockPanel MakeHeaderRow(Control title, Control? right = null)
    {
        var header = new DockPanel { LastChildFill = true };
        if (right != null)
        {
            DockPanel.SetDock(right, Dock.Right);
            header.Children.Add(right);
        }

        header.Children.Add(title);
        return header;
    }

    /// <summary>
    /// A single-line text box on the field colors (class "text"), vertically centered, no margin.
    /// </summary>
    internal static T AsCenteredTextField<T>(T textBox) where T : TextBox
    {
        textBox.Classes.Add(TextClass);
        textBox.VerticalAlignment = VerticalAlignment.Center;
        textBox.Margin = new Thickness(0);
        return textBox;
    }

    /// <summary>
    /// A dim text for the left side of the button bar or a section header (set or bind its text).
    /// </summary>
    internal static TextBlock MakeDimText()
    {
        return new TextBlock
        {
            Foreground = DimTextBrush,
            FontSize = 12.5,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
    }

    /// <summary>
    /// A fixed dim status text for the left side of the button bar.
    /// </summary>
    internal static TextBlock MakeDimStatusText(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 0, 12, 0),
        };
    }
}
