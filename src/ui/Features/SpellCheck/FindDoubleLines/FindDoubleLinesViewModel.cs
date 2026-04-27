using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.SpellCheck.FindDoubleLines;

public partial class FindDoubleLinesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<DoubleLineItem> _subtitles;
    [ObservableProperty] private DoubleLineItem? _selectedSubtitle;
    [ObservableProperty] private bool _hasDoubleLines;

    public Window? Window { get; set; }

    public bool GoToPressed { get; private set; }

    public FindDoubleLinesViewModel()
    {
        Subtitles = new ObservableCollection<DoubleLineItem>();
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
        var addedIndices = new HashSet<int>();

        for (var i = 0; i < subtitleLineViewModels.Count - 1; i++)
        {
            var current = subtitleLineViewModels[i];
            var next = subtitleLineViewModels[i + 1];

            var currentText = HtmlUtil.RemoveHtmlTags(current.Text, true);
            var nextText = HtmlUtil.RemoveHtmlTags(next.Text, true);

            if (string.IsNullOrWhiteSpace(currentText) || string.IsNullOrWhiteSpace(nextText))
            {
                continue;
            }

            if (string.Equals(currentText, nextText, StringComparison.OrdinalIgnoreCase))
            {
                if (!addedIndices.Contains(i))
                {
                    Subtitles.Add(new DoubleLineItem(current));
                    addedIndices.Add(i);
                }

                if (!addedIndices.Contains(i + 1))
                {
                    Subtitles.Add(new DoubleLineItem(next));
                    addedIndices.Add(i + 1);
                }
            }
        }

        HasDoubleLines = Subtitles.Count > 0;
    }

    internal void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        HasDoubleLines = SelectedSubtitle != null;
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
