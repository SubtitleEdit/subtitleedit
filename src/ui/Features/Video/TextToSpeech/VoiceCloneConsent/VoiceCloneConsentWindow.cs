using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager;
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TextFieldsUncentered | EditorWindowStyles.MuteDisabledPrimary);

        vm.Window = this;
        DataContext = vm;

        Content = BuildContent(vm);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Grid BuildContent(VoiceCloneConsentViewModel vm)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children =
            {
                TtsVoicesChrome.MakeTitle(Se.Language.Video.TextToSpeech.VoiceCloneConsentHeader, Se.Language.Video.TextToSpeech.VoiceCloneConsentIntro),
                TtsVoicesChrome.MakeBulletCard(VoiceCloneConsentViewModel.ConsentPoints),
                BuildLinks(vm),
                BuildAcceptRow(vm),
            },
        };

        var body = new Border
        {
            Child = stack,
            Background = EditorChrome.PanelBrush,
            Padding = new Thickness(20, 18),
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };
        grid.Add(body, 0, 0);
        grid.Add(BuildActions(vm), 1, 0);
        return grid;
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
        checkBox.FontWeight = FontWeight.SemiBold;

        return checkBox;
    }

    private static Control BuildActions(VoiceCloneConsentViewModel vm)
    {
        var accept = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.VoiceCloneConsentAccept, vm.AcceptCommand);
        // Bound to the checkbox so cloning cannot start on unread terms.
        accept.Bind(
            Button.IsEnabledProperty,
            new Avalonia.Data.Binding(nameof(vm.IsAccepted)));

        accept.Classes.Add("primary");

        var cancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return EditorChrome.MakeFlushButtonBar(null, cancel, accept);
    }
}
