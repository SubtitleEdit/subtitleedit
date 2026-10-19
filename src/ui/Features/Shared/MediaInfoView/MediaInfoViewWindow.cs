using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.MediaInfoView;

/// <summary>
/// Media information, in the editor look (see <see cref="EditorChrome"/>): the report (the view
/// model puts its text box in the content border), Open containing folder and OK at the bottom.
/// </summary>
public class MediaInfoViewWindow : Window
{
    public MediaInfoViewWindow(MediaInfoViewViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.MediaInformation;
        Width = 900;
        Height = 500;
        MinWidth = 500;
        MinHeight = 400;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var contentBorder = new Border
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
        };
        vm.TextBoxContainer = contentBorder;

        var buttonOpenContainingFolder = EditorChrome.MakeTextToolButton("fa-solid fa-folder-open", Se.Language.General.OpenContainingFolder, vm.OpenContainingFolderCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        var body = new Border
        {
            Child = contentBorder, // the view model's text box brings its own frame
            Padding = new Thickness(14),
        };

        Content = EditorChrome.MakeRoot(body, EditorChrome.MakeDockedButtonBar(buttonOpenContainingFolder, buttonOk));

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
    }
}
