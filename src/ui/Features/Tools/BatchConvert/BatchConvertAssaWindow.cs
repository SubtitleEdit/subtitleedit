using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

/// <summary>
/// Batch convert's ASSA target settings, in the editor look (see <see cref="EditorChrome"/>): the
/// source style options on top, the ASSA header with its edit actions under them, OK in the button bar.
/// </summary>
public class BatchConvertAssaWindow : Window
{
    public BatchConvertAssaWindow(BatchConvertAssaViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BatchConvert.BatchConvertSettings;
        CanResize = true;
        Width = 900;
        Height = 700;
        MinWidth = 600;
        MinHeight = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var keepSourceFonts = EditorChrome.MakeSwitchRow(nameof(vm.KeepSourceEmbeddedFonts), Se.Language.Tools.BatchConvert.KeepSourceEmbeddedFonts);
        keepSourceFonts.Bind(IsEnabledProperty, new Binding(nameof(vm.KeepSourceEmbeddedFontsIsEnabled)));

        var panelOptions = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeSwitchRow(nameof(vm.UseSourceStylesIfPossible), Se.Language.Tools.BatchConvert.UseSourceStylesIfPossible),
                keepSourceFonts,
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        grid.Add(new Border
        {
            Child = EditorChrome.MakeEditorSection(Se.Language.General.Options, panelOptions, first: true),
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
        }, 0, 0);
        grid.Add(MakeEditView(vm), 1, 0);
        grid.Add(EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk), 2, 0);

        Content = grid;

        Loaded += (_, _) =>
        {
            vm.Loaded();
            vm.SourceViewTextBox.Focus();
        };
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Border MakeEditView(BatchConvertAssaViewModel vm)
    {
        var contentBorder = new Border
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
        };
        vm.TextBoxContainer = contentBorder;

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Children =
            {
                EditorChrome.MakeActionButton("fa-solid fa-palette", Se.Language.Tools.BatchConvert.EditStyles, vm.EditStylesCommand),
                EditorChrome.MakeActionButton("fa-solid fa-sliders", Se.Language.Tools.BatchConvert.EditProperties, vm.EditPropertiesCommand),
                EditorChrome.MakeActionButton("fa-solid fa-paperclip", Se.Language.Tools.BatchConvert.EditAttachments, vm.EditAttachmentCommand),
            },
        };

        var header = new DockPanel();
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.Tools.BatchConvert.AssaSource));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 8,
        };

        grid.Add(header, 0, 0);
        grid.Add(EditorChrome.MakeListFrame(contentBorder), 1, 0);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(14, 10, 14, 14),
        };
    }
}
