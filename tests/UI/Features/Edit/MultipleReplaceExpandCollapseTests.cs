using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Linq;

namespace UITests.Features.Edit;

/// <summary>
/// Which rule categories are expanded is remembered in the settings. It used to be pushed onto the
/// tree item containers from <c>Initialize</c>, which runs before the window is even constructed -
/// so there was nothing to push it onto and every category came back collapsed (#13526). It is now
/// carried by RuleTreeNode.IsExpanded, bound two-way from the tree's item container theme.
/// </summary>
public class MultipleReplaceExpandCollapseTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static void SeedSettings(bool dutchExpanded, bool englishExpanded)
    {
        Se.Settings.Edit.MultipleReplace.Categories.Clear();
        foreach (var (name, isExpanded) in new[] { ("Dutch", dutchExpanded), ("English", englishExpanded) })
        {
            var c = new SeEditMultipleReplace.MultipleReplaceCategory
            {
                Name = name,
                IsActive = true,
                IsExpanded = isExpanded,
            };
            c.Rules.Add(new MultipleReplaceRule
            {
                Active = true,
                Find = $"{name}1",
                ReplaceWith = $"repl{name}1",
                Type = MultipleReplaceType.CaseInsensitive,
            });

            Se.Settings.Edit.MultipleReplace.Categories.Add(c);
        }
    }

    private static MultipleReplaceViewModel NewViewModel()
    {
        return new MultipleReplaceViewModel(new WindowService(new NullServiceProvider()), new FileHelper());
    }

    private static TreeViewItem CategoryContainer(MultipleReplaceViewModel vm, string categoryName)
    {
        return vm.RulesTreeView.GetVisualDescendants()
            .OfType<TreeViewItem>()
            .First(i => i.DataContext is RuleTreeNode { IsCategory: true } node && node.CategoryName == categoryName);
    }

    [AvaloniaFact]
    public void RememberedExpansion_IsRestoredWhenTheWindowOpens()
    {
        SeedSettings(dutchExpanded: true, englishExpanded: false);
        var vm = NewViewModel();

        // The order the real dialog uses: WindowService configures the view model - which is where
        // Initialize runs - and only then constructs and shows the window (#13526).
        vm.Initialize(new Nikse.SubtitleEdit.Core.Common.Subtitle());
        var window = new MultipleReplaceWindow(vm);
        try
        {
            // Anything Initialize queued runs before the tree has laid out, so before a single
            // item container exists - which is what made the old restore a no-op.
            Dispatcher.UIThread.RunJobs();

            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.True(CategoryContainer(vm, "Dutch").IsExpanded);
            Assert.False(CategoryContainer(vm, "English").IsExpanded);
        }
        finally
        {
            window.Close();
            Se.Settings.Edit.MultipleReplace.Categories.Clear();
        }
    }

    [AvaloniaFact]
    public void ExpandingInTheTree_SurvivesCloseAndReopen()
    {
        SeedSettings(dutchExpanded: false, englishExpanded: false);
        try
        {
            var vm = NewViewModel();
            var window = new MultipleReplaceWindow(vm);
            window.Show();
            Dispatcher.UIThread.RunJobs();

            // What a click on the chevron does.
            CategoryContainer(vm, "English").IsExpanded = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(vm.Nodes.First(n => n.CategoryName == "English").IsExpanded);

            window.Close();
            Dispatcher.UIThread.RunJobs();

            Assert.True(Se.Settings.Edit.MultipleReplace.Categories.First(c => c.Name == "English").IsExpanded);
            Assert.False(Se.Settings.Edit.MultipleReplace.Categories.First(c => c.Name == "Dutch").IsExpanded);

            var reopened = NewViewModel();
            var reopenedWindow = new MultipleReplaceWindow(reopened);
            try
            {
                reopenedWindow.Show();
                Dispatcher.UIThread.RunJobs();

                Assert.True(CategoryContainer(reopened, "English").IsExpanded);
                Assert.False(CategoryContainer(reopened, "Dutch").IsExpanded);
            }
            finally
            {
                reopenedWindow.Close();
            }
        }
        finally
        {
            Se.Settings.Edit.MultipleReplace.Categories.Clear();
        }
    }

    [AvaloniaFact]
    public void ExpandAllAndCollapseAll_AreRemembered()
    {
        SeedSettings(dutchExpanded: false, englishExpanded: false);
        try
        {
            var vm = NewViewModel();
            var window = new MultipleReplaceWindow(vm);
            window.Show();
            Dispatcher.UIThread.RunJobs();

            vm.ExpandAllCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.True(CategoryContainer(vm, "Dutch").IsExpanded);
            Assert.True(CategoryContainer(vm, "English").IsExpanded);

            vm.CollapseAllCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.False(CategoryContainer(vm, "Dutch").IsExpanded);
            Assert.False(CategoryContainer(vm, "English").IsExpanded);

            vm.ExpandAllCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            window.Close();
            Dispatcher.UIThread.RunJobs();

            Assert.All(Se.Settings.Edit.MultipleReplace.Categories, c => Assert.True(c.IsExpanded));
        }
        finally
        {
            Se.Settings.Edit.MultipleReplace.Categories.Clear();
        }
    }
}
