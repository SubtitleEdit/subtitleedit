using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings.VideoControlsItems;

/// <summary>
/// Order and visibility of the video controls in the editor look (see <see cref="EditorChrome"/>):
/// a live preview on top, the reorderable list with its actions below, Cancel + OK in the
/// button bar.
/// </summary>
public class VideoControlsItemsWindow : Window
{
    public VideoControlsItemsWindow(VideoControlsItemsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.VideoControls;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);
        vm.Window = this;
        DataContext = vm;

        // Live preview of the controls row - not interactive, and no tab stops.
        var preview = new VideoPlayerControl(new EmptyVideoPlayer())
        {
            Height = 100,
            ClickToTogglePlay = false,
            IsHitTestVisible = false,
            Focusable = false,
            StopIsVisible = true,
            FullScreenIsVisible = true,
            SubtitleButtonsIsVisible = true,
            PlaybackSpeedIsVisible = true,
        };
        KeyboardNavigation.SetTabNavigation(preview, KeyboardNavigationMode.None);
        preview.SetPreviewCaptions("00:01:23.456 / 01:39:43.040", "movie.mkv", "libmpv");
        // A plain border: no corner radius or clipping around a video player (the native video
        // surface on macOS disappears otherwise).
        var previewBorder = new Border
        {
            Child = preview,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
        };

        void UpdatePreview() => preview.ApplyControlsLayout(vm.GetCurrentItems());

        UpdatePreview();
        vm.LayoutChanged += UpdatePreview;
        // Closing, not Closed: the OpenGL deinit that frees mpv's render context runs before
        // Closed, so disposing there leaked the preview's mpv core (issue #15971).
        Closing += (_, _) =>
        {
            vm.LayoutChanged -= UpdatePreview;
            preview.CloseAndDisposePlayer();
        };

        var listBox = new ListBox
        {
            Height = 440, // all items without scrolling
        };
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding(nameof(VideoControlsItemsViewModel.Items)));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(nameof(VideoControlsItemsViewModel.SelectedItem)) { Mode = BindingMode.TwoWay });
        AutomationProperties.SetName(listBox, Se.Language.Options.Settings.VideoControls);
        listBox.ItemTemplate = new FuncDataTemplate<VideoControlsItemDisplay>((_, _) =>
        {
            var checkBox = new CheckBox { MinWidth = 0 };
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(VideoControlsItemDisplay.IsVisible)) { Mode = BindingMode.TwoWay });
            // A check box with no content is announced as a nameless "check box" (#12087).
            checkBox.Bind(AutomationProperties.NameProperty, new Binding(nameof(VideoControlsItemDisplay.Name)));

            var icon = new Optris.Icons.Avalonia.Icon
            {
                Width = 20,
                Margin = new Thickness(2, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = EditorChrome.IconBrush,
            };
            icon.Bind(Optris.Icons.Avalonia.Icon.ValueProperty, new Binding(nameof(VideoControlsItemDisplay.IconName)));

            var textBlock = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(VideoControlsItemDisplay.Name)));

            // Hidden items are dimmed, so the list reads like the resulting controls row.
            var content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { icon, textBlock },
            };
            content.Bind(OpacityProperty, new Binding(nameof(VideoControlsItemDisplay.IsVisible))
            {
                Converter = new Avalonia.Data.Converters.FuncValueConverter<bool, double>(v => v ? 1.0 : 0.45),
            });

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { checkBox, content },
            };
        }, true);

        // Same as the check box in the list row, but easier to find.
        var eyeIcon = new Optris.Icons.Avalonia.Icon { Value = IconNames.Eye, Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center };
        var checkBoxVisible = new CheckBox
        {
            FontSize = EditorChrome.FieldFontSize,
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { eyeIcon, new TextBlock { Text = Se.Language.General.Visible, VerticalAlignment = VerticalAlignment.Center } },
            },
            [AutomationProperties.NameProperty] = Se.Language.General.Visible,
        };
        checkBoxVisible.Bind(CheckBox.IsCheckedProperty, new Binding($"{nameof(VideoControlsItemsViewModel.SelectedItem)}.{nameof(VideoControlsItemDisplay.IsVisible)}") { Mode = BindingMode.TwoWay });
        checkBoxVisible.Bind(IsEnabledProperty, new Binding(nameof(VideoControlsItemsViewModel.SelectedItem)) { Converter = Avalonia.Data.Converters.ObjectConverters.IsNotNull });

        var moveButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                EditorChrome.MakeTextToolButton(IconNames.ArrowUpThin, Se.Language.General.MoveUp, vm.MoveUpCommand),
                EditorChrome.MakeTextToolButton(IconNames.ArrowDownThin, Se.Language.General.MoveDown, vm.MoveDownCommand),
            },
        };

        var sidePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = new Thickness(14, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                moveButtons,
                checkBoxVisible,
                EditorChrome.MakeTextToolButton(IconNames.Restore, Se.Language.General.Reset, vm.ResetCommand),
            },
        };

        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        contentGrid.Add(EditorChrome.MakeCompactListFrame(listBox), 0, 0);
        contentGrid.Add(sidePanel, 0, 1);

        var sections = new StackPanel
        {
            Width = 580,
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.Preview, previewBorder, first: true),
                EditorChrome.MakeEditorSection(Se.Language.Options.Settings.VideoControls, contentGrid),
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, listBox); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
