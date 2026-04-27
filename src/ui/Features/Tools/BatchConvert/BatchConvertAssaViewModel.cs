using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertAssaViewModel : ObservableObject
{
    [ObservableProperty] private bool _useSourceStylesIfPossible;
    [ObservableProperty] private string _text;
    [ObservableProperty] private StyleDisplay? _currentStyle;
    [ObservableProperty] private ObservableCollection<string> _fonts;
    [ObservableProperty] private ObservableCollection<BorderStyleItem> _borderTypes;
    [ObservableProperty] private BorderStyleItem _selectedBorderType;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Border TextBoxContainer { get; set; }
    public ITextBoxWrapper SourceViewTextBox { get; set; }

    private readonly IWindowService _windowService;

    private Subtitle _subtitle;

    public BatchConvertAssaViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Text = string.Empty;
        SourceViewTextBox = new TextEditorWrapper(new TextEditor(), new Border());
        TextBoxContainer = new Border();
        Fonts = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        BorderTypes = new ObservableCollection<BorderStyleItem>(BorderStyleItem.List());
        SelectedBorderType = BorderTypes[0];
        _subtitle = new Subtitle();
        _subtitle.Paragraphs.Add(new Paragraph("Sample subtitle", 0, 2000));
        Text = _subtitle.ToText(new AdvancedSubStationAlpha());

        UseSourceStylesIfPossible = Se.Settings.Tools.BatchConvert.AssaUseSourceStylesIfPossible;
        _subtitle.Header = Se.Settings.Tools.BatchConvert.AssaHeader;
        _subtitle.Footer = Se.Settings.Tools.BatchConvert.AssaFooter;
    }

    [RelayCommand]
    private async Task EditStyles()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<AssaStylesWindow, AssaStylesViewModel>(Window, vm =>
        {
            vm.Initialize(_subtitle, new AdvancedSubStationAlpha(), string.Empty, string.Empty, null);
        });

        if (result.OkPressed)
        {
            _subtitle.Header = result.Header;
            Text = _subtitle.ToText(new AdvancedSubStationAlpha());
        }
    }

    [RelayCommand]
    private async Task EditAttachment()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<AssaAttachmentsWindow, AssaAttachmentsViewModel>(Window, vm =>
        {
            vm.Initialize(_subtitle, new AdvancedSubStationAlpha(), string.Empty);
        });

        if (result.OkPressed)
        {
            _subtitle.Footer = result.Footer;
            Text = _subtitle.ToText(new AdvancedSubStationAlpha());
        }
    }

    [RelayCommand]
    private async Task EditProperties()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<AssaPropertiesWindow, AssaPropertiesViewModel>(Window, vm =>
        {
            vm.Initialize(_subtitle, new AdvancedSubStationAlpha(), string.Empty, string.Empty);
        });

        if (result.OkPressed)
        {
            _subtitle.Header = result.Header;
            Text = _subtitle.ToText(new AdvancedSubStationAlpha());
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Se.Settings.Tools.BatchConvert.AssaUseSourceStylesIfPossible = UseSourceStylesIfPossible;
        Se.Settings.Tools.BatchConvert.AssaHeader = _subtitle.Header;
        Se.Settings.Tools.BatchConvert.AssaFooter = _subtitle.Footer;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private TextBoxWrapper CreateSimpleTextBoxWrapper()
    {
        var textBox = new TextBox
        {
            Margin = new Thickness(0, 0, 10, 0),
            [!TextBox.TextProperty] = new Binding(nameof(Text)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            AcceptsReturn = true,
        };

        return new TextBoxWrapper(textBox);
    }

    private TextEditorWrapper CreateAdvancedTextBoxWrapper(string text)
    {
        var textBox = new TextEditor
        {
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ShowLineNumbers = true,
            WordWrap = false,
        };

        // Override the built-in link color with our softer pastel color
        textBox.TextArea.TextView.LinkTextForegroundBrush = UiUtil.MakeLinkForeground();

        // Add syntax highlighting for subtitle source formats
        var lineTransformer = new AssaSourceSyntaxHighlighting();
        if (lineTransformer != null)
        {
            textBox.TextArea.TextView.LineTransformers.Add(lineTransformer);
        }

        // Setup two-way binding manually since TextEditor doesn't support direct binding
        var isUpdatingFromViewModel = false;
        var isUpdatingFromEditor = false;

        void UpdateEditorFromViewModel()
        {
            if (isUpdatingFromEditor)
            {
                return;
            }

            isUpdatingFromViewModel = true;
            try
            {
                var text = Text ?? string.Empty;
                if (textBox.Text != text)
                {
                    textBox.Text = text;
                }
            }
            finally
            {
                isUpdatingFromViewModel = false;
            }
        }

        void UpdateViewModelFromEditor()
        {
            if (isUpdatingFromViewModel)
            {
                return;
            }

            isUpdatingFromEditor = true;
            try
            {
                if (Text != textBox.Text)
                {
                    Text = textBox.Text;
                }
            }
            finally
            {
                isUpdatingFromEditor = false;
            }
        }

        // Listen to ViewModel changes
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Text))
            {
                UpdateEditorFromViewModel();
            }
        };

        // Listen to TextEditor changes
        textBox.TextChanged += (s, e) => UpdateViewModelFromEditor();

        // Initial text load
        UpdateEditorFromViewModel();

        var textBoxBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            Child = textBox,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        return new TextEditorWrapper(textBox, textBoxBorder);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Loaded()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Task.Delay(50).Wait(); // Slight delay to ensure control is ready  

            var useSimpleTextBox = Text.Length > 2_000_000;
            if (useSimpleTextBox)
            {
                SourceViewTextBox = CreateSimpleTextBoxWrapper();
            }
            else
            {
                SourceViewTextBox = CreateAdvancedTextBoxWrapper(Text);
            }

            TextBoxContainer.Child = SourceViewTextBox.ContentControl;

            Task.Delay(50).Wait(); // Slight delay to ensure control is ready  
            SourceViewTextBox.Focus();
            SourceViewTextBox.CaretIndex = 0;
        }, DispatcherPriority.Input);
    }
}