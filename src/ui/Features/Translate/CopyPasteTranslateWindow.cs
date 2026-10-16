using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// Translate via copy-paste in the editor look (see <see cref="EditorChrome"/>): block size and line
/// separator in a toolbar, the lines in the middle, Translate, Cancel and OK in the button bar.
/// </summary>
public class CopyPasteTranslateWindow : Window
{
    public CopyPasteTranslateWindow(CopyPasteTranslateViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Translate.TranslateViaCopyPaste);
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var buttonTranslate = UiUtil.MakeButton(Se.Language.General.Translate, vm.TranslateCommand);
        buttonTranslate.Classes.Add("primary");
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonTranslate, buttonCancel, buttonOk);

        var listFrame = MakeSubtitlesView(vm);
        listFrame.Margin = new Thickness(14, 12);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
        };
        grid.Add(MakeControlsView(vm), 0, 0);
        grid.Add(EditorChrome.MakePanel(listFrame), 1, 0);
        grid.Add(buttonBar, 2, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(vm.SubtitleGrid); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    /// <summary>
    /// Block size and line separator in a toolbar on top.
    /// </summary>
    private static Border MakeControlsView(CopyPasteTranslateViewModel vm)
    {
        var numericUpDownMaxBlockSize = UiUtil.MakeNumericUpDownInt(100, 500_000, 5000, 110, vm, nameof(vm.MaxBlockSize));
        numericUpDownMaxBlockSize.Classes.Add("field");
        AutomationProperties.SetName(numericUpDownMaxBlockSize, Se.Language.Translate.MaxBlockSize);

        var textBoxLineSeparator = new TextBox
        {
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center,
        };
        textBoxLineSeparator.Classes.Add("text");
        AutomationProperties.SetName(textBoxLineSeparator, Se.Language.Translate.LineSeparator);
        textBoxLineSeparator.Bind(TextBox.TextProperty, new Binding(nameof(vm.LineSeparator))
        {
            Source = vm,
            Mode = BindingMode.TwoWay
        });

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.Translate.MaxBlockSize, new Thickness(4, 0, 0, 0)),
                numericUpDownMaxBlockSize,
                EditorChrome.MakeToolbarSeparator(),
                EditorChrome.MakeDimLabel(Se.Language.Translate.LineSeparator, new Thickness(0)),
                textBoxLineSeparator,
            }
        };

        var toolbar = EditorChrome.MakeToolbar(panel);
        toolbar.Padding = new Thickness(12, 8);
        return toolbar;
    }

    private static Border MakeSubtitlesView(CopyPasteTranslateViewModel vm)
    {
        vm.SubtitleGrid = TableViewExtras.MakeTableView(multiSelect: false);
        vm.SubtitleGrid.Height = double.NaN; // auto size inside scroll viewer
        AutomationProperties.SetName(vm.SubtitleGrid, Se.Language.General.Lines);
        vm.SubtitleGrid.ItemsSource = vm.Subtitles;
        vm.SubtitleGrid.DataContext = vm.Subtitles;

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
            Width = new GridLength(50),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
            Width = new GridLength(120),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
            Width = new GridLength(120),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.OriginalText)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Translation,
            CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        vm.SubtitleGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle))
        {
            Source = vm,
            Mode = BindingMode.TwoWay
        });

        return EditorChrome.MakeListFrame(vm.SubtitleGrid);
    }
}
