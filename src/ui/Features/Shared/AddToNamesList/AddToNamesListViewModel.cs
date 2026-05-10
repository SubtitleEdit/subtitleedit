using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.AddToNamesList;

public partial class AddToNamesListViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _multiNames;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSingleMode))]
    [NotifyPropertyChangedFor(nameof(MultiModeButtonText))]
    private bool _isMultiMode;

    public bool IsSingleMode => !IsMultiMode;
    public string MultiModeButtonText => IsMultiMode ? Se.Language.General.SingleMode : Se.Language.General.MultiMode;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AddToNamesListViewModel()
    {
        Title = string.Empty;
        Name = string.Empty;
        MultiNames = string.Empty;
        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();
    }

    internal void Initialize(
        string name,
        List<SpellCheckDictionaryDisplay>? dictionaries = null,
        SpellCheckDictionaryDisplay? dictionary = null)
    {
        Name = name.Trim();
        if (dictionaries != null)
        {
            Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>(dictionaries);
            SelectedDictionary = dictionary ?? (Dictionaries.Count > 0 ? Dictionaries[0] : null);
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        var dictionary = SelectedDictionary;
        if (dictionary == null)
        {
            return;
        }

        var fiveLetterLanguageName = dictionary.GetFiveLetterLanguageName();
        if (fiveLetterLanguageName == null)
        {
            var message = "Could not find language from file name: " + dictionary.DictionaryFileName;
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var nameList = new NameList(Se.DictionariesFolder, fiveLetterLanguageName, false, string.Empty);

        if (IsMultiMode)
        {
            await OkMultiMode(nameList);
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            return;
        }

        var ok = nameList.Add(Name.Trim());
        if (!ok)
        {
            var message = $"Could not add name \"{Name}\" to \"Name list\" - perhaps it already exists";
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        OkPressed = true;
        Window?.Close();
    }

    private async Task OkMultiMode(NameList nameList)
    {
        var names = MultiNames.SplitToLines()
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .Distinct()
            .ToList();

        if (names.Count == 0)
        {
            return;
        }

        var hasCasingIssue = names.Any(n => !StartsWithUpper(n) || IsAllUpper(n));
        if (hasCasingIssue)
        {
            var warnResult = await MessageBox.Show(
                Window!,
                Se.Language.General.Warning,
                Se.Language.SpellCheck.SomeNamesAreNotProperCase,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (warnResult != MessageBoxResult.Yes)
            {
                return;
            }
        }

        var confirm = await MessageBox.Show(
            Window!,
            Se.Language.General.Question,
            string.Format(Se.Language.SpellCheck.ImportXNames, names.Count),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var addedCount = 0;
        foreach (var n in names)
        {
            if (nameList.Add(n))
            {
                addedCount++;
            }
        }

        if (addedCount == 0)
        {
            await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.SpellCheck.NoNamesAdded, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        OkPressed = true;
        Window?.Close();
    }

    private static bool StartsWithUpper(string s)
    {
        foreach (var c in s)
        {
            if (char.IsLetter(c))
            {
                return char.IsUpper(c);
            }
        }
        return false;
    }

    private static bool IsAllUpper(string s)
    {
        var hasLetter = false;
        foreach (var c in s)
        {
            if (char.IsLetter(c))
            {
                hasLetter = true;
                if (char.IsLower(c))
                {
                    return false;
                }
            }
        }
        return hasLetter;
    }

    [RelayCommand]
    private void ToggleMultiMode()
    {
        IsMultiMode = !IsMultiMode;
        if (IsMultiMode)
        {
            Title = Se.Language.SpellCheck.AddNamesToNamesList;
            if (string.IsNullOrEmpty(MultiNames) && !string.IsNullOrWhiteSpace(Name))
            {
                MultiNames = Name;
            }
        }
        else
        {
            Title = Se.Language.SpellCheck.AddNameToNamesList;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Enter && !IsMultiMode)
        {
            e.Handled = true;
            using var _ = Ok();
        }
    }
}
