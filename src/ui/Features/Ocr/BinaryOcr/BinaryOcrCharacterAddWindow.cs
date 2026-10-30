using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

/// <summary>
/// Add a character to the binary image compare database, in the editor look (see
/// <see cref="EditorChrome"/>): the whole line image on top, the current letter image with its
/// text below, Abort/Skip/Use once/OK in the button bar.
/// </summary>
public class BinaryOcrCharacterAddWindow : Window
{
    public BinaryOcrCharacterAddWindow(BinaryOcrCharacterAddViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title)));
        Title = string.Empty;
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        DataContext = vm;

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            MaxHeight = 350,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.SentenceBitmap)));

        // subtitle bitmaps are light or dark text on transparent pixels, so without a backdrop they
        // vanish into the window - white text on the light theme (#15111)
        var imageBackdrop = BinaryOcrChrome.MakeImageFrame(image, ImagePreviewBackground.CreateBrush());
        imageBackdrop.HorizontalAlignment = HorizontalAlignment.Center;

        var body = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Whole image for subtitle
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Controls
            },
        };
        body.Add(EditorChrome.MakeEditorSection(Se.Language.Ocr.OcrImage, imageBackdrop, first: true), 0, 0);
        body.Add(MakeControlsView(vm), 1, 0);

        var buttonOk = EditorChrome.AsFlushPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonInspectAdditions = EditorChrome.MakeTextToolButton(IconNames.History, Se.Language.General.InspectAdditions, vm.InspectAdditionsCommand)
            .WithBindIsVisible(nameof(vm.IsInspectAdditionsVisible));
        buttonInspectAdditions.HorizontalAlignment = HorizontalAlignment.Left;
        var buttonUseOnce = EditorChrome.WithoutMargin(UiUtil.MakeButton(Se.Language.General.UseOnce, vm.UseOnceCommand).WithBindIsVisible(nameof(vm.ShowUseOnce)));
        var buttonSkip = EditorChrome.WithoutMargin(UiUtil.MakeButton(Se.Language.General.Skip, vm.SkipCommand).WithBindIsVisible(nameof(vm.ShowSkip)));
        var buttonAbort = EditorChrome.WithoutMargin(UiUtil.MakeButton(Se.Language.General.Abort, vm.AbortCommand));
        var buttonBar = EditorChrome.MakeDockedButtonBar(buttonInspectAdditions, buttonAbort, buttonSkip, buttonUseOnce, buttonOk);

        Content = EditorChrome.MakeRoot(body, buttonBar);

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;
        vm.TextBoxNew.KeyUp += vm.TextBoxNewOnKeyUp;
        vm.TextBoxNew.PointerReleased += vm.TextBoxMacPointerReleased;
        vm.TextBoxNew.Bind(TextBox.FontStyleProperty, new Binding(nameof(vm.IsNewTextItalic)) { Converter = new BoolToFontStyleConverter() });

        var menuFlyout = new MenuFlyout();
        CharactersFlyoutMenuHelper.MakeFlyoutLetters(menuFlyout, vm.InsertSpecialCharacterCommand);
        vm.TextBoxNew.ContextFlyout = menuFlyout;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        });
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
        KeyDown += (_, args) => vm.KeyDown(args);
        KeyUp += (_, args) => vm.KeyUp(args);
    }

    private static Control MakeControlsView(BinaryOcrCharacterAddViewModel vm)
    {
        vm.TextBoxNew = BinaryOcrChrome.AsField(UiUtil.MakeTextBox(100, vm, nameof(vm.NewText))).WithAccessibleName(Se.Language.General.Text);
        vm.TextBoxNew.Width = 220;
        vm.TextBoxNew.HorizontalAlignment = HorizontalAlignment.Left;
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            vm.TextBoxNew.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            MinWidth = 30,
            StretchDirection = StretchDirection.DownOnly, // natural size, as in the old auto-sized column
            MinHeight = 30,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.CurrentBitmap)));
        var panelCurrentImage = BinaryOcrChrome.MakeImageFrame(image, ImagePreviewBackground.CreateBrush());
        panelCurrentImage.HorizontalAlignment = HorizontalAlignment.Left;

        var switchItalic = EditorChrome.MakeSwitch(nameof(vm.IsNewTextItalic), Se.Language.General.Italic);
        switchItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var switchAutoSubmitFirstChar = EditorChrome.MakeSwitch(nameof(vm.SubmitOnFirstLetter), Se.Language.Ocr.AutoSubmitFirstCharacter);

        var panelCurrent = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                BinaryOcrChrome.MakeInfo(nameof(vm.ResolutionAndTopMargin)),
                panelCurrentImage,
                vm.TextBoxNew,
                BinaryOcrChrome.MakeSwitchRow(switchItalic, Se.Language.General.Italic),
                BinaryOcrChrome.MakeSwitchRow(switchAutoSubmitFirstChar, Se.Language.Ocr.AutoSubmitFirstCharacter),
            },
        };

        // Shrink/expand the letter selection - in the section header, right
        var buttonShrink = EditorChrome.MakeTextToolButton(IconNames.Minus, Se.Language.General.Shrink, vm.ShrinkCommand)
            .WithBindEnabled(nameof(vm.CanShrink));
        var buttonExpand = EditorChrome.MakeTextToolButton(IconNames.Plus, Se.Language.General.Expand, vm.ExpandCommand)
            .WithBindEnabled(nameof(vm.CanExpand));
        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                buttonShrink,
                buttonExpand,
            },
        };

        var section = EditorChrome.MakeEditorSection(Se.Language.Ocr.CurrentImage, panelCurrent, panelButtons);
        return new ScrollViewer { Content = section };
    }
}
