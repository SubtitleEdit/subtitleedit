using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Assa;

public partial class AssaSingleStyleViewModel : ObservableObject
{
    [ObservableProperty] private StyleDisplay? _currentStyle;
    [ObservableProperty] private ObservableCollection<string> _fonts;
    [ObservableProperty] private ObservableCollection<BorderStyleItem> _borderTypes;
    [ObservableProperty] private BorderStyleItem _selectedBorderType;
    [ObservableProperty] private Bitmap? _imagePreview;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AssaSingleStyleViewModel()
    {
        Fonts = new ObservableCollection<string>(FontHelper.GetLibAssaFonts());
        BorderTypes = new ObservableCollection<BorderStyleItem>(BorderStyleItem.List());
        SelectedBorderType = BorderTypes[0];
    }

    public void Initialize(SsaStyle style)
    {
        CurrentStyle = new StyleDisplay(style);
    }

    /// <summary>
    /// The font combo box binds SelectedItem to CurrentStyle.FontName; a font missing from
    /// the item list would make Avalonia clear the selection and null out the style's font.
    /// Make sure the font is listed before the style becomes current (#13101).
    /// </summary>
    partial void OnCurrentStyleChanging(StyleDisplay? value)
    {
        var fontName = value?.FontName;
        if (!string.IsNullOrEmpty(fontName) && !Fonts.Contains(fontName))
        {
            Fonts.Insert(0, fontName);
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

    internal void BorderTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
    }
}