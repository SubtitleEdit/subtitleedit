using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;

public partial class GetDictionariesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<GetSpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private GetSpellCheckDictionaryDisplay? selectedDictionary;
    [ObservableProperty] private string _description;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isDownloadEnabled;
    [ObservableProperty] private bool _isProgressVisible;
    [ObservableProperty] private double _progressOpacity;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public string? DictionaryFileName { get; private set; }

    private Task? _downloadTask;
    private bool _done;
    private readonly System.Timers.Timer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;

    private readonly ISpellCheckDictionaryDownloadService _spellCheckDictionaryDownloadService;
    private readonly IZipUnpacker _zipUnpacker;
    private readonly IFolderHelper _folderHelper;

    public GetDictionariesViewModel(ISpellCheckDictionaryDownloadService spellCheckDictionaryDownloadService, IZipUnpacker zipUnpacker, IFolderHelper folderHelper)
    {
        _spellCheckDictionaryDownloadService = spellCheckDictionaryDownloadService;
        _zipUnpacker = zipUnpacker;
        _folderHelper = folderHelper;

        Dictionaries = new ObservableCollection<GetSpellCheckDictionaryDisplay>();
        SelectedDictionary = null;
        Description = string.Empty;
        IsDownloadEnabled = true;
        IsProgressVisible = false;
        StatusText = string.Empty;
        DictionaryFileName = string.Empty;

        _cancellationTokenSource = new CancellationTokenSource();
        _downloadStream = new MemoryStream();
        _progressOpacity = 0;

        LoadDictionaries();
        _timer = new System.Timers.Timer(500);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    private readonly Lock _lockObj = new();

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_lockObj)
        {
            if (_done)
            {
                return;
            }

            if (_downloadTask is { IsCompleted: true })
            {
                _timer.Stop();
                _done = true;

                if (_downloadStream.Length == 0)
                {
                    StatusText = "Download failed";
                    return;
                }

                DictionaryFileName = UnpackDictionary();
                OkPressed = true;
                Close();
            }
            else if (_downloadTask is { IsFaulted: true })
            {
                _timer.Stop();
                _done = true;
                var ex = _downloadTask.Exception?.InnerException ?? _downloadTask.Exception;
                if (ex is OperationCanceledException)
                {
                    StatusText = "Download canceled";
                    Close();
                }
                else
                {
                    StatusText = "Download failed";
                }
            }
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    private string? UnpackDictionary()
    {
        var folder = Se.DictionariesFolder;

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var outputFileNames = new List<string>();

        _downloadStream.Position = 0;
        _zipUnpacker.UnpackZipStream(
            _downloadStream,
            folder,
            string.Empty,
            true,
            new List<string> { ".dic", ".aff" },
            outputFileNames);

        _downloadStream.Dispose();
        
        var dicFiles = outputFileNames.Where(p=>p.EndsWith(".dic")).ToList();
        if (dicFiles.Count == 0)
        {
            return string.Empty;
        }

        var largestFileSize = (long)-1;
        var largestFileName = string.Empty;

        foreach (var file in dicFiles)
        {
            var fi = new FileInfo(file);
            if (fi.Length > largestFileSize)
            {
                largestFileSize = fi.Length;
                largestFileName = file;
            }
        }

        return largestFileName;
    }

    private void LoadDictionaries()
    {
        var uri = new Uri("avares://SubtitleEdit/Assets/HunspellDictionaries.json");
        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);

        var jsonContent = reader.ReadToEndAsync().Result;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var dictionaries = JsonSerializer.Deserialize<List<GetSpellCheckDictionaryDisplay>>(jsonContent, options);
        foreach (var dictionary in dictionaries ?? new List<GetSpellCheckDictionaryDisplay>())
        {
            Dictionaries.Add(dictionary);
        }

        var englishName = CultureInfo.CurrentCulture.EnglishName;
        if (englishName.Contains('(') && englishName.Contains(')'))
        {
            var start = englishName.IndexOf('(') + 1;
            var end = englishName.IndexOf(')');
            englishName = englishName.Substring(start, end - start).Trim();
        }

        var selected = Dictionaries.FirstOrDefault(d => d.EnglishName.Contains(englishName, StringComparison.OrdinalIgnoreCase) ||
                                                        d.NativeName.Contains(englishName, StringComparison.OrdinalIgnoreCase));
        if (selected == null)
        {
            if (LanguageHelper.CountryToLanguage.TryGetValue(englishName.ToLower(), out var languageName))
            {
                selected = Dictionaries.FirstOrDefault(d => d.EnglishName.Equals(languageName, StringComparison.OrdinalIgnoreCase) ||
                                                            d.NativeName.Equals(languageName, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (selected != null)
        {
            SelectedDictionary = selected;
            Description = selected.Description;
        }
        else
        {
            SelectedDictionary = Dictionaries.FirstOrDefault();
            Description = string.Empty;
        }
    }

    [RelayCommand]
    private void Download()
    {
        var selected = SelectedDictionary;
        if (selected == null)
        {
            return;
        }

        ProgressOpacity = 1.0;
        IsDownloadEnabled = false;
        IsProgressVisible = true;
        Progress = 0;

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            Progress = percentage;
            StatusText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        var folder = Se.FfmpegFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _downloadTask = _spellCheckDictionaryDownloadService.DownloadDictionary(
            _downloadStream,
            selected.DownloadLink,
            downloadProgress,
            _cancellationTokenSource.Token);
    }

    [RelayCommand]
    private async Task OpenFolder()
    {
        if (!Directory.Exists(Se.DictionariesFolder))
        {
            Directory.CreateDirectory(Se.DictionariesFolder);
        }

        await _folderHelper.OpenFolder(Window!, Se.DictionariesFolder);
    }

    [RelayCommand]
    private void Ok()
    {
        _timer.Stop();
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
        _timer.Stop();
        _done = true;
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