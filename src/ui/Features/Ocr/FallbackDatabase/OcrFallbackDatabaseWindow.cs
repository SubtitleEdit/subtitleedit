using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Chrome = Nikse.SubtitleEdit.Features.Ocr.OcrDialogChrome;

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
        Chrome.Apply(this);

        var labelDatabase = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0));
        labelDatabase.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Label)));

        var comboBoxDatabases = new ComboBox
        {
            Width = 340,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedDatabase)) { Mode = BindingMode.TwoWay },
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Databases)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetLabeledBy(comboBoxDatabases, labelDatabase);
        comboBoxDatabases.Bind(AutomationProperties.NameProperty, new Binding(nameof(vm.Label)));

        var buttonOk = Chrome.MakePrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = Chrome.ButtonBar(null, buttonCancel, buttonOk);

        var body = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(16, 14, 16, 16),
            Children = { labelDatabase, comboBoxDatabases },
        };

        Content = Chrome.Root(body, buttonBar);

        UiUtil.FocusOnFirstActivation(this, comboBoxDatabases);
        KeyDown += vm.KeyDown;
    }
}
