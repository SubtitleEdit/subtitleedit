using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.ResolutionResampler;

/// <summary>
/// Changes the script resolution and rescales what depends on it - styled like ASSA draw:
/// source and target as compact W × H fields with presets, the options as toggles, and
/// the scale factor in the button bar.
/// </summary>
public class AssaResolutionResamplerWindow : Window
{
    private static readonly (int Width, int Height)[] Presets = [(640, 360), (1280, 720), (1920, 1080), (3840, 2160)];

    public AssaResolutionResamplerWindow(AssaResolutionResamplerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.ResolutionResamplerTitle;
        CanResize = false;
        Width = 560;
        SizeToContent = SizeToContent.Height;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        var sections = new StackPanel { Background = EditorChrome.PanelBrush };

        // Opened as the video-open prompt: say why the dialog appeared (#14367)
        if (vm.IsVideoPrompt)
        {
            sections.Children.Add(MakePromptBanner(vm.PromptText));
        }

        sections.Children.Add(MakeResolutionSection(vm, Se.Language.Assa.ResolutionResamplerSourceRes,
            nameof(vm.SourceWidth), nameof(vm.SourceHeight), vm.SourceFromVideoCommand,
            () => (vm.SourceWidth, vm.SourceHeight), (w, h) => { vm.SourceWidth = w; vm.SourceHeight = h; },
            first: !vm.IsVideoPrompt, out var sourceWidthBox));
        sections.Children.Add(MakeResolutionSection(vm, Se.Language.Assa.ResolutionResamplerTargetRes,
            nameof(vm.TargetWidth), nameof(vm.TargetHeight), vm.TargetFromVideoCommand,
            () => (vm.TargetWidth, vm.TargetHeight), (w, h) => { vm.TargetWidth = w; vm.TargetHeight = h; },
            first: false, out _));
        sections.Children.Add(MakeOptionsSection(vm));

        var root = new DockPanel();
        var buttonBar = MakeButtonBar(vm);
        DockPanel.SetDock(buttonBar, Dock.Bottom);
        root.Children.Add(buttonBar);
        root.Children.Add(sections);
        Content = root;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, sourceWidthBox);
        KeyDown += vm.KeyDown;
        Closing += (_, _) => vm.OnClosing();
    }

    private static Border MakePromptBanner(string text)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        row.Children.Add(new Optris.Icons.Avalonia.Icon
        {
            Value = "fa-solid fa-circle-info",
            FontSize = 14,
            Foreground = EditorChrome.AccentBrush,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 1, 0, 0),
        });
        row.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = EditorChrome.FieldFontSize,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 490,
        });

        return new Border
        {
            Child = row,
            Background = EditorChrome.AccentSoftBrush,
            Padding = new Thickness(14, 10),
        };
    }

    private static Border MakeResolutionSection(
        AssaResolutionResamplerViewModel vm,
        string title,
        string widthProperty,
        string heightProperty,
        System.Windows.Input.ICommand fromVideoCommand,
        Func<(int Width, int Height)> get,
        Action<int, int> set,
        bool first,
        out NumericUpDown widthBox)
    {
        widthBox = EditorChrome.MakeField(widthProperty, 1, 8192, 90, "W");
        var heightBox = EditorChrome.MakeField(heightProperty, 1, 8192, 90, "H");
        AutomationProperties.SetName(widthBox, title + " - " + Se.Language.General.Width);
        AutomationProperties.SetName(heightBox, title + " - " + Se.Language.General.Height);
        var fromVideo = EditorChrome.CreateToolButton("fa-solid fa-film", Se.Language.Assa.ResolutionResamplerFromVideo, fromVideoCommand, 28);

        var size = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        size.Children.Add(widthBox);
        size.Children.Add(new TextBlock { Text = "×", Foreground = EditorChrome.DimTextBrush, VerticalAlignment = VerticalAlignment.Center });
        size.Children.Add(heightBox);
        size.Children.Add(fromVideo);

        // Common sizes, one click each; the active one lit
        var presets = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        foreach (var (w, h) in Presets)
        {
            var text = $"{w} × {h}";
            var button = new Button
            {
                Content = new TextBlock { Text = text, FontSize = EditorChrome.FieldFontSize, Margin = new Thickness(8, 0) },
                Height = EditorChrome.ScaledSize(26),
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            button.Classes.Add("tool");
            AutomationProperties.SetName(button, title + " " + text);
            button.Click += (_, _) => set(w, h);

            void Update() => button.Classes.Set("active", get() == (w, h));
            Update();
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == widthProperty || e.PropertyName == heightProperty)
                {
                    Update();
                }
            };
            presets.Children.Add(button);
        }

        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(size);
        content.Children.Add(presets);
        return EditorChrome.MakeEditorSection(title, content, first: first);
    }

    private static Border MakeOptionsSection(AssaResolutionResamplerViewModel vm)
    {
        var content = new StackPanel { Spacing = 4 };
        content.Children.Add(EditorChrome.MakeToggleRow(nameof(vm.ChangeMargins), Se.Language.Assa.ResolutionResamplerChangeMargins));
        content.Children.Add(EditorChrome.MakeToggleRow(nameof(vm.ChangeFontSize), Se.Language.Assa.ResolutionResamplerChangeFontSize));
        content.Children.Add(EditorChrome.MakeToggleRow(nameof(vm.ChangePositions), Se.Language.Assa.ResolutionResamplerChangePositions));
        content.Children.Add(EditorChrome.MakeToggleRow(nameof(vm.ChangeDrawing), Se.Language.Assa.ResolutionResamplerChangeDrawing));
        if (vm.IsVideoPrompt)
        {
            content.Children.Add(EditorChrome.MakeToggleRow(nameof(vm.AskOnVideoOpen), Se.Language.Assa.ResolutionResamplerAskOnVideoOpen));
        }

        return EditorChrome.MakeEditorSection(Se.Language.General.Options, content);
    }

    private static Border MakeButtonBar(AssaResolutionResamplerViewModel vm)
    {
        // How much everything is scaled, e.g. "×5 · ×3.75"
        var scale = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };

        void UpdateScale()
        {
            if (vm.SourceWidth <= 0 || vm.SourceHeight <= 0)
            {
                scale.Text = string.Empty;
                return;
            }

            var x = (double)vm.TargetWidth / vm.SourceWidth;
            var y = (double)vm.TargetHeight / vm.SourceHeight;
            scale.Text = Math.Abs(x - y) < 0.005 ? $"×{x:0.##}" : $"×{x:0.##} · ×{y:0.##}";
        }

        UpdateScale();
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(vm.SourceWidth) or nameof(vm.SourceHeight) or nameof(vm.TargetWidth) or nameof(vm.TargetHeight))
            {
                UpdateScale();
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(EditorChrome.WithoutMargin(buttonCancel));
        buttons.Children.Add(EditorChrome.WithoutMargin(buttonOk));

        var bar = new DockPanel();
        DockPanel.SetDock(buttons, Dock.Right);
        bar.Children.Add(buttons);
        bar.Children.Add(scale);

        return EditorChrome.WrapButtonBar(bar);
    }
}
