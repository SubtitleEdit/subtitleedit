using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa;

public class AssaStylesWindow : Window
{
    public AssaStylesWindow(AssaStylesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        CanResize = true;
        Width = 1200;
        Height = 850;
        MinWidth = 1100;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
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

        grid.Add(MakeLeftView(vm), 0);
        grid.Add(MakeRightView(vm), 0, 1);
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Grid MakeLeftView(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeFileStyles(vm), 0);
        grid.Add(MakeStorageStyles(vm), 1);

        return grid;
    }

    private static Border MakeFileStyles(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) }, // label
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }, // data grid
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel(Se.Language.Assa.StylesInFile).WithBold();

        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Extended,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.FileStyles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.FontName)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontSize,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.FontSize)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Usages,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.UsageCount)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedFileStyle)) { Source = vm });
        dataGrid.SelectionChanged += vm.FileStylesChanged;
        dataGrid.KeyDown += vm.FileStylesKeyDown;
        vm.FileStyleGrid = dataGrid;

        var flyout = new MenuFlyout();
        flyout.Opening += vm.FilesContextMenuOpening;
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        var menuItemCopyToStorageStyles = new MenuItem
        {
            Header = Se.Language.Assa.CopyToStorageStyles,
            DataContext = vm,
            Command = vm.FileCopyToStorageCommand,
        };
        menuItemCopyToStorageStyles.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsFileStyleSelected)) { Source = vm });
        flyout.Items.Add(menuItemCopyToStorageStyles);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.FileRemoveCommand,
        };
        menuItemDelete.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteVisible)) { Source = vm });
        flyout.Items.Add(menuItemDelete);

        var menuItemClear = new MenuItem
        {
            Header = Se.Language.General.Clear,
            DataContext = vm,
            Command = vm.FileRemoveAllCommand,
        };
        menuItemClear.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteAllVisible)) { Source = vm });
        flyout.Items.Add(menuItemClear);

        var menuItemTakeUsagesFrom = new MenuItem
        {
            Header = Se.Language.Assa.TakeUsagesFromDotDotDot,
            DataContext = vm,
            Command = vm.FileTakeUsagesFromCommand,
        };
        menuItemTakeUsagesFrom.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsTakeUsagesFromVisible)) { Source = vm });
        flyout.Items.Add(menuItemTakeUsagesFrom);

        var buttonNew = UiUtil.MakeButton(vm.FileNewCommand, IconNames.Plus, Se.Language.General.New);
        var buttonRemove = UiUtil.MakeButton(vm.FileRemoveCommand, IconNames.Trash, Se.Language.General.Delete);
        var buttonDuplicate = UiUtil.MakeButton(vm.FilesDuplicateCommand, IconNames.Duplicate, Se.Language.General.Duplicate);
        var buttonImport = UiUtil.MakeButton(vm.FileImportCommand, IconNames.Import, Se.Language.General.Import);
        var buttonExport = UiUtil.MakeButton(vm.FileExportCommand, IconNames.Export, Se.Language.General.Export);
        var buttonCopyToStorage = UiUtil.MakeButton(Se.Language.Assa.CopyToStorageStyles, vm.FileCopyToStorageCommand).WithBindEnabled(nameof(vm.IsFileStyleSelected));
        var panelButtons = UiUtil.MakeButtonBar(
            buttonNew,
            buttonDuplicate,
            buttonRemove,
            buttonImport,
            buttonExport,
            buttonCopyToStorage
        ).WithAlignmentLeft();

        grid.Add(label, 0, 0);
        grid.Add(dataGrid, 1, 0);
        grid.Add(panelButtons, 2, 0);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5);
    }

    private static Border MakeStorageStyles(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel(Se.Language.Assa.StylesSaved).WithBold();

        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Extended,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.StorageStyles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.FontName)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FontSize,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.FontSize)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.IsDefault,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(StyleDisplay.IsDefault)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedStorageStyle)) { Source = vm });
        dataGrid.SelectionChanged += vm.StorageStylesChanged;
        vm.StorageStyleGrid = dataGrid;

        var flyout = new MenuFlyout();
        flyout.Opening += vm.StoreContextMenuOpening;
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        var menuItemCopyToFileStyles = new MenuItem
        {
            Header = Se.Language.Assa.CopyToFileStyles,
            DataContext = vm,
            Command = vm.StorageCopyToFilesCommand,
        };
        menuItemCopyToFileStyles.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsCopyToFileStylesVisible)) { Source = vm });
        flyout.Items.Add(menuItemCopyToFileStyles);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.StorageRemoveCommand,
        };
        menuItemDelete.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteVisible)) { Source = vm });
        flyout.Items.Add(menuItemDelete);

        var menuItemClear = new MenuItem
        {
            Header = Se.Language.General.Clear,
            DataContext = vm,
            Command = vm.StorageRemoveAllCommand,
        };
        menuItemClear.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteAllVisible)) { Source = vm });
        flyout.Items.Add(menuItemClear);

        var menuItemClearSetAsDefault = new MenuItem
        {
            Header = Se.Language.Assa.SetStyleAsDefault,
            DataContext = vm,
            Command = vm.StorageSetDefaultCommand,
        };
        menuItemClearSetAsDefault.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsSetStyleAsDefaultVisible)) { Source = vm });
        flyout.Items.Add(menuItemClearSetAsDefault);

        var buttonNew = UiUtil.MakeButton(vm.StorageNewCommand, IconNames.Plus, Se.Language.General.New);
        var buttonDuplicate = UiUtil.MakeButton(vm.StorageDuplicateCommand, IconNames.Duplicate, Se.Language.General.Duplicate);
        var buttonRemove = UiUtil.MakeButton(vm.StorageRemoveCommand, IconNames.Trash, Se.Language.General.Delete);
        var buttonImport = UiUtil.MakeButton(vm.StorageImportCommand, IconNames.Import, Se.Language.General.Import);
        var buttonExport = UiUtil.MakeButton(vm.StorageExportCommand, IconNames.Export, Se.Language.General.Export);
        var buttonSetDefault = UiUtil.MakeButton(vm.StorageSetDefaultCommand, IconNames.Check, Se.Language.Assa.SetStyleAsDefault);
        var buttonCopyToFiles = UiUtil.MakeButton(Se.Language.Assa.CopyToFileStyles, vm.StorageCopyToFilesCommand).WithBindEnabled(nameof(vm.IsStorageStyleSelected));
        var panelButtons = UiUtil.MakeButtonBar(
            buttonNew,
            buttonDuplicate,
            buttonRemove,
            buttonImport,
            buttonExport,
            buttonSetDefault,
            buttonCopyToFiles
        ).WithAlignmentLeft();

        grid.Add(label, 0, 0);
        grid.Add(dataGrid, 1, 0);
        grid.Add(panelButtons, 2, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Grid MakeRightView(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeSelectedStyleView(vm), 0);
        grid.Add(MakePreviewView(vm), 1);

        return grid;
    }

    private static Border MakeSelectedStyleView(AssaStylesViewModel vm)
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

        var label = UiUtil.MakeLabel().WithBold().WithBindText(vm, nameof(vm.CurrentTitle));

        var labelName = UiUtil.MakeLabel(Se.Language.General.Name);
        var textBoxName = UiUtil.MakeTextBox(200, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Name));
        var panelName = UiUtil.MakeHorizontalPanel(labelName, textBoxName).WithMarginBottom(10);

        var labelFontName = UiUtil.MakeLabel(Se.Language.General.FontName);
        var comboBoxFontName = MakeSearchableFontNameBox(vm);
        var buttonFontBrowse = UiUtil.MakeButtonBrowse(vm.BrowseFontNameCommand);
        var labelFontSize = UiUtil.MakeLabel(Se.Language.General.FontSize);
        var numericUpDownFontSize = UiUtil.MakeNumericUpDownOneDecimal(1, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.FontSize));
        numericUpDownFontSize.Increment = 1;
        var panelFont = UiUtil.MakeHorizontalPanel(labelFontName, comboBoxFontName, buttonFontBrowse, labelFontSize, numericUpDownFontSize);

        var checkBoxBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Bold));
        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Italic));
        var checkBoxUnderline = UiUtil.MakeCheckBox(Se.Language.General.Underline, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Underline));
        var checkBoxStrikeout = UiUtil.MakeCheckBox(Se.Language.General.Strikeout, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Strikeout));
        var panelFontStyle = UiUtil.MakeHorizontalPanel(checkBoxBold, checkBoxItalic, checkBoxUnderline, checkBoxStrikeout).WithMarginBottom(10);

        var labelScaleX = UiUtil.MakeLabel("Scale X").WithMinWidth(60);
        var numericUpDownScaleX = UiUtil.MakeNumericUpDownOneDecimal(1, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ScaleX));
        numericUpDownScaleX.Increment = 1;
        var labelScaleY = UiUtil.MakeLabel("Scale Y").WithMinWidth(60);
        var numericUpDownScaleY = UiUtil.MakeNumericUpDownOneDecimal(1, 1000, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ScaleY));
        numericUpDownScaleY.Increment = 1;
        var panelTransform1 = UiUtil.MakeHorizontalPanel(labelScaleX, numericUpDownScaleX, labelScaleY, numericUpDownScaleY);

        var labelSpacing = UiUtil.MakeLabel("Spacing").WithMinWidth(60);
        var numericUpDownSpacing = UiUtil.MakeNumericUpDownOneDecimal(-100, 100, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Spacing));
        numericUpDownSpacing.Increment = 1;
        var labelAngle = UiUtil.MakeLabel("Angle").WithMinWidth(60);
        var numericUpDownAngle = UiUtil.MakeNumericUpDownOneDecimal(-360, 360, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.Angle));
        numericUpDownAngle.Increment = 1;
        var panelTransform2 = UiUtil.MakeHorizontalPanel(labelSpacing, numericUpDownSpacing, labelAngle, numericUpDownAngle).WithMarginBottom(10);

        var labelColorPrimary = UiUtil.MakeLabel(Se.Language.Assa.Primary);
        var colorPickerPrimary = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorPrimary));
        var labelColorOutline = UiUtil.MakeLabel(Se.Language.General.Outline);
        var colorPickerOutline = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorOutline));
        var labelColorShadow = UiUtil.MakeLabel(Se.Language.General.Shadow);
        var colorPickerShadow = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorShadow));
        var labelColorSecondary = UiUtil.MakeLabel(Se.Language.Assa.Secondary);
        var colorPickerSecondary = UiUtil.MakeColorPicker(vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ColorSecondary));
        var panelColors = UiUtil.MakeHorizontalPanel(
            labelColorPrimary,
            colorPickerPrimary,
            labelColorOutline,
            colorPickerOutline,
            labelColorShadow,
            colorPickerShadow,
            labelColorSecondary,
            colorPickerSecondary
        ).WithMarginBottom(10);

        var alignmentView = MakeAlignmentView(vm);
        var marginView = MakeMarginView(vm);
        var borderView = MakeBorderView(vm);
        var panelMore = UiUtil.MakeHorizontalPanel(alignmentView, marginView, borderView);

        grid.Add(label, 0, 0);
        grid.Add(panelName, 1, 0);
        grid.Add(panelFont, 2, 0);
        grid.Add(panelFontStyle, 3, 0);
        grid.Add(panelTransform1, 4, 0);
        grid.Add(panelTransform2, 5, 0);
        grid.Add(panelColors, 6, 0);
        grid.Add(panelMore, 7, 0);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5);
    }

    private static AutoCompleteBox MakeSearchableFontNameBox(AssaStylesViewModel vm)
    {
        const double fontBoxWidth = 240;
        var box = new AutoCompleteBox
        {
            DataContext = vm,
            Width = fontBoxWidth,
            MinWidth = fontBoxWidth,
            MaxWidth = fontBoxWidth,
            ItemsSource = vm.Fonts,
            MinimumPrefixLength = 0,
            PlaceholderText = Se.Language.General.SearchFontNames,
            [!AutoCompleteBox.TextProperty] = new Binding(nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.FontName)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<string>((fontName, _) =>
            {
                var item = MakeFontSuggestionTextBlock(fontName, fontBoxWidth);
                item.PointerEntered += (_, _) => vm.PreviewFontName(fontName);
                return item;
            }, true),
        };
        box.DropDownClosed += (_, _) => vm.ClearPreviewFontName();
        KeepFontDropDownOpen(box, FontMatches);
        return box;
    }

    private static Control MakeFontSuggestionTextBlock(string? fontName, double width)
    {
        var border = new Border
        {
            Width = width - 8,
            Height = 18,
            Background = Brushes.Transparent,
            Margin = new Thickness(-5, -4, 0, -4),
            Padding = new Thickness(2, 0, 0, 0),
            Child = new TextBlock
            {
                Text = fontName,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            },
        };
        border.AddHandler(InputElement.PointerWheelChangedEvent, (s, e) => UiUtil.ScrollDropDownItemOnPointerWheel(e.Source ?? s, e));
        return border;
    }

    private static bool FontMatches(string? searchText, string? fontName)
    {
        return string.IsNullOrEmpty(searchText) ||
               fontName?.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) == true;
    }

    private static void KeepFontDropDownOpen(AutoCompleteBox box, Func<string?, string?, bool> filter)
    {
        var userActivated = false;
        var showAllForInitialOpen = false;
        var initialOpenText = string.Empty;
        var skipNextPointerRelease = false;
        var keepOpenForTextBoxClick = false;

        box.TextFilter = (searchText, itemText) =>
        {
            if (showAllForInitialOpen && string.Equals(searchText, initialOpenText, StringComparison.CurrentCulture))
            {
                return true;
            }

            return filter(searchText, itemText);
        };

        void OpenDropDown()
        {
            if (!userActivated)
            {
                return;
            }

            UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
            Dispatcher.UIThread.Post(() =>
            {
                box.IsDropDownOpen = true;
                UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
            }, DispatcherPriority.Background);
            DispatcherTimer.RunOnce(() =>
            {
                box.IsDropDownOpen = true;
                UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
            }, TimeSpan.FromMilliseconds(10));
            DispatcherTimer.RunOnce(() =>
            {
                box.IsDropDownOpen = true;
                UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
            }, TimeSpan.FromMilliseconds(50));
        }

        bool IsPointerInsideFontSearchBox(PointerEventArgs e)
        {
            return e.Source is Visual visual &&
                   (ReferenceEquals(visual, box) || visual.GetVisualAncestors().Contains(box));
        }

        box.AddHandler(InputElement.PointerReleasedEvent, (_, e) =>
        {
            if (skipNextPointerRelease)
            {
                skipNextPointerRelease = false;
                return;
            }

            userActivated = true;
            showAllForInitialOpen = true;
            initialOpenText = box.Text ?? string.Empty;
            if (keepOpenForTextBoxClick)
            {
                e.Handled = true;
                keepOpenForTextBoxClick = false;
            }

            OpenDropDown();
        }, RoutingStrategies.Tunnel, true);
        box.AddHandler(InputElement.PointerPressedEvent, (_, e) =>
        {
            if (box.IsDropDownOpen && e.ClickCount == 1 && IsPointerInsideFontSearchBox(e))
            {
                keepOpenForTextBoxClick = true;
                e.Handled = true;
                UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
                return;
            }

            if (e.ClickCount > 1)
            {
                skipNextPointerRelease = true;
                userActivated = true;
                showAllForInitialOpen = true;
                initialOpenText = box.Text ?? string.Empty;
                Dispatcher.UIThread.Post(() =>
                {
                    box.GetVisualDescendants().OfType<TextBox>().FirstOrDefault()?.SelectAll();
                    OpenDropDown();
                });
            }
        }, RoutingStrategies.Tunnel, true);
        box.DropDownOpening += (_, _) => UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
        box.DropDownClosing += (_, e) =>
        {
            if (!keepOpenForTextBoxClick)
            {
                return;
            }

            e.Cancel = true;
            keepOpenForTextBoxClick = false;
            UiUtil.StabilizeDropDownWidth(box, box.ItemsSource);
        };
        box.DropDownClosed += (_, _) =>
        {
            keepOpenForTextBoxClick = false;
        };
        box.TextChanged += (_, _) =>
        {
            if (!string.Equals(box.Text ?? string.Empty, initialOpenText, StringComparison.CurrentCulture))
            {
                showAllForInitialOpen = false;
            }

            OpenDropDown();
        };
    }
    private static Border MakeAlignmentView(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Alignment);

        grid.Add(label, 0, 0, 1, 3);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn7), "align"), 1, 0);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn8), "align"), 1, 1);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn9), "align"), 1, 2);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn4), "align"), 2, 0);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn5), "align"), 2, 1);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn6), "align"), 2, 2);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn1), "align"), 3, 0);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn2), "align"), 3, 1);
        grid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.AlignmentAn3), "align"), 3, 2);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeMarginView(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.Margin);
        grid.Add(label, 0);

        var labelMarginLeft = UiUtil.MakeLabel(Se.Language.General.Left);
        var numericUpDownMarginLeft = UiUtil.MakeNumericUpDownInt(0, 1000, 10, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.MarginLeft));
        grid.Add(labelMarginLeft, 1, 0);
        grid.Add(numericUpDownMarginLeft, 1, 1);

        var labelMarginRight = UiUtil.MakeLabel(Se.Language.General.Right);
        var numericUpDownMarginRight = UiUtil.MakeNumericUpDownInt(0, 1000, 10, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.MarginRight));
        grid.Add(labelMarginRight, 2, 0);
        grid.Add(numericUpDownMarginRight, 2, 1);

        var labelMarginVertical = UiUtil.MakeLabel(Se.Language.General.Vertical);
        var numericUpDownMarginVertical = UiUtil.MakeNumericUpDownInt(0, 1000, 10, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.MarginVertical));
        grid.Add(labelMarginVertical, 3, 0);
        grid.Add(numericUpDownMarginVertical, 3, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeBorderView(AssaStylesViewModel vm)
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
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowSpacing = 5,
        };

        var label = UiUtil.MakeLabel(Se.Language.General.BorderStyle);
        grid.Add(label, 1, 0);

        var comboBoxBorderType = UiUtil.MakeComboBox(vm.BorderTypes, vm, nameof(vm.SelectedBorderType));
        comboBoxBorderType.SelectionChanged += vm.BorderTypeChanged;
        grid.Add(comboBoxBorderType, 2, 0, 1, 2);

        var labelOutlineWidth = UiUtil.MakeLabel(Se.Language.General.OutlineWidth);
        var numericUpDownOutlineWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 100, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.OutlineWidth));
        numericUpDownOutlineWidth.Increment = 0.5m;
        grid.Add(labelOutlineWidth, 3, 0);
        grid.Add(numericUpDownOutlineWidth, 3, 1);

        var labelShadowWidth = UiUtil.MakeLabel(Se.Language.General.ShadowWidth);
        var numericUpDownShadowWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 100, 130, vm, nameof(vm.CurrentStyle) + "." + nameof(StyleDisplay.ShadowWidth));
        numericUpDownShadowWidth.Increment = 0.5m;
        grid.Add(labelShadowWidth, 4, 0);
        grid.Add(numericUpDownShadowWidth, 4, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakePreviewView(AssaStylesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
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
            Width = 360,
            Height = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
        };

        grid.Add(label, 0);
        grid.Add(image, 1);

        return UiUtil.MakeBorderForControl(grid);
    }
}
