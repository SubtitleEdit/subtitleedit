using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;

public class TransparentSettingsWindow : Window
{
    public TransparentSettingsWindow(TransparentSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.OutputSettingsTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var checkBoxUseSourceFolder = new RadioButton
        {
            Content = Se.Language.General.UseSourceFolder,
            FontSize = EditorChrome.FieldFontSize,
            IsChecked = vm.UseSourceFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!Avalonia.Controls.Primitives.ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.UseSourceFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };

        var checkBoxUseOutputFolder = new RadioButton
        {
            Content = Se.Language.General.UseOutputFolder,
            FontSize = EditorChrome.FieldFontSize,
            IsChecked = vm.UseOutputFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!Avalonia.Controls.Primitives.ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.UseOutputFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };

        var textBoxOutputFolder = new TextBox
        {
            Text = vm.OutputFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.OutputFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },

            // Bound, not read once at construction: ticking "use output folder" has to enable the
            // box, otherwise the path can only be set through Browse (the burn-in twin binds it).
            [!Control.IsEnabledProperty] = new Binding(nameof(vm.UseOutputFolder)) { Mode = BindingMode.OneWay },
            Width = 400,
        };
        textBoxOutputFolder.Classes.Add("text");
        Avalonia.Automation.AutomationProperties.SetName(textBoxOutputFolder, Se.Language.General.OutputFolder);

        var buttonBrowse = EditorChrome.CreateToolButton("fa-solid fa-folder-open", Se.Language.General.UseOutputFolder, vm.BrowseOutputFolderCommand, 28);

        var panelOutputFolder = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 4,
            Margin = new Thickness(28, 0, 0, 0), // under the radio button's text
            Children =
            {
                textBoxOutputFolder,
                buttonBrowse
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var rows = new StackPanel
        {
            Spacing = 6,
            Children =
            {
                checkBoxUseSourceFolder,
                checkBoxUseOutputFolder,
                panelOutputFolder,
            },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        grid.Add(EditorChrome.MakePanel(EditorChrome.MakeEditorSection(Se.Language.General.OutputFolder, rows, first: true)), 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, checkBoxUseSourceFolder); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
