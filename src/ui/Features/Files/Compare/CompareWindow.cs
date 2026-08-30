﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;
using System;
using System.Collections.ObjectModel;

public class CompareWindow : Window
{
    public CompareWindow(CompareViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Compare;
        Width = 1200;
        Height = 600;
        MinWidth = 900;
        MinHeight = 500;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto), // browse buttons + file names
                new RowDefinition(GridLength.Star), // subtitle views
                new RowDefinition(GridLength.Auto), // status text
                new RowDefinition(GridLength.Auto), // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10
        };

        var buttonLeftFileName = UiUtil.MakeButtonBrowse(vm.PickLeftSubtitleFileCommand, accessibleName: Se.Language.General.OpenOriginalSubtitleFileTitle);
        var labelLeftFileName = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.LeftFileNameDisplay)),
            [!ToolTip.TipProperty] = new Binding(nameof(vm.LeftFileName)),
        };
        var labelLeftFileNameHasChanges = UiUtil.MakeLabel("*").WithBindVisible(vm, nameof(vm.LeftFileNameHasChanges));
        var panelLeftBrowse = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Children = { buttonLeftFileName, MakeIcon(IconNames.FileOutline), labelLeftFileName, labelLeftFileNameHasChanges },
        };
        grid.Add(panelLeftBrowse, 0);

        var buttonRightFileName = UiUtil.MakeButtonBrowse(vm.PickRightSubtitleFileCommand, accessibleName: Se.Language.General.OpenSubtitleFileTitle);
        var buttonRightReload = UiUtil.MakeButton(string.Format(Se.Language.File.LoadXFromFile, System.IO.Path.GetFileName(vm.LeftFileName)), vm.ReloadRightFromFileCommand)
            .WithIconLeft(IconNames.Refresh)
            .WithBindIsVisible(nameof(vm.IsReloadFromFileVisible));
        var labelRightFileName = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.RightFileNameDisplay)),
            [!ToolTip.TipProperty] = new Binding(nameof(vm.RightFileName)),
        };
        var panelRightBrowse = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Children = { buttonRightFileName, buttonRightReload, MakeIcon(IconNames.FileOutline), labelRightFileName },
        };
        grid.Add(panelRightBrowse, 0, 1);

        // left subtitle view (original)
        var leftView = MakeSubtitlesView(vm.LeftSubtitles, nameof(vm.SelectedLeft), vm.FileGridOnDragOver, vm.FileGridOnDropLeft);
        grid.Add(leftView, 1);

        // right subtitle view (modified)
        var rightView = MakeSubtitlesView(vm.RightSubtitles, nameof(vm.SelectedRight), vm.FileGridOnDragOver, vm.FileGridOnDropRight);
        grid.Add(rightView, 1, 1);

        // status text on the left, color legend for the difference highlighting on the right
        var statusText = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.StatusText));
        statusText.VerticalAlignment = VerticalAlignment.Center;
        var panelLegend = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0), // line up with the button bar's right edge
            Children =
            {
                MakeLegendSwatch(CompareColors.OnlyInOneFile, Se.Language.File.CompareOnlyInOneFile),
                MakeLegendSwatch(CompareColors.TextOrTimeDifference, Se.Language.File.CompareTextOrTimeDifference),
                MakeLegendSwatch(CompareColors.NumberDifference, Se.Language.File.CompareNumberDifference),
            },
        };
        grid.Add(MakeTwoColumnBar(statusText, panelLegend), 2, 0, 1, 2);

        // what to display + the two compare options
        var labelDisplayType = UiUtil.MakeLabel(Se.Language.General.Show).WithMarginRight(5);
        var comboBoxCompareVisual = UiUtil.MakeComboBox(vm.CompareVisuals, vm, nameof(vm.SelectedCompareVisual));
        comboBoxCompareVisual.SelectionChanged += vm.ComboBoxCompareVisualSelectionChanged;
        var checkBoxIgnoreWhiteSpace = UiUtil.MakeCheckBox(Se.Language.File.IgnoreWhitespace, vm, nameof(vm.IgnoreWhiteSpace))
            .WithMarginLeft(20);
        checkBoxIgnoreWhiteSpace.IsCheckedChanged += vm.CheckBoxChanged;
        AddHint(checkBoxIgnoreWhiteSpace, Se.Language.File.IgnoreWhitespaceHint);
        var checkBoxIgnoreFormatting = UiUtil.MakeCheckBox(Se.Language.File.IgnoreFormatting, vm, nameof(vm.IgnoreFormatting))
            .WithMarginLeft(14);
        checkBoxIgnoreFormatting.IsCheckedChanged += vm.CheckBoxChanged;
        AddHint(checkBoxIgnoreFormatting, Se.Language.File.IgnoreFormattingHint);
        var panelOptions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                MakeIcon(IconNames.Filter),
                labelDisplayType,
                comboBoxCompareVisual,
                checkBoxIgnoreWhiteSpace,
                checkBoxIgnoreFormatting,
            },
        };

        // buttons
        var buttonPreviousDifference = UiUtil.MakeButton(vm.PreviousDifferenceCommand, IconNames.ChevronLeft, Se.Language.File.PreviousDifference).WithBindIsVisible(nameof(vm.IsExportVisible));
        var buttonNextDifference = UiUtil.MakeButton(vm.NextDifferenceCommand, IconNames.ChevronRight, Se.Language.File.NextDifference).WithBindIsVisible(nameof(vm.IsExportVisible));
        // Bound like its two neighbours: with only one side loaded (which is how Tools > Compare
        // always opens) the collections are never padded to equal length, and Export indexes the
        // right-hand list by the left-hand count.
        var buttonExport = UiUtil.MakeButton(Se.Language.General.Export, vm.ExportCommand)
            .WithIconLeft(IconNames.Export)
            .WithBindIsVisible(nameof(vm.IsExportVisible))
            .WithMarginLeft(15);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonPreviousDifference,
            buttonNextDifference,
            buttonExport,
            buttonOk
            );

        // One row holding both: the two used to be dropped on top of each other in the same cell,
        // which only stayed readable while the left group was short enough to clear the buttons.
        grid.Add(MakeTwoColumnBar(panelOptions, panelButtons), 3, 0, 1, 2);

        Content = grid;

        Activated += delegate
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (vm.LeftGrid != null)
                {
                    TableViewExtras.FocusRow(vm.LeftGrid); // initial focus on an input, not an action button - a focused button clicks on bare Space
                }
            });
        };
        KeyDown += vm.KeyDown;

        vm.LeftGrid = (leftView.Child as Border)?.Child as TableView;
        vm.RightGrid = (rightView.Child as Border)?.Child as TableView;
        if (vm.LeftGrid != null && vm.RightGrid != null)
        {
            vm.LeftGrid.SelectionChanged += vm.LeftGridSelectionChanged;
            vm.RightGrid.SelectionChanged += vm.RightGridSelectionChanged;

            // Scrolling either side keeps the other aligned to the same rows, like SE4 (#13504).
            vm.ScrollSync = new TableViewScrollSync(vm.LeftGrid, vm.RightGrid);
        }

        Closing += delegate
        {
            UiUtil.SaveWindowPosition(this);
            vm.SaveSettings(); // the compare options are remembered between sessions (#14299)
        };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    /// <summary>Left group and right group in one row, so a wide left group cannot overlap the buttons.</summary>
    private static Grid MakeTwoColumnBar(Control left, Control right)
    {
        var bar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
        };

        bar.Add(left, 0);
        bar.Add(right, 0, 1);

        return bar;
    }

    private static ContentControl MakeIcon(string iconName)
    {
        var icon = new ContentControl
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.75,
        };

        Attached.SetIcon(icon, iconName);

        return icon;
    }

    private static void AddHint(Control control, string hint)
    {
        if (Se.Settings.Appearance.ShowHints)
        {
            UiUtil.AttachHoverTooltip(control, hint);
        }
    }

    private static Control MakeLegendSwatch(Color color, string label)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(14, 0, 0, 0),
            Children =
            {
                new Border
                {
                    Width = 12,
                    Height = 12,
                    CornerRadius = new CornerRadius(3),
                    Background = new SolidColorBrush(color),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(0x60, 0x60, 0x60, 0x60)),
                    BorderThickness = new Thickness(1),
                    VerticalAlignment = VerticalAlignment.Center,
                },
                new TextBlock
                {
                    Text = label,
                    Opacity = 0.8,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };
    }

    private static Border MakeSubtitlesView(ObservableCollection<CompareItem> items, string selectedBinding, Delegate fileGridOnDragOver, Delegate fileGridOnDrop)
    {
        var dg = TableViewExtras.MakeTableView(multiSelect: false);
        dg.ItemsSource = items;
        dg.Height = double.NaN;
        dg.Margin = new Thickness(2);

        // Number column
        dg.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Width = new GridLength(50),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.NumberBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(CompareItem.NumberDisplay))
                };

                border.Child = textBlock;
                return border;
            })
        });

        // StartTime column
        dg.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            Width = new GridLength(120),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.StartTimeBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(CompareItem.StartTimeDisplay)),
                };

                border.Child = textBlock;
                return border;
            })
        });

        // EndTime column
        dg.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Hide,
            Width = new GridLength(120),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.EndTimeBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(CompareItem.EndTimeDisplay)),
                };

                border.Child = textBlock;
                return border;
            })
        });

        // Text column
        dg.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.TextBackgroundBrush))
                };

                var stackPanel = new StackPanel();
                if (item.TextPanel.Parent is Panel parent)
                {
                    parent.Children.Remove(item.TextPanel);
                }
                stackPanel.Children.Add(item.TextPanel);

                border.Child = stackPanel;
                return border;
            })
        });

        dg.Bind(TableView.SelectedItemProperty, new Binding(selectedBinding)
        {
            Mode = BindingMode.TwoWay
        });

        // hack to make drag and drop work on the grid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = dg,
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, fileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, fileGridOnDrop, RoutingStrategies.Bubble);

        return UiUtil.MakeBorderForControl(dropHost);
    }
}
