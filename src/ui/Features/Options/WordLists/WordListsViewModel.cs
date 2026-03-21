using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.WordLists;

public partial class WordListsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<LanguageItem> _languages;
    [ObservableProperty] private LanguageItem? _selectedLanguage;

    [ObservableProperty] private ObservableCollection<string> _names;
    [ObservableProperty] private string? _selectedName;
    [ObservableProperty] private string _newName;

    [ObservableProperty] private ObservableCollection<string> _userWords;
    [ObservableProperty] private string? _selectedUserWord;
    [ObservableProperty] private string _newUserWord;

    [ObservableProperty] private ObservableCollection<OcrFixItem> _ocrFixes;
    [ObservableProperty] private OcrFixItem? _selectedOcrFix;
    [ObservableProperty] private string _newOcrFixFind;
    [ObservableProperty] private string _newOcrFixReplace;

    public Window? Window { get; set; }

    private IFolderHelper _folderHelper;

    public WordListsViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;

        Languages = new ObservableCollection<LanguageItem>();
        Names = new ObservableCollection<string>();
        UserWords = new ObservableCollection<string>();
        OcrFixes = new ObservableCollection<OcrFixItem>();
        NewName = string.Empty;
        NewUserWord = string.Empty;
        NewOcrFixFind = string.Empty;
        NewOcrFixReplace = string.Empty;

        InitComboBoxWordListLanguages();
        SelectedLanguage = Languages.FirstOrDefault(p => p.CultureSpecificCode == Se.Settings.Options.LastLanguage) ?? Languages.FirstOrDefault();
    }

    private void InitComboBoxWordListLanguages()
    {
        //Examples: da_DK_user.xml, eng_OCRFixReplaceList.xml, en_names.xml
        var dir = Se.DictionariesFolder;
        if (Directory.Exists(dir))
        {
            var cultures = new List<CultureInfo>();
            // Specific culture e.g: en-US, en-GB...
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                var seFile = Path.Combine(dir, culture.Name.Replace('-', '_') + "_se.xml");
                var userFile = Path.Combine(dir, culture.Name.Replace('-', '_') + "_user.xml");
                if (File.Exists(seFile) || File.Exists(userFile))
                {
                    if (!cultures.Contains(culture))
                    {
                        cultures.Add(culture);
                    }
                }
            }

            // Neutral culture e.g: "en" for all (en-US, en-GB, en-JM...)
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                var ocrFixGeneralFile = Path.Combine(dir, culture.GetThreeLetterIsoLanguageName() + "_OCRFixReplaceList.xml");
                var ocrFixUserFile = Path.Combine(dir, culture.GetThreeLetterIsoLanguageName() + "_OCRFixReplaceList_User.xml");
                var namesFile = Path.Combine(dir, culture.TwoLetterISOLanguageName + "_names.xml");
                var seFile = Path.Combine(dir, culture.Name.Replace('-', '_') + "_se.xml");
                if (File.Exists(ocrFixGeneralFile) || File.Exists(ocrFixUserFile) || File.Exists(namesFile) || File.Exists(seFile))
                {
                    var alreadyInList = false;
                    foreach (var ci in cultures)
                    {
                        // If culture is already added to the list, it doesn't matter if it's "culture specific" do not re-add.
                        if (ci.GetThreeLetterIsoLanguageName().Equals(culture.GetThreeLetterIsoLanguageName(), StringComparison.Ordinal))
                        {
                            alreadyInList = true;
                            break;
                        }
                    }
                    if (!alreadyInList)
                    {
                        cultures.Add(culture);
                    }
                }
            }

            // English is the default selected language
            Languages.Clear();
            Se.Settings.Options.LastLanguage = Se.Settings.Options.LastLanguage ?? "en_US";
            for (var index = 0; index < cultures.Count; index++)
            {
                var ci = cultures[index];
                var cultureCode = ci.Name.Replace('-', '_');
                Languages.Add(new LanguageItem(ci.EnglishName, ci.TwoLetterISOLanguageName, cultureCode));
            }
        }
    }

    [RelayCommand]
    private async Task AddName()
    {
        var name = NewName.Trim();
        if (string.IsNullOrEmpty(name) || SelectedLanguage == null || Window == null)
        {
            return;
        }

        if (Names.Contains(name) || !SaveName(SelectedLanguage, name))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Options.WordLists.UnableToAddItem, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        int insertIndex = 0;
        while (insertIndex < Names.Count && string.Compare(Names[insertIndex], name, StringComparison.OrdinalIgnoreCase) < 0)
        {
            insertIndex++;
        }

        Names.Insert(insertIndex, name);
        SelectedName = name;
        NewName = string.Empty;
    }

    [RelayCommand]
    private async Task RemoveName()
    {
        var selected = SelectedName;
        if (selected == null || SelectedLanguage == null || Window == null)
        {
            return;
        }

        if (!RemoveName(SelectedLanguage, selected))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Options.WordLists.UnableToRemoveItem, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var idx = Names.IndexOf(selected);
        Names.Remove(selected);

        if (Names.Count == 0)
        {
            SelectedName = null;
        }
        else if (idx >= Names.Count)
        {
            SelectedName = Names[Names.Count - 1];
        }
        else
        {
            SelectedName = Names[idx];
        }
    }

    [RelayCommand]
    private async Task AddWord()
    {
        var word = NewUserWord.Trim();
        if (string.IsNullOrEmpty(word) || SelectedLanguage == null || Window == null)
        {
            return;
        }

        if (Names.Contains(word) || !SaveUserWord(SelectedLanguage, word))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Options.WordLists.UnableToAddItem, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        int insertIndex = 0;
        while (insertIndex < UserWords.Count && string.Compare(UserWords[insertIndex], word, StringComparison.OrdinalIgnoreCase) < 0)
        {
            insertIndex++;
        }

        UserWords.Insert(insertIndex, word);
        SelectedUserWord = word;
        NewUserWord = string.Empty;
    }

    [RelayCommand]
    private async Task RemoveWord()
    {
        var selected = SelectedUserWord;
        if (selected == null || SelectedLanguage == null || Window == null)
        {
            return;
        }

        if (!RemoveUserWord(SelectedLanguage, selected))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Options.WordLists.UnableToRemoveItem, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var idx = UserWords.IndexOf(selected);

        UserWords.Remove(selected);

        if (UserWords.Count == 0)
        {
            SelectedUserWord = null;
        }
        else if (idx >= UserWords.Count)
        {
            SelectedUserWord = UserWords[UserWords.Count - 1];
        }
        else
        {
            SelectedUserWord = UserWords[idx];
        }
    }

    [RelayCommand]
    private async Task AddOcrFix()
    {
        var fixFind = NewOcrFixFind.Trim();
        var fixReplace = NewOcrFixReplace.Trim();
        if (string.IsNullOrWhiteSpace(fixFind) || string.IsNullOrWhiteSpace(fixReplace) || SelectedLanguage == null || Window == null)
        {
            return;
        }

        if (OcrFixes.Any(f => f.Find.Equals(fixFind, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        if (!SaveOcrFix(SelectedLanguage, fixFind, fixReplace))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Options.WordLists.UnableToAddItem, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        int insertIndex = 0;
        while (insertIndex < OcrFixes.Count && string.Compare(OcrFixes[insertIndex].Find, fixFind, StringComparison.OrdinalIgnoreCase) < 0)
        {
            insertIndex++;
        }

        var fix = new OcrFixItem(fixFind, fixReplace);
        OcrFixes.Insert(insertIndex, fix);
        SelectedOcrFix = fix;
        NewOcrFixFind = string.Empty;
        NewOcrFixReplace = string.Empty;
    }

    [RelayCommand]
    private async Task RemoveOcrFix()
    {
        var selected = SelectedOcrFix;
        if (selected == null || SelectedLanguage == null || Window == null)
        {
            return;
        }

        if (!RemoveOcrFix(SelectedLanguage, selected.Find, selected.ReplaceWith))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Options.WordLists.UnableToRemoveItem, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var idx = OcrFixes.IndexOf(selected);

        OcrFixes.Remove(selected);

        if (OcrFixes.Count == 0)
        {
            SelectedOcrFix = null;
        }
        else if (idx >= OcrFixes.Count)
        {
            SelectedOcrFix = OcrFixes[OcrFixes.Count - 1];
        }
        else
        {
            SelectedOcrFix = OcrFixes[idx];
        }
    }

    [RelayCommand]
    private void OpenDictionariesFolder()
    {
        if (Window == null)
        {
            return;
        }

        var dictionariesPath = Se.DictionariesFolder;
        if (!Directory.Exists(dictionariesPath))
        {
            Directory.CreateDirectory(dictionariesPath);
        }

        _folderHelper.OpenFolder(Window!, dictionariesPath);
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    private bool SaveName(LanguageItem lang, string name)
    {
        var nameList = new NameList(Se.DictionariesFolder, lang.CultureSpecificCode, false, string.Empty);
        return nameList.Add(name);
    }

    private bool RemoveName(LanguageItem lang, string name)
    {
        var nameList = new NameList(Se.DictionariesFolder, lang.CultureSpecificCode, false, string.Empty);
        return nameList.Remove(name);
    }

    private bool SaveUserWord(LanguageItem lang, string word)
    {
        return UserWordsHelper.AddToUserDictionary(word, lang.CultureSpecificCode);
    }

    private bool RemoveUserWord(LanguageItem lang, string word)
    {
        return UserWordsHelper.RemoveWord(word, lang.CultureSpecificCode);
    }

    private bool SaveOcrFix(LanguageItem lang, string find, string replace)
    {
        var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(lang.TwoLetterISOLanguageName);
        var list = new OcrFixReplaceList2(Path.Combine(Se.DictionariesFolder, threeLetterCode + "_OCRFixReplaceList.xml"));

        return list.AddWordOrPartial(find, replace);
    }

    private bool RemoveOcrFix(LanguageItem lang, string find, string replace)
    {
        var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(lang.TwoLetterISOLanguageName);
        var list = new OcrFixReplaceList2(Path.Combine(Se.DictionariesFolder, threeLetterCode + "_OCRFixReplaceList.xml"));

        return list.RemoveWordOrPartial(find);
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
            UiUtil.ShowHelp("features/word-lists");
        }
    }

    internal void SelectedLanguageChanged()
    {
        Names.Clear();
        UserWords.Clear();
        OcrFixes.Clear();

        var lang = SelectedLanguage;
        if (lang == null)
        {
            return;
        }

        Names.AddRange(LoadNames(lang));
        UserWords.AddRange(LoadUserWords(lang));
        OcrFixes.AddRange(LoadOcrFixList(lang));
    }

    private List<string> LoadNames(LanguageItem lang)
    {
        var nameList = new NameList(Se.DictionariesFolder, lang.CultureSpecificCode, false, string.Empty);
        var names = nameList.GetAllNames();
        names.Sort();
        return names;
    }

    private List<string> LoadUserWords(LanguageItem lang)
    {
        var list = new List<string>();
        UserWordsHelper.LoadUserWordList(list, lang.CultureSpecificCode);
        list.Sort();
        return list;
    }

    private List<OcrFixItem> LoadOcrFixList(LanguageItem lang)
    {
        var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(lang.TwoLetterISOLanguageName);
        var list = new OcrFixReplaceList2(Path.Combine(Se.DictionariesFolder, threeLetterCode + "_OCRFixReplaceList.xml"));

        var result = new List<OcrFixItem>();
        foreach (var item in list.WordReplaceList)
        {
            result.Add(new OcrFixItem(item.Key, item.Value));
        }

        return result;
    }

    internal void Closing()
    {
        var lang = SelectedLanguage;
        if (lang == null)
        {
            return;
        }

        Se.Settings.Options.LastLanguage = lang.CultureSpecificCode;
    }

    internal void Loaded()
    {
        Dispatcher.UIThread.Post(SelectedLanguageChanged);
    }

    internal void NameTextBoxKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            using var _ = AddName();
        }
    }

    internal void UserWordTextBoxKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            using var _ = AddWord();
            using var _1 = AddName();
        }
    }

    internal void OcrFixTextBoxKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            using var _ = AddOcrFix();
        }
    }
}