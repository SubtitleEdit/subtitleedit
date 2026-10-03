using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.PickTargetLanguages;

public partial class PickTargetLanguageItem : ObservableObject
{
    [ObservableProperty] private bool _isChecked;

    public TranslationPair Language { get; }
    public string Name => Language.ToString();

    public PickTargetLanguageItem(TranslationPair language, bool isChecked)
    {
        Language = language;
        _isChecked = isChecked;
    }
}

/// <summary>
/// Checklist of the engine's target languages for Batch Convert's "Also translate to" - each
/// checked language gives one more output file per input file.
/// </summary>
public partial class PickTargetLanguagesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<PickTargetLanguageItem> _languages = new();
    [ObservableProperty] private string _searchText = string.Empty;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private List<PickTargetLanguageItem> _all = new();

    public void Initialize(IEnumerable<TranslationPair> languages, IEnumerable<TranslationPair> selected)
    {
        var selectedCodes = new HashSet<string>(selected.Select(p => p.Code), StringComparer.OrdinalIgnoreCase);
        _all = languages
            .Select(p => new PickTargetLanguageItem(p, selectedCodes.Contains(p.Code)))
            .ToList();
        SearchTextChanged();
    }

    public List<TranslationPair> GetSelectedLanguages()
    {
        return _all.Where(p => p.IsChecked).Select(p => p.Language).ToList();
    }

    public void SearchTextChanged()
    {
        var filter = SearchText?.Trim() ?? string.Empty;
        var items = string.IsNullOrEmpty(filter)
            ? _all
            : _all.Where(p =>
                p.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                p.Language.Code.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

        Languages.Clear();
        foreach (var item in items)
        {
            Languages.Add(item);
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        foreach (var item in _all)
        {
            item.IsChecked = false;
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
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
        OkPressed = false;
        Close();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            CancelCommand.Execute(null);
        }
    }
}
