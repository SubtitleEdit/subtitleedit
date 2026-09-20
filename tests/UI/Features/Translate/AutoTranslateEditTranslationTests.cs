using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Linq;

namespace UITests.Features.Translate;

/// <summary>
/// The translation column can be edited in place: a slip in the machine translation is fixed in
/// the grid instead of after closing the window. Enter commits without also firing the window's
/// default button, Escape drops the edit without cancelling the window, and an edited row counts
/// as a translation so OK becomes available.
/// </summary>
public class AutoTranslateEditTranslationTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static (AutoTranslateViewModel Vm, AutoTranslateWindow Window) OpenWindow()
    {
        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello there.", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("How are you?", 2000, 4000));
        vm.Initialize(subtitle);

        var window = new AutoTranslateWindow(vm) { Width = 1000, Height = 700 };
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return (vm, window);
    }

    private static Border TranslationCell(TableView grid, object row)
    {
        var container = (Visual)grid.ContainerFromItem(row)!;
        return container.GetVisualDescendants().OfType<Border>()
            .Single(b => b.Cursor?.ToString() == new Cursor(StandardCursorType.Ibeam).ToString());
    }

    private static void Click(Border cell)
    {
        cell.RaiseEvent(new PointerReleasedEventArgs(cell, new Pointer(0, PointerType.Mouse, true), cell,
            new Avalonia.Point(2, 2), 0, PointerPointProperties.None, KeyModifiers.None, MouseButton.Left)
        { RoutedEvent = InputElement.PointerReleasedEvent });
        Dispatcher.UIThread.RunJobs();
    }

    private static void PressKey(Control target, Key key)
    {
        target.RaiseEvent(new KeyEventArgs { Key = key, RoutedEvent = InputElement.KeyDownEvent, Source = target });
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void ClickOnSelectedRow_EditsTranslation_AndEnterCommits()
    {
        var (vm, window) = OpenWindow();
        try
        {
            var grid = window.GetVisualDescendants().OfType<TableView>().First();
            var row = vm.Rows[0];
            row.TranslatedText = "Hallo dar.";
            grid.SelectedItem = row;
            Dispatcher.UIThread.RunJobs();
            Assert.False(vm.IsOkEnabled);

            var cell = TranslationCell(grid, row);
            Click(cell);
            var textBox = Assert.IsType<TextBox>(cell.Child);
            Assert.Equal("Hallo dar.", textBox.Text);

            textBox.Text = "Hej der.";
            PressKey(textBox, Key.Enter);

            Assert.Equal("Hej der.", row.TranslatedText);
            Assert.IsType<TextBlock>(cell.Child);
            Assert.True(vm.IsOkEnabled); // the edit is something worth keeping
            Assert.True(window.IsVisible); // Enter did not run the default button
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Escape_DropsTheEdit_WithoutClosingTheWindow()
    {
        var (vm, window) = OpenWindow();
        try
        {
            var grid = window.GetVisualDescendants().OfType<TableView>().First();
            var row = vm.Rows[1];
            row.TranslatedText = "Hvordan går det?";
            grid.SelectedItem = row;
            Dispatcher.UIThread.RunJobs();

            var cell = TranslationCell(grid, row);
            Click(cell);
            var textBox = Assert.IsType<TextBox>(cell.Child);
            textBox.Text = "garbage";
            PressKey(textBox, Key.Escape);

            Assert.Equal("Hvordan går det?", row.TranslatedText);
            Assert.IsType<TextBlock>(cell.Child);
            Assert.True(window.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ClickOnUnselectedRow_OnlySelects()
    {
        var (vm, window) = OpenWindow();
        try
        {
            var grid = window.GetVisualDescendants().OfType<TableView>().First();
            grid.SelectedItem = vm.Rows[0];
            Dispatcher.UIThread.RunJobs();

            var cell = TranslationCell(grid, vm.Rows[1]);
            Click(cell);

            Assert.IsType<TextBlock>(cell.Child);
        }
        finally
        {
            window.Close();
        }
    }
}
