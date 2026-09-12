using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic;

public interface IAutoBackupService
{
    void StartAutoBackup(MainViewModel mainViewModel);
    void StopAutobackup();
    public List<string> GetAutoBackupFiles();
    public void CleanAutoBackupFolder();

    /// <summary>Copies Settings.json to the settings backup folder if the newest backup is older than the configured interval.</summary>
    void BackupSettingsIfDue();

    /// <summary>Copies Settings.json to the settings backup folder regardless of the interval; returns the backup path, or null when nothing was written.</summary>
    string? BackupSettingsNow();

    List<string> GetSettingsBackupFiles();
    void CleanSettingsBackupFolder();
}

public partial class AutoBackupService : IAutoBackupService
{
    private MainViewModel? _mainViewModel;
    private DispatcherTimer? _timerAutoBackup;
    private int _backupInFlight;
    // Source-generated rather than RegexOptions.Compiled: this type is constructed from the
    // MainViewModel ctor, and Compiled emits IL at construction time on the start-up path
    // (~3.6 ms and 13 KB, vs ~1.7 ms and zero allocation here) for a pattern only used when
    // cleaning the backup folder. Matching stays just as fast - it compiles at build time.
    [GeneratedRegex(@"^\d\d\d\d-\d\d-\d\d_\d\d-\d\d-\d\d")]
    private static partial Regex RegexFileNamePattern();

