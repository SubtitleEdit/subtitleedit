using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;

/// <summary>
/// Earlier override tags, newest first - styled like ASSA draw and the apply override tags window.
/// </summary>
public class AssaTagHistoryWindow : Window
{
    private static readonly FontFamily MonoFont = new("Menlo, Consolas, Cascadia Mono, DejaVu Sans Mono, monospace");

    public AssaTagHistoryWindow(AssaTagHistoryViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.OverrideTagsHistory;
        CanResize = false;
        Width = 520;
        SizeToContent = SizeToContent.Height;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        var listBox = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(vm.OverrideTags)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedOverrideTag)) { Mode = BindingMode.TwoWay },
            Height = 300,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(4),
            ItemTemplate = new FuncDataTemplate<string>((_, _) => new TextBlock
            {
                FontFamily = MonoFont,
                FontSize = EditorChrome.FieldFontSize,
                TextTrimming = TextTrimming.CharacterEllipsis,
                [!TextBlock.TextProperty] = new Binding("."),
            }),
        };
        AutomationProperties.SetName(listBox, Se.Language.Assa.OverrideTagsHistory);
        listBox.DoubleTapped += (_, _) => vm.OkCommand.Execute(null);

        var delete = EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Delete, vm.DeleteItemCommand, 24);
        var section = EditorChrome.MakeEditorSection(Se.Language.General.History, listBox, delete, first: true);
        section.Background = EditorChrome.PanelBrush;

        var root = new DockPanel();
        var buttonBar = MakeButtonBar(vm);
        DockPanel.SetDock(buttonBar, Dock.Bottom);
        root.Children.Add(buttonBar);
        root.Children.Add(section);
        Content = root;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, listBox);
        KeyDown += vm.KeyDown;
    }

    private static Border MakeButtonBar(AssaTagHistoryViewModel vm)
    {
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(buttonCancel);
        buttons.Children.Add(buttonOk);

        return EditorChrome.WrapButtonBar(buttons);
    }
}
