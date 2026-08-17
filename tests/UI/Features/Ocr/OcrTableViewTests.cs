using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Logic;
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

        // Smaller than the view model's ImageMaxWidth/Height, so the thumbnails are shown at
        // their natural size; the zoom test asks for a wider one, which the maxes constrain.
        public SKSizeI BitmapSize { get; init; } = new(120, 40);

        public SKBitmap GetBitmap(int index)
        {
            var bitmap = new SKBitmap(BitmapSize.Width, BitmapSize.Height);
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

    private static OcrViewModel MakeViewModel(int itemCount, SKSizeI? bitmapSize = null)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<OcrViewModel>();

        var source = bitmapSize is { } size
            ? new FakeOcrSubtitle { Count = itemCount, BitmapSize = size }
            : new FakeOcrSubtitle { Count = itemCount };
        foreach (var item in source.MakeOcrSubtitleItems())
        {
            item.Text = $"Line {item.Number}";
            vm.OcrSubtitleItems.Add(item);
        }

        return vm;
    }

    private static TableView GetTableView(Window window) =>
        window.GetVisualDescendants().OfType<TableView>().Single();

    /// <summary>Index of the topmost row still inside the viewport. The TableView template
    /// pins the column header inside the ScrollViewer, so rows are measured against the
    /// content presenter - the ScrollViewer's own origin is a header height above.</summary>
    private static int FirstVisibleIndex(TableView grid, ScrollViewer scrollViewer)
    {
        var origin = (Visual?)scrollViewer.Presenter ?? scrollViewer;
        var best = -1;
        var bestTop = double.MaxValue;
        foreach (var row in grid.GetRealizedContainers().OfType<TableViewRow>())
        {
            var top = ((Visual)row).TranslatePoint(new Point(0, 0), origin)?.Y;
            if (top == null || row.Bounds.Height <= 0 || top.Value + row.Bounds.Height <= 0.5)
            {
                continue;
            }

            if (top.Value < bestTop)
            {
                bestTop = top.Value;
                best = grid.IndexFromContainer(row);
            }
        }

        return best;
    }

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
    public void OcrWindow_UsesIndexMappedScrollBar()
    {
        // Same fix as the main subtitle grid (#13579): the rows here hold a bitmap each and
        // vary even more in height, so the virtualizing panel's pixel-extent estimate - and
        // with it the native thumb - moves on every scroll. The grid is wrapped in
        // TableViewIndexScrollBar, which hides the native vertical bar and maps its own to
        // row indices.
        var vm = MakeViewModel(500);
        var window = ShowWindow(vm);
        var tableView = GetTableView(window);

        var wrapper = window.GetVisualDescendants().OfType<TableViewIndexScrollBar>().Single();
        var bar = wrapper.BarForTest;
        var scrollViewer = tableView.GetVisualDescendants().OfType<ScrollViewer>().First();

        var nativeBar = tableView.GetVisualDescendants().OfType<ScrollBar>()
            .First(s => s.Orientation == Orientation.Vertical && !ReferenceEquals(s, bar));
        Assert.False(nativeBar.IsVisible, "the native pixel-mapped vertical bar should be hidden");

        // Row units, not pixels: a handful of rows out of 500 are visible, so the maximum
        // sits just below the row count while the pixel extent is many thousands.
        Assert.InRange(bar.Maximum, 400, 499);
        Assert.True(scrollViewer.Extent.Height - scrollViewer.Viewport.Height > bar.Maximum * 2,
            "sanity: the pixel extent should be far larger than the row-based maximum");

        // The bar drives the view in row indices...
        bar.Value = 250;
        wrapper.ApplyPendingForTest();
        window.UpdateLayout();
        var row = tableView.ContainerFromIndex(250);
        Assert.NotNull(row);
        var viewportOrigin = (Visual?)scrollViewer.Presenter ?? scrollViewer;
        var top = ((Visual)row!).TranslatePoint(new Point(0, 0), viewportOrigin)!.Value.Y;
        Assert.InRange(top, -1, 1);

        // ...and scrolling the view drives the bar back, without ever moving backwards.
        var previous = bar.Value;
        for (var i = 0; i < 10; i++)
        {
            scrollViewer.Offset = new Vector(0, scrollViewer.Offset.Y + 120);
            window.UpdateLayout();
            Assert.True(bar.Value >= previous - 0.001,
                $"thumb moved backwards while scrolling down: {previous} -> {bar.Value}");
            previous = bar.Value;
        }

        window.Close();
    }

    [AvaloniaFact]
    public void OcrWindow_ZoomKeepsTheRowAtTheTop()
    {
        // Ctrl+plus/minus re-measures every row (the thumbnails are bound to
        // ImageMaxWidth/Height), and the ScrollViewer keeps its *pixel* offset across that -
        // which lands on a different row: zooming in used to drop the user ~80 rows back,
        // and zooming out far enough pinned the list to its end. The index bar knows which
        // row was at the top, so it is put back there.
        var vm = MakeViewModel(500, new SKSizeI(600, 120)); // wide enough for the maxes to bind
        var window = ShowWindow(vm);
        var tableView = GetTableView(window);
        var wrapper = window.GetVisualDescendants().OfType<TableViewIndexScrollBar>().Single();
        var bar = wrapper.BarForTest;
        var scrollViewer = tableView.GetVisualDescendants().OfType<ScrollViewer>().First();

        double RowHeight() => tableView.GetRealizedContainers().OfType<TableViewRow>()
            .Select(r => r.Bounds.Height).DefaultIfEmpty(0).Max();

        void SettleLayout()
        {
            // Twice: the first pass measures the new row heights, the second runs the
            // re-placement PreserveTopRow posted at Loaded priority.
            for (var i = 0; i < 2; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }
        }

        bar.Value = 250;
        wrapper.ApplyPendingForTest();
        window.UpdateLayout();
        Assert.Equal(250, FirstVisibleIndex(tableView, scrollViewer));
        var heightBefore = RowHeight();

        // Zoom in five notches, like holding Ctrl+plus.
        for (var i = 0; i < 5; i++)
        {
            vm.ImageMaxHeight *= 1.1;
            vm.ImageMaxWidth *= 1.1;
        }

        SettleLayout();

        Assert.True(RowHeight() > heightBefore, "sanity: zooming in should make the rows taller");
        Assert.Equal(250, FirstVisibleIndex(tableView, scrollViewer));
        Assert.Equal(250, bar.Value, 1);

        // ...and ten notches back out, which shrinks the pixel extent below the old offset.
        for (var i = 0; i < 10; i++)
        {
            vm.ImageMaxHeight *= 0.9;
            vm.ImageMaxWidth *= 0.9;
        }

        SettleLayout();

        Assert.True(RowHeight() < heightBefore, "sanity: zooming out should make the rows shorter");
        Assert.Equal(250, FirstVisibleIndex(tableView, scrollViewer));
        Assert.Equal(250, bar.Value, 1);

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
