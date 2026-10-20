using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Edit;

/// <summary>
/// "Add rules..." enters a whole list of find/replace pairs in one window: Enter adds the pair and
/// puts the cursor back in "Find what", OK hands the list to the category (#16215). Subtitle Edit 4
/// worked that way; a "New rule" dialog per word did not.
/// </summary>
public class MultipleReplaceAddRulesTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private (AddRulesViewModel Vm, AddRulesWindow Window, TextBox FindBox, TextBox ReplaceBox) OpenAddRules()
    {
        var vm = new AddRulesViewModel();
        vm.Initialize("c1", MultipleReplaceType.CaseInsensitive, wholeWord: true);
        var window = new AddRulesWindow(vm);
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var boxes = window.GetVisualDescendants().OfType<TextBox>().ToList();
        var findBox = boxes.First(b => Avalonia.Automation.AutomationProperties.GetName(b) == Nikse.SubtitleEdit.Logic.Config.Se.Language.Edit.MultipleReplace.FindWhat);
        var replaceBox = boxes.First(b => Avalonia.Automation.AutomationProperties.GetName(b) == Nikse.SubtitleEdit.Logic.Config.Se.Language.General.ReplaceWith);
        return (vm, window, findBox, replaceBox);
    }

    private static void Type(Window window, TextBox box, string text)
    {
        box.Focus();
        Dispatcher.UIThread.RunJobs();
        window.KeyTextInput(text);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void Enter_AddsTheRule_ClearsTheBoxes_AndFocusesFindWhat()
    {
        var (vm, window, findBox, replaceBox) = OpenAddRules();

        Type(window, findBox, "colour");
        Type(window, replaceBox, "color");
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        var rule = Assert.Single(vm.Rules);
        Assert.Equal("colour", rule.Find);
        Assert.Equal("color", rule.ReplaceWith);
        Assert.Equal(MultipleReplaceType.CaseInsensitive, rule.Type);
        Assert.True(rule.WholeWord, "type and whole word come from Initialize and stick for every rule");
        Assert.True(rule.Active);

        Assert.Equal(string.Empty, vm.FindWhat);
        Assert.Equal(string.Empty, vm.ReplaceWith);
        Assert.Same(findBox, window.FocusManager?.GetFocusedElement());

        // Nothing to find is nothing to add - the next word can just be typed.
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.Single(vm.Rules);
        Assert.False(vm.OkPressed, "Enter adds a rule, it does not close the window");
    }

    [AvaloniaFact]
    public void Ok_KeepsThePairStillInTheBoxes()
    {
        var (vm, window, findBox, _) = OpenAddRules();

        Type(window, findBox, "first");
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        Type(window, findBox, "second");
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.OkPressed);
        Assert.Equal("first,second", string.Join(",", vm.Rules.Select(r => r.Find)));
    }

    [AvaloniaFact]
    public void CtrlEnter_IsOk()
    {
        var (vm, window, findBox, _) = OpenAddRules();

        Type(window, findBox, "word");
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.Control);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.OkPressed);
        Assert.Equal("word", Assert.Single(vm.Rules).Find);
    }

    [AvaloniaFact]
    public void EditSelected_TakesTheRuleBackIntoTheBoxes()
    {
        var vm = new AddRulesViewModel();
        vm.Initialize("c1", MultipleReplaceType.CaseSensitive, wholeWord: false);
        vm.FindWhat = "teh";
        vm.ReplaceWith = "the";
        vm.AddCommand.Execute(null);
        vm.IsRegularExpression = true; // changed since - the row's own type must come back

        vm.SelectedRule = vm.Rules[0];
        vm.EditSelectedCommand.Execute(null);

        Assert.Empty(vm.Rules);
        Assert.Equal("teh", vm.FindWhat);
        Assert.Equal("the", vm.ReplaceWith);
        Assert.True(vm.IsCaseSensitive);
        Assert.False(vm.IsRegularExpression);
    }

    [AvaloniaFact]
    public void RemoveSelected_SelectsTheNeighbour()
    {
        var vm = new AddRulesViewModel();
        foreach (var word in new[] { "a", "b", "c" })
        {
            vm.FindWhat = word;
            vm.AddCommand.Execute(null);
        }

        vm.SelectedRule = vm.Rules[1];
        vm.RemoveSelectedCommand.Execute(null);

        Assert.Equal("a,c", string.Join(",", vm.Rules.Select(r => r.Find)));
        Assert.Equal("c", vm.SelectedRule?.Find);
    }

    private static (MultipleReplaceViewModel Vm, RuleTreeNode Category) MakeViewModel()
    {
        var vm = new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
        vm.PreviewIntervalMs = 25;
        vm.Nodes.Clear();

        var category = new RuleTreeNode(true) { CategoryName = "c1", IsActive = true, IsExpanded = false };
        for (var r = 1; r <= 2; r++)
        {
            category.SubNodes!.Add(new RuleTreeNode(false)
            {
                Find = $"r{r}",
                IsActive = true,
                Parent = category,
                Type = MultipleReplaceType.CaseInsensitive,
            });
        }

        vm.Nodes.Add(category);
        return (vm, category);
    }

    private static List<MultipleReplaceRule> Rules(params string[] finds)
    {
        return finds.Select(f => new MultipleReplaceRule { Find = f, ReplaceWith = f.ToUpperInvariant(), Active = true }).ToList();
    }

    private static string Order(RuleTreeNode category) => string.Join(",", category.SubNodes!.Select(n => n.Find));

    [AvaloniaFact]
    public void InsertRules_OnACategory_AppendsInOrder_ShowsIt_AndSelectsTheLast()
    {
        var (vm, category) = MakeViewModel();

        vm.InsertRules(category, Rules("a", "b"));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("r1,r2,a,b", Order(category));
        Assert.Same(category, vm.SelectedCategory); // the new rules have to be visible
        Assert.Equal("r1,r2,a,b", Visible(vm));
        Assert.Equal("b", vm.SelectedNode?.Find);
        Assert.All(category.SubNodes!, n => Assert.Same(category, n.Parent));
    }

    [AvaloniaFact]
    public void InsertRules_OnARule_InsertsRightAfterIt()
    {
        var (vm, category) = MakeViewModel();

        vm.InsertRules(category.SubNodes![0], Rules("a", "b"));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("r1,a,b,r2", Order(category));
        Assert.Equal("b", vm.SelectedNode?.Find);
    }

    [AvaloniaFact]
    public void InsertRules_WithNothing_ChangesNothing()
    {
        var (vm, category) = MakeViewModel();

        vm.InsertRules(category, new List<MultipleReplaceRule>());

        Assert.Equal("r1,r2", Order(category));
        Assert.Null(vm.SelectedNode);
    }

    private static string Visible(MultipleReplaceViewModel vm) => string.Join(",", vm.VisibleRules.Select(n => n.Find));

    // #16215: the type-in row above the rules - Enter adds the rule to the end of the category,
    // empties the fields for the next one and keeps type and whole word.
    [AvaloniaFact]
    public void TypeInRow_AddsToTheEndOfTheCategory_AndClearsTheFields()
    {
        var (vm, category) = MakeViewModel();
        vm.SelectedCategory = category;
        vm.NewRuleType = vm.RuleTypes.First(t => t.Type == MultipleReplaceType.CaseSensitive);
        vm.NewWholeWord = true;

        vm.NewFind = "colour";
        vm.NewReplaceWith = "color";
        vm.NewRuleTextBoxKeyDown(null, new KeyEventArgs { Key = Key.Enter, RoutedEvent = InputElement.KeyDownEvent });
        vm.NewFind = "grey";
        vm.NewReplaceWith = "gray";
        vm.AddNewRuleCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("r1,r2,colour,grey", Order(category));
        var added = category.SubNodes![3];
        Assert.Equal("gray", added.ReplaceWith);
        Assert.Equal(MultipleReplaceType.CaseSensitive, added.Type);
        Assert.True(added.WholeWord);
        Assert.True(added.IsActive);
        Assert.Same(category, added.Parent);
        Assert.Equal(string.Empty, vm.NewFind);
        Assert.Equal(string.Empty, vm.NewReplaceWith);
        Assert.Equal(MultipleReplaceType.CaseSensitive, vm.NewRuleType?.Type);
        Assert.Same(added, vm.SelectedNode);
    }

    [AvaloniaFact]
    public void TypeInRow_WithoutFindText_AddsNothing()
    {
        var (vm, category) = MakeViewModel();
        vm.SelectedCategory = category;

        vm.NewReplaceWith = "x";
        vm.AddNewRuleCommand.Execute(null);

        Assert.Equal("r1,r2", Order(category));
    }

    // Adding clears a filter, otherwise the rule just added could be hidden.
    [AvaloniaFact]
    public void TypeInRow_ClearsTheFilter()
    {
        var (vm, category) = MakeViewModel();
        vm.SelectedCategory = category;
        vm.RulesFilter = "r1";
        Assert.Equal("r1", Visible(vm));

        vm.NewFind = "new";
        vm.AddNewRuleCommand.Execute(null);

        Assert.Equal(string.Empty, vm.RulesFilter);
        Assert.Equal("r1,r2,new", Visible(vm));
    }

    // New rules go to the end, so "Newest first" shows the list bottom up - display only, the
    // rules still run in their own order.
    [AvaloniaFact]
    public void NewestFirst_ShowsTheLastRuleOnTop_WithoutReordering()
    {
        var (vm, category) = MakeViewModel();
        vm.SelectedCategory = category;
        vm.NewestFirst = true;

        vm.NewFind = "new";
        vm.AddNewRuleCommand.Execute(null);

        Assert.Equal("new,r2,r1", Visible(vm));
        Assert.Equal("r1,r2,new", Order(category));
        Assert.Equal(new[] { 3, 2, 1 }, vm.VisibleRules.Select(n => n.Number));
    }

    // In the bottom-up list, up on screen is later in the run order.
    [AvaloniaFact]
    public void NewestFirst_MoveUpMovesUpOnScreen()
    {
        var (vm, category) = MakeViewModel();
        vm.SelectedCategory = category;
        vm.NewestFirst = true;

        vm.SelectedNode = category.SubNodes![0]; // r1, at the bottom of the list
        vm.MoveSelectedRuleUpCommand.Execute(null);

        Assert.Equal("r2,r1", Order(category));
        Assert.Equal("r1,r2", Visible(vm));
    }

    [AvaloniaFact]
    public void Filter_MatchesFindReplaceAndDescription_AndBlocksReordering()
    {
        var (vm, category) = MakeViewModel();
        vm.SelectedCategory = category;
        category.SubNodes![1].Description = "spelling";

        vm.RulesFilter = "SPELL";
        Assert.Equal("r2", Visible(vm));
        Assert.False(vm.CanReorderRules);

        vm.SelectedNode = category.SubNodes[1];
        vm.MoveSelectedRuleUpCommand.Execute(null);
        Assert.Equal("r1,r2", Order(category));

        vm.RulesFilter = string.Empty;
        Assert.True(vm.CanReorderRules);
    }

    // Deleting the selected rule selects the one that took its place, so Delete can be held down.
    [AvaloniaFact]
    public void RemoveSelectedRule_SelectsTheNextOne()
    {
        var oldPrompt = Se.Settings.General.PromptBeforeDelete;
        Se.Settings.General.PromptBeforeDelete = false;
        try
        {
            var (vm, category) = MakeViewModel();
            vm.SelectedCategory = category;
            vm.SelectedNode = category.SubNodes![0];

            vm.RemoveSelectedRuleCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("r2", Order(category));
            Assert.Same(category.SubNodes[0], vm.SelectedNode);
        }
        finally
        {
            Se.Settings.General.PromptBeforeDelete = oldPrompt;
        }
    }
}
