using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Features.WebVtt;

public class WebVttStylesWindow : Window
{
    public WebVttStylesWindow(WebVttStylesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        CanResize = true;
        Width = 1150;
        Height = 750;
        MinWidth = 900;
        MinHeight = 560;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand)
            .WithBindIsVisible(nameof(vm.IsApplyVisible));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonApply, buttonOk, buttonCancel);

        grid.Add(MakeStylesView(vm), 0, 0);
        grid.Add(MakeRightView(vm), 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        Activated += delegate { TableViewExtras.FocusRow(vm.StyleGrid); };
        KeyDown += vm.KeyDown;

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Border MakeStylesView(WebVttStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // styles
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // duplicate names warning
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Styles).WithBold();

        // No header sorting: the styles are written to the file header in list order on
        // OK/Apply, so the collection order is not presentation-only.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Styles;

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(WebVttStyleDisplay.Name)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.FontName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(WebVttStyleDisplay.FontNameDisplay)),
            Width = new GridLength(150),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.FontSize,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(WebVttStyleDisplay.FontSizeDisplay)),
            Width = new GridLength(80),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Italic,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(WebVttStyleDisplay.ItalicDisplay)),
            Width = new GridLength(70),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Usages,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(WebVttStyleDisplay.UsageCount)),
            Width = new GridLength(80),
        });

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedStyle)) { Source = vm });
        dataGrid.AddHandler(InputElement.KeyDownEvent, vm.StylesMoveKeyDown, RoutingStrategies.Tunnel);
        TableViewExtras.AttachListNavigation(dataGrid);
        vm.StyleGrid = dataGrid;

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.RemoveCommand,
        });
        flyout.Items.Add(new MenuItem
        {
            Header = Se.Language.General.Clear,
            DataContext = vm,
            Command = vm.RemoveAllCommand,
        });
        AddMoveMenuItems(flyout, vm);
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        var labelDuplicates = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.DuplicateStyleNames));
        labelDuplicates.Foreground = Brushes.OrangeRed;
        labelDuplicates.Bind(IsVisibleProperty, new Binding(nameof(vm.HasDuplicateStyleNames)) { Source = vm });

        var buttonNew = UiUtil.MakeButton(vm.NewCommand, IconNames.Plus, Se.Language.General.New);
        var buttonDuplicate = UiUtil.MakeButton(vm.DuplicateCommand, IconNames.Duplicate, Se.Language.General.Duplicate);
        var buttonRemove = UiUtil.MakeButton(vm.RemoveCommand, IconNames.Trash, Se.Language.General.Delete);
        var buttonImport = UiUtil.MakeButton(vm.ImportCommand, IconNames.Import, Se.Language.General.Import);
        var buttonExport = UiUtil.MakeButton(vm.ExportCommand, IconNames.Export, Se.Language.General.Export);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonNew,
            buttonDuplicate,
            buttonRemove,
            buttonImport,
            buttonExport
        ).WithAlignmentLeft();

        grid.Add(label, 0, 0);
        grid.Add(dataGrid, 1, 0);
        grid.Add(labelDuplicates, 2, 0);
        grid.Add(panelButtons, 3, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Grid MakeRightView(WebVttStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowSpacing = 5,
        };

        grid.Add(MakeSelectedStyleView(vm), 0);
        grid.Add(MakePreviewView(vm), 1);
        grid.Add(MakeRawStyleView(vm), 2);

        return grid;
    }

    private static Border MakeSelectedStyleView(WebVttStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Style).WithBold();

        var labelName = UiUtil.MakeLabel(Se.Language.General.Name).WithMinWidth(70);
        var textBoxName = UiUtil.MakeTextBox(220, vm, nameof(vm.SelectedStyle) + "." + nameof(WebVttStyleDisplay.Name));
        // A blank or already-used name would silently overwrite another style on save.
        textBoxName.Bind(TemplatedControl.BorderBrushProperty, new Binding(nameof(vm.IsNameInvalid))
        {
            Source = vm,
            Converter = new FuncValueConverter<bool, IBrush?>(invalid => invalid ? Brushes.OrangeRed : null),
        });
        var panelName = UiUtil.MakeHorizontalPanel(labelName, textBoxName);

        var labelFontName = UiUtil.MakeLabel(Se.Language.General.FontName).WithMinWidth(70);
        var comboBoxFontName = UiUtil.MakeComboBox(vm.Fonts, vm, nameof(vm.SelectedStyle) + "." + nameof(WebVttStyleDisplay.FontName)).WithMinWidth(160);
        var labelFontSize = UiUtil.MakeLabel(Se.Language.General.FontSize);
        var numericUpDownFontSize = UiUtil.MakeNumericUpDownOneDecimal(0, 500, 110, vm, nameof(vm.SelectedStyle) + "." + nameof(WebVttStyleDisplay.FontSize));
        numericUpDownFontSize.Increment = 1;
        var panelFont = UiUtil.MakeHorizontalPanel(labelFontName, comboBoxFontName, labelFontSize, numericUpDownFontSize);

        var checkBoxBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.SelectedStyle) + "." + nameof(WebVttStyleDisplay.Bold));
        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.SelectedStyle) + "." + nameof(WebVttStyleDisplay.Italic));
        var checkBoxUnderline = UiUtil.MakeCheckBox(Se.Language.General.Underline, vm, nameof(vm.SelectedStyle) + "." + nameof(WebVttStyleDisplay.Underline));
        var checkBoxStrikeout = UiUtil.MakeCheckBox(Se.Language.General.Strikeout, vm, nameof(vm.SelectedStyle) + "." + nameof(WebVttStyleDisplay.Strikeout));
        var panelFontStyle = UiUtil.MakeHorizontalPanel(checkBoxBold, checkBoxItalic, checkBoxUnderline, checkBoxStrikeout);

        // Every CSS property is optional, so each color has its own "use it" check box - an
        // unchecked color is left out of the style instead of being written as a default.
        var panelColor = MakeColorRow(
            vm,
            Se.Language.General.Color,
            nameof(WebVttStyleDisplay.UseColor),
            nameof(WebVttStyleDisplay.Color));
        var panelBackgroundColor = MakeColorRow(
            vm,
            Se.Language.General.Background,
            nameof(WebVttStyleDisplay.UseBackgroundColor),
            nameof(WebVttStyleDisplay.BackgroundColor));

        var panelShadow = MakeColorRow(
            vm,
            Se.Language.General.Shadow,
            nameof(WebVttStyleDisplay.UseShadow),
            nameof(WebVttStyleDisplay.ShadowColor));
        var labelShadowWidth = UiUtil.MakeLabel(Se.Language.General.ShadowWidth);
        var numericUpDownShadowWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 100, 110, vm, nameof(vm.SelectedStyle) + "." + nameof(WebVttStyleDisplay.ShadowWidth));
        numericUpDownShadowWidth.Increment = 1;
        panelShadow.Children.Add(labelShadowWidth);
        panelShadow.Children.Add(numericUpDownShadowWidth);

        grid.Add(label, 0, 0);
        grid.Add(panelName, 1, 0);
        grid.Add(panelFont, 2, 0);
        grid.Add(panelFontStyle, 3, 0);
        grid.Add(UiUtil.MakeHorizontalPanel(panelColor, panelBackgroundColor), 4, 0);
        grid.Add(panelShadow, 5, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static StackPanel MakeColorRow(WebVttStylesViewModel vm, string text, string useProperty, string colorProperty)
    {
        var checkBox = UiUtil.MakeCheckBox(text, vm, nameof(vm.SelectedStyle) + "." + useProperty);
        var button = MakeStyleColorPickerButton(vm, colorProperty);
        return UiUtil.MakeHorizontalPanel(checkBox, button);
    }

    private static Border MakePreviewView(WebVttStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Preview).WithBold();

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform, // Scale the preview frame to fit while keeping aspect ratio
            MinHeight = 140,
            MaxHeight = 220,
        };

        grid.Add(label, 0);
        grid.Add(image, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    /// <summary>
    /// The CSS the style was loaded from next to the CSS it will be saved as, so an edit to a
    /// hand-written style shows exactly what it changes.
    /// </summary>
    private static Border MakeRawStyleView(WebVttStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
        };

        var labelBeforeTitle = UiUtil.MakeLabel(Se.Language.General.Before).WithBold();
        var labelAfterTitle = UiUtil.MakeLabel(Se.Language.General.After).WithBold();

        var labelBefore = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.CssBefore));
        labelBefore.MinHeight = 70;
        labelBefore.VerticalContentAlignment = VerticalAlignment.Top;

        var labelAfter = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.CssAfter));
        labelAfter.MinHeight = 70;
        labelAfter.VerticalContentAlignment = VerticalAlignment.Top;

        grid.Add(labelBeforeTitle, 0, 0);
        grid.Add(labelAfterTitle, 0, 1);
        grid.Add(labelBefore, 1, 0);
        grid.Add(labelAfter, 1, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Button MakeStyleColorPickerButton(WebVttStylesViewModel vm, string colorPropertyName)
    {
        var swatch = new Border
        {
            Width = 30,
            Height = 20,
            CornerRadius = new CornerRadius(3),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            VerticalAlignment = VerticalAlignment.Center,
        };
        swatch.Bind(Border.BackgroundProperty, new Binding(nameof(vm.SelectedStyle) + "." + colorPropertyName)
        {
            Source = vm,
            Converter = new ColorToBrushConverter(),
        });

        var button = new Button
        {
            Content = swatch,
            Padding = new Thickness(4, 2),
            VerticalAlignment = VerticalAlignment.Center,
        };

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
