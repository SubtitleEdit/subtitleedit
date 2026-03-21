using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect;

public class AssaApplyAdvancedEffectWindow : Window
{
    public AssaApplyAdvancedEffectWindow(AssaApplyAdvancedEffectViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.ApplyAdvancedEffectTitle;
        CanResize = true;
        Width = 1100;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        vm.Window = this;
        DataContext = vm;

        // ── Left panel ────────────────────────────────────────────────────────
        var chooseLabel = UiUtil.MakeLabel(Se.Language.Assa.ChooseEffect).WithBold();

        var effectListBox = new ListBox
        {
            ItemsSource = vm.OverrideTags,
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedOverrideTag))
            {
                Mode = BindingMode.TwoWay,
            },
            ItemTemplate = new FuncDataTemplate<IAdvancedEffectDisplay>((item, _) =>
            {
                var panel = new StackPanel { Margin = new Thickness(4, 6, 4, 6), Spacing = 3 };

                var nameBlock = new TextBlock { FontWeight = FontWeight.SemiBold, FontSize = 13 };
                nameBlock.Bind(TextBlock.TextProperty, new Binding(nameof(IAdvancedEffectDisplay.Name)));

                var descBlock = new TextBlock
                {
                    FontSize = 11,
                    Opacity = 0.65,
                    TextWrapping = TextWrapping.Wrap,
                };
                descBlock.Bind(TextBlock.TextProperty, new Binding(nameof(IAdvancedEffectDisplay.Description)));

                panel.Children.Add(nameBlock);
                panel.Children.Add(descBlock);

                if (item is AdvancedEffectSnow snowItem)
                {
                    var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 6, 0, 0) };
                    row.Children.Add(new TextBlock { Text = Se.Language.Assa.AdvancedEffectSnowFlakeCount, VerticalAlignment = VerticalAlignment.Center, FontSize = 12 });
                    row.Children.Add(UiUtil.MakeNumericUpDownInt(10, 3000, 200, 130, snowItem, nameof(AdvancedEffectSnow.FlakeCount)));
                    panel.Children.Add(row);
                }
                else if (item is AdvancedEffectStarfield starfieldItem)
                {
                    var starCountRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 6, 0, 0) };
                    starCountRow.Children.Add(new TextBlock { Text = Se.Language.Assa.AdvancedEffectStarfieldStarCount, VerticalAlignment = VerticalAlignment.Center, FontSize = 12 });
                    starCountRow.Children.Add(UiUtil.MakeNumericUpDownInt(50, 3000, 650, 130, starfieldItem, nameof(AdvancedEffectStarfield.StarCount)));
                    var speedRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                    speedRow.Children.Add(new TextBlock { Text = Se.Language.Assa.AdvancedEffectStarfieldSpeed, VerticalAlignment = VerticalAlignment.Center, FontSize = 12 });
                    speedRow.Children.Add(UiUtil.MakeNumericUpDownOneDecimal(0.1m, 10.0m, 130, starfieldItem, nameof(AdvancedEffectStarfield.SpeedMultiplier)));
                    var settingsStack = new StackPanel { Spacing = 4 };
                    settingsStack.Children.Add(starCountRow);
                    settingsStack.Children.Add(speedRow);
                    panel.Children.Add(settingsStack);
                }
                else if (item is AdvancedEffectWordSpacing wordSpacingItem)
                {
                    var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 6, 0, 0) };
                    row.Children.Add(new TextBlock { Text = Se.Language.Assa.AdvancedEffectWordSpacingPixels, VerticalAlignment = VerticalAlignment.Center, FontSize = 12 });
                    row.Children.Add(UiUtil.MakeNumericUpDownOneDecimal(0m, 100m, 130, wordSpacingItem, nameof(AdvancedEffectWordSpacing.SpacingPixels)));
                    panel.Children.Add(row);
                }

                return panel;
            }),
        };

        var scopePanel = new StackPanel
        {
            Spacing = 5,
            Margin = new Thickness(0, 8, 0, 0),
            Children =
            {
                UiUtil.MakeLabel(Se.Language.General.ApplyTo).WithBold(),
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustAll,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustAll)),
                },
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustSelectedLines,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLines)),
                },
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustSelectedLinesAndForward,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLinesAndForward)),
                },
                UiUtil.MakeLabel().WithBindText(vm, nameof(vm.SelectionInfo)).WithOpacity(0.6),
            },
        };

        var leftGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            RowSpacing = 8,
        };
        leftGrid.Add(chooseLabel, 0);
        leftGrid.Add(UiUtil.MakeBorderForControlNoPadding(effectListBox), 1);
        leftGrid.Add(scopePanel, 2);

        // ── Right panel (video + subtitle navigation) ─────────────────────────
        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;

        var comboBoxLeft = UiUtil.MakeComboBoxBindText(
            vm.Paragraphs, vm, nameof(SubtitleDisplayItem.Text), nameof(vm.SelectedParagraphIndex));
        comboBoxLeft.Width = double.NaN;
        comboBoxLeft.MinHeight = 50;
        comboBoxLeft.HorizontalAlignment = HorizontalAlignment.Stretch;
        vm.ComboBoxLeft = comboBoxLeft;
        comboBoxLeft.SelectionChanged += vm.ComboBoxParagraphsChanged;

        var buttonPlay = UiUtil.MakeButton(Se.Language.Assa.PlayCurrent, vm.PlayAndBackCommand)
            .WithLeftAlignment();

        var videoGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto,Auto"),
            RowSpacing = 8,
        };
        videoGrid.Add(vm.VideoPlayerControl, 0);
        videoGrid.Add(comboBoxLeft, 1);
        videoGrid.Add(buttonPlay, 2);

        // ── Main layout ───────────────────────────────────────────────────────
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("260,*"),
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 12,
            RowSpacing = 10,
        };
        mainGrid.Add(leftGrid, 0, 0);
        mainGrid.Add(UiUtil.MakeBorderForControl(videoGrid), 0, 1);
        mainGrid.Add(buttonPanel, 1, 0, 1, 2);

        Content = mainGrid;

        Activated += delegate { buttonOk.Focus(); };
        AddHandler(KeyDownEvent, vm.OnKeyDownHandler,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, e) => vm.OnClosing();
    }
}
