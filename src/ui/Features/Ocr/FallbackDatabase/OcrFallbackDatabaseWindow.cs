using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Ocr.FallbackDatabase;

public class OcrFallbackDatabaseWindow : Window
{
    public OcrFallbackDatabaseWindow(OcrFallbackDatabaseViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        vm.Window = this;
        DataContext = vm;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.EngineDialog);

        // A section header, like the other pickers (it was a plain label)
        var labelDatabase = EditorChrome.MakeBoundSectionHeader(nameof(vm.Label));
        labelDatabase.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Label))
        {
            Converter = new FuncValueConverter<string?, string>(text => (text ?? string.Empty).ToUpperInvariant()),
        });

        var comboBoxDatabases = new ComboBox
        {
            Width = 340,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedDatabase)) { Mode = BindingMode.TwoWay },
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Databases)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetLabeledBy(comboBoxDatabases, labelDatabase);
        comboBoxDatabases.Bind(AutomationProperties.NameProperty, new Binding(nameof(vm.Label)));

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var body = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(16, 14, 16, 16),
            Children = { labelDatabase, comboBoxDatabases },
        };

        Content = EditorChrome.MakeRoot(body, buttonBar, stretch: false);

        UiUtil.FocusOnFirstActivation(this, comboBoxDatabases);
        KeyDown += vm.KeyDown;
    }
}
