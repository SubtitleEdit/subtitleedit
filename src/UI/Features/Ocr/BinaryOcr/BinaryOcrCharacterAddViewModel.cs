using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

public partial class BinaryOcrCharacterAddViewModel : ObservableObject
{
    public Window? Window { get; set; }

    [ObservableProperty] private string _title;
    [ObservableProperty] private string _newText;
    [ObservableProperty] private string _resolutionAndTopMargin;
    [ObservableProperty] private bool _isNewTextItalic;
    [ObservableProperty] private bool _submitOnFirstLetter;
    [ObservableProperty] private bool _canShrink;
    [ObservableProperty] private bool _canExpand;
    [ObservableProperty] private bool _showUseOnce;
    [ObservableProperty] private bool _showSkip;
    [ObservableProperty] private bool _isInspectAdditionsVisible;
    [ObservableProperty] private Bitmap _sentenceBitmap;
    [ObservableProperty] private Bitmap _currentBitmap;

    private List<ImageSplitterItem2> _letters;
    private ImageSplitterItem2 _splitItem;

    public BinaryOcrBitmap? BinaryOcrBitmap { get; private set; }
    public TextBox TextBoxNew { get; set; }
    public bool OkPressed { get; set; }
    public bool AbortPressed { get; set; }
    public bool SkipPressed { get; set; }
    public bool UseOncePressed { get; set; }
    public bool InspectHistoryPressed { get; set; }

    private int _startFromNumber;
    private NikseBitmap2 _nBmp;
    private OcrSubtitleItem? _item;
    private int _expandCount;
    private BinaryOcrDb _db;
    private bool _isControlDown;
    private bool _isWinDown;
    public BinaryOcrBitmap? FirstBinaryOcrBitmap { get; set; }

    public BinaryOcrCharacterAddViewModel()
    {
        Title = Se.Language.Ocr.AddNewCharcter;
        NewText = string.Empty;
        ResolutionAndTopMargin = string.Empty;
        IsNewTextItalic = false;
        SubmitOnFirstLetter = false;
        _letters = new List<ImageSplitterItem2>();

        BinaryOcrBitmap = null;
        _db = new BinaryOcrDb(string.Empty);
        SentenceBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        CurrentBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        _splitItem = new ImageSplitterItem2(string.Empty);
        TextBoxNew = new TextBox();
        _nBmp = new NikseBitmap2(1, 1);
        ShowSkip = true;
        ShowUseOnce = true;
        LoadSettings();
    }

    private void LoadSettings()
    {
        IsNewTextItalic = Se.Settings.Ocr.IsNewLetterItalic;
        SubmitOnFirstLetter = Se.Settings.Ocr.SubmitOnFirstLetter;
    }

    private void SaveSettings()
    {
        Se.Settings.Ocr.IsNewLetterItalic = IsNewTextItalic;
        Se.Settings.Ocr.SubmitOnFirstLetter = SubmitOnFirstLetter;
        Se.SaveSettings();
    }

    public void Initialize(
        NikseBitmap2 nBmp,
        OcrSubtitleItem item,
        List<ImageSplitterItem2> letters,
        int i,
        BinaryOcrDb db,
        int maxWrongPixels,
        object addHistoryManager,
        bool showUseOnce,
        bool showSkip)
    {
        _db = db;
        _letters = letters;
        _startFromNumber = i;
        _nBmp = nBmp;
        _item = item;
        ShowSkip = showSkip;
        ShowUseOnce = showUseOnce;
        if (i >= 0 && i < letters.Count)
        {
            _splitItem = letters[i];
        }

        UpdateShrintExpand();
        SetImages(_item, _nBmp);
        SetTitle();

        // Support both NOcr and BinaryOcr history managers
        if (addHistoryManager is BinaryOcrAddHistoryManager binaryHistoryManager)
        {
            IsInspectAdditionsVisible = binaryHistoryManager.Items.Count > 0;
        }
        else if (addHistoryManager is Features.Ocr.NOcr.NOcrAddHistoryManager nOcrHistoryManager)
        {
            IsInspectAdditionsVisible = nOcrHistoryManager.Items.Count > 0;
        }
    }

