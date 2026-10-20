using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.Chapters;

/// <summary>
/// Write the chapters into a copy of the video, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
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
        SizeToContent = SizeToContent.Height; // a fixed 320 px left an empty band below the output file
        MinWidth = 560;
        MinHeight = 280;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var header = MakeHeader(vm);

        var hint = EditorChrome.MakeHint(language.WriteToVideoDescription);

        var textBoxOutput = EditorChrome.MakeTextField(nameof(vm.OutputFileName), language.OutputFileName);

        var buttonBrowse = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", language.OutputFileName, vm.BrowseOutputFileNameCommand, 28);

        var outputGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 4,
        };
        outputGrid.Add(textBoxOutput, 0, 0);
        outputGrid.Add(buttonBrowse, 0, 1);

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.IsIndeterminate = true;
        var progressView = EditorChrome.MakeProgressStrip(progressBar, nameof(vm.ProgressText), nameof(vm.IsWriting));

        var buttonWrite = UiUtil.MakeButton(language.WriteToVideo, vm.WriteCommand)
            .WithBindIsEnabled(nameof(vm.IsWriting), InverseBooleanConverter.Instance);
        buttonWrite.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = EditorChrome.MakeButtonBar(null, buttonCancel, buttonWrite);

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(hint);
        rows.Children.Add(EditorChrome.MakeDimLabel(language.OutputFileName, new Thickness(0, 4, 0, 0)));
        rows.Children.Add(outputGrid);

        var settings = new StackPanel();
        settings.Children.Add(new Border { Child = header, Padding = new Thickness(14, 12, 14, 12) });
        settings.Children.Add(EditorChrome.MakeEditorSection(language.WriteToVideo.TrimEnd('.', '…'), rows));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        grid.Add(new Border
        {
            Background = EditorChrome.PanelBrush,
            Child = new ScrollViewer
            {
                Content = settings,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            },
        }, 0);
        grid.Add(progressView, 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxOutput);
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
