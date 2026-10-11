using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.CommandPalette;

public class CommandPaletteWindow : Window
{
    public CommandPaletteWindow(CommandPaletteViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Main.CommandPaletteTitle;
        CanResize = true;
        Width = 650;
        Height = 500;
        MinWidth = 400;
        MinHeight = 250;
        vm.Window = this;
        DataContext = vm;

        var textBoxSearch = new TextBox
        {
            PlaceholderText = Se.Language.Main.CommandPaletteSearchPlaceholder,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        }.WithSearchAndClearIcons();
        textBoxSearch.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm, Mode = BindingMode.TwoWay });
        textBoxSearch.WithAccessibleName(Se.Language.Main.CommandPaletteTitle);

        // Arrow keys/Enter in the search box drive the list, so typing and picking never need the mouse.
        textBoxSearch.AddHandler(KeyDownEvent, (_, e) => vm.OnKeyDown(e), RoutingStrategies.Tunnel);

        var listBox = new ListBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ItemsSource = vm.Items,
            Focusable = false,
            ItemTemplate = new FuncDataTemplate<CommandPaletteItem>((item, _) => MakeItemView(item), true),
        };
        listBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedItem)) { Source = vm, Mode = BindingMode.TwoWay });
        listBox.DoubleTapped += (_, _) => vm.OkCommand.Execute(null);
        vm.ListBox = listBox;

        var labelNothingFound = new TextBlock
        {
            Text = Se.Language.Main.CommandPaletteNoCommandsFound,
            Opacity = 0.7,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 20, 0, 0),
        }.WithBindVisible(vm, nameof(vm.IsNothingFound));

        var listPanel = new Panel
        {
            Children = { listBox, labelNothingFound },
        };

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
        };

        grid.Add(textBoxSearch, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(listPanel), 1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxSearch);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Control MakeItemView(CommandPaletteItem? item)
    {
        if (item == null)
        {
            return new TextBlock();
        }

        var name = new TextBlock
        {
            Text = item.DisplayName,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };

        var group = new TextBlock
        {
            Text = item.GroupName,
            Opacity = 0.6,
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };

        var shortcut = new TextBlock
        {
            Text = item.ShortcutText,
            Opacity = 0.7,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 0, 0),
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        grid.Add(name, 0, 0);
        grid.Add(group, 0, 1);
        grid.Add(shortcut, 0, 2);
        return grid;
    }
}