    private void SetImages(OcrSubtitleItem? item, NikseBitmap2 nBmp)
    {
        if (item == null)
        {
            return;
        }

        var tempBitmap = item.GetSkBitmap();
        var topDiff = tempBitmap.Height - nBmp.Height;
        var skBitmap = RemoveTopLines(tempBitmap, topDiff);

        if (_splitItem.NikseBitmap != null)
        {
            BinaryOcrBitmap = new BinaryOcrBitmap(_splitItem.NikseBitmap)
            {
                X = _splitItem.X,
                Y = _splitItem.Top,
                ExpandCount = 0,
            };
            FirstBinaryOcrBitmap = BinaryOcrBitmap;
            ResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, BinaryOcrBitmap.Width, BinaryOcrBitmap.Height, BinaryOcrBitmap.Y);
        }

        if (_splitItem.NikseBitmap != null)
        {
            var rect = new SKRectI(
                _splitItem.X,
                _splitItem.Y,
                _splitItem.X + _splitItem.NikseBitmap.Width,
                _splitItem.Y + _splitItem.NikseBitmap.Height);

            CurrentBitmap = _splitItem.NikseBitmap!.GetBitmap().ToAvaloniaBitmap();

            if (_expandCount > 0)
            {
                var minMarginTop = int.MaxValue;
                var minX = int.MaxValue;
                var minY = int.MaxValue;
                var maxX = 0;
                var maxY = 0;

                for (var i = _startFromNumber; i < _startFromNumber + _expandCount + 1 && i < _letters.Count; i++)
                {
                    var letter = _letters[i];
                    if (letter.NikseBitmap != null)
                    {
                        minMarginTop = Math.Min(minMarginTop, letter.Top);
                        minX = Math.Min(minX, letter.X);
                        minY = Math.Min(minY, letter.Y);
                        maxX = Math.Max(maxX, letter.X + letter.NikseBitmap.Width);
                        maxY = Math.Max(maxY, letter.Y + letter.NikseBitmap.Height);
                    }
                }

                rect = new SKRectI(minX, minY, maxX, maxY);
                var subset = new SKBitmap();
                if (!nBmp.GetBitmap().ExtractSubset(subset, rect))
                {
                    throw new InvalidOperationException("Subset extraction failed.");
                }
                CurrentBitmap = subset.ToAvaloniaBitmap();

                var nikseBitmap = new NikseBitmap2(subset);
                BinaryOcrBitmap = new BinaryOcrBitmap(nikseBitmap)
                {
                    X = minX,
                    Y = minY,
                    ExpandCount = _expandCount + 1,
                };

                // Build expanded list
                BinaryOcrBitmap.ExpandedList = new List<BinaryOcrBitmap>();
                for (var j = 1; j <= _expandCount; j++)
                {
                    if (_startFromNumber + j < _letters.Count && _letters[_startFromNumber + j].NikseBitmap != null)
                    {
                        var expandedBitmap = new BinaryOcrBitmap(_letters[_startFromNumber + j].NikseBitmap!)
                        {
                            X = _letters[_startFromNumber + j].X,
                            Y = _letters[_startFromNumber + j].Top,
                        };
                        BinaryOcrBitmap.ExpandedList.Add(expandedBitmap);
                    }
                }

                ResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, BinaryOcrBitmap.Width, BinaryOcrBitmap.Height, BinaryOcrBitmap.Y);
            }

