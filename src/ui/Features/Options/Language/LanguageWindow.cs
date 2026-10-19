using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Language;

/// <summary>
/// Pick the UI language, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class LanguageWindow : Window
{
    public LanguageWindow(LanguageViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.ChooseLanguage.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        var combo = new ComboBox
        {
            ItemsSource = vm.Languages,
            SelectedValue = vm.SelectedLanguage,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 240,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(LanguageViewModel.SelectedLanguage)),
        };
        AutomationProperties.SetName(combo, Se.Language.General.Language);

        var section = EditorChrome.MakeEditorSection(
            Se.Language.General.Language,
            combo,
            first: true);
        section.MinWidth = 360;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, combo); // initial focus on an input, not an action button - a focused button clicks on bare Space
        Loaded += (_, _) => vm.OnLoaded();
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
