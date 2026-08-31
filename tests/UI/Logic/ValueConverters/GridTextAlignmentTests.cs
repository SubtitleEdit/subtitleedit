using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Globalization;

namespace UITests.Logic.ValueConverters;

/// <summary>
/// Issue #14316: SE 4 could center the text in the subtitle grid; SE 5 had the setting for the
/// edit box only. The grid's Text column gets its alignment from
/// <see cref="TeletextAlignmentPreviewConverter"/>, which used to hardcode a left fallback -
/// the new "center text in subtitle grid" setting rides in as the converter parameter, so the
/// teletext preview still wins whenever it is actually driving the alignment.
/// </summary>
public class GridTextAlignmentTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    private static object Convert(bool teletextPreviewActive, TextAlignment? teletextAlignment, object? parameter) =>
        TeletextAlignmentPreviewConverter.Instance.Convert(
            new object?[] { teletextPreviewActive, teletextAlignment },
            typeof(TextAlignment),
            parameter,
            Culture);

    // The default: no setting passed, no teletext preview - unchanged left alignment.
    [Fact]
    public void NoParameter_StillFallsBackToStart()
    {
        Assert.Equal(TextAlignment.Start, Convert(false, null, null));
    }

    [Fact]
    public void CenterParameter_IsUsedWhenTeletextPreviewIsOff()
    {
        Assert.Equal(TextAlignment.Center, Convert(false, null, TextAlignment.Center));
    }

    // The teletext preview shows where the line really sits on a teletext page, so it has to
    // outrank a cosmetic grid preference.
    [Fact]
    public void TeletextPreview_StillWins_OverTheCenterSetting()
    {
        Assert.Equal(TextAlignment.End, Convert(true, TextAlignment.End, TextAlignment.Center));
    }

    // An active preview on a line with no alignment of its own falls back like everything else.
    [Fact]
    public void TeletextPreviewWithNoAlignment_UsesTheParameter()
    {
        Assert.Equal(TextAlignment.Center, Convert(true, null, TextAlignment.Center));
    }

    // The grid wires the fallback in as MultiBinding.ConverterParameter. That only works if
    // Avalonia actually forwards it to the converter, so bind it for real rather than trusting
    // the direct calls above.
    [AvaloniaFact]
    public void ConverterParameter_ReachesTheConverter_ThroughAMultiBinding()
    {
        var source = new TeletextPreviewStub { IsTeletextPreviewActive = false };
        var textBlock = new TextBlock
        {
            DataContext = source,
            [!TextBlock.TextAlignmentProperty] = new MultiBinding
            {
                Converter = TeletextAlignmentPreviewConverter.Instance,
                ConverterParameter = TextAlignment.Center,
                Bindings =
                {
                    new Binding(nameof(TeletextPreviewStub.IsTeletextPreviewActive)) { Mode = BindingMode.OneWay },
                    new Binding(nameof(TeletextPreviewStub.TeletextTextAlignment)) { Mode = BindingMode.OneWay },
                },
            },
        };

        var window = new Window { Width = 200, Height = 100, Content = textBlock };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(TextAlignment.Center, textBlock.TextAlignment);

        window.Close();
    }

    private sealed class TeletextPreviewStub
    {
        public bool IsTeletextPreviewActive { get; set; }
        public TextAlignment? TeletextTextAlignment { get; set; }
    }
}
