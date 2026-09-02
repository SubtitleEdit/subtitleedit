using System.IO.Compression;
using System.Text;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// Opening a spreadsheet (drag and drop, or the "Spreadsheet files" type in the open dialog)
/// imports it: no SubtitleFormat claims .xlsx/.ods, so this only works through the
/// UnknownFormatImporter fallback at the end of SubtitleOpen (#14168).
/// </summary>
public class SubtitleOpenSpreadsheetTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public SubtitleOpenSpreadsheetTests()
    {
        // SubtitleOpen walks the binary formats, one of which asks for code page 850 - the app
        // registers the provider in Program.cs, the headless test host does not.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _tempDirectory = Path.Combine(Path.GetTempPath(), "SubtitleEdit.UITests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    private (Window Window, MainViewModel Vm) ShowEmptyMainWindow()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        _windows.Add(window);
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        return (window, vm);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private string WriteZip(string name, string entryName, string entryContent)
    {
        var fileName = Path.Combine(_tempDirectory, name);
        using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
            writer.Write(entryContent);
        }

        return fileName;
    }

    private string WriteXlsx(string name)
    {
        return WriteZip(name, "xl/worksheets/sheet1.xml", """
            <?xml version="1.0" encoding="UTF-8"?>
            <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
              <sheetData>
                <row r="1"><c r="A1" t="inlineStr"><is><t>Start</t></is></c><c r="B1" t="inlineStr"><is><t>End</t></is></c><c r="C1" t="inlineStr"><is><t>Text</t></is></c></row>
                <row r="2"><c r="A2" t="inlineStr"><is><t>00:00:01.000</t></is></c><c r="B2" t="inlineStr"><is><t>00:00:03.500</t></is></c><c r="C2" t="inlineStr"><is><t>Hello there.</t></is></c></row>
                <row r="3"><c r="A3" t="inlineStr"><is><t>00:00:04.000</t></is></c><c r="B3" t="inlineStr"><is><t>00:00:06.000</t></is></c><c r="C3" t="inlineStr"><is><t>General Kenobi!</t></is></c></row>
              </sheetData>
            </worksheet>
            """);
    }

    private string WriteOds(string name)
    {
        return WriteZip(name, "content.xml", """
            <?xml version="1.0" encoding="UTF-8"?>
            <office:document-content
                xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
                xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0"
                xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
              <office:body><office:spreadsheet>
                <table:table table:name="Sheet1">
                  <table:table-row>
                    <table:table-cell><text:p>Start</text:p></table:table-cell>
                    <table:table-cell><text:p>End</text:p></table:table-cell>
                    <table:table-cell><text:p>Text</text:p></table:table-cell>
                  </table:table-row>
                  <table:table-row>
                    <table:table-cell><text:p>00:00:01.000</text:p></table:table-cell>
                    <table:table-cell><text:p>00:00:03.500</text:p></table:table-cell>
                    <table:table-cell><text:p>Hello there.</text:p></table:table-cell>
                  </table:table-row>
                  <table:table-row>
                    <table:table-cell><text:p>00:00:04.000</text:p></table:table-cell>
                    <table:table-cell><text:p>00:00:06.000</text:p></table:table-cell>
                    <table:table-cell><text:p>General Kenobi!</text:p></table:table-cell>
                  </table:table-row>
                </table:table>
              </office:spreadsheet></office:body>
            </office:document-content>
            """);
    }

    private string WriteCsv(string name)
    {
        var fileName = Path.Combine(_tempDirectory, name);
        File.WriteAllText(fileName,
            "Start,End,Text" + Environment.NewLine +
            "00:00:01.000,00:00:03.500,\"Hello there.\"" + Environment.NewLine +
            "00:00:04.000,00:00:06.000,\"General Kenobi!\"" + Environment.NewLine);
        return fileName;
    }

    private static void AssertImported(MainViewModel vm)
    {
        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Equal("Hello there.", vm.Subtitles[0].Text);
        Assert.Equal(1000, vm.Subtitles[0].StartTime.TotalMilliseconds, 0);
        Assert.Equal(3500, vm.Subtitles[0].EndTime.TotalMilliseconds, 0);
        Assert.Equal("General Kenobi!", vm.Subtitles[1].Text);
    }

    [AvaloniaFact]
    public async Task OpeningAnXlsxImportsIt()
    {
        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(WriteXlsx("sheet.xlsx"), skipLoadVideo: true);
        Settle(window);

        AssertImported(vm);
    }

    [AvaloniaFact]
    public async Task OpeningAnOdsImportsIt()
    {
        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(WriteOds("sheet.ods"), skipLoadVideo: true);
        Settle(window);

        AssertImported(vm);
    }

    [AvaloniaFact]
    public async Task OpeningACsvImportsIt()
    {
        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(WriteCsv("sheet.csv"), skipLoadVideo: true);
        Settle(window);

        AssertImported(vm);
    }
}
