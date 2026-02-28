using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nikse.SubtitleEdit.Features.Shared.Bookmarks;

public partial class BookmarkEditViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string? _bookmarkText;
    [ObservableProperty] private bool _showRemoveButton;
    [ObservableProperty] private bool _showListButton;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public bool ListPressed { get; private set; }

    private List<SubtitleLineViewModel> _selectedItems;

    public BookmarkEditViewModel()
    {
        Title = string.Empty;
        BookmarkText = string.Empty;
        _selectedItems = new List<SubtitleLineViewModel>();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void List()
    {
        ListPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Delete()
    {
        BookmarkText = null;
        OkPressed = true;
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

    internal void Initialize(List<SubtitleLineViewModel> selectedItems, List<SubtitleLineViewModel> allBookmarks)
    {
        Title = selectedItems.All(p => p.Bookmark == null) ? GetAddLabel(selectedItems) : GetEditLabel(selectedItems);
        BookmarkText = selectedItems.First().Bookmark ?? string.Empty;
        ShowRemoveButton = !selectedItems.All(p => p.Bookmark == null);
        ShowListButton = allBookmarks.Count > 1;
        _selectedItems = selectedItems;
    }

    private string GetEditLabel(List<SubtitleLineViewModel> selectedItems)
    {
        if (selectedItems.Count > 1)
        {
            return string.Format(Se.Language.General.BookmarkEditForSelectedLinesX, selectedItems.Count);
        }

        return Se.Language.General.BookmarkEdit;
    }

    private string GetAddLabel(List<SubtitleLineViewModel> selectedItems)
    {
        if (selectedItems.Count > 1)
        {
            return string.Format(Se.Language.General.BookmarkAddForSelectedLinesX, selectedItems.Count);
        }

        return Se.Language.General.BookmarkAdd;
    }

    internal void OnTextBoxKeyDown(KeyEventArgs args)
    {
        if (args.Key == Key.Enter)
        {
            args.Handled = true;
            Ok();
        }
    }
}