using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.Renumber;

/// <summary>
/// Renumber, in the editor look (see <see cref="EditorChrome"/>): the start number in a section,
/// Cancel and OK in the button bar.
/// </summary>
public class RenumberWindow : Window
{
    public RenumberWindow(RenumberViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.Renumber.Title);
        CanResize = false;
        SizeToContent = SizeToContent.Height;
        Width = 360;
        LineToolsChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var numericUpDown = LineToolsChrome.MakeCompact(
            UiUtil.MakeNumericUpDownInt(0, 99999, 1, 150, vm, nameof(vm.StartNumber)),
            Se.Language.Tools.Renumber.StartFromNumber);

        var section = EditorChrome.MakeEditorSection(
            Se.Language.Tools.Renumber.Title,
            LineToolsChrome.MakeFieldGrid((Se.Language.Tools.Renumber.StartFromNumber, numericUpDown)),
            first: true);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = LineToolsChrome.MakeRoot(section, null, buttonCancel, buttonOk);

        Loaded += delegate { numericUpDown.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
