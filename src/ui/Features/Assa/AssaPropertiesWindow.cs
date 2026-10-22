using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa;

/// <summary>
/// The script's [Script Info]: credits, resolution and render options - styled like ASSA draw.
/// </summary>
public class AssaPropertiesWindow : Window
{
    public AssaPropertiesWindow(AssaPropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        Width = 640;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        var sections = new StackPanel { Background = EditorChrome.PanelBrush };
        sections.Children.Add(MakeScriptSection(vm, out var textBoxTitle));
        sections.Children.Add(MakeResolutionSection(vm));
        sections.Children.Add(MakeOptionsSection(vm));

        var root = new DockPanel();
        var buttonBar = MakeButtonBar(vm);
        DockPanel.SetDock(buttonBar, Dock.Bottom);
        root.Children.Add(buttonBar);
        root.Children.Add(sections);
        Content = root;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, textBoxTitle);
        KeyDown += vm.KeyDown;
    }

    /// <summary>
    /// A dark text field with its label above it.
    /// </summary>
    private static StackPanel MakeTextField(string label, string property, out TextBox textBox)
    {
        textBox = new TextBox
        {
            FontSize = EditorChrome.FieldFontSize,
            MinHeight = 28,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(6),
            [!TextBox.TextProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(textBox, label);

        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(new TextBlock { Text = label, FontSize = EditorChrome.FieldFontSize, Foreground = EditorChrome.DimTextBrush });
        panel.Children.Add(textBox);
        return panel;
    }

    private static Border MakeScriptSection(AssaPropertiesViewModel vm, out TextBox firstInput)
    {
        var title = MakeTextField(Se.Language.General.Title, nameof(vm.ScriptTitle), out firstInput);

        // The credits two by two
        var grid = new Grid
        {
            ColumnSpacing = 12,
            RowSpacing = 10,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
        };
        grid.Add(MakeTextField(Se.Language.Assa.OriginalScript, nameof(vm.OriginalScript), out _), 0, 0);
        grid.Add(MakeTextField(Se.Language.General.Translation, nameof(vm.Translation), out _), 0, 1);
        grid.Add(MakeTextField(Se.Language.General.Editing, nameof(vm.Editing), out _), 1, 0);
        grid.Add(MakeTextField(Se.Language.General.Timing, nameof(vm.Timing), out _), 1, 1);
        grid.Add(MakeTextField(Se.Language.General.Sync, nameof(vm.SyncPoint), out _), 2, 0);
        grid.Add(MakeTextField(Se.Language.General.UpdatedBy, nameof(vm.UpdatedBy), out _), 2, 1);

        var content = new StackPanel { Spacing = 10 };
        content.Children.Add(title);
        content.Children.Add(grid);
        content.Children.Add(MakeTextField(Se.Language.General.UpdateDetails, nameof(vm.UpdateDetails), out _));
        return EditorChrome.MakeEditorSection(Se.Language.General.Script, content, first: true);
    }

    private static Border MakeResolutionSection(AssaPropertiesViewModel vm)
    {
        var width = EditorChrome.MakeField(nameof(vm.VideoWidth), 0, 10000, 90, "W");
        var height = EditorChrome.MakeField(nameof(vm.VideoHeight), 0, 10000, 90, "H");
        AutomationProperties.SetName(width, Se.Language.General.Width);
        AutomationProperties.SetName(height, Se.Language.General.Height);

        var browse = EditorChrome.CreateToolButton("fa-solid fa-folder-open", Se.Language.General.VideoResolution, vm.BrowseResolutionCommand, 28);
        var fromVideo = EditorChrome.CreateToolButton("fa-solid fa-film", Se.Language.General.PickResolutionFromCurrentVideo, vm.GetResolutionFromCurrentVideoCommand, 28);
        fromVideo.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.ShowGetResolutionFromCurrentVideo)) { Source = vm });

        var size = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        size.Children.Add(width);
        size.Children.Add(new TextBlock { Text = "×", Foreground = EditorChrome.DimTextBrush, VerticalAlignment = VerticalAlignment.Center });
        size.Children.Add(height);
        size.Children.Add(browse);
        size.Children.Add(fromVideo);

        // Common sizes, one click each; the active one lit
        var presets = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        foreach (var (text, w, h) in new[] { ("640 × 360", 640, 360), ("1280 × 720", 1280, 720), ("1920 × 1080", 1920, 1080), ("3840 × 2160", 3840, 2160) })
        {
            var button = new Button
            {
                Content = new TextBlock { Text = text, FontSize = EditorChrome.FieldFontSize, Margin = new Thickness(8, 0) },
                Height = 26,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            button.Classes.Add("tool");
            AutomationProperties.SetName(button, text);
            button.Click += (_, _) =>
            {
                vm.VideoWidth = w;
                vm.VideoHeight = h;
            };

            void Update() => button.Classes.Set("active", vm.VideoWidth == w && vm.VideoHeight == h);
            Update();
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(vm.VideoWidth) or nameof(vm.VideoHeight))
                {
                    Update();
                }
            };
            presets.Children.Add(button);
        }

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(size);
        content.Children.Add(presets);
        return EditorChrome.MakeEditorSection(Se.Language.General.VideoResolution, content);
    }

    private static Border MakeOptionsSection(AssaPropertiesViewModel vm)
    {
        var wrapStyle = UiUtil.MakeComboBox(vm.WrapStyles, vm, nameof(vm.SelectedWrapStyle));
        wrapStyle.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(wrapStyle, Se.Language.Assa.WrapStyle);
        var scaling = UiUtil.MakeComboBox(vm.BorderAndShadowScalingStyles, vm, nameof(vm.SelectedBorderAndShadowScalingStyle));
        scaling.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(scaling, Se.Language.Assa.BorderAndShadowScaling);

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(Labeled(Se.Language.Assa.WrapStyle, wrapStyle), 0, 0);
        grid.Add(Labeled(Se.Language.Assa.BorderAndShadowScaling, scaling), 0, 1);
        return EditorChrome.MakeEditorSection(Se.Language.General.Options, grid);
    }

    private static StackPanel Labeled(string label, Control control)
    {
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(new TextBlock { Text = label, FontSize = EditorChrome.FieldFontSize, Foreground = EditorChrome.DimTextBrush, TextTrimming = TextTrimming.CharacterEllipsis });
        panel.Children.Add(control);
        return panel;
    }

    private static Border MakeButtonBar(AssaPropertiesViewModel vm)
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
