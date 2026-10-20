using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

/// <summary>
/// Name a new (or renamed) binary image compare database, in the editor look (see
/// <see cref="EditorChrome"/>): the name in a section, Cancel and OK in the button bar.
/// </summary>
public class BinaryOcrDbNewWindow : Window
{
    public BinaryOcrDbNewWindow(BinaryOcrDbNewViewModel vm)
    {
        Title = Se.Language.Ocr.NewBinaryImageCompareDatabase;
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        DataContext = vm;

        var textBoxDatabaseName = BinaryOcrChrome.AsField(UiUtil.MakeTextBox(200, vm, nameof(vm.DatabaseName)))
            .WithAccessibleName(Se.Language.General.Name);
        textBoxDatabaseName.Width = 320;

        var buttonOk = EditorChrome.AsFlushPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = EditorChrome.WithoutMargin(UiUtil.MakeButtonCancel(vm.CancelCommand));

        var section = EditorChrome.MakeEditorSection(Se.Language.General.Name, textBoxDatabaseName, first: true);
        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            textBoxDatabaseName.Focus(); // hack to make OnKeyDown work
        });

        textBoxDatabaseName.KeyDown += vm.TextBoxDatabaseNameKeyDown;
        KeyDown += (_, e) => vm.KeyDown(e);
        Loaded += (_, _) => Title = vm.Title;
    }
}
