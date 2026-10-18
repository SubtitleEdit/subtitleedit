using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

/// <summary>
/// Pick burn-in effects, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class BurnInEffectWindow : Window
{
    private readonly BurnInEffectViewModel _vm;

    public BurnInEffectWindow(BurnInEffectViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Effect;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        // A StackPanel to hold all checkboxes (the view model keeps them in CheckBoxes)
        var checkBoxPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            MinWidth = 260,
        };

        // Create checkboxes for each effect
        foreach (var effect in vm.Effects)
        {
            // On/off switches like the other dialogs' options (these were checkboxes)
            var checkBox = new ToggleSwitch
            {
                OnContent = null,
                OffContent = null,
                MinWidth = 0,
                Tag = effect, // Store the effect item for later reference
                Name = effect.Type.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Avalonia.Automation.AutomationProperties.SetName(checkBox, effect.Name);
            vm.CheckBoxes.Add(checkBox);

            // Handle checkbox state changes
            checkBox.IsCheckedChanged += (sender, e) =>
            {
                var cb = sender as ToggleSwitch;
                var effectItem = cb?.Tag as BurnInEffectItem;
                if (effectItem != null && cb != null)
                {
                    if (cb.IsChecked == true)
                    {
                        vm.AddSelectedEffect(effectItem);
                    }
                    else
                    {
                        vm.RemoveSelectedEffect(effectItem);
                    }
                }
            };

            checkBoxPanel.Children.Add(EditorChrome.MakeSwitchRow(checkBox, effect.Name));
        }

        grid.Add(EditorChrome.MakePanel(EditorChrome.MakeEditorSection(Se.Language.General.Effect, checkBoxPanel, first: true)), 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, () => { (vm.CheckBoxes.Count > 0 ? vm.CheckBoxes[0] : buttonCancel).Focus(); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.VideoPlayerControl?.CloseAndDisposePlayer();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}