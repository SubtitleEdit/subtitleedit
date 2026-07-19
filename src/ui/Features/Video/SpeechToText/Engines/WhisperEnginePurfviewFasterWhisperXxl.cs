using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class WhisperEnginePurfviewFasterWhisperXxl : ISpeechToTextEngine
{
    public static string StaticName => "Purfview Faster Whisper XXL";
    public string Name => StaticName;
    public string Choice => WhisperChoice.PurfviewFasterWhisperXxl;
    public string Url => "https://github.com/Purfview/whisper-standalone-win";
    public List<WhisperLanguage> Languages => WhisperLanguage.Languages.OrderBy(p => p.Name).ToList();
    public List<WhisperModel> Models
    {
        get
        {
            var models = new WhisperPurfviewFasterWhisperModel().Models;
            return models.ToList();
        }
    }

    public string Extension => string.Empty;
    public string UnpackSkipFolder => "Whisper-Faster/";

    public bool IsEngineInstalled()
    {
        var executableFile = GetExecutable();
        return File.Exists(executableFile);
    }

    public string GetAndCreateWhisperFolder()
    {
        var baseFolder = Se.SpeechToTextFolder;
        if (!Directory.Exists(baseFolder))
        {
            Directory.CreateDirectory(baseFolder);
        }

        var folder = Path.Combine(baseFolder, "Purfview-Faster-Whisper-XXL");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    /// <summary>
    /// The on-disk "_models" sub-folder name for a model. Most models follow the
    /// "faster-whisper-&lt;name&gt;" convention, but the distil models are published as
    /// "faster-distil-whisper-&lt;name&gt;" (the words are in a different order), so use the
    /// model's explicit Folder when it has one. Building the name from Name alone produced
    /// e.g. "faster-whisper-distil-large-v3.5", which faster-whisper-xxl.exe cannot find, so
    /// it silently re-downloaded the model into its own correctly-named folder (#12133).
    /// </summary>
    internal static string GetModelFolderName(WhisperModel model)
    {
        return !string.IsNullOrEmpty(model.Folder)
            ? model.Folder
            : "faster-whisper-" + model.Name;
    }

    public string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var baseFolder = GetAndCreateWhisperFolder();

        var folder = Path.Combine(baseFolder, "_models");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        if (whisperModel != null)
        {
            folder = Path.Combine(folder, GetModelFolderName(whisperModel));
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        return folder;
    }

    public string GetExecutable()
    {
        var fullPath = Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
        return fullPath;
    }

    public bool IsModelInstalled(WhisperModel model)
    {
        var baseFolder = GetAndCreateWhisperFolder();
        var folder = Path.Combine(baseFolder, "_models", GetModelFolderName(model));
        if (!Directory.Exists(folder))
        {
            return false;
        }

        var binFileName = Path.Combine(folder, "model.bin");
        if (!File.Exists(binFileName))
        {
            // Fallback: try URL-based filename for predefined models
            if (model.Urls != null && model.Urls.Length > 0)
            {
                var urlBin = model.Urls.FirstOrDefault(p => p.EndsWith(".bin"));
                if (urlBin != null)
                {
                    binFileName = Path.Combine(folder, Path.GetFileName(urlBin));
                }
            }

            if (!File.Exists(binFileName))
            {
                return false;
            }
        }

        // Require the finished file, not an in-progress download: the downloader writes a
        // "model.bin.$$$" partial and only renames it to "model.bin" on success, so a partial
        // never matches above. The size floor is a second guard against a truncated file.
        var fileInfo = new FileInfo(binFileName);
        return fileInfo.Length > 10_000_000;
    }

    public string GetModelForCmdLine(string modelName)
    {
        return modelName;
    }

    public bool SupportsCustomModels => true;

    public bool CustomModelIsFolder => true;

    public string ImportCustomModel(string sourcePath)
    {
        return ImportFasterWhisperFolderModel(sourcePath, GetAndCreateWhisperModelFolder(null));
    }

    /// <summary>
    /// Validates and copies a faster-whisper model folder (containing model.bin) into the
    /// shared "_models" folder. The destination is normalized to "faster-whisper-&lt;name&gt;" so
    /// both this engine and the CTranslate2 engine (which builds its --model path as
    /// "_models/faster-whisper-&lt;name&gt;") resolve it. Returns the display name (without prefix).
    /// </summary>
    internal static string ImportFasterWhisperFolderModel(string sourceDir, string modelsFolder)
    {
        if (!Directory.Exists(sourceDir))
        {
            throw new DirectoryNotFoundException("Model folder not found: " + sourceDir);
        }

        var modelBin = Path.Combine(sourceDir, "model.bin");
        if (!File.Exists(modelBin))
        {
            throw new Exception("A faster-whisper model folder must contain a 'model.bin' file.");
        }

        if (new FileInfo(modelBin).Length < 10_000_000)
        {
            throw new Exception("The 'model.bin' file is too small (must be at least 10 MB) - is it really a faster-whisper model?");
        }

        var sourceName = Path.GetFileName(sourceDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var baseName = sourceName;
        if (baseName.StartsWith("faster-whisper-", StringComparison.OrdinalIgnoreCase))
        {
            baseName = baseName.Substring("faster-whisper-".Length);
        }

        var destFolder = Path.Combine(modelsFolder, "faster-whisper-" + baseName);
        CopyDirectory(sourceDir, destFolder);

        return baseName;
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
        }

        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            CopyDirectory(subDir, Path.Combine(destDir, Path.GetFileName(subDir)));
        }
    }

    public override string ToString()
    {
        return Name;
    }

    public async Task<string> GetHelpText()
    {
        var assetName = $"{StaticName.Replace(" ", string.Empty)}.txt";
        var uri = new Uri($"avares://SubtitleEdit/Assets/SpeechToText/{assetName}");

        await using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);

        var contents = await reader.ReadToEndAsync();
        return contents;
    }

    public string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        var folder = GetAndCreateWhisperModelFolder(whisperModel);
        var fileNameOnly = Path.GetFileName(url);
        var fileName = Path.Combine(folder, fileNameOnly);
        return fileName;
    }

    public string GetExecutableFileName()
    {
        if (OperatingSystem.IsWindows())
        {
            return "faster-whisper-xxl.exe";
        }

        return "faster-whisper-xxl";
    }

    public bool CanBeDownloaded()
    {
        return true;
    }

    public string DownloadSizeText
    {
        get
        {
            // Bundles full CUDA + Python runtime, so the archive is large.
            if (OperatingSystem.IsWindows())
            {
                return "~1.4 GB";
            }
            if (OperatingSystem.IsLinux())
            {
                return "~1.6 GB";
            }
            return string.Empty;
        }
    }

    public string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterPurfviewFasterWhisperXxl;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterPurfviewFasterWhisperXxl = value;
    }
}
