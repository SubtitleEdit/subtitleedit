using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.ConvertActors;

public partial class ConvertActorsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ConvertActorsDisplayItem> _subtitles;
    [ObservableProperty] private ConvertActorsDisplayItem? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<ConvertActorTypeDisplay> _fromTypes;
    [ObservableProperty] private ConvertActorTypeDisplay? _selectedFromType;
    [ObservableProperty] private ObservableCollection<ConvertActorTypeDisplay> _toTypes;
    [ObservableProperty] private ConvertActorTypeDisplay? _selectedToType;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _allSubtitles;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _setColor;
    [ObservableProperty] private Color _selectedColor;
    [ObservableProperty] private bool _changeCasing;
    [ObservableProperty] private int _selectedCasingIndex;
    [ObservableProperty] private bool _onlyNames;
    [ObservableProperty] private ObservableCollection<string> _casingOptions;
    [ObservableProperty] private bool _isCasingVisible;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> FixedSubtitle { get; set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private SubtitleFormat _format = new SubRip();

    public ConvertActorsViewModel()
    {
        Subtitles = new ObservableCollection<ConvertActorsDisplayItem>();
        var types = ConvertActorTypeDisplay.GetTypes();
        FromTypes = new ObservableCollection<ConvertActorTypeDisplay>(types);
        ToTypes = new ObservableCollection<ConvertActorTypeDisplay>(ConvertActorTypeDisplay.GetTypes());
        AllSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        FixedSubtitle = new List<SubtitleLineViewModel>();
        StatusText = string.Empty;
        CasingOptions = new ObservableCollection<string>
        {
            Se.Language.General.NormalCasing,
            Se.Language.Tools.ChangeCasing.AllUppercase,
            Se.Language.Tools.ChangeCasing.AllLowercase,
            "Proper case",
        };

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

    partial void OnSelectedFromTypeChanged(ConvertActorTypeDisplay? value) => _dirty = true;
    partial void OnSelectedToTypeChanged(ConvertActorTypeDisplay? value) => _dirty = true;
    partial void OnSetColorChanged(bool value) => _dirty = true;
    partial void OnSelectedColorChanged(Color value) => _dirty = true;
    partial void OnChangeCasingChanged(bool value) { IsCasingVisible = value; _dirty = true; }
    partial void OnSelectedCasingIndexChanged(int value) => _dirty = true;
    partial void OnOnlyNamesChanged(bool value) => _dirty = true;

    private void UpdatePreview()
    {
        if (SelectedFromType == null || SelectedToType == null)
        {
            return;
        }

        var allSubtitlesCopy = AllSubtitles.ToList();
        var paragraphs = allSubtitlesCopy.Select(vm => vm.ToParagraph()).ToList();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.AddRange(paragraphs);

        var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
        var converter = new ActorConverter(_format, languageCode);

        converter.ToSquare = SelectedToType.Type == ConvertActorType.InlineSquareBrackets;
        converter.ToParentheses = SelectedToType.Type == ConvertActorType.InlineParentheses;
        converter.ToColon = SelectedToType.Type == ConvertActorType.InlineColon;
        converter.ToActor = SelectedToType.Type == ConvertActorType.Actor;

        var fromSquare = SelectedFromType.Type == ConvertActorType.InlineSquareBrackets;
        var fromParentheses = SelectedFromType.Type == ConvertActorType.InlineParentheses;
        var fromColon = SelectedFromType.Type == ConvertActorType.InlineColon;
        var fromActor = SelectedFromType.Type == ConvertActorType.Actor;

        int? changeCasing = ChangeCasing ? SelectedCasingIndex : (int?)null;
        SkiaSharp.SKColor? color = SetColor ? SelectedColor.ToSkColor() : (SkiaSharp.SKColor?)null;

        var items = new List<ConvertActorsDisplayItem>();
        var count = 0;

        for (var i = 0; i < allSubtitlesCopy.Count; i++)
        {
            var vm = allSubtitlesCopy[i];
            var p = paragraphs[i];
            var oldText = p.Text;

            if (fromSquare && Contains(p.Text, '[', ']'))
            {
                ProcessBracketActors(vm, p, '[', ']', converter, changeCasing, color, items, ref count);
            }
            else if (fromParentheses && Contains(p.Text, '(', ')'))
            {
                ProcessBracketActors(vm, p, '(', ')', converter, changeCasing, color, items, ref count);
            }
            else if (fromColon && p.Text.Contains(':'))
            {
                var newText = converter.FixActorsFromBeforeColon(p, ':', changeCasing, color);
                if (newText != oldText)
                {
                    var updatedVm = new SubtitleLineViewModel(vm);
                    updatedVm.Text = newText;
                    items.Add(new ConvertActorsDisplayItem(vm) { NewText = newText, IsChecked = true, UpdatedViewModel = updatedVm });
                    count++;
                }
            }
            else if (fromActor && !string.IsNullOrEmpty(p.Actor))
            {
                var newText = converter.FixActorsFromActor(p, changeCasing, color);
                if (newText != oldText)
                {
                    var updatedVm = new SubtitleLineViewModel(vm);
                    updatedVm.Text = newText;
                    items.Add(new ConvertActorsDisplayItem(vm) { NewText = newText, IsChecked = true, UpdatedViewModel = updatedVm });
                    count++;
                }
            }
        }

        var statusText = string.Format(Se.Language.Tools.ConvertActors.NumberOfConversionsX, count);
        Dispatcher.UIThread.Post(() =>
        {
            Subtitles.Clear();
            foreach (var item in items)
            {
                Subtitles.Add(item);
            }

            StatusText = statusText;
        });
    }

    private void ProcessBracketActors(
        SubtitleLineViewModel vm,
        Paragraph p,
        char startChar,
        char endChar,
        ActorConverter converter,
        int? changeCasing,
        SkiaSharp.SKColor? color,
        List<ConvertActorsDisplayItem> items,
        ref int count)
    {
        var oldText = p.Text;
        var result = converter.FixActors(p, startChar, endChar, changeCasing, color);
        if (result.Skip)
        {
            return;
        }

        var isChecked = result.Selected || !OnlyNames;
        var updatedVm = new SubtitleLineViewModel(vm);
        updatedVm.Text = result.Paragraph.Text;
        updatedVm.Actor = result.Paragraph.Actor;

        items.Add(new ConvertActorsDisplayItem(vm)
        {
            NewText = result.Paragraph.Text,
            IsChecked = isChecked,
            UpdatedViewModel = updatedVm,
        });
        count++;

        if (converter.ToActor && result.NextParagraph != null)
        {
            var nextVm = new SubtitleLineViewModel(vm, generateNewId: true);
            nextVm.Text = result.NextParagraph.Text;
            nextVm.Actor = result.NextParagraph.Actor;

            items.Add(new ConvertActorsDisplayItem(vm)
            {
                Text = string.Empty,
                NewText = result.NextParagraph.Text,
                IsChecked = isChecked,
                IsNextParagraph = true,
                UpdatedViewModel = nextVm,
            });
        }
    }

    private static bool Contains(string text, char start, char end)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var startIdx = text.IndexOf(start);
        if (startIdx < 0)
        {
            return false;
        }

        return text.IndexOf(end) > startIdx;
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
        var settings = Se.Settings.Tools.ConvertActors;
        SelectedFromType = FromTypes.FirstOrDefault(p => p.Type.ToString() == settings.LastFromType) ?? FromTypes.First();
        SelectedToType = ToTypes.FirstOrDefault(p => p.Type.ToString() == settings.LastToType) ?? ToTypes.Last();
        SetColor = settings.SetColor;
        SelectedColor = settings.Color.FromHexToColor();
        ChangeCasing = settings.ChangeCasing;
        SelectedCasingIndex = settings.CasingType;
        OnlyNames = settings.OnlyNames;
        IsCasingVisible = settings.ChangeCasing;
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Tools.ConvertActors;
        settings.LastFromType = SelectedFromType?.Type.ToString() ?? string.Empty;
        settings.LastToType = SelectedToType?.Type.ToString() ?? string.Empty;
        settings.SetColor = SetColor;
        settings.Color = SelectedColor.FromColorToHex();
        settings.ChangeCasing = ChangeCasing;
        settings.CasingType = SelectedCasingIndex;
        settings.OnlyNames = OnlyNames;
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        _timerUpdatePreview.Stop();

        var checkedChanges = Subtitles
            .Where(s => s.IsChecked && !s.IsNextParagraph)
            .ToDictionary(s => s.OriginalId, s => s);

        var checkedNextParagraphs = Subtitles
            .Where(s => s.IsChecked && s.IsNextParagraph)
            .GroupBy(s => s.OriginalId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<SubtitleLineViewModel>();
        foreach (var vm in AllSubtitles)
        {
            if (checkedChanges.TryGetValue(vm.Id, out var change))
            {
                var updated = new SubtitleLineViewModel(vm);
                updated.Text = change.NewText;
                updated.Actor = change.UpdatedViewModel.Actor;
                result.Add(updated);
            }
            else
            {
                result.Add(new SubtitleLineViewModel(vm));
            }

            if (checkedNextParagraphs.TryGetValue(vm.Id, out var nextItems))
            {
                foreach (var next in nextItems)
                {
                    result.Add(new SubtitleLineViewModel(next.UpdatedViewModel, generateNewId: false));
                }
            }
        }

        for (var i = 0; i < result.Count; i++)
        {
            result[i].Number = i + 1;
        }

        FixedSubtitle = result;
        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _timerUpdatePreview.Stop();
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
            UiUtil.ShowHelp("features/convert-actors");
        }
    }

    internal void SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _dirty = true;
    }

    internal void ColorChanged(object? sender, ColorChangedEventArgs e)
    {
        _dirty = true;
    }
}