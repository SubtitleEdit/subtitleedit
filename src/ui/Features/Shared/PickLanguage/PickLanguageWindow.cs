using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickLanguage;

/// <summary>
/// Pick a language, in the shared picker layout (see <see cref="EditorChrome"/>).
/// </summary>
public class PickLanguageWindow : Window
{
    public PickLanguageWindow(PickLanguageViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.Title ?? Se.Language.Options.Settings.FavoriteLanguages;
        CanResize = true;
        Width = 500;
        Height = 600;
        MinWidth = 350;
        MinHeight = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var textBoxSearch = EditorChrome.MakeSearchBox(vm, nameof(vm.SearchText), null, 220);
        textBoxSearch.TextChanged += (_, _) => vm.SearchTextChanged();

        var listBoxLanguages = new ListBox
        {
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        listBoxLanguages.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Languages)));
        listBoxLanguages.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedLanguage)) { Mode = BindingMode.TwoWay });
        listBoxLanguages.DoubleTapped += (_, _) => vm.OkCommand.Execute(null);
        AutomationProperties.SetName(listBoxLanguages, Se.Language.General.Language);

        var section = EditorChrome.MakePickerListSection(Se.Language.General.Language, listBoxLanguages, textBoxSearch);

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, textBoxSearch);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
