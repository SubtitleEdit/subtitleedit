using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeFormatting;

/// <summary>
/// Change formatting, in the editor look (see <see cref="EditorChrome"/>): from/to (and color) on
/// top, the before/after preview below, the summary plus Cancel and OK in the button bar.
/// </summary>
public class ChangeFormattingWindow : Window
{
    public ChangeFormattingWindow(ChangeFormattingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.ChangeFormatting);
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 900;
        MinHeight = 500;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var comboBoxFrom = UiUtil.MakeComboBox(vm.FromTypes, vm, nameof(vm.SelectedFromType))
            .WithAccessibleName(Se.Language.General.From);
        comboBoxFrom.SelectionChanged += vm.SelectionChanged;

        var comboBoxTo = UiUtil.MakeComboBox(vm.ToTypes, vm, nameof(vm.SelectedToType))
            .WithAccessibleName(Se.Language.General.To);
        comboBoxTo.SelectionChanged += vm.SelectionChanged;

        var colorPicker = UiUtil.MakeColorPickerButton(vm, nameof(vm.SelectedColor))
            .WithAccessibleName(Se.Language.General.Color);

        // The color only matters when converting to color, so its label hides with it.
        var panelColor = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(16, 0, 0, 0),
            Children = { EditorChrome.MakeDimLabel(Se.Language.General.Color, new Thickness(0)), colorPicker },
        };
        panelColor.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsColorVisible)));

        var panelControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            MinHeight = 30,
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.General.From, new Thickness(0)),
                comboBoxFrom,
                EditorChrome.MakeDimLabel(Se.Language.General.To, new Thickness(16, 0, 0, 0)),
                comboBoxTo,
                panelColor,
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeCancelOkRoot(EditorChrome.MakeOptionsAndLists(EditorChrome.MakeEditorSection(Se.Language.General.Convert, panelControls, first: true), MakeSubtitleView(vm)), EditorChrome.MakeStatusText(nameof(vm.StatusText), vm), buttonCancel, buttonOk);

        UiUtil.FocusOnFirstActivation(this, comboBoxFrom); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Border MakeSubtitleView(ChangeFormattingViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var colorConverter = new TextWithSubtitleSyntaxHighlightingConverter();
        // No header-click sorting (the DataGrid's CanUserSortColumns is not carried
        // over): before/after formatting previews in subtitle order.
        var dataGridSubtitle = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.General.Preview);
        dataGridSubtitle.Width = double.NaN;
        dataGridSubtitle.Height = double.NaN;
        dataGridSubtitle.DataContext = vm;
        dataGridSubtitle.ItemsSource = vm.Subtitles;
        dataGridSubtitle.Columns.AddRange(new[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ChangeFormattingDisplayItem.Number)),
                Width = new GridLength(60), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Show,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ChangeFormattingDisplayItem.StartTime)) { Converter = fullTimeConverter },
                Width = new GridLength(115),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Duration,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ChangeFormattingDisplayItem.Duration)) { Converter = shortTimeConverter },
                Width = new GridLength(90),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Before,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Width = new GridLength(1, GridUnitType.Star),
                CellTemplate = new FuncDataTemplate<ChangeFormattingDisplayItem>((value, nameScope) =>
                {
                    var border = new Border
                    {
                        Padding = new Thickness(4, 2),
                    };

                    var textBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.NoWrap,
                        [!TextBlock.InlinesProperty] = new Binding(nameof(ChangeFormattingDisplayItem.Text)) { Converter = colorConverter, Mode = BindingMode.OneWay },
                    };

                    if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                    {
                        textBlock.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                    }

                    border.Child = textBlock;
                    return border;
                })
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.After,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Width = new GridLength(1, GridUnitType.Star),
                CellTemplate = new FuncDataTemplate<ChangeFormattingDisplayItem>((value, nameScope) =>
                {
                    var border = new Border
                    {
                        Padding = new Thickness(4, 2),
                    };

                    var textBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.NoWrap,
                        [!TextBlock.InlinesProperty] = new Binding(nameof(ChangeFormattingDisplayItem.NewText)) { Converter = colorConverter, Mode = BindingMode.OneWay },
                    };

                    if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                    {
                        textBlock.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                    }

                    border.Child = textBlock;
                    return border;
                })
            },
        });

        return EditorChrome.MakeListSection(Se.Language.General.Preview, dataGridSubtitle);
    }
}