    public void StartAutoBackup(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;

        if (!Se.Settings.General.AutoBackupOn)
        {
            return;
        }

        var minutes = Se.Settings.General.AutoBackupIntervalMinutes;
        if (minutes < 1)
        {
            minutes = 1;
        }

        // DispatcherTimer ticks on the UI thread, which HasChanges/GetUpdateSubtitle
        // require (they mutate the shared subtitle and enumerate the UI-bound
        // collection) - take the snapshot here, then write the file off-thread from
        // a copy. It also makes Stop() synchronous: no straggler tick can run after
        // StopAutobackup(), unlike Timers.Timer whose queued Elapsed could.
        _timerAutoBackup = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMinutes(minutes) };
        _timerAutoBackup.Tick += (_, _) =>
        {
            if (_mainViewModel is not { } vm || !vm.HasChanges())
            {
                return;
            }

            // A save on a very large subtitle can outlive the timer interval; skip this
            // tick rather than write concurrently (two saves in the same wall-clock
            // second would also collide on the same timestamped filename). The next
            // tick picks up newer state anyway.
            if (Interlocked.CompareExchange(ref _backupInFlight, 1, 0) != 0)
            {
                return;
            }

            var saveFormat = vm.SelectedSubtitleFormat;
            var subtitle = new Subtitle(vm.GetUpdateSubtitle(), false);
            Task.Run(() =>
            {
                try
                {
                    SaveAutoBackup(subtitle, saveFormat);
                }
                finally
                {
                    Interlocked.Exchange(ref _backupInFlight, 0);
                }
            });
        };
        _timerAutoBackup.Start();
    }

    public void StopAutobackup()
    {
        _timerAutoBackup?.Stop();
        _timerAutoBackup = null;
    }

    private static void SaveAutoBackup(Subtitle subtitle, SubtitleFormat saveFormat)
    {
        if (subtitle.Paragraphs.Count == 0)
        {
            return;
        }

        var folder = Se.AutoBackupFolder;
        if (!Directory.Exists(folder))
        {
            try
            {
                Directory.CreateDirectory(folder);
            }
            catch
            {
                return;
            }
        }

        var title = string.Empty;
        if (!string.IsNullOrEmpty(subtitle.FileName))
        {
            title = "_" + Path.GetFileNameWithoutExtension(subtitle.FileName);
        }

        var fileName = Path.Combine(folder,
            $"{DateTime.Now.Year:0000}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}_{DateTime.Now.Hour:00}-{DateTime.Now.Minute:00}-{DateTime.Now.Second:00}{title}{saveFormat.Extension}");
        // IsTextBased only rules out binary formats; the read-only text formats (WSB, FTE,
        // DlDd, SPU image...) are "text based" but their ToText throws, and the catch below
        // then swallowed it - leaving the user with auto-backup silently doing nothing.
        var format = saveFormat.IsTextBased ? saveFormat : new SubRip();
        string text;
        try
        {
            text = format.ToText(subtitle, string.Empty);
        }
        catch
        {
            try
            {
                format = new SubRip();
                fileName = Path.ChangeExtension(fileName, format.Extension);
                text = format.ToText(subtitle, string.Empty);
            }
            catch
            {
                return;
            }
        }

        try
        {
            File.WriteAllText(fileName, text);
        }
        catch
        {
            // ignore - a backup that cannot be written must not disturb editing
        }
    }

    public List<string> GetAutoBackupFiles()
    {
        var result = new List<string>();
        var folder = Se.AutoBackupFolder;
        if (Directory.Exists(folder))
        {
            var files = Directory.GetFiles(folder, "*.*");
            foreach (var fileName in files)
            {
                var path = Path.GetFileName(fileName);
                if (RegexFileNamePattern().IsMatch(path))
                {
                    result.Add(fileName);
                }
            }
        }

        return result;
    }

    private const string SettingsBackupSuffix = "_Settings.json";
    private const string SettingsBackupTimeFormat = "yyyy-MM-dd_HH-mm-ss";
    private static readonly Lock SettingsBackupLocker = new Lock();

    public void BackupSettingsIfDue()
    {
        if (!Se.Settings.General.SettingsBackupOn)
        {
            return;
        }

        var intervalDays = Math.Max(1, Se.Settings.General.SettingsBackupIntervalDays);
        lock (SettingsBackupLocker)
        {
            var newest = GetNewestSettingsBackupTime(Se.SettingsBackupFolder);
            if (newest.HasValue && newest.Value > DateTime.Now.AddDays(-intervalDays))
            {
                return;
            }

            CopySettingsToBackupFolder();
        }

        CleanSettingsBackupFolder();
    }

    public string? BackupSettingsNow()
    {
        string? result;
        lock (SettingsBackupLocker)
        {
            result = CopySettingsToBackupFolder();
        }

        CleanSettingsBackupFolder();
        return result;
    }

    private static string? CopySettingsToBackupFolder()
    {
        var settingsFileName = Se.GetSettingsFilePath();
        if (!File.Exists(settingsFileName))
        {
            return null;
        }

        var folder = Se.SettingsBackupFolder;
        try
        {
            Directory.CreateDirectory(folder);
            var target = Path.Combine(folder, DateTime.Now.ToString(SettingsBackupTimeFormat, CultureInfo.InvariantCulture) + SettingsBackupSuffix);
            File.Copy(settingsFileName, target, overwrite: true);
            return target;
        }
        catch (Exception exception)
        {
            // A backup that cannot be written must not disturb start-up.
            Se.LogError(exception, "Could not back up settings");
            return null;
        }
    }

    /// <summary>
    /// Time stamp of the newest settings backup, read from the file name rather than the file
    /// system so a copied or touched backup folder does not postpone the next backup.
    /// </summary>
    internal static DateTime? GetNewestSettingsBackupTime(string folder)
    {
        DateTime? newest = null;
        foreach (var fileName in GetSettingsBackupFiles(folder))
        {
            var time = ParseSettingsBackupTime(fileName);
            if (time.HasValue && (!newest.HasValue || time.Value > newest.Value))
            {
                newest = time;
            }
        }

        return newest;
    }

    internal static DateTime? ParseSettingsBackupTime(string fileName)
    {
        var name = Path.GetFileName(fileName);
        if (name.Length < SettingsBackupTimeFormat.Length ||
            !DateTime.TryParseExact(name[..SettingsBackupTimeFormat.Length], SettingsBackupTimeFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
        {
            return null;
        }

        return time;
    }

    public List<string> GetSettingsBackupFiles() => GetSettingsBackupFiles(Se.SettingsBackupFolder);

    internal static List<string> GetSettingsBackupFiles(string folder)
    {
        var result = new List<string>();
        if (!Directory.Exists(folder))
        {
            return result;
        }

        foreach (var fileName in Directory.GetFiles(folder, "*" + SettingsBackupSuffix))
        {
            if (RegexFileNamePattern().IsMatch(Path.GetFileName(fileName)))
            {
                result.Add(fileName);
            }
        }

        return result;
    }

    /// <summary>Keeps only the newest <see cref="SeGeneral.SettingsBackupMaxCount"/> settings backups.</summary>
    public void CleanSettingsBackupFolder()
    {
        var keep = Math.Max(1, Se.Settings.General.SettingsBackupMaxCount);
        lock (SettingsBackupLocker)
        {
            foreach (var fileName in GetSettingsBackupFilesToDelete(Se.SettingsBackupFolder, keep))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    internal static List<string> GetSettingsBackupFilesToDelete(string folder, int keep)
    {
        var files = GetSettingsBackupFiles(folder);
        files.Sort((a, b) => string.CompareOrdinal(Path.GetFileName(b), Path.GetFileName(a))); // newest first
        return files.Count <= keep ? new List<string>() : files.GetRange(keep, files.Count - keep);
    }

    private static readonly Lock Locker = new Lock();

    public void CleanAutoBackupFolder()
    {
        var autoBackupFolder = Se.AutoBackupFolder;
        var autoBackupDeleteAfterDays = Se.Settings.General.AutoBackupDeleteAfterDays;

        lock (Locker) // only allow one thread
        {
            if (Directory.Exists(autoBackupFolder))
            {
                var targetDate = DateTime.Now.AddDays(-autoBackupDeleteAfterDays);
                var files = Directory.GetFiles(autoBackupFolder, "*.*");
                foreach (var fileName in files)
                {
                    try
                    {
                        var name = Path.GetFileName(fileName);
                        if (RegexFileNamePattern().IsMatch(name) && 
                            Convert.ToDateTime(name.Substring(0, 10), CultureInfo.InvariantCulture) <= targetDate)
                        {
                            File.Delete(fileName);
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }
    }
}