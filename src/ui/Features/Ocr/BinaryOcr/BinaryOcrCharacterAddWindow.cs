using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

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
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Whole image for subtitle
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Controls
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            Margin = new Thickness(0, 0, 0, 10),
            MaxHeight = 350,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.SentenceBitmap)));

        var controlsView = MakeControlsView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonInspectAdditions = UiUtil.MakeButton(Se.Language.General.InspectAdditions, vm.InspectAdditionsCommand).WithBindIsVisible(nameof(vm.IsInspectAdditionsVisible));
        var buttonUseOnce = UiUtil.MakeButton(Se.Language.General.UseOnce, vm.UseOnceCommand).WithBindIsVisible(nameof(vm.ShowUseOnce));
        var buttonSkip = UiUtil.MakeButton(Se.Language.General.Skip, vm.SkipCommand).WithBindIsVisible(nameof(vm.ShowSkip));
        var buttonAbort = UiUtil.MakeButton(Se.Language.General.Abort, vm.AbortCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonInspectAdditions, buttonUseOnce, buttonSkip, buttonAbort);

        grid.Add(image, 0, 0);
        grid.Add(controlsView, 1, 0);
        grid.Add(buttonBar, 2, 0);

        Content = grid;

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;
        vm.TextBoxNew.KeyUp += vm.TextBoxNewOnKeyUp;
        vm.TextBoxNew.PointerReleased += vm.TextBoxMacPointerReleased;
        var menuFlyout = new MenuFlyout();
        CharactersFlyoutMenuHelper.MakeFlyoutLetters(menuFlyout, vm.InsertSpecialCharacterCommand); 
        vm.TextBoxNew.ContextFlyout = menuFlyout;

        Activated += delegate
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        };
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
        KeyDown += (_, args) => vm.KeyDown(args);
        KeyUp += (_, args) => vm.KeyUp(args);
    }

    private static Grid MakeControlsView(BinaryOcrCharacterAddViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 20,
            Width = double.NaN,
        };

        vm.TextBoxNew = UiUtil.MakeTextBox(100, vm, nameof(vm.NewText));
        var image = new Image
        {
            Margin = new Thickness(5),
            Stretch = Stretch.Uniform,
            MinWidth = 30,
            MinHeight = 30,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.CurrentBitmap)));


        var panelCurrentImage = new StackPanel
        {
            Background = new SolidColorBrush(Colors.LightGray),
            Children = { image },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Thickness(5, 2, 0, 5),
        };

        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.IsNewTextItalic));
        checkBoxItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var checkBoAutoSubmitFirsChar = UiUtil.MakeCheckBox(Se.Language.Ocr.AutoSubmitFirstCharacter, vm, nameof(vm.SubmitOnFirstLetter));

        var panelCurrent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.CurrentImage).WithBold(),
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ResolutionAndTopMargin)),
                panelCurrentImage,
                vm.TextBoxNew,
                checkBoxItalic,
                checkBoAutoSubmitFirsChar,
            },
        };

        var buttonShrink = UiUtil.MakeButton(Se.Language.General.Shrink, vm.ShrinkCommand)
            .WithMinWidth(100).WithBindEnabled(nameof(vm.CanShrink));
        var buttonExpand = UiUtil.MakeButton(Se.Language.General.Expand, vm.ExpandCommand)
            .WithMinWidth(100).WithBindEnabled(nameof(vm.CanExpand));
        var panelButtons = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Width = double.NaN,
            Margin = new Thickness(0, 0, 0, 5),
            Children =
            {
                buttonShrink,
                buttonExpand,
            },
        };

        grid.Add(panelCurrent, 0, 0);
        grid.Add(panelButtons, 0, 1);

        return grid;
    }
}
