using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System.Collections.ObjectModel;

namespace UITests.Features.Translate;

/// <summary>
/// #14926 follow-ups in the auto-translate window: the engines' known models are offered in a
/// drop-down, and a source language that cannot be detected no longer defaults to English.
/// </summary>
public class AutoTranslateModelAndSourceTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [AvaloniaFact]
    public void EngineWithKnownModels_ShowsModelDropDown_ElseTextBox()
    {
        var lastName = Se.Settings.AutoTranslate.AutoTranslateLastName;
        var geminiModel = Se.Settings.AutoTranslate.GeminiModel;
        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello there.", 0, 2000));
        vm.Initialize(subtitle);
        var window = new AutoTranslateWindow(vm) { Width = 1000, Height = 700 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t is GeminiTranslate);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.True(vm.ModelComboIsVisible);
            Assert.False(vm.ModelTextBoxIsVisible);
            Assert.Contains("gemini-flash-lite-latest", vm.ModelPresets);
            var combo = window.GetVisualDescendants().OfType<ComboBox>().Single(c => c.IsEditable && c.IsVisible);
            Assert.False(string.IsNullOrEmpty(vm.ModelText));
            Assert.Equal(vm.ModelText, combo.Text);

            combo.Text = "gemini-flash-lite-latest";
            Assert.Equal("gemini-flash-lite-latest", vm.ModelText);

            // Swapping one model list for another must not wipe the model the new engine restored.
            vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t is AnthropicTranslate);
            Dispatcher.UIThread.RunJobs();
            Assert.True(vm.ModelComboIsVisible);
            Assert.False(string.IsNullOrEmpty(vm.ModelText));
            Assert.Equal(vm.ModelText, combo.Text);

            vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t is GeminiTranslate);
            Dispatcher.UIThread.RunJobs();
            Assert.False(string.IsNullOrEmpty(vm.ModelText));
            Assert.Equal(vm.ModelText, combo.Text);

            vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t is OpenAiCompatibleTranslate);
            Dispatcher.UIThread.RunJobs();
            Assert.True(vm.ModelTextBoxIsVisible);
            Assert.False(vm.ModelComboIsVisible);

            vm.SelectedAutoTranslator = vm.AutoTranslators.First(t => t is GoogleTranslateV1);
            Dispatcher.UIThread.RunJobs();
            Assert.False(vm.ModelTextBoxIsVisible);
            Assert.False(vm.ModelComboIsVisible);
        }
        finally
        {
            window.Close();
            Se.Settings.AutoTranslate.AutoTranslateLastName = lastName;
            Se.Settings.AutoTranslate.GeminiModel = geminiModel;
        }
    }

    [Fact]
    public void UndetectableSource_UsesLastSource_NotEnglish()
    {
        var lastSource = Se.Settings.AutoTranslate.AutoTranslateLastSource;
        try
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Ja, klar.", 0, 1000));
            var languages = new ObservableCollection<TranslationPair>(new GoogleTranslateV1().GetSupportedSourceLanguages());

            Se.Settings.AutoTranslate.AutoTranslateLastSource = "de";
            Assert.Equal("de", AutoTranslateViewModel.EvaluateDefaultSourceLanguageCode(null, subtitle, languages));

            Se.Settings.AutoTranslate.AutoTranslateLastSource = string.Empty;
            Assert.Equal("en", AutoTranslateViewModel.EvaluateDefaultSourceLanguageCode(null, subtitle, languages));
        }
        finally
        {
            Se.Settings.AutoTranslate.AutoTranslateLastSource = lastSource;
        }
    }

    [Fact]
    public void DetectableSource_IsStillDetected()
    {
        var lastSource = Se.Settings.AutoTranslate.AutoTranslateLastSource;
        try
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("¿Dónde está mi coche?", 0, 1000));
            var languages = new ObservableCollection<TranslationPair>(new GoogleTranslateV1().GetSupportedSourceLanguages());

            Se.Settings.AutoTranslate.AutoTranslateLastSource = "de";
            Assert.Equal("es", AutoTranslateViewModel.EvaluateDefaultSourceLanguageCode(null, subtitle, languages));
        }
        finally
        {
            Se.Settings.AutoTranslate.AutoTranslateLastSource = lastSource;
        }
    }
}
