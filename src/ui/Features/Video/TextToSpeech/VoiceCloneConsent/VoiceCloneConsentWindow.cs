using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceCloneConsent;

/// <summary>
/// Shown once, before the first voice clone. The "accept" button stays disabled until the
/// checkbox is ticked, so accepting is a deliberate act rather than a reflex click on a
/// default-focused OK — the same shape as the IndexTTS-2.5 licence window.
/// </summary>
public class VoiceCloneConsentWindow : Window
{
    public VoiceCloneConsentWindow(VoiceCloneConsentViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.VoiceCloneConsentTitle;
        // Explicit width rather than SizeToContent.WidthAndHeight: on macOS the latter makes
        // text-heavy windows far too wide.
        Width = 640;
        SizeToContent = SizeToContent.Height;
        CanResize = false;

        vm.Window = this;
        DataContext = vm;

        Content = BuildContent(vm);
    }

    private static Border BuildContent(VoiceCloneConsentViewModel vm)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children =
            {
                BuildHeader(),
                BuildPoints(),
                BuildLinks(vm),
                BuildAcceptRow(vm),
                BuildActions(vm),
            },
        };

        var outerGrid = new Grid { Margin = UiUtil.MakeWindowMargin() };
        outerGrid.Children.Add(stack);

        return new Border
        {
            Child = outerGrid,
            Padding = new Thickness(4),
        };
    }

    private static StackPanel BuildHeader()
    {
        var title = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.VoiceCloneConsentHeader,
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        };

        var subtitle = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.VoiceCloneConsentIntro,
            FontSize = 12,
            Opacity = 0.75,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0),
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Children = { title, subtitle },
        };
    }

    private static Border BuildPoints()
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
        };

        foreach (var point in VoiceCloneConsentViewModel.ConsentPoints)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "•", Opacity = 0.7, VerticalAlignment = VerticalAlignment.Top },
                    new TextBlock { Text = point, TextWrapping = TextWrapping.Wrap, MaxWidth = 560 },
                },
            };
            stack.Children.Add(row);
        }

        return new Border
        {
            Child = stack,
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 128, 128, 128)),
        };
    }

    private static StackPanel BuildLinks(VoiceCloneConsentViewModel vm)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children =
            {
                UiUtil.MakeLink(Se.Language.Video.TextToSpeech.VoiceCloneConsentReadMore, vm.OpenAiActCommand),
            },
        };
    }

    private static CheckBox BuildAcceptRow(VoiceCloneConsentViewModel vm)
    {
        var checkBox = UiUtil.MakeCheckBox(vm, nameof(vm.IsAccepted));
        // A wrapping TextBlock rather than the string overload: this label is a full sentence and
        // a CheckBox's plain-string content does not wrap, so a translation longer than the
        // window is simply cut off - and the one line the user has to read is the one line that
        // must not be.
        checkBox.Content = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.VoiceCloneConsentCheckBox,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 560,
        };

        return checkBox;
    }

    private static Control BuildActions(VoiceCloneConsentViewModel vm)
    {
        var accept = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.VoiceCloneConsentAccept, vm.AcceptCommand);
        // Bound to the checkbox so cloning cannot start on unread terms.
        accept.Bind(
            Button.IsEnabledProperty,
            new Avalonia.Data.Binding(nameof(vm.IsAccepted)));

        var cancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return UiUtil.MakeButtonBar(accept, cancel);
    }
}
