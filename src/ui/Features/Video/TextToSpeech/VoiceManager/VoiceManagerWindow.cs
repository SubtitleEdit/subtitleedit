using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Optris.Icons.Avalonia;
using System.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager;

public class VoiceManagerWindow : Window
{
    private readonly VoiceManagerViewModel _vm;
    private readonly MenuFlyout _copyToFlyout = new();
    private static readonly IBrush InstalledBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0xB0, 0x43));
    private static readonly IBrush NotInstalledBrush = new SolidColorBrush(Color.FromArgb(0x70, 0x80, 0x80, 0x80));

    public VoiceManagerWindow(VoiceManagerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.VoiceManagerTitle;
        Width = 1000;
        Height = 680;
        MinWidth = 720;
        MinHeight = 480;
        CanResize = true;
        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var enginePane = MakeEnginePane(vm);
        var toolbar = MakeToolbar(vm);
        var grid = MakeVoiceGrid(vm);
        var details = MakeDetailsPane(vm);
        var footer = MakeFooter(vm);

        var rightGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            RowSpacing = 8,
        };
        rightGrid.Add(toolbar, 0);
        rightGrid.Add(grid, 1);
        rightGrid.Add(details, 2);

        var mainGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(240) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnSpacing = 12,
            Margin = UiUtil.MakeWindowMargin(),
        };
        mainGrid.Add(enginePane, 0, 0);
        mainGrid.Add(rightGrid, 0, 1);
        mainGrid.Add(footer, 1, 0, 1, 2);
        Content = mainGrid;

        vm.PropertyChanged += OnViewModelPropertyChanged;
        Closed += (_, _) => vm.PropertyChanged -= OnViewModelPropertyChanged;
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        Loaded += (_, _) => UiUtil.RestoreWindowPosition(this);
        UiUtil.FocusOnFirstActivation(this, () => TableViewExtras.FocusRow(_voiceGrid!));
    }

    private static Control MakeEnginePane(VoiceManagerViewModel vm)
    {
        var header = new TextBlock
        {
            Text = Se.Language.General.Engine,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(2, 0, 0, 4),
        };

        var listBox = new ListBox
        {
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Engines)),
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedEngine)) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<VoiceManagerEngineItem>((item, _) =>
            {
                // Installed dot: green once the engine reports its binaries/models present, grey
                // when not, hidden until the async check has answered.
                var dot = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 7, 0),
                    [!Shape.FillProperty] = new Binding(nameof(VoiceManagerEngineItem.IsInstalled))
                    {
                        Converter = new FuncValueConverter<bool?, IBrush>(v => v == true ? InstalledBrush : NotInstalledBrush),
                    },
                    [!IsVisibleProperty] = new Binding(nameof(VoiceManagerEngineItem.IsInstalled))
                    {
                        Converter = new FuncValueConverter<bool?, bool>(v => v != null),
                    },
                };
                dot.Bind(ToolTip.TipProperty, new Binding(nameof(VoiceManagerEngineItem.IsInstalled))
                {
                    Converter = new FuncValueConverter<bool?, string>(v => v == true ? Se.Language.General.Installed : Se.Language.General.NotInstalled),
                });

                var icon = new Icon
                {
                    Value = item.Icon,
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = UiUtil.GetTextColor(0.95d),
                    Margin = new Thickness(0, 0, 8, 0),
                };
                Grid.SetColumn(icon, 1);

                var name = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    [!TextBlock.TextProperty] = new Binding(nameof(VoiceManagerEngineItem.Name)),
                };
                Grid.SetColumn(name, 2);

                var badge = new Border
                {
                    CornerRadius = new CornerRadius(9),
                    Padding = new Thickness(7, 0, 7, 1),
                    Margin = new Thickness(6, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromArgb(34, 128, 128, 128)),
                    Child = new TextBlock
                    {
                        FontSize = UiUtil.ScaledFontSize(11),
                        Opacity = 0.85,
                        [!TextBlock.TextProperty] = new Binding(nameof(VoiceManagerEngineItem.CountText)),
                    },
                    [!IsVisibleProperty] = new Binding(nameof(VoiceManagerEngineItem.HasCount)),
                };
                Grid.SetColumn(badge, 3);

                return new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(15) },
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = GridLength.Auto },
                    },
                    Margin = new Thickness(0, 1),
                    Children = { dot, icon, name, badge },
                };
            }, true),
        };

        var panel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };
        panel.Add(header, 0);
        panel.Add(listBox, 1);
        return panel;
    }

    private Control MakeToolbar(VoiceManagerViewModel vm)
    {
        var buttonPlay = UiUtil.MakeButton(vm.PlayOrStopCommand, IconNames.Play, Se.Language.General.Play)
            .WithBindEnabled(nameof(vm.IsPlayEnabled));
        _buttonPlay = buttonPlay;

        var buttonRename = UiUtil.MakeButton(vm.RenameVoiceCommand, IconNames.Pencil, Se.Language.Video.TextToSpeech.RenameVoiceDotDotDot)
            .WithBindEnabled(nameof(vm.IsFileVoiceSelected));
        var buttonDelete = UiUtil.MakeButton(vm.DeleteVoiceCommand, IconNames.Trash, Se.Language.Video.TextToSpeech.DeleteVoiceDotDotDot)
            .WithBindEnabled(nameof(vm.IsFileVoiceSelected));

        var buttonCopyTo = UiUtil.MakeButton(null, IconNames.Copy, Se.Language.Video.TextToSpeech.CopyVoiceToDotDotDot)
            .WithBindEnabled(nameof(vm.IsFileVoiceSelected));
        buttonCopyTo.Click += (_, _) => _copyToFlyout.ShowAt(buttonCopyTo);
        // Built when opened: the targets are "every cloning engine but the current one", which
        // changes with the engine pick. It is a small menu, so rebuilding is cheaper than syncing.
        _copyToFlyout.Opening += (_, _) => RebuildCopyToMenu(vm);

        var buttonImport = UiUtil.MakeButton(vm.ImportVoiceCommand, IconNames.Import, Se.Language.Video.TextToSpeech.ImportVoiceDotDotDot)
            .WithBindIsVisible(nameof(vm.IsImportVisible));
        var buttonPacks = UiUtil.MakeButton(vm.DownloadVoicePacksCommand, IconNames.CloudDownload, Se.Language.Video.TextToSpeech.DownloadVoicePacksDotDotDot)
            .WithBindIsVisible(nameof(vm.IsVoicePacksVisible));
        var buttonFolder = UiUtil.MakeButton(vm.OpenVoicesFolderCommand, IconNames.FolderOpen, Se.Language.Video.TextToSpeech.OpenVoicesFolder)
            .WithBindEnabled(nameof(vm.IsOpenFolderEnabled));
        var buttonRefresh = UiUtil.MakeButton(vm.RefreshCommand, IconNames.Refresh, Se.Language.General.Refresh);

        var searchBox = new TextBox
        {
            Watermark = Se.Language.Video.TextToSpeech.SearchVoices,
            MinWidth = 180,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.FilterText)) { Mode = BindingMode.TwoWay },
        };
        searchBox.WithSearchAndClearIcons();

        var left = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Children =
            {
                buttonPlay,
                MakeToolbarSeparator(),
                buttonRename,
                buttonDelete,
                buttonCopyTo,
                MakeToolbarSeparator(),
                buttonImport,
                buttonPacks,
                buttonFolder,
                buttonRefresh,
            },
        };

        var bar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        bar.Add(left, 0, 0);
        bar.Add(searchBox, 0, 1);
        return bar;
    }

    private static Control MakeToolbarSeparator() => new Border
    {
        Width = 1,
        Margin = new Thickness(6, 4),
        Background = UiUtil.GetBorderBrush(),
    };

    private void RebuildCopyToMenu(VoiceManagerViewModel vm)
    {
        _copyToFlyout.Items.Clear();
        foreach (var target in vm.CopyTargets)
        {
            var item = new Avalonia.Controls.MenuItem
            {
                Header = target.Name,
                Command = vm.CopyVoiceToCommand,
                CommandParameter = target,
            };
            var requirement = VoiceReferenceTranscript.GetRequirement(target);
            if (requirement == TranscriptRequirement.Required)
            {
                item.Icon = new Icon { Value = IconNames.FormTextBox, FontSize = 14 };
            }

            _copyToFlyout.Items.Add(item);
        }
    }

    private TableView? _voiceGrid;
    private Button? _buttonPlay;

    private Control MakeVoiceGrid(VoiceManagerViewModel vm)
    {
        var tableView = TableViewExtras.MakeTableView(multiSelect: false);
        tableView.Width = double.NaN;
        tableView.Height = double.NaN;
        tableView[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Voices));
        tableView[!TableView.SelectedItemProperty] = new Binding(nameof(vm.SelectedVoice)) { Mode = BindingMode.TwoWay };
        _voiceGrid = tableView;
        tableView.WithAccessibleName(Se.Language.Video.TextToSpeech.VoiceManagerTitle); // #12087

        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
            NameBinding = new Binding(nameof(VoiceManagerRow.DisplayName)),
            CellTemplate = new FuncDataTemplate<VoiceManagerRow>((item, _) =>
            {
                var icon = new Icon
                {
                    Value = item.KindIcon,
                    FontSize = 15,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = UiUtil.GetTextColor(item.Kind == VoiceKind.Clone ? 0.95d : 0.55d),
                    Margin = new Thickness(6, 0, 8, 0),
                };
                var text = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    [!TextBlock.TextProperty] = new Binding(nameof(VoiceManagerRow.DisplayName)),
                };
                return new StackPanel { Orientation = Orientation.Horizontal, Children = { icon, text } };
            }, true),
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Type,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(120),
            NameBinding = new Binding(nameof(VoiceManagerRow.KindText)),
            CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(VoiceManagerRow.KindText)),
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(80),
            NameBinding = new Binding(nameof(VoiceManagerRow.Duration)),
            CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(VoiceManagerRow.Duration)),
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Format,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(170),
            NameBinding = new Binding(nameof(VoiceManagerRow.Format)),
            CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(VoiceManagerRow.Format)),
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Video.TextToSpeech.Transcript,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(90),
            NameBinding = new Binding(nameof(VoiceManagerRow.TranscriptText)),
            CellTemplate = new FuncDataTemplate<VoiceManagerRow>((_, _) => new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(0x2E, 0x8B, 0x57)),
                [!TextBlock.TextProperty] = new Binding(nameof(VoiceManagerRow.TranscriptText)),
            }, true),
        });

        TableViewExtras.AttachListNavigation(tableView);
        tableView.DoubleTapped += (_, _) =>
        {
            if (vm.IsPlayEnabled)
            {
                vm.PlayOrStopCommand.Execute(null);
            }
        };

        var menuPlay = new Avalonia.Controls.MenuItem { Header = Se.Language.General.Play, Command = vm.PlayOrStopCommand };
        var menuRename = new Avalonia.Controls.MenuItem { Header = Se.Language.Video.TextToSpeech.RenameVoiceDotDotDot, Command = vm.RenameVoiceCommand };
        var menuDelete = new Avalonia.Controls.MenuItem { Header = Se.Language.Video.TextToSpeech.DeleteVoiceDotDotDot, Command = vm.DeleteVoiceCommand };
        var menuCopyTo = new Avalonia.Controls.MenuItem { Header = Se.Language.Video.TextToSpeech.CopyVoiceTo };
        var flyout = new MenuFlyout { Items = { menuPlay, menuRename, menuDelete, menuCopyTo } };
        flyout.Opening += (_, _) =>
        {
            menuPlay.IsEnabled = vm.IsPlayEnabled;
            menuRename.IsEnabled = vm.IsFileVoiceSelected;
            menuDelete.IsEnabled = vm.IsFileVoiceSelected;
            menuCopyTo.IsEnabled = vm.IsFileVoiceSelected;
            menuCopyTo.Items.Clear();
            foreach (var target in vm.CopyTargets)
            {
                menuCopyTo.Items.Add(new Avalonia.Controls.MenuItem { Header = target.Name, Command = vm.CopyVoiceToCommand, CommandParameter = target });
            }
        };
        tableView.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(tableView);

        var emptyText = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            MaxWidth = 420,
            Foreground = UiUtil.GetTextColor(0.6d),
            IsHitTestVisible = false,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.EmptyText)),
            [!IsVisibleProperty] = new Binding(nameof(vm.IsEmptyTextVisible)),
        };

        var loading = new ProgressBar
        {
            IsIndeterminate = true,
            Height = 4,
            VerticalAlignment = VerticalAlignment.Top,
            [!IsVisibleProperty] = new Binding(nameof(vm.IsLoading)),
        };

        return new Grid { Children = { tableView, emptyText, loading } };
    }

    private Control MakeDetailsPane(VoiceManagerViewModel vm)
    {
        var settings = Se.Settings.Waveform;
        var audioVisualizer = new AudioVisualizer
        {
            DrawGridLines = settings.DrawGridLines,
            WaveformColor = settings.WaveformColor.FromHexToColor(),
            WaveformBackgroundColor = settings.WaveformBackgroundColor.FromHexToColor(),
            WaveformSelectedColor = settings.WaveformSelectedColor.FromHexToColor(),
            WaveformCursorColor = settings.WaveformCursorColor.FromHexToColor(),
            WaveformShotChangeColor = settings.WaveformShotChangeColor.FromHexToColor(),
            WaveformParagraphLeftColor = settings.WaveformParagraphLeftColor.FromHexToColor(),
            WaveformParagraphRightColor = settings.WaveformParagraphRightColor.FromHexToColor(),
            WaveformFancyHighColor = settings.WaveformFancyHighColor.FromHexToColor(),
            ParagraphBackground = settings.ParagraphBackground.FromHexToColor(),
            ParagraphSelectedBackground = settings.ParagraphSelectedBackground.FromHexToColor(),
            WaveformDrawStyle = InitWaveform.GetWaveformDrawStyle(settings.WaveformDrawStyle),
            WaveformHeightPercentage = settings.SpectrogramCombinedWaveformHeight,
            InvertMouseWheel = settings.InvertMouseWheel,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = 110,
            IsReadOnly = true,
            FocusOnMouseOver = false,
        };
        vm.AudioVisualizer = audioVisualizer;
        audioVisualizer.Bind(AudioVisualizer.WavePeaksProperty, new Binding(nameof(vm.WavePeakData)));
        audioVisualizer.SizeChanged += (_, _) => vm.FitWaveformToClip();
        // The control only raises OnPrimarySingleClicked when something listens to
        // OnVideoPositionChanged, hence the empty handler.
        audioVisualizer.OnVideoPositionChanged += (_, _) => { };
        audioVisualizer.OnPrimarySingleClicked += (_, e) => vm.SeekTo(e.Seconds);

        var waveformBorder = UiUtil.MakeBorderForControlNoPadding(audioVisualizer);

        var infoText = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(11.5),
            Foreground = UiUtil.GetTextColor(0.65d),
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.VoiceInfoText)),
        };

        var transcriptHeader = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.Transcript,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var transcriptHint = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(11),
            Foreground = UiUtil.GetTextColor(0.6d),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(10, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TranscriptHint)),
        };
        var buttonStt = UiUtil.MakeButton(vm.TranscribeWithSpeechToTextCommand, IconNames.Waveform, Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot);
        var buttonSave = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.SaveTranscript, vm.SaveTranscriptCommand)
            .WithIconLeft(IconNames.ContentSave)
            .WithBindEnabled(nameof(vm.IsTranscriptDirty));
        var transcriptBar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 4,
        };
        transcriptBar.Add(transcriptHeader, 0, 0);
        transcriptBar.Add(transcriptHint, 0, 1);
        transcriptBar.Add(buttonStt, 0, 2);
        transcriptBar.Add(buttonSave, 0, 3);

        var transcriptBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 64,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.Transcript)) { Mode = BindingMode.TwoWay },
        };

        var transcriptPanel = new StackPanel
        {
            Spacing = 4,
            Children = { transcriptBar, transcriptBox },
            [!IsVisibleProperty] = new Binding(nameof(vm.IsTranscriptVisible)),
        };

        return new StackPanel
        {
            Spacing = 6,
            Children = { waveformBorder, infoText, transcriptPanel },
        };
    }

    private static Control MakeFooter(VoiceManagerViewModel vm)
    {
        var status = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = UiUtil.GetTextColor(0.6d),
            FontSize = UiUtil.ScaledFontSize(11.5),
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
        };
        var count = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = UiUtil.GetTextColor(0.6d),
            FontSize = UiUtil.ScaledFontSize(11.5),
            Margin = new Thickness(12, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.VoiceCountText)),
        };
        var buttonClose = UiUtil.MakeButtonDone(vm.CloseCommand);
        var buttons = UiUtil.MakeButtonBar(buttonClose);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        grid.Add(status, 0, 0);
        grid.Add(count, 0, 1);
        grid.Add(buttons, 0, 2);
        return grid;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VoiceManagerViewModel.IsPlaying) && _buttonPlay != null)
        {
            Attached.SetIcon(_buttonPlay, _vm.IsPlaying ? IconNames.StopCircle : IconNames.Play);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled)
        {
            _vm.OnKeyDown(e);
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing();
    }
}
