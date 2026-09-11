using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace UITests.Features.Edit;

/// <summary>
/// Multiple replace rules are user-entered regular expressions, so they get the same five second
/// match timeout as find/replace: without it a pattern with catastrophic backtracking held the
/// preview lock forever, and with it the UI thread, which waits on that same lock in "Ok" and
/// "Apply".
///
/// The second half pins the other side of #13534's "validate every rule so a broken one is marked
/// even in an unticked category": validation must not be what builds the RegexOptions.Compiled
/// regex, or opening the dialog emits IL for every rule in the file.
/// </summary>
public class MultipleReplaceRegexTimeoutTests : IDisposable
{
    private readonly ShortRegexTimeout _shortRegexTimeout = new();

    public void Dispose() => _shortRegexTimeout.Dispose();

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    // 30 a's and no "b": "(a+)+b" has to try every way of splitting them before giving up.
    private const string EvilPattern = "(a+)+b";
    private static readonly string EvilLine = new string('a', 30) + "c";

    // Well above the five second timeout, but far below "never returns" - the point is that the
    // preview comes back at all, not how quickly.
    private const int MaxSeconds = 60;

    private static MultipleReplaceViewModel NewViewModel()
    {
        var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25; // the tests wait for the preview timer; see PreviewIntervalMs
        vm.Nodes.Clear();
        return vm;
    }

    private static RuleTreeNode AddCategory(MultipleReplaceViewModel vm, string name, bool isActive = true)
    {
        var category = new RuleTreeNode(true) { CategoryName = name, IsActive = isActive };
        vm.Nodes.Add(category);
        return category;
    }

    private static RuleTreeNode AddRule(RuleTreeNode category, string find, string replaceWith, MultipleReplaceType type)
    {
        var node = new RuleTreeNode(false)
        {
            Find = find,
            ReplaceWith = replaceWith,
            IsActive = true,
            Parent = category,
            Type = type,
        };
        category.SubNodes!.Add(node);
        return node;
    }

    private static ConcurrentDictionary<string, Regex> CompiledRegexes(MultipleReplaceViewModel vm)
    {
        var field = typeof(MultipleReplaceViewModel).GetField("_compiledRegExList", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (ConcurrentDictionary<string, Regex>)field.GetValue(vm)!;
    }

    private static void Settle(int ms)
    {
        var end = Environment.TickCount64 + ms;
        while (Environment.TickCount64 < end)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(20);
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static void WaitUntil(Func<bool> done, int timeoutMs)
    {
        var end = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < end)
        {
            Dispatcher.UIThread.RunJobs();
            if (done())
            {
                Dispatcher.UIThread.RunJobs();
                return;
            }

            Thread.Sleep(20);
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static Subtitle OneLine(string text)
    {
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph(text, 0, 2000));
        return s;
    }

    [AvaloniaFact]
    public void CatastrophicPattern_GivesUpAndMarksTheRule()
    {
        var vm = NewViewModel();
        var category = AddCategory(vm, "c1");
        var evil = AddRule(category, EvilPattern, "x", MultipleReplaceType.RegularExpression);

        var stopwatch = Stopwatch.StartNew();
        vm.Initialize(OneLine(EvilLine));
        WaitUntil(() => evil.HasError, MaxSeconds * 1000);
        stopwatch.Stop();

        Assert.True(evil.HasError, "the rule that timed out was not marked in the tree");
        Assert.True(stopwatch.Elapsed.TotalSeconds < MaxSeconds, $"the preview took {stopwatch.Elapsed.TotalSeconds:0.0}s");
        Assert.Empty(vm.Fixes);
    }

    // The rule that gives up must cost only itself, exactly like one that will not compile.
    [AvaloniaFact]
    public void CatastrophicPattern_LeavesTheOtherRulesWorking()
    {
        var vm = NewViewModel();
        var category = AddCategory(vm, "c1");
        AddRule(category, EvilPattern, "x", MultipleReplaceType.RegularExpression);
        AddRule(category, "colour", "color", MultipleReplaceType.CaseInsensitive);

        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(EvilLine + " colour", 0, 2000));
        vm.Initialize(subtitle);

        WaitUntil(() => vm.Fixes.Count == 1, MaxSeconds * 1000);

        Assert.Single(vm.Fixes);
        Assert.Equal(EvilLine + " color", vm.Fixes[0].After);
    }

    // A pattern the timeout stopped is not retried on every remaining line: five seconds each
    // over a whole file is not a preview, it is a hang with extra steps.
    [AvaloniaFact]
    public void CatastrophicPattern_IsNotRetriedForEveryLine()
    {
        var vm = NewViewModel();
        var category = AddCategory(vm, "c1");
        var evil = AddRule(category, EvilPattern, "x", MultipleReplaceType.RegularExpression);

        var subtitle = new Subtitle();
        for (var i = 0; i < 20; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph(EvilLine, 0, 2000));
        }

        var stopwatch = Stopwatch.StartNew();
        vm.Initialize(subtitle);
        WaitUntil(() => evil.HasError, MaxSeconds * 1000);
        stopwatch.Stop();

        Assert.True(evil.HasError);

        // Twenty lines at the five second timeout each would be 100 seconds; one is ~5.
        Assert.True(stopwatch.Elapsed.TotalSeconds < MaxSeconds, $"the preview took {stopwatch.Elapsed.TotalSeconds:0.0}s for 20 lines");
    }

