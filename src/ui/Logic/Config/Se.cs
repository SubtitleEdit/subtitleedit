using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config.Language;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Config;

public class Se
{
    internal const int CurrentMacOsFontMigrationVersion = 1;
    internal const int CurrentShortcutsMigrationVersion = 2;

    public static string Version { get; set; } = "v5.2.0-beta8";

    public SeGeneral General { get; set; } = new();
    public List<SeShortCut> Shortcuts { get; set; } = new();
    public int? ShortcutsMigrationVersion { get; set; }
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

    /// <summary>Name of the translation currently held in <see cref="Language"/>, so repeat
    /// <see cref="LoadLanguage()"/> calls for the same language skip the expensive deserialize.</summary>
    private static string? _loadedLanguage;

    static Se()
    {
        ExePath = AppContext.BaseDirectory;
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        // Portable mode - keeping data next to the executable - is a Windows notion: a zip
        // install that lives outside Program Files. Off Windows both folder paths come back
        // empty, and StartsWith(string.Empty) is always true, so every install already counted
        // as "in Program Files" and used the per-user data folder. That is the behaviour we
        // want off Windows, but it was reached by accident; state it outright, because flipping
        // it would move the data folder out from under existing macOS and Linux installs.
        IsInstalledInProgramFiles =
            !OperatingSystem.IsWindows() ||
            IsUnder(ExePath, programFiles) ||
            IsUnder(ExePath, programFilesX86);

        IsPortable = !IsInstalledInProgramFiles;

        static bool IsUnder(string path, string root) =>
            !string.IsNullOrEmpty(root) && path.StartsWith(root, StringComparison.OrdinalIgnoreCase);

        DataFolder = ResolveDataFolder(IsPortable, ExePath, GetApplicationDataFolder());

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

    /// <summary>
    /// The per-user application-data folder - <c>%AppData%</c> on Windows, <c>$XDG_CONFIG_HOME</c>
    /// or <c>~/.config</c> on Linux, <c>~/Library/Application Support</c> on macOS.
    /// <para>
    /// The folder option matters off Windows. With the default (None) the runtime hands back an
    /// EMPTY string whenever the folder does not exist yet - a fresh account, a container, a
    /// sandboxed HOME - which quietly turned DataFolder into the relative "Subtitle Edit" and
    /// scattered settings, dictionaries and themes into whatever the working directory happened
    /// to be. DoNotVerify asks for the path only: it never touches the file system, so it cannot
    /// come back empty for a folder that is merely missing, and it cannot throw. Creating the
    /// folder is left to the guarded <see cref="Directory.CreateDirectory(string)"/> in the
    /// static constructor, which makes the whole chain and logs on failure. The Create option
    /// would create it here instead, but it throws when the folder is missing AND uncreatable
    /// (a read-only or non-existent HOME) - and a throw in the static constructor takes the app
    /// down at startup, before there is a window to report it in.
    /// </para>
    /// </summary>
    internal static string GetApplicationDataFolder()
        => Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);

    /// <summary>
    /// Picks the data folder from the portable flag and the per-user application-data folder.
    /// Falls back to the executable folder when there is no application-data folder at all -
    /// <see cref="Environment.GetFolderPath(Environment.SpecialFolder)"/> still comes back empty
    /// off Windows when the home directory cannot be determined (no HOME and no passwd entry, as
    /// in a container running under an unknown uid) - so the result is always absolute and never
    /// resolves against the working directory. Split out of the static constructor because that
    /// reads the real environment and runs only once per process.
    /// </summary>
    internal static string ResolveDataFolder(bool isPortable, string exePath, string appDataFolder)
        => isPortable || string.IsNullOrEmpty(appDataFolder)
            ? exePath
            : Path.Combine(appDataFolder, "Subtitle Edit");

