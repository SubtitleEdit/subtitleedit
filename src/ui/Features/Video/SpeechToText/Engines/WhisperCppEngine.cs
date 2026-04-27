using Nikse.SubtitleEdit.Core.AudioToText;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class WhisperCppEngine : ISpeechToTextEngine
{
    private readonly List<ISpeechToTextEngine> _backends;

    public static string StaticName => "Whisper CPP";
    public IReadOnlyList<ISpeechToTextEngine> Backends => _backends;
    public ISpeechToTextEngine SelectedBackend { get; private set; }

    public WhisperCppEngine()
    {
        _backends = new List<ISpeechToTextEngine>
        {
            new WhisperEngineCpp(),
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _backends.Add(new WhisperEngineCppCuBlas());
            _backends.Add(new WhisperEngineCppVulkan());
        }

        SelectedBackend = _backends[0];
    }

    public bool TrySelectBackendChoice(string choice)
    {
        var backend = _backends.FirstOrDefault(p =>
            string.Equals(p.Choice, choice, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.Name, choice, StringComparison.OrdinalIgnoreCase));

        if (backend == null)
        {
            return false;
        }

        SelectBackend(backend);
        return true;
    }

    public void SelectBackend(ISpeechToTextEngine backend)
    {
        var match = _backends.FirstOrDefault(p => ReferenceEquals(p, backend) || string.Equals(p.Choice, backend.Choice, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            SelectedBackend = match;
        }
    }

    public static string GetBackendDisplayName(ISpeechToTextEngine backend)
    {
        return backend switch
        {
            WhisperEngineCpp => "CPU",
            WhisperEngineCppCuBlas => "cuBLAS",
            WhisperEngineCppVulkan => "Vulkan",
            _ => backend.Name,
        };
    }

    public string Name => StaticName;
    public string Choice => SelectedBackend.Choice;
    public string Url => SelectedBackend.Url;
    public List<WhisperLanguage> Languages => SelectedBackend.Languages;
    public List<WhisperModel> Models => SelectedBackend.Models;
    public string Extension => SelectedBackend.Extension;
    public string UnpackSkipFolder => SelectedBackend.UnpackSkipFolder;
    public string CommandLineParameter
    {
        get => SelectedBackend.CommandLineParameter;
        set => SelectedBackend.CommandLineParameter = value;
    }

    public bool IsEngineInstalled() => SelectedBackend.IsEngineInstalled();
    public bool CanBeDownloaded() => SelectedBackend.CanBeDownloaded();
    public string GetAndCreateWhisperFolder() => SelectedBackend.GetAndCreateWhisperFolder();
    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel) => SelectedBackend.GetAndCreateWhisperModelFolder(whisperModel);
    public string GetExecutable() => SelectedBackend.GetExecutable();
    public bool IsModelInstalled(WhisperModel model) => SelectedBackend.IsModelInstalled(model);
    public string GetModelForCmdLine(string modelName) => SelectedBackend.GetModelForCmdLine(modelName);
    public Task<string> GetHelpText() => SelectedBackend.GetHelpText();
    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url) => SelectedBackend.GetWhisperModelDownloadFileName(whisperModel, url);

    public override string ToString()
    {
        return Name;
    }
}
