using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using System.Collections;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

public class ExportCustomTextFormatWindow : Window
{
    public ExportCustomTextFormatWindow(ExportCustomTextFormatViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.TitleExportCustomFormat;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var buttonSaveAs = UiUtil.MakeButton(Se.Language.General.SaveDotDotDot, vm.SaveAsCommand)
            .WithBindIsVisible(vm, nameof(vm.IsSaveButtonVisible));
        buttonSaveAs.IsDefault = true; // the dialog's accept button - Enter runs it (#14586)

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        // The accent goes to the accept button: Save as when it is shown, else OK.
        void UpdatePrimary()
        {
            buttonSaveAs.Classes.Set("primary", vm.IsSaveButtonVisible);
            buttonOk.Classes.Set("primary", !vm.IsSaveButtonVisible);
        }

        UpdatePrimary();
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.IsSaveButtonVisible))
            {
                UpdatePrimary();
            }
        };

        var sections = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        sections.Add(MakeFormatsView(vm, out var formatsGrid), 0, 0);
        sections.Add(MakePreviewView(vm), 0, 1);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk, buttonSaveAs));

        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(formatsGrid); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.OnKeyDown;
    }

    private static Border MakeFormatsView(ExportCustomTextFormatViewModel vm, out TableView formatsGrid)
    {
        var dataGrid = TableViewExtras.MakeTableView().WithAccessibleName(Se.Language.File.Export.CustomTextFormats);
        formatsGrid = dataGrid;
        dataGrid.Height = double.NaN; // auto size inside scroll viewer
        dataGrid.ItemsSource = vm.CustomFormats;
        dataGrid.DataContext = vm;

        dataGrid.DoubleTapped += vm.OnCustomFormatGridDoubleTapped;

        // Columns
        // The DataGrid sized the name column to content (Auto); TableView treats Auto
        // as star, so use a fixed width that fits typical format names.
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            Binding = new Binding(nameof(CustomFormatItem.Name)),
            Width = new GridLength(140),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(CustomFormatItem.FormatParagraph)),
            Width = new GridLength(1, GridUnitType.Star), // star sizing to take all available space
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedCustomFormat))
        {
            Source = vm,
            Mode = BindingMode.TwoWay
        });
        dataGrid.SelectionChanged += vm.GridSelectionChanged;
        dataGrid.DoubleTapped += (s, e) => vm.FormatEditCommand.Execute(null);
        dataGrid.KeyDown += (s, e) =>
        {
            var _ = vm.GridKeyDown(e);
        };
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                dataGrid.SelectedItem = target;
                if (target is { } scrollTarget)
                {
                    dataGrid.ScrollIntoView(scrollTarget);
                }

                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        var flyout = new MenuFlyout();
        var deleteMenuItem = new MenuItem
        {
            Header = Se.Language.General.Delete,
            Command = vm.FormatDeleteCommand,
            [!MenuItem.CommandParameterProperty] = new Binding(nameof(vm.SelectedCustomFormat))
            {
                Source = vm
            }
        };
        flyout.Items.Add(deleteMenuItem);
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        var actions = EditorChrome.MakeListActions(EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.New, vm.FormatNewCommand, 26), EditorChrome.CreateToolButton("fa-solid fa-pen", Se.Language.General.Edit, vm.FormatEditCommand, 26), EditorChrome.CreateToolButton("fa-solid fa-trash-can", Se.Language.General.Remove, vm.FormatDeleteCommand, 26));

        return EditorChrome.MakeListSection(Se.Language.File.Export.CustomTextFormats, dataGrid, actions, first: true);
    }

    private static Border MakePreviewView(ExportCustomTextFormatViewModel vm)
    {
        var textBox = EditorChrome.AsTextArea(new TextBox
        {
            AcceptsReturn = true,
            // Read-only preview - see ManualChosenEncodingWindow (#14313).
            AcceptsTab = false,
            IsReadOnly = true,
            Width = double.NaN,
            Height = double.NaN,
        }).WithAccessibleName(Se.Language.General.Preview);
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PreviewText)));

        return EditorChrome.MakeFillSection(Se.Language.General.Preview, textBox, lineLeft: true);
    }
}
