using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class BinaryOcrCharacterHistoryViewModel : ObservableObject
{
    public Window? Window { get; set; }

    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<BinaryOcrAddHistoryItem> _historyItems;
    [ObservableProperty] private BinaryOcrAddHistoryItem? _selectedHistoryItem;
    [ObservableProperty] private string _newText;
    [ObservableProperty] private string _lineIndex;
    [ObservableProperty] private string _resolutionAndTopMargin;
    [ObservableProperty] private string _zoomFactorInfo;
    [ObservableProperty] private bool _isNewTextItalic;
    [ObservableProperty] private Bitmap _currentBitmap;

    public BinaryOcrBitmap? BinaryOcrBitmap { get; private set; }
    public TextBox TextBoxNew { get; set; }
    public bool OkPressed { get; set; }
    private BinaryOcrDb _db;
    private bool _isControlDown;

    public BinaryOcrCharacterHistoryViewModel()
    {
        Title = "Inspect Binary OCR Additions";
        NewText = string.Empty;
        ResolutionAndTopMargin = string.Empty;
        IsNewTextItalic = false;
        ZoomFactorInfo = string.Empty;
        LineIndex = string.Empty;

        HistoryItems = new ObservableCollection<BinaryOcrAddHistoryItem>();
        BinaryOcrBitmap = null;
        _db = new BinaryOcrDb(string.Empty);
        CurrentBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        TextBoxNew = new TextBox();
    }

    public void Initialize(BinaryOcrDb binaryOcrDb, BinaryOcrAddHistoryManager addHistoryManager)
    {
        _db = binaryOcrDb;
        addHistoryManager.ClearNotInOcrDb(binaryOcrDb);
        foreach (var historyItem in addHistoryManager.Items.OrderByDescending(p => p.DateTime))
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
        // Binary OCR doesn't use zoom
    }

    [RelayCommand]
    private void ZoomOut()
    {
        // Binary OCR doesn't use zoom
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
    }

    [RelayCommand]
    private void UpdateAndClose()
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

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
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
        }
    }

    public void KeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlDown = false;
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

        BinaryOcrBitmap = selectedItem.BinaryOcrBitmap;
        IsNewTextItalic = BinaryOcrBitmap.Italic;
        NewText = BinaryOcrBitmap.Text ?? string.Empty;
        LineIndex = string.Format(Se.Language.Ocr.LineIndexX, selectedItem.LineIndex + 1);
        ResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, BinaryOcrBitmap.Width, BinaryOcrBitmap.Height, BinaryOcrBitmap.Y);
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