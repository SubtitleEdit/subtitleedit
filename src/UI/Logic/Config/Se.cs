using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config.Language;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Nikse.SubtitleEdit.Logic.Config;

public class Se
{
    public SeGeneral General { get; set; } = new();
    public List<SeShortCut> Shortcuts { get; set; }
    public string Color1 { get; set; } = "#ffff00ff";
    public string Color2 { get; set; } = "#ff0000ff";
    public string Color3 { get; set; } = "#00ff00ff";
    public string Color4 { get; set; } = "#00ffffff";
    public string Color5 { get; set; } = "#000000ff";
    public string Color6 { get; set; } = "#ffffffff";
    public string Color7 { get; set; } = "#ffa500ff";
    public string Color8 { get; set; } = "#ffc0cbff";
    public string Surround1Left { get; set; } = "♪";
    public string Surround1Right { get; set; } = "♪";
    public string Surround2Left { get; set; } = "♫";
    public string Surround2Right { get; set; } = "♫";
    public string Surround3Left { get; set; } = "[";
    public string Surround3Right { get; set; } = "]";
    public SeFile File { get; set; } = new();
    public SeEdit Edit { get; set; } = new();
    public SeTools Tools { get; set; } = new();
    public SeOptions Options { get; set; } = new();
    public SeAutoTranslate AutoTranslate { get; set; } = new();
    public SeSync Synchronization { get; set; } = new();
    public SeSpellCheck SpellCheck { get; set; } = new();
    public SeAppearance Appearance { get; set; } = new();
    public SeAssa Assa { get; set; } = new();
    public SeVideo Video { get; set; } = new();
    public SeWaveform Waveform { get; set; } = new();
    public SeFormats Formats { get; set; } = new();
    public SeOcr Ocr { get; set; } = new();
    public static SeLanguage Language { get; set; } = new();
    public static Se Settings { get; set; } = new();

    public static readonly bool IsInstalledInProgramFiles;
    public static readonly bool IsPortable;
    public static readonly string ExePath;
    public static readonly string DataFolder;

    static Se()
    {
        ExePath = AppContext.BaseDirectory;
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        IsInstalledInProgramFiles =
            ExePath.StartsWith(programFiles, StringComparison.OrdinalIgnoreCase) ||
            ExePath.StartsWith(programFilesX86, StringComparison.OrdinalIgnoreCase);

        IsPortable = !IsInstalledInProgramFiles;

        LogError("Info: Subtitle Edit started. IsPortable: " + IsPortable + ", ExePath: " + ExePath);

        DataFolder = IsPortable
            ? ExePath
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");

        if (!Directory.Exists(DataFolder))
        {
            try
            {
                Directory.CreateDirectory(DataFolder);
            }
            catch
            {
                SeLogger.Error("Error creating data folder: " + DataFolder);
            }
        }
    }

