using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Nikse.SubtitleEdit.Features.Files.ImportCsvXlsxCustomColumns;

public partial class CsvColumnDefinition : ObservableObject
{
    [ObservableProperty] private CsvColumnRole _role;

    public int Index { get; }
    public string HeaderName { get; }

    public event EventHandler? RoleChanged;

    public CsvColumnDefinition(int index, string headerName, CsvColumnRole role)
    {
        Index = index;
        HeaderName = headerName;
        _role = role;
    }

    partial void OnRoleChanged(CsvColumnRole value)
    {
        RoleChanged?.Invoke(this, EventArgs.Empty);
    }
}
