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
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;

/// <summary>
/// Applies override tags to lines, styled like ASSA draw: presets, the tags and what to apply
/// them to on the left, the video preview with a line picker on the right.
/// </summary>
public class AssaApplyCustomOverrideTagsWindow : Window
{
    private static readonly FontFamily MonoFont = new("Menlo, Consolas, Cascadia Mono, DejaVu Sans Mono, monospace");

    public AssaApplyCustomOverrideTagsWindow(AssaApplyCustomOverrideTagsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.ApplyOverrideTags;
        CanResize = true;
        Width = 1200;
        Height = 780;
        MinWidth = 950;
        MinHeight = 600;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        vm.Window = this;
        DataContext = vm;

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(380)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(MakeSettingsPanel(vm, out var firstInput), 0, 0);
        content.Add(MakePreview(vm), 0, 1);

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        mainGrid.Add(content, 0, 0);
        mainGrid.Add(MakeButtonBar(vm), 1, 0);
        Content = mainGrid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, firstInput);

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, e) => vm.OnClosing();
    }

    private static Border MakeSettingsPanel(AssaApplyCustomOverrideTagsViewModel vm, out Control firstInput)
    {
        var tags = MakeTagsSection(vm, out firstInput);
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };
        grid.Add(MakePresetsSection(vm), 0, 0);
        grid.Add(tags, 1, 0);
        grid.Add(MakeApplyToSection(vm), 2, 0);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };
    }

    /// <summary>
    /// The stock tags as a list: name and tag code, a click uses it, "+" appends it.
    /// </summary>
    private static Control MakePresetsSection(AssaApplyCustomOverrideTagsViewModel vm)
    {
        var list = new ItemsControl
        {
            ItemsSource = vm.OverrideTags,
            ItemTemplate = new FuncDataTemplate<OverrideTagDisplay>((preset, _) =>
            {
                if (preset == null)
                {
                    return new Panel();
                }

                var use = EditorChrome.CreateToolButton("fa-solid fa-check", Se.Language.General.Use + ": " + preset.Name, null, 24);
                use.Click += (_, _) =>
                {
                    vm.SelectedOverrideTag = preset;
                    vm.UseCommand.Execute(null);
                };
                var append = EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.Append + ": " + preset.Name, null, 24);
                append.Click += (_, _) =>
                {
                    vm.SelectedOverrideTag = preset;
                    vm.AppendCommand.Execute(null);
                };

                var text = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
                text.Children.Add(new TextBlock { Text = preset.Name, FontSize = 12.5, TextTrimming = TextTrimming.CharacterEllipsis });
                text.Children.Add(new TextBlock
                {
                    Text = preset.Tag,
                    FontFamily = MonoFont,
                    FontSize = 11,
                    Foreground = EditorChrome.FaintTextBrush,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                });

                var row = new Grid
                {
                    ColumnSpacing = 4,
                    ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
                };
                row.Add(text, 0, 0);
                row.Add(use, 0, 1);
                row.Add(append, 0, 2);

                var border = new Border
                {
                    Child = row,
                    Padding = new Thickness(10, 6, 6, 6),
                    CornerRadius = new CornerRadius(7),
                    Background = Brushes.Transparent,
                };
                if (Se.Settings.Appearance.ShowHints)
                {
                    ToolTip.SetTip(border, preset.Tag);
                }

                // A click on the row (not on its buttons) uses the preset
                border.PointerPressed += (_, e) =>
                {
                    if (e.Source is Visual visual && Avalonia.VisualTree.VisualExtensions.FindAncestorOfType<Button>(visual, includeSelf: true) == null)
                    {
                        vm.SelectedOverrideTag = preset;
                        vm.UseCommand.Execute(null);
                        e.Handled = true;
                    }
                };
                border.Cursor = new Cursor(StandardCursorType.Hand);
                border.PointerEntered += (_, _) => border.Background = EditorChrome.HoverBrush;
                border.PointerExited += (_, _) => border.Background = Brushes.Transparent;
                return border;
            }),
        };

        var header = new DockPanel { Margin = new Thickness(14, 10, 10, 6) };
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.Assa.ChooseOverrideTagToAdd));

        var grid = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(header, 0, 0);
        grid.Add(new ScrollViewer
        {
            Content = list,
            Padding = new Thickness(6, 0, 6, 8),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        }, 1, 0);
        return grid;
    }

    private static Border MakeTagsSection(AssaApplyCustomOverrideTagsViewModel vm, out Control firstInput)
    {
        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 70,
            MaxHeight = 140,
            FontFamily = MonoFont,
            FontSize = 12.5,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(6),
            [!TextBox.TextProperty] = new Binding(nameof(vm.CurrentTag)) { Mode = BindingMode.TwoWay },
        };
        textBox.WithAccessibleName(Se.Language.Assa.ApplyOverrideTags); // the chosen tags (#12087)
        firstInput = textBox;

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-eraser", Se.Language.General.Clear, vm.ClearCommand, 24));
        var history = EditorChrome.CreateToolButton("fa-solid fa-clock-rotate-left", Se.Language.General.History, vm.ShowHistoryCommand, 24);
        history.Bind(IsVisibleProperty, new Binding(nameof(vm.IsHistoryVisible)));
        actions.Children.Add(history);

        return EditorChrome.MakeEditorSection(Se.Language.Assa.ApplyOverrideTags, textBox, actions);
    }

    private static Border MakeApplyToSection(AssaApplyCustomOverrideTagsViewModel vm)
    {
        RadioButton Radio(string text, string property) => new()
        {
            Content = new TextBlock { Text = text, FontSize = 12.5 },
            GroupName = "assa-override-tags-apply-to",
            [!RadioButton.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };

        var content = new StackPanel { Spacing = 2 };
        content.Children.Add(Radio(Se.Language.Sync.AdjustAll, nameof(vm.AdjustAll)));
        content.Children.Add(Radio(Se.Language.Sync.AdjustSelectedLines, nameof(vm.AdjustSelectedLines)));
        content.Children.Add(Radio(Se.Language.Sync.AdjustSelectedLinesAndForward, nameof(vm.AdjustSelectedLinesAndForward)));
        content.Children.Add(new TextBlock
        {
            FontSize = 11.5,
            Foreground = EditorChrome.FaintTextBrush,
            Margin = new Thickness(0, 4, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SelectionInfo)),
        });
        return EditorChrome.MakeEditorSection(Se.Language.General.Lines, content);
    }

    private static Control MakePreview(AssaApplyCustomOverrideTagsViewModel vm)
    {
        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;

        var comboBoxLeft = UiUtil.MakeComboBoxBindText(vm.Paragraphs, vm, nameof(SubtitleDisplayItem.Text), nameof(vm.SelectedParagraphIndex));
        comboBoxLeft.Width = double.NaN;
        comboBoxLeft.MinHeight = 40;
        comboBoxLeft.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboBoxLeft.WithAccessibleName(Se.Language.General.Lines); // subtitle line picker under the video (#12087)
        vm.ComboBoxLeft = comboBoxLeft;
        comboBoxLeft.SelectionChanged += vm.ComboBoxParagraphsChanged;

        var play = EditorChrome.CreateToolButton("fa-solid fa-play", Se.Language.General.PlayCurrent, vm.PlayAndBackCommand, 40);

        var lineRow = new Grid
        {
            ColumnSpacing = 8,
            Margin = new Thickness(0, 10, 0, 0),
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        lineRow.Add(play, 0, 0);
        lineRow.Add(comboBoxLeft, 0, 1);

        // No clip or rounded corners around the player: on macOS the video is a native surface
        var panel = new Grid
        {
            Margin = new Thickness(12),
            RowDefinitions = { new RowDefinition(new GridLength(1, GridUnitType.Star)), new RowDefinition(GridLength.Auto) },
        };
        panel.Add(vm.VideoPlayerControl, 0, 0);
        panel.Add(lineRow, 1, 0);
        return panel;
    }

    private static Border MakeButtonBar(AssaApplyCustomOverrideTagsViewModel vm)
    {
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(buttonCancel);
        buttons.Children.Add(buttonOk);

        return EditorChrome.WrapButtonBar(buttons);
    }
}
