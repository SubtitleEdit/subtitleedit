using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeFormatting;

public partial class ChangeFormattingViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ChangeFormattingDisplayItem> _subtitles;
    [ObservableProperty] private ChangeFormattingDisplayItem? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<ChangeFormattingTypeDisplay> _fromTypes;
    [ObservableProperty] private ChangeFormattingTypeDisplay? _selectedFromType;
    [ObservableProperty] private ObservableCollection<ChangeFormattingTypeDisplay> _toTypes;
    [ObservableProperty] private ChangeFormattingTypeDisplay? _selectedToType;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private Color _selectedColor;
    [ObservableProperty] private bool _isColorVisible;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _allSubtitles;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public List<SubtitleLineViewModel> FixedSubtitle { get; set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private SubtitleFormat _format = new SubRip();

    public ChangeFormattingViewModel()
    {
        Subtitles = new ObservableCollection<ChangeFormattingDisplayItem>();
        FromTypes = new ObservableCollection<ChangeFormattingTypeDisplay>(ChangeFormattingTypeDisplay.GetFromTypes());
        ToTypes = new ObservableCollection<ChangeFormattingTypeDisplay>(ChangeFormattingTypeDisplay.GetToTypes());
        AllSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        FixedSubtitle = new List<SubtitleLineViewModel>();
        StatusText = string.Empty;

        LoadSettings();

        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.Elapsed += (s, e) =>
        {
            _timerUpdatePreview.Stop();
            if (_dirty)
            {
                _dirty = false;
                UpdatePreview();
            }
            _timerUpdatePreview.Start();
        };
    }

    private void UpdatePreview()
    {
        if (SelectedFromType == null || SelectedToType == null)
        {
            return;
        }

        var allSubtitles = new ObservableCollection<SubtitleLineViewModel>(AllSubtitles.Select(p => new SubtitleLineViewModel(p)));
        var fixedCount = 0;
        Dispatcher.UIThread.Post(() =>
        {
            Subtitles.Clear();
            foreach (var v in allSubtitles)
            {
                var vm = new ChangeFormattingDisplayItem(v);
                vm.NewText = FormattingReplacer.Replace(v.Text, SelectedFromType.Type, SelectedToType.Type, SelectedColor, _format);
                if (vm.Text != vm.NewText)
                {
                    vm.SubtitleLineViewModel.Text = vm.NewText;
                    fixedCount++;
                }
                Subtitles.Add(vm);
            }

            StatusText = string.Format(Se.Language.General.LinesChangedX, fixedCount);
        });
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles, SubtitleFormat format)
    {
        AllSubtitles.Clear();
        AllSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));
        _dirty = true;
        _format = format;
        _timerUpdatePreview.Start();
    }

    private void LoadSettings()
    {
        SelectedFromType = FromTypes.FirstOrDefault(p => p.Type.ToString() == Se.Settings.Tools.ChangeFormatting.LastFromType) ?? FromTypes.First();
        SelectedToType = ToTypes.FirstOrDefault(p => p.Type.ToString() == Se.Settings.Tools.ChangeFormatting.LastToType) ?? ToTypes.First();
        SelectedColor = Se.Settings.Tools.ChangeFormatting.Color.FromHexToColor();
        IsColorVisible = SelectedToType?.Type == ChangeFormattingType.Color;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.ChangeFormatting.LastFromType = SelectedFromType?.Type.ToString() ?? string.Empty;
        Se.Settings.Tools.ChangeFormatting.LastToType = SelectedToType?.Type.ToString() ?? string.Empty;
        Se.Settings.Tools.ChangeFormatting.Color = SelectedColor.FromColorToHex();
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        FixedSubtitle = Subtitles.Select(p => new SubtitleLineViewModel(p.SubtitleLineViewModel)).ToList();
        SaveSettings();
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/change-formatting");
        }
    }

    internal void SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _dirty = true;
        IsColorVisible = SelectedToType?.Type == ChangeFormattingType.Color;
    }

    internal void ColorChanged(object? sender, ColorChangedEventArgs e)
    {
        _dirty = true;
    }
}