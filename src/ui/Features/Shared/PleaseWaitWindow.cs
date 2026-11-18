using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared;

/// <summary>
/// Busy indicator in the editor look (see <see cref="EditorChrome"/>): the status text over
/// an accent progress bar, plus a Cancel button for callers that set
/// <see cref="PleaseWaitViewModel.CanCancel"/>.
/// </summary>
public class PleaseWaitWindow : Window
{
    public PleaseWaitWindow(PleaseWaitViewModel vm)
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
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        DataContext = vm;

        var titleText = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(15),
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8),
        };
        titleText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusText)));

        var progressBar = new ProgressBar
        {
            IsIndeterminate = true,
            Minimum = 0,
            Maximum = 100,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 6,
            MinHeight = 6,
            Foreground = EditorChrome.AccentBrush,
            Background = EditorChrome.LineBrush,
            CornerRadius = new CornerRadius(3),
        };
        progressBar.Bind(ProgressBar.IsIndeterminateProperty, new Binding(nameof(vm.IsIndeterminate)));
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand);
        buttonCancel.HorizontalAlignment = HorizontalAlignment.Right;
        buttonCancel.Margin = new Thickness(0, 6, 0, 0);
        buttonCancel.Bind(IsVisibleProperty, new Binding(nameof(vm.CanCancel)));

        Content = new Border
        {
            Background = EditorChrome.PanelBrush,
            Padding = new Thickness(20, 18, 20, 20),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    titleText,
                    progressBar,
                    buttonCancel,
                }
            },
        };
    }
}
