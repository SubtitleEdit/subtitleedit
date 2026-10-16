using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

/// <summary>
/// Edit one custom text format, in the editor look (see <see cref="EditorChrome"/>): name,
/// extension and time code/new line formats on top, the header/text/footer templates below
/// (right-click inserts a tag), the preview to the right, Cancel and OK in the button bar.
/// </summary>
public class EditCustomTextFormatWindow : Window
{
    public EditCustomTextFormatWindow(EditCustomTextFormatViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.TitleExportCustomFormat;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 700;
        MinHeight = 500;
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title)));
        vm.Window = this;
        DataContext = vm;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var sections = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        sections.Add(MakeFormatsView(vm, out var textBoxName), 0, 0);
        sections.Add(MakePreviewView(vm), 0, 1);

        Content = EditorChrome.MakeCancelOkRoot(sections, null, buttonCancel, buttonOk);

        UiUtil.FocusOnFirstActivation(this, textBoxName); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.OnKeyDown;
    }

    private static Grid MakeFormatsView(EditCustomTextFormatViewModel vm, out TextBox textBoxName)
    {
        textBoxName = EditorChrome.AsCenteredTextField(UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.Name)))
            .WithAccessibleName(Se.Language.General.Name);
        textBoxName.HorizontalAlignment = HorizontalAlignment.Stretch;

        // The extension keeps its dot handling in CustomFormatItem (#15699).
        var textBoxFileExtension = EditorChrome.AsCenteredTextField(UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.Extension)))
            .WithAccessibleName(Se.Language.General.FileExtension);
        textBoxFileExtension.HorizontalAlignment = HorizontalAlignment.Stretch;

        var textBoxHeader = MakeTemplateBox(vm, nameof(CustomFormatItem.FormatHeader), Se.Language.General.Header);
        vm.TextBoxHeader = textBoxHeader;

        var textBoxParagraph = MakeTemplateBox(vm, nameof(CustomFormatItem.FormatParagraph), Se.Language.General.Text);
        vm.TextBoxParagraph = textBoxParagraph;

        var textBoxTimeCode = new AutoCompleteBox
        {
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 160,
            BorderBrush = EditorChrome.FieldBorderBrush,
            PlaceholderText = Se.Language.General.TimeCodeFormat,
            ItemsSource = vm.TimeCodeList,
            [!AutoCompleteBox.TextProperty] = new Binding(nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.FormatTimeCode)),
            MinimumPrefixLength = 0,
        }.WithAccessibleName(Se.Language.General.TimeCodeFormat);

        var textBoxNewLine = new AutoCompleteBox
        {
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 160,
            BorderBrush = EditorChrome.FieldBorderBrush,
            PlaceholderText = Se.Language.File.Export.NewLineFormat,
            ItemsSource = vm.NewLineList,
            [!AutoCompleteBox.TextProperty] = new Binding(nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.FormatNewLine)),
            MinimumPrefixLength = 0,
        }.WithAccessibleName(Se.Language.File.Export.NewLineFormat);

        var textBoxFooter = MakeTemplateBox(vm, nameof(CustomFormatItem.FormatFooter), Se.Language.General.Footer);
        vm.TextBoxFooter = textBoxFooter;

        AddTagMenu(textBoxHeader, vm, vm.HeaderFooterTags, vm.InsertHeaderTagCommand);
        AddTagMenu(textBoxParagraph, vm, vm.ParagraphTags, vm.InsertParagraphTagCommand);
        AddTagMenu(textBoxFooter, vm, vm.HeaderFooterTags, vm.InsertFooterTagCommand);

        // Name, extension and the formats: labels in a column sized to the longest one, so
        // translations do not get clipped.
        var fields = new Grid
        {
            ColumnSpacing = 12,
            RowSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        var rows = new (string Label, Control Field)[]
        {
            (Se.Language.General.Name, textBoxName),
            (Se.Language.General.FileExtension, textBoxFileExtension),
            (Se.Language.General.TimeCodeFormat, textBoxTimeCode),
            (Se.Language.File.Export.NewLineFormat, textBoxNewLine),
        };
        for (var i = 0; i < rows.Length; i++)
        {
            fields.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            fields.Add(EditorChrome.MakeDimLabel(rows[i].Label.TrimEnd(':'), new Thickness(0)), i, 0);
            fields.Add(rows[i].Field, i, 1);
        }

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 90 }, // header
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 90 }, // text
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 90 }, // footer
            },
        };
        grid.Add(EditorChrome.MakeEditorSection(Se.Language.General.Format, fields, first: true), 0, 0);
        grid.Add(EditorChrome.MakeFillSection(Se.Language.General.Header, textBoxHeader), 1, 0);
        grid.Add(EditorChrome.MakeFillSection(Se.Language.General.Text, textBoxParagraph), 2, 0);
        grid.Add(EditorChrome.MakeFillSection(Se.Language.General.Footer, textBoxFooter), 3, 0);
        return grid;
    }

    private static TextBox MakeTemplateBox(EditCustomTextFormatViewModel vm, string property, string accessibleName)
    {
        var textBox = EditorChrome.AsTextArea(UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.SelectedCustomFormat) + "." + property));
        textBox.AcceptsReturn = true;
        textBox.Margin = new Thickness(0);
        return textBox.WithAccessibleName(accessibleName);
    }

    /// <summary>
    /// Right-click on a template box inserts one of its tags.
    /// </summary>
    private static void AddTagMenu(TextBox textBox, EditCustomTextFormatViewModel vm, IEnumerable<string> tags, System.Windows.Input.ICommand command)
    {
        var flyout = new MenuFlyout();
        textBox.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(textBox);
        foreach (var item in tags)
        {
            var menuItem = new MenuItem
            {
                Header = item,
                DataContext = vm,
                Command = command,
                CommandParameter = item,
            };
            flyout.Items.Add(menuItem);
        }
    }

    private static Border MakePreviewView(EditCustomTextFormatViewModel vm)
    {
        var textBox = EditorChrome.AsTextArea(new TextBox
        {
            AcceptsReturn = true,
            // Read-only preview - see ManualChosenEncodingWindow (#14313).
            AcceptsTab = false,
            IsReadOnly = true,
            Width = double.NaN,
            Height = double.NaN,
        }).WithAccessibleName(Se.Language.General.Preview);
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PreviewText)));

        return EditorChrome.MakeFillSection(Se.Language.General.Preview, textBox, lineLeft: true);
    }
}
