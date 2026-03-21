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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class NOcrCharacterHistoryViewModel : ObservableObject
{
    public NOcrCharacterHistoryWindow? Window { get; set; }

    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<NOcrAddHistoryItem> _historyItems;
    [ObservableProperty] private NOcrAddHistoryItem? _selectedHistoryItem;
    [ObservableProperty] private string _newText;
    [ObservableProperty] private string _lineIndex;
    [ObservableProperty] private string _resolutionAndTopMargin;
    [ObservableProperty] private string _zoomFactorInfo;
    [ObservableProperty] private bool _isNewTextItalic;
    [ObservableProperty] private Bitmap _currentBitmap;

    public NOcrChar NOcrChar { get; private set; }
    public NOcrDrawingCanvasView NOcrDrawingCanvas { get; set; }
    public TextBox TextBoxNew { get; set; }
    public bool OkPressed { get; set; }
    private NOcrDb _nOcrDb;
    private bool _isControlDown;

    public NOcrCharacterHistoryViewModel()
    {
        Title = Se.Language.Ocr.InspectNOcrAdditions;
        NewText = string.Empty;
        ResolutionAndTopMargin = string.Empty;
        IsNewTextItalic = false;
        ZoomFactorInfo = string.Empty;
        LineIndex = string.Empty;

        HistoryItems = new ObservableCollection<NOcrAddHistoryItem>();
        NOcrChar = new NOcrChar();
        _nOcrDb = new NOcrDb(string.Empty);
        CurrentBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        NOcrDrawingCanvas = new NOcrDrawingCanvasView();
        TextBoxNew = new TextBox();
    }

    public void Initialize(NOcrDb nOcrDb, NOcrAddHistoryManager nOcrAddHistoryManager)
    {
        _nOcrDb = nOcrDb;
        NOcrDrawingCanvas.ZoomFactor = Se.Settings.Ocr.NOcrZoomFactor;
        nOcrAddHistoryManager.ClearNotInOcrDb(nOcrDb);
        foreach (var historyItem in nOcrAddHistoryManager.Items.OrderByDescending(p => p.DateTime))
        {
            HistoryItems.Add(historyItem);
        }

        SelectedHistoryItem = HistoryItems.FirstOrDefault();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Abort()
    {
        Close();
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
    private void Update()
    {
        var item = NOcrChar;
        if (string.IsNullOrEmpty(NewText))
        {
            return;
        }

        item.Text = NewText;
        item.Italic = IsNewTextItalic;
        _nOcrDb.Save();
    }

    [RelayCommand]
    private void UpdateAndClose()
    {
        var item = NOcrChar;
        if (string.IsNullOrEmpty(NewText))
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

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
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
        if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(TextBoxNew.Text))
        {
            Ok();
        }
        else if (e.Key == Key.Escape)
        {
            Abort();
        }
    }

    public void KeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Abort();
        }
        else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlDown = true;
            NOcrDrawingCanvas.IsControlDown = _isControlDown;
        }
    }

    public void KeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlDown = false;
            NOcrDrawingCanvas.IsControlDown = _isControlDown;
        }
    }

    public void ItalicCheckChanged(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBoxNew.FontStyle = IsNewTextItalic ? FontStyle.Italic : FontStyle.Normal;
        });
    }

    public void HistoryItemChanged(object? sender, SelectionChangedEventArgs e)
    {
        HistoryItemChanged();
    }

    private void HistoryItemChanged()
    {
        var selectedItem = SelectedHistoryItem;
        if (selectedItem == null)
        {
            return;
        }

        if (selectedItem.Bitmap != null)
        {
            CurrentBitmap = selectedItem.Bitmap.GetBitmap().ToAvaloniaBitmap();
        }

        NOcrChar = selectedItem.NOcrChar;
        IsNewTextItalic = NOcrChar.Italic;
        NewText = NOcrChar.Text;
        LineIndex = string.Format(Se.Language.Ocr.LineIndexX, selectedItem.LineIndex + 1);
        ResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, NOcrChar.Width, NOcrChar.Height, NOcrChar.MarginTop);

        NOcrDrawingCanvas.BackgroundImage = CurrentBitmap;
        NOcrDrawingCanvas.ZoomFactor = NOcrDrawingCanvas.ZoomFactor;
        ZoomFactorInfo = string.Format(Se.Language.Ocr.ZoomFactorX, NOcrDrawingCanvas.ZoomFactor);

        ShowOcrPoints();
    }

    internal void PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_isControlDown)
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