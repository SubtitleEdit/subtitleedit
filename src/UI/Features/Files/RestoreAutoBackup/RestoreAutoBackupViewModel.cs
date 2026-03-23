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

    public Window? Window { get; set; }
    public string? RestoreFileName { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IAutoBackupService _autoBackupService;
    private readonly IFolderHelper _folderHelper;

    public RestoreAutoBackupViewModel(IAutoBackupService autoBackupService, IFolderHelper folderHelper)
    {
        _autoBackupService = autoBackupService;
        _folderHelper = folderHelper;
        Files = new ObservableCollection<DisplayFile>();
        Initialize();
    }

    private void Initialize()
    {
        var files = new List<DisplayFile>();
        foreach (var fileName in _autoBackupService.GetAutoBackupFiles())
        {
            var fileInfo = new FileInfo(fileName);

            var path = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            var displayDate = path[..19].Replace('_', ' ');
            displayDate = displayDate.Remove(13, 1).Insert(13, ":");
            displayDate = displayDate.Remove(16, 1).Insert(16, ":");

            files.Add(new DisplayFile(fileName, displayDate, Utilities.FormatBytesToDisplayFileSize(fileInfo.Length)));
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
    }

    [RelayCommand]
    private async Task DeleteAllFiles()
    {
        if (Window == null)
        {
            return;
        }

        var answer = MessageBoxResult.Yes;
        if (Se.Settings.General.PromptDeleteLines)
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

        foreach (var file in Files)
        {
            File.Delete(file.FullPath);
        }

        Files.Clear();
        IsEmptyFilesVisible = false;
        IsOkButtonEnabled = false;
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
            "Restore auto-backup file?",
            $"Do you want to restore \"{file.FileName}\" from {file.DateAndTime}?",
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
        if (Window == null)
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

        await _folderHelper.OpenFolder(Window, folder);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    public void DataGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        IsOkButtonEnabled = e.AddedItems.Count > 0;
    }
}