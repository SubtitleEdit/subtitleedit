using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ManualChosenEncoding;

public class ManualChosenEncodingWindow : Window
{
    public ManualChosenEncodingWindow(ManualChosenEncodingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.ManualChosenEncoding.Title;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var searchBox = EditorChrome.AsCenteredTextField(new TextBox
        {
            PlaceholderText = Se.Language.File.ManualChosenEncoding.SearchEncodings,
            Width = 240,
        }.WithSearchAndClearIcons()).WithAccessibleName(Se.Language.General.Search);
        searchBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm });
        searchBox.TextChanged += (s, e) => vm.SearchTextChanged();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var labelCurrentEncoding = EditorChrome.MakeDimText();
        labelCurrentEncoding.DataContext = vm;
        labelCurrentEncoding.Bind(TextBlock.TextProperty, new Binding(nameof(vm.CurrentEncodingText)));

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)) { MinHeight = 140 },
                new RowDefinition(new GridLength(1, GridUnitType.Star)) { MinHeight = 120 },
            },
        };
        sections.Add(EditorChrome.MakeListSection(Se.Language.General.Encoding, MakeEncodingsView(vm), searchBox, first: true), 0, 0);
        sections.Add(EditorChrome.MakeFillSection(Se.Language.General.Preview, MakePreviewBox(vm)), 1, 0);

        Content = EditorChrome.MakeCancelOkRoot(sections, labelCurrentEncoding, buttonCancel, buttonOk);

        UiUtil.FocusOnFirstActivation(this, searchBox); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static TableView MakeEncodingsView(ManualChosenEncodingViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.General.Encoding);
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Encodings;

        var columnCodePage = new SeTableViewColumn
        {
            Header = Se.Language.File.ManualChosenEncoding.CodePage,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding($"{nameof(TextEncoding.Encoding)}.{nameof(System.Text.Encoding.CodePage)}"),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(100),
        };
        var columnName = new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(TextEncoding.DisplayName)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        var columnGroup = new SeTableViewColumn
        {
            Header = Se.Language.General.Group,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding($"{nameof(TextEncoding.Encoding)}.{nameof(System.Text.Encoding.BodyName)}"),
            Width = new GridLength(180),
        };
        dataGrid.Columns.AddRange(new TableViewColumn[] { columnCodePage, columnName, columnGroup });

        // An encoding pick list whose order is presentation-only (the caller consumes
        // just SelectedEncoding), so header sorting is safe to wire.
        static string GetBodyName(TextEncoding encoding)
        {
            try
            {
                return encoding.Encoding.BodyName;
            }
            catch
            {
                return string.Empty; // some code pages have no body name
            }
        }

        var sorter = new TableViewHeaderSorter(dataGrid);
        sorter.AddSortable<TextEncoding, int>(columnCodePage, x => x.Encoding.CodePage)
            .AddSortable<TextEncoding, string>(columnName, x => x.DisplayName)
            .AddSortable<TextEncoding, string>(columnGroup, GetBodyName);

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedEncoding)) { Source = vm });
        dataGrid.SelectionChanged += vm.EncodingChanged;

        return dataGrid;
    }

    private static TextBox MakePreviewBox(ManualChosenEncodingViewModel vm)
    {
        var textBox = EditorChrome.AsTextArea(new TextBox
        {
            AcceptsReturn = true,
            // Read-only: a typed tab could never land here anyway, and accepting it only
            // swallowed Tab/Shift+Tab so the box could not be left from the keyboard (#14313).
            AcceptsTab = false,
            IsReadOnly = true,
            Width = double.NaN,
            Height = double.NaN,
        }).WithAccessibleName(Se.Language.General.Preview);
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PreviewText)) { Source = vm, Mode = BindingMode.OneWay });
        return textBox;
    }
}
