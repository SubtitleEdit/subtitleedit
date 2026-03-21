using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Files.ManualChosenEncoding;

public partial class ManualChosenEncodingViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    [ObservableProperty] private TextEncoding? _selectedEncoding;
    [ObservableProperty] private string _currentEncodingText;
    [ObservableProperty] private string _previewText;
    [ObservableProperty] private bool _isOkEnabled;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private byte[] _fileBuffer;
    private List<TextEncoding> _allEncodings;
    private bool _dirty;
    private readonly System.Timers.Timer _timerSearch;

    public ManualChosenEncodingViewModel()
    {
        SearchText = string.Empty;
        _allEncodings = EncodingHelper.GetEncodings();
        Encodings = new ObservableCollection<TextEncoding>(_allEncodings);
        PreviewText = string.Empty;
        CurrentEncodingText = string.Empty;
        _fileBuffer = [];

        _timerSearch = new System.Timers.Timer(500);
        _timerSearch.Elapsed += (s, e) =>
        {
            _timerSearch.Stop();
            if (_dirty)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    _dirty = false;
                    UpdateSearchEncodings();
                });
            }
            _timerSearch.Start();
        };
    }

    private void UpdateSearchEncodings()
    {
        if (string.IsNullOrWhiteSpace(SearchText) && Encodings.Count == _allEncodings.Count)
        {
            return;
        }

        Encodings.Clear();
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Encodings.AddRange(_allEncodings);
            return;
        }

        foreach (var encoding in _allEncodings)
        {
            if (encoding.DisplayName.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase))
            {
                Encodings.Add(encoding);
            }
        }

        if (Encodings.Count > 0)
        {
            SelectedEncoding = Encodings[0];
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(string fileName)
    {
        try
        {
            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var readCount = (int)Math.Min(100000, fs.Length);
            _fileBuffer = new byte[readCount];
            fs.ReadExactly(_fileBuffer, 0, readCount);
            IsOkEnabled = true;
        }
        catch
        {
            _fileBuffer = [];
            IsOkEnabled = false;
        }

        Encoding encoding;
        if (_fileBuffer.Length > 10 && _fileBuffer[0] == 0xef && _fileBuffer[1] == 0xbb && _fileBuffer[2] == 0xbf) // UTF-8 BOM
        {
            SelectedEncoding = Encodings.FirstOrDefault(e => e.DisplayName == TextEncoding.Utf8WithBom);

            _timerSearch.Start();

            Dispatcher.UIThread.Invoke(() =>
            {
                EncodingChanged();
            });

            return;
        }

        encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
        SelectedEncoding = Encodings.FirstOrDefault(e => e.Encoding.CodePage == encoding.CodePage && e.DisplayName != TextEncoding.Utf8WithBom);
        _timerSearch.Start();

        Dispatcher.UIThread.Invoke(() =>
        {
            EncodingChanged();
        });
    }

    internal void EncodingChanged(object? sender, SelectionChangedEventArgs e)
    {
        EncodingChanged();
    }

    private void EncodingChanged()
    {
        var selected = SelectedEncoding;
        if (selected == null || _fileBuffer.Length == 0)
        {
            CurrentEncodingText = string.Empty;
            PreviewText = string.Empty;
            return;
        }

        PreviewText = selected.Encoding.GetString(_fileBuffer).Replace("\0", " ");
        CurrentEncodingText = selected.DisplayName;
    }

    internal void SearchTextChanged()
    {
        _dirty = true;
    }
}