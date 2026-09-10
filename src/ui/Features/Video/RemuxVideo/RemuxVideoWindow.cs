using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Features.Video.RemuxVideo;

public class RemuxVideoWindow : Window
{
    private readonly RemuxVideoViewModel _vm;
    private Button? _buttonBrowseVideo;

    public RemuxVideoWindow(RemuxVideoViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.RemuxVideoTitle;
        CanResize = true;
        Width = 760;
        Height = 640;
        MinWidth = 600;
        MinHeight = 520;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Video;
        var notRemuxing = new Binding(nameof(vm.IsRemuxing)) { Converter = InverseBooleanConverter.Instance };

        // 1. Video
        var textBoxVideo = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.VideoFileName));
        textBoxVideo.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxVideo.Bind(TextBox.IsEnabledProperty, notRemuxing);
        var labelVideoSize = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.VideoFileSize)).WithMarginRight(5);
        _buttonBrowseVideo = UiUtil.MakeButtonBrowse(vm.BrowseVideoCommand);
        _buttonBrowseVideo.Bind(Button.IsEnabledProperty, notRemuxing);

        var gridVideo = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
        };
        gridVideo.Add(textBoxVideo, 0, 0);
        gridVideo.Add(labelVideoSize, 0, 1);
        gridVideo.Add(_buttonBrowseVideo, 0, 2);

        var panelVideo = new StackPanel
        {
            Spacing = 4,
            Children =
            {
                MakeSectionHeader(IconNames.MovieOpenOutline, l.RemuxVideoInputVideo, vm, null),
                gridVideo,
            },
        };

        // 2. Audio files
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
            (vm.SelectAudioTrackCommand, nameof(vm.IsAudioSelectTrackVisible)));

        // 3. Subtitle files
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
            null);

        // 4. Output
        var labelFormat = UiUtil.MakeLabel(l.RemuxVideoOutputFormat);
        var comboBoxFormat = UiUtil.MakeComboBox(vm.OutputFormats, vm, nameof(vm.SelectedOutputFormat));
        comboBoxFormat.Width = 100;
        comboBoxFormat.Bind(ComboBox.IsEnabledProperty, notRemuxing);

        var textBoxOutput = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.OutputFileName));
        textBoxOutput.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxOutput.Bind(TextBox.IsEnabledProperty, notRemuxing);
        var buttonBrowseOutput = UiUtil.MakeButtonBrowse(vm.BrowseOutputFileCommand);
        buttonBrowseOutput.Bind(Button.IsEnabledProperty, notRemuxing);

        var gridOutput = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
        };
        gridOutput.Add(labelFormat, 0, 0);
        gridOutput.Add(comboBoxFormat, 0, 1);
        gridOutput.Add(textBoxOutput, 0, 2);
        gridOutput.Add(buttonBrowseOutput, 0, 3);

        var panelOutput = new StackPanel
        {
            Spacing = 4,
            Children =
            {
                MakeSectionHeader(IconNames.ContentSave, l.RemuxVideoOutputFile, vm, null),
                gridOutput,
            },
        };

        // 5. Progress
        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Height = 16,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsRemuxing)));

        var labelProgress = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.ProgressText));

        var progressPanel = new StackPanel
        {
            Spacing = 3,
            Children = { progressBar, labelProgress },
        };

        // 6. Buttons
        var buttonOpenFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenFolderCommand)
            .WithIconLeft(IconNames.FolderOpen)
            .WithMarginRight(5);
        buttonOpenFolder.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsCompleted)));

        var buttonPlay = UiUtil.MakeButton(Se.Language.General.Play, vm.PlayCommand)
            .WithIconLeft(IconNames.Play)
            .WithMarginRight(5);
        buttonPlay.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsCompleted)));

        var buttonRemux = UiUtil.MakeButton(l.RemuxVideoTitle, vm.RemuxCommand)
            .WithMarginRight(5);
        buttonRemux.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.CanRemux)));

        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand).WithMarginRight(5);
        buttonDone.Bind(Button.IsEnabledProperty, notRemuxing);

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var buttonPanel = UiUtil.MakeButtonBar(buttonOpenFolder, buttonPlay, buttonRemux, buttonDone, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            RowSpacing = 12,
            Margin = UiUtil.MakeWindowMargin(),
        };
        grid.Add(panelVideo, 0);
        grid.Add(panelAudio, 1);
        grid.Add(panelSubtitle, 2);
        grid.Add(panelOutput, 3);
        grid.Add(progressPanel, 4);
        grid.Add(buttonPanel, 5);

        Content = grid;

        KeyDown += vm.KeyDown;
        UiUtil.FocusOnFirstActivation(this, () => { _buttonBrowseVideo?.Focus(); });
    }

    private static StackPanel MakeSectionHeader(string iconName, string text, RemuxVideoViewModel vm, string? infoPropertyPath)
    {
        var icon = new Optris.Icons.Avalonia.Icon
        {
            Value = iconName,
            FontSize = 18,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var label = UiUtil.MakeLabel(text);
        label.FontWeight = FontWeight.SemiBold;
        label.VerticalAlignment = VerticalAlignment.Center;

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { icon, label },
        };

        if (infoPropertyPath != null)
        {
            var labelInfo = UiUtil.MakeLabel().WithBindText(vm, infoPropertyPath);
            labelInfo.Opacity = 0.7;
            labelInfo.VerticalAlignment = VerticalAlignment.Center;
            panel.Children.Add(labelInfo);
        }

        return panel;
    }

    /// <summary>
    /// A list of files with an icon, the file name and a details line, add/remove/clear
    /// buttons underneath and a context menu with remove + move up/down (Ctrl+Up/Down).
    /// </summary>
    private static Grid MakeFileListSection(
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
        (ICommand Command, string VisiblePath)? selectTrack)
    {
        var notRemuxing = new Binding(nameof(vm.IsRemuxing)) { Converter = InverseBooleanConverter.Instance };

        var listBox = new ListBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 90,
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

                var textName = new TextBlock { FontWeight = FontWeight.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis };
                textName.Bind(TextBlock.TextProperty, new Binding(nameof(RemuxFileItem.Name)));
                ToolTip.SetTip(textName, new Binding(nameof(RemuxFileItem.FileName)));

                var textDetails = new TextBlock { FontSize = 11, Opacity = 0.7, TextTrimming = TextTrimming.CharacterEllipsis };
                textDetails.Bind(TextBlock.TextProperty, new Binding(nameof(RemuxFileItem.Details)));

                var textPanel = new StackPanel
                {
                    Spacing = 1,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children = { textName, textDetails },
                };

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { icon, textPanel },
                };
            }, true),
        };
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding(itemsPropertyPath));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(selectedPropertyPath) { Mode = BindingMode.TwoWay });
        listBox.Bind(ListBox.IsEnabledProperty, notRemuxing);
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

        var buttonAdd = UiUtil.MakeButton(addCommand as CommunityToolkit.Mvvm.Input.IRelayCommand, IconNames.Plus, Se.Language.General.AddDotDotDot);
        buttonAdd.Bind(Button.IsEnabledProperty, notRemuxing);
        var buttonRemove = UiUtil.MakeButton(removeCommand as CommunityToolkit.Mvvm.Input.IRelayCommand, IconNames.Trash, Se.Language.General.Remove);
        buttonRemove.Bind(Button.IsEnabledProperty, new Binding(removeEnabledPath));
        var buttonClear = UiUtil.MakeButton(clearCommand as CommunityToolkit.Mvvm.Input.IRelayCommand, IconNames.Close, Se.Language.General.Clear);
        buttonClear.Bind(Button.IsEnabledProperty, notRemuxing);
        var buttonMoveUp = UiUtil.MakeButton(moveUpCommand as CommunityToolkit.Mvvm.Input.IRelayCommand, IconNames.ArrowUpThin, Se.Language.General.MoveUp);
        buttonMoveUp.Bind(Button.IsEnabledProperty, new Binding(moveUpEnabledPath));
        var buttonMoveDown = UiUtil.MakeButton(moveDownCommand as CommunityToolkit.Mvvm.Input.IRelayCommand, IconNames.ArrowDownThin, Se.Language.General.MoveDown);
        buttonMoveDown.Bind(Button.IsEnabledProperty, new Binding(moveDownEnabledPath));

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(0, 4, 0, 0),
            Children = { buttonAdd, buttonRemove, buttonClear, buttonMoveUp, buttonMoveDown },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            RowSpacing = 4,
        };
        grid.Add(MakeSectionHeader(iconName, header, vm, infoPropertyPath), 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(listBox), 1);
        grid.Add(panelButtons, 2);

        return grid;
    }
}
