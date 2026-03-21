using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public partial class EditCategoryViewModel : ObservableObject
{
    [ObservableProperty] private string _categoryName;
    
    public Window? Window { get; set; }
    
    public bool OkPressed { get; private set; }
    public string? Title { get; internal set; }

    public EditCategoryViewModel()
    {
        CategoryName = string.Empty;
    }

    public void Initialize(string title, RuleTreeNode node)
    {
        Title = title;
        CategoryName = node.CategoryName;
    }

    [RelayCommand]                   
    private void Ok() 
    {
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void Cancel() 
    {
        Window?.Close();
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(object newCategory, RuleTreeNode node)
    {
        throw new NotImplementedException();
    }
}