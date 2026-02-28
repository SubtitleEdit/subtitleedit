using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static partial class InitLayout
{
    public const int WaveFormHeight = 150;
    public static int LayoutWithoutVideo = 12;

    public class LayoutPositions
    {
        public List<double> RowHeights { get; set; } = new();
        public List<double> ColumnWidths { get; set; } = new();
        public List<double> NestedRowHeights { get; set; } = new();
        public List<double> NestedColumnWidths { get; set; } = new();

        // store unit types so we do not convert Star/Auto to fixed pixels on restore
        public List<GridUnitType> RowUnitTypes { get; set; } = new();
        public List<GridUnitType> ColumnUnitTypes { get; set; } = new();
        public List<GridUnitType> NestedRowUnitTypes { get; set; } = new();
        public List<GridUnitType> NestedColumnUnitTypes { get; set; } = new();
    }

    public static LayoutPositions SaveLayoutPositions(Grid? contentGrid)
    {
        if (contentGrid == null || contentGrid.Children.Count == 0)
        {
            return new LayoutPositions();
        }

        var positions = new LayoutPositions();

        // Save main grid row heights + unit types
        foreach (var rowDef in contentGrid.RowDefinitions)
        {
            positions.RowHeights.Add(rowDef.ActualHeight);
            positions.RowUnitTypes.Add(rowDef.Height.GridUnitType);
        }

        // Save main grid column widths + unit types
        foreach (var colDef in contentGrid.ColumnDefinitions)
        {
            positions.ColumnWidths.Add(colDef.ActualWidth);
            positions.ColumnUnitTypes.Add(colDef.Width.GridUnitType);
        }

        // Look for nested grids
        foreach (var child in contentGrid.Children)
        {
            if (child is Border border && border.Child is Grid nestedGrid)
            {
                // Save nested grid row heights + unit types
                foreach (var rowDef in nestedGrid.RowDefinitions)
                {
                    positions.NestedRowHeights.Add(rowDef.ActualHeight);
                    positions.NestedRowUnitTypes.Add(rowDef.Height.GridUnitType);
                }

                // Save nested grid column widths + unit types
                foreach (var colDef in nestedGrid.ColumnDefinitions)
                {
                    positions.NestedColumnWidths.Add(colDef.ActualWidth);
                    positions.NestedColumnUnitTypes.Add(colDef.Width.GridUnitType);
                }

                break; // Only process the first nested grid found
            }
        }

        return positions;
    }

    public static void RestoreLayoutPositions(LayoutPositions positions, Grid? contentGrid)
    {
        if (contentGrid == null ||
            contentGrid.Children.Count == 0 ||
            positions.ColumnWidths.Count + positions.RowHeights.Count == 0)
        {
            return;
        }

        // Restore main grid row heights - preserve Pixel, restore Star ratio, keep Auto
        for (var i = 0; i < Math.Min(contentGrid.RowDefinitions.Count, positions.RowHeights.Count); i++)
        {
            var savedHeight = positions.RowHeights[i];
            var unitType = i < positions.RowUnitTypes.Count ? positions.RowUnitTypes[i] : GridUnitType.Pixel; // default pixel
            if (savedHeight <= 0)
            {
                continue;
            }

            if (unitType == GridUnitType.Pixel)
            {
                contentGrid.RowDefinitions[i].Height = new GridLength(savedHeight, GridUnitType.Pixel);
            }
            else if (unitType == GridUnitType.Star)
            {
                contentGrid.RowDefinitions[i].Height = new GridLength(savedHeight, GridUnitType.Star);
            }
            // Auto: do nothing
        }

        // Restore main grid column widths - preserve Pixel, restore Star ratio, keep Auto
        for (var i = 0; i < Math.Min(contentGrid.ColumnDefinitions.Count, positions.ColumnWidths.Count); i++)
        {
            var savedWidth = positions.ColumnWidths[i];
            var unitType = i < positions.ColumnUnitTypes.Count ? positions.ColumnUnitTypes[i] : GridUnitType.Pixel;
            if (savedWidth <= 0)
            {
                continue;
            }

            if (unitType == GridUnitType.Pixel)
            {
                contentGrid.ColumnDefinitions[i].Width = new GridLength(savedWidth, GridUnitType.Pixel);
            }
            else if (unitType == GridUnitType.Star)
            {
                contentGrid.ColumnDefinitions[i].Width = new GridLength(savedWidth, GridUnitType.Star);
            }
            // Auto: do nothing
        }

        // Look for nested grids to restore
        foreach (var child in contentGrid.Children)
        {
            if (child is Border border && border.Child is Grid nestedGrid)
            {
                // Restore nested grid row heights - preserve Pixel, restore Star ratio, keep Auto
                for (var i = 0; i < Math.Min(nestedGrid.RowDefinitions.Count, positions.NestedRowHeights.Count); i++)
                {
                    var savedHeight = positions.NestedRowHeights[i];
                    var unitType = i < positions.NestedRowUnitTypes.Count ? positions.NestedRowUnitTypes[i] : GridUnitType.Pixel;
                    if (savedHeight <= 0)
                    {
                        continue;
                    }

                    if (unitType == GridUnitType.Pixel)
                    {
                        nestedGrid.RowDefinitions[i].Height = new GridLength(savedHeight, GridUnitType.Pixel);
                    }
                    else if (unitType == GridUnitType.Star)
                    {
                        nestedGrid.RowDefinitions[i].Height = new GridLength(savedHeight, GridUnitType.Star);
                    }
                    // Auto: do nothing
                }

                // Restore nested grid column widths (video/list view splitter) - preserve Pixel, restore Star ratio, keep Auto
                for (var i = 0; i < Math.Min(nestedGrid.ColumnDefinitions.Count, positions.NestedColumnWidths.Count); i++)
                {
                    var savedWidth = positions.NestedColumnWidths[i];
                    var unitType = i < positions.NestedColumnUnitTypes.Count ? positions.NestedColumnUnitTypes[i] : GridUnitType.Pixel;
                    if (savedWidth <= 0)
                    {
                        continue;
                    }

                    if (unitType == GridUnitType.Pixel)
                    {
                        nestedGrid.ColumnDefinitions[i].Width = new GridLength(savedWidth, GridUnitType.Pixel);
                    }
                    else if (unitType == GridUnitType.Star)
                    {
                        nestedGrid.ColumnDefinitions[i].Width = new GridLength(savedWidth, GridUnitType.Star);
                    }
                    // Auto: do nothing (usually the splitter column)
                }

                break; // Only process the first nested grid found
            }
        }
    }

    public static int MakeLayout(MainView mainPage, MainViewModel vm, int layoutNumber)
    {
        return layoutNumber switch
        {
            2 => MakeLayout2(mainPage, vm),
            3 => MakeLayout3(mainPage, vm),
            4 => MakeLayout4(mainPage, vm),
            5 => MakeLayout5(mainPage, vm),
            6 => MakeLayout6(mainPage, vm),
            7 => MakeLayout7(mainPage, vm),
            8 => MakeLayout8(mainPage, vm),
            9 => MakeLayout9(mainPage, vm),
            10 => MakeLayout10(mainPage, vm),
            11 => MakeLayout11(mainPage, vm),
            12 => MakeLayout12(mainPage, vm),
            _ => MakeLayout1(mainPage, vm)
        };
    }

    private static int MakeLayout1(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top content (will hold the nested grid)
        var topContent = new Border();
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Create a nested grid with columns (for vertical split)
        var nestedGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left part of nested grid
        var nestedLeft = new Border
        {
            Child = InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm)
        };
        Grid.SetColumn(nestedLeft, 0);
        nestedGrid.Children.Add(nestedLeft);

        // Vertical GridSplitter
        var nestedSplitter = new GridSplitter
        {
            Width = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(nestedSplitter, 1);
        nestedGrid.Children.Add(nestedSplitter);

        // Right part of nested grid
        var nestedRight = new Border
        {
            Child = InitVideoPlayer.MakeLayoutVideoPlayer(vm),
        };
        Grid.SetColumn(nestedRight, 2);
        nestedGrid.Children.Add(nestedRight);

        // Add nested grid to top content
        topContent.Child = nestedGrid;

        // Main GridSplitter (horizontal)
        var splitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom content
        var bottomContent = new Border
        {
            Child = InitWaveform.MakeWaveform(vm),
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = double.NaN,
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(contentGrid);

        // set waveform height
        contentGrid.RowDefinitions[2].Height = new GridLength(WaveFormHeight, GridUnitType.Pixel);

        return 1;
    }

    private static int MakeLayout2(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top content (will hold the nested grid)
        var topContent = new Border();
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Create a nested grid with columns (for vertical split)
        var nestedGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left part of nested grid
        var nestedLeft = new Border
        {
            Child = InitVideoPlayer.MakeLayoutVideoPlayer(vm),
        };
        Grid.SetColumn(nestedLeft, 0);
        nestedGrid.Children.Add(nestedLeft);

        // Vertical GridSplitter
        var nestedSplitter = new GridSplitter
        {
            Width = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(nestedSplitter, 1);
        nestedGrid.Children.Add(nestedSplitter);

        // Right part of nested grid
        var nestedRight = new Border
        {
            Child = InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm)
        };
        Grid.SetColumn(nestedRight, 2);
        nestedGrid.Children.Add(nestedRight);

        // Add nested grid to top content
        topContent.Child = nestedGrid;

        // Main GridSplitter (horizontal)
        var splitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom content
        var bottomContent = new Border
        {
            Child = InitWaveform.MakeWaveform(vm),
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = double.NaN,
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(contentGrid);

        // set waveform height
        contentGrid.RowDefinitions[2].Height = new GridLength(WaveFormHeight, GridUnitType.Pixel);

        return 2;
    }

    private static int MakeLayout3(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left content (will hold the nested grid)
        var leftContent = new Border();
        Grid.SetColumn(leftContent, 0);
        contentGrid.Children.Add(leftContent);

        // Create a nested grid with rows (for horizontal split in left section)
        var nestedGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(WaveFormHeight, GridUnitType.Pixel)
            }
        };

        // Top part of nested grid
        var nestedTop = new Border
        {
            Child = InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm)
        };
        Grid.SetRow(nestedTop, 0);
        nestedGrid.Children.Add(nestedTop);

        // Horizontal GridSplitter in left section
        var nestedSplitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(nestedSplitter, 1);
        nestedGrid.Children.Add(nestedSplitter);

        // Bottom part of nested grid
        var nestedBottom = new Border
        {
            Child = InitWaveform.MakeWaveform(vm),
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = double.NaN,
        };
        Grid.SetRow(nestedBottom, 2);
        nestedGrid.Children.Add(nestedBottom);

        // Add nested grid to left content
        leftContent.Child = nestedGrid;

        // Main GridSplitter (vertical)
        var splitter = new GridSplitter
        {
            Width = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Right content
        var rightContent = new Border
        {
            Child = InitVideoPlayer.MakeLayoutVideoPlayer(vm),
        };
        Grid.SetColumn(rightContent, 2);
        contentGrid.Children.Add(rightContent);

        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(contentGrid);

        return 3;
    }

    private static int MakeLayout4(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left content (single panel, no splitter)
        var leftContent = new Border
        {
            Child = InitVideoPlayer.MakeLayoutVideoPlayer(vm),
        };
        Grid.SetColumn(leftContent, 0);
        contentGrid.Children.Add(leftContent);

        // Main GridSplitter (vertical between left and right sections)
        var mainSplitter = new GridSplitter
        {
            Width = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(mainSplitter, 1);
        contentGrid.Children.Add(mainSplitter);

        // Right content (will hold the nested grid for right section)
        var rightContent = new Border();
        Grid.SetColumn(rightContent, 2);
        contentGrid.Children.Add(rightContent);

        // Create a nested grid with rows for right section (horizontal split)
        var rightNestedGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(WaveFormHeight, GridUnitType.Pixel),
            }
        };

        // Top part of right nested grid
        var rightNestedTop = new Border
        {
            Child = InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm)
        };
        Grid.SetRow(rightNestedTop, 0);
        rightNestedGrid.Children.Add(rightNestedTop);

        // Horizontal GridSplitter in right section
        var rightNestedSplitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(rightNestedSplitter, 1);
        rightNestedGrid.Children.Add(rightNestedSplitter);

        // Bottom part of right nested grid
        var rightNestedBottom = new Border
        {
            Child = InitWaveform.MakeWaveform(vm),
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = double.NaN,
        };
        Grid.SetRow(rightNestedBottom, 2);
        rightNestedGrid.Children.Add(rightNestedBottom);

        // Add right nested grid to right content
        rightContent.Child = rightNestedGrid;

        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(contentGrid);

        return 4;
    }

    private static int MakeLayout5(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top section
        var topContent = new Border
        {
            Child = InitVideoPlayer.MakeLayoutVideoPlayer(vm),
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // First horizontal splitter (between top and middle)
        var firstSplitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(firstSplitter, 1);
        contentGrid.Children.Add(firstSplitter);

        // Middle section
        var middleContent = new Border
        {
            Child = InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm)
        };
        Grid.SetRow(middleContent, 2);
        contentGrid.Children.Add(middleContent);

        // Second horizontal splitter (between middle and bottom)
        var secondSplitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(secondSplitter, 3);
        contentGrid.Children.Add(secondSplitter);

        // Bottom section
        var bottomContent = new Border
        {
            Child = InitWaveform.MakeWaveform(vm),
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = double.NaN,
        };
        Grid.SetRow(bottomContent, 4);
        contentGrid.Children.Add(bottomContent);

        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(contentGrid);

        return 5;
    }

    private static int MakeLayout6(MainView mainPage, MainViewModel vm)
    {

        var contentGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(WaveFormHeight, GridUnitType.Pixel),
            }
        };

        // video player
        var borderVideo = new Border
        {
            Child = InitVideoPlayer.MakeLayoutVideoPlayer(vm),
            Width = 0, 
            Height = 0,  
            ZIndex = -1000,
        };
        Grid.SetRow(borderVideo, 0);
        contentGrid.Children.Add(borderVideo);

        // Top section
        var topContent = new Border
        {
            Child = InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm)
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Horizontal splitter
        var splitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom section
        var bottomContent = new Border
        {
            Child = InitWaveform.MakeWaveform(vm),
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = double.NaN,
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(contentGrid);

        return 6;
    }

    private static int MakeLayout7(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left section
        var leftContent = new Border
        {
            Child = InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm)
        };
        Grid.SetColumn(leftContent, 0);
        contentGrid.Children.Add(leftContent);

        // Vertical splitter
        var splitter = new GridSplitter
        {
            Width = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Right section
        var rightContent = new Border
        {
            Child = InitVideoPlayer.MakeLayoutVideoPlayer(vm),
        };
        Grid.SetColumn(rightContent, 2);
        contentGrid.Children.Add(rightContent);

        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(contentGrid);

        return 7;
    }

    private static int MakeLayout8(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top section
        var topContent = new Border
        {
            Child = InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm)
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Horizontal splitter
        var splitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom section
        var bottomContent = new Border
        {
            Child = InitVideoPlayer.MakeLayoutVideoPlayer(vm),
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(contentGrid);

        return 8;
    }

    private static int MakeLayout9(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top section
        var topContent = new Border
        {
            Child = InitVideoPlayer.MakeLayoutVideoPlayer(vm),
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Horizontal splitter
        var splitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom section
        var bottomContent = new Border
        {
            Child = InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm)
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(contentGrid);

        return 9;
    }

    private static int MakeLayout10(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top content (will hold the nested grid)
        var topContent = new Border();
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Create a nested grid with columns (for vertical split)
        var nestedGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left part of nested grid
        var nestedLeft = new Border
        {
            Child = InitVideoPlayer.MakeLayoutVideoPlayer(vm),
        };
        Grid.SetColumn(nestedLeft, 0);
        nestedGrid.Children.Add(nestedLeft);

        // Vertical GridSplitter
        var nestedSplitter = new GridSplitter
        {
            Width = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(nestedSplitter, 1);
        nestedGrid.Children.Add(nestedSplitter);

        // Right part of nested grid
        var nestedRight = new Border
        {
            Child = InitWaveform.MakeWaveform(vm),
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = double.NaN,
        };
        Grid.SetColumn(nestedRight, 2);
        nestedGrid.Children.Add(nestedRight);

        // Add nested grid to top content
        topContent.Child = nestedGrid;

        // Main GridSplitter (horizontal)
        var splitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom content
        var bottomContent = new Border
        {
            Child = InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm)
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(contentGrid);

        return 10;
    }

    private static int MakeLayout11(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top section
        var topContent = new Border
        {
            Child = InitVideoPlayer.MakeLayoutVideoPlayer(vm),
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // First horizontal splitter (between top and middle)
        var firstSplitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(firstSplitter, 1);
        contentGrid.Children.Add(firstSplitter);

        // Middle section
        var middleContent = new Border
        {
            Child = InitWaveform.MakeWaveform(vm),
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = double.NaN,
        };
        Grid.SetRow(middleContent, 2);
        contentGrid.Children.Add(middleContent);

        // Second horizontal splitter (between middle and bottom)
        var secondSplitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(secondSplitter, 3);
        contentGrid.Children.Add(secondSplitter);

        // Bottom section
        var bottomContent = new Border
        {
            Child = InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm)
        };
        Grid.SetRow(bottomContent, 4);
        contentGrid.Children.Add(bottomContent);

        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(contentGrid);

        return 11;
    }

    private static int MakeLayout12(MainView mainPage, MainViewModel vm)
    {
        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm));

        // hide video player
        if (vm.VideoPlayerControl != null)
        {
            vm.VideoPlayerControl.RemoveControlFromParent();
            vm.ContentGrid.Children.Add(vm.VideoPlayerControl);
            vm.VideoPlayerControl.IsVisible = false;
        }

        return 12;
    }

    internal static void MakeLayout12KeepVideo(MainView mainPage, MainViewModel vm)
    {
        CleanupOldContent(vm.ContentGrid);
        vm.ContentGrid.Children.Add(InitListViewAndEditBox.MakeLayoutListViewAndEditBox(mainPage, vm));
    }

    internal static bool LayoutHasNoVideo(int layoutNumber)
    {
        return layoutNumber == LayoutWithoutVideo;
    }

    private static void CleanupOldContent(Grid contentGrid)
    {
        // Recursively dispose controls and unhook events
        foreach (var child in contentGrid.Children)
        {
            CleanupControl(child);
        }

        contentGrid.Children.Clear();
    }

    private static void CleanupControl(Control? control)
    {
        if (control == null)
        {
            return;
        }

        // Handle AudioVisualizer - clear specific references
        if (control is AudioVisualizer audioVisualizer)
        {
            audioVisualizer.MenuFlyout = new MenuFlyout();
        }
        // Handle DataGrid - clear data bindings and sources
        else if (control is DataGrid dataGrid)
        {
            // Clear data bindings to prevent event handlers from being retained
            dataGrid.ItemsSource = null;
            dataGrid.SelectedItem = null;
            dataGrid.SelectedItems?.Clear();
            dataGrid.Columns.Clear();
            dataGrid.ContextFlyout = null;
        }
        // Handle TextBox - clear event handlers
        else if (control is TextBox textBox)
        {
            textBox.TextChanged -= null;
            textBox.GotFocus -= null;
            textBox.PointerReleased -= null;
        }
        // Handle Panels (Grid, StackPanel, etc.)
        else if (control is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                CleanupControl(child);
            }
            panel.Children.Clear();
        }
        // Handle Borders
        else if (control is Border border)
        {
            CleanupControl(border.Child);
            border.Child = null;
        }
        // Handle ContentControl (Button, etc.)
        else if (control is ContentControl contentControl)
        {
            if (contentControl.Content is Control childControl)
            {
                CleanupControl(childControl);
            }
            contentControl.Content = null;
        }

        // Dispose if the control implements IDisposable
        if (control is IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
    }
}