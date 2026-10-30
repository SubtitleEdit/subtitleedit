using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Files.ExportDialogueList;

/// <summary>
/// Export Excel dialogue list, in the editor look (see <see cref="EditorChrome"/>): the columns to
/// include (tick + reorder) and the options on the left, a preview of the first rows on the right,
/// Done and Save as in the button bar.
/// </summary>
public class ExportDialogueListWindow : Window
{
    private readonly ExportDialogueListViewModel _vm;
    private readonly Grid _previewGrid;

    public ExportDialogueListWindow(ExportDialogueListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.TitleExportDialogueList;
        CanResize = true;
        Width = 1000;
        Height = 700;
        MinWidth = 700;
        MinHeight = 450;
        _vm = vm;
        vm.Window = this;
        DataContext = vm;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);

        var buttonSaveAs = UiUtil.MakeButton(Se.Language.General.SaveDotDotDot, vm.SaveAsCommand);
        buttonSaveAs.Classes.Add("primary");
        buttonSaveAs.IsDefault = true;
        var buttonDone = UiUtil.MakeButtonDone(vm.CancelCommand);

        _previewGrid = new Grid { Margin = new Thickness(4) };

        var sections = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto, MinWidth = 320 },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        sections.Add(MakeSettingsView(vm, out var listBox), 0, 0);
        sections.Add(MakePreviewView(), 0, 1);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonDone, buttonSaveAs));

        vm.PreviewChanged += UpdatePreview;
        Closing += (_, _) => vm.PreviewChanged -= UpdatePreview;
        Opened += (_, _) => UpdatePreview();

        UiUtil.FocusOnFirstActivation(this, listBox); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.OnKeyDown;
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
    }

    private static ScrollViewer MakeSettingsView(ExportDialogueListViewModel vm, out ListBox listBox)
    {
        listBox = new ListBox { Height = 330, MinWidth = 210 };
        listBox.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Columns)));
        listBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedColumn)) { Mode = BindingMode.TwoWay });
        AutomationProperties.SetName(listBox, Se.Language.General.Columns);
        listBox.ItemTemplate = new FuncDataTemplate<DialogueListColumnItem>((_, _) =>
        {
            var checkBox = new CheckBox { MinWidth = 0 };
            checkBox.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(DialogueListColumnItem.IsChecked)) { Mode = BindingMode.TwoWay });
            checkBox.Bind(IsEnabledProperty, new Binding(nameof(DialogueListColumnItem.IsAvailable)));
            // A check box with no content is announced as a nameless "check box" (#12087).
            checkBox.Bind(AutomationProperties.NameProperty, new Binding(nameof(DialogueListColumnItem.Name)));

            var textBlock = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(DialogueListColumnItem.Name)));
            textBlock.Bind(OpacityProperty, new Binding(nameof(DialogueListColumnItem.IsChecked))
            {
                Converter = new Avalonia.Data.Converters.FuncValueConverter<bool, double>(v => v ? 1.0 : 0.5),
            });

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { checkBox, textBlock },
            };
        }, true);

        var moveButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Margin = new Thickness(10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                EditorChrome.MakeTextToolButton(IconNames.ArrowUpThin, Se.Language.General.MoveUp, vm.MoveUpCommand),
                EditorChrome.MakeTextToolButton(IconNames.ArrowDownThin, Se.Language.General.MoveDown, vm.MoveDownCommand),
            },
        };

        var columnsGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        columnsGrid.Add(EditorChrome.MakeCompactListFrame(listBox), 0, 0);
        columnsGrid.Add(moveButtons, 0, 1);

        var comboBoxTimeCodeFormat = UiUtil.MakeComboBox(vm.TimeCodeFormats, vm, null)
            .WithMinWidth(180)
            .WithAccessibleName(Se.Language.General.TimeCodes);
        comboBoxTimeCodeFormat.Bind(SelectingItemsControl.SelectedIndexProperty, new Binding(nameof(vm.SelectedTimeCodeFormatIndex)) { Mode = BindingMode.TwoWay });
        var gridTimeCodes = EditorChrome.MakeFieldGrid((Se.Language.General.Format, comboBoxTimeCodeFormat));

        var rowLineBreaks = EditorChrome.MakeToggleRow(nameof(vm.LineBreaksInCell), Se.Language.File.Export.DialogueListLineBreaksInCell, out _);
        var rowRemoveFormatting = EditorChrome.MakeToggleRow(nameof(vm.RemoveFormatting), Se.Language.General.RemoveFormatting, out _);
        var rowHeader = EditorChrome.MakeToggleRow(nameof(vm.IncludeHeader), Se.Language.File.Export.DialogueListHeaderRow, out _);

        static StackPanel Stack(params Control[] controls)
        {
            var panel = new StackPanel { Spacing = 8 };
            panel.Children.AddRange(controls);
            return panel;
        }

        var sections = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.Columns, columnsGrid, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.TimeCodes, Stack(gridTimeCodes)),
                EditorChrome.MakeEditorSection(Se.Language.General.Text, Stack(rowLineBreaks, rowRemoveFormatting, rowHeader)),
            },
        };

        // Scrolls on short screens, so the button bar stays reachable
        return new ScrollViewer
        {
            Content = sections,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
    }

    private Border MakePreviewView()
    {
        var scrollViewer = new ScrollViewer
        {
            Content = _previewGrid,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
        AutomationProperties.SetName(scrollViewer, Se.Language.General.Preview);
        return EditorChrome.MakeFillSection(Se.Language.General.Preview, scrollViewer, lineLeft: true);
    }

    /// <summary>
    /// Rebuilds the preview: a spreadsheet-like grid of the header and the first rows.
    /// </summary>
    internal void UpdatePreview()
    {
        var (headers, rows) = _vm.GetPreview();
        var includeHeader = _vm.IncludeHeader;
        _previewGrid.Children.Clear();
        _previewGrid.ColumnDefinitions.Clear();
        _previewGrid.RowDefinitions.Clear();

        for (var c = 0; c < headers.Count; c++)
        {
            _previewGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }

        var rowIndex = 0;
        if (includeHeader)
        {
            _previewGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var c = 0; c < headers.Count; c++)
            {
                AddCell(headers[c], rowIndex, c, isHeader: true);
            }

            rowIndex++;
        }

        foreach (var row in rows)
        {
            _previewGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var c = 0; c < headers.Count && c < row.Count; c++)
            {
                AddCell(FormatCell(row[c]), rowIndex, c, isHeader: false);
            }

            rowIndex++;
        }
    }

    private static string FormatCell(object? value) => value switch
    {
        null => string.Empty,
        double d => d.ToString(CultureInfo.CurrentCulture),
        IFormattable f => f.ToString(null, CultureInfo.CurrentCulture),
        _ => value.ToString() ?? string.Empty,
    };

    private void AddCell(string text, int row, int column, bool isHeader)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            MaxWidth = 360,
            TextWrapping = TextWrapping.Wrap,
            FontWeight = isHeader ? FontWeight.SemiBold : FontWeight.Normal,
            Margin = new Thickness(6, 3),
        };
        var border = new Border
        {
            Child = textBlock,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(0, 0, 1, 1),
            Background = isHeader ? EditorChrome.PanelBrush : Brushes.Transparent,
        };
        _previewGrid.Add(border, row, column);
    }
}