    public static string DictionariesFolder => Path.Combine(DataFolder, "Dictionaries");
    public static string ThemesFolder => Path.Combine(DataFolder, "Themes");
    public static string AutoBackupFolder => Path.Combine(DataFolder, "AutoBackup");
    public static string TtsFolder => Path.Combine(DataFolder, "TTS");
    public static string OcrFolder => Path.Combine(DataFolder, "OCR");
    public static string TranslationFolder => Path.Combine(DataFolder, "Languages");
    public static string PaddleOcrFolder => Path.Combine(OcrFolder, "PaddleOCR3-1");
    public static string PaddleOcrModelsFolder => Path.Combine(PaddleOcrFolder, "models");
    public static string GoogleLensOcrFolder => Path.Combine(OcrFolder, "Google-Lens");
    public static string VlcFolder => Path.Combine(DataFolder, "VLC");
    public static string SevenZipFolder => Path.Combine(DataFolder, "7Zip");
    public static string TesseractFolder
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(DataFolder, "Tesseract550");
            }

            var folders = new List<string>();
            if (Directory.Exists("/opt/homebrew/Cellar/tesseract-lang"))
            {
                foreach (var folder in Directory.EnumerateDirectories("/opt/homebrew/Cellar/tesseract"))
                {
                    folders.Add(Path.Combine(folder, "bin"));
                }
            }

            folders.Add("/usr/local/bin");
            folders.Add("/usr/bin");
            folders.Add("/opt/homebrew/bin");
            folders.Add("/opt/local/bin");
            folders.Add("/usr/local/Cellar");

            foreach (var folder in folders.OrderByDescending(p => p))
            {
                var path = Path.Combine(folder, "tesseract");
                if (System.IO.File.Exists(path))
                {
                    return folder;
                }
            }

            return Path.Combine(DataFolder, "Tesseract550");
        }
    }

    public static string TesseractModelFolder
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(TesseractFolder, "tessdata");
            }

            var folders = new List<string>();

            if (Directory.Exists("/opt/homebrew/Cellar/tesseract-lang"))
            {
                foreach (var folder in Directory.EnumerateDirectories("/opt/homebrew/Cellar/tesseract-lang"))
                {
                    folders.Add(Path.Combine(folder, "share/tessdata"));
                }
            }

            if (Directory.Exists("/usr/share/tesseract-ocr"))
            {
                foreach (var folder in Directory.EnumerateDirectories("/usr/share/tesseract-ocr"))
                {
                    folders.Add(Path.Combine(folder, "tessdata"));
                }
            }

            folders.Add(Path.Combine("/usr/share/tessdata"));
            folders.Add(Path.Combine("/opt/homebrew/share/tessdata/"));

            foreach (var folder in folders.OrderByDescending(p => p))
            {
                if (Directory.Exists(folder))
                {
                    return folder;
                }
            }

            return Path.Combine(TesseractFolder, "tessdata");
        }
    }

    public static string FfmpegFolder => Path.Combine(DataFolder, "ffmpeg");
    public static string WhisperFolder => Path.Combine(DataFolder, "Whisper");
    public static string WaveformsFolder => Path.Combine(DataFolder, "Waveforms");
    public static string SpectrogramsFolder => Path.Combine(DataFolder, "Spectrograms");
    public static string ShotChangesFolder => Path.Combine(DataFolder, "ShotChanges");

    public static string Version { get; set; } = "v5.0.0-beta1";

    public Se()
    {
        Shortcuts = new List<SeShortCut>();
    }

    public void InitializeMainShortcuts(MainViewModel vm)
    {
        if (Shortcuts.Count > 0)
        {
            return;
        }

        Shortcuts = ShortcutsMain.GetDefaultShortcuts(vm);
    }

    public static void SaveSettings()
    {
        var settingsFileName = GetSettingsFilePath();
        SaveSettings(settingsFileName);
    }

    public static void SaveSettings(string settingsFileName)
    {
        var settings = Settings;
        var json = JsonSerializer.Serialize(settings);
        System.IO.File.WriteAllText(settingsFileName, json);

        UpdateLibSeSettings();
    }

    public static void LoadSettings()
    {
        var settingsFileName = GetSettingsFilePath();
        LoadSettings(settingsFileName);
    }

    internal static string GetSettingsFilePath()
    {
        return Path.Combine(DataFolder, "Settings.json");
    }

    public static void LoadSettings(string settingsFileName)
    {
        if (System.IO.File.Exists(settingsFileName))
        {
            var json = System.IO.File.ReadAllText(settingsFileName);

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                options.GetType().GetProperty("IgnoreUnknownProperties")?.SetValue(options, true);

                Settings = JsonSerializer.Deserialize<Se>(json)!;
            }
            catch (Exception exception)
            {
                Se.LogError(exception);
                Settings = new Se();
            }

            SetDefaultValues();

            UpdateLibSeSettings();
        }
    }

    private static void SetDefaultValues()
    {
        if (Settings.Tools == null)
        {
            Settings.Tools = new();
        }

        if (Settings.AutoTranslate == null)
        {
            Settings.AutoTranslate = new();
        }

        if (Settings.File == null)
        {
            Settings.File = new();
        }

        if (Settings.General == null)
        {
            Settings.General = new();
        }

        if (Settings.Synchronization == null)
        {
            Settings.Synchronization = new();
        }

        if (Settings.SpellCheck == null)
        {
            Settings.SpellCheck = new();
        }

        if (Settings.Appearance == null)
        {
            Settings.Appearance = new();
        }

        if (Settings.Assa == null)
        {
            Settings.Assa = new();
        }

        if (Settings.Video == null)
        {
            Settings.Video = new();
        }

        if (Settings.Waveform == null)
        {
            Settings.Waveform = new();
        }

        if (Settings.Ocr == null)
        {
            Settings.Ocr = new();
        }

        if (Settings.Formats == null)
        {
            Settings.Formats = new SeFormats();
        }

        if (Settings.Tools.FixCommonErrors.Profiles.Count == 0)
        {
            Settings.Tools.FixCommonErrors.Profiles.Add(new SeFixCommonErrorsProfile
            {
                ProfileName = "Default",
                SelectedRules = new()
                {
                    nameof(FixEmptyLines),
                    nameof(FixOverlappingDisplayTimes),
                    nameof(FixLongDisplayTimes),
                    nameof(FixShortDisplayTimes),
                    nameof(FixShortGaps),
                    nameof(FixInvalidItalicTags),
                    nameof(FixUnneededSpaces),
                    nameof(FixMissingSpaces),
                    nameof(FixUnneededPeriods),
                },
            });
            Settings.Tools.FixCommonErrors.LastProfileName = "Default";
        }
    }

    private static string GetSeInfo()
    {
        try
        {
            return $"{Version} - {Environment.OSVersion} - {IntPtr.Size * 8}-bit";
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void UpdateLibSeSettings()
    {
        Configuration.Settings.General.FFmpegLocation = Settings.General.FfmpegPath;
        Configuration.Settings.General.UseDarkTheme = Settings.Appearance.Theme == "Dark";
        Configuration.Settings.General.UseTimeFormatHHMMSSFF = Settings.General.UseFrameMode;

        var tts = Settings.Tools.AudioToText;
        Configuration.Settings.Tools.WhisperChoice = tts.WhisperChoice;
        Configuration.Settings.Tools.WhisperIgnoreVersion = tts.WhisperIgnoreVersion;
        Configuration.Settings.Tools.WhisperDeleteTempFiles = tts.WhisperDeleteTempFiles;
        Configuration.Settings.Tools.WhisperModel = tts.WhisperModel;
        Configuration.Settings.Tools.WhisperLanguageCode = tts.WhisperLanguageCode;
        Configuration.Settings.Tools.WhisperLocation = tts.WhisperLocation;
        Configuration.Settings.Tools.WhisperCtranslate2Location = tts.WhisperCtranslate2Location;
        Configuration.Settings.Tools.WhisperPurfviewFasterWhisperLocation = tts.WhisperPurfviewFasterWhisperLocation;
        Configuration.Settings.Tools.WhisperPurfviewFasterWhisperDefaultCmd = tts.WhisperPurfviewFasterWhisperDefaultCmd;
        Configuration.Settings.Tools.WhisperXLocation = tts.WhisperXLocation;
        Configuration.Settings.Tools.WhisperStableTsLocation = tts.WhisperStableTsLocation;
        Configuration.Settings.Tools.WhisperCppModelLocation = tts.WhisperCppModelLocation;
        Configuration.Settings.Tools.WhisperExtraSettings = tts.WhisperCustomCommandLineArguments;
        Configuration.Settings.Tools.WhisperExtraSettingsHistory = tts.WhisperExtraSettingsHistory;
        Configuration.Settings.Tools.WhisperAutoAdjustTimings = tts.WhisperAutoAdjustTimings;
        Configuration.Settings.Tools.WhisperUseLineMaxChars = tts.WhisperUseLineMaxChars;
        Configuration.Settings.Tools.WhisperPostProcessingAddPeriods = tts.WhisperPostProcessingAddPeriods;
        Configuration.Settings.Tools.WhisperPostProcessingMergeLines = tts.WhisperPostProcessingMergeLines;
        Configuration.Settings.Tools.WhisperPostProcessingSplitLines = tts.WhisperPostProcessingSplitLines;
        Configuration.Settings.Tools.WhisperPostProcessingFixCasing = tts.WhisperPostProcessingFixCasing;
        Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration = tts.WhisperPostProcessingFixShortDuration;
        Configuration.Settings.Tools.VoskPostProcessing = tts.PostProcessing;

        Configuration.Settings.Tools.AutoTranslateLastName = Settings.AutoTranslate.AutoTranslateLastName;

        Configuration.Settings.Tools.ImportTextSplitting = Settings.Tools.ImportTextSplitting;
        Configuration.Settings.Tools.ImportTextSplittingLineMode = Settings.Tools.ImportTextSplittingLineMode;
        Configuration.Settings.Tools.ImportTextLineBreak = Settings.Tools.ImportTextLineBreak;
        Configuration.Settings.Tools.ImportTextMergeShortLines = Settings.Tools.ImportTextMergeShortLines;
        Configuration.Settings.Tools.ImportTextAutoSplitAtBlank = Settings.Tools.ImportTextAutoSplitAtBlank;
        Configuration.Settings.Tools.ImportTextRemoveLinesNoLetters = Settings.Tools.ImportTextRemoveLinesNoLetters;
        Configuration.Settings.Tools.ImportTextGenerateTimeCodes = Settings.Tools.ImportTextGenerateTimeCodes;
        Configuration.Settings.Tools.ImportTextAutoBreak = Settings.Tools.ImportTextAutoBreak;
        Configuration.Settings.Tools.ImportTextAutoBreakAtEnd = Settings.Tools.ImportTextAutoBreakAtEnd;
        Configuration.Settings.Tools.ImportTextGap = Settings.Tools.ImportTextGap;
        Configuration.Settings.Tools.ImportTextAutoSplitNumberOfLines = Settings.Tools.ImportTextAutoSplitNumberOfLines;
        Configuration.Settings.Tools.ImportTextAutoBreakAtEndMarkerText = Settings.Tools.ImportTextAutoBreakAtEndMarkerText;
        Configuration.Settings.Tools.ImportTextDurationAuto = Settings.Tools.ImportTextDurationAuto;
        Configuration.Settings.Tools.ImportTextFixedDuration = Settings.Tools.ImportTextFixedDuration;
    }

    public static string GetErrorLogFilePath()
    {
        return Path.Combine(DataFolder, "error_log.txt");
    }

    public static string GetWhisperLogFilePath()
    {
        return Path.Combine(DataFolder, "whisper_log.txt");
    }

    public static void WriteWhisperLog(string log)
    {
        try
        {
            var filePath = GetWhisperLogFilePath();
            using var writer = new StreamWriter(filePath, true, Encoding.UTF8);
            writer.WriteLine("-----------------------------------------------------------------------------");
            writer.WriteLine($"Date: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
            writer.WriteLine($"SE: {GetSeInfo()}");
            writer.WriteLine(log);
            writer.WriteLine();
        }
        catch
        {
            // ignore
        }
    }

    public static void LogError(Exception exception)
    {
        LogError(exception.Message + Environment.NewLine + exception.StackTrace);
    }

    public static void LogError(Exception exception, string message)
    {
        LogError(exception.Message + Environment.NewLine + message + Environment.NewLine + exception.StackTrace);
    }

    public static void LogError(string error)
    {
        try
        {
            var filePath = GetErrorLogFilePath();
            using var writer = new StreamWriter(filePath, true, Encoding.UTF8);
            writer.WriteLine("-----------------------------------------------------------------------------");
            writer.WriteLine($"Date: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
            writer.WriteLine($"SE: {GetSeInfo()}");
            writer.WriteLine(error);
            writer.WriteLine();
        }
        catch
        {
            // ignore
        }
    }
}