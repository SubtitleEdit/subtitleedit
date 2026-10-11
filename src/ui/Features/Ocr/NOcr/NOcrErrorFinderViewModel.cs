using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

/// <summary>
/// "Find errors in nOCR database": reads the subtitle images with nOCR and one or two other
/// engines, and lists the database entries that read other characters than those engines.
/// </summary>
public partial class NOcrErrorFinderViewModel : ObservableObject
{
    private const int MaxCompareEngines = 2;
    private const int BatchSize = 25;

    [ObservableProperty] private ObservableCollection<OcrCompareEngine> _engines = new();
    [ObservableProperty] private ObservableCollection<NOcrErrorFinderItem> _items = new();
    [ObservableProperty] private NOcrErrorFinderItem? _selectedItem;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _canStart;
    [ObservableProperty] private bool _hasSelection;
    [ObservableProperty] private bool _canChangeText;
    [ObservableProperty] private string _changeTextText = string.Format(Se.Language.Ocr.ChangeTextToX, string.Empty);
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _statusText = string.Empty;

    public Window? Window { get; set; }

    private NOcrDb _db = new(string.Empty);
    private List<OcrSubtitleItem> _lines = new();
    private Func<OcrSubtitleItem, List<NOcrGlyphRead>> _readNOcr = _ => new List<NOcrGlyphRead>();
    private Func<Window, OcrSubtitleItem, Task> _inspectLine = (_, _) => Task.CompletedTask;
    private NOcrDbErrorFinder _finder = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _backupSaved;

    public void Initialize(
        NOcrDb db,
        List<OcrSubtitleItem> lines,
        List<OcrCompareEngine> engines,
        Func<OcrSubtitleItem, List<NOcrGlyphRead>> readNOcr,
        Func<Window, OcrSubtitleItem, Task> inspectLine)
    {
        _db = db;
        _lines = lines;
        _readNOcr = readNOcr;
        _inspectLine = inspectLine;

        foreach (var engine in engines)
        {
            engine.IsChecked = Engines.Count < MaxCompareEngines;
            engine.PropertyChanged += EnginePropertyChanged;
            Engines.Add(engine);
        }

        UpdateEngineStates();
        StatusText = Engines.Count == 0 ? Se.Language.Ocr.NoOtherOcrEngineToCompareWith : string.Empty;
    }

