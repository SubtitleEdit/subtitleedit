using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Features.Shared;

namespace Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;

public partial class RestoreAutoBackupViewModel : ObservableObject
{
    [ObservableProperty] private bool _isOkButtonEnabled;
    [ObservableProperty] private DisplayFile? _selectedFile;
    [ObservableProperty] private ObservableCollection<DisplayFile> _files;
    [ObservableProperty] private bool _isEmptyFilesVisible;

    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private DisplayFile? _selectedSettingsFile;
    [ObservableProperty] private ObservableCollection<DisplayFile> _settingsFiles;
    [ObservableProperty] private bool _isRestoreSettingsEnabled;
    [ObservableProperty] private bool _isSettingsFilesVisible;
    [ObservableProperty] private string _settingsBackupInfo = string.Empty;
    [ObservableProperty] private string _lastSettingsBackupText = string.Empty;

    public Window? Window { get; set; }
    public string? RestoreFileName { get; set; }

    public bool OkPressed { get; private set; }

    /// <summary>Set when a settings backup was restored into <see cref="Se.Settings"/> while the window was open.</summary>
    public bool SettingsRestored { get; private set; }

    private readonly IAutoBackupService _autoBackupService;
    private readonly IFolderHelper _folderHelper;

    public RestoreAutoBackupViewModel(IAutoBackupService autoBackupService, IFolderHelper folderHelper)
    {
        _autoBackupService = autoBackupService;
        _folderHelper = folderHelper;
        Files = new ObservableCollection<DisplayFile>();
        SettingsFiles = new ObservableCollection<DisplayFile>();
        Initialize();
    }

    private void Initialize()
    {
        var files = new List<DisplayFile>();
        foreach (var fileName in _autoBackupService.GetAutoBackupFiles())
        {
            var displayFile = MakeDisplayFile(fileName);
            if (displayFile != null)
            {
                files.Add(displayFile);
            }
        }

        foreach (var file in files.OrderByDescending(f => f.DateAndTime))
        {
            Files.Add(file);
        }

        if (Files.Count > 0)
        {
            SelectedFile = Files[0];
            IsEmptyFilesVisible = true;
        }

        LoadSettingsBackups();
    }

    private static DisplayFile? MakeDisplayFile(string fileName)
    {
        var path = Path.GetFileName(fileName);
        if (string.IsNullOrEmpty(path) || path.Length < 19)
        {
            return null;
        }

        var fileInfo = new FileInfo(fileName);
        var displayDate = path[..19].Replace('_', ' ');
        displayDate = displayDate.Remove(13, 1).Insert(13, ":");
        displayDate = displayDate.Remove(16, 1).Insert(16, ":");

        return new DisplayFile(fileName, displayDate, Utilities.FormatBytesToDisplayFileSize(fileInfo.Length));
    }

    private void LoadSettingsBackups()
    {
        var files = new List<DisplayFile>();
        foreach (var fileName in _autoBackupService.GetSettingsBackupFiles())
        {
            var displayFile = MakeDisplayFile(fileName);
            if (displayFile != null)
            {
                files.Add(displayFile);
            }
        }

        SettingsFiles.Clear();
        foreach (var file in files.OrderByDescending(f => f.DateAndTime))
        {
            SettingsFiles.Add(file);
        }

        IsSettingsFilesVisible = SettingsFiles.Count > 0;
        SelectedSettingsFile = SettingsFiles.FirstOrDefault();
        IsRestoreSettingsEnabled = SelectedSettingsFile != null;

        var l = Se.Language.File.RestoreAutoBackup;
        var general = Se.Settings.General;
        SettingsBackupInfo = general.SettingsBackupOn
            ? string.Format(l.SettingsBackupInfo, Math.Max(1, general.SettingsBackupIntervalDays), Math.Max(1, general.SettingsBackupMaxCount))
            : l.SettingsBackupOff;
        LastSettingsBackupText = SelectedSettingsFile != null
            ? string.Format(l.LastSettingsBackupX, SettingsFiles[0].DateAndTime)
            : l.NoSettingsBackupsYet;
    }

