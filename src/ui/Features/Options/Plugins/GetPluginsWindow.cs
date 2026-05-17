using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Plugins;

public class GetPluginsWindow : Window
{
    private static readonly IBrush AccentBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));

    public GetPluginsWindow(GetPluginsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Plugins.GetPluginsTitle;
        CanResize = true;
        Width = 700;
        Height = 500;
        MinWidth = 500;
        MinHeight = 300;
        vm.Window = this;
        DataContext = vm;

        var listBox = new ListBox();
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding(nameof(vm.Plugins)));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(nameof(vm.SelectedPlugin)) { Mode = BindingMode.TwoWay });
        listBox.ItemTemplate = new FuncDataTemplate<GetPluginsDisplayItem>((_, _) =>
        {
            var name = new TextBlock { FontWeight = FontWeight.Bold, VerticalAlignment = VerticalAlignment.Bottom };
            name.Bind(TextBlock.TextProperty, new Binding(nameof(GetPluginsDisplayItem.Name)));

            var version = new TextBlock { Opacity = 0.7, FontSize = 11, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Bottom };
            version.Bind(TextBlock.TextProperty, new Binding(nameof(GetPluginsDisplayItem.Version)));

            var author = new TextBlock { Opacity = 0.7, FontSize = 11, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Bottom };
            author.Bind(TextBlock.TextProperty, new Binding(nameof(GetPluginsDisplayItem.Author)));

            // Small accent-coloured "Update" pill that appears only when an update is available.
            var updateBadge = new Border
            {
                Background = AccentBrush,
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(6, 1),
                Margin = new Thickness(8, 0, 0, 1),
                VerticalAlignment = VerticalAlignment.Bottom,
                Child = new TextBlock
                {
                    Text = Se.Language.Plugins.Update,
                    FontSize = 10,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = Brushes.White,
                },
            };
            updateBadge.Bind(IsVisibleProperty, new Binding(nameof(GetPluginsDisplayItem.UpdateAvailable)));

            var titleRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { name, version, author, updateBadge },
            };

            var description = new TextBlock { Opacity = 0.8, TextWrapping = TextWrapping.Wrap };
            description.Bind(TextBlock.TextProperty, new Binding(nameof(GetPluginsDisplayItem.Description)));

            var status = new TextBlock { FontSize = 11 };
            status.Bind(TextBlock.TextProperty, new Binding(nameof(GetPluginsDisplayItem.StatusText)));
            status.Bind(TextBlock.ForegroundProperty, new Binding(nameof(GetPluginsDisplayItem.StatusBrush)));
            status.Bind(TextBlock.FontWeightProperty, new Binding(nameof(GetPluginsDisplayItem.StatusFontWeight)));
            status.Bind(TextBlock.OpacityProperty, new Binding(nameof(GetPluginsDisplayItem.StatusOpacity)));

            var textPanel = new StackPanel
            {
                Spacing = 3,
                Children = { titleRow, description, status },
            };

            var actionLabel = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            actionLabel.Bind(TextBlock.TextProperty, new Binding(nameof(GetPluginsDisplayItem.ActionText)));
            actionLabel.Bind(IsVisibleProperty, new Binding(nameof(GetPluginsDisplayItem.NotBusy)));

            // Slim progress bar with the percentage inside.
            var downloadProgress = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Width = 56,
                Height = 14,
                ShowProgressText = true,
                ProgressTextFormat = "{0:0}%",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            downloadProgress.Bind(ProgressBar.ValueProperty, new Binding(nameof(GetPluginsDisplayItem.DownloadProgress)));
            downloadProgress.Bind(IsVisibleProperty, new Binding(nameof(GetPluginsDisplayItem.IsBusy)));

            var buttonContent = new Grid();
            buttonContent.Children.Add(actionLabel);
            buttonContent.Children.Add(downloadProgress);

            var installButton = new Button
            {
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 100,
                Command = vm.InstallCommand,
                Content = buttonContent,
            };
            installButton.Bind(Button.CommandParameterProperty, new Binding());
            installButton.Bind(IsEnabledProperty, new Binding(nameof(GetPluginsDisplayItem.CanInstall)));

            var rowGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto },
                },
            };
            rowGrid.Add(textPanel, 0, 0);
            rowGrid.Add(installButton, 0, 1);

            // Thin separator at the bottom of each row.
            return new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x22, 0x80, 0x80, 0x80)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 8, 0, 8),
                Child = rowGrid,
            };
        }, true);

        var statusMessage = new TextBlock { Opacity = 0.8 };
        statusMessage.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusMessage)));

        var buttonRefresh = UiUtil.MakeButton(Se.Language.General.Refresh, vm.RefreshCommand).WithMinWidth(120);
        var buttonClose = UiUtil.MakeButton(Se.Language.General.Close, vm.CloseCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonRefresh, buttonClose);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        grid.Add(listBox, 0, 0);
        grid.Add(statusMessage, 1, 0);
        grid.Add(panelButtons, 2, 0);

        Content = grid;

        Activated += delegate { buttonClose.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
