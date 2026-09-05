using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.GrammarCheck;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Tools.GrammarCheck;

/// <summary>
/// The path from a LanguageTool match to a changed subtitle line runs through two offset mappings -
/// batch text to line, line to paragraph - and several matches can land on the same line. These tests
/// drive that path with canned matches, so it is covered without a server.
/// </summary>
public class GrammarCheckViewModelTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static GrammarCheckViewModel MakeViewModel(Subtitle subtitle)
    {
        var viewModel = new GrammarCheckViewModel(new WindowService(new NullServiceProvider()));
        viewModel.Initialize(subtitle, null);
        return viewModel;
    }

    private static Subtitle MakeSubtitle(params string[] texts)
    {
        var subtitle = new Subtitle();
        for (var i = 0; i < texts.Length; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph(texts[i], i * 1000, i * 1000 + 900));
        }

        return subtitle;
    }

    private static LanguageToolMatch MakeMatch(int offset, int length, string[] replacements,
        string categoryId = "GRAMMAR", string issueType = "grammar")
    {
        return new LanguageToolMatch
        {
            Offset = offset,
            Length = length,
            Message = "Test message",
            ShortMessage = "Test",
            RuleId = "TEST_RULE",
            CategoryId = categoryId,
            IssueType = issueType,
            Replacements = replacements,
        };
    }

    [Fact]
    public void AddMatches_MapsAMatchOntoItsParagraphWithoutTouchingTheTags()
    {
        var subtitle = MakeSubtitle("<i>He go to school</i>", "I has a apple");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(subtitle.Paragraphs.Select(p => p.Text).ToList());

        viewModel.AddMatches(
            new[] { MakeMatch(annotated.Text.IndexOf("go", StringComparison.Ordinal), 2, new[] { "goes" }) },
            annotated,
            new[] { 0, 1 });

        var item = Assert.Single(viewModel.Suggestions);
        Assert.Equal(1, item.Number);
        Assert.Equal(0, item.ParagraphIndex);
        Assert.Equal("go", item.Fragment);
        Assert.Equal("<i>He goes to school</i>", item.After);
        Assert.True(item.IsSelected);
    }

    [Fact]
    public void AddMatches_SecondLine_KeepsTheParagraphNumber()
    {
        var subtitle = MakeSubtitle("He goes to school.", "I has a apple");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(subtitle.Paragraphs.Select(p => p.Text).ToList());

        viewModel.AddMatches(
            new[] { MakeMatch(annotated.Text.IndexOf("has", StringComparison.Ordinal), 3, new[] { "have" }) },
            annotated,
            new[] { 0, 1 });

        var item = Assert.Single(viewModel.Suggestions);
        Assert.Equal(2, item.Number);
        Assert.Equal(1, item.ParagraphIndex);
        Assert.Equal("I have a apple", item.After);
    }

    [Fact]
    public void AddMatches_BatchDoesNotStartAtTheFirstParagraph_UsesTheGivenIndexes()
    {
        var subtitle = MakeSubtitle("One.", "Two.", "I has a apple");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { subtitle.Paragraphs[2].Text });

        viewModel.AddMatches(
            new[] { MakeMatch(annotated.Text.IndexOf("has", StringComparison.Ordinal), 3, new[] { "have" }) },
            annotated,
            new[] { 2 });

        var item = Assert.Single(viewModel.Suggestions);
        Assert.Equal(3, item.Number);
        Assert.Equal(2, item.ParagraphIndex);
    }

    [Fact]
    public void AddMatches_MatchCrossingATag_IsDropped()
    {
        var subtitle = MakeSubtitle("a <i>apple</i>");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { subtitle.Paragraphs[0].Text });

        viewModel.AddMatches(new[] { MakeMatch(0, 10, new[] { "an apple" }) }, annotated, new[] { 0 });

        Assert.Empty(viewModel.Suggestions);
    }

    [Fact]
    public void AddMatches_RuleWithoutAReplacement_CannotBeApplied()
    {
        var subtitle = MakeSubtitle("He go to school");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { subtitle.Paragraphs[0].Text });

        viewModel.AddMatches(new[] { MakeMatch(3, 2, Array.Empty<string>()) }, annotated, new[] { 0 });

        var item = Assert.Single(viewModel.Suggestions);
        Assert.False(item.CanApply);
        Assert.False(item.IsSelected);
        Assert.Equal(item.Before, item.After);
    }

    [Fact]
    public void AddMatches_StyleIssues_StartUnticked()
    {
        var subtitle = MakeSubtitle("He is very very tired.");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { subtitle.Paragraphs[0].Text });

        viewModel.AddMatches(
            new[] { MakeMatch(6, 9, new[] { "is very" }, "STYLE", "style") },
            annotated,
            new[] { 0 });

        var item = Assert.Single(viewModel.Suggestions);
        Assert.True(item.CanApply);
        Assert.False(item.IsSelected);
    }

    [Fact]
    public void Ok_AppliesTheTickedFixesAndCountsTheChangedLines()
    {
        var subtitle = MakeSubtitle("<i>He go to school</i>", "I has a apple");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(subtitle.Paragraphs.Select(p => p.Text).ToList());

        viewModel.AddMatches(new[]
        {
            MakeMatch(annotated.Text.IndexOf("go", StringComparison.Ordinal), 2, new[] { "goes" }),
            MakeMatch(annotated.Text.IndexOf("has", StringComparison.Ordinal), 3, new[] { "have" }),
        }, annotated, new[] { 0, 1 });

        viewModel.OkCommand.Execute(null);

        Assert.True(viewModel.OkPressed);
        Assert.Equal(2, viewModel.FixedCount);
        Assert.Equal("<i>He goes to school</i>", viewModel.FixedSubtitle.Paragraphs[0].Text);
        Assert.Equal("I have a apple", viewModel.FixedSubtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void Ok_UntickedFix_LeavesTheLineAlone()
    {
        var subtitle = MakeSubtitle("He go to school");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { subtitle.Paragraphs[0].Text });

        viewModel.AddMatches(new[] { MakeMatch(3, 2, new[] { "goes" }) }, annotated, new[] { 0 });
        viewModel.Suggestions[0].IsSelected = false;
        viewModel.OkCommand.Execute(null);

        Assert.Equal(0, viewModel.FixedCount);
        Assert.Equal("He go to school", viewModel.FixedSubtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void Ok_TwoFixesInOneLine_BothLandCorrectly()
    {
        var subtitle = MakeSubtitle("I has a apple");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { subtitle.Paragraphs[0].Text });

        viewModel.AddMatches(new[]
        {
            MakeMatch(2, 3, new[] { "have" }),
            MakeMatch(6, 1, new[] { "an" }),
        }, annotated, new[] { 0 });

        viewModel.OkCommand.Execute(null);

        Assert.Equal(1, viewModel.FixedCount);
        Assert.Equal("I have an apple", viewModel.FixedSubtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void Ok_AlternativeReplacementPicked_IsTheOneApplied()
    {
        var subtitle = MakeSubtitle("He go to school");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { subtitle.Paragraphs[0].Text });

        viewModel.AddMatches(new[] { MakeMatch(3, 2, new[] { "goes", "went" }) }, annotated, new[] { 0 });
        viewModel.SelectedSuggestion = viewModel.Suggestions[0];
        viewModel.SelectedReplacementOption = "went";
        viewModel.OkCommand.Execute(null);

        Assert.Equal("He went to school", viewModel.FixedSubtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void PickingAnotherReplacement_RaisesAfterSoTheGridCanFollow()
    {
        var subtitle = MakeSubtitle("He go to school");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { subtitle.Paragraphs[0].Text });

        viewModel.AddMatches(new[] { MakeMatch(3, 2, new[] { "goes", "went" }) }, annotated, new[] { 0 });
        var item = viewModel.Suggestions[0];
        var raised = new List<string?>();
        item.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        viewModel.SelectedSuggestion = item;
        viewModel.SelectedReplacementOption = "went";

        Assert.Contains(nameof(GrammarCheckSuggestionItem.After), raised);
        Assert.Equal("He went to school", item.After);
    }

    [Fact]
    public void SelectingARow_ListsEveryReplacementAsAnOption()
    {
        var subtitle = MakeSubtitle("He go to school");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(new List<string> { subtitle.Paragraphs[0].Text });

        viewModel.AddMatches(new[] { MakeMatch(3, 2, new[] { "goes", "went" }) }, annotated, new[] { 0 });
        viewModel.SelectedSuggestion = viewModel.Suggestions[0];

        Assert.Equal(new[] { "goes", "went" }, viewModel.ReplacementOptions);
        Assert.Equal("goes", viewModel.SelectedReplacementOption);
        Assert.True(viewModel.HasReplacementOptions);
        Assert.Contains("Test message", viewModel.MessageText);
    }

    [Fact]
    public void SetFilter_ShowsOnlyTheChosenCategory()
    {
        var subtitle = MakeSubtitle("He go to school", "i has a apple");
        var viewModel = MakeViewModel(subtitle);
        var annotated = LanguageToolAnnotatedText.Build(subtitle.Paragraphs.Select(p => p.Text).ToList());

        viewModel.AddMatches(new[]
        {
            MakeMatch(3, 2, new[] { "goes" }),
            MakeMatch(annotated.Text.IndexOf("i has", StringComparison.Ordinal), 1, new[] { "I" }, "CASING", "uppercase"),
        }, annotated, new[] { 0, 1 });

        Assert.Equal(2, viewModel.Suggestions.Count);

        var casingChip = viewModel.FilterChips.First(c => c.Label == Se.Language.Tools.GrammarCheck.CategoryCasing);
        Assert.Equal(1, casingChip.Count);
        viewModel.SetFilterCommand.Execute(casingChip);

        Assert.Equal("I", Assert.Single(viewModel.Suggestions).Replacement);
    }

    private static List<LanguageToolLanguage> SampleLanguages()
    {
        return new List<LanguageToolLanguage>
        {
            new() { Name = "English", Code = "en", LongCode = "en" },
            new() { Name = "English (US)", Code = "en", LongCode = "en-US" },
            new() { Name = "German (Germany)", Code = "de", LongCode = "de-DE" },
        };
    }

    private static Subtitle MakeEnglishSubtitle()
    {
        return MakeSubtitle(
            "I have not seen him since the day we left the city.",
            "He said he would come back for us before the winter.",
            "But that was a long time ago, and nothing has changed.");
    }

    [Fact]
    public void PopulateLanguages_NothingChosenYet_UsesTheSubtitleLanguage()
    {
        var settings = Se.Settings.Tools.GrammarCheck;
        var saved = settings.Language;
        try
        {
            settings.Language = "auto";
            var subtitle = MakeEnglishSubtitle();
            Assert.Equal("en", LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle)); // guards the fixture
            var viewModel = MakeViewModel(subtitle);

            viewModel.PopulateLanguages(SampleLanguages());

            Assert.Equal("en", viewModel.SelectedLanguage?.LongCode);
        }
        finally
        {
            settings.Language = saved;
        }
    }

    [Fact]
    public void PopulateLanguages_ASavedLanguage_WinsOverTheDetectedOne()
    {
        var settings = Se.Settings.Tools.GrammarCheck;
        var saved = settings.Language;
        try
        {
            settings.Language = "de-DE";
            var viewModel = MakeViewModel(MakeEnglishSubtitle());

            viewModel.PopulateLanguages(SampleLanguages());

            Assert.Equal("de-DE", viewModel.SelectedLanguage?.LongCode);
        }
        finally
        {
            settings.Language = saved;
        }
    }

    [Fact]
    public void PopulateLanguages_Reload_KeepsWhatTheUserPicked()
    {
        var settings = Se.Settings.Tools.GrammarCheck;
        var saved = settings.Language;
        try
        {
            settings.Language = "auto";
            var viewModel = MakeViewModel(MakeEnglishSubtitle());
            viewModel.PopulateLanguages(SampleLanguages());
            viewModel.SelectedLanguage = viewModel.Languages.First(x => x.LongCode == "en-US");

            viewModel.PopulateLanguages(SampleLanguages());

            Assert.Equal("en-US", viewModel.SelectedLanguage?.LongCode);
        }
        finally
        {
            settings.Language = saved;
        }
    }

    [Fact]
    public void PopulateLanguages_AlwaysOffersAutoFirst()
    {
        var viewModel = MakeViewModel(MakeEnglishSubtitle());

        viewModel.PopulateLanguages(SampleLanguages());

        Assert.True(viewModel.Languages[0].IsAuto);
        Assert.Equal(4, viewModel.Languages.Count);
    }

    [AvaloniaFact]
    public void Window_ShowsTheServerBoxAndTheCheckButton()
    {
        var viewModel = new GrammarCheckViewModel(new WindowService(new NullServiceProvider()));
        var window = new GrammarCheckWindow(viewModel);

        // WithIconLeft swaps the button text for an icon+text panel and keeps the text as its UIA name
        var buttons = window.GetLogicalDescendants().OfType<Button>().ToList();
        Assert.Contains(buttons, b => AutomationProperties.GetName(b) == Se.Language.Tools.GrammarCheck.Check);
        Assert.Contains(window.GetLogicalDescendants().OfType<TextBox>(),
            t => t.Text == Se.Settings.Tools.GrammarCheck.ServerUrl);
        Assert.Contains(window.GetLogicalDescendants().OfType<ComboBox>(),
            c => c.ItemsSource == viewModel.Languages);
    }
}
