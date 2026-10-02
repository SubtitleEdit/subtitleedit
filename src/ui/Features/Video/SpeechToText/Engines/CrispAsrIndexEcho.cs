using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// Index-Echo S2TT 2B (Bilibili IndexTeam, Apache-2.0, issue #15563): Chinese speech in,
/// translated subtitles out, with native sentence timestamps. A speech *translation* model, so
/// the language list is the target language (passed as --target-lang), not the spoken one - the
/// released recipe only does Chinese to English, Japanese or Spanish. Needs CrispASR v0.8.41+.
///
/// Each model is a pair: the audio tower GGUF (passed with -m) names its Qwen3.5 decoder in its
/// metadata, and crispasr loads that decoder from the same folder, so both files must be present.
/// Q4 quants were rejected upstream (changed translations), and the 9B pair (~18 GB, CPU/CUDA
/// only) is left out.
/// </summary>
public class CrispAsrIndexEcho : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR Index-Echo";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrIndexEcho;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";
    public override string BackendName => "index-echo";
    public override string DefaultLanguage => "en";
    public override bool IncludeLanguage => false;
    public override bool HasNativeTimestamps => true;

    public override List<WhisperLanguage> Languages =>
        new()
        {
            new WhisperLanguage("en", "chinese → english"),
            new WhisperLanguage("ja", "chinese → japanese"),
            new WhisperLanguage("es", "chinese → spanish"),
        };

    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "index-echo-2b-q8_0.gguf",
                Size = "2.7 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/index-echo-2b-GGUF/resolve/main/index-echo-2b-q8_0.gguf",
                    "https://huggingface.co/cstr/index-echo-2b-GGUF/resolve/main/index-echo-2b-decoder-q8_0.gguf",
                ],
            },
            new WhisperModel
            {
                Name = "index-echo-2b-f16.gguf",
                Size = "5.1 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/index-echo-2b-GGUF/resolve/main/index-echo-2b-f16.gguf",
                    "https://huggingface.co/cstr/index-echo-2b-GGUF/resolve/main/index-echo-2b-decoder-f16.gguf",
                ],
            },
       };

    /// <summary>The decoder that pairs with an audio tower, e.g. index-echo-2b-decoder-q8_0.gguf.</summary>
    internal static string GetDecoderFileName(string towerFileName)
    {
        return towerFileName.Replace("index-echo-2b-", "index-echo-2b-decoder-", StringComparison.Ordinal);
    }

    /// <summary>
    /// Index-Echo writes each cue as the Chinese transcript line followed by the translation line;
    /// SE keeps the translation. A cue with a single line (an unparsed window) is kept as is.
    /// Japanese translations come wrapped in 「」 corner brackets, which are stripped when they
    /// enclose the whole line.
    /// </summary>
    internal static string GetTranslation(string text)
    {
        var lines = text.SplitToLines();
        if (lines.Count < 2)
        {
            return text;
        }

        var translation = string.Join(Environment.NewLine, lines.Skip(1)).Trim();
        if (translation.Length > 2 &&
            translation.StartsWith('「') &&
            translation.EndsWith('」') &&
            translation.IndexOf('「', 1) < 0)
        {
            translation = translation[1..^1].Trim();
        }

        return translation;
    }

    public override string Extension => string.Empty;
    public override string UnpackSkipFolder => string.Empty;

    public override bool IsEngineInstalled()
    {
        var executableFile = GetExecutable();
        return File.Exists(executableFile);
    }

    public override string ToString()
    {
        return CrispAsrEngine.GetBackendDisplayName(this);
    }

    public override string GetAndCreateWhisperFolder()
    {
        var folder = Se.CrispAsrFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public override string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var folder = GetAndCreateWhisperFolder();
        var modelsFolder = Path.Combine(folder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public override string GetExecutable()
    {
        string fullPath = Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
        return fullPath;
    }

    public override bool IsModelInstalled(WhisperModel model)
    {
        // The tower alone is useless - crispasr refuses to run without its decoder.
        foreach (var fileName in new[] { model.Name, GetDecoderFileName(model.Name) })
        {
            var modelFile = GetModelForCmdLine(fileName);
            if (!File.Exists(modelFile) || new FileInfo(modelFile).Length < 10_000_000)
            {
                return false;
            }
        }

        return true;
    }

    public override string GetModelForCmdLine(string modelName)
    {
        var modelFileName = Path.Combine(GetAndCreateWhisperModelFolder(null), modelName);
        return modelFileName;
    }

    public override string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        var folder = GetAndCreateWhisperModelFolder(whisperModel);
        var fileNameOnly = Path.GetFileName(url);
        var fileName = Path.Combine(folder, fileNameOnly);
        return fileName;
    }

    internal static string GetExecutableFileName()
    {
        if (OperatingSystem.IsWindows())
        {
            return "crispasr.exe";
        }

        return "crispasr";
    }

    public override bool CanBeDownloaded()
    {
        return true;
    }

    public override string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrIndexEcho;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrIndexEcho = value;
    }
}
