using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

namespace UITests.Features.Tools.ChangeCasing;

/// <summary>
/// Selecting a name in Fix names selects the first preview line it changes (#17229), like SE4.
/// </summary>
public class FixNamesSelectHitTests
{
    private static FixNamesViewModel MakeViewModel()
    {
        var vm = new FixNamesViewModel();
        vm.Names.Add(new FixNameItem("US", true));
        vm.Names.Add(new FixNameItem("Patience", true));
        vm.Hits.Add(new FixNameHitItem("", 2, "plug in the usb cable.", "plug in the usb cable.", true));
        vm.Hits.Add(new FixNameHitItem("", 5, "I lived in the us.", "I lived in the US.", true));
        vm.Hits.Add(new FixNameHitItem("", 7, "Thanks, patience.", "Thanks, Patience.", true));
        return vm;
    }

    [AvaloniaFact]
    public void FindFirstHit_MatchesTheNameAsAWord()
    {
        var vm = MakeViewModel();

        vm.SelectedName = vm.Names[0];
        Assert.Equal(5, vm.FindFirstHitForSelectedName()?.LineIndex);

        vm.SelectedName = vm.Names[1];
        Assert.Equal(7, vm.FindFirstHitForSelectedName()?.LineIndex);
    }

    [AvaloniaFact]
    public void FindFirstHit_NoSelectionOrNoHit_IsNull()
    {
        var vm = MakeViewModel();
        Assert.Null(vm.FindFirstHitForSelectedName());

        vm.SelectedName = new FixNameItem("Bob", false);
        Assert.Null(vm.FindFirstHitForSelectedName());
    }

    [AvaloniaFact]
    public void SelectingAName_SelectsItsFirstPreviewLine()
    {
        var vm = MakeViewModel();
        var window = new FixNamesWindow(vm);
        window.Show();

        var tables = window.GetLogicalDescendants().OfType<TableView>().ToList();
        var namesView = tables.Single(t => ReferenceEquals(t.ItemsSource, vm.Names));
        var hitsView = tables.Single(t => ReferenceEquals(t.ItemsSource, vm.Hits));

        namesView.SelectedItem = vm.Names[1];
        Assert.Same(vm.Hits[2], hitsView.SelectedItem);

        namesView.SelectedItem = vm.Names[0];
        Assert.Same(vm.Hits[1], hitsView.SelectedItem);

        window.Close();
    }
}
