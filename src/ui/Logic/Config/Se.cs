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
    internal const int CurrentMacOsFontMigrationVersion = 1;

    public static string Version { get; set; } = "v5.1.0-beta10";

    public SeGeneral General { get; set; } = new();
    public List<SeShortCut> Shortcuts { get; set; } = new();
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
    public string Actor1 { get; set; } = "Actor 1";
    public string Actor2 { get; set; } = "Actor 2";
    public string Actor3 { get; set; } = "Actor 3";
    public string Actor4 { get; set; } = "Actor 4";
    public string Actor5 { get; set; } = "Actor 5";
    public string Actor6 { get; set; } = "Actor 6";
    public string Actor7 { get; set; } = "Actor 7";
    public string Actor8 { get; set; } = "Actor 8";
    public string Actor9 { get; set; } = "Actor 9";
    public string Actor10 { get; set; } = "Actor 10";
    public SeFile File { get; set; } = new();
    public SeEdit Edit { get; set; } = new();
    public SeTools Tools { get; set; } = new();
    public SeOptions Options { get; set; } = new();
    public SeAutoTranslate AutoTranslate { get; set; } = new();
    public SeSync Synchronization { get; set; } = new();
    public SeSpellCheck SpellCheck { get; set; } = new();
    public SeAppearance Appearance { get; set; } = new();
    public SeAssa Assa { get; set; } = new();
    public SeSsa Ssa { get; set; } = new();
    public SeVideo Video { get; set; } = new();
    public SeWaveform Waveform { get; set; } = new();
    public SeBeautifyTimeCodes BeautifyTimeCodes { get; set; } = new();
    public SeFormats Formats { get; set; } = new();
    public SeOcr Ocr { get; set; } = new();
    public SePlugins Plugins { get; set; } = new();
    public static SeLanguage Language { get; set; } = new();
    public static Se Settings { get; set; } = new();

    public static readonly bool IsInstalledInProgramFiles;
    public static readonly bool IsPortable;
    public static readonly string ExePath;
    public static readonly string DataFolder;
    internal static string? SettingsFilePathOverride { get; set; }

    static Se()
    {
        ExePath = AppContext.BaseDirectory;
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        IsInstalledInProgramFiles =
            ExePath.StartsWith(programFiles, StringComparison.OrdinalIgnoreCase) ||
            ExePath.StartsWith(programFilesX86, StringComparison.OrdinalIgnoreCase);

        IsPortable = !IsInstalledInProgramFiles;

        DataFolder = IsPortable
            ? ExePath
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit");

        try
        {
            Directory.CreateDirectory(DataFolder);
        }
        catch
        {
            SeLogger.Error("Error creating data folder: " + DataFolder);
        }

        // Sync libse Configuration so it uses the same data folder as Se.
        // Without this, Configuration.GetDataDirectory() uses its own heuristics and may
        // create or return %AppData%\Subtitle Edit even when running in portable mode.
        Configuration.DataDirectory = DataFolder;
        Configuration.BaseDirectory = Se.DataFolder;
        NetflixQualityCheck.NetflixCheckShotChange.ShotChangeDirectory = Se.ShotChangesFolder;
    }

    private static string? _dictionariesFolder;
    public static string DictionariesFolder
    {
        get => _dictionariesFolder ?? Path.Combine(DataFolder, "Dictionaries");
        set => _dictionariesFolder = value;
    }
    public static string ThemesFolder => Path.Combine(DataFolder, "Themes");
    public static string AutoBackupFolder => Path.Combine(DataFolder, "AutoBackup");
    public static string FfmpegFolder => Path.Combine(DataFolder, "ffmpeg");
    public static string TextToSpeechFolder => Path.Combine(DataFolder, "TextToSpeech");
    public static string SpeechToTextFolder => Path.Combine(DataFolder, "SpeechToText");
    public static string CrispAsrFolder => Path.Combine(DataFolder, "CrispASR");
    public static string LlamaCppFolder => Path.Combine(DataFolder, "llama.cpp");
    public static string WaveformsFolder => Path.Combine(DataFolder, "Waveforms");
    public static string SpectrogramsFolder => Path.Combine(DataFolder, "Spectrograms");
    public static string ShotChangesFolder => Path.Combine(DataFolder, "ShotChanges");
    public static string PluginsFolder => Path.Combine(DataFolder, "Plugins");

    /// <summary>Root for persistent per-plugin data folders; not scanned for plugins (no manifest).</summary>
    public static string PluginsDataFolder => Path.Combine(PluginsFolder, "Data");

    public static string OcrFolder => Path.Combine(DataFolder, "OCR");
    public static string TranslationFolder => Path.Combine(DataFolder, "Languages");
    public static string PaddleOcrFolder => Path.Combine(OcrFolder, "PaddleOCR3-1");
    public static string PaddleOcrModelsFolder => Path.Combine(PaddleOcrFolder, "models");
    public static string GoogleLensOcrFolder => Path.Combine(OcrFolder, "Google-Lens");
    public static string VlcFolder => Path.Combine(DataFolder, "VLC");
    public static string SevenZipFolder => Path.Combine(DataFolder, "7Zip");
    private static readonly Lazy<string> _tesseractFolder = new(ResolveTesseractFolder);
    public static string TesseractFolder => _tesseractFolder.Value;

    private static string ResolveTesseractFolder()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(DataFolder, "Tesseract550");
        }

        var folders = new List<string>();
        if (Directory.Exists("/opt/homebrew/Cellar/tesseract"))
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
        folders.Add("/app/bin"); // bundled into the Flatpak sandbox (issue #11646)

        foreach (var folder in folders)
        {
            var path = Path.Combine(folder, "tesseract");
            if (System.IO.File.Exists(path))
            {
                return folder;
            }
        }

        return Path.Combine(DataFolder, "Tesseract550");
    }

    private static readonly Lazy<string> _tesseractModelFolder = new(ResolveTesseractModelFolder);
    public static string TesseractModelFolder => _tesseractModelFolder.Value;

    private static string ResolveTesseractModelFolder()
    {
        var modelFolder = Path.Combine(DataFolder, "Tesseract550", "tessdata");
        SeedBundledTesseractModels(modelFolder);
        return modelFolder;
    }

    // In the Flatpak sandbox the English model is bundled read-only at
    // /app/share/tessdata (issue #11646). Copy it into the writable model folder
    // on first use so OCR works out of the box; additional languages are still
    // downloaded into the same folder by DownloadTesseractModelViewModel.
    private static void SeedBundledTesseractModels(string modelFolder)
    {
        const string bundledEng = "/app/share/tessdata/eng.traineddata";
        if (!System.IO.File.Exists(bundledEng))
        {
            return;
        }

        try
        {
            var target = Path.Combine(modelFolder, "eng.traineddata");
            if (!System.IO.File.Exists(target))
            {
                Directory.CreateDirectory(modelFolder);
                System.IO.File.Copy(bundledEng, target);
            }
        }
        catch (Exception ex)
        {
            SeLogger.Error("Error seeding bundled Tesseract model: " + ex.Message);
        }
    }

    public void InitializeMainShortcuts(MainViewModel vm)
    {
        var defaults = ShortcutsMain.GetDefaultShortcuts(vm);

        if (Shortcuts.Count == 0)
        {
            Shortcuts = defaults;
            return;
        }

        var existing = new HashSet<string>(Shortcuts.Select(s => s.ActionName), StringComparer.Ordinal);
        foreach (var def in defaults)
        {
            if (!existing.Contains(def.ActionName))
            {
                Shortcuts.Add(def);
            }
        }
    }

    public static void SaveSettings()
    {
        var settingsFileName = GetSettingsFilePath();
        SaveSettings(settingsFileName);
    }

    public static void SaveSettings(string settingsFileName)
    {
        var directory = Path.GetDirectoryName(settingsFileName);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        try
        {
            // Atomic write: serialize directly to a temp file (UTF-8, no string round-trip)
            // and replace, so a process kill mid-write can't leave a truncated settings file.
            var tempFileName = settingsFileName + ".tmp";
            using (var stream = System.IO.File.Create(tempFileName))
            {
                JsonSerializer.Serialize(stream, Settings, SeJsonContext.Default.Se);
            }

            System.IO.File.Move(tempFileName, settingsFileName, overwrite: true);
        }
        catch (Exception exception)
        {
            // Log with context (e.g. no write access to the data folder) and rethrow so callers
            // that can show UI - like the settings dialog - can tell the user the save failed
            // instead of it disappearing silently (#12180).
            Se.LogError(exception, $"Failed to save settings to '{settingsFileName}'");
            throw;
        }

        UpdateLibSeSettings();
    }

    public static void LoadSettings()
    {
        var settingsFileName = GetSettingsFilePath();
        LoadSettings(settingsFileName);
    }

    public static string GetSettingsFilePath()
    {
        return SettingsFilePathOverride ?? Path.Combine(DataFolder, "Settings.json");
    }

    public static void LoadSettings(string settingsFileName)
    {
        if (!System.IO.File.Exists(settingsFileName))
        {
            MigrateMacOsFontSettings(Settings.Appearance, OperatingSystem.IsMacOS(), false);
            return;
        }

        try
        {
            // Stream + source-generated metadata: no UTF-16 string round-trip and no
            // runtime reflection over the settings type graph.
            using var stream = System.IO.File.OpenRead(settingsFileName);
            Settings = JsonSerializer.Deserialize(stream, SeJsonContext.Default.Se)!;
        }
        catch (Exception exception)
        {
            Se.LogError(exception);
            Settings = new Se();
        }

        SetDefaultValues();

        MigrateMacOsFontSettings(Settings.Appearance, OperatingSystem.IsMacOS(), true);

        UpdateLibSeSettings();
    }

    internal static void MigrateMacOsFontSettings(SeAppearance appearance, bool isMacOs, bool isLegacySettings)
    {
        if (!isMacOs || appearance.MacOsFontMigrationVersion.GetValueOrDefault() >= CurrentMacOsFontMigrationVersion)
        {
            return;
        }

        if (isLegacySettings && appearance.FontName is ".AppleSystemUIFont" or "Default")
        {
            appearance.FontName = "Helvetica Neue";
        }

        // Once marked, a later explicit System Font selection must remain untouched.
        appearance.MacOsFontMigrationVersion = CurrentMacOsFontMigrationVersion;
    }

    /// <summary>
    /// Loads the UI translation named in <see cref="Settings"/>.General.Language into the global
    /// <see cref="Language"/>. Must run before the main window is built: on macOS the native menu
    /// bar is constructed at startup and reads <see cref="Language"/> directly, so the translation
    /// has to be in place by then or the menu bar renders in English (issue #11505). English — or a
    /// missing/unreadable translation file — leaves the built-in English defaults untouched.
    /// </summary>
    public static void LoadLanguage()
    {
        Settings.General.Language ??= "English";
        if (Settings.General.Language == "English")
        {
            return;
        }

        try
        {
            var jsonFileName = Path.Combine(TranslationFolder, Settings.General.Language + ".json");
            if (!System.IO.File.Exists(jsonFileName))
            {
                return;
            }

            var json = System.IO.File.ReadAllText(jsonFileName, Encoding.UTF8);
            var language = JsonSerializer.Deserialize<SeLanguage>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            if (language != null)
            {
                Language = language;
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to load UI language");
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

        if (Settings.Edit == null)
        {
            Settings.Edit = new();
        }

        if (Settings.Options == null)
        {
            Settings.Options = new();
        }

        if (Settings.General == null)
        {
            Settings.General = new();
        }

        // Custom continuation style is a nested object; an old settings file (or hand edit)
        // may omit it, so guard against null before the libse bridge dereferences it.
        if (Settings.General.CustomContinuationStyle == null)
        {
            Settings.General.CustomContinuationStyle = new();
        }

        if (Settings.General.Profiles != null)
        {
            foreach (var profile in Settings.General.Profiles)
            {
                if (profile.CustomContinuationStyle == null)
                {
                    profile.CustomContinuationStyle = new();
                }
            }
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

        // Add toolbar items introduced after an older settings file was written (e.g. VideoSeek),
        // so the waveform toolbar's per-type lookup never misses and new items are customizable.
        Settings.Waveform.EnsureAllToolbarItems();

        if (Settings.BeautifyTimeCodes == null)
        {
            Settings.BeautifyTimeCodes = new();
        }

        if (Settings.Ocr == null)
        {
            Settings.Ocr = new();
        }

        if (Settings.Formats == null)
        {
            Settings.Formats = new SeFormats();
        }

        if (Settings.Plugins == null)
        {
            Settings.Plugins = new SePlugins();
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

        Configuration.Settings.Proxy.ProxyAddress = Settings.General.ProxyAddress ?? string.Empty;
        Configuration.Settings.Proxy.UserName = Settings.General.ProxyUserName ?? string.Empty;
        if (!string.IsNullOrEmpty(Settings.General.ProxyPassword))
        {
            Configuration.Settings.Proxy.EncodePassword(Settings.General.ProxyPassword);
        }
        else
        {
            Configuration.Settings.Proxy.Password = null;
        }

        var stt = Settings.Tools.AudioToText;
        Configuration.Settings.Tools.WhisperChoice = stt.WhisperChoice;
        Configuration.Settings.Tools.WhisperIgnoreVersion = stt.WhisperIgnoreVersion;
        Configuration.Settings.Tools.WhisperDeleteTempFiles = stt.WhisperDeleteTempFiles;
        Configuration.Settings.Tools.WhisperModel = stt.WhisperModel;
        Configuration.Settings.Tools.WhisperLanguageCode = stt.WhisperLanguageCode;
        Configuration.Settings.Tools.WhisperLocation = stt.WhisperLocation;
        Configuration.Settings.Tools.WhisperCtranslate2Location = stt.WhisperCtranslate2Location;
        Configuration.Settings.Tools.WhisperPurfviewFasterWhisperLocation = stt.WhisperPurfviewFasterWhisperLocation;
        Configuration.Settings.Tools.WhisperPurfviewFasterWhisperDefaultCmd = stt.WhisperPurfviewFasterWhisperDefaultCmd;
        Configuration.Settings.Tools.WhisperXLocation = stt.WhisperXLocation;
        Configuration.Settings.Tools.WhisperStableTsLocation = stt.WhisperStableTsLocation;
        Configuration.Settings.Tools.WhisperCppModelLocation = stt.WhisperCppModelLocation;
        Configuration.Settings.Tools.WhisperExtraSettings = stt.WhisperCustomCommandLineArguments;
        Configuration.Settings.Tools.WhisperExtraSettingsHistory = stt.WhisperExtraSettingsHistory;
        Configuration.Settings.Tools.WhisperAutoAdjustTimings = stt.WhisperAutoAdjustTimings;
        Configuration.Settings.Tools.WhisperUseLineMaxChars = stt.WhisperUseLineMaxChars;
        Configuration.Settings.Tools.WhisperPostProcessingAddPeriods = stt.WhisperPostProcessingAddPeriods;
        Configuration.Settings.Tools.WhisperPostProcessingMergeLines = stt.WhisperPostProcessingMergeLines;
        Configuration.Settings.Tools.WhisperPostProcessingSplitLines = stt.WhisperPostProcessingSplitLines;
        Configuration.Settings.Tools.WhisperPostProcessingFixCasing = stt.WhisperPostProcessingFixCasing;
        Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration = stt.WhisperPostProcessingFixShortDuration;
        Configuration.Settings.Tools.VoskPostProcessing = stt.PostProcessing;

        Configuration.Settings.Tools.OpenAiCompatibleSttUrl = Settings.Tools.OpenAiCompatibleSttUrl;
        Configuration.Settings.Tools.OpenAiCompatibleSttApiKey = Settings.Tools.OpenAiCompatibleSttApiKey;
        Configuration.Settings.Tools.OpenAiCompatibleSttModel = Settings.Tools.OpenAiCompatibleSttModel;
        Configuration.Settings.Tools.OpenAiCompatibleSttExtraHeaders = Settings.Tools.OpenAiCompatibleSttExtraHeaders;
        Configuration.Settings.Tools.OpenAiCompatibleSttTimeoutSeconds = Settings.Tools.OpenAiCompatibleSttTimeoutSeconds;
        Configuration.Settings.Tools.OpenAiCompatibleSttLanguage = Settings.Tools.OpenAiCompatibleSttLanguage;
        Configuration.Settings.Tools.OpenAiCompatibleSttTemperature = Settings.Tools.OpenAiCompatibleSttTemperature;
        Configuration.Settings.Tools.OpenAiCompatibleSttPrompt = Settings.Tools.OpenAiCompatibleSttPrompt;
        Configuration.Settings.Tools.OpenAiCompatibleSttAutoTranscribeOnAudioSelection = Settings.Tools.OpenAiCompatibleSttAutoTranscribeOnAudioSelection;
        Configuration.Settings.Tools.OpenAiCompatibleSttStream = Settings.Tools.OpenAiCompatibleSttStream;

        Configuration.Settings.Tools.OpenRouterSttApiKey = Settings.Tools.OpenRouterSttApiKey;
        Configuration.Settings.Tools.OpenRouterSttModel = Settings.Tools.OpenRouterSttModel;
        Configuration.Settings.Tools.OpenRouterSttLanguage = Settings.Tools.OpenRouterSttLanguage;
        Configuration.Settings.Tools.OpenRouterSttTemperature = Settings.Tools.OpenRouterSttTemperature;
        Configuration.Settings.Tools.OpenRouterSttPrompt = Settings.Tools.OpenRouterSttPrompt;
        Configuration.Settings.Tools.OpenRouterSttTimeoutSeconds = Settings.Tools.OpenRouterSttTimeoutSeconds;

        Configuration.Settings.Tools.DashScopeSttApiKey = Settings.Tools.DashScopeSttApiKey;
        Configuration.Settings.Tools.DashScopeSttModel = Settings.Tools.DashScopeSttModel;
        Configuration.Settings.Tools.DashScopeSttLanguage = Settings.Tools.DashScopeSttLanguage;
        Configuration.Settings.Tools.DashScopeSttRegion = Settings.Tools.DashScopeSttRegion;
        Configuration.Settings.Tools.DashScopeSttEnableWords = Settings.Tools.DashScopeSttEnableWords;
        Configuration.Settings.Tools.DashScopeSttTimeoutSeconds = Settings.Tools.DashScopeSttTimeoutSeconds;

        Configuration.Settings.Tools.AutoTranslateLastName = Settings.AutoTranslate.AutoTranslateLastName;

        // BeautifyTimeCodes profile: skip apply on a fresh install so libse's built-in
        // default-preset values stay intact. Once the user clicks OK in the profile editor,
        // Saved flips to true and the persisted profile takes over.
        if (Settings.BeautifyTimeCodes.Saved)
        {
            Settings.BeautifyTimeCodes.ApplyTo(Configuration.Settings.BeautifyTimeCodes);
        }

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

        Configuration.Settings.Tools.MusicSymbol = Settings.Tools.MusicSymbol;
        Configuration.Settings.Tools.MusicSymbolReplace = Settings.Tools.MusicSymbolReplace;

        var dc = Settings.File.DCinemaSmpte;
        var ss = Configuration.Settings.SubtitleSettings;
        ss.DCinemaAutoGenerateSubtitleId = dc.DCinemaAutoGenerateSubtitleId;
        ss.DCinemaFontSize = dc.DCinemaFontSize;
        ss.DCinemaBottomMargin = dc.DCinemaBottomMargin;
        ss.DCinemaFadeUpTime = dc.DCinemaFadeUpTime;
        ss.DCinemaFadeDownTime = dc.DCinemaFadeDownTime;
        ss.CurrentDCinemaSubtitleId = dc.CurrentDCinemaSubtitleId;
        ss.CurrentDCinemaMovieTitle = dc.CurrentDCinemaMovieTitle;
        ss.CurrentDCinemaReelNumber = dc.CurrentDCinemaReelNumber;
        ss.CurrentDCinemaIssueDate = dc.CurrentDCinemaIssueDate;
        ss.CurrentDCinemaLanguage = dc.CurrentDCinemaLanguage;
        ss.CurrentDCinemaEditRate = dc.CurrentDCinemaEditRate;
        ss.CurrentDCinemaTimeCodeRate = dc.CurrentDCinemaTimeCodeRate;
        ss.CurrentDCinemaStartTime = dc.CurrentDCinemaStartTime;
        ss.CurrentDCinemaFontId = dc.CurrentDCinemaFontId;
        ss.CurrentDCinemaFontUri = dc.CurrentDCinemaFontUri;
        ss.CurrentDCinemaFontEffect = dc.CurrentDCinemaFontEffect;
        ss.CurrentDCinemaFontSize = dc.CurrentDCinemaFontSize;
        if (!string.IsNullOrEmpty(dc.CurrentDCinemaFontColor))
        {
            try { ss.CurrentDCinemaFontColor = dc.CurrentDCinemaFontColor.FromHex(); } catch { }
        }
        if (!string.IsNullOrEmpty(dc.CurrentDCinemaFontEffectColor))
        {
            try { ss.CurrentDCinemaFontEffectColor = dc.CurrentDCinemaFontEffectColor.FromHex(); } catch { }
        }
    }

    /// <summary>
    /// Copies the active rule profile's continuation style (incl. the custom-style fields and
    /// pause) from <see cref="Settings"/> into the libse <see cref="Configuration.Settings"/> that
    /// the fix/merge engines read. Without this the "Custom" continuation style falls back to
    /// libse defaults.
    /// </summary>
    public static void ApplyContinuationStyleToLibSe()
    {
        var g = Settings.General;

        if (Enum.TryParse<Core.Enums.ContinuationStyle>(g.ContinuationStyle, out var cs))
        {
            Configuration.Settings.General.ContinuationStyle = cs;
        }

        (g.CustomContinuationStyle ?? new CustomContinuationStyle()).ApplyToGeneralSettings(Configuration.Settings.General);
    }

    public static string GetErrorLogFilePath()
    {
        return Path.Combine(DataFolder, "error-log.txt");
    }

    public static string GetToolsLogFilePath()
    {
        return Path.Combine(DataFolder, "tools-log.txt");
    }

    public static void WriteToolsLog(string log)
    {
        WriteToolsLog(log, false);
    }

    /// <summary>
    /// Writes an entry to the tools log. When <paramref name="force"/> is true the entry is written
    /// even if the "write tools log" setting is off — use this for hard-failure diagnostics (e.g. an
    /// engine produced output that could not be parsed) that must be available for a bug report.
    /// </summary>
    public static void WriteToolsLog(string log, bool force)
    {
        if (!force && !Settings.Tools.WriteToolsLog)
        {
            return;
        }

        try
        {
            var filePath = GetToolsLogFilePath();
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
