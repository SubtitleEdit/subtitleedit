using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls;

namespace UITests.Controls;

/// <summary>
/// Under Fluent 2 a focused TextBox paints its PART_BorderElement with a border and a 2px accent
/// underline from theme resources. Inside TimeCodeUpDown / SecondsUpDown that drew a second
/// field (with its own underline) inside the spinner's border and underline. Only the spinner's
/// own border may show.
/// </summary>
public class UpDownInnerTextBoxBorderTests
{
    public static TheoryData<string> Controls => new() { nameof(TimeCodeUpDown), nameof(SecondsUpDown) };

    [AvaloniaTheory]
    [MemberData(nameof(Controls))]
    public void FocusedInnerTextBoxDrawsNoBorderOrUnderline(string controlName)
    {
        Control control = controlName == nameof(TimeCodeUpDown)
            ? new TimeCodeUpDown { Value = TimeSpan.FromMilliseconds(68023) }
            : new SecondsUpDown { Value = TimeSpan.FromSeconds(1.5) };

        var window = new Window { Width = 300, Height = 100, Content = control };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var textBox = control.GetVisualDescendants().OfType<TextBox>().First(t => t.Name == "PART_TextBox");
            Assert.True(textBox.Focus());
            Dispatcher.UIThread.RunJobs();

            var border = textBox.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_BorderElement");
            Assert.Equal(new Thickness(0), border.BorderThickness);
            Assert.True(border.BorderBrush is null || border.BorderBrush is ISolidColorBrush { Color.A: 0 },
                $"inner border brush should be transparent, was {border.BorderBrush}");
            Assert.True(border.Background is null || border.Background is ISolidColorBrush { Color.A: 0 },
                $"inner background should be transparent, was {border.Background}");
        }
        finally
        {
            window.Close();
        }
    }
}