    [RelayCommand]
    private async Task DeleteAllFiles()
    {
        if (Window == null)
        {
            return;
        }

        var answer = MessageBoxResult.Yes;
        if (Se.Settings.General.PromptBeforeDelete)
        {
            answer = await MessageBox.Show(
                Window,
                Se.Language.General.Delete,
                 Se.Language.File.RestoreAutoBackup.DeleteAllSubtitleBackups,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
        }
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        // A single locked/read-only file must not abort the sweep and leave the list
        // claiming everything was deleted - keep what could not be removed.
        var remaining = DeleteFiles(Files);

        Files.Clear();
        foreach (var file in remaining)
        {
            Files.Add(file);
        }

        IsEmptyFilesVisible = Files.Count > 0;
        SelectedFile = Files.FirstOrDefault();
        IsOkButtonEnabled = SelectedFile != null;
    }

    private static List<DisplayFile> DeleteFiles(IEnumerable<DisplayFile> files)
    {
        var remaining = new List<DisplayFile>();
        foreach (var file in files)
        {
            try
            {
                File.Delete(file.FullPath);
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Could not delete auto-backup file " + file.FullPath);
                remaining.Add(file);
            }
        }

        return remaining;
    }

    [RelayCommand]
    private async Task RestoreFile()
    {
        if (SelectedFile is not { } file || Window == null || !File.Exists(file.FullPath))
        {
            return;
        }
        var answer = await MessageBox.Show(
            Window,
            Se.Language.File.RestoreAutoBackup.RestoreAutoBackupFile,
            string.Format(Se.Language.File.RestoreAutoBackup.RestoreXFromY, file.FileName, file.DateAndTime),
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        OkPressed = true;
        RestoreFileName = file.FullPath;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task OpenFolder()
    {
        await OpenFolder(Se.AutoBackupFolder);
    }

    [RelayCommand]
    private async Task OpenSettingsFolder()
    {
        await OpenFolder(Se.SettingsBackupFolder);
    }

    private async Task OpenFolder(string folder)
    {
        if (Window == null)
        {
            return;
        }

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

        await _folderHelper.OpenFolder(Window, folder);
    }

    [RelayCommand]
    private async Task BackupSettingsNow()
    {
        if (Window == null)
        {
            return;
        }

        // Persist first so the snapshot holds what the user sees, not the last written file.
        string? backupFileName = null;
        try
        {
            Se.SaveSettings();
            backupFileName = await Task.Run(() => _autoBackupService.BackupSettingsNow());
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Could not back up settings");
        }

        LoadSettingsBackups();
        if (backupFileName != null)
        {
            SelectedSettingsFile = SettingsFiles.FirstOrDefault(f => f.FullPath == backupFileName) ?? SelectedSettingsFile;
        }
    }

    [RelayCommand]
    private async Task RestoreSettings()
    {
        if (SelectedSettingsFile is not { } file || Window == null || !File.Exists(file.FullPath))
        {
            return;
        }

        var l = Se.Language.File.RestoreAutoBackup;
        var answer = await MessageBox.Show(
            Window,
            l.RestoreSettings,
            string.Format(l.RestoreSettingsFromX, file.DateAndTime),
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            // Safety net: the settings being replaced get their own backup, so a wrong pick
            // is itself undoable from this list.
            Se.SaveSettings();
            _autoBackupService.BackupSettingsNow();

            // Load into the live Se.Settings and write it back: the main window saves
            // settings on exit, so a plain file copy would be overwritten by the old
            // in-memory state a moment later.
            Se.LoadSettings(file.FullPath);
            Se.SaveSettings();
            SettingsRestored = true;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Could not restore settings from " + file.FullPath);
            LoadSettingsBackups();
            await MessageBox.Show(Window, l.RestoreSettings, string.Format(l.SettingsRestoreFailed, exception.Message), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        LoadSettingsBackups();
        await MessageBox.Show(Window, l.RestoreSettings, l.SettingsRestored, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    [RelayCommand]
    private async Task DeleteAllSettingsFiles()
    {
        if (Window == null)
        {
            return;
        }

        var answer = MessageBoxResult.Yes;
        if (Se.Settings.General.PromptBeforeDelete)
        {
            answer = await MessageBox.Show(
                Window,
                Se.Language.General.Delete,
                Se.Language.File.RestoreAutoBackup.DeleteAllSettingsBackups,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
        }
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        DeleteFiles(SettingsFiles);
        LoadSettingsBackups();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/file", "restore-auto-backup");
        }
    }

    /// <summary>
    /// Follows the selection itself instead of only the grid's SelectionChanged event: the
    /// row the grid picks on its own (AlwaysSelected) raises no event, which left Restore
    /// disabled while row 0 looked selected.
    /// </summary>
    partial void OnSelectedFileChanged(DisplayFile? value)
    {
        IsOkButtonEnabled = value != null;
    }

    partial void OnSelectedSettingsFileChanged(DisplayFile? value)
    {
        IsRestoreSettingsEnabled = value != null;
    }

    public void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        IsOkButtonEnabled = SelectedFile != null;
    }

    public void SettingsGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        IsRestoreSettingsEnabled = SelectedSettingsFile != null;
    }
}
