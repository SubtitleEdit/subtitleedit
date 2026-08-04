using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ssa;

public class SsaAttachmentsWindow : Window
{
    public SsaAttachmentsWindow(SsaAttachmentsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        CanResize = true;
        Width = 1200;
        Height = 850;
        MinWidth = 1100;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelFontsAndImages = UiUtil.MakeLabel(Se.Language.Assa.FontsAndGraphics);
        var labelPreview = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.PreviewTitle));
        var buttonCopyToClipboard = UiUtil.MakeButton(Se.Language.General.CopyToClipboard, vm.CopyFontNameToClipboardCommand)
            .WithBindIsVisible(nameof(vm.IsCopyFontnameToClipboardVisible));
        var previewLine = UiUtil.MakeHorizontalPanel(labelPreview, buttonCopyToClipboard);

        var buttonAttach = UiUtil.MakeButton(Se.Language.General.AttachDotDotDot, vm.FileAttachCommand);
        var buttonImport = UiUtil.MakeButton(Se.Language.General.ImportDotDotDot, vm.FileImportCommand);
        var buttonExport = UiUtil.MakeButton(Se.Language.General.ExportDotDotDot, vm.FileExportCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonAttach, buttonImport, buttonExport, buttonOk, buttonCancel);

        grid.Add(labelFontsAndImages, 0);
        grid.Add(previewLine, 0, 1);
        grid.Add(MakeLeftView(vm, out var attachmentsGrid), 1);
        grid.Add(MakeRightView(vm), 1, 1);
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        Activated += delegate { TableViewExtras.FocusRow(attachmentsGrid); };
        KeyDown += vm.KeyDown;

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Border MakeLeftView(SsaAttachmentsViewModel vm, out TableView tableView)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        // No header sorting: the attachment order is written back to the subtitle
        // footer ([Fonts]/[Graphics] sections) in list order on OK, so the collection
        // order is not presentation-only.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        tableView = dataGrid;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Attachments;

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.FileName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SsaAttachmentItem.FileName)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Type,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SsaAttachmentItem.Category)),
            Width = new GridLength(140),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Size,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SsaAttachmentItem.Size)),
            Width = new GridLength(100),
        });

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedAttachment)) { Source = vm });
        dataGrid.SelectionChanged += vm.DataGridSelectionChanged;
        dataGrid.KeyDown += vm.AttachmentsDataGridKeyDown;
        TableViewExtras.AttachListNavigation(dataGrid);

        var flyout = new MenuFlyout();
        flyout.Opening += vm.AttachmentsContextMenuOpening;
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.AttachmentRemoveCommand,
        };
        menuItemDelete.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteVisible)) { Source = vm });
        flyout.Items.Add(menuItemDelete);

        var menuItemClear = new MenuItem
        {
            Header = Se.Language.General.Clear,
            DataContext = vm,
            Command = vm.AttachemntsRemoveAllCommand,
        };
        menuItemClear.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteAllVisible)) { Source = vm });
        flyout.Items.Add(menuItemClear);

        grid.Add(dataGrid, 0);

        return UiUtil.MakeBorderForControlNoPadding(grid);
    }

    private static Border MakeRightView(SsaAttachmentsViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewImage)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
        };

        grid.Add(image, 0);

        return UiUtil.MakeBorderForControl(grid);
    }
}
