using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// Changing one font size in the ASSA styles dialog must leave every line's style alone (#15126).
/// Runs the real command: the file is opened from disk, an original with display-only rows is
/// imported, the dialog window is driven and closed with OK or Apply. Two things used to send the
/// whole file to the first style: a header that did not spell "ScriptType: v4.00+" exactly (the
/// dialog then wrote the stock header back, styles gone), and style names with the SSA "*" prefix
/// (stripped on the way back, so no name matched the header).
/// </summary>
public class AssaStylesDialogKeepsLineStylesTests
{
    private static readonly string[] LineStyles =
        ["Gothic", "Gothic", "Default", "Default", "Default", "Gothic", "B Gothic", "Default", "Gothic", "Default"];

    [AvaloniaTheory]
    [InlineData("ScriptType: v4.00+", "")]
    [InlineData("", "")] // no ScriptType line at all
    [InlineData("ScriptType: V4.00+", "")]
    [InlineData("ScriptType: v4.00+", "*")] // SSA-style "*Name" in header and events
    public async Task FontSizeChange_ThenOk_KeepsEveryLineStyle(string scriptTypeLine, string star)
    {
        await Run("[Script Info]", scriptTypeLine, star, useApply: false);
    }

    [AvaloniaFact]
    public async Task FontSizeChange_ThenApply_WithLowercaseScriptInfo_KeepsEveryLineStyle()
    {
        await Run("[script info]", "ScriptType: v4.00+", string.Empty, useApply: true);
    }

    private static async Task Run(string scriptInfoLine, string scriptTypeLine, string star, bool useApply)
    {
        var (window, vm) = CreateMainViewModel();
        var dir = Path.Combine(Path.GetTempPath(), $"se-15126-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            var assFile = Path.Combine(dir, "jpn.ass");
            await File.WriteAllTextAsync(assFile, MakeAss(scriptInfoLine, scriptTypeLine, star));
            await vm.SubtitleOpen(assFile, skipLoadVideo: true);
            Dispatcher.UIThread.RunJobs();

            var expected = LineStyles.Select(s => star + s).ToArray();
            Assert.Equal(expected, vm.Subtitles.Select(p => p.Style).ToArray());

            // An original with three lines the working subtitle does not have: display-only rows
            // sit between the working rows, so the dialog's subtitle and the grid differ in length.
            var reference = new Subtitle();
            for (var i = 0; i < LineStyles.Length; i++)
            {
                reference.Paragraphs.Add(new Paragraph($"Line {i + 1}", (i * 2 + 1) * 1000, (i * 2 + 2.5) * 1000));
            }

            reference.Paragraphs.Add(new Paragraph("Extra A", 3400, 3900));
            reference.Paragraphs.Add(new Paragraph("Extra B", 9400, 9900));
            reference.Paragraphs.Add(new Paragraph("Extra C", 15400, 15900));
            reference.Paragraphs.Sort((a, b) => a.StartTime.TotalMilliseconds.CompareTo(b.StartTime.TotalMilliseconds));
            var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
            InvokeImportOriginalSubtitle(vm, Path.Combine(dir, "eng.srt"), reference, match, isReadOnly: false);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(3, vm.Subtitles.Count(p => p.IsReferenceOnly));

            var rows = vm.Subtitles.ToList();
            var stylesBefore = vm.Subtitles.Select(p => p.Style).ToArray();

            var dialogTask = vm.ShowAssaStylesCommand.ExecuteAsync(null);
            Dispatcher.UIThread.RunJobs();
            var dialogWindow = window.OwnedWindows.OfType<AssaStylesWindow>().Single();
            var dialogVm = (AssaStylesViewModel)dialogWindow.DataContext!;
            Assert.Equal(3, dialogVm.FileStyles.Count);

            var gothic = dialogVm.FileStyles.Single(s => s.Name == star + "Gothic");
            dialogVm.SelectedFileStyle = gothic;
            Dispatcher.UIThread.RunJobs();
            gothic.FontSize = 39;
            Dispatcher.UIThread.RunJobs();

            if (useApply)
            {
                dialogVm.ApplyCommand.Execute(null);
                Dispatcher.UIThread.RunJobs();
                Assert.Equal(stylesBefore, vm.Subtitles.Select(p => p.Style).ToArray());
                dialogVm.CancelCommand.Execute(null);
            }
            else
            {
                dialogVm.OkCommand.Execute(null);
            }

            Dispatcher.UIThread.RunJobs();
            await dialogTask;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(stylesBefore, vm.Subtitles.Select(p => p.Style).ToArray());
            Assert.Equal(rows, vm.Subtitles);

            var header = vm.GetUpdateSubtitle().Header;
            Assert.Equal(new[] { star + "Default", star + "Gothic", star + "B Gothic" }, AdvancedSubStationAlpha.GetStylesFromHeader(header));
            Assert.Contains($"Style: {star}Gothic,CinemaGothic,39,", header);
            Assert.Contains("PlayResX: 1234", header);
            Assert.Contains("ScriptType: v4.00+", header);

            // What a save writes: every line still under its own style.
            var saved = new AdvancedSubStationAlpha().ToText(vm.GetSaveSubtitle(), string.Empty);
            var savedStyles = saved.SplitToLines()
                .Where(l => l.StartsWith("Dialogue:"))
                .Select(l => l.Split(',')[3])
                .ToArray();
            Assert.Equal(expected, savedStyles);
        }
        finally
        {
            CloseWindow(window, vm);
            try
            {
                Directory.Delete(dir, true);
            }
            catch
            {
                // temp dir cleanup only
            }
        }
    }

    private static string MakeAss(string scriptInfoLine, string scriptTypeLine, string star)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(scriptInfoLine);
        sb.AppendLine("Title: test");
        if (scriptTypeLine.Length > 0)
        {
            sb.AppendLine(scriptTypeLine);
        }

        sb.AppendLine("PlayResX: 1234");
        sb.AppendLine("PlayResY: 516");
        sb.AppendLine();
        sb.AppendLine("[V4+ Styles]");
        sb.AppendLine(SsaStyle.DefaultAssStyleFormat);
        sb.AppendLine($"Style: {star}Default,cinema,40,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,1,1,2,10,10,7,1");
        sb.AppendLine($"Style: {star}Gothic,CinemaGothic,35,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,1,1,2,10,10,7,1");
        sb.AppendLine($"Style: {star}B Gothic,CinemaGothic_B,35,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,1,1,2,10,10,7,1");
        sb.AppendLine();
        sb.AppendLine("[Events]");
        sb.AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
        for (var i = 0; i < LineStyles.Length; i++)
        {
            var s = TimeCode.FromSeconds(i * 2 + 1).ToString().Replace(',', '.');
            var e = TimeCode.FromSeconds(i * 2 + 2.5).ToString().Replace(',', '.');
            sb.AppendLine($"Dialogue: 0,{s},{e},{star}{LineStyles[i]},,0,0,0,,行{i + 1}");
        }

        return sb.ToString();
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static void InvokeImportOriginalSubtitle(
        MainViewModel vm, string fileName, Subtitle subtitle, ImportOriginalHelper.OriginalMatch? match, bool isReadOnly)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ImportOriginalSubtitle not found");

        method.Invoke(vm, new object?[] { 0, fileName, subtitle, match, isReadOnly });
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
