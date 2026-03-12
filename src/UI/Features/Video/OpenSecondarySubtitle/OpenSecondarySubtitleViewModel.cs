using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public partial class OpenSecondarySubtitleViewModel : ObservableObject
{
    [ObservableProperty] private bool _alignmentAn1;
    [ObservableProperty] private bool _alignmentAn2;
    [ObservableProperty] private bool _alignmentAn3;
    [ObservableProperty] private bool _alignmentAn4;
    [ObservableProperty] private bool _alignmentAn5;
    [ObservableProperty] private bool _alignmentAn6;
    [ObservableProperty] private bool _alignmentAn7;
    [ObservableProperty] private bool _alignmentAn8;
    [ObservableProperty] private bool _alignmentAn9;
    [ObservableProperty] private Color _subtitleColor;
    [ObservableProperty] private BorderStyleItem _selectedBorderStyle;
    [ObservableProperty] private decimal _fontSize;

    public ObservableCollection<BorderStyleItem> BorderStyles { get; } = new(BorderStyleItem.List());

    private Subtitle _secondarySubtitle = new Subtitle();
    private readonly IWindowService _windowService;

    public Subtitle? ResultSubtitle { get; private set; }
    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public OpenSecondarySubtitleViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        _alignmentAn2 = true;
        _subtitleColor = Colors.White;
        _fontSize = 20;
        _selectedBorderStyle = BorderStyles[0];
    }

    [RelayCommand]
    private async Task ChooseColor()
    {
        if (Window == null)
        {
            return;
        }

        var vm = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(
            Window, viewModel => { viewModel.SelectedColor = SubtitleColor; });

        if (vm.OkPressed)
        {
            SubtitleColor = vm.SelectedColor;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        ResultSubtitle = BuildAssaSubtitle();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public void Initialize(Subtitle secondarySubtitle)
    {
        _secondarySubtitle = secondarySubtitle;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    private string GetAlignment()
    {
        if (AlignmentAn1) return "1";
        if (AlignmentAn3) return "3";
        if (AlignmentAn4) return "4";
        if (AlignmentAn5) return "5";
        if (AlignmentAn6) return "6";
        if (AlignmentAn7) return "7";
        if (AlignmentAn8) return "8";
        if (AlignmentAn9) return "9";
        return "2";
    }

    private Subtitle BuildAssaSubtitle()
    {
        var style = new SsaStyle
        {
            Name = "Default",
            FontName = "Arial",
            FontSize = FontSize,
            Primary = SubtitleColor.ToSKColor(),
            Outline = Colors.Black.ToSKColor(),
            Background = Colors.Black.ToSKColor(),
            Secondary = Colors.Yellow.ToSKColor(),
            Alignment = GetAlignment(),
            OutlineWidth = 2,
            ShadowWidth = 1,
            MarginLeft = 10,
            MarginRight = 10,
            MarginVertical = 10,
            BorderStyle = ((int)SelectedBorderStyle.Style).ToString(),
            ScaleX = 100,
            ScaleY = 100,
        };

        var format = new AdvancedSubStationAlpha();
        var tempSub = new Subtitle();
        var tempText = format.ToText(tempSub, string.Empty);
        var tempLines = tempText.SplitToLines();
        format.LoadSubtitle(tempSub, tempLines, string.Empty);

        var header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            tempSub.Header,
            new List<SsaStyle> { style });

        var result = new Subtitle(_secondarySubtitle);
        result.Header = header;

        return result;
    }
}