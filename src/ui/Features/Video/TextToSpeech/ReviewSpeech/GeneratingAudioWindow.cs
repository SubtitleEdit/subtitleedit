using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;

/// <summary>
/// "Please wait" while one line is regenerated, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class GeneratingAudioWindow : Window
{
    public GeneratingAudioWindow(GeneratingAudioViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Empty;

        // Fix the width and only auto-size the height. SizeToContent.WidthAndHeight
        // renders far too wide on macOS, so keep an explicit width for a consistent
        // look across platforms.
        Width = 360;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TextFieldsBare | EditorWindowStyles.MuteDisabledPrimary);

        DataContext = vm;

        var titleText = new TextBlock
        {
            Text = Se.Language.General.PleaseWait,
            FontSize = EditorChrome.ScaledFont(15),
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        var progressBar = new ProgressBar
        {
            IsIndeterminate = true,
            MinWidth = 300,
            Height = 8,
        };

        var panel = new StackPanel
        {
            Spacing = 14,
            Children =
            {
                titleText,
                progressBar,
            }
        };

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = new StackPanel
        {
            Children =
            {
                new Border
                {
                    Background = EditorChrome.PanelBrush,
                    Padding = new Thickness(24, 22),
                    Child = panel,
                },
                EditorChrome.MakeButtonBar(null, buttonCancel),
            }
        };

        Loaded += delegate
        {
            buttonCancel.Focus();
        };

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                vm.CancelCommand?.Execute(null);
                e.Handled = true;
            }
        };
    }
}
