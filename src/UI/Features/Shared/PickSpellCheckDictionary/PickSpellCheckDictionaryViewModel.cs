using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.PickSpellCheckDictionary;

public partial class PickSpellCheckDictionaryViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? selectedDictionary;
    [ObservableProperty] private string _description;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isDownloadEnabled;
    [ObservableProperty] private bool _isProgressVisible;
    [ObservableProperty] private double _progressOpacity;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public string? DictionaryFileName { get; private set; }

    private readonly ISpellCheckDictionaryDownloadService _spellCheckDictionaryDownloadService;
    private readonly IZipUnpacker _zipUnpacker;
    private readonly IFolderHelper _folderHelper;
    private readonly ISpellCheckManager _spellCheckManager;
    private readonly IWindowService _windowService;

    public PickSpellCheckDictionaryViewModel(ISpellCheckDictionaryDownloadService spellCheckDictionaryDownloadService, IZipUnpacker zipUnpacker, IFolderHelper folderHelper, ISpellCheckManager spellCheckManager, IWindowService windowService)
    {
        _spellCheckDictionaryDownloadService = spellCheckDictionaryDownloadService;
        _zipUnpacker = zipUnpacker;
        _folderHelper = folderHelper;
        _spellCheckManager = spellCheckManager;
        _windowService = windowService;

        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();
        SelectedDictionary = null;
        Description = string.Empty;
        IsDownloadEnabled = true;
        IsProgressVisible = false;
        StatusText = string.Empty;
        DictionaryFileName = string.Empty;

        _progressOpacity = 0;

        LoadDictionaries();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    private void LoadDictionaries()
    {
        var spellCheckLanguages = _spellCheckManager.GetDictionaryLanguages(Se.DictionariesFolder);
        Dictionaries.Clear();
        Dictionaries.AddRange(spellCheckLanguages);
        if (Dictionaries.Count > 0)
        {
            if (!string.IsNullOrEmpty(Se.Settings.SpellCheck.LastLanguageDictionaryFile))
            {
                SelectedDictionary = Dictionaries.FirstOrDefault(l => l.DictionaryFileName == Se.Settings.SpellCheck.LastLanguageDictionaryFile);
            }

            SelectedDictionary = Dictionaries.FirstOrDefault(l => l.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
            if (SelectedDictionary == null)
            {
                SelectedDictionary = Dictionaries[0];
            }

            _spellCheckManager.Initialize(SelectedDictionary.DictionaryFileName, SpellCheckDictionaryDisplay.GetTwoLetterLanguageCode(SelectedDictionary));
        }
    }

    [RelayCommand]
    private async Task BrowseDictionary()
    {
        var result = await _windowService.ShowDialogAsync<GetDictionariesWindow, GetDictionariesViewModel>(Window!, vm => { });
        if (result.OkPressed)
        {
            LoadDictionaries();
            SelectedDictionary = Dictionaries.FirstOrDefault(d => d.DictionaryFileName == result.DictionaryFileName);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}