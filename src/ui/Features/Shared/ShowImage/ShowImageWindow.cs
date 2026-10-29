using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.ShowImage;

/// <summary>
/// Shows a subtitle image (with its text below, if any) in the editor look (see
/// <see cref="EditorChrome"/>), on the image preview background.
/// </summary>
public class ShowImageWindow : Window
{
    public ShowImageWindow(ShowImageViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        Width = 1000;
        Height = 760;
        MinWidth = 400;
        MinHeight = 300;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewImage)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Stretch = Stretch.Uniform,
        };

        var flyout = new MenuFlyout();
        image.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(image);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.CopyImageToClipboard,
            DataContext = vm,
            Command = vm.CopyImageToClipboardCommand,
        };
        flyout.Items.Add(menuItemDelete);

        var menuItemClear = new MenuItem
        {
            Header = Se.Language.General.SaveImageAsDotDotDot,
            DataContext = vm,
            Command = vm.SaveImageAsCommand,
        };
        flyout.Items.Add(menuItemClear);


        var label = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.Text)).WithFontSize(30).WithAlignmentCenter();
        label.Margin = new Thickness(0, 12, 0, 0);
        label.Bind(IsVisibleProperty, new Binding(nameof(vm.Text)) { Converter = Avalonia.Data.Converters.StringConverters.IsNotNullOrEmpty });
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            label.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        // The subtitle image on the image preview background (Settings > Appearance), like OCR
        var imageFrame = new Border
        {
            Child = image,
            Background = ImagePreviewBackground.CreateBrush(),
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12),
            ClipToBounds = true,
        };

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = new Thickness(14),
        };

        grid.Add(imageFrame, 0);
        grid.Add(label, 1);

        Content = EditorChrome.MakeRoot(grid, EditorChrome.MakeButtonBar(null, buttonOk));

        Loaded += delegate { buttonOk.Focus(); vm.Loaded(); }; // hack to make OnKeyDown work
        Closing += (_, e) => vm.Closing(e);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
