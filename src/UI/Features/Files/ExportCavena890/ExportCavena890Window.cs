using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportCavena890;

public class ExportCavena890Window : Window
{
    public ExportCavena890Window(ExportCavena890ViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Export Cavena 890";
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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
        
        var buttonImport = UiUtil.MakeButton(Se.Language.General.Import, vm.OkCommand).WithRightAlignment();

        var labelTranslatedTitle = UiUtil.MakeLabel("Translated title");
        var textBoxTranslatedTitle = UiUtil.MakeTextBox(300, vm, nameof(vm.TranslatedTitle));
        
        var labelOriginalTitle = UiUtil.MakeLabel("Original title");
        var textBoxOriginalTitle = UiUtil.MakeTextBox(300, vm, nameof(vm.OriginalTitle));
        
        var labelTranslator = UiUtil.MakeLabel("Translator");
        var textBoxTranslator = UiUtil.MakeTextBox(300, vm, nameof(vm.Translator));
        
        var labelComment =  UiUtil.MakeLabel("Comment");
        var textBoxComment = UiUtil.MakeTextBox(300, vm, nameof(vm.Comment));
        
        var labelLanguage = UiUtil.MakeLabel("Language");
        var textBoxLanguage = UiUtil.MakeTextBox(300, vm, nameof(vm.Language));
        
        var labelStartOfProgramme = UiUtil.MakeLabel("Start of programme");
        var textBoxStartOfProgramme = UiUtil.MakeTextBox(300, vm, nameof(vm.StartOfProgramme));
        
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar( buttonOk, buttonCancel);

        grid.Add(buttonImport, 0, 1);
        grid.Add(labelTranslatedTitle, 1, 0);
        grid.Add(textBoxTranslatedTitle, 1, 1);
        grid.Add(labelOriginalTitle, 2, 0);
        grid.Add(textBoxOriginalTitle, 2, 1);
        grid.Add(labelTranslator, 3, 0);
        grid.Add(textBoxTranslator, 3, 1);
        grid.Add(labelComment, 4, 0);
        grid.Add(textBoxComment, 4, 1);
        grid.Add(labelLanguage, 5, 0);
        grid.Add(textBoxLanguage, 5, 1);
        grid.Add(labelStartOfProgramme, 6, 0);
        grid.Add(textBoxStartOfProgramme, 6, 1);
        grid.Add(panelButtons, 7, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
    }
}
