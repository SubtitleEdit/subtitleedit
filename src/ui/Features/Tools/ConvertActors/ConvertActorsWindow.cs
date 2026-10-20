using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.ConvertActors;

/// <summary>
/// Convert actors, in the editor look (see <see cref="EditorChrome"/>): from/to and the options
/// side by side, the preview below, the summary plus Cancel and OK in the button bar.
/// </summary>
public class ConvertActorsWindow : Window
{
    public ConvertActorsWindow(ConvertActorsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.ConvertActors.Title);
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 900;
        MinHeight = 500;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var comboBoxFrom = UiUtil.MakeComboBox(vm.FromTypes, vm, nameof(vm.SelectedFromType))
            .WithAccessibleName(Se.Language.Tools.ConvertActors.ConvertActorFrom);
        comboBoxFrom.SelectionChanged += vm.SelectionChanged;

        var comboBoxTo = UiUtil.MakeComboBox(vm.ToTypes, vm, nameof(vm.SelectedToType))
            .WithAccessibleName(Se.Language.Tools.ConvertActors.ConvertActorTo);
        comboBoxTo.SelectionChanged += vm.SelectionChanged;

        var rowSetColor = EditorChrome.MakeToggleRow(nameof(vm.SetColor), Se.Language.Tools.ConvertActors.SetColor, out _);
        var colorPicker = UiUtil.MakeColorPickerButton(vm, nameof(vm.SelectedColor))
            .WithAccessibleName(Se.Language.General.Color);
        colorPicker.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.SetColor)) { Source = vm, Mode = BindingMode.OneWay });
        colorPicker.Margin = new Thickness(4, 0, 0, 0);
        rowSetColor.Children.Add(colorPicker);

        var rowChangeCasing = EditorChrome.MakeToggleRow(nameof(vm.ChangeCasing), Se.Language.General.ChangeCasing, out _);
        var comboBoxCasing = new ComboBox
        {
            ItemsSource = vm.CasingOptions,
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 0, 0),
        }.WithAccessibleName(Se.Language.General.Casing);
        comboBoxCasing.Bind(ComboBox.SelectedIndexProperty, new Binding(nameof(vm.SelectedCasingIndex)) { Mode = BindingMode.TwoWay });
        comboBoxCasing.Bind(ComboBox.IsVisibleProperty, new Binding(nameof(vm.IsCasingVisible)) { Source = vm, Mode = BindingMode.OneWay });
        comboBoxCasing.SelectionChanged += vm.SelectionChanged;
        rowChangeCasing.Children.Add(comboBoxCasing);

        var rowOnlyNames = EditorChrome.MakeToggleRow(nameof(vm.OnlyNames), Se.Language.Tools.ConvertActors.OnlyNames, out _);

        // The color and casing pickers show up next to their switch, so the rows keep a fixed
        // height and the list below does not jump.
        foreach (var row in new[] { rowSetColor, rowChangeCasing, rowOnlyNames })
        {
            row.MinHeight = 30;
        }

        var panelConversion = EditorChrome.MakeFieldGrid((Se.Language.Tools.ConvertActors.ConvertActorFrom, comboBoxFrom), (Se.Language.Tools.ConvertActors.ConvertActorTo, comboBoxTo));

        var panelOptions = new StackPanel
        {
            Spacing = 4,
            Children = { rowSetColor, rowChangeCasing, rowOnlyNames },
        };

        var panelControls = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        panelControls.Add(EditorChrome.MakeEditorSection(Se.Language.General.Convert, panelConversion, first: true), 0, 0);
        panelControls.Add(new Border
        {
            Child = EditorChrome.MakeEditorSection(Se.Language.General.Options, panelOptions, first: true),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeCancelOkRoot(EditorChrome.MakeOptionsAndLists(panelControls, MakeSubtitleView(vm)), EditorChrome.MakeStatusText(nameof(vm.StatusText), vm), buttonCancel, buttonOk);

        UiUtil.FocusOnFirstActivation(this, comboBoxFrom); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Border MakeSubtitleView(ConvertActorsViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var colorConverter = new TextWithSubtitleSyntaxHighlightingConverter();

        // Sorting dropped in the DataGrid -> TableView conversion: the grid previews
        // subtitle lines in timeline order.
        var dataGrid = TableViewExtras.MakeTableView().WithAccessibleName(Se.Language.General.Preview);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Subtitles;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Width = new GridLength(55),
                    CellTemplate = new FuncDataTemplate<ConvertActorsDisplayItem>((_, _) =>
                        new CheckBox
                        {
                            Focusable = false,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            [!CheckBox.IsCheckedProperty] = new Binding(nameof(ConvertActorsDisplayItem.IsChecked)) { Mode = BindingMode.TwoWay },
                        }),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(ConvertActorsDisplayItem.Number)),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(ConvertActorsDisplayItem.StartTime)) { Converter = fullTimeConverter },
                    Width = new GridLength(120),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Width = new GridLength(1, GridUnitType.Star),
                    CellTemplate = new FuncDataTemplate<ConvertActorsDisplayItem>((_, _) =>
                    {
                        var border = new Border { Padding = new Thickness(4, 2) };
                        var textBlock = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            TextWrapping = TextWrapping.NoWrap,
                            [!TextBlock.InlinesProperty] = new Binding(nameof(ConvertActorsDisplayItem.Text)) { Converter = colorConverter, Mode = BindingMode.OneWay },
                        };
                        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                        {
                            textBlock.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                        }
                        border.Child = textBlock;
                        return border;
                    }),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Width = new GridLength(1, GridUnitType.Star),
                    CellTemplate = new FuncDataTemplate<ConvertActorsDisplayItem>((_, _) =>
                    {
                        var border = new Border { Padding = new Thickness(4, 2) };
                        var textBlock = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            TextWrapping = TextWrapping.NoWrap,
                            [!TextBlock.InlinesProperty] = new Binding(nameof(ConvertActorsDisplayItem.NewText)) { Converter = colorConverter, Mode = BindingMode.OneWay },
                        };
                        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                        {
                            textBlock.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                        }
                        border.Child = textBlock;
                        return border;
                    }),
                },
        });

        // Extended selection is native ListBox behavior on TableView; only the
        // Space-toggles-checkbox piece of the old CheckboxMultiSelect needs wiring.
        TableViewExtras.AddSpaceToggle<ConvertActorsDisplayItem>(dataGrid,
            item => item.IsChecked, (item, v) => item.IsChecked = v);

        return EditorChrome.MakeListSection(Se.Language.General.Preview, dataGrid);
    }
}
