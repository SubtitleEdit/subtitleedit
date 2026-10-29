using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Files.ExportDialogueList;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Files.ExportDialogueList;

public class ExportDialogueListViewModelTests
{
    private static ExportDialogueListViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        return services.BuildServiceProvider().GetRequiredService<ExportDialogueListViewModel>();
    }

    private static List<DialogueListRow> Rows(string originalText = "") => new()
    {
        new DialogueListRow(1, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), "Hello", originalText, "Anna", string.Empty, 0, false),
        new DialogueListRow(2, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(6), "World", originalText, string.Empty, string.Empty, 0, false),
    };

    private static List<DialogueListColumn> Checked(ExportDialogueListViewModel vm) =>
        vm.Columns.Where(c => c.IsChecked).Select(c => c.Column).ToList();

    [AvaloniaFact]
    public void FirstExport_TicksDefaultsWithData()
    {
        Se.Settings.File.ExportDialogueList = new SeExportDialogueList();
        var vm = MakeViewModel();
        vm.Initialize(Rows(), null, 25);

        // Actor has data, Original text has none (and cannot be ticked without an original)
        Assert.Equal(new[] { DialogueListColumn.Number, DialogueListColumn.Start, DialogueListColumn.End, DialogueListColumn.Text, DialogueListColumn.Actor }, Checked(vm));
        Assert.False(vm.Columns.Single(c => c.Column == DialogueListColumn.OriginalText).IsAvailable);
        Assert.Equal(Enum.GetValues<DialogueListColumn>().Length, vm.Columns.Count);
    }

    [AvaloniaFact]
    public void SavedOrderAndSelection_AreRestored()
    {
        Se.Settings.File.ExportDialogueList = new SeExportDialogueList
        {
            ColumnOrder = new List<string> { "Text", "Start" },
            SelectedColumns = new List<string> { "Text", "Start", "Style" },
            TimeCodeFormat = "Seconds",
            LineBreaksInCell = false,
        };
        var vm = MakeViewModel();
        vm.Initialize(Rows(), null, 25);

        Assert.Equal(DialogueListColumn.Text, vm.Columns[0].Column);
        Assert.Equal(DialogueListColumn.Start, vm.Columns[1].Column);
        // Style was chosen before, but this file has no styles - not ticked this time
        Assert.Equal(new[] { DialogueListColumn.Text, DialogueListColumn.Start }, Checked(vm));
        Assert.Equal((int)DialogueListTimeCodeFormat.Seconds, vm.SelectedTimeCodeFormatIndex);
        Assert.False(vm.LineBreaksInCell);
        Assert.Equal(new[] { "Text", "Start" }, vm.GetPreview().Headers);
    }

    [AvaloniaFact]
    public void MoveUp_ReordersColumnsAndPreview()
    {
        Se.Settings.File.ExportDialogueList = new SeExportDialogueList();
        var vm = MakeViewModel();
        vm.Initialize(Rows("Hej"), null, 25);
        var changed = 0;
        vm.PreviewChanged += () => changed++;

        vm.SelectedColumn = vm.Columns.Single(c => c.Column == DialogueListColumn.Text);
        for (var i = 0; i < 3; i++) // past the unticked Gap and Duration, then End
        {
            vm.MoveUpCommand.Execute(null);
        }

        Assert.Equal(new[] { "#", "Start", "Text", "End", "Original text", "Actor" }, vm.GetPreview().Headers);
        Assert.True(changed > 0);
    }

    [AvaloniaFact]
    public void Window_ShowsPreviewRows()
    {
        Se.Settings.File.ExportDialogueList = new SeExportDialogueList();
        var vm = MakeViewModel();
        vm.Initialize(Rows(), null, 25);
        var window = new ExportDialogueListWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdatePreview();

            var texts = window.GetVisualDescendants().OfType<Avalonia.Controls.TextBlock>().Select(t => t.Text).ToList();
            Assert.Contains("Start", texts);
            Assert.Contains("00:00:01:00", texts);
            Assert.Contains("Anna", texts);
        }
        finally
        {
            window.Close();
        }
    }
}
