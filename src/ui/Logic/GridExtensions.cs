namespace Nikse.SubtitleEdit.Logic;

public static class GridExtensions
{
    public static void Add(this Avalonia.Controls.Grid grid, Avalonia.Controls.Control control, int row, int column = 0, int rowSpan = 1, int columnSpan = 1)
    {
        Avalonia.Controls.Grid.SetRow(control, row);
        Avalonia.Controls.Grid.SetColumn(control, column);
        Avalonia.Controls.Grid.SetRowSpan(control, rowSpan);
        Avalonia.Controls.Grid.SetColumnSpan(control, columnSpan);
        grid.Children.Add(control);
    }
}
