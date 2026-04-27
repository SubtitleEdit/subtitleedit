using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.Statistics;

public class StatisticsWindow : Window
{
    public StatisticsWindow(StatisticsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        Title = Se.Language.File.Statistics.Title;
        CanResize = true;
        Width = 950;
        Height = 850;
        MinWidth = 800;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        };

        var textBoxGeneralStatistics = new TextBox
        {
            IsReadOnly = true,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            DataContext = vm,
        };
        textBoxGeneralStatistics.Bind(TextBox.TextProperty, new Binding(nameof(vm.TextGeneral)));


        var labelMostUsedWords = UiUtil.MakeLabel(Se.Language.File.Statistics.MostUsedWords).WithMarginTop(10);
        var textBoxMostUsedWords = new TextBox
        {
            IsReadOnly = true,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            DataContext = vm,
        };  
        textBoxMostUsedWords.Bind(TextBox.TextProperty, new Binding(nameof(vm.TextMostUsedWords)));

        var labelMostUsedLines = UiUtil.MakeLabel(Se.Language.File.Statistics.MostUsedLines).WithMarginTop(10);
        var textBoxMostUsedLines = new TextBox
        {
            IsReadOnly = true,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            DataContext = vm,
        };  
        textBoxMostUsedLines.Bind(TextBox.TextProperty, new Binding(nameof(vm.TextMostUsedLines)));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonExport = UiUtil.MakeButton(Se.Language.General.Export, vm.ExportCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonExport, buttonOk);

        grid.Add(textBoxGeneralStatistics, 0, 0, 1, 2);
        grid.Add(labelMostUsedWords, 1, 0);
        grid.Add(labelMostUsedLines, 1, 1);
        grid.Add(textBoxMostUsedWords, 2, 0);
        grid.Add(textBoxMostUsedLines, 2, 1);
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }
}
