using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Options.Plugins;

/// <summary>
/// The online plugin catalog in the editor look (see <see cref="EditorChrome"/>): one row per
/// plugin with its install/update button, the status and Refresh + Close in the button bar.
/// </summary>
public class GetPluginsWindow : Window
{
    public GetPluginsWindow(GetPluginsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Plugins.GetPluginsTitle;
        CanResize = true;
        Width = 720;
        Height = 540;
        MinWidth = 500;
        MinHeight = 300;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);
        vm.Window = this;
        DataContext = vm;

        var listBox = new ListBox();
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding(nameof(vm.Plugins)));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(nameof(vm.SelectedPlugin)) { Mode = BindingMode.TwoWay });
        ScrollViewer.SetHorizontalScrollBarVisibility(listBox, ScrollBarVisibility.Disabled);
        AutomationProperties.SetName(listBox, Se.Language.Plugins.GetPluginsTitle);
        listBox.ItemTemplate = new FuncDataTemplate<GetPluginsDisplayItem>((_, _) =>
        {
            var name = new TextBlock { FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Bottom };
            name.Bind(TextBlock.TextProperty, new Binding(nameof(GetPluginsDisplayItem.Name)));

            var version = new TextBlock { Foreground = EditorChrome.FaintTextBrush, FontSize = 11.5, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Bottom };
            version.Bind(TextBlock.TextProperty, new Binding(nameof(GetPluginsDisplayItem.Version)));

            var author = new TextBlock { Foreground = EditorChrome.FaintTextBrush, FontSize = 11.5, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Bottom };
            author.Bind(TextBlock.TextProperty, new Binding(nameof(GetPluginsDisplayItem.Author)));

            // Small accent-coloured "Update" pill that appears only when an update is available.
            var updateBadge = new Border
            {
                Background = EditorChrome.AccentBrush,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(7, 1),
                Margin = new Thickness(8, 0, 0, 1),
                VerticalAlignment = VerticalAlignment.Bottom,
                Child = new TextBlock
                {
                    Text = Se.Language.General.Update,
                    FontSize = 10.5,
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

            var description = new TextBlock { FontSize = 12.5, Foreground = EditorChrome.DimTextBrush, TextWrapping = TextWrapping.Wrap };
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
                FontSize = 12.5,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            actionLabel.Bind(TextBlock.TextProperty, new Binding(nameof(GetPluginsDisplayItem.ActionText)));
            actionLabel.Bind(IsVisibleProperty, new Binding(nameof(GetPluginsDisplayItem.NotBusy)));

            // Busy state: a 60-wide bar that fills the button's full content height with the percent inside.
            // MinWidth=0 is needed to override FluentTheme's default ProgressBar MinWidth, which would
            // otherwise blow past our Width=60.
            var downloadProgress = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Width = 80,
                MinWidth = 0,
                ShowProgressText = true,
                ProgressTextFormat = "{0:0}%",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            downloadProgress.Bind(ProgressBar.ValueProperty, new Binding(nameof(GetPluginsDisplayItem.DownloadProgress)));
            downloadProgress.Bind(IsVisibleProperty, new Binding(nameof(GetPluginsDisplayItem.IsBusy)));

            var buttonContent = new Grid();
            buttonContent.Children.Add(actionLabel);
            buttonContent.Children.Add(downloadProgress);

            var installButton = new Button
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Width = 110,
                MinHeight = 30,
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(10, 0, 0, 0),
                Command = vm.InstallCommand,
                Content = buttonContent,
            };
            installButton.Bind(Button.CommandParameterProperty, new Binding());
            installButton.Bind(IsEnabledProperty, new Binding(nameof(GetPluginsDisplayItem.CanInstall)));
            installButton.Bind(AutomationProperties.NameProperty, new Binding(nameof(GetPluginsDisplayItem.ActionText)));

            // A generous hit area: a 32x32 square with the icon inside. Without an explicit
            // size the button can be missed during a short download (the icon glyph itself
            // is only ~16px wide). Transparent background still receives hit-tests so the
            // whole 32x32 area is clickable.
            var cancelText = Se.Language.General.Cancel.Replace("_", string.Empty);
            var cancelButton = new Button
            {
                Content = new Icon
                {
                    Value = IconNames.Close,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 32,
                Height = 32,
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 0, 0),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Command = vm.CancelDownloadCommand,
            };
            cancelButton.Classes.Add("tool");
            AutomationProperties.SetName(cancelButton, cancelText);
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(cancelButton, cancelText);
            }
            cancelButton.Bind(IsVisibleProperty, new Binding(nameof(GetPluginsDisplayItem.IsBusy)));

            var rowGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                },
            };
            rowGrid.Add(textPanel, 0, 0);
            rowGrid.Add(installButton, 0, 1);
            rowGrid.Add(cancelButton, 0, 2);

            // Thin separator at the bottom of each row.
            return new Border
            {
                BorderBrush = EditorChrome.LineBrush,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 8, 0, 8),
                Child = rowGrid,
            };
        }, true);

        var listSection = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 14),
        };
        listSection.Add(EditorChrome.MakeSectionHeader(Se.Language.Plugins.GetPluginsTitle), 0);
        listSection.Add(EditorChrome.MakeCompactListFrame(listBox, compact: false), 1);

        var statusMessage = new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
        };
        statusMessage.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusMessage)));

        var buttonRefresh = EditorChrome.MakeTextToolButton(IconNames.Refresh, Se.Language.General.Refresh, vm.RefreshCommand);
        var buttonClose = UiUtil.MakeButton(Se.Language.General.Close, vm.CloseCommand);
        buttonClose.Classes.Add("primary");

        Content = EditorChrome.MakeRoot(listSection, EditorChrome.MakeDockedButtonBar(statusMessage, buttonRefresh, buttonClose));

        UiUtil.FocusOnFirstActivation(this, buttonClose);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
