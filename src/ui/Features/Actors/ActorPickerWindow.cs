using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Actors;

public class ActorPickerWindow : Window
{
    public ActorPickerWindow(ActorPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.ActorPickerTitle);
        Width = 260;
        Height = 420;
        MinWidth = 200;
        MinHeight = 220;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var itemsControl = new ItemsControl
        {
            // MainViewModel replaces the whole Actors collection (e.g. SyncActorPickerActors),
            // so this needs a real binding - a plain ItemsSource = vm.Actors would keep pointing
            // at the original, empty collection.
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Actors)),
            ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 }),
            ItemTemplate = new FuncDataTemplate<ActorDisplayItem>((actor, _) =>
            {
                // A TextBlock, not a plain string Content: Avalonia's Button template treats a
                // bare string as access-key text and would eat a literal "_" in the actor name.
                var nameText = new TextBlock
                {
                    Text = actor!.Name,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                // Null = no shortcut slot for this row (past the 10th); empty string = slot
                // exists, nothing assigned yet.
                var shortcutBadge = new Border
                {
                    IsVisible = actor.ShortcutText != null,
                    Background = UiUtil.GetTextColor(0.08d),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(5, 2),
                    Margin = new Thickness(6, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = new TextBlock
                    {
                        Text = actor.ShortcutText,
                        FontSize = UiUtil.ScaledFontSize(10),
                        Opacity = 0.7,
                    },
                };

                var buttonContent = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                };
                buttonContent.Add(nameText, 0, 0);
                buttonContent.Add(shortcutBadge, 0, 1);

                var assignButton = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Padding = new Thickness(10, 6),
                    BorderThickness = new Thickness(0),
                    Background = Brushes.Transparent,
                    Command = vm.SetActorByNameCommand,
                    CommandParameter = actor,
                    Content = buttonContent,
                };

                assignButton.ContextMenu = new ContextMenu
                {
                    Items =
                    {
                        new MenuItem
                        {
                            Header = Se.Language.General.Rename + "...",
                            Command = vm.RenameActorCommand,
                            CommandParameter = actor,
                        },
                    },
                };

                var subtleBorderBrush = UiUtil.GetTextColor(0.3d);

                var rowBorder = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = subtleBorderBrush,
                    CornerRadius = new CornerRadius(UiUtil.CornerRadius),
                    Child = assignButton,
                };

                // DataContext here is the row's own ActorDisplayItem, not the window's vm.
                var accentBrush = UiUtil.GetAccentBrush();
                var accentColor = (accentBrush as SolidColorBrush)?.Color ?? Colors.DodgerBlue;
                rowBorder.Bind(Border.BorderBrushProperty, new Binding(nameof(ActorDisplayItem.IsHighlighted))
                {
                    Converter = new BooleanToBrushConverter
                    {
                        TrueBrush = accentBrush,
                        FalseBrush = subtleBorderBrush,
                    },
                });
                rowBorder.Bind(Border.BackgroundProperty, new Binding(nameof(ActorDisplayItem.IsHighlighted))
                {
                    Converter = new BooleanToBrushConverter
                    {
                        TrueBrush = new SolidColorBrush(accentColor, 0.18),
                        FalseBrush = Brushes.Transparent,
                    },
                });

                return rowBorder;
            }),
        };

        var scrollViewer = new ScrollViewer
        {
            Content = itemsControl,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var buttonNewActor = UiUtil.MakeButton(Se.Language.General.NewDotDotDot, vm.NewActorCommand);
        buttonNewActor.HorizontalAlignment = HorizontalAlignment.Stretch;
        var buttonClearActor = UiUtil.MakeButton(Se.Language.General.Clear, vm.ClearCommand);
        buttonClearActor.HorizontalAlignment = HorizontalAlignment.Stretch;
        buttonClearActor.WithBindEnabled(nameof(vm.IsClearEnabled));
        var bottomBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 6,
            Margin = new Thickness(0, 8, 0, 0),
        };
        bottomBar.Add(buttonNewActor, 0, 0);
        bottomBar.Add(buttonClearActor, 0, 1);

        var grid = new Grid
        {
            Margin = UiUtil.MakeWindowMargin(),
            RowDefinitions = new RowDefinitions("*,Auto"),
        };
        grid.Add(scrollViewer, 0, 0);
        grid.Add(bottomBar, 1, 0);

        Content = grid;

        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
    }
}
