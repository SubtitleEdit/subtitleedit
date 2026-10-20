using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;

/// <summary>
/// Generate a video with transparent subtitles, in the editor look (see <see cref="EditorChrome"/>):
/// how the subtitle looks on the left (style, position, effect), the preview with the cut range on
/// the right and the output settings (resolution, frame rate, format) under it; mode switch,
/// progress and Generate at the bottom. In batch mode the job list takes the place of the preview.
/// </summary>
public class TransparentSubtitlesWindow : Window
{
    private readonly TransparentSubtitlesViewModel _vm;
    private ComboBox? _comboBoxFontName;

    private const double LabelWidth = 78;

    public TransparentSubtitlesWindow(TransparentSubtitlesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.VideoTransparent.Title;
        CanResize = true;
        Width = 1180;
        Height = 780;
        MinWidth = 960;
        MinHeight = 520;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        AddStyles();

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        // Same guard as the burn-in window (issues #13904, #14360): the style column scrolls on
        // its own and the preview gives up height before anything else, so the progress bar and
        // the button bar are always reachable - even on a screen too short for the dialog.
        var styleScroller = new ScrollViewer
        {
            Content = MakeStylePanel(vm),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        var left = new Border
        {
            Child = styleScroller,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };

        var right = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)), // preview, or the batch list
                new RowDefinition(GridLength.Auto), // cut range + source video
                new RowDefinition(GridLength.Auto), // resolution / frame rate / format
            },
        };
        var stage = new Panel { Margin = new Thickness(14, 12, 14, 8) };
        stage.Children.Add(MakePreview(vm));
        stage.Children.Add(MakeBatchView(vm));
        right.Add(stage, 0, 0);
        right.Add(MakeCutRow(vm), 1, 0);
        right.Add(MakeOutputPanel(vm), 2, 0);

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(310)),
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
                new RowDefinition(GridLength.Auto), // progress, while generating
                new RowDefinition(GridLength.Auto), // mode, buttons
            },
        };
        root.Add(content, 0, 0);
        root.Add(MakeProgressView(vm), 1, 0);
        root.Add(MakeButtonBar(vm), 2, 0);
        Content = root;

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.IsBatchMode) && vm.IsBatchMode)
            {
                // The player is hidden in batch mode - stop its audio too.
                vm.VideoPlayerControl?.VideoPlayer.Pause();
            }
        };

        UiUtil.FocusOnFirstActivation(this, () => { _comboBoxFontName?.Focus(); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
        Loaded += (_, _) => vm.Loaded();
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing();
    }

    /// <summary>
    /// Compact combo boxes and time code fields, the bold toggle, the alignment cells and the
    /// accent Generate button.
    /// </summary>
    private void AddStyles()
    {
        Styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
            },
        });
        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<TextBox>())
        {
            Setters = { new Setter(TemplatedControl.MinHeightProperty, 26.0) },
        });
        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<ButtonSpinner>())
        {
            Setters = { new Setter(TemplatedControl.MinHeightProperty, 26.0) },
        });

        Styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool"))
        {
            Setters =
            {
                new Setter(ToggleButton.BackgroundProperty, Brushes.Transparent),
                new Setter(ToggleButton.BorderThicknessProperty, new Thickness(0)),
                new Setter(ToggleButton.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(ToggleButton.ForegroundProperty, EditorChrome.IconBrush),
                new Setter(ToggleButton.PaddingProperty, new Thickness(0)),
            },
        });
        Styles.Add(new Style(x => x.OfType<ToggleButton>().Class("tool").Class(":checked").Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(Avalonia.Controls.Presenters.ContentPresenter.BackgroundProperty, EditorChrome.AccentBrush),
                new Setter(Avalonia.Controls.Presenters.ContentPresenter.ForegroundProperty, Brushes.White),
            },
        });

        Styles.Add(new Style(x => x.OfType<RadioButton>().Class("anchor").Template().OfType<Border>())
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, EditorChrome.FieldBrush),
                new Setter(Border.BorderBrushProperty, EditorChrome.FieldBorderBrush),
            },
        });
        Styles.Add(new Style(x => x.OfType<RadioButton>().Class("anchor").Class(":pointerover").Template().OfType<Border>())
        {
            Setters = { new Setter(Border.BackgroundProperty, EditorChrome.HoverBrush) },
        });
        Styles.Add(new Style(x => x.OfType<RadioButton>().Class("anchor").Class(":checked").Template().OfType<Border>())
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, EditorChrome.AccentBrush),
                new Setter(Border.BorderBrushProperty, EditorChrome.AccentBrush),
            },
        });
        Styles.Add(new Style(x => x.OfType<RadioButton>().Class("anchor").Template().OfType<Ellipse>())
        {
            Setters = { new Setter(Shape.FillProperty, EditorChrome.FaintTextBrush) },
        });
        Styles.Add(new Style(x => x.OfType<RadioButton>().Class("anchor").Class(":checked").Template().OfType<Ellipse>())
        {
            Setters = { new Setter(Shape.FillProperty, Brushes.White) },
        });

        // Generate is the window's main action: both halves of the split button in the accent
        foreach (var state in new[] { string.Empty, ":pointerover" })
        {
            var hover = state.Length > 0;
            Styles.Add(new Style(x =>
            {
                var selector = x.OfType<SplitButton>().Class("primary").Template().OfType<Button>();
                return hover ? selector.Class(":pointerover").Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>() : selector.Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>();
            })
            {
                Setters =
                {
                    new Setter(Avalonia.Controls.Presenters.ContentPresenter.BackgroundProperty, hover ? EditorChrome.AccentHoverBrush : EditorChrome.AccentBrush),
                    new Setter(Avalonia.Controls.Presenters.ContentPresenter.ForegroundProperty, Brushes.White),
                },
            });
        }
    }

    /// <summary>
    /// A label of fixed width and the control(s) after it, so the rows line up.
    /// </summary>
    private static Grid Row(string? label, Control content, string? labelBinding = null, bool autoLabelWidth = false)
    {
        var text = EditorChrome.MakeDimLabel(label ?? string.Empty, new Thickness(0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        if (labelBinding != null)
        {
            text.Bind(TextBlock.TextProperty, new Binding(labelBinding));
        }

        var grid = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = { new ColumnDefinition(autoLabelWidth ? GridLength.Auto : new GridLength(LabelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
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

    private static ComboBox Combo<T>(System.Collections.ObjectModel.ObservableCollection<T> items, TransparentSubtitlesViewModel vm, string property, string name)
    {
        var comboBox = UiUtil.MakeComboBox(items, vm, property);
        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBox, name);
        return comboBox;
    }

    private static NumericUpDown Field(TransparentSubtitlesViewModel vm, string property, double min, double max, string prefix, string name, string format = "0.#", decimal increment = 1, double width = 76)
    {
        var field = EditorChrome.MakeField(property, min, max, width, prefix, format);
        field.Increment = increment;
        field.ValueChanged += vm.NumericUpDownChanged;
        AutomationProperties.SetName(field, name);
        return field;
    }

    private static TextBlock Hint(string property) => new()
    {
        FontSize = 11.5,
        Foreground = EditorChrome.DimTextBrush,
        VerticalAlignment = VerticalAlignment.Center,
        TextTrimming = TextTrimming.CharacterEllipsis,
        [!TextBlock.TextProperty] = new Binding(property),
    };

    private static ToggleSwitch Switch(string property, string name)
    {
        var toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, name);
        return toggle;
    }

    private Control MakeStylePanel(TransparentSubtitlesViewModel vm)
    {
        var panel = new StackPanel();
        panel.Children.Add(MakeStyleSection(vm));
        panel.Children.Add(MakePositionSection(vm));
        panel.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Effect,
            MakeLinkRow("fa-solid fa-wand-magic-sparkles", Se.Language.General.Effect, nameof(vm.DisplayEffect), vm.ShowEffectsCommand)));
        return panel;
    }

    /// <summary>
    /// A row that opens a dialog: icon, what is set now, and a "..." button.
    /// </summary>
    private static Grid MakeLinkRow(string icon, string name, string valueProperty, System.Windows.Input.ICommand command)
    {
        var button = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", name, command, 26);
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        row.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 13, Foreground = EditorChrome.IconBrush, VerticalAlignment = VerticalAlignment.Center }, 0, 0);
        var value = Hint(valueProperty);
        value.FontSize = 12.5;
        row.Add(value, 0, 1);
        row.Add(button, 0, 2);
        return row;
    }

    private Border MakeStyleSection(TransparentSubtitlesViewModel vm)
    {
        var fontName = Combo(vm.FontNames, vm, nameof(vm.SelectedFontName), Se.Language.General.FontName);
        fontName.SelectionChanged += vm.ComboBoxChanged;
        _comboBoxFontName = fontName;

        var sizeFactor = Field(vm, nameof(vm.FontFactor), 0.1, 1.0, "Aa", Se.Language.Video.BurnIn.FontSizeFactor, "0.00", 0.05m);
        var bold = new ToggleButton
        {
            Content = new TextBlock { Text = "B", FontWeight = FontWeight.Bold, FontSize = 13 },
            Width = 28,
            Height = 28,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.FontIsBold)) { Mode = BindingMode.TwoWay },
        };
        bold.Classes.Add("tool");
        bold.PropertyChanged += vm.CheckBoxChanged;
        AutomationProperties.SetName(bold, Se.Language.General.Bold);
        var factorHint = Hint(nameof(vm.FontFactorText));
        var sizeRow = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        sizeRow.Add(sizeFactor, 0, 0);
        sizeRow.Add(bold, 0, 1);
        sizeRow.Add(factorHint, 0, 2);

        var textColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.FontTextColor), true);
        AutomationProperties.SetName(textColor, Se.Language.General.TextColor);
        var outlineColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.FontOutlineColor), true);
        AutomationProperties.SetName(outlineColor, Se.Language.General.Outline);
        var shadowColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.FontShadowColor), true);
        AutomationProperties.SetName(shadowColor, Se.Language.General.Shadow);

        var boxType = Combo(vm.FontBoxTypes, vm, nameof(vm.SelectedFontBoxType), Se.Language.Video.BurnIn.BoxType);
        boxType.SelectionChanged += vm.BoxTypeChanged;

        var rows = new StackPanel { Spacing = 7 };
        rows.Children.Add(fontName);
        rows.Children.Add(sizeRow);
        rows.Children.Add(Row(Se.Language.Video.BurnIn.BoxType, boxType));
        rows.Children.Add(Row(Se.Language.General.TextColor, Horizontal(textColor)));
        rows.Children.Add(Row(null, Horizontal(outlineColor, Field(vm, nameof(vm.SelectedFontOutline), 0, 50, "W", Se.Language.General.OutlineWidth, increment: 0.5m, width: 70)), nameof(vm.FontOutlineText)));
        rows.Children.Add(Row(null, Horizontal(shadowColor, Field(vm, nameof(vm.SelectedFontShadowWidth), 0, 50, "W", Se.Language.General.ShadowWidth, increment: 0.5m, width: 70)), nameof(vm.FontShadowText)));
        rows.Children.Add(Row(Se.Language.General.Spacing, Horizontal(Field(vm, nameof(vm.SelectedFontSpacing), -20, 100, "↔", Se.Language.General.Spacing, width: 70))));
        return EditorChrome.MakeEditorSection(Se.Language.Video.BurnIn.SubtitleStyle, rows, first: true);
    }

    /// <summary>
    /// The 3 x 3 alignment grid beside the margins.
    /// </summary>
    private static Border MakePositionSection(TransparentSubtitlesViewModel vm)
    {
        var anchors = new UniformGrid { Columns = 3, Rows = 3, Width = 90, Height = 90, VerticalAlignment = VerticalAlignment.Top };
        foreach (var item in vm.FontAlignments)
        {
            var alignment = item;
            var radio = new RadioButton
            {
                GroupName = "transparent-subtitles-alignment",
                Width = 26,
                Height = 26,
                Margin = new Thickness(0, 0, 4, 4),
                Template = new FuncControlTemplate<RadioButton>((_, _) => new Border
                {
                    CornerRadius = new CornerRadius(6),
                    BorderThickness = new Thickness(1),
                    Child = new Ellipse { Width = 6, Height = 6, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
                }),
                IsChecked = vm.SelectedFontAlignment?.Code == alignment.Code,
            };
            radio.Classes.Add("anchor");
            AutomationProperties.SetName(radio, alignment.Name);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(radio, alignment.Name);
            }

            radio.IsCheckedChanged += (_, _) =>
            {
                if (radio.IsChecked == true && vm.SelectedFontAlignment?.Code != alignment.Code)
                {
                    vm.SelectedFontAlignment = alignment;
                    vm.ComboBoxChanged(radio, null!); // what the old alignment combo box did on change
                }
            };
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.SelectedFontAlignment))
                {
                    radio.IsChecked = vm.SelectedFontAlignment?.Code == alignment.Code;
                }
            };
            anchors.Children.Add(radio);
        }

        var margins = new StackPanel { Spacing = 6 };
        margins.Children.Add(Field(vm, nameof(vm.FontMarginHorizontal), 0, 1000, "H", Se.Language.General.Horizontal, "0", width: double.NaN));
        margins.Children.Add(Field(vm, nameof(vm.FontMarginVertical), 0, 1000, "V", Se.Language.General.Vertical, "0", width: double.NaN));

        var row = new Grid { ColumnSpacing = 14, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) } };
        row.Add(anchors, 0, 0);
        row.Add(margins, 0, 1);
        return EditorChrome.MakeEditorSection(Se.Language.General.Alignment + " · " + Se.Language.General.Margin, row);
    }

    private static Control MakePreview(TransparentSubtitlesViewModel vm)
    {
        // Live preview (single mode): the loaded video plays in an embedded mpv player
        // while the current style/effects are generated as an ASSA subtitle and rendered
        // on top via libass (sub-add/sub-reload) - see TransparentSubtitlesViewModel.LoadVideoPreview.
        // Single mode always has a video (no video forces batch mode), so the player can
        // fully replace the old static image preview there.
        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = true;
        vm.VideoPlayerControl.FullScreenCommand = vm.PreviewFullScreenCommand;
        vm.VideoPlayerControl.MinHeight = 180;
        vm.VideoPlayerControl.HorizontalAlignment = HorizontalAlignment.Stretch;
        vm.VideoPlayerControl.VerticalAlignment = VerticalAlignment.Stretch;
        AutomationProperties.SetName(vm.VideoPlayerControl, Se.Language.General.Preview);

        // A plain border: the native video surface on macOS disappears under a rounded, clipping one
        return new Border
        {
            Child = vm.VideoPlayerControl,
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.IsBatchMode)) { Converter = InverseBooleanConverter.Instance },
        };
    }

    /// <summary>
    /// Cut range (from → to, each with a pick-from-video button) and the source video's name and size.
    /// </summary>
    private static Border MakeCutRow(TransparentSubtitlesViewModel vm)
    {
        TimeCodeUpDown Time(string property, string name)
        {
            var time = new TimeCodeUpDown
            {
                [!TimeCodeUpDown.ValueProperty] = new Binding(property),
                DataContext = vm,
                VerticalAlignment = VerticalAlignment.Center,
                [!TimeCodeUpDown.IsEnabledProperty] = new Binding(nameof(vm.IsCutActive)),
            };
            AutomationProperties.SetName(time, name);
            return time;
        }

        var pickFrom = EditorChrome.CreateToolButton("fa-solid fa-film", Se.Language.Video.BurnIn.FromTime, vm.BrowseCutFromCommand, 28);
        pickFrom.Bind(IsEnabledProperty, new Binding(nameof(vm.IsCutActive)));
        var pickTo = EditorChrome.CreateToolButton("fa-solid fa-film", Se.Language.Video.BurnIn.ToTime, vm.BrowseCutToCommand, 28);
        pickTo.Bind(IsEnabledProperty, new Binding(nameof(vm.IsCutActive)));

        var cut = Horizontal(
            Switch(nameof(vm.IsCutActive), Se.Language.General.Cut),
            new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-scissors", FontSize = 13, Foreground = EditorChrome.IconBrush, VerticalAlignment = VerticalAlignment.Center },
            new TextBlock { Text = Se.Language.General.Cut, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) },
            Time(nameof(vm.CutFrom), Se.Language.Video.BurnIn.FromTime),
            pickFrom,
            new TextBlock { Text = "→", Foreground = EditorChrome.DimTextBrush, VerticalAlignment = VerticalAlignment.Center },
            Time(nameof(vm.CutTo), Se.Language.Video.BurnIn.ToTime),
            pickTo);
        cut.Bind(IsVisibleProperty, new Binding(nameof(vm.IsBatchMode)) { Converter = InverseBooleanConverter.Instance });

        // The source video, single mode only
        var fileName = UiUtil.MakeFilePathLabel(vm, nameof(vm.VideoFileName));
        fileName.Foreground = EditorChrome.DimTextBrush;
        fileName.FontSize = 12;
        fileName.VerticalAlignment = VerticalAlignment.Center;
        var size = Hint(nameof(vm.VideoFileSize));
        var source = Horizontal(new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-file-video", FontSize = 12, Foreground = EditorChrome.FaintTextBrush, VerticalAlignment = VerticalAlignment.Center }, fileName, size);
        source.HorizontalAlignment = HorizontalAlignment.Right;
        source.Bind(IsVisibleProperty, new Binding(nameof(vm.IsBatchMode)) { Converter = InverseBooleanConverter.Instance });

        var row = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
            Margin = new Thickness(14, 0, 14, 10),
        };
        row.Add(cut, 0, 0);
        row.Add(source, 0, 1);
        return new Border { Child = row };
    }

    /// <summary>
    /// Resolution, frame rate and container side by side - there is no audio and no encoder to set,
    /// the output is always a transparent video.
    /// </summary>
    private static Border MakeOutputPanel(TransparentSubtitlesViewModel vm)
    {
        // Resolution: W x H, or the source's own size; the picker sets either
        var width = EditorChrome.MakeField(nameof(vm.VideoWidth), 0, 10_000, 76, "W");
        AutomationProperties.SetName(width, Se.Language.General.Width);
        var height = EditorChrome.MakeField(nameof(vm.VideoHeight), 0, 10_000, 76, "H");
        AutomationProperties.SetName(height, Se.Language.General.Height);
        var size = Horizontal(width, new TextBlock { Text = "×", Foreground = EditorChrome.DimTextBrush, VerticalAlignment = VerticalAlignment.Center }, height);
        size.Bind(IsVisibleProperty, new Binding(nameof(vm.UseSourceResolution)) { Converter = InverseBooleanConverter.Instance });
        var source = new TextBlock
        {
            Text = Se.Language.General.UseSourceResolution,
            FontSize = 12.5,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.IsVisibleProperty] = new Binding(nameof(vm.UseSourceResolution)),
        };
        var pick = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.Resolution, vm.BrowseResolutionCommand, 28);
        var resolution = new Grid { ColumnSpacing = 4, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) } };
        var sizeOrSource = new Panel();
        sizeOrSource.Children.Add(size);
        sizeOrSource.Children.Add(source);
        resolution.Add(sizeOrSource, 0, 0);
        resolution.Add(pick, 0, 1);

        var frameRate = Combo(vm.FrameRates, vm, nameof(vm.SelectedFrameRate), Se.Language.General.FrameRate).WithFrameRateDisplay();
        var extension = Combo(vm.VideoExtensions, vm, nameof(vm.SelectedVideoExtension), Se.Language.General.VideoExtension);

        var columns = new Grid
        {
            ColumnSpacing = 28,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        columns.Add(Row(Se.Language.General.Resolution, resolution, autoLabelWidth: true), 0, 0);
        columns.Add(Row(Se.Language.General.FrameRate, frameRate, autoLabelWidth: true), 0, 1);
        columns.Add(Row(Se.Language.General.VideoExtension, extension, autoLabelWidth: true), 0, 2);

        return new Border
        {
            Child = EditorChrome.MakeEditorSection(Se.Language.Video.BurnIn.VideoOutput, columns, first: true),
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        };
    }

    private static Border MakeBatchView(TransparentSubtitlesViewModel vm)
    {
        // No header sorting: the batch queue is processed top-to-bottom, so the list
        // order is meaningful.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        AutomationProperties.SetName(dataGrid, Se.Language.General.BatchMode);
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.JobItems;
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.SubtitleFileName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(BurnInJobItem.SubtitleFileNameShort)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Size,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(BurnInJobItem.Resolution)),
            Width = new GridLength(100),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.VideoFile,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(BurnInJobItem.InputVideoFileNameShort)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Status,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(BurnInJobItem.Status)),
            Width = new GridLength(120),
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedJobItem)) { Source = vm });

        var actions = Horizontal(
            EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.AddDotDotDot, vm.AddCommand, 26),
            EditorChrome.CreateToolButton("fa-solid fa-minus", Se.Language.General.Remove, vm.RemoveCommand, 26),
            EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Clear, vm.ClearCommand, 26),
            EditorChrome.MakeToolbarSeparator(),
            EditorChrome.CreateToolButton("fa-solid fa-file-video", Se.Language.General.PickVideoFile, vm.PickVideoFileCommand, 26));
        actions.Spacing = 2;

        // Batch items are separate files (possibly with no video at all), so there is no player
        // here: a static image rendered from the current style settings shows the look instead.
        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.None, // Prevents stretching of the image
        };
        var imagePreview = new ScrollViewer
        {
            Content = image,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = 140,
        };
        var preview = new Border
        {
            Child = imagePreview,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(6),
        };

        // Where the results go
        var outputFolder = UiUtil.MakeLink(string.Empty, vm.OpenOutputFolderCommand)
            .WithFilePathText(vm, nameof(vm.OutputFolder))
            .WithBindVisible(vm, nameof(vm.UseOutputFolderVisible));
        var useSourceFolder = EditorChrome.MakeDimLabel(Se.Language.General.UseSourceFolder, new Thickness(0));
        useSourceFolder.Bind(IsVisibleProperty, new Binding(nameof(vm.UseSourceFolderVisible)));
        var output = Horizontal(
            new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-folder", FontSize = 12, Foreground = EditorChrome.FaintTextBrush, VerticalAlignment = VerticalAlignment.Center },
            outputFolder,
            useSourceFolder,
            EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.Video.BurnIn.OutputProperties, vm.OutputPropertiesCommand, 26));

        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.BatchMode));

        var grid = new Grid
        {
            RowSpacing = 8,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };
        grid.Add(header, 0, 0);
        grid.Add(dataGrid, 1, 0);
        grid.Add(output, 2, 0);
        grid.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Preview), 3, 0);
        grid.Add(preview, 4, 0);

        return new Border
        {
            Child = grid,
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.IsBatchMode)),
        };
    }

    private Border MakeProgressView(TransparentSubtitlesViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var statusText = Hint(nameof(vm.ProgressText));
        statusText.FontSize = 12;

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

    private static Border MakeButtonBar(TransparentSubtitlesViewModel vm)
    {
        // Single / batch switch - batch is not available while generating, single only when a
        // video was loaded
        Button Mode(string text, System.Windows.Input.ICommand command, bool batch)
        {
            var button = new Button
            {
                Content = new TextBlock { Text = text, FontSize = 12.5, Margin = new Thickness(12, 0) },
                Height = 28,
                Command = command,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            button.Classes.Add("tool");
            AutomationProperties.SetName(button, text);

            void Update()
            {
                button.Classes.Set("active", vm.IsBatchMode == batch);
                button.IsEnabled = !vm.IsGenerating && (batch || !vm.IsBatchMode || vm.IsSingleModeVisible);
            }

            Update();
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(vm.IsBatchMode) or nameof(vm.IsGenerating) or nameof(vm.IsSingleModeVisible))
                {
                    Update();
                }
            };
            return button;
        }

        var modes = new Border
        {
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(2),
            VerticalAlignment = VerticalAlignment.Center,
            Child = Horizontal(Mode(Se.Language.General.SingleMode, vm.SingleModeCommand, false), Mode(Se.Language.General.BatchMode, vm.BatchModeCommand, true)),
        };
        ((StackPanel)modes.Child).Spacing = 2;

        var buttonGenerate = new SeSplitButton
        {
            Content = Se.Language.General.Generate,
            Command = vm.GenerateCommand,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.Video.PromptForFfmpegParamsAndGenerate,
                        Command = vm.PromptFfmpegParametersAndGeenrateCommand,
                    },
                }
            }
        };
        buttonGenerate.Classes.Add("primary");
        buttonGenerate.Bind(SplitButton.IsEnabledProperty, new Binding(nameof(vm.IsGenerating)) { Converter = InverseBooleanConverter.Instance });
        AutomationProperties.SetName(buttonGenerate, Se.Language.General.Generate);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindEnabled(nameof(vm.IsGenerating), InverseBooleanConverter.Instance);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = Horizontal(buttonCancel, buttonOk, buttonGenerate);
        buttons.Spacing = 10;

        var bar = new DockPanel();
        DockPanel.SetDock(modes, Dock.Left);
        DockPanel.SetDock(buttons, Dock.Right);
        bar.Children.Add(modes);
        bar.Children.Add(buttons);
        bar.Children.Add(new Border());

        return EditorChrome.WrapButtonBar(bar);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
