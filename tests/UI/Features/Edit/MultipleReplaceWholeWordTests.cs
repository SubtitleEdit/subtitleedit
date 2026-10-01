using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace UITests.Features.Edit;

/// <summary>
/// "Whole word" on a Multiple replace rule: replacing "Zeyn" with "Zeynep" turned every existing
/// "Zeynep" into "Zeynepep", and the only way around it was a regular expression (#15510).
/// </summary>
public class MultipleReplaceWholeWordTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static MultipleReplaceViewModel NewViewModel()
    {
        var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25;
        vm.Nodes.Clear();
        return vm;
    }

    private static RuleTreeNode AddRule(MultipleReplaceViewModel vm, string find, string replaceWith, MultipleReplaceType type, bool wholeWord)
    {
        var category = new RuleTreeNode(true) { CategoryName = "c1", IsActive = true };
        vm.Nodes.Add(category);
        var node = new RuleTreeNode(false)
        {
            Find = find,
            ReplaceWith = replaceWith,
            IsActive = true,
            Parent = category,
            Type = type,
            WholeWord = wholeWord,
        };
        category.SubNodes!.Add(node);
        return node;
    }

    private static Subtitle OneLine(string text)
    {
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph(text, 0, 2000));
        return s;
    }

    private static void WaitForPreview(MultipleReplaceViewModel vm, int expectedFixes)
    {
        var end = Environment.TickCount64 + 3000;
        while (Environment.TickCount64 < end)
        {
            Dispatcher.UIThread.RunJobs();
            if (vm.Fixes.Count == expectedFixes)
            {
                Dispatcher.UIThread.RunJobs();
                return;
            }

            Thread.Sleep(20);
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static string Preview(MultipleReplaceType type, bool wholeWord, string find, string replaceWith, string text)
    {
        var vm = NewViewModel();
        AddRule(vm, find, replaceWith, type, wholeWord);
        vm.Initialize(OneLine(text));
        WaitForPreview(vm, 1);
        Assert.Single(vm.Fixes);
        return vm.Fixes[0].After;
    }

    [AvaloniaFact]
    public void WithoutWholeWord_ReplacesInsideLongerWords()
    {
        Assert.Equal("Zeynep and Zeynepep", Preview(MultipleReplaceType.CaseInsensitive, false, "Zeyn", "Zeynep", "Zeyn and Zeynep"));
    }

    [AvaloniaFact]
    public void CaseInsensitiveWholeWord_LeavesLongerWordsAlone()
    {
        Assert.Equal("Zeynep, Zeynep and Zeynep!", Preview(MultipleReplaceType.CaseInsensitive, true, "Zeyn", "Zeynep", "zeyn, Zeynep and ZEYN!"));
    }

    [AvaloniaFact]
    public void CaseSensitiveWholeWord_HonoursCase()
    {
        Assert.Equal("zeyn Zeynep Zeynep", Preview(MultipleReplaceType.CaseSensitive, true, "Zeyn", "Zeynep", "zeyn Zeyn Zeynep"));
    }

    [AvaloniaFact]
    public void WholeWord_FindTextEndingInPunctuation()
    {
        Assert.Equal("Mister Smith, Mrs. Smith", Preview(MultipleReplaceType.CaseSensitive, true, "Mr.", "Mister", "Mr. Smith, Mrs. Smith"));
    }

    [AvaloniaFact]
    public void WholeWord_NonLatinLetters()
    {
        Assert.Equal("Бог и Аллах", Preview(MultipleReplaceType.CaseInsensitive, true, "алла", "Бог", "Алла и Аллах"));
    }

    [AvaloniaFact]
    public void WholeWord_DollarInReplacementIsLiteral()
    {
        Assert.Equal("cost $1 now", Preview(MultipleReplaceType.CaseInsensitive, true, "price", "$1", "cost price now"));
    }

    [AvaloniaFact]
    public void RegularExpression_IgnoresWholeWordFlag()
    {
        Assert.Equal("ZXep", Preview(MultipleReplaceType.RegularExpression, true, "eyn", "X", "Zeynep"));
    }

    [AvaloniaFact]
    public void TogglingWholeWord_UpdatesThePreview()
    {
        var vm = NewViewModel();
        var rule = AddRule(vm, "Zeyn", "Zeynep", MultipleReplaceType.CaseInsensitive, false);
        vm.Initialize(OneLine("Zeynep"));
        WaitForPreview(vm, 1);
        Assert.Single(vm.Fixes);

        rule.WholeWord = true;
        vm.OnActiveChanged(null, new Avalonia.Interactivity.RoutedEventArgs());
        WaitForPreview(vm, 0);
        Assert.Empty(vm.Fixes);
    }

    [Fact]
    public void CanUseWholeWord_FollowsType()
    {
        var node = new RuleTreeNode(false) { Type = MultipleReplaceType.CaseSensitive, WholeWord = true };
        Assert.True(node.CanUseWholeWord);
        Assert.True(node.IsWholeWordActive);

        node.Type = MultipleReplaceType.RegularExpression;
        Assert.False(node.CanUseWholeWord);
        Assert.False(node.IsWholeWordActive);
    }

    private static CategoryImportExportItem OneRuleExport(bool wholeWord)
    {
        var category = new RuleTreeNode(true) { CategoryName = "Names", IsActive = true };
        category.SubNodes!.Add(new RuleTreeNode(category, new MultipleReplaceRule
        {
            Active = true,
            Find = "Zeyn",
            ReplaceWith = "Zeynep",
            Type = MultipleReplaceType.CaseInsensitive,
            WholeWord = wholeWord,
        }));
        return new CategoryImportExportItem(new List<RuleTreeNode> { category });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Csv_RoundTripsWholeWord(bool wholeWord)
    {
        var imported = CsvImporter.Import(CsvExporter.Export(OneRuleExport(wholeWord)));
        var rule = imported.RuleTreeNodeList()!.Single().SubNodes!.Single();
        Assert.Equal(wholeWord, rule.WholeWord);
        Assert.Equal(MultipleReplaceType.CaseInsensitive, rule.Type);
    }

    [Fact]
    public void Csv_OlderExportWithoutWholeWordColumn()
    {
        var imported = CsvImporter.Import("Category,Find,ReplaceWith,Description,Active,Type\r\nNames,Zeyn,Zeynep,,true,CaseSensitive\r\n");
        var rule = imported.RuleTreeNodeList()!.Single().SubNodes!.Single();
        Assert.False(rule.WholeWord);
        Assert.Equal(MultipleReplaceType.CaseSensitive, rule.Type);
    }

    [Fact]
    public void Json_RoundTripsWholeWord()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(OneRuleExport(true));
        var imported = System.Text.Json.JsonSerializer.Deserialize<CategoryImportExportItem>(json)!;
        Assert.True(imported.RuleTreeNodeList()!.Single().SubNodes!.Single().WholeWord);
    }
}
