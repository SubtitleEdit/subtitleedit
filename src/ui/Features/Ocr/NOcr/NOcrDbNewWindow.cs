using Avalonia;
using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Sync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

/// <summary>
/// New/rename nOCR database in the editor look (see <see cref="EditorChrome"/>): the name in one
/// section, Cancel + OK in the button bar.
/// </summary>
public class NOcrDbNewWindow : Window
{
    public NOcrDbNewWindow(NOcrDbNewViewModel vm)
    {
        Title = Se.Language.Ocr.NewNOcrDatabase;
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        NOcrChrome.Apply(this);
        DataContext = vm;

        var textBoxDatabaseName = NOcrChrome.StyleTextBox(UiUtil.MakeTextBox(300, vm, nameof(vm.DatabaseName)), Se.Language.General.Name);
        textBoxDatabaseName.Margin = new Thickness(0);

        var section = EditorChrome.MakeEditorSection(
            Se.Language.Ocr.Database,
            SyncTimingChrome.Row(Se.Language.General.Name, textBoxDatabaseName, 60),
            first: true);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = NOcrChrome.MakeRoot(section, SyncTimingChrome.MakeButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            textBoxDatabaseName.Focus(); // hack to make OnKeyDown work
        });

        textBoxDatabaseName.KeyDown += vm.TextBoxDatabaseNameKeyDown;
        KeyDown += (_, e) => vm.KeyDown(e);
        Loaded += (_, _) => Title = vm.Title;
    }
}
