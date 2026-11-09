using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PickLanguage;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.DoNotBreakAfterList;

public partial class DoNotBreakAfterListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<DoNotBreakLanguageItem> _languages;
    [ObservableProperty] private DoNotBreakLanguageItem? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<NoBreakAfterListItem> _items;
    [ObservableProperty] private NoBreakAfterListItem? _selectedItem;
    [ObservableProperty] private string _newItemText;
    [ObservableProperty] private bool _isTextItem;
    [ObservableProperty] private bool _isRegexItem;

    public Window? Window { get; set; }

    private readonly IWindowService _windowService;

    public DoNotBreakAfterListViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Languages = new ObservableCollection<DoNotBreakLanguageItem>();
        Items = new ObservableCollection<NoBreakAfterListItem>();
        NewItemText = string.Empty;
        IsTextItem = true;

        InitLanguages(null);
    }

    private void InitLanguages(string? selectTwoLetterCode)
    {
        var languages = new List<DoNotBreakLanguageItem>();
        if (Directory.Exists(Se.DictionariesFolder))
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            foreach (var fileName in Directory.GetFiles(Se.DictionariesFolder, "*_NoBreakAfterList.xml"))
            {
                try
                {
                    var name = Path.GetFileName(fileName);
                    var languageId = name.Substring(0, name.IndexOf('_'));
                    var ci = cultures.FirstOrDefault(p => p.TwoLetterISOLanguageName == languageId)
                             ?? CultureInfo.GetCultureInfoByIetfLanguageTag(languageId);
                    var displayName = ci.EnglishName + " (" + ci.NativeName.CapitalizeFirstLetter() + ")";
                    languages.Add(new DoNotBreakLanguageItem(displayName, ci.TwoLetterISOLanguageName, fileName));
                }
                catch
                {
                    // a file name that does not map to a culture is not editable by language - skip it
                }
            }
        }

        Languages.Clear();
        Languages.AddRange(languages.OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase));

        SelectedLanguage =
            Languages.FirstOrDefault(p => p.TwoLetterCode == selectTwoLetterCode) ??
            Languages.FirstOrDefault(p => p.TwoLetterCode == "en") ??
            Languages.FirstOrDefault();
    }

    internal void SelectedLanguageChanged()
    {
        Items.Clear();
        SelectedItem = null;

        var language = SelectedLanguage;
        if (language == null)
        {
            return;
        }

        Items.AddRange(LoadItems(language.FileName));
    }

    private static List<NoBreakAfterListItem> LoadItems(string fileName)
    {
        var items = new List<NoBreakAfterListItem>();
        try
        {
            // The shipped list plus the user's edits from <lang>_NoBreakAfterList_User.xml (#16375).
            items.AddRange(NoBreakAfterListFile.LoadMerged(fileName).Select(p => new NoBreakAfterListItem(p.Text, p.IsRegex)));
        }
        catch
        {
            // an unreadable list shows as empty; saving a new item will rewrite the user file
        }

        return items.OrderBy(p => p.Text, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Saves the list as edits on top of the shipped file. The shipped file is replaced on every
    /// upgrade, so writing to it directly lost the user's changes (#16375).
    /// </summary>
    private static bool SaveItems(string fileName, IEnumerable<NoBreakAfterListItem> items)
    {
        try
        {
            NoBreakAfterListFile.SaveUserChanges(fileName, ToEntries(items));
            Utilities.ResetNoBreakAfterList(); // the break engine caches the active list per language
            return true;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, $"Failed to save no-break-after list for '{fileName}'");
            return false;
        }
    }

    private static IEnumerable<NoBreakAfterListFile.Entry> ToEntries(IEnumerable<NoBreakAfterListItem> items)
    {
        return items.Select(p => new NoBreakAfterListFile.Entry(p.Text, p.IsRegex));
    }

    [RelayCommand]
    private async Task AddItem()
    {
        var text = NewItemText.Trim();
        var language = SelectedLanguage;
        if (string.IsNullOrEmpty(text) || language == null || Window == null)
        {
            return;
        }

        if (IsRegexItem && !RegexUtils.IsValidRegex(text))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.SourceView.InvalidRegularExpression, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var existing = Items.FirstOrDefault(p => p.Text == text && p.IsRegex == IsRegexItem);
        if (existing != null)
        {
            SelectedItem = existing;
            return;
        }

        var item = new NoBreakAfterListItem(text, IsRegexItem);
        if (!SaveItems(language.FileName, Items.Append(item)))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Options.WordLists.UnableToAddItem, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var insertIndex = 0;
        while (insertIndex < Items.Count && string.Compare(Items[insertIndex].Text, item.Text, StringComparison.OrdinalIgnoreCase) < 0)
        {
            insertIndex++;
        }

        Items.Insert(insertIndex, item);
        SelectedItem = item;
        NewItemText = string.Empty;
    }

    [RelayCommand]
    private async Task RemoveItem()
    {
        var selected = SelectedItem;
        var language = SelectedLanguage;
        if (selected == null || language == null || Window == null)
        {
            return;
        }

        if (!SaveItems(language.FileName, Items.Where(p => p != selected)))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Options.WordLists.UnableToRemoveItem, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var idx = Items.IndexOf(selected);
        Items.Remove(selected);

        if (Items.Count == 0)
        {
            SelectedItem = null;
        }
        else if (idx >= Items.Count)
        {
            SelectedItem = Items[Items.Count - 1];
        }
        else
        {
            SelectedItem = Items[idx];
        }
    }

    [RelayCommand]
    private async Task NewLanguage()
    {
        if (Window == null)
        {
            return;
        }

        var existingCodes = Languages.Select(p => p.TwoLetterCode).ToList();
        var viewModel = await _windowService.ShowDialogAsync<PickLanguageWindow, PickLanguageViewModel>(
            Window, vm =>
            {
                vm.Title = Se.Language.Options.Settings.UseDoNotBreakAfterList;
                vm.Initialize(existingCodes);
            });

        if (!viewModel.OkPressed || viewModel.SelectedLanguage == null)
        {
            return;
        }

        var twoLetterCode = viewModel.SelectedLanguage.Code;
        var fileName = Path.Combine(Se.DictionariesFolder, twoLetterCode + "_NoBreakAfterList.xml");
        if (!File.Exists(fileName))
        {
            var seedItems = new[] { "Dr", "Dr.", "Mr.", "Mrs.", "Ms." }
                .Select(p => new NoBreakAfterListItem(p, false));
            try
            {
                NoBreakAfterListFile.SaveItems(fileName, ToEntries(seedItems));
                Utilities.ResetNoBreakAfterList();
            }
            catch (Exception exception)
            {
                Se.LogError(exception, $"Failed to create no-break-after list '{fileName}'");
                await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Options.WordLists.UnableToAddItem, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        InitLanguages(twoLetterCode);
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    partial void OnSelectedItemChanged(NoBreakAfterListItem? value)
    {
        if (value == null)
        {
            return;
        }

        NewItemText = value.Text;
        IsRegexItem = value.IsRegex;
        IsTextItem = !value.IsRegex;
    }

    internal void ItemTextBoxKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            // Fire-and-forget discard: "using var" would call Task.Dispose when this handler
            // returns, which throws while the task is still awaiting a message box.
            _ = AddItem();
        }
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
            UiUtil.ShowHelp("features/settings", "tools");
        }
    }
}