    private void EnginePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OcrCompareEngine.IsChecked))
        {
            UpdateEngineStates();
        }
    }

    private void UpdateEngineStates()
    {
        var checkedCount = Engines.Count(p => p.IsChecked);
        foreach (var engine in Engines)
        {
            engine.IsEnabled = !IsRunning && (engine.IsChecked || checkedCount < MaxCompareEngines);
        }

        CanStart = !IsRunning && checkedCount > 0 && _lines.Count > 0;
    }

    partial void OnIsRunningChanged(bool value) => UpdateEngineStates();

    partial void OnSelectedItemChanged(NOcrErrorFinderItem? value)
    {
        HasSelection = value != null && !IsRunning;
        var suggestion = value?.Suspect.SuggestedText;
        CanChangeText = HasSelection && suggestion != null;
        ChangeTextText = string.Format(Se.Language.Ocr.ChangeTextToX, suggestion ?? value?.Suspect.MostCommonExpected ?? string.Empty);
    }

    [RelayCommand]
    private void Start()
    {
        if (IsRunning || !CanStart)
        {
            return;
        }

        var engines = Engines.Where(p => p.IsChecked).ToList();
        _finder = new NOcrDbErrorFinder();
        Items.Clear();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        IsRunning = true;
        OnSelectedItemChanged(null);
        ProgressValue = 0;

        _ = Task.Run(async () =>
        {
            try
            {
                for (var start = 0; start < _lines.Count && !token.IsCancellationRequested; start += BatchSize)
                {
                    var indices = Enumerable.Range(start, Math.Min(BatchSize, _lines.Count - start)).ToList();
                    var glyphs = new Dictionary<int, List<NOcrGlyphRead>>();
                    var images = new List<(int Index, SKBitmap Bitmap)>();
                    foreach (var i in indices)
                    {
                        glyphs[i] = _readNOcr(_lines[i]);
                        images.Add((i, _lines[i].GetSkBitmap()));
                    }

                    var reads = new List<Dictionary<int, string>>();
                    foreach (var engine in engines)
                    {
                        Dispatcher.UIThread.Post(() => StatusText = string.Format(Se.Language.Ocr.CheckingLineXOfY, start + 1, _lines.Count) + "  " + engine.Name);
                        reads.Add(await engine.Read(images, token));
                    }

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    foreach (var i in indices)
                    {
                        var controls = reads.Select(p => p.TryGetValue(i, out var text) ? text : null).ToList();
                        _finder.AddLine(i, glyphs[i], controls);
                    }

                    var suspects = _finder.GetSuspects();
                    var done = indices[^1] + 1;
                    Dispatcher.UIThread.Post(() =>
                    {
                        ProgressValue = done * 100.0 / _lines.Count;
                        ShowSuspects(suspects);
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // stopped by the user
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Find errors in nOCR database failed");
                Dispatcher.UIThread.Post(() => StatusText = exception.Message);
                return;
            }
            finally
            {
                Dispatcher.UIThread.Post(() =>
                {
                    IsRunning = false;
                    OnSelectedItemChanged(SelectedItem);
                });
            }

            Dispatcher.UIThread.Post(() => ShowSuspects(_finder.GetSuspects()));
        }, token);
    }

    private void ShowSuspects(List<NOcrDbSuspectEntry> suspects)
    {
        var selected = SelectedItem?.Suspect.Entry;
        var shown = Items.ToDictionary<NOcrErrorFinderItem, NOcrChar>(p => p.Suspect.Entry, ReferenceEqualityComparer.Instance);
        Items.Clear();
        foreach (var suspect in suspects)
        {
            // keep the item (and its image) when the counts did not change
            Items.Add(shown.TryGetValue(suspect.Entry, out var old) && old.Used == suspect.Uses
                ? old
                : new NOcrErrorFinderItem(suspect));
        }

        SelectedItem = Items.FirstOrDefault(p => ReferenceEquals(p.Suspect.Entry, selected)) ?? Items.FirstOrDefault();
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        StatusText = string.Format(Se.Language.Ocr.NOcrErrorCheckStatusXYZ, _finder.LinesCompared, _finder.LinesChecked, Items.Count);
        if (_backupSaved)
        {
            StatusText += "  -  " + string.Format(Se.Language.Ocr.BackupSavedAsX, Path.GetFileName(_db.FileName + ".bak"));
        }
    }

    [RelayCommand]
    private void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }

    [RelayCommand]
    private void ChangeText()
    {
        var item = SelectedItem;
        if (item == null || IsRunning || item.Suspect.SuggestedText is not { } suggestion)
        {
            return;
        }

        BackupOnce();
        item.Suspect.Entry.Text = suggestion;
        SaveAndRemove(item);
    }

    [RelayCommand]
    private void Delete()
    {
        var item = SelectedItem;
        if (item == null || IsRunning)
        {
            return;
        }

        BackupOnce();
        _db.Remove(item.Suspect.Entry);
        SaveAndRemove(item);
    }

    [RelayCommand]
    private async Task Inspect()
    {
        var item = SelectedItem;
        if (item == null || IsRunning || Window == null || item.Suspect.Samples.Count == 0)
        {
            return;
        }

        var lineIndex = item.Suspect.Samples[0].LineIndex;
        if (lineIndex >= 0 && lineIndex < _lines.Count)
        {
            await _inspectLine(Window, _lines[lineIndex]);
        }
    }

    private void BackupOnce()
    {
        if (_backupSaved || !File.Exists(_db.FileName))
        {
            return;
        }

        File.Copy(_db.FileName, _db.FileName + ".bak", true);
        _backupSaved = true;
    }

    private void SaveAndRemove(NOcrErrorFinderItem item)
    {
        // Save also clears the match cache, so the next OCR run sees the change.
        _db.Save();
        _finder.Forget(item.Suspect.Entry);

        var index = Items.IndexOf(item);
        Items.Remove(item);
        SelectedItem = Items.Count == 0 ? null : Items[Math.Min(index, Items.Count - 1)];
        UpdateStatus();
    }

    [RelayCommand]
    private void Done()
    {
        Window?.Close();
    }

    internal void OnClosing()
    {
        _cancellationTokenSource?.Cancel();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Delete && SelectedItem != null && !IsRunning)
        {
            e.Handled = true;
            Delete();
        }
    }
}
