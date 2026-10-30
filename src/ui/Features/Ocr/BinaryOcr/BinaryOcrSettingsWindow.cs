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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.TextToolToggles | EditorWindowStyles.CompactSplitButtons);
        DataContext = vm;

        var labelTitle = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 480,
            HorizontalAlignment = HorizontalAlignment.Left,
        }.WithBindText(vm, nameof(vm.ActionText));

        // The same flat buttons, in the same order (Delete last), as the nOCR database dialog
        var buttonEdit = EditorChrome.MakeTextToolButton(IconNames.Pencil, Se.Language.General.Edit, vm.EditCommand);
        var buttonDelete = EditorChrome.MakeTextToolButton(IconNames.Trash, Se.Language.General.Delete, vm.DeleteCommand);
        var buttonRename = EditorChrome.MakeTextToolButton(IconNames.FormTextBox, Se.Language.General.Rename, vm.RenameCommand);
        var buttonNew = EditorChrome.MakeTextToolButton(IconNames.Plus, Se.Language.General.New, vm.NewCommand);
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
                    Spacing = 4,
                    Children = { buttonEdit, buttonRename, buttonNew, buttonDelete },
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
