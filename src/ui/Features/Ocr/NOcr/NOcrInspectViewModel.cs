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
using Nikse.SubtitleEdit.Features.Ocr.NOcr;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class NOcrInspectViewModel : ObservableObject
{
    public NOcrInspectWindow? Window { get; set; }

    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<NOcrLine> _linesForeground;
    [ObservableProperty] private NOcrLine? _selectedLineForeground;
    [ObservableProperty] private ObservableCollection<NOcrLine> _linesBackground;
    [ObservableProperty] private NOcrLine? _selectedLineBackground;
    [ObservableProperty] private bool _isNewLinesForegroundActive;
    [ObservableProperty] private bool _isNewLinesBackgroundActive;
    [ObservableProperty] private string _newText;
    [ObservableProperty] private string _resolutionAndTopMargin;
    [ObservableProperty] private string _matchResolutionAndTopMargin;
    [ObservableProperty] private string _zoomFactorInfo;
    [ObservableProperty] private bool _isNewTextItalic;
    [ObservableProperty] private bool _submitOnFirstLetter;
    [ObservableProperty] private Bitmap? _sentenceImageSource;
    [ObservableProperty] private Bitmap? _itemImageSource;
    [ObservableProperty] private bool _isEditControlsEnabled;
    [ObservableProperty] private ObservableCollection<int> _noOfLinesToAutoDrawList;
    [ObservableProperty] private int _selectedNoOfLinesToAutoDraw;
    [ObservableProperty] private Bitmap _currentBitmap;
    [ObservableProperty] private Bitmap _sentenceBitmap;

    private List<ImageSplitterItem2> _letters;
    private List<NOcrChar?> _matches;
    private ImageSplitterItem2 _splitItem;
    public NOcrChar NOcrChar { get; private set; }
    public NOcrDrawingCanvasView NOcrDrawingCanvas { get; set; }
    public StackPanel PanelLines { get; set; }
    public TextBox TextBoxNew { get; set; }
    public bool OkPressed { get; set; }
    public bool AddBetterMatchPressed { get; set; }
    public int LetterIndex { get; internal set; }

    private SKBitmap _sentenceBitmapOriginal;
    private NOcrDb _nOcrDb;
    private bool _isControlDown = false;
    private bool _isWinDown = false;

    public NOcrInspectViewModel()
    {
        Title = Se.Language.Ocr.InspectImageMatches;
        LinesForeground = new ObservableCollection<NOcrLine>();
        LinesBackground = new ObservableCollection<NOcrLine>();
        PanelLines = new StackPanel();
        IsNewLinesForegroundActive = true;
        IsNewLinesBackgroundActive = false;
        NewText = string.Empty;
        ResolutionAndTopMargin = string.Empty;
        MatchResolutionAndTopMargin = string.Empty;
        IsNewTextItalic = false;
        SubmitOnFirstLetter = false;
        _letters = new List<ImageSplitterItem2>();
        _matches = new List<NOcrChar?>();
        _sentenceBitmapOriginal = new SKBitmap(1, 1, true);
        ZoomFactorInfo = string.Empty;

        const int maxLines = 500;
        NoOfLinesToAutoDrawList = new ObservableCollection<int>();
        for (var i = 10; i <= maxLines; i++)
        {
            NoOfLinesToAutoDrawList.Add(i);
        }

        SelectedNoOfLinesToAutoDraw = Se.Settings.Ocr.NOcrNoOfLinesToAutoDraw;
        NOcrChar = new NOcrChar();
        _nOcrDb = new NOcrDb(string.Empty);
        SentenceBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        CurrentBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        _splitItem = new ImageSplitterItem2(string.Empty);
        NOcrDrawingCanvas = new NOcrDrawingCanvasView();
        TextBoxNew = new TextBox();
    }

    internal void Initialize(SKBitmap sKBitmap, OcrSubtitleItem? selectedOcrSubtitleItem, NOcrDb? nOcrDb, int selectedNOcrMaxWrongPixels, List<ImageSplitterItem2> letters, List<NOcrChar?> matches)
    {
        _letters = letters;
        _matches = matches;
        _nOcrDb = nOcrDb ?? new NOcrDb(string.Empty);
        _sentenceBitmapOriginal = sKBitmap;
        NOcrDrawingCanvas.BackgroundImage = CurrentBitmap;
        NOcrDrawingCanvas.ZoomFactor = Se.Settings.Ocr.NOcrZoomFactor;

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
        if (_splitItem.NikseBitmap != null)
        {
            var rect = new SKRect(_splitItem.X, _splitItem.Y, _splitItem.X + _splitItem.NikseBitmap.Width, _splitItem.Y + _splitItem.NikseBitmap.Height);
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
        NOcrChar.Text = NewText;
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
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
    private void Update()
    {
        var item = NOcrChar;
        if (item == null || string.IsNullOrEmpty(NewText))
        {
            return;
        }

        item.Text = NewText;
        item.Italic = IsNewTextItalic;
        _nOcrDb.Save();

        Close();
    }

    [RelayCommand]
    private async Task Delete()
    {
        var item = NOcrChar;
        if (item == null)
        {
            return;
        }

        var answer = await MessageBox.Show(
                   Window!,
                   "Delete nOCR item?",
                   $"Do you want to delete the current nOCR item?",
                   MessageBoxButtons.YesNoCancel,
                   MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        _nOcrDb.OcrCharacters.Remove(item);
        _nOcrDb.OcrCharactersExpanded.Remove(item);
        _nOcrDb.Save();

        Close();
    }

    [RelayCommand]
    private void AddBetterMatch()
    {
        NOcrChar.Text = NewText;
        AddBetterMatchPressed = true;
        Close();
    }

    [RelayCommand]
    private void DrawAgain()
    {
        NOcrChar.LinesForeground.Clear();
        NOcrChar.LinesBackground.Clear();
        NOcrChar.GenerateLineSegments(SelectedNoOfLinesToAutoDraw, false, NOcrChar, _splitItem.NikseBitmap!);
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

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    private void ShowOcrPoints()
    {
        NOcrDrawingCanvas.MissPaths.Clear();
        NOcrDrawingCanvas.HitPaths.Clear();

        NOcrDrawingCanvas.MissPaths.AddRange(NOcrChar.LinesBackground);
        NOcrDrawingCanvas.HitPaths.AddRange(NOcrChar.LinesForeground);
        NOcrDrawingCanvas.InvalidateVisual();
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
        WrapPanel currentLine = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(2)
        };
        PanelLines.Children.Add(currentLine);

        for (var i = 0; i < _matches.Count; i++)
        {
            NOcrChar? match = _matches[i];
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

    private void OnLetterClicked(int index, NOcrChar? match)
    {
        LetterIndex = index;
        _splitItem = _letters[index];

        IsEditControlsEnabled = match != null;

        if (match != null)
        {
            MatchResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, match.Width, match.Height, match.MarginTop);
        }
        else
        {
            MatchResolutionAndTopMargin = string.Empty;
        }

        if (_splitItem.NikseBitmap != null)
        {
            NOcrChar = match ?? new NOcrChar
            {
                Width = _splitItem.NikseBitmap.Width,
                Height = _splitItem.NikseBitmap.Height,
                MarginTop = 0
            };

            NewText = match?.Text ?? string.Empty;
            IsNewTextItalic = match is { Italic: true };
            ResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, _splitItem.NikseBitmap.Width, _splitItem.NikseBitmap.Height, _splitItem.Top);

            CurrentBitmap = _splitItem.NikseBitmap!.GetBitmap().ToAvaloniaBitmap();
            NOcrDrawingCanvas.BackgroundImage = CurrentBitmap;

            if (match == null)
            {
                NOcrDrawingCanvas.BackgroundImage = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
                NOcrDrawingCanvas.HitPaths.Clear();
                NOcrDrawingCanvas.MissPaths.Clear();
            }
            else
            {
                NOcrDrawingCanvas.HitPaths.Clear();
                foreach (var line in match.LinesForeground)
                {
                    NOcrDrawingCanvas.HitPaths.Add(new NOcrLine(
                        line.GetScaledStart(match, NOcrChar.Width, NOcrChar.Height),
                        line.GetScaledEnd(match, NOcrChar.Width, NOcrChar.Height)));
                }

                NOcrDrawingCanvas.MissPaths.Clear();
                foreach (var line in match.LinesBackground)
                {
                    NOcrDrawingCanvas.MissPaths.Add(new NOcrLine(
                        line.GetScaledStart(match, NOcrChar.Width, NOcrChar.Height),
                        line.GetScaledEnd(match, NOcrChar.Width, NOcrChar.Height)));
                }
            }

            NOcrDrawingCanvas.InvalidateVisual();
            NOcrDrawingCanvas.ZoomFactor = Se.Settings.Ocr.NOcrZoomFactor; 
            ZoomFactorInfo = string.Format(Se.Language.Ocr.ZoomFactorX, NOcrDrawingCanvas.ZoomFactor);
        }
        else
        {
            NOcrDrawingCanvas.HitPaths.Clear();
            NOcrDrawingCanvas.MissPaths.Clear();
            NOcrDrawingCanvas.InvalidateVisual();
        }

        InitSentenceBitmap();
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
                Cancel();
            }
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
}
