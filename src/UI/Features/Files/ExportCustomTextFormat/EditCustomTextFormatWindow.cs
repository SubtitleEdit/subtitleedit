using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

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

        var label = new Label
        {
            Content = Se.Language.Tools.AdjustDurations.AdjustVia,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeFormatsView(vm), 0);
        grid.Add(MakePreviewView(vm), 0, 1);
        grid.Add(panelButtons, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
    }

    private static Grid MakeFormatsView(EditCustomTextFormatViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // header
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // text
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // footer
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelName = UiUtil.MakeLabel(Se.Language.General.Name).WithMinWidth(100);
        var textBoxName = UiUtil.MakeTextBox(200, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.Name));
        var panelName = UiUtil.MakeHorizontalPanel(labelName, textBoxName);
        grid.Add(panelName, 0);

        var labelFileExtension = UiUtil.MakeLabel(Se.Language.General.FileExtension).WithMinWidth(100);
        var textBoxFileExtension = UiUtil.MakeTextBox(200, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.Extension));
        var panelFileExtension = UiUtil.MakeHorizontalPanel(labelFileExtension, textBoxFileExtension).WithMarginTop(5);
        grid.Add(panelFileExtension, 1);

        var labelHeader = UiUtil.MakeLabel(Se.Language.General.Header).WithMarginTop(5);
        var textBoxHeader = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.FormatHeader));
        textBoxHeader.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxHeader.VerticalAlignment = VerticalAlignment.Stretch;
        textBoxHeader.AcceptsReturn = true;
        vm.TextBoxHeader = textBoxHeader;
        grid.Add(labelHeader, 2);
        grid.Add(textBoxHeader, 3);

        var labelText = UiUtil.MakeLabel(Se.Language.General.Text).WithMarginTop(5);
        var textBoxParagraph = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.FormatParagraph));
        textBoxParagraph.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxParagraph.VerticalAlignment = VerticalAlignment.Stretch;
        textBoxParagraph.AcceptsReturn = true;
        vm.TextBoxParagraph = textBoxParagraph;
        grid.Add(labelText, 4);
        grid.Add(textBoxParagraph, 5);

        var labelTimeCodeFormat = UiUtil.MakeLabel(Se.Language.File.Export.TimeCodeFormat).WithMinWidth(120);
        var textBoxTimeCode = new AutoCompleteBox
        {
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 200,
            Margin = new Thickness(0, 0, 0, 3),
            Watermark = Se.Language.File.Export.TimeCodeFormat,
            ItemsSource = vm.TimeCodeList,
            [!AutoCompleteBox.TextProperty] = new Binding(nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.FormatTimeCode)),
            MinimumPrefixLength = 0,
        };

        var panelTimeCode = UiUtil.MakeHorizontalPanel(labelTimeCodeFormat, textBoxTimeCode).WithMarginTop(4);
        grid.Add(panelTimeCode, 6);

        var labelNewLineFormat = UiUtil.MakeLabel(Se.Language.File.Export.NewLineFormat).WithMinWidth(120);
        var textBoxNewLine = new AutoCompleteBox
        {
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 200,
            Margin = new Thickness(0, 0, 0, 3),
            Watermark = Se.Language.File.Export.TimeCodeFormat,
            ItemsSource = vm.NewLineList,
            [!AutoCompleteBox.TextProperty] = new Binding(nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.FormatNewLine)),
            MinimumPrefixLength = 0,
        };

        var panelNewLine = UiUtil.MakeHorizontalPanel(labelNewLineFormat, textBoxNewLine).WithMarginTop(4);
        grid.Add(panelNewLine, 7);

        var labelFooter = UiUtil.MakeLabel(Se.Language.General.Footer).WithMarginTop(5);
        var textBoxFooter = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.FormatFooter));
        textBoxFooter.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxFooter.VerticalAlignment = VerticalAlignment.Stretch;
        textBoxFooter.AcceptsReturn = true;
        vm.TextBoxFooter = textBoxFooter;
        grid.Add(labelFooter, 8);
        grid.Add(textBoxFooter, 9);


        var flyoutHeader = new MenuFlyout();
        textBoxHeader.ContextFlyout = flyoutHeader;
        foreach (var item in vm.HeaderFooterTags)
        {
            var menuItem = new MenuItem
            {
                Header = item,
                DataContext = vm,
                Command = vm.InsertHeaderTagCommand,
                CommandParameter = item,
            };
            flyoutHeader.Items.Add(menuItem);
        }

        var flyoutParagraph = new MenuFlyout();
        textBoxParagraph.ContextFlyout = flyoutParagraph;
        foreach (var item in vm.ParagraphTags)
        {
            var menuItem = new MenuItem
            {
                Header = item,
                DataContext = vm,
                Command = vm.InsertParagraphTagCommand,
                CommandParameter = item,
            };
            flyoutParagraph.Items.Add(menuItem);
        }

        var flyoutFooter = new MenuFlyout();
        textBoxFooter.ContextFlyout = flyoutFooter;
        foreach (var item in vm.HeaderFooterTags)
        {
            var menuItem = new MenuItem
            {
                Header = item,
                DataContext = vm,
                Command = vm.InsertFooterTagCommand,
                CommandParameter = item,
            };
            flyoutFooter.Items.Add(menuItem);
        }

        return grid;
    }

    private static Grid MakePreviewView(EditCustomTextFormatViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(UiUtil.MakeLabel(Se.Language.General.Preview), 0);
        var textBox = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = true,
            IsReadOnly = true,
            Width = double.NaN,
            Height = double.NaN,
        };
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PreviewText)));

        grid.Add(textBox, 1);

        return grid;
    }
}
