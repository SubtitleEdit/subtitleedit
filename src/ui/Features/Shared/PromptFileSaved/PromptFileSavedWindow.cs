using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;

public class PromptFileSavedWindow : Window
{
    public PromptFileSavedWindow(PromptFileSavedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var isDark = UiTheme.IsDarkThemeEnabled();
        var okForeground = new SolidColorBrush(isDark ? Color.FromRgb(76, 194, 122) : Color.FromRgb(46, 158, 91));
        var okBackground = new SolidColorBrush(isDark ? Color.FromArgb(60, 46, 158, 91) : Color.FromArgb(36, 46, 158, 91));

        // Green check in a soft circle: success is readable before the text is.
        var checkIcon = new ContentControl
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = okForeground,
        };
        Attached.SetIcon(checkIcon, IconNames.Check);
        var okBall = new Border
        {
            Width = 44,
            Height = 44,
            CornerRadius = new CornerRadius(22),
            Background = okBackground,
            VerticalAlignment = VerticalAlignment.Top,
            Child = checkIcon,
        };

        var labelHeadline = new TextBlock
        {
            FontSize = 14.5,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 2, 0, 6),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Title)) { Mode = BindingMode.OneWay },
        };

        var labelFileName = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 340,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.FileNameDisplay)) { Mode = BindingMode.OneWay },
        };

        var labelFolder = new TextBlock
        {
            FontSize = 11.5,
            Opacity = 0.65,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 310,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.FolderDisplay)) { Mode = BindingMode.OneWay },
        };
        var buttonCopyPath = UiUtil.MakeButton(vm.CopyPathCommand, IconNames.Copy);
        buttonCopyPath.Padding = new Thickness(4);
        buttonCopyPath.Margin = new Thickness(2, 0, 0, 0);
        ToolTip.SetTip(buttonCopyPath, Se.Language.General.Copy);
        vm.CopyPathButton = buttonCopyPath; // for the copied-feedback icon flip

        var panelPath = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Margin = new Thickness(0, 1, 0, 0),
            Children = { labelFolder, buttonCopyPath },
        };

        var panelChips = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(0, 8, 0, 0),
            Children =
            {
                MakeChip(nameof(vm.ExtensionChip), nameof(vm.HasExtensionChip), emphasized: true),
                MakeChip(nameof(vm.FileSizeChip), nameof(vm.HasFileSizeChip), emphasized: false),
                MakeChip(nameof(vm.DurationChip), nameof(vm.HasDurationChip), emphasized: false),
            },
        };

        var panelDetails = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { labelHeadline, labelFileName, panelPath, panelChips },
        };

        var panelMain = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            Margin = new Thickness(6, 6, 6, 4),
            Children = { okBall, panelDetails },
        };

        var buttonOpenContainingFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.ShowInFolderCommand)
            .WithIconLeft(IconNames.FolderOpen)
            .WithBindIsVisible(nameof(vm.IsShowInFolderVisible));
        // "Play" for media files, "Open file" for everything else - same command; the view model
        // is initialized before the window is constructed, so the label/icon read is static.
        var buttonOpenFile = UiUtil.MakeButton(vm.OpenFileButtonText, vm.ShowFileCommand)
            .WithBindIsVisible(nameof(vm.IsShowFileVisible));
        if (vm.IsMediaFile)
        {
            buttonOpenFile = buttonOpenFile.WithIconLeft(IconNames.Play);
        }
        var buttonDone = UiUtil.MakeButtonDone(vm.OkCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOpenContainingFolder, buttonOpenFile, buttonDone);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelMain, 0);
        grid.Add(buttonPanel, 1);

        Content = grid;

        Activated += delegate { buttonDone.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    // Small rounded meta chip (extension / file size / duration), same visual language as the
    // info chips in the text-to-speech window.
    private static Border MakeChip(string textPropertyPath, string visiblePropertyPath, bool emphasized)
    {
        return new Border
        {
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(9, 1, 9, 2),
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.FromArgb(28, 128, 128, 128)),
            Child = new TextBlock
            {
                FontSize = 11,
                FontWeight = emphasized ? FontWeight.SemiBold : FontWeight.Normal,
                Opacity = emphasized ? 0.9 : 0.75,
                [!TextBlock.TextProperty] = new Binding(textPropertyPath) { Mode = BindingMode.OneWay },
            },
            [!Border.IsVisibleProperty] = new Binding(visiblePropertyPath) { Mode = BindingMode.OneWay },
        };
    }
}
