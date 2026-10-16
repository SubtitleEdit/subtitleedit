using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

/// <summary>
/// The nOCR database actions in the editor look (see <see cref="EditorChrome"/>): what to do
/// with the current database as icon actions in one section, Cancel in the button bar.
/// </summary>
public class NOcrSettingsWindow : Window
{
    public NOcrSettingsWindow(NOcrSettingsViewModel vm)
    {
        Title = Se.Language.Ocr.NOcrDatabase;
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.TextToolToggles | EditorWindowStyles.CompactSplitButtons);
        DataContext = vm;

        var labelTitle = new TextBlock
        {
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 460,
        }.WithBindText(vm, nameof(vm.ActionText));

        var buttonEdit = EditorChrome.MakeTextToolButton(IconNames.Pencil, Se.Language.General.Edit, vm.EditCommand);
        var buttonDelete = EditorChrome.MakeTextToolButton(IconNames.Trash, Se.Language.General.Delete, vm.DeleteCommand);
        var buttonRename = EditorChrome.MakeTextToolButton(IconNames.FormTextBox, Se.Language.General.Rename, vm.RenameCommand);
        var buttonNew = EditorChrome.MakeTextToolButton(IconNames.Plus, Se.Language.General.New, vm.NewCommand);
        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Children = { buttonEdit, buttonRename, buttonNew, buttonDelete },
        };

        var content = new StackPanel
        {
            Spacing = 12,
            Children = { labelTitle, actions },
        };

        var section = EditorChrome.MakeEditorSection(Se.Language.Ocr.NOcrDatabase, content, first: true);
        section.MinWidth = 380;

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var root = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(null, buttonCancel));
        Content = root;

        // Semi-hidden power-user features: right-click anywhere in the dialog to train a new
        // nOCR database from installed fonts (port of SE4's "Train nOCR"), or to optimize the
        // current one.
        var menuFlyout = new MenuFlyout();
        menuFlyout.Items.Add(new MenuItem
        {
            Header = Se.Language.Ocr.TrainNOcrDatabase,
            Command = vm.TrainCommand,
        });
        menuFlyout.Items.Add(new MenuItem
        {
            Header = Se.Language.Ocr.OptimizeNOcrDatabase,
            Command = vm.OptimizeCommand,
        });
        root.ContextFlyout = menuFlyout;
        UiUtil.AttachMacContextFlyoutHandler(this, root);

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            buttonEdit.Focus(); // hack to make OnKeyDown work
        });
        KeyDown += (_, e) => vm.KeyDown(e);
    }
}
