using Nikse.SubtitleEdit.Core.AudioToText;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrEngine : CrispAsrEngineBase
{
    private const string NamePrefix = "Crisp ASR ";
    private readonly List<CrispAsrEngineBase> _backends;

    public static string StaticName => "Crisp ASR";
    public IReadOnlyList<CrispAsrEngineBase> Backends => _backends;
    public CrispAsrEngineBase SelectedBackend { get; private set; }
    public string SelectedBackendDisplay => GetBackendDisplayName(SelectedBackend);

    public CrispAsrEngine()
    {
        _backends = new List<CrispAsrEngineBase>
        {
            new CrispAsrParakeet(),
            new CrispAsrCanary(),
            new CrispAsrCohere(),
            new CrispAsrFireRed(),
            new CrispAsrGlm(),
            new CrispAsrGranite(),
            new CrispAsrQwen3(),
            new CrispAsrOmni(),
        };

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

    public void SelectBackend(CrispAsrEngineBase backend)
    {
        var match = _backends.FirstOrDefault(p => ReferenceEquals(p, backend) || string.Equals(p.Choice, backend.Choice, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            SelectedBackend = match;
        }
    }

    public static string GetBackendDisplayName(ICrispAsrEngine backend)
    {
        return backend.Name.StartsWith(NamePrefix, StringComparison.OrdinalIgnoreCase)
            ? backend.Name[NamePrefix.Length..]
            : backend.Name;
    }

    public override string Name => StaticName;
    public override string Choice => SelectedBackend.Choice;
    public override string Url => SelectedBackend.Url;
    public override string BackendName => SelectedBackend.BackendName;
    public override string DefaultLanguage => SelectedBackend.DefaultLanguage;
    public override bool IncludeLanguage => SelectedBackend.IncludeLanguage;
    public override List<WhisperLanguage> Languages => SelectedBackend.Languages;
    public override List<WhisperModel> Models => SelectedBackend.Models;
    public override string Extension => SelectedBackend.Extension;
    public override string UnpackSkipFolder => SelectedBackend.UnpackSkipFolder;
    public override string CommandLineParameter
    {
        get => SelectedBackend.CommandLineParameter;
        set => SelectedBackend.CommandLineParameter = value;
    }

    public override bool IsEngineInstalled() => SelectedBackend.IsEngineInstalled();
    public override bool CanBeDownloaded() => SelectedBackend.CanBeDownloaded();
    public override string GetAndCreateWhisperFolder() => SelectedBackend.GetAndCreateWhisperFolder();
    public override string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel) => SelectedBackend.GetAndCreateWhisperModelFolder(whisperModel);
    public override string GetExecutable() => SelectedBackend.GetExecutable();
    public override bool IsModelInstalled(WhisperModel model) => SelectedBackend.IsModelInstalled(model);
    public override string GetModelForCmdLine(string modelName) => SelectedBackend.GetModelForCmdLine(modelName);
    public override string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url) => SelectedBackend.GetWhisperModelDownloadFileName(whisperModel, url);
    public override Task<string> GetHelpText() => SelectedBackend.GetHelpText();

    public override string ToString()
    {
        return Name;
    }
}
