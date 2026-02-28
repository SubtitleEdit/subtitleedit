using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

public partial class BinaryOcrDbEditViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _characters;
    [ObservableProperty] private string? _selectedCharacter;
    [ObservableProperty] private ObservableCollection<BinaryOcrBitmap> _currentCharacterItems;
    [ObservableProperty] private BinaryOcrBitmap? _selectedCurrentCharacterItem;
    [ObservableProperty] private Bitmap? _itemBitmap;
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
    public TextBox TextBoxItem { get; set; }

    public bool OkPressed { get; set; }
    private BinaryOcrDb _binaryImageCompareDatabase;

    public BinaryOcrDbEditViewModel()
    {
        IsNewLinesForegroundActive = true;
        IsNewLinesBackgroundActive = false;
        DatabaseName = string.Empty;
        Characters = new ObservableCollection<string>();
        CurrentCharacterItems = new ObservableCollection<BinaryOcrBitmap>();
        ItemText = string.Empty;
        IsItemItalic = false;
        TextBoxItem = new TextBox();
        ResolutionAndTopMargin = string.Empty;
        ZoomFactorInfo = string.Empty;
        ExpandInfo = string.Empty;
        ItemBitmap = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        Title = string.Empty;
        _binaryImageCompareDatabase = new BinaryOcrDb(string.Empty, false);
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

        _binaryImageCompareDatabase.Save();

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
                   "Delete \"Binary image compare\" item?",
                   $"Do you want to delete the current \"Binary image compare\" item?",
                   MessageBoxButtons.YesNoCancel,
                   MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        _binaryImageCompareDatabase.CompareImages.Remove(item);
        _binaryImageCompareDatabase.CompareImagesExpanded.Remove(item);
        _binaryImageCompareDatabase.Save();
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
    }

    internal void Initialize(string imageCompareName)
    {
        _binaryImageCompareDatabase = new BinaryOcrDb(Path.Combine(Se.OcrFolder, imageCompareName + BinaryOcrDb.Extension), true);
        var allImages = _binaryImageCompareDatabase.AllCompareImages;
        var characters = allImages.Where(c => !string.IsNullOrWhiteSpace(c.Text))
             .Select(c => c.Text)
             .Distinct()
             .OrderBy(c => c)
             .ToList();
        foreach (var s in characters)
        {
            if (!string.IsNullOrEmpty(s))
            {
                Characters.Add(s);
            }
        }

        SelectedCharacter = characters.FirstOrDefault();
        CharactersChanged();

        Title = string.Format(Se.Language.Ocr.EditNOcrDatabaseXWithYItems, imageCompareName, allImages.Count);
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

        var items = _binaryImageCompareDatabase.AllCompareImages.Where(c => c.Text == selectedCharacter).ToList();
        foreach (var item in items)
        {
            CurrentCharacterItems.Add(item);
        }
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

        ItemText = selectedItem.Text ?? string.Empty;
        IsItemItalic = selectedItem.Italic;
        ItemBitmap = selectedItem.ToSKBitmap().ToAvaloniaBitmap();
        ResolutionAndTopMargin = string.Format(Se.Language.Ocr.ResolutionXYAndTopmarginZ, selectedItem.Width, selectedItem.Height, selectedItem.Y);

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
    }
}
