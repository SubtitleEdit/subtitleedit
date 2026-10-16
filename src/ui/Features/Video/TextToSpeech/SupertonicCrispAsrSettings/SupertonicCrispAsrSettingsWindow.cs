using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SupertonicCrispAsrSettings;

/// <summary>
/// Supertonic (CrispASR) settings, in the editor look (see <see cref="TtsSettingsChrome"/>).
/// </summary>
public class SupertonicCrispAsrSettingsWindow : Window
{
    private const int LabelWidth = 170;

    private readonly SupertonicCrispAsrSettingsViewModel _vm;

    public SupertonicCrispAsrSettingsWindow(SupertonicCrispAsrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.Video.TtsCrispAsrSettingsTitle, "Supertonic");
        TtsSettingsChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        // The weights are OpenRAIL-M: commercial use is allowed, but the licence carries use
        // restrictions and an attribution requirement. Surface that (with a link to the model
        // page) rather than leaving it buried in the download source.
        var license = TtsSettingsChrome.MakeLicenseLink("License: OpenRAIL-M (use restrictions + attribution)", vm.OpenLicensePageCommand);
        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);
        Content = TtsSettingsChrome.MakeContent(
            TtsSettingsChrome.MakeHeader("Supertonic (CrispASR)", new SupertonicCrispAsr().Description, license),
            TtsSettingsChrome.MakeButtonBar(close),
            BuildModels(vm),
            BuildVoice(vm));
    }

    private static Border BuildModels(SupertonicCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.General.Engine, TtsSettingsChrome.MakeStatus(nameof(vm.EngineBrush), nameof(vm.EngineLabel),
                TtsSettingsChrome.MakeEngineDownloadButton(vm.RedownloadEngineCommand, nameof(vm.EngineDownloadButtonText)))),
            (Se.Language.General.Model, TtsSettingsChrome.MakeStatus(nameof(vm.ModelBrush), nameof(vm.ModelLabel))),
            (Se.Language.General.InstallFolder, TtsSettingsChrome.MakeFolder(nameof(vm.ModelsFolder), vm.OpenModelsFolderCommand)));
        return EditorChrome.MakeEditorSection(Se.Language.General.Models, rows);
    }

    private static Border BuildVoice(SupertonicCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.Video.Voices, TtsSettingsChrome.MakeFixedValue("10 preset voices (no voice cloning)")),
            // Server accepts 0.25-4.0; cap UI at 0.5-2.0 since anything outside that range is
            // already a hack rather than a "preferred pace" choice.
            (Se.Language.General.Speed, TtsSettingsChrome.MakeSlider(Se.Language.General.Speed, 0.5, 2.0, 0.05, nameof(vm.Speed), nameof(vm.SpeedLabel))));
        return EditorChrome.MakeEditorSection(Se.Language.General.Voice, rows);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
