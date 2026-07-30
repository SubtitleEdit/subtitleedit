using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using SkiaSharp;

namespace UITests.Features.Ocr;

/// <summary>
/// TableView pilot #2 (after Show history, #12704): the OCR subtitle grid is SE's heaviest -
/// virtualized template columns with a bitmap per row. Asserts the behaviour the DataGrid it
/// replaced provided: columns declared, rows realized and virtualized, cell templates
/// resolving images, and selection round-tripping to the view model.
/// </summary>
public class OcrTableViewTests
{
    private sealed class FakeOcrSubtitle : IOcrSubtitle
    {
        public int Count { get; init; }

        public SKBitmap GetBitmap(int index)
        {
            var bitmap = new SKBitmap(120, 40);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);
            return bitmap;
        }

        public TimeSpan GetStartTime(int index) => TimeSpan.FromSeconds(index * 4);
        public TimeSpan GetEndTime(int index) => TimeSpan.FromSeconds(index * 4 + 2);

        public List<OcrSubtitleItem> MakeOcrSubtitleItems()
        {
            var items = new List<OcrSubtitleItem>(Count);
            for (var i = 0; i < Count; i++)
            {
                items.Add(new OcrSubtitleItem(this, i));
            }

            return items;
        }

        public bool GetIsForced(int index) => false;
        public SKPointI GetPosition(int index) => new(0, 0);
        public SKSizeI GetScreenSize(int index) => new(1920, 1080);
    }

    private static OcrViewModel MakeViewModel(int itemCount)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<OcrViewModel>();

        var source = new FakeOcrSubtitle { Count = itemCount };
        foreach (var item in source.MakeOcrSubtitleItems())
        {
            item.Text = $"Line {item.Number}";
            vm.OcrSubtitleItems.Add(item);
        }

        return vm;
    }

    private static TableView GetTableView(Window window) =>
        window.GetVisualDescendants().OfType<TableView>().Single();

    private static OcrWindow ShowWindow(OcrViewModel vm)
    {
        var window = new OcrWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        return window;
    }

    [AvaloniaFact]
    public void OcrWindow_TableViewDeclaresColumnsAndBindsItems()
    {
        var vm = MakeViewModel(3);
        var window = ShowWindow(vm);
        var tableView = GetTableView(window);

        // No forced subtitles in the fake source, so the Forced column is not added.
        Assert.Equal(5, tableView.Columns!.Count);
        Assert.Equal(3, tableView.ItemsSource!.Cast<object>().Count());

        // TableView has no content-based sizing (Auto acts as 1*), so the narrow columns
        // are pixel-sized from their widest content and only Text is star-sized.
        Assert.All(tableView.Columns.Take(4), c => Assert.True(c.Width.IsAbsolute && c.Width.Value > 0));
        Assert.True(tableView.Columns[^1].Width.IsStar);

        // The image column tracks the Ctrl+plus/minus zoom via ImageMaxWidth.
        var imageWidthBefore = tableView.Columns[3].Width.Value;
        vm.ImageMaxWidth *= 1.1;
        Dispatcher.UIThread.RunJobs();
        Assert.True(tableView.Columns[3].Width.Value > imageWidthBefore);

        window.Close();
    }

    [AvaloniaFact]
    public void OcrWindow_RowsRealizeWithImageAndText()
    {
        var vm = MakeViewModel(3);
        var window = ShowWindow(vm);
        var tableView = GetTableView(window);

        var firstRow = tableView.GetVisualDescendants().OfType<TableViewRow>().First();
        var cells = firstRow.GetVisualDescendants().OfType<TableViewCell>().ToList();
        Assert.Equal(5, cells.Count);

        // Text cells resolve through TableViewColumn.Binding.
        var texts = firstRow.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
        Assert.Contains("1", texts); // number column
        Assert.Contains("Line 1", texts); // text column template

        // The image cell template produced an Image with the fake bitmap.
        Assert.Contains(firstRow.GetVisualDescendants().OfType<Image>(), i => i.Source != null);

        window.Close();
    }

    [AvaloniaFact]
    public void OcrWindow_SelectionRoundTripsToViewModel()
    {
        var vm = MakeViewModel(5);
        var window = ShowWindow(vm);
        var tableView = GetTableView(window);

        tableView.SelectedIndex = 2;
        Dispatcher.UIThread.RunJobs();

        Assert.Same(vm.OcrSubtitleItems[2], vm.SelectedOcrSubtitleItem);

        vm.SelectedOcrSubtitleItem = vm.OcrSubtitleItems[4];
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(4, tableView.SelectedIndex);

        window.Close();
    }

    [AvaloniaFact]
    public void OcrWindow_VirtualizesLargeItemSources()
    {
        var vm = MakeViewModel(2000);
        var window = ShowWindow(vm);
        var tableView = GetTableView(window);

        var realized = tableView.GetVisualDescendants().OfType<TableViewRow>().Count();
        Assert.True(realized < 100, $"Expected only visible rows to be realized, got {realized}");

        // Scrolling to the far end realizes rows there instead of the top.
        tableView.ScrollIntoView(1999);
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var lastRowTexts = tableView.GetVisualDescendants().OfType<TableViewRow>()
            .SelectMany(r => r.GetVisualDescendants().OfType<TextBlock>())
            .Select(t => t.Text)
            .ToList();
        Assert.Contains("Line 2000", lastRowTexts);

        window.Close();
    }

    [AvaloniaFact]
    public void OcrWindow_MultiSelectAddsToSelectedItems()
    {
        var vm = MakeViewModel(10);
        var window = ShowWindow(vm);
        var tableView = GetTableView(window);

        // Extended selection is native ListBox behavior now (DataGridCheckboxMultiSelect
        // was DataGrid-specific); the VM reads SubtitleGrid.SelectedItems for bulk actions.
        tableView.SelectedItems!.Clear();
        tableView.SelectedItems.Add(vm.OcrSubtitleItems[1]);
        tableView.SelectedItems.Add(vm.OcrSubtitleItems[2]);
        tableView.SelectedItems.Add(vm.OcrSubtitleItems[3]);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(3, vm.SubtitleGrid.SelectedItems!.Count);

        window.Close();
    }
}
