using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveUnicodeCharacters;

/// <summary>
/// SE 5 port of the SE 4 "Remove Unicode characters" plugin: finds every character above
/// Latin-1 (code point &gt; 255) in the subtitle and lets the user remove or replace each
/// one. Characters are scanned per rune, so surrogate pairs (emoji etc.) are treated as one
/// character instead of two broken halves.
/// </summary>
public partial class RemoveUnicodeCharactersViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<RemoveUnicodeCharacterItem> _characters;
    [ObservableProperty] private RemoveUnicodeCharacterItem? _selectedCharacter;
    [ObservableProperty] private string _statusText;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> FixedSubtitle { get; set; }

    private List<SubtitleLineViewModel> _allSubtitles;

    public RemoveUnicodeCharactersViewModel()
    {
        Characters = new ObservableCollection<RemoveUnicodeCharacterItem>();
        StatusText = string.Empty;
        FixedSubtitle = new List<SubtitleLineViewModel>();
        _allSubtitles = new List<SubtitleLineViewModel>();
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles)
    {
        _allSubtitles = subtitles.Select(p => new SubtitleLineViewModel(p)).ToList();

        var replaceList = Se.Settings.Tools.RemoveUnicodeCharacters.ReplaceList;
        var totalCount = 0;
        var found = new Dictionary<string, (int Count, List<int> Lines)>();
        for (var i = 0; i < _allSubtitles.Count; i++)
        {
            var lineNumber = i + 1;
            foreach (var rune in _allSubtitles[i].Text.EnumerateRunes())
            {
                if (rune.Value <= 255)
                {
                    continue;
                }

                totalCount++;
                var key = rune.ToString();
                if (!found.TryGetValue(key, out var entry))
                {
                    entry = (0, new List<int>());
                }

                entry.Count++;
                if (entry.Lines.Count == 0 || entry.Lines[entry.Lines.Count - 1] != lineNumber)
                {
                    entry.Lines.Add(lineNumber);
                }

                found[key] = entry;
            }
        }

        Characters.Clear();
        foreach (var kvp in found.OrderBy(p => p.Key, System.StringComparer.Ordinal))
        {
            var codePoint = System.Text.Rune.GetRuneAt(kvp.Key, 0).Value;
            Characters.Add(new RemoveUnicodeCharacterItem
            {
                Character = kvp.Key,
                CodeDisplay = "U+" + codePoint.ToString(codePoint <= 0xFFFF ? "X4" : "X6"),
                Count = kvp.Value.Count,
                LinesDisplay = string.Join(", ", kvp.Value.Lines),
                ReplaceWith = replaceList.FirstOrDefault(p => p.Character == kvp.Key)?.ReplaceWith ?? string.Empty,
            });
        }

        StatusText = totalCount == 0
            ? Se.Language.Tools.RemoveUnicodeCharacters.NoCharactersFound
            : string.Format(Se.Language.Tools.RemoveUnicodeCharacters.CharactersFoundX, totalCount);
    }

    [RelayCommand]
    private void Ok()
    {
        var replacements = Characters
            .Where(c => c.IsChecked)
            .ToDictionary(c => c.Character, c => c.ReplaceWith ?? string.Empty);

        FixedSubtitle = _allSubtitles.Select(vm =>
        {
            var updated = new SubtitleLineViewModel(vm);
            foreach (var kvp in replacements)
            {
                updated.Text = updated.Text.Replace(kvp.Key, kvp.Value);
            }

            return updated;
        }).ToList();

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var item in Characters)
        {
            item.IsChecked = true;
        }
    }

    [RelayCommand]
    private void InvertSelection()
    {
        foreach (var item in Characters)
        {
            item.IsChecked = !item.IsChecked;
        }
    }

    /// <summary>
    /// Remembers non-empty "replace with" mappings for the next run; entries for characters
    /// not present in this subtitle are kept untouched.
    /// </summary>
    private void SaveSettings()
    {
        var replaceList = Se.Settings.Tools.RemoveUnicodeCharacters.ReplaceList;
        foreach (var item in Characters)
        {
            var existing = replaceList.FirstOrDefault(p => p.Character == item.Character);
            if (!string.IsNullOrEmpty(item.ReplaceWith))
            {
                if (existing != null)
                {
                    existing.ReplaceWith = item.ReplaceWith;
                }
                else
                {
                    replaceList.Add(new SeUnicodeReplaceListItem { Character = item.Character, ReplaceWith = item.ReplaceWith });
                }
            }
            else if (existing != null)
            {
                replaceList.Remove(existing);
            }
        }

        Se.SaveSettings();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
