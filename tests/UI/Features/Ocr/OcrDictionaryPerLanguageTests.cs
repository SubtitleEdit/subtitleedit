using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr;

// Changing the OCR language selected the first installed dictionary of that language, so with
// several English dictionaries installed "English (Australia)" kept replacing the user's
// "English (United States)". The dictionary last used for a language now wins.
public class OcrDictionaryPerLanguageTests
{
    private static readonly SpellCheckDictionaryDisplay EnAu = new() { Name = "English (Australia) [en_AU]", DictionaryFileName = Path.Combine("dic", "en_AU.dic") };
    private static readonly SpellCheckDictionaryDisplay EnGb = new() { Name = "English (United Kingdom) [en_GB]", DictionaryFileName = Path.Combine("dic", "en_GB.dic") };
    private static readonly SpellCheckDictionaryDisplay EnUs = new() { Name = "English (United States) [en_US]", DictionaryFileName = Path.Combine("dic", "en_US.dic") };
    private static readonly SpellCheckDictionaryDisplay FrFr = new() { Name = "French [fr_FR]", DictionaryFileName = Path.Combine("dic", "fr_FR.dic") };

    private static List<SpellCheckDictionaryDisplay> English() => new() { EnAu, EnGb, EnUs };

    [AvaloniaFact]
    public void OcrLanguageChange_RestoresTheDictionaryLastUsedForThatLanguage()
    {
        var original = Se.Settings.Ocr.LastDictionaryFilePerLanguage;
        Se.Settings.Ocr.LastDictionaryFilePerLanguage = new Dictionary<string, string>();
        try
        {
            var services = new ServiceCollection();
            services.AddSubtitleEditServices();
            var vm = services.BuildServiceProvider().GetRequiredService<OcrViewModel>();
            vm.Dictionaries.Clear();
            vm.Dictionaries.Add(new SpellCheckDictionaryDisplay { Name = "[" + Se.Language.General.None + "]", DictionaryFileName = string.Empty });
            vm.Dictionaries.Add(EnAu);
            vm.Dictionaries.Add(EnGb);
            vm.Dictionaries.Add(EnUs);
            vm.Dictionaries.Add(FrFr);

            vm.SelectedDictionary = EnUs; // the user's choice for English
            vm.SelectedOllamaLanguage = "French";
            Assert.Same(FrFr, vm.SelectedDictionary);

            vm.SelectedOllamaLanguage = "English";

            Assert.Same(EnUs, vm.SelectedDictionary);
        }
        finally
        {
            Se.Settings.Ocr.LastDictionaryFilePerLanguage = original;
        }
    }

    [Fact]
    public void NoCandidates_ReturnsNull()
    {
        Assert.Null(OcrViewModel.PickDictionaryForLanguage(new List<SpellCheckDictionaryDisplay>(), EnUs, null));
    }

    [Fact]
    public void NothingRemembered_OtherLanguageSelected_PicksFirst()
    {
        var picked = OcrViewModel.PickDictionaryForLanguage(English(), FrFr, new Dictionary<string, string>());

        Assert.Same(EnAu, picked);
    }

    [Fact]
    public void NothingRemembered_CurrentIsOfTheLanguage_KeepsCurrent()
    {
        var picked = OcrViewModel.PickDictionaryForLanguage(English(), EnUs, new Dictionary<string, string>());

        Assert.Same(EnUs, picked);
    }

    [Fact]
    public void RememberedDictionary_WinsOverFirst()
    {
        var remembered = new Dictionary<string, string> { ["en"] = "en_US.dic" };

        var picked = OcrViewModel.PickDictionaryForLanguage(English(), FrFr, remembered);

        Assert.Same(EnUs, picked);
    }

    [Fact]
    public void RememberedDictionaryNoLongerInstalled_FallsBack()
    {
        var remembered = new Dictionary<string, string> { ["en"] = "en_CA.dic" };

        var picked = OcrViewModel.PickDictionaryForLanguage(English(), EnGb, remembered);

        Assert.Same(EnGb, picked);
    }
}
