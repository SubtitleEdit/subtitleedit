using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;

/// <summary>
/// "N parts saved to folder", in the editor look (see <see cref="EditorChrome"/>): the message and the
/// folder link on the panel, OK at the bottom.
/// </summary>
public class PartsSavedWindow : Window
{
    private readonly PartsSavedViewModel _vm;

    public PartsSavedWindow(PartsSavedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.SplitSubtitle.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelSavedTo = new TextBlock
        {
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 560,
        }.WithBindText(vm, nameof(vm.XPartsSavedToFolder));

        var folderIcon = new Optris.Icons.Avalonia.Icon
        {
            Value = "fa-solid fa-folder-open",
            FontSize = 12,
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var labelFolder = UiUtil.MakeLink(string.Empty, vm.OpenFolderCommand).WithBindText(vm, nameof(vm.FolderName));
        labelFolder.VerticalAlignment = VerticalAlignment.Center;
        var panelFolder = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { folderIcon, labelFolder },
        };

        var panel = new StackPanel
        {
            Spacing = 10,
            Children = { labelSavedTo, panelFolder },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        var section = new Border
        {
            Child = panel,
            Padding = new Thickness(16, 16, 16, 18),
        };

        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(null, buttonOk));

        UiUtil.FocusOnFirstActivation(this, buttonOk); // hack to make OnKeyDown work
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
