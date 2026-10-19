using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Features.Video.RemuxVideo;

/// <summary>
/// Remux, in the editor look (see <see cref="EditorChrome"/>): the audio and subtitle lists on the
/// left, the input video and the output file on the right; progress and Remux at the bottom.
/// </summary>
public class RemuxVideoWindow : Window
{
    private readonly RemuxVideoViewModel _vm;
    private Button? _buttonBrowseVideo;

    private const double LabelWidth = 96;

    public RemuxVideoWindow(RemuxVideoViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.RemuxVideoTitle;
        CanResize = true;
        Width = 920;
        Height = 660;
        MinWidth = 740;
        MinHeight = 500;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        AddStyles();

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        // The settings column scrolls on its own and the lists give up height first, so the
        // progress bar and the button bar stay reachable on a short screen.
        var settingsScroller = new ScrollViewer
        {
            Content = MakeSettingsPanel(vm),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        var right = new Border
        {
            Child = settingsScroller,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(320)),
            },
        };
        content.Add(MakeListsPanel(vm), 0, 0);
        content.Add(right, 0, 1);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto), // progress, while remuxing
                new RowDefinition(GridLength.Auto), // status + buttons
            },
        };
        root.Add(content, 0, 0);
        root.Add(MakeProgressView(vm), 1, 0);
        root.Add(MakeButtonBar(vm), 2, 0);
        Content = root;

        KeyDown += vm.KeyDown;
        Closing += delegate
        {
            vm.OnClosing();
            UiUtil.SaveWindowPosition(this);
        };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
        UiUtil.FocusOnFirstActivation(this, () => { _buttonBrowseVideo?.Focus(); });
    }

    private static Binding NotRemuxing(RemuxVideoViewModel vm) => new(nameof(vm.IsRemuxing)) { Converter = InverseBooleanConverter.Instance };

    /// <summary>
    /// The language strings are labels ("Output file:"); as section headers they go without the colon.
    /// </summary>
    private static string Caption(string text) => text.Trim().TrimEnd(':').TrimEnd();

    /// <summary>
    /// Compact combo boxes and text boxes, the file lists and the accent Remux button.
    /// </summary>
    private void AddStyles()
    {
        EditorChrome.AddComboBoxEllipsisStyle(Styles);
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
        Styles.Add(new Style(x => x.OfType<TextBox>().Class("path"))
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 4)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center),
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
        Styles.Add(new Style(x => x.OfType<ListBox>().Class("files"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
            },
        });
        Styles.Add(new Style(x => x.OfType<ListBox>().Class("files").Child().OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 6)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(5)),
                new Setter(Layoutable.MarginProperty, new Thickness(4, 2)),
            },
        });

        // Remux is the window's main action: both halves of the split button in the accent
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
    /// A label of fixed width and the control after it, so the rows line up.
    /// </summary>
    private static Grid Row(string label, Control content)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        var grid = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(LabelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
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

    /// <summary>
    /// A switch with its label beside it; the label wraps instead of being cut off.
    /// </summary>
    private static Grid SwitchRow(ToggleSwitch toggle, string text)
    {
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        row.Add(toggle, 0, 0);
        row.Add(new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap }, 0, 1);
        return row;
    }

    /// <summary>
    /// A file path box with a "..." browse button after it.
    /// </summary>
    private static Grid PathRow(TextBox textBox, Button browse)
    {
        var grid = new Grid
        {
            ColumnSpacing = 4,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(textBox, 0, 0);
        grid.Add(browse, 0, 1);
        return grid;
    }

    private Control MakeSettingsPanel(RemuxVideoViewModel vm)
    {
        var l = Se.Language.Video;

        // Input video
        var textBoxVideo = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.VideoFileName))
            .WithAccessibleName(l.RemuxVideoVideoFile); // the section header above is not a label of the box (#12087)
        textBoxVideo.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxVideo.Classes.Add("path");
        textBoxVideo.Bind(TextBox.IsEnabledProperty, NotRemuxing(vm));
        _buttonBrowseVideo = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.OpenVideoFile, vm.BrowseVideoCommand, 28);
        _buttonBrowseVideo.Bind(Button.IsEnabledProperty, NotRemuxing(vm));

        var videoSize = Hint(nameof(vm.VideoFileSize));
        var videoRows = new StackPanel { Spacing = 6 };
        videoRows.Children.Add(PathRow(textBoxVideo, _buttonBrowseVideo));
        videoRows.Children.Add(videoSize);

        // Output
        var comboBoxFormat = UiUtil.MakeComboBox(vm.OutputFormats, vm, nameof(vm.SelectedOutputFormat));
        comboBoxFormat.Width = double.NaN;
        comboBoxFormat.MinWidth = 100;
        comboBoxFormat.HorizontalAlignment = HorizontalAlignment.Left;
        comboBoxFormat.Bind(ComboBox.IsEnabledProperty, NotRemuxing(vm));
        AutomationProperties.SetName(comboBoxFormat, Caption(l.RemuxVideoOutputFormat));

        var textBoxOutput = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.OutputFileName));
        textBoxOutput.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxOutput.Classes.Add("path");
        textBoxOutput.Bind(TextBox.IsEnabledProperty, NotRemuxing(vm));
        AutomationProperties.SetName(textBoxOutput, Caption(l.RemuxVideoOutputFile));
        var buttonBrowseOutput = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Caption(l.RemuxVideoOutputFile), vm.BrowseOutputFileCommand, 28);
        buttonBrowseOutput.Bind(Button.IsEnabledProperty, NotRemuxing(vm));

        var switchFastStart = Switch(nameof(vm.FastStart), l.RemuxVideoFastStart);
        switchFastStart.Bind(ToggleSwitch.IsEnabledProperty, NotRemuxing(vm));
        var fastStartRow = SwitchRow(switchFastStart, l.RemuxVideoFastStart);
        fastStartRow.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsFastStartVisible)));

        // .mp4: the subtitles as mov_text tracks or as CEA-608 captions in the video (#16164)
        var comboBoxSubtitlesAs = UiUtil.MakeComboBox(vm.SubtitlesAsOptions, vm, nameof(vm.SelectedSubtitlesAs));
        comboBoxSubtitlesAs.Width = double.NaN;
        comboBoxSubtitlesAs.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboBoxSubtitlesAs.Bind(ComboBox.IsEnabledProperty, NotRemuxing(vm));
        AutomationProperties.SetName(comboBoxSubtitlesAs, Caption(l.RemuxVideoSubtitlesAs));
        var subtitlesAsRow = Row(Caption(l.RemuxVideoSubtitlesAs), comboBoxSubtitlesAs);
        subtitlesAsRow.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsSubtitlesAsVisible)));

        var captionsHint = new TextBlock
        {
            Text = l.RemuxVideoMp4CaptionsHint,
            FontSize = 11.5,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
        };
        captionsHint.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsMp4CaptionsHintVisible)));

        // Broadcast .scc/.mcc files start at the time of day (01:00:00:00); the offset is taken
        // off the caption times so they land in the video (#16220)
        var captionOffset = new TimeCodeUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            MinWidth = 150,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.CaptionOffset)) { Mode = BindingMode.TwoWay },
        };
        captionOffset.Bind(TimeCodeUpDown.IsEnabledProperty, NotRemuxing(vm));
        AutomationProperties.SetName(captionOffset, Caption(l.RemuxVideoCaptionOffset));
        var captionOffsetHint = new TextBlock
        {
            Text = l.RemuxVideoCaptionOffsetHint,
            FontSize = 11.5,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
        };
        var captionOffsetRows = new StackPanel { Spacing = 4 };
        captionOffsetRows.Children.Add(Row(Caption(l.RemuxVideoCaptionOffset), captionOffset));
        captionOffsetRows.Children.Add(captionOffsetHint);
        captionOffsetRows.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsCaptionOffsetVisible)));

        var outputRows = new StackPanel { Spacing = 8 };
        outputRows.Children.Add(Row(Caption(l.RemuxVideoOutputFormat), comboBoxFormat));
        outputRows.Children.Add(subtitlesAsRow);
        outputRows.Children.Add(captionsHint);
        outputRows.Children.Add(captionOffsetRows);
        outputRows.Children.Add(PathRow(textBoxOutput, buttonBrowseOutput));
        outputRows.Children.Add(fastStartRow);

        var panel = new StackPanel();
        panel.Children.Add(EditorChrome.MakeEditorSection(Caption(l.RemuxVideoInputVideo), videoRows, first: true));
        panel.Children.Add(EditorChrome.MakeEditorSection(Caption(l.RemuxVideoOutputFile), outputRows));
        return panel;
    }

    private static Grid MakeListsPanel(RemuxVideoViewModel vm)
    {
        var l = Se.Language.Video;

        // Audio files
        var panelAudio = MakeFileListSection(
            IconNames.MusicNote,
            l.RemuxVideoAudioFiles,
            nameof(vm.AudioFilesInfo),
            nameof(vm.AudioFiles),
            nameof(vm.SelectedAudioFile),
            vm.AddAudioCommand,
            vm.RemoveAudioCommand, nameof(vm.IsAudioRemoveEnabled),
            vm.ClearAudioCommand,
            vm.MoveAudioUpCommand, nameof(vm.IsAudioMoveUpEnabled),
            vm.MoveAudioDownCommand, nameof(vm.IsAudioMoveDownEnabled),
            vm.AudioListKeyDown,
            vm,
            (vm.SelectAudioTrackCommand, nameof(vm.IsAudioSelectTrackVisible)),
            first: true);

        // Mix all audio files into one track, each at its own volume (shown for two or more files)
        var switchMixAudio = Switch(nameof(vm.MixAudio), l.RemuxVideoMixAudio);
        switchMixAudio.Bind(ToggleSwitch.IsEnabledProperty, NotRemuxing(vm));
        var labelVolume = EditorChrome.MakeDimLabel(Caption(l.RemuxVideoVolumePercent), new Thickness(12, 0, 0, 0));
        labelVolume.Bind(TextBlock.IsEnabledProperty, new Binding(nameof(vm.IsVolumeEnabled)));
        var numericVolume = UiUtil.MakeNumericUpDownInt(0, 200, 100, 76, vm, $"{nameof(vm.SelectedAudioFile)}.{nameof(RemuxFileItem.VolumePercent)}")
            .WithAccessibleName(l.RemuxVideoVolumePercent);
        numericVolume.Classes.Add("field");
        numericVolume.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.IsVolumeEnabled)));
        var panelMix = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 8, 0, 0),
            Children =
            {
                switchMixAudio,
                new TextBlock { Text = l.RemuxVideoMixAudio, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center },
                labelVolume,
                numericVolume,
            },
        };
        panelMix.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsMixAudioVisible)));
        var audioGrid = (Grid)panelAudio.Child!;
        audioGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        audioGrid.Add(panelMix, 2);

        // Subtitle files
        var panelSubtitle = MakeFileListSection(
            IconNames.SubtitlesOutline,
            l.RemuxVideoSubtitleFiles,
            nameof(vm.SubtitleFilesInfo),
            nameof(vm.SubtitleFiles),
            nameof(vm.SelectedSubtitleFile),
            vm.AddSubtitleCommand,
            vm.RemoveSubtitleCommand, nameof(vm.IsSubtitleRemoveEnabled),
            vm.ClearSubtitleCommand,
            vm.MoveSubtitleUpCommand, nameof(vm.IsSubtitleMoveUpEnabled),
            vm.MoveSubtitleDownCommand, nameof(vm.IsSubtitleMoveDownEnabled),
            vm.SubtitleListKeyDown,
            vm,
            null,
            first: false);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        grid.Add(panelAudio, 0);
        grid.Add(panelSubtitle, 1);
        return grid;
    }

    /// <summary>
    /// A list of files with an icon, the file name and a details line, a header with add, remove,
    /// clear and move buttons, and a context menu with remove + move up/down (Ctrl+Up/Down).
    /// </summary>
    private static Border MakeFileListSection(
        string iconName,
        string header,
        string infoPropertyPath,
        string itemsPropertyPath,
        string selectedPropertyPath,
        ICommand addCommand,
        ICommand removeCommand, string removeEnabledPath,
        ICommand clearCommand,
        ICommand moveUpCommand, string moveUpEnabledPath,
        ICommand moveDownCommand, string moveDownEnabledPath,
        System.EventHandler<KeyEventArgs> keyDown,
        RemuxVideoViewModel vm,
        (ICommand Command, string VisiblePath)? selectTrack,
        bool first)
    {
        var listBox = new ListBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 70,
            ItemTemplate = new FuncDataTemplate<RemuxFileItem>((_, _) =>
            {
                var icon = new Optris.Icons.Avalonia.Icon
                {
                    Value = iconName,
                    FontSize = 20,
                    Opacity = 0.75,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2, 0, 8, 0),
                };

                var textName = new TextBlock { FontWeight = FontWeight.SemiBold, FontSize = 12.5, TextTrimming = TextTrimming.CharacterEllipsis };
                textName.Bind(TextBlock.TextProperty, new Binding(nameof(RemuxFileItem.Name)));
                if (Se.Settings.Appearance.ShowHints)
                {
                    ToolTip.SetTip(textName, new Binding(nameof(RemuxFileItem.FileName)));
                }

                // The folder after the name, trimmed from the front so the nearest folders stay
                // readable - the same information the video box shows in its full path.
                var textFolder = new TextBlock
                {
                    Opacity = 0.6,
                    FontSize = 12,
                    TextTrimming = TextTrimming.PrefixCharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(8, 0, 0, 0),
                };
                textFolder.Bind(TextBlock.TextProperty, new Binding(nameof(RemuxFileItem.Folder)));

                var nameRow = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    },
                };
                nameRow.Add(textName, 0, 0);
                nameRow.Add(textFolder, 0, 1);

                var textDetails = new TextBlock { FontSize = UiUtil.ScaledFontSize(11), Opacity = 0.7, TextTrimming = TextTrimming.CharacterEllipsis };
                textDetails.Bind(TextBlock.TextProperty, new Binding(nameof(RemuxFileItem.Details)));

                var textPanel = new StackPanel
                {
                    Spacing = 1,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children = { nameRow, textDetails },
                };

                // A Grid, not a horizontal StackPanel: the text column must be width-bound for
                // the folder's prefix trimming to happen at all.
                // CC1/CC3 when the subtitle file becomes closed captions in the video
                var textChannel = new TextBlock
                {
                    FontSize = UiUtil.ScaledFontSize(10.5),
                    FontWeight = FontWeight.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                textChannel.Bind(TextBlock.TextProperty, new Binding(nameof(RemuxFileItem.CaptionChannel)));
                var channelBadge = new Border
                {
                    Background = EditorChrome.AccentSoftBrush,
                    CornerRadius = new CornerRadius(9),
                    Padding = new Thickness(7, 1),
                    Margin = new Thickness(8, 0, 2, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = textChannel,
                };
                channelBadge.Bind(Visual.IsVisibleProperty, new Binding(nameof(RemuxFileItem.HasCaptionChannel)));

                var row = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = GridLength.Auto },
                    },
                };
                row.Add(icon, 0, 0);
                row.Add(textPanel, 0, 1);
                row.Add(channelBadge, 0, 2);
                return row;
            }, true),
        };
        listBox.Classes.Add("files");
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding(itemsPropertyPath));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(selectedPropertyPath) { Mode = BindingMode.TwoWay });
        listBox.Bind(ListBox.IsEnabledProperty, NotRemuxing(vm));
        listBox.WithAccessibleName(Caption(header)); // the section header above the list is not its label (#12087)
        listBox.AddHandler(InputElement.KeyDownEvent, keyDown, RoutingStrategies.Tunnel);

        var flyout = new MenuFlyout();
        listBox.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(listBox);

        if (selectTrack != null)
        {
            var menuItemSelectTrack = new MenuItem
            {
                Header = Se.Language.Video.RemuxVideoSelectAudioTrackDotDotDot,
                Icon = new Optris.Icons.Avalonia.Icon { Value = IconNames.Tune, VerticalAlignment = VerticalAlignment.Center },
                DataContext = vm,
                Command = selectTrack.Value.Command,
            };
            menuItemSelectTrack.Bind(MenuItem.IsVisibleProperty, new Binding(selectTrack.Value.VisiblePath) { Source = vm });
            flyout.Items.Add(menuItemSelectTrack);
            var separator = new Separator();
            separator.Bind(Separator.IsVisibleProperty, new Binding(selectTrack.Value.VisiblePath) { Source = vm });
            flyout.Items.Add(separator);
        }

        var menuItemRemove = new MenuItem
        {
            Header = Se.Language.General.Remove,
            Icon = new Optris.Icons.Avalonia.Icon { Value = IconNames.Trash, VerticalAlignment = VerticalAlignment.Center },
            InputGesture = new KeyGesture(Key.Delete),
            DataContext = vm,
            Command = removeCommand,
        };
        menuItemRemove.Bind(MenuItem.IsEnabledProperty, new Binding(removeEnabledPath) { Source = vm });
        flyout.Items.Add(menuItemRemove);
        flyout.Items.Add(new Separator());

        var menuItemMoveUp = new MenuItem
        {
            Header = Se.Language.General.MoveUp,
            Icon = new Optris.Icons.Avalonia.Icon { Value = IconNames.ArrowUpThin, VerticalAlignment = VerticalAlignment.Center },
            InputGesture = new KeyGesture(Key.Up, KeyModifiers.Control),
            DataContext = vm,
            Command = moveUpCommand,
        };
        menuItemMoveUp.Bind(MenuItem.IsEnabledProperty, new Binding(moveUpEnabledPath) { Source = vm });
        flyout.Items.Add(menuItemMoveUp);

        var menuItemMoveDown = new MenuItem
        {
            Header = Se.Language.General.MoveDown,
            Icon = new Optris.Icons.Avalonia.Icon { Value = IconNames.ArrowDownThin, VerticalAlignment = VerticalAlignment.Center },
            InputGesture = new KeyGesture(Key.Down, KeyModifiers.Control),
            DataContext = vm,
            Command = moveDownCommand,
        };
        menuItemMoveDown.Bind(MenuItem.IsEnabledProperty, new Binding(moveDownEnabledPath) { Source = vm });
        flyout.Items.Add(menuItemMoveDown);

        Button Action(string icon, string name, ICommand command, Binding isEnabled)
        {
            var button = EditorChrome.CreateToolButton(icon, name, command, 26);
            button.Bind(Button.IsEnabledProperty, isEnabled);
            return button;
        }

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                Action("fa-solid fa-plus", Se.Language.General.AddDotDotDot, addCommand, NotRemuxing(vm)),
                Action("fa-solid fa-minus", Se.Language.General.Remove, removeCommand, new Binding(removeEnabledPath)),
                Action("fa-solid fa-trash", Se.Language.General.Clear, clearCommand, NotRemuxing(vm)),
                EditorChrome.MakeToolbarSeparator(),
                Action("fa-solid fa-arrow-up", Se.Language.General.MoveUp, moveUpCommand, new Binding(moveUpEnabledPath)),
                Action("fa-solid fa-arrow-down", Se.Language.General.MoveDown, moveDownCommand, new Binding(moveDownEnabledPath)),
            },
        };

        // Header: icon, title and the "n files" info; the list actions on the right
        var title = EditorChrome.MakeSectionHeader(Caption(header));
        title.TextTrimming = TextTrimming.CharacterEllipsis;
        var info = Hint(infoPropertyPath);
        info.Margin = new Thickness(8, 0, 0, 0);
        var titleRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        titleRow.Add(new Optris.Icons.Avalonia.Icon { Value = iconName, FontSize = 14, Foreground = EditorChrome.IconBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) }, 0, 0);
        titleRow.Add(title, 0, 1);
        titleRow.Add(info, 0, 2);

        var headerPanel = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(actions, Dock.Right);
        headerPanel.Children.Add(actions);
        headerPanel.Children.Add(titleRow);

        var list = new Border
        {
            Child = listBox,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(0, 2),
            ClipToBounds = true,
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };
        grid.Add(headerPanel, 0);
        grid.Add(list, 1);

        return new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    private Border MakeProgressView(RemuxVideoViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.IsIndeterminateProperty, new Binding(nameof(vm.IsFinalizing)));

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
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.IsRemuxing)),
        };
    }

    private static Border MakeButtonBar(RemuxVideoViewModel vm)
    {
        var l = Se.Language.Video;

        // Open the output folder once the remux is done
        var buttonOpenFolder = EditorChrome.CreateToolButton("fa-solid fa-folder-open", Se.Language.General.OpenContainingFolder, vm.OpenFolderCommand, 30);
        buttonOpenFolder.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsCompleted)));
        buttonOpenFolder.Margin = new Thickness(0, 0, 8, 0);

        // The result ("Remux completed", errors) - while remuxing the progress row shows the text
        var status = Hint(nameof(vm.ProgressText));
        status.FontSize = 12;
        status.Bind(Visual.IsVisibleProperty, NotRemuxing(vm));

        var buttonRemux = new SeSplitButton
        {
            Content = l.RemuxVideoTitle,
            Command = vm.RemuxCommand,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = l.PromptForFfmpegParamsAndGenerate,
                        Command = vm.PromptFfmpegParametersAndRemuxCommand,
                    },
                }
            }
        };
        buttonRemux.Classes.Add("primary");
        buttonRemux.Bind(SplitButton.IsEnabledProperty, new Binding(nameof(vm.CanRemux)));
        AutomationProperties.SetName(buttonRemux, l.RemuxVideoTitle);

        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand);
        buttonDone.Bind(Button.IsEnabledProperty, NotRemuxing(vm));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
        buttons.Children.Add(buttonCancel);
        buttons.Children.Add(buttonDone);
        buttons.Children.Add(buttonRemux);

        var bar = new DockPanel();
        DockPanel.SetDock(buttonOpenFolder, Dock.Left);
        DockPanel.SetDock(buttons, Dock.Right);
        bar.Children.Add(buttonOpenFolder);
        bar.Children.Add(buttons);
        status.Margin = new Thickness(0, 0, 12, 0);
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
}
