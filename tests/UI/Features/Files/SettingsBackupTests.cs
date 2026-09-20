using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Files;

/// <summary>
/// Settings.json gets a daily snapshot next to the subtitle auto-backups. The rotation and
/// "is a backup due" decisions read the time stamp from the file name, so a copied folder
/// (fresh write times) neither postpones the next backup nor changes which files are pruned.
/// </summary>
public class SettingsBackupTests
{
    [Fact]
    public void ParseSettingsBackupTime_ReadsTheFileNamePrefix()
    {
        // Path.GetFileName only splits on the platform separator, so the test must not
        // hard-code a Windows path (it made the test fail on the Linux CI runner).
        var path = Path.Combine(Path.GetTempPath(), "x", "2026-09-12_08-30-05_Settings.json");
        var time = AutoBackupService.ParseSettingsBackupTime(path);

        Assert.Equal(new DateTime(2026, 9, 12, 8, 30, 5), time);
        Assert.Null(AutoBackupService.ParseSettingsBackupTime("Settings.json"));
        Assert.Null(AutoBackupService.ParseSettingsBackupTime("2026-13-40_99-00-00_Settings.json"));
    }

    [Fact]
    public void GetSettingsBackupFilesToDelete_KeepsTheNewestByName()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-settings-backup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var names = new[]
            {
                "2026-09-10_10-00-00_Settings.json",
                "2026-09-12_10-00-00_Settings.json",
                "2026-09-11_10-00-00_Settings.json",
                "2026-09-09_10-00-00_Settings.json",
            };
            foreach (var name in names)
            {
                File.WriteAllText(Path.Combine(folder, name), "{}");
            }

            // Unrelated files in the folder are never touched.
            File.WriteAllText(Path.Combine(folder, "notes.txt"), "keep");
            File.WriteAllText(Path.Combine(folder, "2026-09-12_10-00-00_Other.json"), "keep");

            var toDelete = AutoBackupService.GetSettingsBackupFilesToDelete(folder, 2).Select(Path.GetFileName).ToList();

            Assert.Equal(new[] { "2026-09-10_10-00-00_Settings.json", "2026-09-09_10-00-00_Settings.json" }, toDelete);
            Assert.Empty(AutoBackupService.GetSettingsBackupFilesToDelete(folder, 4));

            var newest = AutoBackupService.GetNewestSettingsBackupTime(folder);
            Assert.Equal(new DateTime(2026, 9, 12, 10, 0, 0), newest);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void IsSameSettingsIgnoringRecentFiles_OnlyRecentFilesMayDiffer()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-settings-backup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string Write(string name, string json)
            {
                var path = Path.Combine(folder, name);
                File.WriteAllText(path, json);
                return path;
            }

            var backup = Write("backup.json", """{"File":{"RecentFilesMaximum":25,"RecentFiles":[{"SubtitleFileName":"a.srt"}]},"Video":{"RecentFiles":["a.mkv"]},"General":{"FontSize":12}}""");
            var recentOnly = Write("recent.json", """{ "File": { "RecentFilesMaximum": 25, "RecentFiles": [] }, "Video": { "RecentFiles": ["b.mkv", "a.mkv"] }, "General": { "FontSize": 12 } }""");
            var changed = Write("changed.json", """{"File":{"RecentFilesMaximum":25,"RecentFiles":[{"SubtitleFileName":"a.srt"}]},"Video":{"RecentFiles":["a.mkv"]},"General":{"FontSize":13}}""");
            var broken = Write("broken.json", """{"File":{"RecentFilesMaximum":25""");

            Assert.True(AutoBackupService.IsSameSettingsIgnoringRecentFiles(recentOnly, backup));
            Assert.False(AutoBackupService.IsSameSettingsIgnoringRecentFiles(changed, backup));

