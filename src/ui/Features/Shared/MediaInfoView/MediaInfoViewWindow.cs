using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Edit;
using Nikse.SubtitleEdit.Features.Sync;
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
        EditDialogChrome.Apply(this);
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

        var buttonOpenContainingFolder = SyncTimingChrome.MakeTextToolButton("fa-solid fa-folder-open", Se.Language.General.OpenContainingFolder, vm.OpenContainingFolderCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        var body = new Border
        {
            Child = contentBorder, // the view model's text box brings its own frame
            Padding = new Thickness(14),
        };

        Content = SyncTimingChrome.MakeRoot(body, SyncTimingChrome.MakeButtonBar(buttonOpenContainingFolder, buttonOk));

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
    }
}
