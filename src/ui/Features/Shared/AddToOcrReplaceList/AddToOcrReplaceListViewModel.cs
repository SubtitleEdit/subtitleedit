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
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.AddToOcrReplaceList;

public partial class AddToOcrReplaceListViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private string _from;
    [ObservableProperty] private string _to;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AddToOcrReplaceListViewModel()
    {
        Title = string.Empty;
        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();
        From = string.Empty;
        To = string.Empty;
    }

    internal void Initialize(
           string from,
           List<SpellCheckDictionaryDisplay>? dictionaries = null,
           SpellCheckDictionaryDisplay? dictionary = null)
    {
        From = from.Trim();
        if (dictionaries != null)
        {
            Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>(dictionaries);
            SelectedDictionary = dictionary ?? (Dictionaries.Count > 0 ? Dictionaries[0] : null);
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (Window == null)
        {
            return;
        }

        var dictionary = SelectedDictionary;
        if (dictionary == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(From) || string.IsNullOrWhiteSpace(To))
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



        var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(fiveLetterLanguageName.Substring(0, 2));
        var list = new OcrFixReplaceList2(Path.Combine(Se.DictionariesFolder, threeLetterCode + "_OCRFixReplaceList.xml"));
        list.WordReplaceList.TryGetValue(From, out var existing);

        if (existing != null)
        {
            var message = "Find word already exists in replace list";
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var ok = list.AddWordOrPartial(From, To);
        if (!ok)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Options.WordLists.UnableToAddItem, MessageBoxButtons.OK, MessageBoxIcon.Error);
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