            // When in doubt, take the backup.
            Assert.False(AutoBackupService.IsSameSettingsIgnoringRecentFiles(broken, backup));
            Assert.False(AutoBackupService.IsSameSettingsIgnoringRecentFiles(Path.Combine(folder, "missing.json"), backup));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void GetNewestSettingsBackupTime_IsNullForMissingFolder()
    {
        Assert.Null(AutoBackupService.GetNewestSettingsBackupTime(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))));
    }

    [Fact]
    public void TryLoadSettings_RejectsAnInvalidBackupAndKeepsTheLiveSettings()
    {
        // Restoring goes through Se.Settings, and LoadSettings swaps in defaults when the file
        // does not parse - so a truncated backup used to wipe every setting and report success.
        var folder = Path.Combine(Path.GetTempPath(), "se-settings-restore-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var savedSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.FavoriteLanguages = "marker-before-restore";

            var truncated = Path.Combine(folder, "2026-09-12_09-00-00_Settings.json");
            File.WriteAllText(truncated, "{ \"General\": { \"FavoriteLanguages\": \"from-backup\", \"Lay");

            Assert.False(Se.TryLoadSettings(truncated));
            Assert.Equal("marker-before-restore", Se.Settings.General.FavoriteLanguages);

            Assert.False(Se.TryLoadSettings(Path.Combine(folder, "missing_Settings.json")));
            Assert.Equal("marker-before-restore", Se.Settings.General.FavoriteLanguages);
        }
        finally
        {
            Se.Settings = savedSettings;
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void TryLoadSettings_ReplacesTheLiveSettingsFromAValidBackup()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-settings-restore-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var savedSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.FavoriteLanguages = "from-backup";
            var backup = Path.Combine(folder, "2026-09-12_09-00-00_Settings.json");
            Se.SaveSettings(backup);

            Se.Settings = new Se();
            Se.Settings.General.FavoriteLanguages = "current";

            Assert.True(Se.TryLoadSettings(backup));
            Assert.Equal("from-backup", Se.Settings.General.FavoriteLanguages);
        }
        finally
        {
            Se.Settings = savedSettings;
            Directory.Delete(folder, recursive: true);
        }
    }

    private sealed class StubAutoBackupService : IAutoBackupService
    {
        public List<string> SubtitleFiles { get; } = new();
        public List<string> SettingsFiles { get; } = new();
        public void StartAutoBackup(MainViewModel mainViewModel) { }
        public void StopAutobackup() { }
        public List<string> GetAutoBackupFiles() => SubtitleFiles;
        public void CleanAutoBackupFolder() { }
        public void BackupSettingsIfDue() { }
        public string? BackupSettingsNow() => null;
        public List<string> GetSettingsBackupFiles() => SettingsFiles;
        public void CleanSettingsBackupFolder() { }
    }

    [AvaloniaFact]
    public void RestoreWindow_ListsSettingsBackupsInTheirOwnTab()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-settings-backup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var older = Path.Combine(folder, "2026-09-11_09-00-00_Settings.json");
            var newer = Path.Combine(folder, "2026-09-12_09-00-00_Settings.json");
            File.WriteAllText(older, "{}");
            File.WriteAllText(newer, "{ \"a\": 1 }");

            var service = new StubAutoBackupService();
            service.SettingsFiles.AddRange(new[] { older, newer });

            var vm = new RestoreAutoBackupViewModel(service, new FolderHelper());
            var window = new RestoreAutoBackupWindow(vm);

            Assert.Empty(vm.Files);
            Assert.Equal(2, vm.SettingsFiles.Count);
            Assert.Equal("2026-09-12 09:00:00", vm.SettingsFiles[0].DateAndTime); // newest first
            Assert.Equal("Settings", vm.SettingsFiles[0].FileName);
            Assert.Same(vm.SettingsFiles[0], vm.SelectedSettingsFile);
            Assert.True(vm.IsRestoreSettingsEnabled);
            Assert.True(vm.IsSettingsFilesVisible);
            Assert.Contains("2026-09-12 09:00:00", vm.LastSettingsBackupText);

            var tabControl = window.GetVisualDescendants().OfType<TabControl>().FirstOrDefault()
                             ?? (window.Content as Grid)?.Children.OfType<TabControl>().First();
            Assert.NotNull(tabControl);
            Assert.Equal(2, tabControl!.Items.Count);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
