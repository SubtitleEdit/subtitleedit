using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;

/// <summary>
/// "File saved" in the editor look (see <see cref="EditorChrome"/>): a green check, the file
/// name, where it went and meta chips, the file actions and Done in the button bar.
/// </summary>
public class PromptFileSavedWindow : Window
{
    // Follow the app theme, also when it changes while the dialog is open
    private static readonly IBrush OkForeground = EditorChrome.Themed(Color.FromRgb(76, 194, 122), Color.FromRgb(46, 158, 91));
    private static readonly IBrush OkBackground = EditorChrome.Themed(Color.FromArgb(60, 46, 158, 91), Color.FromArgb(36, 46, 158, 91));

    public PromptFileSavedWindow(PromptFileSavedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        // Green check in a soft circle: success is readable before the text is.
        var checkIcon = new ContentControl
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = OkForeground,
        };
        Attached.SetIcon(checkIcon, IconNames.Check);
        var okBall = new Border
        {
            Width = 44,
            Height = 44,
            CornerRadius = new CornerRadius(22),
            Background = OkBackground,
            VerticalAlignment = VerticalAlignment.Top,
            Child = checkIcon,
        };

        var labelHeadline = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(14.5),
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
            FontSize = UiUtil.ScaledFontSize(11.5),
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 310,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.FolderDisplay)) { Mode = BindingMode.OneWay },
        };
        var buttonCopyPath = UiUtil.MakeButton(vm.CopyPathCommand, IconNames.Copy, Se.Language.General.Copy);
        buttonCopyPath.Classes.Add("tool");
        buttonCopyPath.Width = 24;
        buttonCopyPath.Height = 24;
        buttonCopyPath.Margin = new Thickness(2, 0, 0, 0);
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
                MakeChip(nameof(vm.ElapsedChip), nameof(vm.HasElapsedChip), emphasized: false),
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
            Margin = new Thickness(20, 18, 24, 18),
            Children = { okBall, panelDetails },
        };

        var buttonOpenContainingFolder = EditorChrome.MakeTextToolButton("fa-solid fa-folder-open", Se.Language.General.OpenContainingFolder, vm.ShowInFolderCommand)
            .WithBindIsVisible(nameof(vm.IsShowInFolderVisible));
        // "Play" for media files, "Open file" for everything else - same command; the view model
        // is initialized before the window is constructed, so the label/icon read is static.
        var buttonOpenFile = EditorChrome.MakeTextToolButton(vm.IsMediaFile ? "fa-solid fa-play" : "fa-regular fa-file-lines", vm.OpenFileButtonText, vm.ShowFileCommand)
            .WithBindIsVisible(nameof(vm.IsShowFileVisible));
        var buttonDone = EditorChrome.AsPrimary(UiUtil.MakeButtonDone(vm.OkCommand));
        var panelActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(-8, 0, 0, 0), // the flat buttons' text lines up with the content
            Children = { buttonOpenContainingFolder, buttonOpenFile },
        };

        Content = EditorChrome.MakeRoot(panelMain, EditorChrome.MakeButtonBar(panelActions, buttonDone));

        UiUtil.FocusOnFirstActivation(this, buttonDone); // hack to make OnKeyDown work
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
            Background = EditorChrome.HoverBrush,
            Child = new TextBlock
            {
                FontSize = UiUtil.ScaledFontSize(11),
                FontWeight = emphasized ? FontWeight.SemiBold : FontWeight.Normal,
                Opacity = emphasized ? 0.9 : 0.75,
                [!TextBlock.TextProperty] = new Binding(textPropertyPath) { Mode = BindingMode.OneWay },
            },
            [!Border.IsVisibleProperty] = new Binding(visiblePropertyPath) { Mode = BindingMode.OneWay },
        };
    }
}
