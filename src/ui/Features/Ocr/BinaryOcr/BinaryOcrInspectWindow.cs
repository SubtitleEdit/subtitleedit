using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

/// <summary>
/// Inspect the binary image compare matches of a line, in the editor look (see
/// <see cref="EditorChrome"/>): the matched letters and the line image on top, the current letter
/// image and its match below, OK in the button bar.
/// </summary>
public class BinaryOcrInspectWindow : Window
{
    public BinaryOcrInspectWindow(BinaryOcrInspectViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.BinaryImageCompareInspectImageMatches;
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        DataContext = vm;

        var body = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Lines
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Controls
            },
        };
        body.Add(MakeLinesView(vm), 0, 0);
        body.Add(MakeControlsView(vm), 1, 0);

        var buttonOk = EditorChrome.AsFlushPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var labelInfo = EditorChrome.MakeDimStatusText("Binary OCR uses pixel-by-pixel comparison. No manual drawing required.");
        var buttonBar = EditorChrome.MakeDockedButtonBar(labelInfo, buttonOk);

        Content = EditorChrome.MakeRoot(body, buttonBar);

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        });
        KeyDown += (_, e) => vm.KeyDown(e);
        KeyUp += (_, e) => vm.KeyUp(e);
        Loaded += (_, _) => vm.OnLoaded();
    }

    private static Border MakeLinesView(BinaryOcrInspectViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 14,
        };

        // The matched letters as buttons, one wrap panel per line (filled by the view model)
        vm.PanelLines = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(4),
        };
        var linesFrame = EditorChrome.MakeListFrame(new ScrollViewer
        {
            Content = vm.PanelLines,
            MaxHeight = 220,
        }, clearListBackground: true);

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.SentenceBitmap)),
            Stretch = Stretch.Uniform,
            MaxWidth = 300,
            MaxHeight = 200,
        };

        // subtitle bitmaps are light or dark text on transparent pixels, so without a backdrop they
        // vanish into the window - white text on the light theme (#15111)
        var imageBackdrop = BinaryOcrChrome.MakeImageFrame(image, ImagePreviewBackground.CreateBrush());
        imageBackdrop.HorizontalAlignment = HorizontalAlignment.Center;
        imageBackdrop.VerticalAlignment = VerticalAlignment.Top;

        grid.Add(linesFrame, 0, 0);
        grid.Add(imageBackdrop, 0, 1);

        return EditorChrome.MakeEditorSection(Se.Language.Ocr.InspectImageMatches, grid, first: true);
    }

    private static Border MakeControlsView(BinaryOcrInspectViewModel vm)
    {
        vm.TextBoxNew = BinaryOcrChrome.AsField(UiUtil.MakeTextBox(100, vm, nameof(vm.NewText)))
            .WithBindEnabled(nameof(vm.IsEditControlsEnabled))
            .WithAccessibleName(Se.Language.General.Match);
        vm.TextBoxNew.Width = 220;
        vm.TextBoxNew.HorizontalAlignment = HorizontalAlignment.Left;
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            vm.TextBoxNew.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.CurrentBitmap)),
            Stretch = Stretch.Uniform,
            MinWidth = 30,
            StretchDirection = StretchDirection.DownOnly, // natural size, as in the old auto-sized column
            MinHeight = 30,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var panelCurrentImage = BinaryOcrChrome.MakeImageFrame(image, ImagePreviewBackground.CreateBrush());
        panelCurrentImage.HorizontalAlignment = HorizontalAlignment.Left;

        var panelCurrent = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                panelCurrentImage,
                BinaryOcrChrome.MakeInfo(nameof(vm.ResolutionAndTopMargin)),
            },
        };

        var switchItalic = EditorChrome.MakeSwitch(nameof(vm.IsNewTextItalic), Se.Language.General.Italic);
        switchItalic.Bind(Avalonia.Input.InputElement.IsEnabledProperty, new Binding(nameof(vm.IsEditControlsEnabled)));
        switchItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var buttonUpdate = BinaryOcrChrome.MakeActionButton(Se.Language.General.Update, IconNames.Check, vm.UpdateCommand)
            .WithBindEnabled(nameof(vm.IsEditControlsEnabled));
        var buttonDelete = BinaryOcrChrome.MakeActionButton(Se.Language.General.Delete, IconNames.Trash, vm.DeleteCommand)
            .WithBindEnabled(nameof(vm.IsEditControlsEnabled));
        var buttonAddBetterMatch = BinaryOcrChrome.MakeActionButton(Se.Language.Ocr.AddBetterMatch, IconNames.Plus, vm.AddBetterMatchCommand)
            .WithBindEnabled(nameof(vm.CanAddBetterMatch));

        var panelMatch = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                vm.TextBoxNew,
                BinaryOcrChrome.MakeSwitchRow(switchItalic, Se.Language.General.Italic),
                BinaryOcrChrome.MakeInfo(nameof(vm.MatchResolutionAndTopMargin)),
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Margin = new Thickness(0, 8, 0, 0),
                    Children = { buttonUpdate, buttonDelete, buttonAddBetterMatch },
                },
            },
        };

        var sectionCurrent = EditorChrome.MakeEditorSection(Se.Language.Ocr.CurrentImage, panelCurrent);
        var sectionMatch = EditorChrome.MakeEditorSection(Se.Language.General.Match, panelMatch);
        sectionMatch.BorderThickness = new Thickness(1, 1, 0, 0);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        grid.Add(sectionCurrent, 0, 0);
        grid.Add(sectionMatch, 0, 1);

        return new Border { Child = new ScrollViewer { Content = grid } };
    }
}
