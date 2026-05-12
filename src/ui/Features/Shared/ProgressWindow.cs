using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Shared;

public class ProgressWindow : Window
{
    private readonly ProgressWindowViewModel _viewModel;

    public ProgressWindow(ProgressWindowViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Progress";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContext = _viewModel;

        var titleText = new TextBlock
        {
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };
        titleText.Bind(TextBlock.TextProperty, new Binding(nameof(ProgressWindowViewModel.StatusText)));

        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Height = 20,
            MinWidth = 400,
        };
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(ProgressWindowViewModel.Progress)));

        var statusText = new TextBlock();
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(ProgressWindowViewModel.StatusText)));

        var buttonCancel = UiUtil.MakeButtonCancel(_viewModel.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonCancel);

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                titleText,
                progressBar,
                statusText,
                buttonBar,
            }
        };
    }

    public void UpdateProgress(double value, string? statusText = null)
    {
        _viewModel.UpdateProgress(value, statusText);
    }
}
