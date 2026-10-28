using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Ytt;

/// <summary>
/// YouTube timed text styles, laid out like the WebVTT styles window: the pen / position / window
/// style lists on the left in tabs, the preview on the right with the editor for the selected
/// entry under it.
/// </summary>
public class YttStylesWindow : Window
{
    private const string Pen = nameof(YttStylesViewModel.SelectedPen) + ".";
    private const string Position = nameof(YttStylesViewModel.SelectedPosition) + ".";
    private const string WindowStyle = nameof(YttStylesViewModel.SelectedWindowStyle) + ".";

    public YttStylesWindow(YttStylesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)) { Source = vm });
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
                new ColumnDefinition(new GridLength(400)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(MakeListPanel(vm), 0, 0);
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

        KeyDown += vm.KeyDown;
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

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

    private static Border MakeListPanel(YttStylesViewModel vm)
    {
        var tabs = new TabControl
        {
            Margin = new Thickness(8, 0, 8, 8),
            Items =
            {
                new TabItem { Header = Se.Language.File.Ytt.Pens, Content = MakeList(vm, vm.Pens, nameof(vm.SelectedPen), Se.Language.File.Ytt.Pens) },
                new TabItem { Header = Se.Language.File.Ytt.WindowPositions, Content = MakeList(vm, vm.Positions, nameof(vm.SelectedPosition), Se.Language.File.Ytt.WindowPositions) },
                new TabItem { Header = Se.Language.File.Ytt.WindowStyles, Content = MakeList(vm, vm.WindowStyles, nameof(vm.SelectedWindowStyle), Se.Language.File.Ytt.WindowStyles) },
            },
            [!SelectingItemsControl.SelectedIndexProperty] = new Binding(nameof(vm.SelectedTabIndex)) { Mode = BindingMode.TwoWay },
        };

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, Margin = new Thickness(0, 6, 0, 0) };
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.New, vm.NewCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-regular fa-clone", Se.Language.General.Duplicate, vm.DuplicateCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Delete, vm.DeleteCommand, 24));
        var header = new StackPanel { Margin = new Thickness(14, 10, 10, 8) };
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Styles));
        header.Children.Add(actions);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        grid.Add(header, 0, 0);
        grid.Add(tabs, 1, 0);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };
    }

    private static Control MakeList(YttStylesViewModel vm, IEnumerable items, string selectedProperty, string name)
    {
        var table = TableViewExtras.MakeTableView();
        AutomationProperties.SetName(table, name);
        table.DataContext = vm;
        table.ItemsSource = items;
        table.Columns.Add(Column(Se.Language.General.Id, nameof(YttPenDisplay.IdText), new GridLength(50)));
        table.Columns.Add(Column(Se.Language.General.Description, nameof(YttPenDisplay.Description), new GridLength(1, GridUnitType.Star)));
        table.Columns.Add(Column(Se.Language.General.Usages, nameof(YttPenDisplay.UsageCount), new GridLength(70)));
        table.Bind(TableView.SelectedItemProperty, new Binding(selectedProperty) { Source = vm, Mode = BindingMode.TwoWay });
        TableViewExtras.AttachListNavigation(table);
        return table;
    }

    private static SeTableViewColumn Column(string header, string property, GridLength width) => new()
    {
        Header = header,
        CellTheme = UiUtil.TableViewCellTheme,
        HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        Binding = new Binding(property),
        Width = width,
    };

    private static Control MakePreview(YttStylesViewModel vm)
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

    private static Control MakeEditorPanel(YttStylesViewModel vm)
    {
        var panel = new Panel();
        panel.Children.Add(MakePenEditor(vm));
        panel.Children.Add(MakePositionEditor(vm));
        panel.Children.Add(MakeWindowStyleEditor(vm));
        return new Border
        {
            Child = panel,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        };
    }

    private static MultiBinding TabVisible(int tabIndex, string selectedProperty) => new MultiBinding
    {
        Converter = BoolConverters.And,
        Bindings =
        {
            new Binding(nameof(YttStylesViewModel.SelectedTabIndex)) { Converter = new FuncValueConverter<int, bool>(i => i == tabIndex) },
            new Binding(selectedProperty) { Converter = ObjectConverters.IsNotNull },
        },
    };

    private static Control MakePenEditor(YttStylesViewModel vm)
    {
        var fontStyle = MakeIndexCombo(YttStylesViewModel.FontStyleNames, Pen + nameof(YttPenDisplay.FontStyleIndex), Se.Language.File.Ytt.FontStyle);
        var size = EditorChrome.MakeField(Pen + nameof(YttPenDisplay.Size), 0, 1000, 90, "sz");
        AutomationProperties.SetName(size, Se.Language.File.Ytt.Size);
        var realSize = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0));
        realSize.Bind(TextBlock.TextProperty, new Binding(Pen + nameof(YttPenDisplay.RealSizeText)));

        var styleRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        styleRow.Children.Add(Toggle("B", Pen + nameof(YttPenDisplay.Bold), Se.Language.General.Bold, FontWeight.Bold));
        styleRow.Children.Add(Toggle("I", Pen + nameof(YttPenDisplay.Italic), Se.Language.General.Italic, style: FontStyle.Italic));
        styleRow.Children.Add(Toggle("U", Pen + nameof(YttPenDisplay.Underline), Se.Language.General.Underline, decorations: TextDecorations.Underline));
        var offset = MakeIndexCombo(YttStylesViewModel.OffsetNames, Pen + nameof(YttPenDisplay.OffsetIndex), Se.Language.File.Ytt.Offset);

        var font = new StackPanel { Spacing = 8 };
        font.Children.Add(EditorChrome.MakePropertyRow(Se.Language.File.Ytt.FontStyle, fontStyle));
        font.Children.Add(EditorChrome.MakePropertyRow(Se.Language.File.Ytt.Size, size));
        font.Children.Add(realSize);
        font.Children.Add(styleRow);
        font.Children.Add(EditorChrome.MakePropertyRow(Se.Language.File.Ytt.Offset, offset));

        var colors = new StackPanel { Spacing = 6 };
        colors.Children.Add(MakeColorRow(vm, Se.Language.File.Ytt.TextColor, Pen + nameof(YttPenDisplay.UseForeColor), Pen + nameof(YttPenDisplay.ForeColor), Pen + nameof(YttPenDisplay.ForeOpacity)));
        colors.Children.Add(MakeColorRow(vm, Se.Language.File.Ytt.BackgroundColor, Pen + nameof(YttPenDisplay.UseBackColor), Pen + nameof(YttPenDisplay.BackColor), Pen + nameof(YttPenDisplay.BackOpacity)));

        var edgeType = MakeIndexCombo(YttStylesViewModel.EdgeTypeNames, Pen + nameof(YttPenDisplay.EdgeTypeIndex), Se.Language.File.Ytt.Edge);
        var edge = new StackPanel { Spacing = 6 };
        edge.Children.Add(EditorChrome.MakePropertyRow(Se.Language.File.Ytt.Edge, edgeType));
        var edgeColor = MakeColorRow(vm, Se.Language.General.Color, Pen + nameof(YttPenDisplay.UseEdgeColor), Pen + nameof(YttPenDisplay.EdgeColor), null);
        edgeColor.Bind(IsEnabledProperty, new Binding(Pen + nameof(YttPenDisplay.EdgeTypeIndex)) { Converter = new FuncValueConverter<int, bool>(i => i > 0) });
        edge.Children.Add(edgeColor);

        return Columns(
            new Control[]
            {
                EditorChrome.MakeEditorSection(Se.Language.General.Font, font, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.Color, colors, first: true),
                EditorChrome.MakeEditorSection(Se.Language.File.Ytt.Edge, edge, first: true),
            },
            TabVisible(YttStylesViewModel.TabPens, nameof(vm.SelectedPen)));
    }

    private static Control MakePositionEditor(YttStylesViewModel vm)
    {
        var anchor = MakeIndexCombo(YttStylesViewModel.AnchorPointNames, Position + nameof(YttPositionDisplay.AnchorPointIndex), Se.Language.File.Ytt.AnchorPoint);
        var horizontal = EditorChrome.MakeField(Position + nameof(YttPositionDisplay.Horizontal), 0, 100, 90, "ah");
        AutomationProperties.SetName(horizontal, Se.Language.File.Ytt.Horizontal);
        var vertical = EditorChrome.MakeField(Position + nameof(YttPositionDisplay.Vertical), 0, 100, 90, "av");
        AutomationProperties.SetName(vertical, Se.Language.File.Ytt.Vertical);
        var real = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0));
        real.Bind(TextBlock.TextProperty, new Binding(Position + nameof(YttPositionDisplay.RealPositionText)));

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(EditorChrome.MakePropertyRow(Se.Language.File.Ytt.AnchorPoint, anchor));
        rows.Children.Add(EditorChrome.MakePropertyRow(Se.Language.File.Ytt.Horizontal, horizontal));
        rows.Children.Add(EditorChrome.MakePropertyRow(Se.Language.File.Ytt.Vertical, vertical));
        rows.Children.Add(real);

        return Columns(
            new Control[] { EditorChrome.MakeEditorSection(Se.Language.File.Ytt.WindowPosition, rows, first: true) },
            TabVisible(YttStylesViewModel.TabPositions, nameof(vm.SelectedPosition)));
    }

    private static Control MakeWindowStyleEditor(YttStylesViewModel vm)
    {
        var justify = MakeIndexCombo(YttStylesViewModel.JustifyNames, WindowStyle + nameof(YttWindowStyleDisplay.JustifyIndex), Se.Language.File.Ytt.Justify);
        var direction = MakeIndexCombo(YttStylesViewModel.PrintDirectionNames, WindowStyle + nameof(YttWindowStyleDisplay.PrintDirectionIndex), Se.Language.File.Ytt.TextDirection);
        var reverse = new CheckBox
        {
            Content = Se.Language.File.Ytt.ColumnsRightToLeft,
            [!ToggleButton.IsCheckedProperty] = new Binding(WindowStyle + nameof(YttWindowStyleDisplay.ReverseColumns)) { Mode = BindingMode.TwoWay },
            [!IsEnabledProperty] = new Binding(WindowStyle + nameof(YttWindowStyleDisplay.PrintDirectionIndex)) { Converter = new FuncValueConverter<int, bool>(i => i >= 3) },
        };

        var useOpacity = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(WindowStyle + nameof(YttWindowStyleDisplay.UseWindowOpacity)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(useOpacity, Se.Language.File.Ytt.WindowOpacity);
        var opacity = EditorChrome.MakeField(WindowStyle + nameof(YttWindowStyleDisplay.WindowOpacity), 0, 255, 90);
        opacity.Bind(IsEnabledProperty, new Binding(WindowStyle + nameof(YttWindowStyleDisplay.UseWindowOpacity)));
        AutomationProperties.SetName(opacity, Se.Language.File.Ytt.WindowOpacity);

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(EditorChrome.MakePropertyRow(Se.Language.File.Ytt.Justify, justify));
        rows.Children.Add(EditorChrome.MakePropertyRow(Se.Language.File.Ytt.TextDirection, direction));
        rows.Children.Add(reverse);
        rows.Children.Add(EditorChrome.MakePropertyRow(Se.Language.File.Ytt.WindowOpacity, useOpacity, opacity));

        return Columns(
            new Control[] { EditorChrome.MakeEditorSection(Se.Language.File.Ytt.WindowStyle, rows, first: true) },
            TabVisible(YttStylesViewModel.TabWindowStyles, nameof(vm.SelectedWindowStyle)));
    }

    private static Grid Columns(Control[] sections, MultiBinding isVisible)
    {
        var grid = new Grid { [!IsVisibleProperty] = isVisible };
        for (var i = 0; i < 3; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        }

        for (var i = 0; i < sections.Length; i++)
        {
            var border = new Border
            {
                Child = sections[i],
                BorderBrush = EditorChrome.LineBrush,
                BorderThickness = new Thickness(i > 0 ? 1 : 0, 0, 0, 0),
            };
            grid.Add(border, 0, i);
        }

        return grid;
    }

    private static ComboBox MakeIndexCombo(string[] items, string property, string name)
    {
        var combo = new ComboBox
        {
            ItemsSource = items,
            MinWidth = 160,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!SelectingItemsControl.SelectedIndexProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(combo, name);
        return combo;
    }

    private static ToggleButton Toggle(string text, string property, string name, FontWeight weight = FontWeight.Normal, FontStyle style = FontStyle.Normal, TextDecorationCollection? decorations = null)
    {
        var toggle = new ToggleButton
        {
            Content = new TextBlock { Text = text, FontWeight = weight, FontStyle = style, TextDecorations = decorations, FontSize = EditorChrome.FieldFontSize },
            Width = 28,
            Height = EditorChrome.ScaledSize(28),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        toggle.Classes.Add("tool");
        AutomationProperties.SetName(toggle, name);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(toggle, name);
        }

        return toggle;
    }

    /// <summary>On/off switch, label, color swatch and (optionally) an opacity field 0-255.</summary>
    private static Grid MakeColorRow(YttStylesViewModel vm, string text, string useProperty, string colorProperty, string? opacityProperty)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(useProperty) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, text);

        var swatch = MakeColorPickerButton(vm, colorProperty);
        AutomationProperties.SetName(swatch, text);
        swatch.Bind(IsEnabledProperty, new Binding(useProperty));

        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        row.Add(toggle, 0, 0);
        row.Add(new TextBlock { Text = text, FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center }, 0, 1);
        row.Add(swatch, 0, 2);
        if (opacityProperty != null)
        {
            var opacity = EditorChrome.MakeField(opacityProperty, 0, 255, 90, "α");
            AutomationProperties.SetName(opacity, text + " " + Se.Language.File.Ytt.Opacity);
            opacity.Bind(IsEnabledProperty, new Binding(useProperty));
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(opacity, Se.Language.File.Ytt.Opacity + " (0-255)");
            }

            row.Add(opacity, 0, 3);
        }

        return row;
    }

    private static Button MakeColorPickerButton(YttStylesViewModel vm, string colorPath)
    {
        var swatch = new Border
        {
            Width = 34,
            Height = 22,
            CornerRadius = new CornerRadius(5),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.MenuBorderBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!Border.BackgroundProperty] = new Binding(colorPath) { Converter = new ColorToBrushConverter() },
        };

        var button = new Button
        {
            Content = swatch,
            Padding = new Thickness(3),
            VerticalAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");

        var propertyName = colorPath.Substring(colorPath.IndexOf('.') + 1);
        button.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(button) is not Window window)
            {
                return;
            }

            object? target = colorPath.StartsWith(Pen) ? vm.SelectedPen : null;
            if (target == null)
            {
                return;
            }

            var propInfo = target.GetType().GetProperty(propertyName);
            var currentColor = propInfo?.GetValue(target) is Color c ? c : Colors.White;

            var pickerVm = new ColorPickerViewModel();
            pickerVm.Initialize(currentColor);
            var pickerWindow = new ColorPickerWindow(pickerVm);
            await WindowService.ShowModalAsync(window, pickerWindow);

            if (pickerVm.OkPressed)
            {
                // the opacity has its own field
                var picked = pickerVm.SelectedColor;
                propInfo?.SetValue(target, Color.FromRgb(picked.R, picked.G, picked.B));
            }
        };

        return button;
    }

    private static Border MakeButtonBar(YttStylesViewModel vm)
    {
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        buttons.Children.Add(buttonCancel);
        buttons.Children.Add(buttonOk);

        var bar = new DockPanel();
        DockPanel.SetDock(buttons, Dock.Right);
        bar.Children.Add(buttons);
        return EditorChrome.WrapButtonBar(bar);
    }
}
