using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AudioCppTtsSettings;

/// <summary>
/// Settings dialog shared by the audio.cpp engines that have no per-request knobs (Higgs
/// Audio v3, Fish Audio S2 Pro): runtime status with re-download, the two model quants,
/// imported-voices count, and folder shortcuts. Which engine it shows comes from the view
/// model's <see cref="AudioCppTtsSettingsAdapter"/>, handed to Initialize before this
/// constructor runs. In the editor look (see <see cref="TtsSettingsChrome"/>).
/// </summary>
public class AudioCppTtsSettingsWindow : Window
{
    private const int LabelWidth = 150;

    private readonly AudioCppTtsSettingsViewModel _vm;

    public AudioCppTtsSettingsWindow(AudioCppTtsSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.Adapter.EngineName + " settings";
        TtsSettingsChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var close = UiUtil.MakeButtonOk(vm.OkCommand);
        Content = TtsSettingsChrome.MakeContent(
            TtsSettingsChrome.MakeHeader(vm.Adapter.EngineName, vm.Adapter.Description),
            TtsSettingsChrome.MakeButtonBar(close),
            BuildModels(vm));
    }

    private static Border BuildModels(AudioCppTtsSettingsViewModel vm)
    {
        var openVoicesFolder = TtsSettingsChrome.MakeFolderButton(Se.Language.Video.TextToSpeech.OpenVoicesFolder, vm.OpenVoicesFolderCommand);
        openVoicesFolder.IsVisible = vm.HasVoicesFolder;

        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.General.Engine, TtsSettingsChrome.MakeStatus(nameof(vm.EngineBrush), nameof(vm.EngineLabel),
                TtsSettingsChrome.MakeEngineDownloadButton(vm.RedownloadEngineCommand, nameof(vm.EngineDownloadButtonText)))),
            ("Model " + vm.Adapter.ModelKeyDefault, TtsSettingsChrome.MakeStatus(nameof(vm.ModelDefaultBrush), nameof(vm.ModelDefaultLabel),
                TtsSettingsChrome.MakeModelDownloadButton(vm.DownloadModelCommand, vm.Adapter.ModelKeyDefault))),
            ("Model " + vm.Adapter.ModelKeyAlt, TtsSettingsChrome.MakeStatus(nameof(vm.ModelAltBrush), nameof(vm.ModelAltLabel),
                TtsSettingsChrome.MakeModelDownloadButton(vm.DownloadModelCommand, vm.Adapter.ModelKeyAlt))),
            (Se.Language.Video.Voices, TtsSettingsChrome.MakeValue(nameof(vm.VoicesLabel), openVoicesFolder)),
            (Se.Language.General.InstallFolder, TtsSettingsChrome.MakeFolder(nameof(vm.ModelsFolder), vm.OpenModelsFolderCommand)));
        return EditorChrome.MakeEditorSection(Se.Language.General.Models, rows);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
