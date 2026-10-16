using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using System;
using System.Collections;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Files.ImportImages;

/// <summary>
/// Images for OCR, in the editor look (see <see cref="EditorChrome"/>): the image list with
/// import/remove/clear, the selected image on the shared preview backdrop, OK at the bottom.
/// </summary>
public class ImportImagesWindow : Window
{
    public ImportImagesWindow(ImportImagesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Import.TitleImportImages;
        CanResize = true;
        Width = 1100;
        Height = 780;
        MinWidth = 900;
        MinHeight = 520;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        vm.Window = this;
        DataContext = vm;

        // The list gives up height first, so the preview and the buttons stay reachable
        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        var list = MakeImagesView(vm, out var imagesGrid);
        list.Margin = new Thickness(14, 12, 14, 12);
        sections.Add(list, 0, 0);
        sections.Add(MakePreviewView(vm), 1, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var count = new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding($"{nameof(vm.Images)}.Count") { StringFormat = "{0} " + Se.Language.General.Images.ToLowerInvariant() },
        };

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(count, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(imagesGrid); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    /// <summary>
    /// The selected image, on the shared subtitle-bitmap backdrop (#15111): these are usually
    /// light text on a transparent background and would vanish on the light theme.
    /// </summary>
    private static Border MakePreviewView(ImportImagesViewModel vm)
    {
        var image = new Image
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            Margin = new Thickness(8),
        };
        AutomationProperties.SetName(image, Se.Language.General.Preview);

        void Update()
        {
            var old = image.Source as Bitmap;
            image.Source = null;
            old?.Dispose();
            var item = vm.SelectedImage;
            if (item == null || item.Bytes.Length == 0)
            {
                return;
            }

            try
            {
                using var skBitmap = item.GetBitmap();
                image.Source = skBitmap.ToAvaloniaBitmap();
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Import images preview of " + item.FileName);
            }
        }

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.SelectedImage))
            {
                Update();
            }
        };
        Update();

        var backdrop = new Border
        {
            Background = ImagePreviewBackground.CreateBrush(),
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
            Height = 160,
            Child = image,
        };

        return new Border
        {
            Child = MakeHeaderPanel(Se.Language.General.Preview, backdrop, null),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 10, 14, 14),
        };
    }

    /// <summary>
    /// A section header (with actions on the right) above content that takes the rest of the height.
    /// </summary>
    private static DockPanel MakeHeaderPanel(string title, Control content, Control? headerRight)
    {
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8), MinHeight = 28 };
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        header.Children.Add(EditorChrome.MakeSectionHeader(title));
        var panel = new DockPanel();
        DockPanel.SetDock(header, Dock.Top);
        panel.Children.Add(header);
        panel.Children.Add(content);
        return panel;
    }

    private static DockPanel MakeImagesView(ImportImagesViewModel vm, out TableView imagesGrid)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var fileSizeConverter = new FileSizeConverter();
        // No header sorting (the DataGrid's CanUserSortColumns is not carried over):
        // the caller feeds result.Images to OCR in collection order, so reordering the
        // backing collection would reorder the resulting subtitle.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.General.Images);
        imagesGrid = dataGrid;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Images;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.FileName,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ImportImageItem.FileName)),
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Size,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ImportImageItem.Size)) { Converter = fileSizeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(100),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Show,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ImportImageItem.Start)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(130),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Hide,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ImportImageItem.End)){ Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(130),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Duration,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ImportImageItem.Duration)){ Converter = shortTimeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(110),
            },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedImage)) { Source = vm });
        dataGrid.SelectionChanged += vm.DataGridSelectionChanged;
        dataGrid.KeyDown += vm.AttachmentsDataGridKeyDown;
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                dataGrid.SelectedItem = target;
                if (target != null)
                {
                    dataGrid.ScrollIntoView(target);
                }

                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        // Empty list: say what to do (a tap on the empty list opens the file picker, see FileGridTapped)
        var hint = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8,
            IsHitTestVisible = false,
        };
        hint.Children.Add(new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-images", FontSize = 34, Foreground = EditorChrome.FaintTextBrush, HorizontalAlignment = HorizontalAlignment.Center });
        hint.Children.Add(new TextBlock
        {
            Text = Se.Language.File.Import.ImportFileLabel,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            MaxWidth = 380,
        });
        void UpdateHint() => hint.IsVisible = vm.Images.Count == 0;
        vm.Images.CollectionChanged += (_, _) => UpdateHint();
        UpdateHint();

        var listHost = new Grid();
        listHost.Children.Add(dataGrid);
        listHost.Children.Add(hint);

        // hack to make drag and drop work on the grid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = listHost,
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.FileGridOnDrop, RoutingStrategies.Bubble);

        dropHost.Tapped += (s, e) =>
        {
            vm.FileGridTapped();
        };

        var flyout = new MenuFlyout();
        flyout.Opening += vm.ImagesContextMenuOpening;
        dropHost.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dropHost);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Remove,
            DataContext = vm,
            Command = vm.ImageRemoveCommand,
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

        var menuItemImport = new MenuItem
        {
            Header = Se.Language.General.ImportDotDotDot,
            DataContext = vm,
            Command = vm.FileImportCommand,
        };
        flyout.Items.Add(menuItemImport);

        // The time code file name patterns, behind an info icon
        var info = new Optris.Icons.Avalonia.Icon
        {
            Value = "fa-solid fa-circle-info",
            FontSize = 13,
            Foreground = EditorChrome.IconBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
            Background = Brushes.Transparent,
            IsVisible = Se.Settings.Appearance.ShowHints,
        };
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(info, Se.Language.File.Import.ImportFilesInfo);
        }

        var buttonRemove = EditorChrome.CreateToolButton("fa-solid fa-minus", Se.Language.General.Remove, vm.ImageRemoveCommand, 28);
        var buttonClear = EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Clear, vm.AttachemntsRemoveAllCommand, 28);
        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                info,
                EditorChrome.MakeTextToolButton("fa-solid fa-plus", Se.Language.General.ImportDotDotDot, vm.FileImportCommand),
                buttonRemove,
                buttonClear,
            },
        };

        return MakeHeaderPanel(Se.Language.General.Images, EditorChrome.MakeListFrame(dropHost, clearListBackground: true), actions);
    }
}
