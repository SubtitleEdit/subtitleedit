using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public class DownloadSpeechToTextModelsWindow : Window
{
    public DownloadSpeechToTextModelsWindow(DownloadSpeechToTextModelsViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AudioToText.DownloadingSpeechToTextModel;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 500;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContext = vm;

        var labelSelectModel = new TextBlock
        {
            Text = Se.Language.Video.AudioToText.SelectModel,
        };

        var comboBoxModel = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel)).WithMinWidth(200);

        var buttonDownload = UiUtil.MakeButton(Se.Language.General.Download, vm.DownloadCommand).WithLeftAlignment();
        buttonDownload.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.DownloadIsEnabled)));

        var buttonOpenFolder = new Button
        {
            Margin = new Thickness(4, 0),
            Padding = new Thickness(12, 6),
            Height = 33,
            Command = vm.OpenModelFolderCommand,
        };
        Attached.SetIcon(buttonOpenFolder, "fa-solid fa-folder-open");

        var buttonAddCustomModel = UiUtil.MakeButton(Se.Language.Video.AudioToText.AddCustomModelDotDotDot, vm.AddCustomModelCommand);
        buttonAddCustomModel.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.SupportsCustomModels)));

        var panelModelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 0),
            Children =
            {
                buttonDownload,
                buttonOpenFolder,
                buttonAddCustomModel
            }
        };

        var labelCustomModelHelp = new TextBlock
        {
            Text = Se.Language.Video.AudioToText.CustomModelHelp,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.6,
            MaxWidth = 460,
            Margin = new Thickness(0, 6, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        labelCustomModelHelp.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.SupportsCustomModels)));


        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Margin = new Thickness(0, 0, 0, 8);
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.OpacityProperty, new Binding(nameof(vm.ProgressOpacity)));
        var statusText = new TextBlock
        {
            Margin = new Thickness(0, 1, 0, 1),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));
        statusText.Bind(TextBlock.OpacityProperty, new Binding(nameof(vm.ProgressOpacity)));

        var fileText = new TextBlock
        {
            Margin = new Thickness(0, 1, 0, 1),
            HorizontalAlignment = HorizontalAlignment.Center,
            Opacity = 0.5,
        };
        fileText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressFileName)));

        var panelStatus = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 35, 0, 10),
            Children =
            {
                progressBar,
                statusText,
                fileText,
            }
        };

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonCancel);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            RowDefinitions = new RowDefinitions("Auto, Auto, Auto, Auto, Auto"),
            Margin = UiUtil.MakeWindowMargin(),
        };

        var row = 0;
        grid.Children.Add(labelSelectModel);
        Grid.SetColumnSpan(labelSelectModel, 2);
        Grid.SetRow(labelSelectModel, row);
        Grid.SetColumn(labelSelectModel, 0);
        row++;

        grid.Children.Add(comboBoxModel);
        Grid.SetRow(comboBoxModel, row);
        Grid.SetColumn(comboBoxModel, 0);

        grid.Children.Add(panelModelButtons);
        Grid.SetRow(panelModelButtons, row);
        Grid.SetColumn(panelModelButtons, 1);
        row++;

        grid.Children.Add(labelCustomModelHelp);
        Grid.SetColumnSpan(labelCustomModelHelp, 2);
        Grid.SetRow(labelCustomModelHelp, row);
        Grid.SetColumn(labelCustomModelHelp, 0);
        row++;

        grid.Children.Add(panelStatus);
        Grid.SetColumnSpan(panelStatus, 2);
        Grid.SetRow(panelStatus, row);
        Grid.SetColumn(panelStatus, 0);
        row++;

        grid.Children.Add(buttonBar);
        Grid.SetColumnSpan(buttonBar, 2);
        Grid.SetRow(buttonBar, row);
        Grid.SetColumn(buttonBar, 0);

        Content = grid;

        Activated += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}