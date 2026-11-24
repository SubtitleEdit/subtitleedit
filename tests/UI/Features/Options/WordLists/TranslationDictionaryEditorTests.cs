using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Options.WordLists;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace UITests.Features.Options.WordLists;

public class TranslationDictionaryEditorTests
{
    [AvaloniaFact]
    public void EditAddAndRemoveRulesUsedByAutoTranslate()
    {
        var folder = Directory.CreateTempSubdirectory();
        var vm = new TranslationDictionaryViewModel();
        vm.Initialize(new LanguageItem("Danish", "da", "da_DK"), folder.FullName);
        var window = new TranslationDictionaryWindow(vm);
        window.Show();
        try
        {
            var source = new TranslationPair("English", "English", "en");
            var target = new TranslationPair("Danish", "Danish", "da");
            var original = Assert.Single(vm.Entries, entry => entry.Source == "Thank you." && entry.Draft == "Thank you.");
            vm.SelectedEntry = original;
            Dispatcher.UIThread.RunJobs();
            var output = window.GetVisualDescendants().OfType<TextBox>()
                .Single(box => AutomationProperties.GetName(box) == Se.Language.Options.WordLists.TranslationRuleOutput);
            Assert.Equal("Tak.", output.Text);
            output.SetCurrentValue(TextBox.TextProperty, "Tak skal du have.");
            Dispatcher.UIThread.RunJobs();
            vm.SaveRuleCommand.Execute(null);
            Assert.Equal("Tak skal du have.", TranslationPostProcessDictionary.Get(source, target, folder.FullName)
                ?.Match("Thank you.", "Thank you.")?.Replacement);

            vm.NewRuleCommand.Execute(null);
            vm.SourceCue = "Keep going.";
            vm.EngineTranslation = "Behold gående.";
            vm.ReviewOnly = true;
            vm.Output = "Fortsæt.\nBliv ved.";
            vm.SaveRuleCommand.Execute(null);
            Assert.Equal(new[] { "Fortsæt.", "Bliv ved." }, TranslationPostProcessDictionary.Get(source, target, folder.FullName)
                ?.Match("Keep going.", "Behold gående.")?.Suggestions);

            vm.RemoveRuleCommand.Execute(null);
            Assert.Null(TranslationPostProcessDictionary.Get(source, target, folder.FullName)?.Match("Keep going.", "Behold gående."));
        }
        finally
        {
            window.Close();
            folder.Delete(recursive: true);
        }
    }
}
