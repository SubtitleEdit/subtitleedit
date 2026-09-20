using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features;

/// <summary>
/// An auto-hiding scrollbar overlays the content it scrolls, so in the edit text box it covered
/// the last letters of each wrapped line (#15033). The box must give the scrollbar its own column.
/// </summary>
public class EditTextBoxScrollBarTests
{
    [AvaloniaFact]
    public void MainEditTextBox_VerticalScrollBarDoesNotOverlayText()
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

        var vm = (MainViewModel)view.DataContext!;
        try
        {
            var textBox = (TextBox)vm.EditTextBox.TextControl;
            Assert.False(ScrollViewer.GetAllowAutoHide(textBox));

            textBox.Text = string.Join(" ", Enumerable.Repeat("word", 2000));
            Dispatcher.UIThread.RunJobs();

            var scrollViewer = textBox.GetVisualDescendants().OfType<ScrollViewer>().First();
            var presenter = scrollViewer.GetVisualDescendants().OfType<ScrollContentPresenter>().First();
            var scrollBar = scrollViewer.GetVisualDescendants().OfType<ScrollBar>()
                .First(sb => sb.Orientation == Avalonia.Layout.Orientation.Vertical);

            Assert.True(scrollBar.IsVisible);
            Assert.True(scrollBar.Bounds.Width > 0);
            Assert.True(presenter.Bounds.Right <= scrollBar.Bounds.Left + 0.5,
                $"text area {presenter.Bounds} runs under the scrollbar {scrollBar.Bounds}");
        }
        finally
        {
            window.Closing -= vm.OnClosing;
            window.Close();
        }
    }
}
