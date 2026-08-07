using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.WebVtt;

/// <summary>
/// Check-list of WebVTT styles. Used for the three places a subset of styles is chosen:
/// setting styles on the selected lines, picking which styles to import, and picking which
/// styles to export.
/// </summary>
public partial class WebVttStylePickerViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _buttonAcceptText;
    [ObservableProperty] private ObservableCollection<WebVttStyleDisplay> _styles;
    [ObservableProperty] private WebVttStyleDisplay? _selectedStyle;
    [ObservableProperty] private string _selectedStyleCss;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public List<WebVttStyleDisplay> CheckedStyles => Styles.Where(p => p.IsSelected).ToList();

    public WebVttStylePickerViewModel()
    {
        _title = string.Empty;
        _buttonAcceptText = string.Empty;
        _selectedStyleCss = string.Empty;
        _styles = new ObservableCollection<WebVttStyleDisplay>();
    }

    public void Initialize(string title, string buttonAcceptText, List<WebVttStyleDisplay> styles)
    {
        Title = title;
        ButtonAcceptText = buttonAcceptText;
        Styles.Clear();
        Styles.AddRange(styles);

        if (Styles.Count > 0)
        {
            SelectedStyle = Styles[0];
        }
    }

    partial void OnSelectedStyleChanged(WebVttStyleDisplay? value)
    {
        SelectedStyleCss = value == null ? string.Empty : value.Css.Replace("; ", ";" + System.Environment.NewLine);
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var style in Styles)
        {
            style.IsSelected = true;
        }
    }

    [RelayCommand]
    private void InvertSelection()
    {
        foreach (var style in Styles)
        {
            style.IsSelected = !style.IsSelected;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => Window?.Close());
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/webvtt-styles");
        }
    }
}
