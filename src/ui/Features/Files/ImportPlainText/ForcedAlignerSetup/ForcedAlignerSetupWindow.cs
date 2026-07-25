using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText.ForcedAlignerSetup;

public class ForcedAlignerSetupWindow : Window
{
    public ForcedAlignerSetupWindow(ForcedAlignerSetupViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        // Explicit width plus height-only auto-sizing: WidthAndHeight comes out far too
        // wide on macOS.
        Width = 620;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        Title = Se.Language.File.Import.ForcedAlignerSetupTitle;
        vm.Window = this;
        DataContext = vm;

        var labelIntro = UiUtil.MakeTextBlock(Se.Language.File.Import.ForcedAlignerSetupIntro);
        labelIntro.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        labelIntro.MaxWidth = 570;

        var labelEngine = UiUtil.MakeLabel(Se.Language.File.Import.ForcedAlignerEngine);
        var labelEngineStatus = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.EngineStatus));
        var buttonDownloadEngine = UiUtil.MakeButton(
            Se.Language.File.Import.ForcedAlignerDownloadEngine, vm.DownloadEngineCommand);

        var labelAligner = UiUtil.MakeLabel(Se.Language.File.Import.ForcedAlignerModel);
        var comboAligner = UiUtil.MakeComboBox(vm.Aligners, vm, nameof(vm.SelectedAligner));
        comboAligner.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboAligner.Width = 420;

        var labelAlignerStatus = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.AlignerStatus));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // intro
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // engine
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // engine status
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // aligner
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // aligner status
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelIntro, 0, 0, 1, 2);
        grid.Add(labelEngine, 1, 0);
        grid.Add(buttonDownloadEngine, 1, 1);
        grid.Add(labelEngineStatus, 2, 0, 1, 2);
        grid.Add(labelAligner, 3, 0);
        grid.Add(comboAligner, 3, 1);
        grid.Add(labelAlignerStatus, 4, 0, 1, 2);
        grid.Add(panelButtons, 5, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
