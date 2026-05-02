using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

public partial class BinaryOcrInspectViewModel : ObservableObject
{
    public Window? Window { get; set; }

    [ObservableProperty] private string _title;
    [ObservableProperty] private string _newText;
    [ObservableProperty] private string _resolutionAndTopMargin;
    [ObservableProperty] private string _matchResolutionAndTopMargin;
    [ObservableProperty] private string _zoomFactorInfo;
    [ObservableProperty] private bool _isNewTextItalic;
    [ObservableProperty] private bool _submitOnFirstLetter;
    [ObservableProperty] private Bitmap? _sentenceImageSource;
    [ObservableProperty] private Bitmap? _itemImageSource;
    [ObservableProperty] private bool _isEditControlsEnabled;
    [ObservableProperty] private bool _canAddBetterMatch;
    [ObservableProperty] private ObservableCollection<int> _noOfLinesToAutoDrawList;
    [ObservableProperty] private int _selectedNoOfLinesToAutoDraw;
    [ObservableProperty] private Bitmap _currentBitmap;
    [ObservableProperty] private Bitmap _sentenceBitmap;

    private List<ImageSplitterItem2> _letters;
    private List<BinaryOcrMatcher.CompareMatch?> _matches;
    private ImageSplitterItem2 _splitItem;
    public BinaryOcrBitmap? BinaryOcrBitmap { get; private set; }
    public StackPanel PanelLines { get; set; }
    public TextBox TextBoxNew { get; set; }
    public bool OkPressed { get; set; }
    public bool AddBetterMatchPressed { get; set; }
    public int LetterIndex { get; internal set; }

    private SKBitmap _sentenceBitmapOriginal;
    private BinaryOcrDb _db;
    private double _maxErrorPercent;
    private NikseBitmap2 _nBmp;
    private NikseBitmap2? _displayBitmap;
    private SKRectI _displayBounds;
    private int _displayTopMargin;
    private OcrSubtitleItem? _ocrSubtitleItem;
    private int _maxWrongPixels;
    private BinaryOcrAddHistoryManager _binaryOcrAddHistoryManager;
    private bool _isControlDown = false;
    private bool _isWinDown = false;

    public BinaryOcrInspectViewModel()
    {
        Title = Se.Language.Ocr.InspectImageMatches;
        PanelLines = new StackPanel();
        NewText = string.Empty;
        ResolutionAndTopMargin = string.Empty;
        MatchResolutionAndTopMargin = string.Empty;
        IsNewTextItalic = false;
        SubmitOnFirstLetter = false;
        _letters = new List<ImageSplitterItem2>();
        _matches = new List<BinaryOcrMatcher.CompareMatch?>();
        _sentenceBitmapOriginal = new SKBitmap(1, 1, true);
        ZoomFactorInfo = string.Empty;

        const int maxLines = 500;
        NoOfLinesToAutoDrawList = new ObservableCollection<int>();
        for (var i = 0; i <= maxLines; i++)
        {
            NoOfLinesToAutoDrawList.Add(i);
        }

        SelectedNoOfLinesToAutoDraw = Se.Settings.Ocr.NOcrNoOfLinesToAutoDraw;
        BinaryOcrBitmap = null;
        _db = new BinaryOcrDb(string.Empty);
        _nBmp = new NikseBitmap2(1, 1);
        _ocrSubtitleItem = null;
        _binaryOcrAddHistoryManager = new BinaryOcrAddHistoryManager();
        SentenceBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        CurrentBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        _splitItem = new ImageSplitterItem2(string.Empty);
        TextBoxNew = new TextBox();
    }

    internal void Initialize(
        SKBitmap sKBitmap,
        NikseBitmap2 nBmp,
        OcrSubtitleItem? selectedOcrSubtitleItem,
        BinaryOcrDb db,
        double maxErrorPercent,
        int maxWrongPixels,
        BinaryOcrAddHistoryManager binaryOcrAddHistoryManager,
        List<ImageSplitterItem2> letters,
        List<BinaryOcrMatcher.CompareMatch?> matches)
    {
        _letters = letters;
        _matches = matches;
        _db = db;
        _maxErrorPercent = maxErrorPercent;
        _maxWrongPixels = maxWrongPixels;
        _binaryOcrAddHistoryManager = binaryOcrAddHistoryManager;
        _sentenceBitmapOriginal = sKBitmap;
        _nBmp = nBmp;
        _ocrSubtitleItem = selectedOcrSubtitleItem;

        if (letters.Count > 0)
        {
            OnLetterClicked(0, _matches[0]);
        }

        if (selectedOcrSubtitleItem != null)
        {
            InitSentenceBitmap();
        }
    }

