using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

/// <summary>
/// Remove burned-in subtitles - review, then remove. Left: the frame preview (mask on top, or
/// the inpainted result) with frame/line navigation, and the timeline below it with the OCR
/// lines as boxes on the waveform. Right: the lines (include/leave out) and the settings.
/// Bottom: progress, and once done "Open video" / "Show in folder".
/// </summary>
public class RemoveBurnedInWindow : Window
{
    private readonly RemoveBurnedInViewModel _vm;
    private const double LabelWidth = 150;

    public RemoveBurnedInWindow(RemoveBurnedInViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.RemoveBurnedIn.Title;
        Width = 1320;
        Height = 880;
        MinWidth = 980;
        MinHeight = 640;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.VideoTool);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var left = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // preview
                new RowDefinition { Height = new GridLength(170) }, // timeline
            },
        };
        left.Add(MakePreviewSection(vm), 0);
        left.Add(MakeTimelineSection(vm), 1);

        var right = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // lines
                new RowDefinition { Height = GridLength.Auto }, // settings
            },
        };
        right.Add(MakeLinesSection(vm), 0);
        right.Add(MakeSettingsSections(vm), 1);

        var columns = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(430) },
            },
        };
        columns.Add(left, 0, 0);
        columns.Add(new Border
        {
            Child = right,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        var progressView = EditorChrome.MakeProgressStrip(progressBar, nameof(vm.ProgressText), nameof(vm.IsRunning));

        var l = Se.Language.Video.RemoveBurnedIn;
        var buttonStart = EditorChrome.MakeActionButton(IconNames.PlayCircle, l.Start, vm.StartCommand)
            .WithBindEnabled(nameof(vm.CanStart)).AsPrimary();
        var buttonClose = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = EditorChrome.MakeButtonBar(MakeDonePanel(vm), buttonClose, buttonStart);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
        };
        grid.Add(EditorChrome.MakePanel(columns), 0);
        grid.Add(progressView, 1);
        grid.Add(buttonPanel, 2);
        Content = grid;

        Loaded += (_, _) => vm.OnLoaded();
    }

    // ── preview ──────────────────────────────────────────────────────

    private static Control MakePreviewSection(RemoveBurnedInViewModel vm)
    {
        var l = Se.Language.Video.RemoveBurnedIn;

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewBitmap)),
        };
        AutomationProperties.SetName(image, l.Preview);

        var busy = new ProgressBar
        {
            IsIndeterminate = true,
            Height = 3,
            MinHeight = 3,
            VerticalAlignment = VerticalAlignment.Top,
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsPreviewBusy)),
        };

        var status = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 3),
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Child = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 12,
                [!TextBlock.TextProperty] = new Binding(nameof(vm.PreviewStatus)),
            },
        };

        // The selected rectangle line's box, drawn and edited in video pixels over the frame.
        var rectangleSelector = new CropAreaSelector
        {
            DimOutside = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsRectangleSelected)),
        };
        rectangleSelector.Bind(CropAreaSelector.VideoWidthProperty, new Binding(nameof(vm.VideoWidth)));
        rectangleSelector.Bind(CropAreaSelector.VideoHeightProperty, new Binding(nameof(vm.VideoHeight)));
        rectangleSelector.Bind(CropAreaSelector.SelectionXProperty, new Binding(nameof(vm.SelectedRectX)) { Mode = BindingMode.TwoWay });
        rectangleSelector.Bind(CropAreaSelector.SelectionYProperty, new Binding(nameof(vm.SelectedRectY)) { Mode = BindingMode.TwoWay });
        rectangleSelector.Bind(CropAreaSelector.SelectionWidthProperty, new Binding(nameof(vm.SelectedRectWidth)) { Mode = BindingMode.TwoWay });
        rectangleSelector.Bind(CropAreaSelector.SelectionHeightProperty, new Binding(nameof(vm.SelectedRectHeight)) { Mode = BindingMode.TwoWay });
        AutomationProperties.SetName(rectangleSelector, l.RectangleHint);
        SetHint(rectangleSelector, l.RectangleHint);

        var frame = new Panel
        {
            Background = Brushes.Black,
            ClipToBounds = true,
            Children = { image, rectangleSelector, busy, status },
        };

        var navigation = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0),
            Children =
            {
                EditorChrome.CreateToolButton(IconNames.ChevronDoubleLeft, l.PreviousLine, vm.PreviousLineCommand, 30, "PageUp"),
                EditorChrome.CreateToolButton(IconNames.ChevronLeft, l.PreviousFrame, vm.PreviousFrameCommand, 30, "←"),
                new TextBlock
                {
                    MinWidth = 110,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = new FontFamily("Cascadia Mono,Consolas,Menlo,Monaco,monospace"),
                    [!TextBlock.TextProperty] = new Binding(nameof(vm.PositionText)),
                },
                EditorChrome.CreateToolButton(IconNames.ChevronRight, l.NextFrame, vm.NextFrameCommand, 30, "→"),
                EditorChrome.CreateToolButton(IconNames.ChevronDoubleRight, l.NextLine, vm.NextLineCommand, 30, "PageDown"),
            },
        };

        var content = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(navigation, Dock.Bottom);
        content.Children.Add(navigation);
        content.Children.Add(EditorChrome.MakeVideoFrame(frame));

        var resultToggle = EditorChrome.MakeSwitchRow(nameof(vm.ShowResult), l.ShowResult);
        SetHint(resultToggle, l.ShowResultHint);
        return EditorChrome.MakeDockedFillSection(l.Preview, content, resultToggle, first: true);
    }

    // ── timeline ─────────────────────────────────────────────────────

    private static Control MakeTimelineSection(RemoveBurnedInViewModel vm)
    {
        var l = Se.Language.Video.RemoveBurnedIn;
        var colors = WaveformColors.Current;
        var av = new AudioVisualizer
        {
            Width = double.NaN,
            IsReadOnly = false,

            // OCR lines often touch (no gap): with the default 0.1 s minimum gap such a line could
            // not be moved or resized against its neighbour. Overlap stays blocked.
            MinGapSeconds = 0,
            DrawGridLines = Se.Settings.Waveform.DrawGridLines,
            WaveformColor = colors.Waveform,
            WaveformSelectedColor = colors.Selected,
            WaveformGridColor = colors.Grid,
        };
        av.OnVideoPositionChanged += vm.AudioVisualizerPositionChanged;
        av.OnPrimarySingleClicked += vm.AudioVisualizerParagraphClicked;
        av.OnDragEnded += vm.AudioVisualizerDragEnded;
        av.OnSelectRequested += vm.AudioVisualizerSelectRequested;
        av.OnNewSelectionInsert += vm.AudioVisualizerNewSelectionInsert;
        av.FlyoutMenuOpening += vm.AudioVisualizerFlyoutMenuOpening;
        AutomationProperties.SetName(av, Se.Language.General.Waveform);
        vm.AudioVisualizer = av;

        var hint = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                EditorChrome.MakeBoundHint(nameof(vm.WaveformStatus)),
                EditorChrome.MakeHint(l.TimelineHint),
            },
        };

        var frame = new Border
        {
            Child = av,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
        };
        return EditorChrome.MakeDockedFillSection(l.Timeline, frame, hint, headerGap: 6);
    }

    // ── lines ────────────────────────────────────────────────────────

    private static Control MakeLinesSection(RemoveBurnedInViewModel vm)
    {
        var l = Se.Language.Video.RemoveBurnedIn;
        var timeConverter = new TimeSpanToDisplayFullConverter();
        var table = TableViewExtras.MakeTableView(multiSelect: true);
        table.DataContext = vm;
        table.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Lines)) { Source = vm });
        table.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedLine)) { Source = vm, Mode = BindingMode.TwoWay });
        AutomationProperties.SetName(table, l.Lines);
        table.Columns.Add(new SeTableViewColumn
        {
            Header = string.Empty,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<RemoveBurnedInLineItem>(nameof(RemoveBurnedInLineItem.IsIncluded), focusable: true),
            Width = new GridLength(36),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(RemoveBurnedInLineItem.Number)),
            Width = new GridLength(40),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding("Line.StartTime") { Converter = timeConverter },
            Width = new GridLength(96),
        });
        table.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(RemoveBurnedInLineItem.Text)),
            Width = new GridLength(1, GridUnitType.Star),
        });

        var insert = EditorChrome.MakeTextToolButton(IconNames.Plus, l.InsertRectangle, vm.InsertRectangleCommand)
            .WithBindEnabled(nameof(vm.IsRunning), InverseBooleanConverter.Instance);
        SetHint(insert, l.InsertRectangleHint);
        var delete = EditorChrome.CreateToolButton(IconNames.Trash, Se.Language.General.Delete, vm.DeleteLineCommand, 30, "Delete")
            .WithBindEnabled(nameof(vm.CanDeleteLine));

        // Right-click menu and Delete key: delete the selected lines, or insert a rectangle.
        List<RemoveBurnedInLineItem> SelectedItems() => table.SelectedItems?.OfType<RemoveBurnedInLineItem>().ToList() ?? new();
        var flyout = new MenuFlyout();
        var menuInsert = new MenuItem { Header = l.InsertRectangle, InputGesture = new KeyGesture(Key.Insert) };
        menuInsert.Click += (_, _) => vm.InsertRectangleCommand.Execute(null);
        var menuDelete = new MenuItem { Header = Se.Language.General.Delete, InputGesture = new KeyGesture(Key.Delete) };
        menuDelete.Click += (_, _) => vm.DeleteLines(SelectedItems());
        flyout.Items.Add(menuInsert);
        flyout.Items.Add(menuDelete);
        flyout.Opening += (_, _) =>
        {
            menuInsert.IsEnabled = !vm.IsRunning;
            menuDelete.IsEnabled = !vm.IsRunning && SelectedItems().Count > 0;
        };
        table.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(table);
        table.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Delete && e.KeyModifiers == KeyModifiers.None && e.Source is not TextBox)
            {
                vm.DeleteLines(SelectedItems());
                e.Handled = true;
            }
        };
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(0, 6, 0, 0),
            Children = { insert, delete },
        };

        var content = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(toolbar, Dock.Bottom);
        content.Children.Add(toolbar);
        content.Children.Add(EditorChrome.MakeListFrame(table));
        return EditorChrome.MakeDockedFillSection(l.Lines, content, EditorChrome.MakeBoundHint(nameof(vm.LinesSummary)), first: true);
    }

    // ── settings ─────────────────────────────────────────────────────

    private static Control MakeSettingsSections(RemoveBurnedInViewModel vm)
    {
        var l = Se.Language.Video.RemoveBurnedIn;

        var methodCombo = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Methods)),
            [!ComboBox.SelectedIndexProperty] = new Binding(nameof(vm.MethodIndex)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(methodCombo, l.Method);
        SetHint(methodCombo, l.MethodHint);

        var brightness = EditorChrome.MakeField(nameof(vm.BrightnessMinimum), 1, 255, 90);
        SetHint(brightness, Se.Language.Video.VideoOcr.TextBrightnessMinimumHint);
        var grow = EditorChrome.MakeField(nameof(vm.Grow), 0, 40, 90);
        SetHint(grow, l.GrowMaskHint);
        var extraWidth = EditorChrome.MakeField(nameof(vm.ExtraWidth), 0, 100, 90);
        SetHint(extraWidth, l.ExtraWidthHint);
        var outline = EditorChrome.MakeSwitchRow(nameof(vm.RequireOutline), l.RequireOutline);
        SetHint(outline, l.RequireOutlineHint);
        var temporal = EditorChrome.MakeSwitchRow(nameof(vm.Temporal), l.TemporalConsistency);
        SetHint(temporal, l.TemporalConsistencyHint);

        // Temporal consistency only applies to inpainting.
        temporal.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsInpaint)));

        var blurStrength = EditorChrome.MakeField(nameof(vm.BlurStrength), 1, 100, 90);
        SetHint(blurStrength, l.BlurStrengthHint);
        var blurRow = EditorChrome.MakeLabeledRow(l.BlurStrength, blurStrength, LabelWidth);
        blurRow.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsBlur)));

        var maskRows = new StackPanel { Spacing = 8 };
        maskRows.Children.Add(EditorChrome.MakeLabeledRow(l.Method, methodCombo, LabelWidth));
        maskRows.Children.Add(blurRow);
        maskRows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.Video.VideoOcr.TextBrightnessMinimum, brightness, LabelWidth));
        maskRows.Children.Add(EditorChrome.MakeLabeledRow(l.GrowMask, grow, LabelWidth));
        maskRows.Children.Add(EditorChrome.MakeLabeledRow(l.ExtraWidth, extraWidth, LabelWidth));
        maskRows.Children.Add(outline);
        maskRows.Children.Add(temporal);

        var outputBox = EditorChrome.MakeTextField(nameof(vm.OutputFileName), l.OutputFile);
        var browse = UiUtil.MakeButtonBrowse(vm.BrowseOutputCommand);
        browse.Margin = new Thickness(6, 0, 0, 0);
        var outputPanel = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(browse, Dock.Right);
        outputPanel.Children.Add(browse);
        outputPanel.Children.Add(outputBox);
        var crf = EditorChrome.MakeField(nameof(vm.Crf), 0, 51, 90);
        SetHint(crf, l.QualityHint);
        var outputRows = new StackPanel { Spacing = 8 };
        outputRows.Children.Add(EditorChrome.MakeLabeledRow(l.OutputFile, outputPanel, LabelWidth));
        outputRows.Children.Add(EditorChrome.MakeLabeledRow(l.Quality, crf, LabelWidth));

        // The black boxes go to the main window as an ASSA subtitle - no video is written.
        var outputSection = EditorChrome.MakeEditorSection(Se.Language.Video.BurnIn.VideoOutput, outputRows);
        outputSection.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.WritesVideo)));

        return new ScrollViewer
        {
            MaxHeight = 430,
            Content = new StackPanel
            {
                Children =
                {
                    EditorChrome.MakeEditorSection(l.TextMask, maskRows),
                    outputSection,
                },
            },
        };
    }

    // ── done ─────────────────────────────────────────────────────────

    private static Control MakeDonePanel(RemoveBurnedInViewModel vm)
    {
        var l = Se.Language.Video.RemoveBurnedIn;
        var icon = new Optris.Icons.Avalonia.Icon
        {
            Value = IconNames.CheckCircle,
            FontSize = 18,
            Foreground = EditorChrome.SuccessTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 460,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.DoneText)),
        };
        var open = EditorChrome.MakeActionButton(IconNames.MovieOpenOutline, l.OpenVideo, vm.OpenVideoCommand);
        var folder = EditorChrome.MakeActionButton(IconNames.FolderOpen, Se.Language.General.OpenContainingFolder, vm.ShowInFolderCommand);
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = { icon, text, open, folder },
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsDone)),
        };
    }

    /// <summary>Tooltips follow the "Show button hints" setting.</summary>
    private static void SetHint(Control control, string hint)
    {
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(control, hint);
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (_vm.IsRunning && !_vm.CloseConfirmed)
        {
            // Same question as Escape/Cancel before stopping a run.
            e.Cancel = true;
            _ = _vm.ConfirmCloseWhileRunning();
            return;
        }

        _vm.OnClosing();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
