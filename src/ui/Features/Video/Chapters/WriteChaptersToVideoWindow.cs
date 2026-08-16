using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.Chapters;

public class WriteChaptersToVideoWindow : Window
{
    private readonly WriteChaptersToVideoViewModel _vm;

    public WriteChaptersToVideoWindow(WriteChaptersToVideoViewModel vm)
    {
        var language = Se.Language.Video.Chapters;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = language.WriteToVideoTitle;
        CanResize = true;
        Width = 640;
        Height = 340;
        MinWidth = 560;
        MinHeight = 300;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var header = MakeHeader(vm);

        var hint = new TextBlock
        {
            Text = language.WriteToVideoDescription,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.7,
            Margin = new Thickness(0, 0, 0, 4),
        };

        var labelOutput = UiUtil.MakeLabel(language.OutputFileName);
        var textBoxOutput = new TextBox
        {
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.OutputFileName)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(textBoxOutput, language.OutputFileName);

        var buttonBrowse = UiUtil.MakeButtonBrowse(vm.BrowseOutputFileNameCommand, accessibleName: language.OutputFileName);

        var outputGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 6,
        };
        outputGrid.Add(textBoxOutput, 0, 0);
        outputGrid.Add(buttonBrowse, 0, 1);

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.IsIndeterminate = true;
        progressBar.Bind(IsVisibleProperty, new Binding(nameof(vm.IsWriting)));

        var progressText = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ProgressText)),
            Opacity = 0.7,
            Margin = new Thickness(0, 4, 0, 0),
            [!IsVisibleProperty] = new Binding(nameof(vm.IsWriting)),
        };

        var buttonWrite = UiUtil.MakeButton(language.WriteToVideo, vm.WriteCommand)
            .WithIconLeft(IconNames.Export)
            .WithBindIsEnabled(nameof(vm.IsWriting), new InverseBooleanConverter());
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonWrite, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 8,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(header, 0);
        grid.Add(hint, 1);
        grid.Add(labelOutput, 2);
        grid.Add(outputGrid, 3);
        grid.Add(UiUtil.MakeVerticalPanel(progressBar, progressText), 4);
        grid.Add(panelButtons, 5);

        Content = grid;

        Activated += delegate { textBoxOutput.Focus(); };
    }

    /// <summary>
    /// Source video on the left, chapter count badge on the right, so it is clear what is about to
    /// be written into what.
    /// </summary>
    private static StackPanel MakeHeader(WriteChaptersToVideoViewModel vm)
    {
        var glyph = ChaptersWindow.MakeGlyph(IconNames.Bookmark, 15);

        var fileName = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.InputFileNameDisplay)),
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };

        var badge = ChaptersWindow.MakeCountBadge(new Binding(nameof(vm.ChapterCountDisplay)));

        return UiUtil.MakeHorizontalPanel(glyph, fileName, badge);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
