using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Ocr.NOcr;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class NOcrCharacterAddViewModel : ObservableObject
{
    public NOcrCharacterAddWindow? Window { get; set; }

    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<NOcrLine> _linesForeground;
    [ObservableProperty] private NOcrLine? _selectedLineForeground;
    [ObservableProperty] private ObservableCollection<NOcrLine> _linesBackground;
    [ObservableProperty] private NOcrLine? _selectedLineBackground;
    [ObservableProperty] private bool _isNewLinesForegroundActive;
    [ObservableProperty] private bool _isNewLinesBackgroundActive;
    [ObservableProperty] private string _newText;
    [ObservableProperty] private string _resolutionAndTopMargin;
    [ObservableProperty] private string _zoomFactorInfo;
    [ObservableProperty] private bool _isNewTextItalic;
    [ObservableProperty] private bool _submitOnFirstLetter;
    [ObservableProperty] private Bitmap? _sentenceImageSource;
    [ObservableProperty] private Bitmap? _itemImageSource;
    [ObservableProperty] private bool _canShrink;
    [ObservableProperty] private bool _canExpand;
    [ObservableProperty] private bool _showUseOnce;
    [ObservableProperty] private bool _showSkip;
    [ObservableProperty] private bool _isInspectAdditionsVisible;
    [ObservableProperty] private ObservableCollection<int> _noOfLinesToAutoDrawList;
    [ObservableProperty] private int _selectedNoOfLinesToAutoDraw;
    [ObservableProperty] private Bitmap _sentenceBitmap;
    [ObservableProperty] private Bitmap _currentBitmap;

    private List<ImageSplitterItem2> _letters;
    private ImageSplitterItem2 _splitItem;

    public NOcrChar NOcrChar { get; private set; }
    public NOcrDrawingCanvasView NOcrDrawingCanvas { get; set; }
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
    private NOcrDb _nOcrDb;
    private bool _isControlDown;
    private bool _isWinDown;

    public NOcrCharacterAddViewModel()
    {
        Title = Se.Language.Ocr.AddNewCharcter;
        LinesForeground = new ObservableCollection<NOcrLine>();
        LinesBackground = new ObservableCollection<NOcrLine>();
        IsNewLinesForegroundActive = true;
        IsNewLinesBackgroundActive = false;
        NewText = string.Empty;
        ResolutionAndTopMargin = string.Empty;
        IsNewTextItalic = false;
        SubmitOnFirstLetter = false;
        _letters = new List<ImageSplitterItem2>();
        ZoomFactorInfo = string.Empty;

        const int maxLines = 500;
        NoOfLinesToAutoDrawList = new ObservableCollection<int>();
        for (var i = 10; i <= maxLines; i++)
        {
            NoOfLinesToAutoDrawList.Add(i);
        }

        SelectedNoOfLinesToAutoDraw = 100;
        NOcrChar = new NOcrChar();
        _nOcrDb = new NOcrDb(string.Empty);
        SentenceBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        CurrentBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        _splitItem = new ImageSplitterItem2(string.Empty);
        NOcrDrawingCanvas = new NOcrDrawingCanvasView();
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
        SelectedNoOfLinesToAutoDraw = Se.Settings.Ocr.NOcrNoOfLinesToAutoDraw;
    }

    private void SaveSettings()
    {
        Se.Settings.Ocr.IsNewLetterItalic = IsNewTextItalic;
        Se.Settings.Ocr.SubmitOnFirstLetter = SubmitOnFirstLetter;
        Se.Settings.Ocr.NOcrNoOfLinesToAutoDraw = SelectedNoOfLinesToAutoDraw;
        Se.SaveSettings();
    }

    public void Initialize(
        NikseBitmap2 nBmp,
        OcrSubtitleItem item,
        List<ImageSplitterItem2> letters,
        int i,
        NOcrDb nOcrDb,
        int maxWrongPixels,
        NOcrAddHistoryManager nOcrAddHistoryManager,
        bool showUseOnce,
        bool showSkip)
    {
        _nOcrDb = nOcrDb;
        _letters = letters;
        _startFromNumber = i;
        _nBmp = nBmp;
        _item = item;
        ShowSkip = showSkip;
        ShowUseOnce = showUseOnce;
        NOcrDrawingCanvas.ZoomFactor = Se.Settings.Ocr.NOcrZoomFactor;
        if (i >= 0 && i < letters.Count)
        {
            _splitItem = letters[i];
        }

        UpdateShrintExpand();
        SetImages(_item, _nBmp);
        SetTitle();
        IsInspectAdditionsVisible = nOcrAddHistoryManager.Items.Count > 0;
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
            NOcrChar = new NOcrChar
            {
                Width = _splitItem.NikseBitmap.Width,
                Height = _splitItem.NikseBitmap.Height,
                MarginTop = _splitItem.Top,
                ExpandCount = 0,
            };

            ResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, NOcrChar.Width, NOcrChar.Height, NOcrChar.MarginTop);
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

                NOcrChar = new NOcrChar
                {
                    Width = subset.Width,
                    Height = subset.Height,
                    MarginTop = 0,
                    ExpandCount = _expandCount + 1,
                };

                ResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, NOcrChar.Width, NOcrChar.Height, NOcrChar.MarginTop);
            }

            NOcrDrawingCanvas.BackgroundImage = CurrentBitmap;
            NOcrDrawingCanvas.ZoomFactor = NOcrDrawingCanvas.ZoomFactor;
            ZoomFactorInfo = string.Format(Se.Language.Ocr.ZoomFactorX, NOcrDrawingCanvas.ZoomFactor);
            DrawAgain();

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

    public async Task ShowDrawingTips()
    {
        await MessageBox.Show(
            Window!,
            Se.Language.General.Help,
            Se.Language.Ocr.NOcrDrawHelp,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
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
        NOcrChar.Text = NewText;
        NOcrChar.Italic = IsNewTextItalic;
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
    private void DrawAgain()
    {
        NOcrChar.LinesForeground.Clear();
        NOcrChar.LinesBackground.Clear();
        NOcrChar.GenerateLineSegments(SelectedNoOfLinesToAutoDraw, false, NOcrChar, new NikseBitmap2(CurrentBitmap.ToSkBitmap()));
        ShowOcrPoints();
    }

    [RelayCommand]
    private void ClearDraw()
    {
        NOcrChar.LinesForeground.Clear();
        NOcrChar.LinesBackground.Clear();
        ShowOcrPoints();
    }

    [RelayCommand]
    private void ClearDrawForeGround()
    {
        NOcrChar.LinesForeground.Clear();
        ShowOcrPoints();
    }

    [RelayCommand]
    private void ClearDrawBackground()
    {
        NOcrChar.LinesBackground.Clear();
        ShowOcrPoints();
    }

    [RelayCommand]
    private void ZoomIn()
    {
        if (NOcrDrawingCanvas.ZoomFactor < 20)
        {
            NOcrDrawingCanvas.ZoomFactor++;
            Se.Settings.Ocr.NOcrZoomFactor = (int)NOcrDrawingCanvas.ZoomFactor;
        }

        ZoomFactorInfo = string.Format(Se.Language.Ocr.ZoomFactorX, NOcrDrawingCanvas.ZoomFactor);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        if (NOcrDrawingCanvas.ZoomFactor > 1)
        {
            NOcrDrawingCanvas.ZoomFactor--;
            Se.Settings.Ocr.NOcrZoomFactor = (int)NOcrDrawingCanvas.ZoomFactor;
        }

        ZoomFactorInfo = string.Format(Se.Language.Ocr.ZoomFactorX, NOcrDrawingCanvas.ZoomFactor);
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

    private void ShowOcrPoints()
    {
        NOcrDrawingCanvas.MissPaths.Clear();
        NOcrDrawingCanvas.HitPaths.Clear();

        NOcrDrawingCanvas.MissPaths.AddRange(NOcrChar.LinesBackground);
        NOcrDrawingCanvas.HitPaths.AddRange(NOcrChar.LinesForeground);
        NOcrDrawingCanvas.InvalidateVisual();
    }

    private void SetTitle()
    {
        Title = $"Add nOCR character for line  {_startFromNumber + 1}, character {_letters.IndexOf(_splitItem) + 1} of {_letters.Count} using database \"{Path.GetFileNameWithoutExtension(_nOcrDb.FileName)}\"";
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

            if (NOcrDrawingCanvas.IsDrawing)
            {
                NOcrDrawingCanvas.AbortDraw();
            }
            else
            {
                Abort();
            }
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
        else if (e.Key == Key.Z)
        {
            if (_isControlDown || _isWinDown)
            {
                e.Handled = true;
                NOcrDrawingCanvas.UndoLastPath();
            }
        }
        else if (e.Key == Key.Y)
        {
            if (_isControlDown || _isWinDown)
            {
                e.Handled = true;
                NOcrDrawingCanvas.ReDoLastPath();
            }
        }
        else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlDown = true;
            NOcrDrawingCanvas.IsControlDown = _isControlDown;
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
            NOcrDrawingCanvas.IsControlDown = _isControlDown;
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

    internal void DrawModeChanged(object? sender, SelectionChangedEventArgs e)
    {
        NOcrDrawingCanvas.NewLinesAreHits = IsNewLinesForegroundActive;
    }

    internal void DrawModeForegroundChanged(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            NOcrDrawingCanvas.NewLinesAreHits = IsNewLinesForegroundActive;
            IsNewLinesBackgroundActive = !IsNewLinesForegroundActive;
        });
    }

    internal void DrawModeBackgroundChanged(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            IsNewLinesForegroundActive = !IsNewLinesBackgroundActive;
            NOcrDrawingCanvas.NewLinesAreHits = IsNewLinesForegroundActive;
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

    internal void PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_isControlDown || _isWinDown)
        {
            if (e.Delta.Y > 0)
            {
                ZoomIn();
                e.Handled = true;
            }
            else if (e.Delta.Y < 0)
            {
                ZoomOut();
                e.Handled = true;
            }
        }
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