using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Help.CheckForUpdates;

public class CheckForUpdatesWindow : Window
{
    public CheckForUpdatesWindow(CheckForUpdatesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Help.CheckForUpdates;
        Width = 700;
        Height = 550;
        MinWidth = 500;
        MinHeight = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        vm.Window = this;
        DataContext = vm;

        var statusLabel = new TextBlock
        {
            Margin = new Thickness(0, 0, 0, 4),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            DataContext = vm,
        };
        statusLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusText)));

        var downloadIcon = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0),
            Padding = new Thickness(0),
            Foreground = UiUtil.MakeLinkForeground(),
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        Attached.SetIcon(downloadIcon, IconNames.Download);
        downloadIcon.PointerPressed += (_, _) => vm.OpenDownloadPageCommand.Execute(null);

        var downloadLink = UiUtil.MakeLink(Se.Language.Help.CheckForUpdatesDownloadNewVersion, vm.OpenDownloadPageCommand);
        downloadLink.DataContext = vm;

        var downloadPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 4),
            DataContext = vm,
            Children = { downloadIcon, downloadLink },
        };
        downloadPanel.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.IsDownloadLinkVisible)));

        var changeLogBox = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            DataContext = vm,
        };
        changeLogBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.ChangeLogText)));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 5,
        };

        grid.Add(statusLabel, 0);
        grid.Add(downloadPanel, 1);
        grid.Add(changeLogBox, 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += async delegate
        {
            buttonOk.Focus();
            await vm.CheckForUpdates();
        };
        KeyDown += vm.OnKeyDown;
    }
}