    private static string? _dictionariesFolder;
    public static string DictionariesFolder
    {
        get => _dictionariesFolder ?? Path.Combine(DataFolder, "Dictionaries");
        set => _dictionariesFolder = value;
    }
    public static string ThemesFolder => Path.Combine(DataFolder, "Themes");
    public static string FontsFolder => Path.Combine(DataFolder, "Fonts");
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
    public static string CrispEmbedFolder => Path.Combine(OcrFolder, "CrispEmbed");
    public static string VlcFolder => Path.Combine(DataFolder, "VLC");
    public static string SevenZipFolder => Path.Combine(DataFolder, "7Zip");
    private static readonly Lazy<string> _tesseractFolder = new(ResolveTesseractFolder);
    public static string TesseractFolder => _tesseractFolder.Value;

    private static string ResolveTesseractFolder()
    {
        if (OperatingSystem.IsWindows())
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
        MigrateShortcuts();

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

    /// <summary>
    /// One-time shortcut migrations, versioned like <see cref="MigrateMacOsFontSettings"/> so a
    /// binding the user re-assigns afterwards is never touched again.
    ///
    /// Version 1: v5.0.0 - v5.2.0-beta1 shipped F10 as the default for "set end and go to next",
    /// and any visit to the Shortcuts window persisted that default to Settings.json. Once the
    /// F10-suppression check from #12504 landed, the persisted default permanently disabled the
    /// standard F10 menu-bar activation (#13083). The default is gone now, and the stale persisted
    /// copy - indistinguishable from a user assignment - is cleared here once; users who really
    /// want F10 on the action can assign it again and it will stick.
    ///
    /// Version 2: "Text box: Delete selection (no clipboard)" grew into the forward-delete
    /// (Delete key) command and was renamed; the persisted entry is renamed with it so user
    /// assignments - including a deliberately cleared binding - survive.
    /// </summary>
    internal void MigrateShortcuts()
    {
        var fromVersion = ShortcutsMigrationVersion.GetValueOrDefault();
        if (fromVersion >= CurrentShortcutsMigrationVersion)
        {
            return;
        }

        ShortcutsMigrationVersion = CurrentShortcutsMigrationVersion;

        if (fromVersion < 1)
        {
            foreach (var shortcut in Shortcuts)
            {
                if (shortcut.ActionName == nameof(MainViewModel.WaveformSetEndAndGoToNextCommand) &&
                    shortcut.Keys.Count == 1 &&
                    shortcut.Keys[0].Equals(nameof(Avalonia.Input.Key.F10), StringComparison.OrdinalIgnoreCase))
                {
                    shortcut.Keys.Clear();
                }
            }
        }

        if (fromVersion < 2)
        {
            foreach (var shortcut in Shortcuts)
            {
                if (shortcut.ActionName == "TextBoxDeleteSelectionCommand")
                {
                    shortcut.ActionName = nameof(MainViewModel.TextBoxDeleteForwardCommand);
                }
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

        // MainView.Build() calls this again as a safety net for windows created via other entry
        // points, and File > New window repeats it per window. Deserializing the ~185 KB / ~3300
        // property translation graph costs well over 100 ms, so only do it when the loaded
        // translation isn't already the requested one.
        if (_loadedLanguage == Settings.General.Language)
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

            using var stream = System.IO.File.OpenRead(jsonFileName);
            var language = JsonSerializer.Deserialize(stream, SeLanguageJsonContext.Default.SeLanguage);
            if (language != null)
            {
                Language = language;
                _loadedLanguage = Settings.General.Language;
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to load UI language");
        }
    }

    /// <summary>
    /// Loads a translation file chosen at runtime (Options > Language) into <see cref="Language"/>.
    /// Goes through here rather than assigning <see cref="Language"/> directly so the
    /// already-loaded marker stays in step and a later <see cref="LoadLanguage()"/> — e.g. from a
    /// new editor window — doesn't skip a language it hasn't actually loaded.
    /// </summary>
    public static async Task LoadLanguageFromFileAsync(string jsonFileName)
    {
        try
        {
            await using var stream = System.IO.File.OpenRead(jsonFileName);
            var language = await JsonSerializer.DeserializeAsync(stream, SeLanguageJsonContext.Default.SeLanguage);
            Language = language ?? new SeLanguage();
            _loadedLanguage = language == null ? null : Path.GetFileNameWithoutExtension(jsonFileName);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to load UI language from " + jsonFileName);
            Language = new SeLanguage();
            _loadedLanguage = null;
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

    internal static void UpdateLibSeSettings()
    {
        Configuration.Settings.General.FFmpegLocation = Settings.General.FfmpegPath;
        Configuration.Settings.General.UseTimeFormatHHMMSSFF = Settings.General.UseFrameMode;

        Configuration.Settings.Proxy.ProxyAddress = Settings.General.ProxyAddress ?? string.Empty;
        Configuration.Settings.Proxy.UserName = Settings.General.ProxyUserName ?? string.Empty;
        Configuration.Settings.Proxy.Domain = Settings.General.ProxyDomain ?? string.Empty;
        Configuration.Settings.Proxy.UseDefaultCredentials = Settings.General.ProxyUseDefaultCredentials;
        Configuration.Settings.Proxy.BypassList = Settings.General.ProxyBypassList ?? string.Empty;
        if (!string.IsNullOrEmpty(Settings.General.ProxyPassword))
        {
            Configuration.Settings.Proxy.EncodePassword(Settings.General.ProxyPassword);
        }
        else
        {
            Configuration.Settings.Proxy.Password = null;
        }

        Configuration.Settings.Tools.AutoBreakLineEndingEarly = Settings.Tools.AutoBreakLineEndingEarly;
        Configuration.Settings.Tools.AutoBreakCommaBreakEarly = Settings.Tools.AutoBreakCommaBreakEarly;
        Configuration.Settings.Tools.AutoBreakDashEarly = Settings.Tools.AutoBreakDashEarly;
        Configuration.Settings.Tools.AutoBreakUsePixelWidth = Settings.Tools.AutoBreakUsePixelWidth;
        Configuration.Settings.Tools.AutoBreakPreferBottomHeavy = Settings.Tools.AutoBreakPreferBottomHeavy;
        Configuration.Settings.Tools.AutoBreakPreferBottomPercent = Settings.Tools.AutoBreakPreferBottomPercent;
        Configuration.Settings.Tools.UseNoLineBreakAfter = Settings.Tools.UseNoLineBreakAfter;

        var stt = Settings.Tools.AudioToText;
        Configuration.Settings.Tools.WhisperChoice = stt.WhisperChoice;
        Configuration.Settings.Tools.WhisperLocation = stt.WhisperLocation;
        Configuration.Settings.Tools.WhisperCtranslate2Location = stt.WhisperCtranslate2Location;
        Configuration.Settings.Tools.WhisperXLocation = stt.WhisperXLocation;
        Configuration.Settings.Tools.WhisperStableTsLocation = stt.WhisperStableTsLocation;
        Configuration.Settings.Tools.WhisperCppModelLocation = stt.WhisperCppModelLocation;




        Configuration.Settings.Tools.AutoTranslateDelaySeconds = (int)Math.Round(Settings.AutoTranslate.RequestDelaySeconds, MidpointRounding.AwayFromZero);

        // BeautifyTimeCodes profile: skip apply on a fresh install so libse's built-in
        // default-preset values stay intact. Once the user clicks OK in the profile editor,
        // Saved flips to true and the persisted profile takes over.
        if (Settings.BeautifyTimeCodes.Saved)
        {
            Settings.BeautifyTimeCodes.ApplyTo(Configuration.Settings.BeautifyTimeCodes);
        }


        Configuration.Settings.Tools.MusicSymbol = Settings.Tools.MusicSymbol;
        Configuration.Settings.Tools.MusicSymbolReplace = Settings.Tools.MusicSymbolReplace;

        var dc = Settings.File.DCinemaSmpte;
        var ss = Configuration.Settings.SubtitleSettings;
        ss.WebVttUseXTimestampMap = Settings.Formats.WebVttUseXTimestampMap;
        ss.WebVttUseMultipleXTimestampMap = Settings.Formats.WebVttUseMultipleXTimestampMap;
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

    /// <summary>
    /// Copies every rule in <paramref name="profile"/> into the general settings; callers still
    /// need to run the libse bridge afterwards. Kept in one place because the profile picker used
    /// to apply the fields inline and quietly dropped the two duration limits.
    /// </summary>
    public static void ApplyRuleProfile(RulesProfile profile)
    {
        var g = Settings.General;

        g.CurrentProfile = profile.Name;
        g.SubtitleLineMaximumLength = profile.SubtitleLineMaximumLength;
        g.SubtitleMaximumCharactersPerSeconds = (double)profile.SubtitleMaximumCharactersPerSeconds;
        g.SubtitleOptimalCharactersPerSeconds = (double)profile.SubtitleOptimalCharactersPerSeconds;
        g.SubtitleMaximumWordsPerMinute = (double)profile.SubtitleMaximumWordsPerMinute;
        g.SubtitleMinimumDisplayMilliseconds = profile.SubtitleMinimumDisplayMilliseconds;
        g.SubtitleMaximumDisplayMilliseconds = profile.SubtitleMaximumDisplayMilliseconds;
        g.MinimumBetweenLines.Milliseconds = profile.MinimumMillisecondsBetweenLines;
        g.MinimumBetweenLines.Frames = SubtitleFormat.MillisecondsToFrames(profile.MinimumMillisecondsBetweenLines);
        g.MaxNumberOfLines = profile.MaxNumberOfLines;
        g.UnbreakLinesShorterThan = profile.MergeLinesShorterThan;
        g.DialogStyle = profile.DialogStyle.ToString();
        g.ContinuationStyle = profile.ContinuationStyle.ToString();
        g.CpsLineLengthStrategy = profile.CpsLineLengthStrategy;
        g.CustomContinuationStyle = new CustomContinuationStyle(profile.CustomContinuationStyle);
    }

    /// <summary>
    /// Pushes the rule settings into libse's Configuration, which is what the fix/merge engines
    /// and the duration helpers read. Sits next to <see cref="ApplyRuleProfile"/> so the two
    /// field lists stay in step - the duration limits were missing here for the same reason.
    /// </summary>
    public static void ApplyRuleSettingsToLibSe()
    {
        var g = Settings.General;
        var libSe = Configuration.Settings.General;

        libSe.SubtitleLineMaximumLength = g.SubtitleLineMaximumLength;
        libSe.SubtitleMaximumCharactersPerSeconds = g.SubtitleMaximumCharactersPerSeconds;
        libSe.SubtitleOptimalCharactersPerSeconds = g.SubtitleOptimalCharactersPerSeconds;
        libSe.SubtitleMaximumWordsPerMinute = g.SubtitleMaximumWordsPerMinute;
        libSe.SubtitleMinimumDisplayMilliseconds = g.SubtitleMinimumDisplayMilliseconds;
        libSe.SubtitleMaximumDisplayMilliseconds = g.SubtitleMaximumDisplayMilliseconds;
        libSe.MinimumMillisecondsBetweenLines = g.MinimumBetweenLines.GetMilliseconds();
        libSe.MaxNumberOfLines = g.MaxNumberOfLines;
        libSe.MergeLinesShorterThan = g.UnbreakLinesShorterThan;
        libSe.CpsLineLengthStrategy = g.CpsLineLengthStrategy;

        if (Enum.TryParse<Core.Enums.DialogType>(g.DialogStyle, out var dt))
        {
            libSe.DialogStyle = dt;
        }

        ApplyContinuationStyleToLibSe();
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
