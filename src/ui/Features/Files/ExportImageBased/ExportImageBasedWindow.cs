using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

/// <summary>
/// Export to images (Blu-ray sup, VobSub, D-Cinema, FCP, ...), in the editor look (see
/// <see cref="EditorChrome"/>): how the images look on the left (font, outline/shadow, box,
/// image, position, effect), the lines and the preview of the rendered image on the right,
/// profile, progress and Export at the bottom.
/// </summary>
public class ExportImageBasedWindow : Window
{
    private const double LabelWidth = 112;

    public ExportImageBasedWindow(ExportImageBasedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        CanResize = true;
        Width = 1100;
        Height = 800;
        MinWidth = 920;
        MinHeight = 600;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        // The settings column scrolls on its own and the lines give up height before anything
        // else, so the progress and the button bar stay reachable on a short screen.
        var settingsScroller = new ScrollViewer
        {
            Content = MakeSettingsPanel(vm),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        var left = new Border
        {
            Child = settingsScroller,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };

        var right = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)), // lines
                new RowDefinition(GridLength.Auto), // preview
            },
        };
        right.Add(MakeSubtitlesView(vm), 0, 0);
        right.Add(MakePreviewView(vm), 1, 0);

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(370)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(left, 0, 0);
        content.Add(right, 0, 1);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto), // progress, while exporting
                new RowDefinition(GridLength.Auto), // profile + buttons
            },
        };
        root.Add(content, 0, 0);
        root.Add(MakeProgressView(vm), 1, 0);
        root.Add(MakeButtonBar(vm), 2, 0);
        Content = root;

        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(vm.SubtitleGrid); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
        KeyUp += (_, e) => vm.OnKeyUp(e);
        Loaded += (_, e) => vm.OnLoaded();
        Closing += (_, e) => vm.OnClosing();

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    /// <summary>
    /// A dim label of fixed width and the control after it, so the rows line up. The label text
    /// can follow a view model property (outline/box color, whose meaning depends on the box type).
    /// </summary>
    private static Grid Row(string? label, Control content, string? labelBinding = null)
    {
        var text = EditorChrome.MakeDimLabel(label ?? string.Empty, new Thickness(0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        if (labelBinding != null)
        {
            text.Bind(TextBlock.TextProperty, new Binding(labelBinding));
        }

        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(LabelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

    private static StackPanel Horizontal(params Control[] controls)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
        foreach (var control in controls)
        {
            panel.Children.Add(control);
        }

        return panel;
    }

    /// <summary>
    /// A stretched combo box that regenerates the preview on change.
    /// </summary>
    private static ComboBox Combo<T>(ObservableCollection<T> items, ExportImageBasedViewModel vm, string property, string name)
    {
        var comboBox = UiUtil.MakeComboBox(items, vm, property);
        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboBox.SelectionChanged += vm.ComboChanged;
        AutomationProperties.SetName(comboBox, name);
        return comboBox;
    }

    private static Button ColorButton(ExportImageBasedViewModel vm, string property, string name)
    {
        var button = UiUtil.MakeColorPickerButton(vm, property, true);
        AutomationProperties.SetName(button, name);
        return button;
    }

    private static ToggleSwitch Switch(ExportImageBasedViewModel vm, string property, string name)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        toggle.IsCheckedChanged += vm.CheckBoxChanged;
        AutomationProperties.SetName(toggle, name);
        return toggle;
    }

    /// <summary>
    /// A switch with its label beside it (and optional controls after the label).
    /// </summary>
    private static Grid SwitchRow(ToggleSwitch toggle, string text, params Control[] after)
    {
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        row.Add(toggle, 0, 0);
        row.Add(new TextBlock { Text = text, FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap }, 0, 1);
        if (after.Length > 0)
        {
            row.Add(Horizontal(after), 0, 2);
        }

        return row;
    }

    private static StackPanel Rows(params Control[] rows)
    {
        var panel = new StackPanel { Spacing = 7 };
        foreach (var row in rows)
        {
            panel.Children.Add(row);
        }

        return panel;
    }

    private static Control MakeSettingsPanel(ExportImageBasedViewModel vm)
    {
        var panel = new StackPanel();
        panel.Children.Add(MakeFontSection(vm));
        panel.Children.Add(MakeOutlineSection(vm));
        panel.Children.Add(MakeBoxSection(vm));
        panel.Children.Add(MakeImageSection(vm));
        panel.Children.Add(MakePositionSection(vm));
        panel.Children.Add(MakeEffectSection(vm));
        return panel;
    }

    private static Border MakeFontSection(ExportImageBasedViewModel vm)
    {
        var fontFamily = Combo(vm.FontFamilies, vm, nameof(vm.SelectedFontFamily), Se.Language.General.FontName);
        var fontSize = Combo(vm.FontSizes, vm, nameof(vm.SelectedFontSize), Se.Language.General.FontSize);
        var lineSpacing = Combo(vm.LineSpacings, vm, nameof(vm.SelectedLineSpacing), Se.Language.File.Export.LineSpacingPercent);

        var switches = new WrapPanel { Orientation = Orientation.Horizontal, ItemSpacing = 18, LineSpacing = 6 };
        switches.Children.Add(SwitchRow(Switch(vm, nameof(vm.IsBold), Se.Language.General.Bold), Se.Language.General.Bold));
        switches.Children.Add(SwitchRow(Switch(vm, nameof(vm.IsRightToLeft), Se.Language.General.RightToLeft), Se.Language.General.RightToLeft));

        return EditorChrome.MakeEditorSection(Se.Language.General.Font, Rows(
            fontFamily,
            Row(Se.Language.General.FontSize, fontSize),
            Row(Se.Language.General.FontColor, Horizontal(ColorButton(vm, nameof(vm.FontColor), Se.Language.General.FontColor))),
            Row(Se.Language.File.Export.LineSpacingPercent, lineSpacing),
            switches), first: true);
    }

    private static Border MakeOutlineSection(ExportImageBasedViewModel vm)
    {
        var outlineWidth = Combo(vm.OutlineWidths, vm, nameof(vm.SelectedOutlineWidth), Se.Language.General.OutlineWidth);
        outlineWidth.MinWidth = 90;
        var shadowWidth = Combo(vm.ShadowWidths, vm, nameof(vm.SelectedShadowWidth), Se.Language.General.ShadowWidth);
        shadowWidth.MinWidth = 90;

        return EditorChrome.MakeEditorSection(Se.Language.General.Outline + " · " + Se.Language.General.Shadow, Rows(
            Row(null, Horizontal(ColorButton(vm, nameof(vm.OutlineColor), Se.Language.General.OutlineColor), outlineWidth), nameof(vm.OutlineColorText)),
            Row(null, Horizontal(ColorButton(vm, nameof(vm.ShadowColor), Se.Language.General.ShadowColor), shadowWidth), nameof(vm.ShadowColorText))));
    }

    private static Border MakeBoxSection(ExportImageBasedViewModel vm)
    {
        var boxType = Combo(vm.BoxTypes, vm, nameof(vm.SelectedBoxType), Se.Language.Video.BurnIn.BoxType);
        var cornerRadius = Combo(vm.BoxCornerRadiusList, vm, nameof(vm.SelectedBoxCornerRadius), Se.Language.General.BoxCornerRadius);

        // The four box paddings, each with its side named under it (#12087: they had only tooltips)
        var paddings = new Grid { ColumnSpacing = 4 };
        var sides = new[]
        {
            (nameof(vm.BoxPaddingLeft), Se.Language.General.Left),
            (nameof(vm.BoxPaddingRight), Se.Language.General.Right),
            (nameof(vm.BoxPaddingTop), Se.Language.General.Top),
            (nameof(vm.BoxPaddingBottom), Se.Language.General.Bottom),
        };
        for (var i = 0; i < sides.Length; i++)
        {
            var (property, name) = sides[i];
            var comboBox = Combo(vm.BoxPaddingValues, vm, property, name);
            comboBox.MinWidth = 0;
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(comboBox, name);
            }

            var cell = new StackPanel { Spacing = 2 };
            cell.Children.Add(comboBox);
            cell.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = EditorChrome.ScaledFont(10.5),
                Foreground = EditorChrome.FaintTextBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            });
            paddings.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            paddings.Add(cell, 0, i);
        }

        return EditorChrome.MakeEditorSection(Se.Language.General.Box, Rows(
            Row(Se.Language.Video.BurnIn.BoxType, boxType),
            Row(null, Horizontal(ColorButton(vm, nameof(vm.BoxColor), Se.Language.General.BoxColor)), nameof(vm.BoxColorText)),
            Row(Se.Language.General.BoxCornerRadius, cornerRadius),
            Row(Se.Language.General.BoxPadding, paddings)));
    }

    /// <summary>
    /// Image size and frame rate, the frame-sized image option and the 3D settings.
    /// </summary>
    private static Border MakeImageSection(ExportImageBasedViewModel vm)
    {
        var resolution = UiUtil.MakeComboBox(vm.Resolutions, vm, nameof(vm.SelectedResolution));
        resolution.HorizontalAlignment = HorizontalAlignment.Stretch;
        resolution.SelectionChanged += vm.ComboResolutionChanged;
        AutomationProperties.SetName(resolution, Se.Language.General.Resolution);

        var frameRate = Combo(vm.FrameRates, vm, nameof(vm.SelectedFrameRate), Se.Language.General.FrameRate).WithFrameRateDisplay();

        // Only shown for the formats that can use a frame-sized image (see IsFullFrameVisible).
        var fullFrame = Switch(vm, nameof(vm.IsFullFrame), Se.Language.File.Export.FullFrameImage);
        var fullFrameColor = ColorButton(vm, nameof(vm.FullFrameBackgroundColor), Se.Language.General.BackgroundColor);
        fullFrameColor.Bind(IsVisibleProperty, new Binding(nameof(vm.IsFullFrame)) { Source = vm });
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(fullFrame, Se.Language.File.Export.FullFrameImageHint);
            ToolTip.SetTip(fullFrameColor, Se.Language.General.BackgroundColor);
        }

        var fullFrameRow = SwitchRow(fullFrame, Se.Language.File.Export.FullFrameImage, fullFrameColor)
            .WithBindIsVisible(vm, nameof(vm.IsFullFrameVisible));

        // SE4's "3D": the subtitle drawn once per eye for frame-packed 3D video. D-Cinema has only
        // the depth, as the Z-position (see IsMode3DVisible).
        var mode3D = Combo(vm.Modes3D, vm, nameof(vm.SelectedMode3D), Se.Language.File.Export.Stereo3D);
        var depth3D = Combo(vm.Depths3D, vm, nameof(vm.SelectedDepth3D), Se.Language.File.Export.Depth3D);
        depth3D.Bind(IsEnabledProperty, new Binding(nameof(vm.IsDepth3DEnabled)) { Source = vm });
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(mode3D, Se.Language.File.Export.Stereo3DHint);
            ToolTip.SetTip(depth3D, Se.Language.File.Export.Depth3DHint);
        }

        var mode3DRow = Row(Se.Language.File.Export.Stereo3D, mode3D);
        mode3DRow.Bind(IsVisibleProperty, new Binding(nameof(vm.IsMode3DVisible)) { Source = vm });
        var depth3DRow = Row(null, depth3D, nameof(vm.Depth3DText));

        // A 3D Blu-ray's own depth for each subtitle, from its 3D-Plane (see Stereo3DPlane).
        var browsePlane3D = EditorChrome.CreateToolButton("fa-solid fa-folder-open", Se.Language.File.Export.OpenPlane3DTitle, vm.BrowsePlane3DCommand, 28);
        browsePlane3D.Bind(IsEnabledProperty, new Binding(nameof(vm.IsDepth3DEnabled)) { Source = vm });
        var plane3DFile = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Plane3DText)) { Source = vm },
        };
        var clearPlane3D = EditorChrome.CreateToolButton("fa-solid fa-xmark", Se.Language.General.Clear, vm.ClearPlane3DCommand, 28)
            .WithBindIsVisible(vm, nameof(vm.IsPlane3DLoaded));
        var plane3D = new Grid
        {
            ColumnSpacing = 4,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        plane3D.Add(browsePlane3D, 0, 0);
        plane3D.Add(plane3DFile, 0, 1);
        plane3D.Add(clearPlane3D, 0, 2);
        var plane3DRow = Row(Se.Language.File.Export.Plane3D, plane3D);
        plane3DRow.Bind(IsVisibleProperty, new Binding(nameof(vm.IsMode3DVisible)) { Source = vm });
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(plane3DRow.Children[0], Se.Language.File.Export.Plane3DHint);
        }

        return EditorChrome.MakeEditorSection(Se.Language.General.Image, Rows(
            Row(Se.Language.General.Resolution, resolution),
            Row(Se.Language.General.FrameRate, frameRate),
            fullFrameRow,
            mode3DRow,
            depth3DRow,
            plane3DRow));
    }

    private static Border MakePositionSection(ExportImageBasedViewModel vm)
    {
        return EditorChrome.MakeEditorSection(Se.Language.General.Alignment + " · " + Se.Language.General.Margin, Rows(
            Row(Se.Language.General.Alignment, Combo(vm.Alignments, vm, nameof(vm.SelectedAlignment), Se.Language.General.Alignment)),
            Row(Se.Language.General.ContentAlignment, Combo(vm.ContentAlignments, vm, nameof(vm.SelectedContentAlignment), Se.Language.General.ContentAlignment)),
            Row(Se.Language.File.Export.LeftRightMargin, Combo(vm.LeftRightMargins, vm, nameof(vm.SelectedLeftRightMargin), Se.Language.File.Export.LeftRightMargin)),
            Row(Se.Language.General.TopBottomMargin, Combo(vm.TopBottomMargins, vm, nameof(vm.SelectedTopBottomMargin), Se.Language.General.TopBottomMargin)),
            Row(Se.Language.File.Export.PaddingLeftRight, Combo(vm.PaddingsLeftRight, vm, nameof(vm.SelectedPaddingLeftRight), Se.Language.File.Export.PaddingLeftRight)),
            Row(Se.Language.File.Export.PaddingTopBottom, Combo(vm.PaddingsTopBottom, vm, nameof(vm.SelectedPaddingTopBottom), Se.Language.File.Export.PaddingTopBottom))));
    }

    private static Border MakeEffectSection(ExportImageBasedViewModel vm)
    {
        var settings = EditorChrome.CreateToolButton("fa-solid fa-gear", Se.Language.File.Export.TextEffectSettingsTitle, vm.ShowTextEffectSettingsCommand, 28);
        return EditorChrome.MakeEditorSection(Se.Language.General.Effect,
            SwitchRow(Switch(vm, nameof(vm.IsTextEffectEnabled), Se.Language.File.Export.TextEffect), Se.Language.File.Export.TextEffect, settings));
    }

    /// <summary>
    /// A section header above content that takes the rest of the height (lists, the preview).
    /// </summary>
    private static DockPanel MakeHeaderPanel(string title, Control content, Control? headerRight = null)
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

    private static Control MakeSubtitlesView(ExportImageBasedViewModel vm)
    {
        vm.SubtitleGrid = TableViewExtras.MakeTableView().WithAccessibleName(Se.Language.General.Lines); // #12087
        vm.SubtitleGrid.Height = double.NaN; // auto size inside scroll viewer
        vm.SubtitleGrid.ItemsSource = vm.Subtitles;
        vm.SubtitleGrid.DataContext = vm.Subtitles;

        //   vm.SubtitleGrid.DoubleTapped += vm.OnSubtitleGridDoubleTapped;

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var textToFlowDirectionConverter = new TextToFlowDirectionConverter();

        // Columns
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
            Header = Se.Language.General.Hide,
            Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter },
            Width = new GridLength(120),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        // The DataGrid sized this column to content (Auto); TableView treats Auto as
        // star, so use a fixed width that fits the short duration format.
        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            Width = new GridLength(90),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            NameBinding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.DurationBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
                };

                border.Child = textBlock;
                return border;
            })
        });

        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            NameBinding = new Binding(nameof(SubtitleLineViewModel.Text)),
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((value, nameScope) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.TextBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.Text)),

                    // Right-to-left text needs a right-to-left cell, or Avalonia splits the
                    // line at every zero width non-joiner and reverses the word order (#13160).
                    [!TextBlock.FlowDirectionProperty] = new Binding(nameof(SubtitleLineViewModel.Text)) { Converter = textToFlowDirectionConverter },
                };

                border.Child = textBlock;
                return border;
            })
        });

        vm.SubtitleGrid.DataContext = vm.Subtitles;
        vm.SubtitleGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle))
        {
            Source = vm,
            Mode = BindingMode.TwoWay
        });
        vm.SubtitleGrid.SelectionChanged += vm.SubtitleGrid_SelectionChanged;

        var flyout = new MenuFlyout();

        //flyout.Opening += vm.SubtitleContextOpening;

        var deleteMenuItem = new MenuItem { Header = Se.Language.General.Delete };
        deleteMenuItem.Command = vm.DeleteSelectedLinesCommand;
        flyout.Items.Add(deleteMenuItem);

        flyout.Items.Add(new Separator());

        var italicMenuItem = new MenuItem { Header = Se.Language.General.Italic };
        italicMenuItem.Command = vm.ToggleLinesItalicCommand;
        flyout.Items.Add(italicMenuItem);

        var boldMenuItem = new MenuItem { Header = Se.Language.General.Bold };
        boldMenuItem.Command = vm.ToggleLinesBoldCommand;
        flyout.Items.Add(boldMenuItem);

        // Set the ContextFlyout property
        vm.SubtitleGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(vm.SubtitleGrid);
        //vm.SubtitleGrid.AddHandler(InputElement.PointerPressedEvent, vm.SubtitleGrid_PointerPressed,
        //    RoutingStrategies.Tunnel);
        //vm.SubtitleGrid.AddHandler(InputElement.PointerReleasedEvent, vm.SubtitleGrid_PointerReleased,
        //    RoutingStrategies.Tunnel);

        var count = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding($"{nameof(vm.Subtitles)}.Count") { Source = vm },
        };

        return new Border
        {
            Padding = new Thickness(14, 12, 14, 12),
            Child = MakeHeaderPanel(Se.Language.General.Lines, EditorChrome.MakeListFrame(vm.SubtitleGrid, clearListBackground: true), count),
        };
    }

    /// <summary>
    /// The rendered image of the selected line, on the shared subtitle-bitmap backdrop (#15111):
    /// export images are text on a transparent background and would vanish into a light window.
    /// </summary>
    private Control MakePreviewView(ExportImageBasedViewModel vm)
    {
        var previewBackground = ImagePreviewBackground.CreateBrush();

        var imagePreview = new Image
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            Margin = new Thickness(8),
        };
        imagePreview.Bind(Image.SourceProperty, new Binding(nameof(vm.BitmapPreview))
        {
            Source = vm,
            Mode = BindingMode.OneWay
        });
        AutomationProperties.SetName(imagePreview, Se.Language.General.Preview);

        var backdrop = new Border
        {
            Background = previewBackground,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
            Height = 200,
            Child = imagePreview,
        };

        var flyout = new MenuFlyout();
        flyout.Items.Add(ImagePreviewBackground.MakeMenuItem(vm.WindowService, () => vm.Window, previewBackground));
        backdrop.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(backdrop);

        var imageInfo = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 0, 8, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ImageInfo)) { Source = vm },
        };

        var actions = Horizontal(
            imageInfo,
            EditorChrome.MakeTextToolButton("fa-solid fa-floppy-disk", Se.Language.General.SaveDotDotDot, vm.SavePreviewCommand),
            EditorChrome.MakeTextToolButton("fa-solid fa-expand", Se.Language.General.ShowPreview, vm.ShowPreviewCommand));
        actions.Spacing = 2;

        return new Border
        {
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Background = EditorChrome.PanelBrush,
            Padding = new Thickness(14, 10, 14, 14),
            Child = MakeHeaderPanel(Se.Language.General.Preview, backdrop, actions),
        };
    }

    private static Border MakeProgressView(ExportImageBasedViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var statusText = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ProgressText)),
        };

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(progressBar, 0, 0);
        grid.Add(statusText, 0, 1);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 8),
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.IsGenerating)),
        };
    }

    private static Border MakeButtonBar(ExportImageBasedViewModel vm)
    {
        var comboProfile = UiUtil.MakeComboBox(vm.Profiles, vm, nameof(vm.SelectedProfile));
        comboProfile.SelectionChanged += vm.ProfileChanged;
        comboProfile.MinWidth = 160;
        AutomationProperties.SetName(comboProfile, Se.Language.General.Profile);
        var buttonProfiles = EditorChrome.CreateToolButton("fa-solid fa-sliders", Se.Language.File.Export.ExportImagesProfiles, vm.ShowProfileCommand, 28);
        var profile = Horizontal(EditorChrome.MakeDimLabel(Se.Language.General.Profile, new Thickness(0, 0, 2, 0)), comboProfile, buttonProfiles);

        var buttonExport = UiUtil.MakeButton(Se.Language.General.ExportDotDotDot, vm.ExportCommand)
            .WithBindIsVisible(vm, nameof(vm.IsExportButtonVisible));
        buttonExport.IsDefault = true; // the dialog's accept button - Enter runs it (#14586)
        buttonExport.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand).WithBindIsVisible(nameof(vm.IsGenerating));
        var buttonDone = UiUtil.MakeButtonDone(vm.CancelCommand).WithBindIsVisible(nameof(vm.IsGenerating), InverseBooleanConverter.Instance);

        return EditorChrome.MakeDockedButtonBar(profile, buttonDone, buttonCancel, buttonExport);
    }
}
