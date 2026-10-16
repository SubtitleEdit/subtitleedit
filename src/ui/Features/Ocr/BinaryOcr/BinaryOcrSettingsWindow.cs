using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

/// <summary>
/// Pick an action for the binary image compare database (edit, delete, rename, new), in the editor
/// look (see <see cref="EditorChrome"/>): the actions in a section, Cancel in the button bar.
/// </summary>
public class BinaryOcrSettingsWindow : Window
{
    public BinaryOcrSettingsWindow(BinaryOcrSettingsViewModel vm)
    {
        Title = Se.Language.Ocr.BinaryImageCompareDatabase;
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        DataContext = vm;

        var labelTitle = new TextBlock
        {
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 480,
            HorizontalAlignment = HorizontalAlignment.Left,
        }.WithBindText(vm, nameof(vm.ActionText));

        var buttonEdit = BinaryOcrChrome.MakeActionButton(Se.Language.General.Edit, IconNames.Pencil, vm.EditCommand);
        var buttonDelete = BinaryOcrChrome.MakeActionButton(Se.Language.General.Delete, IconNames.Trash, vm.DeleteCommand);
        var buttonRename = BinaryOcrChrome.MakeActionButton(Se.Language.General.Rename, IconNames.FormTextBox, vm.RenameCommand);
        var buttonNew = BinaryOcrChrome.MakeActionButton(Se.Language.General.New, IconNames.Plus, vm.NewCommand);
        var buttonCancel = EditorChrome.WithoutMargin(UiUtil.MakeButtonCancel(vm.CancelCommand));

        var panel = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                labelTitle,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Children = { buttonEdit, buttonDelete, buttonRename, buttonNew },
                },
            },
        };

        var section = EditorChrome.MakeEditorSection(Se.Language.Ocr.Database, panel, first: true);
        section.MinWidth = 420;
        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(null, buttonCancel));

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            buttonEdit.Focus(); // hack to make OnKeyDown work
        });
        KeyDown += (_, e) => vm.KeyDown(e);
    }
}
