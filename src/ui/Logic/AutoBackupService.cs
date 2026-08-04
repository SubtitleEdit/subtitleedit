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
        try
        {
            var format = saveFormat.IsTextBased ? saveFormat : new SubRip();
            File.WriteAllText(fileName, format.ToText(subtitle, string.Empty));
        }
        catch
        {
            // ignore
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