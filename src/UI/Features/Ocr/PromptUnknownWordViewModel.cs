using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class PromptUnknownWordViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _suggestions;
    [ObservableProperty] private string _selectedSuggestion;
    [ObservableProperty] private string? _text;
    [ObservableProperty] private string _wholeText;
    [ObservableProperty] private string _word;
    [ObservableProperty] private bool _doEditWholeText;
    [ObservableProperty] private bool _areSuggestionsEnabled;

    public StackPanel PanelWholeText { get; set; }
    public TextBox TextBoxWholeText { get; set; }
    public TextBox TextBoxWord { get; set; }
    public Bitmap? Bitmap { get; set; }
    public Window? Window { get; set; }

    public bool ChangeAllPressed { get; private set; }
    public bool ChangeOncePressed { get; private set; }
    public bool SkipAllPressed { get; private set; }
    public bool SkipOncePressed { get; private set; }
    public bool GoogleItPressed { get; private set; }
    public bool AddToNamesListPressed { get; private set; }
    public bool AddToUserDictionaryPressed { get; private set; }
    public bool ChangeWholeTextPressed { get; private set; }

    public PromptUnknownWordViewModel()
    {
        Suggestions = new ObservableCollection<string>();
        PanelWholeText = new StackPanel();
        TextBoxWholeText = new TextBox();
        TextBoxWord = new TextBox();
        SelectedSuggestion = string.Empty;
        Text = string.Empty;
        WholeText = string.Empty;
        Word = string.Empty;
    }

    public void Initialize(Bitmap bitmap, string wholeText, UnknownWordItem word, List<string> suggestions)
    {
        Bitmap = bitmap;
        WholeText = wholeText;
        Word = word.Word.FixedWord;
        Suggestions.AddRange(suggestions);

        Dispatcher.UIThread.Invoke(() =>
        {
            HighLightCurrentWord(word);
        });
    }

    private void HighLightCurrentWord(UnknownWordItem word)
    {
        var textBlock = new TextBlock();
        var idx = word.Word.WordIndex;
        var paragraph = word.Result.GetText();   
        var w = word.Word.FixedWord;

        if (!paragraph.Substring(idx).StartsWith(w))
        { 
            idx = paragraph.IndexOf(w, StringComparison.Ordinal);
            if (idx < 0)
            {
                // still not found - just show the whole text without highlighting
                textBlock.Inlines!.Add(new Run(paragraph));
                PanelWholeText.Children.Clear();
                PanelWholeText.Children.Add(textBlock);
                return;
            }
        }

        if (idx > 0)
        {
            var text = paragraph.Substring(0, idx);
            textBlock.Inlines!.Add(new Run(text));
        }

        textBlock.Inlines!.Add(new Run
        {
            Text = w,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.Red
        });

        if (idx + w.Length < paragraph.Length)
        {
            var text = paragraph.Substring(idx + w.Length);
            textBlock.Inlines!.Add(new Run(text));
        }

        PanelWholeText.Children.Clear();
        PanelWholeText.Children.Add(textBlock);
    }

    [RelayCommand]
    private void EditWholeText()
    {
    }

    [RelayCommand]
    private void EditWordOnly()
    {
    }

    [RelayCommand]
    private void SuggestionUse()
    {
        var selected = SelectedSuggestion;
        if (string.IsNullOrEmpty(selected))
        {
            return;
        }

        Word = SelectedSuggestion;
        ChangeOnce();
    }

    [RelayCommand]
    private void SuggestionUseAlways()
    {
        Word = SelectedSuggestion;
        ChangeAll();
    }

    [RelayCommand]
    private void ChangeAll()
    {
        ChangeAllPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void ChangeOnce()
    {
        if (DoEditWholeText)
        {
            ChangeWholeTextPressed = true;
        }
        else
        {
            ChangeOncePressed = true;
        }

        Window?.Close();
    }

    [RelayCommand]
    private void SkipOnce()
    {
        SkipOncePressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void GoogleIt()
    {

    }

    [RelayCommand]
    private void SkipAll()
    {
        SkipAllPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void AddToNamesList()
    {
        AddToNamesListPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void AddToUserDictionary()
    {
        AddToUserDictionaryPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        UiUtil.RestoreWindowPosition(Window);
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        UiUtil.SaveWindowPosition(Window);
    }

    internal void ListBoxSuggestionsDoubleTapped(object? sender, TappedEventArgs e)
    {
        SuggestionUse();
    }

    internal void OnEditWholeTextClicked()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Task.Delay(50).Wait();
            if (DoEditWholeText)
            {
                TextBoxWholeText.Focus();
            }
            else
            {
                TextBoxWord.Focus();
            }

            AreSuggestionsEnabled = SelectedSuggestion != null && !DoEditWholeText;
        });
    }

    internal void ListBoxSuggestionsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AreSuggestionsEnabled = SelectedSuggestion != null && !DoEditWholeText;
    }
}