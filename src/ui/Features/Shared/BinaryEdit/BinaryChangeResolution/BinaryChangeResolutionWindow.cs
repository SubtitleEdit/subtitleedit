using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryChangeResolution;

/// <summary>
/// Change the video resolution in the editor look (see <see cref="EditorChrome"/>): preset and
/// size in a settings column, the preview on the right, Cancel/OK at the bottom.
/// </summary>
public class BinaryChangeResolutionWindow : Window
{
    public BinaryChangeResolutionWindow(BinaryChangeResolutionViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.VideoResolution;
        Width = 800;
        Height = 600;
        CanResize = true;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        Content = BinaryEditChrome.MakePreviewDialogRoot(
            MakeControlsPanel(vm, out var presetComboBox),
            MakePreviewPanel(vm),
            EditorChrome.MakeCancelOkBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, presetComboBox); // an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeControlsPanel(BinaryChangeResolutionViewModel vm, out ComboBox presetComboBox)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
        };

        presetComboBox = UiUtil.MakeComboBox(vm.Resolutions, vm, nameof(vm.SelectedResolution), null);
        presetComboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(presetComboBox, Se.Language.General.Resolution);
        panel.Children.Add(presetComboBox);

        panel.Children.Add(BinaryEditChrome.MakeRow(Se.Language.General.Width, MakeSizeInput(vm, nameof(vm.NewWidth), Se.Language.General.Width), 80));
        panel.Children.Add(BinaryEditChrome.MakeRow(Se.Language.General.Height, MakeSizeInput(vm, nameof(vm.NewHeight), Se.Language.General.Height), 80));

        panel.Children.Add(new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SizeText)),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0),
            FontSize = EditorChrome.FieldFontSize,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
        });

        panel.Children.Add(new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ScaleText)),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
        });

        return EditorChrome.MakeEditorSection(Se.Language.General.Resolution, panel, first: true);
    }

    private static NumericUpDown MakeSizeInput(BinaryChangeResolutionViewModel vm, string propertyName, string name)
    {
        var field = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 16384,
            Increment = 2,
            FormatString = "0",
            [!NumericUpDown.ValueProperty] = new Binding(propertyName)
            {
                Mode = BindingMode.TwoWay,
                Converter = new NullableIntConverter(),
            },
        };
        return EditorChrome.AsField(field, name, 100, alignLeft: true, center: true);
    }

    private static Border MakePreviewPanel(BinaryChangeResolutionViewModel vm)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new Image
            {
                Stretch = Avalonia.Media.Stretch.None,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                [!Image.SourceProperty] = new Binding(nameof(vm.PreviewBitmap)),
            },
        };

        // Light or dark text on transparency is invisible on a flat backdrop - checkerboard (issue #12692).
        return BinaryEditChrome.MakePreviewFrame(scrollViewer, UiUtil.GetCheckerboardBrush());
    }
}
