using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ssa;

/// <summary>
/// Fonts and graphics embedded in the SSA script, styled like ASSA draw: actions on top, the list on
/// the left, the preview on the right.
/// </summary>
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
        Height = 820;
        MinWidth = 900;
        MinHeight = 550;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(480)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(MakeLeftView(vm, out var attachmentsGrid), 0, 0);
        content.Add(MakeRightView(vm), 0, 1);

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        mainGrid.Add(MakeToolbar(vm), 0, 0);
        mainGrid.Add(content, 1, 0);
        mainGrid.Add(MakeButtonBar(vm), 2, 0);
        Content = mainGrid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(attachmentsGrid); });
        KeyDown += vm.KeyDown;

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    /// <summary>
    /// Icon button with its label beside it, for the actions bar.
    /// </summary>
    private static Button MakeAction(string icon, string text, System.Windows.Input.ICommand command)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7, Margin = new Thickness(10, 0) };
        content.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 13, VerticalAlignment = VerticalAlignment.Center });
        content.Children.Add(new TextBlock { Text = text, FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button
        {
            Content = content,
            Height = EditorChrome.ScaledSize(30),
            Command = command,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");

        // Icon + label in a panel: a screen reader needs the name spelled out (#12087)
        AutomationProperties.SetName(button, text);
        return button;
    }

    private static Border MakeToolbar(SsaAttachmentsViewModel vm)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(MakeAction("fa-solid fa-paperclip", Se.Language.General.AttachDotDotDot, vm.FileAttachCommand));
        panel.Children.Add(MakeAction("fa-solid fa-file-import", Se.Language.General.ImportDotDotDot, vm.FileImportCommand));
        panel.Children.Add(MakeAction("fa-solid fa-file-export", Se.Language.General.ExportDotDotDot, vm.FileExportCommand));
        panel.Children.Add(EditorChrome.MakeToolbarSeparator());
        panel.Children.Add(MakeAction("fa-solid fa-scissors", Se.Language.Assa.TrimFontsDotDotDot, vm.TrimFontsToUsedCharactersCommand));

        return new Border
        {
            Child = panel,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 6),
        };
    }

    private static Border MakeButtonBar(SsaAttachmentsViewModel vm)
    {
        // "2 fonts · 1 graphics · 120 KB"
        var summary = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        void UpdateSummary()
        {
            var fonts = vm.Attachments.Count(a => a.Category == Se.Language.General.Fonts);
            var bytes = vm.Attachments.Sum(a => (long)a.Bytes.Length);
            summary.Text = string.Format(Se.Language.Assa.AttachmentsSummary, fonts, vm.Attachments.Count - fonts, Utilities.FormatBytesToDisplayFileSize(bytes));
        }

        UpdateSummary();
        vm.Attachments.CollectionChanged += (_, _) => UpdateSummary();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        buttons.Children.Add(EditorChrome.WithoutMargin(buttonCancel));
        buttons.Children.Add(EditorChrome.WithoutMargin(buttonOk));

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        grid.Add(summary, 0, 0);
        grid.Add(buttons, 0, 1);
        return EditorChrome.WrapButtonBar(grid);
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
        AutomationProperties.SetName(dataGrid, Se.Language.Assa.FontsAndGraphics);
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
        dataGrid.AddHandler(InputElement.KeyDownEvent, vm.AttachmentsMoveKeyDown, RoutingStrategies.Tunnel);
        TableViewExtras.AttachListNavigation(dataGrid);
        vm.AttachmentGrid = dataGrid;

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

        StylesDialogHelper.AddMoveMenuItems(flyout, vm, nameof(vm.IsMoveVisible), vm.MoveUpCommand, vm.MoveDownCommand, vm.MoveToTopCommand, vm.MoveToBottomCommand);

        grid.Add(dataGrid, 0);

        // Header: title with move/delete actions, then the list
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-arrow-up", Se.Language.General.MoveUp, vm.MoveUpCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-arrow-down", Se.Language.General.MoveDown, vm.MoveDownCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Delete, vm.AttachmentRemoveCommand, 24));
        var header = new DockPanel { Margin = new Thickness(14, 10, 10, 8) };
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.Assa.FontsAndGraphics));

        var panel = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) } };
        grid.Margin = new Thickness(8, 0);
        panel.Add(header, 0, 0);
        panel.Add(grid, 1, 0);
        return new Border
        {
            Child = panel,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };
    }

    private static Control MakeRightView(SsaAttachmentsViewModel vm)
    {
        var title = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PreviewTitle)),
        };

        // Font actions, only for a font
        var fontActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.IsCopyFontnameToClipboardVisible)),
        };
        fontActions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-copy", Se.Language.General.CopyToClipboard, vm.CopyFontNameToClipboardCommand, 28));
        fontActions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-font", Se.Language.Assa.FontCollectorCopyFontsToSeFontsFolder, vm.CopyToSeFontsFolderCommand, 28));

        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
        DockPanel.SetDock(fontActions, Dock.Right);
        header.Children.Add(fontActions);
        header.Children.Add(title);

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewImage)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
        };
        var stage = new Border
        {
            Child = image,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
        };

        var panel = new Grid { Margin = new Thickness(16, 12), RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) } };
        panel.Add(header, 0, 0);
        panel.Add(stage, 1, 0);
        return panel;
    }
}
