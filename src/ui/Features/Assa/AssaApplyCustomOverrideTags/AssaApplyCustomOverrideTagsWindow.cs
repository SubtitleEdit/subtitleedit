using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;

public class AssaApplyCustomOverrideTagsWindow : Window
{
    public AssaApplyCustomOverrideTagsWindow(AssaApplyCustomOverrideTagsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.ApplyOverrideTags;
        CanResize = true;
        Width = 1100;
        Height = 700;
        MinWidth = 900;
        MinHeight = 650;
        vm.Window = this;
        DataContext = vm;

        var labelOverrideTag = UiUtil.MakeLabel(Se.Language.Assa.ChooseOverrideTagToAdd);
        var comboBoxOverrideTags = UiUtil.MakeComboBox(vm.OverrideTags, vm, nameof(vm.SelectedOverrideTag));
        var buttonUseOverrideTag = UiUtil.MakeButton(Se.Language.General.Use, vm.UseCommand);
        var buttonAppendOverrideTag = UiUtil.MakeButton(Se.Language.General.Append, vm.AppendCommand);
        var buttonClear = UiUtil.MakeButton(Se.Language.General.Clear, vm.ClearCommand);
        var panelOverrideTags = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                comboBoxOverrideTags,
                buttonUseOverrideTag,
                buttonAppendOverrideTag,
                buttonClear,
            }
        };

        var textBoxCurrent = new TextBox
        {
            AcceptsReturn = true,
            Height = 60,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        textBoxCurrent.Bind(TextBox.TextProperty, new Binding(nameof(vm.CurrentTag)));
        
        var panelRadioButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 10, 0, 0),
            Children =
            {
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustAll,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustAll))
                },
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustSelectedLines,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLines))
                },
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustSelectedLinesAndForward,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLinesAndForward))
                },
                UiUtil.MakeLabel().WithBindText(vm, nameof(vm.SelectionInfo)).WithOpacity(0.6),
            },
        };
        
        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;

        var comboBoxLeft = UiUtil.MakeComboBoxBindText(vm.Paragraphs, vm, nameof(SubtitleDisplayItem.Text), nameof(vm.SelectedParagraphIndex));
        comboBoxLeft.Width = double.NaN;
        comboBoxLeft.MinHeight = 50;
        comboBoxLeft.HorizontalAlignment = HorizontalAlignment.Stretch;
        vm.ComboBoxLeft = comboBoxLeft;
        comboBoxLeft.SelectionChanged += vm.ComboBoxParagraphsChanged;

        var buttonPlay = UiUtil.MakeButton(Se.Language.Assa.PlayCurrent, vm.PlayAndBackCommand).WithLeftAlignment();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var gridVideo = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // video player
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // combo box
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        gridVideo.Add(vm.VideoPlayerControl, 0);
        gridVideo.Add(comboBoxLeft, 1);
        gridVideo.Add(buttonPlay, 2);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // override tags
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // override tags
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // text box with current chosen tags
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // video player 
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelOverrideTag, 0);
        grid.Add(panelOverrideTags, 1, 0, 1, 1);
        grid.Add(textBoxCurrent, 2, 0, 1, 1);
        grid.Add(panelRadioButtons, 0, 1, 3, 1);
        grid.Add(UiUtil.MakeBorderForControl(gridVideo), 3, 0, 1, 2);
        grid.Add(buttonPanel, 4, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, e) => vm.OnClosing();
    }
}
