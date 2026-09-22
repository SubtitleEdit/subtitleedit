using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace UITests.Features.Edit;

/// <summary>
/// #15165: after Replace all the window itself must say how many occurrences were replaced (the
/// main-window status bar sits under the dialog and was missed), and Replace &amp; find next keeps a
/// running total.
/// </summary>
public class ReplaceWindowFeedbackTests
{
    [AvaloniaFact]
    public void ReplaceAll_ShowsTotalInWindow()
    {
        var vm = new ReplaceViewModel();
        vm.SearchText = "foo";

        vm.ReportReplaceAll(12);

        Assert.Equal(12, vm.ReplacedCount);
        Assert.Equal(string.Format(Se.Language.Edit.Find.ReplacedXOccurrences, 12), vm.CountResult);
        Assert.Equal(IconNames.CheckCircle, vm.ResultIcon);
    }

    [AvaloniaFact]
    public void ReplaceAll_WithNothingToReplace_SaysNoMatches()
    {
        var vm = new ReplaceViewModel();
        vm.SearchText = "foo";

        vm.ReportReplaceAll(0);

        Assert.Equal(Se.Language.General.FoundNoMatches, vm.CountResult);
        Assert.Equal(IconNames.Information, vm.ResultIcon);
    }

    [AvaloniaFact]
    public void ReplaceAndFindNext_AccumulatesAcrossPresses()
    {
        var vm = new ReplaceViewModel();
        vm.SearchText = "foo";

        vm.ReportReplaced(1);
        Assert.Equal(Se.Language.Edit.Find.ReplacedOneOccurrence, vm.CountResult);

        vm.ReportReplaced(1);
        vm.ReportReplaced(1);

        Assert.Equal(3, vm.ReplacedCount);
        Assert.Equal(string.Format(Se.Language.Edit.Find.ReplacedXOccurrences, 3), vm.CountResult);
    }

    [AvaloniaFact]
    public void ReplaceAndFindNext_WithoutAChange_LeavesTallyAlone()
    {
        var vm = new ReplaceViewModel();
        vm.SearchText = "foo";
        vm.ReportReplaced(1);

        vm.ReportReplaced(0);

        Assert.Equal(1, vm.ReplacedCount);
        Assert.Equal(Se.Language.Edit.Find.ReplacedOneOccurrence, vm.CountResult);
    }

    [AvaloniaFact]
    public void NewSearchText_ResetsTallyAndClearsMessage()
    {
        var vm = new ReplaceViewModel();
        vm.SearchText = "foo";
        vm.ReportReplaced(1);
        vm.ReportReplaced(1);

        vm.SearchText = "bar";

        Assert.Equal(0, vm.ReplacedCount);
        Assert.Equal(string.Empty, vm.CountResult);
    }

    [AvaloniaFact]
    public void FindNext_ClearsMessage_ButKeepsTally()
    {
        var vm = new ReplaceViewModel();
        vm.SearchText = "foo";
        vm.ReportReplaced(1);

        vm.FindNextCommand.Execute(null);

        Assert.Equal(string.Empty, vm.CountResult);
        Assert.Equal(1, vm.ReplacedCount);

        // The next single replace continues the count rather than restarting at one.
        vm.ReportReplaced(1);
        Assert.Equal(string.Format(Se.Language.Edit.Find.ReplacedXOccurrences, 2), vm.CountResult);
    }

    [AvaloniaFact]
    public void ResultLine_IsHidden_UntilThereIsAMessage()
    {
        var vm = new ReplaceViewModel();
        vm.RefreshSubtitles(["Hello world"]);
        var window = new ReplaceWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var resultPanel = window.GetLogicalDescendants().OfType<StackPanel>().Single(p => p.Name == FindWindowParts.ResultPanelName);
            var resultText = resultPanel.Children.OfType<TextBlock>().Single();
            Assert.False(resultPanel.IsVisible);

            vm.ReportReplaceAll(2);
            Dispatcher.UIThread.RunJobs();

            Assert.True(resultPanel.IsVisible);
            Assert.Equal(string.Format(Se.Language.Edit.Find.ReplacedXOccurrences, 2), resultText.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ReplaceWindow_HasHistoryDropdown_LikeFind()
    {
        var vm = new ReplaceViewModel();
        vm.RefreshSubtitles(["Hello world"]);
        var window = new ReplaceWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var historyButton = window.GetLogicalDescendants().OfType<Button>().First(b => b.Flyout != null);
            Assert.False(historyButton.IsVisible);

            vm.SearchHistory.Add("foo");
            Assert.True(historyButton.IsVisible);
            Assert.Single(((MenuFlyout)historyButton.Flyout!).Items);

            vm.ShowHistoryCommand.Execute("foo");
            Assert.Equal("foo", vm.SearchText);
        }
        finally
        {
            window.Close();
        }
    }
}

/// <summary>
/// The Find/Replace buttons now carry icons. The icon helper used to flatten the button content
/// with ToString(), which for the AccessText that MakeButton builds for a "_Find next" label would
/// have printed the type name and dropped the Alt underline.
/// </summary>
public class IconButtonAccessKeyTests
{
    [AvaloniaFact]
    public void WithIconLeft_KeepsAccessTextAndHotKey()
    {
        var button = UiUtil.MakeButton("_Find next", new RelayCommand(() => { })).WithIconLeft(IconNames.ChevronRight);

        var accessText = Assert.Single(button.GetLogicalDescendants().OfType<AccessText>());
        Assert.Equal("_Find next", accessText.Text);
        Assert.Equal(Key.F, button.HotKey?.Key);
        Assert.Equal("Find next", AutomationProperties.GetName(button));
    }

    [AvaloniaFact]
    public void WithIconLeft_PlainLabel_StaysPlain()
    {
        var button = UiUtil.MakeButton("Count", new RelayCommand(() => { })).WithIconLeft(IconNames.Counter);

        Assert.Empty(button.GetLogicalDescendants().OfType<AccessText>());
        Assert.Equal("Count", button.GetLogicalDescendants().OfType<TextBlock>().Single().Text);
        Assert.Equal("Count", AutomationProperties.GetName(button));
    }

    [AvaloniaFact]
    public void ReplaceWindow_IconButtons_StillFireAltAccessKeys()
    {
        var vm = new ReplaceViewModel();
        vm.RefreshSubtitles(["Hello world"]);
        var window = new ReplaceWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var hotKeys = window.GetLogicalDescendants().OfType<Button>()
                .Where(b => b.HotKey != null)
                .Select(b => b.HotKey!.Key)
                .ToList();

            Assert.Contains(Key.F, hotKeys);
            Assert.Contains(Key.R, hotKeys);
            Assert.Contains(Key.A, hotKeys);
        }
        finally
        {
            window.Close();
        }
    }
}
