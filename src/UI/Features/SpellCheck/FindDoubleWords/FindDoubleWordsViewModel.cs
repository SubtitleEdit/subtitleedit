using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.SpellCheck.FindDoubleWords;

public partial class FindDoubleWordsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<DoubleWordItem> _subtitles;
    [ObservableProperty] private DoubleWordItem? _selectedSubtitle;
    [ObservableProperty] private bool _hasDoubleWords;

    public Window? Window { get; set; }

    public bool GoToPressed { get; private set; }

    public FindDoubleWordsViewModel()
    {
        Subtitles = new ObservableCollection<DoubleWordItem>();
    }

    [RelayCommand]
    private void GoTo()
    {
        GoToPressed = true;
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

    internal void Initialize(List<SubtitleLineViewModel> subtitleLineViewModels)
    {
        foreach (var subtitleLine in subtitleLineViewModels)
        {
            var doubleWord = GetDoubleWordMatch(subtitleLine.Text);
            if (!string.IsNullOrEmpty(doubleWord))
            {
                Subtitles.Add(new DoubleWordItem(subtitleLine, doubleWord));
            }
        }

        HasDoubleWords = SelectedSubtitle != null;
    }

    private string GetDoubleWordMatch(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Remove HTML/ASSA formatting
        var text = HtmlUtil.RemoveHtmlTags(input, true);

        // Split into words
        var separators = new[] { ' ', '\t', '\r', '\n' };
        var words = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        var list = new List<string>();

        string? prev = null;
        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i];

            if (string.IsNullOrWhiteSpace(word))
            {
                prev = word;
                continue;
            }

            if (prev != null && !IsAllLetters(prev))
            {
                prev = word;
                continue;
            }

            var trimmedWord = word.TrimEnd('.', ',', '!', '?');
            if (prev != null && string.Equals(prev, trimmedWord, StringComparison.OrdinalIgnoreCase))
            {
                list.Add(trimmedWord);
            }

            prev = word;
        }

        if (list.Count > 0)
        {
            return string.Join(", ", list);
        }

        return string.Empty;
    }

    private static bool IsAllLetters(string word)
    {
        foreach (var c in word)
        {
            if (!char.IsLetter(c))
            {
                return false;
            }
        }

        return true;
    }

    internal void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        HasDoubleWords = SelectedSubtitle != null;
    }

    internal void OnBookmarksGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(GoTo);
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            GoTo();
        }
    }
}