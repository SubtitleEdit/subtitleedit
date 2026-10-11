using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using UITests;

namespace UITests.Features.Main;

/// <summary>
/// The toolbar's Format combo gave every item a fixed 150 px width, so long names were cut off
/// in the open drop-down too, e.g. "DVD Studio Pro with one sp" (#15901).
/// </summary>
public class MainFormatDropDownWidthTests
{
    [AvaloniaFact]
    public void LongFormatName_IsNotTrimmedInDropDown()
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
            var comboBox = window.GetVisualDescendants()
                .OfType<ComboBox>()
                .First(c => AutomationProperties.GetName(c) == Se.Language.General.Format);
            var format = comboBox.Items.OfType<SubtitleFormat>()
                .First(f => f.Name == "DVD Studio Pro with one space/semicolon");
            var index = comboBox.Items.IndexOf(format);

            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();
            comboBox.ScrollIntoView(index);
            Dispatcher.UIThread.RunJobs();

            var item = (Control)comboBox.ContainerFromIndex(index)!;
            item.UpdateLayout();
            var text = item.GetVisualDescendants().OfType<TextBlock>().First(t => t.Text == format.Name);
            var probe = new TextBlock { Text = format.Name, FontSize = text.FontSize, FontFamily = text.FontFamily };
            probe.Measure(Size.Infinity);

            Assert.True(probe.DesiredSize.Width <= text.Bounds.Width + 0.5,
                $"needs {probe.DesiredSize.Width:0} px, item text is {text.Bounds.Width:0} px");

            comboBox.IsDropDownOpen = false;
        }
        finally
        {
            foreach (var ownedWindow in window.OwnedWindows.ToArray())
            {
                ownedWindow.Close();
            }

            window.SuppressSaveChangesPromptOnClose(vm);
            window.Close();
        }
    }
}
