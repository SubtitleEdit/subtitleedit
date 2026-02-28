using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.AddToNamesList;

public partial class AddToNamesListViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private string _name;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AddToNamesListViewModel()
    {
        Title = string.Empty;
        Name = string.Empty;
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

        if (string.IsNullOrWhiteSpace(Name))
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
        var ok = nameList.Add(Name);
        if (!ok)
        {
            var message = $"Could not add name \"{Name}\" to \"Name list\" - perhaps it already exists";
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