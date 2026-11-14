using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Options.WordLists;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

namespace UITests.Features.Options.WordLists;

/// <summary>
/// Spell-checked OCR regex rules ("RegularExpressionsIfSpelledCorrectly") can be added, changed and
/// listed from Word lists - Regex rules, next to the plain ones (#16377).
/// </summary>
public class RegexRulesSpellCheckTests : IDisposable
{
    private const string ShippedXml =
        "<ReplaceList>" +
        "<RegularExpressions><RegEx find=\"\\s\\x22\" replaceWith=\" «\" /></RegularExpressions>" +
        "<RegularExpressionsIfSpelledCorrectly>" +
        "<RegEx find=\"\\bde([A-Z][a-z]+)\\b\" spellCheck=\"$1\" replaceWith=\"de $1\" />" +
        "</RegularExpressionsIfSpelledCorrectly>" +
        "</ReplaceList>";

    private const string Exclamation = "\\b[iI]([A-ZÁÉÍÓÚÑÜ][a-záéíóúñü]+)";

    private readonly string _folder;
    private readonly string _fileName;
    private readonly List<Window> _windows = new();

    public RegexRulesSpellCheckTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "se-regexrules-ui-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        _fileName = Path.Combine(_folder, "spa_OCRFixReplaceList.xml");
        File.WriteAllText(_fileName, ShippedXml);
    }

    [AvaloniaFact]
    public void ShippedSpellCheckedRule_IsListedWithItsWord()
    {
        var vm = MakeViewModel();

        var gated = Assert.Single(vm.Rules, r => r.IsSpellChecked);
        Assert.Equal("$1", gated.SpellCheck);

        vm.SelectedRule = gated;
        Assert.Equal("$1", vm.EditSpellCheck);

        vm.SelectedShowIndex = 4; // Spell-checked
        Assert.Same(gated, Assert.Single(vm.Rules));
    }

    [AvaloniaFact]
    public async Task AddSpellCheckedRule_SavesToUserFile()
    {
        var vm = MakeViewModel();
        vm.NewRuleCommand.Execute(null);
        vm.EditFind = Exclamation;
        vm.EditReplace = "¡$1";
        vm.EditSpellCheck = "$1";
        Assert.True(vm.CanAdd);

        await vm.AddRuleCommand.ExecuteAsync(null);

        var added = Assert.Single(vm.Rules, r => r.IsUserRule);
        Assert.True(added.IsSpellChecked);
        var saved = new OcrFixReplaceList2(_fileName).GetRegexRules().Single(r => r.IsUserRule);
        Assert.Equal("$1", saved.SpellCheck);

        // The same pattern can still be added as a plain rule.
        vm.EditSpellCheck = string.Empty;
        Assert.True(vm.CanAdd);
    }

    [AvaloniaFact]
    public async Task ClearingSpellCheckWord_TurnsRuleIntoPlainRule()
    {
        var vm = MakeViewModel();
        vm.EditFind = Exclamation;
        vm.EditReplace = "¡$1";
        vm.EditSpellCheck = "$1";
        await vm.AddRuleCommand.ExecuteAsync(null);

        vm.EditSpellCheck = string.Empty;
        Assert.True(vm.CanUpdate);
        await vm.UpdateRuleCommand.ExecuteAsync(null);

        Assert.False(vm.SelectedRule!.IsSpellChecked);
        Assert.False(new OcrFixReplaceList2(_fileName).GetRegexRules().Single(r => r.IsUserRule).IsSpellChecked);
    }

    [AvaloniaFact]
    public void TestPane_ShowsWordToSpellCheck()
    {
        var vm = MakeViewModel();
        vm.EditFind = Exclamation;
        vm.EditReplace = "¡$1";
        vm.EditSpellCheck = "$1";
        vm.TestText = "- iFuera!";

        Assert.Contains(string.Format(Se.Language.Options.WordLists.SpellCheckWordsX, "Fuera"), vm.MatchInfo);
        Assert.Equal("- ¡Fuera!", RegexRulesViewModel.Describe(vm.OutputSegments));
    }

    [AvaloniaFact]
    public void Window_HasSpellCheckWordField()
    {
        var vm = MakeViewModel();
        var window = new RegexRulesWindow(vm);
        _windows.Add(window);
        window.Show();

        Assert.Contains(window.GetLogicalDescendants().OfType<TextBox>(),
            t => AutomationProperties.GetName(t) == Se.Language.Options.WordLists.SpellCheckWord);

        var dir = Environment.GetEnvironmentVariable("SHOT_DIR");
        if (!string.IsNullOrEmpty(dir))
        {
            vm.SelectedRule = vm.Rules.First(r => r.IsSpellChecked);
            vm.TestText = "Vengo deMadrid, deTodo";
            for (var i = 0; i < 5; i++)
            {
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            }

            window.CaptureRenderedFrame()!.Save(Path.Combine(dir, "regex-rules-spell-check.png"), PngBitmapEncoderOptions.Default);
        }
    }

    private RegexRulesViewModel MakeViewModel()
    {
        var vm = new RegexRulesViewModel();
        vm.Initialize(_fileName, "Spanish");
        return vm;
    }

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}
