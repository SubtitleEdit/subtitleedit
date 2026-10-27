using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Features.WebVtt;

/// <summary>
/// WebVTT styles (::cue classes), styled like ASSA draw: the style list on the left, the preview
/// on the right with the style editor and the CSS before/after under it.
/// </summary>
public class WebVttStylesWindow : Window
{
    private const string Current = nameof(WebVttStylesViewModel.SelectedStyle) + ".";
    private static readonly FontFamily MonoFont = new("Menlo, Consolas, Cascadia Mono, DejaVu Sans Mono, monospace");

    public WebVttStylesWindow(WebVttStylesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        CanResize = true;
        Width = 1200;
        Height = 800;
        MinWidth = 950;
        MinHeight = 600;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        AddStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        var right = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        right.Add(MakePreview(vm), 0, 0);
        right.Add(MakeEditorPanel(vm), 1, 0);

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(380)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(MakeStylesPanel(vm), 0, 0);
        content.Add(right, 0, 1);

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        mainGrid.Add(content, 0, 0);
        mainGrid.Add(MakeButtonBar(vm), 1, 0);
        Content = mainGrid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(vm.StyleGrid); });
        KeyDown += vm.KeyDown;

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    /// <summary>
    /// Toggle buttons (B/I/U/S) in the editor accent.
    /// </summary>
    private static void AddStyles(Styles styles)
    {
        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool"))
        {
            Setters =
            {
                new Setter(ToggleButton.BackgroundProperty, Brushes.Transparent),
                new Setter(ToggleButton.BorderThicknessProperty, new Thickness(0)),
                new Setter(ToggleButton.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(ToggleButton.ForegroundProperty, EditorChrome.IconBrush),
                new Setter(ToggleButton.PaddingProperty, new Thickness(0)),
            },
        });
        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool").Class(":checked").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(ContentPresenter.BackgroundProperty, EditorChrome.AccentBrush),
                new Setter(ContentPresenter.ForegroundProperty, Brushes.White),
            },
        });
    }

    private static Border MakeStylesPanel(WebVttStylesViewModel vm)
    {
        // No header sorting: the styles are written to the file header in list order on
        // OK/Apply, so the collection order is not presentation-only.
        var dataGrid = TableViewExtras.MakeTableView();
        AutomationProperties.SetName(dataGrid, Se.Language.General.Styles);
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Styles;
        dataGrid.Columns.Add(Column(Se.Language.General.Name, nameof(WebVttStyleDisplay.Name), new GridLength(1, GridUnitType.Star)));
        dataGrid.Columns.Add(Column(Se.Language.General.FontSize, nameof(WebVttStyleDisplay.FontSizeDisplay), new GridLength(70)));
        dataGrid.Columns.Add(Column(Se.Language.General.Usages, nameof(WebVttStyleDisplay.UsageCount), new GridLength(70)));

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedStyle)) { Source = vm });
        dataGrid.AddHandler(InputElement.KeyDownEvent, vm.StylesMoveKeyDown, RoutingStrategies.Tunnel);
        TableViewExtras.AttachListNavigation(dataGrid);
        vm.StyleGrid = dataGrid;

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem { Header = Se.Language.General.Delete, DataContext = vm, Command = vm.RemoveCommand });
        flyout.Items.Add(new MenuItem { Header = Se.Language.General.Clear, DataContext = vm, Command = vm.RemoveAllCommand });
        AddMoveMenuItems(flyout, vm);
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        // Title, then the actions under it
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, Margin = new Thickness(0, 6, 0, 0) };
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.New, vm.NewCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-regular fa-clone", Se.Language.General.Duplicate, vm.DuplicateCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Delete, vm.RemoveCommand, 24));
        actions.Children.Add(EditorChrome.MakeToolbarSeparator());
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-file-import", Se.Language.General.Import, vm.ImportCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-file-export", Se.Language.General.Export, vm.ExportCommand, 24));
        var header = new StackPanel { Margin = new Thickness(14, 10, 10, 8) };
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Styles));
        header.Children.Add(actions);

        // Two styles with one name would overwrite each other on save
        var duplicates = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 255, 69, 0)),
            BorderBrush = Brushes.OrangeRed,
            BorderThickness = new Thickness(3, 0, 0, 0),
            Padding = new Thickness(10, 6),
            Margin = new Thickness(8, 0, 8, 8),
            Child = new TextBlock
            {
                FontSize = EditorChrome.FieldFontSize,
                TextWrapping = TextWrapping.Wrap,
                [!TextBlock.TextProperty] = new Binding(nameof(vm.DuplicateStyleNames)),
            },
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.HasDuplicateStyleNames)),
        };

        dataGrid.Margin = new Thickness(8, 0, 8, 8);
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
        grid.Add(dataGrid, 1, 0);
        grid.Add(duplicates, 2, 0);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };
    }

    private static SeTableViewColumn Column(string header, string property, GridLength width) => new()
    {
        Header = header,
        CellTheme = UiUtil.TableViewCellTheme,
        HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        Binding = new Binding(property),
        Width = width,
    };

    private static Border MakeEditorPanel(WebVttStylesViewModel vm)
    {
        var columns = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };

        var first = new StackPanel();
        first.Children.Add(MakeNameSection(vm));
        first.Children.Add(MakeFontSection(vm));
        columns.Add(first, 0, 0);
        columns.Add(new Border
        {
            Child = MakeColorsSection(vm),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0),
        }, 0, 1);
        columns.Add(MakeCssSection(vm), 0, 2);

        return new Border
        {
            Child = columns,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            [!Border.IsEnabledProperty] = new Binding(nameof(vm.SelectedStyle)) { Converter = ObjectConverters.IsNotNull },
        };
    }

    private static Border MakeNameSection(WebVttStylesViewModel vm)
    {
        var name = new TextBox
        {
            FontSize = EditorChrome.FieldFontSize,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(6),
            [!TextBox.TextProperty] = new Binding(Current + nameof(WebVttStyleDisplay.Name)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(name, Se.Language.General.Name);

        // A blank or already-used name would silently overwrite another style on save.
        name.Bind(TemplatedControl.BorderBrushProperty, new Binding(nameof(vm.IsNameInvalid))
        {
            Source = vm,
            Converter = new FuncValueConverter<bool, IBrush?>(invalid => invalid ? Brushes.OrangeRed : EditorChrome.FieldBorderBrush),
        });

        return EditorChrome.MakeEditorSection(Se.Language.General.Name, name, first: true);
    }

    private static Border MakeFontSection(WebVttStylesViewModel vm)
    {
        var fontName = UiUtil.MakeComboBox(vm.Fonts, vm, Current + nameof(WebVttStyleDisplay.FontName));
        fontName.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(fontName, Se.Language.General.FontName);

        var size = EditorChrome.MakeField(Current + nameof(WebVttStyleDisplay.FontSize), 0, 500, 76, "Aa", "0.#");
        AutomationProperties.SetName(size, Se.Language.General.FontSize);

        ToggleButton Toggle(string text, string property, string name, FontWeight weight = FontWeight.Normal, FontStyle style = FontStyle.Normal, TextDecorationCollection? decorations = null)
        {
            var toggle = new ToggleButton
            {
                Content = new TextBlock { Text = text, FontWeight = weight, FontStyle = style, TextDecorations = decorations, FontSize = EditorChrome.FieldFontSize },
                Width = 28,
                Height = EditorChrome.ScaledSize(28),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                [!ToggleButton.IsCheckedProperty] = new Binding(Current + property) { Mode = BindingMode.TwoWay },
            };
            toggle.Classes.Add("tool");
            AutomationProperties.SetName(toggle, name);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(toggle, name);
            }

            return toggle;
        }

        var styleRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        styleRow.Children.Add(size);
        styleRow.Children.Add(new Border { Width = 6 });
        styleRow.Children.Add(Toggle("B", nameof(WebVttStyleDisplay.Bold), Se.Language.General.Bold, FontWeight.Bold));
        styleRow.Children.Add(Toggle("I", nameof(WebVttStyleDisplay.Italic), Se.Language.General.Italic, style: FontStyle.Italic));
        styleRow.Children.Add(Toggle("U", nameof(WebVttStyleDisplay.Underline), Se.Language.General.Underline, decorations: TextDecorations.Underline));
        styleRow.Children.Add(Toggle("S", nameof(WebVttStyleDisplay.Strikeout), Se.Language.General.Strikeout, decorations: TextDecorations.Strikethrough));

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(fontName);
        content.Children.Add(styleRow);
        return EditorChrome.MakeEditorSection(Se.Language.General.Font, content);
    }

    /// <summary>
    /// Every CSS property is optional, so each color has its own on/off switch - a color that is
    /// off is left out of the style instead of being written as a default.
    /// </summary>
    private static Border MakeColorsSection(WebVttStylesViewModel vm)
    {
        var rows = new StackPanel { Spacing = 6 };
        rows.Children.Add(MakeColorRow(vm, Se.Language.General.Color, nameof(WebVttStyleDisplay.UseColor), nameof(WebVttStyleDisplay.Color)));
        rows.Children.Add(MakeColorRow(vm, Se.Language.General.Background, nameof(WebVttStyleDisplay.UseBackgroundColor), nameof(WebVttStyleDisplay.BackgroundColor)));
        rows.Children.Add(MakeColorRow(vm, Se.Language.General.Shadow, nameof(WebVttStyleDisplay.UseShadow), nameof(WebVttStyleDisplay.ShadowColor)));

        var shadowWidth = EditorChrome.MakeField(Current + nameof(WebVttStyleDisplay.ShadowWidth), 0, 100, double.NaN, "W", "0.#");
        AutomationProperties.SetName(shadowWidth, Se.Language.General.ShadowWidth);
        shadowWidth.Bind(IsEnabledProperty, new Binding(Current + nameof(WebVttStyleDisplay.UseShadow)));
        rows.Children.Add(EditorChrome.MakePropertyRow(Se.Language.General.Width, shadowWidth));

        return EditorChrome.MakeEditorSection(Se.Language.General.Color, rows, first: true);
    }

    private static Grid MakeColorRow(WebVttStylesViewModel vm, string text, string useProperty, string colorProperty)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(Current + useProperty) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, text);

        var swatch = MakeStyleColorPickerButton(vm, colorProperty);
        AutomationProperties.SetName(swatch, text);
        swatch.Bind(IsEnabledProperty, new Binding(Current + useProperty));

        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        row.Add(toggle, 0, 0);
        row.Add(new TextBlock { Text = text, FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center }, 0, 1);
        row.Add(swatch, 0, 2);
        return row;
    }

    /// <summary>
    /// The CSS the style was loaded from next to the CSS it will be saved as, so an edit to a
    /// hand-written style shows exactly what it changes.
    /// </summary>
    private static Border MakeCssSection(WebVttStylesViewModel vm)
    {
        Border Code(string property) => new()
        {
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 6),
            MinHeight = 64,
            Child = new TextBlock
            {
                FontFamily = MonoFont,
                FontSize = EditorChrome.ScaledFont(11.5),
                TextWrapping = TextWrapping.Wrap,
                [!TextBlock.TextProperty] = new Binding(property),
            },
        };

        var content = new StackPanel { Spacing = 4 };
        content.Children.Add(EditorChrome.MakeDimLabel(Se.Language.General.Before, new Thickness(0)));
        content.Children.Add(Code(nameof(vm.CssBefore)));
        content.Children.Add(EditorChrome.MakeDimLabel(Se.Language.General.After, new Thickness(0, 6, 0, 0)));
        content.Children.Add(Code(nameof(vm.CssAfter)));
        return EditorChrome.MakeEditorSection("CSS", content, first: true);
    }

    private static Control MakePreview(WebVttStylesViewModel vm)
    {
        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AutomationProperties.SetName(image, Se.Language.General.Preview);

        return new Border { Child = image, Margin = new Thickness(16, 12) };
    }

    private static Border MakeButtonBar(WebVttStylesViewModel vm)
    {
        var summary = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        void UpdateSummary() => summary.Text = $"{vm.Styles.Count} {Se.Language.General.Styles.ToLowerInvariant()}";
        UpdateSummary();
        vm.Styles.CollectionChanged += (_, _) => UpdateSummary();

        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand)
            .WithBindIsVisible(nameof(vm.IsApplyVisible));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        buttons.Children.Add(buttonApply);
        buttons.Children.Add(buttonCancel);
        buttons.Children.Add(buttonOk);

        var bar = new DockPanel();
        DockPanel.SetDock(buttons, Dock.Right);
        bar.Children.Add(buttons);
        bar.Children.Add(summary);
        return EditorChrome.WrapButtonBar(bar);
    }

    private static Button MakeStyleColorPickerButton(WebVttStylesViewModel vm, string colorPropertyName)
    {
        var swatch = new Border
        {
            Width = 34,
            Height = 22,
            CornerRadius = new CornerRadius(5),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.MenuBorderBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        swatch.Bind(Border.BackgroundProperty, new Binding(Current + colorPropertyName)
        {
            Source = vm,
            Converter = new ColorToBrushConverter(),
        });

        var button = new Button
        {
            Content = swatch,
            Padding = new Thickness(3),
            VerticalAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");

        button.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(button) is not Window window || vm.SelectedStyle is null)
            {
                return;
            }

            var propInfo = typeof(WebVttStyleDisplay).GetProperty(colorPropertyName);
            var currentColor = propInfo?.GetValue(vm.SelectedStyle) is Color c ? c : Colors.White;

            var pickerVm = new ColorPickerViewModel();
            pickerVm.Initialize(currentColor);
            var pickerWindow = new ColorPickerWindow(pickerVm);
            await WindowService.ShowModalAsync(window, pickerWindow);

            if (pickerVm.OkPressed)
            {
                propInfo?.SetValue(vm.SelectedStyle, pickerVm.SelectedColor);
            }
        };

        return button;
    }

    /// <summary>
    /// The "move up/down/to top/to bottom" block of the styles context menu. The styles are
    /// written to the file header in list order, so this is real reordering, not a view sort.
    /// </summary>
    private static void AddMoveMenuItems(MenuFlyout flyout, WebVttStylesViewModel vm)
    {
        flyout.Items.Add(new Separator());

        var items = new (string Header, ICommand Command, KeyGesture? Gesture)[]
        {
            (Se.Language.General.MoveUp, vm.MoveUpCommand, new KeyGesture(Key.Up, KeyModifiers.Control)),
            (Se.Language.General.MoveDown, vm.MoveDownCommand, new KeyGesture(Key.Down, KeyModifiers.Control)),
            (Se.Language.General.MoveToTop, vm.MoveToTopCommand, null),
            (Se.Language.General.MoveToBottom, vm.MoveToBottomCommand, null),
        };

        foreach (var (header, command, gesture) in items)
        {
            flyout.Items.Add(new MenuItem
            {
                Header = header,
                DataContext = vm,
                Command = command,
                InputGesture = gesture,
            });
        }
    }
}
