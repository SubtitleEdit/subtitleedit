using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;

/// <summary>
/// Pick a subtitle format, in the shared picker layout (see <see cref="EditorChrome"/>): the
/// formats with the search box in the header, the source preview of the selected format below.
/// </summary>
public class PickSubtitleFormatWindow : Window
{
    public PickSubtitleFormatWindow(PickSubtitleFormatViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.PickSubtitleFormat;
        CanResize = true;
        Width = 900;
        Height = 700;
        MinWidth = 600;
        MinHeight = 500;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var textBoxSearch = EditorChrome.MakeSearchBox(vm, nameof(vm.SearchText), Se.Language.General.SearchSubtitleFormats, 260);
        textBoxSearch.TextChanged += (_, _) => vm.SearchTextChanged();

        var listBoxFormats = new ListBox
        {
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        listBoxFormats.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.SubtitleFormatNames)));
        listBoxFormats.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitleFormatName)) { Mode = BindingMode.TwoWay });
        listBoxFormats.SelectionChanged += (_, _) => vm.SelectedSubtitleFormatNameChanged();
        listBoxFormats.DoubleTapped += (_, _) => vm.OkCommand.Execute(null);
        AutomationProperties.SetName(listBoxFormats, Se.Language.General.SubtitleFormats);

        var formatsSection = EditorChrome.MakePickerListSection(Se.Language.General.SubtitleFormats, listBoxFormats, textBoxSearch);

        // Use the PreviewContainer from ViewModel (the section's frame is its border)
        vm.PreviewContainer.VerticalAlignment = VerticalAlignment.Stretch;
        vm.PreviewContainer.HorizontalAlignment = HorizontalAlignment.Stretch;
        var previewSection = EditorChrome.MakePickerListSection(Se.Language.General.Preview, vm.PreviewContainer, lineOnTop: true);

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },  // Formats
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },  // Preview
            },
        };
        sections.Add(formatsSection, 0);
        sections.Add(previewSection, 1);

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            textBoxSearch.Focus();
        });

        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