    private void InitSentenceBitmap()
    {
        var skBitmap = _sentenceBitmapOriginal.Copy();
        if (_displayBitmap != null)
        {
            var rect = new SKRect(_displayBounds.Left, _displayBounds.Top, _displayBounds.Right, _displayBounds.Bottom);
            using (var canvas = new SKCanvas(skBitmap))
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Red,
                    StrokeWidth = 2, // Thickness of the rectangle border
                    IsAntialias = true
                })
                {
                    canvas.DrawRect(rect, paint);
                }
            }
        }

        SentenceBitmap = skBitmap.ToAvaloniaBitmap();
    }

    [RelayCommand]
    private void Ok()
    {
        if (BinaryOcrBitmap != null)
        {
            BinaryOcrBitmap.Text = NewText;
        }
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private async Task Update()
    {
        if (string.IsNullOrEmpty(NewText))
        {
            return;
        }

        var dbBitmap = FindDbBitmap(_matches[LetterIndex]);
        if (dbBitmap == null)
        {
            return;
        }

        // Update the actual database entry
        dbBitmap.Text = NewText;
        dbBitmap.Italic = IsNewTextItalic;

        // Keep local objects in sync
        var match = _matches[LetterIndex];
        if (match != null)
        {
            match.Text = NewText;
            match.Italic = IsNewTextItalic;
        }

        _db.Save();

        // Rebuild letter buttons so the updated text is reflected
        PanelLines.Children.Clear();
        OnLoaded();
        OnLetterClicked(LetterIndex, _matches[LetterIndex]);

        await MessageBox.Show(
            Window!,
            "Binary OCR",
            "Binary OCR character saved",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    [RelayCommand]
    private async Task Delete()
    {
        var dbBitmap = FindDbBitmap(_matches[LetterIndex]);
        if (dbBitmap == null)
        {
            return;
        }

        var answer = await MessageBox.Show(
                   Window!,
                   "Delete Binary OCR item?",
                   $"Do you want to delete the current Binary OCR item?",
                   MessageBoxButtons.YesNoCancel,
                   MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        // Remove the actual database entry
        _db.CompareImages.Remove(dbBitmap);
        _db.CompareImagesExpanded.Remove(dbBitmap);
        _db.Save();

        _matches[LetterIndex] = new BinaryOcrMatcher().GetCompareMatch(
            _letters[LetterIndex],
            out _,
            _letters,
            LetterIndex,
            _db,
            _maxErrorPercent);

        PanelLines.Children.Clear();
        OnLoaded();
        OnLetterClicked(LetterIndex, _matches[LetterIndex]);
    }

    [RelayCommand]
    private async Task AddBetterMatch()
    {
        if (_ocrSubtitleItem == null)
        {
            return;
        }

        var addVm = new BinaryOcrCharacterAddViewModel();
        addVm.Initialize(_nBmp, _ocrSubtitleItem, _letters, LetterIndex, _db, _maxWrongPixels,
            _binaryOcrAddHistoryManager, false, false);
        var addWindow = new BinaryOcrCharacterAddWindow(addVm);
        await addWindow.ShowDialog(Window!);

        if (addVm.OkPressed && addVm.BinaryOcrBitmap != null)
        {
            var previewBitmap = addVm.PreviewBitmap ?? _letters[LetterIndex].NikseBitmap;
            _binaryOcrAddHistoryManager.Add(addVm.BinaryOcrBitmap, previewBitmap, addVm.PreviewTopMargin, 0);
            _db.Add(addVm.BinaryOcrBitmap);
            _ = Task.Run(_db.Save);
            ReloadMatches();
        }
    }

    private BinaryOcrBitmap? FindDbBitmap(BinaryOcrMatcher.CompareMatch? match)
    {
        if (match?.Name == null)
        {
            return null;
        }

        return _db.CompareImages.FirstOrDefault(b => b.Key == match.Name)
               ?? _db.CompareImagesExpanded.FirstOrDefault(b => b.Key == match.Name);
    }

    private void ReloadMatches()
    {
        for (var i = 0; i < _letters.Count; i++)
        {
            if (_letters[i].NikseBitmap == null)
            {
                continue;
            }

            _matches[i] = new BinaryOcrMatcher().GetCompareMatch(
                _letters[i],
                out _,
                _letters,
                i,
                _db,
                _maxErrorPercent);
        }

        PanelLines.Children.Clear();
        OnLoaded();
        OnLetterClicked(LetterIndex, _matches[LetterIndex]);
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void TextBoxNewOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Ok();
        }
    }

    internal void OnLoaded()
    {
        if (Window != null)
        {
            Window.Title = Title;
        }

        WrapPanel currentLine = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(2)
        };
        PanelLines.Children.Add(currentLine);

        for (var i = 0; i < _matches.Count; i++)
        {
            BinaryOcrMatcher.CompareMatch? match = _matches[i];
            if (match == null)
            {
                var buttonNotFound = UiUtil.MakeButton(string.Empty)
                    .WithIconLeft(IconNames.Help)
                    .WithMargin(4)
                    .WithPadding(8)
                    .WithMinWidth(0); ;
                var indexNotFound = i;
                buttonNotFound.Click += (_, _) => OnLetterClicked(indexNotFound, match);
                currentLine.Children.Add(buttonNotFound);
                continue;
            }

            if (match.Text == Environment.NewLine)
            {
                currentLine = new WrapPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(2)
                };
                PanelLines.Children.Add(currentLine);
                continue;
            }

            if (match.Text == " ")
            {
                var labelSpace = UiUtil.MakeLabel(string.Empty).WithMarginLeft(10);
                currentLine.Children.Add(labelSpace);
                continue;
            }

            var button = new Button
            {
                Content = match.Text ?? "*",
                Margin = new Thickness(4),
                Padding = new Thickness(8)
            };

            var index = i;
            button.Click += (_, _) => OnLetterClicked(index, match);
            currentLine.Children.Add(button);
        }
    }

    private void OnLetterClicked(int index, BinaryOcrMatcher.CompareMatch? match)
    {
        LetterIndex = index;
        _splitItem = _letters[index];
        _displayBitmap = null;
        _displayBounds = default;
        _displayTopMargin = 0;
        var expandedGroup = match is { ExpandCount: > 1 }
            ? ExpandedOcrGroup.Create(_nBmp, _letters, index, match.ExpandCount)
            : null;

        IsEditControlsEnabled = match != null;

        CanAddBetterMatch = match != null && match.ImageSplitterItem?.NikseBitmap != null;
        if (_splitItem.NikseBitmap != null)
        {
            BinaryOcrBitmap x;
            if (expandedGroup != null)
            {
                x = expandedGroup.CreateBinaryOcrBitmap();
            }
            else
            {
                x = new BinaryOcrBitmap(_splitItem.NikseBitmap);
            }

            x.X = _splitItem.X;
            x.Y = _splitItem.Top;
            x.Text = match?.Text ?? string.Empty;
            x.Italic = match?.Italic ?? false;
            CanAddBetterMatch = match is { ExpandCount: > 1 }
                ? _db.FindExactMatchExpanded(x) < 0
                : _db.FindExactMatch(x) < 0;
        }

        var matchResolution = string.Empty;
        if (match != null && _splitItem.NikseBitmap != null)
        {
            // Try to find the actual BinaryOcrBitmap from the database
            BinaryOcrBitmap? dbBitmap = null;
            if (match.Name != null)
            {
                var key = match.Name;
                dbBitmap = _db.CompareImages.FirstOrDefault(b => b.Key == key) ??
                          _db.CompareImagesExpanded.FirstOrDefault(b => b.Key == key);
            }

            if (dbBitmap != null)
            {
                matchResolution = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, dbBitmap.Width, dbBitmap.Height, dbBitmap.Y);
            }
        }

        if (_splitItem.NikseBitmap != null)
        {
            BinaryOcrBitmap = new BinaryOcrBitmap(_splitItem.NikseBitmap)
            {
                X = _splitItem.X,
                Y = _splitItem.Top,
                Text = match?.Text ?? string.Empty,
                Italic = match?.Italic ?? false
            };

            NewText = match?.Text ?? string.Empty;
            IsNewTextItalic = match is { Italic: true };
            _displayBitmap = _splitItem.NikseBitmap;
            _displayBounds = new SKRectI(_splitItem.X, _splitItem.Y, _splitItem.X + _splitItem.NikseBitmap.Width, _splitItem.Y + _splitItem.NikseBitmap.Height);
            _displayTopMargin = _splitItem.Top;

            if (expandedGroup != null)
            {
                _displayBitmap = expandedGroup.PreviewBitmap;
                _displayBounds = expandedGroup.Bounds;
                _displayTopMargin = expandedGroup.PreviewTopMargin;
                BinaryOcrBitmap = expandedGroup.CreateBinaryOcrBitmap();
                BinaryOcrBitmap.Text = match?.Text ?? string.Empty;
                BinaryOcrBitmap.Italic = match?.Italic ?? false;
            }

            ResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, _displayBitmap.Width, _displayBitmap.Height, _displayTopMargin);
            MatchResolutionAndTopMargin = expandedGroup != null
                ? ResolutionAndTopMargin
                : matchResolution;

            CurrentBitmap = _displayBitmap.GetBitmap().ToAvaloniaBitmap();
        }
        else
        {
            MatchResolutionAndTopMargin = matchResolution;
        }

        InitSentenceBitmap();
    }

    public void KeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (e.Key == Key.Left)
        {
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            e.Handled = true;
        }
        else if (e.Key == Key.I)
        {
            if (_isControlDown || _isWinDown)
            {
                e.Handled = true;
                IsNewTextItalic = !IsNewTextItalic;
            }
        }
        else if (e.Key == Key.F)
        {
            if (_isControlDown || _isWinDown)
            {
                e.Handled = true;
                SubmitOnFirstLetter = !SubmitOnFirstLetter;
            }
        }
        else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlDown = true;
        }
        else if (e.Key == Key.LWin || e.Key == Key.RWin)
        {
            _isWinDown = true;
        }
    }

    public void KeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlDown = false;
        }
        else if (e.Key == Key.LWin || e.Key == Key.RWin)
        {
            _isWinDown = false;
        }
    }

    public void ItalicCheckChanged(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBoxNew.FontStyle = IsNewTextItalic ? FontStyle.Italic : FontStyle.Normal;
        });
    }
}
