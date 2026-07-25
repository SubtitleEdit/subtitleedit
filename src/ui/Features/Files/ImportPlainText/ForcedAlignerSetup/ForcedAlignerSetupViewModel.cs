using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText.ForcedAlignerSetup;

public partial class ForcedAlignerSetupViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ForcedAlignerOption> _aligners;
    [ObservableProperty] private ForcedAlignerOption? _selectedAligner;
    [ObservableProperty] private string _engineStatus;
    [ObservableProperty] private string _alignerStatus;
    [ObservableProperty] private bool _isEngineInstalled;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private int _windowSeconds;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>Set once <see cref="Ok"/> runs, so the caller doesn't re-resolve any of this.</summary>
    public string ExecutablePath { get; private set; } = string.Empty;
    public string AlignerModelPath { get; private set; } = string.Empty;

    private readonly IWindowService _windowService;
    private readonly CrispAsrParakeet _engine = new();

    public ForcedAlignerSetupViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        _aligners = new ObservableCollection<ForcedAlignerOption>();
        _engineStatus = string.Empty;
        _alignerStatus = string.Empty;
        _windowSeconds = 240;
    }

    public void Initialize()
    {
        // "Built-in" means "let the ASR backend do its own timing" - there is no such
        // thing when we are aligning without an ASR pass, so it isn't offered here.
        foreach (var option in ForcedAlignerOption.All().Where(o => !o.IsBuiltIn))
        {
            option.IsInstalled = File.Exists(_engine.GetModelForCmdLine(option.FileName));
            option.Display = option.IsInstalled
                ? option.BaseDisplay
                : $"{option.BaseDisplay}  -  not installed ({option.Size})";
            Aligners.Add(option);
        }

        var configured = ForcedAlignerOption.FromChoice(Se.Settings.Tools.AudioToText.CrispAsrForcedAligner);
        SelectedAligner = Aligners.FirstOrDefault(a => a.Choice == configured.Choice)
                          ?? Aligners.FirstOrDefault(a => a.IsInstalled)
                          ?? Aligners.FirstOrDefault();

        RefreshStatus();
    }

    private void RefreshStatus()
    {
        IsEngineInstalled = File.Exists(_engine.GetExecutable());
        EngineStatus = IsEngineInstalled
            ? $"Crisp ASR engine installed ({CrispAsrVersion.TryGet(_engine.GetExecutable()) ?? "version unknown"})"
            : "Crisp ASR engine is not installed";

        if (SelectedAligner == null)
        {
            AlignerStatus = string.Empty;
            return;
        }

        AlignerStatus = SelectedAligner.IsInstalled
            ? $"{SelectedAligner.FileName} is installed"
            : $"{SelectedAligner.FileName} will be downloaded ({SelectedAligner.Size})";
    }

    partial void OnSelectedAlignerChanged(ForcedAlignerOption? value) => RefreshStatus();

    [RelayCommand]
    private async Task DownloadEngine()
    {
        if (Window == null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
                Window, viewModel =>
                {
                    viewModel.Engine = _engine;
                    viewModel.StartDownload();
                });

            if (vm.OkPressed)
            {
                RefreshStatus();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (Window == null || SelectedAligner == null)
        {
            return;
        }

        if (!File.Exists(_engine.GetExecutable()))
        {
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                "The Crisp ASR engine has to be installed before aligning.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var modelPath = _engine.GetModelForCmdLine(SelectedAligner.FileName);
        if (!File.Exists(modelPath))
        {
            var display = new SpeechToTextModelDisplay
            {
                Model = SelectedAligner.ToWhisperModel(),
                Engine = _engine,
            };

            var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
                Window, viewModel =>
                {
                    viewModel.SetModels(new ObservableCollection<SpeechToTextModelDisplay> { display }, _engine, display);
                    viewModel.StartDownload();
                });

            if (!vm.OkPressed || !File.Exists(modelPath))
            {
                return;
            }
        }

        // Remember the choice so the next alignment - and speech-to-text - starts here.
        Se.Settings.Tools.AudioToText.CrispAsrForcedAligner = SelectedAligner.Choice;

        ExecutablePath = _engine.GetExecutable();
        AlignerModelPath = modelPath;
        OkPressed = true;
        Window.Close();
    }

    [RelayCommand]
    private void Cancel() => Window?.Close();

    public void OnKeyDown(Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Escape)
        {
            Cancel();
        }
    }
}
