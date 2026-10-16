using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.MergeTwoSubtitles;

/// <summary>
/// Merge two subtitles, in the editor look (see <see cref="EditorChrome"/>): the two subtitles side
/// by side, the two ASSA styles and the preview below them (ASSA output only), the output format,
/// Cancel and Merge in the bar at the bottom.
/// </summary>
public class MergeTwoSubtitlesWindow : Window
{
    private TableView? _tableViewSubtitle1;

    public MergeTwoSubtitlesWindow(MergeTwoSubtitlesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.MergeTwoSubtitles.Title;
        CanResize = true;
        Width = 1200;
        Height = 800;
        MinWidth = 950;
        MinHeight = 600;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var listsView = MakeListsView(vm);
        var stylesView = MakeStylesView(vm);

        var buttonOk = UiUtil.MakeButton(Se.Language.Tools.MergeTwoSubtitles.Merge, vm.OkCommand)
            .WithBindEnabled(nameof(vm.IsMergeEnabled));
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // lists
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // styles + preview
            },
        };
        sections.Add(listsView, 0, 0);
        sections.Add(stylesView, 1, 0);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(MakeFormatRow(vm), buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            // initial focus on an input, not an action button - a focused button clicks on bare Space
            if (_tableViewSubtitle1 != null)
            {
                TableViewExtras.FocusRow(_tableViewSubtitle1);
            }
        });
        KeyDown += vm.KeyDown;

        // Files dropped elsewhere on the window: two files fill both slots, one fills the first empty slot.
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, vm.FileOnDragOver, RoutingStrategies.Bubble);
        AddHandler(DragDrop.DropEvent, (object? _, DragEventArgs e) => vm.FileOnDrop(0, e), RoutingStrategies.Bubble);
    }

    private Grid MakeListsView(MergeTwoSubtitlesViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        grid.Add(MakeOneListView(vm,
            Se.Language.Tools.MergeTwoSubtitles.Subtitle1,
            nameof(vm.Items1),
            nameof(vm.SelectedItem1),
            nameof(vm.File1Display),
            vm.LoadFile1Command,
            1), 0, 0);

        grid.Add(MakeOneListView(vm,
            Se.Language.Tools.MergeTwoSubtitles.Subtitle2,
            nameof(vm.Items2),
            nameof(vm.SelectedItem2),
            nameof(vm.File2Display),
            vm.LoadFile2Command,
            2), 0, 1);

        return grid;
    }

    private Border MakeOneListView(MergeTwoSubtitlesViewModel vm,
        string title,
        string itemsPath,
        string selectedItemPath,
        string fileNamePath,
        CommunityToolkit.Mvvm.Input.IRelayCommand loadCommand,
        int slot)
    {
        var labelTitle = EditorChrome.MakeSectionHeader(title);
        var labelFile = EditorChrome.MakeDimLabel(string.Empty, new Thickness(10, 0, 0, 0)).WithBindText(vm, fileNamePath);
        labelFile.TextTrimming = TextTrimming.CharacterEllipsis;
        var buttonLoad = EditorChrome.MakeTextToolButton("fa-solid fa-folder-open", Se.Language.Tools.MergeTwoSubtitles.LoadFromFile, loadCommand);

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();

        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        _tableViewSubtitle1 ??= dataGrid; // first list created is subtitle 1
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(MergeTwoSubtitlesDisplayItem.Number)),
            Width = new GridLength(60),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.StartTime,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(MergeTwoSubtitlesDisplayItem.StartTime)) { Converter = fullTimeConverter },
            Width = new GridLength(120),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.EndTime,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(MergeTwoSubtitlesDisplayItem.EndTime)) { Converter = fullTimeConverter },
            Width = new GridLength(120),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(MergeTwoSubtitlesDisplayItem.Text)),
            Width = new GridLength(1, GridUnitType.Star),
        });

        dataGrid.Bind(TableView.ItemsSourceProperty, new Binding(itemsPath) { Source = vm });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(selectedItemPath) { Source = vm });
        dataGrid.WithAccessibleName(title);
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0 &&
                (e.Key == Key.Home ? items[0] : items[^1]) is { } target)
            {
                dataGrid.SelectedItem = target;
                dataGrid.ScrollIntoView(target);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        // Title and file name on the left, the load button on the right
        var header = new DockPanel();
        DockPanel.SetDock(buttonLoad, Dock.Right);
        header.Children.Add(buttonLoad);
        DockPanel.SetDock(labelTitle, Dock.Left);
        header.Children.Add(labelTitle);
        header.Children.Add(labelFile);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        grid.Add(header, 0, 0);
        grid.Add(EditorChrome.MakeListFrame(dataGrid, clearListBackground: true), 1, 0);

        // Drop a file onto this side to load it into this slot.
        var border = new Border
        {
            Child = grid,
            Background = Brushes.Transparent, // hit-test the empty parts of the area too
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(slot == 2 ? 1 : 0, 0, 0, 0),
            Padding = new Thickness(14, 10, 14, 14),
        };
        DragDrop.SetAllowDrop(border, true);
        border.AddHandler(DragDrop.DragOverEvent, vm.FileOnDragOver, RoutingStrategies.Bubble);
        border.AddHandler(DragDrop.DropEvent, (object? _, DragEventArgs e) => vm.FileOnDrop(slot, e), RoutingStrategies.Bubble);
        return border;
    }

    private static StackPanel MakeFormatRow(MergeTwoSubtitlesViewModel vm)
    {
        var label = EditorChrome.MakeDimLabel(Se.Language.General.OutputFormat, new Thickness(0));
        var combo = UiUtil.MakeComboBox(vm.OutputFormats, vm, nameof(vm.SelectedOutputFormat))
            .WithMinWidth(260)
            .WithAccessibleName(Se.Language.General.OutputFormat);

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                label,
                combo,
            },
        };
    }

    /// <summary>
    /// Style 1, style 2 and the preview side by side, with a line on top - ASSA output only.
    /// </summary>
    private static Border MakeStylesView(MergeTwoSubtitlesViewModel vm)
    {
        var l = Se.Language.Tools.MergeTwoSubtitles;

        var col1 = MakeStyleColumn(vm, l.Style1,
            nameof(vm.FontName1), nameof(vm.FontSize1),
            nameof(vm.Bold1), nameof(vm.Italic1),
            nameof(vm.PrimaryColor1), nameof(vm.OutlineColor1),
            nameof(vm.OutlineWidth1), nameof(vm.ShadowWidth1),
            nameof(vm.AlignTop1), "AlignGroup1");

        var col2 = MakeStyleColumn(vm, l.Style2,
            nameof(vm.FontName2), nameof(vm.FontSize2),
            nameof(vm.Bold2), nameof(vm.Italic2),
            nameof(vm.PrimaryColor2), nameof(vm.OutlineColor2),
            nameof(vm.OutlineWidth2), nameof(vm.ShadowWidth2),
            nameof(vm.AlignTop2), "AlignGroup2");

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1.4, GridUnitType.Star) }, // the preview scales down to fit
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        grid.Add(EditorChrome.MakeEditorSection(l.Style1, col1, first: true), 0, 0);
        grid.Add(WithLineLeft(EditorChrome.MakeEditorSection(l.Style2, col2, first: true)), 0, 1);
        grid.Add(WithLineLeft(MakePreviewView(vm)), 0, 2);

        var border = new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        };
        border.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsAssaSelected)) { Source = vm });
        return border;
    }

    private static Border WithLineLeft(Control content)
    {
        return new Border
        {
            Child = content,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };
    }

    private static StackPanel MakeStyleColumn(MergeTwoSubtitlesViewModel vm,
        string title,
        string fontNamePath, string fontSizePath,
        string boldPath, string italicPath,
        string primaryColorPath, string outlineColorPath,
        string outlineWidthPath, string shadowWidthPath,
        string alignTopPath, string alignGroupName)
    {
        const double labelWidth = 84;

        var comboFontName = UiUtil.MakeComboBox(vm.FontNames, vm, fontNamePath)
            .WithAccessibleName($"{title} - {Se.Language.General.FontName}");
        comboFontName.HorizontalAlignment = HorizontalAlignment.Stretch;

        var numFontSize = UiUtil.MakeNumericUpDownInt(6, 200, 20, 90, vm, fontSizePath)
            .WithAccessibleName($"{title} - {Se.Language.General.FontSize}");
        numFontSize.Classes.Add("field");

        var checkBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, boldPath);
        var checkItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, italicPath);
        var stylePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children = { checkBold, checkItalic },
        };

        var primaryPicker = UiUtil.MakeColorPickerButton(vm, primaryColorPath, false)
            .WithAccessibleName($"{title} - {Se.Language.General.TextColor}");
        var outlinePicker = UiUtil.MakeColorPickerButton(vm, outlineColorPath, false)
            .WithAccessibleName($"{title} - {Se.Language.General.OutlineColor}");

        var numOutlineWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 20, 90, vm, outlineWidthPath)
            .WithAccessibleName($"{title} - {Se.Language.General.OutlineWidth}");
        numOutlineWidth.Classes.Add("field");

        var numShadowWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 20, 90, vm, shadowWidthPath)
            .WithAccessibleName($"{title} - {Se.Language.General.ShadowWidth}");
        numShadowWidth.Classes.Add("field");

        var radioTop = UiUtil.MakeRadioButton(Se.Language.General.Top, vm, alignTopPath, alignGroupName);
        var radioBottom = new RadioButton
        {
            Content = Se.Language.General.Bottom,
            GroupName = alignGroupName,
            DataContext = vm,
        };
        radioBottom.Bind(RadioButton.IsCheckedProperty, new Binding(alignTopPath)
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
            Converter = InverseBooleanConverter.Instance,
        });
        var alignPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children = { radioTop, radioBottom },
        };

        return new StackPanel
        {
            Spacing = 6,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.FontName, comboFontName, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.FontSize, numFontSize, labelWidth),
                EditorChrome.MakeLabeledRow(string.Empty, stylePanel, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.TextColor, primaryPicker, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.OutlineColor, outlinePicker, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.OutlineWidth, numOutlineWidth, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.ShadowWidth, numShadowWidth, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.Alignment, alignPanel, labelWidth),
            },
        };
    }

    private static Border MakePreviewView(MergeTwoSubtitlesViewModel vm)
    {
        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly, // 480 wide, smaller in a narrow window
        };

        return EditorChrome.MakeEditorSection(Se.Language.General.Preview, image, first: true);
    }
}