    // Validation covers every rule so the tree can mark a broken one anywhere; compiling is what
    // must stay lazy. RegexOptions.Compiled emits IL that is never reclaimed, and a rule pack
    // import (#13529) brings in hundreds of rules the user may never tick.
    [AvaloniaFact]
    public void RuleInUntickedCategory_IsValidatedButNotCompiled()
    {
        var vm = NewViewModel();
        var ticked = AddCategory(vm, "ticked");
        AddRule(ticked, "red", "blue", MultipleReplaceType.RegularExpression);

        var unticked = AddCategory(vm, "unticked", isActive: false);
        var goodButUnused = AddRule(unticked, "green", "yellow", MultipleReplaceType.RegularExpression);
        var broken = AddRule(unticked, "(unclosed", string.Empty, MultipleReplaceType.RegularExpression);

        vm.Initialize(OneLine("The colour is red."));
        WaitUntil(() => vm.Fixes.Count == 1, 3000);
        Settle(300);

        // Validated: the broken rule in the unticked category is still marked...
        Assert.True(broken.HasError);
        Assert.False(goodButUnused.HasError);

        // ...but only the rule that actually ran was compiled.
        var compiled = CompiledRegexes(vm);
        Assert.Contains("red", compiled.Keys);
        Assert.DoesNotContain("green", compiled.Keys);
        Assert.DoesNotContain("(unclosed", compiled.Keys);
    }

    [AvaloniaFact]
    public void TickingTheCategory_CompilesItsRulesThen()
    {
        var vm = NewViewModel();
        var category = AddCategory(vm, "later", isActive: false);
        AddRule(category, "red", "blue", MultipleReplaceType.RegularExpression);

        vm.Initialize(OneLine("The colour is red."));
        Settle(600);
        Assert.DoesNotContain("red", CompiledRegexes(vm).Keys);

        category.IsActive = true;
        vm.OnActiveChanged(null, new Avalonia.Interactivity.RoutedEventArgs());
        WaitUntil(() => vm.Fixes.Count == 1, 3000);

        Assert.Single(vm.Fixes);
        Assert.Contains("red", CompiledRegexes(vm).Keys);
    }

    // Every runnable regex must carry the timeout, whatever route built it.
    [AvaloniaFact]
    public void CompiledRules_CarryTheMatchTimeout()
    {
        var vm = NewViewModel();
        var category = AddCategory(vm, "c1");
        AddRule(category, "red", "blue", MultipleReplaceType.RegularExpression);

        vm.Initialize(OneLine("The colour is red."));
        WaitUntil(() => vm.Fixes.Count == 1, 3000);

        Assert.All(CompiledRegexes(vm).Values, r => Assert.Equal(RegexUtils.UserPatternMatchTimeout, r.MatchTimeout));
    }
}
