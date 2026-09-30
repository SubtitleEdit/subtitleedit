using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.PickDvdTitle;

public partial class PickDvdTitleViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<DvdTitleDisplay> _titles;
    [ObservableProperty] private DvdTitleDisplay? _selectedTitle;
    [ObservableProperty] private string _videoInfo;

    public Window? Window { get; set; }
    public TableView TitlesGrid { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }
    public DvdTitle? SelectedDvdTitle { get; private set; }

    public PickDvdTitleViewModel()
    {
        Titles = new ObservableCollection<DvdTitleDisplay>();
        TitlesGrid = new TableView();
        WindowTitle = string.Empty;
        VideoInfo = string.Empty;
    }

    public void Initialize(List<DvdTitle> titles, DvdTitle? defaultTitle, string fileName)
    {
        WindowTitle = UiUtil.FormatTitleWithFileName(Se.Language.File.PickDvdTitleX, Path.GetFileName(fileName));
        var multipleTitleSets = titles.Select(p => p.TitleSetNumber).Distinct().Count() > 1;
        foreach (var title in titles)
        {
            var name = multipleTitleSets
                ? $"{title.TitleSetNumber}.{title.ProgramChain.Number}"
                : title.ProgramChain.Number.ToString(CultureInfo.InvariantCulture);
            var languages = string.Join(", ", title.Ifo.SubtitleStreams
                .Where(p => title.ProgramChain.HasSubtitleStream(p.Index))
                .Select(p => p.ToString())
                .Distinct());
            var status = title.IsComplete
                ? string.Empty
                : string.Format(Se.Language.File.DvdVobFilesMissingX, (int)Math.Round(title.AvailableShare * 100));
            Titles.Add(new DvdTitleDisplay(title, name, languages, status));
        }

        var index = defaultTitle == null ? 0 : titles.IndexOf(defaultTitle);
        SelectedTitle = Titles.ElementAtOrDefault(Math.Max(0, index));
        UpdateVideoInfo();
    }

    partial void OnSelectedTitleChanged(DvdTitleDisplay? value)
    {
        UpdateVideoInfo();
    }

    private void UpdateVideoInfo()
    {
        var title = SelectedTitle?.Title;
        VideoInfo = title == null
            ? string.Empty
            : $"{Path.GetFileName(title.IfoFileName)}: {title.Ifo.Video}, {title.VobFileNames.Count} VOB";
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    [RelayCommand]
    private void Ok()
    {
        if (SelectedTitle == null)
        {
            return;
        }

        SelectedDvdTitle = SelectedTitle.Title;
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    internal void OnKeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && TitlesGrid.IsKeyboardFocusWithin)
        {
            Ok();
            e.Handled = true;
        }
    }

    internal void SelectAndScrollToSelected()
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Assign the view model property as well as the grid index - AlwaysSelected has
            // already put the grid on row 0, so the index alone raises no SelectionChanged.
            var selected = SelectedTitle ?? Titles.FirstOrDefault();
            if (selected == null)
            {
                return;
            }

            SelectedTitle = selected;
            TitlesGrid.SelectedIndex = Titles.IndexOf(selected);
            TitlesGrid.ScrollIntoView(selected);
        }, DispatcherPriority.Background);
    }
}