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
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;

public partial class GetDictionariesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<GetSpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private GetSpellCheckDictionaryDisplay? selectedDictionary;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isDownloadEnabled;
    [ObservableProperty] private bool _isProgressVisible;
    [ObservableProperty] private double _progressOpacity;
    [ObservableProperty] private string _selectedStatusText;
    [ObservableProperty] private string _downloadButtonText;
    [ObservableProperty] private string _dictionariesFolder;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public SpellCheckDictionaryDisplay? SpellCheckDictionary { get; private set; }

    private Task? _downloadTask;
    private bool _done;
    private readonly System.Timers.Timer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly ISpellCheckDictionaryDownloadService _spellCheckDictionaryDownloadService;
    private readonly IZipUnpacker _zipUnpacker;
    private readonly IFolderHelper _folderHelper;

    /// <summary>
    /// Legacy archive entries do not expose a .dic file name in their download URL, so the
    /// installed state is detected by looking for a .dic file with this language prefix.
    /// </summary>
    private static readonly Dictionary<string, string> LegacyDicPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Finnish", "fi" },
        { "Polish", "pl_PL" },
        { "Basque", "eu" },
        { "Irish", "ga" },
        { "Khmer", "km" },
        { "Latin", "la" },
        { "Lower Sorbian", "dsb" },
        { "Malay", "ms" },
        { "Malayalam", "ml" },
        { "Macedonian", "mk" },
        { "Vietnamese and English", "vi_VN-and" },
        { "Welsh", "cy" },
        { "Xhosa", "xh" },
        { "Zulu", "zu" },
    };

    public GetDictionariesViewModel(ISpellCheckDictionaryDownloadService spellCheckDictionaryDownloadService, IZipUnpacker zipUnpacker, IFolderHelper folderHelper)
    {
        _spellCheckDictionaryDownloadService = spellCheckDictionaryDownloadService;
        _zipUnpacker = zipUnpacker;
        _folderHelper = folderHelper;

        Dictionaries = new ObservableCollection<GetSpellCheckDictionaryDisplay>();
        SelectedDictionary = null;
        IsDownloadEnabled = true;
        IsProgressVisible = false;
        StatusText = string.Empty;
        SpellCheckDictionary = null;

        _cancellationTokenSource = new CancellationTokenSource();
        _progressOpacity = 0;

        SelectedStatusText = string.Empty;
        DownloadButtonText = Se.Language.General.Download;
        DictionariesFolder = Se.DictionariesFolder;

        LoadDictionaries();
        RefreshInstalledStatus();
        _timer = new System.Timers.Timer(500);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    private readonly Lock _lockObj = new();

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_lockObj)
        {
            if (_done || _downloadTask == null)
            {
                return;
            }

            var ex = _downloadTask.Exception?.InnerException ?? _downloadTask.Exception;
            if (_downloadTask.IsCanceled || ex is OperationCanceledException)
            {
                _timer.Stop();
                _done = true;
                StatusText = "Download canceled";
                Close();
            }
            else if (_downloadTask is { IsFaulted: true })
            {
                HandleDownloadFailure();
            }
            else if (_downloadTask is { IsCompletedSuccessfully: true })
            {
                if (SpellCheckDictionary == null)
                {
                    HandleDownloadFailure();
                }
                else
                {
                    _timer.Stop();
                    _done = true;
                    OkPressed = true;
                    Close();
                }
            }
        }
    }

    /// <summary>
    /// Resets the window to its idle state after a failed download so the user can
    /// retry or close it normally. The timer keeps running to pick up a retry.
    /// </summary>
    private void HandleDownloadFailure()
    {
        _downloadTask = null;
        Dispatcher.UIThread.Post(() =>
        {
            StatusText = "Download failed";
            Progress = 0;
            ProgressOpacity = 0;
            IsProgressVisible = false;
            IsDownloadEnabled = true;
        });
    }

    private void Close()
    {
        _timer.Stop();
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    /// <summary>
    /// Downloads every file of the selected dictionary. A LibreOffice entry has direct
    /// .aff/.dic links that are saved as-is; a legacy entry has a single .oxt/.zip/.xpi
    /// archive that is unzipped. Sets <see cref="SpellCheckDictionary"/> to the largest .dic.
    /// </summary>
    private async Task DownloadAndUnpackAsync(GetSpellCheckDictionaryDisplay dictionary, IProgress<float> progress, CancellationToken cancellationToken)
    {
        var files = dictionary.Files;
        var folder = Se.DictionariesFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var dicFiles = new List<string>();

        for (var i = 0; i < files.Count; i++)
        {
            var url = files[i];
            var fileIndex = i;
            var fileProgress = new Progress<float>(p => progress.Report((fileIndex + p) / files.Count));

            using var stream = new MemoryStream();
            await _spellCheckDictionaryDownloadService.DownloadDictionary(stream, url, fileProgress, cancellationToken);

            if (stream.Length == 0)
            {
                throw new InvalidOperationException($"Dictionary download failed: {url}");
            }

            stream.Position = 0;

            if (IsHunspellFile(url))
            {
                var targetFileName = Path.Combine(folder, GetFileNameFromUrl(url));
                await using (var fileStream = File.Create(targetFileName))
                {
                    await stream.CopyToAsync(fileStream, cancellationToken);
                }

                if (targetFileName.EndsWith(".dic", StringComparison.OrdinalIgnoreCase))
                {
                    dicFiles.Add(targetFileName);
                }
            }
            else
            {
                var outputFileNames = new List<string>();
                _zipUnpacker.UnpackZipStream(
                    stream,
                    folder,
                    string.Empty,
                    true,
                    new List<string> { ".dic", ".aff" },
                    outputFileNames);

                dicFiles.AddRange(outputFileNames.Where(p => p.EndsWith(".dic", StringComparison.OrdinalIgnoreCase)));
            }
        }

        var largestDicFile = GetLargestFile(dicFiles);
        SpellCheckDictionary = string.IsNullOrEmpty(largestDicFile)
            ? null
            : new SpellCheckDictionaryDisplay
            {
                Name = dictionary.EnglishName,
                DictionaryFileName = largestDicFile,
            };
    }

    private static bool IsHunspellFile(string url)
    {
        var path = RemoveUrlQuery(url);
        return path.EndsWith(".dic", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".aff", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetFileNameFromUrl(string url)
    {
        var path = RemoveUrlQuery(url);
        var slashIndex = path.LastIndexOf('/');
        return slashIndex >= 0 ? path.Substring(slashIndex + 1) : path;
    }

    private static string RemoveUrlQuery(string url)
    {
        var queryIndex = url.IndexOf('?');
        return queryIndex >= 0 ? url.Substring(0, queryIndex) : url;
    }

    private static string GetLargestFile(IEnumerable<string> files)
    {
        var largestFileSize = (long)-1;
        var largestFileName = string.Empty;

        foreach (var file in files)
        {
            var fi = new FileInfo(file);
            if (fi.Exists && fi.Length > largestFileSize)
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

        var jsonContent = reader.ReadToEnd();

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

        SelectedDictionary = selected ?? Dictionaries.FirstOrDefault();
    }

    partial void OnSelectedDictionaryChanged(GetSpellCheckDictionaryDisplay? value)
    {
        UpdateSelectedStatus();
    }

    /// <summary>
    /// Marks each dictionary as installed or not by checking the dictionaries folder,
    /// and updates the green/gray status dots accordingly.
    /// </summary>
    private void RefreshInstalledStatus()
    {
        var installedDicFiles = new List<string>();
        if (Directory.Exists(Se.DictionariesFolder))
        {
            foreach (var file in Directory.GetFiles(Se.DictionariesFolder, "*.dic"))
            {
                installedDicFiles.Add(Path.GetFileName(file));
            }
        }

        foreach (var dictionary in Dictionaries)
        {
            dictionary.IsInstalled = IsEntryInstalled(dictionary, installedDicFiles);
        }

        UpdateSelectedStatus();
    }

    private void UpdateSelectedStatus()
    {
        var selected = SelectedDictionary;
        if (selected == null)
        {
            SelectedStatusText = string.Empty;
            DownloadButtonText = Se.Language.General.Download;
            return;
        }
        
        SelectedStatusText = selected.IsInstalled ? Se.Language.General.Installed : Se.Language.General.NotInstalled;
        DownloadButtonText = selected.IsInstalled ? Se.Language.General.Redownload : Se.Language.General.Download;
    }

    private static bool IsEntryInstalled(GetSpellCheckDictionaryDisplay entry, List<string> installedDicFiles)
    {
        var dicNames = entry.Files
            .Select(GetFileNameFromUrl)
            .Where(name => name.EndsWith(".dic", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (dicNames.Count > 0)
        {
            return dicNames.Any(name => installedDicFiles.Any(f => f.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }

        return LegacyDicPrefixes.TryGetValue(entry.EnglishName, out var prefix) &&
               installedDicFiles.Any(f => f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    [RelayCommand]
    private void Download()
    {
        var selected = SelectedDictionary;
        if (selected == null || selected.Files.Count == 0)
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

        _downloadTask = DownloadAndUnpackAsync(selected, downloadProgress, _cancellationTokenSource.Token);
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