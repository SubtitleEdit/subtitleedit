using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Optris.Icons.Avalonia;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace UITests.Features.Edit;

/// <summary>
/// The Multiple replace preview runs on a 250 ms timer that stops itself while generating and
/// only restarts afterwards, so anything that threw took the preview down for the rest of the
/// session - after that no rule tick, no category tick and no edit ever changed it again.
/// A half-typed regular expression in the live edit panel was enough to trigger it (#13534).
/// </summary>
public class MultipleReplacePreviewTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static MultipleReplaceViewModel NewViewModel()
    {
        var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25; // the tests wait for the preview timer; see PreviewIntervalMs
        vm.Nodes.Clear();
        return vm;
    }

    private static RuleTreeNode AddCategory(MultipleReplaceViewModel vm, string name)
    {
        var category = new RuleTreeNode(true) { CategoryName = name, IsActive = true };
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

    private static bool TimerEnabled(MultipleReplaceViewModel vm)
    {
        var field = typeof(MultipleReplaceViewModel).GetField("_timerReplace", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return ((System.Timers.Timer)field.GetValue(vm)!).Enabled;
    }

    // The preview is generated on a background timer, so waiting is the only way to observe it.
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

    // Unconditional wait - used where the preview must be given the chance to pick up a state it
    // is expected to reject, so waiting for a fix count would return before the timer ever ticked.
    private static void Settle(int ms = 200) // eight 25 ms preview intervals
    {
        var end = Environment.TickCount64 + ms;
        while (Environment.TickCount64 < end)
        {
            Dispatcher.UIThread.RunJobs();
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

    // Typing "(red|green)" passes through "(", which Regex cannot compile.
    [AvaloniaFact]
    public void IncompleteRegexKeystroke_DoesNotStopThePreviewTimer()
    {
        var vm = NewViewModel();
        var category = AddCategory(vm, "c1");
        var rule = AddRule(category, "red", "blue", MultipleReplaceType.RegularExpression);
        vm.Initialize(OneLine("The colour is red."));

        WaitForPreview(vm, 1);
        Assert.Single(vm.Fixes);

        rule.Find = "(";
        vm.RuleTextChanged(null, null!);
        Settle();

        Assert.True(TimerEnabled(vm));
    }

    // The whole reported symptom: after a bad keystroke the preview kept showing stale fixes,
    // including hits from a category the user had just unticked.
    [AvaloniaFact]
    public void AfterAnIncompleteRegex_PreviewStillFollowsTheRules()
    {
        var vm = NewViewModel();
        var category = AddCategory(vm, "c1");
        AddRule(category, "colour", "color", MultipleReplaceType.CaseInsensitive);
        var regexRule = AddRule(category, "red", "blue", MultipleReplaceType.RegularExpression);
        vm.Initialize(OneLine("The colour is red."));

        WaitForPreview(vm, 1);
        Assert.Single(vm.Fixes);

        // the timer must actually tick on the broken pattern - that is what used to kill it
        regexRule.Find = "(";
        vm.RuleTextChanged(null, null!);
        Settle();

        regexRule.Find = "(red|green)";
        vm.RuleTextChanged(null, null!);
        Settle();
        Assert.Single(vm.Fixes);
        Assert.Equal("The color is blue.", vm.Fixes[0].After);

        category.IsActive = false;
        vm.OnActiveChanged(null, new RoutedEventArgs());
        WaitForPreview(vm, 0);
        Assert.Empty(vm.Fixes);
    }

    // A rule that can never compile must cost only itself, not every other rule in the tree.
    [AvaloniaFact]
    public void InvalidRegexRule_OnlySkipsThatRule()
    {
        var vm = NewViewModel();
        var good = AddCategory(vm, "good");
        AddRule(good, "colour", "color", MultipleReplaceType.CaseInsensitive);
        var bad = AddCategory(vm, "bad");
        AddRule(bad, "(unclosed", string.Empty, MultipleReplaceType.RegularExpression);
        vm.Initialize(OneLine("The colour is red."));

        WaitForPreview(vm, 1);

        Assert.Single(vm.Fixes);
        Assert.Equal("The color is red.", vm.Fixes[0].After);
    }

    // The preview thread used to iterate the live rule tree while the user edited it on the UI
    // thread, which can throw "collection was modified" - and again kill the timer.
    [AvaloniaFact]
    public void EditingRulesWhileThePreviewRuns_KeepsThePreviewLive()
    {
        var vm = NewViewModel();
        var category = AddCategory(vm, "c1");
        for (var i = 0; i < 300; i++)
        {
            AddRule(category, "w" + i, "x" + i, MultipleReplaceType.CaseInsensitive);
        }

        var subtitle = new Subtitle();
        for (var i = 0; i < 300; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph($"the w{i} thing", 0, 2000));
        }

        vm.Initialize(subtitle);

        for (var i = 0; i < 150; i++)
        {
            AddRule(category, "z" + i, "y" + i, MultipleReplaceType.CaseInsensitive);
            category.SubNodes!.RemoveAt(category.SubNodes.Count - 1);
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(2);
        }

        WaitForPreview(vm, 300);
        Assert.Equal(300, vm.Fixes.Count);
        Assert.True(TimerEnabled(vm));

        category.IsActive = false;
        vm.OnActiveChanged(null, new RoutedEventArgs());
        WaitForPreview(vm, 0);
        Assert.Empty(vm.Fixes);
    }

    // Rules run in tree order - the first rule to match a line wins, so the order the preview
    // applies them in has to be the order they are listed in.
    [AvaloniaFact]
    public void RulesAreAppliedInTreeOrder()
    {
        var vm = NewViewModel();
        var category = AddCategory(vm, "c1");
        AddRule(category, "aa", "bb", MultipleReplaceType.CaseInsensitive);
        AddRule(category, "bb", "cc", MultipleReplaceType.CaseInsensitive);
        vm.Initialize(OneLine("aa"));

        WaitForPreview(vm, 1);

        Assert.Single(vm.Fixes);
        Assert.Equal("cc", vm.Fixes[0].After);
    }

    // A rule that is skipped silently reads as a rule that simply does nothing, so the tree marks
    // it - and clears the mark once the pattern compiles again.
    [AvaloniaFact]
    public void BrokenRegexRule_IsMarkedInTheTreeUntilItCompiles()
    {
        var vm = NewViewModel();
        var category = AddCategory(vm, "c1");
        var good = AddRule(category, "colour", "color", MultipleReplaceType.CaseInsensitive);
        var regexRule = AddRule(category, "(", string.Empty, MultipleReplaceType.RegularExpression);
        vm.Initialize(OneLine("The colour is red."));

        Settle();

        Assert.True(regexRule.HasError);
        Assert.Contains("Invalid regular expression", regexRule.ErrorMessage);
        Assert.False(good.HasError);

        regexRule.Find = "(red|green)";
        vm.RuleTextChanged(null, null!);
        Settle();

        Assert.False(regexRule.HasError);
        Assert.Null(regexRule.ErrorMessage);
    }

    // Rules in an unticked category never run, but the user still wants to see that one of them
    // is broken.
    [AvaloniaFact]
    public void BrokenRegexRule_IsMarkedEvenInAnUntickedCategory()
    {
        var vm = NewViewModel();
        var off = AddCategory(vm, "off");
        off.IsActive = false;
        var broken = AddRule(off, "(", string.Empty, MultipleReplaceType.RegularExpression);
        var inactive = AddRule(off, "[", string.Empty, MultipleReplaceType.RegularExpression);
        inactive.IsActive = false;
        vm.Initialize(OneLine("The colour is red."));

        Settle();

        Assert.True(broken.HasError);
        Assert.True(inactive.HasError);
    }

    // The marker is a control in the tree, not just a view model flag. The message rides on a
    // tooltip, so it follows the "show hints" setting like every other tip in the app - but the
    // icon itself has to be there either way, otherwise a broken rule looks like a working one.
    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void BrokenRegexRule_ShowsAWarningInTheTree(bool showHints)
    {
        var oldShowHints = Se.Settings.Appearance.ShowHints;
        Se.Settings.Appearance.ShowHints = showHints;
        var vm = NewViewModel();
        var category = AddCategory(vm, "c1");
        var regexRule = AddRule(category, "(", string.Empty, MultipleReplaceType.RegularExpression);
        vm.Initialize(OneLine("The colour is red."));
        var window = new MultipleReplaceWindow(vm);
        try
        {
            window.Show();
            vm.ExpandAllCommand.Execute(null);
            Settle();

            Assert.True(regexRule.HasError);
            var marker = vm.RulesTreeView.GetVisualDescendants()
                .OfType<Label>()
                .FirstOrDefault(l => Attached.GetIcon(l) == IconNames.Alert);

            Assert.NotNull(marker);
            Assert.True(marker!.IsVisible);
            Assert.Equal(showHints ? regexRule.ErrorMessage : null, ToolTip.GetTip(marker));
        }
        finally
        {
            window.Close();
            Se.Settings.Appearance.ShowHints = oldShowHints;
        }
    }

    // "Ok" and "Apply" generate the preview synchronously on the UI thread, which is also the
    // thread the rule snapshot is taken on - that has to run inline instead of waiting on itself.
    [AvaloniaFact]
    public void OkAndApply_GenerateOnTheUiThreadWithoutDeadlocking()
    {
        var vm = NewViewModel();
        var category = AddCategory(vm, "c1");
        AddRule(category, "colour", "color", MultipleReplaceType.CaseInsensitive);
        var window = new MultipleReplaceWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        vm.Initialize(OneLine("The colour is red."));
        WaitForPreview(vm, 1);

        var applied = 0;
        vm.OnApply = (_, count) => applied = count;

        vm.ApplyCommand.Execute(null);
        Assert.Equal(1, applied);

        vm.OkCommand.Execute(null);
        Assert.True(vm.OkPressed);
        Assert.Equal("The color is red.", vm.FixedSubtitle.Paragraphs[0].Text);
    }

    // Closing the window has to stop the timer - the view model is transient, so otherwise every
    // visit leaves another one ticking for the life of the process.
    [AvaloniaFact]
    public void ClosingTheWindow_StopsThePreviewTimer()
    {
        var vm = NewViewModel();
        AddRule(AddCategory(vm, "c1"), "colour", "color", MultipleReplaceType.CaseInsensitive);
        var window = new MultipleReplaceWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        vm.Initialize(OneLine("The colour is red."));
        WaitForPreview(vm, 1);
        Assert.True(TimerEnabled(vm));

        window.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.False(TimerEnabled(vm));
    }
}

/// <summary>
/// Unticking a category has to drop its rules from the preview - #13534 was reported this way,
/// so pin the path end to end with a real click on the real window.
/// </summary>
public class MultipleReplaceCategoryActiveTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static void SeedSettings()
    {
        Se.Settings.Edit.MultipleReplace.Categories.Clear();
        foreach (var name in new[] { "Dutch", "English" })
        {
            var c = new SeEditMultipleReplace.MultipleReplaceCategory { Name = name, IsActive = true, IsExpanded = true };
            for (var r = 1; r <= 2; r++)
            {
                c.Rules.Add(new MultipleReplaceRule
                {
                    Active = true,
                    Find = $"{name}{r}",
                    ReplaceWith = $"repl{name}{r}",
                    Type = MultipleReplaceType.CaseInsensitive,
                });
            }

            Se.Settings.Edit.MultipleReplace.Categories.Add(c);
        }
    }

    private static Subtitle MakeSubtitle()
    {
        var s = new Subtitle();
        foreach (var name in new[] { "Dutch", "English" })
        {
            for (var r = 1; r <= 2; r++)
            {
                s.Paragraphs.Add(new Paragraph($"line with {name}{r} in it", 0, 2000));
            }
        }

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

    [AvaloniaFact]
    public void ClickingACategoryCheckBox_DropsThatCategorysRulesFromThePreview()
    {
        SeedSettings();
        var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25; // the tests wait for the preview timer; see PreviewIntervalMs
        vm.Initialize(MakeSubtitle());
        var window = new MultipleReplaceWindow(vm);
        try
        {
            window.Show();
            vm.ExpandAllCommand.Execute(null);
            for (var i = 0; i < 20; i++)
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(10);
            }

            WaitForPreview(vm, 4);
            Assert.Equal(4, vm.Fixes.Count);

            var english = vm.Nodes.First(n => n.CategoryName == "English");
            var container = (TreeViewItem)vm.RulesTreeView.ContainerFromItem(english)!;
            var checkBox = container.GetVisualDescendants().OfType<CheckBox>().First();
            var point = checkBox.TranslatePoint(new Point(checkBox.Bounds.Width / 2, checkBox.Bounds.Height / 2), window);
            Assert.NotNull(point);

            window.MouseDown(point!.Value, MouseButton.Left);
            window.MouseUp(point.Value, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.False(english.IsActive);
            WaitForPreview(vm, 2);
            Assert.Equal(2, vm.Fixes.Count);
            Assert.All(vm.Fixes, f => Assert.Contains("Dutch", f.Before));
        }
        finally
        {
            window.Close();
            Se.Settings.Edit.MultipleReplace.Categories.Clear();
        }
    }

    [AvaloniaFact]
    public void UntickedCategory_SurvivesCloseAndReopen()
    {
        SeedSettings();
        try
        {
            var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25; // the tests wait for the preview timer; see PreviewIntervalMs
            var window = new MultipleReplaceWindow(vm);
            window.Show();
            Dispatcher.UIThread.RunJobs();

            vm.Nodes.First(n => n.CategoryName == "English").IsActive = false;
            window.Close();
            Dispatcher.UIThread.RunJobs();

            Assert.False(Se.Settings.Edit.MultipleReplace.Categories.First(c => c.Name == "English").IsActive);

            var reopened = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        reopened.PreviewIntervalMs = 25; // the tests wait for the preview timer; see PreviewIntervalMs
            Assert.False(reopened.Nodes.First(n => n.CategoryName == "English").IsActive);
            Assert.True(reopened.Nodes.First(n => n.CategoryName == "Dutch").IsActive);
        }
        finally
        {
            Se.Settings.Edit.MultipleReplace.Categories.Clear();
        }
    }

    // Imported rules used to come back without a parent link, which silently disabled duplicate,
    // insert before / after, delete and the four move commands until the window was reopened.
    [AvaloniaFact]
    public void ImportedRules_KnowTheirCategory()
    {
        var item = new CategoryImportExportItem
        {
            Categories = new()
            {
                new CategoryImportExportItem.RuleImportExportCategory
                {
                    Name = "imported",
                    Rules = new()
                    {
                        new CategoryImportExportItem.RuleImportExportItem
                        {
                            Find = "colour",
                            ReplaceWith = "color",
                            IsActive = true,
                            Type = nameof(MultipleReplaceType.CaseInsensitive),
                        },
                    },
                },
            },
        };

        var method = typeof(CategoryImportExportItem).GetMethod("RuleTreeNodeList", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var nodes = (System.Collections.Generic.List<RuleTreeNode>)method.Invoke(item, null)!;

        var category = Assert.Single(nodes);
        var rule = Assert.Single(category.SubNodes!);
        Assert.Same(category, rule.Parent);
    }
}
