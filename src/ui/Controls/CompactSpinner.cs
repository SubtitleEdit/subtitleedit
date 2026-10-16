using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// Turns a ButtonSpinner's two wide side-by-side up/down buttons into a narrow stacked pair
/// (used by TimeCodeUpDown and SecondsUpDown everywhere).
/// The theme sets the panel orientation and button sizes inside its template, where they beat
/// any style, so they are changed here once the template is applied.
/// </summary>
internal static class CompactSpinner
{
    private const double ButtonWidth = 22;

    internal static void Attach(ButtonSpinner spinner)
    {
        spinner.TemplateApplied -= OnTemplateApplied;
        spinner.TemplateApplied += OnTemplateApplied;
    }

    private static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (e.NameScope.Find<StackPanel>("PART_SpinnerPanel") is { } panel)
        {
            panel.Orientation = Orientation.Vertical;
            panel.VerticalAlignment = VerticalAlignment.Center;
        }

        foreach (var name in new[] { "PART_IncreaseButton", "PART_DecreaseButton" })
        {
            if (e.NameScope.Find<RepeatButton>(name) is { } button)
            {
                button.Width = ButtonWidth;
                button.MinWidth = 0;
                button.Height = 13;
                button.MinHeight = 0;
                button.Padding = new Avalonia.Thickness(0);
            }
        }
    }
}
