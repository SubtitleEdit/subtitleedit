using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

public class EditEmbeddedTrackWindow : Window
{
    public EditEmbeddedTrackWindow(EditEmbeddedTrackViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Settings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var labelName = UiUtil.MakeLabel(Se.Language.General.Name);
        var textBoxName = UiUtil.MakeTextBox(300, vm, nameof(vm.Name)); 

        var labelTitleOrLanguage = UiUtil.MakeLabel(Se.Language.General.Language);
        var textBoxTitleOrLanguage = UiUtil.MakeTextBox(300, vm, nameof(vm.TitleOrlanguage));

        var checkBoxForced = UiUtil.MakeCheckBox(Se.Language.General.Forced, vm, nameof(vm.IsForced));
        var checkBoxDefault = UiUtil.MakeCheckBox(Se.Language.General.Default, vm, nameof(vm.IsDefault));

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelName, 0, 0);
        grid.Add(textBoxName, 0, 1);

        grid.Add(labelTitleOrLanguage, 1, 0);
        grid.Add(textBoxTitleOrLanguage, 1, 1);

        grid.Add(checkBoxForced, 2, 1);

        grid.Add(checkBoxDefault, 3, 1);

        grid.Add(buttonPanel, 4, 0, 1, 2);


        Content = grid;

        Loaded += (_,e) => vm.EditEmbeddedTrackWindowLoaded(e);
        KeyDown += (_, args) => vm.OnKeyDown(args);
    }
}