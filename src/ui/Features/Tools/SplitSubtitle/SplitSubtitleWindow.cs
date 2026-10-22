using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;

/// <summary>
/// Split subtitle, in the editor look (see <see cref="EditorChrome"/>): output folder/format/encoding
/// and how to split side by side, the parts below, the subtitle info, Cancel and Save at the bottom.
/// </summary>
public class SplitSubtitleWindow : Window
{
    private TextBox _textBoxOutputFolder = null!;

    public SplitSubtitleWindow(SplitSubtitleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.SplitSubtitle.Title);
        CanResize = true;
        Width = 900;
        Height = 700;
        MinWidth = 700;
        MinHeight = 420;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButton(Se.Language.Tools.SplitSubtitle.SaveSplitParts, vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var labelSubtitleInfo = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0)).WithBindText(vm, nameof(vm.SubtitleInfo));
        labelSubtitleInfo.TextTrimming = TextTrimming.CharacterEllipsis;

        var top = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        top.Add(MakeInputOutputView(vm), 0, 0);
        top.Add(new Border
        {
            Child = MakeOptionsView(vm),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // output + split options
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // split items list
            },
        };
        sections.Add(top, 0, 0);
        sections.Add(MakeListView(vm), 1, 0);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(labelSubtitleInfo, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, _textBoxOutputFolder); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private Border MakeInputOutputView(SplitSubtitleViewModel vm)
    {
        const double labelWidth = 100;

        var textBoxOutputFolder = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.OutputFolder))
            .WithAccessibleName(Se.Language.General.OutputFolder);
        textBoxOutputFolder.Classes.Add("text");
        textBoxOutputFolder.HorizontalAlignment = HorizontalAlignment.Stretch;
        _textBoxOutputFolder = textBoxOutputFolder;
        var buttonBrowse = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.PickOutputFolder, vm.BrowseCommand, 28);
        var buttonOpen = EditorChrome.CreateToolButton("fa-solid fa-folder-open", Se.Language.General.OpenOutputFolder, vm.OpenFolderCommand, 28);
        var panelOutputFolder = new Grid
        {
            ColumnSpacing = 4,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        panelOutputFolder.Add(textBoxOutputFolder, 0, 0);
        panelOutputFolder.Add(buttonBrowse, 0, 1);
        panelOutputFolder.Add(buttonOpen, 0, 2);

        var comboBoxFormat = new ComboBox
        {
            Width = 240,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Formats)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedSubtitleFormat)),
            DataContext = vm,
            ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleFormat.Name)),
                    // No fixed width: it cut long names off in the drop-down too, e.g.
                    // "DVD Studio Pro with one space" (#15901). The closed box trims instead.
                    TextTrimming = TextTrimming.CharacterEllipsis,
                }, true)
        }.WithAccessibleName(Se.Language.General.Format);

        var comboBoxEncoding = new ComboBox
        {
            Width = 240,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Encodings)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedEncoding)),
            DataContext = vm,
        }.WithAccessibleName(Se.Language.General.Encoding);

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.OutputFolder, panelOutputFolder, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.Format, comboBoxFormat, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.Encoding, comboBoxEncoding, labelWidth),
            },
            Spacing = 8,
        };

        return EditorChrome.MakeEditorSection(Se.Language.General.OutputProperties, panel, first: true);
    }

    private static Border MakeOptionsView(SplitSubtitleViewModel vm)
    {
        var radioLines = UiUtil.MakeRadioButton(Se.Language.General.Lines, vm, nameof(vm.SplitByLines));
        radioLines.IsCheckedChanged += (s, e) => vm.SetDirty();
        var radioCharacters = UiUtil.MakeRadioButton(Se.Language.General.Characters, vm, nameof(vm.SplitByCharacters));
        radioCharacters.IsCheckedChanged += (s, e) => vm.UpdateSplit();
        var radioTime = UiUtil.MakeRadioButton(Se.Language.General.Time, vm, nameof(vm.SplitByTime));
        radioTime.IsCheckedChanged += (s, e) => vm.SetDirty();
        foreach (var radio in new[] { radioLines, radioCharacters, radioTime })
        {
            radio.FontSize = EditorChrome.FieldFontSize;
        }

        var panelSplitBy = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            Children = { radioLines, radioCharacters, radioTime },
        };

        var numberUpDownParts = UiUtil.MakeNumericUpDownInt(1, vm.PartsMax, 1, 110, vm, nameof(vm.NumberOfEqualParts))
            .WithAccessibleName(Se.Language.Tools.SplitSubtitle.NumberOfEqualParts);
        numberUpDownParts.Classes.Add("field");
        numberUpDownParts.ValueChanged += (s, e) => vm.SetDirty();
        numberUpDownParts.Bind(NumericUpDown.MaximumProperty, new Binding(nameof(vm.PartsMax)) { Source = vm, Mode = BindingMode.OneWay });
        var panelParts = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Margin = new Thickness(0, 6, 0, 0),
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.Tools.SplitSubtitle.NumberOfEqualParts, new Thickness(0)),
                numberUpDownParts,
            },
        };

        var panel = new StackPanel
        {
            MinWidth = 200,
            Children = { panelSplitBy, panelParts },
        };

        return EditorChrome.MakeEditorSection(Se.Language.Tools.SplitSubtitle.SplitBy, panel, first: true);
    }

    private Control MakeListView(SplitSubtitleViewModel vm)
    {
        // Sorting dropped in the DataGrid -> TableView conversion: the list shows the
        // split parts in output order (part 1, part 2, ...).
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.General.Preview);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.SplitItems;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NoSymbolLines,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(SplitDisplayItem.Lines)),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(80),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Characters,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(SplitDisplayItem.Characters)),
                    Width = new GridLength(100),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.FileName,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(SplitDisplayItem.FileName)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSpiltItem)) { Source = vm });
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var index = e.Key == Key.Home ? 0 : items.Count - 1;
                dataGrid.SelectedIndex = index;
                dataGrid.ScrollIntoView(index);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        // The list fills the rest of the window, so the section header and list are laid out
        // in a grid rather than in a section's stack panel.
        var content = new Grid
        {
            RowSpacing = 10,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        content.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Preview), 0, 0);
        content.Add(EditorChrome.MakeListFrame(dataGrid, clearListBackground: true), 1, 0);
        return new Border
        {
            Child = content,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }
}