            using (var canvas = new SKCanvas(skBitmap))
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = new SKColor(255, 0, 0, 140), // Semi-transparent red
                    StrokeWidth = 2, // Thickness of the rectangle border
                    IsAntialias = true,
                })
                {
                    canvas.DrawRect(rect, paint);
                }
            }
        }

        SentenceBitmap = skBitmap.ToAvaloniaBitmap();
    }

    public static SKBitmap RemoveTopLines(SKBitmap original, int linesToRemove)
    {
        if (linesToRemove <= 0 || linesToRemove >= original.Height)
        {
            return original.Copy();
        }

        int newHeight = original.Height - linesToRemove;
        var newBitmap = new SKBitmap(original.Width, newHeight);

        using (var canvas = new SKCanvas(newBitmap))
        {
            var sourceRect = new SKRect(0, linesToRemove, original.Width, original.Height);
            var destRect = new SKRect(0, 0, original.Width, newHeight);
            canvas.DrawBitmap(original, sourceRect, destRect);
        }

        return newBitmap;
    }

    [RelayCommand]
    private void Shrink()
    {
        if (_expandCount <= 0)
        {
            return;
        }

        _expandCount--;
        SetImages(_item, _nBmp);
        UpdateShrintExpand();
    }

    [RelayCommand]
    private void Expand()
    {
        if (_startFromNumber + _expandCount < _letters.Count - 1 && _letters[_startFromNumber + _expandCount + 1].NikseBitmap == null)
        {
            return;
        }

        _expandCount++;
        SetImages(_item, _nBmp);
        UpdateShrintExpand();
    }

    [RelayCommand]
    private void Ok()
    {
        if (BinaryOcrBitmap == null || _db.FindExactMatch(BinaryOcrBitmap) >= 0)
        {
            Close();
            return;
        }

        BinaryOcrBitmap.Text = NewText;
        BinaryOcrBitmap.Italic = IsNewTextItalic;
        OkPressed = true;
        SaveSettings();
        Close();
    }

    [RelayCommand]
    private void InspectAdditions()
    {
        InspectHistoryPressed = true;
        SaveSettings();
        Close();
    }

    [RelayCommand]
    private void UseOnce()
    {
        UseOncePressed = true;
        Close();
    }

    [RelayCommand]
    private void Skip()
    {
        SkipPressed = true;
        Close();
    }

    [RelayCommand]
    private void Abort()
    {
        AbortPressed = true;
        Close();
    }

    [RelayCommand]
    private void InsertSpecialCharacter(object parameter)
    {
        if (parameter is string str)
        {
            var selectionStart = TextBoxNew.SelectionStart;
            NewText = NewText.Insert(selectionStart, str);
            Dispatcher.UIThread.Post(() =>
            {
                TextBoxNew.SelectionStart = selectionStart + str.Length;
                TextBoxNew.SelectionEnd = TextBoxNew.SelectionStart;
                TextBoxNew.Focus();
            });
        }   
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    private void UpdateShrintExpand()
    {
        CanExpand = _startFromNumber + _expandCount < _letters.Count - 1 && _letters[_startFromNumber + _expandCount + 1].NikseBitmap != null;
        CanShrink = _expandCount > 0;
    }

    private void SetTitle()
    {
        Title = $"Add Binary OCR character for line  {_startFromNumber + 1}, character {_letters.IndexOf(_splitItem) + 1} of {_letters.Count} using database \"{Path.GetFileNameWithoutExtension(_db.FileName)}\"";
    }

    internal void TextBoxNewOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(TextBoxNew.Text))
        {
            e.Handled = true;
            Ok();
        }
    }

    internal void TextBoxNewOnKeyUp(object? sender, KeyEventArgs e)
    {
        if (SubmitOnFirstLetter && NewText.Length >= 1)
        {
            e.Handled = true;
            Ok();
        }
    }

    public void KeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Abort();
        }
        else if (e.Key == Key.Left)
        {
            if (CanShrink)
            {
                Shrink();
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            if (CanExpand)
            {
                Expand();
            }
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

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        UiUtil.RestoreWindowPosition(Window);
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        UiUtil.SaveWindowPosition(Window);
    }

    internal void TextBoxMacPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (OperatingSystem.IsMacOS() &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            sender is Control control)
        {
            var args = new ContextRequestedEventArgs(e);
            control.RaiseEvent(args);
            e.Handled = args.Handled;
        }
    }
}