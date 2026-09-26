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
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Options.Settings.VideoControlsItems;

public class VideoControlsItemsWindow : Window
{
    public VideoControlsItemsWindow(VideoControlsItemsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.VideoControls;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
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
        };
        KeyboardNavigation.SetTabNavigation(preview, KeyboardNavigationMode.None);
        preview.SetPreviewCaptions("00:01:23.456 / 01:39:43.040", "movie.mkv", "libmpv");
        var previewBorder = new Border
        {
            Child = preview,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            ClipToBounds = true,
        };

        void UpdatePreview() => preview.ApplyControlsLayout(vm.GetCurrentItems());

        UpdatePreview();
        vm.LayoutChanged += UpdatePreview;
        Closed += (_, _) =>
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
        listBox.ItemTemplate = new FuncDataTemplate<VideoControlsItemDisplay>((_, _) =>
        {
            var checkBox = new CheckBox();
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(VideoControlsItemDisplay.IsVisible)) { Mode = BindingMode.TwoWay });
            // A check box with no content is announced as a nameless "check box" (#12087).
            checkBox.Bind(AutomationProperties.NameProperty, new Binding(nameof(VideoControlsItemDisplay.Name)));

            var icon = new Optris.Icons.Avalonia.Icon
            {
                Width = 20,
                Margin = new Thickness(2, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center,
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
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { eyeIcon, new TextBlock { Text = Se.Language.General.Visible, VerticalAlignment = VerticalAlignment.Center } },
            },
            [AutomationProperties.NameProperty] = Se.Language.General.Visible,
        };
        checkBoxVisible.Bind(CheckBox.IsCheckedProperty, new Binding($"{nameof(VideoControlsItemsViewModel.SelectedItem)}.{nameof(VideoControlsItemDisplay.IsVisible)}") { Mode = BindingMode.TwoWay });
        checkBoxVisible.Bind(IsEnabledProperty, new Binding(nameof(VideoControlsItemsViewModel.SelectedItem)) { Converter = Avalonia.Data.Converters.ObjectConverters.IsNotNull });

        var sidePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Margin = new Thickness(10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                UiUtil.MakeButton(Se.Language.General.MoveUp, vm.MoveUpCommand).WithIconLeft(IconNames.ArrowUpThin).WithMinWidth(130),
                UiUtil.MakeButton(Se.Language.General.MoveDown, vm.MoveDownCommand).WithIconLeft(IconNames.ArrowDownThin).WithMinWidth(130),
                new Border { Height = 10 },
                checkBoxVisible,
                new Border { Height = 10 },
                UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetCommand).WithIconLeft(IconNames.Restore).WithMinWidth(130),
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
        contentGrid.Add(listBox, 0, 0);
        contentGrid.Add(sidePanel, 0, 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Width = 560,
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        grid.Add(previewBorder, 0, 0);
        grid.Add(contentGrid, 1, 0);
        grid.Add(panelButtons, 2, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, listBox); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
