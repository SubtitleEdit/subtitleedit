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

public class ChangeFormattingWindow : Window
{
    public ChangeFormattingWindow(ChangeFormattingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ChangeFormatting;
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 900;
        MinHeight = 500;
        vm.Window = this;
        DataContext = vm;

        var labelFrom = UiUtil.MakeLabel(Se.Language.General.From);
        var comboBoxFrom = UiUtil.MakeComboBox(vm.FromTypes, vm, nameof(vm.SelectedFromType));
        comboBoxFrom.SelectionChanged += vm.SelectionChanged;

        var labelTo = UiUtil.MakeLabel(Se.Language.General.To);
        var comboBoxTo = UiUtil.MakeComboBox(vm.ToTypes, vm, nameof(vm.SelectedToType));
        comboBoxTo.SelectionChanged += vm.SelectionChanged;

        var labelColor = UiUtil.MakeLabel(Se.Language.General.Color);
        var colorPicker = UiUtil.MakeColorPickerButton(vm, nameof(vm.SelectedColor));
        colorPicker.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsColorVisible)));

        var panelControls = UiUtil.MakeHorizontalPanel(
            labelFrom,
            comboBoxFrom,
            labelTo,
            comboBoxTo,
            labelColor,
            colorPicker);

        var subtitleView = MakeSubtitleView(vm);

        var labelStatus = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.StatusText)).WithAlignmentTop();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Bridge gap smaller than
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Subtitle view
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelControls, 0);
        grid.Add(subtitleView, 1);
        grid.Add(labelStatus, 2);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { comboBoxFrom.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
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
        var dataGridSubtitle = TableViewExtras.MakeTableView(multiSelect: false);
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
                        textBlock.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
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
                        textBlock.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                    }

                    border.Child = textBlock;
                    return border;
                })
            },
        });

        return UiUtil.MakeBorderForControlNoPadding(dataGridSubtitle);
    }
}
