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
using Nikse.SubtitleEdit.Logic.Ocr;
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
        for (var i = 10; i <= maxLines; i++)
        {
            NoOfLinesToAutoDrawList.Add(i);
        }

        SelectedNoOfLinesToAutoDraw = Se.Settings.Ocr.NOcrNoOfLinesToAutoDraw;
        BinaryOcrBitmap = null;
        _db = new BinaryOcrDb(string.Empty);
        SentenceBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        CurrentBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        _splitItem = new ImageSplitterItem2(string.Empty);
        TextBoxNew = new TextBox();
    }

    internal void Initialize(SKBitmap sKBitmap, OcrSubtitleItem? selectedOcrSubtitleItem, BinaryOcrDb db, int selectedNOcrMaxWrongPixels, List<ImageSplitterItem2> letters, List<BinaryOcrMatcher.CompareMatch?> matches)
    {
        _letters = letters;
        _matches = matches;
        _db = db;
        _sentenceBitmapOriginal = sKBitmap;

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
        var item = BinaryOcrBitmap;
        if (item == null || string.IsNullOrEmpty(NewText))
        {
            return;
        }

        item.Text = NewText;
        item.Italic = IsNewTextItalic;
        _db.Save();

        Close();
    }

    [RelayCommand]
    private async Task Delete()
    {
        var item = BinaryOcrBitmap;
        if (item == null)
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

        if (item.ExpandCount > 0)
        {
            _db.CompareImagesExpanded.Remove(item);
        }
        else
        {
            _db.CompareImages.Remove(item);
        }
        _db.Save();

        Close();
    }

    [RelayCommand]
    private void AddBetterMatch()
    {
        if (BinaryOcrBitmap != null)
        {
            BinaryOcrBitmap.Text = NewText;
        }
        AddBetterMatchPressed = true;
        Close();
    }

    [RelayCommand]
    private void DrawAgain()
    {
        // Binary OCR doesn't use drawing - images are compared pixel by pixel
    }

    [RelayCommand]
    private void ClearDraw()
    {
        // Binary OCR doesn't use drawing - images are compared pixel by pixel
    }

    [RelayCommand]
    private void ClearDrawForeGround()
    {
        // Binary OCR doesn't use drawing - images are compared pixel by pixel
    }

    [RelayCommand]
    private void ClearDrawBackground()
    {
        // Binary OCR doesn't use drawing - images are compared pixel by pixel
    }

    [RelayCommand]
    private void ZoomIn()
    {
        // Binary OCR inspect doesn't use zoom
    }

    [RelayCommand]
    private void ZoomOut()
    {
        // Binary OCR inspect doesn't use zoom
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

        IsEditControlsEnabled = match != null;

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
                MatchResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, dbBitmap.Width, dbBitmap.Height, dbBitmap.Y);
            }
            else
            {
                MatchResolutionAndTopMargin = string.Empty;
            }
        }
        else
        {
            MatchResolutionAndTopMargin = string.Empty;
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
            ResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, _splitItem.NikseBitmap.Width, _splitItem.NikseBitmap.Height, _splitItem.Top);

            CurrentBitmap = _splitItem.NikseBitmap!.GetBitmap().ToAvaloniaBitmap();
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
