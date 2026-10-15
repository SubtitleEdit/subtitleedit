using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// The saved styles grid starts with its first row selected, but the view model never got that
/// selection: clicking the row only raised GotFocus, and the editor showed nothing to edit.
/// </summary>
public class AssaStylesFirstSavedStyleTests
{
    [AvaloniaFact]
    public void ClickingTheFirstSavedStyle_OpensItInTheEditor()
    {
        var saved = Se.Settings.Assa.StoredStyles.ToList();
        Se.Settings.Assa.StoredStyles.Clear();
        var first = SeAssaStyle.MakeStorageDefault();
        first.Name = "SavedA";
        var second = SeAssaStyle.MakeStorageDefault();
        second.Name = "SavedB";
        second.IsDefault = false;
        Se.Settings.Assa.StoredStyles.Add(first);
        Se.Settings.Assa.StoredStyles.Add(second);
        try
        {
            var vm = new AssaStylesViewModel(new FileHelper(), new StubWindowService());
            var subtitle = new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader };
            subtitle.Paragraphs.Add(new Paragraph("x", 0, 1000) { Extra = "Default" });
            vm.Initialize(subtitle, new AdvancedSubStationAlpha(), "a.ass", "Default", null);
            var window = new AssaStylesWindow(vm) { Width = 1300, Height = 850 };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var grid = vm.StorageStyleGrid;
            var container = (Visual?)grid.ContainerFromItem(vm.StorageStylesView.First(s => s.Name == "SavedA"));
            Assert.NotNull(container);
            var point = container!.TranslatePoint(new Point(container.Bounds.Width / 2, container.Bounds.Height / 2), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("SavedA", vm.CurrentStyle?.Name);
            Assert.Equal(Se.Language.Assa.StylesSaved, vm.CurrentTitle);
            window.Close();
        }
        finally
        {
            Se.Settings.Assa.StoredStyles.Clear();
            Se.Settings.Assa.StoredStyles.AddRange(saved);
        }
    }
}
