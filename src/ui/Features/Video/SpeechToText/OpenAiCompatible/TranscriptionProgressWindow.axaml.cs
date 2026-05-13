using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Data;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;

public class TranscriptionProgressWindow : Window
{
    public TranscriptionProgressViewModel ViewModel => (TranscriptionProgressViewModel)DataContext!;

    public TranscriptionProgressWindow()
    {
        InitializeComponent();
    }

    public TranscriptionProgressWindow(TranscriptionProgressViewModel viewModel)
    {
        DataContext = viewModel;
        viewModel.Window = this;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Title = "Transcribing...";
        Width = 500;
        Height = 450;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Topmost = true;
        CanResize = false;

        var connectionInfoText = new TextBlock
        {
            FontSize = 11,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 2),
            TextWrapping = TextWrapping.Wrap
        };
        connectionInfoText.Bind(TextBlock.TextProperty, new Binding("ServerUrl") { StringFormat = "Server: {0}" });

        var modelInfoText = new TextBlock
        {
            FontSize = 11,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 10)
        };
        modelInfoText.Bind(TextBlock.TextProperty, new Binding("ModelName") { StringFormat = "Model: {0}" });

        var statusText = new TextBlock
        {
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        statusText.Bind(TextBlock.TextProperty, new Binding("StatusText"));

        var streamedTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontFamily = new FontFamily("Monospace"),
            FontSize = 12
        };
        var streamedBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(5),
            Child = new ScrollViewer { Content = streamedTextBlock }
        };
        streamedTextBlock.Bind(TextBlock.TextProperty, new Binding("StreamedText"));

        var segmentsList = new ListBox
        {
            MaxHeight = 150,
            Margin = new Thickness(0, 5, 0, 0)
        };
        segmentsList.Bind(ItemsControl.ItemsSourceProperty, new Binding("ReceivedSegments"));

        var expander = new Expander
        {
            Margin = new Thickness(0, 10, 0, 0),
            IsExpanded = true,
            Content = segmentsList
        };
        expander.Bind(HeaderedContentControl.HeaderProperty, 
            new Binding("SegmentCount") { StringFormat = "Received Segments ({0})" });

        var cancelButton = new Button
        {
            Content = "Cancel",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };
        cancelButton.Bind(Button.CommandProperty, new Binding("CancelCommand"));
        cancelButton.Bind(Button.IsVisibleProperty, new Binding("!IsCompleted"));

        var closeButton = new Button
        {
            Content = "Close",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0),
            IsDefault = true
        };
        closeButton.Bind(Button.CommandProperty, new Binding("CloseCommand"));
        closeButton.Bind(Button.IsVisibleProperty, new Binding("IsCompleted"));

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0),
            Children = { cancelButton, closeButton }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            Margin = new Thickness(10),
            Children = { connectionInfoText, modelInfoText, statusText, streamedBorder, expander, buttonPanel }
        };
        Grid.SetRow(connectionInfoText, 0);
        Grid.SetRow(modelInfoText, 1);
        Grid.SetRow(statusText, 2);
        Grid.SetRow(streamedBorder, 3);
        Grid.SetRow(expander, 4);
        Grid.SetRow(buttonPanel, 5);

        Content = grid;
    }
}
