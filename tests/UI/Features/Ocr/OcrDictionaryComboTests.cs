using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr;

// Downloading/re-downloading a spell-check dictionary in the OCR window left the Dictionary combo
// empty. DictionaryChanged() runs from the combo's SelectionChanged, which fires while
// LoadDictionaries() has momentarily cleared the list; the old code called Dictionaries.First() on the
// empty collection, threw, and aborted LoadDictionaries before it could re-add any items.
public class OcrDictionaryComboTests
{
    private static OcrViewModel ResolveViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<OcrViewModel>();
    }

    [AvaloniaFact]
    public void DictionaryChanged_EmptyList_DoesNotThrow()
    {
        var vm = ResolveViewModel();

        // Reproduce the mid-repopulate state: list cleared, nothing selected.
        vm.Dictionaries.Clear();
        vm.SelectedDictionary = null;

        var ex = Record.Exception(() => vm.DictionaryChanged());

        Assert.Null(ex);
        Assert.False(vm.IsDictionaryLoaded);
    }

    [AvaloniaFact]
    public void DictionaryChanged_NoneSelected_IsNotLoaded_RealDictionarySelected_IsLoaded()
    {
        var vm = ResolveViewModel();

        vm.Dictionaries.Clear();
        var none = new SpellCheckDictionaryDisplay { Name = "[None]", DictionaryFileName = string.Empty };
        var real = new SpellCheckDictionaryDisplay { Name = "Dutch [nl_NL]", DictionaryFileName = @"C:\dic\nl_NL.dic" };
        vm.Dictionaries.Add(none);
        vm.Dictionaries.Add(real);

        // First item ([None]) selected => not loaded.
        vm.SelectedDictionary = none;
        vm.DictionaryChanged();
        Assert.False(vm.IsDictionaryLoaded);

        // A real dictionary (index > 0) selected => loaded.
        vm.SelectedDictionary = real;
        vm.DictionaryChanged();
        Assert.True(vm.IsDictionaryLoaded);
    }
}
