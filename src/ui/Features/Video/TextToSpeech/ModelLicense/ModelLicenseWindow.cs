using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ModelLicense;

/// <summary>
/// Shown once before an engine's first model download when the weights carry their own
/// licence. The "accept" button stays disabled until the checkbox is ticked, so acceptance is
/// a deliberate act rather than a reflex click on a default-focused OK. The content comes from
/// a <see cref="ModelLicenseDefinition"/>, which the caller hands to the view model's
/// Initialize before this constructor runs.
/// </summary>
public class ModelLicenseWindow : Window
{
    public ModelLicenseWindow(ModelLicenseViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.Definition.DialogTitle;
        // Explicit width rather than SizeToContent.WidthAndHeight: on macOS the latter makes
        // text-heavy windows far too wide.
        Width = 640;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TextFieldsUncentered | EditorWindowStyles.MuteDisabledPrimary);

        vm.Window = this;
        DataContext = vm;

        var checkBoxAccept = BuildAcceptRow(vm);
        Content = BuildContent(vm, checkBoxAccept);

        UiUtil.FocusOnFirstActivation(this, checkBoxAccept);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Grid BuildContent(ModelLicenseViewModel vm, CheckBox checkBoxAccept)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children =
            {
                TtsVoicesChrome.MakeTitle(vm.Definition.Header, vm.Definition.Intro),
                TtsVoicesChrome.MakeBulletCard(vm.Definition.SummaryPoints),
                BuildLinks(vm),
                checkBoxAccept,
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

    private static StackPanel BuildLinks(ModelLicenseViewModel vm)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children =
            {
                UiUtil.MakeLink(Se.Language.Video.IndexTts25LicenseReadFullText, vm.OpenLicenseCommand),
                UiUtil.MakeLink(Se.Language.Video.IndexTts25LicenseModelPage, vm.OpenModelPageCommand),
            },
        };
    }

    private static CheckBox BuildAcceptRow(ModelLicenseViewModel vm)
    {
        var checkBox = UiUtil.MakeCheckBox(
            vm.Definition.AcceptCheckBoxText,
            vm,
            nameof(vm.IsAccepted));
        return checkBox;
    }

    private static Control BuildActions(ModelLicenseViewModel vm)
    {
        var accept = UiUtil.MakeButton(Se.Language.Video.IndexTts25LicenseAcceptAndDownload, vm.AcceptCommand);
        // Bound to the checkbox so the download cannot start on an unread licence.
        accept.Bind(
            Button.IsEnabledProperty,
            new Avalonia.Data.Binding(nameof(vm.IsAccepted)));

        accept.Classes.Add("primary");

        var cancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return EditorChrome.MakeButtonBar(null, cancel, accept);
    }
}
