using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportDvbTeletext;

public class ExportDvbTeletextWindow : Window
{
    public ExportDvbTeletextWindow(ExportDvbTeletextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.TitleExportDvbTeletext;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelPageNumber = UiUtil.MakeLabel(Se.Language.File.Export.ExportDvbTeletextPageNumber);
        var numericPageNumber = UiUtil.MakeNumericUpDownInt(100, 899, 888, 120, vm, nameof(vm.PageNumber));

        var labelLanguage = UiUtil.MakeLabel(Se.Language.General.Language);
        var textBoxLanguage = UiUtil.MakeTextBox(120, vm, nameof(vm.LanguageCode));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(labelPageNumber, 0, 0);
        grid.Add(numericPageNumber, 0, 1);
        grid.Add(labelLanguage, 1, 0);
        grid.Add(textBoxLanguage, 1, 1);
        grid.Add(panelButtons, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { numericPageNumber.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.OnKeyDown;
    }
}
