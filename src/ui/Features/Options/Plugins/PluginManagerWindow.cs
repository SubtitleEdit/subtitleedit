using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Plugins;

public class PluginManagerWindow : Window
{
    public PluginManagerWindow(PluginManagerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Plugins.Title;
        CanResize = true;
        Width = 650;
        Height = 450;
        MinWidth = 500;
        MinHeight = 300;
        vm.Window = this;
        DataContext = vm;

        var listBox = new ListBox();
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding(nameof(vm.Plugins)));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(nameof(vm.SelectedPlugin)) { Mode = BindingMode.TwoWay });
        listBox.ItemTemplate = new FuncDataTemplate<PluginDisplayItem>((_, _) =>
        {
            var checkBox = new CheckBox { VerticalAlignment = VerticalAlignment.Center };
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(PluginDisplayItem.IsEnabled)) { Mode = BindingMode.TwoWay });
            checkBox.Bind(IsEnabledProperty, new Binding(nameof(PluginDisplayItem.CanRun)));

            var name = new TextBlock { FontWeight = FontWeight.Bold };
            name.Bind(TextBlock.TextProperty, new Binding(nameof(PluginDisplayItem.Name)));

            var version = new TextBlock { Opacity = 0.7, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Bottom };
            version.Bind(TextBlock.TextProperty, new Binding(nameof(PluginDisplayItem.Version)));

            var author = new TextBlock { Opacity = 0.7, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Bottom };
            author.Bind(TextBlock.TextProperty, new Binding(nameof(PluginDisplayItem.Author)));

            var titleRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { name, version, author },
            };

            var description = new TextBlock { Opacity = 0.8, TextWrapping = TextWrapping.Wrap };
            description.Bind(TextBlock.TextProperty, new Binding(nameof(PluginDisplayItem.Description)));

            var status = new TextBlock { Opacity = 0.6, FontSize = 11 };
            status.Bind(TextBlock.TextProperty, new Binding(nameof(PluginDisplayItem.StatusText)));

            var textPanel = new StackPanel
            {
                Spacing = 2,
                Margin = new Thickness(8, 0, 0, 0),
                Children = { titleRow, description, status },
            };

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 4),
                Children = { checkBox, textPanel },
            };
        }, true);

        var buttonGetPlugins = UiUtil.MakeButton(Se.Language.Plugins.GetPluginsOnline, vm.GetPluginsOnlineCommand)
            .WithIconLeft(IconNames.CloudDownload)
            .WithHorizontalAlignmentStretch()
            .WithMinWidth(180);
        var buttonOpenFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenPluginsFolderCommand)
            .WithIconLeft(IconNames.FolderOpen)
            .WithHorizontalAlignmentStretch()
            .WithMinWidth(180);
        var buttonRemove = UiUtil.MakeButton(Se.Language.General.Remove, vm.RemovePluginCommand)
            .WithIconLeft(IconNames.Trash)
            .WithHorizontalAlignmentStretch()
            .WithMinWidth(180);

        var sidePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Margin = new Thickness(10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Children = { buttonGetPlugins, buttonOpenFolder, buttonRemove },
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

        var buttonClose = UiUtil.MakeButton(Se.Language.General.Close, vm.CloseCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonClose);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        grid.Add(contentGrid, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        Activated += delegate { buttonClose.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
