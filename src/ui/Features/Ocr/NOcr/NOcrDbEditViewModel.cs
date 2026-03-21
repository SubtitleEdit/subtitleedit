using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

public partial class NOcrDbEditViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _characters;
    [ObservableProperty] private string? _selectedCharacter;
    [ObservableProperty] private ObservableCollection<NOcrChar> _currentCharacterItems;
    [ObservableProperty] private NOcrChar? _selectedCurrentCharacterItem;
    [ObservableProperty] private bool _isNewLinesForegroundActive;
    [ObservableProperty] private bool _isNewLinesBackgroundActive;
    [ObservableProperty] private string _itemText;
    [ObservableProperty] private bool _isItemItalic;
    [ObservableProperty] private string _databaseName;
    [ObservableProperty] private string _resolutionAndTopMargin;
    [ObservableProperty] private string _zoomFactorInfo;
    [ObservableProperty] private string _expandInfo;
    [ObservableProperty] private string _title;

    public Window? Window { get; set; }
    public NOcrDrawingCanvasView NOcrDrawingCanvas { get; set; }
    public TextBox TextBoxItem { get; set; }

    public bool OkPressed { get; set; }
    private NOcrDb _nOcrDb;
    private bool _isControlDown;

    public NOcrDbEditViewModel()
    {
        IsNewLinesForegroundActive = true;
        IsNewLinesBackgroundActive = false;
        DatabaseName = string.Empty;
        Characters = new ObservableCollection<string>();
        CurrentCharacterItems = new ObservableCollection<NOcrChar>();
        ItemText = string.Empty;
        IsItemItalic = false;
        _nOcrDb = new NOcrDb(string.Empty);
        NOcrDrawingCanvas = new NOcrDrawingCanvasView();
        TextBoxItem = new TextBox();
        ResolutionAndTopMargin = string.Empty;
        ZoomFactorInfo = string.Empty;
        ExpandInfo = string.Empty;
        NOcrDrawingCanvas.ZoomFactor = Se.Settings.Ocr.NOcrZoomFactor;
        Title = string.Empty;
    }

    [RelayCommand]
    private void Ok()
    {
        if (string.IsNullOrWhiteSpace(DatabaseName))
        {
            return;
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
    private void DeleteCurrentItem()
    {
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
    private async Task Update()
    {
        var item = SelectedCurrentCharacterItem;
        if (item == null)
        {
            return;
        }

        // Validate the item text
        if (string.IsNullOrWhiteSpace(ItemText))
        {
            await MessageBox.Show(Window!, "Validation Error", "Item text cannot be empty.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        item.Text = ItemText;
        item.Italic = IsItemItalic;
        _nOcrDb.Save();

        Close();
    }

    [RelayCommand]
    private async Task Delete()
    {
        var item = SelectedCurrentCharacterItem;
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


    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
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

    internal void TextBoxDatabaseNameKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
    }

    internal void Initialize(NOcrDb nOcrDb)
    {
        _nOcrDb = nOcrDb;
        var characters = nOcrDb.OcrCharactersCombined.Where(c => !string.IsNullOrWhiteSpace(c.Text))
            .Select(c => c.Text)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        Characters.AddRange(characters);

        SelectedCharacter = characters.FirstOrDefault();
        CharactersChanged();

        Title = string.Format(Se.Language.Ocr.EditNOcrDatabaseXWithYItems, System.IO.Path.GetFileNameWithoutExtension(nOcrDb.FileName), nOcrDb.OcrCharactersCombined.Count);
    }

    internal void CharactersChanged(object? sender, SelectionChangedEventArgs e)
    {
        CharactersChanged();
    }

    internal void CharactersChanged()
    {
        CurrentCharacterItems.Clear();
        var selectedCharacter = SelectedCharacter;
        if (string.IsNullOrWhiteSpace(selectedCharacter))
        {
            return;
        }

        var items = _nOcrDb.OcrCharactersCombined.Where(c => c.Text == selectedCharacter).ToList();
        CurrentCharacterItems.AddRange(items);
        SelectedCurrentCharacterItem = CurrentCharacterItems.FirstOrDefault();
    }

    internal void CurrentCharacterItemsChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedItem = SelectedCurrentCharacterItem;
        if (selectedItem == null)
        {
            ItemText = string.Empty;
            IsItemItalic = false;
            ResolutionAndTopMargin = string.Empty;
            ExpandInfo = string.Empty;
            return;
        }

        ItemText = selectedItem.Text;
        IsItemItalic = selectedItem.Italic;
        ResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, selectedItem.Width, selectedItem.Height, selectedItem.MarginTop);

        if (selectedItem.ExpandCount == 0)
        {
            ExpandInfo = string.Empty;
        }
        else
        {
            ExpandInfo = string.Format(Se.Language.Ocr.ExpandInfoX, selectedItem.ExpandCount);
        }

        var skBitmap = new SKBitmap(selectedItem.Width, selectedItem.Height);
        using (var canvas = new SKCanvas(skBitmap))
        {
            canvas.Clear(SKColors.LightGray);
        }
        NOcrDrawingCanvas.SetBackgroundImage(skBitmap.ToAvaloniaBitmap());

        NOcrDrawingCanvas.SetStrokeWidth(1);
        NOcrDrawingCanvas.MissPaths.Clear();
        NOcrDrawingCanvas.MissPaths.AddRange(selectedItem.LinesBackground);
        NOcrDrawingCanvas.HitPaths.Clear();
        NOcrDrawingCanvas.HitPaths.AddRange(selectedItem.LinesForeground);
        NOcrDrawingCanvas.InvalidateVisual();
        ZoomFactorInfo = string.Format(Se.Language.Ocr.ZoomFactorX, NOcrDrawingCanvas.ZoomFactor);
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
