using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;

public partial class VoiceSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _voiceTestText;
    [ObservableProperty] private bool _isImportVoiceVisible;

    private ITtsEngine? _engine;
    private readonly IFileHelper _fileHelper;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public bool RefreshVoices { get; private set; }

    public VoiceSettingsViewModel(IFileHelper fileHelper)
    {
        VoiceTestText = Se.Settings.Video.TextToSpeech.VoiceTestText;
        _fileHelper = fileHelper;
    }

    [RelayCommand]
    private void Ok()
    {
        Se.Settings.Video.TextToSpeech.VoiceTestText = VoiceTestText;
        Se.SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task ImportVoice()
    {
        if (Window == null || _engine == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window!, "Open audio file (for clone)", Se.Language.General.AudioFiles, "*.wav;*.mp3");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var ok = _engine.ImportVoice(fileName);
        if (ok)
        {
            var fileNameOnly = System.IO.Path.GetFileName(fileName);
            await MessageBox.Show(Window, Se.Language.Video.TextToSpeech.VoiceImportSuccessTitle, string.Format(Se.Language.Video.TextToSpeech.VoiceXImported, fileNameOnly));
            RefreshVoices = true;
        }
    }

    [RelayCommand]
    private void RefreshVoiceList()
    {
        Se.Settings.Video.TextToSpeech.VoiceTestText = VoiceTestText;
        Se.SaveSettings();
        RefreshVoices = true;
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(ITtsEngine engine)
    {
        _engine = engine;
        IsImportVoiceVisible = engine.GetType() == typeof(Qwen3TtsCpp);
    }
}