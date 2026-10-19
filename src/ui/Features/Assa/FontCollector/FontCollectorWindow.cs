using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.FontCollector;

/// <summary>
/// Fonts used by the subtitle, installed fonts and SE's collected fonts - styled like ASSA draw:
/// view switcher and actions on top, the list, the sample-text preview under it, status in the
/// button bar.
/// </summary>
public class FontCollectorWindow : Window
{
    private static readonly IBrush FoundBrush = new ImmutableSolidColorBrush(Color.FromRgb(46, 160, 90));
    private static readonly IBrush NotFoundBrush = new ImmutableSolidColorBrush(Color.FromRgb(220, 70, 70));

    private readonly FontCollectorViewModel _vm;

    public FontCollectorWindow(FontCollectorViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.FontCollectorTitle;
        CanResize = true;
        Width = 960;
        Height = 680;
        MinWidth = 700;
        MinHeight = 480;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        // One list per view; only the selected one shows
        var lists = new Panel();
        lists.Children.Add(OnTab(vm, 0, MakeCurrentSubtitleList(vm)));
        lists.Children.Add(OnTab(vm, 1, MakeInstalledFontsList(vm)));
        lists.Children.Add(OnTab(vm, 2, MakeCollectedFontsList(vm)));

        var content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(new GridLength(190)),
            },
        };
        content.Add(new Border
        {
            Child = lists,
            Background = EditorChrome.PanelBrush,
            Padding = new Thickness(8),
        }, 0, 0);
        content.Add(MakePreview(vm), 1, 0);

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
    }

    private static T OnTab<T>(FontCollectorViewModel vm, int index, T control) where T : Control
    {
        control.Bind(IsVisibleProperty, new Binding(nameof(vm.SelectedTabIndex))
        {
            Source = vm,
            Converter = new FuncValueConverter<int, bool>(selected => selected == index),
        });
        return control;
    }

    /// <summary>
    /// Icon button with its label beside it, for the toolbar.
    /// </summary>
    private static Button MakeAction(string icon, string text, System.Windows.Input.ICommand? command)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7, Margin = new Thickness(10, 0) };
        content.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 13, VerticalAlignment = VerticalAlignment.Center });
        content.Children.Add(new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button
        {
            Content = content,
            Height = 30,
            Command = command,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");

        // Icon + label in a panel: a screen reader needs the name spelled out (#12087)
        AutomationProperties.SetName(button, text);
        return button;
    }

    private static Border MakeToolbar(FontCollectorViewModel vm)
    {
        // View switcher: the active view lit
        var views = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        foreach (var (index, icon, text) in new[]
                 {
                     (0, "fa-solid fa-closed-captioning", Se.Language.Assa.FontCollectorCurrentSubtitle),
                     (1, "fa-solid fa-font", Se.Language.Tools.PickFontNameInstalledFonts),
                     (2, "fa-solid fa-folder", Se.Language.Tools.PickFontNameCollectedFonts),
                 })
        {
            var button = MakeAction(icon, text, null);
            button.Click += (_, _) => vm.SelectedTabIndex = index;

            void Update() => button.Classes.Set("active", vm.SelectedTabIndex == index);
            Update();
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.SelectedTabIndex))
                {
                    Update();
                }
            };
            views.Children.Add(button);
        }

        // Actions for the view on show
        var subtitleActions = OnTab(vm, 0, new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 });
        subtitleActions.Children.Add(MakeAction("fa-solid fa-paperclip", Se.Language.Assa.FontCollectorEmbedFontsDotDotDot, vm.EmbedFontsInSubtitleCommand));
        subtitleActions.Children.Add(MakeAction("fa-solid fa-copy", Se.Language.Assa.FontCollectorCopyFontsToSeFontsFolder, vm.CopyFontsToSeFontsFolderCommand));

        var installedActions = OnTab(vm, 1, new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 });
        installedActions.Children.Add(MakeAction("fa-solid fa-copy", Se.Language.Assa.FontCollectorCopyFontsToSeFontsFolder, vm.CopyInstalledFontToSeFontsFolderCommand));

        var collectedActions = OnTab(vm, 2, new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 });
        collectedActions.Children.Add(MakeAction("fa-solid fa-file-import", Se.Language.Assa.FontCollectorImportFontDotDotDot, vm.ImportFontCommand));
        collectedActions.Children.Add(MakeAction("fa-solid fa-folder-open", Se.Language.Assa.FontCollectorOpenFontsFolder, vm.OpenSeFontsFolderCommand));

        var actions = new Panel();
        actions.Children.Add(subtitleActions);
        actions.Children.Add(installedActions);
        actions.Children.Add(collectedActions);

        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(views);
        panel.Children.Add(EditorChrome.MakeToolbarSeparator());
        panel.Children.Add(actions);

        return new Border
        {
            Child = panel,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 6),
        };
    }

    private static SeTableViewColumn Column(string header, string property, GridLength width) => new()
    {
        Header = header,
        CellTheme = UiUtil.TableViewCellTheme,
        HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        Binding = new Binding(property),
        Width = width,
    };

    /// <summary>The fonts the current subtitle uses, with a found/embedded/not-found badge.</summary>
    private static TableView MakeCurrentSubtitleList(FontCollectorViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView().WithAccessibleName(Se.Language.Assa.FontCollectorCurrentSubtitle); // the view name (#12087)
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.FontItems;

        var fontNameColumn = Column(Se.Language.General.FontName, nameof(FontCollectorItem.FontName), new GridLength(200));
        var usedInColumn = Column(Se.Language.Assa.FontCollectorUsedIn, nameof(FontCollectorItem.UsedIn), new GridLength(1, GridUnitType.Star));
        var statusColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Status,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<FontCollectorItem>((_, _) => MakeStatusBadge()),
            NameBinding = new Binding(nameof(FontCollectorItem.Status)),
            Width = new GridLength(120),
        };
        var fontFilesColumn = Column(Se.Language.Assa.FontCollectorFontFiles, nameof(FontCollectorItem.FileDisplay), new GridLength(1, GridUnitType.Star));
        dataGrid.Columns.Add(fontNameColumn);
        dataGrid.Columns.Add(usedInColumn);
        dataGrid.Columns.Add(statusColumn);
        dataGrid.Columns.Add(fontFilesColumn);
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedFontItem)) { Source = vm });
        dataGrid.SelectionChanged += vm.FontItemsGridSelectionChanged;

        // Header sorting is safe here: the font list is presentation-only (copying
        // fonts to a folder uses the found-file set, not the row order, and the
        // background scan holds item references, not indexes).
        var sorter = new TableViewHeaderSorter(dataGrid);
        sorter.AddSortable<FontCollectorItem, string>(fontNameColumn, x => x.FontName)
            .AddSortable<FontCollectorItem, string>(usedInColumn, x => x.UsedIn)
            .AddSortable<FontCollectorItem, string>(statusColumn, x => x.Status)
            .AddSortable<FontCollectorItem, string>(fontFilesColumn, x => x.FileDisplay);

        return dataGrid;
    }

    /// <summary>
    /// Status as a small colored pill: found green, embedded accent, not found red.
    /// </summary>
    private static Border MakeStatusBadge()
    {
        IBrush BrushFor(string? status)
        {
            if (status == Se.Language.Assa.FontCollectorFound)
            {
                return FoundBrush;
            }

            if (status == Se.Language.Assa.FontCollectorNotFound)
            {
                return NotFoundBrush;
            }

            return EditorChrome.AccentBrush;
        }

        return new Border
        {
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(8, 1),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!Border.BackgroundProperty] = new Binding(nameof(FontCollectorItem.Status)) { Converter = new FuncValueConverter<string, IBrush>(BrushFor) },
            [!Border.IsVisibleProperty] = new Binding(nameof(FontCollectorItem.Status)) { Converter = StringConverters.IsNotNullOrEmpty },
            Child = new TextBlock
            {
                FontSize = 11.5,
                FontWeight = FontWeight.SemiBold,
                Foreground = Brushes.White,
                [!TextBlock.TextProperty] = new Binding(nameof(FontCollectorItem.Status)),
            },
        };
    }

    /// <summary>All fonts installed on this machine.</summary>
    private static TableView MakeInstalledFontsList(FontCollectorViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.Tools.PickFontNameInstalledFonts);
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.InstalledFontNames;
        dataGrid.Columns.Add(Column(Se.Language.General.FontName, ".", new GridLength(1, GridUnitType.Star)));
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedInstalledFontName)));
        return dataGrid;
    }

    /// <summary>The fonts collected in SE's own Fonts folder.</summary>
    private static TableView MakeCollectedFontsList(FontCollectorViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.Tools.PickFontNameCollectedFonts);
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.CollectedFonts;
        dataGrid.Columns.Add(Column(Se.Language.General.FontName, nameof(CollectedFont.Name), new GridLength(240)));
        dataGrid.Columns.Add(Column(Se.Language.Assa.FontCollectorFontFiles, nameof(CollectedFont.FilePath), new GridLength(1, GridUnitType.Star)));
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedCollectedFont)));
        dataGrid.KeyDown += vm.CollectedFontsGridKeyDown;

        var flyout = new MenuFlyout();
        flyout.Opening += vm.CollectedFontsContextMenuOpening;
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.DeleteCollectedFontCommand,
        };
        menuItemDelete.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteCollectedFontVisible)) { Source = vm });
        flyout.Items.Add(menuItemDelete);

        return dataGrid;
    }

    private static Border MakePreview(FontCollectorViewModel vm)
    {
        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.FontPreview)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
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
            Padding = new Thickness(12),
        };

        var panel = new Grid { RowSpacing = 8, RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) } };
        panel.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Preview), 0, 0);
        panel.Add(stage, 1, 0);
        return new Border
        {
            Child = panel,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 10, 14, 12),
        };
    }

    private static Border MakeButtonBar(FontCollectorViewModel vm)
    {
        var status = new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
        };

        var buttonDone = UiUtil.MakeButtonDone(vm.CloseCommand);
        buttonDone.Classes.Add("primary");

        // Embed option, beside Done while the subtitle's fonts show
        var trim = OnTab(vm, 0, EditorChrome.MakeToggleRow(nameof(vm.TrimFontsToUsedCharacters), Se.Language.Assa.FontCollectorTrimFonts));
        trim.Margin = new Thickness(0, 0, 16, 0);

        var bar = new DockPanel();
        DockPanel.SetDock(buttonDone, Dock.Right);
        DockPanel.SetDock(trim, Dock.Right);
        bar.Children.Add(buttonDone);
        bar.Children.Add(trim);
        bar.Children.Add(status);
        return new Border
        {
            Child = bar,
            Background = EditorChrome.ButtonBarBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
