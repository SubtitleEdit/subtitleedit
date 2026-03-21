using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tmds.DBus.Protocol;

namespace Nikse.SubtitleEdit.Features.Shared.AddToUserDictionary;

public partial class AddToUserDictionaryViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private string _word;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AddToUserDictionaryViewModel()
    {
        Title = string.Empty;
        Word = string.Empty;
        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();
    }

    internal void Initialize(
        string word,
        List<SpellCheckDictionaryDisplay>? dictionaries = null,
        SpellCheckDictionaryDisplay? dictionary = null)
    {
        Word = word.Trim().ToLowerInvariant();
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

        if (string.IsNullOrWhiteSpace(Word))
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

        var ok = UserWordsHelper.AddToUserDictionary(Word, fiveLetterLanguageName);
        if (!ok)
        {
            var message = $"Could not add word \"{Word}\" to \"User dictionary\" - perhaps it already exists";
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        OkPressed = true;
        Window?.Close();
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
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
            using var _ = Ok();
        }
    }
}