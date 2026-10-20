using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.GoToVideoPosition;

/// <summary>
/// Go to video position, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class GoToVideoPositionWindow : Window
{
    private readonly GoToVideoPositionViewModel _vm;

    public GoToVideoPositionWindow(GoToVideoPositionViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.GoToVideoPosition;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.VideoTool);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        vm.UpDown = new TimeCodeUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(_vm.Time))
            {
                Mode = BindingMode.TwoWay,
            },
        };
        AutomationProperties.SetName(vm.UpDown, Se.Language.General.VideoPosition);

        var section = EditorChrome.MakeEditorSection(
            Se.Language.General.VideoPosition,
            vm.UpDown,
            first: true);
        section.Background = EditorChrome.PanelBrush;
        section.MinWidth = 300;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).AsPrimary();
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(section, 0, 0);
        root.Add(EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk), 1, 0);

        Content = root;

        UiUtil.FocusOnFirstActivation(this, () => { vm.Activated(); });
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
